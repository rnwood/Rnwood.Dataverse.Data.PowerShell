using System;
using System.Management.Automation;
using Microsoft.Crm.Sdk.Messages;
using Microsoft.PowerPlatform.Dataverse.Client;
using Microsoft.Xrm.Sdk;

namespace Rnwood.Dataverse.Data.PowerShell.Commands
{
    [Cmdlet(VerbsCommon.Get, "DataverseUserLicenseInfo")]
    [OutputType(typeof(RetrieveUserLicenseInfoResponse))]
    ///<summary>Executes RetrieveUserLicenseInfoRequest SDK message.</summary>
    public class GetDataverseUserLicenseInfoCmdlet : OrganizationServiceCmdlet
    {
        [Parameter(Mandatory = true, HelpMessage = "DataverseConnection instance obtained from Get-DataverseConnection cmdlet")]
        public override ServiceClient Connection { get; set; }
        [Parameter(Mandatory = false, HelpMessage = "SystemUserId parameter")]
        public Guid SystemUserId { get; set; }

        protected override void ProcessRecord()
        {
            base.ProcessRecord();

            var request = new RetrieveUserLicenseInfoRequest();
            request.SystemUserId = SystemUserId;
            var response = (RetrieveUserLicenseInfoResponse)Connection.Execute(request);
            WriteObject(response);
        }
    }
}
