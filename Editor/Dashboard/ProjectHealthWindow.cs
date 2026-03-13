using UnityEditor;
using UnityEngine;
using UnityBestPractices.Editor.Validator;

namespace UnityBestPractices.Editor.Dashboard
{
    public class ProjectHealthWindow : EditorWindow
    {
        private ProjectDashboardData _data;
        private Vector2 _scrollPosition;
        private bool _showDetails = true;

        [MenuItem("Tools/Unity Best Practices/Project Health")]
        public static void ShowWindow()
        {
            var window = GetWindow<ProjectHealthWindow>("Project Health");
            window.minSize = new Vector2(420, 400);
            window.Show();
        }

        private void OnEnable()
        {
            _data = ProjectDashboardData.Gather();
        }

        private void OnFocus()
        {
            _data = ProjectDashboardData.Gather();
        }

        private void OnGUI()
        {
            if (_data == null)
            {
                _data = ProjectDashboardData.Gather();
                return;
            }

            _scrollPosition = EditorGUILayout.BeginScrollView(_scrollPosition);

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
            EditorGUILayout.EndVertical();

            GUILayout.Space(6);

            if (_data.ValidationResults == null)
            {
                EditorGUILayout.BeginVertical(EditorStyles.helpBox);
                var prevColor = GUI.color;
                GUI.color = new Color(0.6f, 0.6f, 0.6f);
                GUILayout.Label("Not analyzed yet — click Run Analysis.", EditorStyles.wordWrappedLabel);
                GUI.color = prevColor;
                EditorGUILayout.EndVertical();
            }
            else
            {
                DrawHealthSummary();
                GUILayout.Space(8);
                _showDetails = EditorGUILayout.Foldout(_showDetails, "Details", true, EditorStyles.foldoutHeader);
                if (_showDetails)
                {
                    GUILayout.Space(4);
                    DrawValidationDetails();
                }
            }

            EditorGUILayout.EndScrollView();
        }

        private void DrawHealthSummary()
        {
            EditorGUILayout.BeginVertical(EditorStyles.helpBox);
            EditorGUILayout.BeginHorizontal();

            if (_data.TotalErrors > 0)
            {
                GUI.color = new Color(0.9f, 0.3f, 0.3f);
                GUILayout.Label($"{_data.TotalErrors} Error{(_data.TotalErrors != 1 ? "s" : "")}", GUILayout.Width(100));
                GUI.color = Color.white;
            }
            else
            {
                GUI.color = new Color(0.5f, 0.5f, 0.5f);
                GUILayout.Label("0 Errors", GUILayout.Width(100));
                GUI.color = Color.white;
            }

            if (_data.TotalWarnings > 0)
            {
                GUI.color = new Color(0.9f, 0.7f, 0.2f);
                GUILayout.Label($"{_data.TotalWarnings} Warning{(_data.TotalWarnings != 1 ? "s" : "")}");
                GUI.color = Color.white;
            }
            else
            {
                GUI.color = new Color(0.5f, 0.5f, 0.5f);
                GUILayout.Label("0 Warnings");
                GUI.color = Color.white;
            }

            EditorGUILayout.EndHorizontal();

            if (_data.TotalErrors == 0 && _data.TotalWarnings == 0)
            {
                GUI.color = new Color(0.3f, 0.8f, 0.3f);
                GUILayout.Label("All checks passed!");
                GUI.color = Color.white;
            }

            EditorGUILayout.EndVertical();
        }

        private void DrawValidationDetails()
        {
            if (_data.ValidationResults == null || _data.ValidationResults.Length == 0)
                return;

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
                    GUILayout.Label($"{result.ValidatorName} ({result.Issues.Length} issue{(result.Issues.Length != 1 ? "s" : "")})", EditorStyles.boldLabel);

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
                        DrawIssue(issue);
                }

                EditorGUILayout.EndVertical();
                GUILayout.Space(3);
            }
        }

        private void DrawIssue(ValidationIssue issue)
        {
            EditorGUILayout.BeginHorizontal();

            string icon = "";
            Color iconColor = Color.white;
            switch (issue.Severity)
            {
                case ValidationSeverity.Error:   icon = "L"; iconColor = new Color(0.9f, 0.3f, 0.3f); break;
                case ValidationSeverity.Warning: icon = "⚠"; iconColor = new Color(0.9f, 0.7f, 0.2f); break;
                case ValidationSeverity.Info:    icon = "i"; iconColor = new Color(0.4f, 0.7f, 0.9f); break;
            }

            GUI.color = iconColor;
            GUILayout.Label(icon, GUILayout.Width(20));
            GUI.color = Color.white;

            EditorGUILayout.LabelField(issue.Message, EditorStyles.wordWrappedLabel);

            if (!string.IsNullOrEmpty(issue.AssetPath))
            {
                if (GUILayout.Button("⊙", GUILayout.Width(30)))
                {
                    var asset = AssetDatabase.LoadAssetAtPath<Object>(issue.AssetPath);
                    if (asset != null)
                    {
                        EditorGUIUtility.PingObject(asset);
                        Selection.activeObject = asset;
                    }
                }
            }

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
