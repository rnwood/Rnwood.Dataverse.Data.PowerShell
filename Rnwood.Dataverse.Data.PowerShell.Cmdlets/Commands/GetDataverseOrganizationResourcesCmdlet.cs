using System;
using System.Management.Automation;
using Microsoft.Crm.Sdk.Messages;
using Microsoft.PowerPlatform.Dataverse.Client;
using Microsoft.Xrm.Sdk;

namespace Rnwood.Dataverse.Data.PowerShell.Commands
{
    [Cmdlet(VerbsCommon.Get, "DataverseOrganizationResources")]
    [OutputType(typeof(RetrieveOrganizationResourcesResponse))]
    ///<summary>Executes RetrieveOrganizationResourcesRequest SDK message.</summary>
    public class GetDataverseOrganizationResourcesCmdlet : OrganizationServiceCmdlet
    {
        [Parameter(Mandatory = true, HelpMessage = "DataverseConnection instance obtained from Get-DataverseConnection cmdlet")]
        public override ServiceClient Connection { get; set; }

        protected override void ProcessRecord()
        {
            base.ProcessRecord();

            var request = new RetrieveOrganizationResourcesRequest();

            var response = (RetrieveOrganizationResourcesResponse)Connection.Execute(request);
            WriteObject(response);
        }
    }
}
