using System;
using System.Management.Automation;
using Microsoft.Crm.Sdk.Messages;
using Microsoft.PowerPlatform.Dataverse.Client;
using Microsoft.Xrm.Sdk;

namespace Rnwood.Dataverse.Data.PowerShell.Commands
{
    [Cmdlet(VerbsCommon.Get, "DataverseLicenseInfo")]
    [OutputType(typeof(RetrieveLicenseInfoResponse))]
    ///<summary>Executes RetrieveLicenseInfoRequest SDK message.</summary>
    public class GetDataverseLicenseInfoCmdlet : OrganizationServiceCmdlet
    {
        [Parameter(Mandatory = true, HelpMessage = "DataverseConnection instance obtained from Get-DataverseConnection cmdlet")]
        public override ServiceClient Connection { get; set; }
        [Parameter(Mandatory = false, HelpMessage = "AccessMode parameter")]
        public Int32 AccessMode { get; set; }

        protected override void ProcessRecord()
        {
            base.ProcessRecord();

            var request = new RetrieveLicenseInfoRequest();
            request.AccessMode = AccessMode;
            var response = (RetrieveLicenseInfoResponse)Connection.Execute(request);
            WriteObject(response);
        }
    }
}
