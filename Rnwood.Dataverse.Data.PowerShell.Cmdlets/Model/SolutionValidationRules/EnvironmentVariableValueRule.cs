using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.PowerPlatform.Dataverse.Client;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Query;

namespace Rnwood.Dataverse.Data.PowerShell.Commands.Model.SolutionValidationRules
{
    /// <summary>
    /// Rule SV007: Solution should not include environment variable values.
    /// Environment variable values are environment-specific and should be configured
    /// in each environment, not transported via solutions.
    /// </summary>
    public class EnvironmentVariableValueRule : ISolutionValidationRule
    {
        private readonly ServiceClient _connection;

        /// <summary>
        /// Initializes a new instance of the EnvironmentVariableValueRule class.
        /// </summary>
        /// <param name="connection">The Dataverse connection.</param>
        public EnvironmentVariableValueRule(ServiceClient connection)
        {
            _connection = connection ?? throw new ArgumentNullException(nameof(connection));
        }

        /// <inheritdoc/>
        public string RuleId => "SV007";

        /// <inheritdoc/>
        public string RuleName => "Environment Variable Value Included";

        /// <inheritdoc/>
        public string Description => "Solution should not include environment variable values. Environment variable values are environment-specific and should be configured in each environment, not transported via solutions.";

        /// <inheritdoc/>
        public SolutionValidationSeverity Severity => SolutionValidationSeverity.Error;

        /// <inheritdoc/>
        public string DocumentationUrl => "https://github.com/rnwood/Rnwood.Dataverse.Data.PowerShell/blob/main/docs/solution-validation-rules/SV007.md";

        /// <inheritdoc/>
        public List<SolutionValidationIssue> Validate(List<SolutionComponent> components, SolutionValidationContext context)
        {
            context.WriteVerbose?.Invoke($"Validating Rule {RuleId}: {RuleName}...");

            var issues = new List<SolutionValidationIssue>();

            // Component type 380 is Environment Variable Value (environmentvariablevalue)
            const int EnvironmentVariableValueComponentType = 380;

            // Find all environment variable value components
            var envVarValueComponents = components
                .Where(c => c.ComponentType == EnvironmentVariableValueComponentType)
                .ToList();

            if (envVarValueComponents.Count == 0)
            {
                context.WriteVerbose?.Invoke($"Rule {RuleId}: No environment variable values found");
                return issues;
            }

            context.WriteVerbose?.Invoke($"Rule {RuleId}: Found {envVarValueComponents.Count} environment variable value(s)");

            // Get details for each environment variable value
            foreach (var component in envVarValueComponents)
            {
                if (!component.ObjectId.HasValue)
                {
                    continue;
                }

                var envVarValueInfo = GetEnvironmentVariableValueInfo(component.ObjectId.Value, context);
                
                if (envVarValueInfo != null)
                {
                    var issue = new SolutionValidationIssue
                    {
                        RuleId = RuleId,
                        RuleName = RuleName,
                        Severity = Severity,
                        ComponentIdentifier = envVarValueInfo.SchemaName ?? component.ObjectId?.ToString(),
                        ComponentType = component.ComponentType,
                        ComponentTypeName = "Environment Variable Value",
                        Message = $"Environment variable value '{envVarValueInfo.SchemaName}' (for definition '{envVarValueInfo.DefinitionSchemaName}') should not be included in the solution. Environment variable values are environment-specific and should be configured in each target environment, not transported via solutions.",
                        DocumentationUrl = DocumentationUrl
                    };

                    issue.Details["EnvironmentVariableValueId"] = component.ObjectId.Value;
                    issue.Details["EnvironmentVariableDefinitionId"] = envVarValueInfo.DefinitionId;
                    issue.Details["EnvironmentVariableDefinitionSchemaName"] = envVarValueInfo.DefinitionSchemaName;
                    issue.Details["Value"] = envVarValueInfo.Value;

                    issues.Add(issue);
                }
            }

            context.WriteVerbose?.Invoke($"Rule {RuleId}: Found {issues.Count} violation(s)");

            return issues;
        }

        /// <summary>
        /// Gets information about an environment variable value.
        /// </summary>
        private EnvironmentVariableValueInfo GetEnvironmentVariableValueInfo(Guid valueId, SolutionValidationContext context)
        {
            try
            {
                // Query environmentvariablevalue and join to environmentvariabledefinition
                var query = new QueryExpression("environmentvariablevalue")
                {
                    ColumnSet = new ColumnSet("environmentvariablevalueid", "schemaname", "value", "environmentvariabledefinitionid"),
                    Criteria = new FilterExpression
                    {
                        Conditions =
                        {
                            new ConditionExpression("environmentvariablevalueid", ConditionOperator.Equal, valueId)
                        }
                    },
                    TopCount = 1,
                    LinkEntities =
                    {
                        new LinkEntity
                        {
                            LinkFromEntityName = "environmentvariablevalue",
                            LinkFromAttributeName = "environmentvariabledefinitionid",
                            LinkToEntityName = "environmentvariabledefinition",
                            LinkToAttributeName = "environmentvariabledefinitionid",
                            Columns = new ColumnSet("schemaname", "displayname"),
                            EntityAlias = "definition",
                            JoinOperator = JoinOperator.LeftOuter
                        }
                    }
                };

                var results = _connection.RetrieveMultiple(query);
                if (results.Entities.Count > 0)
                {
                    var entity = results.Entities[0];
                    return new EnvironmentVariableValueInfo
                    {
                        ValueId = entity.Id,
                        SchemaName = entity.GetAttributeValue<string>("schemaname"),
                        Value = entity.GetAttributeValue<string>("value"),
                        DefinitionId = entity.GetAttributeValue<EntityReference>("environmentvariabledefinitionid")?.Id ?? Guid.Empty,
                        DefinitionSchemaName = entity.GetAttributeValue<AliasedValue>("definition.schemaname")?.Value as string,
                        DefinitionDisplayName = entity.GetAttributeValue<AliasedValue>("definition.displayname")?.Value as string
                    };
                }
            }
            catch (Exception ex)
            {
                context.WriteVerbose?.Invoke($"Rule {RuleId}: Error querying environment variable value {valueId}: {ex.Message}");
            }

            return null;
        }

        private class EnvironmentVariableValueInfo
        {
            public Guid ValueId { get; set; }
            public string SchemaName { get; set; }
            public string Value { get; set; }
            public Guid DefinitionId { get; set; }
            public string DefinitionSchemaName { get; set; }
            public string DefinitionDisplayName { get; set; }
        }
    }
}
