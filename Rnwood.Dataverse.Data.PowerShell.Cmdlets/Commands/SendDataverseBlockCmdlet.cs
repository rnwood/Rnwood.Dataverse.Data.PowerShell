using System;
using System.Management.Automation;
using Microsoft.Crm.Sdk.Messages;
using Microsoft.PowerPlatform.Dataverse.Client;
using Microsoft.Xrm.Sdk;

namespace Rnwood.Dataverse.Data.PowerShell.Commands
{
    [Cmdlet(VerbsCommunications.Send, "DataverseBlock", SupportsShouldProcess = true, ConfirmImpact = ConfirmImpact.Medium)]
    [OutputType(typeof(UploadBlockResponse))]
    ///<summary>Executes UploadBlockRequest SDK message.</summary>
    public class SendDataverseBlockCmdlet : OrganizationServiceCmdlet
    {
        [Parameter(Mandatory = true, HelpMessage = "DataverseConnection instance obtained from Get-DataverseConnection cmdlet")]
        public override ServiceClient Connection { get; set; }
        [Parameter(Mandatory = false, HelpMessage = "BlockId parameter")]
        public String BlockId { get; set; }
        [Parameter(Mandatory = false, HelpMessage = "BlockData parameter")]
        public Byte[] BlockData { get; set; }
        [Parameter(Mandatory = false, HelpMessage = "FileContinuationToken parameter")]
        public String FileContinuationToken { get; set; }

        protected override void ProcessRecord()
        {
            base.ProcessRecord();

            var request = new UploadBlockRequest();
            request.BlockId = BlockId;            request.BlockData = BlockData;            request.FileContinuationToken = FileContinuationToken;
            if (ShouldProcess("Executing UploadBlockRequest", "UploadBlockRequest"))
            {
                var response = (UploadBlockResponse)Connection.Execute(request);
                WriteObject(response);
            }
        }
    }
}
