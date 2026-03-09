using UnityEditor;
using UnityEngine;

namespace Unity.BestPractices.Editor
{
    /// <summary>
    /// Applies recommended Unity 6 project settings in one click.
    /// Accessible via the menu: <b>Window → Best Practices → Configure Project Settings</b>.
    /// </summary>
    /// <remarks>
    /// Settings applied:
    /// <list type="bullet">
    ///   <item>Enter Play Mode — enables DisableDomainReload + DisableSceneReload for faster iteration</item>
    ///   <item>Scripting Backend — sets IL2CPP for Standalone, Android, and iOS</item>
    ///   <item>API Compatibility Level — sets .NET Standard 2.1 for Standalone</item>
    /// </list>
    /// Individual settings can be reviewed in Edit → Project Settings.
    /// </remarks>
    public static class ConfigureProjectSettings
    {
        [MenuItem("Window/Best Practices/Configure Project Settings")]
        public static void Execute()
        {
            bool confirmed = EditorUtility.DisplayDialog(
                "Configure Project Settings",
                "This will apply the following recommended Unity 6 settings:\n\n" +
                "• Enter Play Mode: enable (DisableDomainReload + DisableSceneReload)\n" +
                "• Scripting Backend: IL2CPP (Standalone, Android, iOS)\n" +
                "• API Compatibility: .NET Standard 2.1 (Standalone)\n\n" +
                "Existing settings will be overwritten. Proceed?",
                "Apply Settings",
                "Cancel"
            );

            if (!confirmed)
                return;

            ApplySettings();
        }

        /// <summary>
        /// Applies recommended settings without showing a confirmation dialog.
        /// Called from the New Project Wizard.
        /// </summary>
        public static void ApplySettings()
        {
            // Enter Play Mode — skip both domain and scene reload for faster iteration.
            // Requires static state to be reset manually (e.g. in [RuntimeInitializeOnLoadMethod]).
            EditorSettings.enterPlayModeOptionsEnabled = true;
            EditorSettings.enterPlayModeOptions =
                EnterPlayModeOptions.DisableDomainReload | EnterPlayModeOptions.DisableSceneReload;

            // Scripting backend: IL2CPP for release platforms
            SetIL2CPP(BuildTargetGroup.Standalone);
            SetIL2CPP(BuildTargetGroup.Android);
            SetIL2CPP(BuildTargetGroup.iOS);

            // API Compatibility: .NET Standard 2.1 for Standalone
            PlayerSettings.SetApiCompatibilityLevel(
                BuildTargetGroup.Standalone,
                ApiCompatibilityLevel.NET_Standard);

            AssetDatabase.SaveAssets();
            Debug.Log("[Best Practices] Project settings configured. Review them in Edit > Project Settings.");
        }

        private static void SetIL2CPP(BuildTargetGroup group)
        {
            PlayerSettings.SetScriptingBackend(group, ScriptingImplementation.IL2CPP);
        }
    }
}
