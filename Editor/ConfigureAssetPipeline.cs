using UnityEditor;
using UnityEngine;

namespace Unity.BestPractices.Editor
{
    /// <summary>
    /// Manages Asset Pipeline preferences — auto refresh behaviour, import parallelism, and texture compression.
    /// Most of these are machine-local (EditorPrefs) and are not committed to VCS.
    /// </summary>
    public static class ConfigureAssetPipeline
    {
        // ── Auto Refresh ─────────────────────────────────────────────────────
        // kAutoRefreshMode values (EditorPrefs, machine-local):
        //   0 = Disabled
        //   1 = Enabled
        //   2 = Enabled Outside Playmode  ← recommended

        private const string k_AutoRefreshKey = "kAutoRefreshMode";

        public static int AutoRefreshMode => EditorPrefs.GetInt(k_AutoRefreshKey, 1);

        public static bool IsAutoRefreshRecommended => AutoRefreshMode == 2;
        public static bool IsAutoRefreshDisabled => AutoRefreshMode == 0;
        public static bool IsAutoRefreshAlwaysOn => AutoRefreshMode == 1;

        public static void ApplyAutoRefreshRecommended()
        {
            EditorPrefs.SetInt(k_AutoRefreshKey, 2);
            Debug.Log("[Best Practices] Auto Refresh set to 'Enabled Outside Playmode' — refreshes while editing, never during a playtest.");
        }

        public static void ApplyAutoRefreshOff()
        {
            EditorPrefs.SetInt(k_AutoRefreshKey, 0);
            Debug.Log("[Best Practices] Auto Refresh disabled. Trigger manually with Cmd+R / Ctrl+R.");
        }

        public static void ApplyAutoRefreshAlwaysOn()
        {
            EditorPrefs.SetInt(k_AutoRefreshKey, 1);
            Debug.Log("[Best Practices] Auto Refresh set to always enabled.");
        }

        // ── Import Worker Count % ─────────────────────────────────────────────
        // EditorPrefs key confirmed in Unity 6 source: "AssetPipelineImportWorkerCountPercentage"
        // Default: 25. Machine-local.

        private const string k_WorkerCountKey = "AssetPipelineImportWorkerCountPercentage";
        private const int k_RecommendedWorkerPct = 50;

        public static int ImportWorkerCountPct => EditorPrefs.GetInt(k_WorkerCountKey, 25);

        public static bool IsImportWorkerCountRecommended => ImportWorkerCountPct >= k_RecommendedWorkerPct;

        public static void ApplyImportWorkerCountRecommended()
        {
            EditorPrefs.SetInt(k_WorkerCountKey, k_RecommendedWorkerPct);
            Debug.Log($"[Best Practices] Import Worker Count set to {k_RecommendedWorkerPct}% — better parallel import throughput.");
        }

        // ── Compress Textures on Import ───────────────────────────────────────
        // Stored in EditorSettings.asset as m_CompressAssetsOnImport (bool 0/1).

        public static bool IsCompressTexturesConfigured
        {
            get
            {
                var assets = AssetDatabase.LoadAllAssetsAtPath("ProjectSettings/EditorSettings.asset");
                if (assets.Length == 0) return false;
                var so = new SerializedObject(assets[0]);
                var prop = so.FindProperty("m_CompressAssetsOnImport");
                // If the property doesn't exist it means it always compresses (default Unity behaviour)
                return prop == null || prop.boolValue;
            }
        }

        public static void ApplyCompressTextures()
        {
            var assets = AssetDatabase.LoadAllAssetsAtPath("ProjectSettings/EditorSettings.asset");
            if (assets.Length == 0) return;
            var so = new SerializedObject(assets[0]);
            so.UpdateIfRequiredOrScript();
            var prop = so.FindProperty("m_CompressAssetsOnImport");
            if (prop != null)
            {
                prop.boolValue = true;
                so.ApplyModifiedProperties();
            }
            Debug.Log("[Best Practices] Compress Textures on Import enabled.");
        }
    }
}
