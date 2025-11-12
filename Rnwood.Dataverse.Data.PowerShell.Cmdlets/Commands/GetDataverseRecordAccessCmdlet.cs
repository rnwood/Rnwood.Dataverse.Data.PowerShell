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
        /// Gets or sets the target entity reference for which to retrieve access.
        /// </summary>
        [Parameter(Mandatory = true, Position = 0, ValueFromPipeline = true, HelpMessage = "The record for which to retrieve access rights.")]
        public EntityReference Target { get; set; }

        /// <summary>
        /// Executes the RetrieveSharedPrincipalsAndAccess request.
        /// </summary>
        protected override void ProcessRecord()
        {
            base.ProcessRecord();

            var request = new RetrieveSharedPrincipalsAndAccessRequest
            {
                Target = Target
            };

            var response = (RetrieveSharedPrincipalsAndAccessResponse)Connection.Execute(request);

            foreach (var principalAccess in response.PrincipalAccesses)
            {
                WriteObject(principalAccess);
            }
        }
    }
}
