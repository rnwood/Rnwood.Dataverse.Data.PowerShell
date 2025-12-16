using System;
using System.Management.Automation;
using Microsoft.Crm.Sdk.Messages;
using Microsoft.Xrm.Sdk;

namespace Rnwood.Dataverse.Data.PowerShell.Commands
{
    /// <summary>
    /// Retrieves solution dependencies in Dataverse.
    /// </summary>
    [Cmdlet(VerbsCommon.Get, "DataverseSolutionDependency")]
    [OutputType(typeof(Entity))]
    public class GetDataverseSolutionDependencyCmdlet : OrganizationServiceCmdlet
    {
        private const string PARAMSET_MISSING = "Missing";
        private const string PARAMSET_UNINSTALL = "Uninstall";

        /// <summary>
        /// Gets or sets the unique name of the solution.
        /// </summary>
        [Parameter(Mandatory = true, Position = 0, ValueFromPipelineByPropertyName = true, HelpMessage = "Unique name of the solution")]
        [Alias("UniqueName")]
        public string SolutionUniqueName { get; set; }

        /// <summary>
        /// Gets or sets whether to retrieve missing dependencies for the solution.
        /// </summary>
        [Parameter(Mandatory = true, ParameterSetName = PARAMSET_MISSING, HelpMessage = "Retrieve missing dependencies for the solution")]
        public SwitchParameter Missing { get; set; }

        /// <summary>
        /// Gets or sets whether to retrieve dependencies that would prevent solution uninstall.
        /// </summary>
        [Parameter(Mandatory = true, ParameterSetName = PARAMSET_UNINSTALL, HelpMessage = "Retrieve dependencies that would prevent solution uninstall")]
        public SwitchParameter Uninstall { get; set; }

        /// <summary>
        /// Executes the appropriate solution dependency request based on parameter set.
        /// </summary>
        protected override void ProcessRecord()
        {
            base.ProcessRecord();

            if (ParameterSetName == PARAMSET_MISSING)
            {
                var request = new RetrieveMissingDependenciesRequest
                {
                    SolutionUniqueName = SolutionUniqueName
                };

                var response = (RetrieveMissingDependenciesResponse)Connection.Execute(request);

                foreach (var entity in response.EntityCollection.Entities)
                {
                    WriteObject(entity);
                }
            }
            else if (ParameterSetName == PARAMSET_UNINSTALL)
            {
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
}
