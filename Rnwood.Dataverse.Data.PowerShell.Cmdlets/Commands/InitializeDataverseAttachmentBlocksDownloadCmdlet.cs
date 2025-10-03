using System;
using System.Management.Automation;
using Microsoft.Crm.Sdk.Messages;
using Microsoft.PowerPlatform.Dataverse.Client;
using Microsoft.Xrm.Sdk;

namespace Rnwood.Dataverse.Data.PowerShell.Commands
{
    [Cmdlet(VerbsData.Initialize, "DataverseAttachmentBlocksDownload")]
    [OutputType(typeof(InitializeAttachmentBlocksDownloadResponse))]
    ///<summary>Executes InitializeAttachmentBlocksDownloadRequest SDK message.</summary>
    public class InitializeDataverseAttachmentBlocksDownloadCmdlet : OrganizationServiceCmdlet
    {
        [Parameter(Mandatory = true, HelpMessage = "DataverseConnection instance obtained from Get-DataverseConnection cmdlet")]
        public override ServiceClient Connection { get; set; }
        [Parameter(Mandatory = false, ValueFromPipelineByPropertyName = true, HelpMessage = "Target parameter")]
        public object Target { get; set; }

        protected override void ProcessRecord()
        {
            base.ProcessRecord();

            var request = new InitializeAttachmentBlocksDownloadRequest();
            if (Target != null)
            {
                request.Target = DataverseTypeConverter.ToEntityReference(Target, null, "Target");
            }

            var response = (InitializeAttachmentBlocksDownloadResponse)Connection.Execute(request);
            WriteObject(response);
        }
    }
}
