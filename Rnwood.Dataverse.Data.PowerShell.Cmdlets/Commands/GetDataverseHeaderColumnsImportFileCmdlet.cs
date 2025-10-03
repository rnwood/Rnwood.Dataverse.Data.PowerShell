using System;
using System.Management.Automation;
using Microsoft.Crm.Sdk.Messages;
using Microsoft.PowerPlatform.Dataverse.Client;
using Microsoft.Xrm.Sdk;

namespace Rnwood.Dataverse.Data.PowerShell.Commands
{
    [Cmdlet(VerbsCommon.Get, "DataverseHeaderColumnsImportFile")]
    [OutputType(typeof(GetHeaderColumnsImportFileResponse))]
    ///<summary>Executes GetHeaderColumnsImportFileRequest SDK message.</summary>
    public class GetDataverseHeaderColumnsImportFileCmdlet : OrganizationServiceCmdlet
    {
        [Parameter(Mandatory = true, HelpMessage = "DataverseConnection instance obtained from Get-DataverseConnection cmdlet")]
        public override ServiceClient Connection { get; set; }
        [Parameter(Mandatory = false, HelpMessage = "ImportFileId parameter")]
        public Guid ImportFileId { get; set; }

        protected override void ProcessRecord()
        {
            base.ProcessRecord();

            var request = new GetHeaderColumnsImportFileRequest();
            request.ImportFileId = ImportFileId;
            var response = (GetHeaderColumnsImportFileResponse)Connection.Execute(request);
            WriteObject(response);
        }
    }
}
