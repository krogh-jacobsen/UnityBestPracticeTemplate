using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;
using Unity.BestPractices.Editor;

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
                string capturedFolder = folder;
                var issue = new ValidationIssue(
                    $"No .asmdef covers scripts under {relPath}/",
                    ValidationSeverity.Warning,
                    relPath,
                    Name
                );
                issue.FixAction = () => CreateAsmdefInFolder(capturedFolder);
                issues.Add(issue);
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
            if (!directory.StartsWith(Application.dataPath)) return null;
            string relative = directory.Substring(Application.dataPath.Length).TrimStart('/', '\\');
            if (string.IsNullOrEmpty(relative)) return null;
            int sep = relative.IndexOfAny(new[] { '/', '\\' });
            return sep < 0 ? relative : relative.Substring(0, sep);
        }

        /// <summary>
        /// Creates a minimal .asmdef file at the root of the given top-level Assets folder.
        /// Uses company + product name as the namespace root, like GenerateAssemblyDefinitions does.
        /// </summary>
        private static void CreateAsmdefInFolder(string topLevelFolderName)
        {
            string company = PlayerSettings.companyName.Replace(" ", "");
            string product = PlayerSettings.productName.Replace(" ", "");

            bool isEditorFolder = topLevelFolderName.IndexOf("Editor", System.StringComparison.OrdinalIgnoreCase) >= 0;
            string suffix = isEditorFolder ? "Editor" : "Runtime";
            string asmName = $"{company}.{product}.{topLevelFolderName}";
            string includePlatforms = isEditorFolder ? "\"Editor\"" : "";
            string includePlatformsJson = isEditorFolder ? "[\"Editor\"]" : "[]";

            string json = $@"{{
    ""name"": ""{asmName}"",
    ""rootNamespace"": ""{asmName}"",
    ""references"": [],
    ""includePlatforms"": {includePlatformsJson},
    ""excludePlatforms"": [],
    ""allowUnsafeCode"": false,
    ""overrideReferences"": false,
    ""precompiledReferences"": [],
    ""autoReferenced"": true,
    ""defineConstraints"": [],
    ""versionDefines"": [],
    ""noEngineReferences"": false
}}";

            string destDir = Path.Combine(Application.dataPath, topLevelFolderName);
            string destFile = Path.Combine(destDir, $"{topLevelFolderName}.asmdef");

            Directory.CreateDirectory(destDir);
            File.WriteAllText(destFile, json);
            AssetDatabase.Refresh();

            Debug.Log($"[BestPractice] Created .asmdef: Assets/{topLevelFolderName}/{topLevelFolderName}.asmdef (assembly: {asmName})");
        }
    }
}
