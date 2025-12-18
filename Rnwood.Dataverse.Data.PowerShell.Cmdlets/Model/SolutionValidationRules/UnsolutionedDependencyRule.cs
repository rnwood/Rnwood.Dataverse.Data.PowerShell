using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.PowerPlatform.Dataverse.Client;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Query;

namespace Rnwood.Dataverse.Data.PowerShell.Commands.Model.SolutionValidationRules
{
    /// <summary>
    /// Rule SV005: Solution components should not depend on components that are not in any solution,
    /// or are in system/hidden solutions. Such dependencies indicate components that may not be
    /// properly managed or deployed.
    /// </summary>
    public class UnsolutionedDependencyRule : ISolutionValidationRule
    {
        private readonly ServiceClient _connection;

        /// <summary>
        /// Initializes a new instance of the UnsolutionedDependencyRule class.
        /// </summary>
        /// <param name="connection">The Dataverse connection.</param>
        public UnsolutionedDependencyRule(ServiceClient connection)
        {
            _connection = connection ?? throw new ArgumentNullException(nameof(connection));
        }

        /// <inheritdoc/>
        public string RuleId => "SV005";

        /// <inheritdoc/>
        public string RuleName => "Unsolutioned Dependency";

        /// <inheritdoc/>
        public string Description => "Solution components should not depend on components that are not in any solution or are in system/hidden solutions. Such dependencies indicate unmanaged components that may not be properly deployed.";

        /// <inheritdoc/>
        public SolutionValidationSeverity Severity => SolutionValidationSeverity.Error;

        /// <inheritdoc/>
        public string DocumentationUrl => "https://github.com/rnwood/Rnwood.Dataverse.Data.PowerShell/blob/main/docs/solution-validation-rules/SV005.md";

        /// <inheritdoc/>
        public List<SolutionValidationIssue> Validate(List<SolutionComponent> components, SolutionValidationContext context)
        {
            context.WriteVerbose?.Invoke($"Validating Rule {RuleId}: {RuleName}...");

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

            context.WriteVerbose?.Invoke($"Rule {RuleId}: Found {dependencies.Count} dependencies to check");

            // For each dependency, check if it's in a valid solution
            foreach (var dependency in dependencies)
            {
                var dependencySolution = GetComponentSolutionInfo(dependency.RequiredComponentId, context);
                
                // Check if dependency is unsolutioned or in a system/hidden solution
                bool isUnsolutioned = dependencySolution == null;
                bool isSystemOrHidden = dependencySolution != null && 
                    (dependencySolution.IsManaged == true && 
                     (dependencySolution.IsVisible == false || IsSystemSolution(dependencySolution.SolutionName)));

                if (isUnsolutioned || isSystemOrHidden)
                {
                    var dependentComponent = components.FirstOrDefault(c => 
                        c.ObjectId.HasValue && c.ObjectId.Value == dependency.DependentComponentId);

                    if (dependentComponent != null)
                    {
                        string message;
                        if (isUnsolutioned)
                        {
                            message = $"Component '{dependentComponent.UniqueName ?? dependentComponent.ObjectId?.ToString()}' depends on a component (ID: {dependency.RequiredComponentId}) that is not in any solution. Unsolutioned dependencies may not be deployed correctly.";
                        }
                        else
                        {
                            message = $"Component '{dependentComponent.UniqueName ?? dependentComponent.ObjectId?.ToString()}' depends on a component from system/hidden solution '{dependencySolution.SolutionName}'. System solution dependencies should be avoided as they may not be consistently available across environments.";
                        }

                        var issue = new SolutionValidationIssue
                        {
                            RuleId = RuleId,
                            RuleName = RuleName,
                            Severity = Severity,
                            ComponentIdentifier = dependentComponent.UniqueName ?? dependentComponent.ObjectId?.ToString(),
                            ComponentType = dependentComponent.ComponentType,
                            ComponentTypeName = dependentComponent.ComponentTypeName ?? ComponentTypeResolver.GetComponentTypeNameFallback(dependentComponent.ComponentType),
                            Message = message,
                            DocumentationUrl = DocumentationUrl
                        };

                        issue.Details["RequiredComponentId"] = dependency.RequiredComponentId;
                        issue.Details["IsUnsolutioned"] = isUnsolutioned;
                        if (dependencySolution != null)
                        {
                            issue.Details["DependencySolution"] = dependencySolution.SolutionName;
                            issue.Details["IsSystemSolution"] = IsSystemSolution(dependencySolution.SolutionName);
                            issue.Details["IsHidden"] = dependencySolution.IsVisible == false;
                        }

                        issues.Add(issue);
                    }
                }
            }

            context.WriteVerbose?.Invoke($"Rule {RuleId}: Found {issues.Count} violation(s)");

            return issues;
        }

        /// <summary>
        /// Checks if a solution is a system solution based on its unique name.
        /// </summary>
        private bool IsSystemSolution(string solutionName)
        {
            if (string.IsNullOrEmpty(solutionName))
            {
                return false;
            }

            // Common system solution names/prefixes
            var systemSolutionPrefixes = new[]
            {
                "System",
                "Basic",
                "msdyn_",
                "msdynce_",
                "msft_",
                "Microsoft",
                "Internal"
            };

            return systemSolutionPrefixes.Any(prefix => 
                solutionName.StartsWith(prefix, StringComparison.OrdinalIgnoreCase));
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
        /// Gets solution information for a component.
        /// </summary>
        private SolutionInfo GetComponentSolutionInfo(Guid componentId, SolutionValidationContext context)
        {
            try
            {
                // Query solutioncomponent to find which solution this component belongs to
                // Exclude system and hidden solutions
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
                            Columns = new ColumnSet("uniquename", "friendlyname", "ismanaged", "isvisible"),
                            EntityAlias = "solution"
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
                        IsManaged = entity.GetAttributeValue<AliasedValue>("solution.ismanaged")?.Value as bool?,
                        IsVisible = entity.GetAttributeValue<AliasedValue>("solution.isvisible")?.Value as bool?
                    };
                }
            }
            catch (Exception ex)
            {
                context.WriteVerbose?.Invoke($"Rule {RuleId}: Error querying solution info for component {componentId}: {ex.Message}");
            }

            return null;
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
            public bool? IsManaged { get; set; }
            public bool? IsVisible { get; set; }
        }
    }
}
