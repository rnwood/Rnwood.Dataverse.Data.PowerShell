using System;
using System.Management.Automation;
using Microsoft.Crm.Sdk.Messages;
using Microsoft.PowerPlatform.Dataverse.Client;
using Microsoft.Xrm.Sdk;

namespace Rnwood.Dataverse.Data.PowerShell.Commands
{
    [Cmdlet(VerbsData.Import, "DataverseMappingsImportMap", SupportsShouldProcess = true, ConfirmImpact = ConfirmImpact.Medium)]
    [OutputType(typeof(ImportMappingsImportMapResponse))]
    ///<summary>Executes ImportMappingsImportMapRequest SDK message.</summary>
    public class ImportDataverseMappingsImportMapCmdlet : OrganizationServiceCmdlet
    {
        [Parameter(Mandatory = true, HelpMessage = "DataverseConnection instance obtained from Get-DataverseConnection cmdlet")]
        public override ServiceClient Connection { get; set; }
        [Parameter(Mandatory = false, HelpMessage = "MappingsXml parameter")]
        public String MappingsXml { get; set; }
        [Parameter(Mandatory = false, HelpMessage = "ReplaceIds parameter")]
        public Boolean ReplaceIds { get; set; }

        protected override void ProcessRecord()
        {
            base.ProcessRecord();

            var request = new ImportMappingsImportMapRequest();
            request.MappingsXml = MappingsXml;            request.ReplaceIds = ReplaceIds;
            if (ShouldProcess("Executing ImportMappingsImportMapRequest", "ImportMappingsImportMapRequest"))
            {
                var response = (ImportMappingsImportMapResponse)Connection.Execute(request);
                WriteObject(response);
            }
        }
    }
}
