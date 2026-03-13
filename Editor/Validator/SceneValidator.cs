using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;

namespace UnityBestPractices.Editor.Validator
{
    public class SceneValidator : IValidator
    {
        public string Name => "Scene Configuration";
        public string Description => "Checks if all scenes are included in build settings";

        public ValidationResult Validate()
        {
            var issues = new List<ValidationIssue>();

            // Get all scenes in build settings
            var scenesInBuild = new HashSet<string>();
            foreach (var scene in EditorBuildSettings.scenes)
            {
                if (!string.IsNullOrEmpty(scene.path))
                {
                    scenesInBuild.Add(scene.path);
                }
            }

            // Find all .unity files in the project
            string[] allSceneFiles = Directory.GetFiles(Application.dataPath, "*.unity", SearchOption.AllDirectories);

            // Collect paths for all scenes missing from build (used by Fix / Add All)
            var missingScenePaths = new List<string>();

            foreach (var sceneFile in allSceneFiles)
            {
                // Convert to relative path (Assets/...)
                string relativePath = "Assets" + sceneFile.Substring(Application.dataPath.Length).Replace('\\', '/');

                // Check if scene is in build settings
                if (!scenesInBuild.Contains(relativePath))
                {
                    missingScenePaths.Add(relativePath);
                    string sceneName = Path.GetFileNameWithoutExtension(sceneFile);
                    string capturedPath = relativePath;
                    var issue = new ValidationIssue(
                        $"Scene '{sceneName}' not in build settings",
                        ValidationSeverity.Warning,
                        relativePath,
                        Name
                    );
                    issue.FixAction = () => AddSceneToBuild(capturedPath);
                    issue.FixLabel = "Add";
                    issues.Add(issue);
                }
            }

            // Check if there are any scenes in build settings at all
            if (EditorBuildSettings.scenes.Length == 0)
            {
                issues.Add(new ValidationIssue(
                    "No scenes configured in build settings",
                    ValidationSeverity.Error,
                    "",
                    Name
                ));
            }

            var result = new ValidationResult(Name, issues.ToArray());

            if (missingScenePaths.Count > 0)
            {
                var capturedMissing = new List<string>(missingScenePaths);
                result.FixAllAction = () =>
                {
                    foreach (var path in capturedMissing)
                        AddSceneToBuild(path);
                };
                result.FixAllLabel = "Add All";
            }

            return result;
        }

        private static void AddSceneToBuild(string scenePath)
        {
            var scenes = new List<EditorBuildSettingsScene>(EditorBuildSettings.scenes);

            // Guard: don't add duplicates
            foreach (var s in scenes)
            {
                if (s.path == scenePath) return;
            }

            scenes.Add(new EditorBuildSettingsScene(scenePath, true));
            EditorBuildSettings.scenes = scenes.ToArray();
        }
    }
}
