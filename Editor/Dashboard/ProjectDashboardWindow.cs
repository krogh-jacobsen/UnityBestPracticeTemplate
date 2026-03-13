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
        private bool _showTools = true;

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

            // Status Overview
            DrawStatusOverview();

            GUILayout.Space(10);

            // LLM Instruction Files
            DrawLLMInstructions();

            GUILayout.Space(10);

            // Tools quick-actions
            DrawTools();

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

                foreach (var file in _data.LLMInstructionFiles)
                {
                    EditorGUILayout.BeginHorizontal();
                    GUILayout.Label(file.DisplayName, EditorStyles.label);
                    GUILayout.FlexibleSpace();
                    if (GUILayout.Button("Open", GUILayout.Width(60)))
                    {
                        string fullPath = Path.GetFullPath(
                            Path.Combine(Application.dataPath, "..", file.AssetPath));
                        InternalEditorUtility.OpenFileAtLineExternal(fullPath, 1, 0);
                    }
                    EditorGUILayout.EndHorizontal();
                }
            }

            EditorGUILayout.EndVertical();
        }

        private void DrawTools()
        {
            EditorGUILayout.BeginVertical(EditorStyles.helpBox);

            _showTools = EditorGUILayout.Foldout(_showTools, "TOOLS", true, EditorStyles.foldoutHeader);

            if (_showTools)
            {
                GUILayout.Space(4);

                // ── Setup Actions ────────────────────────────────────────────
                GUILayout.Label("Setup Actions", EditorStyles.miniLabel);

                DrawToolRow("Setup Project Folders", "Run", SetupProjectFolders.Execute);
                DrawToolRow("Generate .gitignore", "Run", GenerateGitIgnore.Execute);
                DrawToolRow("Generate .editorconfig", "Run", GenerateEditorConfig.Execute);
                DrawToolRow("Generate Assembly Defs", "Run", GenerateAssemblyDefinitions.Execute);
                DrawToolRow("Configure Import Presets", "Run", ConfigurePresets.Execute);
                DrawToolRow("Configure Project Settings", "Run", ConfigureProjectSettings.ApplySettings);

                GUILayout.Space(6);

                // ── Windows ──────────────────────────────────────────────────
                GUILayout.Label("Windows", EditorStyles.miniLabel);

                DrawToolRow("New Project Wizard", "Open", NewProjectWizard.ShowWindow);
                DrawToolRow("PlayerPrefs Inspector", "Open", PlayerPrefsInspectorWindow.ShowWindow);
                DrawToolRow("Layer Collision Matrix", "Open", LayerCollisionMatrixWindow.ShowWindow);

                GUILayout.Space(6);

                // ── AI Assistance ─────────────────────────────────────────────
                GUILayout.Label("AI Assistance", EditorStyles.miniLabel);

                DrawToolRow("AI Files (LLM Instructions + Skills)", "Copy to Project", CopyAIFilesToProject.Execute, 120);
            }

            EditorGUILayout.EndVertical();
        }

        private static void DrawToolRow(string label, string buttonLabel, System.Action action, int buttonWidth = 60)
        {
            EditorGUILayout.BeginHorizontal();
            GUILayout.Label(label, EditorStyles.label);
            GUILayout.FlexibleSpace();
            if (GUILayout.Button(buttonLabel, GUILayout.Width(buttonWidth)))
                action();
            EditorGUILayout.EndHorizontal();
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
                    string categoryLabel = $"{result.ValidatorName} ({result.Issues.Length} issue{(result.Issues.Length != 1 ? "s" : "")})";
                    GUILayout.Label(categoryLabel, EditorStyles.boldLabel);
                    GUILayout.Space(5);

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
                if (GUILayout.Button("Fix", GUILayout.Width(36)))
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
