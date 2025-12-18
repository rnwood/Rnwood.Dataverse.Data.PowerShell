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
    /// Rule SV004: Solution components should only depend on components from authorized solutions/publishers.
    /// This helps prevent unintended dependencies and circular dependencies.
    /// </summary>
    public class UnauthorizedDependencyRule : ISolutionValidationRule
    {
        private readonly ServiceClient _connection;
        private readonly List<string> _allowedSolutionPatterns;
        private readonly List<string> _allowedPublisherPatterns;

        /// <summary>
        /// Initializes a new instance of the UnauthorizedDependencyRule class.
        /// </summary>
        /// <param name="connection">The Dataverse connection.</param>
        /// <param name="allowedSolutionPatterns">List of allowed solution name patterns (supports wildcards).</param>
        /// <param name="allowedPublisherPatterns">List of allowed publisher unique name patterns (supports wildcards).</param>
        public UnauthorizedDependencyRule(
            ServiceClient connection,
            List<string> allowedSolutionPatterns = null,
            List<string> allowedPublisherPatterns = null)
        {
            _connection = connection ?? throw new ArgumentNullException(nameof(connection));
            _allowedSolutionPatterns = allowedSolutionPatterns ?? new List<string>();
            _allowedPublisherPatterns = allowedPublisherPatterns ?? new List<string>();
        }

        /// <inheritdoc/>
        public string RuleId => "SV004";

        /// <inheritdoc/>
        public string RuleName => "Unauthorized Dependency";

        /// <inheritdoc/>
        public string Description => "Solution components should only depend on components from authorized solutions or publishers. This helps prevent unintended dependencies and reduces the risk of circular dependencies.";

        /// <inheritdoc/>
        public SolutionValidationSeverity Severity => SolutionValidationSeverity.Error;

        /// <inheritdoc/>
        public string DocumentationUrl => "https://github.com/rnwood/Rnwood.Dataverse.Data.PowerShell/blob/main/docs/solution-validation-rules/SV004.md";

        /// <inheritdoc/>
        public List<SolutionValidationIssue> Validate(List<SolutionComponent> components, SolutionValidationContext context)
        {
            // Skip if no restrictions configured
            if (_allowedSolutionPatterns.Count == 0 && _allowedPublisherPatterns.Count == 0)
            {
                context.WriteVerbose?.Invoke($"Rule {RuleId}: Skipped (no allowed solutions/publishers configured)");
                return new List<SolutionValidationIssue>();
            }

            context.WriteVerbose?.Invoke($"Validating Rule {RuleId}: {RuleName}...");
            context.WriteVerbose?.Invoke($"  Allowed solution patterns: {string.Join(", ", _allowedSolutionPatterns)}");
            context.WriteVerbose?.Invoke($"  Allowed publisher patterns: {string.Join(", ", _allowedPublisherPatterns)}");

            var issues = new List<SolutionValidationIssue>();

            // Query dependencies for the solution components
            var componentIds = components
                .Where(c => c.ObjectId.HasValue)
                .Select(c => c.ObjectId.Value)
                .Distinct()
                .ToList();

            if (componentIds.Count == 0)
            {
                context.WriteVerbose?.Invoke($"Rule {RuleId}: No components with IDs to validate");
                return issues;
            }

            // Query dependencies (required components) for our solution components
            var dependencies = QueryComponentDependencies(componentIds, context);

            // For each dependency, check if it's from an allowed solution/publisher
            foreach (var dependency in dependencies)
            {
                var dependencySolution = GetComponentSolutionInfo(dependency.RequiredComponentId, context);
                if (dependencySolution == null)
                {
                    continue; // System component or unable to determine
                }

                // Check if the dependency is authorized
                bool isAuthorized = IsAuthorizedDependency(dependencySolution, context);

                if (!isAuthorized)
                {
                    var dependentComponent = components.FirstOrDefault(c => 
                        c.ObjectId.HasValue && c.ObjectId.Value == dependency.DependentComponentId);

                    if (dependentComponent != null)
                    {
                        var issue = new SolutionValidationIssue
                        {
                            RuleId = RuleId,
                            RuleName = RuleName,
                            Severity = Severity,
                            ComponentIdentifier = dependentComponent.UniqueName ?? dependentComponent.ObjectId?.ToString(),
                            ComponentType = dependentComponent.ComponentType,
                            ComponentTypeName = dependentComponent.ComponentTypeName ?? ComponentTypeResolver.GetComponentTypeNameFallback(dependentComponent.ComponentType),
                            Message = $"Component '{dependentComponent.UniqueName ?? dependentComponent.ObjectId?.ToString()}' depends on component from unauthorized solution '{dependencySolution.SolutionName}' (publisher: '{dependencySolution.PublisherName}'). Only dependencies on components from allowed solutions/publishers are permitted.",
                            DocumentationUrl = DocumentationUrl
                        };

                        issue.Details["DependencySolution"] = dependencySolution.SolutionName;
                        issue.Details["DependencyPublisher"] = dependencySolution.PublisherName;
                        issue.Details["DependencyPublisherUniqueName"] = dependencySolution.PublisherUniqueName;
                        issue.Details["RequiredComponentId"] = dependency.RequiredComponentId;

                        issues.Add(issue);
                    }
                }
            }

            context.WriteVerbose?.Invoke($"Rule {RuleId}: Found {issues.Count} violation(s)");

            return issues;
        }

        /// <summary>
        /// Queries component dependencies from Dataverse.
        /// </summary>
        private List<DependencyInfo> QueryComponentDependencies(List<Guid> componentIds, SolutionValidationContext context)
        {
            var dependencies = new List<DependencyInfo>();

            // Query in batches to avoid query limits
            const int batchSize = 100;
            for (int i = 0; i < componentIds.Count; i += batchSize)
            {
                var batch = componentIds.Skip(i).Take(batchSize).ToList();

                var query = new QueryExpression("dependency")
                {
                    ColumnSet = new ColumnSet("dependentcomponentobjectid", "requiredcomponentobjectid"),
                    Criteria = new FilterExpression
                    {
                        Conditions =
                        {
                            new ConditionExpression("dependentcomponentobjectid", ConditionOperator.In, batch.Cast<object>().ToArray())
                        }
                    }
                };

                try
                {
                    var results = _connection.RetrieveMultiple(query);
                    foreach (var entity in results.Entities)
                    {
                        dependencies.Add(new DependencyInfo
                        {
                            DependentComponentId = entity.GetAttributeValue<Guid>("dependentcomponentobjectid"),
                            RequiredComponentId = entity.GetAttributeValue<Guid>("requiredcomponentobjectid")
                        });
                    }
                }
                catch (Exception ex)
                {
                    context.WriteVerbose?.Invoke($"Rule {RuleId}: Error querying dependencies: {ex.Message}");
                }
            }

            return dependencies;
        }

        /// <summary>
        /// Gets solution and publisher information for a component.
        /// </summary>
        private SolutionInfo GetComponentSolutionInfo(Guid componentId, SolutionValidationContext context)
        {
            try
            {
                // Query solutioncomponent to find which solution this component belongs to
                var query = new QueryExpression("solutioncomponent")
                {
                    ColumnSet = new ColumnSet("solutionid"),
                    Criteria = new FilterExpression
                    {
                        Conditions =
                        {
                            new ConditionExpression("objectid", ConditionOperator.Equal, componentId)
                        }
                    },
                    TopCount = 1,
                    LinkEntities =
                    {
                        new LinkEntity
                        {
                            LinkFromEntityName = "solutioncomponent",
                            LinkFromAttributeName = "solutionid",
                            LinkToEntityName = "solution",
                            LinkToAttributeName = "solutionid",
                            Columns = new ColumnSet("uniquename", "friendlyname", "publisherid"),
                            EntityAlias = "solution",
                            LinkEntities =
                            {
                                new LinkEntity
                                {
                                    LinkFromEntityName = "solution",
                                    LinkFromAttributeName = "publisherid",
                                    LinkToEntityName = "publisher",
                                    LinkToAttributeName = "publisherid",
                                    Columns = new ColumnSet("uniquename", "friendlyname"),
                                    EntityAlias = "publisher"
                                }
                            }
                        }
                    }
                };

                var results = _connection.RetrieveMultiple(query);
                if (results.Entities.Count > 0)
                {
                    var entity = results.Entities[0];
                    return new SolutionInfo
                    {
                        SolutionName = entity.GetAttributeValue<AliasedValue>("solution.uniquename")?.Value as string,
                        SolutionFriendlyName = entity.GetAttributeValue<AliasedValue>("solution.friendlyname")?.Value as string,
                        PublisherUniqueName = entity.GetAttributeValue<AliasedValue>("publisher.uniquename")?.Value as string,
                        PublisherName = entity.GetAttributeValue<AliasedValue>("publisher.friendlyname")?.Value as string
                    };
                }
            }
            catch (Exception ex)
            {
                context.WriteVerbose?.Invoke($"Rule {RuleId}: Error querying solution info for component {componentId}: {ex.Message}");
            }

            return null;
        }

        /// <summary>
        /// Checks if a dependency is authorized based on solution/publisher patterns.
        /// </summary>
        private bool IsAuthorizedDependency(SolutionInfo solutionInfo, SolutionValidationContext context)
        {
            // Check solution patterns
            if (_allowedSolutionPatterns.Any())
            {
                foreach (var pattern in _allowedSolutionPatterns)
                {
                    if (MatchesPattern(solutionInfo.SolutionName, pattern))
                    {
                        return true;
                    }
                }
            }

            // Check publisher patterns
            if (_allowedPublisherPatterns.Any())
            {
                foreach (var pattern in _allowedPublisherPatterns)
                {
                    if (MatchesPattern(solutionInfo.PublisherUniqueName, pattern))
                    {
                        return true;
                    }
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

        private class DependencyInfo
        {
            public Guid DependentComponentId { get; set; }
            public Guid RequiredComponentId { get; set; }
        }

        private class SolutionInfo
        {
            public string SolutionName { get; set; }
            public string SolutionFriendlyName { get; set; }
            public string PublisherUniqueName { get; set; }
            public string PublisherName { get; set; }
        }
    }
}
