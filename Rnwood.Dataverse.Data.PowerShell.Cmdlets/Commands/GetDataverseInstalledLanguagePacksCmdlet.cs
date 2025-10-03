using System;
using System.Management.Automation;
using Microsoft.Crm.Sdk.Messages;
using Microsoft.PowerPlatform.Dataverse.Client;
using Microsoft.Xrm.Sdk;

namespace Rnwood.Dataverse.Data.PowerShell.Commands
{
    [Cmdlet(VerbsCommon.Get, "DataverseInstalledLanguagePacks")]
    [OutputType(typeof(RetrieveInstalledLanguagePacksResponse))]
    ///<summary>Executes RetrieveInstalledLanguagePacksRequest SDK message.</summary>
    public class GetDataverseInstalledLanguagePacksCmdlet : OrganizationServiceCmdlet
    {
        [Parameter(Mandatory = true, HelpMessage = "DataverseConnection instance obtained from Get-DataverseConnection cmdlet")]
        public override ServiceClient Connection { get; set; }

        protected override void ProcessRecord()
        {
            base.ProcessRecord();

            var request = new RetrieveInstalledLanguagePacksRequest();

            var response = (RetrieveInstalledLanguagePacksResponse)Connection.Execute(request);
            WriteObject(response);
        }
    }
}
