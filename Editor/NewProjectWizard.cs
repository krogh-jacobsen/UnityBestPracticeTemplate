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
        private string m_projectName = "";

        [MenuItem("Tools/Unity Workbench/New Project Wizard", false, 20)]
        public static void ShowWindow()
        {
            var window = GetWindow<NewProjectWizard>("New Project Wizard");
            window.minSize = new Vector2(440, 520);
            window.Show();
        }

        private void OnEnable()
        {
            if (string.IsNullOrEmpty(m_projectName))
                m_projectName = EditorPrefs.GetString(SetupProjectFolders.k_ProjectNamePrefKey, "");
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
            EditorGUILayout.BeginHorizontal();
            GUILayout.Label("Project name:", GUILayout.Width(90));
            m_projectName = EditorGUILayout.TextField(m_projectName);
            EditorGUILayout.EndHorizontal();
            GUILayout.Space(5);
            bool canRun = !string.IsNullOrWhiteSpace(m_projectName);
            using (new EditorGUI.DisabledScope(!canRun))
            {
                if (GUILayout.Button("Run Full Setup", GUILayout.Height(32)))
                    RunFullSetup();
            }
            EditorGUILayout.EndVertical();
        }

        private void DrawSteps()
        {
            string projectRoot = Path.GetDirectoryName(Application.dataPath);
            string savedProjectName = EditorPrefs.GetString(SetupProjectFolders.k_ProjectNamePrefKey, "");
            bool hasFolders = !string.IsNullOrEmpty(savedProjectName) && AssetDatabase.IsValidFolder($"Assets/{savedProjectName}");
            bool hasGitIgnore = File.Exists(Path.Combine(projectRoot, ".gitignore"));
            bool hasGitAttributes = File.Exists(Path.Combine(projectRoot, ".gitattributes"));
            bool hasEditorConfig = File.Exists(Path.Combine(projectRoot, ".editorconfig"));
            bool hasEnterPlayMode = EditorSettings.enterPlayModeOptionsEnabled;
            bool iterationConfigured = ConfigureIterationSettings.IsScriptChangesConfigured
                && ConfigureIterationSettings.IsAsyncShadersDev
                && ConfigureIterationSettings.IsManagedStrippingDev;
            bool assetPipelineConfigured = ConfigureAssetPipeline.IsAutoRefreshRecommended
                && ConfigureAssetPipeline.IsImportWorkerCountRecommended
                && ConfigureAssetPipeline.IsCompressTexturesConfigured;

            DrawStep(
                "1. Project Folders",
                "Creates the recommended folder hierarchy under Assets/{ProjectName}. Opens a prompt for the project name.",
                hasFolders ? $"Assets/{savedProjectName} exists" : "Not created",
                hasFolders,
                "Run",
                ProjectFolderSetupPrompt.ShowWindow,
                ShowProjectFoldersExplainer
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
                () => EditorApplication.ExecuteMenuItem("Tools/Unity Workbench/Setup/Setup Tags and Layers")
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
                "6. Generate .gitattributes",
                "Creates a .gitattributes that configures UnityYAMLMerge for YAML files, LF endings for source files, and binary flags for assets.",
                hasGitAttributes ? ".gitattributes present" : "Not created",
                hasGitAttributes,
                "Run",
                GenerateGitAttributes.Execute
            );

            DrawStep(
                "7. Generate .editorconfig",
                "Creates an .editorconfig enforcing the package C# naming and formatting conventions.",
                hasEditorConfig ? ".editorconfig present" : "Not created",
                hasEditorConfig,
                "Run",
                GenerateEditorConfig.Execute
            );

            DrawStep(
                "8. Project Settings",
                "Applies recommended Unity 6 settings: Enter Play Mode, IL2CPP backend, .NET Standard 2.1, Input System, Incremental GC, Asset Serialization, Version Control, Create at Origin, New Hierarchy, Asset Manager import location.",
                hasEnterPlayMode ? "Enter Play Mode enabled" : "Not configured",
                hasEnterPlayMode,
                "Run",
                ConfigureProjectSettings.ApplySettings
            );

            DrawStep(
                "9. Iteration Settings",
                "Applies universal iteration defaults: Script Changes → Recompile After Finished Playing, Async Shader Compilation on, Managed Stripping disabled. Keeps your Play sessions intact and editor responsive.",
                iterationConfigured ? "Configured" : "Not configured",
                iterationConfigured,
                "Run",
                () =>
                {
                    ConfigureIterationSettings.ApplyScriptChanges();
                    ConfigureIterationSettings.ApplyAsyncShadersOn();
                    ConfigureIterationSettings.ApplyManagedStrippingDev();
                }
            );

            DrawStep(
                "10. Asset Pipeline",
                "Sets Auto Refresh to Outside Playmode, Import Worker Count to 50%, and enables Compress Textures on Import. Auto Refresh and Worker Count are machine-local preferences (not committed to VCS).",
                assetPipelineConfigured ? "Configured" : "Not configured",
                assetPipelineConfigured,
                "Run",
                () =>
                {
                    ConfigureAssetPipeline.ApplyAutoRefreshRecommended();
                    ConfigureAssetPipeline.ApplyImportWorkerCountRecommended();
                    ConfigureAssetPipeline.ApplyCompressTextures();
                }
            );
        }

        private static void DrawStep(
            string title,
            string description,
            string statusText,
            bool isComplete,
            string buttonLabel,
            System.Action action,
            System.Action explainerAction = null)
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

            // Explainer button
            if (explainerAction != null)
            {
                if (GUILayout.Button("?", GUILayout.Width(22), GUILayout.Height(18)))
                    explainerAction.Invoke();
            }

            // Run button
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

        private static void ShowProjectFoldersExplainer()
        {
            WizardExplainerWindow.Show("Project Folders — Structure", DrawProjectFoldersContent);
        }

        private static void DrawProjectFoldersContent()
        {
            GUILayout.Label(
                "The following folders are created under Assets/_ProjectName/. Existing folders are skipped.",
                EditorStyles.wordWrappedMiniLabel);
            GUILayout.Space(6);

            void Folder(string label, int depth)
            {
                EditorGUI.indentLevel = depth;
                EditorGUILayout.LabelField(label + "/");
            }

            Folder("Assets", 0);
            Folder("  _ProjectName", 0);

            EditorGUI.indentLevel = 0;
            GUILayout.Space(4);
            GUILayout.Label("Art", EditorStyles.boldLabel);
            Folder("Art", 1);
            Folder("Animations", 2);
            Folder("Clips", 3);
            Folder("Controllers", 3);
            Folder("Audio", 2);
            Folder("Ambience", 3);
            Folder("Music", 3);
            Folder("SFX", 3);
            Folder("UI", 3);
            Folder("Fonts", 2);
            Folder("Materials", 2);
            Folder("Physics", 3);
            Folder("Models", 2);
            Folder("Shaders", 2);
            Folder("Sprites", 2);
            Folder("Textures", 2);
            Folder("Albedo", 3);
            Folder("Normal", 3);
            Folder("Roughness", 3);
            Folder("Mask", 3);
            Folder("HDRI", 3);
            Folder("VFX", 2);

            EditorGUI.indentLevel = 0;
            GUILayout.Space(4);
            GUILayout.Label("Code", EditorStyles.boldLabel);
            Folder("Scripts", 1);
            Folder("Core", 2);
            Folder("UI", 2);
            Folder("Utilities", 2);
            Folder("Editor", 1);

            EditorGUI.indentLevel = 0;
            GUILayout.Space(4);
            GUILayout.Label("Data", EditorStyles.boldLabel);
            Folder("Data", 1);
            Folder("Prefabs", 1);
            Folder("Scenes", 1);
            Folder("Settings", 1);

            EditorGUI.indentLevel = 0;
            GUILayout.Space(4);
            GUILayout.Label("UI Toolkit", EditorStyles.boldLabel);
            Folder("UI", 1);
            Folder("Sprites", 2);
            Folder("Atlas", 3);
            Folder("UXML", 2);
            Folder("USS", 2);
            Folder("Settings", 2);

            EditorGUI.indentLevel = 0;
            GUILayout.Space(4);
            GUILayout.Label("Special Unity Folders", EditorStyles.boldLabel);
            Folder("Resources", 1);
            Folder("StreamingAssets", 1);

            EditorGUI.indentLevel = 0;
            GUILayout.Space(4);
            GUILayout.Label("Organisation", EditorStyles.boldLabel);
            Folder("Documentation", 1);
            Folder("Plugins", 1);
            Folder("ThirdParty", 1);

            EditorGUI.indentLevel = 0;
        }

        private void RunFullSetup()
        {
            SetupProjectFolders.Execute(m_projectName.Trim());
            GenerateAssemblyDefinitions.Execute();
            ConfigurePresets.ApplyAllPresets();
            EditorApplication.ExecuteMenuItem("Tools/Unity Workbench/Setup/Setup Tags and Layers");
            GenerateGitIgnore.Execute();
            GenerateGitAttributes.Execute();
            GenerateEditorConfig.Execute();
            ConfigureProjectSettings.ApplySettings();
            ConfigureIterationSettings.ApplyScriptChanges();
            ConfigureIterationSettings.ApplyAsyncShadersOn();
            ConfigureIterationSettings.ApplyManagedStrippingDev();
            ConfigureAssetPipeline.ApplyAutoRefreshRecommended();
            ConfigureAssetPipeline.ApplyImportWorkerCountRecommended();
            ConfigureAssetPipeline.ApplyCompressTextures();

            Debug.Log("[Best Practices] Full project setup complete.");
        }
    }
}
