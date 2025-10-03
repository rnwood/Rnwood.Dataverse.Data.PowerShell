using System;
using System.Management.Automation;
using Microsoft.Crm.Sdk.Messages;
using Microsoft.PowerPlatform.Dataverse.Client;
using Microsoft.Xrm.Sdk;

namespace Rnwood.Dataverse.Data.PowerShell.Commands
{
    [Cmdlet(VerbsDiagnostic.Test, "DataversePromoteEmail")]
    [OutputType(typeof(CheckPromoteEmailResponse))]
    ///<summary>Executes CheckPromoteEmailRequest SDK message.</summary>
    public class TestDataversePromoteEmailCmdlet : OrganizationServiceCmdlet
    {
        [Parameter(Mandatory = true, HelpMessage = "DataverseConnection instance obtained from Get-DataverseConnection cmdlet")]
        public override ServiceClient Connection { get; set; }
        [Parameter(Mandatory = false, HelpMessage = "MessageId parameter")]
        public String MessageId { get; set; }
        [Parameter(Mandatory = false, HelpMessage = "Subject parameter")]
        public String Subject { get; set; }
        [Parameter(Mandatory = false, HelpMessage = "DirectionCode parameter")]
        public Int32 DirectionCode { get; set; }

        protected override void ProcessRecord()
        {
            base.ProcessRecord();

            var request = new CheckPromoteEmailRequest();
            request.MessageId = MessageId;            request.Subject = Subject;            request.DirectionCode = DirectionCode;
            var response = (CheckPromoteEmailResponse)Connection.Execute(request);
            WriteObject(response);
        }
    }
}
