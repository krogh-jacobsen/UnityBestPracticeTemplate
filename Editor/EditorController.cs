using UnityEditor;
using Unity.BestPractices;
using UnityEngine;

namespace Unity.BestPractices.Editor
{
    public static class EditorController
    {
        [MenuItem("Window/Best Practices/Hello editor")]
        public static void LogHelloMenu()
        {
			Debug.Log("Hello from Editor. Activated by best practice menu!");
        }

		[MenuItem("Window/Best Practices/Hello editor using package")]
        public static void LogHelloFromRuntime()
        {
			RuntimeController.LogHelloMenu();
        }

        [MenuItem("Window/Best Practices/Setup Project Folders")]
        public static void SetupProjectFolders()
        {
            string root = "_Project";
            string[] folders = new string[]
            {
                "Art",
                "Art/Materials",
                "Art/Models",
                "Art/Textures",
                "Art/Animations",
                "Audio",
                "Prefabs",
                "Scripts",
                "Scenes",
                "Settings",
                "UI"
            };

            CreateFolder("Assets", root);

            foreach (string folder in folders)
            {
                string parent = "Assets/" + root;
                string[] subfolders = folder.Split('/');
                
                string currentPath = parent;
                foreach (string subfolder in subfolders)
                {
                    CreateFolder(currentPath, subfolder);
                    currentPath += "/" + subfolder;
                }
            }

            AssetDatabase.Refresh();
            Debug.Log("Project folder structure setup complete under Assets/_Project");
        }

        private static void CreateFolder(string parent, string folderName)
        {
            string path = $"{parent}/{folderName}";
            if (!AssetDatabase.IsValidFolder(path))
            {
                AssetDatabase.CreateFolder(parent, folderName);
            }
        }
    }
}
