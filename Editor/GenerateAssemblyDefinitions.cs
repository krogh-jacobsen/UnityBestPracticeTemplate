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
        /// Creates assembly definition files under <c>Assets/_ProjectName</c>.
        /// Skips any <c>.asmdef</c> files that already exist.
        /// </summary>
        [MenuItem("Window/Best Practices/Generate Assembly Definitions")]
        public static void Execute()
        {
            string root = "_ProjectName";
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

            string companyName = PlayerSettings.companyName.Replace(" ", "");
            string productName = PlayerSettings.productName.Replace(" ", "");
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

