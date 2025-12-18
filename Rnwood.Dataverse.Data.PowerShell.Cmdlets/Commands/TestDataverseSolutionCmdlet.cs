using System;
using System.Collections.Generic;
using System.Linq;
using System.Management.Automation;
using Microsoft.PowerPlatform.Dataverse.Client;
using Microsoft.Xrm.Sdk.Query;
using Rnwood.Dataverse.Data.PowerShell.Commands.Model;

namespace Rnwood.Dataverse.Data.PowerShell.Commands
{
    /// <summary>
    /// Tests a Dataverse solution for common issues and best practices.
    /// </summary>
    [Cmdlet(VerbsDiagnostic.Test, "DataverseSolution", SupportsShouldProcess = true)]
    [OutputType(typeof(SolutionValidationResult))]
    public class TestDataverseSolutionCmdlet : OrganizationServiceCmdlet
    {
        /// <summary>
        /// Gets or sets the unique name of the solution to validate.
        /// </summary>
        [Parameter(Mandatory = true, Position = 0, ValueFromPipeline = true, ValueFromPipelineByPropertyName = true, HelpMessage = "The unique name of the solution to validate.")]
        [ValidateNotNullOrEmpty]
        [Alias("SolutionName")]
        public string UniqueName { get; set; }

        /// <summary>
        /// Gets or sets whether to include informational messages in the output.
        /// </summary>
        [Parameter(HelpMessage = "Include informational messages in the validation output.")]
        public SwitchParameter IncludeInfo { get; set; }

        /// <summary>
        /// Processes the cmdlet request.
        /// </summary>
        protected override void ProcessRecord()
        {
            base.ProcessRecord();

            if (!ShouldProcess(UniqueName, "Validate solution"))
            {
                return;
            }

            WriteVerbose($"Starting validation of solution '{UniqueName}'...");

            // Create validation result
            var result = new SolutionValidationResult
            {
                SolutionUniqueName = UniqueName,
                ValidationTimestamp = DateTime.UtcNow,
                IsValid = true
            };

            try
            {
                // Get solution ID
                Guid solutionId = GetSolutionId(UniqueName);
                WriteVerbose($"Found solution with ID: {solutionId}");

                // Extract components from the solution
                var extractor = new EnvironmentComponentExtractor(Connection, this, solutionId);
                var components = extractor.GetComponents(includeImpliedSubcomponents: true);
                result.TotalComponents = components.Count;

                WriteVerbose($"Found {components.Count} components in solution");

                // Run validation rules
                ValidateRule1_ManagedTablesWithIncludeSubcomponents(components, result);
                ValidateRule2_ManagedNonTableComponentsNotCustomized(components, result);
                ValidateRule3_ManagedSubcomponentsNotCustomized(components, result);

                // Determine if solution is valid
                result.IsValid = result.ErrorCount == 0;

                WriteVerbose($"Validation complete. Found {result.ErrorCount} errors, {result.WarningCount} warnings, {result.InfoCount} info messages");

                // Output result
                WriteObject(result);

                // Write summary to verbose stream
                WriteSummary(result);
            }
            catch (Exception ex)
            {
                WriteError(new ErrorRecord(ex, "ValidationFailed", ErrorCategory.InvalidOperation, UniqueName));
            }
        }

        /// <summary>
        /// Gets the solution ID from the unique name.
        /// </summary>
        private Guid GetSolutionId(string uniqueName)
        {
            var query = new QueryExpression("solution")
            {
                ColumnSet = new ColumnSet("solutionid"),
                Criteria = new FilterExpression
                {
                    Conditions =
                    {
                        new ConditionExpression("uniquename", ConditionOperator.Equal, uniqueName)
                    }
                }
            };

            var results = Connection.RetrieveMultiple(query);
            if (results.Entities.Count == 0)
            {
                throw new InvalidOperationException($"Solution '{uniqueName}' not found.");
            }

            return results.Entities[0].Id;
        }

        /// <summary>
        /// Rule 1: Managed table components should not be in the solution with "include subcomponents".
        /// </summary>
        private void ValidateRule1_ManagedTablesWithIncludeSubcomponents(List<SolutionComponent> components, SolutionValidationResult result)
        {
            WriteVerbose("Validating Rule SV001: Managed tables with 'Include Subcomponents'...");

            const int EntityComponentType = 1; // Entity/Table component type

            var violations = components
                .Where(c => c.ComponentType == EntityComponentType)
                .Where(c => c.IsManaged == true)
                .Where(c => !c.IsSubcomponent)
                .Where(c => c.RootComponentBehavior == (int)RootComponentBehavior.IncludeSubcomponents)
                .ToList();

            foreach (var component in violations)
            {
                var issue = new SolutionValidationIssue
                {
                    RuleId = SolutionValidationRules.ManagedTableIncludeSubcomponents,
                    RuleName = SolutionValidationRules.GetRuleName(SolutionValidationRules.ManagedTableIncludeSubcomponents),
                    Severity = SolutionValidationSeverity.Error,
                    ComponentIdentifier = component.UniqueName ?? component.ObjectId?.ToString(),
                    ComponentType = component.ComponentType,
                    ComponentTypeName = component.ComponentTypeName ?? ComponentTypeResolver.GetComponentTypeNameFallback(component.ComponentType),
                    Message = $"Managed table '{component.UniqueName}' is included with 'Include Subcomponents' behavior. This can cause issues when the managed table has been customized in the target environment.",
                    DocumentationUrl = SolutionValidationRules.GetDocumentationUrl(SolutionValidationRules.ManagedTableIncludeSubcomponents)
                };

                issue.Details["Behavior"] = RootComponentBehaviorExtensions.GetDisplayName(component.RootComponentBehavior);
                issue.Details["IsManaged"] = component.IsManaged;

                result.Issues.Add(issue);
            }

            WriteVerbose($"Rule SV001: Found {violations.Count} violations");
        }

        /// <summary>
        /// Rule 2: Managed non-table components should only be in the solution if they are customized.
        /// </summary>
        private void ValidateRule2_ManagedNonTableComponentsNotCustomized(List<SolutionComponent> components, SolutionValidationResult result)
        {
            WriteVerbose("Validating Rule SV002: Managed non-table components not customized...");

            const int EntityComponentType = 1; // Entity/Table component type

            var violations = components
                .Where(c => c.ComponentType != EntityComponentType)
                .Where(c => c.IsManaged == true)
                .Where(c => !c.IsSubcomponent)
                .Where(c => c.IsCustomized != true)
                .ToList();

            foreach (var component in violations)
            {
                var issue = new SolutionValidationIssue
                {
                    RuleId = SolutionValidationRules.ManagedNonTableNotCustomized,
                    RuleName = SolutionValidationRules.GetRuleName(SolutionValidationRules.ManagedNonTableNotCustomized),
                    Severity = SolutionValidationSeverity.Warning,
                    ComponentIdentifier = component.UniqueName ?? component.ObjectId?.ToString(),
                    ComponentType = component.ComponentType,
                    ComponentTypeName = component.ComponentTypeName ?? ComponentTypeResolver.GetComponentTypeNameFallback(component.ComponentType),
                    Message = $"Managed {component.ComponentTypeName ?? "component"} '{component.UniqueName ?? component.ObjectId?.ToString()}' is not customized but is included in the solution. Including unmodified managed components can bloat the solution.",
                    DocumentationUrl = SolutionValidationRules.GetDocumentationUrl(SolutionValidationRules.ManagedNonTableNotCustomized)
                };

                issue.Details["IsManaged"] = component.IsManaged;
                issue.Details["IsCustomized"] = component.IsCustomized;

                result.Issues.Add(issue);
            }

            WriteVerbose($"Rule SV002: Found {violations.Count} violations");
        }

        /// <summary>
        /// Rule 3: Table managed subcomponents should only be in the solution if they are customized.
        /// </summary>
        private void ValidateRule3_ManagedSubcomponentsNotCustomized(List<SolutionComponent> components, SolutionValidationResult result)
        {
            WriteVerbose("Validating Rule SV003: Managed subcomponents not customized...");

            var violations = components
                .Where(c => c.IsSubcomponent)
                .Where(c => c.IsManaged == true)
                .Where(c => c.IsCustomized != true)
                .ToList();

            foreach (var component in violations)
            {
                var issue = new SolutionValidationIssue
                {
                    RuleId = SolutionValidationRules.ManagedSubcomponentNotCustomized,
                    RuleName = SolutionValidationRules.GetRuleName(SolutionValidationRules.ManagedSubcomponentNotCustomized),
                    Severity = SolutionValidationSeverity.Warning,
                    ComponentIdentifier = component.UniqueName ?? component.ObjectId?.ToString(),
                    ComponentType = component.ComponentType,
                    ComponentTypeName = component.ComponentTypeName ?? ComponentTypeResolver.GetComponentTypeNameFallback(component.ComponentType),
                    Message = $"Managed subcomponent '{component.UniqueName ?? component.ObjectId?.ToString()}' (type: {component.ComponentTypeName ?? "Unknown"}) is not customized but is included in the solution. Including unmodified managed subcomponents is unnecessary.",
                    DocumentationUrl = SolutionValidationRules.GetDocumentationUrl(SolutionValidationRules.ManagedSubcomponentNotCustomized)
                };

                issue.Details["IsManaged"] = component.IsManaged;
                issue.Details["IsCustomized"] = component.IsCustomized;
                issue.Details["IsSubcomponent"] = component.IsSubcomponent;
                issue.Details["ParentTableName"] = component.ParentTableName;
                issue.Details["ParentIsManaged"] = component.ParentIsManaged;

                result.Issues.Add(issue);
            }

            WriteVerbose($"Rule SV003: Found {violations.Count} violations");
        }

        /// <summary>
        /// Writes a summary of the validation results to the verbose stream.
        /// </summary>
        private void WriteSummary(SolutionValidationResult result)
        {
            WriteVerbose("========================================");
            WriteVerbose($"Validation Summary for '{result.SolutionUniqueName}'");
            WriteVerbose("========================================");
            WriteVerbose($"Total Components: {result.TotalComponents}");
            WriteVerbose($"Valid: {result.IsValid}");
            WriteVerbose($"Errors: {result.ErrorCount}");
            WriteVerbose($"Warnings: {result.WarningCount}");
            WriteVerbose($"Info: {result.InfoCount}");

            if (result.Issues.Any())
            {
                WriteVerbose("");
                WriteVerbose("Issues by Rule:");

                var groupedIssues = result.Issues.GroupBy(i => i.RuleId);
                foreach (var group in groupedIssues)
                {
                    WriteVerbose($"  {group.Key} ({SolutionValidationRules.GetRuleName(group.Key)}): {group.Count()} issue(s)");
                }
            }

            WriteVerbose("========================================");
        }
    }
}
