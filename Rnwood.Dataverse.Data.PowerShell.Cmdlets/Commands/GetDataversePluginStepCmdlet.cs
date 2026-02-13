using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Query;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Management.Automation;

namespace Rnwood.Dataverse.Data.PowerShell.Commands
{
    /// <summary>
    /// Retrieves SDK message processing step records (plugin steps) from a Dataverse environment.
    /// </summary>
    [Cmdlet(VerbsCommon.Get, "DataversePluginStep", DefaultParameterSetName = "All")]
    [OutputType(typeof(PSObject))]
    public class GetDataversePluginStepCmdlet : OrganizationServiceCmdlet
    {
        /// <summary>
        /// Gets or sets the ID of the plugin step to retrieve.
        /// </summary>
        [Parameter(ParameterSetName = "ById", Mandatory = true, ValueFromPipelineByPropertyName = true, HelpMessage = "ID of the plugin step to retrieve")]
        public Guid Id { get; set; }

        /// <summary>
        /// Gets or sets the name of the plugin step to retrieve.
        /// </summary>
        [Parameter(ParameterSetName = "ByName", Mandatory = true, HelpMessage = "Name of the plugin step to retrieve")]
        public string Name { get; set; }

        /// <summary>
        /// Gets or sets the plugin type ID to filter steps by.
        /// </summary>
        [Parameter(ParameterSetName = "ByPluginType", Mandatory = true, HelpMessage = "Plugin type ID to filter steps by")]
        [Parameter(ParameterSetName = "All", HelpMessage = "Plugin type ID to filter steps by")]
        public Guid? PluginTypeId { get; set; }

        /// <summary>
        /// Process the cmdlet.
        /// </summary>
        protected override void ProcessRecord()
        {
            base.ProcessRecord();

            QueryExpression query = new QueryExpression("sdkmessageprocessingstep")
            {
                ColumnSet = new ColumnSet(true)
            };

            if (ParameterSetName == "ById")
            {
                query.Criteria.AddCondition("sdkmessageprocessingstepid", ConditionOperator.Equal, Id);
            }
            else if (ParameterSetName == "ByName")
            {
                query.Criteria.AddCondition("name", ConditionOperator.Equal, Name);
            }

            if (PluginTypeId.HasValue)
            {
                query.Criteria.AddCondition("plugintypeid", ConditionOperator.Equal, PluginTypeId.Value);
            }

            // Add linked entities to fetch lookup names to avoid N+1 queries
            // Link to plugintype
            LinkEntity pluginTypeLink = new LinkEntity
            {
                LinkFromEntityName = "sdkmessageprocessingstep",
                LinkFromAttributeName = "plugintypeid",
                LinkToEntityName = "plugintype",
                LinkToAttributeName = "plugintypeid",
                EntityAlias = "plugintype_link",
                JoinOperator = JoinOperator.LeftOuter,
                Columns = new ColumnSet("typename")
            };
            query.LinkEntities.Add(pluginTypeLink);

            // Link to sdkmessage
            LinkEntity messageLink = new LinkEntity
            {
                LinkFromEntityName = "sdkmessageprocessingstep",
                LinkFromAttributeName = "sdkmessageid",
                LinkToEntityName = "sdkmessage",
                LinkToAttributeName = "sdkmessageid",
                EntityAlias = "sdkmessage_link",
                JoinOperator = JoinOperator.LeftOuter,
                Columns = new ColumnSet("name")
            };
            query.LinkEntities.Add(messageLink);

            // Link to sdkmessagefilter - note: this entity doesn't have a traditional "name" field
            // We're joining it but not including columns to avoid extra properties
            LinkEntity filterLink = new LinkEntity
            {
                LinkFromEntityName = "sdkmessageprocessingstep",
                LinkFromAttributeName = "sdkmessagefilterid",
                LinkToEntityName = "sdkmessagefilter",
                LinkToAttributeName = "sdkmessagefilterid",
                EntityAlias = "sdkmessagefilter_link",
                JoinOperator = JoinOperator.LeftOuter,
                Columns = new ColumnSet(false) // Don't include any columns to avoid extra properties
            };
            query.LinkEntities.Add(filterLink);

            // Link to organization
            LinkEntity organizationLink = new LinkEntity
            {
                LinkFromEntityName = "sdkmessageprocessingstep",
                LinkFromAttributeName = "organizationid",
                LinkToEntityName = "organization",
                LinkToAttributeName = "organizationid",
                EntityAlias = "organization_link",
                JoinOperator = JoinOperator.LeftOuter,
                Columns = new ColumnSet("name")
            };
            query.LinkEntities.Add(organizationLink);

            EntityMetadataFactory entityMetadataFactory = new EntityMetadataFactory(Connection);
            DataverseEntityConverter converter = new DataverseEntityConverter(Connection, entityMetadataFactory);

            foreach (Entity entity in QueryHelpers.ExecuteQueryWithPaging(query, Connection, WriteVerbose))
            {
                // Populate EntityReference names from linked entity data to avoid N+1 queries
                PopulateEntityReferenceNamesFromLinks(entity, "plugintypeid", "plugintype_link", "typename");
                PopulateEntityReferenceNamesFromLinks(entity, "sdkmessageid", "sdkmessage_link", "name");
                PopulateEntityReferenceNamesFromLinks(entity, "organizationid", "organization_link", "name");
                // Note: sdkmessagefilterid doesn't have a simple name field, so we skip it

                PSObject psObject = converter.ConvertToPSObject(entity, new ColumnSet(true), _ => ValueType.Display, WriteVerbose);
                WriteObject(psObject);
            }
        }

        /// <summary>
        /// Populates an EntityReference Name property from linked entity aliased values.
        /// Removes the aliased value after populating to avoid extra properties in output.
        /// </summary>
        private void PopulateEntityReferenceNamesFromLinks(Entity entity, string referenceAttribute, string linkAlias, string nameAttribute)
        {
            string aliasedAttributeName = $"{linkAlias}.{nameAttribute}";
            
            if (entity.Contains(aliasedAttributeName) && entity.Contains(referenceAttribute))
            {
                var aliasedValue = entity.GetAttributeValue<AliasedValue>(aliasedAttributeName);
                var entityRef = entity.GetAttributeValue<EntityReference>(referenceAttribute);
                
                if (aliasedValue != null && entityRef != null && aliasedValue.Value != null)
                {
                    entityRef.Name = aliasedValue.Value.ToString();
                }
                
                // Remove the aliased value to prevent extra properties in output
                entity.Attributes.Remove(aliasedAttributeName);
            }
        }
    }
}
