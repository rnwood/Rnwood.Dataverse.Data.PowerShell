using System;
using System.Management.Automation;
using Microsoft.Crm.Sdk.Messages;
using Microsoft.Xrm.Sdk;

namespace Rnwood.Dataverse.Data.PowerShell.Commands
{
    /// <summary>
    /// Retrieves dependencies that prevent a solution from being uninstalled.
    /// </summary>
    [Cmdlet(VerbsCommon.Get, "DataverseUninstallDependency")]
    [OutputType(typeof(Entity))]
    public class GetDataverseUninstallDependencyCmdlet : OrganizationServiceCmdlet
    {
        /// <summary>
        /// Gets or sets the unique name of the solution.
        /// </summary>
        [Parameter(Mandatory = true, Position = 0, ValueFromPipelineByPropertyName = true, HelpMessage = "Unique name of the solution")]
        [Alias("UniqueName")]
        public string SolutionUniqueName { get; set; }

        /// <summary>
        /// Executes the RetrieveDependenciesForUninstall request.
        /// </summary>
        protected override void ProcessRecord()
        {
            base.ProcessRecord();

            var request = new RetrieveDependenciesForUninstallRequest
            {
                SolutionUniqueName = SolutionUniqueName
            };

            var response = (RetrieveDependenciesForUninstallResponse)Connection.Execute(request);

            foreach (var entity in response.EntityCollection.Entities)
            {
                WriteObject(entity);
            }
        }
    }
}
