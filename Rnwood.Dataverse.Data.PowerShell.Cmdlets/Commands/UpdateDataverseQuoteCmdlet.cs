using System;
using System.Management.Automation;
using Microsoft.Crm.Sdk.Messages;
using Microsoft.PowerPlatform.Dataverse.Client;
using Microsoft.Xrm.Sdk;

namespace Rnwood.Dataverse.Data.PowerShell.Commands
{
    [Cmdlet(VerbsData.Update, "DataverseQuote", SupportsShouldProcess = true, ConfirmImpact = ConfirmImpact.Medium)]
    [OutputType(typeof(ReviseQuoteResponse))]
    ///<summary>Executes ReviseQuoteRequest SDK message.</summary>
    public class UpdateDataverseQuoteCmdlet : OrganizationServiceCmdlet
    {
        [Parameter(Mandatory = true, HelpMessage = "DataverseConnection instance obtained from Get-DataverseConnection cmdlet")]
        public override ServiceClient Connection { get; set; }
        [Parameter(Mandatory = false, HelpMessage = "QuoteId parameter")]
        public Guid QuoteId { get; set; }
        [Parameter(Mandatory = false, HelpMessage = "ColumnSet parameter")]
        public Microsoft.Xrm.Sdk.Query.ColumnSet ColumnSet { get; set; }

        protected override void ProcessRecord()
        {
            base.ProcessRecord();

            var request = new ReviseQuoteRequest();
            request.QuoteId = QuoteId;            request.ColumnSet = ColumnSet;
            if (ShouldProcess("Executing ReviseQuoteRequest", "ReviseQuoteRequest"))
            {
                var response = (ReviseQuoteResponse)Connection.Execute(request);
                WriteObject(response);
            }
        }
    }
}
