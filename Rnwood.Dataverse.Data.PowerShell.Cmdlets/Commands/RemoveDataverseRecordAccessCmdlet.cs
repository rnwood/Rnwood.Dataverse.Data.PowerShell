using System;
using System.Management.Automation;
using Microsoft.Crm.Sdk.Messages;
using Microsoft.Xrm.Sdk;

namespace Rnwood.Dataverse.Data.PowerShell.Commands
{
    /// <summary>
    /// Revokes access rights for a security principal (user or team) on a specific record.
    /// </summary>
    [Cmdlet(VerbsCommon.Remove, "DataverseRecordAccess", SupportsShouldProcess = true, ConfirmImpact = ConfirmImpact.Medium)]
    public class RemoveDataverseRecordAccessCmdlet : OrganizationServiceCmdlet
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
        /// Gets or sets the security principal (user or team) for which to revoke access.
        /// </summary>
        [Parameter(Mandatory = true, Position = 2, HelpMessage = "The security principal (user or team) for which to revoke access rights.")]
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

            var target = new EntityReference(TableName, Id);
            string principalType = IsTeam.IsPresent ? "team" : "systemuser";
            var principalRef = new EntityReference(principalType, Principal);

            if (ShouldProcess($"{TableName} {Id}", $"Revoke access from {principalType} {Principal}"))
            {
                var request = new RevokeAccessRequest
                {
                    Target = target,
                    Revokee = principalRef
                };

                Connection.Execute(request);
                WriteVerbose($"Revoked access from {principalType} {Principal} on {TableName} {Id}");
            }
        }
    }
}
