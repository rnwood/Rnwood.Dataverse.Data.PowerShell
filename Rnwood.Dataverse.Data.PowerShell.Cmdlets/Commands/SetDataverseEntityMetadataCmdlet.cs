using Microsoft.Crm.Sdk.Messages;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Messages;
using Microsoft.Xrm.Sdk.Metadata;
using Microsoft.Xrm.Sdk.Query;
using System;
using System.Linq;
using System.Management.Automation;
using System.ServiceModel;

namespace Rnwood.Dataverse.Data.PowerShell.Commands
{
    /// <summary>
    /// Creates or updates an entity (table) in Dataverse.
    /// </summary>
    [Cmdlet(VerbsCommon.Set, "DataverseEntityMetadata", DefaultParameterSetName = "ByProperties", SupportsShouldProcess = true, ConfirmImpact = ConfirmImpact.Medium)]
    [OutputType(typeof(PSObject))]
    public class SetDataverseEntityMetadataCmdlet : OrganizationServiceCmdlet
    {
        /// <summary>
        /// Gets or sets the logical name of the entity.
        /// </summary>
        [Parameter(Mandatory = true, Position = 0, ParameterSetName = "ByProperties", HelpMessage = "Logical name of the entity (table)")]
        [Alias("TableName")]
        public string EntityName { get; set; }
        
        /// <summary>
        /// Gets or sets the entity metadata object to update. When provided, all property values from the metadata object are used.
        /// </summary>
        [Parameter(Mandatory = true, Position = 0, ParameterSetName = "ByEntityMetadata", ValueFromPipeline = true, HelpMessage = "EntityMetadata object to update")]
        public EntityMetadata EntityMetadata { get; set; }

        /// <summary>
        /// Gets or sets the schema name of the entity (used for create).
        /// </summary>
        [Parameter(ParameterSetName = "ByProperties", HelpMessage = "Schema name of the entity (required for create, e.g., 'new_CustomEntity')")]
        public string SchemaName { get; set; }

        /// <summary>
        /// Gets or sets the display name of the entity.
        /// </summary>
        [Parameter(ParameterSetName = "ByProperties", HelpMessage = "Display name of the entity (required for create)")]
        public string DisplayName { get; set; }

        /// <summary>
        /// Gets or sets the display collection name of the entity.
        /// </summary>
        [Parameter(ParameterSetName = "ByProperties", HelpMessage = "Display collection name (plural) of the entity (required for create)")]
        public string DisplayCollectionName { get; set; }

        /// <summary>
        /// Gets or sets the description of the entity.
        /// </summary>
        [Parameter(ParameterSetName = "ByProperties", HelpMessage = "Description of the entity")]
        public string Description { get; set; }

        /// <summary>
        /// Gets or sets the ownership type of the entity.
        /// </summary>
        [Parameter(ParameterSetName = "ByProperties", HelpMessage = "Ownership type: UserOwned, TeamOwned, OrganizationOwned")]
        [ValidateSet("UserOwned", "TeamOwned", "OrganizationOwned")]
        public string OwnershipType { get; set; }

        /// <summary>
        /// Gets or sets whether the entity supports activities.
        /// </summary>
        [Parameter(ParameterSetName = "ByProperties", HelpMessage = "Whether the entity supports activities")]
        public SwitchParameter HasActivities { get; set; }

        /// <summary>
        /// Gets or sets whether the entity supports notes.
        /// </summary>
        [Parameter(ParameterSetName = "ByProperties", HelpMessage = "Whether the entity supports notes (annotations)")]
        public SwitchParameter HasNotes { get; set; }

        /// <summary>
        /// Gets or sets whether audit is enabled.
        /// </summary>
        [Parameter(ParameterSetName = "ByProperties", HelpMessage = "Whether audit is enabled for this entity")]
        public SwitchParameter IsAuditEnabled { get; set; }

        /// <summary>
        /// Gets or sets whether change tracking is enabled.
        /// </summary>
        [Parameter(ParameterSetName = "ByProperties", HelpMessage = "Whether change tracking is enabled")]
        public SwitchParameter ChangeTrackingEnabled { get; set; }
        
        /// <summary>
        /// Gets or sets the vector icon name for the entity (SVG icon identifier).
        /// </summary>
        [Parameter(ParameterSetName = "ByProperties", HelpMessage = "Vector icon name (SVG icon identifier) for the entity")]
        public string IconVectorName { get; set; }
        
        /// <summary>
        /// Gets or sets the large icon name for the entity.
        /// </summary>
        [Parameter(ParameterSetName = "ByProperties", HelpMessage = "Large icon name for the entity")]
        public string IconLargeName { get; set; }
        
        /// <summary>
        /// Gets or sets the medium icon name for the entity.
        /// </summary>
        [Parameter(ParameterSetName = "ByProperties", HelpMessage = "Medium icon name for the entity")]
        public string IconMediumName { get; set; }
        
        /// <summary>
        /// Gets or sets the small icon name for the entity.
        /// </summary>
        [Parameter(ParameterSetName = "ByProperties", HelpMessage = "Small icon name for the entity")]
        public string IconSmallName { get; set; }

        /// <summary>
        /// Gets or sets the primary attribute schema name (for create).
        /// </summary>
        [Parameter(ParameterSetName = "ByProperties", HelpMessage = "Schema name for the primary name attribute (required for create, e.g., 'new_name')")]
        public string PrimaryAttributeSchemaName { get; set; }

        /// <summary>
        /// Gets or sets the primary attribute display name (for create).
        /// </summary>
        [Parameter(ParameterSetName = "ByProperties", HelpMessage = "Display name for the primary name attribute")]
        public string PrimaryAttributeDisplayName { get; set; }

        /// <summary>
        /// Gets or sets the primary attribute max length (for create).
        /// </summary>
        [Parameter(ParameterSetName = "ByProperties", HelpMessage = "Maximum length for the primary name attribute (default 100)")]
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
        /// Gets or sets whether to skip validation of icon properties against webresources.
        /// </summary>
        [Parameter(HelpMessage = "If specified, skips validation that icon properties reference valid webresources")]
        public SwitchParameter SkipIconValidation { get; set; }

        /// <summary>
        /// Processes the cmdlet.
        /// </summary>
        protected override void ProcessRecord()
        {
            base.ProcessRecord();
            
            // If using EntityMetadata parameter set, delegate to UpdateEntityFromMetadata
            if (ParameterSetName == "ByEntityMetadata")
            {
                UpdateEntityFromMetadata();
                return;
            }

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
            
            // Validate and set icon properties if provided
            if (!SkipIconValidation)
            {
                if (!string.IsNullOrWhiteSpace(IconVectorName))
                {
                    ValidateIconWebResource(IconVectorName, nameof(IconVectorName), 11); // 11 = SVG
                }
                
                if (!string.IsNullOrWhiteSpace(IconLargeName))
                {
                    ValidateIconWebResource(IconLargeName, nameof(IconLargeName), 5, 6, 7); // 5=PNG, 6=JPG, 7=GIF
                }
                
                if (!string.IsNullOrWhiteSpace(IconMediumName))
                {
                    ValidateIconWebResource(IconMediumName, nameof(IconMediumName), 5, 6, 7); // 5=PNG, 6=JPG, 7=GIF
                }
                
                if (!string.IsNullOrWhiteSpace(IconSmallName))
                {
                    ValidateIconWebResource(IconSmallName, nameof(IconSmallName), 5, 6, 7); // 5=PNG, 6=JPG, 7=GIF
                }
            }
            
            if (!string.IsNullOrWhiteSpace(IconVectorName))
            {
                entity.IconVectorName = IconVectorName;
            }
            
            if (!string.IsNullOrWhiteSpace(IconLargeName))
            {
                entity.IconLargeName = IconLargeName;
            }
            
            if (!string.IsNullOrWhiteSpace(IconMediumName))
            {
                entity.IconMediumName = IconMediumName;
            }
            
            if (!string.IsNullOrWhiteSpace(IconSmallName))
            {
                entity.IconSmallName = IconSmallName;
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
            
            // Validate icon properties before updating
            if (!SkipIconValidation)
            {
                if (MyInvocation.BoundParameters.ContainsKey(nameof(IconVectorName)))
                {
                    ValidateIconWebResource(IconVectorName, nameof(IconVectorName), 11); // 11 = SVG
                }
                
                if (MyInvocation.BoundParameters.ContainsKey(nameof(IconLargeName)))
                {
                    ValidateIconWebResource(IconLargeName, nameof(IconLargeName), 5, 6, 7); // 5=PNG, 6=JPG, 7=GIF
                }
                
                if (MyInvocation.BoundParameters.ContainsKey(nameof(IconMediumName)))
                {
                    ValidateIconWebResource(IconMediumName, nameof(IconMediumName), 5, 6, 7); // 5=PNG, 6=JPG, 7=GIF
                }
                
                if (MyInvocation.BoundParameters.ContainsKey(nameof(IconSmallName)))
                {
                    ValidateIconWebResource(IconSmallName, nameof(IconSmallName), 5, 6, 7); // 5=PNG, 6=JPG, 7=GIF
                }
            }
            
            // Update icon vector name
            if (MyInvocation.BoundParameters.ContainsKey(nameof(IconVectorName)))
            {
                entityToUpdate.IconVectorName = IconVectorName;
                hasChanges = true;
            }
            
            // Update icon large name
            if (MyInvocation.BoundParameters.ContainsKey(nameof(IconLargeName)))
            {
                entityToUpdate.IconLargeName = IconLargeName;
                hasChanges = true;
            }
            
            // Update icon medium name
            if (MyInvocation.BoundParameters.ContainsKey(nameof(IconMediumName)))
            {
                entityToUpdate.IconMediumName = IconMediumName;
                hasChanges = true;
            }
            
            // Update icon small name
            if (MyInvocation.BoundParameters.ContainsKey(nameof(IconSmallName)))
            {
                entityToUpdate.IconSmallName = IconSmallName;
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

        private void UpdateEntityFromMetadata()
        {
            if (EntityMetadata == null)
            {
                ThrowTerminatingError(new ErrorRecord(
                    new ArgumentNullException(nameof(EntityMetadata)),
                    "EntityMetadataNull",
                    ErrorCategory.InvalidArgument,
                    null));
                return;
            }

            // Ensure we have the MetadataId and LogicalName
            if (EntityMetadata.MetadataId == null || EntityMetadata.MetadataId == Guid.Empty)
            {
                ThrowTerminatingError(new ErrorRecord(
                    new ArgumentException("EntityMetadata must have a valid MetadataId"),
                    "InvalidMetadataId",
                    ErrorCategory.InvalidArgument,
                    EntityMetadata));
                return;
            }

            if (string.IsNullOrWhiteSpace(EntityMetadata.LogicalName))
            {
                ThrowTerminatingError(new ErrorRecord(
                    new ArgumentException("EntityMetadata must have a LogicalName"),
                    "InvalidLogicalName",
                    ErrorCategory.InvalidArgument,
                    EntityMetadata));
                return;
            }

            var request = new UpdateEntityRequest
            {
                Entity = EntityMetadata,
                MergeLabels = true
            };

            if (!ShouldProcess($"Entity '{EntityMetadata.LogicalName}'", "Update entity metadata from EntityMetadata object"))
            {
                return;
            }

            WriteVerbose($"Updating entity '{EntityMetadata.LogicalName}' from EntityMetadata object");

            Connection.Execute(request);

            WriteVerbose($"Entity updated successfully");

            // Invalidate cache for this entity
            var connectionKey = MetadataCache.GetConnectionKey(Connection as Microsoft.PowerPlatform.Dataverse.Client.ServiceClient);
            if (connectionKey != null)
            {
                MetadataCache.InvalidateEntity(connectionKey, EntityMetadata.LogicalName);
                WriteVerbose($"Invalidated metadata cache for entity '{EntityMetadata.LogicalName}'");
            }

            // Publish the entity if specified
            if (Publish && ShouldProcess($"Entity '{EntityMetadata.LogicalName}'", "Publish"))
            {
                var publishRequest = new PublishXmlRequest
                {
                    ParameterXml = $"<importexportxml><entities><entity>{EntityMetadata.LogicalName}</entity></entities></importexportxml>"
                };
                Connection.Execute(publishRequest);
                WriteVerbose($"Published entity '{EntityMetadata.LogicalName}'");
            }

            if (PassThru)
            {
                // Retrieve and return the updated entity
                var retrieveRequest = new RetrieveEntityRequest
                {
                    LogicalName = EntityMetadata.LogicalName,
                    EntityFilters = EntityFilters.Entity | EntityFilters.Attributes,
                    RetrieveAsIfPublished = true
                };

                var retrieveResponse = (RetrieveEntityResponse)Connection.Execute(retrieveRequest);
                var result = ConvertEntityMetadataToPSObject(retrieveResponse.EntityMetadata);
                WriteObject(result);
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

        /// <summary>
        /// Validates that an icon property references a valid webresource.
        /// </summary>
        /// <param name="iconName">The name of the icon/webresource to validate</param>
        /// <param name="propertyName">The name of the property being validated (for error messages)</param>
        /// <param name="allowedWebResourceTypes">The allowed webresource types (e.g., 11 for SVG, 5 for PNG, 6 for JPG, 7 for GIF)</param>
        private void ValidateIconWebResource(string iconName, string propertyName, params int[] allowedWebResourceTypes)
        {
            if (string.IsNullOrWhiteSpace(iconName))
            {
                return; // Empty values are allowed (clears the icon)
            }

            string typesDescription = allowedWebResourceTypes.Length == 1 
                ? $"type {allowedWebResourceTypes[0]}" 
                : $"one of types {string.Join(", ", allowedWebResourceTypes)}";
            WriteVerbose($"Validating {propertyName} '{iconName}' references a valid webresource of {typesDescription}");

            // Query for the webresource by name (including unpublished)
            var query = new QueryExpression("webresource")
            {
                ColumnSet = new ColumnSet("name", "webresourcetype"),
                Criteria = new FilterExpression
                {
                    Conditions =
                    {
                        new ConditionExpression("name", ConditionOperator.Equal, iconName)
                    }
                }
            };

            // Query both published and unpublished webresources
            var publishedResults = QueryHelpers.ExecuteQueryWithPaging(query, Connection, WriteVerbose).ToList();
            var unpublishedResults = QueryHelpers.ExecuteQueryWithPaging(query, Connection, WriteVerbose, unpublished: true).ToList();
            
            // Combine results (unpublished takes precedence if both exist)
            var allResults = unpublishedResults.Any() ? unpublishedResults : publishedResults;

            if (!allResults.Any())
            {
                ThrowTerminatingError(new ErrorRecord(
                    new ArgumentException($"{propertyName} '{iconName}' does not reference a valid webresource. No webresource with name '{iconName}' was found (checked both published and unpublished)."),
                    "InvalidIconWebResource",
                    ErrorCategory.InvalidArgument,
                    iconName));
                return;
            }

            // Validate webresource type
            var webResource = allResults.First();
            var webResourceType = webResource.GetAttributeValue<OptionSetValue>("webresourcetype");
            
            if (webResourceType == null || !allowedWebResourceTypes.Contains(webResourceType.Value))
            {
                var actualType = webResourceType?.Value.ToString() ?? "unknown";
                var expectedTypes = allowedWebResourceTypes.Length == 1
                    ? $"type {allowedWebResourceTypes[0]}"
                    : $"one of types {string.Join(", ", allowedWebResourceTypes)}";
                ThrowTerminatingError(new ErrorRecord(
                    new ArgumentException($"{propertyName} '{iconName}' references a webresource with type {actualType}, but {expectedTypes} is required."),
                    "InvalidIconWebResourceType",
                    ErrorCategory.InvalidArgument,
                    iconName));
                return;
            }

            WriteVerbose($"{propertyName} '{iconName}' validated successfully");
        }
    }
}
