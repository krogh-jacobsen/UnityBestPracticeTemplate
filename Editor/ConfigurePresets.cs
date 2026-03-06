using UnityEditor;
using UnityEditor.Presets;
using UnityEngine;

namespace Unity.BestPractices.Editor
{
    public static class ConfigurePresets
    {
        private const string k_PackagePresetsPath = "Packages/com.unity.best-practices/Editor/Presets";
        private const string k_PresetManagerAssetPath = "ProjectSettings/PresetManager.asset";

        // Serialized field names discovered from ProjectSettings/PresetManager.asset:
        // m_DefaultPresets[i].first          — PresetType struct (m_NativeTypeID, ...)
        // m_DefaultPresets[i].second[j]      — DefaultPreset entry
        // m_DefaultPresets[i].second[j].m_Filter  — glob filter string
        // m_DefaultPresets[i].second[j].m_Preset  — Preset asset reference

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
                Debug.LogWarning("[Best Practices] Could not find 'm_DefaultPresets' in PresetManager.asset");
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

            string modelPath = $"{k_PackagePresetsPath}/Models";
            addedCount += RegisterPreset(defaultList, $"{modelPath}/FBXModelImporter.preset",               "glob:\"Assets/Art/Models/**\"");

            string animationPath = $"{k_PackagePresetsPath}/Animations";
            addedCount += RegisterPreset(defaultList, $"{animationPath}/FBXAnimationImporter.preset",       "glob:\"Assets/Art/Animations/**\"");

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

        // Returns 1 if a new entry was added, 0 if skipped or failed.
        private static int RegisterPreset(SerializedProperty defaultList, string presetPath, string folderFilter)
        {
            Preset preset = AssetDatabase.LoadAssetAtPath<Preset>(presetPath);
            if (preset == null)
            {
                Debug.LogWarning($"[Best Practices] Preset not found: {presetPath}");
                return 0;
            }

            // Read the importer native type ID from the preset's own serialized data.
            SerializedObject presetSO = new SerializedObject(preset);
            SerializedProperty nativeTypeIDProp = presetSO.FindProperty("m_TargetType.m_NativeTypeID");
            if (nativeTypeIDProp == null)
            {
                Debug.LogWarning($"[Best Practices] Could not read native type ID from: {presetPath}");
                return 0;
            }
            int nativeTypeID = nativeTypeIDProp.intValue;

            // Find the outer group entry whose 'first.m_NativeTypeID' matches this importer type.
            int typeIndex = -1;
            for (int i = 0; i < defaultList.arraySize; i++)
            {
                SerializedProperty entry   = defaultList.GetArrayElementAtIndex(i);
                SerializedProperty typeID  = entry.FindPropertyRelative("first.m_NativeTypeID");
                if (typeID != null && typeID.intValue == nativeTypeID)
                {
                    typeIndex = i;
                    break;
                }
            }

            // No group for this importer type yet — create one.
            if (typeIndex == -1)
            {
                defaultList.InsertArrayElementAtIndex(defaultList.arraySize);
                typeIndex = defaultList.arraySize - 1;
                SerializedProperty newGroup = defaultList.GetArrayElementAtIndex(typeIndex);

                SerializedProperty newTypeID = newGroup.FindPropertyRelative("first.m_NativeTypeID");
                SerializedProperty newInner  = newGroup.FindPropertyRelative("second");

                if (newTypeID == null || newInner == null)
                {
                    defaultList.DeleteArrayElementAtIndex(typeIndex);
                    Debug.LogWarning($"[Best Practices] Could not create type group for: {presetPath}");
                    return 0;
                }

                newTypeID.intValue = nativeTypeID;
                newInner.ClearArray();
            }

            // Get the inner 'second' array for this type group.
            SerializedProperty group      = defaultList.GetArrayElementAtIndex(typeIndex);
            SerializedProperty innerArray = group.FindPropertyRelative("second");

            // Skip if this filter path is already registered.
            for (int i = 0; i < innerArray.arraySize; i++)
            {
                SerializedProperty innerEntry = innerArray.GetArrayElementAtIndex(i);
                SerializedProperty filterProp = innerEntry.FindPropertyRelative("m_Filter");
                if (filterProp != null && filterProp.stringValue == folderFilter)
                    return 0;
            }

            // Append new entry to the inner array.
            innerArray.InsertArrayElementAtIndex(innerArray.arraySize);
            SerializedProperty newEntry  = innerArray.GetArrayElementAtIndex(innerArray.arraySize - 1);
            SerializedProperty newFilter = newEntry.FindPropertyRelative("m_Filter");
            SerializedProperty newPreset = newEntry.FindPropertyRelative("m_Preset");

            if (newFilter == null || newPreset == null)
            {
                innerArray.DeleteArrayElementAtIndex(innerArray.arraySize - 1);
                Debug.LogWarning($"[Best Practices] Inner entry missing m_Filter/m_Preset for: {presetPath}");
                return 0;
            }

            newFilter.stringValue        = folderFilter;
            newPreset.objectReferenceValue = preset;
            return 1;
        }
    }
}
