using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Messages;
using Microsoft.Xrm.Sdk.Metadata;
using System;
using System.Linq;
using System.Management.Automation;

namespace Rnwood.Dataverse.Data.PowerShell.Commands
{
    /// <summary>
    /// Retrieves alternate key metadata for a specific entity (table) from Dataverse.
    /// </summary>
    [Cmdlet(VerbsCommon.Get, "DataverseEntityKeyMetadata")]
    [OutputType(typeof(EntityKeyMetadata))]
    public class GetDataverseEntityKeyMetadataCmdlet : OrganizationServiceCmdlet
    {
        /// <summary>
        /// Gets or sets the logical name of the entity to retrieve key metadata for.
        /// </summary>
        [Parameter(Mandatory = true, Position = 0, ValueFromPipeline = true, ValueFromPipelineByPropertyName = true, HelpMessage = "Logical name of the entity (table) to retrieve key metadata for")]
        [ArgumentCompleter(typeof(TableNameArgumentCompleter))]
        [Alias("TableName")]
        public string EntityName { get; set; }

        /// <summary>
        /// Gets or sets the logical name of the specific key to retrieve.
        /// If not specified, returns all keys for the entity.
        /// </summary>
        [Parameter(Mandatory = false, Position = 1, HelpMessage = "Logical name of the specific key to retrieve. If not specified, returns all keys for the entity")]
        public string KeyName { get; set; }

        /// <summary>
        /// Gets or sets whether to use the shared metadata cache.
        /// </summary>
        [Parameter(HelpMessage = "Use the shared global metadata cache for improved performance")]
        public SwitchParameter UseMetadataCache { get; set; }

        /// <summary>
        /// Gets or sets whether to retrieve only published metadata.
        /// When not specified (default), retrieves unpublished (draft) metadata which includes all changes.
        /// </summary>
        [Parameter(HelpMessage = "Retrieve only published metadata. By default, unpublished (draft) metadata is retrieved which includes all changes")]
        public SwitchParameter Published { get; set; }

        /// <summary>
        /// Processes the cmdlet.
        /// </summary>
        protected override void ProcessRecord()
        {
            base.ProcessRecord();

            EntityMetadata entityMetadata;

            // Try cache first if enabled
            if (UseMetadataCache)
            {
                var connectionKey = MetadataCache.GetConnectionKey(Connection as Microsoft.PowerPlatform.Dataverse.Client.ServiceClient);
                var retrieveAsIfPublished = !Published.IsPresent;
                if (MetadataCache.TryGetEntityMetadata(connectionKey, EntityName, EntityFilters.All, retrieveAsIfPublished, out entityMetadata))
                {
                    WriteVerbose($"Retrieved entity metadata for '{EntityName}' from cache");
                }
                else
                {
                    var request = new RetrieveEntityRequest
                    {
                        LogicalName = EntityName,
                        EntityFilters = EntityFilters.All,
                        RetrieveAsIfPublished = !Published.IsPresent
                    };

                    WriteVerbose($"Retrieving entity metadata for '{EntityName}'");

                    var response = (RetrieveEntityResponse)Connection.Execute(request);
                    entityMetadata = response.EntityMetadata;

                    // Cache the result
                    MetadataCache.AddEntityMetadata(connectionKey, EntityName, EntityFilters.All, retrieveAsIfPublished, entityMetadata);
                }
            }
            else
            {
                var request = new RetrieveEntityRequest
                {
                    LogicalName = EntityName,
                    EntityFilters = EntityFilters.All,
                    RetrieveAsIfPublished = !Published.IsPresent
                };

                WriteVerbose($"Retrieving entity metadata for '{EntityName}'");

                var response = (RetrieveEntityResponse)Connection.Execute(request);
                entityMetadata = response.EntityMetadata;
            }

            if (entityMetadata.Keys == null || !entityMetadata.Keys.Any())
            {
                WriteVerbose($"Entity '{EntityName}' has no alternate keys defined");
                if (!string.IsNullOrWhiteSpace(KeyName))
                {
                    ThrowTerminatingError(new ErrorRecord(
                        new InvalidOperationException($"Alternate key '{KeyName}' not found on entity '{EntityName}'"),
                        "KeyNotFound",
                        ErrorCategory.ObjectNotFound,
                        KeyName));
                }
                return;
            }

            if (!string.IsNullOrWhiteSpace(KeyName))
            {
                // Return specific key
                var key = entityMetadata.Keys.FirstOrDefault(k => 
                    string.Equals(k.LogicalName, KeyName, StringComparison.OrdinalIgnoreCase));

                if (key == null)
                {
                    ThrowTerminatingError(new ErrorRecord(
                        new InvalidOperationException($"Alternate key '{KeyName}' not found on entity '{EntityName}'"),
                        "KeyNotFound",
                        ErrorCategory.ObjectNotFound,
                        KeyName));
                    return;
                }

                WriteObject(key);
            }
            else
            {
                // Return all keys, sorted by LogicalName
                var results = entityMetadata.Keys
                    .OrderBy(k => k.LogicalName, StringComparer.OrdinalIgnoreCase)
                    .ToArray();

                WriteVerbose($"Found {results.Length} alternate key(s) for entity '{EntityName}'");
                WriteObject(results, true);
            }
        }
    }
}
