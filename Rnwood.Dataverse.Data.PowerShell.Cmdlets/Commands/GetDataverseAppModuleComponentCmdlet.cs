using Microsoft.Crm.Sdk.Messages;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Messages;
using Microsoft.Xrm.Sdk.Query;
using System;
using System.Linq;
using System.Management.Automation;
using Rnwood.Dataverse.Data.PowerShell.Commands.Model;

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
        public Guid AppModuleId { get; set; }

        /// <summary>
        /// Gets or sets the app module unique name to filter components by.
        /// </summary>
        [Parameter(ValueFromPipelineByPropertyName = true, HelpMessage = "Filter components by app module unique name.")]
        public string AppModuleUniqueName { get; set; }

        /// <summary>
        /// Gets or sets the object ID (component entity record ID) to filter by.
        /// </summary>
        [Parameter(ValueFromPipelineByPropertyName = true, HelpMessage = "Filter components by object ID (the ID of the component entity record).")]
        public Guid ObjectId { get; set; }

        /// <summary>
        /// Gets or sets the component type to filter by.
        /// </summary>
        [Parameter(HelpMessage = "Filter components by component type (Entity, View, BusinessProcessFlow, RibbonCommand, Chart, Form, SiteMap)")]
        public AppModuleComponentType? ComponentType { get; set; }

        /// <summary>
        /// Gets or sets whether to return raw values instead of display values.
        /// </summary>
        [Parameter(HelpMessage = "Return raw values instead of display values")]
        public SwitchParameter Raw { get; set; }

        /// <summary>
        /// Processes the cmdlet request.
        /// </summary>
        protected override void ProcessRecord()
        {
            base.ProcessRecord();

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

            if (AppModuleId != Guid.Empty)
            {
                var appModuleQuery = new QueryExpression("appmodule")
                {
                    ColumnSet = new ColumnSet("appmoduleidunique"),
                    Criteria = new FilterExpression
                    {
                        Conditions =
                        {
                            new ConditionExpression("appmoduleid", ConditionOperator.Equal, AppModuleId)
                        }
                    }
                };

                // Try published appmodules first
                var response = Connection.Execute(new RetrieveMultipleRequest { Query = appModuleQuery });
                var entities = ((RetrieveMultipleResponse)response).EntityCollection.Entities;

                // If not found, try unpublished appmodules
                if (entities.Count == 0)
                {
                    response = Connection.Execute(new RetrieveUnpublishedMultipleRequest { Query = appModuleQuery });
                    entities = ((RetrieveUnpublishedMultipleResponse)response).EntityCollection.Entities;
                }

                if (entities.Count == 0)
                {
                    throw new Exception($"App module with ID {AppModuleId} not found.");
                }

                Guid appModuleIdUnique = entities[0].GetAttributeValue<Guid>("appmoduleidunique");
                query.Criteria.AddCondition("appmoduleidunique", ConditionOperator.Equal, appModuleIdUnique);
                WriteVerbose($"Filtering by app module ID: {AppModuleId}");
            }

            if (!string.IsNullOrEmpty(AppModuleUniqueName))
            {
                var appModuleQuery = new QueryExpression("appmodule")
                {
                    ColumnSet = new ColumnSet("appmoduleidunique"),
                    Criteria = new FilterExpression
                    {
                        Conditions =
                        {
                            new ConditionExpression("uniquename", ConditionOperator.Equal, AppModuleUniqueName)
                        }
                    }
                };

                // Try published appmodules first
                var response = Connection.Execute(new RetrieveMultipleRequest { Query = appModuleQuery });
                var entities = ((RetrieveMultipleResponse)response).EntityCollection.Entities;

                // If not found, try unpublished appmodules
                if (entities.Count == 0)
                {
                    response = Connection.Execute(new RetrieveUnpublishedMultipleRequest { Query = appModuleQuery });
                    entities = ((RetrieveUnpublishedMultipleResponse)response).EntityCollection.Entities;
                }

                if (entities.Count == 0)
                {
                    throw new Exception($"App module with unique name {AppModuleUniqueName} not found.");
                }

                Guid appModuleIdUnique = entities[0].GetAttributeValue<Guid>("appmoduleidunique");
                query.Criteria.AddCondition("appmoduleidunique", ConditionOperator.Equal, appModuleIdUnique);
                WriteVerbose($"Filtering by app module unique name: {AppModuleUniqueName}");
            }

            if (ObjectId != Guid.Empty)
            {
                query.Criteria.AddCondition("objectid", ConditionOperator.Equal, ObjectId);
                WriteVerbose($"Filtering by object ID: {ObjectId}");
            }

            if (ComponentType.HasValue)
            {
                query.Criteria.AddCondition("componenttype", ConditionOperator.Equal, (int)ComponentType.Value);
                WriteVerbose($"Filtering by component type: {ComponentType.Value}");
            }

            // Execute query with paging
            WriteVerbose("Executing query for appmodulecomponent");
            var components = QueryHelpers.ExecuteQueryWithPaging(query, Connection, WriteVerbose);

            WriteVerbose($"Found {components.Count()} app module component(s)");

            // Convert to PSObjects and output with streaming
            foreach (var component in components)
            {
                if (Raw.IsPresent)
                {
                    WriteObject(component);
                }
                else
                {
                    // Create PSObject with commonly used properties
                    PSObject psObject = new PSObject();

                    // Add normalized Id property for easier pipeline usage
                    var id = component.GetAttributeValue<Guid?>("appmodulecomponentid");
                    psObject.Properties.Add(new PSNoteProperty("Id", id));

                    // Add key properties
                    var appModuleId = component.GetAttributeValue<EntityReference>("appmoduleidunique")?.Id;
                    psObject.Properties.Add(new PSNoteProperty("AppModuleId", appModuleId));

                    var objectId = component.GetAttributeValue<Guid?>("objectid");
                    psObject.Properties.Add(new PSNoteProperty("ObjectId", objectId));

                    var rootAppModuleComponentId = component.GetAttributeValue<Guid?>("rootappmodulecomponentid");
                    psObject.Properties.Add(new PSNoteProperty("RootAppModuleComponentId", rootAppModuleComponentId));

                    var componentType = component.GetAttributeValue<OptionSetValue>("componenttype")?.Value;
                    var componentTypeEnum = AppModuleComponentTypeExtensions.FromInt(componentType);
                    psObject.Properties.Add(new PSNoteProperty("ComponentType", componentTypeEnum));

                    var rootComponentBehavior = component.GetAttributeValue<OptionSetValue>("rootcomponentbehavior")?.Value;
                    var behaviorEnum = RootComponentBehaviorExtensions.FromInt(rootComponentBehavior);
                    psObject.Properties.Add(new PSNoteProperty("RootComponentBehavior", behaviorEnum));

                    var isDefault = component.GetAttributeValue<bool?>("isdefault");
                    psObject.Properties.Add(new PSNoteProperty("IsDefault", isDefault));

                    var isMetadata = component.GetAttributeValue<bool?>("ismetadata");
                    psObject.Properties.Add(new PSNoteProperty("IsMetadata", isMetadata));

                    WriteObject(psObject);
                }
            }
        }
    }
}
