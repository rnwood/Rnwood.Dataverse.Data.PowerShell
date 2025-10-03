using System;
using System.Management.Automation;
using Microsoft.Crm.Sdk.Messages;
using Microsoft.PowerPlatform.Dataverse.Client;
using Microsoft.Xrm.Sdk;

namespace Rnwood.Dataverse.Data.PowerShell.Commands
{
    [Cmdlet(VerbsData.Export, "DataverseTranslation")]
    [OutputType(typeof(ExportTranslationResponse))]
    ///<summary>Executes ExportTranslationRequest SDK message.</summary>
    public class ExportDataverseTranslationCmdlet : OrganizationServiceCmdlet
    {
        [Parameter(Mandatory = true, HelpMessage = "DataverseConnection instance obtained from Get-DataverseConnection cmdlet")]
        public override ServiceClient Connection { get; set; }
        [Parameter(Mandatory = false, HelpMessage = "SolutionName parameter")]
        public String SolutionName { get; set; }

        protected override void ProcessRecord()
        {
            base.ProcessRecord();

            var request = new ExportTranslationRequest();
            request.SolutionName = SolutionName;
            var response = (ExportTranslationResponse)Connection.Execute(request);
            WriteObject(response);
        }
    }
}
