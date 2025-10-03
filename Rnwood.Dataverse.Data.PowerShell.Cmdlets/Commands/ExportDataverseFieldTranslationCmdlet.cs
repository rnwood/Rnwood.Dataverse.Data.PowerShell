using System;
using System.Management.Automation;
using Microsoft.Crm.Sdk.Messages;
using Microsoft.PowerPlatform.Dataverse.Client;
using Microsoft.Xrm.Sdk;

namespace Rnwood.Dataverse.Data.PowerShell.Commands
{
    [Cmdlet(VerbsData.Export, "DataverseFieldTranslation")]
    [OutputType(typeof(ExportFieldTranslationResponse))]
    ///<summary>Executes ExportFieldTranslationRequest SDK message.</summary>
    public class ExportDataverseFieldTranslationCmdlet : OrganizationServiceCmdlet
    {
        [Parameter(Mandatory = true, HelpMessage = "DataverseConnection instance obtained from Get-DataverseConnection cmdlet")]
        public override ServiceClient Connection { get; set; }

        protected override void ProcessRecord()
        {
            base.ProcessRecord();

            var request = new ExportFieldTranslationRequest();

            var response = (ExportFieldTranslationResponse)Connection.Execute(request);
            WriteObject(response);
        }
    }
}
