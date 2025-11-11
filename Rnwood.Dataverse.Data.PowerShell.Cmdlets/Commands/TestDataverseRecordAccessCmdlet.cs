using System;
using System.Management.Automation;
using Microsoft.Crm.Sdk.Messages;
using Microsoft.Xrm.Sdk;

namespace Rnwood.Dataverse.Data.PowerShell.Commands
{
    /// <summary>
    /// Tests the access rights a security principal (user or team) has for a specific record.
    /// </summary>
    [Cmdlet(VerbsDiagnostic.Test, "DataverseRecordAccess")]
    [OutputType(typeof(AccessRights))]
    public class TestDataverseRecordAccessCmdlet : OrganizationServiceCmdlet
    {
        /// <summary>
        /// Gets or sets the target entity reference for which to check access.
        /// </summary>
        [Parameter(Mandatory = true, Position = 0, HelpMessage = "The record for which to check access rights.")]
        public EntityReference Target { get; set; }

        /// <summary>
        /// Gets or sets the security principal (user or team) for which to check access.
        /// </summary>
        [Parameter(Mandatory = true, Position = 1, HelpMessage = "The security principal (user or team) for which to check access rights.")]
        public Guid Principal { get; set; }

        /// <summary>
        /// Executes the RetrievePrincipalAccess request.
        /// </summary>
        protected override void BeginProcessing()
        {
            base.BeginProcessing();

            var request = new RetrievePrincipalAccessRequest
            {
                Target = Target,
                Principal = new EntityReference("systemuser", Principal)
            };

            var response = (RetrievePrincipalAccessResponse)Connection.Execute(request);

            WriteObject(response.AccessRights);
        }
    }
}
