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

        /// <summary>Label shown on the per-issue fix button (defaults to "Fix").</summary>
        [NonSerialized]
        public string FixLabel = "Fix";

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

        /// <summary>
        /// Optional delegate that fixes all issues in this result at once.
        /// When set, an "Fix All" / "Add All" button is shown in the validator category header.
        /// Not serialized — populated at runtime by the validator.
        /// </summary>
        [NonSerialized]
        public System.Action FixAllAction;

        /// <summary>Label shown on the fix-all button (defaults to "Fix All").</summary>
        [NonSerialized]
        public string FixAllLabel = "Fix All";

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
