using System;
using System.Linq;
using System.Management.Automation;
using Microsoft.Crm.Sdk.Messages;
using Microsoft.PowerPlatform.Dataverse.Client;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Query;

namespace Rnwood.Dataverse.Data.PowerShell.Commands
{
    /// <summary>
    /// Adds or updates a solution component in an unmanaged solution.
    /// If the component already exists with a different behavior, it is removed and re-added with the new behavior.
    /// </summary>
    [Cmdlet(VerbsCommon.Set, "DataverseSolutionComponent", SupportsShouldProcess = true, ConfirmImpact = ConfirmImpact.Medium)]
    [OutputType(typeof(PSObject))]
    public class SetDataverseSolutionComponentCmdlet : OrganizationServiceCmdlet
    {
        /// <summary>
        /// Gets or sets the solution's unique name.
        /// </summary>
        [Parameter(Mandatory = true, Position = 0, HelpMessage = "The unique name of the solution.")]
        [ValidateNotNullOrEmpty]
        public string SolutionName { get; set; }

        /// <summary>
        /// Gets or sets the ID of the solution component.
        /// </summary>
        [Parameter(Mandatory = true, Position = 1, ValueFromPipelineByPropertyName = true, HelpMessage = "The ID of the solution component.")]
        [Alias("ObjectId")]
        public Guid ComponentId { get; set; }

        /// <summary>
        /// Gets or sets the value that represents the solution component type.
        /// </summary>
        [Parameter(Mandatory = true, Position = 2, ValueFromPipelineByPropertyName = true, HelpMessage = "The value that represents the solution component type.")]
        public int ComponentType { get; set; }

        /// <summary>
        /// Gets or sets the root component behavior. 
        /// 0 = Include Subcomponents (default)
        /// 1 = Do Not Include Subcomponents
        /// 2 = Include As Shell
        /// </summary>
        [Parameter(Mandatory = false, HelpMessage = "The root component behavior. 0 = Include Subcomponents (default), 1 = Do Not Include Subcomponents, 2 = Include As Shell")]
        [ValidateRange(0, 2)]
        public int Behavior { get; set; } = 0;

        /// <summary>
        /// Gets or sets a value that indicates whether other solution components that are required by the solution component should also be added to the unmanaged solution.
        /// </summary>
        [Parameter(HelpMessage = "Indicates whether other solution components that are required by the solution component should also be added to the unmanaged solution.")]
        public SwitchParameter AddRequiredComponents { get; set; }

        /// <summary>
        /// Indicates whether the subcomponents should be included.
        /// </summary>
        [Parameter(HelpMessage = "Indicates whether the subcomponents should be included.")]
        public SwitchParameter DoNotIncludeSubcomponents { get; set; }

        /// <summary>
        /// Gets or sets a value that specifies if the component is added to the solution with its metadata.
        /// </summary>
        [Parameter(HelpMessage = "Specifies if the component is added to the solution with its metadata.")]
        public string[] IncludedComponentSettingsValues { get; set; }

        /// <summary>
        /// If specified, the InputObject with component details is written to the pipeline.
        /// </summary>
        [Parameter(HelpMessage = "If specified, outputs the component information to the pipeline.")]
        public SwitchParameter PassThru { get; set; }

        /// <summary>
        /// Processes the cmdlet request.
        /// </summary>
        protected override void ProcessRecord()
        {
            base.ProcessRecord();

            // Get solution ID from solution name
            Guid solutionId = GetSolutionId(SolutionName);
            WriteVerbose($"Solution '{SolutionName}' has ID: {solutionId}");

            // Check if component already exists in solution
            var existingComponent = GetExistingComponent(solutionId, ComponentId, ComponentType);

            if (existingComponent != null)
            {
                var existingBehavior = existingComponent.Contains("rootcomponentbehavior")
                    ? existingComponent.GetAttributeValue<OptionSetValue>("rootcomponentbehavior")?.Value ?? 0
                    : 0;

                WriteVerbose($"Component already exists in solution with behavior: {GetBehaviorName(existingBehavior)}");

                if (existingBehavior == Behavior)
                {
                    WriteVerbose($"Component behavior is unchanged. No action needed.");
                    OutputComponentInfo(solutionId, ComponentId, ComponentType, Behavior, wasUpdated: false);
                    return;
                }

                // Behavior has changed - need to remove and re-add
                WriteVerbose($"Component behavior changed from {GetBehaviorName(existingBehavior)} to {GetBehaviorName(Behavior)}. Removing and re-adding component.");

                if (ShouldProcess($"Component {ComponentId} (type {ComponentType})", "Remove from solution to change behavior"))
                {
                    RemoveComponent(solutionId, ComponentId, ComponentType);
                    WriteVerbose("Component removed successfully.");
                }
                else
                {
                    return;
                }
            }
            else
            {
                WriteVerbose($"Component does not exist in solution. Adding component with behavior: {GetBehaviorName(Behavior)}");
            }

            // Add component (either new or re-adding after removal)
            if (ShouldProcess($"Component {ComponentId} (type {ComponentType})", $"Add to solution '{SolutionName}' with behavior: {GetBehaviorName(Behavior)}"))
            {
                AddComponent(solutionId, ComponentId, ComponentType, Behavior);
                WriteVerbose("Component added successfully.");
                OutputComponentInfo(solutionId, ComponentId, ComponentType, Behavior, wasUpdated: existingComponent != null);
            }
        }

        private Guid GetSolutionId(string solutionName)
        {
            var query = new QueryExpression("solution")
            {
                ColumnSet = new ColumnSet("solutionid"),
                Criteria = new FilterExpression
                {
                    Conditions =
                    {
                        new ConditionExpression("uniquename", ConditionOperator.Equal, solutionName)
                    }
                },
                TopCount = 1
            };

            var result = Connection.RetrieveMultiple(query);
            if (result.Entities.Count == 0)
            {
                ThrowTerminatingError(new ErrorRecord(
                    new Exception($"Solution '{solutionName}' not found."),
                    "SolutionNotFound",
                    ErrorCategory.ObjectNotFound,
                    solutionName));
            }

            return result.Entities[0].GetAttributeValue<Guid>("solutionid");
        }

        private Entity GetExistingComponent(Guid solutionId, Guid componentId, int componentType)
        {
            var query = new QueryExpression("solutioncomponent")
            {
                ColumnSet = new ColumnSet("solutioncomponentid", "objectid", "componenttype", "rootcomponentbehavior"),
                Criteria = new FilterExpression
                {
                    Conditions =
                    {
                        new ConditionExpression("solutionid", ConditionOperator.Equal, solutionId),
                        new ConditionExpression("objectid", ConditionOperator.Equal, componentId),
                        new ConditionExpression("componenttype", ConditionOperator.Equal, componentType)
                    }
                },
                TopCount = 1
            };

            var result = Connection.RetrieveMultiple(query);
            return result.Entities.FirstOrDefault();
        }

        private void RemoveComponent(Guid solutionId, Guid componentId, int componentType)
        {
            var request = new RemoveSolutionComponentRequest
            {
                SolutionUniqueName = SolutionName,
                ComponentId = componentId,
                ComponentType = componentType
            };

            Connection.Execute(request);
        }

        private void AddComponent(Guid solutionId, Guid componentId, int componentType, int behavior)
        {
            var request = new AddSolutionComponentRequest
            {
                SolutionUniqueName = SolutionName,
                ComponentId = componentId,
                ComponentType = componentType,
                AddRequiredComponents = AddRequiredComponents.IsPresent,
                DoNotIncludeSubcomponents = DoNotIncludeSubcomponents.IsPresent,
                IncludedComponentSettingsValues = IncludedComponentSettingsValues
            };

            Connection.Execute(request);
        }

        private void OutputComponentInfo(Guid solutionId, Guid componentId, int componentType, int behavior, bool wasUpdated)
        {
            if (PassThru.IsPresent)
            {
                var result = new PSObject();
                result.Properties.Add(new PSNoteProperty("SolutionName", SolutionName));
                result.Properties.Add(new PSNoteProperty("SolutionId", solutionId));
                result.Properties.Add(new PSNoteProperty("ComponentId", componentId));
                result.Properties.Add(new PSNoteProperty("ComponentType", componentType));
                result.Properties.Add(new PSNoteProperty("Behavior", GetBehaviorName(behavior)));
                result.Properties.Add(new PSNoteProperty("BehaviorValue", behavior));
                result.Properties.Add(new PSNoteProperty("WasUpdated", wasUpdated));
                WriteObject(result);
            }
        }

        private string GetBehaviorName(int behavior)
        {
            switch (behavior)
            {
                case 0: return "Include Subcomponents";
                case 1: return "Do Not Include Subcomponents";
                case 2: return "Include As Shell";
                default: return $"Unknown ({behavior})";
            }
        }
    }
}
