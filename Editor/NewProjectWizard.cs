using System.IO;
using UnityEditor;
using UnityEngine;

namespace Unity.BestPractices.Editor
{
    /// <summary>
    /// Guided wizard for setting up a new Unity project with Best Practices.
    /// Run all setup steps in one click, or execute each step individually.
    /// Accessible via <b>Window → Best Practices → New Project Wizard</b>.
    /// </summary>
    public class NewProjectWizard : EditorWindow
    {
        private Vector2 m_ScrollPosition;

        [MenuItem("Window/Best Practices/New Project Wizard")]
        public static void ShowWindow()
        {
            var window = GetWindow<NewProjectWizard>("New Project Wizard");
            window.minSize = new Vector2(440, 520);
            window.Show();
        }

        private void OnGUI()
        {
            m_ScrollPosition = EditorGUILayout.BeginScrollView(m_ScrollPosition);

            DrawHeader();
            GUILayout.Space(8);
            DrawRunAllButton();
            GUILayout.Space(8);
            DrawSteps();

            EditorGUILayout.EndScrollView();
        }

        private void DrawHeader()
        {
            EditorGUILayout.BeginVertical(EditorStyles.helpBox);
            GUILayout.Label("NEW PROJECT WIZARD", EditorStyles.boldLabel);
            GUILayout.Space(2);
            GUILayout.Label(
                "Run all recommended setup steps for a new Unity project, or execute each step individually.",
                EditorStyles.wordWrappedLabel);
            EditorGUILayout.EndVertical();
        }

        private void DrawRunAllButton()
        {
            EditorGUILayout.BeginVertical(EditorStyles.helpBox);
            GUILayout.Label(
                "Run Full Setup applies all steps below in order without individual confirmation dialogs.",
                EditorStyles.wordWrappedLabel);
            GUILayout.Space(5);
            if (GUILayout.Button("Run Full Setup", GUILayout.Height(32)))
                RunFullSetup();
            EditorGUILayout.EndVertical();
        }

        private void DrawSteps()
        {
            string projectRoot = Path.GetDirectoryName(Application.dataPath);
            bool hasFolders = AssetDatabase.IsValidFolder("Assets/_ProjectName");
            bool hasGitIgnore = File.Exists(Path.Combine(projectRoot, ".gitignore"));
            bool hasEditorConfig = File.Exists(Path.Combine(projectRoot, ".editorconfig"));
            bool hasEnterPlayMode = EditorSettings.enterPlayModeOptionsEnabled;

            DrawStep(
                "1. Project Folders",
                "Creates the recommended Assets/_ProjectName folder hierarchy.",
                hasFolders ? "Assets/_ProjectName exists" : "Not created",
                hasFolders,
                "Run",
                SetupProjectFolders.Execute
            );

            DrawStep(
                "2. Assembly Definitions",
                "Generates .asmdef files in Scripts, Editor, and Tests using Company.Product as the namespace.",
                "Run after step 1",
                false,
                "Run",
                GenerateAssemblyDefinitions.Execute
            );

            DrawStep(
                "3. Import Presets",
                "Registers audio and texture import presets in the Preset Manager.",
                "Requires preset assets in the package",
                false,
                "Run",
                ConfigurePresets.Execute
            );

            DrawStep(
                "4. Tags & Layers",
                "Registers recommended tags and physics layers from a ScriptableObject config.",
                "",
                false,
                "Run",
                () => EditorApplication.ExecuteMenuItem("Window/Best Practices/Setup Tags and Layers")
            );

            DrawStep(
                "5. Generate .gitignore",
                "Creates a Unity-optimised .gitignore at the project root.",
                hasGitIgnore ? ".gitignore present" : "Not created",
                hasGitIgnore,
                "Run",
                GenerateGitIgnore.Execute
            );

            DrawStep(
                "6. Generate .editorconfig",
                "Creates an .editorconfig enforcing the package C# naming and formatting conventions.",
                hasEditorConfig ? ".editorconfig present" : "Not created",
                hasEditorConfig,
                "Run",
                GenerateEditorConfig.Execute
            );

            DrawStep(
                "7. Project Settings",
                "Applies recommended Unity 6 settings: Enter Play Mode, IL2CPP scripting backend, .NET Standard 2.1.",
                hasEnterPlayMode ? "Enter Play Mode enabled" : "Not configured",
                hasEnterPlayMode,
                "Run",
                ConfigureProjectSettings.ApplySettings
            );
        }

        private static void DrawStep(
            string title,
            string description,
            string statusText,
            bool isComplete,
            string buttonLabel,
            System.Action action)
        {
            EditorGUILayout.BeginVertical(EditorStyles.helpBox);
            EditorGUILayout.BeginHorizontal();

            // Status indicator
            var prevColor = GUI.color;
            GUI.color = isComplete ? new Color(0.3f, 0.8f, 0.3f) : new Color(0.7f, 0.7f, 0.7f);
            GUILayout.Label(isComplete ? "[OK]" : "[  ]", GUILayout.Width(36));
            GUI.color = prevColor;

            // Title
            GUILayout.Label(title, EditorStyles.boldLabel);
            GUILayout.FlexibleSpace();

            // Button
            if (GUILayout.Button(buttonLabel, GUILayout.Width(60)))
                action?.Invoke();

            EditorGUILayout.EndHorizontal();

            // Description
            GUILayout.Label(description, EditorStyles.wordWrappedMiniLabel);

            // Status text
            if (!string.IsNullOrEmpty(statusText))
            {
                GUI.color = isComplete ? new Color(0.5f, 0.8f, 0.5f) : new Color(0.6f, 0.6f, 0.6f);
                GUILayout.Label(statusText, EditorStyles.miniLabel);
                GUI.color = Color.white;
            }

            EditorGUILayout.EndVertical();
            GUILayout.Space(3);
        }

        private static void RunFullSetup()
        {
            SetupProjectFolders.Execute();
            GenerateAssemblyDefinitions.Execute();
            ConfigurePresets.Execute();
            EditorApplication.ExecuteMenuItem("Window/Best Practices/Setup Tags and Layers");
            GenerateGitIgnore.Execute();
            GenerateEditorConfig.Execute();
            ConfigureProjectSettings.ApplySettings();

            Debug.Log("[Best Practices] Full project setup complete.");
        }
    }
}
