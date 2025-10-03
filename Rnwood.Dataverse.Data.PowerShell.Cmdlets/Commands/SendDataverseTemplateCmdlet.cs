using System;
using System.Management.Automation;
using Microsoft.Crm.Sdk.Messages;
using Microsoft.PowerPlatform.Dataverse.Client;
using Microsoft.Xrm.Sdk;

namespace Rnwood.Dataverse.Data.PowerShell.Commands
{
    [Cmdlet(VerbsCommunications.Send, "DataverseTemplate", SupportsShouldProcess = true, ConfirmImpact = ConfirmImpact.Medium)]
    [OutputType(typeof(SendTemplateResponse))]
    ///<summary>Executes SendTemplateRequest SDK message.</summary>
    public class SendDataverseTemplateCmdlet : OrganizationServiceCmdlet
    {
        [Parameter(Mandatory = true, HelpMessage = "DataverseConnection instance obtained from Get-DataverseConnection cmdlet")]
        public override ServiceClient Connection { get; set; }
        [Parameter(Mandatory = false, HelpMessage = "TemplateId parameter")]
        public Guid TemplateId { get; set; }
        [Parameter(Mandatory = false, ValueFromPipelineByPropertyName = true, HelpMessage = "Sender parameter")]
        public object Sender { get; set; }
        [Parameter(Mandatory = false, HelpMessage = "RecipientType parameter")]
        public String RecipientType { get; set; }
        [Parameter(Mandatory = false, HelpMessage = "RecipientIds parameter")]
        public Guid[] RecipientIds { get; set; }
        [Parameter(Mandatory = false, HelpMessage = "RegardingType parameter")]
        public String RegardingType { get; set; }
        [Parameter(Mandatory = false, HelpMessage = "RegardingId parameter")]
        public Guid RegardingId { get; set; }
        [Parameter(Mandatory = false, HelpMessage = "DeliveryPriorityCode parameter")]
        public object DeliveryPriorityCode { get; set; }

        protected override void ProcessRecord()
        {
            base.ProcessRecord();

            var request = new SendTemplateRequest();
            request.TemplateId = TemplateId;            if (Sender != null)
            {
                request.Sender = DataverseTypeConverter.ToEntityReference(Sender, null, "Sender");
            }
            request.RecipientType = RecipientType;            request.RecipientIds = RecipientIds;            request.RegardingType = RegardingType;            request.RegardingId = RegardingId;            if (DeliveryPriorityCode != null)
            {
                request.DeliveryPriorityCode = DataverseTypeConverter.ToOptionSetValue(DeliveryPriorityCode, "DeliveryPriorityCode");
            }

            if (ShouldProcess("Executing SendTemplateRequest", "SendTemplateRequest"))
            {
                var response = (SendTemplateResponse)Connection.Execute(request);
                WriteObject(response);
            }
        }
    }
}
