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

    /// <summary>
    /// Defines the validation rules for Dataverse solutions.
    /// </summary>
    public static class SolutionValidationRules
    {
        /// <summary>
        /// Rule: Managed table components should not be in the solution with "include subcomponents".
        /// </summary>
        public const string ManagedTableIncludeSubcomponents = "SV001";

        /// <summary>
        /// Rule: Managed non-table components should only be in the solution if they are customized.
        /// </summary>
        public const string ManagedNonTableNotCustomized = "SV002";

        /// <summary>
        /// Rule: Table managed subcomponents should only be in the solution if they are customized.
        /// </summary>
        public const string ManagedSubcomponentNotCustomized = "SV003";

        /// <summary>
        /// Gets the documentation URL for a specific rule.
        /// </summary>
        /// <param name="ruleId">The rule identifier.</param>
        /// <returns>The URL to the documentation for the rule.</returns>
        public static string GetDocumentationUrl(string ruleId)
        {
            string baseUrl = "https://github.com/rnwood/Rnwood.Dataverse.Data.PowerShell/blob/main/docs/solution-validation-rules";
            return $"{baseUrl}/{ruleId}.md";
        }

        /// <summary>
        /// Gets the display name for a rule.
        /// </summary>
        /// <param name="ruleId">The rule identifier.</param>
        /// <returns>The display name of the rule.</returns>
        public static string GetRuleName(string ruleId)
        {
            switch (ruleId)
            {
                case ManagedTableIncludeSubcomponents:
                    return "Managed Table Include Subcomponents";
                case ManagedNonTableNotCustomized:
                    return "Managed Non-Table Not Customized";
                case ManagedSubcomponentNotCustomized:
                    return "Managed Subcomponent Not Customized";
                default:
                    return "Unknown Rule";
            }
        }

        /// <summary>
        /// Gets the description for a rule.
        /// </summary>
        /// <param name="ruleId">The rule identifier.</param>
        /// <returns>The description of the rule.</returns>
        public static string GetRuleDescription(string ruleId)
        {
            switch (ruleId)
            {
                case ManagedTableIncludeSubcomponents:
                    return "Managed table components should not be included with 'Include Subcomponents' behavior as this can cause issues when importing the solution into environments where the managed table has been customized.";
                case ManagedNonTableNotCustomized:
                    return "Managed non-table components should only be included in the solution if they have been customized. Including unmodified managed components can bloat the solution and cause upgrade issues.";
                case ManagedSubcomponentNotCustomized:
                    return "Managed table subcomponents (attributes, relationships, forms, views, etc.) should only be included if they have been customized. Including unmodified managed subcomponents is unnecessary and can cause issues.";
                default:
                    return "No description available.";
            }
        }
    }
}
