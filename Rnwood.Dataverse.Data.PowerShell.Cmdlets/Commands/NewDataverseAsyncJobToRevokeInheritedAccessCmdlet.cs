using System;
using System.Management.Automation;
using Microsoft.Crm.Sdk.Messages;
using Microsoft.PowerPlatform.Dataverse.Client;
using Microsoft.Xrm.Sdk;

namespace Rnwood.Dataverse.Data.PowerShell.Commands
{
    [Cmdlet(VerbsCommon.New, "DataverseAsyncJobToRevokeInheritedAccess", SupportsShouldProcess = true, ConfirmImpact = ConfirmImpact.Medium)]
    [OutputType(typeof(CreateAsyncJobToRevokeInheritedAccessResponse))]
    ///<summary>Executes CreateAsyncJobToRevokeInheritedAccessRequest SDK message.</summary>
    public class NewDataverseAsyncJobToRevokeInheritedAccessCmdlet : OrganizationServiceCmdlet
    {
        [Parameter(Mandatory = true, HelpMessage = "DataverseConnection instance obtained from Get-DataverseConnection cmdlet")]
        public override ServiceClient Connection { get; set; }
        [Parameter(Mandatory = false, HelpMessage = "RelationshipSchema parameter")]
        public String RelationshipSchema { get; set; }

        protected override void ProcessRecord()
        {
            base.ProcessRecord();

            var request = new CreateAsyncJobToRevokeInheritedAccessRequest();
            request.RelationshipSchema = RelationshipSchema;
            if (ShouldProcess("Executing CreateAsyncJobToRevokeInheritedAccessRequest", "CreateAsyncJobToRevokeInheritedAccessRequest"))
            {
                var response = (CreateAsyncJobToRevokeInheritedAccessResponse)Connection.Execute(request);
                WriteObject(response);
            }
        }
    }
}
