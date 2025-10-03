using System;
using System.Management.Automation;
using Microsoft.Crm.Sdk.Messages;
using Microsoft.PowerPlatform.Dataverse.Client;
using Microsoft.Xrm.Sdk;

namespace Rnwood.Dataverse.Data.PowerShell.Commands
{
    [Cmdlet(VerbsLifecycle.Complete, "DataverseQuote")]
    [OutputType(typeof(WinQuoteResponse))]
    ///<summary>Executes WinQuoteRequest SDK message.</summary>
    public class CompleteDataverseQuoteCmdlet : OrganizationServiceCmdlet
    {
        [Parameter(Mandatory = true, HelpMessage = "DataverseConnection instance obtained from Get-DataverseConnection cmdlet")]
        public override ServiceClient Connection { get; set; }
        [Parameter(Mandatory = false, HelpMessage = "QuoteClose parameter")]
        public PSObject QuoteClose { get; set; }
        [Parameter(Mandatory = false, HelpMessage = "Status parameter")]
        public object Status { get; set; }

        protected override void ProcessRecord()
        {
            base.ProcessRecord();

            var request = new WinQuoteRequest();
            if (QuoteClose != null)
            {
                var entity = new Entity();
                foreach (PSPropertyInfo prop in QuoteClose.Properties)
                {
                    if (prop.Name != "Id" && prop.Name != "TableName" && prop.Name != "LogicalName")
                    {
                        entity[prop.Name] = prop.Value;
                    }
                }
                request.QuoteClose = entity;
            }
            if (Status != null)
            {
                request.Status = DataverseTypeConverter.ToOptionSetValue(Status, "Status");
            }

            var response = (WinQuoteResponse)Connection.Execute(request);
            WriteObject(response);
        }
    }
}
