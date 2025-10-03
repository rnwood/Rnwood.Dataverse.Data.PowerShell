using System;
using System.Management.Automation;
using Microsoft.Crm.Sdk.Messages;
using Microsoft.PowerPlatform.Dataverse.Client;

namespace Rnwood.Dataverse.Data.PowerShell.Commands
{
    [Cmdlet(VerbsLifecycle.Invoke, "DataverseWorkflow")]
    [OutputType(typeof(ExecuteWorkflowResponse))]
    ///<summary>Executes a workflow against a specific record.</summary>
    public class InvokeDataverseWorkflowCmdlet : OrganizationServiceCmdlet
    {
        [Parameter(Mandatory = true, HelpMessage = "DataverseConnection instance obtained from Get-DataverseConnection cmdlet")]
        public override ServiceClient Connection { get; set; }

        [Parameter(Mandatory = true, ValueFromPipelineByPropertyName = true, HelpMessage = "ID of the record to execute the workflow against")]
        public Guid EntityId { get; set; }

        [Parameter(Mandatory = true, HelpMessage = "ID of the workflow to execute")]
        public Guid WorkflowId { get; set; }

        [Parameter(Mandatory = false, HelpMessage = "Optional input arguments for the workflow as a hashtable")]
        public System.Collections.Hashtable InputArguments { get; set; }

        protected override void ProcessRecord()
        {
            base.ProcessRecord();

            ExecuteWorkflowRequest request = new ExecuteWorkflowRequest
            {
                EntityId = EntityId,
                WorkflowId = WorkflowId
            };

            // Add input arguments if provided
            if (InputArguments != null && InputArguments.Count > 0)
            {
                request.InputArguments = new InputArgumentCollection();
                foreach (System.Collections.DictionaryEntry entry in InputArguments)
                {
                    request.InputArguments.Add((string)entry.Key, entry.Value);
                }
            }

            ExecuteWorkflowResponse response = (ExecuteWorkflowResponse)Connection.Execute(request);
            WriteObject(response);
        }
    }
}
