using UnityEditor;
using UnityEditor.Build;
using UnityEditor.VersionControl;
using UnityEngine;

namespace Unity.BestPractices.Editor
{
    /// <summary>
    /// Applies recommended Unity 6 project settings — either all at once or individually.
    /// Open the dedicated panel via <b>Window → Best Practices → Configure Project Settings</b>,
    /// or call <see cref="ApplySettings"/> to apply everything non-interactively.
    /// </summary>
    public static class ConfigureProjectSettings
    {
        // ── Status checks ────────────────────────────────────────────────────

        public static bool IsEnterPlayModeConfigured =>
            EditorSettings.enterPlayModeOptionsEnabled &&
            EditorSettings.enterPlayModeOptions.HasFlag(EnterPlayModeOptions.DisableDomainReload) &&
            EditorSettings.enterPlayModeOptions.HasFlag(EnterPlayModeOptions.DisableSceneReload);

        public static bool IsIL2CPPConfigured =>
            PlayerSettings.GetScriptingBackend(NamedBuildTarget.Standalone) == ScriptingImplementation.IL2CPP &&
            PlayerSettings.GetScriptingBackend(NamedBuildTarget.Android) == ScriptingImplementation.IL2CPP &&
            PlayerSettings.GetScriptingBackend(NamedBuildTarget.iOS) == ScriptingImplementation.IL2CPP;

        public static bool IsApiCompatibilityConfigured =>
            PlayerSettings.GetApiCompatibilityLevel(NamedBuildTarget.Standalone) == ApiCompatibilityLevel.NET_Standard;

        public static bool IsAssetSerializationConfigured =>
            EditorSettings.serializationMode == SerializationMode.ForceText;

        public static bool IsVersionControlConfigured =>
            VersionControlSettings.mode == "Visible Meta Files";

        // ── Individual apply methods ──────────────────────────────────────────

        [MenuItem("Window/Best Practices/Configure Project Settings")]
        public static void Execute()
        {
            ProjectSettingsWindow.ShowWindow();
        }

        public static void ApplyEnterPlayMode()
        {
            EditorSettings.enterPlayModeOptionsEnabled = true;
            EditorSettings.enterPlayModeOptions =
                EnterPlayModeOptions.DisableDomainReload | EnterPlayModeOptions.DisableSceneReload;
            Debug.Log("[Best Practices] Enter Play Mode options configured.");
        }

        public static void ApplyIL2CPP()
        {
            PlayerSettings.SetScriptingBackend(NamedBuildTarget.Standalone, ScriptingImplementation.IL2CPP);
            PlayerSettings.SetScriptingBackend(NamedBuildTarget.Android, ScriptingImplementation.IL2CPP);
            PlayerSettings.SetScriptingBackend(NamedBuildTarget.iOS, ScriptingImplementation.IL2CPP);
            AssetDatabase.SaveAssets();
            Debug.Log("[Best Practices] Scripting backend set to IL2CPP for Standalone, Android, iOS.");
        }

        public static void ApplyApiCompatibility()
        {
            PlayerSettings.SetApiCompatibilityLevel(NamedBuildTarget.Standalone, ApiCompatibilityLevel.NET_Standard);
            AssetDatabase.SaveAssets();
            Debug.Log("[Best Practices] API Compatibility set to .NET Standard 2.1 for Standalone.");
        }

        public static void ApplyAssetSerialization()
        {
            EditorSettings.serializationMode = SerializationMode.ForceText;
            Debug.Log("[Best Practices] Asset Serialization set to Force Text.");
        }

        public static void ApplyVersionControl()
        {
            VersionControlSettings.mode = "Visible Meta Files";
            Debug.Log("[Best Practices] Version Control set to Visible Meta Files.");
        }

        // ── Apply all (used by New Project Wizard) ────────────────────────────

        /// <summary>Applies all recommended settings without showing a confirmation dialog.</summary>
        public static void ApplySettings()
        {
            ApplyEnterPlayMode();
            ApplyIL2CPP();
            ApplyApiCompatibility();
            ApplyAssetSerialization();
            ApplyVersionControl();
            Debug.Log("[Best Practices] All project settings configured. Review them in Edit > Project Settings.");
        }
    }
}
