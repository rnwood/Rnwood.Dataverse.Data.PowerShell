using System;
using System.Management.Automation;
using Microsoft.Crm.Sdk.Messages;
using Microsoft.PowerPlatform.Dataverse.Client;
using Microsoft.Xrm.Sdk;

namespace Rnwood.Dataverse.Data.PowerShell.Commands
{
    [Cmdlet(VerbsCommunications.Send, "DataverseIncomingEmail", SupportsShouldProcess = true, ConfirmImpact = ConfirmImpact.Medium)]
    [OutputType(typeof(DeliverIncomingEmailResponse))]
    ///<summary>Executes DeliverIncomingEmailRequest SDK message.</summary>
    public class SendDataverseIncomingEmailCmdlet : OrganizationServiceCmdlet
    {
        [Parameter(Mandatory = true, HelpMessage = "DataverseConnection instance obtained from Get-DataverseConnection cmdlet")]
        public override ServiceClient Connection { get; set; }
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
        [Parameter(Mandatory = false, HelpMessage = "Attachments parameter")]
        public Microsoft.Xrm.Sdk.EntityCollection Attachments { get; set; }
        [Parameter(Mandatory = false, HelpMessage = "ExtraProperties parameter")]
        public PSObject ExtraProperties { get; set; }
        [Parameter(Mandatory = false, HelpMessage = "ValidateBeforeCreate parameter")]
        public Boolean ValidateBeforeCreate { get; set; }

        protected override void ProcessRecord()
        {
            base.ProcessRecord();

            var request = new DeliverIncomingEmailRequest();
            request.MessageId = MessageId;            request.Subject = Subject;            request.From = From;            request.To = To;            request.Cc = Cc;            request.Bcc = Bcc;            request.ReceivedOn = ReceivedOn;            request.SubmittedBy = SubmittedBy;            request.Importance = Importance;            request.Body = Body;            request.Attachments = Attachments;            if (ExtraProperties != null)
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
            request.ValidateBeforeCreate = ValidateBeforeCreate;
            if (ShouldProcess("Executing DeliverIncomingEmailRequest", "DeliverIncomingEmailRequest"))
            {
                var response = (DeliverIncomingEmailResponse)Connection.Execute(request);
                WriteObject(response);
            }
        }
    }
}
