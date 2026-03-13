using UnityEditor;
using UnityEditorInternal;
using UnityEngine;
using System.IO;
using Unity.BestPractices.Editor;
using UnityBestPractices.Editor.Validator;

namespace UnityBestPractices.Editor.Dashboard
{
    public class ProjectDashboardWindow : EditorWindow
    {
        private ProjectDashboardData m_data;
        private Vector2 m_scrollPosition;
        private bool m_showLLMFiles = false;
        private bool m_showSkills = false;
        private bool m_showTools = true;
        private bool m_showProjectSettings = true;
        private bool m_showIteration = true;
        private bool m_showAssetPipeline = true;
        private bool m_showWindows = true;
        private bool m_showPresets = false;

        [MenuItem("Tools/Unity Best Practices/Project Dashboard")]
        public static void ShowWindow()
        {
            var window = GetWindow<ProjectDashboardWindow>("Project Dashboard");
            window.minSize = new Vector2(400, 500);
            window.Show();
        }

        private void OnEnable()
        {
            RefreshData();
        }

        private void OnFocus()
        {
            RefreshData();
        }

        private void RefreshData()
        {
            m_data = ProjectDashboardData.Gather();
        }

        private void OnGUI()
        {
            if (m_data == null)
            {
                RefreshData();
                return;
            }

            m_scrollPosition = EditorGUILayout.BeginScrollView(m_scrollPosition);

            // Header
            DrawHeader();

            GUILayout.Space(10);

            // Windows
            DrawWindows();

            GUILayout.Space(10);

            // Tools quick-actions
            DrawTools();

            GUILayout.Space(10);

            // Project Settings
            DrawProjectSettings();

            GUILayout.Space(10);

            // Iteration Settings
            DrawIterationSettings();

            GUILayout.Space(10);

            // Asset Pipeline
            DrawAssetPipeline();

            GUILayout.Space(10);

            // LLM Instruction Files
            DrawLLMInstructions();

            GUILayout.Space(10);

            // Agent Skills
            DrawAgentSkills();

            EditorGUILayout.EndScrollView();
        }

        private void DrawHeader()
        {
            EditorGUILayout.BeginVertical(EditorStyles.helpBox);

            EditorGUILayout.BeginHorizontal();
            GUILayout.Label("PROJECT DASHBOARD", EditorStyles.boldLabel);
            GUILayout.FlexibleSpace();
            if (m_data.PackageVersion != null)
                GUILayout.Label($"v{m_data.PackageVersion}", EditorStyles.miniLabel);
            EditorGUILayout.EndHorizontal();

            GUILayout.Space(5);

            EditorGUILayout.BeginHorizontal();
            if (GUILayout.Button("Refresh", GUILayout.Width(100)))
                RefreshData();
            if (GUILayout.Button(new GUIContent("Domain Reload", "Triggers a full scripting domain reload — equivalent to saving any script. Useful to force a clean state after bulk setting changes."), GUILayout.Width(120)))
                EditorUtility.RequestScriptReload();
            if (GUILayout.Button("New Project Wizard", GUILayout.Width(160)))
                NewProjectWizard.ShowWindow();
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.EndVertical();
        }

        private void DrawStatusLine(string label, string value, bool isGood)
        {
            EditorGUILayout.BeginHorizontal();

            // Icon
            string icon = isGood ? "" : "�";
            Color iconColor = isGood ? new Color(0.3f, 0.8f, 0.3f) : new Color(0.9f, 0.7f, 0.2f);

            var originalColor = GUI.color;
            GUI.color = iconColor;
            GUILayout.Label(icon, GUILayout.Width(20));
            GUI.color = originalColor;

            // Label and Value
            GUILayout.Label(label + ":", GUILayout.Width(120));
            GUILayout.Label(value);

            EditorGUILayout.EndHorizontal();
        }

        private void DrawLLMInstructions()
        {
            EditorGUILayout.BeginVertical(EditorStyles.helpBox);

            string projectRoot = Path.GetDirectoryName(Application.dataPath);
            int llmTotal = m_data.LLMInstructionFilesCount;
            int llmInstalled = 0;
            if (m_data.LLMInstructionFiles != null)
            {
                foreach (var f in m_data.LLMInstructionFiles)
                {
                    if (!string.IsNullOrEmpty(f.FullPath) && File.Exists(CopyAIFilesToProject.GetLLMInstructionDestPath(f.FullPath, projectRoot))) llmInstalled++;
                }
            }
            string header = $"LLM INSTRUCTION FILES — {llmInstalled}/{llmTotal} installed";
            m_showLLMFiles = EditorGUILayout.Foldout(m_showLLMFiles, header, true, EditorStyles.foldoutHeader);
            GUILayout.Label("Markdown files that give AI assistants context about your project's coding standards and Unity conventions. Copied to .github/instructions/ so GitHub Copilot, Cursor, and other editors pick them up automatically.", EditorStyles.wordWrappedMiniLabel);

            if (m_showLLMFiles && m_data.LLMInstructionFiles != null)
            {
                GUILayout.Space(4);

                foreach (var file in m_data.LLMInstructionFiles)
                {
                    string fullPath = file.FullPath;
                    string destPath = CopyAIFilesToProject.GetLLMInstructionDestPath(fullPath, projectRoot);
                    bool installed = File.Exists(destPath);
                    bool outdated = installed && CopyAIFilesToProject.LLMInstructionIsOutdated(fullPath, projectRoot);

                    EditorGUILayout.BeginHorizontal();

                    var prevColor = GUI.color;
                    GUI.color = outdated ? new Color(0.9f, 0.75f, 0.2f)
                        : installed ? new Color(0.3f, 0.8f, 0.3f)
                        : new Color(0.7f, 0.7f, 0.7f);
                    GUILayout.Label(outdated ? "[Outdated]" : installed ? "[Configured]" : "[  ]",
                        GUILayout.Width(outdated ? 70 : installed ? 84 : 36));
                    GUI.color = prevColor;

                    GUILayout.Label(file.DisplayName, EditorStyles.label);
                    GUILayout.FlexibleSpace();

                    if (installed)
                    {
                        if (GUILayout.Button(new GUIContent("Open", "Open the local copy of this file"), GUILayout.Width(50)))
                            InternalEditorUtility.OpenFileAtLineExternal(destPath, 1, 0);

                        if (outdated && GUILayout.Button(new GUIContent("Update", "Overwrite the local copy with the latest version from the package"), GUILayout.Width(60)))
                            CopyAIFilesToProject.CopySingleLLMInstruction(fullPath, projectRoot);
                    }
                    else
                    {
                        if (GUILayout.Button(new GUIContent("View", "View this file from the package"), GUILayout.Width(50)))
                            InternalEditorUtility.OpenFileAtLineExternal(fullPath, 1, 0);

                        if (GUILayout.Button(new GUIContent("Add", "Copy this file to .github/instructions/ in your project"), GUILayout.Width(40)))
                            CopyAIFilesToProject.CopySingleLLMInstruction(fullPath, projectRoot);
                    }

                    EditorGUILayout.EndHorizontal();
                }

            }

            // ── AI Assistance ─────────────────────────────────────────────
            GUILayout.Label("Quick setup", EditorStyles.miniLabel);

            DrawToolCard(
                "Copy All LLM Instructions",
                "Copies all LLM instruction files to .github/instructions/ and .github/copilot-instructions.md so AI assistants can reference them locally.",
                false, "",
                "Copy to Project", CopyAIFilesToProject.ExecuteLLMInstructionsOnly, 110, showBadge: false);

            EditorGUILayout.EndVertical();
        }

        private void DrawAgentSkills()
        {
            EditorGUILayout.BeginVertical(EditorStyles.helpBox);

            string projectRoot = Path.GetDirectoryName(Application.dataPath);
            int skillTotal = m_data.AgentSkillFilesCount;
            int skillInstalled = 0;
            if (m_data.AgentSkillFiles != null)
            {
                foreach (var s in m_data.AgentSkillFiles)
                {
                    if (!string.IsNullOrEmpty(s.FullPath) && File.Exists(CopyAIFilesToProject.GetAgentSkillPromptDestPath(s.FullPath, projectRoot))) skillInstalled++;
                }
            }
            string header = $"AGENT SKILLS — {skillInstalled}/{skillTotal} installed";
            m_showSkills = EditorGUILayout.Foldout(m_showSkills, header, true, EditorStyles.foldoutHeader);
            GUILayout.Label("Reusable prompt templates that define specialized AI workflows for your project. Copied to .github/prompts/ (GitHub Copilot reusable prompts) and .claude/commands/ (Claude Code slash commands).", EditorStyles.wordWrappedMiniLabel);

            if (m_showSkills && m_data.AgentSkillFiles != null)
            {
                GUILayout.Space(4);

                foreach (var skill in m_data.AgentSkillFiles)
                {
                    string fullPath = skill.FullPath;
                    string destPath = CopyAIFilesToProject.GetAgentSkillPromptDestPath(fullPath, projectRoot);
                    bool installed = File.Exists(destPath);
                    bool outdated = installed && CopyAIFilesToProject.AgentSkillIsOutdated(fullPath, projectRoot);

                    EditorGUILayout.BeginHorizontal();

                    var prevColor = GUI.color;
                    GUI.color = outdated ? new Color(0.9f, 0.75f, 0.2f)
                        : installed ? new Color(0.3f, 0.8f, 0.3f)
                        : new Color(0.7f, 0.7f, 0.7f);
                    GUILayout.Label(outdated ? "[Outdated]" : installed ? "[Configured]" : "[  ]",
                        GUILayout.Width(outdated ? 70 : installed ? 84 : 36));
                    GUI.color = prevColor;

                    GUILayout.Label(skill.DisplayName, EditorStyles.label);
                    GUILayout.FlexibleSpace();

                    if (installed)
                    {
                        if (GUILayout.Button(new GUIContent("Open", "Open the local copy of this skill"), GUILayout.Width(50)))
                            InternalEditorUtility.OpenFileAtLineExternal(destPath, 1, 0);

                        if (outdated && GUILayout.Button(new GUIContent("Update", "Overwrite the local copy with the latest version from the package"), GUILayout.Width(60)))
                            CopyAIFilesToProject.CopySingleAgentSkill(fullPath, projectRoot);
                    }
                    else
                    {
                        if (GUILayout.Button(new GUIContent("View", "View this skill file from the package"), GUILayout.Width(50)))
                            InternalEditorUtility.OpenFileAtLineExternal(fullPath, 1, 0);

                        if (GUILayout.Button(new GUIContent("Add", "Copy to .github/prompts/ (Copilot) and .claude/commands/ (Claude Code)"), GUILayout.Width(40)))
                            CopyAIFilesToProject.CopySingleAgentSkill(fullPath, projectRoot);
                    }

                    EditorGUILayout.EndHorizontal();
                }

            }

            // ── AI Assistance ─────────────────────────────────────────────
            GUILayout.Label("Quick setup", EditorStyles.miniLabel);

            DrawToolCard(
                "Copy All Agent Skills",
                "Copies all agent skill files to .github/prompts/ (Copilot reusable prompts) and .claude/commands/ (Claude Code slash commands).",
                false, "",
                "Copy to Project", CopyAIFilesToProject.ExecuteAgentSkillsOnly, 110, showBadge: false);


            EditorGUILayout.EndVertical();
        }

        private void DrawIterationSettings()
        {
            EditorGUILayout.BeginVertical(EditorStyles.helpBox);

            // Compute profile state for header label
            var profile = ConfigureIterationSettings.CurrentProfile;
            string profileLabel = profile switch
            {
                ConfigureIterationSettings.ProfileState.Dev => "Dev Profile",
                ConfigureIterationSettings.ProfileState.Release => "Release Profile",
                ConfigureIterationSettings.ProfileState.Mixed => "Mixed",
                _ => "Unknown"
            };
            m_showIteration = EditorGUILayout.Foldout(m_showIteration,
                $"ITERATION SETTINGS — {profileLabel}", true, EditorStyles.foldoutHeader);

            if (m_showIteration)
            {
                GUILayout.Space(4);

                // Info banner
                var prevColor = GUI.color;
                GUI.color = new Color(1f, 0.92f, 0.6f);
                EditorGUILayout.BeginVertical(EditorStyles.helpBox);
                GUI.color = prevColor;
                GUILayout.Label(
                    "These settings are tuned for day-to-day development speed — switch to Release Profile before profiling or shipping.",
                    EditorStyles.wordWrappedMiniLabel);
                EditorGUILayout.EndVertical();

                GUILayout.Space(6);

                // Profile buttons
                EditorGUILayout.BeginHorizontal();
                bool isDev = profile == ConfigureIterationSettings.ProfileState.Dev;
                bool isRelease = profile == ConfigureIterationSettings.ProfileState.Release;

                using (new EditorGUI.DisabledScope(isDev))
                {
                    if (GUILayout.Button(isDev ? "✓ Dev Profile" : "Apply Dev Profile", GUILayout.Height(26)))
                    {
                        ConfigureIterationSettings.ApplyDevFastProfile();
                        Repaint();
                    }
                }
                using (new EditorGUI.DisabledScope(isRelease))
                {
                    if (GUILayout.Button(isRelease ? "✓ Release Profile" : "Apply Release Profile", GUILayout.Height(26)))
                    {
                        ConfigureIterationSettings.ApplyReleasePerfProfile();
                        Repaint();
                    }
                }
                EditorGUILayout.EndHorizontal();

                GUILayout.Space(6);

                bool isMono = ConfigureIterationSettings.IsBackendDev;

                // 1. Scripting Backend
                DrawIterationCard(
                    "Scripting Backend (Standalone)",
                    "Mono: fastest compile + build loop for daily iteration.\nIL2CPP: required for release perf testing and final builds.",
                    ConfigureIterationSettings.IsBackendDev,
                    ConfigureIterationSettings.IsBackendRelease,
                    ConfigureIterationSettings.ApplyBackendDev,
                    ConfigureIterationSettings.ApplyBackendRelease);

                // 2. IL2CPP Code Generation (N/A when Mono is active)
                using (new EditorGUI.DisabledScope(isMono))
                {
                    DrawIterationCard(
                        "IL2CPP Code Generation" + (isMono ? " [N/A — Mono active]" : ""),
                        "Faster builds (OptimizeSize): shorter build times during dev.\nFaster runtime (OptimizeSpeed): maximises runtime performance for release.",
                        ConfigureIterationSettings.IsIL2CppCodeGenDev,
                        ConfigureIterationSettings.IsIL2CppCodeGenRelease,
                        ConfigureIterationSettings.ApplyIL2CppCodeGenDev,
                        ConfigureIterationSettings.ApplyIL2CppCodeGenRelease);
                }

                // 3. Script Changes While Playing (single recommended value — no Dev/Release split)
                {
                    bool set = ConfigureIterationSettings.IsScriptChangesConfigured;
                    EditorGUILayout.BeginVertical(EditorStyles.helpBox);
                    EditorGUILayout.BeginHorizontal();
                    var c = GUI.color;
                    GUI.color = set ? new Color(0.3f, 0.8f, 0.3f) : new Color(0.7f, 0.7f, 0.7f);
                    GUILayout.Label(set ? "[Set]" : "[—]", GUILayout.Width(set ? 38 : 30));
                    GUI.color = c;
                    GUILayout.Label("Script Changes While Playing", EditorStyles.boldLabel);
                    GUILayout.FlexibleSpace();
                    using (new EditorGUI.DisabledScope(set))
                    {
                        if (GUILayout.Button("Apply", GUILayout.Width(50)))
                        {
                            ConfigureIterationSettings.ApplyScriptChanges();
                            Repaint();
                        }
                    }
                    EditorGUILayout.EndHorizontal();
                    GUILayout.Label("Recompile After Finished Playing — Unity queues the recompile until you exit Play mode, keeping your session intact. Prevents mid-session domain reloads that corrupt DI container graphs (e.g. VContainer) and cause hard-to-reproduce bugs.", EditorStyles.wordWrappedMiniLabel);
                    EditorGUILayout.EndVertical();
                    GUILayout.Space(2);
                }

                // 4. Async Shader Compilation
                DrawIterationCard(
                    "Async Shader Compilation",
                    "On (Dev): shader variants compile in the background — no hitches while iterating.\nOff (Release): fully deterministic compilation, no pink placeholder shaders in builds.",
                    ConfigureIterationSettings.IsAsyncShadersDev,
                    ConfigureIterationSettings.IsAsyncShadersRelease,
                    ConfigureIterationSettings.ApplyAsyncShadersOn,
                    ConfigureIterationSettings.ApplyAsyncShadersOff);

                // 5. Managed Stripping Level
                DrawIterationCard(
                    "Managed Stripping Level (Standalone)",
                    "Disabled (Dev): faster builds, full symbols, easier debugging — nothing is stripped.\nMinimal (Release): safely removes unused IL, reducing build size without breaking reflection.",
                    ConfigureIterationSettings.IsManagedStrippingDev,
                    ConfigureIterationSettings.IsManagedStrippingRelease,
                    ConfigureIterationSettings.ApplyManagedStrippingDev,
                    ConfigureIterationSettings.ApplyManagedStrippingRelease);

                // 6. Burst Async (only shown when Burst package is installed)
                if (ConfigureIterationSettings.IsBurstInstalled)
                {
                    bool burstOk = ConfigureIterationSettings.IsBurstAsyncConfigured;
                    EditorGUILayout.BeginVertical(EditorStyles.helpBox);
                    EditorGUILayout.BeginHorizontal();
                    var c = GUI.color;
                    GUI.color = burstOk ? new Color(0.3f, 0.8f, 0.3f) : new Color(0.7f, 0.7f, 0.7f);
                    GUILayout.Label(burstOk ? "[Dev]" : "[—]", GUILayout.Width(40));
                    GUI.color = c;
                    GUILayout.Label("Burst: Async Compilation", EditorStyles.boldLabel);
                    GUILayout.FlexibleSpace();
                    using (new EditorGUI.DisabledScope(burstOk))
                    {
                        if (GUILayout.Button("Apply", GUILayout.Width(50)))
                        {
                            ConfigureIterationSettings.ApplyBurstAsync();
                            Repaint();
                        }
                    }
                    EditorGUILayout.EndHorizontal();
                    GUILayout.Label("Disables synchronous Burst compilation — improves editor startup and responsiveness during iteration.", EditorStyles.wordWrappedMiniLabel);
                    EditorGUILayout.EndVertical();
                    GUILayout.Space(2);
                }
            }

            EditorGUILayout.EndVertical();
        }

        private void DrawIterationCard(
            string title, string description,
            bool isDevMode, bool isReleaseMode,
            System.Action applyDev, System.Action applyRelease)
        {
            EditorGUILayout.BeginVertical(EditorStyles.helpBox);
            EditorGUILayout.BeginHorizontal();

            var prevColor = GUI.color;
            if (isDevMode)
                GUI.color = new Color(0.3f, 0.8f, 0.3f);
            else if (isReleaseMode)
                GUI.color = new Color(0.4f, 0.7f, 1f);
            else
                GUI.color = new Color(0.7f, 0.7f, 0.7f);

            string badge = isDevMode ? "[Dev]" : isReleaseMode ? "[Release]" : "[—]";
            int badgeWidth = isReleaseMode ? 62 : 40;
            GUILayout.Label(badge, GUILayout.Width(badgeWidth));
            GUI.color = prevColor;

            GUILayout.Label(title, EditorStyles.boldLabel);
            GUILayout.FlexibleSpace();

            using (new EditorGUI.DisabledScope(isDevMode))
            {
                if (GUILayout.Button("Dev", GUILayout.Width(40)))
                {
                    applyDev?.Invoke();
                    Repaint();
                }
            }
            using (new EditorGUI.DisabledScope(isReleaseMode))
            {
                if (GUILayout.Button("Release", GUILayout.Width(56)))
                {
                    applyRelease?.Invoke();
                    Repaint();
                }
            }

            EditorGUILayout.EndHorizontal();
            GUILayout.Label(description, EditorStyles.wordWrappedMiniLabel);
            EditorGUILayout.EndVertical();
            GUILayout.Space(2);
        }

        private void DrawAssetPipeline()
        {
            EditorGUILayout.BeginVertical(EditorStyles.helpBox);

            m_showAssetPipeline = EditorGUILayout.Foldout(m_showAssetPipeline,
                "ASSET PIPELINE", true, EditorStyles.foldoutHeader);

            if (m_showAssetPipeline)
            {
                GUILayout.Space(4);

                // Info banner
                var prevColor = GUI.color;
                GUI.color = new Color(1f, 0.92f, 0.6f);
                EditorGUILayout.BeginVertical(EditorStyles.helpBox);
                GUI.color = prevColor;
                GUILayout.Label(
                    "These preferences are machine-local (EditorPrefs) and are not committed to version control.",
                    EditorStyles.wordWrappedMiniLabel);
                EditorGUILayout.EndVertical();

                GUILayout.Space(6);

                // 1. Auto Refresh — 3-way picker
                {
                    bool isRecommended = ConfigureAssetPipeline.IsAutoRefreshRecommended;  // mode == 2
                    bool isOff = ConfigureAssetPipeline.IsAutoRefreshDisabled;      // mode == 0
                    bool isAlwaysOn = ConfigureAssetPipeline.IsAutoRefreshAlwaysOn;      // mode == 1

                    EditorGUILayout.BeginVertical(EditorStyles.helpBox);
                    EditorGUILayout.BeginHorizontal();
                    var c = GUI.color;
                    GUI.color = isRecommended ? new Color(0.3f, 0.8f, 0.3f)
                              : isOff ? new Color(0.7f, 0.7f, 0.7f)
                              : new Color(0.4f, 0.7f, 1f);
                    GUILayout.Label(isRecommended ? "[Set]" : "[—]", GUILayout.Width(isRecommended ? 38 : 30));
                    GUI.color = c;
                    GUILayout.Label("Auto Refresh", EditorStyles.boldLabel);
                    GUILayout.FlexibleSpace();

                    using (new EditorGUI.DisabledScope(isOff))
                    {
                        if (GUILayout.Button("Disabled", GUILayout.Width(65)))
                        {
                            ConfigureAssetPipeline.ApplyAutoRefreshOff();
                            Repaint();
                        }
                    }
                    using (new EditorGUI.DisabledScope(isRecommended))
                    {
                        var orig = GUI.backgroundColor;
                        if (!isRecommended) GUI.backgroundColor = new Color(0.3f, 0.9f, 0.3f);
                        if (GUILayout.Button("Outside Playmode ★", GUILayout.Width(148)))
                        {
                            ConfigureAssetPipeline.ApplyAutoRefreshRecommended();
                            Repaint();
                        }
                        GUI.backgroundColor = orig;
                    }
                    using (new EditorGUI.DisabledScope(isAlwaysOn))
                    {
                        if (GUILayout.Button("Always On", GUILayout.Width(72)))
                        {
                            ConfigureAssetPipeline.ApplyAutoRefreshAlwaysOn();
                            Repaint();
                        }
                    }

                    EditorGUILayout.EndHorizontal();
                    GUILayout.Label(
                        "Outside Playmode (recommended): Unity only refreshes assets when not in Play mode — prevents surprise reimports breaking your Play session.\nDisabled: manual refresh only (Cmd+R). Always On: default Unity behaviour.",
                        EditorStyles.wordWrappedMiniLabel);
                    EditorGUILayout.EndVertical();
                    GUILayout.Space(2);
                }

                // 2. Import Worker Count
                {
                    int pct = ConfigureAssetPipeline.ImportWorkerCountPct;
                    bool isRecommended = ConfigureAssetPipeline.IsImportWorkerCountRecommended;

                    EditorGUILayout.BeginVertical(EditorStyles.helpBox);
                    EditorGUILayout.BeginHorizontal();
                    var c = GUI.color;
                    GUI.color = isRecommended ? new Color(0.3f, 0.8f, 0.3f) : new Color(0.7f, 0.7f, 0.7f);
                    GUILayout.Label(isRecommended ? "[Set]" : "[—]", GUILayout.Width(isRecommended ? 38 : 30));
                    GUI.color = c;
                    GUILayout.Label($"Import Worker Count — {pct}%", EditorStyles.boldLabel);
                    GUILayout.FlexibleSpace();
                    using (new EditorGUI.DisabledScope(isRecommended))
                    {
                        if (GUILayout.Button("Recommended (50%)", GUILayout.Width(140)))
                        {
                            ConfigureAssetPipeline.ApplyImportWorkerCountRecommended();
                            Repaint();
                        }
                    }
                    EditorGUILayout.EndHorizontal();
                    GUILayout.Label(
                        "Parallel asset import workers as a percentage of logical CPU cores. 50% balances import speed with editor responsiveness. Default is 25%.",
                        EditorStyles.wordWrappedMiniLabel);
                    EditorGUILayout.EndVertical();
                    GUILayout.Space(2);
                }

                // 3. Compress Textures on Import
                {
                    bool ok = ConfigureAssetPipeline.IsCompressTexturesConfigured;

                    EditorGUILayout.BeginVertical(EditorStyles.helpBox);
                    EditorGUILayout.BeginHorizontal();
                    var c = GUI.color;
                    GUI.color = ok ? new Color(0.3f, 0.8f, 0.3f) : new Color(0.7f, 0.7f, 0.7f);
                    GUILayout.Label(ok ? "[Set]" : "[—]", GUILayout.Width(ok ? 38 : 30));
                    GUI.color = c;
                    GUILayout.Label("Compress Textures on Import", EditorStyles.boldLabel);
                    GUILayout.FlexibleSpace();
                    using (new EditorGUI.DisabledScope(ok))
                    {
                        if (GUILayout.Button("Enable", GUILayout.Width(58)))
                        {
                            ConfigureAssetPipeline.ApplyCompressTextures();
                            Repaint();
                        }
                    }
                    EditorGUILayout.EndHorizontal();
                    GUILayout.Label(
                        "Ensures texture compression runs at import time, reducing GPU memory and build size. Should be enabled in all projects.",
                        EditorStyles.wordWrappedMiniLabel);
                    EditorGUILayout.EndVertical();
                    GUILayout.Space(2);
                }
            }

            EditorGUILayout.EndVertical();
        }

        private void DrawProjectSettings()
        {
            EditorGUILayout.BeginVertical(EditorStyles.helpBox);

            int settingsOk = (ConfigureProjectSettings.IsEnterPlayModeConfigured ? 1 : 0)
                + (ConfigureProjectSettings.IsIL2CPPConfigured ? 1 : 0)
                + (ConfigureProjectSettings.IsApiCompatibilityConfigured ? 1 : 0)
                + (ConfigureProjectSettings.IsAssetSerializationConfigured ? 1 : 0)
                + (ConfigureProjectSettings.IsVersionControlConfigured ? 1 : 0)
                + (ConfigureProjectSettings.IsInputSystemConfigured ? 1 : 0)
                + (ConfigureProjectSettings.IsIncrementalGCConfigured ? 1 : 0)
                + (ConfigureProjectSettings.IsCreateObjectsAtOriginConfigured ? 1 : 0)
                + (ConfigureProjectSettings.IsNewHierarchyWindowConfigured ? 1 : 0)
                + (ConfigureProjectSettings.IsAssetManagerImportLocationConfigured ? 1 : 0);
            m_showProjectSettings = EditorGUILayout.Foldout(m_showProjectSettings, $"PROJECT SETTINGS — {settingsOk}/10 configured", true, EditorStyles.foldoutHeader);

            if (m_showProjectSettings)
            {
                GUILayout.Space(4);

                bool allOk = settingsOk == 10;

                using (new EditorGUI.DisabledScope(allOk))
                {
                    if (GUILayout.Button("Apply All Settings", GUILayout.Height(24)))
                    {
                        ConfigureProjectSettings.ApplySettings();
                        RefreshData();
                    }
                }

                GUILayout.Space(4);

                DrawSettingCard(
                    "Enter Play Mode",
                    "Enabled: Unity skips domain and scene reload on entering Play mode — much faster iteration. Requires static state to be reset manually via [RuntimeInitializeOnLoadMethod].\nDisabled: Full domain reload runs on every Play mode entry — slower but requires no extra cleanup code.",
                    ConfigureProjectSettings.IsEnterPlayModeConfigured,
                    ConfigureProjectSettings.ApplyEnterPlayMode,
                    openSettingsPath: "Project/Editor",
                    disableAction: ConfigureProjectSettings.DisableEnterPlayMode);

                DrawSettingCard(
                    "Scripting Backend: IL2CPP",
                    "IL2CPP: best runtime performance and required for iOS. Enables full AOT compilation and better stripping.\nMono (default): faster editor build times but lower runtime performance — use Mono for fast iteration via Iteration Settings.",
                    ConfigureProjectSettings.IsIL2CPPConfigured,
                    ConfigureProjectSettings.ApplyIL2CPP,
                    "Apply",
                    openSettingsPath: "Project/Player");

                DrawSettingCard(
                    "API Compatibility: .NET Standard 2.1",
                    ".NET Standard 2.1: broadest library compatibility with NuGet packages and aligns with modern .NET practices.\n.NET Framework (default): gives access to Unity's full internal API surface, but can cause compatibility issues with external libraries.",
                    ConfigureProjectSettings.IsApiCompatibilityConfigured,
                    ConfigureProjectSettings.ApplyApiCompatibility,
                    "Apply",
                    openSettingsPath: "Project/Player");

                DrawSettingCard(
                    "Asset Serialization: Force Text",
                    "Force Text: all assets serialize as readable YAML — diffs are meaningful and merges are possible in version control.\nBinary or Mixed (default): assets may be stored as binary blobs, making diff and merge in source control impractical.",
                    ConfigureProjectSettings.IsAssetSerializationConfigured,
                    ConfigureProjectSettings.ApplyAssetSerialization,
                    "Apply",
                    openSettingsPath: "Project/Editor");

                DrawSettingCard(
                    "Version Control: Visible Meta Files",
                    "Visible Meta Files: .meta files are written to disk so source control tracks them — prevents GUID regeneration that breaks asset references.\nHidden Meta Files (default): .meta files are hidden; source control will miss them unless configured manually.",
                    ConfigureProjectSettings.IsVersionControlConfigured,
                    ConfigureProjectSettings.ApplyVersionControl,
                    "Apply",
                    openSettingsPath: "Project/Editor");

                DrawSettingCard(
                    "Input System: New Input System Package",
                    "New Input System: event-driven, multi-device, cross-platform input with rebinding support. Required for modern input workflows.\nLegacy Input Manager (default): simple but limited — no controller rebinding, no multi-player input routing. Requires com.unity.inputsystem to be installed; a Unity restart may be needed.",
                    ConfigureProjectSettings.IsInputSystemConfigured,
                    ConfigureProjectSettings.ApplyInputSystem,
                    "Apply",
                    openSettingsPath: "Project/Player");

                DrawSettingCard(
                    "Incremental GC",
                    "Enabled: GC work is spread across multiple frames, eliminating stop-the-world spikes during gameplay.\nDisabled: full stop-the-world GC passes run as needed — may cause visible frame drops in GC-heavy scenes.",
                    ConfigureProjectSettings.IsIncrementalGCConfigured,
                    ConfigureProjectSettings.ApplyIncrementalGC,
                    openSettingsPath: "Project/Player",
                    disableAction: ConfigureProjectSettings.DisableIncrementalGC);

                DrawSettingCard(
                    "Scene View: Create Objects at Origin",
                    "Enabled: new GameObjects always spawn at world origin (0,0,0) — predictable placement regardless of camera position.\nDisabled: new GameObjects spawn in front of the Scene View camera — convenient but inconsistent across machines.",
                    ConfigureProjectSettings.IsCreateObjectsAtOriginConfigured,
                    ConfigureProjectSettings.ApplyCreateObjectsAtOrigin,
                    openSettingsPath: "Preferences/Scene View",
                    disableAction: ConfigureProjectSettings.DisableCreateObjectsAtOrigin);

                DrawSettingCard(
                    "Hierarchy: Use New Hierarchy Window",
                    "Enabled: redesigned Hierarchy with better rendering performance for large scenes and the Query Builder for filtering by component type.\nDisabled: legacy Hierarchy window — no Query Builder, slower with many objects.",
                    ConfigureProjectSettings.IsNewHierarchyWindowConfigured,
                    ConfigureProjectSettings.ApplyNewHierarchyWindow,
                    openSettingsPath: "Preferences/Hierarchy",
                    disableAction: ConfigureProjectSettings.DisableNewHierarchyWindow);

                DrawSettingCard(
                    "Asset Manager: Import to ThirdPartyAssets",
                    "Configured: Asset Manager imports to Assets/ThirdPartyAssets with subfolder creation — keeps third-party assets isolated from your own project folders.\nReset: imports go to Assets/ root — third-party packages mixed with project files, harder to audit and update.",
                    ConfigureProjectSettings.IsAssetManagerImportLocationConfigured,
                    ConfigureProjectSettings.ApplyAssetManagerImportLocation,
                    openSettingsPath: "Preferences/Asset Manager",
                    disableAction: ConfigureProjectSettings.ResetAssetManagerImportLocation,
                    disableLabel: "Reset");
            }

            EditorGUILayout.EndVertical();
        }

        private void DrawSettingCard(string title, string description, bool isConfigured, System.Action applyAction, string buttonLabel = "Enable", string openSettingsPath = "Project/Player", System.Action disableAction = null, string disableLabel = "Disable")
        {
            EditorGUILayout.BeginVertical(EditorStyles.helpBox);
            EditorGUILayout.BeginHorizontal();

            var prevColor = GUI.color;
            GUI.color = isConfigured ? new Color(0.3f, 0.8f, 0.3f) : new Color(0.7f, 0.7f, 0.7f);
            GUILayout.Label(isConfigured ? "[Configured]" : "[  ]", GUILayout.Width(isConfigured ? 84 : 36));
            GUI.color = prevColor;

            GUILayout.Label(title, EditorStyles.boldLabel);
            GUILayout.FlexibleSpace();

            if (isConfigured)
            {
                if (GUILayout.Button("Settings", GUILayout.Width(58)))
                {
                    if (openSettingsPath.StartsWith("Preferences/"))
                        SettingsService.OpenUserPreferences(openSettingsPath);
                    else
                        SettingsService.OpenProjectSettings(openSettingsPath);
                }
                if (disableAction != null)
                {
                    var dc = GUI.color;
                    GUI.color = new Color(0.9f, 0.5f, 0.5f);
                    if (GUILayout.Button(disableLabel, GUILayout.Width(52)))
                    {
                        disableAction.Invoke();
                        Repaint();
                    }
                    GUI.color = dc;
                }
            }
            else
            {
                if (GUILayout.Button(buttonLabel, GUILayout.Width(44)))
                {
                    applyAction?.Invoke();
                    Repaint();
                }
            }

            EditorGUILayout.EndHorizontal();
            GUILayout.Label(description, EditorStyles.wordWrappedMiniLabel);
            EditorGUILayout.EndVertical();
            GUILayout.Space(2);
        }

        private void DrawTools()
        {
            EditorGUILayout.BeginVertical(EditorStyles.helpBox);

            string projectRoot = Path.GetDirectoryName(Application.dataPath);
            string gitignorePath = Path.Combine(projectRoot, ".gitignore");
            string gitattributesPath = Path.Combine(projectRoot, ".gitattributes");
            string editorconfigPath = Path.Combine(projectRoot, ".editorconfig");
            bool gitignoreExists = File.Exists(gitignorePath);
            bool gitattributesExists = File.Exists(gitattributesPath);
            bool editorconfigExists = File.Exists(editorconfigPath);
            bool foldersOk = m_data.ExistingFoldersCount >= m_data.TotalRecommendedFolders * 0.7f;
            int presetCount = (ConfigurePresets.IsAmbienceConfigured ? 1 : 0)
                + (ConfigurePresets.IsMusicConfigured ? 1 : 0)
                + (ConfigurePresets.IsSFXConfigured ? 1 : 0)
                + (ConfigurePresets.IsUIAudioConfigured ? 1 : 0)
                + (ConfigurePresets.IsSingleSpriteConfigured ? 1 : 0)
                + (ConfigurePresets.IsSpriteAtlasConfigured ? 1 : 0)
                + (ConfigurePresets.IsAlbedoConfigured ? 1 : 0)
                + (ConfigurePresets.IsNormalConfigured ? 1 : 0)
                + (ConfigurePresets.IsRoughnessConfigured ? 1 : 0)
                + (ConfigurePresets.IsMaskConfigured ? 1 : 0)
                + (ConfigurePresets.IsHDRIConfigured ? 1 : 0)
                + (ConfigurePresets.IsFBXModelConfigured ? 1 : 0)
                + (ConfigurePresets.IsFBXAnimationConfigured ? 1 : 0);
            int setupDone = (foldersOk ? 1 : 0) + (gitignoreExists ? 1 : 0) + (gitattributesExists ? 1 : 0) + (editorconfigExists ? 1 : 0) + (presetCount == 13 ? 1 : 0);

            m_showTools = EditorGUILayout.Foldout(m_showTools, $"PROJECT CONFIGURATION — {setupDone}/5 done", true, EditorStyles.foldoutHeader);

            if (m_showTools)
            {
                GUILayout.Space(4);

                // ── Setup Actions ─────────────────────────────────────────────
                GUILayout.Label("Setup Actions", EditorStyles.miniLabel);

                DrawToolCard(
                    "Setup Project Folders",
                    "Creates the recommended folder structure under Assets/_ProjectName (Art, Audio, Prefabs, Scripts, Scenes, Settings, UI).",
                    foldersOk,
                    foldersOk ? $"{m_data.ExistingFoldersCount}/{m_data.TotalRecommendedFolders} folders present" : $"{m_data.ExistingFoldersCount}/{m_data.TotalRecommendedFolders} folders — run to create missing ones",
                    "Run", () => { SetupProjectFolders.Execute(); RefreshData(); },
                    helpAction: () => ExplainerWindow.Show(
                        "Setup Project Folders",
                        new[]
                        {
                            "Creates a standardised folder hierarchy under Assets/_ProjectName. " +
                            "Having a consistent structure makes it easier to navigate and maintain " +
                            "the project across a team and aligns with Unity's recommended layout.",
                            "If some folders already exist they are left untouched. Only the " +
                            "missing ones are created, so it is safe to run on an existing project."
                        },
                        preview:
                            "Assets/\n" +
                            "└── _ProjectName/\n" +
                            "    ├── Art/\n" +
                            "    │   ├── Audio/\n" +
                            "    │   │   ├── Ambience/\n" +
                            "    │   │   ├── Music/\n" +
                            "    │   │   ├── SFX/\n" +
                            "    │   │   └── UI/\n" +
                            "    │   ├── Animations/\n" +
                            "    │   ├── Models/\n" +
                            "    │   ├── Textures/\n" +
                            "    │   │   ├── Albedo/\n" +
                            "    │   │   ├── HDRI/\n" +
                            "    │   │   ├── Mask/\n" +
                            "    │   │   ├── Normal/\n" +
                            "    │   │   └── Roughness/\n" +
                            "    │   └── VFX/\n" +
                            "    ├── Prefabs/\n" +
                            "    ├── Scenes/\n" +
                            "    ├── Scripts/\n" +
                            "    ├── Settings/\n" +
                            "    └── UI/\n" +
                            "        └── Sprites/\n" +
                            "            └── Atlas/",
                        runLabel: "Run Now",
                        runAction: () => { SetupProjectFolders.Execute(); RefreshData(); }));

                DrawFileToolCard(
                    "Generate .gitignore",
                    "Creates a Unity-optimised .gitignore at the project root, excluding Library, Temp, build outputs and IDE files.",
                    gitignorePath, GenerateGitIgnore.Execute);

                DrawFileToolCard(
                    "Generate .gitattributes",
                    "Creates a .gitattributes at the project root that sets LF line endings for text files, configures UnityYAMLMerge for Unity YAML assets, and marks binary assets so Git never attempts text diffs on them.",
                    gitattributesPath, GenerateGitAttributes.Execute);

                DrawFileToolCard(
                    "Generate .editorconfig",
                    "Creates an .editorconfig enforcing C# naming and formatting conventions (Allman braces, 4-space indent, m_ prefix rules).",
                    editorconfigPath, GenerateEditorConfig.Execute);

                DrawToolCard(
                    "Assembly Definitions",
                    "Manage and generate .asmdef files. Opens the Assembly Definitions panel where you can inspect all existing .asmdef files and generate the recommended set.",
                    false, "",
                    "Open", AssemblyDefinitionsWindow.ShowWindow);

                // ── Import Presets ────────────────────────────────────────────
                GUILayout.Space(4);
                GUILayout.Label("Import Presets", EditorStyles.miniLabel);

                EditorGUILayout.BeginVertical(EditorStyles.helpBox);

                bool allPresetsOk = presetCount == 13;

                EditorGUILayout.BeginHorizontal();
                m_showPresets = EditorGUILayout.Foldout(m_showPresets, $"Configure Import Presets — {presetCount}/13 registered", true, EditorStyles.foldoutHeader);
                GUILayout.FlexibleSpace();
                using (new EditorGUI.DisabledScope(allPresetsOk))
                {
                    if (GUILayout.Button("Apply All", GUILayout.Width(70)))
                    {
                        ConfigurePresets.ApplyAllPresets();
                        Repaint();
                    }
                }
                EditorGUILayout.EndHorizontal();

                if (m_showPresets)
                {
                    GUILayout.Label("AUDIO", EditorStyles.centeredGreyMiniLabel);
                    DrawPresetCard("Ambience", "glob: Assets/_ProjectName/Art/Audio/Ambience/**", ConfigurePresets.IsAmbienceConfigured, ConfigurePresets.ApplyAmbience);
                    DrawPresetCard("Music", "glob: Assets/_ProjectName/Art/Audio/Music/**", ConfigurePresets.IsMusicConfigured, ConfigurePresets.ApplyMusic);
                    DrawPresetCard("SFX", "glob: Assets/_ProjectName/Art/Audio/SFX/**", ConfigurePresets.IsSFXConfigured, ConfigurePresets.ApplySFX);
                    DrawPresetCard("UI Audio", "glob: Assets/_ProjectName/Art/Audio/UI/**", ConfigurePresets.IsUIAudioConfigured, ConfigurePresets.ApplyUIAudio);

                    GUILayout.Space(2);
                    GUILayout.Label("TEXTURES", EditorStyles.centeredGreyMiniLabel);
                    DrawPresetCard("Single Sprite", "glob: Assets/_ProjectName/UI/Sprites/**", ConfigurePresets.IsSingleSpriteConfigured, ConfigurePresets.ApplySingleSprite);
                    DrawPresetCard("Sprite Atlas", "glob: Assets/_ProjectName/UI/Sprites/Atlas/**", ConfigurePresets.IsSpriteAtlasConfigured, ConfigurePresets.ApplySpriteAtlas);
                    DrawPresetCard("Albedo", "glob: Assets/_ProjectName/Art/Textures/Albedo/**", ConfigurePresets.IsAlbedoConfigured, ConfigurePresets.ApplyAlbedo);
                    DrawPresetCard("Normal", "glob: Assets/_ProjectName/Art/Textures/Normal/**", ConfigurePresets.IsNormalConfigured, ConfigurePresets.ApplyNormal);
                    DrawPresetCard("Roughness", "glob: Assets/_ProjectName/Art/Textures/Roughness/**", ConfigurePresets.IsRoughnessConfigured, ConfigurePresets.ApplyRoughness);
                    DrawPresetCard("Mask", "glob: Assets/_ProjectName/Art/Textures/Mask/**", ConfigurePresets.IsMaskConfigured, ConfigurePresets.ApplyMask);
                    DrawPresetCard("HDRI", "glob: Assets/_ProjectName/Art/Textures/HDRI/**", ConfigurePresets.IsHDRIConfigured, ConfigurePresets.ApplyHDRI);

                    GUILayout.Space(2);
                    GUILayout.Label("MODELS", EditorStyles.centeredGreyMiniLabel);
                    DrawPresetCard("FBX Model", "glob: Assets/_ProjectName/Art/Models/**", ConfigurePresets.IsFBXModelConfigured, ConfigurePresets.ApplyFBXModel);

                    GUILayout.Space(2);
                    GUILayout.Label("ANIMATIONS", EditorStyles.centeredGreyMiniLabel);
                    DrawPresetCard("FBX Animation", "glob: Assets/_ProjectName/Art/Animations/**", ConfigurePresets.IsFBXAnimationConfigured, ConfigurePresets.ApplyFBXAnimation);
                }

                EditorGUILayout.EndVertical();
            }

            EditorGUILayout.EndVertical();
        }

        private void DrawWindows()
        {
            EditorGUILayout.BeginVertical(EditorStyles.helpBox);

            m_showWindows = EditorGUILayout.Foldout(m_showWindows, "WINDOWS", true, EditorStyles.foldoutHeader);

            if (m_showWindows)
            {
                GUILayout.Space(4);

                DrawWindowCard("New Project Wizard",
                    "Guided wizard that runs all setup steps in order — use this to configure a new project from scratch.",
                    NewProjectWizard.ShowWindow);

                DrawWindowCard("PlayerPrefs Inspector",
                    "View, edit and delete all PlayerPrefs keys stored for this project.",
                    PlayerPrefsInspectorWindow.ShowWindow);

                DrawWindowCard("Layer Collision Matrix",
                    "Visual editor for configuring which physics layers collide with each other.",
                    LayerCollisionMatrixWindow.ShowWindow);

                DrawWindowCard("Assembly Definitions",
                    "Lists all .asmdef files in the project with their key configuration data — names, namespaces, platform filters and references. Also provides a one-click Generate button.",
                    AssemblyDefinitionsWindow.ShowWindow);

                DrawWindowCard("Project Health",
                    "Runs validation checks across the project and lists errors, warnings and available auto-fixes.",
                    ProjectHealthWindow.ShowWindow);
            }

            EditorGUILayout.EndVertical();
        }

        private static void DrawWindowCard(string title, string description, System.Action openAction)
        {
            EditorGUILayout.BeginHorizontal(EditorStyles.helpBox);

            EditorGUILayout.BeginVertical();
            GUILayout.Label(title, EditorStyles.boldLabel);
            GUILayout.Label(description, EditorStyles.wordWrappedMiniLabel);
            EditorGUILayout.EndVertical();

            GUILayout.FlexibleSpace();

            var prevColor = GUI.color;
            GUI.color = new Color(0.6f, 0.85f, 1f);
            if (GUILayout.Button("Open", GUILayout.Width(50), GUILayout.ExpandHeight(true)))
                openAction?.Invoke();
            GUI.color = prevColor;

            EditorGUILayout.EndHorizontal();
            GUILayout.Space(3);
        }

        private void DrawPresetCard(string title, string filter, bool isConfigured, System.Action applyAction)
        {
            EditorGUILayout.BeginHorizontal();

            var prevColor = GUI.color;
            GUI.color = isConfigured ? new Color(0.3f, 0.8f, 0.3f) : new Color(0.7f, 0.7f, 0.7f);
            GUILayout.Label(isConfigured ? "[Configured]" : "[  ]", GUILayout.Width(isConfigured ? 84 : 36));
            GUI.color = prevColor;

            GUILayout.Label(title, EditorStyles.boldLabel, GUILayout.Width(110));
            GUI.color = new Color(0.6f, 0.6f, 0.6f);
            GUILayout.Label(filter, EditorStyles.miniLabel);
            GUI.color = prevColor;
            GUILayout.FlexibleSpace();

            if (isConfigured)
            {
                if (GUILayout.Button("Open", GUILayout.Width(44)))
                    SettingsService.OpenProjectSettings("Project/Preset Manager");
            }
            else
            {
                if (GUILayout.Button("Run", GUILayout.Width(44)))
                {
                    applyAction?.Invoke();
                    Repaint();
                }
            }

            EditorGUILayout.EndHorizontal();
        }

        private void DrawToolCard(string title, string description, bool isComplete, string statusText,
            string buttonLabel, System.Action action, int buttonWidth = 60, bool showBadge = true,
            System.Action helpAction = null)
        {
            EditorGUILayout.BeginVertical(EditorStyles.helpBox);
            EditorGUILayout.BeginHorizontal();

            if (showBadge)
            {
                var prevColor = GUI.color;
                GUI.color = isComplete ? new Color(0.3f, 0.8f, 0.3f) : new Color(0.7f, 0.7f, 0.7f);
                GUILayout.Label(isComplete ? "[Configured]" : "[  ]", GUILayout.Width(isComplete ? 84 : 36));
                GUI.color = prevColor;
            }

            GUILayout.Label(title, EditorStyles.boldLabel);
            GUILayout.FlexibleSpace();

            if (helpAction != null)
            {
                if (GUILayout.Button("?", GUILayout.Width(20)))
                    helpAction.Invoke();
            }

            if (GUILayout.Button(buttonLabel, GUILayout.Width(buttonWidth)))
            {
                action?.Invoke();
                Repaint();
            }

            EditorGUILayout.EndHorizontal();

            GUILayout.Label(description, EditorStyles.wordWrappedMiniLabel);

            if (!string.IsNullOrEmpty(statusText))
            {
                var statusColor = GUI.color;
                GUI.color = isComplete ? new Color(0.5f, 0.8f, 0.5f) : new Color(0.6f, 0.6f, 0.6f);
                GUILayout.Label(statusText, EditorStyles.miniLabel);
                GUI.color = statusColor;
            }

            EditorGUILayout.EndVertical();
            GUILayout.Space(3);
        }

        private void DrawFileToolCard(string title, string description, string filePath, System.Action generateAction)
        {
            bool exists = File.Exists(filePath);
            string statusText = exists ? System.IO.Path.GetFileName(filePath) + " found at project root" : "Not created yet";

            EditorGUILayout.BeginVertical(EditorStyles.helpBox);
            EditorGUILayout.BeginHorizontal();

            var prevColor = GUI.color;
            GUI.color = exists ? new Color(0.3f, 0.8f, 0.3f) : new Color(0.7f, 0.7f, 0.7f);
            GUILayout.Label(exists ? "[Configured]" : "[  ]", GUILayout.Width(exists ? 84 : 36));
            GUI.color = prevColor;

            GUILayout.Label(title, EditorStyles.boldLabel);
            GUILayout.FlexibleSpace();

            if (exists)
            {
                if (GUILayout.Button("Open", GUILayout.Width(60)))
                    InternalEditorUtility.OpenFileAtLineExternal(filePath, 1, 0);
            }
            else
            {
                if (GUILayout.Button("Run", GUILayout.Width(60)))
                {
                    generateAction?.Invoke();
                    Repaint();
                }
            }

            EditorGUILayout.EndHorizontal();

            GUILayout.Label(description, EditorStyles.wordWrappedMiniLabel);

            prevColor = GUI.color;
            GUI.color = exists ? new Color(0.5f, 0.8f, 0.5f) : new Color(0.6f, 0.6f, 0.6f);
            GUILayout.Label(statusText, EditorStyles.miniLabel);
            GUI.color = prevColor;

            EditorGUILayout.EndVertical();
            GUILayout.Space(3);
        }
    }
}
