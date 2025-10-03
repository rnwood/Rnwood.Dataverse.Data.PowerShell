using System;
using System.Management.Automation;
using Microsoft.Crm.Sdk.Messages;
using Microsoft.PowerPlatform.Dataverse.Client;
using Microsoft.Xrm.Sdk;

namespace Rnwood.Dataverse.Data.PowerShell.Commands
{
    [Cmdlet(VerbsLifecycle.Stop, "DataverseContract")]
    [OutputType(typeof(CancelContractResponse))]
    ///<summary>Executes CancelContractRequest SDK message.</summary>
    public class StopDataverseContractCmdlet : OrganizationServiceCmdlet
    {
        [Parameter(Mandatory = true, HelpMessage = "DataverseConnection instance obtained from Get-DataverseConnection cmdlet")]
        public override ServiceClient Connection { get; set; }
        [Parameter(Mandatory = false, HelpMessage = "ContractId parameter")]
        public Guid ContractId { get; set; }
        [Parameter(Mandatory = false, HelpMessage = "CancelDate parameter")]
        public DateTime CancelDate { get; set; }
        [Parameter(Mandatory = false, HelpMessage = "Status parameter")]
        public object Status { get; set; }

        protected override void ProcessRecord()
        {
            base.ProcessRecord();

            var request = new CancelContractRequest();
            request.ContractId = ContractId;            request.CancelDate = CancelDate;            if (Status != null)
            {
                request.Status = DataverseTypeConverter.ToOptionSetValue(Status, "Status");
            }

            var response = (CancelContractResponse)Connection.Execute(request);
            WriteObject(response);
        }
    }
}
