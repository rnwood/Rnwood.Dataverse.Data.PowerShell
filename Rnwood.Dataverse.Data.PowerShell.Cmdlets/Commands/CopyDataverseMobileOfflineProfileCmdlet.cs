using System;
using System.Management.Automation;
using Microsoft.Crm.Sdk.Messages;
using Microsoft.PowerPlatform.Dataverse.Client;
using Microsoft.Xrm.Sdk;

namespace Rnwood.Dataverse.Data.PowerShell.Commands
{
    [Cmdlet(VerbsCommon.Copy, "DataverseMobileOfflineProfile", SupportsShouldProcess = true, ConfirmImpact = ConfirmImpact.Medium)]
    [OutputType(typeof(CloneMobileOfflineProfileResponse))]
    ///<summary>Executes CloneMobileOfflineProfileRequest SDK message.</summary>
    public class CopyDataverseMobileOfflineProfileCmdlet : OrganizationServiceCmdlet
    {
        [Parameter(Mandatory = true, HelpMessage = "DataverseConnection instance obtained from Get-DataverseConnection cmdlet")]
        public override ServiceClient Connection { get; set; }
        [Parameter(Mandatory = false, ValueFromPipelineByPropertyName = true, HelpMessage = "Source parameter")]
        public object Source { get; set; }

        protected override void ProcessRecord()
        {
            base.ProcessRecord();

            var request = new CloneMobileOfflineProfileRequest();
            if (Source != null)
            {
                request.Source = DataverseTypeConverter.ToEntityReference(Source, null, "Source");
            }

            if (ShouldProcess("Executing CloneMobileOfflineProfileRequest", "CloneMobileOfflineProfileRequest"))
            {
                var response = (CloneMobileOfflineProfileResponse)Connection.Execute(request);
                WriteObject(response);
            }
        }
    }
}
