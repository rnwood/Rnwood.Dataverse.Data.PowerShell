using System;
using System.Management.Automation;
using Microsoft.Crm.Sdk.Messages;
using Microsoft.Xrm.Sdk;

namespace Rnwood.Dataverse.Data.PowerShell.Commands
{
    /// <summary>
    /// Retrieves missing dependencies for a solution.
    /// </summary>
    [Cmdlet(VerbsCommon.Get, "DataverseMissingDependency")]
    [OutputType(typeof(Entity))]
    public class GetDataverseMissingDependencyCmdlet : OrganizationServiceCmdlet
    {
        /// <summary>
        /// Gets or sets the unique name of the solution.
        /// </summary>
        [Parameter(Mandatory = true, Position = 0, ValueFromPipelineByPropertyName = true, HelpMessage = "Unique name of the solution")]
        [Alias("UniqueName")]
        public string SolutionUniqueName { get; set; }

        /// <summary>
        /// Executes the RetrieveMissingDependencies request.
        /// </summary>
        protected override void ProcessRecord()
        {
            base.ProcessRecord();

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
    }
}
