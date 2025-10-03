using System;
using System.Management.Automation;
using Microsoft.Crm.Sdk.Messages;
using Microsoft.PowerPlatform.Dataverse.Client;
using Microsoft.Xrm.Sdk;

namespace Rnwood.Dataverse.Data.PowerShell.Commands
{
    [Cmdlet(VerbsCommon.Get, "DataverseEntityRibbon")]
    [OutputType(typeof(RetrieveEntityRibbonResponse))]
    ///<summary>Executes RetrieveEntityRibbonRequest SDK message.</summary>
    public class GetDataverseEntityRibbonCmdlet : OrganizationServiceCmdlet
    {
        [Parameter(Mandatory = true, HelpMessage = "DataverseConnection instance obtained from Get-DataverseConnection cmdlet")]
        public override ServiceClient Connection { get; set; }
        [Parameter(Mandatory = false, HelpMessage = "EntityName parameter")]
        public String EntityName { get; set; }
        [Parameter(Mandatory = false, HelpMessage = "RibbonLocationFilter parameter")]
        public Microsoft.Crm.Sdk.Messages.RibbonLocationFilters RibbonLocationFilter { get; set; }

        protected override void ProcessRecord()
        {
            base.ProcessRecord();

            var request = new RetrieveEntityRibbonRequest();
            request.EntityName = EntityName;            request.RibbonLocationFilter = RibbonLocationFilter;
            var response = (RetrieveEntityRibbonResponse)Connection.Execute(request);
            WriteObject(response);
        }
    }
}
