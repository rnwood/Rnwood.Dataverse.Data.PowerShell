using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Messages;
using Microsoft.Xrm.Sdk.Metadata;
using System;
using System.Linq;
using System.Management.Automation;

namespace Rnwood.Dataverse.Data.PowerShell.Commands
{
    /// <summary>
    /// Retrieves relationship metadata from Dataverse.
    /// </summary>
    [Cmdlet(VerbsCommon.Get, "DataverseRelationshipMetadata")]
    [OutputType(typeof(RelationshipMetadataBase))]
    public class GetDataverseRelationshipMetadataCmdlet : OrganizationServiceCmdlet
    {
        /// <summary>
        /// Gets or sets the logical name of the entity.
        /// If not specified, returns all relationships in the system.
        /// </summary>
        [Parameter(Mandatory = false, Position = 0, HelpMessage = "Logical name of the entity (table). If not specified, returns all relationships.")]
        [ArgumentCompleter(typeof(TableNameArgumentCompleter))]
        [Alias("TableName")]
        public string EntityName { get; set; }

        /// <summary>
        /// Gets or sets the schema name of the relationship.
        /// If not specified, returns all relationships for the entity (or all relationships if no entity specified).
        /// </summary>
        [Parameter(Mandatory = false, Position = 1, HelpMessage = "Schema name of the relationship. If not specified, returns all relationships.")]
        public string RelationshipName { get; set; }

        /// <summary>
        /// Gets or sets the relationship type filter.
        /// </summary>
        [Parameter(HelpMessage = "Filter by relationship type: OneToMany, ManyToOne, or ManyToMany")]
        [ValidateSet("OneToMany", "ManyToOne", "ManyToMany")]
        public string RelationshipType { get; set; }

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

            if (!string.IsNullOrWhiteSpace(RelationshipName))
            {
                // Retrieve specific relationship
                RetrieveSpecificRelationship();
            }
            else if (!string.IsNullOrWhiteSpace(EntityName))
            {
                // Retrieve all relationships for the entity
                RetrieveEntityRelationships();
            }
            else
            {
                // Retrieve all relationships in the system
                RetrieveAllRelationships();
            }
        }

        private void RetrieveSpecificRelationship()
        {
            var request = new RetrieveRelationshipRequest
            {
                Name = RelationshipName
            };

            WriteVerbose($"Retrieving relationship metadata for '{RelationshipName}'");

            var response = (RetrieveRelationshipResponse)Connection.Execute(request);
            var relationship = response.RelationshipMetadata;

            // Filter by relationship type if specified
            if (!string.IsNullOrWhiteSpace(RelationshipType) && !MatchesRelationshipType(relationship, RelationshipType))
            {
                WriteWarning($"Relationship '{RelationshipName}' is not a {RelationshipType} relationship");
                return;
            }

            WriteObject(relationship);
        }

        private void RetrieveEntityRelationships()
        {
            var request = new RetrieveEntityRequest
            {
                LogicalName = EntityName,
                EntityFilters = EntityFilters.Relationships,
                RetrieveAsIfPublished = !Published.IsPresent
            };

            WriteVerbose($"Retrieving relationships for entity '{EntityName}'");

            var response = (RetrieveEntityResponse)Connection.Execute(request);
            var entityMetadata = response.EntityMetadata;

            var relationships = new System.Collections.Generic.List<RelationshipMetadataBase>();

            // Add OneToMany relationships (where this entity is the primary/referenced entity)
            if (entityMetadata.OneToManyRelationships != null)
            {
                relationships.AddRange(entityMetadata.OneToManyRelationships);
            }

            // Add ManyToOne relationships (where this entity is the related/referencing entity)
            if (entityMetadata.ManyToOneRelationships != null)
            {
                relationships.AddRange(entityMetadata.ManyToOneRelationships);
            }

            // Add ManyToMany relationships
            if (entityMetadata.ManyToManyRelationships != null)
            {
                relationships.AddRange(entityMetadata.ManyToManyRelationships);
            }

            // Filter by relationship type if specified
            if (!string.IsNullOrWhiteSpace(RelationshipType))
            {
                relationships = relationships.Where(r => MatchesRelationshipType(r, RelationshipType)).ToList();
            }

            WriteVerbose($"Retrieved {relationships.Count} relationships");

            var results = relationships
                .OrderBy(r => r.SchemaName, StringComparer.OrdinalIgnoreCase)
                .ToArray();

            WriteObject(results, true);
        }

        private void RetrieveAllRelationships()
        {
            WriteVerbose("Retrieving all relationships from all entities");

            // Retrieve all entities with relationships
            var allEntitiesRequest = new RetrieveAllEntitiesRequest
            {
                EntityFilters = EntityFilters.Relationships,
                RetrieveAsIfPublished = !Published.IsPresent
            };

            var allEntitiesResponse = (RetrieveAllEntitiesResponse)Connection.Execute(allEntitiesRequest);
            var entities = allEntitiesResponse.EntityMetadata;

            var allRelationships = new System.Collections.Generic.Dictionary<string, RelationshipMetadataBase>();

            foreach (var entity in entities)
            {
                // Add OneToMany relationships
                if (entity.OneToManyRelationships != null)
                {
                    foreach (var rel in entity.OneToManyRelationships)
                    {
                        if (!allRelationships.ContainsKey(rel.SchemaName))
                        {
                            allRelationships[rel.SchemaName] = rel;
                        }
                    }
                }

                // Add ManyToOne relationships (skip duplicates as they're the same as OneToMany from the other side)
                if (entity.ManyToOneRelationships != null)
                {
                    foreach (var rel in entity.ManyToOneRelationships)
                    {
                        if (!allRelationships.ContainsKey(rel.SchemaName))
                        {
                            allRelationships[rel.SchemaName] = rel;
                        }
                    }
                }

                // Add ManyToMany relationships
                if (entity.ManyToManyRelationships != null)
                {
                    foreach (var rel in entity.ManyToManyRelationships)
                    {
                        if (!allRelationships.ContainsKey(rel.SchemaName))
                        {
                            allRelationships[rel.SchemaName] = rel;
                        }
                    }
                }
            }

            // Filter by relationship type if specified
            var relationships = allRelationships.Values.AsEnumerable();
            if (!string.IsNullOrWhiteSpace(RelationshipType))
            {
                relationships = relationships.Where(r => MatchesRelationshipType(r, RelationshipType));
            }

            WriteVerbose($"Retrieved {allRelationships.Count} unique relationships");

            var results = relationships
                .OrderBy(r => r.SchemaName, StringComparer.OrdinalIgnoreCase)
                .ToArray();

            WriteObject(results, true);
        }

        private bool MatchesRelationshipType(RelationshipMetadataBase relationship, string relationshipType)
        {
            switch (relationshipType)
            {
                case "OneToMany":
                    return relationship is OneToManyRelationshipMetadata && 
                           relationship.RelationshipType == Microsoft.Xrm.Sdk.Metadata.RelationshipType.OneToManyRelationship;
                case "ManyToOne":
                    return relationship is OneToManyRelationshipMetadata &&
                           relationship.RelationshipType == Microsoft.Xrm.Sdk.Metadata.RelationshipType.OneToManyRelationship;
                case "ManyToMany":
                    return relationship is ManyToManyRelationshipMetadata;
                default:
                    return true;
            }
        }
    }
}
