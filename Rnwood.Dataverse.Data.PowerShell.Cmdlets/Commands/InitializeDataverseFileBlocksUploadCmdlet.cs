using System;
using System.Management.Automation;
using Microsoft.Crm.Sdk.Messages;
using Microsoft.PowerPlatform.Dataverse.Client;
using Microsoft.Xrm.Sdk;

namespace Rnwood.Dataverse.Data.PowerShell.Commands
{
    [Cmdlet(VerbsData.Initialize, "DataverseFileBlocksUpload")]
    [OutputType(typeof(InitializeFileBlocksUploadResponse))]
    ///<summary>Executes InitializeFileBlocksUploadRequest SDK message.</summary>
    public class InitializeDataverseFileBlocksUploadCmdlet : OrganizationServiceCmdlet
    {
        [Parameter(Mandatory = true, HelpMessage = "DataverseConnection instance obtained from Get-DataverseConnection cmdlet")]
        public override ServiceClient Connection { get; set; }
        [Parameter(Mandatory = false, ValueFromPipelineByPropertyName = true, HelpMessage = "Target parameter")]
        public object Target { get; set; }
        [Parameter(Mandatory = false, HelpMessage = "FileName parameter")]
        public String FileName { get; set; }
        [Parameter(Mandatory = false, HelpMessage = "FileAttributeName parameter")]
        public String FileAttributeName { get; set; }

        protected override void ProcessRecord()
        {
            base.ProcessRecord();

            var request = new InitializeFileBlocksUploadRequest();
            if (Target != null)
            {
                request.Target = DataverseTypeConverter.ToEntityReference(Target, null, "Target");
            }
            request.FileName = FileName;            request.FileAttributeName = FileAttributeName;
            var response = (InitializeFileBlocksUploadResponse)Connection.Execute(request);
            WriteObject(response);
        }
    }
}
