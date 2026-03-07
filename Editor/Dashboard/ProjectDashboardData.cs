using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityBestPractices.Editor.Validator;

namespace UnityBestPractices.Editor.Dashboard
{
    public class ProjectDashboardData
    {
        // Folder Structure
        public int ExistingFoldersCount;
        public int TotalRecommendedFolders;

        // Presets
        public bool HasAudioPresets;
        public bool HasTexturePresets;

        // LLM Instructions
        public int LLMInstructionFilesCount;

        // Git
        public bool HasGitIgnore;
        public bool IsGitInitialized;

        // Validation Results
        public ValidationResult[] ValidationResults;
        public int TotalErrors;
        public int TotalWarnings;
        public int TotalInfos;

        public static ProjectDashboardData Gather()
        {
            var data = new ProjectDashboardData();

            // Gather folder structure info
            GatherFolderStructureData(data);

            // Gather preset info
            GatherPresetData(data);

            // Gather LLM instruction files
            GatherLLMInstructionData(data);

            // Gather Git info
            GatherGitData(data);

            // Run validators
            GatherValidationData(data);

            return data;
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
            string llmPath = Path.Combine(Application.dataPath, "Editor", "LLMInstructions");

            if (Directory.Exists(llmPath))
            {
                data.LLMInstructionFilesCount = Directory.GetFiles(llmPath, "*.md").Length;
            }
        }

        private static void GatherGitData(ProjectDashboardData data)
        {
            string projectRoot = Directory.GetParent(Application.dataPath).FullName;

            data.HasGitIgnore = File.Exists(Path.Combine(projectRoot, ".gitignore"));
            data.IsGitInitialized = Directory.Exists(Path.Combine(projectRoot, ".git"));
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
                new InputSystemValidator()
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
