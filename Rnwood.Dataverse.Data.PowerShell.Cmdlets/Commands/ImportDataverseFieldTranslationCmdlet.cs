using System;
using System.Management.Automation;
using Microsoft.Crm.Sdk.Messages;
using Microsoft.PowerPlatform.Dataverse.Client;
using Microsoft.Xrm.Sdk;

namespace Rnwood.Dataverse.Data.PowerShell.Commands
{
    [Cmdlet(VerbsData.Import, "DataverseFieldTranslation", SupportsShouldProcess = true, ConfirmImpact = ConfirmImpact.Medium)]
    [OutputType(typeof(ImportFieldTranslationResponse))]
    ///<summary>Executes ImportFieldTranslationRequest SDK message.</summary>
    public class ImportDataverseFieldTranslationCmdlet : OrganizationServiceCmdlet
    {
        [Parameter(Mandatory = true, HelpMessage = "DataverseConnection instance obtained from Get-DataverseConnection cmdlet")]
        public override ServiceClient Connection { get; set; }
        [Parameter(Mandatory = false, HelpMessage = "TranslationFile parameter")]
        public Byte[] TranslationFile { get; set; }

        protected override void ProcessRecord()
        {
            base.ProcessRecord();

            var request = new ImportFieldTranslationRequest();
            request.TranslationFile = TranslationFile;
            if (ShouldProcess("Executing ImportFieldTranslationRequest", "ImportFieldTranslationRequest"))
            {
                var response = (ImportFieldTranslationResponse)Connection.Execute(request);
                WriteObject(response);
            }
        }
    }
}
