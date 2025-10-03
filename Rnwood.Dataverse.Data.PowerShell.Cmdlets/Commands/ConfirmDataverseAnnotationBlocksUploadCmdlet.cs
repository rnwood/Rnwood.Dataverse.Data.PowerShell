using System;
using System.Management.Automation;
using Microsoft.Crm.Sdk.Messages;
using Microsoft.PowerPlatform.Dataverse.Client;
using Microsoft.Xrm.Sdk;

namespace Rnwood.Dataverse.Data.PowerShell.Commands
{
    [Cmdlet(VerbsLifecycle.Confirm, "DataverseAnnotationBlocksUpload")]
    [OutputType(typeof(CommitAnnotationBlocksUploadResponse))]
    ///<summary>Executes CommitAnnotationBlocksUploadRequest SDK message.</summary>
    public class ConfirmDataverseAnnotationBlocksUploadCmdlet : OrganizationServiceCmdlet
    {
        [Parameter(Mandatory = true, HelpMessage = "DataverseConnection instance obtained from Get-DataverseConnection cmdlet")]
        public override ServiceClient Connection { get; set; }
        [Parameter(Mandatory = false, HelpMessage = "Target parameter")]
        public PSObject Target { get; set; }
        [Parameter(Mandatory = false, HelpMessage = "BlockList parameter")]
        public String[] BlockList { get; set; }
        [Parameter(Mandatory = false, HelpMessage = "FileContinuationToken parameter")]
        public String FileContinuationToken { get; set; }

        protected override void ProcessRecord()
        {
            base.ProcessRecord();

            var request = new CommitAnnotationBlocksUploadRequest();
            if (Target != null)
            {
                var entity = new Entity();
                foreach (PSPropertyInfo prop in Target.Properties)
                {
                    if (prop.Name != "Id" && prop.Name != "TableName" && prop.Name != "LogicalName")
                    {
                        entity[prop.Name] = prop.Value;
                    }
                }
                request.Target = entity;
            }
            request.BlockList = BlockList;            request.FileContinuationToken = FileContinuationToken;
            var response = (CommitAnnotationBlocksUploadResponse)Connection.Execute(request);
            WriteObject(response);
        }
    }
}
