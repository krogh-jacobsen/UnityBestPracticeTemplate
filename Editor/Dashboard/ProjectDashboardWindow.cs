using UnityEditor;
using UnityEditorInternal;
using UnityEngine;
using System.IO;
using Unity.BestPractices.Editor;
using UnityBestPractices.Editor.Validator;

namespace UnityBestPractices.Editor.Dashboard
{
    public class ProjectDashboardWindow : EditorWindow
    {
        private ProjectDashboardData _data;
        private Vector2 _scrollPosition;
        private bool _showLLMFiles = false;
        private bool _showSkills = false;
        private bool _showTools = true;
        private bool _showProjectSettings = true;
        private bool _showWindows = true;
        private bool _showPresets = false;

        [MenuItem("Tools/Unity Best Practices/Project Dashboard")]
        public static void ShowWindow()
        {
            var window = GetWindow<ProjectDashboardWindow>("Project Dashboard");
            window.minSize = new Vector2(400, 500);
            window.Show();
        }

        private void OnEnable()
        {
            RefreshData();
        }

        private void OnFocus()
        {
            RefreshData();
        }

        private void RefreshData()
        {
            _data = ProjectDashboardData.Gather();
        }

        private void OnGUI()
        {
            if (_data == null)
            {
                RefreshData();
                return;
            }

            _scrollPosition = EditorGUILayout.BeginScrollView(_scrollPosition);

            // Header
            DrawHeader();

            GUILayout.Space(10);

            // Windows
            DrawWindows();

            GUILayout.Space(10);

            // Tools quick-actions
            DrawTools();

            GUILayout.Space(10);

            // Project Settings
            DrawProjectSettings();

            GUILayout.Space(10);

            GUILayout.Space(10);

            // LLM Instruction Files
            DrawLLMInstructions();

            GUILayout.Space(10);

            // Agent Skills
            DrawAgentSkills();

            EditorGUILayout.EndScrollView();
        }

        private void DrawHeader()
        {
            EditorGUILayout.BeginVertical(EditorStyles.helpBox);

            EditorGUILayout.BeginHorizontal();
            GUILayout.Label("PROJECT DASHBOARD", EditorStyles.boldLabel);
            GUILayout.FlexibleSpace();
            if (_data.PackageVersion != null)
                GUILayout.Label($"v{_data.PackageVersion}", EditorStyles.miniLabel);
            EditorGUILayout.EndHorizontal();

            GUILayout.Space(5);

            EditorGUILayout.BeginHorizontal();
            if (GUILayout.Button("Refresh", GUILayout.Width(100)))
                RefreshData();
            if (GUILayout.Button("New Project Wizard", GUILayout.Width(160)))
                NewProjectWizard.ShowWindow();
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.EndVertical();
        }

        private void DrawStatusLine(string label, string value, bool isGood)
        {
            EditorGUILayout.BeginHorizontal();

            // Icon
            string icon = isGood ? "" : "�";
            Color iconColor = isGood ? new Color(0.3f, 0.8f, 0.3f) : new Color(0.9f, 0.7f, 0.2f);

            var originalColor = GUI.color;
            GUI.color = iconColor;
            GUILayout.Label(icon, GUILayout.Width(20));
            GUI.color = originalColor;

            // Label and Value
            GUILayout.Label(label + ":", GUILayout.Width(120));
            GUILayout.Label(value);

            EditorGUILayout.EndHorizontal();
        }

        private void DrawLLMInstructions()
        {
            EditorGUILayout.BeginVertical(EditorStyles.helpBox);

            string projectRoot = Path.GetDirectoryName(Application.dataPath);
            int llmTotal = _data.LLMInstructionFilesCount;
            int llmInstalled = 0;
            if (_data.LLMInstructionFiles != null)
            {
                foreach (var f in _data.LLMInstructionFiles)
                {
                    string fp = Path.GetFullPath(Path.Combine(Application.dataPath, "..", f.AssetPath));
                    if (File.Exists(CopyAIFilesToProject.GetLLMInstructionDestPath(fp, projectRoot))) llmInstalled++;
                }
            }
            string header = $"LLM INSTRUCTION FILES — {llmInstalled}/{llmTotal} installed";
            _showLLMFiles = EditorGUILayout.Foldout(_showLLMFiles, header, true, EditorStyles.foldoutHeader);
            GUILayout.Label("Markdown files that give AI assistants context about your project's coding standards and Unity conventions. Copied to .github/instructions/ so GitHub Copilot, Cursor, and other editors pick them up automatically.", EditorStyles.wordWrappedMiniLabel);

            if (_showLLMFiles && _data.LLMInstructionFiles != null)
            {
                GUILayout.Space(4);

                foreach (var file in _data.LLMInstructionFiles)
                {
                    string fullPath = Path.GetFullPath(
                        Path.Combine(Application.dataPath, "..", file.AssetPath));
                    string destPath = CopyAIFilesToProject.GetLLMInstructionDestPath(fullPath, projectRoot);
                    bool installed = File.Exists(destPath);
                    bool outdated = installed && CopyAIFilesToProject.LLMInstructionIsOutdated(fullPath, projectRoot);

                    EditorGUILayout.BeginHorizontal();

                    var prevColor = GUI.color;
                    GUI.color = outdated ? new Color(0.9f, 0.75f, 0.2f)
                        : installed ? new Color(0.3f, 0.8f, 0.3f)
                        : new Color(0.7f, 0.7f, 0.7f);
                    GUILayout.Label(outdated ? "[Outdated]" : installed ? "[Found]" : "[  ]",
                        GUILayout.Width(outdated ? 70 : installed ? 56 : 36));
                    GUI.color = prevColor;

                    GUILayout.Label(file.DisplayName, EditorStyles.label);
                    GUILayout.FlexibleSpace();

                    if (installed)
                    {
                        if (GUILayout.Button(new GUIContent("Open", "Open the local copy of this file"), GUILayout.Width(50)))
                            InternalEditorUtility.OpenFileAtLineExternal(destPath, 1, 0);

                        if (outdated && GUILayout.Button(new GUIContent("Update", "Overwrite the local copy with the latest version from the package"), GUILayout.Width(60)))
                            CopyAIFilesToProject.CopySingleLLMInstruction(fullPath, projectRoot);
                    }
                    else
                    {
                        if (GUILayout.Button(new GUIContent("View", "View this file from the package"), GUILayout.Width(50)))
                            InternalEditorUtility.OpenFileAtLineExternal(fullPath, 1, 0);

                        if (GUILayout.Button(new GUIContent("Add", "Copy this file to .github/instructions/ in your project"), GUILayout.Width(40)))
                            CopyAIFilesToProject.CopySingleLLMInstruction(fullPath, projectRoot);
                    }

                    EditorGUILayout.EndHorizontal();
                }

            }

            // ── AI Assistance ─────────────────────────────────────────────
            GUILayout.Label("Quick setup", EditorStyles.miniLabel);

            DrawToolCard(
                "Copy All LLM Instructions",
                "Copies all LLM instruction files to .github/instructions/ and .github/copilot-instructions.md so AI assistants can reference them locally.",
                false, "",
                "Copy to Project", CopyAIFilesToProject.ExecuteLLMInstructionsOnly, 110, showBadge: false);

            EditorGUILayout.EndVertical();
        }

        private void DrawAgentSkills()
        {
            EditorGUILayout.BeginVertical(EditorStyles.helpBox);

            string projectRoot = Path.GetDirectoryName(Application.dataPath);
            int skillTotal = _data.AgentSkillFilesCount;
            int skillInstalled = 0;
            if (_data.AgentSkillFiles != null)
            {
                foreach (var s in _data.AgentSkillFiles)
                {
                    string fp = Path.GetFullPath(Path.Combine(Application.dataPath, "..", s.AssetPath));
                    if (File.Exists(CopyAIFilesToProject.GetAgentSkillPromptDestPath(fp, projectRoot))) skillInstalled++;
                }
            }
            string header = $"AGENT SKILLS — {skillInstalled}/{skillTotal} installed";
            _showSkills = EditorGUILayout.Foldout(_showSkills, header, true, EditorStyles.foldoutHeader);
            GUILayout.Label("Reusable prompt templates that define specialized AI workflows for your project. Copied to .github/prompts/ (GitHub Copilot reusable prompts) and .claude/commands/ (Claude Code slash commands).", EditorStyles.wordWrappedMiniLabel);

            if (_showSkills && _data.AgentSkillFiles != null)
            {
                GUILayout.Space(4);

                foreach (var skill in _data.AgentSkillFiles)
                {
                    string fullPath = Path.GetFullPath(
                        Path.Combine(Application.dataPath, "..", skill.AssetPath));
                    string destPath = CopyAIFilesToProject.GetAgentSkillPromptDestPath(fullPath, projectRoot);
                    bool installed = File.Exists(destPath);
                    bool outdated = installed && CopyAIFilesToProject.AgentSkillIsOutdated(fullPath, projectRoot);

                    EditorGUILayout.BeginHorizontal();

                    var prevColor = GUI.color;
                    GUI.color = outdated ? new Color(0.9f, 0.75f, 0.2f)
                        : installed ? new Color(0.3f, 0.8f, 0.3f)
                        : new Color(0.7f, 0.7f, 0.7f);
                    GUILayout.Label(outdated ? "[Outdated]" : installed ? "[Found]" : "[  ]",
                        GUILayout.Width(outdated ? 70 : installed ? 56 : 36));
                    GUI.color = prevColor;

                    GUILayout.Label(skill.DisplayName, EditorStyles.label);
                    GUILayout.FlexibleSpace();

                    if (installed)
                    {
                        if (GUILayout.Button(new GUIContent("Open", "Open the local copy of this skill"), GUILayout.Width(50)))
                            InternalEditorUtility.OpenFileAtLineExternal(destPath, 1, 0);

                        if (outdated && GUILayout.Button(new GUIContent("Update", "Overwrite the local copy with the latest version from the package"), GUILayout.Width(60)))
                            CopyAIFilesToProject.CopySingleAgentSkill(fullPath, projectRoot);
                    }
                    else
                    {
                        if (GUILayout.Button(new GUIContent("View", "View this skill file from the package"), GUILayout.Width(50)))
                            InternalEditorUtility.OpenFileAtLineExternal(fullPath, 1, 0);

                        if (GUILayout.Button(new GUIContent("Add", "Copy to .github/prompts/ (Copilot) and .claude/commands/ (Claude Code)"), GUILayout.Width(40)))
                            CopyAIFilesToProject.CopySingleAgentSkill(fullPath, projectRoot);
                    }

                    EditorGUILayout.EndHorizontal();
                }

            }

            // ── AI Assistance ─────────────────────────────────────────────
            GUILayout.Label("Quick setup", EditorStyles.miniLabel);

            DrawToolCard(
                "Copy All Agent Skills",
                "Copies all agent skill files to .github/prompts/ (Copilot reusable prompts) and .claude/commands/ (Claude Code slash commands).",
                false, "",
                "Copy to Project", CopyAIFilesToProject.ExecuteAgentSkillsOnly, 110, showBadge: false);


            EditorGUILayout.EndVertical();
        }

        private void DrawProjectSettings()
        {
            EditorGUILayout.BeginVertical(EditorStyles.helpBox);

            int settingsOk = (ConfigureProjectSettings.IsEnterPlayModeConfigured ? 1 : 0)
                + (ConfigureProjectSettings.IsIL2CPPConfigured ? 1 : 0)
                + (ConfigureProjectSettings.IsApiCompatibilityConfigured ? 1 : 0)
                + (ConfigureProjectSettings.IsAssetSerializationConfigured ? 1 : 0)
                + (ConfigureProjectSettings.IsVersionControlConfigured ? 1 : 0)
                + (ConfigureProjectSettings.IsInputSystemConfigured ? 1 : 0)
                + (ConfigureProjectSettings.IsIncrementalGCConfigured ? 1 : 0);
            _showProjectSettings = EditorGUILayout.Foldout(_showProjectSettings, $"PROJECT SETTINGS — {settingsOk}/7 configured", true, EditorStyles.foldoutHeader);

            if (_showProjectSettings)
            {
                GUILayout.Space(4);

                bool allOk = settingsOk == 7;

                using (new EditorGUI.DisabledScope(allOk))
                {
                    if (GUILayout.Button("Apply All Settings", GUILayout.Height(24)))
                    {
                        ConfigureProjectSettings.ApplySettings();
                        RefreshData();
                    }
                }

                GUILayout.Space(4);

                DrawSettingCard(
                    "Enter Play Mode",
                    "Enables DisableDomainReload + DisableSceneReload for faster iteration.\nRequires static state to be reset manually via [RuntimeInitializeOnLoadMethod].",
                    ConfigureProjectSettings.IsEnterPlayModeConfigured,
                    ConfigureProjectSettings.ApplyEnterPlayMode);

                DrawSettingCard(
                    "Scripting Backend: IL2CPP",
                    "Sets IL2CPP as the scripting backend for Standalone, Android and iOS builds.\nImproves runtime performance and enables full AOT compilation.",
                    ConfigureProjectSettings.IsIL2CPPConfigured,
                    ConfigureProjectSettings.ApplyIL2CPP);

                DrawSettingCard(
                    "API Compatibility: .NET Standard 2.1",
                    "Sets API compatibility to .NET Standard 2.1 for Standalone.\nBroadens library compatibility and aligns with modern .NET practices.",
                    ConfigureProjectSettings.IsApiCompatibilityConfigured,
                    ConfigureProjectSettings.ApplyApiCompatibility);

                DrawSettingCard(
                    "Asset Serialization: Force Text",
                    "Forces all assets to serialize as readable YAML text.\nMakes diffs meaningful and merges possible in version control.",
                    ConfigureProjectSettings.IsAssetSerializationConfigured,
                    ConfigureProjectSettings.ApplyAssetSerialization);

                DrawSettingCard(
                    "Version Control: Visible Meta Files",
                    "Ensures .meta files are written to disk so source control can track them.\nPrevents GUID regeneration which would break all references to tracked assets.",
                    ConfigureProjectSettings.IsVersionControlConfigured,
                    ConfigureProjectSettings.ApplyVersionControl);

                DrawSettingCard(
                    "Input System: New Input System Package",
                    "Switches Active Input Handling to the new Input System package.\nRequires com.unity.inputsystem to be installed. A Unity restart may be needed after applying.",
                    ConfigureProjectSettings.IsInputSystemConfigured,
                    ConfigureProjectSettings.ApplyInputSystem);

                DrawSettingCard(
                    "Incremental GC",
                    "Enables incremental garbage collection, spreading GC work across multiple frames.\nReduces frame-rate spikes caused by full GC passes during gameplay.",
                    ConfigureProjectSettings.IsIncrementalGCConfigured,
                    ConfigureProjectSettings.ApplyIncrementalGC);
            }

            EditorGUILayout.EndVertical();
        }

        private void DrawSettingCard(string title, string description, bool isConfigured, System.Action applyAction)
        {
            EditorGUILayout.BeginVertical(EditorStyles.helpBox);
            EditorGUILayout.BeginHorizontal();

            var prevColor = GUI.color;
            GUI.color = isConfigured ? new Color(0.3f, 0.8f, 0.3f) : new Color(0.7f, 0.7f, 0.7f);
            GUILayout.Label(isConfigured ? "[Configured]" : "[  ]", GUILayout.Width(isConfigured ? 84 : 36));
            GUI.color = prevColor;

            GUILayout.Label(title, EditorStyles.boldLabel);
            GUILayout.FlexibleSpace();

            if (isConfigured)
            {
                if (GUILayout.Button("Open", GUILayout.Width(44)))
                    SettingsService.OpenProjectSettings("Project/Player");
            }
            else
            {
                if (GUILayout.Button("Run", GUILayout.Width(44)))
                {
                    applyAction?.Invoke();
                    Repaint();
                }
            }

            EditorGUILayout.EndHorizontal();
            GUILayout.Label(description, EditorStyles.wordWrappedMiniLabel);
            EditorGUILayout.EndVertical();
            GUILayout.Space(2);
        }

        private void DrawTools()
        {
            EditorGUILayout.BeginVertical(EditorStyles.helpBox);

            string projectRoot = Path.GetDirectoryName(Application.dataPath);
            string gitignorePath = Path.Combine(projectRoot, ".gitignore");
            string editorconfigPath = Path.Combine(projectRoot, ".editorconfig");
            bool gitignoreExists = File.Exists(gitignorePath);
            bool editorconfigExists = File.Exists(editorconfigPath);
            bool foldersOk = _data.ExistingFoldersCount >= _data.TotalRecommendedFolders * 0.7f;
            int presetCount = (ConfigurePresets.IsAmbienceConfigured ? 1 : 0)
                + (ConfigurePresets.IsMusicConfigured ? 1 : 0)
                + (ConfigurePresets.IsSFXConfigured ? 1 : 0)
                + (ConfigurePresets.IsUIAudioConfigured ? 1 : 0)
                + (ConfigurePresets.IsSingleSpriteConfigured ? 1 : 0)
                + (ConfigurePresets.IsSpriteAtlasConfigured ? 1 : 0)
                + (ConfigurePresets.IsAlbedoConfigured ? 1 : 0)
                + (ConfigurePresets.IsNormalConfigured ? 1 : 0)
                + (ConfigurePresets.IsRoughnessConfigured ? 1 : 0)
                + (ConfigurePresets.IsMaskConfigured ? 1 : 0)
                + (ConfigurePresets.IsHDRIConfigured ? 1 : 0)
                + (ConfigurePresets.IsFBXModelConfigured ? 1 : 0)
                + (ConfigurePresets.IsFBXAnimationConfigured ? 1 : 0);
            int setupDone = (foldersOk ? 1 : 0) + (gitignoreExists ? 1 : 0) + (editorconfigExists ? 1 : 0) + (presetCount == 13 ? 1 : 0);

            _showTools = EditorGUILayout.Foldout(_showTools, $"PROJECT CONFIGURATION — {setupDone}/4 done", true, EditorStyles.foldoutHeader);

            if (_showTools)
            {
                GUILayout.Space(4);

                // ── Setup Actions ─────────────────────────────────────────────
                GUILayout.Label("Setup Actions", EditorStyles.miniLabel);

                DrawToolCard(
                    "Setup Project Folders",
                    "Creates the recommended folder structure under Assets/_ProjectName (Art, Audio, Prefabs, Scripts, Scenes, Settings, UI).",
                    foldersOk,
                    foldersOk ? $"{_data.ExistingFoldersCount}/{_data.TotalRecommendedFolders} folders present" : $"{_data.ExistingFoldersCount}/{_data.TotalRecommendedFolders} folders — run to create missing ones",
                    "Run", SetupProjectFolders.Execute);

                DrawFileToolCard(
                    "Generate .gitignore",
                    "Creates a Unity-optimised .gitignore at the project root, excluding Library, Temp, build outputs and IDE files.",
                    gitignorePath, GenerateGitIgnore.Execute);

                DrawFileToolCard(
                    "Generate .editorconfig",
                    "Creates an .editorconfig enforcing C# naming and formatting conventions (Allman braces, 4-space indent, m_ prefix rules).",
                    editorconfigPath, GenerateEditorConfig.Execute);

                DrawToolCard(
                    "Generate Assembly Definitions",
                    "Generates .asmdef files for Scripts, Editor and Tests using Company.Product as the namespace root. Run after Setup Project Folders.",
                    false, "",
                    "Run", GenerateAssemblyDefinitions.Execute);

                // ── Import Presets ────────────────────────────────────────────
                GUILayout.Space(4);
                GUILayout.Label("Import Presets", EditorStyles.miniLabel);

                EditorGUILayout.BeginVertical(EditorStyles.helpBox);

                bool allPresetsOk = presetCount == 13;

                EditorGUILayout.BeginHorizontal();
                _showPresets = EditorGUILayout.Foldout(_showPresets, $"Configure Import Presets — {presetCount}/13 registered", true, EditorStyles.foldoutHeader);
                GUILayout.FlexibleSpace();
                using (new EditorGUI.DisabledScope(allPresetsOk))
                {
                    if (GUILayout.Button("Apply All", GUILayout.Width(70)))
                    {
                        ConfigurePresets.ApplyAllPresets();
                        Repaint();
                    }
                }
                EditorGUILayout.EndHorizontal();

                if (_showPresets)
                {
                    GUILayout.Label("AUDIO", EditorStyles.centeredGreyMiniLabel);
                    DrawPresetCard("Ambience", "glob: Assets/_ProjectName/Art/Audio/Ambience/**", ConfigurePresets.IsAmbienceConfigured, ConfigurePresets.ApplyAmbience);
                    DrawPresetCard("Music", "glob: Assets/_ProjectName/Art/Audio/Music/**", ConfigurePresets.IsMusicConfigured, ConfigurePresets.ApplyMusic);
                    DrawPresetCard("SFX", "glob: Assets/_ProjectName/Art/Audio/SFX/**", ConfigurePresets.IsSFXConfigured, ConfigurePresets.ApplySFX);
                    DrawPresetCard("UI Audio", "glob: Assets/_ProjectName/Art/Audio/UI/**", ConfigurePresets.IsUIAudioConfigured, ConfigurePresets.ApplyUIAudio);

                    GUILayout.Space(2);
                    GUILayout.Label("TEXTURES", EditorStyles.centeredGreyMiniLabel);
                    DrawPresetCard("Single Sprite", "glob: Assets/_ProjectName/UI/Sprites/**", ConfigurePresets.IsSingleSpriteConfigured, ConfigurePresets.ApplySingleSprite);
                    DrawPresetCard("Sprite Atlas", "glob: Assets/_ProjectName/UI/Sprites/Atlas/**", ConfigurePresets.IsSpriteAtlasConfigured, ConfigurePresets.ApplySpriteAtlas);
                    DrawPresetCard("Albedo", "glob: Assets/_ProjectName/Art/Textures/Albedo/**", ConfigurePresets.IsAlbedoConfigured, ConfigurePresets.ApplyAlbedo);
                    DrawPresetCard("Normal", "glob: Assets/_ProjectName/Art/Textures/Normal/**", ConfigurePresets.IsNormalConfigured, ConfigurePresets.ApplyNormal);
                    DrawPresetCard("Roughness", "glob: Assets/_ProjectName/Art/Textures/Roughness/**", ConfigurePresets.IsRoughnessConfigured, ConfigurePresets.ApplyRoughness);
                    DrawPresetCard("Mask", "glob: Assets/_ProjectName/Art/Textures/Mask/**", ConfigurePresets.IsMaskConfigured, ConfigurePresets.ApplyMask);
                    DrawPresetCard("HDRI", "glob: Assets/_ProjectName/Art/Textures/HDRI/**", ConfigurePresets.IsHDRIConfigured, ConfigurePresets.ApplyHDRI);

                    GUILayout.Space(2);
                    GUILayout.Label("MODELS", EditorStyles.centeredGreyMiniLabel);
                    DrawPresetCard("FBX Model", "glob: Assets/_ProjectName/Art/Models/**", ConfigurePresets.IsFBXModelConfigured, ConfigurePresets.ApplyFBXModel);

                    GUILayout.Space(2);
                    GUILayout.Label("ANIMATIONS", EditorStyles.centeredGreyMiniLabel);
                    DrawPresetCard("FBX Animation", "glob: Assets/_ProjectName/Art/Animations/**", ConfigurePresets.IsFBXAnimationConfigured, ConfigurePresets.ApplyFBXAnimation);
                }

                EditorGUILayout.EndVertical();

                GUILayout.Space(6);

                // ── AI Assistance ─────────────────────────────────────────────
                GUILayout.Label("AI Assistance", EditorStyles.miniLabel);

                DrawToolCard(
                    "AI Files — LLM Instructions + Skills",
                    "Copies LLM instruction files to .github/instructions/ and AgentSkill files to .github/prompts/ and .claude/commands/ so AI assistants can reference them locally.",
                    false, "",
                    "Copy to Project", CopyAIFilesToProject.Execute, 110);
            }

            EditorGUILayout.EndVertical();
        }

        private void DrawWindows()
        {
            EditorGUILayout.BeginVertical(EditorStyles.helpBox);

            _showWindows = EditorGUILayout.Foldout(_showWindows, "WINDOWS", true, EditorStyles.foldoutHeader);

            if (_showWindows)
            {
                GUILayout.Space(4);

                DrawWindowCard("New Project Wizard",
                    "Guided wizard that runs all setup steps in order — use this to configure a new project from scratch.",
                    NewProjectWizard.ShowWindow);

                DrawWindowCard("PlayerPrefs Inspector",
                    "View, edit and delete all PlayerPrefs keys stored for this project.",
                    PlayerPrefsInspectorWindow.ShowWindow);

                DrawWindowCard("Layer Collision Matrix",
                    "Visual editor for configuring which physics layers collide with each other.",
                    LayerCollisionMatrixWindow.ShowWindow);

                DrawWindowCard("Project Health",
                    "Runs validation checks across the project and lists errors, warnings and available auto-fixes.",
                    ProjectHealthWindow.ShowWindow);
            }

            EditorGUILayout.EndVertical();
        }

        private static void DrawWindowCard(string title, string description, System.Action openAction)
        {
            EditorGUILayout.BeginHorizontal(EditorStyles.helpBox);

            EditorGUILayout.BeginVertical();
            GUILayout.Label(title, EditorStyles.boldLabel);
            GUILayout.Label(description, EditorStyles.wordWrappedMiniLabel);
            EditorGUILayout.EndVertical();

            GUILayout.FlexibleSpace();

            var prevColor = GUI.color;
            GUI.color = new Color(0.6f, 0.85f, 1f);
            if (GUILayout.Button("Open", GUILayout.Width(50), GUILayout.ExpandHeight(true)))
                openAction?.Invoke();
            GUI.color = prevColor;

            EditorGUILayout.EndHorizontal();
            GUILayout.Space(3);
        }

        private static void DrawPresetCard(string title, string filter, bool isConfigured, System.Action applyAction)
        {
            EditorGUILayout.BeginHorizontal();

            var prevColor = GUI.color;
            GUI.color = isConfigured ? new Color(0.3f, 0.8f, 0.3f) : new Color(0.7f, 0.7f, 0.7f);
            GUILayout.Label(isConfigured ? "[Configured]" : "[  ]", GUILayout.Width(isConfigured ? 84 : 36));
            GUI.color = prevColor;

            GUILayout.Label(title, EditorStyles.boldLabel, GUILayout.Width(110));
            GUI.color = new Color(0.6f, 0.6f, 0.6f);
            GUILayout.Label(filter, EditorStyles.miniLabel);
            GUI.color = prevColor;
            GUILayout.FlexibleSpace();

            if (isConfigured)
            {
                if (GUILayout.Button("Open", GUILayout.Width(44)))
                    SettingsService.OpenProjectSettings("Project/Preset Manager");
            }
            else
            {
                if (GUILayout.Button("Run", GUILayout.Width(44)))
                {
                    applyAction?.Invoke();
                }
            }

            EditorGUILayout.EndHorizontal();
        }

        private static void DrawToolCard(string title, string description, bool isComplete, string statusText,
            string buttonLabel, System.Action action, int buttonWidth = 60, bool showBadge = true)
        {
            EditorGUILayout.BeginVertical(EditorStyles.helpBox);
            EditorGUILayout.BeginHorizontal();

            if (showBadge)
            {
                var prevColor = GUI.color;
                GUI.color = isComplete ? new Color(0.3f, 0.8f, 0.3f) : new Color(0.7f, 0.7f, 0.7f);
                GUILayout.Label(isComplete ? "[OK]" : "[  ]", GUILayout.Width(36));
                GUI.color = prevColor;
            }

            GUILayout.Label(title, EditorStyles.boldLabel);
            GUILayout.FlexibleSpace();

            if (GUILayout.Button(buttonLabel, GUILayout.Width(buttonWidth)))
                action?.Invoke();

            EditorGUILayout.EndHorizontal();

            GUILayout.Label(description, EditorStyles.wordWrappedMiniLabel);

            if (!string.IsNullOrEmpty(statusText))
            {
                var statusColor = GUI.color;
                GUI.color = isComplete ? new Color(0.5f, 0.8f, 0.5f) : new Color(0.6f, 0.6f, 0.6f);
                GUILayout.Label(statusText, EditorStyles.miniLabel);
                GUI.color = statusColor;
            }

            EditorGUILayout.EndVertical();
            GUILayout.Space(3);
        }

        private static void DrawFileToolCard(string title, string description, string filePath, System.Action generateAction)
        {
            bool exists = File.Exists(filePath);
            string statusText = exists ? System.IO.Path.GetFileName(filePath) + " found at project root" : "Not created yet";

            EditorGUILayout.BeginVertical(EditorStyles.helpBox);
            EditorGUILayout.BeginHorizontal();

            var prevColor = GUI.color;
            GUI.color = exists ? new Color(0.3f, 0.8f, 0.3f) : new Color(0.7f, 0.7f, 0.7f);
            GUILayout.Label(exists ? "[OK]" : "[  ]", GUILayout.Width(36));
            GUI.color = prevColor;

            GUILayout.Label(title, EditorStyles.boldLabel);
            GUILayout.FlexibleSpace();

            if (exists)
            {
                if (GUILayout.Button("Open", GUILayout.Width(60)))
                    InternalEditorUtility.OpenFileAtLineExternal(filePath, 1, 0);
            }
            else
            {
                if (GUILayout.Button("Run", GUILayout.Width(60)))
                    generateAction?.Invoke();
            }

            EditorGUILayout.EndHorizontal();

            GUILayout.Label(description, EditorStyles.wordWrappedMiniLabel);

            prevColor = GUI.color;
            GUI.color = exists ? new Color(0.5f, 0.8f, 0.5f) : new Color(0.6f, 0.6f, 0.6f);
            GUILayout.Label(statusText, EditorStyles.miniLabel);
            GUI.color = prevColor;

            EditorGUILayout.EndVertical();
            GUILayout.Space(3);
        }
    }
}
