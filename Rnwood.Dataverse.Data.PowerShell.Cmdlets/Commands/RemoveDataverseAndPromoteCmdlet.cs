using System;
using System.Management.Automation;
using Microsoft.Crm.Sdk.Messages;
using Microsoft.PowerPlatform.Dataverse.Client;
using Microsoft.Xrm.Sdk;

namespace Rnwood.Dataverse.Data.PowerShell.Commands
{
    [Cmdlet(VerbsCommon.Remove, "DataverseAndPromote", SupportsShouldProcess = true, ConfirmImpact = ConfirmImpact.Medium)]
    [OutputType(typeof(DeleteAndPromoteResponse))]
    ///<summary>Executes DeleteAndPromoteRequest SDK message.</summary>
    public class RemoveDataverseAndPromoteCmdlet : OrganizationServiceCmdlet
    {
        [Parameter(Mandatory = true, HelpMessage = "DataverseConnection instance obtained from Get-DataverseConnection cmdlet")]
        public override ServiceClient Connection { get; set; }
        [Parameter(Mandatory = false, HelpMessage = "UniqueName parameter")]
        public String UniqueName { get; set; }

        protected override void ProcessRecord()
        {
            base.ProcessRecord();

            var request = new DeleteAndPromoteRequest();
            request.UniqueName = UniqueName;
            if (ShouldProcess("Executing DeleteAndPromoteRequest", "DeleteAndPromoteRequest"))
            {
                var response = (DeleteAndPromoteResponse)Connection.Execute(request);
                WriteObject(response);
            }
        }
    }
}
