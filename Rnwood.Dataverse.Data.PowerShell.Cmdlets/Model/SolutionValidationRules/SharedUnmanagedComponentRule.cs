using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using Microsoft.PowerPlatform.Dataverse.Client;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Query;

namespace Rnwood.Dataverse.Data.PowerShell.Commands.Model.SolutionValidationRules
{
    /// <summary>
    /// Rule SV006: Current solution should not include unmanaged components/subcomponents 
    /// that are already in other unmanaged solutions. This helps prevent component ownership 
    /// conflicts and unintended sharing between solutions.
    /// </summary>
    public class SharedUnmanagedComponentRule : ISolutionValidationRule
    {
        private readonly ServiceClient _connection;
        private readonly List<string> _ignoredSolutionPatterns;

        /// <summary>
        /// Initializes a new instance of the SharedUnmanagedComponentRule class.
        /// </summary>
        /// <param name="connection">The Dataverse connection.</param>
        /// <param name="ignoredSolutionPatterns">List of solution name patterns to ignore (supports wildcards).</param>
        public SharedUnmanagedComponentRule(
            ServiceClient connection,
            List<string> ignoredSolutionPatterns = null)
        {
            _connection = connection ?? throw new ArgumentNullException(nameof(connection));
            _ignoredSolutionPatterns = ignoredSolutionPatterns ?? new List<string>();
        }

        /// <inheritdoc/>
        public string RuleId => "SV006";

        /// <inheritdoc/>
        public string RuleName => "Shared Unmanaged Component";

        /// <inheritdoc/>
        public string Description => "Solution should not include unmanaged components or subcomponents that are already in other unmanaged solutions. Shared unmanaged components can cause ownership conflicts and deployment issues.";

        /// <inheritdoc/>
        public SolutionValidationSeverity Severity => SolutionValidationSeverity.Error;

        /// <inheritdoc/>
        public string DocumentationUrl => "https://github.com/rnwood/Rnwood.Dataverse.Data.PowerShell/blob/main/docs/solution-validation-rules/SV006.md";

        /// <inheritdoc/>
        public List<SolutionValidationIssue> Validate(List<SolutionComponent> components, SolutionValidationContext context)
        {
            context.WriteVerbose?.Invoke($"Validating Rule {RuleId}: {RuleName}...");
            
            if (_ignoredSolutionPatterns.Any())
            {
                context.WriteVerbose?.Invoke($"  Ignored solution patterns: {string.Join(", ", _ignoredSolutionPatterns)}");
            }

            var issues = new List<SolutionValidationIssue>();

            // Filter to only unmanaged components in this solution
            var unmanagedComponents = components
                .Where(c => c.IsManaged == false)
                .Where(c => c.ObjectId.HasValue)
                .ToList();

            if (unmanagedComponents.Count == 0)
            {
                context.WriteVerbose?.Invoke($"Rule {RuleId}: No unmanaged components to validate");
                return issues;
            }

            context.WriteVerbose?.Invoke($"Rule {RuleId}: Checking {unmanagedComponents.Count} unmanaged component(s) for sharing");

            // Check each unmanaged component for presence in other solutions
            foreach (var component in unmanagedComponents)
            {
                var otherSolutions = GetOtherSolutionsContainingComponent(
                    component.ObjectId.Value, 
                    context.SolutionId,
                    context);

                // Filter out system/hidden solutions and ignored solutions
                var conflictingSolutions = otherSolutions
                    .Where(s => !IsSystemOrHiddenSolution(s))
                    .Where(s => !IsIgnoredSolution(s.SolutionName))
                    .ToList();

                if (conflictingSolutions.Any())
                {
                    var solutionNames = string.Join(", ", conflictingSolutions.Select(s => $"'{s.SolutionName}'"));
                    
                    var issue = new SolutionValidationIssue
                    {
                        RuleId = RuleId,
                        RuleName = RuleName,
                        Severity = Severity,
                        ComponentIdentifier = component.UniqueName ?? component.ObjectId?.ToString(),
                        ComponentType = component.ComponentType,
                        ComponentTypeName = component.ComponentTypeName ?? ComponentTypeResolver.GetComponentTypeNameFallback(component.ComponentType),
                        Message = $"Unmanaged {(component.IsSubcomponent ? "subcomponent" : "component")} '{component.UniqueName ?? component.ObjectId?.ToString()}' is shared with other unmanaged solution(s): {solutionNames}. Shared unmanaged components can cause ownership conflicts.",
                        DocumentationUrl = DocumentationUrl
                    };

                    issue.Details["IsSubcomponent"] = component.IsSubcomponent;
                    issue.Details["SharedSolutions"] = conflictingSolutions.Select(s => s.SolutionName).ToList();
                    issue.Details["SharedSolutionCount"] = conflictingSolutions.Count;

                    issues.Add(issue);
                }
            }

            context.WriteVerbose?.Invoke($"Rule {RuleId}: Found {issues.Count} violation(s)");

            return issues;
        }

        /// <summary>
        /// Gets other solutions that contain the specified component.
        /// </summary>
        private List<SolutionInfo> GetOtherSolutionsContainingComponent(
            Guid componentId, 
            Guid currentSolutionId,
            SolutionValidationContext context)
        {
            var solutions = new List<SolutionInfo>();

            try
            {
                // Query solutioncomponent to find all solutions containing this component
                var query = new QueryExpression("solutioncomponent")
                {
                    ColumnSet = new ColumnSet("solutionid"),
                    Criteria = new FilterExpression
                    {
                        Conditions =
                        {
                            new ConditionExpression("objectid", ConditionOperator.Equal, componentId),
                            new ConditionExpression("solutionid", ConditionOperator.NotEqual, currentSolutionId)
                        }
                    },
                    LinkEntities =
                    {
                        new LinkEntity
                        {
                            LinkFromEntityName = "solutioncomponent",
                            LinkFromAttributeName = "solutionid",
                            LinkToEntityName = "solution",
                            LinkToAttributeName = "solutionid",
                            Columns = new ColumnSet("uniquename", "friendlyname", "ismanaged", "isvisible"),
                            EntityAlias = "solution"
                        }
                    }
                };

                var results = _connection.RetrieveMultiple(query);
                foreach (var entity in results.Entities)
                {
                    var solutionInfo = new SolutionInfo
                    {
                        SolutionName = entity.GetAttributeValue<AliasedValue>("solution.uniquename")?.Value as string,
                        SolutionFriendlyName = entity.GetAttributeValue<AliasedValue>("solution.friendlyname")?.Value as string,
                        IsManaged = entity.GetAttributeValue<AliasedValue>("solution.ismanaged")?.Value as bool? ?? false,
                        IsVisible = entity.GetAttributeValue<AliasedValue>("solution.isvisible")?.Value as bool? ?? true
                    };

                    // Only include unmanaged solutions
                    if (!solutionInfo.IsManaged)
                    {
                        solutions.Add(solutionInfo);
                    }
                }
            }
            catch (Exception ex)
            {
                context.WriteVerbose?.Invoke($"Rule {RuleId}: Error querying solutions for component {componentId}: {ex.Message}");
            }

            return solutions;
        }

        /// <summary>
        /// Checks if a solution is a system or hidden solution.
        /// </summary>
        private bool IsSystemOrHiddenSolution(SolutionInfo solution)
        {
            if (solution == null)
            {
                return false;
            }

            // Check if hidden
            if (solution.IsVisible == false)
            {
                return true;
            }

            // Check if system solution by name pattern
            if (string.IsNullOrEmpty(solution.SolutionName))
            {
                return false;
            }

            var systemSolutionPrefixes = new[]
            {
                "System",
                "Basic",
                "msdyn_",
                "msdynce_",
                "msft_",
                "Microsoft",
                "Internal",
                "Default",
                "Active"
            };

            return systemSolutionPrefixes.Any(prefix => 
                solution.SolutionName.StartsWith(prefix, StringComparison.OrdinalIgnoreCase));
        }

        /// <summary>
        /// Checks if a solution should be ignored based on configured patterns.
        /// </summary>
        private bool IsIgnoredSolution(string solutionName)
        {
            if (string.IsNullOrEmpty(solutionName) || !_ignoredSolutionPatterns.Any())
            {
                return false;
            }

            foreach (var pattern in _ignoredSolutionPatterns)
            {
                if (MatchesPattern(solutionName, pattern))
                {
                    return true;
                }
            }

            return false;
        }

        /// <summary>
        /// Checks if a value matches a pattern (supports wildcards * and ?).
        /// </summary>
        private bool MatchesPattern(string value, string pattern)
        {
            if (string.IsNullOrEmpty(value) || string.IsNullOrEmpty(pattern))
            {
                return false;
            }

            // Convert wildcard pattern to regex
            var regexPattern = "^" + Regex.Escape(pattern)
                .Replace("\\*", ".*")
                .Replace("\\?", ".") + "$";

            return Regex.IsMatch(value, regexPattern, RegexOptions.IgnoreCase);
        }

        private class SolutionInfo
        {
            public string SolutionName { get; set; }
            public string SolutionFriendlyName { get; set; }
            public bool IsManaged { get; set; }
            public bool IsVisible { get; set; }
        }
    }
}
