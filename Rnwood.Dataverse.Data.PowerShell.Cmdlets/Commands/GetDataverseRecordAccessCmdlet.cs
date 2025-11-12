using System;
using System.Management.Automation;
using Microsoft.Crm.Sdk.Messages;
using Microsoft.Xrm.Sdk;

namespace Rnwood.Dataverse.Data.PowerShell.Commands
{
    /// <summary>
    /// Retrieves all principals (users or teams) who have access to a specific record.
    /// </summary>
    [Cmdlet(VerbsCommon.Get, "DataverseRecordAccess")]
    [OutputType(typeof(PrincipalAccess))]
    public class GetDataverseRecordAccessCmdlet : OrganizationServiceCmdlet
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
        /// Executes the RetrieveSharedPrincipalsAndAccess request.
        /// </summary>
        protected override void ProcessRecord()
        {
            base.ProcessRecord();

            var target = new EntityReference(TableName, Id);

            var request = new RetrieveSharedPrincipalsAndAccessRequest
            {
                Target = target
            };

            var response = (RetrieveSharedPrincipalsAndAccessResponse)Connection.Execute(request);

            foreach (var principalAccess in response.PrincipalAccesses)
            {
                WriteObject(principalAccess);
            }
        }
    }
}
