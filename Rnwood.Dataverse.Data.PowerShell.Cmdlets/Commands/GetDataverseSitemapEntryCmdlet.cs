using System;
using System.Collections.Generic;
using System.Linq;
using System.Management.Automation;
using System.Xml.Linq;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Query;

namespace Rnwood.Dataverse.Data.PowerShell.Commands
{
    /// <summary>
    /// Retrieves sitemap entries (Areas, Groups, SubAreas) from a Dataverse sitemap.
    /// </summary>
    [Cmdlet(VerbsCommon.Get, "DataverseSitemapEntry")]
    [OutputType(typeof(SitemapEntryInfo))]
    public class GetDataverseSitemapEntryCmdlet : OrganizationServiceCmdlet
    {
        /// <summary>
        /// Gets or sets the sitemap object from pipeline.
        /// </summary>
        [Parameter(ValueFromPipeline = true, HelpMessage = "Sitemap object from Get-DataverseSitemap.")]
        public SitemapInfo Sitemap { get; set; }

        /// <summary>
        /// Gets or sets the name of the sitemap to retrieve entries from.
        /// </summary>
        [Parameter(Position = 0, ValueFromPipelineByPropertyName = true, HelpMessage = "The name of the sitemap to retrieve entries from.")]
        [Alias("Name")]
        public string SitemapName { get; set; }

        /// <summary>
        /// Gets or sets the ID of the sitemap to retrieve entries from.
        /// </summary>
        [Parameter(ValueFromPipelineByPropertyName = true, HelpMessage = "The ID of the sitemap to retrieve entries from.")]
        public Guid? SitemapId { get; set; }

        /// <summary>
        /// Gets or sets the type of entries to retrieve.
        /// </summary>
        [Parameter(HelpMessage = "The type of entries to retrieve (Area, Group, SubArea). If not specified, all types are returned.")]
        public SitemapEntryType? EntryType { get; set; }

        /// <summary>
        /// Gets or sets the ID of a specific entry to retrieve.
        /// </summary>
        [Parameter(HelpMessage = "The ID of a specific entry to retrieve.")]
        public string EntryId { get; set; }

        /// <summary>
        /// Gets or sets the parent Area ID to filter by (for Groups and SubAreas).
        /// </summary>
        [Parameter(HelpMessage = "Filter entries by parent Area ID (for Groups and SubAreas).")]
        public string ParentAreaId { get; set; }

        /// <summary>
        /// Gets or sets the parent Group ID to filter by (for SubAreas).
        /// </summary>
        [Parameter(HelpMessage = "Filter SubAreas by parent Group ID.")]
        public string ParentGroupId { get; set; }

        /// <summary>
        /// Processes the cmdlet request.
        /// </summary>
        protected override void ProcessRecord()
        {
            base.ProcessRecord();

            // Get sitemap name and ID from Sitemap object if provided
            string sitemapName = SitemapName;
            Guid? sitemapId = SitemapId;

            if (Sitemap != null)
            {
                if (string.IsNullOrEmpty(sitemapName))
                    sitemapName = Sitemap.Name;
                if (!sitemapId.HasValue || sitemapId.Value == Guid.Empty)
                    sitemapId = Sitemap.Id;
            }

            if (string.IsNullOrEmpty(sitemapName) && (!sitemapId.HasValue || sitemapId.Value == Guid.Empty))
            {
                ThrowTerminatingError(new ErrorRecord(
                    new ArgumentException("Either SitemapName, SitemapId, or a Sitemap object must be provided."),
                    "MissingSitemapIdentifier",
                    ErrorCategory.InvalidArgument,
                    null));
                return;
            }

            WriteVerbose($"Retrieving sitemap entries from sitemap '{sitemapName ?? sitemapId.ToString()}'...");

            // Retrieve the sitemap
            var query = new QueryExpression("sitemap")
            {
                ColumnSet = new ColumnSet("sitemapid", "sitemapname", "sitemapxml"),
                TopCount = 1
            };

            if (sitemapId.HasValue && sitemapId.Value != Guid.Empty)
            {
                query.Criteria.AddCondition("sitemapid", ConditionOperator.Equal, sitemapId.Value);
            }
            else
            {
                query.Criteria.AddCondition("sitemapname", ConditionOperator.Equal, sitemapName);
            }

            var sitemaps = Connection.RetrieveMultiple(query);

            if (sitemaps.Entities.Count == 0)
            {
                ThrowTerminatingError(new ErrorRecord(
                    new InvalidOperationException($"Sitemap '{sitemapName ?? sitemapId.ToString()}' not found."),
                    "SitemapNotFound",
                    ErrorCategory.ObjectNotFound,
                    sitemapName ?? sitemapId.ToString()));
                return;
            }

            var sitemap = sitemaps.Entities[0];
            var sitemapXml = sitemap.GetAttributeValue<string>("sitemapxml");

            if (string.IsNullOrEmpty(sitemapXml))
            {
                WriteVerbose("Sitemap has no XML content.");
                return;
            }

            // Parse the XML
            XDocument doc;
            try
            {
                doc = XDocument.Parse(sitemapXml);
            }
            catch (Exception ex)
            {
                ThrowTerminatingError(new ErrorRecord(
                    new InvalidOperationException($"Failed to parse sitemap XML: {ex.Message}"),
                    "InvalidSitemapXml",
                    ErrorCategory.InvalidData,
                    sitemapXml));
                return;
            }

            var entries = new List<SitemapEntryInfo>();

            // Process all entries in hierarchical order: Areas, then Groups, then SubAreas
            var areas = doc.Root?.Elements("Area");
            if (areas != null)
            {
                foreach (var area in areas)
                {
                    var areaId = area.Attribute("Id")?.Value;
                    
                    // Add Area if it matches the filter
                    if (!EntryType.HasValue || EntryType.Value == SitemapEntryType.Area)
                    {
                        if (string.IsNullOrEmpty(EntryId) || areaId == EntryId)
                        {
                            var entry = new SitemapEntryInfo
                            {
                                EntryType = SitemapEntryType.Area,
                                Id = areaId,
                                ResourceId = area.Attribute("ResourceId")?.Value,
                                Title = area.Attribute("Title")?.Value,
                                Description = area.Attribute("Description")?.Value,
                                Icon = area.Attribute("Icon")?.Value,
                                ShowInAppNavigation = ParseBool(area.Attribute("ShowInAppNavigation")?.Value)
                            };
                            entries.Add(entry);
                        }
                    }

                    // Process Groups within this Area
                    if ((!EntryType.HasValue || EntryType.Value == SitemapEntryType.Group || EntryType.Value == SitemapEntryType.SubArea) &&
                        (string.IsNullOrEmpty(ParentAreaId) || ParentAreaId == areaId))
                    {
                        ProcessGroups(area, areaId, entries);
                    }
                }
            }

            WriteVerbose($"Found {entries.Count} sitemap entry(ies)");

            foreach (var entry in entries)
            {
                WriteObject(entry);
            }
        }

        private void ProcessGroups(XElement areaElement, string areaId, List<SitemapEntryInfo> entries)
        {
            var groups = areaElement.Elements("Group");
            foreach (var group in groups)
            {
                var groupId = group.Attribute("Id")?.Value;

                // Add Group if it matches the filter
                if (!EntryType.HasValue || EntryType.Value == SitemapEntryType.Group)
                {
                    if (string.IsNullOrEmpty(EntryId) || groupId == EntryId)
                    {
                        var entry = new SitemapEntryInfo
                        {
                            EntryType = SitemapEntryType.Group,
                            Id = groupId,
                            ParentAreaId = areaId,
                            ResourceId = group.Attribute("ResourceId")?.Value,
                            Title = group.Attribute("Title")?.Value,
                            Description = group.Attribute("Description")?.Value,
                            IsDefault = ParseBool(group.Attribute("IsProfile")?.Value)
                        };
                        entries.Add(entry);
                    }
                }

                // Process SubAreas within this Group
                if ((!EntryType.HasValue || EntryType.Value == SitemapEntryType.SubArea) &&
                    (string.IsNullOrEmpty(ParentGroupId) || ParentGroupId == groupId))
                {
                    ProcessSubAreas(group, areaId, groupId, entries);
                }
            }
        }

        private void ProcessSubAreas(XElement groupElement, string areaId, string groupId, List<SitemapEntryInfo> entries)
        {
            var subAreas = groupElement.Elements("SubArea");
            foreach (var subArea in subAreas)
            {
                var subAreaId = subArea.Attribute("Id")?.Value;

                // Add SubArea if it matches the filter
                if (string.IsNullOrEmpty(EntryId) || subAreaId == EntryId)
                {
                    var entry = new SitemapEntryInfo
                    {
                        EntryType = SitemapEntryType.SubArea,
                        Id = subAreaId,
                        ParentAreaId = areaId,
                        ParentGroupId = groupId,
                        ResourceId = subArea.Attribute("ResourceId")?.Value,
                        Title = subArea.Attribute("Title")?.Value,
                        Description = subArea.Attribute("Description")?.Value,
                        Icon = subArea.Attribute("Icon")?.Value,
                        Entity = subArea.Attribute("Entity")?.Value,
                        Url = subArea.Attribute("Url")?.Value,
                        IsDefault = ParseBool(subArea.Attribute("IsDefault")?.Value),
                        Privilege = subArea.Attribute("Privilege")?.Value
                    };
                    entries.Add(entry);
                }
            }
        }

        private bool? ParseBool(string value)
        {
            if (string.IsNullOrEmpty(value))
                return null;

            if (bool.TryParse(value, out bool result))
                return result;

            return null;
        }
    }
}
