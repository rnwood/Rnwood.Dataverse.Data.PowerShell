using System;
using System.Management.Automation;
using Microsoft.Crm.Sdk.Messages;
using Microsoft.PowerPlatform.Dataverse.Client;
using Microsoft.Xrm.Sdk;

namespace Rnwood.Dataverse.Data.PowerShell.Commands
{
    [Cmdlet(VerbsCommon.Copy, "DataverseContract", SupportsShouldProcess = true, ConfirmImpact = ConfirmImpact.Medium)]
    [OutputType(typeof(CloneContractResponse))]
    ///<summary>Executes CloneContractRequest SDK message.</summary>
    public class CopyDataverseContractCmdlet : OrganizationServiceCmdlet
    {
        [Parameter(Mandatory = true, HelpMessage = "DataverseConnection instance obtained from Get-DataverseConnection cmdlet")]
        public override ServiceClient Connection { get; set; }
        [Parameter(Mandatory = false, HelpMessage = "ContractId parameter")]
        public Guid ContractId { get; set; }
        [Parameter(Mandatory = false, HelpMessage = "IncludeCanceledLines parameter")]
        public Boolean IncludeCanceledLines { get; set; }

        protected override void ProcessRecord()
        {
            base.ProcessRecord();

            var request = new CloneContractRequest();
            request.ContractId = ContractId;            request.IncludeCanceledLines = IncludeCanceledLines;
            if (ShouldProcess("Executing CloneContractRequest", "CloneContractRequest"))
            {
                var response = (CloneContractResponse)Connection.Execute(request);
                WriteObject(response);
            }
        }
    }
}
