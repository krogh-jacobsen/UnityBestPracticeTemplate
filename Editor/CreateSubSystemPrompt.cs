using UnityEditor;
using UnityEngine;

namespace Unity.BestPractices.Editor
{
    /// <summary>
    /// Small popup window that collects a sub-system name and options before running
    /// <see cref="SetupProjectFolders.CreateSubSystem"/> and optionally
    /// <see cref="GenerateAssemblyDefinitions.CreateSubSystemAsmdef"/>.
    /// </summary>
    public class CreateSubSystemPrompt : EditorWindow
    {
        private string m_subSystemName = "";
        private bool m_createAsmdef = true;
        private string m_projectName = "";

        public static void ShowWindow()
        {
            var window = GetWindow<CreateSubSystemPrompt>(true, "Create Game Sub-system", true);
            window.minSize = new Vector2(360, 140);
            window.maxSize = new Vector2(360, 140);
            window.m_projectName = EditorPrefs.GetString(SetupProjectFolders.k_ProjectNamePrefKey, "");
            window.m_subSystemName = "";
            window.m_createAsmdef = true;
            window.ShowUtility();
        }

        private void OnGUI()
        {
            GUILayout.Space(10);

            bool hasProjectName = !string.IsNullOrWhiteSpace(m_projectName);

            if (hasProjectName)
            {
                EditorGUILayout.BeginHorizontal();
                var c = GUI.color;
                GUI.color = new Color(0.5f, 0.8f, 0.5f);
                GUILayout.Label($"Project root:  Assets/{m_projectName}/", EditorStyles.miniLabel);
                GUI.color = c;
                EditorGUILayout.EndHorizontal();
                GUILayout.Space(6);
            }
            else
            {
                var c = GUI.color;
                GUI.color = new Color(0.9f, 0.7f, 0.3f);
                GUILayout.Label("No project name saved. Set one in the dashboard first.", EditorStyles.wordWrappedMiniLabel);
                GUI.color = c;
                GUILayout.Space(6);
            }

            GUILayout.Label("Sub-system name:", EditorStyles.label);
            GUILayout.Space(2);

            GUI.SetNextControlName("SubSystemNameField");
            m_subSystemName = EditorGUILayout.TextField(m_subSystemName);
            EditorGUI.FocusTextInControl("SubSystemNameField");

            GUILayout.Space(6);

            m_createAsmdef = EditorGUILayout.ToggleLeft(
                new GUIContent("Create Assembly Definition", "Generates a .asmdef in Scripts/ referencing the project's Runtime assembly"),
                m_createAsmdef);

            GUILayout.Space(8);

            bool canCreate = !string.IsNullOrWhiteSpace(m_subSystemName) && hasProjectName;
            using (new EditorGUI.DisabledScope(!canCreate))
            {
                if (GUILayout.Button("Create Sub-system", GUILayout.Height(28)))
                {
                    string trimmed = m_subSystemName.Trim();
                    SetupProjectFolders.CreateSubSystem(trimmed);
                    if (m_createAsmdef)
                        GenerateAssemblyDefinitions.CreateSubSystemAsmdef(m_projectName, trimmed);
                    Close();
                    EditorUtility.DisplayDialog(
                        "Sub-system Created",
                        $"Assets/{m_projectName}/{trimmed}/ created with Scripts/, UI/, Prefabs/, and Art/ subfolders." +
                        (m_createAsmdef ? "\n\nAssembly definition generated in Scripts/." : ""),
                        "OK");
                }
            }
        }
    }
}
