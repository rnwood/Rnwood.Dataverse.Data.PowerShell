using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Query;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Management.Automation;

namespace Rnwood.Dataverse.Data.PowerShell.Commands
{
    /// <summary>
    /// Retrieves plugin type records from a Dataverse environment.
    /// </summary>
    [Cmdlet(VerbsCommon.Get, "DataversePluginType", DefaultParameterSetName = "All")]
    [OutputType(typeof(PSObject))]
    public class GetDataversePluginTypeCmdlet : OrganizationServiceCmdlet
    {
        /// <summary>
        /// Gets or sets the ID of the plugin type to retrieve.
        /// </summary>
        [Parameter(ParameterSetName = "ById", Mandatory = true, ValueFromPipelineByPropertyName = true, HelpMessage = "ID of the plugin type to retrieve")]
        public Guid Id { get; set; }

        /// <summary>
        /// Gets or sets the type name of the plugin type to retrieve.
        /// </summary>
        [Parameter(ParameterSetName = "ByName", Mandatory = true, HelpMessage = "Type name of the plugin type to retrieve")]
        public string TypeName { get; set; }

        /// <summary>
        /// Gets or sets the plugin assembly ID to filter types by.
        /// </summary>
        [Parameter(ParameterSetName = "ByAssembly", Mandatory = true, HelpMessage = "Plugin assembly ID to filter types by")]
        [Parameter(ParameterSetName = "All", HelpMessage = "Plugin assembly ID to filter types by")]
        public Guid? PluginAssemblyId { get; set; }

        /// <summary>
        /// Process the cmdlet.
        /// </summary>
        protected override void ProcessRecord()
        {
            base.ProcessRecord();

            QueryExpression query = new QueryExpression("plugintype")
            {
                ColumnSet = new ColumnSet(true)
            };

            if (ParameterSetName == "ById")
            {
                query.Criteria.AddCondition("plugintypeid", ConditionOperator.Equal, Id);
            }
            else if (ParameterSetName == "ByName")
            {
                query.Criteria.AddCondition("typename", ConditionOperator.Equal, TypeName);
            }

            if (PluginAssemblyId.HasValue)
            {
                query.Criteria.AddCondition("pluginassemblyid", ConditionOperator.Equal, PluginAssemblyId.Value);
            }

            // Add linked entity to fetch pluginassembly name to avoid N+1 queries
            LinkEntity assemblyLink = new LinkEntity
            {
                LinkFromEntityName = "plugintype",
                LinkFromAttributeName = "pluginassemblyid",
                LinkToEntityName = "pluginassembly",
                LinkToAttributeName = "pluginassemblyid",
                EntityAlias = "pluginassembly_link",
                JoinOperator = JoinOperator.LeftOuter,
                Columns = new ColumnSet("name")
            };
            query.LinkEntities.Add(assemblyLink);

            // Add linked entity to fetch organization name to avoid N+1 queries
            LinkEntity organizationLink = new LinkEntity
            {
                LinkFromEntityName = "plugintype",
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
                PopulateEntityReferenceNamesFromLinks(entity, "pluginassemblyid", "pluginassembly_link", "name");
                PopulateEntityReferenceNamesFromLinks(entity, "organizationid", "organization_link", "name");

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
