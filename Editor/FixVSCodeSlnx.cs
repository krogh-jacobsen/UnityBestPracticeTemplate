using UnityEditor;
using UnityEngine;
using System.IO;
using System.Linq;

namespace Unity.BestPractices.Editor
{
    /// <summary>
    /// Fixes "Active document not part of open workspace" errors in VS Code caused by
    /// Unity 6 generating .slnx solution files that OmniSharp does not support.
    /// </summary>
    /// <remarks>
    /// The fix:
    /// 1. Removes <c>dotnet.preferCSharpExtension</c> from <c>.vscode/settings.json</c>
    ///    (which forced OmniSharp).
    /// 2. Adds <c>dotnet.defaultSolution</c> pointing to the project's .slnx file so
    ///    C# Dev Kit knows which solution to load.
    /// 3. Sets <c>dotnet.enableWorkspaceBasedDevelopment</c> to <c>false</c> so C# Dev Kit
    ///    uses the solution file rather than workspace-based discovery.
    /// 4. Adds <c>ms-dotnettools.csdevkit</c> to <c>.vscode/extensions.json</c> so VS Code
    ///    uses C# Dev Kit, which supports .slnx.
    /// </remarks>
    public static class FixVSCodeSlnx
    {
        private const string k_CsDevKitId = "ms-dotnettools.csdevkit";
        private const string k_OmniSharpKey = "dotnet.preferCSharpExtension";
        private const string k_DefaultSolutionKey = "dotnet.defaultSolution";
        private const string k_WorkspaceDevKey = "dotnet.enableWorkspaceBasedDevelopment";

        /// <summary>
        /// Returns true when .vscode/settings.json no longer forces OmniSharp,
        /// has <c>dotnet.defaultSolution</c> pointing to a .slnx file,
        /// has <c>dotnet.enableWorkspaceBasedDevelopment</c> set to false,
        /// and .vscode/extensions.json recommends C# Dev Kit.
        /// </summary>
        public static bool IsFixed(string projectRoot)
        {
            bool settingsClean = !SettingsFileContainsKey(projectRoot, k_OmniSharpKey);
            bool hasDefaultSolution = SettingsFileContainsKey(projectRoot, k_DefaultSolutionKey);
            bool hasWorkspaceDev = SettingsFileContainsKey(projectRoot, k_WorkspaceDevKey);
            bool devKitRecommended = ExtensionsFileContainsDevKit(projectRoot);
            return settingsClean && hasDefaultSolution && hasWorkspaceDev && devKitRecommended;
        }

        /// <summary>
        /// Applies the fix: removes <c>dotnet.preferCSharpExtension</c> from settings.json,
        /// adds <c>dotnet.defaultSolution</c> and <c>dotnet.enableWorkspaceBasedDevelopment</c>,
        /// and ensures <c>ms-dotnettools.csdevkit</c> is in extensions.json recommendations.
        /// </summary>
        [MenuItem("Tools/Unity Project Configurator/Fix VS Code .slnx", false, 160)]
        public static void Execute()
        {
            string projectRoot = Path.GetDirectoryName(Application.dataPath);
            string vscodeDir = Path.Combine(projectRoot, ".vscode");
            Directory.CreateDirectory(vscodeDir);

            FixSettingsJson(projectRoot, vscodeDir);
            FixExtensionsJson(vscodeDir);

            Debug.Log("[BestPractice] VS Code .slnx fix applied. Reload VS Code window (Cmd/Ctrl+Shift+P → Reload Window) to take effect.");
        }

        /// <summary>
        /// Finds the first .slnx file in the project root directory.
        /// Returns the filename (not full path) or null if none is found.
        /// </summary>
        private static string FindSlnxFileName(string projectRoot)
        {
            if (!Directory.Exists(projectRoot))
                return null;

            string[] slnxFiles = Directory.GetFiles(projectRoot, "*.slnx");
            if (slnxFiles.Length == 0)
                return null;

            return Path.GetFileName(slnxFiles[0]);
        }

        private static void FixSettingsJson(string projectRoot, string vscodeDir)
        {
            string path = Path.Combine(vscodeDir, "settings.json");

            // If settings.json does not exist, create it with just the required settings.
            if (!File.Exists(path))
            {
                string slnxName = FindSlnxFileName(projectRoot);
                string content = "{\n";
                if (slnxName != null)
                    content += "    \"" + k_DefaultSolutionKey + "\": \"" + slnxName + "\",\n";
                content += "    \"" + k_WorkspaceDevKey + "\": false\n";
                content += "}\n";
                File.WriteAllText(path, content);
                Debug.Log($"[BestPractice] Created {path} with C# Dev Kit solution settings.");
                return;
            }

            string fileContent = File.ReadAllText(path);
            var lines = new System.Collections.Generic.List<string>(fileContent.Split('\n'));
            bool modified = false;

            // Step 1: Remove the OmniSharp key if present.
            if (fileContent.Contains(k_OmniSharpKey))
            {
                for (int i = lines.Count - 1; i >= 0; i--)
                {
                    if (lines[i].Contains(k_OmniSharpKey))
                    {
                        lines.RemoveAt(i);
                        modified = true;

                        CleanUpTrailingComma(lines, i);
                    }
                }
                Debug.Log($"[BestPractice] Removed '{k_OmniSharpKey}' from {path}");
            }

            // Step 2: Add dotnet.defaultSolution if not present.
            string slnxFileName = FindSlnxFileName(projectRoot);
            if (slnxFileName != null && !lines.Any(l => l.Contains(k_DefaultSolutionKey)))
            {
                InsertSettingBeforeClosingBrace(lines, "\"" + k_DefaultSolutionKey + "\": \"" + slnxFileName + "\"");
                modified = true;
                Debug.Log($"[BestPractice] Added '{k_DefaultSolutionKey}: {slnxFileName}' to {path}");
            }

            // Step 3: Add dotnet.enableWorkspaceBasedDevelopment if not present.
            if (!lines.Any(l => l.Contains(k_WorkspaceDevKey)))
            {
                InsertSettingBeforeClosingBrace(lines, "\"" + k_WorkspaceDevKey + "\": false");
                modified = true;
                Debug.Log($"[BestPractice] Added '{k_WorkspaceDevKey}: false' to {path}");
            }

            if (modified)
                File.WriteAllText(path, string.Join("\n", lines));
        }

        /// <summary>
        /// Inserts a new JSON property line before the last closing brace in settings.json.
        /// Handles trailing comma placement on the preceding property.
        /// </summary>
        private static void InsertSettingBeforeClosingBrace(System.Collections.Generic.List<string> lines, string setting)
        {
            // Find the last closing brace.
            int braceIndex = -1;
            for (int i = lines.Count - 1; i >= 0; i--)
            {
                if (lines[i].Trim().StartsWith("}"))
                {
                    braceIndex = i;
                    break;
                }
            }

            if (braceIndex < 0)
                return;

            // Find the previous non-empty line and ensure it ends with a comma.
            int prevIndex = braceIndex - 1;
            while (prevIndex >= 0 && string.IsNullOrWhiteSpace(lines[prevIndex]))
                prevIndex--;

            if (prevIndex >= 0)
            {
                string trimmed = lines[prevIndex].TrimEnd();
                // Add comma if the previous line is a property value (not an opening brace).
                if (!trimmed.EndsWith(",") && !trimmed.EndsWith("{") && !trimmed.EndsWith("["))
                {
                    lines[prevIndex] = trimmed + ",";
                }
            }

            lines.Insert(braceIndex, "    " + setting);
        }

        /// <summary>
        /// After removing a line at removeIndex, cleans up trailing commas on the
        /// previous non-empty line if the next non-empty line is a closing brace.
        /// </summary>
        private static void CleanUpTrailingComma(System.Collections.Generic.List<string> lines, int removeIndex)
        {
            int prevIndex = removeIndex - 1;
            while (prevIndex >= 0 && string.IsNullOrWhiteSpace(lines[prevIndex]))
                prevIndex--;

            int nextIndex = removeIndex;
            while (nextIndex < lines.Count && string.IsNullOrWhiteSpace(lines[nextIndex]))
                nextIndex++;

            if (prevIndex >= 0 && nextIndex < lines.Count)
            {
                string nextTrimmed = lines[nextIndex].Trim();
                if (nextTrimmed == "}" || nextTrimmed == "},")
                {
                    lines[prevIndex] = lines[prevIndex].TrimEnd().TrimEnd(',');
                }
            }
        }

        private static void FixExtensionsJson(string vscodeDir)
        {
            string path = Path.Combine(vscodeDir, "extensions.json");

            if (!File.Exists(path))
            {
                string newContent = "{\n    \"recommendations\": [\n        \"" + k_CsDevKitId + "\"\n    ]\n}\n";
                File.WriteAllText(path, newContent);
                Debug.Log($"[BestPractice] Created {path} with C# Dev Kit recommendation.");
                return;
            }

            string content = File.ReadAllText(path);

            if (content.Contains(k_CsDevKitId))
                return;

            // Insert the C# Dev Kit entry into the recommendations array.
            // Find the opening bracket of the recommendations array and inject after it.
            int recommendationsIdx = content.IndexOf("\"recommendations\"");
            if (recommendationsIdx < 0)
            {
                // No recommendations array — append one before the closing brace.
                int closingBrace = content.LastIndexOf('}');
                if (closingBrace < 0)
                {
                    File.WriteAllText(path, "{\n    \"recommendations\": [\n        \"" + k_CsDevKitId + "\"\n    ]\n}\n");
                }
                else
                {
                    string prefix = content.Substring(0, closingBrace).TrimEnd().TrimEnd(',');
                    string appended = prefix + ",\n    \"recommendations\": [\n        \"" + k_CsDevKitId + "\"\n    ]\n}\n";
                    File.WriteAllText(path, appended);
                }
            }
            else
            {
                int bracketIdx = content.IndexOf('[', recommendationsIdx);
                if (bracketIdx >= 0)
                {
                    string insertion = "\n        \"" + k_CsDevKitId + "\",";
                    content = content.Insert(bracketIdx + 1, insertion);
                    File.WriteAllText(path, content);
                }
            }

            Debug.Log($"[BestPractice] Added '{k_CsDevKitId}' to recommendations in {path}");
        }

        private static bool SettingsFileContainsKey(string projectRoot, string key)
        {
            string path = Path.Combine(projectRoot, ".vscode", "settings.json");
            return File.Exists(path) && File.ReadAllText(path).Contains(key);
        }

        private static bool ExtensionsFileContainsDevKit(string projectRoot)
        {
            string path = Path.Combine(projectRoot, ".vscode", "extensions.json");
            return File.Exists(path) && File.ReadAllText(path).Contains(k_CsDevKitId);
        }
    }
}
