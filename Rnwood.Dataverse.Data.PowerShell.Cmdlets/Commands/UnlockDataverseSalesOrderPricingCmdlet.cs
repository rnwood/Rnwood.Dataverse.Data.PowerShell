using System;
using System.Management.Automation;
using Microsoft.Crm.Sdk.Messages;
using Microsoft.PowerPlatform.Dataverse.Client;

namespace Rnwood.Dataverse.Data.PowerShell.Commands
{
    [Cmdlet(VerbsCommon.Unlock, "DataverseSalesOrderPricing")]
    [OutputType(typeof(UnlockSalesOrderPricingResponse))]
    ///<summary>Unlocks pricing for a sales order to allow automatic price recalculations.</summary>
    public class UnlockDataverseSalesOrderPricingCmdlet : OrganizationServiceCmdlet
    {
        [Parameter(Mandatory = true, HelpMessage = "DataverseConnection instance obtained from Get-DataverseConnection cmdlet")]
        public override ServiceClient Connection { get; set; }

        [Parameter(Mandatory = true, ValueFromPipelineByPropertyName = true, HelpMessage = "ID of the sales order to unlock pricing for")]
        [Alias("Id")]
        public Guid SalesOrderId { get; set; }

        protected override void ProcessRecord()
        {
            base.ProcessRecord();

            UnlockSalesOrderPricingRequest request = new UnlockSalesOrderPricingRequest
            {
                SalesOrderId = SalesOrderId
            };

            UnlockSalesOrderPricingResponse response = (UnlockSalesOrderPricingResponse)Connection.Execute(request);
            WriteObject(response);
        }
    }
}
