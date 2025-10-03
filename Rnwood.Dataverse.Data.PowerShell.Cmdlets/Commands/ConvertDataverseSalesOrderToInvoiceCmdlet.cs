using System;
using System.Management.Automation;
using Microsoft.Crm.Sdk.Messages;
using Microsoft.PowerPlatform.Dataverse.Client;
using Microsoft.Xrm.Sdk;

namespace Rnwood.Dataverse.Data.PowerShell.Commands
{
    [Cmdlet(VerbsData.Convert, "DataverseSalesOrderToInvoice", SupportsShouldProcess = true, ConfirmImpact = ConfirmImpact.Medium)]
    [OutputType(typeof(ConvertSalesOrderToInvoiceResponse))]
    ///<summary>Executes ConvertSalesOrderToInvoiceRequest SDK message.</summary>
    public class ConvertDataverseSalesOrderToInvoiceCmdlet : OrganizationServiceCmdlet
    {
        [Parameter(Mandatory = true, HelpMessage = "DataverseConnection instance obtained from Get-DataverseConnection cmdlet")]
        public override ServiceClient Connection { get; set; }
        [Parameter(Mandatory = false, HelpMessage = "SalesOrderId parameter")]
        public Guid SalesOrderId { get; set; }
        [Parameter(Mandatory = false, HelpMessage = "ColumnSet parameter")]
        public Microsoft.Xrm.Sdk.Query.ColumnSet ColumnSet { get; set; }
        [Parameter(Mandatory = false, ValueFromPipelineByPropertyName = true, HelpMessage = "ProcessInstanceId parameter")]
        public object ProcessInstanceId { get; set; }

        protected override void ProcessRecord()
        {
            base.ProcessRecord();

            var request = new ConvertSalesOrderToInvoiceRequest();
            request.SalesOrderId = SalesOrderId;            request.ColumnSet = ColumnSet;            if (ProcessInstanceId != null)
            {
                request.ProcessInstanceId = DataverseTypeConverter.ToEntityReference(ProcessInstanceId, null, "ProcessInstanceId");
            }

            if (ShouldProcess("Executing ConvertSalesOrderToInvoiceRequest", "ConvertSalesOrderToInvoiceRequest"))
            {
                var response = (ConvertSalesOrderToInvoiceResponse)Connection.Execute(request);
                WriteObject(response);
            }
        }
    }
}
