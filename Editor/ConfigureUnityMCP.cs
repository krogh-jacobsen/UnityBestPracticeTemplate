using UnityEditor;
using UnityEngine;
using System.IO;

namespace Unity.BestPractices.Editor
{
    /// <summary>
    /// Creates <c>UserSettings/mcp.json</c> to configure Unity MCP (Model Context Protocol),
    /// enabling AI assistants such as GitHub Copilot and Claude Code to interact with the
    /// Unity Editor via the Unity relay server.
    /// Accessible via <b>Tools → Unity Project Configurator → AI → Configure Unity MCP</b>.
    /// </summary>
    /// <remarks>
    /// <c>UserSettings/</c> is excluded from version control (it is listed in the generated
    /// .gitignore), so this is a per-developer machine-local configuration.
    /// The relay binary is installed alongside Unity at <c>~/.unity/relay/</c>.
    /// </remarks>
    public static class ConfigureUnityMCP
    {
        private static readonly string k_RelayPathMacArm64 = Path.Combine(
            System.Environment.GetFolderPath(System.Environment.SpecialFolder.UserProfile),
            ".unity/relay/relay_mac_arm64.app/Contents/MacOS/relay_mac_arm64");

        private const string k_McpJsonContent =
            "{\n" +
            "  \"enabled\": true,\n" +
            "  \"path\": \"\",\n" +
            "  \"mcpServers\": {\n" +
            "    \"unity-mcp\": {\n" +
            "      \"command\": \"~/.unity/relay/relay_mac_arm64.app/Contents/MacOS/relay_mac_arm64\",\n" +
            "      \"args\": [\"--mcp\"],\n" +
            "      \"env\": {\n" +
            "        \"UNITY_MCP_LOG_LEVEL\": \"info\"\n" +
            "      }\n" +
            "    }\n" +
            "  }\n" +
            "}\n";

        /// <summary>
        /// Returns true when <c>UserSettings/mcp.json</c> exists and contains
        /// the Unity MCP server configuration with <c>enabled: true</c>.
        /// </summary>
        public static bool IsConfigured
        {
            get
            {
                string path = GetMcpJsonPath();
                if (!File.Exists(path))
                    return false;

                string content = File.ReadAllText(path);
                return content.Contains("\"enabled\": true") && content.Contains("\"unity-mcp\"");
            }
        }

        /// <summary>
        /// Returns true when the Unity relay binary is present at
        /// <c>~/.unity/relay/relay_mac_arm64.app/</c> (macOS ARM64).
        /// </summary>
        public static bool IsRelayInstalled => File.Exists(k_RelayPathMacArm64);

        /// <summary>
        /// Returns the configured mcp.json path for the current project.
        /// </summary>
        public static string McpJsonPath => GetMcpJsonPath();

        /// <summary>
        /// Writes <c>UserSettings/mcp.json</c> with the Unity MCP relay configuration.
        /// Prompts for confirmation when the file already exists.
        /// </summary>
        [MenuItem("Tools/Unity Project Configurator/AI/Configure Unity MCP", false, 170)]
        public static void Execute()
        {
            string path = GetMcpJsonPath();
            Directory.CreateDirectory(Path.GetDirectoryName(path));

            if (File.Exists(path))
            {
                bool overwrite = EditorUtility.DisplayDialog(
                    "Configure Unity MCP",
                    "UserSettings/mcp.json already exists. Overwrite it?",
                    "Overwrite",
                    "Cancel"
                );
                if (!overwrite) return;
            }

            File.WriteAllText(path, k_McpJsonContent);
            Debug.Log($"[BestPractice] Unity MCP configured at: {path}\nRestart your IDE to activate MCP integration.");
        }

        private static string GetMcpJsonPath()
        {
            return Path.Combine(
                Path.GetDirectoryName(Application.dataPath),
                "UserSettings",
                "mcp.json");
        }
    }
}
