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
    [OutputType(typeof(PSObject))]
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
        /// Gets or sets whether to include attribute metadata in the output.
        /// </summary>
        [Parameter(HelpMessage = "Include attribute (column) metadata in the output")]
        public SwitchParameter IncludeAttributes { get; set; }

        /// <summary>
        /// Gets or sets whether to include relationship metadata in the output.
        /// </summary>
        [Parameter(HelpMessage = "Include relationship metadata in the output")]
        public SwitchParameter IncludeRelationships { get; set; }

        /// <summary>
        /// Gets or sets whether to include privilege metadata in the output.
        /// </summary>
        [Parameter(HelpMessage = "Include privilege metadata in the output")]
        public SwitchParameter IncludePrivileges { get; set; }

        /// <summary>
        /// Gets or sets whether to use the shared metadata cache.
        /// </summary>
        [Parameter(HelpMessage = "Use the shared global metadata cache for improved performance")]
        public SwitchParameter UseMetadataCache { get; set; }

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
            EntityFilters filters = EntityFilters.Entity;

            if (IncludeAttributes)
            {
                filters |= EntityFilters.Attributes;
            }

            if (IncludeRelationships)
            {
                filters |= EntityFilters.Relationships;
            }

            if (IncludePrivileges)
            {
                filters |= EntityFilters.Privileges;
            }

            EntityMetadata[] entities;

            // Try cache first if enabled
            if (UseMetadataCache && MetadataCache.IsEnabled)
            {
                var connectionKey = MetadataCache.GetConnectionKey(Connection as Microsoft.PowerPlatform.Dataverse.Client.ServiceClient);
                if (MetadataCache.TryGetAllEntities(connectionKey, out var cachedEntities))
                {
                    WriteVerbose($"Retrieved {cachedEntities.Count} entities from cache");
                    entities = cachedEntities.ToArray();
                }
                else
                {
                    var request = new RetrieveAllEntitiesRequest
                    {
                        EntityFilters = filters,
                        RetrieveAsIfPublished = false
                    };

                    WriteVerbose($"Retrieving all entity metadata with filters: {filters}");

                    var response = (RetrieveAllEntitiesResponse)Connection.Execute(request);
                    entities = response.EntityMetadata;

                    WriteVerbose($"Retrieved {entities.Length} entities");

                    // Cache the results
                    MetadataCache.AddAllEntities(connectionKey, entities.ToList());
                }
            }
            else
            {
                var request = new RetrieveAllEntitiesRequest
                {
                    EntityFilters = filters,
                    RetrieveAsIfPublished = false
                };

                WriteVerbose($"Retrieving all entity metadata with filters: {filters}");

                var response = (RetrieveAllEntitiesResponse)Connection.Execute(request);
                entities = response.EntityMetadata;

                WriteVerbose($"Retrieved {entities.Length} entities");
            }

            var results = entities
                .OrderBy(e => e.LogicalName, StringComparer.OrdinalIgnoreCase)
                .Select(e => ConvertEntityMetadataToPSObject(e, IncludeAttributes, IncludeRelationships, IncludePrivileges))
                .ToArray();

            WriteObject(results, true);
        }

        private void RetrieveSingleEntity(string entityName)
        {
            EntityFilters filters = EntityFilters.Entity;

            if (IncludeAttributes)
            {
                filters |= EntityFilters.Attributes;
            }

            if (IncludeRelationships)
            {
                filters |= EntityFilters.Relationships;
            }

            if (IncludePrivileges)
            {
                filters |= EntityFilters.Privileges;
            }

            EntityMetadata entityMetadata;

            // Try cache first if enabled
            if (UseMetadataCache && MetadataCache.IsEnabled)
            {
                var connectionKey = MetadataCache.GetConnectionKey(Connection as Microsoft.PowerPlatform.Dataverse.Client.ServiceClient);
                if (MetadataCache.TryGetEntityMetadata(connectionKey, entityName, out entityMetadata))
                {
                    WriteVerbose($"Retrieved entity metadata for '{entityName}' from cache");
                }
                else
                {
                    var request = new RetrieveEntityRequest
                    {
                        LogicalName = entityName,
                        EntityFilters = filters,
                        RetrieveAsIfPublished = false
                    };

                    WriteVerbose($"Retrieving entity metadata for '{entityName}' with filters: {filters}");

                    var response = (RetrieveEntityResponse)Connection.Execute(request);
                    entityMetadata = response.EntityMetadata;

                    // Cache the result
                    MetadataCache.AddEntityMetadata(connectionKey, entityName, entityMetadata);
                }
            }
            else
            {
                var request = new RetrieveEntityRequest
                {
                    LogicalName = entityName,
                    EntityFilters = filters,
                    RetrieveAsIfPublished = false
                };

                WriteVerbose($"Retrieving entity metadata for '{entityName}' with filters: {filters}");

                var response = (RetrieveEntityResponse)Connection.Execute(request);
                entityMetadata = response.EntityMetadata;
            }

            var result = ConvertEntityMetadataToPSObject(entityMetadata, IncludeAttributes, IncludeRelationships, IncludePrivileges);

            WriteObject(result);
        }

        private PSObject ConvertEntityMetadataToPSObject(EntityMetadata metadata, bool includeAttributes, bool includeRelationships, bool includePrivileges)
        {
            var result = new PSObject();

            // Basic properties
            result.Properties.Add(new PSNoteProperty("LogicalName", metadata.LogicalName));
            result.Properties.Add(new PSNoteProperty("SchemaName", metadata.SchemaName));
            result.Properties.Add(new PSNoteProperty("EntitySetName", metadata.EntitySetName));
            result.Properties.Add(new PSNoteProperty("DisplayName", metadata.DisplayName?.UserLocalizedLabel?.Label));
            result.Properties.Add(new PSNoteProperty("DisplayCollectionName", metadata.DisplayCollectionName?.UserLocalizedLabel?.Label));
            result.Properties.Add(new PSNoteProperty("Description", metadata.Description?.UserLocalizedLabel?.Label));
            result.Properties.Add(new PSNoteProperty("PrimaryIdAttribute", metadata.PrimaryIdAttribute));
            result.Properties.Add(new PSNoteProperty("PrimaryNameAttribute", metadata.PrimaryNameAttribute));
            result.Properties.Add(new PSNoteProperty("PrimaryImageAttribute", metadata.PrimaryImageAttribute));
            result.Properties.Add(new PSNoteProperty("ObjectTypeCode", metadata.ObjectTypeCode));
            result.Properties.Add(new PSNoteProperty("MetadataId", metadata.MetadataId));
            result.Properties.Add(new PSNoteProperty("IsCustomEntity", metadata.IsCustomEntity));
            result.Properties.Add(new PSNoteProperty("IsCustomizable", metadata.IsCustomizable?.Value));
            result.Properties.Add(new PSNoteProperty("IsManaged", metadata.IsManaged));
            result.Properties.Add(new PSNoteProperty("IsActivity", metadata.IsActivity));
            result.Properties.Add(new PSNoteProperty("IsActivityParty", metadata.IsActivityParty));
            result.Properties.Add(new PSNoteProperty("IsValidForQueue", metadata.IsValidForQueue));
            result.Properties.Add(new PSNoteProperty("IsConnectionsEnabled", metadata.IsConnectionsEnabled?.Value));
            result.Properties.Add(new PSNoteProperty("IsDocumentManagementEnabled", metadata.IsDocumentManagementEnabled));
            result.Properties.Add(new PSNoteProperty("IsMailMergeEnabled", metadata.IsMailMergeEnabled?.Value));
            result.Properties.Add(new PSNoteProperty("IsAuditEnabled", metadata.IsAuditEnabled?.Value));
            result.Properties.Add(new PSNoteProperty("IsBusinessProcessEnabled", metadata.IsBusinessProcessEnabled));
            result.Properties.Add(new PSNoteProperty("OwnershipType", metadata.OwnershipType?.ToString()));
            result.Properties.Add(new PSNoteProperty("IsLogicalEntity", metadata.IsLogicalEntity));
            result.Properties.Add(new PSNoteProperty("IsIntersect", metadata.IsIntersect));

            // Include attributes if requested
            if (includeAttributes && metadata.Attributes != null)
            {
                var attributes = metadata.Attributes
                    .OrderBy(a => a.LogicalName, StringComparer.OrdinalIgnoreCase)
                    .Select(a => ConvertAttributeMetadataToPSObject(a))
                    .ToArray();
                result.Properties.Add(new PSNoteProperty("Attributes", attributes));
            }

            // Include relationships if requested
            if (includeRelationships)
            {
                if (metadata.OneToManyRelationships != null)
                {
                    var oneToMany = metadata.OneToManyRelationships
                        .OrderBy(r => r.SchemaName, StringComparer.OrdinalIgnoreCase)
                        .Select(r => ConvertOneToManyRelationshipToPSObject(r))
                        .ToArray();
                    result.Properties.Add(new PSNoteProperty("OneToManyRelationships", oneToMany));
                }

                if (metadata.ManyToOneRelationships != null)
                {
                    var manyToOne = metadata.ManyToOneRelationships
                        .OrderBy(r => r.SchemaName, StringComparer.OrdinalIgnoreCase)
                        .Select(r => ConvertManyToOneRelationshipToPSObject(r))
                        .ToArray();
                    result.Properties.Add(new PSNoteProperty("ManyToOneRelationships", manyToOne));
                }

                if (metadata.ManyToManyRelationships != null)
                {
                    var manyToMany = metadata.ManyToManyRelationships
                        .OrderBy(r => r.SchemaName, StringComparer.OrdinalIgnoreCase)
                        .Select(r => ConvertManyToManyRelationshipToPSObject(r))
                        .ToArray();
                    result.Properties.Add(new PSNoteProperty("ManyToManyRelationships", manyToMany));
                }
            }

            // Include privileges if requested
            if (includePrivileges && metadata.Privileges != null)
            {
                var privileges = metadata.Privileges
                    .Select(p => new PSObject(new
                    {
                        Name = p.Name,
                        PrivilegeId = p.PrivilegeId,
                        PrivilegeType = p.PrivilegeType.ToString(),
                        CanBeBasic = p.CanBeBasic,
                        CanBeLocal = p.CanBeLocal,
                        CanBeDeep = p.CanBeDeep,
                        CanBeGlobal = p.CanBeGlobal,
                        CanBeEntityReference = p.CanBeEntityReference,
                        CanBeParentEntityReference = p.CanBeParentEntityReference
                    }))
                    .ToArray();
                result.Properties.Add(new PSNoteProperty("Privileges", privileges));
            }

            return result;
        }

        private PSObject ConvertAttributeMetadataToPSObject(AttributeMetadata attr)
        {
            var result = new PSObject();

            result.Properties.Add(new PSNoteProperty("LogicalName", attr.LogicalName));
            result.Properties.Add(new PSNoteProperty("SchemaName", attr.SchemaName));
            result.Properties.Add(new PSNoteProperty("DisplayName", attr.DisplayName?.UserLocalizedLabel?.Label));
            result.Properties.Add(new PSNoteProperty("Description", attr.Description?.UserLocalizedLabel?.Label));
            result.Properties.Add(new PSNoteProperty("AttributeType", attr.AttributeType?.ToString()));
            result.Properties.Add(new PSNoteProperty("AttributeTypeName", attr.AttributeTypeName?.Value));
            result.Properties.Add(new PSNoteProperty("IsCustomAttribute", attr.IsCustomAttribute));
            result.Properties.Add(new PSNoteProperty("IsCustomizable", attr.IsCustomizable?.Value));
            result.Properties.Add(new PSNoteProperty("IsManaged", attr.IsManaged));
            result.Properties.Add(new PSNoteProperty("IsPrimaryId", attr.IsPrimaryId));
            result.Properties.Add(new PSNoteProperty("IsPrimaryName", attr.IsPrimaryName));
            result.Properties.Add(new PSNoteProperty("IsValidForRead", attr.IsValidForRead));
            result.Properties.Add(new PSNoteProperty("IsValidForCreate", attr.IsValidForCreate));
            result.Properties.Add(new PSNoteProperty("IsValidForUpdate", attr.IsValidForUpdate));
            result.Properties.Add(new PSNoteProperty("IsAuditEnabled", attr.IsAuditEnabled?.Value));
            result.Properties.Add(new PSNoteProperty("IsSecured", attr.IsSecured));
            result.Properties.Add(new PSNoteProperty("RequiredLevel", attr.RequiredLevel?.Value.ToString()));
            result.Properties.Add(new PSNoteProperty("MetadataId", attr.MetadataId));

            // Add type-specific properties
            AddTypeSpecificProperties(result, attr);

            return result;
        }

        private void AddTypeSpecificProperties(PSObject result, AttributeMetadata attr)
        {
            // String attributes
            if (attr is StringAttributeMetadata stringAttr)
            {
                result.Properties.Add(new PSNoteProperty("MaxLength", stringAttr.MaxLength));
                result.Properties.Add(new PSNoteProperty("Format", stringAttr.Format?.ToString()));
                result.Properties.Add(new PSNoteProperty("FormatName", stringAttr.FormatName?.Value));
            }
            // Memo attributes
            else if (attr is MemoAttributeMetadata memoAttr)
            {
                result.Properties.Add(new PSNoteProperty("MaxLength", memoAttr.MaxLength));
            }
            // Integer attributes
            else if (attr is IntegerAttributeMetadata intAttr)
            {
                result.Properties.Add(new PSNoteProperty("MinValue", intAttr.MinValue));
                result.Properties.Add(new PSNoteProperty("MaxValue", intAttr.MaxValue));
                result.Properties.Add(new PSNoteProperty("Format", intAttr.Format?.ToString()));
            }
            // BigInt attributes
            else if (attr is BigIntAttributeMetadata bigIntAttr)
            {
                // BigInt doesn't have min/max in metadata
            }
            // Decimal attributes
            else if (attr is DecimalAttributeMetadata decimalAttr)
            {
                result.Properties.Add(new PSNoteProperty("MinValue", decimalAttr.MinValue));
                result.Properties.Add(new PSNoteProperty("MaxValue", decimalAttr.MaxValue));
                result.Properties.Add(new PSNoteProperty("Precision", decimalAttr.Precision));
            }
            // Double attributes
            else if (attr is DoubleAttributeMetadata doubleAttr)
            {
                result.Properties.Add(new PSNoteProperty("MinValue", doubleAttr.MinValue));
                result.Properties.Add(new PSNoteProperty("MaxValue", doubleAttr.MaxValue));
                result.Properties.Add(new PSNoteProperty("Precision", doubleAttr.Precision));
            }
            // Money attributes
            else if (attr is MoneyAttributeMetadata moneyAttr)
            {
                result.Properties.Add(new PSNoteProperty("MinValue", moneyAttr.MinValue));
                result.Properties.Add(new PSNoteProperty("MaxValue", moneyAttr.MaxValue));
                result.Properties.Add(new PSNoteProperty("Precision", moneyAttr.Precision));
                result.Properties.Add(new PSNoteProperty("PrecisionSource", moneyAttr.PrecisionSource));
            }
            // DateTime attributes
            else if (attr is DateTimeAttributeMetadata dateTimeAttr)
            {
                result.Properties.Add(new PSNoteProperty("Format", dateTimeAttr.Format?.ToString()));
                result.Properties.Add(new PSNoteProperty("DateTimeBehavior", dateTimeAttr.DateTimeBehavior?.Value));
            }
            // Picklist (OptionSet) attributes
            else if (attr is PicklistAttributeMetadata picklistAttr)
            {
                result.Properties.Add(new PSNoteProperty("OptionSet", ConvertOptionSetToPSObject(picklistAttr.OptionSet)));
            }
            // MultiSelectPicklist attributes
            else if (attr is MultiSelectPicklistAttributeMetadata multiPicklistAttr)
            {
                result.Properties.Add(new PSNoteProperty("OptionSet", ConvertOptionSetToPSObject(multiPicklistAttr.OptionSet)));
            }
            // Boolean attributes
            else if (attr is BooleanAttributeMetadata booleanAttr)
            {
                result.Properties.Add(new PSNoteProperty("DefaultValue", booleanAttr.DefaultValue));
                if (booleanAttr.OptionSet != null)
                {
                    result.Properties.Add(new PSNoteProperty("TrueOption", booleanAttr.OptionSet.TrueOption?.Label?.UserLocalizedLabel?.Label));
                    result.Properties.Add(new PSNoteProperty("FalseOption", booleanAttr.OptionSet.FalseOption?.Label?.UserLocalizedLabel?.Label));
                }
            }
            // Lookup attributes
            else if (attr is LookupAttributeMetadata lookupAttr)
            {
                result.Properties.Add(new PSNoteProperty("Targets", lookupAttr.Targets));
            }
            // State attributes
            else if (attr is StateAttributeMetadata stateAttr)
            {
                result.Properties.Add(new PSNoteProperty("OptionSet", ConvertOptionSetToPSObject(stateAttr.OptionSet)));
            }
            // Status attributes
            else if (attr is StatusAttributeMetadata statusAttr)
            {
                result.Properties.Add(new PSNoteProperty("OptionSet", ConvertOptionSetToPSObject(statusAttr.OptionSet)));
            }
            // Image attributes
            else if (attr is ImageAttributeMetadata imageAttr)
            {
                result.Properties.Add(new PSNoteProperty("MaxHeight", imageAttr.MaxHeight));
                result.Properties.Add(new PSNoteProperty("MaxWidth", imageAttr.MaxWidth));
                result.Properties.Add(new PSNoteProperty("CanStoreFullImage", imageAttr.CanStoreFullImage));
            }
            // File attributes
            else if (attr is FileAttributeMetadata fileAttr)
            {
                result.Properties.Add(new PSNoteProperty("MaxSizeInKB", fileAttr.MaxSizeInKB));
            }
        }

        private PSObject ConvertOptionSetToPSObject(OptionSetMetadata optionSet)
        {
            if (optionSet == null) return null;

            var result = new PSObject();
            result.Properties.Add(new PSNoteProperty("Name", optionSet.Name));
            result.Properties.Add(new PSNoteProperty("DisplayName", optionSet.DisplayName?.UserLocalizedLabel?.Label));
            result.Properties.Add(new PSNoteProperty("Description", optionSet.Description?.UserLocalizedLabel?.Label));
            result.Properties.Add(new PSNoteProperty("IsGlobal", optionSet.IsGlobal));
            result.Properties.Add(new PSNoteProperty("IsCustomOptionSet", optionSet.IsCustomOptionSet));
            result.Properties.Add(new PSNoteProperty("IsManaged", optionSet.IsManaged));
            result.Properties.Add(new PSNoteProperty("MetadataId", optionSet.MetadataId));

            if (optionSet.Options != null)
            {
                var options = optionSet.Options
                    .OrderBy(o => o.Value)
                    .Select(o => new PSObject(new
                    {
                        Value = o.Value,
                        Label = o.Label?.UserLocalizedLabel?.Label,
                        Color = o.Color,
                        Description = o.Description?.UserLocalizedLabel?.Label
                    }))
                    .ToArray();
                result.Properties.Add(new PSNoteProperty("Options", options));
            }

            return result;
        }

        private PSObject ConvertOneToManyRelationshipToPSObject(OneToManyRelationshipMetadata rel)
        {
            var result = new PSObject();
            result.Properties.Add(new PSNoteProperty("SchemaName", rel.SchemaName));
            result.Properties.Add(new PSNoteProperty("ReferencedEntity", rel.ReferencedEntity));
            result.Properties.Add(new PSNoteProperty("ReferencedAttribute", rel.ReferencedAttribute));
            result.Properties.Add(new PSNoteProperty("ReferencingEntity", rel.ReferencingEntity));
            result.Properties.Add(new PSNoteProperty("ReferencingAttribute", rel.ReferencingAttribute));
            result.Properties.Add(new PSNoteProperty("IsCustomRelationship", rel.IsCustomRelationship));
            result.Properties.Add(new PSNoteProperty("IsManaged", rel.IsManaged));
            result.Properties.Add(new PSNoteProperty("IsValidForAdvancedFind", rel.IsValidForAdvancedFind));
            result.Properties.Add(new PSNoteProperty("IsHierarchical", rel.IsHierarchical));
            result.Properties.Add(new PSNoteProperty("CascadeConfiguration", rel.CascadeConfiguration));
            return result;
        }

        private PSObject ConvertManyToOneRelationshipToPSObject(OneToManyRelationshipMetadata rel)
        {
            // ManyToOne is just the reverse view of OneToMany
            return ConvertOneToManyRelationshipToPSObject(rel);
        }

        private PSObject ConvertManyToManyRelationshipToPSObject(ManyToManyRelationshipMetadata rel)
        {
            var result = new PSObject();
            result.Properties.Add(new PSNoteProperty("SchemaName", rel.SchemaName));
            result.Properties.Add(new PSNoteProperty("Entity1LogicalName", rel.Entity1LogicalName));
            result.Properties.Add(new PSNoteProperty("Entity1IntersectAttribute", rel.Entity1IntersectAttribute));
            result.Properties.Add(new PSNoteProperty("Entity2LogicalName", rel.Entity2LogicalName));
            result.Properties.Add(new PSNoteProperty("Entity2IntersectAttribute", rel.Entity2IntersectAttribute));
            result.Properties.Add(new PSNoteProperty("IntersectEntityName", rel.IntersectEntityName));
            result.Properties.Add(new PSNoteProperty("IsCustomRelationship", rel.IsCustomRelationship));
            result.Properties.Add(new PSNoteProperty("IsManaged", rel.IsManaged));
            result.Properties.Add(new PSNoteProperty("IsValidForAdvancedFind", rel.IsValidForAdvancedFind));
            return result;
        }
    }
}
