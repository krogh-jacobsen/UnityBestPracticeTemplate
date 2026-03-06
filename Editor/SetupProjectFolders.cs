using UnityEditor;
using UnityEngine;

namespace Unity.BestPractices.Editor
{
    /// <summary>
    /// Creates a recommended default folder structure under <c>Assets/_ProjectName</c>.
    /// Accessible via the menu: <b>Window → Best Practices → Setup Project Folders</b>.
    /// </summary>
    /// <remarks>
    /// The generated structure includes:
    /// <list type="table">
    ///   <listheader><term>Category</term><description>Folders</description></listheader>
    ///   <item><term>Art Assets</term><description>Animations, Audio, Fonts, Materials, Models, Shaders, Sprites, Textures, VFX</description></item>
    ///   <item><term>Code</term><description>Scripts (Core, UI, Utilities), Editor</description></item>
    ///   <item><term>Data</term><description>Data, Prefabs, Scenes, Settings</description></item>
    ///   <item><term>UI</term><description>Sprites, Atlas, UXML, USS, Settings (for UI Toolkit)</description></item>
    ///   <item><term>Special Unity Folders</term><description>Resources, StreamingAssets</description></item>
    ///   <item><term>Organization</term><description>Documentation, Plugins, ThirdParty</description></item>
    /// </list>
    /// Skips any folders that already exist. Calls <see cref="AssetDatabase.Refresh"/> when complete.
    /// </remarks>
    public static class SetupProjectFolders
    {
        private const string k_Root = "_ProjectName";

        private static readonly string[] k_Folders = new string[]
        {
            // Art Assets
            "Art",
            "Art/Animations",
            "Art/Animations/Clips",
            "Art/Animations/Controllers",
            "Art/Audio",
            "Art/Audio/Ambience",
            "Art/Audio/Music",
            "Art/Audio/SFX",
            "Art/Audio/UI",
            "Art/Fonts",
            "Art/Materials",
            "Art/Materials/Physics",
            "Art/Models",
            "Art/Shaders",
            "Art/Sprites",
            "Art/Textures",
            "Art/Textures/Albedo",
            "Art/Textures/Normal",
            "Art/Textures/Roughness",
            "Art/Textures/Mask",
            "Art/Textures/HDRI",
            "Art/VFX",

            // Code
            "Scripts",
            "Scripts/Core",
            "Scripts/UI",
            "Scripts/Utilities",
            "Editor",

            // Data
            "Data",
            "Prefabs",
            "Scenes",
            "Settings",

            // UI
            "UI",
            "UI/Sprites",
            "UI/Sprites/Atlas",
            "UI/UXML",
            "UI/USS",
            "UI/Settings",

            // Special Unity Folders
            "Resources",
            "StreamingAssets",

            // Organization
            "Documentation",
            "Plugins",
            "ThirdParty"
        };

        /// <summary>
        /// Creates the full project folder structure under <c>Assets/_ProjectName</c>.
        /// </summary>
        [MenuItem("Window/Best Practices/Setup Project Folders")]
        public static void Execute()
        {
            EditorController.CreateFolder("Assets", k_Root);

            foreach (string folder in k_Folders)
            {
                string parent = $"Assets/{k_Root}";
                string[] subfolders = folder.Split('/');

                string currentPath = parent;
                foreach (string subfolder in subfolders)
                {
                    EditorController.CreateFolder(currentPath, subfolder);
                    currentPath += "/" + subfolder;
                }
            }

            AssetDatabase.Refresh();
            Debug.Log($"[BestPractice] Project folder structure setup complete under Assets/{k_Root}");
        }
    }
}

