using System;
using System.Management.Automation;
using Microsoft.Crm.Sdk.Messages;
using Microsoft.PowerPlatform.Dataverse.Client;
using Microsoft.Xrm.Sdk;

namespace Rnwood.Dataverse.Data.PowerShell.Commands
{
    [Cmdlet(VerbsData.ConvertFrom, "DataverseImport")]
    [OutputType(typeof(ParseImportResponse))]
    ///<summary>Executes ParseImportRequest SDK message.</summary>
    public class ConvertFromDataverseImportCmdlet : OrganizationServiceCmdlet
    {
        [Parameter(Mandatory = true, HelpMessage = "DataverseConnection instance obtained from Get-DataverseConnection cmdlet")]
        public override ServiceClient Connection { get; set; }
        [Parameter(Mandatory = false, HelpMessage = "ImportId parameter")]
        public Guid ImportId { get; set; }

        protected override void ProcessRecord()
        {
            base.ProcessRecord();

            var request = new ParseImportRequest();
            request.ImportId = ImportId;
            var response = (ParseImportResponse)Connection.Execute(request);
            WriteObject(response);
        }
    }
}
