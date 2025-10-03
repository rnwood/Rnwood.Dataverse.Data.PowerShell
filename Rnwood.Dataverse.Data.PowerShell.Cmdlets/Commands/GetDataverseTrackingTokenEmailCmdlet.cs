using System;
using System.Management.Automation;
using Microsoft.Crm.Sdk.Messages;
using Microsoft.PowerPlatform.Dataverse.Client;
using Microsoft.Xrm.Sdk;

namespace Rnwood.Dataverse.Data.PowerShell.Commands
{
    [Cmdlet(VerbsCommon.Get, "DataverseTrackingTokenEmail")]
    [OutputType(typeof(GetTrackingTokenEmailResponse))]
    ///<summary>Executes GetTrackingTokenEmailRequest SDK message.</summary>
    public class GetDataverseTrackingTokenEmailCmdlet : OrganizationServiceCmdlet
    {
        [Parameter(Mandatory = true, HelpMessage = "DataverseConnection instance obtained from Get-DataverseConnection cmdlet")]
        public override ServiceClient Connection { get; set; }
        [Parameter(Mandatory = false, HelpMessage = "Subject parameter")]
        public String Subject { get; set; }

        protected override void ProcessRecord()
        {
            base.ProcessRecord();

            var request = new GetTrackingTokenEmailRequest();
            request.Subject = Subject;
            var response = (GetTrackingTokenEmailResponse)Connection.Execute(request);
            WriteObject(response);
        }
    }
}
