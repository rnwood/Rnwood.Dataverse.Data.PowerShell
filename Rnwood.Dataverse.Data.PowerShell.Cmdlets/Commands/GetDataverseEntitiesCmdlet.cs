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
    /// Retrieves a list of all entities (tables) in Dataverse.
    /// </summary>
    [Cmdlet(VerbsCommon.Get, "DataverseEntities")]
    [OutputType(typeof(PSObject[]))]
    public class GetDataverseEntitiesCmdlet : OrganizationServiceCmdlet
    {
        /// <summary>
        /// Gets or sets whether to return only custom entities.
        /// </summary>
        [Parameter(HelpMessage = "Return only custom entities")]
        public SwitchParameter OnlyCustom { get; set; }

        /// <summary>
        /// Gets or sets whether to return only managed entities.
        /// </summary>
        [Parameter(HelpMessage = "Return only managed entities")]
        public SwitchParameter OnlyManaged { get; set; }

        /// <summary>
        /// Gets or sets whether to return only unmanaged entities.
        /// </summary>
        [Parameter(HelpMessage = "Return only unmanaged entities")]
        public SwitchParameter OnlyUnmanaged { get; set; }

        /// <summary>
        /// Gets or sets whether to return only customizable entities.
        /// </summary>
        [Parameter(HelpMessage = "Return only customizable entities")]
        public SwitchParameter OnlyCustomizable { get; set; }

        /// <summary>
        /// Gets or sets whether to include detailed entity information.
        /// </summary>
        [Parameter(HelpMessage = "Include detailed entity information (DisplayName, Description, etc.)")]
        public SwitchParameter IncludeDetails { get; set; }

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

            EntityFilters filters = IncludeDetails ? EntityFilters.Entity : EntityFilters.Entity;

            EntityMetadata[] entities;

            // Try cache first if enabled
            if (UseMetadataCache)
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

                    WriteVerbose($"Retrieving all entities from Dataverse");

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

                WriteVerbose($"Retrieving all entities from Dataverse");

                var response = (RetrieveAllEntitiesResponse)Connection.Execute(request);
                entities = response.EntityMetadata;

                WriteVerbose($"Retrieved {entities.Length} entities");
            }

            // Apply filters
            IEnumerable<EntityMetadata> filteredEntities = entities;

            if (OnlyCustom)
            {
                filteredEntities = filteredEntities.Where(e => e.IsCustomEntity == true);
                WriteVerbose($"Filtered to {filteredEntities.Count()} custom entities");
            }

            if (OnlyManaged)
            {
                filteredEntities = filteredEntities.Where(e => e.IsManaged == true);
                WriteVerbose($"Filtered to {filteredEntities.Count()} managed entities");
            }

            if (OnlyUnmanaged)
            {
                filteredEntities = filteredEntities.Where(e => e.IsManaged == false);
                WriteVerbose($"Filtered to {filteredEntities.Count()} unmanaged entities");
            }

            if (OnlyCustomizable)
            {
                filteredEntities = filteredEntities.Where(e => e.IsCustomizable?.Value == true);
                WriteVerbose($"Filtered to {filteredEntities.Count()} customizable entities");
            }

            // Convert to PSObjects
            var results = filteredEntities
                .OrderBy(e => e.LogicalName, StringComparer.OrdinalIgnoreCase)
                .Select(e => ConvertEntityMetadataToPSObject(e, IncludeDetails))
                .ToArray();

            WriteObject(results, true);
        }

        private PSObject ConvertEntityMetadataToPSObject(EntityMetadata metadata, bool includeDetails)
        {
            var result = new PSObject();

            result.Properties.Add(new PSNoteProperty("LogicalName", metadata.LogicalName));
            result.Properties.Add(new PSNoteProperty("SchemaName", metadata.SchemaName));
            result.Properties.Add(new PSNoteProperty("ObjectTypeCode", metadata.ObjectTypeCode));
            result.Properties.Add(new PSNoteProperty("IsCustomEntity", metadata.IsCustomEntity));
            result.Properties.Add(new PSNoteProperty("IsManaged", metadata.IsManaged));

            if (includeDetails)
            {
                result.Properties.Add(new PSNoteProperty("EntitySetName", metadata.EntitySetName));
                result.Properties.Add(new PSNoteProperty("DisplayName", metadata.DisplayName?.UserLocalizedLabel?.Label));
                result.Properties.Add(new PSNoteProperty("DisplayCollectionName", metadata.DisplayCollectionName?.UserLocalizedLabel?.Label));
                result.Properties.Add(new PSNoteProperty("Description", metadata.Description?.UserLocalizedLabel?.Label));
                result.Properties.Add(new PSNoteProperty("PrimaryIdAttribute", metadata.PrimaryIdAttribute));
                result.Properties.Add(new PSNoteProperty("PrimaryNameAttribute", metadata.PrimaryNameAttribute));
                result.Properties.Add(new PSNoteProperty("PrimaryImageAttribute", metadata.PrimaryImageAttribute));
                result.Properties.Add(new PSNoteProperty("IsCustomizable", metadata.IsCustomizable?.Value));
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
                result.Properties.Add(new PSNoteProperty("MetadataId", metadata.MetadataId));
            }

            return result;
        }
    }
}
