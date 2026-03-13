using UnityEditor;
using UnityEngine;

namespace Unity.BestPractices.Editor
{
    /// <summary>
    /// Small popup window that collects a project name before running <see cref="SetupProjectFolders.Execute"/>.
    /// </summary>
    public class ProjectFolderSetupPrompt : EditorWindow
    {
        private string m_projectName = "";

        public static void ShowWindow()
        {
            var window = GetWindow<ProjectFolderSetupPrompt>(true, "Setup Project Folders", true);
            window.minSize = new Vector2(340, 100);
            window.maxSize = new Vector2(340, 100);
            window.m_projectName = EditorPrefs.GetString(SetupProjectFolders.k_ProjectNamePrefKey, "");
            window.ShowUtility();
        }

        private void OnGUI()
        {
            GUILayout.Space(12);
            GUILayout.Label("Project name (becomes the root folder under Assets/):", EditorStyles.wordWrappedLabel);
            GUILayout.Space(4);

            GUI.SetNextControlName("ProjectNameField");
            m_projectName = EditorGUILayout.TextField(m_projectName);
            EditorGUI.FocusTextInControl("ProjectNameField");

            GUILayout.Space(8);

            bool canCreate = !string.IsNullOrWhiteSpace(m_projectName);
            using (new EditorGUI.DisabledScope(!canCreate))
            {
                if (GUILayout.Button("Create Folders", GUILayout.Height(28)))
                {
                    SetupProjectFolders.Execute(m_projectName.Trim());
                    Close();
                }
            }
        }
    }
}
