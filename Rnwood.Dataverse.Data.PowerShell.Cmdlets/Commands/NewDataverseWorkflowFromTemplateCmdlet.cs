using System;
using System.Management.Automation;
using Microsoft.Crm.Sdk.Messages;
using Microsoft.PowerPlatform.Dataverse.Client;
using Microsoft.Xrm.Sdk;

namespace Rnwood.Dataverse.Data.PowerShell.Commands
{
    [Cmdlet(VerbsCommon.New, "DataverseWorkflowFromTemplate", SupportsShouldProcess = true, ConfirmImpact = ConfirmImpact.Medium)]
    [OutputType(typeof(CreateWorkflowFromTemplateResponse))]
    ///<summary>Executes CreateWorkflowFromTemplateRequest SDK message.</summary>
    public class NewDataverseWorkflowFromTemplateCmdlet : OrganizationServiceCmdlet
    {
        [Parameter(Mandatory = true, HelpMessage = "DataverseConnection instance obtained from Get-DataverseConnection cmdlet")]
        public override ServiceClient Connection { get; set; }
        [Parameter(Mandatory = false, HelpMessage = "WorkflowName parameter")]
        public String WorkflowName { get; set; }
        [Parameter(Mandatory = false, HelpMessage = "WorkflowTemplateId parameter")]
        public Guid WorkflowTemplateId { get; set; }

        protected override void ProcessRecord()
        {
            base.ProcessRecord();

            var request = new CreateWorkflowFromTemplateRequest();
            request.WorkflowName = WorkflowName;            request.WorkflowTemplateId = WorkflowTemplateId;
            if (ShouldProcess("Executing CreateWorkflowFromTemplateRequest", "CreateWorkflowFromTemplateRequest"))
            {
                var response = (CreateWorkflowFromTemplateResponse)Connection.Execute(request);
                WriteObject(response);
            }
        }
    }
}
