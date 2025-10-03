using System;
using System.Management.Automation;
using Microsoft.Crm.Sdk.Messages;
using Microsoft.PowerPlatform.Dataverse.Client;
using Microsoft.Xrm.Sdk;

namespace Rnwood.Dataverse.Data.PowerShell.Commands
{
    [Cmdlet(VerbsCommon.Copy, "DataverseDynamicListToStatic", SupportsShouldProcess = true, ConfirmImpact = ConfirmImpact.Medium)]
    [OutputType(typeof(CopyDynamicListToStaticResponse))]
    ///<summary>Executes CopyDynamicListToStaticRequest SDK message.</summary>
    public class CopyDataverseDynamicListToStaticCmdlet : OrganizationServiceCmdlet
    {
        [Parameter(Mandatory = true, HelpMessage = "DataverseConnection instance obtained from Get-DataverseConnection cmdlet")]
        public override ServiceClient Connection { get; set; }
        [Parameter(Mandatory = false, HelpMessage = "ListId parameter")]
        public Guid ListId { get; set; }

        protected override void ProcessRecord()
        {
            base.ProcessRecord();

            var request = new CopyDynamicListToStaticRequest();
            request.ListId = ListId;
            if (ShouldProcess("Executing CopyDynamicListToStaticRequest", "CopyDynamicListToStaticRequest"))
            {
                var response = (CopyDynamicListToStaticResponse)Connection.Execute(request);
                WriteObject(response);
            }
        }
    }
}
