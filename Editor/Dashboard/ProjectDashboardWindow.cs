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
        private string m_projectName = "";
        private string m_subSystemName = "";
        private bool m_subSystemCreateAsmdef = true;
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
            if (string.IsNullOrEmpty(m_projectName))
                m_projectName = EditorPrefs.GetString(SetupProjectFolders.k_ProjectNamePrefKey, "");
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
            {
                RefreshData();
                Repaint();
            }
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
                    ConfigureIterationSettings.ApplyBackendRelease,
                    helpAction: () => ExplainerWindow.Show(
                        "Scripting Backend (Iteration)",
                        new[]
                        {
                            "Dev — Mono\n" +
                            "Compiles to .NET bytecode. Incremental recompiles are very fast, " +
                            "keeping the code-test loop short during day-to-day work. " +
                            "Use this on your local machine.",
                            "Release — IL2CPP\n" +
                            "Transpiles to C++ before compiling. Longer build times but optimal " +
                            "runtime performance and required for iOS. " +
                            "Use this in CI/CD builds and when profiling final performance."
                        }));

                // 2. IL2CPP Code Generation (N/A when Mono is active)
                using (new EditorGUI.DisabledScope(isMono))
                {
                    DrawIterationCard(
                        "IL2CPP Code Generation" + (isMono ? " [N/A — Mono active]" : ""),
                        "Faster builds (OptimizeSize): shorter build times during dev.\nFaster runtime (OptimizeSpeed): maximises runtime performance for release.",
                        ConfigureIterationSettings.IsIL2CppCodeGenDev,
                        ConfigureIterationSettings.IsIL2CppCodeGenRelease,
                        ConfigureIterationSettings.ApplyIL2CppCodeGenDev,
                        ConfigureIterationSettings.ApplyIL2CppCodeGenRelease,
                        helpAction: () => ExplainerWindow.Show(
                            "IL2CPP Code Generation",
                            new[]
                            {
                                "Only applies when the Scripting Backend is set to IL2CPP. " +
                                "Has no effect when using Mono.",
                                "Dev — OptimizeSize\n" +
                                "IL2CPP focuses on producing smaller generated C++ code. " +
                                "Compile times are shorter and the resulting binary is smaller, " +
                                "which is preferable during iteration.",
                                "Release — OptimizeSpeed\n" +
                                "IL2CPP emits the most aggressively inlined and optimised C++. " +
                                "Longer build times but maximum runtime throughput " +
                                "for profiling and shipping."
                            }));
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
                    ConfigureIterationSettings.ApplyAsyncShadersOff,
                    helpAction: () => ExplainerWindow.Show(
                        "Async Shader Compilation",
                        new[]
                        {
                            "Dev — On\n" +
                            "Shader variants are compiled asynchronously in the background. " +
                            "While a variant is compiling Unity renders a plain cyan placeholder. " +
                            "This prevents editor hitches when a new shader variant is encountered " +
                            "for the first time during iteration.",
                            "Release — Off\n" +
                            "All shader variants compile synchronously before rendering begins. " +
                            "No placeholder shaders ever appear. Use this for builds, screenshots " +
                            "and performance profiling so shader variance doesn't affect measurements."
                        }));

                // 5. Managed Stripping Level
                DrawIterationCard(
                    "Managed Stripping Level (Standalone)",
                    "Disabled (Dev): faster builds, full symbols, easier debugging — nothing is stripped.\nMinimal (Release): safely removes unused IL, reducing build size without breaking reflection.",
                    ConfigureIterationSettings.IsManagedStrippingDev,
                    ConfigureIterationSettings.IsManagedStrippingRelease,
                    ConfigureIterationSettings.ApplyManagedStrippingDev,
                    ConfigureIterationSettings.ApplyManagedStrippingRelease,
                    helpAction: () => ExplainerWindow.Show(
                        "Managed Code Stripping",
                        new[]
                        {
                            "Dev — Disabled\n" +
                            "No IL code is removed. The full assembly is preserved, which means " +
                            "faster build times, all debug symbols intact and no risk of the stripper " +
                            "accidentally removing a type accessed only via reflection.",
                            "Release — Minimal\n" +
                            "The IL linker removes types and methods that are provably unreachable. " +
                            "This reduces build size and is considered safe — Minimal stripping " +
                            "respects [Preserve] attributes and link.xml entries. More aggressive " +
                            "levels (Low, Medium, High) require thorough testing to avoid stripping " +
                            "reflection-accessed types."
                        }));

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
            System.Action applyDev, System.Action applyRelease,
            System.Action helpAction = null)
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

            if (helpAction != null)
            {
                if (GUILayout.Button("?", GUILayout.Width(20)))
                    helpAction.Invoke();
            }

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
                    if (GUILayout.Button("Settings", GUILayout.Width(58)))
                        SettingsService.OpenUserPreferences("Preferences/Asset Pipeline");
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
                    if (GUILayout.Button("Settings", GUILayout.Width(58)))
                        SettingsService.OpenUserPreferences("Preferences/Asset Pipeline");
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
                    disableAction: ConfigureProjectSettings.DisableEnterPlayMode,
                    helpAction: () => ExplainerWindow.Show(
                        "Enter Play Mode Options",
                        new[]
                        {
                            "ENABLED (recommended)\n" +
                            "Unity skips Domain Reload and Scene Reload when entering Play mode. " +
                            "This removes the multi-second compile/reload pause on every Play press, " +
                            "making the iteration loop much faster — especially on large projects.",
                            "Trade-off: static fields and singletons are NOT cleared between play sessions. " +
                            "You must reset them yourself using [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]. " +
                            "Failing to do so causes state bleed and hard-to-reproduce bugs.",
                            "DISABLED (Unity default)\n" +
                            "Full domain reload runs on every Play mode entry. All static state is wiped " +
                            "automatically, but each iteration adds several seconds of wait time."
                        }));

                DrawSettingCard(
                    "Scripting Backend: IL2CPP",
                    "IL2CPP: best runtime performance and required for iOS. Enables full AOT compilation and better stripping.\nMono (default): faster editor build times but lower runtime performance — use Mono for fast iteration via Iteration Settings.",
                    ConfigureProjectSettings.IsIL2CPPConfigured,
                    ConfigureProjectSettings.ApplyIL2CPP,
                    "Apply",
                    openSettingsPath: "Project/Player",
                    helpAction: () => ExplainerWindow.Show(
                        "Scripting Backend: IL2CPP",
                        new[]
                        {
                            "IL2CPP (recommended for release)\n" +
                            "Transpiles .NET IL bytecode to C++ before compiling. Produces faster " +
                            "runtime code, enables dead-code stripping, and is required for iOS App Store " +
                            "submissions. Also makes reverse engineering harder.",
                            "Mono (Unity default)\n" +
                            "Compiles directly to .NET bytecode interpreted at runtime. Build times are " +
                            "significantly shorter, making it ideal for day-to-day iteration. " +
                            "Use Mono locally and switch to IL2CPP in CI/CD for release builds.",
                            "Tip: the Iteration Settings panel lets you toggle backends quickly " +
                            "between a Dev (Mono) and Release (IL2CPP) profile."
                        }));

                DrawSettingCard(
                    "API Compatibility: .NET Standard 2.1",
                    ".NET Standard 2.1: broadest library compatibility with NuGet packages and aligns with modern .NET practices.\n.NET Framework (default): gives access to Unity's full internal API surface, but can cause compatibility issues with external libraries.",
                    ConfigureProjectSettings.IsApiCompatibilityConfigured,
                    ConfigureProjectSettings.ApplyApiCompatibility,
                    "Apply",
                    openSettingsPath: "Project/Player",
                    helpAction: () => ExplainerWindow.Show(
                        "API Compatibility Level",
                        new[]
                        {
                            ".NET Standard 2.1 (recommended)\n" +
                            "Targets the cross-platform .NET Standard surface. NuGet libraries written " +
                            "for .NET Standard will compile without issues. Produces smaller builds " +
                            "because unused parts of the BCL can be stripped.",
                            ".NET Framework (Unity default)\n" +
                            "Exposes the full .NET Framework API including Unity internals. " +
                            "Some third-party NuGet packages compiled for .NET Framework will fail or " +
                            "behave inconsistently on non-Windows platforms.",
                            "For most games .NET Standard 2.1 is the right choice. Only switch to " +
                            ".NET Framework if a specific library requires it."
                        }));

                DrawSettingCard(
                    "Asset Serialization: Force Text",
                    "Force Text: all assets serialize as readable YAML — diffs are meaningful and merges are possible in version control.\nBinary or Mixed (default): assets may be stored as binary blobs, making diff and merge in source control impractical.",
                    ConfigureProjectSettings.IsAssetSerializationConfigured,
                    ConfigureProjectSettings.ApplyAssetSerialization,
                    "Apply",
                    openSettingsPath: "Project/Editor",
                    helpAction: () => ExplainerWindow.Show(
                        "Asset Serialization: Force Text",
                        new[]
                        {
                            "Force Text (recommended)\n" +
                            "All scene, prefab and asset files are written as human-readable YAML. " +
                            "Git can diff them line by line and tools like UnityYAMLMerge can resolve " +
                            "most merge conflicts automatically.",
                            "Binary / Mixed (Unity default)\n" +
                            "Assets may be stored as opaque binary blobs. Diffs show only hex garbage " +
                            "and any merge conflict requires manual resolution by discarding one side.",
                            "There is no runtime performance difference — serialization mode only affects " +
                            "how files are stored on disk in the editor."
                        }));

                DrawSettingCard(
                    "Version Control: Visible Meta Files",
                    "Visible Meta Files: .meta files are written to disk so source control tracks them — prevents GUID regeneration that breaks asset references.\nHidden Meta Files (default): .meta files are hidden; source control will miss them unless configured manually.",
                    ConfigureProjectSettings.IsVersionControlConfigured,
                    ConfigureProjectSettings.ApplyVersionControl,
                    "Apply",
                    openSettingsPath: "Project/Editor",
                    helpAction: () => ExplainerWindow.Show(
                        "Version Control: Visible Meta Files",
                        new[]
                        {
                            "Visible Meta Files (recommended)\n" +
                            "Unity writes a .meta file alongside every asset. The .meta file holds the " +
                            "asset's stable GUID, importer settings and labels. When .meta files are visible " +
                            "on disk, Git and other VCS tools track them automatically.",
                            "Hidden Meta Files (Unity default)\n" +
                            "Meta files are hidden from the OS. Source control tools won't detect or commit " +
                            "them unless explicitly configured. Missing .meta files cause Unity to regenerate " +
                            "new GUIDs, breaking all references to those assets across prefabs and scenes.",
                            "Always use Visible Meta Files when working with any version control system."
                        }));

                DrawSettingCard(
                    "Input System: New Input System Package",
                    "New Input System: event-driven, multi-device, cross-platform input with rebinding support. Required for modern input workflows.\nLegacy Input Manager (default): simple but limited — no controller rebinding, no multi-player input routing. Requires com.unity.inputsystem to be installed; a Unity restart may be needed.",
                    ConfigureProjectSettings.IsInputSystemConfigured,
                    ConfigureProjectSettings.ApplyInputSystem,
                    "Apply",
                    openSettingsPath: "Project/Player",
                    helpAction: () => ExplainerWindow.Show(
                        "Input System",
                        new[]
                        {
                            "New Input System Package (recommended)\n" +
                            "Event-driven architecture decouples game logic from device specifics. " +
                            "Supports keyboard, mouse, gamepad, touch and XR out of the box. " +
                            "Players can rebind controls at runtime. Multiple local players share " +
                            "one InputActionAsset cleanly via PlayerInput components.",
                            "Legacy Input Manager (Unity default)\n" +
                            "Simple polling API (Input.GetKey, Input.GetAxis). Works fine for " +
                            "prototypes but has no built-in rebinding, limited multi-player support " +
                            "and no cross-platform abstraction layer.",
                            "Note: applying this setting enables only the New Input System backend. " +
                            "The com.unity.inputsystem package must be installed and a full " +
                            "Unity restart is required after switching."
                        }));

                DrawSettingCard(
                    "Incremental GC",
                    "Enabled: GC work is spread across multiple frames, eliminating stop-the-world spikes during gameplay.\nDisabled: full stop-the-world GC passes run as needed — may cause visible frame drops in GC-heavy scenes.",
                    ConfigureProjectSettings.IsIncrementalGCConfigured,
                    ConfigureProjectSettings.ApplyIncrementalGC,
                    openSettingsPath: "Project/Player",
                    disableAction: ConfigureProjectSettings.DisableIncrementalGC,
                    helpAction: () => ExplainerWindow.Show(
                        "Incremental Garbage Collection",
                        new[]
                        {
                            "ENABLED (recommended)\n" +
                            "The GC spreads its collection work across multiple frames using " +
                            "time-slicing. The per-frame GC budget keeps frame times smooth even " +
                            "when a large number of objects are collected.",
                            "DISABLED (Unity default)\n" +
                            "The GC performs a full stop-the-world collection whenever it runs. " +
                            "In GC-heavy scenes this can cause visible frame spikes of 5-30 ms " +
                            "or more, especially on mobile hardware.",
                            "Incremental GC does not reduce the total amount of garbage — it only " +
                            "spreads the cost. Writing allocation-light code with object pooling " +
                            "remains important for performance-critical paths."
                        }));

                DrawSettingCard(
                    "Scene View: Create Objects at Origin",
                    "Enabled: new GameObjects always spawn at world origin (0,0,0) — predictable placement regardless of camera position.\nDisabled: new GameObjects spawn in front of the Scene View camera — convenient but inconsistent across machines.",
                    ConfigureProjectSettings.IsCreateObjectsAtOriginConfigured,
                    ConfigureProjectSettings.ApplyCreateObjectsAtOrigin,
                    openSettingsPath: "Preferences/Scene View",
                    disableAction: ConfigureProjectSettings.DisableCreateObjectsAtOrigin,
                    helpAction: () => ExplainerWindow.Show(
                        "Create Objects at Origin",
                        new[]
                        {
                            "ENABLED (recommended)\n" +
                            "New GameObjects are always placed at world position (0, 0, 0) regardless " +
                            "of where the Scene View camera is looking. Position is consistent across " +
                            "all team members' machines.",
                            "DISABLED (Unity default)\n" +
                            "New GameObjects are placed in front of the Scene View camera at whatever " +
                            "distance the editor calculates. This is convenient when working in a " +
                            "specific area of the scene, but the spawn position varies per person " +
                            "and can confuse prefab placement scripts that expect origin defaults."
                        }));

                DrawSettingCard(
                    "Hierarchy: Use New Hierarchy Window",
                    "Enabled: redesigned Hierarchy with better rendering performance for large scenes and the Query Builder for filtering by component type.\nDisabled: legacy Hierarchy window — no Query Builder, slower with many objects.",
                    ConfigureProjectSettings.IsNewHierarchyWindowConfigured,
                    ConfigureProjectSettings.ApplyNewHierarchyWindow,
                    openSettingsPath: "Preferences/Hierarchy",
                    disableAction: ConfigureProjectSettings.DisableNewHierarchyWindow,
                    helpAction: () => ExplainerWindow.Show(
                        "New Hierarchy Window",
                        new[]
                        {
                            "ENABLED (recommended)\n" +
                            "Unity 6's redesigned Hierarchy window uses a virtualised renderer that " +
                            "stays responsive even with thousands of GameObjects. Includes the " +
                            "Query Builder — a search bar that filters by component type, tag or " +
                            "layer without writing custom editor code.",
                            "DISABLED (Unity default when upgrading older projects)\n" +
                            "The legacy Hierarchy window renders all rows unconditionally, which " +
                            "slows down scrolling in large scenes. The Query Builder is unavailable."
                        }));

                DrawSettingCard(
                    "Asset Manager: Import to ThirdPartyAssets",
                    "Configured: Asset Manager imports to Assets/ThirdPartyAssets with subfolder creation — keeps third-party assets isolated from your own project folders.\nReset: imports go to Assets/ root — third-party packages mixed with project files, harder to audit and update.",
                    ConfigureProjectSettings.IsAssetManagerImportLocationConfigured,
                    ConfigureProjectSettings.ApplyAssetManagerImportLocation,
                    openSettingsPath: "Preferences/Asset Manager",
                    disableAction: ConfigureProjectSettings.ResetAssetManagerImportLocation,
                    disableLabel: "Reset",
                    helpAction: () => ExplainerWindow.Show(
                        "Asset Manager: Import Location",
                        new[]
                        {
                            "CONFIGURED (recommended)\n" +
                            "Unity Asset Manager downloads are placed into Assets/ThirdPartyAssets " +
                            "with a subfolder per asset pack. This keeps marketplace content " +
                            "completely separate from your own project files, making it easy to " +
                            "audit, update or remove third-party content without touching your code.",
                            "RESET (Unity default)\n" +
                            "Imports go directly into Assets/. Third-party folders mix with your " +
                            "own scripts and art, making housekeeping harder and increasing the " +
                            "risk of accidental file deletion."
                        }));
            }

            EditorGUILayout.EndVertical();
        }

        private void DrawSettingCard(string title, string description, bool isConfigured, System.Action applyAction, string buttonLabel = "Enable", string openSettingsPath = "Project/Player", System.Action disableAction = null, string disableLabel = "Disable", System.Action helpAction = null)
        {
            EditorGUILayout.BeginVertical(EditorStyles.helpBox);
            EditorGUILayout.BeginHorizontal();

            var prevColor = GUI.color;
            GUI.color = isConfigured ? new Color(0.3f, 0.8f, 0.3f) : new Color(0.7f, 0.7f, 0.7f);
            GUILayout.Label(isConfigured ? "[Configured]" : "[  ]", GUILayout.Width(isConfigured ? 84 : 36));
            GUI.color = prevColor;

            GUILayout.Label(title, EditorStyles.boldLabel);
            GUILayout.FlexibleSpace();

            if (helpAction != null)
            {
                if (GUILayout.Button("?", GUILayout.Width(20)))
                    helpAction.Invoke();
            }

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

        // Returns the git repository root by walking up from startPath until a .git folder is found.
        // Falls back to startPath if no .git folder is found.
        private static string FindGitRoot(string startPath)
        {
            string dir = startPath;
            while (!string.IsNullOrEmpty(dir))
            {
                if (Directory.Exists(Path.Combine(dir, ".git")))
                    return dir;
                string parent = Path.GetDirectoryName(dir);
                if (parent == dir) break;
                dir = parent;
            }
            return startPath;
        }

        // Returns the path of fileName if it exists at projectRoot, then at gitRoot, otherwise projectRoot path.
        private static string ResolveFilePath(string projectRoot, string gitRoot, string fileName)
        {
            string atProject = Path.Combine(projectRoot, fileName);
            if (File.Exists(atProject)) return atProject;
            string atGit = Path.Combine(gitRoot, fileName);
            if (File.Exists(atGit)) return atGit;
            return atProject;
        }

        private void DrawTools()
        {
            EditorGUILayout.BeginVertical(EditorStyles.helpBox);

            string projectRoot = Path.GetDirectoryName(Application.dataPath);
            string gitRoot = FindGitRoot(projectRoot);
            string gitignorePath = ResolveFilePath(projectRoot, gitRoot, ".gitignore");
            string gitattributesPath = ResolveFilePath(projectRoot, gitRoot, ".gitattributes");
            string editorconfigPath = ResolveFilePath(projectRoot, gitRoot, ".editorconfig");
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

                DrawSetupFoldersCard(foldersOk);

                DrawSubSystemCard();

                DrawFileToolCard(
                    "Generate .gitignore",
                    "Creates a Unity-optimised .gitignore at the project root, excluding Library, Temp, build outputs and IDE files.",
                    gitignorePath, GenerateGitIgnore.Execute,
                    helpAction: () => ExplainerWindow.Show(
                        "Generate .gitignore",
                        new[]
                        {
                            "A .gitignore tells Git which files and folders to exclude from version control. " +
                            "Without it, generated Unity output (Library/, Temp/, Builds/) and IDE-specific files " +
                            "get committed, causing slow clones, unnecessary diffs and merge conflicts.\n\n" +
                            "Compared to the standard Unity .gitignore template on GitHub, this file adds:\n" +
                            "  • /Packages/packages-lock.json — avoids locking all collaborators to the same resolved package revisions\n" +
                            "  • [Rr]ecordings/ and [Mm]emoryCaptures/ — Unity Recorder and Memory Profiler output\n" +
                            "  • Crashlytics and Gradle template files — Android build artefacts often auto-regenerated\n" +
                            "  • *.apk, *.aab, *.app, *.unitypackage — build outputs that should never be committed\n" +
                            "  • Broader IDE coverage: Rider (.idea/), Visual Studio (.vs/), and per-project files (*.pidb, *.booproj, *.svd, *.opendb, *.VC.db)",
                            "Generated file preview:\n\n" +
                            "# Unity generated\n" +
                            "[Ll]ibrary/\n" +
                            "[Tt]emp/\n" +
                            "[Oo]bj/\n" +
                            "[Bb]uild/\n" +
                            "[Bb]uilds/\n" +
                            "[Ll]ogs/\n" +
                            "[Uu]ser[Ss]ettings/\n" +
                            "[Mm]emoryCaptures/\n" +
                            "[Rr]ecordings/\n\n" +
                            "# Asset meta data should only be ignored when the\n" +
                            "# corresponding asset is also ignored\n" +
                            "!/[Aa]ssets/**/*.meta\n\n" +
                            "# Autogenerated Packages\n" +
                            "/[Pp]ackages/packages-lock.json\n\n" +
                            "# Visual Studio / Rider\n" +
                            ".vs/\n" +
                            ".idea/\n" +
                            "*.csproj  *.sln  *.suo  *.user  *.pidb\n" +
                            "*.booproj  *.svd  *.pdb  *.mdb  *.opendb  *.VC.db\n\n" +
                            "# OS generated\n" +
                            ".DS_Store  .DS_Store?  ._*\n" +
                            ".Spotlight-V100  .Trashes\n" +
                            "ehthumbs.db  Thumbs.db\n\n" +
                            "# Builds\n" +
                            "*.apk  *.aab  *.unitypackage  *.app\n\n" +
                            "# Crashlytics\n" +
                            "crashlytics-build.properties\n\n" +
                            "# Gradle\n" +
                            "/[Aa]ssets/Plugins/Android/baseProjectTemplate.gradle\n" +
                            "/[Aa]ssets/Plugins/Android/launcherTemplate.gradle\n" +
                            "/[Aa]ssets/Plugins/Android/mainTemplate.gradle\n" +
                            "/[Aa]ssets/Plugins/Android/gradleTemplate.properties",
                            "Once created, click Open to inspect or customise it. Re-running the tool overwrites only if you confirm the prompt."
                        },
                        runLabel: "Generate Now",
                        runAction: GenerateGitIgnore.Execute));

                DrawFileToolCard(
                    "Generate .gitattributes",
                    "Creates a .gitattributes at the project root that sets LF line endings for text files, configures UnityYAMLMerge for Unity YAML assets, and marks binary assets so Git never attempts text diffs on them.",
                    gitattributesPath, GenerateGitAttributes.Execute,
                    helpAction: () => ExplainerWindow.Show(
                        "Generate .gitattributes",
                        new[]
                        {
                            "A .gitattributes file controls how Git treats individual files — " +
                            "line endings, diff strategies and merge drivers.",
                            "The generated file does three things:\n" +
                            "  1. Forces LF line endings for all text files so the codebase is consistent " +
                            "across Windows, macOS and Linux machines.\n" +
                            "  2. Registers UnityYAMLMerge as the merge driver for Unity scene, prefab and " +
                            "asset files — dramatically reducing merge conflicts on binary-like YAML files.\n" +
                            "  3. Marks binary assets (png, fbx, mp3 …) as binary so Git never attempts " +
                            "a text diff on them.",
                            "Without this file, Windows developers will frequently corrupt Unity YAML files " +
                            "with CRLF line endings, and scene merges will be unpredictable."
                        },
                        runLabel: "Generate Now",
                        runAction: GenerateGitAttributes.Execute));

                DrawFileToolCard(
                    "Generate .editorconfig",
                    "Creates an .editorconfig enforcing C# naming and formatting conventions (Allman braces, 4-space indent, m_ prefix rules).",
                    editorconfigPath, GenerateEditorConfig.Execute,
                    helpAction: () => ExplainerWindow.Show(
                        "Generate .editorconfig",
                        new[]
                        {
                            "An .editorconfig file defines consistent code style rules that IDEs such as " +
                            "Rider, Visual Studio and VS Code automatically pick up and enforce — no manual " +
                            "formatter configuration needed per developer.",
                            "The generated file enforces the package's C# coding standard:\n" +
                            "  • Allman (open-brace-on-new-line) brace style\n" +
                            "  • 4-space indentation\n" +
                            "  • LF line endings and UTF-8 encoding\n" +
                            "  • m_ prefix required on private instance fields\n" +
                            "  • Naming rules for constants, properties and methods\n" +
                            "  • Various C# diagnostic severity overrides",
                            "Without this file each developer's IDE applies its own default formatting, " +
                            "which leads to noisy diffs and inconsistent style across the team."
                        },
                        runLabel: "Generate Now",
                        runAction: GenerateEditorConfig.Execute));

                DrawToolCard(
                    "Assembly Definitions",
                    "Manage and generate .asmdef files. Opens the Assembly Definitions panel where you can inspect all existing .asmdef files and generate the recommended set.",
                    false, "",
                    "Open", AssemblyDefinitionsWindow.ShowWindow,
                    helpAction: () => ExplainerWindow.Show(
                        "Assembly Definitions",
                        new[]
                        {
                            "Assembly Definition files (.asmdef) split your code into separate compiled " +
                            "assemblies. Unity only recompiles assemblies whose source changed, so " +
                            "larger projects see dramatically shorter recompile times.",
                            "Good practice is to have at least one runtime assembly and one editor-only " +
                            "assembly. The editor assembly references the runtime assembly, but the " +
                            "reverse is not allowed — this keeps editor-only code out of player builds.",
                            "The Assembly Definitions window lists all existing .asmdef files and lets " +
                            "you generate the recommended set for this project in one click."
                        }));

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

        private void DrawSetupFoldersCard(bool foldersOk)
        {
            EditorGUILayout.BeginVertical(EditorStyles.helpBox);
            EditorGUILayout.BeginHorizontal();

            var prevColor = GUI.color;
            GUI.color = foldersOk ? new Color(0.3f, 0.8f, 0.3f) : new Color(0.7f, 0.7f, 0.7f);
            GUILayout.Label(foldersOk ? "[Configured]" : "[  ]", GUILayout.Width(foldersOk ? 84 : 36));
            GUI.color = prevColor;

            GUILayout.Label("Setup Project Folders", EditorStyles.boldLabel);
            GUILayout.FlexibleSpace();

            if (GUILayout.Button("?", GUILayout.Width(20)))
            {
                ExplainerWindow.Show(
                    "Setup Project Folders",
                    new[]
                    {
                        "Creates a standardised folder hierarchy under Assets/{ProjectName}. " +
                        "Having a consistent structure makes it easier to navigate and maintain " +
                        "the project across a team and aligns with Unity's recommended layout.",
                        "If some folders already exist they are left untouched. Only the " +
                        "missing ones are created, so it is safe to run on an existing project."
                    },
                    preview:
                        "Assets/\n" +
                        "└── {ProjectName}/\n" +
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
                    runAction: () => { SetupProjectFolders.Execute(m_projectName.Trim()); RefreshData(); });
            }

            EditorGUILayout.EndHorizontal();

            GUILayout.Label(
                "Creates the recommended folder structure under Assets/{ProjectName}. Enter a project name to use as the root folder.",
                EditorStyles.wordWrappedMiniLabel);

            GUILayout.Space(4);

            EditorGUILayout.BeginHorizontal();
            m_projectName = EditorGUILayout.TextField(m_projectName);
            bool canRun = !string.IsNullOrWhiteSpace(m_projectName);
            using (new EditorGUI.DisabledScope(!canRun))
            {
                if (GUILayout.Button("Run", GUILayout.Width(44)))
                {
                    SetupProjectFolders.Execute(m_projectName.Trim());
                    RefreshData();
                    GUI.FocusControl(null);
                }
            }
            EditorGUILayout.EndHorizontal();

            var statusColor = GUI.color;
            GUI.color = foldersOk ? new Color(0.5f, 0.8f, 0.5f) : new Color(0.6f, 0.6f, 0.6f);
            GUILayout.Label(
                foldersOk
                    ? $"{m_data.ExistingFoldersCount}/{m_data.TotalRecommendedFolders} folders present"
                    : $"{m_data.ExistingFoldersCount}/{m_data.TotalRecommendedFolders} folders — enter a name and run",
                EditorStyles.miniLabel);
            GUI.color = statusColor;

            EditorGUILayout.EndVertical();
            GUILayout.Space(3);
        }

        private void DrawSubSystemCard()
        {
            EditorGUILayout.BeginVertical(EditorStyles.helpBox);
            EditorGUILayout.BeginHorizontal();
            GUILayout.Label("Create Game Sub-system", EditorStyles.boldLabel);
            EditorGUILayout.EndHorizontal();

            GUILayout.Label(
                "Creates a named sub-system folder under Assets/{ProjectName} with Scripts/, UI/, and Art/ subfolders.",
                EditorStyles.wordWrappedMiniLabel);

            GUILayout.Space(4);

            EditorGUILayout.BeginHorizontal();
            m_subSystemName = EditorGUILayout.TextField(m_subSystemName);
            bool canCreate = !string.IsNullOrWhiteSpace(m_subSystemName);
            using (new EditorGUI.DisabledScope(!canCreate))
            {
                if (GUILayout.Button("Create", GUILayout.Width(60)))
                {
                    string trimmed = m_subSystemName.Trim();
                    string projectName = EditorPrefs.GetString(SetupProjectFolders.k_ProjectNamePrefKey, "");
                    SetupProjectFolders.CreateSubSystem(trimmed);
                    if (m_subSystemCreateAsmdef && !string.IsNullOrEmpty(projectName))
                        GenerateAssemblyDefinitions.CreateSubSystemAsmdef(projectName, trimmed);
                    m_subSystemName = "";
                    RefreshData();
                    GUI.FocusControl(null);
                }
            }
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.BeginHorizontal();
            m_subSystemCreateAsmdef = EditorGUILayout.ToggleLeft(
                new GUIContent("Create Assembly Definition", "Generates a .asmdef in Scripts/ referencing the project's Runtime assembly"),
                m_subSystemCreateAsmdef);
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.EndVertical();
            GUILayout.Space(3);
        }

        private void DrawFileToolCard(string title, string description, string filePath, System.Action generateAction,
            System.Action helpAction = null)
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

            if (helpAction != null)
            {
                if (GUILayout.Button("?", GUILayout.Width(20)))
                    helpAction.Invoke();
            }

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
