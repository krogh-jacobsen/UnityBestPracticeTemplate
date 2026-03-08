using UnityEditor;
using UnityEditorInternal;
using UnityEngine;
using System.IO;
using UnityBestPractices.Editor.Validator;

namespace UnityBestPractices.Editor.Dashboard
{
    public class ProjectDashboardWindow : EditorWindow
    {
        private ProjectDashboardData _data;
        private Vector2 _scrollPosition;
        private bool _showValidationDetails = true;
        private bool _showLLMFiles = false;

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
            GUILayout.Label("PROJECT DASHBOARD", EditorStyles.boldLabel);
            GUILayout.Space(5);

            EditorGUILayout.BeginHorizontal();
            if (GUILayout.Button("Refresh", GUILayout.Width(100)))
            {
                RefreshData();
            }
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
            string presetStatus = "";
            if (_data.HasAudioPresets && _data.HasTexturePresets)
                presetStatus = "Audio  Textures ";
            else if (_data.HasAudioPresets)
                presetStatus = "Audio  Textures ";
            else if (_data.HasTexturePresets)
                presetStatus = "Audio  Textures ";
            else
                presetStatus = "Audio  Textures ";

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

            // Git
            string gitStatus = "";
            if (_data.IsGitInitialized && _data.HasGitIgnore)
                gitStatus = "Initialized, .gitignore present";
            else if (_data.IsGitInitialized)
                gitStatus = "Initialized, missing .gitignore";
            else if (_data.HasGitIgnore)
                gitStatus = ".gitignore present, not initialized";
            else
                gitStatus = "Not initialized";

            DrawStatusLine(
                "Git",
                gitStatus,
                _data.IsGitInitialized && _data.HasGitIgnore
            );

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

        private void DrawProjectHealth()
        {
            EditorGUILayout.BeginVertical(EditorStyles.helpBox);

            // Title with fold-out
            EditorGUILayout.BeginHorizontal();
            GUILayout.Label("PROJECT HEALTH", EditorStyles.boldLabel);
            EditorGUILayout.EndHorizontal();

            GUILayout.Space(5);

            // Summary counts
            DrawHealthSummary();

            GUILayout.Space(10);

            // Toggle for details
            _showValidationDetails = EditorGUILayout.Foldout(_showValidationDetails, "Show Details", true);

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
                // Skip if no issues
                if (result.Issues.Length == 0)
                    continue;

                // Category header
                EditorGUILayout.BeginVertical(EditorStyles.helpBox);

                string categoryLabel = $"{result.ValidatorName} ({result.Issues.Length} issue{(result.Issues.Length != 1 ? "s" : "")})";
                GUILayout.Label(categoryLabel, EditorStyles.boldLabel);

                GUILayout.Space(5);

                // Issues
                foreach (var issue in result.Issues)
                {
                    DrawIssue(issue);
                }

                EditorGUILayout.EndVertical();
                GUILayout.Space(5);
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

            EditorGUILayout.EndHorizontal();
        }
    }
}
