using System;
using System.Management.Automation;
using Microsoft.Crm.Sdk.Messages;
using Microsoft.PowerPlatform.Dataverse.Client;
using Microsoft.Xrm.Sdk;

namespace Rnwood.Dataverse.Data.PowerShell.Commands
{
    [Cmdlet(VerbsCommon.Remove, "DataverseFile", SupportsShouldProcess = true, ConfirmImpact = ConfirmImpact.Medium)]
    [OutputType(typeof(DeleteFileResponse))]
    ///<summary>Executes DeleteFileRequest SDK message.</summary>
    public class RemoveDataverseFileCmdlet : OrganizationServiceCmdlet
    {
        [Parameter(Mandatory = true, HelpMessage = "DataverseConnection instance obtained from Get-DataverseConnection cmdlet")]
        public override ServiceClient Connection { get; set; }
        [Parameter(Mandatory = false, HelpMessage = "FileId parameter")]
        public Guid FileId { get; set; }

        protected override void ProcessRecord()
        {
            base.ProcessRecord();

            var request = new DeleteFileRequest();
            request.FileId = FileId;
            if (ShouldProcess("Executing DeleteFileRequest", "DeleteFileRequest"))
            {
                var response = (DeleteFileResponse)Connection.Execute(request);
                WriteObject(response);
            }
        }
    }
}
