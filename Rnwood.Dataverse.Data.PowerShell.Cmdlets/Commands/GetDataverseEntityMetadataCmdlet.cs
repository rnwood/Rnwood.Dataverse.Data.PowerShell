using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Messages;
using Microsoft.Xrm.Sdk.Metadata;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Management.Automation;

namespace Rnwood.Dataverse.Data.PowerShell.Commands
{
    /// <summary>
    /// Retrieves metadata for a specific entity (table) from Dataverse.
    /// </summary>
    [Cmdlet(VerbsCommon.Get, "DataverseEntityMetadata")]
    [OutputType(typeof(EntityMetadata))]
    public class GetDataverseEntityMetadataCmdlet : OrganizationServiceCmdlet
    {
        /// <summary>
        /// Gets or sets the logical name of the entity to retrieve metadata for.
        /// If not specified, returns metadata for all entities.
        /// </summary>
        [Parameter(Mandatory = false, Position = 0, ValueFromPipeline = true, ValueFromPipelineByPropertyName = true, HelpMessage = "Logical name of the entity to retrieve metadata for. If not specified, returns all entities.")]
        [ArgumentCompleter(typeof(TableNameArgumentCompleter))]
        [Alias("TableName")]
        public string EntityName { get; set; }

        /// <summary>
        /// Gets or sets whether to exclude attribute metadata from the output.
        /// </summary>
        [Parameter(HelpMessage = "Exclude attribute (column) metadata from the output")]
        public SwitchParameter ExcludeAttributes { get; set; }

        /// <summary>
        /// Gets or sets whether to exclude relationship metadata from the output.
        /// </summary>
        [Parameter(HelpMessage = "Exclude relationship metadata from the output")]
        public SwitchParameter ExcludeRelationships { get; set; }

        /// <summary>
        /// Gets or sets whether to exclude privilege metadata from the output.
        /// </summary>
        [Parameter(HelpMessage = "Exclude privilege metadata from the output")]
        public SwitchParameter ExcludePrivileges { get; set; }

        /// <summary>
        /// Gets or sets whether to use the shared metadata cache.
        /// </summary>
        [Parameter(HelpMessage = "Use the shared global metadata cache for improved performance")]
        public SwitchParameter UseMetadataCache { get; set; }

        /// <summary>
        /// Gets or sets whether to retrieve only published metadata.
        /// When not specified (default), retrieves unpublished (draft) metadata which includes all changes.
        /// </summary>
        [Parameter(HelpMessage = "Retrieve only published metadata. By default, unpublished (draft) metadata is retrieved which includes all changes.")]
        public SwitchParameter Published { get; set; }

        /// <summary>
        /// Processes the cmdlet.
        /// </summary>
        protected override void ProcessRecord()
        {
            base.ProcessRecord();

            if (string.IsNullOrWhiteSpace(EntityName))
            {
                // List all entities
                RetrieveAllEntities();
            }
            else
            {
                // Retrieve specific entity
                RetrieveSingleEntity(EntityName);
            }
        }

        private void RetrieveAllEntities()
        {
            EntityFilters filters = EntityFilters.Entity | EntityFilters.Attributes | EntityFilters.Relationships | EntityFilters.Privileges;

            if (ExcludeAttributes)
            {
                filters &= ~EntityFilters.Attributes;
            }

            if (ExcludeRelationships)
            {
                filters &= ~EntityFilters.Relationships;
            }

            if (ExcludePrivileges)
            {
                filters &= ~EntityFilters.Privileges;
            }

            EntityMetadata[] entities;

            // Try cache first if enabled
            if (UseMetadataCache)
            {
                var connectionKey = MetadataCache.GetConnectionKey(Connection as Microsoft.PowerPlatform.Dataverse.Client.ServiceClient);
                var retrieveAsIfPublished = !Published.IsPresent;
                if (MetadataCache.TryGetAllEntities(connectionKey, filters, retrieveAsIfPublished, out var cachedEntities))
                {
                    WriteVerbose($"Retrieved {cachedEntities.Count} entities from cache (filters: {filters}, retrieveAsIfPublished: {retrieveAsIfPublished})");
                    entities = cachedEntities.ToArray();
                }
                else
                {
                    var request = new RetrieveAllEntitiesRequest
                    {
                        EntityFilters = filters,
                        RetrieveAsIfPublished = !Published.IsPresent
                    };

                    WriteVerbose($"Retrieving all entity metadata with filters: {filters}");

                    var response = (RetrieveAllEntitiesResponse)QueryHelpers.ExecuteWithThrottlingRetry(Connection, request);
                    entities = response.EntityMetadata;

                    WriteVerbose($"Retrieved {entities.Length} entities");

                    // Cache the results
                    MetadataCache.AddAllEntities(connectionKey, filters, retrieveAsIfPublished, entities.ToList());
                }
            }
            else
            {
                var request = new RetrieveAllEntitiesRequest
                {
                    EntityFilters = filters,
                    RetrieveAsIfPublished = !Published.IsPresent
                };

                WriteVerbose($"Retrieving all entity metadata with filters: {filters}");

                var response = (RetrieveAllEntitiesResponse)QueryHelpers.ExecuteWithThrottlingRetry(Connection, request);
                entities = response.EntityMetadata;

                WriteVerbose($"Retrieved {entities.Length} entities");
            }

            var results = entities
                .OrderBy(e => e.LogicalName, StringComparer.OrdinalIgnoreCase)
                .ToArray();

            WriteObject(results, true);
        }

        private void RetrieveSingleEntity(string entityName)
        {
            EntityFilters filters = EntityFilters.Entity | EntityFilters.Attributes | EntityFilters.Relationships | EntityFilters.Privileges;

            if (ExcludeAttributes)
            {
                filters &= ~EntityFilters.Attributes;
            }

            if (ExcludeRelationships)
            {
                filters &= ~EntityFilters.Relationships;
            }

            if (ExcludePrivileges)
            {
                filters &= ~EntityFilters.Privileges;
            }

            EntityMetadata entityMetadata;

            // Try cache first if enabled
            if (UseMetadataCache)
            {
                var connectionKey = MetadataCache.GetConnectionKey(Connection as Microsoft.PowerPlatform.Dataverse.Client.ServiceClient);
                var retrieveAsIfPublished = !Published.IsPresent;
                if (MetadataCache.TryGetEntityMetadata(connectionKey, entityName, filters, retrieveAsIfPublished, out entityMetadata))
                {
                    WriteVerbose($"Retrieved entity metadata for '{entityName}' from cache (filters: {filters}, retrieveAsIfPublished: {retrieveAsIfPublished})");
                }
                else
                {
                    var request = new RetrieveEntityRequest
                    {
                        LogicalName = entityName,
                        EntityFilters = filters,
                        RetrieveAsIfPublished = !Published.IsPresent
                    };

                    WriteVerbose($"Retrieving entity metadata for '{entityName}' with filters: {filters}");

                    var response = (RetrieveEntityResponse)QueryHelpers.ExecuteWithThrottlingRetry(Connection, request);
                    entityMetadata = response.EntityMetadata;

                    // Cache the result
                    MetadataCache.AddEntityMetadata(connectionKey, entityName, filters, retrieveAsIfPublished, entityMetadata);
                }
            }
            else
            {
                var request = new RetrieveEntityRequest
                {
                    LogicalName = entityName,
                    EntityFilters = filters,
                    RetrieveAsIfPublished = !Published.IsPresent
                };

                WriteVerbose($"Retrieving entity metadata for '{entityName}' with filters: {filters}");

                var response = (RetrieveEntityResponse)QueryHelpers.ExecuteWithThrottlingRetry(Connection, request);
                entityMetadata = response.EntityMetadata;
            }

            WriteObject(entityMetadata);
        }
    }
}
