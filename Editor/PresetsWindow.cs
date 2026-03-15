using UnityEditor;
using UnityEngine;

namespace Unity.BestPractices.Editor
{
    /// <summary>
    /// Dedicated panel for registering import presets in the Preset Manager one at a time.
    /// Each row shows whether the preset is already registered and offers an individual Run button.
    /// When a preset is already configured the row shows green "Configured" and an Open button
    /// that navigates directly to <b>Edit → Project Settings → Preset Manager</b>.
    /// Open via <b>Window → Best Practices → Configure Import Presets</b> or from the Project Dashboard.
    /// </summary>
    public class PresetsWindow : EditorWindow
    {
        private Vector2 m_ScrollPosition;

        public static void ShowWindow()
        {
            var window = GetWindow<PresetsWindow>("Import Presets");
            window.minSize = new Vector2(460, 520);
            window.Show();
        }

        private void OnGUI()
        {
            m_ScrollPosition = EditorGUILayout.BeginScrollView(m_ScrollPosition);

            DrawHeader();
            GUILayout.Space(8);
            DrawConfigureAllButton();
            GUILayout.Space(8);
            DrawPresets();

            EditorGUILayout.EndScrollView();
        }

        private void DrawHeader()
        {
            EditorGUILayout.BeginVertical(EditorStyles.helpBox);
            GUILayout.Label("IMPORT PRESETS", EditorStyles.boldLabel);
            GUILayout.Space(2);
            GUILayout.Label(
                "Register import presets in the Preset Manager individually or all at once. " +
                "Presets apply automatically when assets are imported into the matching folder path. " +
                "Green \"Configured\" means the preset is already registered.",
                EditorStyles.wordWrappedLabel);
            EditorGUILayout.EndVertical();
        }

        private void DrawConfigureAllButton()
        {
            EditorGUILayout.BeginVertical(EditorStyles.helpBox);
            GUILayout.Label("Register all presets below in one click.", EditorStyles.wordWrappedLabel);
            GUILayout.Space(4);
            if (GUILayout.Button("Configure All Presets", GUILayout.Height(28)))
            {
                ConfigurePresets.ApplyAllPresets();
                Repaint();
            }
            EditorGUILayout.EndVertical();
        }

        private void DrawPresets()
        {
            // ── Audio ─────────────────────────────────────────────────────────
            GUILayout.Label("AUDIO", EditorStyles.boldLabel);

            DrawPresetRow("Ambience",
                "Assets/_ProjectName/Art/Audio/Ambience/**",
                ConfigurePresets.IsAmbienceConfigured,
                ConfigurePresets.ApplyAmbience);

            DrawPresetRow("Music",
                "Assets/_ProjectName/Art/Audio/Music/**",
                ConfigurePresets.IsMusicConfigured,
                ConfigurePresets.ApplyMusic);

            DrawPresetRow("SFX",
                "Assets/_ProjectName/Art/Audio/SFX/**",
                ConfigurePresets.IsSFXConfigured,
                ConfigurePresets.ApplySFX);

            DrawPresetRow("UI Audio",
                "Assets/_ProjectName/Art/Audio/UI/**",
                ConfigurePresets.IsUIAudioConfigured,
                ConfigurePresets.ApplyUIAudio);

            GUILayout.Space(6);

            // ── Textures ──────────────────────────────────────────────────────
            GUILayout.Label("TEXTURES", EditorStyles.boldLabel);

            DrawPresetRow("UI Sprite",
                "Assets/_ProjectName/Art/Textures/UI/**",
                ConfigurePresets.IsUISpriteConfigured,
                ConfigurePresets.ApplyUISprite);

            DrawPresetRow("Albedo",
                "Assets/_ProjectName/Art/Textures/Environment/Albedo/**",
                ConfigurePresets.IsAlbedoConfigured,
                ConfigurePresets.ApplyAlbedo);

            DrawPresetRow("Normal Map",
                "Assets/_ProjectName/Art/Textures/Environment/Normal/**",
                ConfigurePresets.IsNormalConfigured,
                ConfigurePresets.ApplyNormal);

            DrawPresetRow("Mask",
                "Assets/_ProjectName/Art/Textures/Environment/Masks/**",
                ConfigurePresets.IsMaskConfigured,
                ConfigurePresets.ApplyMask);

            GUILayout.Space(6);

            // ── Models ────────────────────────────────────────────────────────
            GUILayout.Label("MODELS", EditorStyles.boldLabel);

            DrawPresetRow("FBX Model",
                "Assets/_ProjectName/Art/Models/**",
                ConfigurePresets.IsFBXModelConfigured,
                ConfigurePresets.ApplyFBXModel);

            GUILayout.Space(6);

            // ── Animations ────────────────────────────────────────────────────
            GUILayout.Label("ANIMATIONS", EditorStyles.boldLabel);

            DrawPresetRow("FBX Animation",
                "Assets/_ProjectName/Art/Animations/**",
                ConfigurePresets.IsFBXAnimationConfigured,
                ConfigurePresets.ApplyFBXAnimation);
        }

        private void DrawPresetRow(string title, string filterPath, bool isConfigured, System.Action applyAction)
        {
            EditorGUILayout.BeginVertical(EditorStyles.helpBox);
            EditorGUILayout.BeginHorizontal();

            // Status badge
            var prevColor = GUI.color;
            GUI.color = isConfigured ? new Color(0.3f, 0.8f, 0.3f) : new Color(0.7f, 0.7f, 0.7f);
            GUILayout.Label(isConfigured ? "[OK]" : "[  ]", GUILayout.Width(36));
            GUI.color = prevColor;

            // Title
            GUILayout.Label(title, EditorStyles.boldLabel);
            GUILayout.FlexibleSpace();

            if (isConfigured)
            {
                // Green "Configured" label
                prevColor = GUI.color;
                GUI.color = new Color(0.3f, 0.8f, 0.3f);
                GUILayout.Label("Configured", GUILayout.Width(74));
                GUI.color = prevColor;

                // Open button → Preset Manager settings page
                if (GUILayout.Button("Open", GUILayout.Width(44)))
                    SettingsService.OpenProjectSettings("Project/Preset Manager");
            }
            else
            {
                if (GUILayout.Button("Run", GUILayout.Width(44)))
                {
                    applyAction();
                    Repaint();
                }
            }

            EditorGUILayout.EndHorizontal();

            // Filter path hint
            GUILayout.Label($"Filter:  {filterPath}", EditorStyles.wordWrappedMiniLabel);

            EditorGUILayout.EndVertical();
            GUILayout.Space(2);
        }
    }
}
