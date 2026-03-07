using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace UnityBestPractices.Editor.Validator
{
    public class InputSystemValidator : IValidator
    {
        public string Name => "Input System";
        public string Description => "Validates Input System configuration (old vs new)";

        public ValidationResult Validate()
        {
            var issues = new List<ValidationIssue>();

            // Check which input system is active
            var activeInputHandler = GetActiveInputHandler();

            switch (activeInputHandler)
            {
                case InputHandler.InputManager:
                    issues.Add(new ValidationIssue(
                        "Using Legacy Input Manager. Consider upgrading to new Input System.",
                        ValidationSeverity.Info,
                        "",
                        Name
                    ));
                    break;

                case InputHandler.InputSystem:
                    issues.Add(new ValidationIssue(
                        "Using new Input System (recommended)",
                        ValidationSeverity.Info,
                        "",
                        Name
                    ));

                    // Check if Input Actions asset exists
                    CheckForInputActionsAsset(issues);
                    break;

                case InputHandler.Both:
                    issues.Add(new ValidationIssue(
                        "Using both Input Systems. This may cause conflicts and increased build size.",
                        ValidationSeverity.Warning,
                        "",
                        Name
                    ));
                    break;
            }

            // Check for old Input usage in scripts
            CheckForOldInputUsageInScripts(issues);

            return new ValidationResult(Name, issues.ToArray());
        }

        private enum InputHandler
        {
            InputManager,
            InputSystem,
            Both
        }

        private InputHandler GetActiveInputHandler()
        {
            // Check PlayerSettings for active input handler
            #if UNITY_2019_3_OR_NEWER
            var currentSetting = PlayerSettings.GetScriptingDefineSymbolsForGroup(EditorUserBuildSettings.selectedBuildTargetGroup);

            bool hasNewInputSystem = currentSetting.Contains("ENABLE_INPUT_SYSTEM");
            bool hasOldInputManager = currentSetting.Contains("ENABLE_LEGACY_INPUT_MANAGER") || !hasNewInputSystem;

            if (hasNewInputSystem && hasOldInputManager)
                return InputHandler.Both;
            else if (hasNewInputSystem)
                return InputHandler.InputSystem;
            else
                return InputHandler.InputManager;
            #else
            return InputHandler.InputManager;
            #endif
        }

        private void CheckForInputActionsAsset(List<ValidationIssue> issues)
        {
            // Search for InputActions assets in the project
            string[] inputActionGuids = AssetDatabase.FindAssets("t:InputActionAsset");

            if (inputActionGuids.Length == 0)
            {
                issues.Add(new ValidationIssue(
                    "New Input System enabled but no Input Actions asset found",
                    ValidationSeverity.Warning,
                    "",
                    Name
                ));
            }
        }

        private void CheckForOldInputUsageInScripts(List<ValidationIssue> issues)
        {
            // Find all C# scripts
            string[] scriptGuids = AssetDatabase.FindAssets("t:Script");
            int oldInputUsageCount = 0;

            foreach (string guid in scriptGuids)
            {
                string scriptPath = AssetDatabase.GUIDToAssetPath(guid);

                // Skip package scripts
                if (scriptPath.StartsWith("Packages/"))
                    continue;

                try
                {
                    string scriptContent = System.IO.File.ReadAllText(scriptPath);

                    // Check for common old Input usage patterns
                    if (scriptContent.Contains("Input.GetKey") ||
                        scriptContent.Contains("Input.GetButton") ||
                        scriptContent.Contains("Input.GetAxis") ||
                        scriptContent.Contains("Input.GetMouseButton"))
                    {
                        oldInputUsageCount++;
                    }
                }
                catch
                {
                    // Skip files that can't be read
                }
            }

            if (oldInputUsageCount > 0)
            {
                issues.Add(new ValidationIssue(
                    $"Found {oldInputUsageCount} script{(oldInputUsageCount != 1 ? "s" : "")} using legacy Input class",
                    ValidationSeverity.Info,
                    "",
                    Name
                ));
            }
        }
    }
}
