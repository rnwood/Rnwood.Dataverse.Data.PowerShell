using System;
using System.Management.Automation;
using Microsoft.Crm.Sdk.Messages;
using Microsoft.PowerPlatform.Dataverse.Client;
using Microsoft.Xrm.Sdk;

namespace Rnwood.Dataverse.Data.PowerShell.Commands
{
    [Cmdlet(VerbsCommon.Find, "DataverseExpressionToFetchXml")]
    [OutputType(typeof(QueryExpressionToFetchXmlResponse))]
    ///<summary>Executes QueryExpressionToFetchXmlRequest SDK message.</summary>
    public class FindDataverseExpressionToFetchXmlCmdlet : OrganizationServiceCmdlet
    {
        [Parameter(Mandatory = true, HelpMessage = "DataverseConnection instance obtained from Get-DataverseConnection cmdlet")]
        public override ServiceClient Connection { get; set; }
        [Parameter(Mandatory = false, HelpMessage = "Query parameter")]
        public Microsoft.Xrm.Sdk.Query.QueryBase Query { get; set; }

        protected override void ProcessRecord()
        {
            base.ProcessRecord();

            var request = new QueryExpressionToFetchXmlRequest();
            request.Query = Query;
            var response = (QueryExpressionToFetchXmlResponse)Connection.Execute(request);
            WriteObject(response);
        }
    }
}
