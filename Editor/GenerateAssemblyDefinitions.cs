using UnityEditor;
using UnityEngine;
using System.IO;

namespace Unity.BestPractices.Editor
{
    /// <summary>
    /// Creates assembly definition files in the project's Scripts, Editor, and Tests folders.
    /// Accessible via the menu: <b>Window → Best Practices → Generate Assembly Definitions</b>.
    /// </summary>
    /// <remarks>
    /// Uses <c>PlayerSettings.companyName</c> and <c>PlayerSettings.productName</c> to derive
    /// the root namespace (e.g. <c>CompanyName.ProductName.Runtime</c>).
    /// Should be run after <see cref="SetupProjectFolders"/>.
    /// </remarks>
    public static class GenerateAssemblyDefinitions
    {
        /// <summary>
        /// Data structure representing the contents of a <c>.asmdef</c> file.
        /// </summary>
        private struct AsmdefData
        {
            public string name;
            public string rootNamespace;
            public string[] references;
            public string[] includePlatforms;
            public string[] excludePlatforms;
            public bool overrideReferences;
            public string[] precompiledReferences;
            public string[] defineConstraints;
            public bool autoReferenced;
        }

        /// <summary>
        /// Creates assembly definition files under <c>Assets/{projectName}</c>,
        /// where the project name is read from <see cref="SetupProjectFolders.k_ProjectNamePrefKey"/> in EditorPrefs.
        /// Skips any <c>.asmdef</c> files that already exist.
        /// </summary>
        [MenuItem("Tools/Unity Best Practices/Code/Generate Assembly Definitions", false, 101)]
        public static void Execute()
        {
            string root = EditorPrefs.GetString(SetupProjectFolders.k_ProjectNamePrefKey, "_ProjectName");
            string basePath = $"Assets/{root}";

            if (!AssetDatabase.IsValidFolder(basePath))
            {
                EditorUtility.DisplayDialog(
                    "Generate Assembly Definitions",
                    $"Folder '{basePath}' not found.\n\nRun 'Setup Project Folders' first.",
                    "OK"
                );
                return;
            }

            string companyName = SanitizeIdentifier(PlayerSettings.companyName);
            string productName = SanitizeIdentifier(PlayerSettings.productName);
            string rootNamespace = $"{companyName}.{productName}";

            // Runtime asmdef
            CreateAsmdef($"{basePath}/Scripts/{productName}.Runtime.asmdef", new AsmdefData
            {
                name = $"{rootNamespace}.Runtime",
                rootNamespace = $"{rootNamespace}.Runtime",
                references = new string[0],
                includePlatforms = new string[0],
                excludePlatforms = new string[0],
                autoReferenced = true
            });

            // Editor asmdef
            CreateAsmdef($"{basePath}/Editor/{productName}.Editor.asmdef", new AsmdefData
            {
                name = $"{rootNamespace}.Editor",
                rootNamespace = $"{rootNamespace}.Editor",
                references = new string[] { $"{rootNamespace}.Runtime" },
                includePlatforms = new string[] { "Editor" },
                excludePlatforms = new string[0],
                autoReferenced = true
            });

            // Tests/Runtime asmdef
            string testsRuntimePath = $"{basePath}/Tests/Runtime";
            if (AssetDatabase.IsValidFolder(testsRuntimePath) ||
                Directory.Exists(Path.Combine(Application.dataPath, "..", testsRuntimePath)))
            {
                EditorController.CreateFolder(basePath, "Tests");
                EditorController.CreateFolder($"{basePath}/Tests", "Runtime");
                CreateAsmdef($"{testsRuntimePath}/{productName}.Tests.Runtime.asmdef", new AsmdefData
                {
                    name = $"{rootNamespace}.Tests.Runtime",
                    rootNamespace = $"{rootNamespace}.Tests.Runtime",
                    references = new string[] { $"{rootNamespace}.Runtime" },
                    includePlatforms = new string[0],
                    excludePlatforms = new string[0],
                    overrideReferences = true,
                    precompiledReferences = new string[] { "nunit.framework.dll" },
                    defineConstraints = new string[] { "UNITY_INCLUDE_TESTS" },
                    autoReferenced = false
                });
            }

            // Tests/Editor asmdef
            string testsEditorPath = $"{basePath}/Tests/Editor";
            if (AssetDatabase.IsValidFolder(testsEditorPath) ||
                Directory.Exists(Path.Combine(Application.dataPath, "..", testsEditorPath)))
            {
                EditorController.CreateFolder(basePath, "Tests");
                EditorController.CreateFolder($"{basePath}/Tests", "Editor");
                CreateAsmdef($"{testsEditorPath}/{productName}.Tests.Editor.asmdef", new AsmdefData
                {
                    name = $"{rootNamespace}.Tests.Editor",
                    rootNamespace = $"{rootNamespace}.Tests.Editor",
                    references = new string[] { $"{rootNamespace}.Runtime", $"{rootNamespace}.Editor" },
                    includePlatforms = new string[] { "Editor" },
                    excludePlatforms = new string[0],
                    overrideReferences = true,
                    precompiledReferences = new string[] { "nunit.framework.dll" },
                    defineConstraints = new string[] { "UNITY_INCLUDE_TESTS" },
                    autoReferenced = false
                });
            }

            AssetDatabase.Refresh();
            Debug.Log($"[BestPractice] Assembly definitions generated under {basePath} with namespace '{rootNamespace}'");
        }

        /// <summary>
        /// Creates a runtime assembly definition in the <c>Scripts/</c> folder of the named sub-system,
        /// referencing the project's main Runtime assembly.
        /// </summary>
        /// <param name="projectName">The root project folder name (e.g. <c>"MyGame"</c>).</param>
        /// <param name="subSystemName">The sub-system folder name (e.g. <c>"Inventory"</c>).</param>
        public static void CreateSubSystemAsmdef(string projectName, string subSystemName)
        {
            if (string.IsNullOrWhiteSpace(projectName) || string.IsNullOrWhiteSpace(subSystemName))
                return;

            string scriptsPath = $"Assets/{projectName}/{subSystemName}/Scripts";
            if (!AssetDatabase.IsValidFolder(scriptsPath))
            {
                Debug.LogWarning($"[BestPractice] Scripts folder not found at '{scriptsPath}' — skipping asmdef.");
                return;
            }

            string companyName = SanitizeIdentifier(PlayerSettings.companyName);
            string productName = SanitizeIdentifier(PlayerSettings.productName);
            string rootNamespace = $"{companyName}.{productName}";
            string subNamespace = $"{rootNamespace}.{SanitizeIdentifier(subSystemName)}";

            CreateAsmdef($"{scriptsPath}/{SanitizeIdentifier(subSystemName)}.asmdef", new AsmdefData
            {
                name = subNamespace,
                rootNamespace = subNamespace,
                references = new string[] { $"{rootNamespace}.Runtime" },
                includePlatforms = new string[0],
                excludePlatforms = new string[0],
                autoReferenced = true
            });

            AssetDatabase.Refresh();
            Debug.Log($"[BestPractice] Assembly definition created for sub-system '{subSystemName}' with namespace '{subNamespace}'");
        }

        /// Strips any character that is not a letter, digit, or underscore so the result
        /// is safe to use as a C# identifier and as part of a file name.
        /// Leading digits are prefixed with an underscore to keep the identifier valid.
        private static string SanitizeIdentifier(string input)
        {
            if (string.IsNullOrWhiteSpace(input)) return "Project";
            var sb = new System.Text.StringBuilder();
            foreach (char c in input)
            {
                if (char.IsLetterOrDigit(c)) sb.Append(c);
                // Collapse runs of non-alphanumeric chars into nothing (spaces, colons, hyphens, etc.)
            }
            string result = sb.ToString();
            // Ensure it doesn't start with a digit
            if (result.Length > 0 && char.IsDigit(result[0]))
                result = "_" + result;
            return result.Length > 0 ? result : "Project";
        }

        /// <summary>
        /// Writes a <c>.asmdef</c> JSON file at the specified path. Skips if the file already exists.
        /// </summary>
        /// <param name="path">Asset-relative path for the <c>.asmdef</c> file.</param>
        /// <param name="data">The assembly definition data to serialize.</param>
        private static void CreateAsmdef(string path, AsmdefData data)
        {
            string fullPath = Path.Combine(Application.dataPath, "..", path);
            if (File.Exists(fullPath))
            {
                Debug.Log($"[BestPractice] Skipped (already exists): {path}");
                return;
            }

            string ToJsonArray(string[] arr)
            {
                if (arr == null || arr.Length == 0) return "[]";
                string items = string.Join(", ", System.Array.ConvertAll(arr, s => $"\"{s}\""));
                return $"[{items}]";
            }

            string json = $@"{{
    ""name"": ""{data.name}"",
    ""rootNamespace"": ""{data.rootNamespace}"",
    ""references"": {ToJsonArray(data.references)},
    ""includePlatforms"": {ToJsonArray(data.includePlatforms)},
    ""excludePlatforms"": {ToJsonArray(data.excludePlatforms)},
    ""allowUnsafeCode"": false,
    ""overrideReferences"": {data.overrideReferences.ToString().ToLower()},
    ""precompiledReferences"": {ToJsonArray(data.precompiledReferences)},
    ""autoReferenced"": {data.autoReferenced.ToString().ToLower()},
    ""defineConstraints"": {ToJsonArray(data.defineConstraints)},
    ""versionDefines"": [],
    ""noEngineReferences"": false
}}";

            File.WriteAllText(fullPath, json);
            Debug.Log($"[BestPractice] Created: {path}");
        }
    }
}

