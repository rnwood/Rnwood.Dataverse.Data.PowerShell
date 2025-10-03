using System;
using System.Management.Automation;
using Microsoft.Crm.Sdk.Messages;
using Microsoft.PowerPlatform.Dataverse.Client;
using Microsoft.Xrm.Sdk;

namespace Rnwood.Dataverse.Data.PowerShell.Commands
{
    [Cmdlet(VerbsCommon.Set, "DataverseAccess", SupportsShouldProcess = true, ConfirmImpact = ConfirmImpact.Medium)]
    [OutputType(typeof(ModifyAccessResponse))]
    ///<summary>Executes ModifyAccessRequest SDK message.</summary>
    public class SetDataverseAccessCmdlet : OrganizationServiceCmdlet
    {
        [Parameter(Mandatory = true, HelpMessage = "DataverseConnection instance obtained from Get-DataverseConnection cmdlet")]
        public override ServiceClient Connection { get; set; }
        [Parameter(Mandatory = false, ValueFromPipelineByPropertyName = true, HelpMessage = "Target parameter")]
        public object Target { get; set; }
        [Parameter(Mandatory = false, HelpMessage = "PrincipalAccess parameter")]
        public Microsoft.Crm.Sdk.Messages.PrincipalAccess PrincipalAccess { get; set; }

        protected override void ProcessRecord()
        {
            base.ProcessRecord();

            var request = new ModifyAccessRequest();
            if (Target != null)
            {
                request.Target = DataverseTypeConverter.ToEntityReference(Target, null, "Target");
            }
            request.PrincipalAccess = PrincipalAccess;
            if (ShouldProcess("Executing ModifyAccessRequest", "ModifyAccessRequest"))
            {
                var response = (ModifyAccessResponse)Connection.Execute(request);
                WriteObject(response);
            }
        }
    }
}
