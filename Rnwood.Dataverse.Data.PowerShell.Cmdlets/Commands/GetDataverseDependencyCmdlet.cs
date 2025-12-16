using System;
using System.Management.Automation;
using Microsoft.Crm.Sdk.Messages;
using Microsoft.Xrm.Sdk;

namespace Rnwood.Dataverse.Data.PowerShell.Commands
{
    /// <summary>
    /// Retrieves dependencies that prevent a component from being deleted.
    /// </summary>
    [Cmdlet(VerbsCommon.Get, "DataverseDependency")]
    [OutputType(typeof(Entity))]
    public class GetDataverseDependencyCmdlet : OrganizationServiceCmdlet
    {
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
        /// Executes the RetrieveDependenciesForDelete request.
        /// </summary>
        protected override void ProcessRecord()
        {
            base.ProcessRecord();

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
    }
}
