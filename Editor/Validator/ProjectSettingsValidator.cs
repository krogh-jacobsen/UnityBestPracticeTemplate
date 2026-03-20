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

    }
}
