using System;
using System.IO;
using System.Text.RegularExpressions;
using UnityEditor;
using UnityEngine;

namespace Unity.BestPractices.Editor
{
    /// <summary>
    /// EditorWindow that generates a complete UPM package folder structure from user-provided metadata.
    /// Accessible via <b>Tools → Unity Project Configurator → Setup → Create Package Boilerplate</b>.
    /// </summary>
    public class PackageBoilerplateWindow : EditorWindow
    {
        // ── Metadata fields ─────────────────────────────────────────────────────
        private string m_PackageName = "com.company.packagename";
        private string m_DisplayName = "My Package";
        private string m_Version = "1.0.0";
        private string m_Description = "A description of my package.";
        private string m_AuthorName = "";
        private string m_AuthorEmail = "";
        private string m_AuthorUrl = "";
        private string m_UnityVersion = "6000.3";

        // ── Structure toggles ────────────────────────────────────────────────────
        private bool m_IncludeRuntime = true;
        private bool m_IncludeEditor = true;
        private bool m_IncludeTests = false;
        private bool m_IncludeSamples = false;
        private bool m_IncludeDocs = false;
        private bool m_IncludeReadme = true;
        private bool m_IncludeChangelog = true;
        private bool m_IncludeLicense = false;

        // ── Output ───────────────────────────────────────────────────────────────
        private string m_OutputPath = "";
        private bool m_AddToManifest = true;

        // ── UI state ─────────────────────────────────────────────────────────────
        private Vector2 m_ScrollPosition;
        private string m_ValidationMsg = "";
        private bool m_IsValid = false;

        private static readonly Regex k_PackageNameRegex =
            new Regex(@"^[a-z][a-z0-9]*(\.[a-z][a-z0-9]*){2,}$", RegexOptions.Compiled);

        private static readonly Regex k_VersionRegex =
            new Regex(@"^\d+\.\d+\.\d+$", RegexOptions.Compiled);

        [MenuItem("Tools/Unity Project Configurator/Setup/Create Package Boilerplate", false, 30)]
        public static void ShowWindow()
        {
            var window = GetWindow<PackageBoilerplateWindow>("Package Boilerplate");
            window.minSize = new Vector2(450, 620);
            window.Show();
        }

        private void OnEnable()
        {
            string projectRoot = Path.GetDirectoryName(Application.dataPath);
            m_OutputPath = Path.Combine(projectRoot, "LocalPackages");
            ValidateInputs();
        }

        private void OnGUI()
        {
            m_ScrollPosition = EditorGUILayout.BeginScrollView(m_ScrollPosition);

            DrawHeader();
            GUILayout.Space(6);
            DrawMetadataSection();
            GUILayout.Space(6);
            DrawStructureSection();
            GUILayout.Space(6);
            DrawOutputSection();
            GUILayout.Space(6);
            DrawGenerateSection();

            EditorGUILayout.EndScrollView();
        }

        // ── Section renderers ────────────────────────────────────────────────────

        private void DrawHeader()
        {
            EditorGUILayout.BeginVertical(EditorStyles.helpBox);
            GUILayout.Label("PACKAGE BOILERPLATE", EditorStyles.boldLabel);
            GUILayout.Label(
                "Generate a new UPM package folder with package.json, assembly definitions, and optional extras.",
                EditorStyles.wordWrappedMiniLabel);
            EditorGUILayout.EndVertical();
        }

        private void DrawMetadataSection()
        {
            EditorGUILayout.BeginVertical(EditorStyles.helpBox);
            GUILayout.Label("PACKAGE METADATA", EditorStyles.boldLabel);
            GUILayout.Space(4);

            EditorGUI.BeginChangeCheck();

            m_PackageName = LabeledTextField("Package Name", m_PackageName,
                "Reverse-domain identifier, all lowercase (e.g. com.mycompany.mypackage)");
            m_DisplayName = LabeledTextField("Display Name", m_DisplayName,
                "User-friendly name shown in the Package Manager");
            m_Version = LabeledTextField("Version", m_Version,
                "Semantic version: MAJOR.MINOR.PATCH");
            m_UnityVersion = LabeledTextField("Unity Min Version", m_UnityVersion,
                "Minimum Unity version required (e.g. 6000.3)");

            GUILayout.Space(4);
            GUILayout.Label("Description", EditorStyles.miniLabel);
            m_Description = EditorGUILayout.TextArea(m_Description, GUILayout.Height(48));

            GUILayout.Space(4);
            GUILayout.Label("Author", EditorStyles.miniLabel);
            m_AuthorName = LabeledTextField("  Name", m_AuthorName, "");
            m_AuthorEmail = LabeledTextField("  Email", m_AuthorEmail, "");
            m_AuthorUrl = LabeledTextField("  URL", m_AuthorUrl, "");

            if (EditorGUI.EndChangeCheck())
                ValidateInputs();

            EditorGUILayout.EndVertical();
        }

        private void DrawStructureSection()
        {
            EditorGUILayout.BeginVertical(EditorStyles.helpBox);
            GUILayout.Label("PACKAGE STRUCTURE", EditorStyles.boldLabel);
            GUILayout.Space(4);

            m_IncludeRuntime = EditorGUILayout.ToggleLeft("Runtime/ folder + .asmdef", m_IncludeRuntime);
            m_IncludeEditor = EditorGUILayout.ToggleLeft("Editor/ folder + .asmdef (Editor-only, references Runtime)", m_IncludeEditor);
            m_IncludeTests = EditorGUILayout.ToggleLeft("Tests/ folder + Runtime and Editor asmdefs (NUnit)", m_IncludeTests);
            GUILayout.Space(2);
            m_IncludeSamples = EditorGUILayout.ToggleLeft("Samples~/ folder (not imported by default)", m_IncludeSamples);
            m_IncludeDocs = EditorGUILayout.ToggleLeft("Documentation~/ folder", m_IncludeDocs);
            GUILayout.Space(2);
            m_IncludeReadme = EditorGUILayout.ToggleLeft("README.md", m_IncludeReadme);
            m_IncludeChangelog = EditorGUILayout.ToggleLeft("CHANGELOG.md", m_IncludeChangelog);
            m_IncludeLicense = EditorGUILayout.ToggleLeft("LICENSE (MIT)", m_IncludeLicense);

            EditorGUILayout.EndVertical();
        }

        private void DrawOutputSection()
        {
            EditorGUILayout.BeginVertical(EditorStyles.helpBox);
            GUILayout.Label("OUTPUT", EditorStyles.boldLabel);
            GUILayout.Space(4);

            EditorGUI.BeginChangeCheck();

            EditorGUILayout.BeginHorizontal();
            GUILayout.Label("Output Folder", GUILayout.Width(100));
            m_OutputPath = EditorGUILayout.TextField(m_OutputPath);
            if (GUILayout.Button("Browse", GUILayout.Width(60)))
            {
                string chosen = EditorUtility.OpenFolderPanel("Select Output Folder", m_OutputPath, "");
                if (!string.IsNullOrEmpty(chosen))
                    m_OutputPath = chosen;
            }
            EditorGUILayout.EndHorizontal();

            if (IsLocalPackagesPath(m_OutputPath))
            {
                m_AddToManifest = EditorGUILayout.ToggleLeft(
                    "Add to this project's manifest.json (installs immediately)", m_AddToManifest);
            }

            if (!string.IsNullOrEmpty(m_OutputPath) && m_IsValid)
            {
                string preview = Path.Combine(m_OutputPath, m_PackageName);
                var prevColor = GUI.color;
                GUI.color = new Color(0.6f, 0.6f, 0.6f);
                GUILayout.Label("Will create: " + preview, EditorStyles.wordWrappedMiniLabel);
                GUI.color = prevColor;
            }

            if (EditorGUI.EndChangeCheck())
                ValidateInputs();

            EditorGUILayout.EndVertical();
        }

        private void DrawGenerateSection()
        {
            if (!string.IsNullOrEmpty(m_ValidationMsg))
            {
                var prevColor = GUI.color;
                GUI.color = new Color(1f, 0.5f, 0.4f);
                GUILayout.Label(m_ValidationMsg, EditorStyles.wordWrappedMiniLabel);
                GUI.color = prevColor;
                GUILayout.Space(4);
            }

            EditorGUI.BeginDisabledGroup(!m_IsValid);
            if (GUILayout.Button("Generate Package", GUILayout.Height(32)))
                GeneratePackage();
            EditorGUI.EndDisabledGroup();
        }

        // ── Validation ───────────────────────────────────────────────────────────

        private void ValidateInputs()
        {
            if (string.IsNullOrWhiteSpace(m_PackageName) || !k_PackageNameRegex.IsMatch(m_PackageName))
            {
                m_ValidationMsg = "Package name must be all-lowercase reverse-domain with at least 3 segments (e.g. com.company.pkgname).";
                m_IsValid = false;
                Repaint();
                return;
            }

            if (!k_VersionRegex.IsMatch(m_Version))
            {
                m_ValidationMsg = "Version must follow MAJOR.MINOR.PATCH (e.g. 1.0.0).";
                m_IsValid = false;
                Repaint();
                return;
            }

            if (!Directory.Exists(m_OutputPath))
            {
                m_ValidationMsg = "Output folder does not exist.";
                m_IsValid = false;
                Repaint();
                return;
            }

            string targetFolder = Path.Combine(m_OutputPath, m_PackageName);
            if (Directory.Exists(targetFolder))
            {
                m_ValidationMsg = $"Folder already exists: {targetFolder}";
                m_IsValid = false;
                Repaint();
                return;
            }

            m_ValidationMsg = "";
            m_IsValid = true;
            Repaint();
        }

        // ── Generation ───────────────────────────────────────────────────────────

        private void GeneratePackage()
        {
            string packageRoot = Path.Combine(m_OutputPath, m_PackageName);

            try
            {
                // 1. Root folder
                Directory.CreateDirectory(packageRoot);

                // 2. package.json
                File.WriteAllText(Path.Combine(packageRoot, "package.json"), BuildPackageJson());

                // Derive namespace root: skip first segment (com/net/org), PascalCase the rest
                string nsRoot = DeriveNamespace(m_PackageName);

                // 3. Runtime assembly
                if (m_IncludeRuntime)
                {
                    string runtimeDir = Path.Combine(packageRoot, "Runtime");
                    Directory.CreateDirectory(runtimeDir);
                    WriteAsmdef(
                        path: Path.Combine(runtimeDir, $"{m_PackageName}.Runtime.asmdef"),
                        name: $"{m_PackageName}.Runtime",
                        rootNamespace: $"{nsRoot}.Runtime",
                        references: Array.Empty<string>(),
                        includePlatforms: Array.Empty<string>(),
                        autoReferenced: true);
                }

                // 4. Editor assembly
                if (m_IncludeEditor)
                {
                    string editorDir = Path.Combine(packageRoot, "Editor");
                    Directory.CreateDirectory(editorDir);
                    string[] refs = m_IncludeRuntime
                        ? new[] { $"{m_PackageName}.Runtime" }
                        : Array.Empty<string>();
                    WriteAsmdef(
                        path: Path.Combine(editorDir, $"{m_PackageName}.Editor.asmdef"),
                        name: $"{m_PackageName}.Editor",
                        rootNamespace: $"{nsRoot}.Editor",
                        references: refs,
                        includePlatforms: new[] { "Editor" },
                        autoReferenced: true);
                }

                // 5. Tests assemblies
                if (m_IncludeTests)
                {
                    string testsRuntimeDir = Path.Combine(packageRoot, "Tests", "Runtime");
                    Directory.CreateDirectory(testsRuntimeDir);
                    WriteTestAsmdef(
                        path: Path.Combine(testsRuntimeDir, $"{m_PackageName}.Tests.Runtime.asmdef"),
                        name: $"{m_PackageName}.Tests.Runtime",
                        rootNamespace: $"{nsRoot}.Tests.Runtime",
                        references: m_IncludeRuntime
                            ? new[] { $"{m_PackageName}.Runtime" }
                            : Array.Empty<string>(),
                        includePlatforms: Array.Empty<string>());

                    string testsEditorDir = Path.Combine(packageRoot, "Tests", "Editor");
                    Directory.CreateDirectory(testsEditorDir);
                    var editorTestRefs = new System.Collections.Generic.List<string>();
                    if (m_IncludeRuntime) editorTestRefs.Add($"{m_PackageName}.Runtime");
                    if (m_IncludeEditor) editorTestRefs.Add($"{m_PackageName}.Editor");
                    WriteTestAsmdef(
                        path: Path.Combine(testsEditorDir, $"{m_PackageName}.Tests.Editor.asmdef"),
                        name: $"{m_PackageName}.Tests.Editor",
                        rootNamespace: $"{nsRoot}.Tests.Editor",
                        references: editorTestRefs.ToArray(),
                        includePlatforms: new[] { "Editor" });
                }

                // 6. Samples~ (tilde = not imported by Unity's AssetDatabase)
                if (m_IncludeSamples)
                    Directory.CreateDirectory(Path.Combine(packageRoot, "Samples~"));

                // 7. Documentation~
                if (m_IncludeDocs)
                    Directory.CreateDirectory(Path.Combine(packageRoot, "Documentation~"));

                // 8. README.md
                if (m_IncludeReadme)
                    File.WriteAllText(Path.Combine(packageRoot, "README.md"), BuildReadme());

                // 9. CHANGELOG.md
                if (m_IncludeChangelog)
                    File.WriteAllText(Path.Combine(packageRoot, "CHANGELOG.md"), BuildChangelog());

                // 10. LICENSE
                if (m_IncludeLicense)
                    File.WriteAllText(Path.Combine(packageRoot, "LICENSE"), BuildLicense());

                // 11. Update manifest.json
                if (m_AddToManifest && IsLocalPackagesPath(m_OutputPath))
                    AddToManifest(m_PackageName);

                // 12. Refresh the asset database so Unity picks up the new package
                AssetDatabase.Refresh();

                EditorUtility.DisplayDialog(
                    "Package Created",
                    $"Package '{m_PackageName}' created at:\n{packageRoot}",
                    "OK");

                Debug.Log($"[BestPractice] Package boilerplate created at: {packageRoot}");
            }
            catch (Exception ex)
            {
                EditorUtility.DisplayDialog("Error", $"Failed to create package:\n{ex.Message}", "OK");
                Debug.LogError($"[BestPractice] PackageBoilerplateWindow: {ex}");
            }
        }

        // ── File content builders ────────────────────────────────────────────────

        private string BuildPackageJson()
        {
            string Escape(string s) => s.Replace("\\", "\\\\").Replace("\"", "\\\"");

            return $@"{{
  ""name"": ""{Escape(m_PackageName)}"",
  ""version"": ""{Escape(m_Version)}"",
  ""displayName"": ""{Escape(m_DisplayName)}"",
  ""description"": ""{Escape(m_Description)}"",
  ""unity"": ""{Escape(m_UnityVersion)}"",
  ""author"": {{
    ""name"": ""{Escape(m_AuthorName)}"",
    ""email"": ""{Escape(m_AuthorEmail)}"",
    ""url"": ""{Escape(m_AuthorUrl)}""
  }}
}}";
        }

        private string BuildReadme()
        {
            return $@"# {m_DisplayName}

{m_Description}

## Installation

Add the following line to your project's `Packages/manifest.json` under `dependencies`:

```json
""{m_PackageName}"": ""file:../LocalPackages/{m_PackageName}""
```

## License

See [LICENSE](LICENSE) for details.
";
        }

        private string BuildChangelog()
        {
            string date = DateTime.Now.ToString("yyyy-MM-dd");
            return $@"# Changelog

All notable changes to this project will be documented in this file.

## [{m_Version}] - {date}

### Added
- Initial release.
";
        }

        private string BuildLicense()
        {
            int year = DateTime.Now.Year;
            string author = string.IsNullOrWhiteSpace(m_AuthorName) ? "the author" : m_AuthorName;
            return $@"MIT License

Copyright (c) {year} {author}

Permission is hereby granted, free of charge, to any person obtaining a copy
of this software and associated documentation files (the ""Software""), to deal
in the Software without restriction, including without limitation the rights
to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
copies of the Software, and to permit persons to whom the Software is
furnished to do so, subject to the following conditions:

The above copyright notice and this permission notice shall be included in all
copies or substantial portions of the Software.

THE SOFTWARE IS PROVIDED ""AS IS"", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
SOFTWARE.
";
        }

        // ── ASMDEF helpers ───────────────────────────────────────────────────────

        private static void WriteAsmdef(
            string path,
            string name,
            string rootNamespace,
            string[] references,
            string[] includePlatforms,
            bool autoReferenced)
        {
            string ToJsonArray(string[] arr)
            {
                if (arr == null || arr.Length == 0) return "[]";
                string items = string.Join(", ", Array.ConvertAll(arr, s => $"\"{s}\""));
                return $"[{items}]";
            }

            string json = $@"{{
    ""name"": ""{name}"",
    ""rootNamespace"": ""{rootNamespace}"",
    ""references"": {ToJsonArray(references)},
    ""includePlatforms"": {ToJsonArray(includePlatforms)},
    ""excludePlatforms"": [],
    ""allowUnsafeCode"": false,
    ""overrideReferences"": false,
    ""precompiledReferences"": [],
    ""autoReferenced"": {autoReferenced.ToString().ToLower()},
    ""defineConstraints"": [],
    ""versionDefines"": [],
    ""noEngineReferences"": false
}}";

            File.WriteAllText(path, json);
        }

        private static void WriteTestAsmdef(
            string path,
            string name,
            string rootNamespace,
            string[] references,
            string[] includePlatforms)
        {
            string ToJsonArray(string[] arr)
            {
                if (arr == null || arr.Length == 0) return "[]";
                string items = string.Join(", ", Array.ConvertAll(arr, s => $"\"{s}\""));
                return $"[{items}]";
            }

            string json = $@"{{
    ""name"": ""{name}"",
    ""rootNamespace"": ""{rootNamespace}"",
    ""references"": {ToJsonArray(references)},
    ""includePlatforms"": {ToJsonArray(includePlatforms)},
    ""excludePlatforms"": [],
    ""allowUnsafeCode"": false,
    ""overrideReferences"": true,
    ""precompiledReferences"": [""nunit.framework.dll""],
    ""autoReferenced"": false,
    ""defineConstraints"": [""UNITY_INCLUDE_TESTS""],
    ""versionDefines"": [],
    ""noEngineReferences"": false
}}";

            File.WriteAllText(path, json);
        }

        // ── manifest.json update ─────────────────────────────────────────────────

        private static void AddToManifest(string packageName)
        {
            string projectRoot = Path.GetDirectoryName(Application.dataPath);
            string manifestPath = Path.Combine(projectRoot, "Packages", "manifest.json");

            if (!File.Exists(manifestPath))
            {
                Debug.LogWarning("[BestPractice] manifest.json not found — skipping manifest update.");
                return;
            }

            string content = File.ReadAllText(manifestPath);
            string entry = $"\"{packageName}\": \"file:../LocalPackages/{packageName}\"";

            if (content.Contains($"\"{packageName}\""))
            {
                Debug.Log($"[BestPractice] '{packageName}' already present in manifest.json — skipping.");
                return;
            }

            // Insert after the opening brace of the "dependencies" block
            int depIdx = content.IndexOf("\"dependencies\"", StringComparison.Ordinal);
            if (depIdx < 0)
            {
                Debug.LogWarning("[BestPractice] Could not find 'dependencies' in manifest.json — skipping.");
                return;
            }

            int braceIdx = content.IndexOf('{', depIdx);
            if (braceIdx < 0)
            {
                Debug.LogWarning("[BestPractice] Could not find dependencies opening brace — skipping.");
                return;
            }

            string insertion = $"\n    {entry},";
            content = content.Insert(braceIdx + 1, insertion);
            File.WriteAllText(manifestPath, content);
            Debug.Log($"[BestPractice] Added '{packageName}' to manifest.json.");
        }

        // ── Utilities ────────────────────────────────────────────────────────────

        /// <summary>
        /// Derives a PascalCase root namespace from a reverse-domain package name.
        /// E.g. "com.mycompany.mypackage" → "MyCompany.MyPackage"
        /// </summary>
        private static string DeriveNamespace(string packageName)
        {
            string[] parts = packageName.Split('.');
            // Skip the first TLD segment (com / net / org / unity / etc.)
            int start = parts.Length > 1 ? 1 : 0;
            var sb = new System.Text.StringBuilder();
            for (int i = start; i < parts.Length; i++)
            {
                if (i > start) sb.Append('.');
                string p = parts[i];
                if (p.Length > 0)
                    sb.Append(char.ToUpper(p[0])).Append(p.Substring(1));
            }
            return sb.Length > 0 ? sb.ToString() : "MyPackage";
        }

        private bool IsLocalPackagesPath(string path)
        {
            return path != null &&
                   path.Replace('\\', '/').TrimEnd('/').EndsWith("LocalPackages", StringComparison.OrdinalIgnoreCase);
        }

        private static string LabeledTextField(string label, string value, string tooltip)
        {
            EditorGUILayout.BeginHorizontal();
            GUILayout.Label(new GUIContent(label, tooltip), GUILayout.Width(120));
            string result = EditorGUILayout.TextField(value);
            EditorGUILayout.EndHorizontal();
            return result;
        }
    }
}
