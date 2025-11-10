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
    /// Creates or updates an entity (table) in Dataverse.
    /// </summary>
    [Cmdlet(VerbsCommon.Set, "DataverseEntityMetadata", SupportsShouldProcess = true, ConfirmImpact = ConfirmImpact.Medium)]
    [OutputType(typeof(PSObject))]
    public class SetDataverseEntityMetadataCmdlet : OrganizationServiceCmdlet
    {
        /// <summary>
        /// Gets or sets the logical name of the entity.
        /// </summary>
        [Parameter(Mandatory = true, Position = 0, HelpMessage = "Logical name of the entity (table)")]
        [Alias("TableName")]
        public string EntityName { get; set; }

        /// <summary>
        /// Gets or sets the schema name of the entity (used for create).
        /// </summary>
        [Parameter(HelpMessage = "Schema name of the entity (required for create, e.g., 'new_CustomEntity')")]
        public string SchemaName { get; set; }

        /// <summary>
        /// Gets or sets the display name of the entity.
        /// </summary>
        [Parameter(HelpMessage = "Display name of the entity (required for create)")]
        public string DisplayName { get; set; }

        /// <summary>
        /// Gets or sets the display collection name of the entity.
        /// </summary>
        [Parameter(HelpMessage = "Display collection name (plural) of the entity (required for create)")]
        public string DisplayCollectionName { get; set; }

        /// <summary>
        /// Gets or sets the description of the entity.
        /// </summary>
        [Parameter(HelpMessage = "Description of the entity")]
        public string Description { get; set; }

        /// <summary>
        /// Gets or sets the ownership type of the entity.
        /// </summary>
        [Parameter(HelpMessage = "Ownership type: UserOwned, TeamOwned, OrganizationOwned")]
        [ValidateSet("UserOwned", "TeamOwned", "OrganizationOwned")]
        public string OwnershipType { get; set; }

        /// <summary>
        /// Gets or sets whether the entity supports activities.
        /// </summary>
        [Parameter(HelpMessage = "Whether the entity supports activities")]
        public SwitchParameter HasActivities { get; set; }

        /// <summary>
        /// Gets or sets whether the entity supports notes.
        /// </summary>
        [Parameter(HelpMessage = "Whether the entity supports notes (annotations)")]
        public SwitchParameter HasNotes { get; set; }

        /// <summary>
        /// Gets or sets whether audit is enabled.
        /// </summary>
        [Parameter(HelpMessage = "Whether audit is enabled for this entity")]
        public SwitchParameter IsAuditEnabled { get; set; }

        /// <summary>
        /// Gets or sets whether change tracking is enabled.
        /// </summary>
        [Parameter(HelpMessage = "Whether change tracking is enabled")]
        public SwitchParameter ChangeTrackingEnabled { get; set; }

        /// <summary>
        /// Gets or sets the primary attribute schema name (for create).
        /// </summary>
        [Parameter(HelpMessage = "Schema name for the primary name attribute (required for create, e.g., 'new_name')")]
        public string PrimaryAttributeSchemaName { get; set; }

        /// <summary>
        /// Gets or sets the primary attribute display name (for create).
        /// </summary>
        [Parameter(HelpMessage = "Display name for the primary name attribute")]
        public string PrimaryAttributeDisplayName { get; set; }

        /// <summary>
        /// Gets or sets the primary attribute max length (for create).
        /// </summary>
        [Parameter(HelpMessage = "Maximum length for the primary name attribute (default 100)")]
        public int? PrimaryAttributeMaxLength { get; set; }

        /// <summary>
        /// Gets or sets whether to return the created/updated entity metadata.
        /// </summary>
        [Parameter(HelpMessage = "Return the created or updated entity metadata")]
        public SwitchParameter PassThru { get; set; }

        /// <summary>
        /// Gets or sets whether to publish the entity after creating or updating.
        /// </summary>
        [Parameter(HelpMessage = "If specified, publishes the entity after creating or updating")]
        public SwitchParameter Publish { get; set; }

        /// <summary>
        /// Processes the cmdlet.
        /// </summary>
        protected override void ProcessRecord()
        {
            base.ProcessRecord();

            // Check if entity exists
            EntityMetadata existingEntity = null;
            bool entityExists = false;

            try
            {
                var retrieveRequest = new RetrieveEntityRequest
                {
                    LogicalName = EntityName,
                    EntityFilters = EntityFilters.Entity,
                    RetrieveAsIfPublished = true
                };

                var retrieveResponse = (RetrieveEntityResponse)Connection.Execute(retrieveRequest);
                existingEntity = retrieveResponse.EntityMetadata;
                entityExists = true;
                WriteVerbose($"Entity '{EntityName}' already exists");
            }
            catch (FaultException<OrganizationServiceFault> ex)
            {
                if (QueryHelpers.IsNotFoundException(ex))
                {
                    WriteVerbose($"Entity '{EntityName}' does not exist - will create");
                }
                else
                {
                    throw;
                }
            }

            if (entityExists)
            {
                // Update existing entity
                UpdateEntity(existingEntity);
            }
            else
            {
                // Create new entity
                CreateEntity();
            }
        }

        private void CreateEntity()
        {
            if (string.IsNullOrWhiteSpace(SchemaName))
            {
                ThrowTerminatingError(new ErrorRecord(
                    new ArgumentException("SchemaName is required when creating a new entity"),
                    "SchemaNameRequired",
                    ErrorCategory.InvalidArgument,
                    null));
                return;
            }

            if (string.IsNullOrWhiteSpace(PrimaryAttributeSchemaName))
            {
                ThrowTerminatingError(new ErrorRecord(
                    new ArgumentException("PrimaryAttributeSchemaName is required when creating a new entity"),
                    "PrimaryAttributeSchemaNameRequired",
                    ErrorCategory.InvalidArgument,
                    null));
                return;
            }

            if (string.IsNullOrWhiteSpace(DisplayName))
            {
                ThrowTerminatingError(new ErrorRecord(
                    new ArgumentException("DisplayName is required when creating a new entity"),
                    "DisplayNameRequired",
                    ErrorCategory.InvalidArgument,
                    null));
                return;
            }

            if (string.IsNullOrWhiteSpace(DisplayCollectionName))
            {
                ThrowTerminatingError(new ErrorRecord(
                    new ArgumentException("DisplayCollectionName is required when creating a new entity"),
                    "DisplayCollectionNameRequired",
                    ErrorCategory.InvalidArgument,
                    null));
                return;
            }

            // Get the organization's base language code
            int baseLanguageCode = GetBaseLanguageCode();

            var entity = new EntityMetadata
            {
                SchemaName = SchemaName,
                LogicalName = EntityName
            };

            entity.DisplayName = new Label(DisplayName, baseLanguageCode);

            entity.DisplayCollectionName = new Label(DisplayCollectionName, baseLanguageCode);

            if (!string.IsNullOrWhiteSpace(Description))
            {
                entity.Description = new Label(Description, baseLanguageCode);
            }

            if (!string.IsNullOrWhiteSpace(OwnershipType))
            {
                entity.OwnershipType = (OwnershipTypes)Enum.Parse(typeof(OwnershipTypes), OwnershipType);
            }
            else
            {
                entity.OwnershipType = OwnershipTypes.UserOwned; // Default
            }

            if (HasActivities.IsPresent)
            {
                entity.HasActivities = HasActivities.ToBool();
            }

            if (HasNotes.IsPresent)
            {
                entity.HasNotes = HasNotes.ToBool();
            }

            if (IsAuditEnabled.IsPresent)
            {
                entity.IsAuditEnabled = new BooleanManagedProperty(IsAuditEnabled.ToBool());
            }

            if (ChangeTrackingEnabled.IsPresent)
            {
                entity.ChangeTrackingEnabled = ChangeTrackingEnabled.ToBool();
            }

            // Create primary attribute
            var primaryAttribute = new StringAttributeMetadata
            {
                SchemaName = PrimaryAttributeSchemaName,
                LogicalName = PrimaryAttributeSchemaName.ToLowerInvariant(),
                RequiredLevel = new AttributeRequiredLevelManagedProperty(AttributeRequiredLevel.None),
                MaxLength = PrimaryAttributeMaxLength ?? 100,
                DisplayName = new Label(PrimaryAttributeDisplayName ?? "Name", baseLanguageCode)
            };

            var request = new CreateEntityRequest
            {
                Entity = entity,
                PrimaryAttribute = primaryAttribute
            };

            if (!ShouldProcess($"Entity '{SchemaName}'", $"Create entity with ownership '{entity.OwnershipType}'"))
            {
                return;
            }

            WriteVerbose($"Creating entity '{SchemaName}'");

            var response = (CreateEntityResponse)Connection.Execute(request);

            WriteVerbose($"Entity created successfully with MetadataId: {response.EntityId}");

            // Invalidate cache for this entity
            InvalidateEntityCache();

            // Publish the entity if specified
            if (Publish && ShouldProcess($"Entity '{EntityName}'", "Publish"))
            {
                var publishRequest = new PublishXmlRequest
                {
                    ParameterXml = $"<importexportxml><entities><entity>{EntityName}</entity></entities></importexportxml>"
                };
                Connection.Execute(publishRequest);
                WriteVerbose($"Published entity '{EntityName}'");
            }

            if (PassThru)
            {
                // Retrieve and return the created entity
                var retrieveRequest = new RetrieveEntityRequest
                {
                    LogicalName = EntityName,
                    EntityFilters = EntityFilters.Entity | EntityFilters.Attributes,
                    RetrieveAsIfPublished = true
                };

                var retrieveResponse = (RetrieveEntityResponse)Connection.Execute(retrieveRequest);
                var result = ConvertEntityMetadataToPSObject(retrieveResponse.EntityMetadata);
                WriteObject(result);
            }
        }

        private void UpdateEntity(EntityMetadata existingEntity)
        {
            // Validate immutable properties first
            ValidateImmutableEntityProperties(existingEntity);

            // Get the organization's base language code
            int baseLanguageCode = GetBaseLanguageCode();

            var entityToUpdate = new EntityMetadata
            {
                MetadataId = existingEntity.MetadataId,
                LogicalName = existingEntity.LogicalName
            };

            bool hasChanges = false;

            // Update display name
            if (!string.IsNullOrWhiteSpace(DisplayName))
            {
                entityToUpdate.DisplayName = new Label(DisplayName, baseLanguageCode);
                hasChanges = true;
            }

            // Update display collection name
            if (!string.IsNullOrWhiteSpace(DisplayCollectionName))
            {
                entityToUpdate.DisplayCollectionName = new Label(DisplayCollectionName, baseLanguageCode);
                hasChanges = true;
            }

            // Update description
            if (!string.IsNullOrWhiteSpace(Description))
            {
                entityToUpdate.Description = new Label(Description, baseLanguageCode);
                hasChanges = true;
            }

            // Update audit enabled
            if (IsAuditEnabled.IsPresent)
            {
                entityToUpdate.IsAuditEnabled = new BooleanManagedProperty(IsAuditEnabled.ToBool());
                hasChanges = true;
            }

            // Update change tracking
            if (ChangeTrackingEnabled.IsPresent)
            {
                entityToUpdate.ChangeTrackingEnabled = ChangeTrackingEnabled.ToBool();
                hasChanges = true;
            }

            if (!hasChanges)
            {
                WriteWarning("No changes specified for update");
                
                if (PassThru)
                {
                    var result = ConvertEntityMetadataToPSObject(existingEntity);
                    WriteObject(result);
                }
                return;
            }

            var request = new UpdateEntityRequest
            {
                Entity = entityToUpdate,
                MergeLabels = true
            };

            if (!ShouldProcess($"Entity '{EntityName}'", "Update entity metadata"))
            {
                return;
            }

            WriteVerbose($"Updating entity '{EntityName}'");

            Connection.Execute(request);

            WriteVerbose($"Entity updated successfully");

            // Invalidate cache for this entity
            InvalidateEntityCache();

            // Publish the entity if specified
            if (Publish && ShouldProcess($"Entity '{EntityName}'", "Publish"))
            {
                var publishRequest = new PublishXmlRequest
                {
                    ParameterXml = $"<importexportxml><entities><entity>{EntityName}</entity></entities></importexportxml>"
                };
                Connection.Execute(publishRequest);
                WriteVerbose($"Published entity '{EntityName}'");
            }

            if (PassThru)
            {
                // Retrieve and return the updated entity
                var retrieveRequest = new RetrieveEntityRequest
                {
                    LogicalName = EntityName,
                    EntityFilters = EntityFilters.Entity | EntityFilters.Attributes,
                    RetrieveAsIfPublished = true
                };

                var retrieveResponse = (RetrieveEntityResponse)Connection.Execute(retrieveRequest);
                var result = ConvertEntityMetadataToPSObject(retrieveResponse.EntityMetadata);
                WriteObject(result);
            }
        }

        private void ValidateImmutableEntityProperties(EntityMetadata existingEntity)
        {
            // Check if SchemaName was provided and is different (immutable)
            if (MyInvocation.BoundParameters.ContainsKey(nameof(SchemaName)) &&
                !string.Equals(SchemaName, existingEntity.SchemaName, StringComparison.OrdinalIgnoreCase))
            {
                ThrowTerminatingError(new ErrorRecord(
                    new InvalidOperationException($"Cannot change SchemaName from '{existingEntity.SchemaName}' to '{SchemaName}'. This property is immutable after creation."),
                    "ImmutableSchemaName",
                    ErrorCategory.InvalidOperation,
                    SchemaName));
            }

            // Check if OwnershipType was provided and is different (immutable)
            if (MyInvocation.BoundParameters.ContainsKey(nameof(OwnershipType)))
            {
                var existingOwnershipType = existingEntity.OwnershipType?.ToString() ?? "UserOwned";
                if (!string.Equals(OwnershipType, existingOwnershipType, StringComparison.OrdinalIgnoreCase))
                {
                    ThrowTerminatingError(new ErrorRecord(
                        new InvalidOperationException($"Cannot change OwnershipType from '{existingOwnershipType}' to '{OwnershipType}'. This property is immutable after creation."),
                        "ImmutableOwnershipType",
                        ErrorCategory.InvalidOperation,
                        OwnershipType));
                }
            }

            // Check if HasActivities was provided and is different (immutable after creation)
            if (MyInvocation.BoundParameters.ContainsKey(nameof(HasActivities)) &&
                HasActivities.ToBool() != existingEntity.HasActivities)
            {
                ThrowTerminatingError(new ErrorRecord(
                    new InvalidOperationException($"Cannot change HasActivities from '{existingEntity.HasActivities}' to '{HasActivities.ToBool()}'. This property is immutable after creation."),
                    "ImmutableHasActivities",
                    ErrorCategory.InvalidOperation,
                    null));
            }

            // Check if HasNotes was provided and is different (immutable after creation)
            if (MyInvocation.BoundParameters.ContainsKey(nameof(HasNotes)) &&
                HasNotes.ToBool() != existingEntity.HasNotes)
            {
                ThrowTerminatingError(new ErrorRecord(
                    new InvalidOperationException($"Cannot change HasNotes from '{existingEntity.HasNotes}' to '{HasNotes.ToBool()}'. This property is immutable after creation."),
                    "ImmutableHasNotes",
                    ErrorCategory.InvalidOperation,
                    null));
            }

            // Check if primary attribute properties were provided and are different (immutable after creation)
            var primaryAttribute = existingEntity.Attributes?.FirstOrDefault(a => a.LogicalName == existingEntity.PrimaryNameAttribute);
            if (primaryAttribute != null)
            {
                if (MyInvocation.BoundParameters.ContainsKey(nameof(PrimaryAttributeSchemaName)) &&
                    !string.Equals(PrimaryAttributeSchemaName, primaryAttribute.SchemaName, StringComparison.OrdinalIgnoreCase))
                {
                    ThrowTerminatingError(new ErrorRecord(
                        new InvalidOperationException($"Cannot change PrimaryAttributeSchemaName from '{primaryAttribute.SchemaName}' to '{PrimaryAttributeSchemaName}'. The primary attribute is immutable."),
                        "ImmutablePrimaryAttribute",
                        ErrorCategory.InvalidOperation,
                        PrimaryAttributeSchemaName));
                }

                if (MyInvocation.BoundParameters.ContainsKey(nameof(PrimaryAttributeDisplayName)))
                {
                    var existingDisplayName = primaryAttribute.DisplayName?.UserLocalizedLabel?.Label ?? "";
                    if (!string.Equals(PrimaryAttributeDisplayName, existingDisplayName, StringComparison.OrdinalIgnoreCase))
                    {
                        ThrowTerminatingError(new ErrorRecord(
                            new InvalidOperationException($"Cannot change PrimaryAttributeDisplayName from '{existingDisplayName}' to '{PrimaryAttributeDisplayName}'. To update the primary attribute, use Set-DataverseAttributeMetadata."),
                            "ImmutablePrimaryAttribute",
                            ErrorCategory.InvalidOperation,
                            PrimaryAttributeDisplayName));
                    }
                }

                if (MyInvocation.BoundParameters.ContainsKey(nameof(PrimaryAttributeMaxLength)))
                {
                    var existingMaxLength = (primaryAttribute as StringAttributeMetadata)?.MaxLength ?? 0;
                    if (PrimaryAttributeMaxLength != existingMaxLength)
                    {
                        ThrowTerminatingError(new ErrorRecord(
                            new InvalidOperationException($"Cannot change PrimaryAttributeMaxLength from '{existingMaxLength}' to '{PrimaryAttributeMaxLength}'. To update the primary attribute, use Set-DataverseAttributeMetadata."),
                            "ImmutablePrimaryAttribute",
                            ErrorCategory.InvalidOperation,
                            PrimaryAttributeMaxLength));
                    }
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

        private PSObject ConvertEntityMetadataToPSObject(EntityMetadata metadata)
        {
            var result = new PSObject();
            result.Properties.Add(new PSNoteProperty("LogicalName", metadata.LogicalName));
            result.Properties.Add(new PSNoteProperty("SchemaName", metadata.SchemaName));
            result.Properties.Add(new PSNoteProperty("DisplayName", metadata.DisplayName?.UserLocalizedLabel?.Label));
            result.Properties.Add(new PSNoteProperty("MetadataId", metadata.MetadataId));
            result.Properties.Add(new PSNoteProperty("OwnershipType", metadata.OwnershipType?.ToString()));
            result.Properties.Add(new PSNoteProperty("IsCustomEntity", metadata.IsCustomEntity));
            return result;
        }
    }
}
