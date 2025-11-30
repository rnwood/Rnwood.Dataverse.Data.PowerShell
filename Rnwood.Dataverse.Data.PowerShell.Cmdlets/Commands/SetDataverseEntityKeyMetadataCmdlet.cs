using Microsoft.Crm.Sdk.Messages;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Messages;
using Microsoft.Xrm.Sdk.Metadata;
using System;
using System.Linq;
using System.Management.Automation;
using System.ServiceModel;

namespace Rnwood.Dataverse.Data.PowerShell.Commands
{
    /// <summary>
    /// Creates an alternate key on an entity (table) in Dataverse.
    /// Note: Alternate keys cannot be updated after creation. To modify a key, delete and recreate it.
    /// </summary>
    [Cmdlet(VerbsCommon.Set, "DataverseEntityKeyMetadata", SupportsShouldProcess = true, ConfirmImpact = ConfirmImpact.Medium)]
    [OutputType(typeof(PSObject))]
    public class SetDataverseEntityKeyMetadataCmdlet : OrganizationServiceCmdlet
    {
        /// <summary>
        /// Gets or sets the logical name of the entity.
        /// </summary>
        [Parameter(Mandatory = true, Position = 0, HelpMessage = "Logical name of the entity (table)")]
        [ArgumentCompleter(typeof(TableNameArgumentCompleter))]
        [Alias("TableName")]
        public string EntityName { get; set; }

        /// <summary>
        /// Gets or sets the schema name of the key.
        /// </summary>
        [Parameter(Mandatory = true, Position = 1, HelpMessage = "Schema name of the alternate key (e.g., 'new_customkey')")]
        public string SchemaName { get; set; }

        /// <summary>
        /// Gets or sets the display name of the key.
        /// </summary>
        [Parameter(HelpMessage = "Display name of the alternate key (defaults to SchemaName if not provided)")]
        public string DisplayName { get; set; }

        /// <summary>
        /// Gets or sets the key attributes (columns) that make up the alternate key.
        /// </summary>
        [Parameter(Mandatory = true, HelpMessage = "Array of attribute (column) logical names that make up the alternate key")]
        public string[] KeyAttributes { get; set; }

        /// <summary>
        /// Gets or sets whether to return the created key metadata.
        /// </summary>
        [Parameter(HelpMessage = "Return the created key metadata")]
        public SwitchParameter PassThru { get; set; }

        /// <summary>
        /// Gets or sets whether to publish the entity after creating the key.
        /// </summary>
        [Parameter(HelpMessage = "If specified, publishes the entity after creating the key")]
        public SwitchParameter Publish { get; set; }

        /// <summary>
        /// Gets or sets whether to skip the check for existing key and allow re-creation.
        /// </summary>
        [Parameter(HelpMessage = "If specified, skips checking if the key already exists. Use with caution as it may cause errors if the key exists")]
        public SwitchParameter Force { get; set; }

        /// <summary>
        /// Processes the cmdlet.
        /// </summary>
        protected override void ProcessRecord()
        {
            base.ProcessRecord();

            // Check if key exists (unless Force is specified)
            if (!Force.IsPresent)
            {
                try
                {
                    var retrieveRequest = new RetrieveEntityRequest
                    {
                        LogicalName = EntityName,
                        EntityFilters = EntityFilters.All,
                        RetrieveAsIfPublished = true
                    };

                    var retrieveResponse = (RetrieveEntityResponse)QueryHelpers.ExecuteWithThrottlingRetry(Connection, retrieveRequest);
                    var entityMetadata = retrieveResponse.EntityMetadata;

                    if (entityMetadata.Keys != null)
                    {
                        // Determine the logical name from schema name
                        string expectedLogicalName = SchemaName.ToLowerInvariant();
                        
                        var existingKey = entityMetadata.Keys.FirstOrDefault(k =>
                            string.Equals(k.SchemaName, SchemaName, StringComparison.OrdinalIgnoreCase) ||
                            string.Equals(k.LogicalName, expectedLogicalName, StringComparison.OrdinalIgnoreCase));

                        if (existingKey != null)
                        {
                            ThrowTerminatingError(new ErrorRecord(
                                new InvalidOperationException($"Alternate key '{SchemaName}' already exists on entity '{EntityName}'. Alternate keys cannot be updated after creation. To modify a key, use Remove-DataverseEntityKeyMetadata to delete it first, then create a new one. Use -Force to skip this check."),
                                "KeyAlreadyExists",
                                ErrorCategory.ResourceExists,
                                SchemaName));
                            return;
                        }
                    }
                }
                catch (FaultException<OrganizationServiceFault> ex)
                {
                    if (QueryHelpers.IsNotFoundException(ex))
                    {
                        ThrowTerminatingError(new ErrorRecord(
                            new InvalidOperationException($"Entity '{EntityName}' not found"),
                            "EntityNotFound",
                            ErrorCategory.ObjectNotFound,
                            EntityName));
                        return;
                    }
                    else
                    {
                        throw;
                    }
                }
            }

            // Create new key
            CreateKey();
        }

        private void CreateKey()
        {
            if (KeyAttributes == null || KeyAttributes.Length == 0)
            {
                ThrowTerminatingError(new ErrorRecord(
                    new ArgumentException("KeyAttributes is required and must contain at least one attribute"),
                    "KeyAttributesRequired",
                    ErrorCategory.InvalidArgument,
                    null));
                return;
            }

            // Get the organization's base language code
            int baseLanguageCode = GetBaseLanguageCode();

            // Determine logical name from schema name
            string logicalName = SchemaName.ToLowerInvariant();

            var key = new EntityKeyMetadata
            {
                SchemaName = SchemaName,
                LogicalName = logicalName
            };

            if (!string.IsNullOrWhiteSpace(DisplayName))
            {
                key.DisplayName = new Label(DisplayName, baseLanguageCode);
            }
            else
            {
                key.DisplayName = new Label(SchemaName, baseLanguageCode);
            }

            key.KeyAttributes = KeyAttributes;

            var request = new CreateEntityKeyRequest
            {
                EntityName = EntityName,
                EntityKey = key
            };

            if (!ShouldProcess($"Entity '{EntityName}', Key '{SchemaName}'", $"Create alternate key with attributes: {string.Join(", ", KeyAttributes)}"))
            {
                return;
            }

            WriteVerbose($"Creating alternate key '{SchemaName}' on entity '{EntityName}' with attributes: {string.Join(", ", KeyAttributes)}");

            var response = (CreateEntityKeyResponse)QueryHelpers.ExecuteWithThrottlingRetry(Connection, request);

            WriteVerbose($"Alternate key created successfully with MetadataId: {response.EntityKeyId}");

            // Invalidate cache for this entity
            InvalidateEntityCache();

            // Publish the entity if specified
            if (Publish && ShouldProcess($"Entity '{EntityName}'", "Publish"))
            {
                var publishRequest = new PublishXmlRequest
                {
                    ParameterXml = $"<importexportxml><entities><entity>{EntityName}</entity></entities></importexportxml>"
                };
                QueryHelpers.ExecuteWithThrottlingRetry(Connection, publishRequest);
                WriteVerbose($"Published entity '{EntityName}'");

                // Wait for publish to complete
                PublishHelpers.WaitForPublishComplete(Connection, WriteVerbose);
            }

            if (PassThru)
            {
                // Retrieve and return the created key
                var retrieveRequest = new RetrieveEntityRequest
                {
                    LogicalName = EntityName,
                    EntityFilters = EntityFilters.Entity,
                    RetrieveAsIfPublished = true
                };

                var retrieveResponse = (RetrieveEntityResponse)QueryHelpers.ExecuteWithThrottlingRetry(Connection, retrieveRequest);
                var createdKey = retrieveResponse.EntityMetadata.Keys?.FirstOrDefault(k =>
                    string.Equals(k.LogicalName, logicalName, StringComparison.OrdinalIgnoreCase));

                if (createdKey != null)
                {
                    var result = ConvertEntityKeyMetadataToPSObject(createdKey);
                    WriteObject(result);
                }
            }
        }

        private void InvalidateEntityCache()
        {
            var connectionKey = MetadataCache.GetConnectionKey(Connection as Microsoft.PowerPlatform.Dataverse.Client.ServiceClient);
            if (connectionKey != null)
            {
                MetadataCache.InvalidateEntity(connectionKey, EntityName);
                WriteVerbose($"Invalidated metadata cache for entity '{EntityName}'");
            }
        }

        private PSObject ConvertEntityKeyMetadataToPSObject(EntityKeyMetadata metadata)
        {
            var result = new PSObject();
            result.Properties.Add(new PSNoteProperty("LogicalName", metadata.LogicalName));
            result.Properties.Add(new PSNoteProperty("SchemaName", metadata.SchemaName));
            result.Properties.Add(new PSNoteProperty("DisplayName", metadata.DisplayName?.UserLocalizedLabel?.Label));
            result.Properties.Add(new PSNoteProperty("MetadataId", metadata.MetadataId));
            result.Properties.Add(new PSNoteProperty("EntityLogicalName", metadata.EntityLogicalName));
            result.Properties.Add(new PSNoteProperty("KeyAttributes", metadata.KeyAttributes));
            return result;
        }
    }
}
