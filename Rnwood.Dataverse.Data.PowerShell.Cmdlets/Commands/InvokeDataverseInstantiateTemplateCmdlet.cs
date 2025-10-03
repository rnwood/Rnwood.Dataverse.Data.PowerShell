using System;
using System.Management.Automation;
using Microsoft.Crm.Sdk.Messages;
using Microsoft.PowerPlatform.Dataverse.Client;
using Microsoft.Xrm.Sdk;

namespace Rnwood.Dataverse.Data.PowerShell.Commands
{
    [Cmdlet(VerbsLifecycle.Invoke, "DataverseInstantiateTemplate")]
    [OutputType(typeof(InstantiateTemplateResponse))]
    ///<summary>Executes InstantiateTemplateRequest SDK message.</summary>
    public class InvokeDataverseInstantiateTemplateCmdlet : OrganizationServiceCmdlet
    {
        [Parameter(Mandatory = true, HelpMessage = "DataverseConnection instance obtained from Get-DataverseConnection cmdlet")]
        public override ServiceClient Connection { get; set; }
        [Parameter(Mandatory = false, HelpMessage = "TemplateId parameter")]
        public Guid TemplateId { get; set; }
        [Parameter(Mandatory = false, HelpMessage = "ObjectType parameter")]
        public String ObjectType { get; set; }
        [Parameter(Mandatory = false, HelpMessage = "ObjectId parameter")]
        public Guid ObjectId { get; set; }

        protected override void ProcessRecord()
        {
            base.ProcessRecord();

            var request = new InstantiateTemplateRequest();
            request.TemplateId = TemplateId;            request.ObjectType = ObjectType;            request.ObjectId = ObjectId;
            var response = (InstantiateTemplateResponse)Connection.Execute(request);
            WriteObject(response);
        }
    }
}
