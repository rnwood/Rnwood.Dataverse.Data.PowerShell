using System;
using System.Management.Automation;
using Microsoft.Crm.Sdk.Messages;
using Microsoft.PowerPlatform.Dataverse.Client;
using Microsoft.Xrm.Sdk;

namespace Rnwood.Dataverse.Data.PowerShell.Commands
{
    [Cmdlet(VerbsCommunications.Send, "DataverseFax", SupportsShouldProcess = true, ConfirmImpact = ConfirmImpact.Medium)]
    [OutputType(typeof(SendFaxResponse))]
    ///<summary>Executes SendFaxRequest SDK message.</summary>
    public class SendDataverseFaxCmdlet : OrganizationServiceCmdlet
    {
        [Parameter(Mandatory = true, HelpMessage = "DataverseConnection instance obtained from Get-DataverseConnection cmdlet")]
        public override ServiceClient Connection { get; set; }
        [Parameter(Mandatory = false, HelpMessage = "FaxId parameter")]
        public Guid FaxId { get; set; }
        [Parameter(Mandatory = false, HelpMessage = "IssueSend parameter")]
        public Boolean IssueSend { get; set; }

        protected override void ProcessRecord()
        {
            base.ProcessRecord();

            var request = new SendFaxRequest();
            request.FaxId = FaxId;            request.IssueSend = IssueSend;
            if (ShouldProcess("Executing SendFaxRequest", "SendFaxRequest"))
            {
                var response = (SendFaxResponse)Connection.Execute(request);
                WriteObject(response);
            }
        }
    }
}
