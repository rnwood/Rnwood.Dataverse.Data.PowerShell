using System;
using System.Management.Automation;
using Microsoft.Crm.Sdk.Messages;
using Microsoft.PowerPlatform.Dataverse.Client;
using Microsoft.Xrm.Sdk;

namespace Rnwood.Dataverse.Data.PowerShell.Commands
{
    [Cmdlet(VerbsCommon.Get, "DataverseDeprovisionedLanguages")]
    [OutputType(typeof(RetrieveDeprovisionedLanguagesResponse))]
    ///<summary>Executes RetrieveDeprovisionedLanguagesRequest SDK message.</summary>
    public class GetDataverseDeprovisionedLanguagesCmdlet : OrganizationServiceCmdlet
    {
        [Parameter(Mandatory = true, HelpMessage = "DataverseConnection instance obtained from Get-DataverseConnection cmdlet")]
        public override ServiceClient Connection { get; set; }

        protected override void ProcessRecord()
        {
            base.ProcessRecord();

            var request = new RetrieveDeprovisionedLanguagesRequest();

            var response = (RetrieveDeprovisionedLanguagesResponse)Connection.Execute(request);
            WriteObject(response);
        }
    }
}
