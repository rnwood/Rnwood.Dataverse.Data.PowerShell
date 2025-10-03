using System;
using System.Management.Automation;
using Microsoft.Crm.Sdk.Messages;
using Microsoft.PowerPlatform.Dataverse.Client;
using Microsoft.Xrm.Sdk;

namespace Rnwood.Dataverse.Data.PowerShell.Commands
{
    [Cmdlet(VerbsCommon.Remove, "DataverseMemberList", SupportsShouldProcess = true, ConfirmImpact = ConfirmImpact.Medium)]
    [OutputType(typeof(RemoveMemberListResponse))]
    ///<summary>Executes RemoveMemberListRequest SDK message.</summary>
    public class RemoveDataverseMemberListCmdlet : OrganizationServiceCmdlet
    {
        [Parameter(Mandatory = true, HelpMessage = "DataverseConnection instance obtained from Get-DataverseConnection cmdlet")]
        public override ServiceClient Connection { get; set; }
        [Parameter(Mandatory = false, HelpMessage = "ListId parameter")]
        public Guid ListId { get; set; }
        [Parameter(Mandatory = false, HelpMessage = "EntityId parameter")]
        public Guid EntityId { get; set; }

        protected override void ProcessRecord()
        {
            base.ProcessRecord();

            var request = new RemoveMemberListRequest();
            request.ListId = ListId;            request.EntityId = EntityId;
            if (ShouldProcess("Executing RemoveMemberListRequest", "RemoveMemberListRequest"))
            {
                var response = (RemoveMemberListResponse)Connection.Execute(request);
                WriteObject(response);
            }
        }
    }
}
