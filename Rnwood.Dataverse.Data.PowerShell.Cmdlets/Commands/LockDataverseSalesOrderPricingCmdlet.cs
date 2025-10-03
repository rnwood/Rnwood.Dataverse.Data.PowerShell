using System;
using System.Management.Automation;
using Microsoft.Crm.Sdk.Messages;
using Microsoft.PowerPlatform.Dataverse.Client;

namespace Rnwood.Dataverse.Data.PowerShell.Commands
{
    [Cmdlet(VerbsCommon.Lock, "DataverseSalesOrderPricing")]
    [OutputType(typeof(LockSalesOrderPricingResponse))]
    ///<summary>Locks pricing for a sales order to prevent automatic price recalculations.</summary>
    public class LockDataverseSalesOrderPricingCmdlet : OrganizationServiceCmdlet
    {
        [Parameter(Mandatory = true, HelpMessage = "DataverseConnection instance obtained from Get-DataverseConnection cmdlet")]
        public override ServiceClient Connection { get; set; }

        [Parameter(Mandatory = true, ValueFromPipelineByPropertyName = true, HelpMessage = "ID of the sales order to lock pricing for")]
        [Alias("Id")]
        public Guid SalesOrderId { get; set; }

        protected override void ProcessRecord()
        {
            base.ProcessRecord();

            LockSalesOrderPricingRequest request = new LockSalesOrderPricingRequest
            {
                SalesOrderId = SalesOrderId
            };

            LockSalesOrderPricingResponse response = (LockSalesOrderPricingResponse)Connection.Execute(request);
            WriteObject(response);
        }
    }
}
