using System;
using System.Management.Automation;
using Microsoft.Crm.Sdk.Messages;
using Microsoft.PowerPlatform.Dataverse.Client;
using Microsoft.Xrm.Sdk;

namespace Rnwood.Dataverse.Data.PowerShell.Commands
{
    [Cmdlet(VerbsCommon.Get, "DataverseFormattedImportJobResults")]
    [OutputType(typeof(RetrieveFormattedImportJobResultsResponse))]
    ///<summary>Executes RetrieveFormattedImportJobResultsRequest SDK message.</summary>
    public class GetDataverseFormattedImportJobResultsCmdlet : OrganizationServiceCmdlet
    {
        [Parameter(Mandatory = true, HelpMessage = "DataverseConnection instance obtained from Get-DataverseConnection cmdlet")]
        public override ServiceClient Connection { get; set; }
        [Parameter(Mandatory = false, HelpMessage = "ImportJobId parameter")]
        public Guid ImportJobId { get; set; }

        protected override void ProcessRecord()
        {
            base.ProcessRecord();

            var request = new RetrieveFormattedImportJobResultsRequest();
            request.ImportJobId = ImportJobId;
            var response = (RetrieveFormattedImportJobResultsResponse)Connection.Execute(request);
            WriteObject(response);
        }
    }
}
