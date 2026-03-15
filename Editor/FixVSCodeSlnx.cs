using UnityEditor;
using UnityEngine;
using System.IO;

namespace Unity.BestPractices.Editor
{
    /// <summary>
    /// Fixes "Active document not part of open workspace" errors in VS Code caused by
    /// Unity 6 generating .slnx solution files that OmniSharp does not support.
    /// </summary>
    /// <remarks>
    /// The fix removes <c>dotnet.preferCSharpExtension</c> from <c>.vscode/settings.json</c>
    /// (which forced OmniSharp) and adds <c>ms-dotnettools.csdevkit</c> to
    /// <c>.vscode/extensions.json</c> so VS Code uses C# Dev Kit, which supports .slnx.
    /// </remarks>
    public static class FixVSCodeSlnx
    {
        private const string k_CsDevKitId = "ms-dotnettools.csdevkit";
        private const string k_OmniSharpKey = "dotnet.preferCSharpExtension";

        /// <summary>
        /// Returns true when .vscode/settings.json no longer forces OmniSharp
        /// and .vscode/extensions.json recommends C# Dev Kit.
        /// </summary>
        public static bool IsFixed(string projectRoot)
        {
            bool settingsClean = !SettingsFileContainsOmniSharpKey(projectRoot);
            bool devKitRecommended = ExtensionsFileContainsDevKit(projectRoot);
            return settingsClean && devKitRecommended;
        }

        /// <summary>
        /// Applies the fix: removes <c>dotnet.preferCSharpExtension</c> from settings.json
        /// and ensures <c>ms-dotnettools.csdevkit</c> is in extensions.json recommendations.
        /// </summary>
        [MenuItem("Tools/Unity Project Configurator/Fix VS Code .slnx", false, 160)]
        public static void Execute()
        {
            string projectRoot = Path.GetDirectoryName(Application.dataPath);
            string vscodeDir = Path.Combine(projectRoot, ".vscode");
            Directory.CreateDirectory(vscodeDir);

            FixSettingsJson(vscodeDir);
            FixExtensionsJson(vscodeDir);

            Debug.Log("[BestPractice] VS Code .slnx fix applied. Reload VS Code window (Cmd/Ctrl+Shift+P → Reload Window) to take effect.");
        }

        private static void FixSettingsJson(string vscodeDir)
        {
            string path = Path.Combine(vscodeDir, "settings.json");

            if (!File.Exists(path))
                return;

            string content = File.ReadAllText(path);

            if (!content.Contains(k_OmniSharpKey))
                return;

            // Remove the line that contains the OmniSharp key.
            // Handles both "dotnet.preferCSharpExtension": true and : false, with optional trailing comma.
            var lines = new System.Collections.Generic.List<string>(content.Split('\n'));
            for (int i = lines.Count - 1; i >= 0; i--)
            {
                if (lines[i].Contains(k_OmniSharpKey))
                {
                    lines.RemoveAt(i);

                    // Clean up a trailing comma on the previous non-empty line if the removed line
                    // was the last property (i.e. next non-empty line is a closing brace).
                    int prevIndex = i - 1;
                    while (prevIndex >= 0 && string.IsNullOrWhiteSpace(lines[prevIndex]))
                        prevIndex--;

                    int nextIndex = i;
                    while (nextIndex < lines.Count && string.IsNullOrWhiteSpace(lines[nextIndex]))
                        nextIndex++;

                    if (prevIndex >= 0 && nextIndex < lines.Count)
                    {
                        string nextTrimmed = lines[nextIndex].Trim();
                        if (nextTrimmed == "}" || nextTrimmed == "},")
                        {
                            // Previous line must not end with a trailing comma.
                            lines[prevIndex] = lines[prevIndex].TrimEnd().TrimEnd(',');
                        }
                    }
                }
            }

            File.WriteAllText(path, string.Join("\n", lines));
            Debug.Log($"[BestPractice] Removed '{k_OmniSharpKey}' from {path}");
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

        private static bool SettingsFileContainsOmniSharpKey(string projectRoot)
        {
            string path = Path.Combine(projectRoot, ".vscode", "settings.json");
            return File.Exists(path) && File.ReadAllText(path).Contains(k_OmniSharpKey);
        }

        private static bool ExtensionsFileContainsDevKit(string projectRoot)
        {
            string path = Path.Combine(projectRoot, ".vscode", "extensions.json");
            return File.Exists(path) && File.ReadAllText(path).Contains(k_CsDevKitId);
        }
    }
}
