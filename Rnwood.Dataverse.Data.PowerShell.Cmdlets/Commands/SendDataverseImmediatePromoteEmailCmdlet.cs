using System;
using System.Management.Automation;
using Microsoft.Crm.Sdk.Messages;
using Microsoft.PowerPlatform.Dataverse.Client;
using Microsoft.Xrm.Sdk;

namespace Rnwood.Dataverse.Data.PowerShell.Commands
{
    [Cmdlet(VerbsCommunications.Send, "DataverseImmediatePromoteEmail", SupportsShouldProcess = true, ConfirmImpact = ConfirmImpact.Medium)]
    [OutputType(typeof(DeliverImmediatePromoteEmailResponse))]
    ///<summary>Executes DeliverImmediatePromoteEmailRequest SDK message.</summary>
    public class SendDataverseImmediatePromoteEmailCmdlet : OrganizationServiceCmdlet
    {
        [Parameter(Mandatory = true, HelpMessage = "DataverseConnection instance obtained from Get-DataverseConnection cmdlet")]
        public override ServiceClient Connection { get; set; }
        [Parameter(Mandatory = false, HelpMessage = "EmailId parameter")]
        public Guid EmailId { get; set; }
        [Parameter(Mandatory = false, HelpMessage = "MessageId parameter")]
        public String MessageId { get; set; }
        [Parameter(Mandatory = false, HelpMessage = "Subject parameter")]
        public String Subject { get; set; }
        [Parameter(Mandatory = false, HelpMessage = "From parameter")]
        public String From { get; set; }
        [Parameter(Mandatory = false, HelpMessage = "To parameter")]
        public String To { get; set; }
        [Parameter(Mandatory = false, HelpMessage = "Cc parameter")]
        public String Cc { get; set; }
        [Parameter(Mandatory = false, HelpMessage = "Bcc parameter")]
        public String Bcc { get; set; }
        [Parameter(Mandatory = false, HelpMessage = "ReceivedOn parameter")]
        public DateTime ReceivedOn { get; set; }
        [Parameter(Mandatory = false, HelpMessage = "SubmittedBy parameter")]
        public String SubmittedBy { get; set; }
        [Parameter(Mandatory = false, HelpMessage = "Importance parameter")]
        public String Importance { get; set; }
        [Parameter(Mandatory = false, HelpMessage = "Body parameter")]
        public String Body { get; set; }
        [Parameter(Mandatory = false, HelpMessage = "AttachmentIds parameter")]
        public String[] AttachmentIds { get; set; }
        [Parameter(Mandatory = false, HelpMessage = "EWSUrl parameter")]
        public String EWSUrl { get; set; }
        [Parameter(Mandatory = false, HelpMessage = "AttachmentToken parameter")]
        public String AttachmentToken { get; set; }
        [Parameter(Mandatory = false, HelpMessage = "ExtraProperties parameter")]
        public PSObject ExtraProperties { get; set; }

        protected override void ProcessRecord()
        {
            base.ProcessRecord();

            var request = new DeliverImmediatePromoteEmailRequest();
            request.EmailId = EmailId;            request.MessageId = MessageId;            request.Subject = Subject;            request.From = From;            request.To = To;            request.Cc = Cc;            request.Bcc = Bcc;            request.ReceivedOn = ReceivedOn;            request.SubmittedBy = SubmittedBy;            request.Importance = Importance;            request.Body = Body;            request.AttachmentIds = AttachmentIds;            request.EWSUrl = EWSUrl;            request.AttachmentToken = AttachmentToken;            if (ExtraProperties != null)
            {
                var entity = new Entity();
                foreach (PSPropertyInfo prop in ExtraProperties.Properties)
                {
                    if (prop.Name != "Id" && prop.Name != "TableName" && prop.Name != "LogicalName")
                    {
                        entity[prop.Name] = prop.Value;
                    }
                }
                request.ExtraProperties = entity;
            }

            if (ShouldProcess("Executing DeliverImmediatePromoteEmailRequest", "DeliverImmediatePromoteEmailRequest"))
            {
                var response = (DeliverImmediatePromoteEmailResponse)Connection.Execute(request);
                WriteObject(response);
            }
        }
    }
}
