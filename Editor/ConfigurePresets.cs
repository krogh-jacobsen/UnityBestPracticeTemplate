using UnityEditor;
using UnityEditor.Presets;
using UnityEngine;

namespace Unity.BestPractices.Editor
{
    public static class ConfigurePresets
    {
        private const string k_PackagePresetsPath = "Packages/com.unity.best-practices/Editor/Presets";
        private const string k_PresetManagerAssetPath = "ProjectSettings/PresetManager.asset";

        // ── Preset asset paths ────────────────────────────────────────────────

        private static readonly string k_AmbiencePath = $"{k_PackagePresetsPath}/Audio/AmbienceAudioImporter.preset";
        private static readonly string k_MusicPath = $"{k_PackagePresetsPath}/Audio/MusicAudioImporter.preset";
        private static readonly string k_SFXPath = $"{k_PackagePresetsPath}/Audio/SFXAudioImporter.preset";
        private static readonly string k_UIAudioPath = $"{k_PackagePresetsPath}/Audio/UIAudioImporter.preset";

        private static readonly string k_SingleSpritePath = $"{k_PackagePresetsPath}/Textures/SingleSpriteTextureImporter.preset";
        private static readonly string k_SpriteAtlasPath = $"{k_PackagePresetsPath}/Textures/SpriteAtlasTextureImporter.preset";
        private static readonly string k_AlbedoPath = $"{k_PackagePresetsPath}/Textures/AlbedoTextureImporter.preset";
        private static readonly string k_NormalPath = $"{k_PackagePresetsPath}/Textures/NormalTextureImporter.preset";
        private static readonly string k_RoughnessPath = $"{k_PackagePresetsPath}/Textures/RoughnessTextureImporter.preset";
        private static readonly string k_MaskPath = $"{k_PackagePresetsPath}/Textures/MaskTextureImporter.preset";
        private static readonly string k_HDRIPath = $"{k_PackagePresetsPath}/Textures/HDRITextureImporter.preset";

        private static readonly string k_FBXModelPath = $"{k_PackagePresetsPath}/Models/FBXModelImporter.preset";
        private static readonly string k_FBXAnimPath = $"{k_PackagePresetsPath}/Animations/FBXAnimationImporter.preset";

        // ── Folder filters ────────────────────────────────────────────────────

        private const string k_AmbienceFilter = "glob:\"Assets/**/Audio/Ambience/**\"";
        private const string k_MusicFilter = "glob:\"Assets/**/Audio/Music/**\"";
        private const string k_SFXFilter = "glob:\"Assets/**/Audio/SFX/**\"";
        private const string k_UIAudioFilter = "glob:\"Assets/**/Audio/UI/**\"";

        private const string k_SingleSpriteFilter = "glob:\"Assets/**/UI/Sprites/**\"";
        private const string k_SpriteAtlasFilter = "glob:\"Assets/**/UI/Sprites/Atlas/**\"";
        private const string k_AlbedoFilter = "glob:\"Assets/**/Textures/Albedo/**\"";
        private const string k_NormalFilter = "glob:\"Assets/**/Textures/Normal/**\"";
        private const string k_RoughnessFilter = "glob:\"Assets/**/Textures/Roughness/**\"";
        private const string k_MaskFilter = "glob:\"Assets/**/Textures/Mask/**\"";
        private const string k_HDRIFilter = "glob:\"Assets/**/Textures/HDRI/**\"";

        private const string k_FBXModelFilter = "glob:\"Assets/**/Art/Models/**\"";
        private const string k_FBXAnimFilter = "glob:\"Assets/**/Art/Animations/**\"";

        // ── Status checks ────────────────────────────────────────────────────

        public static bool IsAmbienceConfigured => IsPresetRegistered(k_AmbiencePath, k_AmbienceFilter);
        public static bool IsMusicConfigured => IsPresetRegistered(k_MusicPath, k_MusicFilter);
        public static bool IsSFXConfigured => IsPresetRegistered(k_SFXPath, k_SFXFilter);
        public static bool IsUIAudioConfigured => IsPresetRegistered(k_UIAudioPath, k_UIAudioFilter);

        public static bool IsSingleSpriteConfigured => IsPresetRegistered(k_SingleSpritePath, k_SingleSpriteFilter);
        public static bool IsSpriteAtlasConfigured => IsPresetRegistered(k_SpriteAtlasPath, k_SpriteAtlasFilter);
        public static bool IsAlbedoConfigured => IsPresetRegistered(k_AlbedoPath, k_AlbedoFilter);
        public static bool IsNormalConfigured => IsPresetRegistered(k_NormalPath, k_NormalFilter);
        public static bool IsRoughnessConfigured => IsPresetRegistered(k_RoughnessPath, k_RoughnessFilter);
        public static bool IsMaskConfigured => IsPresetRegistered(k_MaskPath, k_MaskFilter);
        public static bool IsHDRIConfigured => IsPresetRegistered(k_HDRIPath, k_HDRIFilter);

        public static bool IsFBXModelConfigured => IsPresetRegistered(k_FBXModelPath, k_FBXModelFilter);
        public static bool IsFBXAnimationConfigured => IsPresetRegistered(k_FBXAnimPath, k_FBXAnimFilter);

        // ── Menu entry ────────────────────────────────────────────────────────

        [MenuItem("Tools/Unity Workbench/Setup/Configure Import Presets", false, 52)]
        public static void Execute()
        {
            PresetsWindow.ShowWindow();
        }

        // ── Individual apply methods ──────────────────────────────────────────

        public static void ApplyAmbience() => ApplySingle(k_AmbiencePath, k_AmbienceFilter);
        public static void ApplyMusic() => ApplySingle(k_MusicPath, k_MusicFilter);
        public static void ApplySFX() => ApplySingle(k_SFXPath, k_SFXFilter);
        public static void ApplyUIAudio() => ApplySingle(k_UIAudioPath, k_UIAudioFilter);

        public static void ApplySingleSprite() => ApplySingle(k_SingleSpritePath, k_SingleSpriteFilter);
        public static void ApplySpriteAtlas() => ApplySingle(k_SpriteAtlasPath, k_SpriteAtlasFilter);
        public static void ApplyAlbedo() => ApplySingle(k_AlbedoPath, k_AlbedoFilter);
        public static void ApplyNormal() => ApplySingle(k_NormalPath, k_NormalFilter);
        public static void ApplyRoughness() => ApplySingle(k_RoughnessPath, k_RoughnessFilter);
        public static void ApplyMask() => ApplySingle(k_MaskPath, k_MaskFilter);
        public static void ApplyHDRI() => ApplySingle(k_HDRIPath, k_HDRIFilter);

        public static void ApplyFBXModel() => ApplySingle(k_FBXModelPath, k_FBXModelFilter);
        public static void ApplyFBXAnimation() => ApplySingle(k_FBXAnimPath, k_FBXAnimFilter);

        // ── Bulk apply (used by New Project Wizard RunFullSetup) ──────────────

        public static void ApplyAllPresets()
        {
            if (!TryGetDefaultPresets(out SerializedObject pmSO, out SerializedProperty defaultList))
                return;

            int addedCount = 0;
            addedCount += RegisterPreset(defaultList, k_AmbiencePath, k_AmbienceFilter);
            addedCount += RegisterPreset(defaultList, k_MusicPath, k_MusicFilter);
            addedCount += RegisterPreset(defaultList, k_SFXPath, k_SFXFilter);
            addedCount += RegisterPreset(defaultList, k_UIAudioPath, k_UIAudioFilter);
            addedCount += RegisterPreset(defaultList, k_SingleSpritePath, k_SingleSpriteFilter);
            addedCount += RegisterPreset(defaultList, k_SpriteAtlasPath, k_SpriteAtlasFilter);
            addedCount += RegisterPreset(defaultList, k_AlbedoPath, k_AlbedoFilter);
            addedCount += RegisterPreset(defaultList, k_NormalPath, k_NormalFilter);
            addedCount += RegisterPreset(defaultList, k_RoughnessPath, k_RoughnessFilter);
            addedCount += RegisterPreset(defaultList, k_MaskPath, k_MaskFilter);
            addedCount += RegisterPreset(defaultList, k_HDRIPath, k_HDRIFilter);
            addedCount += RegisterPreset(defaultList, k_FBXModelPath, k_FBXModelFilter);
            addedCount += RegisterPreset(defaultList, k_FBXAnimPath, k_FBXAnimFilter);

            if (addedCount > 0)
            {
                pmSO.ApplyModifiedProperties();
                EditorUtility.SetDirty(pmSO.targetObject);
                AssetDatabase.SaveAssets();
                Debug.Log($"[Best Practices] Added {addedCount} preset entries. See Edit > Project Settings > Preset Manager.");
            }
            else
            {
                Debug.Log("[Best Practices] All preset entries were already registered. Nothing added.");
            }
        }

        // ── Internal helpers ──────────────────────────────────────────────────

        private static void ApplySingle(string presetPath, string filter)
        {
            if (!TryGetDefaultPresets(out SerializedObject pmSO, out SerializedProperty defaultList))
                return;

            int added = RegisterPreset(defaultList, presetPath, filter);
            if (added > 0)
            {
                pmSO.ApplyModifiedProperties();
                EditorUtility.SetDirty(pmSO.targetObject);
                AssetDatabase.SaveAssets();
                Debug.Log($"[Best Practices] Registered preset: {System.IO.Path.GetFileNameWithoutExtension(presetPath)}");
            }
            else
            {
                Debug.Log($"[Best Practices] Preset already registered: {System.IO.Path.GetFileNameWithoutExtension(presetPath)}");
            }
        }

        public static bool IsPresetRegistered(string presetPath, string folderFilter)
        {
            if (!TryGetDefaultPresets(out _, out SerializedProperty defaultList))
                return false;

            Preset preset = AssetDatabase.LoadAssetAtPath<Preset>(presetPath);
            if (preset == null) return false;

            // Match by preset object reference so the status reflects any registration,
            // regardless of which filter path was used when the entry was created.
            for (int i = 0; i < defaultList.arraySize; i++)
            {
                SerializedProperty entry = defaultList.GetArrayElementAtIndex(i);
                SerializedProperty innerArray = entry.FindPropertyRelative("second");
                if (innerArray == null) continue;

                for (int j = 0; j < innerArray.arraySize; j++)
                {
                    SerializedProperty presetProp = innerArray.GetArrayElementAtIndex(j).FindPropertyRelative("m_Preset");
                    if (presetProp != null && presetProp.objectReferenceValue == preset)
                        return true;
                }
            }
            return false;
        }

        private static bool TryGetDefaultPresets(out SerializedObject pmSO, out SerializedProperty defaultList)
        {
            pmSO = null;
            defaultList = null;

            UnityEngine.Object[] pmAssets = AssetDatabase.LoadAllAssetsAtPath(k_PresetManagerAssetPath);
            if (pmAssets.Length == 0 || pmAssets[0] == null)
            {
                Debug.LogWarning($"[Best Practices] Could not load {k_PresetManagerAssetPath}");
                return false;
            }

            pmSO = new SerializedObject(pmAssets[0]);
            defaultList = pmSO.FindProperty("m_DefaultPresets");
            if (defaultList == null)
            {
                Debug.LogWarning("[Best Practices] Could not find 'm_DefaultPresets' in PresetManager.asset");
                return false;
            }

            return true;
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
                SerializedProperty entry = defaultList.GetArrayElementAtIndex(i);
                SerializedProperty typeID = entry.FindPropertyRelative("first.m_NativeTypeID");
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
                SerializedProperty newInner = newGroup.FindPropertyRelative("second");

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
            SerializedProperty group = defaultList.GetArrayElementAtIndex(typeIndex);
            SerializedProperty innerArray = group.FindPropertyRelative("second");

            // Skip if this preset asset is already registered (regardless of filter path).
            for (int i = 0; i < innerArray.arraySize; i++)
            {
                SerializedProperty innerEntry = innerArray.GetArrayElementAtIndex(i);
                SerializedProperty presetRef = innerEntry.FindPropertyRelative("m_Preset");
                if (presetRef != null && presetRef.objectReferenceValue == preset)
                    return 0;
            }

            // Append new entry to the inner array.
            innerArray.InsertArrayElementAtIndex(innerArray.arraySize);
            SerializedProperty newEntry = innerArray.GetArrayElementAtIndex(innerArray.arraySize - 1);
            SerializedProperty newFilter = newEntry.FindPropertyRelative("m_Filter");
            SerializedProperty newPreset = newEntry.FindPropertyRelative("m_Preset");

            if (newFilter == null || newPreset == null)
            {
                innerArray.DeleteArrayElementAtIndex(innerArray.arraySize - 1);
                Debug.LogWarning($"[Best Practices] Inner entry missing m_Filter/m_Preset for: {presetPath}");
                return 0;
            }

            newFilter.stringValue = folderFilter;
            newPreset.objectReferenceValue = preset;
            return 1;
        }
    }
}
