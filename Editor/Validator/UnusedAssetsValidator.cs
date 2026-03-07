using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace UnityBestPractices.Editor.Validator
{
    public class UnusedAssetsValidator : IValidator
    {
        public string Name => "Unused Assets";
        public string Description => "Finds assets that are not referenced by any other assets or scenes";

        public ValidationResult Validate()
        {
            var issues = new List<ValidationIssue>();

            // Get all asset paths in the project (excluding special folders)
            string[] allAssetPaths = AssetDatabase.GetAllAssetPaths()
                .Where(path => path.StartsWith("Assets/") &&
                               !path.Contains("/Editor/") &&
                               !path.Contains("/Resources/") && // Resources are loaded dynamically
                               !path.Contains("/StreamingAssets/") &&
                               !IsSceneFile(path) &&
                               !IsScriptFile(path) &&
                               !IsMetaFile(path))
                .ToArray();

            // Build a set of all referenced assets
            var referencedAssets = new HashSet<string>();

            // Check all scenes for references
            string[] sceneGuids = AssetDatabase.FindAssets("t:Scene");
            foreach (string sceneGuid in sceneGuids)
            {
                string scenePath = AssetDatabase.GUIDToAssetPath(sceneGuid);
                string[] dependencies = AssetDatabase.GetDependencies(scenePath, true);
                foreach (string dep in dependencies)
                {
                    referencedAssets.Add(dep);
                }
            }

            // Check all prefabs for references
            string[] prefabGuids = AssetDatabase.FindAssets("t:Prefab");
            foreach (string prefabGuid in prefabGuids)
            {
                string prefabPath = AssetDatabase.GUIDToAssetPath(prefabGuid);
                string[] dependencies = AssetDatabase.GetDependencies(prefabPath, true);
                foreach (string dep in dependencies)
                {
                    referencedAssets.Add(dep);
                }
            }

            // Check all ScriptableObjects for references
            string[] scriptableObjectGuids = AssetDatabase.FindAssets("t:ScriptableObject");
            foreach (string soGuid in scriptableObjectGuids)
            {
                string soPath = AssetDatabase.GUIDToAssetPath(soGuid);
                string[] dependencies = AssetDatabase.GetDependencies(soPath, true);
                foreach (string dep in dependencies)
                {
                    referencedAssets.Add(dep);
                }
            }

            // Find unused assets
            foreach (string assetPath in allAssetPaths)
            {
                if (!referencedAssets.Contains(assetPath))
                {
                    string assetName = System.IO.Path.GetFileName(assetPath);
                    issues.Add(new ValidationIssue(
                        $"Unused asset: {assetName}",
                        ValidationSeverity.Info,
                        assetPath,
                        Name
                    ));
                }
            }

            return new ValidationResult(Name, issues.ToArray());
        }

        private bool IsSceneFile(string path)
        {
            return path.EndsWith(".unity");
        }

        private bool IsScriptFile(string path)
        {
            return path.EndsWith(".cs") || path.EndsWith(".js") || path.EndsWith(".boo");
        }

        private bool IsMetaFile(string path)
        {
            return path.EndsWith(".meta");
        }
    }
}
