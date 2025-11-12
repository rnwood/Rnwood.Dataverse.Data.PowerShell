using System;
using System.Management.Automation;
using Microsoft.Crm.Sdk.Messages;
using Microsoft.Xrm.Sdk;

namespace Rnwood.Dataverse.Data.PowerShell.Commands
{
    /// <summary>
    /// Grants or modifies access rights for a security principal (user or team) on a specific record.
    /// </summary>
    [Cmdlet(VerbsCommon.Set, "DataverseRecordAccess", SupportsShouldProcess = true, ConfirmImpact = ConfirmImpact.Medium)]
    public class SetDataverseRecordAccessCmdlet : OrganizationServiceCmdlet
    {
        /// <summary>
        /// Gets or sets the logical name of the table.
        /// </summary>
        [Parameter(Mandatory = true, Position = 0, ValueFromPipelineByPropertyName = true, HelpMessage = "Logical name of table")]
        [Alias("EntityName")]
        [ArgumentCompleter(typeof(TableNameArgumentCompleter))]
        public string TableName { get; set; }

        /// <summary>
        /// Gets or sets the Id of the record.
        /// </summary>
        [Parameter(Mandatory = true, Position = 1, ValueFromPipelineByPropertyName = true, HelpMessage = "Id of record")]
        public Guid Id { get; set; }

        /// <summary>
        /// Gets or sets the security principal (user or team) for which to set access.
        /// </summary>
        [Parameter(Mandatory = true, Position = 2, HelpMessage = "The security principal (user or team) for which to set access rights.")]
        public Guid Principal { get; set; }

        /// <summary>
        /// Gets or sets the access rights to grant.
        /// </summary>
        [Parameter(Mandatory = true, Position = 3, HelpMessage = "The access rights to grant (e.g., ReadAccess, WriteAccess, DeleteAccess, ShareAccess, AssignAccess).")]
        public AccessRights AccessRights { get; set; }

        /// <summary>
        /// Gets or sets whether this is a team principal (vs. user).
        /// </summary>
        [Parameter(HelpMessage = "Specify if the principal is a team (default is systemuser).")]
        public SwitchParameter IsTeam { get; set; }

        /// <summary>
        /// If specified, replaces all existing access rights with the specified rights. Otherwise, adds the specified rights to existing access.
        /// </summary>
        [Parameter(HelpMessage = "If specified, replaces all existing access rights with the specified rights. Otherwise, adds the specified rights to existing access.")]
        public SwitchParameter Replace { get; set; }

        /// <summary>
        /// Executes the GrantAccess or ModifyAccess request.
        /// </summary>
        protected override void ProcessRecord()
        {
            base.ProcessRecord();

            var target = new EntityReference(TableName, Id);
            string principalType = IsTeam.IsPresent ? "team" : "systemuser";
            var principalRef = new EntityReference(principalType, Principal);

            if (ShouldProcess($"{TableName} {Id}", $"Set access {AccessRights} for {principalType} {Principal}"))
            {
                AccessRights effectiveAccessRights = AccessRights;

                // If not replacing, get current access and add new rights
                if (!Replace.IsPresent)
                {
                    try
                    {
                        var retrieveRequest = new RetrievePrincipalAccessRequest
                        {
                            Target = target,
                            Principal = principalRef
                        };
                        var retrieveResponse = (RetrievePrincipalAccessResponse)Connection.Execute(retrieveRequest);
                        effectiveAccessRights = retrieveResponse.AccessRights | AccessRights;
                        WriteVerbose($"Current access: {retrieveResponse.AccessRights}, adding: {AccessRights}, effective: {effectiveAccessRights}");
                    }
                    catch
                    {
                        // If no existing access, use the specified rights
                        WriteVerbose($"No existing access found, using specified rights: {AccessRights}");
                    }
                }
                else
                {
                    WriteVerbose($"Replace mode: setting access to exactly {AccessRights}");
                }

                // Try to grant access. If access already exists, modify it.
                try
                {
                    var grantRequest = new GrantAccessRequest
                    {
                        Target = target,
                        PrincipalAccess = new PrincipalAccess
                        {
                            Principal = principalRef,
                            AccessMask = effectiveAccessRights
                        }
                    };

                    Connection.Execute(grantRequest);
                    WriteVerbose($"Granted access {effectiveAccessRights} to {principalType} {Principal} on {TableName} {Id}");
                }
                catch (Exception ex) when (ex.Message.Contains("already has access"))
                {
                    // If access already exists, modify it
                    // ModifyAccessRequest replaces the entire access mask
                    var modifyRequest = new ModifyAccessRequest
                    {
                        Target = target,
                        PrincipalAccess = new PrincipalAccess
                        {
                            Principal = principalRef,
                            AccessMask = effectiveAccessRights
                        }
                    };

                    Connection.Execute(modifyRequest);
                    WriteVerbose($"Modified access to {effectiveAccessRights} for {principalType} {Principal} on {TableName} {Id}");
                }
            }
        }
    }
}
