using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;
using UnityBestPractices.Editor.Validator;

namespace UnityBestPractices.Editor.Dashboard
{
    public struct LLMInstructionFile
    {
        public string DisplayName;
        public string AssetPath;
    }

    public struct AgentSkillFile
    {
        public string DisplayName;
        public string AssetPath;
    }

    public class ProjectDashboardData
    {
        // Folder Structure
        public int ExistingFoldersCount;
        public int TotalRecommendedFolders;

        // Presets
        public bool HasAudioPresets;
        public bool HasTexturePresets;

        // LLM Instructions
        public LLMInstructionFile[] LLMInstructionFiles = System.Array.Empty<LLMInstructionFile>();
        public int LLMInstructionFilesCount => LLMInstructionFiles.Length;

        // Agent Skills
        public AgentSkillFile[] AgentSkillFiles = System.Array.Empty<AgentSkillFile>();
        public int AgentSkillFilesCount => AgentSkillFiles.Length;

        // Git & IDE Config
        public bool HasGitIgnore;
        public bool IsGitInitialized;
        public bool HasEditorConfig;

        // Package
        public string PackageVersion;

        // Validation Results
        public ValidationResult[] ValidationResults;
        public int TotalErrors;
        public int TotalWarnings;
        public int TotalInfos;

        public static ProjectDashboardData Gather()
        {
            var data = new ProjectDashboardData();

            GatherPackageVersion(data);
            GatherFolderStructureData(data);
            GatherPresetData(data);
            GatherLLMInstructionData(data);
            GatherAgentSkillData(data);
            GatherGitData(data);

            return data;
        }

        /// <summary>
        /// Runs all validators and populates <see cref="ValidationResults"/> and the totals.
        /// Called explicitly from the dashboard when the user clicks Run Analysis.
        /// </summary>
        public static void RunValidation(ProjectDashboardData data)
        {
            GatherValidationData(data);
        }

        private static void GatherPackageVersion(ProjectDashboardData data)
        {
            var packageInfo = UnityEditor.PackageManager.PackageInfo.FindForAssembly(
                System.Reflection.Assembly.GetExecutingAssembly());
            data.PackageVersion = packageInfo?.version ?? "unknown";
        }

        private static void GatherFolderStructureData(ProjectDashboardData data)
        {
            string[] recommendedFolders = new string[]
            {
                "Scenes", "Scripts", "Prefabs", "Materials", "Textures",
                "Audio", "Animations", "Resources", "Editor", "Plugins"
            };

            data.TotalRecommendedFolders = recommendedFolders.Length;
            data.ExistingFoldersCount = 0;

            foreach (var folder in recommendedFolders)
            {
                string path = Path.Combine(Application.dataPath, folder);
                if (Directory.Exists(path))
                {
                    data.ExistingFoldersCount++;
                }
            }
        }

        private static void GatherPresetData(ProjectDashboardData data)
        {
            // Check for preset folders (simplified check)
            string presetsPath = Path.Combine(Application.dataPath, "Editor", "Presets");

            if (Directory.Exists(presetsPath))
            {
                string audioPresetsPath = Path.Combine(presetsPath, "Audio");
                string texturePresetsPath = Path.Combine(presetsPath, "Textures");

                data.HasAudioPresets = Directory.Exists(audioPresetsPath) &&
                                      Directory.GetFiles(audioPresetsPath, "*.preset").Length > 0;
                data.HasTexturePresets = Directory.Exists(texturePresetsPath) &&
                                        Directory.GetFiles(texturePresetsPath, "*.preset").Length > 0;
            }
        }

        private static void GatherLLMInstructionData(ProjectDashboardData data)
        {
            string[] searchFolders = new string[]
            {
                "Packages/com.unity.best-practices/Editor/LLMInstructions",
                "Assets/Editor/LLMInstructions"
            };

            var files = new List<LLMInstructionFile>();

            foreach (string folder in searchFolders)
            {
                string[] guids = AssetDatabase.FindAssets("t:TextAsset", new[] { folder });
                foreach (string guid in guids)
                {
                    string assetPath = AssetDatabase.GUIDToAssetPath(guid);
                    if (!assetPath.EndsWith(".md", System.StringComparison.OrdinalIgnoreCase))
                        continue;

                    string fileName = Path.GetFileNameWithoutExtension(assetPath);
                    files.Add(new LLMInstructionFile
                    {
                        DisplayName = fileName,
                        AssetPath = assetPath
                    });
                }
            }

            files.Sort((a, b) => string.Compare(a.DisplayName, b.DisplayName, System.StringComparison.OrdinalIgnoreCase));
            data.LLMInstructionFiles = files.ToArray();
        }

        private static void GatherAgentSkillData(ProjectDashboardData data)
        {
            string[] searchFolders = new string[]
            {
                "Packages/com.unity.best-practices/Editor/AgentSkills",
                "Assets/Editor/AgentSkills"
            };

            var files = new List<AgentSkillFile>();

            foreach (string folder in searchFolders)
            {
                string[] guids = AssetDatabase.FindAssets("t:TextAsset", new[] { folder });
                foreach (string guid in guids)
                {
                    string assetPath = AssetDatabase.GUIDToAssetPath(guid);
                    if (!assetPath.EndsWith(".md", System.StringComparison.OrdinalIgnoreCase))
                        continue;

                    files.Add(new AgentSkillFile
                    {
                        DisplayName = Path.GetFileNameWithoutExtension(assetPath),
                        AssetPath = assetPath
                    });
                }
            }

            files.Sort((a, b) => string.Compare(a.DisplayName, b.DisplayName, System.StringComparison.OrdinalIgnoreCase));
            data.AgentSkillFiles = files.ToArray();
        }

        private static void GatherGitData(ProjectDashboardData data)
        {
            string projectRoot = Directory.GetParent(Application.dataPath).FullName;

            data.IsGitInitialized = Directory.Exists(Path.Combine(projectRoot, ".git"));
            data.HasGitIgnore = File.Exists(Path.Combine(projectRoot, ".gitignore"));
            data.HasEditorConfig = File.Exists(Path.Combine(projectRoot, ".editorconfig"));
        }

        private static void GatherValidationData(ProjectDashboardData data)
        {
            var validators = new List<IValidator>
            {
                new AssemblyDefinitionValidator(),
                new SceneValidator(),
                new GitIgnoreValidator(),
                new FolderStructureValidator(),
                new UnusedAssetsValidator(),
                new DefaultNameValidator(),
                new BrokenPrefabValidator(),
                new InputSystemValidator(),
                new ProjectSettingsValidator()
            };

            var results = new List<ValidationResult>();
            data.TotalErrors = 0;
            data.TotalWarnings = 0;
            data.TotalInfos = 0;

            foreach (var validator in validators)
            {
                var result = validator.Validate();
                results.Add(result);

                data.TotalErrors += result.GetErrorCount();
                data.TotalWarnings += result.GetWarningCount();
                data.TotalInfos += result.GetInfoCount();
            }

            data.ValidationResults = results.ToArray();
        }
    }
}
