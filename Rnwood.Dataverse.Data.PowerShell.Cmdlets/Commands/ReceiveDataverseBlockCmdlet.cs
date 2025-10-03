using System;
using System.Management.Automation;
using Microsoft.Crm.Sdk.Messages;
using Microsoft.PowerPlatform.Dataverse.Client;
using Microsoft.Xrm.Sdk;

namespace Rnwood.Dataverse.Data.PowerShell.Commands
{
    [Cmdlet(VerbsCommunications.Receive, "DataverseBlock")]
    [OutputType(typeof(DownloadBlockResponse))]
    ///<summary>Executes DownloadBlockRequest SDK message.</summary>
    public class ReceiveDataverseBlockCmdlet : OrganizationServiceCmdlet
    {
        [Parameter(Mandatory = true, HelpMessage = "DataverseConnection instance obtained from Get-DataverseConnection cmdlet")]
        public override ServiceClient Connection { get; set; }
        [Parameter(Mandatory = false, HelpMessage = "Offset parameter")]
        public Int64 Offset { get; set; }
        [Parameter(Mandatory = false, HelpMessage = "BlockLength parameter")]
        public Int64 BlockLength { get; set; }
        [Parameter(Mandatory = false, HelpMessage = "FileContinuationToken parameter")]
        public String FileContinuationToken { get; set; }

        protected override void ProcessRecord()
        {
            base.ProcessRecord();

            var request = new DownloadBlockRequest();
            request.Offset = Offset;            request.BlockLength = BlockLength;            request.FileContinuationToken = FileContinuationToken;
            var response = (DownloadBlockResponse)Connection.Execute(request);
            WriteObject(response);
        }
    }
}
