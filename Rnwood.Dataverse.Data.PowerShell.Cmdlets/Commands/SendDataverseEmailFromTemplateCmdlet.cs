using System;
using System.Management.Automation;
using Microsoft.Crm.Sdk.Messages;
using Microsoft.PowerPlatform.Dataverse.Client;
using Microsoft.Xrm.Sdk;

namespace Rnwood.Dataverse.Data.PowerShell.Commands
{
    [Cmdlet(VerbsCommunications.Send, "DataverseEmailFromTemplate", SupportsShouldProcess = true, ConfirmImpact = ConfirmImpact.Medium)]
    [OutputType(typeof(SendEmailFromTemplateResponse))]
    ///<summary>Executes SendEmailFromTemplateRequest SDK message.</summary>
    public class SendDataverseEmailFromTemplateCmdlet : OrganizationServiceCmdlet
    {
        [Parameter(Mandatory = true, HelpMessage = "DataverseConnection instance obtained from Get-DataverseConnection cmdlet")]
        public override ServiceClient Connection { get; set; }
        [Parameter(Mandatory = false, HelpMessage = "TemplateId parameter")]
        public Guid TemplateId { get; set; }
        [Parameter(Mandatory = false, HelpMessage = "RegardingType parameter")]
        public String RegardingType { get; set; }
        [Parameter(Mandatory = false, HelpMessage = "RegardingId parameter")]
        public Guid RegardingId { get; set; }
        [Parameter(Mandatory = false, HelpMessage = "Target parameter")]
        public PSObject Target { get; set; }

        protected override void ProcessRecord()
        {
            base.ProcessRecord();

            var request = new SendEmailFromTemplateRequest();
            request.TemplateId = TemplateId;            request.RegardingType = RegardingType;            request.RegardingId = RegardingId;            if (Target != null)
            {
                var entity = new Entity();
                foreach (PSPropertyInfo prop in Target.Properties)
                {
                    if (prop.Name != "Id" && prop.Name != "TableName" && prop.Name != "LogicalName")
                    {
                        entity[prop.Name] = prop.Value;
                    }
                }
                request.Target = entity;
            }

            if (ShouldProcess("Executing SendEmailFromTemplateRequest", "SendEmailFromTemplateRequest"))
            {
                var response = (SendEmailFromTemplateResponse)Connection.Execute(request);
                WriteObject(response);
            }
        }
    }
}
