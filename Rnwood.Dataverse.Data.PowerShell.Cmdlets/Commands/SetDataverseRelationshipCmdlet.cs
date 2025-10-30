using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Messages;
using Microsoft.Xrm.Sdk.Metadata;
using System;
using System.Linq;
using System.Management.Automation;

namespace Rnwood.Dataverse.Data.PowerShell.Commands
{
    /// <summary>
    /// Creates or updates a relationship in Dataverse.
    /// </summary>
    [Cmdlet(VerbsCommon.Set, "DataverseRelationship", SupportsShouldProcess = true, ConfirmImpact = ConfirmImpact.High)]
    [OutputType(typeof(RelationshipMetadataBase))]
    public class SetDataverseRelationshipCmdlet : OrganizationServiceCmdlet
    {
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

            // Check if relationship exists
            bool relationshipExists = CheckRelationshipExists(SchemaName);

            if (relationshipExists)
            {
                // For now, we'll just inform the user that update is not supported
                // Full update support would require more complex logic
                WriteWarning($"Relationship '{SchemaName}' already exists. Updating existing relationships is limited in Dataverse. Consider removing and recreating.");
                
                if (!ShouldProcess(SchemaName, "Update relationship (limited support)"))
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
            catch (Exception ex)
            {
                WriteVerbose($"Relationship does not exist: {ex.Message}");
                return false;
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
                DisplayName = new Label(LookupAttributeDisplayName ?? LookupAttributeSchemaName, 1033),
                Description = new Label(LookupAttributeDescription ?? string.Empty, 1033),
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
            // Relationship updates are very limited in Dataverse
            // Most properties are immutable after creation
            WriteWarning("Relationship update functionality is limited. Most relationship properties are immutable after creation.");
            
            // For now, we just retrieve and display the relationship
            if (PassThru)
            {
                var retrieveRequest = new RetrieveRelationshipRequest
                {
                    Name = SchemaName
                };
                var retrieveResponse = (RetrieveRelationshipResponse)Connection.Execute(retrieveRequest);
                WriteObject(retrieveResponse.RelationshipMetadata);
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
