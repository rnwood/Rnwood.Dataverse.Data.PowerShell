using System;
using System.Management.Automation;
using Microsoft.Crm.Sdk.Messages;
using Microsoft.PowerPlatform.Dataverse.Client;

namespace Rnwood.Dataverse.Data.PowerShell.Commands
{
    [Cmdlet(VerbsCommunications.Send, "DataverseEmail")]
    [OutputType(typeof(SendEmailResponse))]
    ///<summary>Sends an email message.</summary>
    public class SendDataverseEmailCmdlet : OrganizationServiceCmdlet
    {
        [Parameter(Mandatory = true, HelpMessage = "DataverseConnection instance obtained from Get-DataverseConnection cmdlet")]
        public override ServiceClient Connection { get; set; }

        [Parameter(Mandatory = true, ValueFromPipelineByPropertyName = true, HelpMessage = "ID of the email record to send")]
        [Alias("Id")]
        public Guid EmailId { get; set; }

        [Parameter(Mandatory = false, HelpMessage = "Whether to send the email immediately (true) or queue it for sending (false). Default is true.")]
        public bool IssueSend { get; set; } = true;

        [Parameter(Mandatory = false, HelpMessage = "Optional tracking token for email campaign tracking")]
        public string TrackingToken { get; set; }

        protected override void ProcessRecord()
        {
            base.ProcessRecord();

            SendEmailRequest request = new SendEmailRequest
            {
                EmailId = EmailId,
                IssueSend = IssueSend
            };

            if (!string.IsNullOrEmpty(TrackingToken))
            {
                request.TrackingToken = TrackingToken;
            }

            SendEmailResponse response = (SendEmailResponse)Connection.Execute(request);
            WriteObject(response);
        }
    }
}
