using System;
using System.Reflection;
using UnityEditor;
using UnityEditor.Build;
using UnityEngine;

namespace Unity.BestPractices.Editor
{
    /// <summary>
    /// Manages iteration-speed settings tuned for day-to-day development vs pre-release validation.
    /// These are intentionally separate from the permanent project settings in <see cref="ConfigureProjectSettings"/>.
    /// </summary>
    public static class ConfigureIterationSettings
    {
        // ── Status checks ────────────────────────────────────────────────────

        public static bool IsBackendDev =>
            PlayerSettings.GetScriptingBackend(NamedBuildTarget.Standalone) == ScriptingImplementation.Mono2x;

        public static bool IsBackendRelease =>
            PlayerSettings.GetScriptingBackend(NamedBuildTarget.Standalone) == ScriptingImplementation.IL2CPP;

        /// <summary>
        /// IL2CPP Code Generation: Faster (smaller) builds — preferred during dev iteration.
        /// Only meaningful when the scripting backend is IL2CPP.
        /// </summary>
        public static bool IsIL2CppCodeGenDev =>
            PlayerSettings.GetIl2CppCodeGeneration(NamedBuildTarget.Standalone) == Il2CppCodeGeneration.OptimizeSize;

        /// <summary>
        /// IL2CPP Code Generation: Faster runtime — preferred for release / perf testing.
        /// Only meaningful when the scripting backend is IL2CPP.
        /// </summary>
        public static bool IsIL2CppCodeGenRelease =>
            PlayerSettings.GetIl2CppCodeGeneration(NamedBuildTarget.Standalone) == Il2CppCodeGeneration.OptimizeSpeed;

        // Values: 0 = RecompileAndContinuePlaying, 1 = RecompileAfterFinishedPlaying, 2 = StopPlayingAndRecompile
        public static bool IsScriptChangesConfigured
        {
            get
            {
                var assets = AssetDatabase.LoadAllAssetsAtPath("ProjectSettings/EditorSettings.asset");
                if (assets.Length == 0) return false;
                var so = new SerializedObject(assets[0]);
                var prop = so.FindProperty("m_ScriptChangesDuringPlayback");
                return prop != null && prop.intValue == 1;
            }
        }

        /// <summary>Auto refresh is stored in EditorPrefs — machine-local, not committed to VCS.</summary>
        public static bool IsAutoRefreshDisabled =>
            EditorPrefs.GetInt("kAutoRefreshMode", 1) == 0;

        // ── Burst (optional package — resolved via reflection) ────────────────

        /// <summary>True when the Burst package is installed in this project.</summary>
        public static bool IsBurstInstalled => BurstOptionsType != null;

        /// <summary>True when Burst synchronous compilation is OFF (async = better editor responsiveness).</summary>
        public static bool IsBurstAsyncConfigured
        {
            get
            {
                var t = BurstOptionsType;
                if (t == null) return false;
                var prop = t.GetProperty("ForceSynchronousBurstCompilations",
                    BindingFlags.Public | BindingFlags.Static);
                return prop != null && !(bool)prop.GetValue(null);
            }
        }

        private static Type BurstOptionsType =>
            Type.GetType("Unity.Burst.Editor.BurstEditorOptions, Unity.Burst.Editor");

        public static bool IsAsyncShadersDev => EditorSettings.asyncShaderCompilation;
        public static bool IsAsyncShadersRelease => !EditorSettings.asyncShaderCompilation;

        public static bool IsManagedStrippingDev =>
            PlayerSettings.GetManagedStrippingLevel(NamedBuildTarget.Standalone) == ManagedStrippingLevel.Disabled;
        public static bool IsManagedStrippingRelease =>
            PlayerSettings.GetManagedStrippingLevel(NamedBuildTarget.Standalone) != ManagedStrippingLevel.Disabled;

        // ── Profile detection ────────────────────────────────────────────────

        public enum ProfileState { Dev, Release, Mixed, Unknown }

        public static ProfileState CurrentProfile
        {
            get
            {
                bool devBackend = IsBackendDev;
                bool relBackend = IsBackendRelease;
                bool devCodeGen = !relBackend || IsIL2CppCodeGenDev;   // N/A when Mono — counts as dev-ok
                bool relCodeGen = !relBackend || IsIL2CppCodeGenRelease;
                bool scriptChanges = IsScriptChangesConfigured;
                bool autoRefreshDev = IsAutoRefreshDisabled;
                bool asyncShadersDev = IsAsyncShadersDev;
                bool strippingDev = IsManagedStrippingDev;
                bool strippingRel = IsManagedStrippingRelease;
                bool burstOk = !IsBurstInstalled || IsBurstAsyncConfigured;

                bool allDev = devBackend && devCodeGen && scriptChanges && autoRefreshDev && asyncShadersDev && strippingDev && burstOk;
                bool allRelease = relBackend && relCodeGen && scriptChanges && !autoRefreshDev && !asyncShadersDev && strippingRel;

                if (allDev) return ProfileState.Dev;
                if (allRelease) return ProfileState.Release;
                if (!devBackend && !relBackend) return ProfileState.Unknown;
                return ProfileState.Mixed;
            }
        }

        // ── Profile apply methods ────────────────────────────────────────────

        public static void ApplyDevFastProfile()
        {
            ApplyBackendDev();
            PlayerSettings.SetIl2CppCodeGeneration(NamedBuildTarget.Standalone, Il2CppCodeGeneration.OptimizeSize);
            ApplyScriptChanges();
            ApplyAutoRefreshOff();
            ApplyAsyncShadersOn();
            ApplyManagedStrippingDev();
            if (IsBurstInstalled) ApplyBurstAsync();
            AssetDatabase.SaveAssets();
            Debug.Log("[Best Practices] Dev Fast profile applied: Mono backend, OptimizeSize IL2CPP codegen, Async Shaders on, Stripping disabled, Auto Refresh off, Burst async.");
        }

        public static void ApplyReleasePerfProfile()
        {
            ApplyBackendRelease();
            PlayerSettings.SetIl2CppCodeGeneration(NamedBuildTarget.Standalone, Il2CppCodeGeneration.OptimizeSpeed);
            ApplyScriptChanges();
            ApplyAutoRefreshOn();
            ApplyAsyncShadersOff();
            ApplyManagedStrippingRelease();
            AssetDatabase.SaveAssets();
            Debug.Log("[Best Practices] Release Perf profile applied: IL2CPP backend, OptimizeSpeed codegen, Async Shaders off, Stripping minimal, Auto Refresh on.");
        }

        // ── Individual apply methods ─────────────────────────────────────────

        public static void ApplyBackendDev()
        {
            PlayerSettings.SetScriptingBackend(NamedBuildTarget.Standalone, ScriptingImplementation.Mono2x);
            AssetDatabase.SaveAssets();
            Debug.Log("[Best Practices] Scripting backend set to Mono (Standalone) for fast iteration.");
        }

        public static void ApplyBackendRelease()
        {
            PlayerSettings.SetScriptingBackend(NamedBuildTarget.Standalone, ScriptingImplementation.IL2CPP);
            AssetDatabase.SaveAssets();
            Debug.Log("[Best Practices] Scripting backend set to IL2CPP (Standalone) for release validation.");
        }

        public static void ApplyIL2CppCodeGenDev()
        {
            PlayerSettings.SetIl2CppCodeGeneration(NamedBuildTarget.Standalone, Il2CppCodeGeneration.OptimizeSize);
            AssetDatabase.SaveAssets();
            Debug.Log("[Best Practices] IL2CPP Code Generation set to OptimizeSize (Faster builds).");
        }

        public static void ApplyIL2CppCodeGenRelease()
        {
            PlayerSettings.SetIl2CppCodeGeneration(NamedBuildTarget.Standalone, Il2CppCodeGeneration.OptimizeSpeed);
            AssetDatabase.SaveAssets();
            Debug.Log("[Best Practices] IL2CPP Code Generation set to OptimizeSpeed (Faster runtime).");
        }

        public static void ApplyScriptChanges()
        {
            var assets = AssetDatabase.LoadAllAssetsAtPath("ProjectSettings/EditorSettings.asset");
            if (assets.Length == 0) return;
            var so = new SerializedObject(assets[0]);
            so.UpdateIfRequiredOrScript();
            var prop = so.FindProperty("m_ScriptChangesDuringPlayback");
            if (prop != null)
            {
                prop.intValue = 1; // RecompileAfterFinishedPlaying
                so.ApplyModifiedProperties();
            }
            Debug.Log("[Best Practices] Script Changes While Playing set to Recompile After Finished Playing.");
        }

        public static void ApplyAutoRefreshOff()
        {
            EditorPrefs.SetInt("kAutoRefreshMode", 0);
            Debug.Log("[Best Practices] Auto Refresh disabled. Use Ctrl/Cmd+R to refresh manually.");
        }

        public static void ApplyAutoRefreshOn()
        {
            EditorPrefs.SetInt("kAutoRefreshMode", 1);
            Debug.Log("[Best Practices] Auto Refresh re-enabled.");
        }

        public static void ApplyBurstAsync()
        {
            var t = BurstOptionsType;
            if (t == null) return;
            var prop = t.GetProperty("ForceSynchronousBurstCompilations",
                BindingFlags.Public | BindingFlags.Static);
            prop?.SetValue(null, false);
            Debug.Log("[Best Practices] Burst Synchronous Compilation disabled (async mode — better editor responsiveness).");
        }

        public static void ApplyAsyncShadersOn()
        {
            EditorSettings.asyncShaderCompilation = true;
            Debug.Log("[Best Practices] Async Shader Compilation enabled — shader variants compile in the background.");
        }

        public static void ApplyAsyncShadersOff()
        {
            EditorSettings.asyncShaderCompilation = false;
            Debug.Log("[Best Practices] Async Shader Compilation disabled — deterministic shader compilation for release.");
        }

        public static void ApplyManagedStrippingDev()
        {
            PlayerSettings.SetManagedStrippingLevel(NamedBuildTarget.Standalone, ManagedStrippingLevel.Disabled);
            AssetDatabase.SaveAssets();
            Debug.Log("[Best Practices] Managed Stripping Level set to Disabled (Standalone) — faster builds, full symbols.");
        }

        public static void ApplyManagedStrippingRelease()
        {
            PlayerSettings.SetManagedStrippingLevel(NamedBuildTarget.Standalone, ManagedStrippingLevel.Minimal);
            AssetDatabase.SaveAssets();
            Debug.Log("[Best Practices] Managed Stripping Level set to Minimal (Standalone) — reduces IL for release builds.");
        }
    }
}
