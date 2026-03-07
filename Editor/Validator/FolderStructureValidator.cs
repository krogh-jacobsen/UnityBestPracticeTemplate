using System.Collections.Generic;
using System.IO;
using UnityEngine;

namespace UnityBestPractices.Editor.Validator
{
    public class FolderStructureValidator : IValidator
    {
        public string Name => "Folder Structure";
        public string Description => "Checks for recommended Unity project folder structure";

        private static readonly string[] RecommendedFolders = new string[]
        {
            "Scenes",
            "Scripts",
            "Prefabs",
            "Materials",
            "Textures",
            "Audio",
            "Animations",
            "Resources",
            "Editor",
            "Plugins"
        };

        public ValidationResult Validate()
        {
            var issues = new List<ValidationIssue>();
            string assetsPath = Application.dataPath;

            var missingFolders = new List<string>();
            var existingFolders = new List<string>();

            foreach (var folderName in RecommendedFolders)
            {
                string folderPath = Path.Combine(assetsPath, folderName);
                if (Directory.Exists(folderPath))
                {
                    existingFolders.Add(folderName);
                }
                else
                {
                    missingFolders.Add(folderName);
                }
            }

            // Only report as info if folders are missing (not critical)
            if (missingFolders.Count > 0)
            {
                issues.Add(new ValidationIssue(
                    $"Recommended folders missing: {string.Join(", ", missingFolders)}",
                    ValidationSeverity.Info,
                    "",
                    Name
                ));
            }

            // Report positive status
            if (existingFolders.Count == RecommendedFolders.Length)
            {
                issues.Add(new ValidationIssue(
                    $"All recommended folders present ({existingFolders.Count}/{RecommendedFolders.Length})",
                    ValidationSeverity.Info,
                    "",
                    Name
                ));
            }
            else if (existingFolders.Count > 0)
            {
                issues.Add(new ValidationIssue(
                    $"Folder structure: {existingFolders.Count}/{RecommendedFolders.Length} recommended folders present",
                    ValidationSeverity.Info,
                    "",
                    Name
                ));
            }

            return new ValidationResult(Name, issues.ToArray());
        }
    }
}
