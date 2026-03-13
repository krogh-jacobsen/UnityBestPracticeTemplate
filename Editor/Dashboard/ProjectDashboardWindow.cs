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
        private bool _showValidationDetails = false;
        private bool _showLLMFiles = false;
        private bool _showSkills = false;
        private bool _showTools = true;
        private bool _showProjectSettings = true;

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

            // Tools quick-actions
            DrawTools();

            GUILayout.Space(10);

            // Project Settings
            DrawProjectSettings();

            GUILayout.Space(10);

            // Status Overview
            DrawStatusOverview();

            GUILayout.Space(10);

            // LLM Instruction Files
            DrawLLMInstructions();

            GUILayout.Space(10);

            // Agent Skills
            DrawAgentSkills();

            GUILayout.Space(15);

            // Project Health Section
            DrawProjectHealth();

            GUILayout.Space(10);

            // Validation Details
            if (_showValidationDetails)
            {
                DrawValidationDetails();
            }

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

        private void DrawStatusOverview()
        {
            EditorGUILayout.BeginVertical(EditorStyles.helpBox);

            // Folder Structure
            DrawStatusLine(
                "Folder Structure",
                $"{_data.ExistingFoldersCount}/{_data.TotalRecommendedFolders} folders",
                _data.ExistingFoldersCount >= _data.TotalRecommendedFolders * 0.7f
            );

            // Presets
            string presetStatus;
            if (_data.HasAudioPresets && _data.HasTexturePresets)
                presetStatus = "Audio: OK   Textures: OK";
            else if (_data.HasAudioPresets)
                presetStatus = "Audio: OK   Textures: missing";
            else if (_data.HasTexturePresets)
                presetStatus = "Audio: missing   Textures: OK";
            else
                presetStatus = "Audio: missing   Textures: missing";

            DrawStatusLine(
                "Presets",
                presetStatus,
                _data.HasAudioPresets && _data.HasTexturePresets
            );

            // LLM Instructions
            DrawStatusLine(
                "LLM Instructions",
                $"{_data.LLMInstructionFilesCount} files",
                _data.LLMInstructionFilesCount > 0
            );

            // Git & IDE Config
            bool gitIdeGood = _data.IsGitInitialized && _data.HasGitIgnore && _data.HasEditorConfig;
            string gitIdeStatus;
            if (gitIdeGood)
            {
                gitIdeStatus = "All configured";
            }
            else
            {
                var missing = new System.Collections.Generic.List<string>();
                if (!_data.IsGitInitialized) missing.Add("git");
                if (!_data.HasGitIgnore) missing.Add(".gitignore");
                if (!_data.HasEditorConfig) missing.Add(".editorconfig");
                gitIdeStatus = "Missing: " + string.Join(", ", missing);
            }

            DrawStatusLine("Git & IDE", gitIdeStatus, gitIdeGood);

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

            string header = $"LLM INSTRUCTION FILES ({_data.LLMInstructionFilesCount})";
            _showLLMFiles = EditorGUILayout.Foldout(_showLLMFiles, header, true, EditorStyles.foldoutHeader);

            if (_showLLMFiles && _data.LLMInstructionFiles != null)
            {
                GUILayout.Space(4);

                string projectRoot = Path.GetDirectoryName(Application.dataPath);

                var greenStyle = new GUIStyle(EditorStyles.label)
                {
                    normal = { textColor = new Color(0.3f, 0.8f, 0.3f) }
                };

                foreach (var file in _data.LLMInstructionFiles)
                {
                    EditorGUILayout.BeginHorizontal();
                    GUILayout.Label(file.DisplayName, EditorStyles.label);

                    string fullPath = Path.GetFullPath(
                        Path.Combine(Application.dataPath, "..", file.AssetPath));

                    bool alreadyAdded = CopyAIFilesToProject.LLMInstructionExistsInProject(fullPath, projectRoot);
                    if (alreadyAdded)
                        GUILayout.Label("Found", greenStyle, GUILayout.Width(40));

                    GUILayout.FlexibleSpace();

                    if (GUILayout.Button(new GUIContent("Open", "Open this file in the default editor"), GUILayout.Width(50)))
                        InternalEditorUtility.OpenFileAtLineExternal(fullPath, 1, 0);

                    if (GUILayout.Button(new GUIContent("Add", "Copy this file to .github/instructions/ in your project"), GUILayout.Width(40)))
                        CopyAIFilesToProject.CopySingleLLMInstruction(fullPath, projectRoot);

                    EditorGUILayout.EndHorizontal();
                }

                GUILayout.Space(4);
                EditorGUILayout.BeginHorizontal();
                GUILayout.FlexibleSpace();
                if (GUILayout.Button(new GUIContent("Add all", "Copy all LLM instruction files to .github/instructions/ in your project"), GUILayout.Width(70)))
                {
                    foreach (var file in _data.LLMInstructionFiles)
                    {
                        string fullPath = Path.GetFullPath(
                            Path.Combine(Application.dataPath, "..", file.AssetPath));
                        CopyAIFilesToProject.CopySingleLLMInstruction(fullPath, projectRoot);
                    }
                }
                EditorGUILayout.EndHorizontal();
            }

            EditorGUILayout.EndVertical();
        }

        private void DrawAgentSkills()
        {
            EditorGUILayout.BeginVertical(EditorStyles.helpBox);

            string header = $"AGENT SKILLS ({_data.AgentSkillFilesCount})";
            _showSkills = EditorGUILayout.Foldout(_showSkills, header, true, EditorStyles.foldoutHeader);

            if (_showSkills && _data.AgentSkillFiles != null)
            {
                GUILayout.Space(4);

                string projectRoot = Path.GetDirectoryName(Application.dataPath);

                foreach (var skill in _data.AgentSkillFiles)
                {
                    EditorGUILayout.BeginHorizontal();
                    GUILayout.Label(skill.DisplayName, EditorStyles.label);
                    GUILayout.FlexibleSpace();

                    string fullPath = Path.GetFullPath(
                        Path.Combine(Application.dataPath, "..", skill.AssetPath));

                    if (GUILayout.Button(new GUIContent("Open", "Open this skill file in the default editor"), GUILayout.Width(50)))
                        InternalEditorUtility.OpenFileAtLineExternal(fullPath, 1, 0);

                    if (GUILayout.Button(new GUIContent("Add", "Copy to .github/prompts/ (Copilot) and .claude/commands/ (Claude Code)"), GUILayout.Width(40)))
                        CopyAIFilesToProject.CopySingleAgentSkill(fullPath, projectRoot);

                    EditorGUILayout.EndHorizontal();
                }
            }

            EditorGUILayout.EndVertical();
        }

        private void DrawProjectSettings()
        {
            EditorGUILayout.BeginVertical(EditorStyles.helpBox);

            _showProjectSettings = EditorGUILayout.Foldout(_showProjectSettings, "PROJECT SETTINGS", true, EditorStyles.foldoutHeader);

            if (_showProjectSettings)
            {
                GUILayout.Space(4);

                bool allOk = ConfigureProjectSettings.IsEnterPlayModeConfigured
                    && ConfigureProjectSettings.IsIL2CPPConfigured
                    && ConfigureProjectSettings.IsApiCompatibilityConfigured
                    && ConfigureProjectSettings.IsAssetSerializationConfigured
                    && ConfigureProjectSettings.IsVersionControlConfigured;

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
            }

            EditorGUILayout.EndVertical();
        }

        private void DrawSettingCard(string title, string description, bool isConfigured, System.Action applyAction)
        {
            EditorGUILayout.BeginVertical(EditorStyles.helpBox);
            EditorGUILayout.BeginHorizontal();

            var prevColor = GUI.color;
            GUI.color = isConfigured ? new Color(0.3f, 0.8f, 0.3f) : new Color(0.7f, 0.7f, 0.7f);
            GUILayout.Label(isConfigured ? "[OK]" : "[  ]", GUILayout.Width(36));
            GUI.color = prevColor;

            GUILayout.Label(title, EditorStyles.boldLabel);
            GUILayout.FlexibleSpace();

            if (isConfigured)
            {
                prevColor = GUI.color;
                GUI.color = new Color(0.3f, 0.8f, 0.3f);
                GUILayout.Label("Configured", GUILayout.Width(74));
                GUI.color = prevColor;
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

            _showTools = EditorGUILayout.Foldout(_showTools, "TOOLS", true, EditorStyles.foldoutHeader);

            if (_showTools)
            {
                GUILayout.Space(4);

                string projectRoot = Path.GetDirectoryName(Application.dataPath);
                string gitignorePath = Path.Combine(projectRoot, ".gitignore");
                string editorconfigPath = Path.Combine(projectRoot, ".editorconfig");
                bool gitignoreExists = File.Exists(gitignorePath);
                bool editorconfigExists = File.Exists(editorconfigPath);
                bool foldersOk = _data.ExistingFoldersCount >= _data.TotalRecommendedFolders * 0.7f;

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

                DrawToolCard(
                    "Configure Import Presets",
                    "Register audio, texture, model and animation import presets in the Preset Manager individually or all at once.",
                    false, "",
                    "Open", PresetsWindow.ShowWindow);

                GUILayout.Space(6);

                // ── Windows ───────────────────────────────────────────────────
                GUILayout.Label("Windows", EditorStyles.miniLabel);

                DrawToolCard(
                    "New Project Wizard",
                    "Guided wizard that runs all setup steps in order — use this to configure a new project from scratch.",
                    false, "",
                    "Open", NewProjectWizard.ShowWindow);

                DrawToolCard(
                    "PlayerPrefs Inspector",
                    "View, edit and delete all PlayerPrefs keys stored for this project.",
                    false, "",
                    "Open", PlayerPrefsInspectorWindow.ShowWindow);

                DrawToolCard(
                    "Layer Collision Matrix",
                    "Visual editor for configuring which physics layers collide with each other.",
                    false, "",
                    "Open", LayerCollisionMatrixWindow.ShowWindow);

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

        private static void DrawToolCard(string title, string description, bool isComplete, string statusText,
            string buttonLabel, System.Action action, int buttonWidth = 60)
        {
            EditorGUILayout.BeginVertical(EditorStyles.helpBox);
            EditorGUILayout.BeginHorizontal();

            var prevColor = GUI.color;
            GUI.color = isComplete ? new Color(0.3f, 0.8f, 0.3f) : new Color(0.7f, 0.7f, 0.7f);
            GUILayout.Label(isComplete ? "[OK]" : "[  ]", GUILayout.Width(36));
            GUI.color = prevColor;

            GUILayout.Label(title, EditorStyles.boldLabel);
            GUILayout.FlexibleSpace();

            if (GUILayout.Button(buttonLabel, GUILayout.Width(buttonWidth)))
                action?.Invoke();

            EditorGUILayout.EndHorizontal();

            GUILayout.Label(description, EditorStyles.wordWrappedMiniLabel);

            if (!string.IsNullOrEmpty(statusText))
            {
                prevColor = GUI.color;
                GUI.color = isComplete ? new Color(0.5f, 0.8f, 0.5f) : new Color(0.6f, 0.6f, 0.6f);
                GUILayout.Label(statusText, EditorStyles.miniLabel);
                GUI.color = prevColor;
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

        private void DrawProjectHealth()
        {
            EditorGUILayout.BeginVertical(EditorStyles.helpBox);

            EditorGUILayout.BeginHorizontal();
            GUILayout.Label("PROJECT HEALTH", EditorStyles.boldLabel);
            GUILayout.FlexibleSpace();
            if (GUILayout.Button("Run Analysis", GUILayout.Width(110)))
            {
                ProjectDashboardData.RunValidation(_data);
                Repaint();
            }
            EditorGUILayout.EndHorizontal();

            GUILayout.Space(5);

            if (_data.ValidationResults == null)
            {
                var prevColor = GUI.color;
                GUI.color = new Color(0.6f, 0.6f, 0.6f);
                GUILayout.Label("Not analyzed yet — click Run Analysis.", EditorStyles.wordWrappedLabel);
                GUI.color = prevColor;
            }
            else
            {
                DrawHealthSummary();
                GUILayout.Space(10);
                _showValidationDetails = EditorGUILayout.Foldout(_showValidationDetails, "Show Details", true);
            }

            EditorGUILayout.EndVertical();
        }

        private void DrawHealthSummary()
        {
            EditorGUILayout.BeginHorizontal();

            // Errors
            if (_data.TotalErrors > 0)
            {
                GUI.color = new Color(0.9f, 0.3f, 0.3f);
                GUILayout.Label($"L {_data.TotalErrors} Error{(_data.TotalErrors != 1 ? "s" : "")}", GUILayout.Width(100));
                GUI.color = Color.white;
            }
            else
            {
                GUI.color = new Color(0.5f, 0.5f, 0.5f);
                GUILayout.Label("L 0 Errors", GUILayout.Width(100));
                GUI.color = Color.white;
            }

            // Warnings
            if (_data.TotalWarnings > 0)
            {
                GUI.color = new Color(0.9f, 0.7f, 0.2f);
                GUILayout.Label($"� {_data.TotalWarnings} Warning{(_data.TotalWarnings != 1 ? "s" : "")}");
                GUI.color = Color.white;
            }
            else
            {
                GUI.color = new Color(0.5f, 0.5f, 0.5f);
                GUILayout.Label("� 0 Warnings");
                GUI.color = Color.white;
            }

            EditorGUILayout.EndHorizontal();

            // All clear message
            if (_data.TotalErrors == 0 && _data.TotalWarnings == 0)
            {
                GUI.color = new Color(0.3f, 0.8f, 0.3f);
                GUILayout.Label(" All checks passed!");
                GUI.color = Color.white;
            }
        }

        private void DrawValidationDetails()
        {
            if (_data.ValidationResults == null || _data.ValidationResults.Length == 0)
                return;

            EditorGUILayout.BeginVertical(EditorStyles.helpBox);

            foreach (var result in _data.ValidationResults)
            {
                EditorGUILayout.BeginVertical(EditorStyles.helpBox);

                if (result.Issues.Length == 0)
                {
                    EditorGUILayout.BeginHorizontal();
                    var prevColor = GUI.color;
                    GUI.color = new Color(0.3f, 0.8f, 0.3f);
                    GUILayout.Label("[OK]", GUILayout.Width(36));
                    GUI.color = prevColor;
                    GUILayout.Label(result.ValidatorName, EditorStyles.boldLabel);
                    EditorGUILayout.EndHorizontal();
                }
                else
                {
                    EditorGUILayout.BeginHorizontal();
                    string categoryLabel = $"{result.ValidatorName} ({result.Issues.Length} issue{(result.Issues.Length != 1 ? "s" : "")})";
                    GUILayout.Label(categoryLabel, EditorStyles.boldLabel);

                    if (result.FixAllAction != null)
                    {
                        GUILayout.FlexibleSpace();
                        if (GUILayout.Button(result.FixAllLabel, GUILayout.Width(70)))
                        {
                            result.FixAllAction();
                            ProjectDashboardData.RunValidation(_data);
                            Repaint();
                        }
                    }

                    EditorGUILayout.EndHorizontal();
                    foreach (var issue in result.Issues)
                    {
                        DrawIssue(issue);
                    }
                }

                EditorGUILayout.EndVertical();
                GUILayout.Space(3);
            }

            EditorGUILayout.EndVertical();
        }

        private void DrawIssue(ValidationIssue issue)
        {
            EditorGUILayout.BeginHorizontal();

            // Severity icon
            string icon = "";
            Color iconColor = Color.white;

            switch (issue.Severity)
            {
                case ValidationSeverity.Error:
                    icon = "L";
                    iconColor = new Color(0.9f, 0.3f, 0.3f);
                    break;
                case ValidationSeverity.Warning:
                    icon = "�";
                    iconColor = new Color(0.9f, 0.7f, 0.2f);
                    break;
                case ValidationSeverity.Info:
                    icon = "9";
                    iconColor = new Color(0.4f, 0.7f, 0.9f);
                    break;
            }

            GUI.color = iconColor;
            GUILayout.Label(icon, GUILayout.Width(20));
            GUI.color = Color.white;

            // Message
            EditorGUILayout.LabelField(issue.Message, EditorStyles.wordWrappedLabel);

            // Ping asset button if path exists
            if (!string.IsNullOrEmpty(issue.AssetPath))
            {
                if (GUILayout.Button("�", GUILayout.Width(30)))
                {
                    var asset = AssetDatabase.LoadAssetAtPath<Object>(issue.AssetPath);
                    if (asset != null)
                    {
                        EditorGUIUtility.PingObject(asset);
                        Selection.activeObject = asset;
                    }
                }
            }

            // Fix button — shown only when the validator provides a fix action
            if (issue.FixAction != null)
            {
                if (GUILayout.Button(issue.FixLabel, GUILayout.Width(36)))
                {
                    issue.FixAction();
                    ProjectDashboardData.RunValidation(_data);
                    Repaint();
                }
            }

            EditorGUILayout.EndHorizontal();
        }
    }
}
