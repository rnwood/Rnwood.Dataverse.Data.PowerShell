using System.Collections.Generic;
using System.Linq;

namespace Rnwood.Dataverse.Data.PowerShell.Commands.Model.SolutionValidationRules
{
    /// <summary>
    /// Rule SV001: Managed table components should not be in the solution with "include subcomponents".
    /// </summary>
    public class ManagedTableIncludeSubcomponentsRule : ISolutionValidationRule
    {
        /// <inheritdoc/>
        public string RuleId => "SV001";

        /// <inheritdoc/>
        public string RuleName => "Managed Table Include Subcomponents";

        /// <inheritdoc/>
        public string Description => "Managed table components should not be included with 'Include Subcomponents' behavior as this can cause issues when importing the solution into environments where the managed table has been customized.";

        /// <inheritdoc/>
        public SolutionValidationSeverity Severity => SolutionValidationSeverity.Error;

        /// <inheritdoc/>
        public string DocumentationUrl => "https://github.com/rnwood/Rnwood.Dataverse.Data.PowerShell/blob/main/docs/solution-validation-rules/SV001.md";

        /// <inheritdoc/>
        public List<SolutionValidationIssue> Validate(List<SolutionComponent> components, SolutionValidationContext context)
        {
            context.WriteVerbose?.Invoke($"Validating Rule {RuleId}: {RuleName}...");

            const int EntityComponentType = 1; // Entity/Table component type

            var violations = components
                .Where(c => c.ComponentType == EntityComponentType)
                .Where(c => c.IsManaged == true)
                .Where(c => !c.IsSubcomponent)
                .Where(c => c.RootComponentBehavior == (int)RootComponentBehavior.IncludeSubcomponents)
                .ToList();

            var issues = new List<SolutionValidationIssue>();

            foreach (var component in violations)
            {
                var issue = new SolutionValidationIssue
                {
                    RuleId = RuleId,
                    RuleName = RuleName,
                    Severity = Severity,
                    ComponentIdentifier = component.UniqueName ?? component.ObjectId?.ToString(),
                    ComponentType = component.ComponentType,
                    ComponentTypeName = component.ComponentTypeName ?? ComponentTypeResolver.GetComponentTypeNameFallback(component.ComponentType),
                    Message = $"Managed table '{component.UniqueName}' is included with 'Include Subcomponents' behavior. This can cause issues when the managed table has been customized in the target environment.",
                    DocumentationUrl = DocumentationUrl
                };

                issue.Details["Behavior"] = RootComponentBehaviorExtensions.GetDisplayName(component.RootComponentBehavior);
                issue.Details["IsManaged"] = component.IsManaged;

                issues.Add(issue);
            }

            context.WriteVerbose?.Invoke($"Rule {RuleId}: Found {violations.Count} violation(s)");

            return issues;
        }
    }
}
