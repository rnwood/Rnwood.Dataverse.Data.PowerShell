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
    [OutputType(typeof(EntityMetadata[]))]
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
                if (MetadataCache.TryGetAllEntities(connectionKey, filters, out var cachedEntities))
                {
                    WriteVerbose($"Retrieved {cachedEntities.Count} entities from cache (filters: {filters})");
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
                    MetadataCache.AddAllEntities(connectionKey, filters, entities.ToList());
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

            // Convert to array and sort
            var results = filteredEntities
                .OrderBy(e => e.LogicalName, StringComparer.OrdinalIgnoreCase)
                .ToArray();

            WriteObject(results, true);
        }
    }
}
