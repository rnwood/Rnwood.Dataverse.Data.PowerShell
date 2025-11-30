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
    /// Removes a solution component from an unmanaged solution.
    /// </summary>
    [Cmdlet(VerbsCommon.Remove, "DataverseSolutionComponent", SupportsShouldProcess = true, ConfirmImpact = ConfirmImpact.Medium)]
    public class RemoveDataverseSolutionComponentCmdlet : OrganizationServiceCmdlet
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
        /// If specified, the cmdlet will not raise an error if the component does not exist in the solution.
        /// </summary>
        [Parameter(HelpMessage = "If specified, the cmdlet will not raise an error if the component does not exist in the solution.")]
        public SwitchParameter IfExists { get; set; }

        /// <summary>
        /// Processes the cmdlet request.
        /// </summary>
        protected override void ProcessRecord()
        {
            base.ProcessRecord();

            // Get solution ID from solution name
            Guid solutionId = GetSolutionId(SolutionName);
            WriteVerbose($"Solution '{SolutionName}' has ID: {solutionId}");

            // Check if component exists in solution
            var existingComponent = GetExistingComponent(solutionId, ComponentId, ComponentType);

            if (existingComponent == null)
            {
                if (IfExists.IsPresent)
                {
                    WriteVerbose($"Component {ComponentId} (type {ComponentType}) does not exist in solution '{SolutionName}'. No action taken.");
                    return;
                }
                else
                {
                    ThrowTerminatingError(new ErrorRecord(
                        new Exception($"Component {ComponentId} (type {ComponentType}) not found in solution '{SolutionName}'."),
                        "ComponentNotFound",
                        ErrorCategory.ObjectNotFound,
                        ComponentId));
                }
            }

            // Remove component
            if (ShouldProcess($"Component {ComponentId} (type {ComponentType})", $"Remove from solution '{SolutionName}'"))
            {
                RemoveComponent(ComponentId, ComponentType);
                WriteVerbose("Component removed successfully.");
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

            var result = QueryHelpers.RetrieveMultipleWithThrottlingRetry(Connection, query);
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
                ColumnSet = new ColumnSet("solutioncomponentid", "objectid", "componenttype"),
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

            var result = QueryHelpers.RetrieveMultipleWithThrottlingRetry(Connection, query);
            return result.Entities.FirstOrDefault();
        }

        private void RemoveComponent(Guid componentId, int componentType)
        {
            var request = new RemoveSolutionComponentRequest
            {
                SolutionUniqueName = SolutionName,
                ComponentId = componentId,
                ComponentType = componentType
            };

            QueryHelpers.ExecuteWithThrottlingRetry(Connection, request);
        }
    }
}
