using System.IO;
using System.Reflection;
using UnityEditor;
using UnityEngine;

namespace Unity.BestPractices.Editor
{
    /// <summary>
    /// Copies LLM Instruction and AgentSkill files from this package into the host project's
    /// standard AI-tool config folders so they are available locally for AI assistants.
    ///
    /// Destinations:
    /// <list type="bullet">
    ///   <item><description><c>Editor/LLMInstructions/*.md</c> → <c>.github/instructions/*.instructions.md</c> (VS Code Copilot workspace instructions)</description></item>
    ///   <item><description><c>Editor/LLMInstructions/copilot-instructions.md</c> → <c>.github/copilot-instructions.md</c> (GitHub Copilot repository instructions)</description></item>
    ///   <item><description><c>Editor/AgentSkills/*.md</c> → <c>.github/prompts/*.prompt.md</c> (Copilot reusable prompts) and <c>.claude/commands/*.md</c> (Claude Code slash commands)</description></item>
    /// </list>
    ///
    /// All destination files are created (or overwritten) without confirmation — re-run safely at any time.
    /// </summary>
    public static class CopyAIFilesToProject
    {
        private const string k_LLMInstructionsFolder = "Editor/LLMInstructions";
        private const string k_AgentSkillsFolder = "Editor/AgentSkills";
        private const string k_CopilotInstructionsFile = "copilot-instructions.md";

        [MenuItem("Tools/Unity Workbench/AI/Copy AI Files to Project", false, 200)]
        public static void Execute()
        {
            var packageInfo = UnityEditor.PackageManager.PackageInfo.FindForAssembly(
                Assembly.GetExecutingAssembly());

            if (packageInfo == null)
            {
                EditorUtility.DisplayDialog(
                    "Copy AI Files",
                    "Could not resolve package path. Make sure the package is installed correctly.",
                    "OK");
                return;
            }

            string packageRoot = packageInfo.resolvedPath;
            string projectRoot = Path.GetDirectoryName(Application.dataPath);

            int llmCount = CopyLLMInstructions(packageRoot, projectRoot);
            int skillCount = CopyAgentSkills(packageRoot, projectRoot);

            AssetDatabase.Refresh();

            EditorUtility.DisplayDialog(
                "Copy AI Files — Done",
                $"Copied to project root:\n\n" +
                $"• {llmCount} LLM Instruction file(s) → .github/instructions/\n" +
                $"• .github/copilot-instructions.md (consolidated)\n" +
                $"• {skillCount} AgentSkill file(s) → .github/prompts/ and .claude/commands/",
                "OK");
        }
        /// <summary>Copies only LLM instruction files to .github/instructions/ (and .github/copilot-instructions.md).</summary>
        public static void ExecuteLLMInstructionsOnly()
        {
            var packageInfo = UnityEditor.PackageManager.PackageInfo.FindForAssembly(
                Assembly.GetExecutingAssembly());
            if (packageInfo == null) return;

            string packageRoot = packageInfo.resolvedPath;
            string projectRoot = Path.GetDirectoryName(Application.dataPath);
            int count = CopyLLMInstructions(packageRoot, projectRoot);
            AssetDatabase.Refresh();
            Debug.Log($"[Best Practices] Copied {count} LLM instruction file(s) to .github/instructions/");
        }

        /// <summary>Copies only AgentSkill files to .github/prompts/ and .claude/commands/.</summary>
        public static void ExecuteAgentSkillsOnly()
        {
            var packageInfo = UnityEditor.PackageManager.PackageInfo.FindForAssembly(
                Assembly.GetExecutingAssembly());
            if (packageInfo == null) return;

            string packageRoot = packageInfo.resolvedPath;
            string projectRoot = Path.GetDirectoryName(Application.dataPath);
            int count = CopyAgentSkills(packageRoot, projectRoot);
            AssetDatabase.Refresh();
            Debug.Log($"[Best Practices] Copied {count} agent skill file(s) to .github/prompts/ and .claude/commands/");
        }
        /// <summary>Returns the absolute destination path where the given LLM instruction source would be copied in the project.</summary>
        public static string GetLLMInstructionDestPath(string srcAbsPath, string projectRoot)
        {
            string fileName = Path.GetFileName(srcAbsPath);
            if (string.Equals(fileName, k_CopilotInstructionsFile, System.StringComparison.OrdinalIgnoreCase))
                return Path.Combine(projectRoot, ".github", "copilot-instructions.md");

            string baseName = Path.GetFileNameWithoutExtension(fileName);
            return Path.Combine(projectRoot, ".github", "instructions", baseName + ".instructions.md");
        }

        /// <summary>Returns true if the destination file for the given LLM instruction source already exists in the project.</summary>
        public static bool LLMInstructionExistsInProject(string srcAbsPath, string projectRoot)
            => File.Exists(GetLLMInstructionDestPath(srcAbsPath, projectRoot));

        /// <summary>Returns true if the local copy exists but its content differs from the package source.</summary>
        public static bool LLMInstructionIsOutdated(string srcAbsPath, string projectRoot)
        {
            string destPath = GetLLMInstructionDestPath(srcAbsPath, projectRoot);
            if (!File.Exists(destPath)) return false;
            return File.ReadAllText(srcAbsPath) != File.ReadAllText(destPath);
        }

        /// <summary>Copies a single LLM instruction file to .github/instructions/ (and to .github/copilot-instructions.md if it is the consolidated file).</summary>
        public static void CopySingleLLMInstruction(string srcAbsPath, string projectRoot)
        {
            string fileName = Path.GetFileName(srcAbsPath);

            if (string.Equals(fileName, k_CopilotInstructionsFile, System.StringComparison.OrdinalIgnoreCase))
            {
                string dest = Path.Combine(projectRoot, ".github", "copilot-instructions.md");
                Directory.CreateDirectory(Path.GetDirectoryName(dest));
                File.Copy(srcAbsPath, dest, overwrite: true);
            }
            else
            {
                string instructionsDir = Path.Combine(projectRoot, ".github", "instructions");
                Directory.CreateDirectory(instructionsDir);
                string baseName = Path.GetFileNameWithoutExtension(fileName);
                File.Copy(srcAbsPath, Path.Combine(instructionsDir, baseName + ".instructions.md"), overwrite: true);
            }
        }

        /// <summary>Returns the absolute destination path for the .github/prompts/ copy of the given AgentSkill source.</summary>
        public static string GetAgentSkillPromptDestPath(string srcAbsPath, string projectRoot)
        {
            string baseName = Path.GetFileNameWithoutExtension(Path.GetFileName(srcAbsPath));
            return Path.Combine(projectRoot, ".github", "prompts", baseName + ".prompt.md");
        }

        /// <summary>Returns true if the local copy exists but its content differs from the package source.</summary>
        public static bool AgentSkillIsOutdated(string srcAbsPath, string projectRoot)
        {
            string destPath = GetAgentSkillPromptDestPath(srcAbsPath, projectRoot);
            if (!File.Exists(destPath)) return false;
            return File.ReadAllText(srcAbsPath) != File.ReadAllText(destPath);
        }

        /// <summary>Copies a single AgentSkill file to .github/prompts/ and .claude/commands/.</summary>
        public static void CopySingleAgentSkill(string srcAbsPath, string projectRoot)
        {
            string baseName = Path.GetFileNameWithoutExtension(Path.GetFileName(srcAbsPath));

            string promptsDir = Path.Combine(projectRoot, ".github", "prompts");
            Directory.CreateDirectory(promptsDir);
            File.Copy(srcAbsPath, Path.Combine(promptsDir, baseName + ".prompt.md"), overwrite: true);

            string claudeDir = Path.Combine(projectRoot, ".claude", "commands");
            Directory.CreateDirectory(claudeDir);
            File.Copy(srcAbsPath, Path.Combine(claudeDir, baseName + ".md"), overwrite: true);
        }

        private static int CopyLLMInstructions(string packageRoot, string projectRoot)
        {
            string sourceDir = Path.Combine(packageRoot, k_LLMInstructionsFolder);
            if (!Directory.Exists(sourceDir))
                return 0;

            string instructionsDir = Path.Combine(projectRoot, ".github", "instructions");
            Directory.CreateDirectory(instructionsDir);

            string[] sourceFiles = Directory.GetFiles(sourceDir, "*.md");
            int count = 0;

            foreach (string srcPath in sourceFiles)
            {
                string fileName = Path.GetFileName(srcPath);

                if (string.Equals(fileName, k_CopilotInstructionsFile, System.StringComparison.OrdinalIgnoreCase))
                {
                    // Copy consolidated file to .github/copilot-instructions.md
                    string copilotDest = Path.Combine(projectRoot, ".github", "copilot-instructions.md");
                    Directory.CreateDirectory(Path.GetDirectoryName(copilotDest));
                    File.Copy(srcPath, copilotDest, overwrite: true);
                    // Do not also add it to .github/instructions/ to avoid redundancy
                    continue;
                }

                // Rename FileName.md → FileName.instructions.md
                string baseName = Path.GetFileNameWithoutExtension(fileName);
                string destFileName = baseName + ".instructions.md";
                string destPath = Path.Combine(instructionsDir, destFileName);
                File.Copy(srcPath, destPath, overwrite: true);
                count++;
            }

            return count;
        }

        private static int CopyAgentSkills(string packageRoot, string projectRoot)
        {
            string sourceDir = Path.Combine(packageRoot, k_AgentSkillsFolder);
            if (!Directory.Exists(sourceDir))
                return 0;

            string promptsDir = Path.Combine(projectRoot, ".github", "prompts");
            string claudeDir = Path.Combine(projectRoot, ".claude", "commands");
            Directory.CreateDirectory(promptsDir);
            Directory.CreateDirectory(claudeDir);

            string[] sourceFiles = Directory.GetFiles(sourceDir, "*.md");
            int count = 0;

            foreach (string srcPath in sourceFiles)
            {
                string baseName = Path.GetFileNameWithoutExtension(Path.GetFileName(srcPath));

                // .github/prompts/<Name>.prompt.md  (VS Code Copilot reusable prompts)
                string promptDest = Path.Combine(promptsDir, baseName + ".prompt.md");
                File.Copy(srcPath, promptDest, overwrite: true);

                // .claude/commands/<Name>.md  (Claude Code slash commands)
                string claudeDest = Path.Combine(claudeDir, baseName + ".md");
                File.Copy(srcPath, claudeDest, overwrite: true);

                count++;
            }

            return count;
        }
    }
}
