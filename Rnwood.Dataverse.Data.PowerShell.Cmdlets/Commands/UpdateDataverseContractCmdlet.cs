using System;
using System.Management.Automation;
using Microsoft.Crm.Sdk.Messages;
using Microsoft.PowerPlatform.Dataverse.Client;
using Microsoft.Xrm.Sdk;

namespace Rnwood.Dataverse.Data.PowerShell.Commands
{
    [Cmdlet(VerbsData.Update, "DataverseContract", SupportsShouldProcess = true, ConfirmImpact = ConfirmImpact.Medium)]
    [OutputType(typeof(RenewContractResponse))]
    ///<summary>Executes RenewContractRequest SDK message.</summary>
    public class UpdateDataverseContractCmdlet : OrganizationServiceCmdlet
    {
        [Parameter(Mandatory = true, HelpMessage = "DataverseConnection instance obtained from Get-DataverseConnection cmdlet")]
        public override ServiceClient Connection { get; set; }
        [Parameter(Mandatory = false, HelpMessage = "ContractId parameter")]
        public Guid ContractId { get; set; }
        [Parameter(Mandatory = false, HelpMessage = "Status parameter")]
        public Int32 Status { get; set; }
        [Parameter(Mandatory = false, HelpMessage = "IncludeCanceledLines parameter")]
        public Boolean IncludeCanceledLines { get; set; }

        protected override void ProcessRecord()
        {
            base.ProcessRecord();

            var request = new RenewContractRequest();
            request.ContractId = ContractId;            request.Status = Status;            request.IncludeCanceledLines = IncludeCanceledLines;
            if (ShouldProcess("Executing RenewContractRequest", "RenewContractRequest"))
            {
                var response = (RenewContractResponse)Connection.Execute(request);
                WriteObject(response);
            }
        }
    }
}
