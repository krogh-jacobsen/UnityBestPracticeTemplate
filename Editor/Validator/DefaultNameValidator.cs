using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace UnityBestPractices.Editor.Validator
{
    public class DefaultNameValidator : IValidator
    {
        public string Name => "Default Asset Names";
        public string Description => "Finds assets with default Unity names";

        private static readonly string[] DefaultNames = new string[]
        {
            "New Material",
            "New Animation",
            "New Animator Controller",
            "New Physics Material",
            "New Terrain",
            "New Render Texture",
            "New Audio Mixer",
            "GameObject",
            "Cube",
            "Sphere",
            "Capsule",
            "Cylinder",
            "Plane",
            "Quad"
        };

        public ValidationResult Validate()
        {
            var issues = new List<ValidationIssue>();

            // Search for all asset types that commonly have default names
            string[] searchFilters = new string[]
            {
                "t:Material",
                "t:AnimationClip",
                "t:AnimatorController",
                "t:PhysicMaterial",
                "t:Terrain",
                "t:RenderTexture",
                "t:AudioMixerController",
                "t:Prefab"
            };

            var foundAssets = new HashSet<string>();

            foreach (string filter in searchFilters)
            {
                string[] guids = AssetDatabase.FindAssets(filter);
                foreach (string guid in guids)
                {
                    string assetPath = AssetDatabase.GUIDToAssetPath(guid);
                    foundAssets.Add(assetPath);
                }
            }

            // Check each asset for default names
            foreach (string assetPath in foundAssets)
            {
                string assetName = System.IO.Path.GetFileNameWithoutExtension(assetPath);

                foreach (string defaultName in DefaultNames)
                {
                    if (assetName.Equals(defaultName, System.StringComparison.OrdinalIgnoreCase) ||
                        assetName.StartsWith(defaultName + " ", System.StringComparison.OrdinalIgnoreCase) ||
                        assetName.StartsWith(defaultName + "(", System.StringComparison.OrdinalIgnoreCase))
                    {
                        issues.Add(new ValidationIssue(
                            $"Asset has default name: {assetName}",
                            ValidationSeverity.Warning,
                            assetPath,
                            Name
                        ));
                        break;
                    }
                }
            }

            return new ValidationResult(Name, issues.ToArray());
        }
    }
}
