using System;
using System.Management.Automation;
using Microsoft.Crm.Sdk.Messages;
using Microsoft.PowerPlatform.Dataverse.Client;
using Microsoft.Xrm.Sdk;

namespace Rnwood.Dataverse.Data.PowerShell.Commands
{
    [Cmdlet(VerbsLifecycle.Disable, "DataverseLanguage")]
    [OutputType(typeof(DeprovisionLanguageResponse))]
    ///<summary>Executes DeprovisionLanguageRequest SDK message.</summary>
    public class DisableDataverseLanguageCmdlet : OrganizationServiceCmdlet
    {
        [Parameter(Mandatory = true, HelpMessage = "DataverseConnection instance obtained from Get-DataverseConnection cmdlet")]
        public override ServiceClient Connection { get; set; }
        [Parameter(Mandatory = false, HelpMessage = "Language parameter")]
        public Int32 Language { get; set; }

        protected override void ProcessRecord()
        {
            base.ProcessRecord();

            var request = new DeprovisionLanguageRequest();
            request.Language = Language;
            var response = (DeprovisionLanguageResponse)Connection.Execute(request);
            WriteObject(response);
        }
    }
}
