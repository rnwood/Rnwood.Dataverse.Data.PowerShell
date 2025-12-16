using System;
using System.Management.Automation;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Query;
using Microsoft.Crm.Sdk.Messages;

namespace Rnwood.Dataverse.Data.PowerShell.Commands
{
    /// <summary>
    /// Updates properties of a cloud flow in Dataverse or changes its state.
    /// </summary>
    [Cmdlet(VerbsCommon.Set, "DataverseCloudFlow", SupportsShouldProcess = true, ConfirmImpact = ConfirmImpact.Medium)]
    public class SetDataverseCloudFlowCmdlet : OrganizationServiceCmdlet
    {
        /// <summary>
        /// Gets or sets the ID of the cloud flow to update.
        /// </summary>
        [Parameter(Mandatory = true, Position = 0, ParameterSetName = "ById", HelpMessage = "The ID of the cloud flow to update.", ValueFromPipelineByPropertyName = true)]
        [ValidateNotNullOrEmpty]
        public Guid Id { get; set; }

        /// <summary>
        /// Gets or sets the name of the cloud flow to update.
        /// </summary>
        [Parameter(Mandatory = true, Position = 0, ParameterSetName = "ByName", HelpMessage = "The name of the cloud flow to update.")]
        [ValidateNotNullOrEmpty]
        public string Name { get; set; }

        /// <summary>
        /// Gets or sets the new display name for the cloud flow.
        /// </summary>
        [Parameter(HelpMessage = "The new display name for the cloud flow.")]
        public string NewName { get; set; }

        /// <summary>
        /// Gets or sets the new description for the cloud flow.
        /// </summary>
        [Parameter(HelpMessage = "The new description for the cloud flow.")]
        public string Description { get; set; }

        /// <summary>
        /// Gets or sets whether to activate the cloud flow.
        /// </summary>
        [Parameter(HelpMessage = "Activate the cloud flow.")]
        public SwitchParameter Activate { get; set; }

        /// <summary>
        /// Gets or sets whether to deactivate the cloud flow.
        /// </summary>
        [Parameter(HelpMessage = "Deactivate the cloud flow (set to draft).")]
        public SwitchParameter Deactivate { get; set; }

        /// <summary>
        /// Processes the cmdlet request.
        /// </summary>
        protected override void ProcessRecord()
        {
            base.ProcessRecord();

            Guid flowId = Id;

            // If searching by name, resolve to ID
            if (ParameterSetName == "ByName")
            {
                WriteVerbose($"Querying for cloud flow '{Name}'...");

                var query = new QueryExpression("workflow")
                {
                    ColumnSet = new ColumnSet("workflowid", "name"),
                    Criteria = new FilterExpression()
                };

                query.Criteria.AddCondition("name", ConditionOperator.Equal, Name);
                query.Criteria.AddCondition("category", ConditionOperator.Equal, 5);
                query.Criteria.AddCondition("type", ConditionOperator.Equal, 1);
                query.TopCount = 1;

                var flows = Connection.RetrieveMultiple(query);

                if (flows.Entities.Count == 0)
                {
                    ThrowTerminatingError(new ErrorRecord(
                        new InvalidOperationException($"Cloud flow '{Name}' not found."),
                        "FlowNotFound",
                        ErrorCategory.ObjectNotFound,
                        Name));
                    return;
                }

                flowId = flows.Entities[0].Id;
                WriteVerbose($"Found cloud flow with ID: {flowId}");
            }

            if (!ShouldProcess($"Cloud flow '{(ParameterSetName == "ByName" ? Name : flowId.ToString())}'", "Update"))
            {
                return;
            }

            // Retrieve current flow to verify it exists
            WriteVerbose($"Retrieving cloud flow {flowId}...");
            Entity flow;
            try
            {
                flow = Connection.Retrieve("workflow", flowId, new ColumnSet("name", "description", "statecode", "statuscode"));
            }
            catch (Exception ex)
            {
                ThrowTerminatingError(new ErrorRecord(
                    new InvalidOperationException($"Cloud flow with ID '{flowId}' not found or could not be retrieved.", ex),
                    "FlowNotFound",
                    ErrorCategory.ObjectNotFound,
                    flowId));
                return;
            }

            var currentName = flow.GetAttributeValue<string>("name");
            var currentDescription = flow.GetAttributeValue<string>("description");
            var currentStateCode = flow.GetAttributeValue<OptionSetValue>("statecode")?.Value ?? 0;

            WriteVerbose($"Found cloud flow: {currentName} (State: {(currentStateCode == 0 ? "Draft" : "Activated")})");

            bool hasUpdates = false;
            var updateEntity = new Entity("workflow", flowId);

            // Update name if provided
            if (!string.IsNullOrEmpty(NewName) && NewName != currentName)
            {
                updateEntity["name"] = NewName;
                hasUpdates = true;
                WriteVerbose($"Setting name to: {NewName}");
            }

            // Update description if provided
            if (!string.IsNullOrEmpty(Description) && Description != currentDescription)
            {
                updateEntity["description"] = Description;
                hasUpdates = true;
                WriteVerbose($"Setting description to: {Description}");
            }

            // Apply updates before state changes
            if (hasUpdates)
            {
                WriteVerbose("Updating cloud flow properties...");
                Connection.Update(updateEntity);
                WriteVerbose("Cloud flow properties updated successfully.");
            }

            // Handle state changes
            if (Activate.IsPresent && Deactivate.IsPresent)
            {
                ThrowTerminatingError(new ErrorRecord(
                    new ArgumentException("Cannot specify both Activate and Deactivate switches."),
                    "InvalidParameters",
                    ErrorCategory.InvalidArgument,
                    null));
                return;
            }

            if (Activate.IsPresent)
            {
                if (currentStateCode == 1)
                {
                    WriteVerbose("Cloud flow is already activated. No state change needed.");
                }
                else
                {
                    WriteVerbose("Activating cloud flow...");
                    var activateRequest = new SetStateRequest
                    {
                        EntityMoniker = new EntityReference("workflow", flowId),
                        State = new OptionSetValue(1),
                        Status = new OptionSetValue(2)
                    };
                    Connection.Execute(activateRequest);
                    WriteVerbose("Cloud flow activated successfully.");
                }
            }
            else if (Deactivate.IsPresent)
            {
                if (currentStateCode == 0)
                {
                    WriteVerbose("Cloud flow is already in draft state. No state change needed.");
                }
                else
                {
                    WriteVerbose("Deactivating cloud flow...");
                    var deactivateRequest = new SetStateRequest
                    {
                        EntityMoniker = new EntityReference("workflow", flowId),
                        State = new OptionSetValue(0),
                        Status = new OptionSetValue(1)
                    };
                    Connection.Execute(deactivateRequest);
                    WriteVerbose("Cloud flow deactivated successfully.");
                }
            }

            if (!hasUpdates && !Activate.IsPresent && !Deactivate.IsPresent)
            {
                WriteWarning("No updates to apply. Please specify at least one property to update (NewName, Description, Activate, or Deactivate).");
                return;
            }

            WriteObject($"Cloud flow '{currentName}' updated successfully.");
        }
    }
}
