using System;
using System.Reflection;
using UnityEditor;
using UnityEditor.Presets;
using UnityEngine;

namespace Unity.BestPractices.Editor
{
    public static class ConfigurePresets
    {
        private const string k_PackagePresetsPath = "Packages/com.unity.best-practices/Editor/Presets";

        // PresetManager is internal in Unity 6 — access it via reflection.
        private static readonly Type k_PresetManagerType =
            typeof(Preset).Assembly.GetType("UnityEditor.Presets.PresetManager");

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

            // GetPresetType() returns the type of the asset the preset was created from
            // (e.g. AudioImporter), not the type of the Preset asset itself.
            PresetType presetType = preset.GetPresetType();

            MethodInfo getMethod = k_PresetManagerType?.GetMethod("GetDefaultPresetsForType",
                BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic);
            MethodInfo setMethod = k_PresetManagerType?.GetMethod("SetDefaultPresetsForType",
                BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic);

            if (getMethod == null || setMethod == null)
            {
                Debug.LogError("[Best Practices] Could not access PresetManager via reflection. This Unity version may not be supported.");
                return;
            }

            var existing = (DefaultPreset[])getMethod.Invoke(null, new object[] { presetType });

            foreach (DefaultPreset entry in existing)
            {
                if (entry.filter == folderFilter)
                    return;
            }

            DefaultPreset[] updated = new DefaultPreset[existing.Length + 1];
            existing.CopyTo(updated, 0);
            updated[existing.Length] = new DefaultPreset(folderFilter, preset);

            setMethod.Invoke(null, new object[] { presetType, updated });
        }
    }
}
