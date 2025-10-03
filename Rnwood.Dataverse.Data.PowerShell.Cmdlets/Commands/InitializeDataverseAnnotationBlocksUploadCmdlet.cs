using System;
using System.Management.Automation;
using Microsoft.Crm.Sdk.Messages;
using Microsoft.PowerPlatform.Dataverse.Client;
using Microsoft.Xrm.Sdk;

namespace Rnwood.Dataverse.Data.PowerShell.Commands
{
    [Cmdlet(VerbsData.Initialize, "DataverseAnnotationBlocksUpload")]
    [OutputType(typeof(InitializeAnnotationBlocksUploadResponse))]
    ///<summary>Executes InitializeAnnotationBlocksUploadRequest SDK message.</summary>
    public class InitializeDataverseAnnotationBlocksUploadCmdlet : OrganizationServiceCmdlet
    {
        [Parameter(Mandatory = true, HelpMessage = "DataverseConnection instance obtained from Get-DataverseConnection cmdlet")]
        public override ServiceClient Connection { get; set; }
        [Parameter(Mandatory = false, HelpMessage = "Target parameter")]
        public PSObject Target { get; set; }

        protected override void ProcessRecord()
        {
            base.ProcessRecord();

            var request = new InitializeAnnotationBlocksUploadRequest();
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

            var response = (InitializeAnnotationBlocksUploadResponse)Connection.Execute(request);
            WriteObject(response);
        }
    }
}
