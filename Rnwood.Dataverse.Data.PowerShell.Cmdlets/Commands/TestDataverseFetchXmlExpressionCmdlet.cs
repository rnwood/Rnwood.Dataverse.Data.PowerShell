using System;
using System.Management.Automation;
using Microsoft.Crm.Sdk.Messages;
using Microsoft.PowerPlatform.Dataverse.Client;
using Microsoft.Xrm.Sdk;

namespace Rnwood.Dataverse.Data.PowerShell.Commands
{
    [Cmdlet(VerbsDiagnostic.Test, "DataverseFetchXmlExpression")]
    [OutputType(typeof(ValidateFetchXmlExpressionResponse))]
    ///<summary>Executes ValidateFetchXmlExpressionRequest SDK message.</summary>
    public class TestDataverseFetchXmlExpressionCmdlet : OrganizationServiceCmdlet
    {
        [Parameter(Mandatory = true, HelpMessage = "DataverseConnection instance obtained from Get-DataverseConnection cmdlet")]
        public override ServiceClient Connection { get; set; }
        [Parameter(Mandatory = false, HelpMessage = "FetchXml parameter")]
        public String FetchXml { get; set; }

        protected override void ProcessRecord()
        {
            base.ProcessRecord();

            var request = new ValidateFetchXmlExpressionRequest();
            request.FetchXml = FetchXml;
            var response = (ValidateFetchXmlExpressionResponse)Connection.Execute(request);
            WriteObject(response);
        }
    }
}
