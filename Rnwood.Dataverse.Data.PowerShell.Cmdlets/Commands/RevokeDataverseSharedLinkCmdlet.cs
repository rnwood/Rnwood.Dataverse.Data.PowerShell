using System;
using System.Management.Automation;
using Microsoft.Crm.Sdk.Messages;
using Microsoft.PowerPlatform.Dataverse.Client;
using Microsoft.Xrm.Sdk;

namespace Rnwood.Dataverse.Data.PowerShell.Commands
{
    [Cmdlet(VerbsSecurity.Revoke, "DataverseSharedLink", SupportsShouldProcess = true, ConfirmImpact = ConfirmImpact.Medium)]
    [OutputType(typeof(RevokeSharedLinkResponse))]
    ///<summary>Executes RevokeSharedLinkRequest SDK message.</summary>
    public class RevokeDataverseSharedLinkCmdlet : OrganizationServiceCmdlet
    {
        [Parameter(Mandatory = true, HelpMessage = "DataverseConnection instance obtained from Get-DataverseConnection cmdlet")]
        public override ServiceClient Connection { get; set; }
        [Parameter(Mandatory = false, ValueFromPipelineByPropertyName = true, HelpMessage = "Target parameter")]
        public object Target { get; set; }
        [Parameter(Mandatory = false, HelpMessage = "SharedRights parameter")]
        public Microsoft.Crm.Sdk.Messages.AccessRights SharedRights { get; set; }

        protected override void ProcessRecord()
        {
            base.ProcessRecord();

            var request = new RevokeSharedLinkRequest();
            if (Target != null)
            {
                request.Target = DataverseTypeConverter.ToEntityReference(Target, null, "Target");
            }
            request.SharedRights = SharedRights;
            if (ShouldProcess("Executing RevokeSharedLinkRequest", "RevokeSharedLinkRequest"))
            {
                var response = (RevokeSharedLinkResponse)Connection.Execute(request);
                WriteObject(response);
            }
        }
    }
}
