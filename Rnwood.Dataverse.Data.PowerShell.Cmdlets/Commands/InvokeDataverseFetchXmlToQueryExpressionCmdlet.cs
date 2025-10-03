using System;
using System.Management.Automation;
using Microsoft.Crm.Sdk.Messages;
using Microsoft.PowerPlatform.Dataverse.Client;
using Microsoft.Xrm.Sdk;

namespace Rnwood.Dataverse.Data.PowerShell.Commands
{
    [Cmdlet(VerbsLifecycle.Invoke, "DataverseFetchXmlToQueryExpression")]
    [OutputType(typeof(FetchXmlToQueryExpressionResponse))]
    ///<summary>Executes FetchXmlToQueryExpressionRequest SDK message.</summary>
    public class InvokeDataverseFetchXmlToQueryExpressionCmdlet : OrganizationServiceCmdlet
    {
        [Parameter(Mandatory = true, HelpMessage = "DataverseConnection instance obtained from Get-DataverseConnection cmdlet")]
        public override ServiceClient Connection { get; set; }
        [Parameter(Mandatory = false, HelpMessage = "FetchXml parameter")]
        public String FetchXml { get; set; }

        protected override void ProcessRecord()
        {
            base.ProcessRecord();

            var request = new FetchXmlToQueryExpressionRequest();
            request.FetchXml = FetchXml;
            var response = (FetchXmlToQueryExpressionResponse)Connection.Execute(request);
            WriteObject(response);
        }
    }
}
