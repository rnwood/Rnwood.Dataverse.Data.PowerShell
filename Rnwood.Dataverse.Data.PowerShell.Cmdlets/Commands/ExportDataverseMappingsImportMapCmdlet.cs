using System;
using System.Management.Automation;
using Microsoft.Crm.Sdk.Messages;
using Microsoft.PowerPlatform.Dataverse.Client;
using Microsoft.Xrm.Sdk;

namespace Rnwood.Dataverse.Data.PowerShell.Commands
{
    [Cmdlet(VerbsData.Export, "DataverseMappingsImportMap")]
    [OutputType(typeof(ExportMappingsImportMapResponse))]
    ///<summary>Executes ExportMappingsImportMapRequest SDK message.</summary>
    public class ExportDataverseMappingsImportMapCmdlet : OrganizationServiceCmdlet
    {
        [Parameter(Mandatory = true, HelpMessage = "DataverseConnection instance obtained from Get-DataverseConnection cmdlet")]
        public override ServiceClient Connection { get; set; }
        [Parameter(Mandatory = false, HelpMessage = "ImportMapId parameter")]
        public Guid ImportMapId { get; set; }
        [Parameter(Mandatory = false, HelpMessage = "ExportIds parameter")]
        public Boolean ExportIds { get; set; }

        protected override void ProcessRecord()
        {
            base.ProcessRecord();

            var request = new ExportMappingsImportMapRequest();
            request.ImportMapId = ImportMapId;            request.ExportIds = ExportIds;
            var response = (ExportMappingsImportMapResponse)Connection.Execute(request);
            WriteObject(response);
        }
    }
}
