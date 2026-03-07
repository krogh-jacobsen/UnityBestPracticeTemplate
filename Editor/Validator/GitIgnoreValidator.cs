using System.Collections.Generic;
using System.IO;
using UnityEngine;

namespace UnityBestPractices.Editor.Validator
{
    public class GitIgnoreValidator : IValidator
    {
        public string Name => "Git Configuration";
        public string Description => "Checks for .gitignore file and Unity-specific entries";

        private static readonly string[] RequiredGitIgnoreEntries = new string[]
        {
            "[Ll]ibrary/",
            "[Tt]emp/",
            "[Oo]bj/",
            "[Bb]uild/",
            "[Bb]uilds/",
            "[Ll]ogs/",
            "[Uu]ser[Ss]ettings/",
            "*.csproj",
            "*.sln",
            "*.suo",
            "*.user",
            "*.unityproj",
            "*.pidb",
            "*.booproj",
            "*.svd",
            "*.pdb",
            "*.mdb",
            "*.opendb",
            "*.VC.db"
        };

        public ValidationResult Validate()
        {
            var issues = new List<ValidationIssue>();

            // Get project root (parent of Assets folder)
            string projectRoot = Directory.GetParent(Application.dataPath).FullName;
            string gitignorePath = Path.Combine(projectRoot, ".gitignore");

            // Check if .gitignore exists
            if (!File.Exists(gitignorePath))
            {
                issues.Add(new ValidationIssue(
                    "No .gitignore file found in project root",
                    ValidationSeverity.Error,
                    "",
                    Name
                ));
            }
            else
            {
                // Read .gitignore content
                string gitignoreContent = File.ReadAllText(gitignorePath);

                // Check for required Unity entries
                var missingEntries = new List<string>();
                foreach (var requiredEntry in RequiredGitIgnoreEntries)
                {
                    // Check if the entry exists (allowing for variations in casing)
                    if (!gitignoreContent.Contains(requiredEntry))
                    {
                        // Only report the most critical ones as warnings
                        if (requiredEntry.Contains("Library") ||
                            requiredEntry.Contains("Temp") ||
                            requiredEntry.Contains("Obj") ||
                            requiredEntry.Contains("UserSettings"))
                        {
                            missingEntries.Add(requiredEntry);
                        }
                    }
                }

                if (missingEntries.Count > 0)
                {
                    issues.Add(new ValidationIssue(
                        $".gitignore missing critical Unity entries: {string.Join(", ", missingEntries)}",
                        ValidationSeverity.Warning,
                        ".gitignore",
                        Name
                    ));
                }
            }

            // Check if .git folder exists (repository initialized)
            string gitFolderPath = Path.Combine(projectRoot, ".git");
            if (!Directory.Exists(gitFolderPath))
            {
                issues.Add(new ValidationIssue(
                    "Git repository not initialized",
                    ValidationSeverity.Info,
                    "",
                    Name
                ));
            }

            return new ValidationResult(Name, issues.ToArray());
        }
    }
}
