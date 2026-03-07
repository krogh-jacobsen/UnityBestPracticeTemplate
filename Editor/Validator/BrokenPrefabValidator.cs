using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace UnityBestPractices.Editor.Validator
{
    public class BrokenPrefabValidator : IValidator
    {
        public string Name => "Broken Prefab References";
        public string Description => "Finds prefabs with missing or broken references";

        public ValidationResult Validate()
        {
            var issues = new List<ValidationIssue>();

            // Find all prefabs
            string[] prefabGuids = AssetDatabase.FindAssets("t:Prefab");

            foreach (string guid in prefabGuids)
            {
                string prefabPath = AssetDatabase.GUIDToAssetPath(guid);
                GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(prefabPath);

                if (prefab == null)
                    continue;

                // Check for missing scripts
                Component[] components = prefab.GetComponentsInChildren<Component>(true);
                bool hasMissingScripts = false;
                bool hasMissingReferences = false;

                foreach (Component component in components)
                {
                    if (component == null)
                    {
                        hasMissingScripts = true;
                        continue;
                    }

                    // Check for missing references in serialized fields
                    SerializedObject so = new SerializedObject(component);
                    SerializedProperty sp = so.GetIterator();

                    while (sp.NextVisible(true))
                    {
                        if (sp.propertyType == SerializedPropertyType.ObjectReference)
                        {
                            if (sp.objectReferenceValue == null &&
                                sp.objectReferenceInstanceIDValue != 0)
                            {
                                hasMissingReferences = true;
                                break;
                            }
                        }
                    }

                    if (hasMissingReferences)
                        break;
                }

                // Report issues
                if (hasMissingScripts)
                {
                    issues.Add(new ValidationIssue(
                        $"Prefab has missing scripts: {prefab.name}",
                        ValidationSeverity.Error,
                        prefabPath,
                        Name
                    ));
                }

                if (hasMissingReferences)
                {
                    issues.Add(new ValidationIssue(
                        $"Prefab has missing references: {prefab.name}",
                        ValidationSeverity.Warning,
                        prefabPath,
                        Name
                    ));
                }
            }

            return new ValidationResult(Name, issues.ToArray());
        }
    }
}
