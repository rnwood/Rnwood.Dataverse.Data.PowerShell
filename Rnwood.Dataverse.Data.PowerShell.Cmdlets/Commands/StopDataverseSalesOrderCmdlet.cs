using System;
using System.Management.Automation;
using Microsoft.Crm.Sdk.Messages;
using Microsoft.PowerPlatform.Dataverse.Client;
using Microsoft.Xrm.Sdk;

namespace Rnwood.Dataverse.Data.PowerShell.Commands
{
    [Cmdlet(VerbsLifecycle.Stop, "DataverseSalesOrder")]
    [OutputType(typeof(CancelSalesOrderResponse))]
    ///<summary>Executes CancelSalesOrderRequest SDK message.</summary>
    public class StopDataverseSalesOrderCmdlet : OrganizationServiceCmdlet
    {
        [Parameter(Mandatory = true, HelpMessage = "DataverseConnection instance obtained from Get-DataverseConnection cmdlet")]
        public override ServiceClient Connection { get; set; }
        [Parameter(Mandatory = false, HelpMessage = "OrderClose parameter")]
        public PSObject OrderClose { get; set; }
        [Parameter(Mandatory = false, HelpMessage = "Status parameter")]
        public object Status { get; set; }

        protected override void ProcessRecord()
        {
            base.ProcessRecord();

            var request = new CancelSalesOrderRequest();
            if (OrderClose != null)
            {
                var entity = new Entity();
                foreach (PSPropertyInfo prop in OrderClose.Properties)
                {
                    if (prop.Name != "Id" && prop.Name != "TableName" && prop.Name != "LogicalName")
                    {
                        entity[prop.Name] = prop.Value;
                    }
                }
                request.OrderClose = entity;
            }
            if (Status != null)
            {
                request.Status = DataverseTypeConverter.ToOptionSetValue(Status, "Status");
            }

            var response = (CancelSalesOrderResponse)Connection.Execute(request);
            WriteObject(response);
        }
    }
}
