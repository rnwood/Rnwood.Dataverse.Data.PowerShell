using System;
using System.Management.Automation;
using Microsoft.Crm.Sdk.Messages;
using Microsoft.Xrm.Sdk;

namespace Rnwood.Dataverse.Data.PowerShell.Commands
{
    /// <summary>
    /// Retrieves component dependencies in Dataverse.
    /// </summary>
    [Cmdlet(VerbsCommon.Get, "DataverseComponentDependency")]
    [OutputType(typeof(Entity))]
    public class GetDataverseComponentDependencyCmdlet : OrganizationServiceCmdlet
    {
        private const string PARAMSET_REQUIREDBY = "RequiredBy";
        private const string PARAMSET_DEPENDENT = "Dependent";

        /// <summary>
        /// Gets or sets the unique identifier of the component.
        /// </summary>
        [Parameter(Mandatory = true, Position = 0, ValueFromPipelineByPropertyName = true, HelpMessage = "Unique identifier of the component")]
        [Alias("ComponentId", "MetadataId")]
        public Guid ObjectId { get; set; }

        /// <summary>
        /// Gets or sets the component type. Common types: 1=Entity, 2=Attribute, 9=OptionSet, 10=Relationship, 24=Form, 26=View, 29=WebResource, 60=Chart, 80=Process.
        /// </summary>
        [Parameter(Mandatory = true, Position = 1, ValueFromPipelineByPropertyName = true, HelpMessage = "Component type (1=Entity, 2=Attribute, 9=OptionSet, 10=Relationship, 24=Form, 26=View, 29=WebResource, 60=Chart, 80=Process)")]
        public int ComponentType { get; set; }

        /// <summary>
        /// Gets or sets whether to retrieve dependencies that would prevent deletion (components that require this component).
        /// </summary>
        [Parameter(Mandatory = true, ParameterSetName = PARAMSET_REQUIREDBY, HelpMessage = "Retrieve dependencies that would prevent deletion (components that require this component)")]
        public SwitchParameter RequiredBy { get; set; }

        /// <summary>
        /// Gets or sets whether to retrieve components that depend on this component.
        /// </summary>
        [Parameter(Mandatory = true, ParameterSetName = PARAMSET_DEPENDENT, HelpMessage = "Retrieve components that depend on this component")]
        public SwitchParameter Dependent { get; set; }

        /// <summary>
        /// Executes the appropriate dependency request based on parameter set.
        /// </summary>
        protected override void ProcessRecord()
        {
            base.ProcessRecord();

            if (ParameterSetName == PARAMSET_REQUIREDBY)
            {
                var request = new RetrieveDependenciesForDeleteRequest
                {
                    ObjectId = ObjectId,
                    ComponentType = ComponentType
                };

                var response = (RetrieveDependenciesForDeleteResponse)Connection.Execute(request);

                foreach (var entity in response.EntityCollection.Entities)
                {
                    WriteObject(entity);
                }
            }
            else if (ParameterSetName == PARAMSET_DEPENDENT)
            {
                var request = new RetrieveDependentComponentsRequest
                {
                    ObjectId = ObjectId,
                    ComponentType = ComponentType
                };

                var response = (RetrieveDependentComponentsResponse)Connection.Execute(request);

                foreach (var entity in response.EntityCollection.Entities)
                {
                    WriteObject(entity);
                }
            }
        }
    }
}
