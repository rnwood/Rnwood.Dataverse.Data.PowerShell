using System.Collections.Generic;
using System.Linq;

namespace Rnwood.Dataverse.Data.PowerShell.Commands.Model.SolutionValidationRules
{
    /// <summary>
    /// Rule SV003: Table managed subcomponents should only be in the solution if they are customized.
    /// </summary>
    public class ManagedSubcomponentNotCustomizedRule : ISolutionValidationRule
    {
        /// <inheritdoc/>
        public string RuleId => "SV003";

        /// <inheritdoc/>
        public string RuleName => "Managed Subcomponent Not Customized";

        /// <inheritdoc/>
        public string Description => "Managed table subcomponents (attributes, relationships, forms, views, etc.) should only be included if they have been customized. Including unmodified managed subcomponents is unnecessary and can cause issues.";

        /// <inheritdoc/>
        public SolutionValidationSeverity Severity => SolutionValidationSeverity.Warning;

        /// <inheritdoc/>
        public string DocumentationUrl => "https://github.com/rnwood/Rnwood.Dataverse.Data.PowerShell/blob/main/docs/solution-validation-rules/SV003.md";

        /// <inheritdoc/>
        public List<SolutionValidationIssue> Validate(List<SolutionComponent> components, SolutionValidationContext context)
        {
            context.WriteVerbose?.Invoke($"Validating Rule {RuleId}: {RuleName}...");

            var violations = components
                .Where(c => c.IsSubcomponent)
                .Where(c => c.IsManaged == true)
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
                    Message = $"Managed subcomponent '{component.UniqueName ?? component.ObjectId?.ToString()}' (type: {component.ComponentTypeName ?? "Unknown"}) is not customized but is included in the solution. Including unmodified managed subcomponents is unnecessary.",
                    DocumentationUrl = DocumentationUrl
                };

                issue.Details["IsManaged"] = component.IsManaged;
                issue.Details["IsCustomized"] = component.IsCustomized;
                issue.Details["IsSubcomponent"] = component.IsSubcomponent;
                issue.Details["ParentTableName"] = component.ParentTableName;
                issue.Details["ParentIsManaged"] = component.ParentIsManaged;

                issues.Add(issue);
            }

            context.WriteVerbose?.Invoke($"Rule {RuleId}: Found {violations.Count} violation(s)");

            return issues;
        }
    }
}
