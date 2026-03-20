using UnityEditor;
using UnityEngine;

namespace Unity.BestPractices.Editor
{
    /// <summary>
    /// Dedicated panel for applying recommended Unity 6 project settings one step at a time.
    /// Each row shows whether the setting is already configured and offers an individual Run button.
    /// Open via <b>Window → Best Practices → Configure Project Settings</b> or from the Project Dashboard.
    /// </summary>
    public class ProjectSettingsWindow : EditorWindow
    {
        private Vector2 m_ScrollPosition;

        public static void ShowWindow()
        {
            var window = GetWindow<ProjectSettingsWindow>("Project Settings");
            window.minSize = new Vector2(420, 360);
            window.Show();
        }

        private void OnGUI()
        {
            m_ScrollPosition = EditorGUILayout.BeginScrollView(m_ScrollPosition);

            DrawHeader();
            GUILayout.Space(8);
            DrawApplyAllButton();
            GUILayout.Space(8);
            DrawSettings();

            EditorGUILayout.EndScrollView();
        }

        private void DrawHeader()
        {
            EditorGUILayout.BeginVertical(EditorStyles.helpBox);
            GUILayout.Label("PROJECT SETTINGS", EditorStyles.boldLabel);
            GUILayout.Space(2);
            GUILayout.Label(
                "Apply recommended Unity 6 settings individually or all at once. " +
                "Green \"Configured\" means the setting already matches the recommendation.",
                EditorStyles.wordWrappedLabel);
            EditorGUILayout.EndVertical();
        }

        private void DrawApplyAllButton()
        {
            EditorGUILayout.BeginVertical(EditorStyles.helpBox);
            GUILayout.Label("Apply all settings below in one click.", EditorStyles.wordWrappedLabel);
            GUILayout.Space(4);
            if (GUILayout.Button("Apply All Settings", GUILayout.Height(28)))
            {
                ConfigureProjectSettings.ApplySettings();
                Repaint();
            }
            EditorGUILayout.EndVertical();
        }

        private void DrawSettings()
        {
            DrawSettingRow(
                "Enter Play Mode",
                "Enables DisableDomainReload + DisableSceneReload for faster iteration.\n" +
                "Requires static state to be reset manually via [RuntimeInitializeOnLoadMethod].",
                ConfigureProjectSettings.IsEnterPlayModeConfigured,
                ConfigureProjectSettings.ApplyEnterPlayMode
            );

            DrawSettingRow(
                "API Compatibility: .NET Standard 2.1",
                "Sets API compatibility to .NET Standard 2.1 for Standalone.\n" +
                "Broadens library compatibility and aligns with modern .NET practices.",
                ConfigureProjectSettings.IsApiCompatibilityConfigured,
                ConfigureProjectSettings.ApplyApiCompatibility
            );

            DrawSettingRow(
                "Asset Serialization: Force Text",
                "Forces all assets to serialize as readable YAML text.\n" +
                "Makes diffs meaningful and merges possible in version control.",
                ConfigureProjectSettings.IsAssetSerializationConfigured,
                ConfigureProjectSettings.ApplyAssetSerialization
            );

            DrawSettingRow(
                "Version Control: Visible Meta Files",
                "Ensures .meta files are written to disk so source control can track them.\n" +
                "Prevents GUID regeneration which would break all references to tracked assets.",
                ConfigureProjectSettings.IsVersionControlConfigured,
                ConfigureProjectSettings.ApplyVersionControl
            );
        }

        private void DrawSettingRow(string title, string description, bool isConfigured, System.Action applyAction)
        {
            EditorGUILayout.BeginVertical(EditorStyles.helpBox);

            EditorGUILayout.BeginHorizontal();

            // Status badge
            var prevColor = GUI.color;
            GUI.color = isConfigured ? new Color(0.3f, 0.8f, 0.3f) : new Color(0.7f, 0.7f, 0.7f);
            GUILayout.Label(isConfigured ? "[OK]" : "[  ]", GUILayout.Width(36));
            GUI.color = prevColor;

            // Title
            GUILayout.Label(title, EditorStyles.boldLabel);
            GUILayout.FlexibleSpace();

            // "Configured" label
            if (isConfigured)
            {
                prevColor = GUI.color;
                GUI.color = new Color(0.3f, 0.8f, 0.3f);
                GUILayout.Label("Configured", GUILayout.Width(74));
                GUI.color = prevColor;
            }

            // Action button
            using (new EditorGUI.DisabledScope(isConfigured))
            {
                if (GUILayout.Button(isConfigured ? "Run" : "Run", GUILayout.Width(44)))
                {
                    applyAction();
                    Repaint();
                }
            }

            EditorGUILayout.EndHorizontal();

            GUILayout.Space(2);
            GUILayout.Label(description, EditorStyles.wordWrappedMiniLabel);

            EditorGUILayout.EndVertical();
            GUILayout.Space(2);
        }
    }
}
