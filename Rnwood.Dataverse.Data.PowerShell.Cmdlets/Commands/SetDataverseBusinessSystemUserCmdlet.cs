using System;
using System.Management.Automation;
using Microsoft.Crm.Sdk.Messages;
using Microsoft.PowerPlatform.Dataverse.Client;
using Microsoft.Xrm.Sdk;

namespace Rnwood.Dataverse.Data.PowerShell.Commands
{
    [Cmdlet(VerbsCommon.Set, "DataverseBusinessSystemUser", SupportsShouldProcess = true, ConfirmImpact = ConfirmImpact.Medium)]
    [OutputType(typeof(SetBusinessSystemUserResponse))]
    ///<summary>Executes SetBusinessSystemUserRequest SDK message.</summary>
    public class SetDataverseBusinessSystemUserCmdlet : OrganizationServiceCmdlet
    {
        [Parameter(Mandatory = true, HelpMessage = "DataverseConnection instance obtained from Get-DataverseConnection cmdlet")]
        public override ServiceClient Connection { get; set; }
        [Parameter(Mandatory = false, HelpMessage = "UserId parameter")]
        public Guid UserId { get; set; }
        [Parameter(Mandatory = false, HelpMessage = "BusinessId parameter")]
        public Guid BusinessId { get; set; }
        [Parameter(Mandatory = false, ValueFromPipelineByPropertyName = true, HelpMessage = "ReassignPrincipal parameter")]
        public object ReassignPrincipal { get; set; }
        [Parameter(Mandatory = false, HelpMessage = "DoNotMoveAllRecords parameter")]
        public Boolean DoNotMoveAllRecords { get; set; }

        protected override void ProcessRecord()
        {
            base.ProcessRecord();

            var request = new SetBusinessSystemUserRequest();
            request.UserId = UserId;            request.BusinessId = BusinessId;            if (ReassignPrincipal != null)
            {
                request.ReassignPrincipal = DataverseTypeConverter.ToEntityReference(ReassignPrincipal, null, "ReassignPrincipal");
            }
            request.DoNotMoveAllRecords = DoNotMoveAllRecords;
            if (ShouldProcess("Executing SetBusinessSystemUserRequest", "SetBusinessSystemUserRequest"))
            {
                var response = (SetBusinessSystemUserResponse)Connection.Execute(request);
                WriteObject(response);
            }
        }
    }
}
