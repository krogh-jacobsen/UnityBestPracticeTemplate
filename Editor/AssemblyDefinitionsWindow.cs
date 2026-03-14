using UnityEditor;
using UnityEngine;
using System.IO;
using System.Collections.Generic;

namespace Unity.BestPractices.Editor
{
    /// <summary>
    /// Dedicated panel that lists all <c>.asmdef</c> files found in <c>Assets/</c> and displays
    /// their key configuration data so you can verify they are set up correctly and match the
    /// project's folder structure.  Also provides a one-click button to generate the recommended
    /// assembly definitions under <c>Assets/_ProjectName</c>.
    /// Open via <b>Window → Best Practices → Assembly Definitions</b> or from the Project Dashboard.
    /// </summary>
    public class AssemblyDefinitionsWindow : EditorWindow
    {
        private struct AsmdefInfo
        {
            public string AssetPath;
            public string Name;
            public string RootNamespace;
            public string[] References;
            public string[] IncludePlatforms;
            public bool AutoReferenced;
            public bool AllowUnsafeCode;
            public bool OverrideReferences;
            public string[] PrecompiledReferences;
            public string[] DefineConstraints;
        }

        private Vector2 m_ScrollPosition;
        private List<AsmdefInfo> m_Asmdefs;
        private bool m_ShowDetails = true;

        [MenuItem("Tools/Unity Best Practices/Code/Assembly Definitions", false, 100)]
        public static void ShowWindow()
        {
            var window = GetWindow<AssemblyDefinitionsWindow>("Assembly Definitions");
            window.minSize = new Vector2(500, 540);
            window.Show();
        }

        private void OnEnable()
        {
            Refresh();
        }

        private void OnFocus()
        {
            Refresh();
        }

        private void Refresh()
        {
            m_Asmdefs = FindAllAsmdefs();
        }

        private void OnGUI()
        {
            m_ScrollPosition = EditorGUILayout.BeginScrollView(m_ScrollPosition);

            DrawHeader();
            GUILayout.Space(8);
            DrawGenerateSection();
            GUILayout.Space(8);
            DrawAsmdefList();

            EditorGUILayout.EndScrollView();
        }

        // ──────────────────────────────────────────────────────────────────────

        private void DrawHeader()
        {
            EditorGUILayout.BeginVertical(EditorStyles.helpBox);
            GUILayout.Label("ASSEMBLY DEFINITIONS", EditorStyles.boldLabel);
            GUILayout.Space(2);
            GUILayout.Label(
                "Lists every .asmdef file found under Assets/ and shows its key configuration. " +
                "Use this panel to verify names, namespaces, platform filters and references match " +
                "your folder structure. Use the Generate button to create the recommended set.",
                EditorStyles.wordWrappedLabel);
            EditorGUILayout.EndVertical();
        }

        private void DrawGenerateSection()
        {
            EditorGUILayout.BeginVertical(EditorStyles.helpBox);
            GUILayout.Label("Generate recommended assembly definitions for this project.", EditorStyles.wordWrappedLabel);
            GUILayout.Space(4);

            EditorGUILayout.BeginHorizontal();
            if (GUILayout.Button("Generate Assembly Definitions", GUILayout.Height(28)))
            {
                GenerateAssemblyDefinitions.Execute();
                Refresh();
                Repaint();
            }

            if (GUILayout.Button("Refresh", GUILayout.Width(70), GUILayout.Height(28)))
            {
                Refresh();
                Repaint();
            }
            EditorGUILayout.EndHorizontal();

            GUILayout.Space(4);
            GUILayout.Label(
                "Generates .asmdef files for Scripts, Editor and Tests using Company.Product as the namespace root. " +
                "Run after Setup Project Folders.",
                EditorStyles.wordWrappedMiniLabel);
            EditorGUILayout.EndVertical();
        }

        private void DrawAsmdefList()
        {
            EditorGUILayout.BeginVertical(EditorStyles.helpBox);

            EditorGUILayout.BeginHorizontal();
            int count = m_Asmdefs != null ? m_Asmdefs.Count : 0;
            m_ShowDetails = EditorGUILayout.Foldout(m_ShowDetails,
                $"ASSEMBLY DEFINITIONS IN PROJECT — {count} found", true, EditorStyles.foldoutHeader);
            EditorGUILayout.EndHorizontal();

            if (m_ShowDetails)
            {
                GUILayout.Space(4);

                if (count == 0)
                {
                    EditorGUILayout.HelpBox(
                        "No .asmdef files found under Assets/. Use the Generate button above to create the recommended set, or add .asmdef files manually.",
                        MessageType.Info);
                }
                else
                {
                    foreach (var info in m_Asmdefs)
                        DrawAsmdefRow(info);
                }
            }

            EditorGUILayout.EndVertical();
        }

        private static void DrawAsmdefRow(AsmdefInfo info)
        {
            bool isEditorOnly = info.IncludePlatforms != null &&
                                System.Array.IndexOf(info.IncludePlatforms, "Editor") >= 0;
            bool isTestAssembly = info.DefineConstraints != null &&
                                  System.Array.IndexOf(info.DefineConstraints, "UNITY_INCLUDE_TESTS") >= 0;

            EditorGUILayout.BeginVertical(EditorStyles.helpBox);

            // ── Title row ────────────────────────────────────────────────────
            EditorGUILayout.BeginHorizontal();

            // Type badge
            var prevColor = GUI.color;
            if (isTestAssembly)
                GUI.color = new Color(0.8f, 0.6f, 1f);
            else if (isEditorOnly)
                GUI.color = new Color(0.6f, 0.85f, 1f);
            else
                GUI.color = new Color(0.3f, 0.8f, 0.3f);

            string badge = isTestAssembly ? "[Test]" : (isEditorOnly ? "[Editor]" : "[Runtime]");
            GUILayout.Label(badge, EditorStyles.boldLabel, GUILayout.Width(64));
            GUI.color = prevColor;

            GUILayout.Label(info.Name, EditorStyles.boldLabel);
            GUILayout.FlexibleSpace();

            if (GUILayout.Button("Select", GUILayout.Width(52)))
            {
                var asset = AssetDatabase.LoadAssetAtPath<UnityEditorInternal.AssemblyDefinitionAsset>(info.AssetPath);
                if (asset != null)
                {
                    Selection.activeObject = asset;
                    EditorGUIUtility.PingObject(asset);
                }
            }

            EditorGUILayout.EndHorizontal();

            // ── Path ─────────────────────────────────────────────────────────
            prevColor = GUI.color;
            GUI.color = new Color(0.6f, 0.6f, 0.6f);
            GUILayout.Label("Path:  " + info.AssetPath, EditorStyles.miniLabel);
            GUI.color = prevColor;

            // ── Key config data ───────────────────────────────────────────────
            EditorGUILayout.BeginHorizontal();

            // Namespace
            DrawDataPill("Namespace", string.IsNullOrEmpty(info.RootNamespace) ? "(none)" : info.RootNamespace);

            // Platform filter
            string platforms = (info.IncludePlatforms == null || info.IncludePlatforms.Length == 0)
                ? "All Platforms"
                : string.Join(", ", info.IncludePlatforms);
            DrawDataPill("Platforms", platforms);

            // Auto-referenced
            prevColor = GUI.color;
            GUI.color = info.AutoReferenced ? new Color(0.3f, 0.8f, 0.3f) : new Color(0.7f, 0.7f, 0.7f);
            GUILayout.Label(info.AutoReferenced ? "Auto Ref: ON" : "Auto Ref: OFF", EditorStyles.miniLabel, GUILayout.Width(88));
            GUI.color = prevColor;

            // Unsafe code
            if (info.AllowUnsafeCode)
            {
                prevColor = GUI.color;
                GUI.color = new Color(0.9f, 0.7f, 0.2f);
                GUILayout.Label("Unsafe: ON", EditorStyles.miniLabel, GUILayout.Width(72));
                GUI.color = prevColor;
            }

            EditorGUILayout.EndHorizontal();

            // ── References ───────────────────────────────────────────────────
            if (info.References != null && info.References.Length > 0)
            {
                GUILayout.Label("References:  " + string.Join(",  ", info.References),
                    EditorStyles.wordWrappedMiniLabel);
            }

            // ── Precompiled / Define constraints ─────────────────────────────
            if (info.OverrideReferences && info.PrecompiledReferences != null && info.PrecompiledReferences.Length > 0)
            {
                prevColor = GUI.color;
                GUI.color = new Color(0.6f, 0.6f, 0.6f);
                GUILayout.Label("Precompiled:  " + string.Join(",  ", info.PrecompiledReferences),
                    EditorStyles.wordWrappedMiniLabel);
                GUI.color = prevColor;
            }

            if (info.DefineConstraints != null && info.DefineConstraints.Length > 0)
            {
                prevColor = GUI.color;
                GUI.color = new Color(0.6f, 0.6f, 0.6f);
                GUILayout.Label("Constraints:  " + string.Join(",  ", info.DefineConstraints),
                    EditorStyles.wordWrappedMiniLabel);
                GUI.color = prevColor;
            }

            EditorGUILayout.EndVertical();
            GUILayout.Space(2);
        }

        private static void DrawDataPill(string label, string value)
        {
            var prevColor = GUI.color;
            GUI.color = new Color(0.6f, 0.6f, 0.6f);
            GUILayout.Label($"{label}: ", EditorStyles.miniLabel, GUILayout.Width(60));
            GUI.color = prevColor;
            GUILayout.Label(value, EditorStyles.miniLabel);
        }

        // ──────────────────────────────────────────────────────────────────────

        private static List<AsmdefInfo> FindAllAsmdefs()
        {
            var result = new List<AsmdefInfo>();

            string[] guids = AssetDatabase.FindAssets("t:AssemblyDefinitionAsset", new[] { "Assets" });
            foreach (string guid in guids)
            {
                string assetPath = AssetDatabase.GUIDToAssetPath(guid);
                string fullPath = Path.GetFullPath(Path.Combine(Application.dataPath, "..", assetPath));

                if (!File.Exists(fullPath))
                    continue;

                string json = File.ReadAllText(fullPath);
                AsmdefInfo info = ParseAsmdef(json, assetPath);
                result.Add(info);
            }

            result.Sort((a, b) => string.Compare(a.AssetPath, b.AssetPath, System.StringComparison.OrdinalIgnoreCase));
            return result;
        }

        private static AsmdefInfo ParseAsmdef(string json, string assetPath)
        {
            var info = new AsmdefInfo { AssetPath = assetPath };

            info.Name = ExtractStringField(json, "name");
            info.RootNamespace = ExtractStringField(json, "rootNamespace");
            info.References = ExtractStringArray(json, "references");
            info.IncludePlatforms = ExtractStringArray(json, "includePlatforms");
            info.PrecompiledReferences = ExtractStringArray(json, "precompiledReferences");
            info.DefineConstraints = ExtractStringArray(json, "defineConstraints");
            info.AutoReferenced = ExtractBoolField(json, "autoReferenced", true);
            info.AllowUnsafeCode = ExtractBoolField(json, "allowUnsafeCode", false);
            info.OverrideReferences = ExtractBoolField(json, "overrideReferences", false);

            return info;
        }

        /// <summary>Extracts a simple string value for the given JSON key.</summary>
        private static string ExtractStringField(string json, string key)
        {
            string pattern = $"\"{key}\"";
            int keyIndex = json.IndexOf(pattern, System.StringComparison.Ordinal);
            if (keyIndex < 0) return string.Empty;

            int colonIndex = json.IndexOf(':', keyIndex + pattern.Length);
            if (colonIndex < 0) return string.Empty;

            int quoteStart = json.IndexOf('"', colonIndex + 1);
            if (quoteStart < 0) return string.Empty;

            int quoteEnd = json.IndexOf('"', quoteStart + 1);
            if (quoteEnd < 0) return string.Empty;

            return json.Substring(quoteStart + 1, quoteEnd - quoteStart - 1);
        }

        /// <summary>Extracts a JSON string array value for the given key.</summary>
        private static string[] ExtractStringArray(string json, string key)
        {
            string pattern = $"\"{key}\"";
            int keyIndex = json.IndexOf(pattern, System.StringComparison.Ordinal);
            if (keyIndex < 0) return System.Array.Empty<string>();

            int bracketStart = json.IndexOf('[', keyIndex + pattern.Length);
            int bracketEnd = json.IndexOf(']', bracketStart + 1);
            if (bracketStart < 0 || bracketEnd < 0) return System.Array.Empty<string>();

            string arrayContent = json.Substring(bracketStart + 1, bracketEnd - bracketStart - 1).Trim();
            if (string.IsNullOrWhiteSpace(arrayContent)) return System.Array.Empty<string>();

            var items = new List<string>();
            int pos = 0;
            while (pos < arrayContent.Length)
            {
                int q1 = arrayContent.IndexOf('"', pos);
                if (q1 < 0) break;
                int q2 = arrayContent.IndexOf('"', q1 + 1);
                if (q2 < 0) break;
                items.Add(arrayContent.Substring(q1 + 1, q2 - q1 - 1));
                pos = q2 + 1;
            }
            return items.ToArray();
        }

        /// <summary>Extracts a boolean field from the JSON.</summary>
        private static bool ExtractBoolField(string json, string key, bool defaultValue)
        {
            string pattern = $"\"{key}\"";
            int keyIndex = json.IndexOf(pattern, System.StringComparison.Ordinal);
            if (keyIndex < 0) return defaultValue;

            int colonIndex = json.IndexOf(':', keyIndex + pattern.Length);
            if (colonIndex < 0) return defaultValue;

            int valueStart = colonIndex + 1;
            while (valueStart < json.Length && char.IsWhiteSpace(json[valueStart]))
                valueStart++;

            if (valueStart + 4 <= json.Length && json.Substring(valueStart, 4) == "true")
                return true;
            if (valueStart + 5 <= json.Length && json.Substring(valueStart, 5) == "false")
                return false;

            return defaultValue;
        }
    }
}
