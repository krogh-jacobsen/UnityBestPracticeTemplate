using UnityEditor;
using UnityEditor.Presets;
using UnityEngine;

namespace Unity.BestPractices.Editor
{
    public static class ConfigurePresets
    {
        private const string k_PackagePresetsPath = "Packages/com.unity.best-practices/Editor/Presets";
        private const string k_PresetManagerAssetPath = "ProjectSettings/PresetManager.asset";

        /// <summary>
        /// Registers all package import presets in the Preset Manager with folder-path filters.
        /// </summary>
        [MenuItem("Window/Best Practices/Configure Import Presets")]
        private static void SetupPresets()
        {
            bool confirmed = EditorUtility.DisplayDialog(
                "Configure Import Presets",
                "This will register Best Practices import presets in the Preset Manager.\n\n" +
                "Existing entries for the same filter paths will be skipped.\n\n" +
                "Proceed?",
                "Configure",
                "Cancel"
            );

            if (!confirmed)
                return;

            UnityEngine.Object[] pmAssets = AssetDatabase.LoadAllAssetsAtPath(k_PresetManagerAssetPath);
            if (pmAssets.Length == 0 || pmAssets[0] == null)
            {
                Debug.LogWarning($"[Best Practices] Could not load {k_PresetManagerAssetPath}");
                return;
            }

            SerializedObject pmSO = new SerializedObject(pmAssets[0]);
            SerializedProperty defaultList = pmSO.FindProperty("m_DefaultPresets");

            if (defaultList == null)
            {
                LogTopLevelProperties(pmSO);
                return;
            }

            // Always log the actual structure so we can verify field names.
            LogEntryStructure(defaultList);

            int addedCount = 0;

            string audioPath = $"{k_PackagePresetsPath}/Audio";
            addedCount += RegisterPreset(defaultList, $"{audioPath}/AmbienceAudioImporter.preset", "glob:\"Assets/Art/Audio/Ambience/**\"");
            addedCount += RegisterPreset(defaultList, $"{audioPath}/MusicAudioImporter.preset",    "glob:\"Assets/Art/Audio/Music/**\"");
            addedCount += RegisterPreset(defaultList, $"{audioPath}/SFXAudioImporter.preset",      "glob:\"Assets/Art/Audio/SFX/**\"");
            addedCount += RegisterPreset(defaultList, $"{audioPath}/UIAudioImporter.preset",       "glob:\"Assets/Art/Audio/UI/**\"");

            string texturePath = $"{k_PackagePresetsPath}/Textures";
            addedCount += RegisterPreset(defaultList, $"{texturePath}/SingleSpriteTextureImporter.preset",  "glob:\"Assets/Art/Textures/UI/**\"");
            addedCount += RegisterPreset(defaultList, $"{texturePath}/SpriteAtlasTextureImporter.preset",   "glob:\"Assets/Art/Textures/UI/Atlas/**\"");
            addedCount += RegisterPreset(defaultList, $"{texturePath}/AlbedoTextureImporter.preset",        "glob:\"Assets/Art/Textures/Albedo/**\"");
            addedCount += RegisterPreset(defaultList, $"{texturePath}/NormalTextureImporter.preset",        "glob:\"Assets/Art/Textures/Normal/**\"");
            addedCount += RegisterPreset(defaultList, $"{texturePath}/RoughnessTextureImporter.preset",     "glob:\"Assets/Art/Textures/Roughness/**\"");
            addedCount += RegisterPreset(defaultList, $"{texturePath}/MaskTextureImporter.preset",          "glob:\"Assets/Art/Textures/Mask/**\"");
            addedCount += RegisterPreset(defaultList, $"{texturePath}/HDRITextureImporter.preset",          "glob:\"Assets/Art/Textures/HDRI/**\"");

            if (addedCount > 0)
            {
                pmSO.ApplyModifiedProperties();
                EditorUtility.SetDirty(pmAssets[0]);
                AssetDatabase.SaveAssets();
                Debug.Log($"[Best Practices] Added {addedCount} preset entries. See Edit > Project Settings > Preset Manager.");
            }
            else
            {
                Debug.Log("[Best Practices] No preset entries were added (all already registered or errors logged above).");
            }
        }

        // Returns 1 if an entry was added, 0 if skipped or failed.
        private static int RegisterPreset(SerializedProperty defaultList, string presetPath, string folderFilter)
        {
            Preset preset = AssetDatabase.LoadAssetAtPath<Preset>(presetPath);
            if (preset == null)
            {
                Debug.LogWarning($"[Best Practices] Preset not found: {presetPath}");
                return 0;
            }

            // Skip if an entry with this filter already exists.
            for (int i = 0; i < defaultList.arraySize; i++)
            {
                SerializedProperty entry = defaultList.GetArrayElementAtIndex(i);
                SerializedProperty filterProp = entry.FindPropertyRelative("m_Filter");
                if (filterProp != null && filterProp.stringValue == folderFilter)
                    return 0;
            }

            // Insert a new entry (Unity copies the last element's structure).
            int newIndex = defaultList.arraySize;
            defaultList.InsertArrayElementAtIndex(newIndex);
            SerializedProperty newEntry = defaultList.GetArrayElementAtIndex(newIndex);

            SerializedProperty newFilter = newEntry.FindPropertyRelative("m_Filter");
            SerializedProperty newPreset = newEntry.FindPropertyRelative("m_Preset");

            if (newFilter == null || newPreset == null)
            {
                // Structure doesn't match flat (m_Filter + m_Preset) layout — revert.
                defaultList.DeleteArrayElementAtIndex(newIndex);
                Debug.LogWarning(
                    $"[Best Practices] Could not write '{System.IO.Path.GetFileName(presetPath)}': " +
                    "m_Filter or m_Preset not found on entry. Check the diagnostic log above for the actual field names.");
                return 0;
            }

            newFilter.stringValue = folderFilter;
            newPreset.objectReferenceValue = preset;
            return 1;
        }

        // Logs the direct children of the first entry in the list so we can see the actual field names.
        private static void LogEntryStructure(SerializedProperty defaultList)
        {
            var sb = new System.Text.StringBuilder(
                $"[Best Practices] m_DefaultPresets has {defaultList.arraySize} entries.");

            if (defaultList.arraySize == 0)
            {
                sb.Append(" (empty — add one preset manually first to allow structure discovery)");
                Debug.Log(sb.ToString());
                return;
            }

            sb.Append(" First entry direct children:");
            SerializedProperty first = defaultList.GetArrayElementAtIndex(0);
            SerializedProperty iter  = first.Copy();
            SerializedProperty end   = first.GetEndProperty();

            if (iter.Next(true))
            {
                while (!SerializedProperty.EqualContents(iter, end))
                {
                    sb.Append($"\n  [{iter.name}] propertyType={iter.propertyType} isArray={iter.isArray}");

                    if (iter.propertyType == SerializedPropertyType.String)
                        sb.Append($"  value=\"{iter.stringValue}\"");
                    else if (iter.isArray)
                        sb.Append($"  arraySize={iter.arraySize}");
                    else if (iter.propertyType == SerializedPropertyType.Generic)
                    {
                        // Drill one level into structs (e.g. a type field).
                        SerializedProperty child    = iter.Copy();
                        SerializedProperty childEnd = iter.GetEndProperty();
                        if (child.Next(true))
                        {
                            while (!SerializedProperty.EqualContents(child, childEnd))
                            {
                                sb.Append($"\n    [{child.name}] type={child.propertyType}");
                                if (child.propertyType == SerializedPropertyType.Integer)
                                    sb.Append($"  value={child.intValue}");
                                else if (child.propertyType == SerializedPropertyType.String)
                                    sb.Append($"  value=\"{child.stringValue}\"");
                                child.Next(false);
                            }
                        }
                    }

                    iter.Next(false);
                }
            }

            Debug.Log(sb.ToString());
        }

        private static void LogTopLevelProperties(SerializedObject so)
        {
            var iter  = so.GetIterator();
            var props = new System.Text.StringBuilder("[Best Practices] PresetManager.asset top-level properties:");
            if (iter.NextVisible(true))
            {
                do { props.Append($"\n  {iter.propertyPath}"); }
                while (iter.NextVisible(false));
            }
            Debug.LogWarning(props.ToString());
        }
    }
}
