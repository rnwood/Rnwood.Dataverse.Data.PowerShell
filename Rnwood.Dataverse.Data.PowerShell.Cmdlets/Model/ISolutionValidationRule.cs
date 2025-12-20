using System.Collections.Generic;

namespace Rnwood.Dataverse.Data.PowerShell.Commands.Model
{
    /// <summary>
    /// Defines the interface for a solution validation rule.
    /// </summary>
    public interface ISolutionValidationRule
    {
        /// <summary>
        /// Gets the unique identifier for this rule.
        /// </summary>
        string RuleId { get; }

        /// <summary>
        /// Gets the display name for this rule.
        /// </summary>
        string RuleName { get; }

        /// <summary>
        /// Gets the description of what this rule checks.
        /// </summary>
        string Description { get; }

        /// <summary>
        /// Gets the severity level of violations detected by this rule.
        /// </summary>
        SolutionValidationSeverity Severity { get; }

        /// <summary>
        /// Gets the URL to documentation explaining this rule.
        /// </summary>
        string DocumentationUrl { get; }

        /// <summary>
        /// Validates the solution components and returns any issues found.
        /// </summary>
        /// <param name="components">The solution components to validate.</param>
        /// <param name="context">Context information for validation.</param>
        /// <returns>A list of validation issues found by this rule.</returns>
        List<SolutionValidationIssue> Validate(List<SolutionComponent> components, SolutionValidationContext context);
    }

    /// <summary>
    /// Provides context information for solution validation.
    /// </summary>
    public class SolutionValidationContext
    {
        /// <summary>
        /// Gets or sets the solution unique name being validated.
        /// </summary>
        public string SolutionUniqueName { get; set; }

        /// <summary>
        /// Gets or sets the solution ID being validated.
        /// </summary>
        public System.Guid SolutionId { get; set; }

        /// <summary>
        /// Gets or sets a callback for writing verbose messages.
        /// </summary>
        public System.Action<string> WriteVerbose { get; set; }
    }
}
