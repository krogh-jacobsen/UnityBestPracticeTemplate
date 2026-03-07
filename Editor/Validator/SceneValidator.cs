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

            foreach (var sceneFile in allSceneFiles)
            {
                // Convert to relative path (Assets/...)
                string relativePath = "Assets" + sceneFile.Substring(Application.dataPath.Length).Replace('\\', '/');

                // Check if scene is in build settings
                if (!scenesInBuild.Contains(relativePath))
                {
                    string sceneName = Path.GetFileNameWithoutExtension(sceneFile);
                    issues.Add(new ValidationIssue(
                        $"Scene '{sceneName}' not in build settings",
                        ValidationSeverity.Warning,
                        relativePath,
                        Name
                    ));
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

            return new ValidationResult(Name, issues.ToArray());
        }
    }
}
