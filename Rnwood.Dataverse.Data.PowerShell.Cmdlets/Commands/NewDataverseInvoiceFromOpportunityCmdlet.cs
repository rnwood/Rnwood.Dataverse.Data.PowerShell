using System;
using System.Management.Automation;
using Microsoft.Crm.Sdk.Messages;
using Microsoft.PowerPlatform.Dataverse.Client;
using Microsoft.Xrm.Sdk;

namespace Rnwood.Dataverse.Data.PowerShell.Commands
{
    [Cmdlet(VerbsCommon.New, "DataverseInvoiceFromOpportunity", SupportsShouldProcess = true, ConfirmImpact = ConfirmImpact.Medium)]
    [OutputType(typeof(GenerateInvoiceFromOpportunityResponse))]
    ///<summary>Executes GenerateInvoiceFromOpportunityRequest SDK message.</summary>
    public class NewDataverseInvoiceFromOpportunityCmdlet : OrganizationServiceCmdlet
    {
        [Parameter(Mandatory = true, HelpMessage = "DataverseConnection instance obtained from Get-DataverseConnection cmdlet")]
        public override ServiceClient Connection { get; set; }
        [Parameter(Mandatory = false, HelpMessage = "OpportunityId parameter")]
        public Guid OpportunityId { get; set; }
        [Parameter(Mandatory = false, HelpMessage = "ColumnSet parameter")]
        public Microsoft.Xrm.Sdk.Query.ColumnSet ColumnSet { get; set; }

        protected override void ProcessRecord()
        {
            base.ProcessRecord();

            var request = new GenerateInvoiceFromOpportunityRequest();
            request.OpportunityId = OpportunityId;            request.ColumnSet = ColumnSet;
            if (ShouldProcess("Executing GenerateInvoiceFromOpportunityRequest", "GenerateInvoiceFromOpportunityRequest"))
            {
                var response = (GenerateInvoiceFromOpportunityResponse)Connection.Execute(request);
                WriteObject(response);
            }
        }
    }
}
