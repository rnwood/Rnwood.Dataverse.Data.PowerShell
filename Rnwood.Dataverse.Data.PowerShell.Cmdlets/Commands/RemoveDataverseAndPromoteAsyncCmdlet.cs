using System;
using System.Management.Automation;
using Microsoft.Crm.Sdk.Messages;
using Microsoft.PowerPlatform.Dataverse.Client;
using Microsoft.Xrm.Sdk;

namespace Rnwood.Dataverse.Data.PowerShell.Commands
{
    [Cmdlet(VerbsCommon.Remove, "DataverseAndPromoteAsync", SupportsShouldProcess = true, ConfirmImpact = ConfirmImpact.Medium)]
    [OutputType(typeof(DeleteAndPromoteAsyncResponse))]
    ///<summary>Executes DeleteAndPromoteAsyncRequest SDK message.</summary>
    public class RemoveDataverseAndPromoteAsyncCmdlet : OrganizationServiceCmdlet
    {
        [Parameter(Mandatory = true, HelpMessage = "DataverseConnection instance obtained from Get-DataverseConnection cmdlet")]
        public override ServiceClient Connection { get; set; }
        [Parameter(Mandatory = false, HelpMessage = "UniqueName parameter")]
        public String UniqueName { get; set; }

        protected override void ProcessRecord()
        {
            base.ProcessRecord();

            var request = new DeleteAndPromoteAsyncRequest();
            request.UniqueName = UniqueName;
            if (ShouldProcess("Executing DeleteAndPromoteAsyncRequest", "DeleteAndPromoteAsyncRequest"))
            {
                var response = (DeleteAndPromoteAsyncResponse)Connection.Execute(request);
                WriteObject(response);
            }
        }
    }
}
