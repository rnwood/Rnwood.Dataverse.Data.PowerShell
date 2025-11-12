using System;
using System.Management.Automation;
using Microsoft.Crm.Sdk.Messages;
using Microsoft.Xrm.Sdk;

namespace Rnwood.Dataverse.Data.PowerShell.Commands
{
    /// <summary>
    /// Revokes access rights for a security principal (user or team) on a specific record.
    /// </summary>
    [Cmdlet(VerbsCommon.Remove, "DataverseRecordAccess", SupportsShouldProcess = true, ConfirmImpact = ConfirmImpact.High)]
    public class RemoveDataverseRecordAccessCmdlet : OrganizationServiceCmdlet
    {
        /// <summary>
        /// Gets or sets the target entity reference for which to revoke access.
        /// </summary>
        [Parameter(Mandatory = true, Position = 0, HelpMessage = "The record for which to revoke access rights.")]
        public EntityReference Target { get; set; }

        /// <summary>
        /// Gets or sets the security principal (user or team) for which to revoke access.
        /// </summary>
        [Parameter(Mandatory = true, Position = 1, HelpMessage = "The security principal (user or team) for which to revoke access rights.")]
        public Guid Principal { get; set; }

        /// <summary>
        /// Gets or sets whether this is a team principal (vs. user).
        /// </summary>
        [Parameter(HelpMessage = "Specify if the principal is a team (default is systemuser).")]
        public SwitchParameter IsTeam { get; set; }

        /// <summary>
        /// Executes the RevokeAccess request.
        /// </summary>
        protected override void ProcessRecord()
        {
            base.ProcessRecord();

            string principalType = IsTeam.IsPresent ? "team" : "systemuser";
            var principalRef = new EntityReference(principalType, Principal);

            if (ShouldProcess($"{Target.LogicalName} {Target.Id}", $"Revoke access from {principalType} {Principal}"))
            {
                var request = new RevokeAccessRequest
                {
                    Target = Target,
                    Revokee = principalRef
                };

                Connection.Execute(request);
                WriteVerbose($"Revoked access from {principalType} {Principal} on {Target.LogicalName} {Target.Id}");
            }
        }
    }
}
