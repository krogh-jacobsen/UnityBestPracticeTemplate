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
            SerializedProperty defaultList = pmSO.FindProperty("m_DefaultList");

            if (defaultList == null)
            {
                var iter = pmSO.GetIterator();
                var props = new System.Text.StringBuilder(
                    $"[Best Practices] 'm_DefaultList' not found in {k_PresetManagerAssetPath}. " +
                    "Available top-level properties:");
                if (iter.NextVisible(true))
                {
                    do { props.Append($"\n  {iter.propertyPath}"); }
                    while (iter.NextVisible(false));
                }
                Debug.LogWarning(props.ToString());
                return;
            }

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
                Debug.Log("[Best Practices] All preset entries were already registered. Nothing added.");
            }
        }

        // Returns 1 if a new entry was added, 0 if skipped.
        private static int RegisterPreset(SerializedProperty defaultList, string presetPath, string folderFilter)
        {
            Preset preset = AssetDatabase.LoadAssetAtPath<Preset>(presetPath);
            if (preset == null)
            {
                Debug.LogWarning($"[Best Practices] Preset not found: {presetPath}");
                return 0;
            }

            SerializedObject presetSO = new SerializedObject(preset);
            SerializedProperty nativeTypeIDProp = presetSO.FindProperty("m_TargetType.m_NativeTypeID");
            if (nativeTypeIDProp == null)
            {
                Debug.LogWarning($"[Best Practices] Could not read native type ID from: {presetPath}");
                return 0;
            }
            int nativeTypeID = nativeTypeIDProp.intValue;

            // Find the existing entry for this importer type.
            int typeIndex = -1;
            for (int i = 0; i < defaultList.arraySize; i++)
            {
                SerializedProperty entry = defaultList.GetArrayElementAtIndex(i);
                SerializedProperty entryTypeID = entry.FindPropertyRelative("type.m_NativeTypeID");
                if (entryTypeID != null && entryTypeID.intValue == nativeTypeID)
                {
                    typeIndex = i;
                    break;
                }
            }

            // No entry for this importer type yet — create one.
            if (typeIndex == -1)
            {
                defaultList.InsertArrayElementAtIndex(defaultList.arraySize);
                typeIndex = defaultList.arraySize - 1;
                SerializedProperty newTypeEntry = defaultList.GetArrayElementAtIndex(typeIndex);
                newTypeEntry.FindPropertyRelative("type.m_NativeTypeID").intValue = nativeTypeID;
                newTypeEntry.FindPropertyRelative("defaultPresets").ClearArray();
            }

            // Skip if this filter path is already registered.
            SerializedProperty typeEntry = defaultList.GetArrayElementAtIndex(typeIndex);
            SerializedProperty defaultPresets = typeEntry.FindPropertyRelative("defaultPresets");
            for (int i = 0; i < defaultPresets.arraySize; i++)
            {
                if (defaultPresets.GetArrayElementAtIndex(i).FindPropertyRelative("m_Filter").stringValue == folderFilter)
                    return 0;
            }

            // Append the new preset entry.
            defaultPresets.InsertArrayElementAtIndex(defaultPresets.arraySize);
            SerializedProperty newEntry = defaultPresets.GetArrayElementAtIndex(defaultPresets.arraySize - 1);
            newEntry.FindPropertyRelative("m_Filter").stringValue = folderFilter;
            newEntry.FindPropertyRelative("m_Preset").objectReferenceValue = preset;

            return 1;
        }
    }
}
