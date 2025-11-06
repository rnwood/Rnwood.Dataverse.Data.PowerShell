using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Query;
using System;
using System.Linq;
using System.Management.Automation;

namespace Rnwood.Dataverse.Data.PowerShell.Commands
{
    /// <summary>
    /// Retrieves app module component information from a Dataverse environment.
    /// </summary>
    [Cmdlet(VerbsCommon.Get, "DataverseAppModuleComponent")]
    [OutputType(typeof(PSObject))]
    public class GetDataverseAppModuleComponentCmdlet : OrganizationServiceCmdlet
    {
        /// <summary>
        /// Gets or sets the ID of the app module component to retrieve.
        /// </summary>
        [Parameter(Position = 0, ValueFromPipelineByPropertyName = true, HelpMessage = "The ID of the app module component to retrieve.")]
        public Guid Id { get; set; }

        /// <summary>
        /// Gets or sets the app module ID to filter components by.
        /// </summary>
        [Parameter(ValueFromPipelineByPropertyName = true, HelpMessage = "Filter components by app module ID.")]
        [Alias("AppModuleId")]
        public Guid AppModuleIdValue { get; set; }

        /// <summary>
        /// Gets or sets the object ID (component entity record ID) to filter by.
        /// </summary>
        [Parameter(ValueFromPipelineByPropertyName = true, HelpMessage = "Filter components by object ID (the ID of the component entity record).")]
        public Guid ObjectId { get; set; }

        /// <summary>
        /// Gets or sets the component type to filter by.
        /// </summary>
        [Parameter(HelpMessage = "Filter components by component type (1=Entity, 29=Business Process Flow, 60=Chart, 62=Sitemap, etc.)")]
        public int? ComponentType { get; set; }

        /// <summary>
        /// Gets or sets whether to return raw values instead of display values.
        /// </summary>
        [Parameter(HelpMessage = "Return raw values instead of display values")]
        public SwitchParameter Raw { get; set; }

        private DataverseEntityConverter entityConverter;
        private EntityMetadataFactory entityMetadataFactory;

        /// <summary>
        /// Processes the cmdlet request.
        /// </summary>
        protected override void ProcessRecord()
        {
            base.ProcessRecord();

            entityMetadataFactory = new EntityMetadataFactory(Connection);
            entityConverter = new DataverseEntityConverter(Connection, entityMetadataFactory);

            WriteVerbose("Querying app module components (appmodulecomponent)...");
            QueryAppModuleComponents();
        }

        private void QueryAppModuleComponents()
        {
            // Build query
            var query = new QueryExpression("appmodulecomponent")
            {
                ColumnSet = new ColumnSet(true) // Get all columns
            };

            // Add filters
            if (Id != Guid.Empty)
            {
                query.Criteria.AddCondition("appmodulecomponentid", ConditionOperator.Equal, Id);
                WriteVerbose($"Filtering by ID: {Id}");
            }

            if (AppModuleIdValue != Guid.Empty)
            {
                query.Criteria.AddCondition("appmoduleidunique", ConditionOperator.Equal, AppModuleIdValue);
                WriteVerbose($"Filtering by app module ID: {AppModuleIdValue}");
            }

            if (ObjectId != Guid.Empty)
            {
                query.Criteria.AddCondition("objectid", ConditionOperator.Equal, ObjectId);
                WriteVerbose($"Filtering by object ID: {ObjectId}");
            }

            if (ComponentType.HasValue)
            {
                query.Criteria.AddCondition("componenttype", ConditionOperator.Equal, ComponentType.Value);
                WriteVerbose($"Filtering by component type: {ComponentType.Value}");
            }

            // Execute query with paging
            WriteVerbose("Executing query for appmodulecomponent");
            var components = QueryHelpers.ExecuteQueryWithPaging(query, Connection, WriteVerbose);

            WriteVerbose($"Found {components.Count()} app module component(s)");

            // Convert to PSObjects and output with streaming
            foreach (var component in components)
            {
                PSObject psObject;

                if (Raw.IsPresent)
                {
                    // Return raw values
                    psObject = entityConverter.ConvertToPSObject(component, new ColumnSet(true), _ => ValueType.Raw);
                }
                else
                {
                    // Create PSObject with commonly used properties
                    psObject = new PSObject();

                    // Add normalized Id property for easier pipeline usage
                    if (component.Attributes.TryGetValue("appmodulecomponentid", out var idValue))
                    {
                        psObject.Properties.Add(new PSNoteProperty("Id", idValue));
                    }

                    // Add key properties
                    if (component.Attributes.TryGetValue("appmoduleidunique", out var appModuleIdValue))
                    {
                        psObject.Properties.Add(new PSNoteProperty("AppModuleId", appModuleIdValue));
                    }
                    if (component.Attributes.TryGetValue("objectid", out var objectIdValue))
                    {
                        psObject.Properties.Add(new PSNoteProperty("ObjectId", objectIdValue));
                    }
                    if (component.Attributes.TryGetValue("componenttype", out var componentTypeValue))
                    {
                        psObject.Properties.Add(new PSNoteProperty("ComponentType", componentTypeValue));
                    }
                    if (component.Attributes.TryGetValue("rootcomponentbehavior", out var rootComponentBehaviorValue))
                    {
                        psObject.Properties.Add(new PSNoteProperty("RootComponentBehavior", rootComponentBehaviorValue));
                    }
                    if (component.Attributes.TryGetValue("isdefault", out var isDefaultValue))
                    {
                        psObject.Properties.Add(new PSNoteProperty("IsDefault", isDefaultValue));
                    }
                    if (component.Attributes.TryGetValue("ismetadata", out var isMetadataValue))
                    {
                        psObject.Properties.Add(new PSNoteProperty("IsMetadata", isMetadataValue));
                    }
                }

                WriteObject(psObject);
            }
        }
    }
}
