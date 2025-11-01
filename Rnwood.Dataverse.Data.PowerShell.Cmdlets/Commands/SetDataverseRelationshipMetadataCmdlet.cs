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
    /// Creates or updates a relationship in Dataverse.
    /// </summary>
    [Cmdlet(VerbsCommon.Set, "DataverseRelationshipMetadata", SupportsShouldProcess = true, ConfirmImpact = ConfirmImpact.Medium)]
    [OutputType(typeof(RelationshipMetadataBase))]
    public class SetDataverseRelationshipMetadataCmdlet : OrganizationServiceCmdlet
    {
        private int _baseLanguageCode;

        /// <summary>
        /// Gets or sets the schema name of the relationship.
        /// </summary>
        [Parameter(Mandatory = true, Position = 0, HelpMessage = "Schema name of the relationship (e.g., 'new_project_contact')")]
        public string SchemaName { get; set; }

        /// <summary>
        /// Gets or sets the relationship type.
        /// </summary>
        [Parameter(Mandatory = true, HelpMessage = "Type of relationship: OneToMany or ManyToMany")]
        [ValidateSet("OneToMany", "ManyToMany")]
        public string RelationshipType { get; set; }

        /// <summary>
        /// Gets or sets the primary/referenced entity name (for OneToMany) or first entity (for ManyToMany).
        /// </summary>
        [Parameter(Mandatory = true, HelpMessage = "Primary/referenced entity name (OneToMany) or first entity name (ManyToMany)")]
        [ArgumentCompleter(typeof(TableNameArgumentCompleter))]
        [Alias("PrimaryEntity", "Entity1")]
        public string ReferencedEntity { get; set; }

        /// <summary>
        /// Gets or sets the related/referencing entity name (for OneToMany) or second entity (for ManyToMany).
        /// </summary>
        [Parameter(Mandatory = true, HelpMessage = "Related/referencing entity name (OneToMany) or second entity name (ManyToMany)")]
        [ArgumentCompleter(typeof(TableNameArgumentCompleter))]
        [Alias("RelatedEntity", "Entity2")]
        public string ReferencingEntity { get; set; }

        /// <summary>
        /// Gets or sets the lookup attribute schema name (OneToMany only).
        /// </summary>
        [Parameter(HelpMessage = "Schema name of the lookup attribute to create on the referencing entity (OneToMany only, e.g., 'new_ProjectId')")]
        public string LookupAttributeSchemaName { get; set; }

        /// <summary>
        /// Gets or sets the lookup attribute display name (OneToMany only).
        /// </summary>
        [Parameter(HelpMessage = "Display name of the lookup attribute (OneToMany only)")]
        public string LookupAttributeDisplayName { get; set; }

        /// <summary>
        /// Gets or sets the lookup attribute description (OneToMany only).
        /// </summary>
        [Parameter(HelpMessage = "Description of the lookup attribute (OneToMany only)")]
        public string LookupAttributeDescription { get; set; }

        /// <summary>
        /// Gets or sets the required level of the lookup attribute (OneToMany only).
        /// </summary>
        [Parameter(HelpMessage = "Required level of the lookup attribute (OneToMany only): None, SystemRequired, ApplicationRequired, Recommended")]
        [ValidateSet("None", "SystemRequired", "ApplicationRequired", "Recommended")]
        public string LookupAttributeRequiredLevel { get; set; } = "None";

        /// <summary>
        /// Gets or sets the intersect entity name for ManyToMany relationships.
        /// </summary>
        [Parameter(HelpMessage = "Schema name of the intersect entity for ManyToMany relationships (e.g., 'new_project_contact'). If not specified, generated automatically.")]
        public string IntersectEntityName { get; set; }

        /// <summary>
        /// Gets or sets the cascading behavior for assign operations.
        /// </summary>
        [Parameter(HelpMessage = "Cascade behavior for Assign: NoCascade, Cascade, Active, UserOwned, RemoveLink")]
        [ValidateSet("NoCascade", "Cascade", "Active", "UserOwned", "RemoveLink")]
        public string CascadeAssign { get; set; } = "NoCascade";

        /// <summary>
        /// Gets or sets the cascading behavior for share operations.
        /// </summary>
        [Parameter(HelpMessage = "Cascade behavior for Share: NoCascade, Cascade, Active, UserOwned")]
        [ValidateSet("NoCascade", "Cascade", "Active", "UserOwned")]
        public string CascadeShare { get; set; } = "NoCascade";

        /// <summary>
        /// Gets or sets the cascading behavior for unshare operations.
        /// </summary>
        [Parameter(HelpMessage = "Cascade behavior for Unshare: NoCascade, Cascade, Active, UserOwned")]
        [ValidateSet("NoCascade", "Cascade", "Active", "UserOwned")]
        public string CascadeUnshare { get; set; } = "NoCascade";

        /// <summary>
        /// Gets or sets the cascading behavior for reparent operations.
        /// </summary>
        [Parameter(HelpMessage = "Cascade behavior for Reparent: NoCascade, Cascade, Active, UserOwned, RemoveLink")]
        [ValidateSet("NoCascade", "Cascade", "Active", "UserOwned", "RemoveLink")]
        public string CascadeReparent { get; set; } = "NoCascade";

        /// <summary>
        /// Gets or sets the cascading behavior for delete operations.
        /// </summary>
        [Parameter(HelpMessage = "Cascade behavior for Delete: NoCascade, RemoveLink, Restrict, Cascade")]
        [ValidateSet("NoCascade", "RemoveLink", "Restrict", "Cascade")]
        public string CascadeDelete { get; set; } = "RemoveLink";

        /// <summary>
        /// Gets or sets the cascading behavior for merge operations.
        /// </summary>
        [Parameter(HelpMessage = "Cascade behavior for Merge: NoCascade, Cascade")]
        [ValidateSet("NoCascade", "Cascade")]
        public string CascadeMerge { get; set; } = "NoCascade";

        /// <summary>
        /// Gets or sets whether the relationship is searchable.
        /// </summary>
        [Parameter(HelpMessage = "Whether the relationship is searchable")]
        public SwitchParameter IsHierarchical { get; set; }

        /// <summary>
        /// Gets or sets whether the lookup attribute should be searchable (OneToMany only).
        /// </summary>
        [Parameter(HelpMessage = "Whether the lookup attribute is searchable (OneToMany only)")]
        public SwitchParameter IsSearchable { get; set; }

        /// <summary>
        /// Gets or sets whether to return the created/updated relationship.
        /// </summary>
        [Parameter(HelpMessage = "Return the created or updated relationship metadata")]
        public SwitchParameter PassThru { get; set; }

        /// <summary>
        /// Processes the cmdlet.
        /// </summary>
        protected override void ProcessRecord()
        {
            base.ProcessRecord();

            _baseLanguageCode = GetBaseLanguageCode();

            // Check if relationship exists
            bool relationshipExists = CheckRelationshipExists(SchemaName);

            if (relationshipExists)
            {
                // For now, we'll just inform the user that update is not supported
                // Full update support would require more complex logic
                WriteWarning($"Relationship '{SchemaName}' already exists");
                
                if (!ShouldProcess(SchemaName, "Update relationship"))
                {
                    return;
                }

                // Only some properties can be updated
                UpdateRelationship();
            }
            else
            {
                if (!ShouldProcess(SchemaName, $"Create {RelationshipType} relationship"))
                {
                    return;
                }

                CreateRelationship();
            }

            // Invalidate cache after modification
            MetadataCache.InvalidateEntity(
                MetadataCache.GetConnectionKey(Connection as Microsoft.PowerPlatform.Dataverse.Client.ServiceClient),
                ReferencedEntity
            );
            MetadataCache.InvalidateEntity(
                MetadataCache.GetConnectionKey(Connection as Microsoft.PowerPlatform.Dataverse.Client.ServiceClient),
                ReferencingEntity
            );
        }

        private bool CheckRelationshipExists(string schemaName)
        {
            try
            {
                var request = new RetrieveRelationshipRequest
                {
                    Name = schemaName
                };

                Connection.Execute(request);
                return true;
            }
            catch (FaultException<OrganizationServiceFault> ex)
            {
                if (ex.HResult == -2146233088) // Object does not exist
                {
                    return false;
                }
                else
                {
                    throw;
                }
            }
        }

        private void CreateRelationship()
        {
            switch (RelationshipType)
            {
                case "OneToMany":
                    CreateOneToManyRelationship();
                    break;
                case "ManyToMany":
                    CreateManyToManyRelationship();
                    break;
            }
        }

        private void CreateOneToManyRelationship()
        {
            if (string.IsNullOrWhiteSpace(LookupAttributeSchemaName))
            {
                throw new PSArgumentException("LookupAttributeSchemaName is required for OneToMany relationships");
            }

            var lookup = new LookupAttributeMetadata
            {
                SchemaName = LookupAttributeSchemaName,
                LogicalName = LookupAttributeSchemaName.ToLower(),
                DisplayName = new Label(LookupAttributeDisplayName ?? LookupAttributeSchemaName, _baseLanguageCode),
                Description = new Label(LookupAttributeDescription ?? string.Empty, _baseLanguageCode),
                RequiredLevel = new AttributeRequiredLevelManagedProperty(ParseRequiredLevel(LookupAttributeRequiredLevel)),
                IsValidForAdvancedFind = new BooleanManagedProperty(IsSearchable.IsPresent)
            };

            var relationship = new OneToManyRelationshipMetadata
            {
                SchemaName = SchemaName,
                ReferencedEntity = ReferencedEntity,
                ReferencingEntity = ReferencingEntity,
                CascadeConfiguration = new CascadeConfiguration
                {
                    Assign = ParseCascadeType(CascadeAssign),
                    Share = ParseCascadeType(CascadeShare),
                    Unshare = ParseCascadeType(CascadeUnshare),
                    Reparent = ParseCascadeType(CascadeReparent),
                    Delete = ParseCascadeType(CascadeDelete),
                    Merge = ParseCascadeType(CascadeMerge)
                },
                IsHierarchical = IsHierarchical.IsPresent
            };

            var request = new CreateOneToManyRequest
            {
                Lookup = lookup,
                OneToManyRelationship = relationship
            };

            WriteVerbose($"Creating OneToMany relationship '{SchemaName}' from {ReferencedEntity} to {ReferencingEntity}");

            var response = (CreateOneToManyResponse)Connection.Execute(request);

            WriteVerbose($"Relationship created with ID: {response.RelationshipId}");

            if (PassThru)
            {
                // Retrieve the created relationship
                var retrieveRequest = new RetrieveRelationshipRequest
                {
                    Name = SchemaName
                };
                var retrieveResponse = (RetrieveRelationshipResponse)Connection.Execute(retrieveRequest);
                WriteObject(retrieveResponse.RelationshipMetadata);
            }
        }

        private void CreateManyToManyRelationship()
        {
            var relationship = new ManyToManyRelationshipMetadata
            {
                SchemaName = SchemaName,
                Entity1LogicalName = ReferencedEntity,
                Entity2LogicalName = ReferencingEntity,
                IntersectEntityName = IntersectEntityName ?? SchemaName.ToLower()
            };

            var request = new CreateManyToManyRequest
            {
                ManyToManyRelationship = relationship
            };

            WriteVerbose($"Creating ManyToMany relationship '{SchemaName}' between {ReferencedEntity} and {ReferencingEntity}");

            var response = (CreateManyToManyResponse)Connection.Execute(request);

            WriteVerbose($"ManyToMany relationship created successfully");

            if (PassThru)
            {
                // Retrieve the created relationship
                var retrieveRequest = new RetrieveRelationshipRequest
                {
                    Name = SchemaName
                };
                var retrieveResponse = (RetrieveRelationshipResponse)Connection.Execute(retrieveRequest);
                WriteObject(retrieveResponse.RelationshipMetadata);
            }
        }

        private void UpdateRelationship()
        {
            // Retrieve the existing relationship
            var retrieveRequest = new RetrieveRelationshipRequest
            {
                Name = SchemaName
            };
            
            var retrieveResponse = (RetrieveRelationshipResponse)Connection.Execute(retrieveRequest);
            var existingRelationship = retrieveResponse.RelationshipMetadata;

            WriteVerbose($"Retrieved existing relationship '{SchemaName}' of type {existingRelationship.RelationshipType}");

            // Validate that immutable properties haven't been changed
            ValidateImmutableProperties(existingRelationship);

            // Clone the relationship for update
            RelationshipMetadataBase relationshipToUpdate;
            bool hasChanges = false;

            if (existingRelationship is OneToManyRelationshipMetadata existingOneToMany)
            {
                var updatedOneToMany = new OneToManyRelationshipMetadata
                {
                    MetadataId = existingOneToMany.MetadataId,
                    SchemaName = existingOneToMany.SchemaName,
                    ReferencedEntity = existingOneToMany.ReferencedEntity,
                    ReferencingEntity = existingOneToMany.ReferencingEntity,
                    ReferencedAttribute = existingOneToMany.ReferencedAttribute,
                    ReferencingAttribute = existingOneToMany.ReferencingAttribute,
                    IsHierarchical = existingOneToMany.IsHierarchical,
                    CascadeConfiguration = new CascadeConfiguration
                    {
                        Assign = existingOneToMany.CascadeConfiguration?.Assign ?? CascadeType.NoCascade,
                        Share = existingOneToMany.CascadeConfiguration?.Share ?? CascadeType.NoCascade,
                        Unshare = existingOneToMany.CascadeConfiguration?.Unshare ?? CascadeType.NoCascade,
                        Reparent = existingOneToMany.CascadeConfiguration?.Reparent ?? CascadeType.NoCascade,
                        Delete = existingOneToMany.CascadeConfiguration?.Delete ?? CascadeType.NoCascade,
                        Merge = existingOneToMany.CascadeConfiguration?.Merge ?? CascadeType.NoCascade
                    }
                };

                // Check if cascade behaviors were specified - if any were, update them
                if (MyInvocation.BoundParameters.ContainsKey(nameof(CascadeAssign)))
                {
                    updatedOneToMany.CascadeConfiguration.Assign = ParseCascadeType(CascadeAssign);
                    hasChanges = true;
                    WriteVerbose($"Updating CascadeAssign to {CascadeAssign}");
                }
                
                if (MyInvocation.BoundParameters.ContainsKey(nameof(CascadeShare)))
                {
                    updatedOneToMany.CascadeConfiguration.Share = ParseCascadeType(CascadeShare);
                    hasChanges = true;
                    WriteVerbose($"Updating CascadeShare to {CascadeShare}");
                }
                
                if (MyInvocation.BoundParameters.ContainsKey(nameof(CascadeUnshare)))
                {
                    updatedOneToMany.CascadeConfiguration.Unshare = ParseCascadeType(CascadeUnshare);
                    hasChanges = true;
                    WriteVerbose($"Updating CascadeUnshare to {CascadeUnshare}");
                }
                
                if (MyInvocation.BoundParameters.ContainsKey(nameof(CascadeReparent)))
                {
                    updatedOneToMany.CascadeConfiguration.Reparent = ParseCascadeType(CascadeReparent);
                    hasChanges = true;
                    WriteVerbose($"Updating CascadeReparent to {CascadeReparent}");
                }
                
                if (MyInvocation.BoundParameters.ContainsKey(nameof(CascadeDelete)))
                {
                    updatedOneToMany.CascadeConfiguration.Delete = ParseCascadeType(CascadeDelete);
                    hasChanges = true;
                    WriteVerbose($"Updating CascadeDelete to {CascadeDelete}");
                }
                
                if (MyInvocation.BoundParameters.ContainsKey(nameof(CascadeMerge)))
                {
                    updatedOneToMany.CascadeConfiguration.Merge = ParseCascadeType(CascadeMerge);
                    hasChanges = true;
                    WriteVerbose($"Updating CascadeMerge to {CascadeMerge}");
                }

                if (MyInvocation.BoundParameters.ContainsKey(nameof(IsHierarchical)))
                {
                    updatedOneToMany.IsHierarchical = IsHierarchical.IsPresent;
                    hasChanges = true;
                    WriteVerbose($"Updating IsHierarchical to {IsHierarchical.IsPresent}");
                }

                relationshipToUpdate = updatedOneToMany;
            }
            else if (existingRelationship is ManyToManyRelationshipMetadata existingManyToMany)
            {
                // ManyToMany relationships have very limited updateable properties
                var updatedManyToMany = new ManyToManyRelationshipMetadata
                {
                    MetadataId = existingManyToMany.MetadataId,
                    SchemaName = existingManyToMany.SchemaName,
                    Entity1LogicalName = existingManyToMany.Entity1LogicalName,
                    Entity2LogicalName = existingManyToMany.Entity2LogicalName,
                    IntersectEntityName = existingManyToMany.IntersectEntityName
                };

                relationshipToUpdate = updatedManyToMany;
                
                // ManyToMany relationships don't have many updateable properties
                WriteWarning("ManyToMany relationships have very limited updateable properties. Most properties are immutable after creation.");
            }
            else
            {
                WriteError(new ErrorRecord(
                    new NotSupportedException($"Unsupported relationship type: {existingRelationship.GetType().Name}"),
                    "UnsupportedRelationshipType",
                    ErrorCategory.InvalidOperation,
                    existingRelationship));
                return;
            }

            if (!hasChanges)
            {
                WriteWarning($"No changes specified for relationship '{SchemaName}'. Specify cascade behavior parameters to update the relationship.");
                
                if (PassThru)
                {
                    WriteObject(existingRelationship);
                }
                return;
            }

            var updateRequest = new UpdateRelationshipRequest
            {
                Relationship = relationshipToUpdate,
                MergeLabels = true
            };

            WriteVerbose($"Updating relationship '{SchemaName}'");

            Connection.Execute(updateRequest);

            WriteVerbose($"Relationship updated successfully");

            if (PassThru)
            {
                // Retrieve and return the updated relationship
                var retrieveUpdatedRequest = new RetrieveRelationshipRequest
                {
                    Name = SchemaName
                };
                var retrieveUpdatedResponse = (RetrieveRelationshipResponse)Connection.Execute(retrieveUpdatedRequest);
                WriteObject(retrieveUpdatedResponse.RelationshipMetadata);
            }
        }

        private void ValidateImmutableProperties(RelationshipMetadataBase existingRelationship)
        {
            // Check if relationship type matches
            if (MyInvocation.BoundParameters.ContainsKey(nameof(RelationshipType)))
            {
                var existingType = existingRelationship is OneToManyRelationshipMetadata ? "OneToMany" : "ManyToMany";
                if (!string.Equals(RelationshipType, existingType, StringComparison.OrdinalIgnoreCase))
                {
                    ThrowTerminatingError(new ErrorRecord(
                        new InvalidOperationException($"Cannot change relationship type from {existingType} to {RelationshipType}. This property is immutable after creation."),
                        "ImmutableRelationshipType",
                        ErrorCategory.InvalidOperation,
                        RelationshipType));
                }
            }

            if (existingRelationship is OneToManyRelationshipMetadata oneToMany)
            {
                // Check ReferencedEntity
                if (MyInvocation.BoundParameters.ContainsKey(nameof(ReferencedEntity)) &&
                    !string.Equals(ReferencedEntity, oneToMany.ReferencedEntity, StringComparison.OrdinalIgnoreCase))
                {
                    ThrowTerminatingError(new ErrorRecord(
                        new InvalidOperationException($"Cannot change ReferencedEntity from '{oneToMany.ReferencedEntity}' to '{ReferencedEntity}'. This property is immutable after creation."),
                        "ImmutableReferencedEntity",
                        ErrorCategory.InvalidOperation,
                        ReferencedEntity));
                }

                // Check ReferencingEntity
                if (MyInvocation.BoundParameters.ContainsKey(nameof(ReferencingEntity)) &&
                    !string.Equals(ReferencingEntity, oneToMany.ReferencingEntity, StringComparison.OrdinalIgnoreCase))
                {
                    ThrowTerminatingError(new ErrorRecord(
                        new InvalidOperationException($"Cannot change ReferencingEntity from '{oneToMany.ReferencingEntity}' to '{ReferencingEntity}'. This property is immutable after creation."),
                        "ImmutableReferencingEntity",
                        ErrorCategory.InvalidOperation,
                        ReferencingEntity));
                }

                // Check lookup attribute properties (immutable after creation)
                if (MyInvocation.BoundParameters.ContainsKey(nameof(LookupAttributeSchemaName)))
                {
                    ThrowTerminatingError(new ErrorRecord(
                        new InvalidOperationException($"Cannot change LookupAttributeSchemaName when updating a relationship. The lookup attribute is immutable after creation."),
                        "ImmutableLookupAttribute",
                        ErrorCategory.InvalidOperation,
                        LookupAttributeSchemaName));
                }

                if (MyInvocation.BoundParameters.ContainsKey(nameof(LookupAttributeDisplayName)))
                {
                    ThrowTerminatingError(new ErrorRecord(
                        new InvalidOperationException($"Cannot change LookupAttributeDisplayName when updating a relationship. The lookup attribute is immutable after creation."),
                        "ImmutableLookupAttribute",
                        ErrorCategory.InvalidOperation,
                        LookupAttributeDisplayName));
                }

                if (MyInvocation.BoundParameters.ContainsKey(nameof(LookupAttributeDescription)))
                {
                    ThrowTerminatingError(new ErrorRecord(
                        new InvalidOperationException($"Cannot change LookupAttributeDescription when updating a relationship. The lookup attribute is immutable after creation."),
                        "ImmutableLookupAttribute",
                        ErrorCategory.InvalidOperation,
                        LookupAttributeDescription));
                }

                if (MyInvocation.BoundParameters.ContainsKey(nameof(LookupAttributeRequiredLevel)))
                {
                    ThrowTerminatingError(new ErrorRecord(
                        new InvalidOperationException($"Cannot change LookupAttributeRequiredLevel when updating a relationship. The lookup attribute is immutable after creation."),
                        "ImmutableLookupAttribute",
                        ErrorCategory.InvalidOperation,
                        LookupAttributeRequiredLevel));
                }

                if (MyInvocation.BoundParameters.ContainsKey(nameof(IsSearchable)))
                {
                    ThrowTerminatingError(new ErrorRecord(
                        new InvalidOperationException($"Cannot change IsSearchable when updating a relationship. The lookup attribute is immutable after creation."),
                        "ImmutableLookupAttribute",
                        ErrorCategory.InvalidOperation,
                        null));
                }
            }
            else if (existingRelationship is ManyToManyRelationshipMetadata manyToMany)
            {
                // Check Entity1
                if (MyInvocation.BoundParameters.ContainsKey(nameof(ReferencedEntity)) &&
                    !string.Equals(ReferencedEntity, manyToMany.Entity1LogicalName, StringComparison.OrdinalIgnoreCase))
                {
                    ThrowTerminatingError(new ErrorRecord(
                        new InvalidOperationException($"Cannot change Entity1 (ReferencedEntity) from '{manyToMany.Entity1LogicalName}' to '{ReferencedEntity}'. This property is immutable after creation."),
                        "ImmutableEntity1",
                        ErrorCategory.InvalidOperation,
                        ReferencedEntity));
                }

                // Check Entity2
                if (MyInvocation.BoundParameters.ContainsKey(nameof(ReferencingEntity)) &&
                    !string.Equals(ReferencingEntity, manyToMany.Entity2LogicalName, StringComparison.OrdinalIgnoreCase))
                {
                    ThrowTerminatingError(new ErrorRecord(
                        new InvalidOperationException($"Cannot change Entity2 (ReferencingEntity) from '{manyToMany.Entity2LogicalName}' to '{ReferencingEntity}'. This property is immutable after creation."),
                        "ImmutableEntity2",
                        ErrorCategory.InvalidOperation,
                        ReferencingEntity));
                }

                // Check IntersectEntityName
                if (MyInvocation.BoundParameters.ContainsKey(nameof(IntersectEntityName)) &&
                    !string.Equals(IntersectEntityName, manyToMany.IntersectEntityName, StringComparison.OrdinalIgnoreCase))
                {
                    ThrowTerminatingError(new ErrorRecord(
                        new InvalidOperationException($"Cannot change IntersectEntityName from '{manyToMany.IntersectEntityName}' to '{IntersectEntityName}'. This property is immutable after creation."),
                        "ImmutableIntersectEntityName",
                        ErrorCategory.InvalidOperation,
                        IntersectEntityName));
                }

                // Check if cascade behaviors are specified (not supported for ManyToMany)
                var cascadeParams = new[] { nameof(CascadeAssign), nameof(CascadeShare), nameof(CascadeUnshare), 
                                            nameof(CascadeReparent), nameof(CascadeDelete), nameof(CascadeMerge) };
                foreach (var param in cascadeParams)
                {
                    if (MyInvocation.BoundParameters.ContainsKey(param))
                    {
                        ThrowTerminatingError(new ErrorRecord(
                            new InvalidOperationException($"Cannot set {param} on ManyToMany relationships. Cascade behaviors are not applicable to this relationship type."),
                            "UnsupportedCascadeBehavior",
                            ErrorCategory.InvalidOperation,
                            param));
                    }
                }

                if (MyInvocation.BoundParameters.ContainsKey(nameof(IsHierarchical)))
                {
                    ThrowTerminatingError(new ErrorRecord(
                        new InvalidOperationException($"Cannot set IsHierarchical on ManyToMany relationships. This property is only applicable to OneToMany relationships."),
                        "UnsupportedIsHierarchical",
                        ErrorCategory.InvalidOperation,
                        null));
                }
            }
        }

        private AttributeRequiredLevel ParseRequiredLevel(string level)
        {
            switch (level)
            {
                case "None":
                    return AttributeRequiredLevel.None;
                case "SystemRequired":
                    return AttributeRequiredLevel.SystemRequired;
                case "ApplicationRequired":
                    return AttributeRequiredLevel.ApplicationRequired;
                case "Recommended":
                    return AttributeRequiredLevel.Recommended;
                default:
                    return AttributeRequiredLevel.None;
            }
        }

        private CascadeType ParseCascadeType(string cascadeType)
        {
            switch (cascadeType)
            {
                case "NoCascade":
                    return CascadeType.NoCascade;
                case "Cascade":
                    return CascadeType.Cascade;
                case "Active":
                    return CascadeType.Active;
                case "UserOwned":
                    return CascadeType.UserOwned;
                case "RemoveLink":
                    return CascadeType.RemoveLink;
                case "Restrict":
                    return CascadeType.Restrict;
                default:
                    return CascadeType.NoCascade;
            }
        }
    }
}
