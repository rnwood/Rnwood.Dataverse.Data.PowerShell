using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Messages;
using Microsoft.Xrm.Sdk.Metadata;
using System;
using System.Linq;
using System.Management.Automation;

namespace Rnwood.Dataverse.Data.PowerShell.Commands
{
    /// <summary>
    /// Retrieves metadata for a specific attribute (column) from Dataverse.
    /// </summary>
    [Cmdlet(VerbsCommon.Get, "DataverseAttributeMetadata")]
    [OutputType(typeof(AttributeMetadata))]
    public class GetDataverseAttributeMetadataCmdlet : OrganizationServiceCmdlet
    {
        /// <summary>
        /// Gets or sets the logical name of the entity.
        /// </summary>
        [Parameter(Mandatory = true, Position = 0, HelpMessage = "Logical name of the entity (table)")]
        [ArgumentCompleter(typeof(TableNameArgumentCompleter))]
        [Alias("TableName")]
        public string EntityName { get; set; }

        /// <summary>
        /// Gets or sets the logical name of the attribute.
        /// If not specified, returns all attributes for the entity.
        /// </summary>
        [Parameter(Mandatory = false, Position = 1, HelpMessage = "Logical name of the attribute (column). If not specified, returns all attributes for the entity.")]
        [ArgumentCompleter(typeof(ColumnNameArgumentCompleter))]
        [Alias("ColumnName")]
        public string AttributeName { get; set; }

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

            if (string.IsNullOrWhiteSpace(AttributeName))
            {
                // List all attributes for the entity
                RetrieveAllAttributes();
            }
            else
            {
                // Retrieve specific attribute
                RetrieveSingleAttribute();
            }
        }

        private void RetrieveAllAttributes()
        {
            EntityMetadata entityMetadata = null;

            // Try cache first if enabled
            if (UseMetadataCache)
            {
                var connectionKey = MetadataCache.GetConnectionKey(Connection as Microsoft.PowerPlatform.Dataverse.Client.ServiceClient);
                var retrieveAsIfPublished = !Published.IsPresent;
                if (MetadataCache.TryGetEntityMetadata(connectionKey, EntityName, EntityFilters.Attributes, retrieveAsIfPublished, out var cachedEntity))
                {
                    WriteVerbose($"Retrieved entity '{EntityName}' from cache with {cachedEntity.Attributes?.Length ?? 0} attributes (retrieveAsIfPublished: {retrieveAsIfPublished})");
                    entityMetadata = cachedEntity;
                }
            }

            // If not in cache, retrieve from server
            if (entityMetadata == null)
            {
                var request = new RetrieveEntityRequest
                {
                    EntityFilters = EntityFilters.Attributes,
                    LogicalName = EntityName,
                    RetrieveAsIfPublished = !Published.IsPresent
                };

                WriteVerbose($"Retrieving all attributes for entity '{EntityName}'");

                var response = (RetrieveEntityResponse)Connection.Execute(request);
                entityMetadata = response.EntityMetadata;

                // Cache the results if caching is enabled
                if (UseMetadataCache)
                {
                    var connectionKey = MetadataCache.GetConnectionKey(Connection as Microsoft.PowerPlatform.Dataverse.Client.ServiceClient);
                    var retrieveAsIfPublished = !Published.IsPresent;
                    MetadataCache.AddEntityMetadata(connectionKey, EntityName, EntityFilters.Attributes, retrieveAsIfPublished, entityMetadata);
                }
            }

            if (entityMetadata.Attributes == null || entityMetadata.Attributes.Length == 0)
            {
                WriteVerbose($"No attributes found for entity '{EntityName}'");
                return;
            }

            WriteVerbose($"Retrieved {entityMetadata.Attributes.Length} attributes");

            var results = entityMetadata.Attributes
                .OrderBy(a => a.LogicalName, StringComparer.Ordinal)
                .ToArray();

            WriteObject(results, true);
        }

        private void RetrieveSingleAttribute()
        {
            var request = new RetrieveAttributeRequest
            {
                EntityLogicalName = EntityName,
                LogicalName = AttributeName,
                RetrieveAsIfPublished = !Published.IsPresent
            };

            WriteVerbose($"Retrieving attribute metadata for '{EntityName}.{AttributeName}'");

            var response = (RetrieveAttributeResponse)Connection.Execute(request);
            var attributeMetadata = response.AttributeMetadata;

            WriteObject(attributeMetadata);
        }
    }
}
