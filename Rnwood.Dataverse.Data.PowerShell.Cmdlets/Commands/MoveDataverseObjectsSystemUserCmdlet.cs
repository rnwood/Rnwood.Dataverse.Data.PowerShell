using System;
using System.Management.Automation;
using Microsoft.Crm.Sdk.Messages;
using Microsoft.PowerPlatform.Dataverse.Client;
using Microsoft.Xrm.Sdk;

namespace Rnwood.Dataverse.Data.PowerShell.Commands
{
    [Cmdlet(VerbsCommon.Move, "DataverseObjectsSystemUser")]
    [OutputType(typeof(ReassignObjectsSystemUserResponse))]
    ///<summary>Executes ReassignObjectsSystemUserRequest SDK message.</summary>
    public class MoveDataverseObjectsSystemUserCmdlet : OrganizationServiceCmdlet
    {
        [Parameter(Mandatory = true, HelpMessage = "DataverseConnection instance obtained from Get-DataverseConnection cmdlet")]
        public override ServiceClient Connection { get; set; }
        [Parameter(Mandatory = false, HelpMessage = "UserId parameter")]
        public Guid UserId { get; set; }
        [Parameter(Mandatory = false, ValueFromPipelineByPropertyName = true, HelpMessage = "ReassignPrincipal parameter")]
        public object ReassignPrincipal { get; set; }

        protected override void ProcessRecord()
        {
            base.ProcessRecord();

            var request = new ReassignObjectsSystemUserRequest();
            request.UserId = UserId;            if (ReassignPrincipal != null)
            {
                request.ReassignPrincipal = DataverseTypeConverter.ToEntityReference(ReassignPrincipal, null, "ReassignPrincipal");
            }

            var response = (ReassignObjectsSystemUserResponse)Connection.Execute(request);
            WriteObject(response);
        }
    }
}
