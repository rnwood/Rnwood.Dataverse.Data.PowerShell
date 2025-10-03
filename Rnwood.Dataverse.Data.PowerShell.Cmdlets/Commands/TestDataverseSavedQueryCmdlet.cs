using System;
using System.Management.Automation;
using Microsoft.Crm.Sdk.Messages;
using Microsoft.PowerPlatform.Dataverse.Client;
using Microsoft.Xrm.Sdk;

namespace Rnwood.Dataverse.Data.PowerShell.Commands
{
    [Cmdlet(VerbsDiagnostic.Test, "DataverseSavedQuery")]
    [OutputType(typeof(ValidateSavedQueryResponse))]
    ///<summary>Executes ValidateSavedQueryRequest SDK message.</summary>
    public class TestDataverseSavedQueryCmdlet : OrganizationServiceCmdlet
    {
        [Parameter(Mandatory = true, HelpMessage = "DataverseConnection instance obtained from Get-DataverseConnection cmdlet")]
        public override ServiceClient Connection { get; set; }
        [Parameter(Mandatory = false, HelpMessage = "FetchXml parameter")]
        public String FetchXml { get; set; }
        [Parameter(Mandatory = false, HelpMessage = "QueryType parameter")]
        public Int32 QueryType { get; set; }

        protected override void ProcessRecord()
        {
            base.ProcessRecord();

            var request = new ValidateSavedQueryRequest();
            request.FetchXml = FetchXml;            request.QueryType = QueryType;
            var response = (ValidateSavedQueryResponse)Connection.Execute(request);
            WriteObject(response);
        }
    }
}
