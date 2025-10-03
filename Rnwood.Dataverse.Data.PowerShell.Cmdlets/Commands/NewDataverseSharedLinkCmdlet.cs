using System;
using System.Management.Automation;
using Microsoft.Crm.Sdk.Messages;
using Microsoft.PowerPlatform.Dataverse.Client;
using Microsoft.Xrm.Sdk;

namespace Rnwood.Dataverse.Data.PowerShell.Commands
{
    [Cmdlet(VerbsCommon.New, "DataverseSharedLink", SupportsShouldProcess = true, ConfirmImpact = ConfirmImpact.Medium)]
    [OutputType(typeof(GenerateSharedLinkResponse))]
    ///<summary>Executes GenerateSharedLinkRequest SDK message.</summary>
    public class NewDataverseSharedLinkCmdlet : OrganizationServiceCmdlet
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

            var request = new GenerateSharedLinkRequest();
            if (Target != null)
            {
                request.Target = DataverseTypeConverter.ToEntityReference(Target, null, "Target");
            }
            request.SharedRights = SharedRights;
            if (ShouldProcess("Executing GenerateSharedLinkRequest", "GenerateSharedLinkRequest"))
            {
                var response = (GenerateSharedLinkResponse)Connection.Execute(request);
                WriteObject(response);
            }
        }
    }
}
