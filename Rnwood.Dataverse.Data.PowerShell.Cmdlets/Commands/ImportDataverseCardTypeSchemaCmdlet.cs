using System;
using System.Management.Automation;
using Microsoft.Crm.Sdk.Messages;
using Microsoft.PowerPlatform.Dataverse.Client;
using Microsoft.Xrm.Sdk;

namespace Rnwood.Dataverse.Data.PowerShell.Commands
{
    [Cmdlet(VerbsData.Import, "DataverseCardTypeSchema", SupportsShouldProcess = true, ConfirmImpact = ConfirmImpact.Medium)]
    [OutputType(typeof(ImportCardTypeSchemaResponse))]
    ///<summary>Executes ImportCardTypeSchemaRequest SDK message.</summary>
    public class ImportDataverseCardTypeSchemaCmdlet : OrganizationServiceCmdlet
    {
        [Parameter(Mandatory = true, HelpMessage = "DataverseConnection instance obtained from Get-DataverseConnection cmdlet")]
        public override ServiceClient Connection { get; set; }
        [Parameter(Mandatory = false, HelpMessage = "SolutionUniqueName parameter")]
        public String SolutionUniqueName { get; set; }

        protected override void ProcessRecord()
        {
            base.ProcessRecord();

            var request = new ImportCardTypeSchemaRequest();
            request.SolutionUniqueName = SolutionUniqueName;
            if (ShouldProcess("Executing ImportCardTypeSchemaRequest", "ImportCardTypeSchemaRequest"))
            {
                var response = (ImportCardTypeSchemaResponse)Connection.Execute(request);
                WriteObject(response);
            }
        }
    }
}
