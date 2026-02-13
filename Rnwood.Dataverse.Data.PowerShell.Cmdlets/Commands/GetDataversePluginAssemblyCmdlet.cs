using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Query;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Management.Automation;

namespace Rnwood.Dataverse.Data.PowerShell.Commands
{
    /// <summary>
    /// Retrieves plugin assembly records from a Dataverse environment.
    /// </summary>
    [Cmdlet(VerbsCommon.Get, "DataversePluginAssembly", DefaultParameterSetName = "All")]
    [OutputType(typeof(PSObject))]
    public class GetDataversePluginAssemblyCmdlet : OrganizationServiceCmdlet
    {
        /// <summary>
        /// Gets or sets the ID of the plugin assembly to retrieve.
        /// </summary>
        [Parameter(ParameterSetName = "ById", Mandatory = true, ValueFromPipelineByPropertyName = true, HelpMessage = "ID of the plugin assembly to retrieve")]
        public Guid Id { get; set; }

        /// <summary>
        /// Gets or sets the name of the plugin assembly to retrieve.
        /// </summary>
        [Parameter(ParameterSetName = "ByName", Mandatory = true, HelpMessage = "Name of the plugin assembly to retrieve")]
        public string Name { get; set; }

        /// <summary>
        /// Process the cmdlet.
        /// </summary>
        protected override void ProcessRecord()
        {
            base.ProcessRecord();

            QueryExpression query = new QueryExpression("pluginassembly")
            {
                ColumnSet = new ColumnSet(true)
            };

            if (ParameterSetName == "ById")
            {
                query.Criteria.AddCondition("pluginassemblyid", ConditionOperator.Equal, Id);
            }
            else if (ParameterSetName == "ByName")
            {
                query.Criteria.AddCondition("name", ConditionOperator.Equal, Name);
            }

            // Add linked entity to fetch organization name to avoid N+1 queries
            LinkEntity organizationLink = new LinkEntity
            {
                LinkFromEntityName = "pluginassembly",
                LinkFromAttributeName = "organizationid",
                LinkToEntityName = "organization",
                LinkToAttributeName = "organizationid",
                EntityAlias = "organization_link",
                JoinOperator = JoinOperator.LeftOuter,
                Columns = new ColumnSet("name")
            };
            query.LinkEntities.Add(organizationLink);

            // Add linked entity to fetch managedidentity name to avoid N+1 queries
            LinkEntity managedIdentityLink = new LinkEntity
            {
                LinkFromEntityName = "pluginassembly",
                LinkFromAttributeName = "managedidentityid",
                LinkToEntityName = "managedidentity",
                LinkToAttributeName = "managedidentityid",
                EntityAlias = "managedidentity_link",
                JoinOperator = JoinOperator.LeftOuter,
                Columns = new ColumnSet("name")
            };
            query.LinkEntities.Add(managedIdentityLink);

            EntityMetadataFactory entityMetadataFactory = new EntityMetadataFactory(Connection);
            DataverseEntityConverter converter = new DataverseEntityConverter(Connection, entityMetadataFactory);

            foreach (Entity entity in QueryHelpers.ExecuteQueryWithPaging(query, Connection, WriteVerbose))
            {
                // Populate EntityReference names from linked entity data to avoid N+1 queries
                PopulateEntityReferenceNamesFromLinks(entity, "organizationid", "organization_link", "name");
                PopulateEntityReferenceNamesFromLinks(entity, "managedidentityid", "managedidentity_link", "name");

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
