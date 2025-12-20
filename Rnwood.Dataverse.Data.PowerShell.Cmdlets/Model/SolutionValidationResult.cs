using System;
using System.Collections.Generic;
using System.Linq;

namespace Rnwood.Dataverse.Data.PowerShell.Commands.Model
{
    /// <summary>
    /// Represents the result of a solution validation operation.
    /// </summary>
    public class SolutionValidationResult
    {
        /// <summary>
        /// Gets or sets the unique name of the solution that was validated.
        /// </summary>
        public string SolutionUniqueName { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the solution passed all validation rules.
        /// </summary>
        public bool IsValid { get; set; }

        /// <summary>
        /// Gets or sets the list of validation issues found.
        /// </summary>
        public List<SolutionValidationIssue> Issues { get; set; } = new List<SolutionValidationIssue>();

        /// <summary>
        /// Gets or sets the total number of components validated.
        /// </summary>
        public int TotalComponents { get; set; }

        /// <summary>
        /// Gets or sets the timestamp when validation was performed.
        /// </summary>
        public DateTime ValidationTimestamp { get; set; }

        /// <summary>
        /// Gets the count of issues by severity.
        /// </summary>
        public int ErrorCount => Issues.Count(i => i.Severity == SolutionValidationSeverity.Error);

        /// <summary>
        /// Gets the count of warnings.
        /// </summary>
        public int WarningCount => Issues.Count(i => i.Severity == SolutionValidationSeverity.Warning);

        /// <summary>
        /// Gets the count of information issues.
        /// </summary>
        public int InfoCount => Issues.Count(i => i.Severity == SolutionValidationSeverity.Info);
    }

    /// <summary>
    /// Represents a single validation issue found during solution validation.
    /// </summary>
    public class SolutionValidationIssue
    {
        /// <summary>
        /// Gets or sets the rule identifier that detected this issue.
        /// </summary>
        public string RuleId { get; set; }

        /// <summary>
        /// Gets or sets the display name of the rule.
        /// </summary>
        public string RuleName { get; set; }

        /// <summary>
        /// Gets or sets the severity of the issue.
        /// </summary>
        public SolutionValidationSeverity Severity { get; set; }

        /// <summary>
        /// Gets or sets the description of the issue.
        /// </summary>
        public string Message { get; set; }

        /// <summary>
        /// Gets or sets the component that has the issue.
        /// </summary>
        public string ComponentIdentifier { get; set; }

        /// <summary>
        /// Gets or sets the component type.
        /// </summary>
        public int ComponentType { get; set; }

        /// <summary>
        /// Gets or sets the component type name.
        /// </summary>
        public string ComponentTypeName { get; set; }

        /// <summary>
        /// Gets or sets the URL to documentation explaining this rule.
        /// </summary>
        public string DocumentationUrl { get; set; }

        /// <summary>
        /// Gets or sets additional details about the issue.
        /// </summary>
        public Dictionary<string, object> Details { get; set; } = new Dictionary<string, object>();
    }

    /// <summary>
    /// Defines the severity levels for validation issues.
    /// </summary>
    public enum SolutionValidationSeverity
    {
        /// <summary>
        /// Informational message only.
        /// </summary>
        Info = 0,

        /// <summary>
        /// Warning that should be reviewed but may not prevent deployment.
        /// </summary>
        Warning = 1,

        /// <summary>
        /// Error that should be fixed before deploying the solution.
        /// </summary>
        Error = 2
    }

}
