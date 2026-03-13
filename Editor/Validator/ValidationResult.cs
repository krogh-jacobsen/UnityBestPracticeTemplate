using System;

namespace UnityBestPractices.Editor.Validator
{
    public enum ValidationSeverity
    {
        Info,
        Warning,
        Error
    }

    [Serializable]
    public class ValidationIssue
    {
        public string Message;
        public ValidationSeverity Severity;
        public string AssetPath;
        public string Category;

        /// <summary>
        /// Optional one-click fix delegate. When set, a Fix button is shown next to the issue in the dashboard.
        /// Not serialized — populated at runtime by the validator.
        /// </summary>
        [NonSerialized]
        public System.Action FixAction;

        public ValidationIssue(string message, ValidationSeverity severity, string assetPath = "", string category = "")
        {
            Message = message;
            Severity = severity;
            AssetPath = assetPath;
            Category = category;
        }
    }

    [Serializable]
    public class ValidationResult
    {
        public string ValidatorName;
        public ValidationIssue[] Issues;
        public bool HasErrors => GetErrorCount() > 0;
        public bool HasWarnings => GetWarningCount() > 0;

        public ValidationResult(string validatorName, ValidationIssue[] issues)
        {
            ValidatorName = validatorName;
            Issues = issues ?? new ValidationIssue[0];
        }

        public int GetErrorCount()
        {
            int count = 0;
            foreach (var issue in Issues)
            {
                if (issue.Severity == ValidationSeverity.Error)
                    count++;
            }
            return count;
        }

        public int GetWarningCount()
        {
            int count = 0;
            foreach (var issue in Issues)
            {
                if (issue.Severity == ValidationSeverity.Warning)
                    count++;
            }
            return count;
        }

        public int GetInfoCount()
        {
            int count = 0;
            foreach (var issue in Issues)
            {
                if (issue.Severity == ValidationSeverity.Info)
                    count++;
            }
            return count;
        }
    }
}
