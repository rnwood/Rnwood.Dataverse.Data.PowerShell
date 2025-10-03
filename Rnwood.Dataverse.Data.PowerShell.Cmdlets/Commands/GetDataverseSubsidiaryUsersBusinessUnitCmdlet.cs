using System;
using System.Management.Automation;
using Microsoft.Crm.Sdk.Messages;
using Microsoft.PowerPlatform.Dataverse.Client;
using Microsoft.Xrm.Sdk;

namespace Rnwood.Dataverse.Data.PowerShell.Commands
{
    [Cmdlet(VerbsCommon.Get, "DataverseSubsidiaryUsersBusinessUnit")]
    [OutputType(typeof(RetrieveSubsidiaryUsersBusinessUnitResponse))]
    ///<summary>Executes RetrieveSubsidiaryUsersBusinessUnitRequest SDK message.</summary>
    public class GetDataverseSubsidiaryUsersBusinessUnitCmdlet : OrganizationServiceCmdlet
    {
        [Parameter(Mandatory = true, HelpMessage = "DataverseConnection instance obtained from Get-DataverseConnection cmdlet")]
        public override ServiceClient Connection { get; set; }
        [Parameter(Mandatory = false, HelpMessage = "EntityId parameter")]
        public Guid EntityId { get; set; }
        [Parameter(Mandatory = false, HelpMessage = "ColumnSet parameter")]
        public Microsoft.Xrm.Sdk.Query.ColumnSet ColumnSet { get; set; }

        protected override void ProcessRecord()
        {
            base.ProcessRecord();

            var request = new RetrieveSubsidiaryUsersBusinessUnitRequest();
            request.EntityId = EntityId;            request.ColumnSet = ColumnSet;
            var response = (RetrieveSubsidiaryUsersBusinessUnitResponse)Connection.Execute(request);
            WriteObject(response);
        }
    }
}
