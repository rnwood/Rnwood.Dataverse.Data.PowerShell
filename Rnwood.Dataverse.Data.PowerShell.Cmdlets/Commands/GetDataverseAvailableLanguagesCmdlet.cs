using System;
using System.Management.Automation;
using Microsoft.Crm.Sdk.Messages;
using Microsoft.PowerPlatform.Dataverse.Client;
using Microsoft.Xrm.Sdk;

namespace Rnwood.Dataverse.Data.PowerShell.Commands
{
    [Cmdlet(VerbsCommon.Get, "DataverseAvailableLanguages")]
    [OutputType(typeof(RetrieveAvailableLanguagesResponse))]
    ///<summary>Executes RetrieveAvailableLanguagesRequest SDK message.</summary>
    public class GetDataverseAvailableLanguagesCmdlet : OrganizationServiceCmdlet
    {
        [Parameter(Mandatory = true, HelpMessage = "DataverseConnection instance obtained from Get-DataverseConnection cmdlet")]
        public override ServiceClient Connection { get; set; }

        protected override void ProcessRecord()
        {
            base.ProcessRecord();

            var request = new RetrieveAvailableLanguagesRequest();

            var response = (RetrieveAvailableLanguagesResponse)Connection.Execute(request);
            WriteObject(response);
        }
    }
}
