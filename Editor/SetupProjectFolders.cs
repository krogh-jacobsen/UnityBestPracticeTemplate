using UnityEditor;
using UnityEngine;

namespace Unity.BestPractices.Editor
{
    /// <summary>
    /// Creates a recommended default folder structure under <c>Assets/{projectName}</c>.
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
    ///   <item><term>Special Unity Folders</term><description>StreamingAssets</description></item>
    ///   <item><term>Organization</term><description>Plugins</description></item>
    ///   <item><term>Assets root (outside project folder)</term><description>Editor, Resources, Documentation, ThirdParty</description></item>
    /// </list>
    /// Skips any folders that already exist. Calls <see cref="AssetDatabase.Refresh"/> when complete.
    /// </remarks>
    public static class SetupProjectFolders
    {
        internal const string k_ProjectNamePrefKey = "BestPractices.ProjectName";

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
            "StreamingAssets",

            // Organization
            "Plugins"
        };

        /// <summary>Opens the project-name prompt window from the menu bar.</summary>
        [MenuItem("Window/Best Practices/Setup Project Folders")]
        public static void OpenPrompt()
        {
            ProjectFolderSetupPrompt.ShowWindow();
        }

        /// <summary>
        /// Creates the full project folder structure under <c>Assets/{projectName}</c>
        /// and saves the project name to <see cref="EditorPrefs"/> for later use.
        /// </summary>
        /// <param name="projectName">The root folder name (e.g. "MyGame" → <c>Assets/MyGame</c>).</param>
        public static void Execute(string projectName)
        {
            if (string.IsNullOrWhiteSpace(projectName))
                return;

            EditorPrefs.SetString(k_ProjectNamePrefKey, projectName);

            EditorController.CreateFolder("Assets", projectName);

            foreach (string folder in k_Folders)
            {
                string parent = $"Assets/{projectName}";
                string[] subfolders = folder.Split('/');

                string currentPath = parent;
                foreach (string subfolder in subfolders)
                {
                    EditorController.CreateFolder(currentPath, subfolder);
                    currentPath += "/" + subfolder;
                }
            }

            // Root-level Assets folders (outside project folder)
            EditorController.CreateFolder("Assets", "Editor");
            EditorController.CreateFolder("Assets", "Resources");
            EditorController.CreateFolder("Assets", "Documentation");
            EditorController.CreateFolder("Assets", "ThirdParty");

            AssetDatabase.Refresh();
            Debug.Log($"[BestPractice] Project folder structure setup complete under Assets/{projectName}");
        }

        /// <summary>
        /// Creates a named game sub-system folder under the saved project root,
        /// containing <c>Scripts/</c>, <c>UI/</c>, and <c>Art/</c> subfolders.
        /// </summary>
        /// <param name="subSystemName">The name of the sub-system (becomes the folder name).</param>
        public static void CreateSubSystem(string subSystemName)
        {
            if (string.IsNullOrWhiteSpace(subSystemName))
                return;

            string projectName = EditorPrefs.GetString(k_ProjectNamePrefKey, "");
            if (string.IsNullOrWhiteSpace(projectName))
            {
                Debug.LogWarning("[BestPractice] No project name found — run Setup Project Folders first.");
                return;
            }

            string root = $"Assets/{projectName}";
            if (!AssetDatabase.IsValidFolder(root))
            {
                Debug.LogWarning($"[BestPractice] Project folder '{root}' does not exist — run Setup Project Folders first.");
                return;
            }

            EditorController.CreateFolder(root, subSystemName);
            string subRoot = $"{root}/{subSystemName}";
            EditorController.CreateFolder(subRoot, "Scripts");
            EditorController.CreateFolder(subRoot, "UI");
            EditorController.CreateFolder(subRoot, "Art");

            AssetDatabase.Refresh();
            Debug.Log($"[BestPractice] Sub-system '{subSystemName}' created at {subRoot}");
        }
    }
}
