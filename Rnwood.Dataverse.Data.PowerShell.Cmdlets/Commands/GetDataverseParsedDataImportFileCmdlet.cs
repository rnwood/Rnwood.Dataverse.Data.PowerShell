using System;
using System.Management.Automation;
using Microsoft.Crm.Sdk.Messages;
using Microsoft.PowerPlatform.Dataverse.Client;
using Microsoft.Xrm.Sdk;

namespace Rnwood.Dataverse.Data.PowerShell.Commands
{
    [Cmdlet(VerbsCommon.Get, "DataverseParsedDataImportFile")]
    [OutputType(typeof(RetrieveParsedDataImportFileResponse))]
    ///<summary>Executes RetrieveParsedDataImportFileRequest SDK message.</summary>
    public class GetDataverseParsedDataImportFileCmdlet : OrganizationServiceCmdlet
    {
        [Parameter(Mandatory = true, HelpMessage = "DataverseConnection instance obtained from Get-DataverseConnection cmdlet")]
        public override ServiceClient Connection { get; set; }
        [Parameter(Mandatory = false, HelpMessage = "ImportFileId parameter")]
        public Guid ImportFileId { get; set; }
        [Parameter(Mandatory = false, HelpMessage = "PagingInfo parameter")]
        public Microsoft.Xrm.Sdk.Query.PagingInfo PagingInfo { get; set; }

        protected override void ProcessRecord()
        {
            base.ProcessRecord();

            var request = new RetrieveParsedDataImportFileRequest();
            request.ImportFileId = ImportFileId;            request.PagingInfo = PagingInfo;
            var response = (RetrieveParsedDataImportFileResponse)Connection.Execute(request);
            WriteObject(response);
        }
    }
}
