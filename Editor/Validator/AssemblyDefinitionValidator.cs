using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;

namespace UnityBestPractices.Editor.Validator
{
    public class AssemblyDefinitionValidator : IValidator
    {
        public string Name => "Assembly Definitions";
        public string Description => "Checks for missing .asmdef files in script folders";

        public ValidationResult Validate()
        {
            var issues = new List<ValidationIssue>();

            // Find all directories containing .cs files
            string[] allCsFiles = Directory.GetFiles(Application.dataPath, "*.cs", SearchOption.AllDirectories);
            var directoriesWithScripts = new HashSet<string>();

            foreach (var csFile in allCsFiles)
            {
                // Skip editor default resources and packages
                if (csFile.Contains("Editor Default Resources") || csFile.Contains("/Packages/"))
                    continue;

                string directory = Path.GetDirectoryName(csFile);
                directoriesWithScripts.Add(directory);
            }

            // Check each directory for .asmdef file
            foreach (var directory in directoriesWithScripts)
            {
                bool hasAsmdef = false;
                string currentDir = directory;

                // Check current directory and parent directories up to Assets
                while (currentDir.Contains("Assets") && !hasAsmdef)
                {
                    string[] asmdefFiles = Directory.GetFiles(currentDir, "*.asmdef", SearchOption.TopDirectoryOnly);
                    if (asmdefFiles.Length > 0)
                    {
                        hasAsmdef = true;
                        break;
                    }

                    // Move to parent directory
                    string parentDir = Directory.GetParent(currentDir)?.FullName;
                    if (parentDir == null || !parentDir.Contains("Assets"))
                        break;
                    currentDir = parentDir;
                }

                if (!hasAsmdef)
                {
                    // Convert to relative path for display
                    string relativePath = "Assets" + directory.Substring(Application.dataPath.Length);
                    issues.Add(new ValidationIssue(
                        $"Missing .asmdef in: {relativePath}",
                        ValidationSeverity.Warning,
                        relativePath,
                        Name
                    ));
                }
            }

            return new ValidationResult(Name, issues.ToArray());
        }
    }
}
