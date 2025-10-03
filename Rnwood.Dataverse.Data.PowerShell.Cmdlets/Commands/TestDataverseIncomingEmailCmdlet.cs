using System;
using System.Management.Automation;
using Microsoft.Crm.Sdk.Messages;
using Microsoft.PowerPlatform.Dataverse.Client;
using Microsoft.Xrm.Sdk;

namespace Rnwood.Dataverse.Data.PowerShell.Commands
{
    [Cmdlet(VerbsDiagnostic.Test, "DataverseIncomingEmail")]
    [OutputType(typeof(CheckIncomingEmailResponse))]
    ///<summary>Executes CheckIncomingEmailRequest SDK message.</summary>
    public class TestDataverseIncomingEmailCmdlet : OrganizationServiceCmdlet
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
        [Parameter(Mandatory = false, HelpMessage = "ExtraProperties parameter")]
        public PSObject ExtraProperties { get; set; }

        protected override void ProcessRecord()
        {
            base.ProcessRecord();

            var request = new CheckIncomingEmailRequest();
            request.MessageId = MessageId;            request.Subject = Subject;            request.From = From;            request.To = To;            request.Cc = Cc;            request.Bcc = Bcc;            if (ExtraProperties != null)
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

            var response = (CheckIncomingEmailResponse)Connection.Execute(request);
            WriteObject(response);
        }
    }
}
