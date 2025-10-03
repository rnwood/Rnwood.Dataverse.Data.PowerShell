using System;
using System.Management.Automation;
using Microsoft.Crm.Sdk.Messages;
using Microsoft.PowerPlatform.Dataverse.Client;
using Microsoft.Xrm.Sdk;

namespace Rnwood.Dataverse.Data.PowerShell.Commands
{
    [Cmdlet(VerbsData.Convert, "DataverseQuoteToSalesOrder", SupportsShouldProcess = true, ConfirmImpact = ConfirmImpact.Medium)]
    [OutputType(typeof(ConvertQuoteToSalesOrderResponse))]
    ///<summary>Executes ConvertQuoteToSalesOrderRequest SDK message.</summary>
    public class ConvertDataverseQuoteToSalesOrderCmdlet : OrganizationServiceCmdlet
    {
        [Parameter(Mandatory = true, HelpMessage = "DataverseConnection instance obtained from Get-DataverseConnection cmdlet")]
        public override ServiceClient Connection { get; set; }
        [Parameter(Mandatory = false, HelpMessage = "QuoteId parameter")]
        public Guid QuoteId { get; set; }
        [Parameter(Mandatory = false, HelpMessage = "ColumnSet parameter")]
        public Microsoft.Xrm.Sdk.Query.ColumnSet ColumnSet { get; set; }
        [Parameter(Mandatory = false, HelpMessage = "QuoteCloseDate parameter")]
        public DateTime QuoteCloseDate { get; set; }
        [Parameter(Mandatory = false, HelpMessage = "QuoteCloseStatus parameter")]
        public object QuoteCloseStatus { get; set; }
        [Parameter(Mandatory = false, HelpMessage = "QuoteCloseSubject parameter")]
        public String QuoteCloseSubject { get; set; }
        [Parameter(Mandatory = false, HelpMessage = "QuoteCloseDescription parameter")]
        public String QuoteCloseDescription { get; set; }
        [Parameter(Mandatory = false, ValueFromPipelineByPropertyName = true, HelpMessage = "ProcessInstanceId parameter")]
        public object ProcessInstanceId { get; set; }

        protected override void ProcessRecord()
        {
            base.ProcessRecord();

            var request = new ConvertQuoteToSalesOrderRequest();
            request.QuoteId = QuoteId;            request.ColumnSet = ColumnSet;            request.QuoteCloseDate = QuoteCloseDate;            if (QuoteCloseStatus != null)
            {
                request.QuoteCloseStatus = DataverseTypeConverter.ToOptionSetValue(QuoteCloseStatus, "QuoteCloseStatus");
            }
            request.QuoteCloseSubject = QuoteCloseSubject;            request.QuoteCloseDescription = QuoteCloseDescription;            if (ProcessInstanceId != null)
            {
                request.ProcessInstanceId = DataverseTypeConverter.ToEntityReference(ProcessInstanceId, null, "ProcessInstanceId");
            }

            if (ShouldProcess("Executing ConvertQuoteToSalesOrderRequest", "ConvertQuoteToSalesOrderRequest"))
            {
                var response = (ConvertQuoteToSalesOrderResponse)Connection.Execute(request);
                WriteObject(response);
            }
        }
    }
}
