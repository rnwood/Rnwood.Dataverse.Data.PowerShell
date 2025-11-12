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
        /// Gets or sets the security principal (user or team) for which to check access.
        /// </summary>
        [Parameter(Mandatory = true, Position = 2, HelpMessage = "The security principal (user or team) for which to check access rights.")]
        public Guid Principal { get; set; }

        /// <summary>
        /// Executes the RetrievePrincipalAccess request.
        /// </summary>
        protected override void BeginProcessing()
        {
            base.BeginProcessing();

            var target = new EntityReference(TableName, Id);

            var request = new RetrievePrincipalAccessRequest
            {
                Target = target,
                Principal = new EntityReference("systemuser", Principal)
            };

            var response = (RetrievePrincipalAccessResponse)Connection.Execute(request);

            WriteObject(response.AccessRights);
        }
    }
}
