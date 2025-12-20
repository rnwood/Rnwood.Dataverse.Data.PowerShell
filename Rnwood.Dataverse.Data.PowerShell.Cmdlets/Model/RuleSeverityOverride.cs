using System;
using System.Collections.Generic;
using System.Linq;

namespace Rnwood.Dataverse.Data.PowerShell.Commands.Model
{
    /// <summary>
    /// Represents a severity override for a validation rule.
    /// </summary>
    public class RuleSeverityOverride
    {
        /// <summary>
        /// Gets or sets the rule ID to override.
        /// </summary>
        public string RuleId { get; set; }

        /// <summary>
        /// Gets or sets the new severity level.
        /// </summary>
        public SolutionValidationSeverity Severity { get; set; }

        /// <summary>
        /// Parses a severity override string in the format "RuleId:Severity" (e.g., "SV001:Warning").
        /// </summary>
        /// <param name="overrideString">The override string to parse.</param>
        /// <returns>A RuleSeverityOverride object.</returns>
        public static RuleSeverityOverride Parse(string overrideString)
        {
            if (string.IsNullOrWhiteSpace(overrideString))
            {
                throw new ArgumentException("Override string cannot be null or empty.", nameof(overrideString));
            }

            var parts = overrideString.Split(new[] { ':' }, 2);
            if (parts.Length != 2)
            {
                throw new ArgumentException($"Invalid override format: '{overrideString}'. Expected format: 'RuleId:Severity' (e.g., 'SV001:Warning').", nameof(overrideString));
            }

            var ruleId = parts[0].Trim();
            var severityStr = parts[1].Trim();

            if (!Enum.TryParse<SolutionValidationSeverity>(severityStr, true, out var severity))
            {
                throw new ArgumentException($"Invalid severity value: '{severityStr}'. Valid values are: Info, Warning, Error.", nameof(overrideString));
            }

            return new RuleSeverityOverride
            {
                RuleId = ruleId,
                Severity = severity
            };
        }

        /// <summary>
        /// Parses multiple severity override strings.
        /// </summary>
        /// <param name="overrideStrings">The override strings to parse.</param>
        /// <returns>A dictionary of rule IDs to severity overrides.</returns>
        public static Dictionary<string, SolutionValidationSeverity> ParseMultiple(string[] overrideStrings)
        {
            if (overrideStrings == null || overrideStrings.Length == 0)
            {
                return new Dictionary<string, SolutionValidationSeverity>();
            }

            return overrideStrings
                .Where(s => !string.IsNullOrWhiteSpace(s))
                .Select(Parse)
                .ToDictionary(o => o.RuleId, o => o.Severity, StringComparer.OrdinalIgnoreCase);
        }
    }
}
