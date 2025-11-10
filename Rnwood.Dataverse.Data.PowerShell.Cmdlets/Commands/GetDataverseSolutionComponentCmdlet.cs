using System;
using System.Collections.Generic;
using System.Management.Automation;
using Microsoft.PowerPlatform.Dataverse.Client;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Messages;
using Microsoft.Xrm.Sdk.Metadata;
using Microsoft.Xrm.Sdk.Metadata.Query;
using Microsoft.Xrm.Sdk.Query;
using Rnwood.Dataverse.Data.PowerShell.Commands.Model;

namespace Rnwood.Dataverse.Data.PowerShell.Commands
{
    /// <summary>
    /// Retrieves the root components from a solution in a Dataverse environment.
    /// </summary>
    [Cmdlet(VerbsCommon.Get, "DataverseSolutionComponent")]
    [OutputType(typeof(PSObject))]
    public class GetDataverseSolutionComponentCmdlet : OrganizationServiceCmdlet
    {
        /// <summary>
        /// Gets or sets the solution's unique name.
        /// </summary>
        [Parameter(Mandatory = true, Position = 0, ValueFromPipeline = true, ParameterSetName = "ByUniqueName", HelpMessage = "The unique name of the solution.")]
        [ValidateNotNullOrEmpty]
        public string SolutionName { get; set; }

        /// <summary>
        /// Gets or sets the solution ID.
        /// </summary>
        [Parameter(Mandatory = true, Position = 0, ValueFromPipeline = true, ParameterSetName = "BySolutionId", HelpMessage = "The ID of the solution.")]
        [ValidateNotNullOrEmpty]
        public Guid SolutionId { get; set; }

        /// <summary>
        /// Gets or sets whether to include subcomponents in the output.
        /// </summary>
        [Parameter(Mandatory = false, HelpMessage = "Include subcomponents (attributes, relationships, forms, views, etc.) for each root component.")]
        public SwitchParameter IncludeSubcomponents { get; set; }

        /// <summary>
        /// Processes the cmdlet request.
        /// </summary>
        protected override void ProcessRecord()
        {
            base.ProcessRecord();

            Guid solutionid;

            if (ParameterSetName == "ByUniqueName")
            {
                var query = new QueryExpression("solution")
                {
                    ColumnSet = new ColumnSet("solutionid"),
                    Criteria = new FilterExpression
                    {
                        Conditions =
                        {
                            new ConditionExpression("uniquename", ConditionOperator.Equal, SolutionName)
                        }
                    }
                };
                var result = Connection.RetrieveMultiple(query);
                if (result.Entities.Count == 0)
                {
                    ThrowTerminatingError(new ErrorRecord(new Exception($"Solution '{SolutionName}' not found."), "SolutionNotFound", ErrorCategory.ObjectNotFound, SolutionName));
                }
                solutionid = result.Entities[0].GetAttributeValue<Guid>("solutionid");
            }
            else
            {
                solutionid = SolutionId;
            }

            // Extract components from the environment
            var extractor = new EnvironmentComponentExtractor(Connection, this, solutionid);
            var components = extractor.GetComponents(IncludeSubcomponents.IsPresent);

            WriteVerbose($"Found {components.Count} components in the solution.");

            // Output components
            foreach (var component in components)
            {
                OutputComponentAsObject(component);
            }
        }

        private void OutputComponentAsObject(SolutionComponent component)
        {
            var result = new PSObject();
            var displayIdentifier = ComponentTypeResolver.GetComponentIdentifier(component, Connection);
            result.Properties.Add(new PSNoteProperty("ObjectId", displayIdentifier));
            result.Properties.Add(new PSNoteProperty("ComponentType", component.ComponentType));
            result.Properties.Add(new PSNoteProperty("ComponentTypeName", ComponentTypeResolver.GetComponentTypeName(Connection, component)));
            
            var behaviorEnum = RootComponentBehaviorExtensions.FromInt(component.RootComponentBehavior);
            result.Properties.Add(new PSNoteProperty("Behavior", behaviorEnum));
            
            result.Properties.Add(new PSNoteProperty("IsSubcomponent", component.IsSubcomponent));
            result.Properties.Add(new PSNoteProperty("IsDefault", component.IsDefault));
            result.Properties.Add(new PSNoteProperty("IsCustom", component.IsDefault));
            result.Properties.Add(new PSNoteProperty("IsCustomized", component.IsCustomized));
            result.Properties.Add(new PSNoteProperty("IsManaged", component.IsManaged));

            if (component.IsSubcomponent)
            {
                result.Properties.Add(new PSNoteProperty("ParentComponentType", component.ParentComponentType));
                result.Properties.Add(new PSNoteProperty("ParentComponentTypeName", ComponentTypeResolver.GetComponentTypeName(Connection, new SolutionComponent { ComponentType = component.ParentComponentType.GetValueOrDefault() })));
                result.Properties.Add(new PSNoteProperty("ParentTableName", component.ParentTableName));
                result.Properties.Add(new PSNoteProperty("ParentIsDefault", component.ParentIsDefault));
                result.Properties.Add(new PSNoteProperty("ParentIsCustom", component.ParentIsCustom));
                result.Properties.Add(new PSNoteProperty("ParentIsCustomized", component.ParentIsCustomized));
                result.Properties.Add(new PSNoteProperty("ParentIsManaged", component.ParentIsManaged));
            }

            WriteObject(result);
        }
    }
}
