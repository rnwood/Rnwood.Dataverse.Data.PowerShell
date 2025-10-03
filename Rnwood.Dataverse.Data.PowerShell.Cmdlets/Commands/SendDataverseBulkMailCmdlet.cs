using System;
using System.Management.Automation;
using Microsoft.Crm.Sdk.Messages;
using Microsoft.PowerPlatform.Dataverse.Client;
using Microsoft.Xrm.Sdk;

namespace Rnwood.Dataverse.Data.PowerShell.Commands
{
    [Cmdlet(VerbsCommunications.Send, "DataverseBulkMail", SupportsShouldProcess = true, ConfirmImpact = ConfirmImpact.Medium)]
    [OutputType(typeof(SendBulkMailResponse))]
    ///<summary>Executes SendBulkMailRequest SDK message.</summary>
    public class SendDataverseBulkMailCmdlet : OrganizationServiceCmdlet
    {
        [Parameter(Mandatory = true, HelpMessage = "DataverseConnection instance obtained from Get-DataverseConnection cmdlet")]
        public override ServiceClient Connection { get; set; }
        [Parameter(Mandatory = false, ValueFromPipelineByPropertyName = true, HelpMessage = "Sender parameter")]
        public object Sender { get; set; }
        [Parameter(Mandatory = false, HelpMessage = "TemplateId parameter")]
        public Guid TemplateId { get; set; }
        [Parameter(Mandatory = false, HelpMessage = "RegardingType parameter")]
        public String RegardingType { get; set; }
        [Parameter(Mandatory = false, HelpMessage = "RegardingId parameter")]
        public Guid RegardingId { get; set; }
        [Parameter(Mandatory = false, HelpMessage = "Query parameter")]
        public Microsoft.Xrm.Sdk.Query.QueryBase Query { get; set; }

        protected override void ProcessRecord()
        {
            base.ProcessRecord();

            var request = new SendBulkMailRequest();
            if (Sender != null)
            {
                request.Sender = DataverseTypeConverter.ToEntityReference(Sender, null, "Sender");
            }
            request.TemplateId = TemplateId;            request.RegardingType = RegardingType;            request.RegardingId = RegardingId;            request.Query = Query;
            if (ShouldProcess("Executing SendBulkMailRequest", "SendBulkMailRequest"))
            {
                var response = (SendBulkMailResponse)Connection.Execute(request);
                WriteObject(response);
            }
        }
    }
}
