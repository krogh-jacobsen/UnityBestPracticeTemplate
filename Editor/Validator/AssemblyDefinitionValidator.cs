using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;

namespace UnityBestPractices.Editor.Validator
{
    public class AssemblyDefinitionValidator : IValidator
    {
        public string Name => "Assembly Definitions";
        public string Description => "Checks whether script folders have .asmdef coverage";

        public ValidationResult Validate()
        {
            var issues = new List<ValidationIssue>();

            string[] allCsFiles = Directory.GetFiles(Application.dataPath, "*.cs", SearchOption.AllDirectories);

            // Collect top-level Assets sub-folders that contain uncovered scripts
            var uncoveredTopLevelFolders = new HashSet<string>();

            foreach (var csFile in allCsFiles)
            {
                if (csFile.Contains("Editor Default Resources") || csFile.Contains("/Packages/"))
                    continue;

                string directory = Path.GetDirectoryName(csFile);

                if (!IsCoveredByAsmdef(directory))
                {
                    string topLevel = GetTopLevelFolder(directory);
                    if (topLevel != null)
                        uncoveredTopLevelFolders.Add(topLevel);
                }
            }

            foreach (var folder in uncoveredTopLevelFolders)
            {
                string relPath = "Assets/" + folder;
                issues.Add(new ValidationIssue(
                    $"No .asmdef covers scripts under {relPath}/",
                    ValidationSeverity.Warning,
                    relPath,
                    Name
                ));
            }

            return new ValidationResult(Name, issues.ToArray());
        }

        private static bool IsCoveredByAsmdef(string directory)
        {
            string current = directory;
            while (current.StartsWith(Application.dataPath))
            {
                if (Directory.GetFiles(current, "*.asmdef", SearchOption.TopDirectoryOnly).Length > 0)
                    return true;
                string parent = Directory.GetParent(current)?.FullName;
                if (parent == null) break;
                current = parent;
            }
            return false;
        }

        /// <summary>Returns the immediate child folder name directly under Assets/, or null.</summary>
        private static string GetTopLevelFolder(string directory)
        {
            // directory is an absolute path; trim the dataPath prefix to get the relative part
            if (!directory.StartsWith(Application.dataPath)) return null;
            string relative = directory.Substring(Application.dataPath.Length).TrimStart('/', '\\');
            if (string.IsNullOrEmpty(relative)) return null;
            int sep = relative.IndexOfAny(new[] { '/', '\\' });
            return sep < 0 ? relative : relative.Substring(0, sep);
        }
    }
}
