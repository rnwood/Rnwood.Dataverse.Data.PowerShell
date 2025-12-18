using System;
using System.Collections.Generic;
using System.Linq;
using System.Management.Automation;
using Microsoft.PowerPlatform.Dataverse.Client;
using Microsoft.Xrm.Sdk.Query;
using Rnwood.Dataverse.Data.PowerShell.Commands.Model;
using Rnwood.Dataverse.Data.PowerShell.Commands.Model.SolutionValidationRules;

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
        /// Gets or sets rule suppressions in format "RuleId" or "RuleId:ComponentIdentifier".
        /// </summary>
        [Parameter(HelpMessage = "Suppress specific rules globally or for specific components. Format: 'RuleId' or 'RuleId:ComponentIdentifier'.")]
        public string[] SuppressRule { get; set; }

        /// <summary>
        /// Gets or sets the minimum severity level at which violations should be output as PowerShell errors.
        /// Useful for CI/CD pipelines to fail builds on validation issues.
        /// </summary>
        [Parameter(HelpMessage = "Output violations at or above this severity level as PowerShell errors. Useful for CI/CD pipelines.")]
        public SolutionValidationSeverity? FailOnSeverity { get; set; }

        /// <summary>
        /// Gets or sets rule severity overrides in format "RuleId:Severity" (e.g., "SV001:Warning").
        /// </summary>
        [Parameter(HelpMessage = "Override the severity of specific rules. Format: 'RuleId:Severity' (e.g., 'SV001:Warning').")]
        public string[] OverrideSeverity { get; set; }

        /// <summary>
        /// Gets or sets allowed solution name patterns for dependency validation.
        /// Supports wildcards (* and ?).
        /// </summary>
        [Parameter(HelpMessage = "Allowed solution name patterns for dependency validation. Supports wildcards (* and ?).")]
        public string[] AllowedDependencySolutions { get; set; }

        /// <summary>
        /// Gets or sets allowed publisher unique name patterns for dependency validation.
        /// Supports wildcards (* and ?).
        /// </summary>
        [Parameter(HelpMessage = "Allowed publisher unique name patterns for dependency validation. Supports wildcards (* and ?).")]
        public string[] AllowedDependencyPublishers { get; set; }

        /// <summary>
        /// Gets or sets solution name patterns to ignore when checking for shared unmanaged components.
        /// Supports wildcards (* and ?).
        /// </summary>
        [Parameter(HelpMessage = "Solution name patterns to ignore when checking for shared unmanaged components. Supports wildcards (* and ?).")]
        public string[] IgnoreSharedComponentSolutions { get; set; }

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

            // Parse suppressions
            var suppressions = RuleSuppression.ParseMultiple(SuppressRule);
            if (suppressions.Any())
            {
                WriteVerbose($"Loaded {suppressions.Count} rule suppression(s)");
                foreach (var suppression in suppressions)
                {
                    if (suppression.IsGlobalSuppression)
                    {
                        WriteVerbose($"  - {suppression.RuleId} (global)");
                    }
                    else
                    {
                        WriteVerbose($"  - {suppression.RuleId}:{suppression.ComponentIdentifier}");
                    }
                }
            }

            // Parse severity overrides
            var severityOverrides = RuleSeverityOverride.ParseMultiple(OverrideSeverity);
            if (severityOverrides.Any())
            {
                WriteVerbose($"Loaded {severityOverrides.Count} severity override(s)");
                foreach (var kvp in severityOverrides)
                {
                    WriteVerbose($"  - {kvp.Key}: {kvp.Value}");
                }
            }

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

                // Create validation context
                var context = new SolutionValidationContext
                {
                    SolutionUniqueName = UniqueName,
                    SolutionId = solutionId,
                    WriteVerbose = WriteVerbose
                };

                // Get all validation rules
                var rules = GetValidationRules();
                WriteVerbose($"Running {rules.Count} validation rule(s)");

                // Run validation rules
                foreach (var rule in rules)
                {
                    var issues = rule.Validate(components, context);
                    
                    // Apply severity overrides
                    if (severityOverrides.ContainsKey(rule.RuleId))
                    {
                        var newSeverity = severityOverrides[rule.RuleId];
                        foreach (var issue in issues)
                        {
                            issue.Severity = newSeverity;
                        }
                        WriteVerbose($"  Applied severity override for rule {rule.RuleId}: {newSeverity}");
                    }
                    
                    // Apply suppressions
                    var unsuppressedIssues = issues.Where(issue => 
                        !suppressions.Any(s => s.Applies(issue.RuleId, issue.ComponentIdentifier))
                    ).ToList();

                    if (issues.Count != unsuppressedIssues.Count)
                    {
                        WriteVerbose($"  Suppressed {issues.Count - unsuppressedIssues.Count} issue(s) for rule {rule.RuleId}");
                    }

                    result.Issues.AddRange(unsuppressedIssues);
                }

                // Determine if solution is valid
                result.IsValid = result.ErrorCount == 0;

                WriteVerbose($"Validation complete. Found {result.ErrorCount} errors, {result.WarningCount} warnings, {result.InfoCount} info messages");

                // Output issues as PowerShell errors if FailOnSeverity is specified
                if (FailOnSeverity.HasValue)
                {
                    var failureIssues = result.Issues
                        .Where(i => i.Severity >= FailOnSeverity.Value)
                        .ToList();

                    foreach (var issue in failureIssues)
                    {
                        var errorMessage = $"[{issue.RuleId}] {issue.Message}";
                        var errorRecord = new ErrorRecord(
                            new InvalidOperationException(errorMessage),
                            issue.RuleId,
                            ErrorCategory.InvalidData,
                            issue.ComponentIdentifier
                        );
                        errorRecord.ErrorDetails = new ErrorDetails($"{errorMessage}\n\nSee: {issue.DocumentationUrl}");
                        WriteError(errorRecord);
                    }

                    if (failureIssues.Any())
                    {
                        WriteVerbose($"Output {failureIssues.Count} issue(s) as PowerShell errors (severity >= {FailOnSeverity.Value})");
                    }
                }

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
        /// Gets the list of validation rules to run.
        /// </summary>
        /// <returns>A list of validation rules.</returns>
        private List<ISolutionValidationRule> GetValidationRules()
        {
            var rules = new List<ISolutionValidationRule>
            {
                new ManagedTableIncludeSubcomponentsRule(),
                new ManagedNonTableNotCustomizedRule(),
                new ManagedSubcomponentNotCustomizedRule()
            };

            // Add unsolutioned dependency validation rule (always runs)
            rules.Add(new UnsolutionedDependencyRule(Connection));

            // Add shared unmanaged component validation rule (always runs)
            var ignoredSolutions = IgnoreSharedComponentSolutions?.ToList() ?? new List<string>();
            rules.Add(new SharedUnmanagedComponentRule(Connection, ignoredSolutions));

            // Add environment variable value validation rule (always runs)
            rules.Add(new EnvironmentVariableValueRule(Connection));

            // Add dependency validation rule if restrictions are configured
            if ((AllowedDependencySolutions != null && AllowedDependencySolutions.Length > 0) ||
                (AllowedDependencyPublishers != null && AllowedDependencyPublishers.Length > 0))
            {
                var allowedSolutions = AllowedDependencySolutions?.ToList() ?? new List<string>();
                var allowedPublishers = AllowedDependencyPublishers?.ToList() ?? new List<string>();
                
                rules.Add(new UnauthorizedDependencyRule(Connection, allowedSolutions, allowedPublishers));
            }

            return rules;
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
                    var firstIssue = group.First();
                    WriteVerbose($"  {group.Key} ({firstIssue.RuleName}): {group.Count()} issue(s)");
                }
            }

            WriteVerbose("========================================");
        }
    }
}
