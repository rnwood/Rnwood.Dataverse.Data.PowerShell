using System;
using System.Collections.Generic;
using System.Linq;

namespace Rnwood.Dataverse.Data.PowerShell.Commands.Model
{
    /// <summary>
    /// Represents a suppression for a validation rule.
    /// </summary>
    public class RuleSuppression
    {
        /// <summary>
        /// Gets or sets the rule ID to suppress.
        /// </summary>
        public string RuleId { get; set; }

        /// <summary>
        /// Gets or sets the component identifier to suppress the rule for (null for global suppression).
        /// </summary>
        public string ComponentIdentifier { get; set; }

        /// <summary>
        /// Gets a value indicating whether this is a global suppression (applies to all components).
        /// </summary>
        public bool IsGlobalSuppression => string.IsNullOrEmpty(ComponentIdentifier);

        /// <summary>
        /// Determines if this suppression applies to the given rule and component.
        /// </summary>
        /// <param name="ruleId">The rule ID to check.</param>
        /// <param name="componentIdentifier">The component identifier to check.</param>
        /// <returns>True if the suppression applies, false otherwise.</returns>
        public bool Applies(string ruleId, string componentIdentifier)
        {
            if (!string.Equals(RuleId, ruleId, StringComparison.OrdinalIgnoreCase))
            {
                return false;
            }

            // Global suppression applies to all components
            if (IsGlobalSuppression)
            {
                return true;
            }

            // Specific suppression only applies to matching component
            return string.Equals(ComponentIdentifier, componentIdentifier, StringComparison.OrdinalIgnoreCase);
        }

        /// <summary>
        /// Parses a suppression string in the format "RuleId" or "RuleId:ComponentIdentifier".
        /// </summary>
        /// <param name="suppressionString">The suppression string to parse.</param>
        /// <returns>A RuleSuppression object.</returns>
        public static RuleSuppression Parse(string suppressionString)
        {
            if (string.IsNullOrWhiteSpace(suppressionString))
            {
                throw new ArgumentException("Suppression string cannot be null or empty.", nameof(suppressionString));
            }

            var parts = suppressionString.Split(new[] { ':' }, 2);
            
            return new RuleSuppression
            {
                RuleId = parts[0].Trim(),
                ComponentIdentifier = parts.Length > 1 ? parts[1].Trim() : null
            };
        }

        /// <summary>
        /// Parses multiple suppression strings.
        /// </summary>
        /// <param name="suppressionStrings">The suppression strings to parse.</param>
        /// <returns>A list of RuleSuppression objects.</returns>
        public static List<RuleSuppression> ParseMultiple(string[] suppressionStrings)
        {
            if (suppressionStrings == null || suppressionStrings.Length == 0)
            {
                return new List<RuleSuppression>();
            }

            return suppressionStrings
                .Where(s => !string.IsNullOrWhiteSpace(s))
                .Select(Parse)
                .ToList();
        }
    }
}
