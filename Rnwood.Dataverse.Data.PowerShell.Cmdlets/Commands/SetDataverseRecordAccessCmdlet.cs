using System;
using System.Management.Automation;
using Microsoft.Crm.Sdk.Messages;
using Microsoft.Xrm.Sdk;

namespace Rnwood.Dataverse.Data.PowerShell.Commands
{
    /// <summary>
    /// Grants or modifies access rights for a security principal (user or team) on a specific record.
    /// </summary>
    [Cmdlet(VerbsCommon.Set, "DataverseRecordAccess", SupportsShouldProcess = true)]
    public class SetDataverseRecordAccessCmdlet : OrganizationServiceCmdlet
    {
        /// <summary>
        /// Gets or sets the target entity reference for which to set access.
        /// </summary>
        [Parameter(Mandatory = true, Position = 0, HelpMessage = "The record for which to set access rights.")]
        public EntityReference Target { get; set; }

        /// <summary>
        /// Gets or sets the security principal (user or team) for which to set access.
        /// </summary>
        [Parameter(Mandatory = true, Position = 1, HelpMessage = "The security principal (user or team) for which to set access rights.")]
        public Guid Principal { get; set; }

        /// <summary>
        /// Gets or sets the access rights to grant.
        /// </summary>
        [Parameter(Mandatory = true, Position = 2, HelpMessage = "The access rights to grant (e.g., ReadAccess, WriteAccess, DeleteAccess, ShareAccess, AssignAccess).")]
        public AccessRights AccessRights { get; set; }

        /// <summary>
        /// Gets or sets whether this is a team principal (vs. user).
        /// </summary>
        [Parameter(HelpMessage = "Specify if the principal is a team (default is systemuser).")]
        public SwitchParameter IsTeam { get; set; }

        /// <summary>
        /// Executes the GrantAccess or ModifyAccess request.
        /// </summary>
        protected override void ProcessRecord()
        {
            base.ProcessRecord();

            string principalType = IsTeam.IsPresent ? "team" : "systemuser";
            var principalRef = new EntityReference(principalType, Principal);

            if (ShouldProcess($"{Target.LogicalName} {Target.Id}", $"Grant access {AccessRights} to {principalType} {Principal}"))
            {
                // Try to grant access. If access already exists, modify it.
                try
                {
                    var grantRequest = new GrantAccessRequest
                    {
                        Target = Target,
                        PrincipalAccess = new PrincipalAccess
                        {
                            Principal = principalRef,
                            AccessMask = AccessRights
                        }
                    };

                    Connection.Execute(grantRequest);
                    WriteVerbose($"Granted access {AccessRights} to {principalType} {Principal} on {Target.LogicalName} {Target.Id}");
                }
                catch (Exception ex) when (ex.Message.Contains("already has access"))
                {
                    // If access already exists, modify it
                    var modifyRequest = new ModifyAccessRequest
                    {
                        Target = Target,
                        PrincipalAccess = new PrincipalAccess
                        {
                            Principal = principalRef,
                            AccessMask = AccessRights
                        }
                    };

                    Connection.Execute(modifyRequest);
                    WriteVerbose($"Modified access to {AccessRights} for {principalType} {Principal} on {Target.LogicalName} {Target.Id}");
                }
            }
        }
    }
}
