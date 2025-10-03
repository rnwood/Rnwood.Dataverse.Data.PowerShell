using System;
using System.Management.Automation;
using Microsoft.Crm.Sdk.Messages;
using Microsoft.PowerPlatform.Dataverse.Client;
using Microsoft.Xrm.Sdk;

namespace Rnwood.Dataverse.Data.PowerShell.Commands
{
    [Cmdlet(VerbsCommon.Remove, "DataverseUserFromRecordTeam", SupportsShouldProcess = true, ConfirmImpact = ConfirmImpact.Medium)]
    [OutputType(typeof(RemoveUserFromRecordTeamResponse))]
    ///<summary>Executes RemoveUserFromRecordTeamRequest SDK message.</summary>
    public class RemoveDataverseUserFromRecordTeamCmdlet : OrganizationServiceCmdlet
    {
        [Parameter(Mandatory = true, HelpMessage = "DataverseConnection instance obtained from Get-DataverseConnection cmdlet")]
        public override ServiceClient Connection { get; set; }
        [Parameter(Mandatory = false, ValueFromPipelineByPropertyName = true, HelpMessage = "Record parameter")]
        public object Record { get; set; }
        [Parameter(Mandatory = false, HelpMessage = "TeamTemplateId parameter")]
        public Guid TeamTemplateId { get; set; }
        [Parameter(Mandatory = false, HelpMessage = "SystemUserId parameter")]
        public Guid SystemUserId { get; set; }

        protected override void ProcessRecord()
        {
            base.ProcessRecord();

            var request = new RemoveUserFromRecordTeamRequest();
            if (Record != null)
            {
                request.Record = DataverseTypeConverter.ToEntityReference(Record, null, "Record");
            }
            request.TeamTemplateId = TeamTemplateId;            request.SystemUserId = SystemUserId;
            if (ShouldProcess("Executing RemoveUserFromRecordTeamRequest", "RemoveUserFromRecordTeamRequest"))
            {
                var response = (RemoveUserFromRecordTeamResponse)Connection.Execute(request);
                WriteObject(response);
            }
        }
    }
}
