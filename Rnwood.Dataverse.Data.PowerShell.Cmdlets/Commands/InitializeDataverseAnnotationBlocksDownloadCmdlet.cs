using System;
using System.Management.Automation;
using Microsoft.Crm.Sdk.Messages;
using Microsoft.PowerPlatform.Dataverse.Client;
using Microsoft.Xrm.Sdk;

namespace Rnwood.Dataverse.Data.PowerShell.Commands
{
    [Cmdlet(VerbsData.Initialize, "DataverseAnnotationBlocksDownload")]
    [OutputType(typeof(InitializeAnnotationBlocksDownloadResponse))]
    ///<summary>Executes InitializeAnnotationBlocksDownloadRequest SDK message.</summary>
    public class InitializeDataverseAnnotationBlocksDownloadCmdlet : OrganizationServiceCmdlet
    {
        [Parameter(Mandatory = true, HelpMessage = "DataverseConnection instance obtained from Get-DataverseConnection cmdlet")]
        public override ServiceClient Connection { get; set; }
        [Parameter(Mandatory = false, ValueFromPipelineByPropertyName = true, HelpMessage = "Target parameter")]
        public object Target { get; set; }

        protected override void ProcessRecord()
        {
            base.ProcessRecord();

            var request = new InitializeAnnotationBlocksDownloadRequest();
            if (Target != null)
            {
                request.Target = DataverseTypeConverter.ToEntityReference(Target, null, "Target");
            }

            var response = (InitializeAnnotationBlocksDownloadResponse)Connection.Execute(request);
            WriteObject(response);
        }
    }
}
