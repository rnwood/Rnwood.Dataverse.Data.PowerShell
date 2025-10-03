using System;
using System.Management.Automation;
using Microsoft.Crm.Sdk.Messages;
using Microsoft.PowerPlatform.Dataverse.Client;
using Microsoft.Xrm.Sdk;

namespace Rnwood.Dataverse.Data.PowerShell.Commands
{
    [Cmdlet(VerbsCommon.Get, "DataverseUserSettingsSystemUser")]
    [OutputType(typeof(RetrieveUserSettingsSystemUserResponse))]
    ///<summary>Executes RetrieveUserSettingsSystemUserRequest SDK message.</summary>
    public class GetDataverseUserSettingsSystemUserCmdlet : OrganizationServiceCmdlet
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

            var request = new RetrieveUserSettingsSystemUserRequest();
            request.EntityId = EntityId;            request.ColumnSet = ColumnSet;
            var response = (RetrieveUserSettingsSystemUserResponse)Connection.Execute(request);
            WriteObject(response);
        }
    }
}
