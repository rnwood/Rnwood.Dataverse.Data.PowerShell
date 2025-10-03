using System;
using System.Management.Automation;
using Microsoft.Crm.Sdk.Messages;
using Microsoft.PowerPlatform.Dataverse.Client;
using Microsoft.Xrm.Sdk;

namespace Rnwood.Dataverse.Data.PowerShell.Commands
{
    [Cmdlet(VerbsData.Import, "DataverseTranslationAsync", SupportsShouldProcess = true, ConfirmImpact = ConfirmImpact.Medium)]
    [OutputType(typeof(ImportTranslationAsyncResponse))]
    ///<summary>Executes ImportTranslationAsyncRequest SDK message.</summary>
    public class ImportDataverseTranslationAsyncCmdlet : OrganizationServiceCmdlet
    {
        [Parameter(Mandatory = true, HelpMessage = "DataverseConnection instance obtained from Get-DataverseConnection cmdlet")]
        public override ServiceClient Connection { get; set; }
        [Parameter(Mandatory = false, HelpMessage = "TranslationFile parameter")]
        public Byte[] TranslationFile { get; set; }
        [Parameter(Mandatory = false, HelpMessage = "ImportJobId parameter")]
        public Guid ImportJobId { get; set; }

        protected override void ProcessRecord()
        {
            base.ProcessRecord();

            var request = new ImportTranslationAsyncRequest();
            request.TranslationFile = TranslationFile;            request.ImportJobId = ImportJobId;
            if (ShouldProcess("Executing ImportTranslationAsyncRequest", "ImportTranslationAsyncRequest"))
            {
                var response = (ImportTranslationAsyncResponse)Connection.Execute(request);
                WriteObject(response);
            }
        }
    }
}
