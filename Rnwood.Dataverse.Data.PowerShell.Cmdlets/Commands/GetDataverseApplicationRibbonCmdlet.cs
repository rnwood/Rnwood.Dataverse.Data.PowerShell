using System;
using System.Management.Automation;
using Microsoft.Crm.Sdk.Messages;
using Microsoft.PowerPlatform.Dataverse.Client;
using Microsoft.Xrm.Sdk;

namespace Rnwood.Dataverse.Data.PowerShell.Commands
{
    [Cmdlet(VerbsCommon.Get, "DataverseApplicationRibbon")]
    [OutputType(typeof(RetrieveApplicationRibbonResponse))]
    ///<summary>Executes RetrieveApplicationRibbonRequest SDK message.</summary>
    public class GetDataverseApplicationRibbonCmdlet : OrganizationServiceCmdlet
    {
        [Parameter(Mandatory = true, HelpMessage = "DataverseConnection instance obtained from Get-DataverseConnection cmdlet")]
        public override ServiceClient Connection { get; set; }

        protected override void ProcessRecord()
        {
            base.ProcessRecord();

            var request = new RetrieveApplicationRibbonRequest();

            var response = (RetrieveApplicationRibbonResponse)Connection.Execute(request);
            WriteObject(response);
        }
    }
}
