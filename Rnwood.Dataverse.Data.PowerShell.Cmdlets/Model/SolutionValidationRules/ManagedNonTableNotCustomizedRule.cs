using System.Collections.Generic;
using System.Linq;

namespace Rnwood.Dataverse.Data.PowerShell.Commands.Model.SolutionValidationRules
{
    /// <summary>
    /// Rule SV002: Managed non-table components should only be in the solution if they are customized.
    /// </summary>
    public class ManagedNonTableNotCustomizedRule : ISolutionValidationRule
    {
        /// <inheritdoc/>
        public string RuleId => "SV002";

        /// <inheritdoc/>
        public string RuleName => "Managed Non-Table Not Customized";

        /// <inheritdoc/>
        public string Description => "Managed non-table components should only be included in the solution if they have been customized. Including unmodified managed components can bloat the solution and cause upgrade issues.";

        /// <inheritdoc/>
        public SolutionValidationSeverity Severity => SolutionValidationSeverity.Warning;

        /// <inheritdoc/>
        public string DocumentationUrl => "https://github.com/rnwood/Rnwood.Dataverse.Data.PowerShell/blob/main/docs/solution-validation-rules/SV002.md";

        /// <inheritdoc/>
        public List<SolutionValidationIssue> Validate(List<SolutionComponent> components, SolutionValidationContext context)
        {
            context.WriteVerbose?.Invoke($"Validating Rule {RuleId}: {RuleName}...");

            const int EntityComponentType = 1; // Entity/Table component type

            var violations = components
                .Where(c => c.ComponentType != EntityComponentType)
                .Where(c => c.IsManaged == true)
                .Where(c => !c.IsSubcomponent)
                .Where(c => c.IsCustomized != true)
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
                    Message = $"Managed {component.ComponentTypeName ?? "component"} '{component.UniqueName ?? component.ObjectId?.ToString()}' is not customized but is included in the solution. Including unmodified managed components can bloat the solution.",
                    DocumentationUrl = DocumentationUrl
                };

                issue.Details["IsManaged"] = component.IsManaged;
                issue.Details["IsCustomized"] = component.IsCustomized;

                issues.Add(issue);
            }

            context.WriteVerbose?.Invoke($"Rule {RuleId}: Found {violations.Count} violation(s)");

            return issues;
        }
    }
}
