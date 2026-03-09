using System.Collections.Generic;
using UnityEditor;

namespace UnityBestPractices.Editor.Validator
{
    public class ProjectSettingsValidator : IValidator
    {
        public string Name => "Project Settings";
        public string Description => "Checks for recommended Unity 6 project settings";

        public ValidationResult Validate()
        {
            var issues = new List<ValidationIssue>();

            CheckEnterPlayModeSettings(issues);
            CheckScriptingBackend(issues);

            return new ValidationResult(Name, issues.ToArray());
        }

        private static void CheckEnterPlayModeSettings(List<ValidationIssue> issues)
        {
            if (!EditorSettings.enterPlayModeOptionsEnabled)
            {
                issues.Add(new ValidationIssue(
                    "Enter Play Mode optimisations are disabled. Enable them in Edit > Project Settings > Editor to speed up iteration. " +
                    "Use Window > Best Practices > Configure Project Settings to apply recommended settings.",
                    ValidationSeverity.Warning,
                    "",
                    "Project Settings"
                ));
            }
        }

        private static void CheckScriptingBackend(List<ValidationIssue> issues)
        {
            var backend = PlayerSettings.GetScriptingBackend(BuildTargetGroup.Standalone);
            if (backend == ScriptingImplementation.Mono2x)
            {
                issues.Add(new ValidationIssue(
                    "Standalone scripting backend is Mono. IL2CPP is recommended for release builds (better performance and security). " +
                    "Change in Edit > Project Settings > Player, or use Window > Best Practices > Configure Project Settings.",
                    ValidationSeverity.Info,
                    "",
                    "Project Settings"
                ));
            }
        }
    }
}
