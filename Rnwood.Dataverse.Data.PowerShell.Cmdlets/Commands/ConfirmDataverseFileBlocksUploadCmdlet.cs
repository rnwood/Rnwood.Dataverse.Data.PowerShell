using System;
using System.Management.Automation;
using Microsoft.Crm.Sdk.Messages;
using Microsoft.PowerPlatform.Dataverse.Client;
using Microsoft.Xrm.Sdk;

namespace Rnwood.Dataverse.Data.PowerShell.Commands
{
    [Cmdlet(VerbsLifecycle.Confirm, "DataverseFileBlocksUpload")]
    [OutputType(typeof(CommitFileBlocksUploadResponse))]
    ///<summary>Executes CommitFileBlocksUploadRequest SDK message.</summary>
    public class ConfirmDataverseFileBlocksUploadCmdlet : OrganizationServiceCmdlet
    {
        [Parameter(Mandatory = true, HelpMessage = "DataverseConnection instance obtained from Get-DataverseConnection cmdlet")]
        public override ServiceClient Connection { get; set; }
        [Parameter(Mandatory = false, HelpMessage = "FileName parameter")]
        public String FileName { get; set; }
        [Parameter(Mandatory = false, HelpMessage = "MimeType parameter")]
        public String MimeType { get; set; }
        [Parameter(Mandatory = false, HelpMessage = "BlockList parameter")]
        public String[] BlockList { get; set; }
        [Parameter(Mandatory = false, HelpMessage = "FileContinuationToken parameter")]
        public String FileContinuationToken { get; set; }

        protected override void ProcessRecord()
        {
            base.ProcessRecord();

            var request = new CommitFileBlocksUploadRequest();
            request.FileName = FileName;            request.MimeType = MimeType;            request.BlockList = BlockList;            request.FileContinuationToken = FileContinuationToken;
            var response = (CommitFileBlocksUploadResponse)Connection.Execute(request);
            WriteObject(response);
        }
    }
}
