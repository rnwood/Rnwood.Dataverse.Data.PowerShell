using System;
using System.Management.Automation;
using Microsoft.Crm.Sdk.Messages;
using Microsoft.PowerPlatform.Dataverse.Client;
using Microsoft.Xrm.Sdk;

namespace Rnwood.Dataverse.Data.PowerShell.Commands
{
    [Cmdlet(VerbsSecurity.Grant, "DataverseAccessUsingSharedLink", SupportsShouldProcess = true, ConfirmImpact = ConfirmImpact.Medium)]
    [OutputType(typeof(GrantAccessUsingSharedLinkResponse))]
    ///<summary>Executes GrantAccessUsingSharedLinkRequest SDK message.</summary>
    public class GrantDataverseAccessUsingSharedLinkCmdlet : OrganizationServiceCmdlet
    {
        [Parameter(Mandatory = true, HelpMessage = "DataverseConnection instance obtained from Get-DataverseConnection cmdlet")]
        public override ServiceClient Connection { get; set; }
        [Parameter(Mandatory = false, HelpMessage = "RecordUrlWithSharedLink parameter")]
        public String RecordUrlWithSharedLink { get; set; }

        protected override void ProcessRecord()
        {
            base.ProcessRecord();

            var request = new GrantAccessUsingSharedLinkRequest();
            request.RecordUrlWithSharedLink = RecordUrlWithSharedLink;
            if (ShouldProcess("Executing GrantAccessUsingSharedLinkRequest", "GrantAccessUsingSharedLinkRequest"))
            {
                var response = (GrantAccessUsingSharedLinkResponse)Connection.Execute(request);
                WriteObject(response);
            }
        }
    }
}
