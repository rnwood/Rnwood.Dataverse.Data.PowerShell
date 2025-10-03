using System;
using System.Management.Automation;
using Microsoft.Crm.Sdk.Messages;
using Microsoft.PowerPlatform.Dataverse.Client;
using Microsoft.Xrm.Sdk;

namespace Rnwood.Dataverse.Data.PowerShell.Commands
{
    [Cmdlet(VerbsData.Convert, "DataverseImport", SupportsShouldProcess = true, ConfirmImpact = ConfirmImpact.Medium)]
    [OutputType(typeof(TransformImportResponse))]
    ///<summary>Executes TransformImportRequest SDK message.</summary>
    public class ConvertDataverseImportCmdlet : OrganizationServiceCmdlet
    {
        [Parameter(Mandatory = true, HelpMessage = "DataverseConnection instance obtained from Get-DataverseConnection cmdlet")]
        public override ServiceClient Connection { get; set; }
        [Parameter(Mandatory = false, HelpMessage = "ImportId parameter")]
        public Guid ImportId { get; set; }

        protected override void ProcessRecord()
        {
            base.ProcessRecord();

            var request = new TransformImportRequest();
            request.ImportId = ImportId;
            if (ShouldProcess("Executing TransformImportRequest", "TransformImportRequest"))
            {
                var response = (TransformImportResponse)Connection.Execute(request);
                WriteObject(response);
            }
        }
    }
}
