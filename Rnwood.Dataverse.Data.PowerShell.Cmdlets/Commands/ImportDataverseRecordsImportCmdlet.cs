using System;
using System.Management.Automation;
using Microsoft.Crm.Sdk.Messages;
using Microsoft.PowerPlatform.Dataverse.Client;
using Microsoft.Xrm.Sdk;

namespace Rnwood.Dataverse.Data.PowerShell.Commands
{
    [Cmdlet(VerbsData.Import, "DataverseRecordsImport", SupportsShouldProcess = true, ConfirmImpact = ConfirmImpact.Medium)]
    [OutputType(typeof(ImportRecordsImportResponse))]
    ///<summary>Executes ImportRecordsImportRequest SDK message.</summary>
    public class ImportDataverseRecordsImportCmdlet : OrganizationServiceCmdlet
    {
        [Parameter(Mandatory = true, HelpMessage = "DataverseConnection instance obtained from Get-DataverseConnection cmdlet")]
        public override ServiceClient Connection { get; set; }
        [Parameter(Mandatory = false, HelpMessage = "ImportId parameter")]
        public Guid ImportId { get; set; }

        protected override void ProcessRecord()
        {
            base.ProcessRecord();

            var request = new ImportRecordsImportRequest();
            request.ImportId = ImportId;
            if (ShouldProcess("Executing ImportRecordsImportRequest", "ImportRecordsImportRequest"))
            {
                var response = (ImportRecordsImportResponse)Connection.Execute(request);
                WriteObject(response);
            }
        }
    }
}
