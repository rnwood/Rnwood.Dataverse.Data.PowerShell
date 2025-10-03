using System;
using System.Management.Automation;
using Microsoft.Crm.Sdk.Messages;
using Microsoft.PowerPlatform.Dataverse.Client;
using Microsoft.Xrm.Sdk;

namespace Rnwood.Dataverse.Data.PowerShell.Commands
{
    [Cmdlet(VerbsCommon.Get, "DataverseExchangeRate")]
    [OutputType(typeof(RetrieveExchangeRateResponse))]
    ///<summary>Executes RetrieveExchangeRateRequest SDK message.</summary>
    public class GetDataverseExchangeRateCmdlet : OrganizationServiceCmdlet
    {
        [Parameter(Mandatory = true, HelpMessage = "DataverseConnection instance obtained from Get-DataverseConnection cmdlet")]
        public override ServiceClient Connection { get; set; }
        [Parameter(Mandatory = false, HelpMessage = "TransactionCurrencyId parameter")]
        public Guid TransactionCurrencyId { get; set; }

        protected override void ProcessRecord()
        {
            base.ProcessRecord();

            var request = new RetrieveExchangeRateRequest();
            request.TransactionCurrencyId = TransactionCurrencyId;
            var response = (RetrieveExchangeRateResponse)Connection.Execute(request);
            WriteObject(response);
        }
    }
}
