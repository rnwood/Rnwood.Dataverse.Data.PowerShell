using System;
using System.Management.Automation;
using Microsoft.Crm.Sdk.Messages;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Query;

namespace Rnwood.Dataverse.Data.PowerShell.Commands
{
    /// <summary>
    /// Retrieves solution dependencies in Dataverse.
    /// </summary>
    [Cmdlet(VerbsCommon.Get, "DataverseSolutionDependency")]
    [OutputType(typeof(PSObject))]
    public class GetDataverseSolutionDependencyCmdlet : OrganizationServiceCmdlet
    {
        private const string PARAMSET_MISSING_BYNAME = "Missing-ByName";
        private const string PARAMSET_MISSING_BYID = "Missing-ById";
        private const string PARAMSET_UNINSTALL_BYNAME = "Uninstall-ByName";
        private const string PARAMSET_UNINSTALL_BYID = "Uninstall-ById";

        /// <summary>
        /// Gets or sets the unique name of the solution.
        /// </summary>
        [Parameter(Mandatory = true, Position = 0, ValueFromPipelineByPropertyName = true, ParameterSetName = PARAMSET_MISSING_BYNAME, HelpMessage = "Unique name of the solution")]
        [Parameter(Mandatory = true, Position = 0, ValueFromPipelineByPropertyName = true, ParameterSetName = PARAMSET_UNINSTALL_BYNAME, HelpMessage = "Unique name of the solution")]
        [Alias("UniqueName")]
        public string SolutionUniqueName { get; set; }

        /// <summary>
        /// Gets or sets the ID of the solution.
        /// </summary>
        [Parameter(Mandatory = true, Position = 0, ValueFromPipelineByPropertyName = true, ParameterSetName = PARAMSET_MISSING_BYID, HelpMessage = "ID of the solution")]
        [Parameter(Mandatory = true, Position = 0, ValueFromPipelineByPropertyName = true, ParameterSetName = PARAMSET_UNINSTALL_BYID, HelpMessage = "ID of the solution")]
        public Guid SolutionId { get; set; }

        /// <summary>
        /// Gets or sets whether to retrieve missing dependencies for the solution.
        /// </summary>
        [Parameter(Mandatory = true, ParameterSetName = PARAMSET_MISSING_BYNAME, HelpMessage = "Retrieve missing dependencies for the solution")]
        [Parameter(Mandatory = true, ParameterSetName = PARAMSET_MISSING_BYID, HelpMessage = "Retrieve missing dependencies for the solution")]
        public SwitchParameter Missing { get; set; }

        /// <summary>
        /// Gets or sets whether to retrieve dependencies that would prevent solution uninstall.
        /// </summary>
        [Parameter(Mandatory = true, ParameterSetName = PARAMSET_UNINSTALL_BYNAME, HelpMessage = "Retrieve dependencies that would prevent solution uninstall")]
        [Parameter(Mandatory = true, ParameterSetName = PARAMSET_UNINSTALL_BYID, HelpMessage = "Retrieve dependencies that would prevent solution uninstall")]
        public SwitchParameter Uninstall { get; set; }

        /// <summary>
        /// Executes the appropriate solution dependency request based on parameter set.
        /// </summary>
        protected override void ProcessRecord()
        {
            base.ProcessRecord();

            var entityMetadataFactory = new EntityMetadataFactory(Connection);
            var entityConverter = new DataverseEntityConverter(Connection, entityMetadataFactory);

            // Get solution unique name if ID was provided
            string solutionUniqueName = SolutionUniqueName;
            if (ParameterSetName == PARAMSET_MISSING_BYID || ParameterSetName == PARAMSET_UNINSTALL_BYID)
            {
                var query = new QueryExpression("solution")
                {
                    ColumnSet = new ColumnSet("uniquename"),
                    Criteria = new FilterExpression
                    {
                        Conditions =
                        {
                            new ConditionExpression("solutionid", ConditionOperator.Equal, SolutionId)
                        }
                    }
                };
                var result = Connection.RetrieveMultiple(query);
                if (result.Entities.Count == 0)
                {
                    ThrowTerminatingError(new ErrorRecord(new Exception($"Solution with ID '{SolutionId}' not found."), "SolutionNotFound", ErrorCategory.ObjectNotFound, SolutionId));
                }
                solutionUniqueName = result.Entities[0].GetAttributeValue<string>("uniquename");
            }

            if (ParameterSetName == PARAMSET_MISSING_BYNAME || ParameterSetName == PARAMSET_MISSING_BYID)
            {
                var request = new RetrieveMissingDependenciesRequest
                {
                    SolutionUniqueName = solutionUniqueName
                };

                var response = (RetrieveMissingDependenciesResponse)Connection.Execute(request);

                foreach (var entity in response.EntityCollection.Entities)
                {
                    WriteObject(ConvertEntityToPSObject(entity, entityConverter));
                }
            }
            else if (ParameterSetName == PARAMSET_UNINSTALL_BYNAME || ParameterSetName == PARAMSET_UNINSTALL_BYID)
            {
                var request = new RetrieveDependenciesForUninstallRequest
                {
                    SolutionUniqueName = solutionUniqueName
                };

                var response = (RetrieveDependenciesForUninstallResponse)Connection.Execute(request);

                foreach (var entity in response.EntityCollection.Entities)
                {
                    WriteObject(ConvertEntityToPSObject(entity, entityConverter));
                }
            }
        }

        private PSObject ConvertEntityToPSObject(Entity entity, DataverseEntityConverter entityConverter)
        {
            // For system entities like 'dependency' and 'missingdependency', metadata may not be available
            // Convert directly from entity attributes without requiring metadata
            PSObject result = new PSObject();
            
            result.Properties.Add(new PSNoteProperty("Id", entity.Id));
            result.Properties.Add(new PSNoteProperty("LogicalName", entity.LogicalName));
            
            foreach (var attribute in entity.Attributes)
            {
                result.Properties.Add(new PSNoteProperty(attribute.Key, attribute.Value));
            }
            
            return result;
        }
    }
}
