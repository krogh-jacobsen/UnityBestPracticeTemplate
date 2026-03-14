using UnityEditor;
using Unity.BestPractices;
using UnityEngine;

namespace Unity.BestPractices.Editor
{
    /// <summary>
    /// Provides shared editor utilities and test menu items for the Best Practices package.
    /// Accessible via the menu: <b>Window → Best Practices</b>.
    /// </summary>
    public static class EditorController
    {
        /// <summary>
        /// Logs a hello message from the editor to the Unity console.
        /// </summary>
        /// <remarks>
        /// Menu: <b>Window → Best Practices → Test Editor Log</b>.
        /// Useful for verifying that the package's editor assembly is loaded correctly.
        /// </remarks>
        [MenuItem("Tools/Unity Workbench/Utilities/Test Editor Log", false, 252)]
        public static void LogHelloMenu()
        {
            Debug.Log("Hello from Editor. Activated by best practice menu!");
        }

        /// <summary>
        /// Logs a hello message by invoking <see cref="RuntimeController.LogHelloMenu"/>
        /// from the runtime assembly.
        /// </summary>
        /// <remarks>
        /// Menu: <b>Window → Best Practices → Test Editor Log (calls runtime)</b>.
        /// Demonstrates that the editor assembly can reference and call into the runtime assembly.
        /// </remarks>
        [MenuItem("Tools/Unity Workbench/Utilities/Test Editor Log (calls runtime)", false, 253)]
        public static void LogHelloFromRuntime()
        {
            RuntimeController.LogHelloMenu();
        }

        /// <summary>
        /// Creates a subfolder inside the given parent path if it does not already exist.
        /// Shared utility used by other generator classes.
        /// </summary>
        /// <param name="parent">The parent folder path (e.g. <c>"Assets/_ProjectName"</c>).</param>
        /// <param name="folderName">The name of the subfolder to create.</param>
        public static void CreateFolder(string parent, string folderName)
        {
            string path = $"{parent}/{folderName}";
            if (!AssetDatabase.IsValidFolder(path))
            {
                AssetDatabase.CreateFolder(parent, folderName);
            }
        }
    }
}
