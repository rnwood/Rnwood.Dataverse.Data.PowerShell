using System;
using System.Collections;
using System.Collections.Generic;
using System.Management.Automation;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Query;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Rnwood.Dataverse.Data.PowerShell.Commands
{
    /// <summary>
    /// Updates an action within a cloud flow in Dataverse.
    /// </summary>
    [Cmdlet(VerbsCommon.Set, "DataverseCloudFlowAction", SupportsShouldProcess = true, ConfirmImpact = ConfirmImpact.Medium)]
    public class SetDataverseCloudFlowActionCmdlet : OrganizationServiceCmdlet
    {
        /// <summary>
        /// Gets or sets the ID of the cloud flow containing the action.
        /// </summary>
        [Parameter(Mandatory = true, ParameterSetName = "ById", HelpMessage = "The ID of the cloud flow containing the action.", ValueFromPipelineByPropertyName = true)]
        [ValidateNotNullOrEmpty]
        public Guid FlowId { get; set; }

        /// <summary>
        /// Gets or sets the name of the cloud flow containing the action.
        /// </summary>
        [Parameter(Mandatory = true, ParameterSetName = "ByName", HelpMessage = "The name of the cloud flow containing the action.")]
        [ValidateNotNullOrEmpty]
        public string FlowName { get; set; }

        /// <summary>
        /// Gets or sets the ID/name of the action to update.
        /// </summary>
        [Parameter(Mandatory = true, Position = 1, HelpMessage = "The ID/name of the action to update.")]
        [ValidateNotNullOrEmpty]
        public string ActionId { get; set; }

        /// <summary>
        /// Gets or sets the new inputs for the action.
        /// </summary>
        [Parameter(HelpMessage = "The new inputs for the action as a hashtable or JSON string.")]
        public object Inputs { get; set; }

        /// <summary>
        /// Gets or sets the new description for the action.
        /// </summary>
        [Parameter(HelpMessage = "The new description for the action.")]
        public string Description { get; set; }

        /// <summary>
        /// Processes the cmdlet request.
        /// </summary>
        protected override void ProcessRecord()
        {
            base.ProcessRecord();

            Guid flowId = FlowId;
            string flowName = FlowName;

            // If searching by name, resolve to ID
            if (ParameterSetName == "ByName")
            {
                WriteVerbose($"Querying for cloud flow '{FlowName}'...");

                var query = new QueryExpression("workflow")
                {
                    ColumnSet = new ColumnSet("workflowid", "name"),
                    Criteria = new FilterExpression()
                };

                query.Criteria.AddCondition("name", ConditionOperator.Equal, FlowName);
                query.Criteria.AddCondition("category", ConditionOperator.Equal, 5);
                query.Criteria.AddCondition("type", ConditionOperator.Equal, 1);
                query.TopCount = 1;

                var flows = Connection.RetrieveMultiple(query);

                if (flows.Entities.Count == 0)
                {
                    ThrowTerminatingError(new ErrorRecord(
                        new InvalidOperationException($"Cloud flow '{FlowName}' not found."),
                        "FlowNotFound",
                        ErrorCategory.ObjectNotFound,
                        FlowName));
                    return;
                }

                flowId = flows.Entities[0].Id;
                flowName = flows.Entities[0].GetAttributeValue<string>("name");
                WriteVerbose($"Found cloud flow '{flowName}' with ID: {flowId}");
            }
            else
            {
                // Retrieve flow name
                try
                {
                    var flow = Connection.Retrieve("workflow", flowId, new ColumnSet("name"));
                    flowName = flow.GetAttributeValue<string>("name");
                }
                catch
                {
                    flowName = flowId.ToString();
                }
            }

            if (!ShouldProcess($"Action '{ActionId}' in cloud flow '{flowName}'", "Update"))
            {
                return;
            }

            WriteVerbose($"Retrieving cloud flow definition for '{flowName}'...");

            // Retrieve the flow with clientdata
            Entity flowEntity;
            try
            {
                flowEntity = Connection.Retrieve("workflow", flowId, new ColumnSet("clientdata", "name"));
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

            var clientData = flowEntity.GetAttributeValue<string>("clientdata");

            if (string.IsNullOrEmpty(clientData))
            {
                ThrowTerminatingError(new ErrorRecord(
                    new InvalidOperationException($"Cloud flow '{flowName}' has no client data."),
                    "NoClientData",
                    ErrorCategory.InvalidData,
                    flowId));
                return;
            }

            WriteVerbose("Parsing flow definition...");

            // Parse the JSON
            JObject flowDefinition;
            try
            {
                flowDefinition = JObject.Parse(clientData);
            }
            catch (JsonException ex)
            {
                ThrowTerminatingError(new ErrorRecord(
                    new InvalidOperationException($"Failed to parse cloud flow definition: {ex.Message}", ex),
                    "ParseError",
                    ErrorCategory.ParserError,
                    clientData));
                return;
            }

            // Find the action
            var actionPath = $"properties.definition.actions.{ActionId}";
            var actionToken = flowDefinition.SelectToken(actionPath);

            if (actionToken == null || actionToken.Type != JTokenType.Object)
            {
                ThrowTerminatingError(new ErrorRecord(
                    new InvalidOperationException($"Action '{ActionId}' not found in cloud flow '{flowName}'."),
                    "ActionNotFound",
                    ErrorCategory.ObjectNotFound,
                    ActionId));
                return;
            }

            var actionObj = (JObject)actionToken;
            bool hasUpdates = false;

            // Update inputs if provided
            if (Inputs != null)
            {
                WriteVerbose("Updating action inputs...");
                JToken inputsToken;

                if (Inputs is string inputsString)
                {
                    // Parse JSON string
                    try
                    {
                        inputsToken = JToken.Parse(inputsString);
                    }
                    catch (JsonException ex)
                    {
                        ThrowTerminatingError(new ErrorRecord(
                            new ArgumentException($"Failed to parse Inputs as JSON: {ex.Message}", ex),
                            "InvalidInputsJson",
                            ErrorCategory.InvalidArgument,
                            Inputs));
                        return;
                    }
                }
                else if (Inputs is Hashtable inputsHashtable)
                {
                    // Convert hashtable to JSON
                    inputsToken = JToken.FromObject(inputsHashtable);
                }
                else if (Inputs is PSObject inputsPSObject)
                {
                    // Convert PSObject to hashtable then to JSON
                    var hashtable = new Hashtable();
                    foreach (var property in inputsPSObject.Properties)
                    {
                        hashtable[property.Name] = property.Value;
                    }
                    inputsToken = JToken.FromObject(hashtable);
                }
                else
                {
                    // Try to convert directly
                    try
                    {
                        inputsToken = JToken.FromObject(Inputs);
                    }
                    catch (Exception ex)
                    {
                        ThrowTerminatingError(new ErrorRecord(
                            new ArgumentException($"Failed to convert Inputs to JSON: {ex.Message}", ex),
                            "InvalidInputsType",
                            ErrorCategory.InvalidArgument,
                            Inputs));
                        return;
                    }
                }

                actionObj["inputs"] = inputsToken;
                hasUpdates = true;
            }

            // Update description if provided
            if (!string.IsNullOrEmpty(Description))
            {
                WriteVerbose($"Updating action description to: {Description}");

                // Ensure metadata object exists
                if (actionObj["metadata"] == null || actionObj["metadata"].Type != JTokenType.Object)
                {
                    actionObj["metadata"] = new JObject();
                }

                ((JObject)actionObj["metadata"])["description"] = Description;
                hasUpdates = true;
            }

            if (!hasUpdates)
            {
                WriteWarning("No updates to apply. Please specify at least one property to update (Inputs or Description).");
                return;
            }

            // Save the updated definition back
            WriteVerbose("Saving updated flow definition...");

            var updateEntity = new Entity("workflow", flowId);
            updateEntity["clientdata"] = flowDefinition.ToString(Formatting.None);

            try
            {
                Connection.Update(updateEntity);
                WriteVerbose("Action updated successfully.");
                WriteObject($"Action '{ActionId}' in cloud flow '{flowName}' updated successfully.");
            }
            catch (Exception ex)
            {
                ThrowTerminatingError(new ErrorRecord(
                    new InvalidOperationException($"Failed to update action: {ex.Message}", ex),
                    "UpdateFailed",
                    ErrorCategory.OperationStopped,
                    ActionId));
            }
        }
    }
}
