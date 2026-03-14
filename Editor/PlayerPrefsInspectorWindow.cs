using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text.RegularExpressions;
using UnityEditor;
using UnityEngine;

namespace Unity.BestPractices.Editor
{
    /// <summary>
    /// Editor window for viewing, editing, and deleting persisted PlayerPrefs entries.
    /// Enumerates all keys from the platform-specific storage then uses the PlayerPrefs
    /// API to read and write values.
    /// <para>Menu: <b>Window → Best Practices → PlayerPrefs Inspector</b></para>
    /// </summary>
    public class PlayerPrefsInspectorWindow : EditorWindow
    {
        #region Types

        private enum PrefType { String, Int, Float }

        private class PrefEntry
        {
            public string Key;
            public string RawValue;
            public PrefType Type;
            public bool IsEditing;
            public string EditBuffer;
        }

        #endregion

        #region Fields

        private List<PrefEntry> m_Entries = new List<PrefEntry>();
        private Vector2 m_Scroll;
        private string m_SearchQuery = string.Empty;
        private string m_NewKey = string.Empty;
        private string m_NewValue = string.Empty;
        private PrefType m_NewType = PrefType.String;
        private string m_StatusMsg = string.Empty;
        private double m_StatusTime;

        private static readonly GUILayoutOption k_TypeWidth = GUILayout.Width(54);
        private static readonly GUILayoutOption k_BtnWidth = GUILayout.Width(50);
        private static readonly GUILayoutOption k_DelWidth = GUILayout.Width(22);

        #endregion

        #region Editor Window

        [MenuItem("Tools/Unity Workbench/Utilities/PlayerPrefs Inspector", false, 251)]
        public static void ShowWindow()
        {
            var w = GetWindow<PlayerPrefsInspectorWindow>("PlayerPrefs Inspector");
            w.minSize = new Vector2(520, 380);
            w.Show();
        }

        private void OnEnable() => Refresh();

        private void OnGUI()
        {
            DrawToolbar();
            DrawAddRow();
            GUILayout.Space(4);
            DrawList();
            DrawStatusBar();
        }

        #endregion

        #region Toolbar

        private void DrawToolbar()
        {
            EditorGUILayout.BeginHorizontal(EditorStyles.toolbar);

            GUILayout.Label("Search:", GUILayout.Width(48));
            m_SearchQuery = EditorGUILayout.TextField(m_SearchQuery, EditorStyles.toolbarSearchField);

            GUILayout.FlexibleSpace();

            if (GUILayout.Button("Refresh", EditorStyles.toolbarButton, GUILayout.Width(60)))
                Refresh();

            GUI.color = new Color(1f, 0.55f, 0.55f);
            if (GUILayout.Button("Delete All", EditorStyles.toolbarButton, GUILayout.Width(72)))
            {
                if (EditorUtility.DisplayDialog(
                    "Delete All PlayerPrefs",
                    "This will permanently delete ALL PlayerPrefs entries for this project.\n\nAre you sure?",
                    "Delete All", "Cancel"))
                {
                    PlayerPrefs.DeleteAll();
                    PlayerPrefs.Save();
                    m_Entries.Clear();
                    SetStatus("All PlayerPrefs deleted.");
                }
            }
            GUI.color = Color.white;

            EditorGUILayout.EndHorizontal();
        }

        #endregion

        #region Add Row

        private void DrawAddRow()
        {
            EditorGUILayout.BeginVertical(EditorStyles.helpBox);
            GUILayout.Label("Add / Set Entry", EditorStyles.boldLabel);
            EditorGUILayout.BeginHorizontal();

            m_NewType = (PrefType)EditorGUILayout.EnumPopup(m_NewType, k_TypeWidth);
            m_NewKey = EditorGUILayout.TextField(m_NewKey, GUILayout.MinWidth(80));
            GUILayout.Label("=", GUILayout.Width(14));
            m_NewValue = EditorGUILayout.TextField(m_NewValue, GUILayout.MinWidth(80));

            if (GUILayout.Button("Set", GUILayout.Width(40)))
                CommitNewEntry();

            EditorGUILayout.EndHorizontal();
            EditorGUILayout.EndVertical();
        }

        private void CommitNewEntry()
        {
            if (string.IsNullOrWhiteSpace(m_NewKey))
            {
                SetStatus("Key cannot be empty.");
                return;
            }

            switch (m_NewType)
            {
                case PrefType.Int:
                    if (int.TryParse(m_NewValue, out int iv))
                        PlayerPrefs.SetInt(m_NewKey, iv);
                    else { SetStatus($"'{m_NewValue}' is not a valid int."); return; }
                    break;
                case PrefType.Float:
                    if (float.TryParse(m_NewValue,
                        System.Globalization.NumberStyles.Float,
                        System.Globalization.CultureInfo.InvariantCulture, out float fv))
                        PlayerPrefs.SetFloat(m_NewKey, fv);
                    else { SetStatus($"'{m_NewValue}' is not a valid float."); return; }
                    break;
                default:
                    PlayerPrefs.SetString(m_NewKey, m_NewValue);
                    break;
            }

            PlayerPrefs.Save();
            string savedKey = m_NewKey;
            m_NewKey = string.Empty;
            m_NewValue = string.Empty;
            Refresh();
            SetStatus($"Set '{savedKey}'.");
        }

        #endregion

        #region Entry List

        private void DrawList()
        {
            string query = m_SearchQuery.Trim().ToLowerInvariant();

            // Column headers
            EditorGUILayout.BeginHorizontal(EditorStyles.toolbar);
            GUILayout.Label("Type", EditorStyles.toolbarButton, k_TypeWidth);
            GUILayout.Label("Key", EditorStyles.toolbarButton, GUILayout.MinWidth(100));
            GUILayout.Label("Value", EditorStyles.toolbarButton, GUILayout.MinWidth(100));
            GUILayout.Label("", EditorStyles.toolbarButton, GUILayout.Width(50 + 22 + 4)); // edit + delete
            EditorGUILayout.EndHorizontal();

            m_Scroll = EditorGUILayout.BeginScrollView(m_Scroll);

            for (int i = m_Entries.Count - 1; i >= 0; i--)
            {
                var e = m_Entries[i];

                if (!string.IsNullOrEmpty(query) &&
                    !e.Key.ToLowerInvariant().Contains(query) &&
                    !e.RawValue.ToLowerInvariant().Contains(query))
                    continue;

                EditorGUILayout.BeginHorizontal();

                // Type badge
                DrawTypeBadge(e.Type);

                // Key (read-only label)
                EditorGUILayout.SelectableLabel(e.Key, GUILayout.MinWidth(100), GUILayout.Height(EditorGUIUtility.singleLineHeight));

                // Value — inline edit
                if (e.IsEditing)
                {
                    e.EditBuffer = EditorGUILayout.TextField(e.EditBuffer, GUILayout.MinWidth(100));
                    if (GUILayout.Button("Save", k_BtnWidth))
                        SaveEdit(e);
                }
                else
                {
                    EditorGUILayout.SelectableLabel(e.RawValue, GUILayout.MinWidth(100), GUILayout.Height(EditorGUIUtility.singleLineHeight));
                    if (GUILayout.Button("Edit", k_BtnWidth))
                    {
                        e.IsEditing = true;
                        e.EditBuffer = e.RawValue;
                    }
                }

                // Delete
                GUI.color = new Color(1f, 0.55f, 0.55f);
                if (GUILayout.Button("✕", k_DelWidth))
                {
                    PlayerPrefs.DeleteKey(e.Key);
                    PlayerPrefs.Save();
                    m_Entries.RemoveAt(i);
                    SetStatus($"Deleted '{e.Key}'.");
                }
                GUI.color = Color.white;

                EditorGUILayout.EndHorizontal();
            }

            EditorGUILayout.EndScrollView();

            if (m_Entries.Count == 0)
                EditorGUILayout.HelpBox("No PlayerPrefs entries found for this project.", MessageType.Info);
        }

        private void SaveEdit(PrefEntry e)
        {
            switch (e.Type)
            {
                case PrefType.Int:
                    if (int.TryParse(e.EditBuffer, out int iv))
                    { PlayerPrefs.SetInt(e.Key, iv); e.RawValue = iv.ToString(); }
                    else { SetStatus($"'{e.EditBuffer}' is not a valid int."); return; }
                    break;
                case PrefType.Float:
                    if (float.TryParse(e.EditBuffer,
                        System.Globalization.NumberStyles.Float,
                        System.Globalization.CultureInfo.InvariantCulture, out float fv))
                    { PlayerPrefs.SetFloat(e.Key, fv); e.RawValue = fv.ToString(System.Globalization.CultureInfo.InvariantCulture); }
                    else { SetStatus($"'{e.EditBuffer}' is not a valid float."); return; }
                    break;
                default:
                    PlayerPrefs.SetString(e.Key, e.EditBuffer);
                    e.RawValue = e.EditBuffer;
                    break;
            }

            PlayerPrefs.Save();
            e.IsEditing = false;
            SetStatus($"Saved '{e.Key}'.");
        }

        private static void DrawTypeBadge(PrefType type)
        {
            Color prev = GUI.color;
            GUI.color = type switch
            {
                PrefType.Int => new Color(0.5f, 0.8f, 1.0f),
                PrefType.Float => new Color(0.7f, 1.0f, 0.6f),
                _ => new Color(0.9f, 0.85f, 0.6f),
            };
            GUILayout.Label(type.ToString(), EditorStyles.helpBox, k_TypeWidth);
            GUI.color = prev;
        }

        #endregion

        #region Status Bar

        private void DrawStatusBar()
        {
            if (!string.IsNullOrEmpty(m_StatusMsg) && EditorApplication.timeSinceStartup - m_StatusTime < 4.0)
            {
                EditorGUILayout.HelpBox(m_StatusMsg, MessageType.None);
                Repaint();
            }
        }

        private void SetStatus(string msg)
        {
            m_StatusMsg = msg;
            m_StatusTime = EditorApplication.timeSinceStartup;
        }

        #endregion

        #region Key Enumeration

        private void Refresh()
        {
            m_Entries.Clear();
            List<string> keys = EnumerateKeys();

            foreach (string key in keys)
            {
                // Detect type: try int, then float, default string
                PrefType type;
                string raw;

                // Use a heuristic: try SetInt / SetFloat sentinel look-ups via the
                // actual stored value. If the key was set with SetInt, GetFloat will
                // return the same number; likewise for float. GetString always returns
                // something. We detect by checking whether the stored value looks like
                // a pure integer, a decimal number, or text.
                string strVal = PlayerPrefs.GetString(key, "\x01NULL\x01");

                if (strVal == "\x01NULL\x01")
                {
                    // Key exists but GetString returned nothing useful — treat numeric
                    float fv = PlayerPrefs.GetFloat(key, float.NaN);
                    int iv = PlayerPrefs.GetInt(key, int.MinValue);
                    if (iv != int.MinValue && !float.IsNaN(fv) && Mathf.Approximately(fv, iv))
                    { type = PrefType.Int; raw = iv.ToString(); }
                    else if (!float.IsNaN(fv))
                    { type = PrefType.Float; raw = fv.ToString(System.Globalization.CultureInfo.InvariantCulture); }
                    else
                    { type = PrefType.String; raw = string.Empty; }
                }
                else
                {
                    // We got a string back — attempt to distinguish
                    // by comparing GetInt / GetFloat to what the string representations imply
                    int iv = PlayerPrefs.GetInt(key, int.MinValue);
                    float fv = PlayerPrefs.GetFloat(key, float.NaN);

                    if (iv != int.MinValue && strVal == iv.ToString())
                    { type = PrefType.Int; raw = iv.ToString(); }
                    else if (!float.IsNaN(fv) && strVal == fv.ToString(System.Globalization.CultureInfo.InvariantCulture))
                    { type = PrefType.Float; raw = fv.ToString(System.Globalization.CultureInfo.InvariantCulture); }
                    else
                    { type = PrefType.String; raw = strVal; }
                }

                m_Entries.Add(new PrefEntry { Key = key, RawValue = raw, Type = type });
            }

            m_Entries.Sort((a, b) => string.Compare(a.Key, b.Key, StringComparison.OrdinalIgnoreCase));
        }

        /// <summary>Returns all PlayerPrefs key names for the current project using platform-specific storage.</summary>
        private static List<string> EnumerateKeys()
        {
#if UNITY_EDITOR_WIN
            return EnumerateKeysWindows();
#elif UNITY_EDITOR_OSX
            return EnumerateKeysMacOS();
#else
            return EnumerateKeysLinux();
#endif
        }

#if UNITY_EDITOR_WIN
        private static List<string> EnumerateKeysWindows()
        {
            var keys = new List<string>();
            try
            {
                string subKey = $@"Software\Unity\UnityEditor\{PlayerSettings.companyName}\{PlayerSettings.productName}";
                using var regKey = Microsoft.Win32.Registry.CurrentUser.OpenSubKey(subKey);
                if (regKey == null) return keys;
                foreach (string name in regKey.GetValueNames())
                {
                    // Unity appends "_hXXXXXXXX" hash suffix — strip it
                    int lastUnderscore = name.LastIndexOf('_');
                    string clean = lastUnderscore > 0 ? name.Substring(0, lastUnderscore) : name;
                    if (!keys.Contains(clean))
                        keys.Add(clean);
                }
            }
            catch (Exception e)
            {
                UnityEngine.Debug.LogWarning($"[PlayerPrefs Inspector] Could not read Windows registry: {e.Message}");
            }
            return keys;
        }
#endif

#if UNITY_EDITOR_OSX
        private static List<string> EnumerateKeysMacOS()
        {
            var keys = new List<string>();
            try
            {
                // Unity stores prefs in ~/Library/Preferences/unity.{company}.{product}.plist
                string company = PlayerSettings.companyName.Replace(" ", "_");
                string product = PlayerSettings.productName.Replace(" ", "_");
                string plistId = $"unity.{company}.{product}";

                var psi = new ProcessStartInfo
                {
                    FileName = "/usr/bin/plutil",
                    Arguments = $"-convert json -o - -r ~/Library/Preferences/{plistId}.plist",
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    UseShellExecute = false,
                    CreateNoWindow = true
                };

                using var proc = Process.Start(psi);
                string json = proc.StandardOutput.ReadToEnd();
                proc.WaitForExit();

                // Simple regex key extractor — avoids a JSON dependency
                var matches = Regex.Matches(json, "\"([^\"]+)\"\\s*:");
                foreach (Match m in matches)
                {
                    string key = m.Groups[1].Value;
                    if (!keys.Contains(key))
                        keys.Add(key);
                }
            }
            catch (Exception e)
            {
                UnityEngine.Debug.LogWarning($"[PlayerPrefs Inspector] Could not read macOS plist: {e.Message}");
            }
            return keys;
        }
#endif

#if UNITY_EDITOR_LINUX
        private static List<string> EnumerateKeysLinux()
        {
            var keys = new List<string>();
            try
            {
                string prefsPath = System.IO.Path.Combine(
                    System.Environment.GetFolderPath(System.Environment.SpecialFolder.ApplicationData),
                    "unity3d",
                    PlayerSettings.companyName,
                    PlayerSettings.productName,
                    "prefs");

                if (!System.IO.File.Exists(prefsPath)) return keys;

                // The Linux prefs file is a binary format: each entry is a
                // type byte + length-prefixed key + length-prefixed value.
                // We extract printable key strings with a simple heuristic scan.
                byte[] bytes = System.IO.File.ReadAllBytes(prefsPath);
                int i = 0;
                while (i < bytes.Length - 2)
                {
                    // Look for printable ASCII sequences of length > 2
                    if (bytes[i] > 0x20 && bytes[i] < 0x7F)
                    {
                        int start = i;
                        while (i < bytes.Length && bytes[i] > 0x20 && bytes[i] < 0x7F) i++;
                        string candidate = System.Text.Encoding.ASCII.GetString(bytes, start, i - start);
                        if (candidate.Length > 2 && !keys.Contains(candidate))
                            keys.Add(candidate);
                    }
                    else
                    {
                        i++;
                    }
                }
            }
            catch (Exception e)
            {
                UnityEngine.Debug.LogWarning($"[PlayerPrefs Inspector] Could not read Linux prefs: {e.Message}");
            }
            return keys;
        }
#endif

        #endregion
    }
}
