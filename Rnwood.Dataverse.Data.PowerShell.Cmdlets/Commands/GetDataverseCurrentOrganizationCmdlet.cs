using System;
using System.Management.Automation;
using Microsoft.Crm.Sdk.Messages;
using Microsoft.PowerPlatform.Dataverse.Client;
using Microsoft.Xrm.Sdk;

namespace Rnwood.Dataverse.Data.PowerShell.Commands
{
    [Cmdlet(VerbsCommon.Get, "DataverseCurrentOrganization")]
    [OutputType(typeof(RetrieveCurrentOrganizationResponse))]
    ///<summary>Executes RetrieveCurrentOrganizationRequest SDK message.</summary>
    public class GetDataverseCurrentOrganizationCmdlet : OrganizationServiceCmdlet
    {
        [Parameter(Mandatory = true, HelpMessage = "DataverseConnection instance obtained from Get-DataverseConnection cmdlet")]
        public override ServiceClient Connection { get; set; }
        [Parameter(Mandatory = false, HelpMessage = "AccessType parameter")]
        public Microsoft.Xrm.Sdk.Organization.EndpointAccessType AccessType { get; set; }

        protected override void ProcessRecord()
        {
            base.ProcessRecord();

            var request = new RetrieveCurrentOrganizationRequest();
            request.AccessType = AccessType;
            var response = (RetrieveCurrentOrganizationResponse)Connection.Execute(request);
            WriteObject(response);
        }
    }
}
