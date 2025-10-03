using System;
using System.Management.Automation;
using Microsoft.Crm.Sdk.Messages;
using Microsoft.PowerPlatform.Dataverse.Client;
using Microsoft.Xrm.Sdk;

namespace Rnwood.Dataverse.Data.PowerShell.Commands
{
    [Cmdlet(VerbsCommon.Get, "DataverseDistinctValuesImportFile")]
    [OutputType(typeof(GetDistinctValuesImportFileResponse))]
    ///<summary>Executes GetDistinctValuesImportFileRequest SDK message.</summary>
    public class GetDataverseDistinctValuesImportFileCmdlet : OrganizationServiceCmdlet
    {
        [Parameter(Mandatory = true, HelpMessage = "DataverseConnection instance obtained from Get-DataverseConnection cmdlet")]
        public override ServiceClient Connection { get; set; }
        [Parameter(Mandatory = false, HelpMessage = "ImportFileId parameter")]
        public Guid ImportFileId { get; set; }
        [Parameter(Mandatory = false, HelpMessage = "columnNumber parameter")]
        public Int32 columnNumber { get; set; }
        [Parameter(Mandatory = false, HelpMessage = "pageNumber parameter")]
        public Int32 pageNumber { get; set; }
        [Parameter(Mandatory = false, HelpMessage = "recordsPerPage parameter")]
        public Int32 recordsPerPage { get; set; }

        protected override void ProcessRecord()
        {
            base.ProcessRecord();

            var request = new GetDistinctValuesImportFileRequest();
            request.ImportFileId = ImportFileId;            request.columnNumber = columnNumber;            request.pageNumber = pageNumber;            request.recordsPerPage = recordsPerPage;
            var response = (GetDistinctValuesImportFileResponse)Connection.Execute(request);
            WriteObject(response);
        }
    }
}
