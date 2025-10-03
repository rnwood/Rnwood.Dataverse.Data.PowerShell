using System;
using System.Management.Automation;
using Microsoft.Crm.Sdk.Messages;
using Microsoft.PowerPlatform.Dataverse.Client;
using Microsoft.Xrm.Sdk;

namespace Rnwood.Dataverse.Data.PowerShell.Commands
{
    [Cmdlet(VerbsCommon.Get, "DataverseSalesOrderProductsFromOpportunity")]
    [OutputType(typeof(GetSalesOrderProductsFromOpportunityResponse))]
    ///<summary>Executes GetSalesOrderProductsFromOpportunityRequest SDK message.</summary>
    public class GetDataverseSalesOrderProductsFromOpportunityCmdlet : OrganizationServiceCmdlet
    {
        [Parameter(Mandatory = true, HelpMessage = "DataverseConnection instance obtained from Get-DataverseConnection cmdlet")]
        public override ServiceClient Connection { get; set; }
        [Parameter(Mandatory = false, HelpMessage = "OpportunityId parameter")]
        public Guid OpportunityId { get; set; }
        [Parameter(Mandatory = false, HelpMessage = "SalesOrderId parameter")]
        public Guid SalesOrderId { get; set; }

        protected override void ProcessRecord()
        {
            base.ProcessRecord();

            var request = new GetSalesOrderProductsFromOpportunityRequest();
            request.OpportunityId = OpportunityId;            request.SalesOrderId = SalesOrderId;
            var response = (GetSalesOrderProductsFromOpportunityResponse)Connection.Execute(request);
            WriteObject(response);
        }
    }
}
