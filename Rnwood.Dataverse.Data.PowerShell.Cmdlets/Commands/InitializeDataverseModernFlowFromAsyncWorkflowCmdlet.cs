using System;
using System.Management.Automation;
using Microsoft.Crm.Sdk.Messages;
using Microsoft.PowerPlatform.Dataverse.Client;
using Microsoft.Xrm.Sdk;

namespace Rnwood.Dataverse.Data.PowerShell.Commands
{
    [Cmdlet(VerbsData.Initialize, "DataverseModernFlowFromAsyncWorkflow")]
    [OutputType(typeof(InitializeModernFlowFromAsyncWorkflowResponse))]
    ///<summary>Executes InitializeModernFlowFromAsyncWorkflowRequest SDK message.</summary>
    public class InitializeDataverseModernFlowFromAsyncWorkflowCmdlet : OrganizationServiceCmdlet
    {
        [Parameter(Mandatory = true, HelpMessage = "DataverseConnection instance obtained from Get-DataverseConnection cmdlet")]
        public override ServiceClient Connection { get; set; }
        [Parameter(Mandatory = false, HelpMessage = "WorkflowId parameter")]
        public Guid WorkflowId { get; set; }

        protected override void ProcessRecord()
        {
            base.ProcessRecord();

            var request = new InitializeModernFlowFromAsyncWorkflowRequest();
            request.WorkflowId = WorkflowId;
            var response = (InitializeModernFlowFromAsyncWorkflowResponse)Connection.Execute(request);
            WriteObject(response);
        }
    }
}
