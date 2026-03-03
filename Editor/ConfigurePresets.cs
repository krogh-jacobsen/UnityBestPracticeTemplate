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

            SetupAudioPresets();
            SetupTexturePresets();

            AssetDatabase.SaveAssets();
            Debug.Log("[Best Practices] Import presets configured. See Edit > Project Settings > Preset Manager.");
        }

        private static void SetupAudioPresets()
        {
            string path = $"{k_PackagePresetsPath}/Audio";
            RegisterPreset($"{path}/AmbienceAudioImporter.preset", "glob:\"Assets/Art/Audio/Ambience/**\"");
            RegisterPreset($"{path}/MusicAudioImporter.preset",    "glob:\"Assets/Art/Audio/Music/**\"");
            RegisterPreset($"{path}/SFXAudioImporter.preset",      "glob:\"Assets/Art/Audio/SFX/**\"");
            RegisterPreset($"{path}/UIAudioImporter.preset",       "glob:\"Assets/Art/Audio/UI/**\"");
        }

        private static void SetupTexturePresets()
        {
            string path = $"{k_PackagePresetsPath}/Textures";
            RegisterPreset($"{path}/SingleSpriteTextureImporter.preset",  "glob:\"Assets/Art/Textures/UI/**\"");
            RegisterPreset($"{path}/SpriteAtlasTextureImporter.preset",   "glob:\"Assets/Art/Textures/UI/Atlas/**\"");
            RegisterPreset($"{path}/AlbedoTextureImporter.preset",        "glob:\"Assets/Art/Textures/Albedo/**\"");
            RegisterPreset($"{path}/NormalTextureImporter.preset",        "glob:\"Assets/Art/Textures/Normal/**\"");
            RegisterPreset($"{path}/RoughnessTextureImporter.preset",     "glob:\"Assets/Art/Textures/Roughness/**\"");
            RegisterPreset($"{path}/MaskTextureImporter.preset",          "glob:\"Assets/Art/Textures/Mask/**\"");
            RegisterPreset($"{path}/HDRITextureImporter.preset",          "glob:\"Assets/Art/Textures/HDRI/**\"");
        }

        private static void RegisterPreset(string presetPath, string folderFilter)
        {
            Preset preset = AssetDatabase.LoadAssetAtPath<Preset>(presetPath);
            if (preset == null)
            {
                Debug.LogWarning($"[Best Practices] Preset not found: {presetPath}");
                return;
            }

            // Read the importer's native type ID from the preset's own serialized data.
            // This avoids any dependency on the internal PresetManager class.
            SerializedObject presetSO = new SerializedObject(preset);
            SerializedProperty nativeTypeIDProp = presetSO.FindProperty("m_TargetType.m_NativeTypeID");
            if (nativeTypeIDProp == null)
            {
                Debug.LogWarning($"[Best Practices] Could not read native type ID from: {presetPath}");
                return;
            }
            int nativeTypeID = nativeTypeIDProp.intValue;

            // Load the Preset Manager settings asset directly.
            UnityEngine.Object[] pmAssets = AssetDatabase.LoadAllAssetsAtPath(k_PresetManagerAssetPath);
            if (pmAssets.Length == 0)
            {
                Debug.LogWarning($"[Best Practices] Could not load {k_PresetManagerAssetPath}");
                return;
            }

            SerializedObject pmSO = new SerializedObject(pmAssets[0]);
            SerializedProperty defaultList = pmSO.FindProperty("m_DefaultList");

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
                    return;
            }

            // Append the new preset entry.
            defaultPresets.InsertArrayElementAtIndex(defaultPresets.arraySize);
            SerializedProperty newEntry = defaultPresets.GetArrayElementAtIndex(defaultPresets.arraySize - 1);
            newEntry.FindPropertyRelative("m_Filter").stringValue = folderFilter;
            newEntry.FindPropertyRelative("m_Preset").objectReferenceValue = preset;

            pmSO.ApplyModifiedProperties();
        }
    }
}
