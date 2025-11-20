using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Management.Automation;
using System.Xml.Linq;
using Microsoft.Crm.Sdk.Messages;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Query;

namespace Rnwood.Dataverse.Data.PowerShell.Commands
{
    /// <summary>
    /// Retrieves sitemap entries (Areas, Groups, SubAreas) from a Dataverse sitemap.
    /// </summary>
    [Cmdlet(VerbsCommon.Get, "DataverseSitemapEntry", DefaultParameterSetName = "Default")]
    [OutputType(typeof(SitemapEntryInfo))]
    public class GetDataverseSitemapEntryCmdlet : OrganizationServiceCmdlet
    {
        /// <summary>
        /// Gets or sets the sitemap object from pipeline.
        /// </summary>
        [Parameter(ValueFromPipeline = true, HelpMessage = "Sitemap object from Get-DataverseSitemap.")]
        public SitemapInfo Sitemap { get; set; }

        /// <summary>
        /// Gets or sets the unique name of the sitemap to retrieve entries from.
        /// </summary>
        [Parameter(Position = 0, ValueFromPipelineByPropertyName = true, HelpMessage = "The unique name of the sitemap to retrieve entries from.")]
        [Alias("Name")]
        public string SitemapUniqueName { get; set; }

        /// <summary>
        /// Gets or sets the ID of the sitemap to retrieve entries from.
        /// </summary>
        [Parameter(ValueFromPipelineByPropertyName = true, HelpMessage = "The ID of the sitemap to retrieve entries from.")]
        public Guid? SitemapId { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether to retrieve Area entries.
        /// </summary>
        [Parameter(ParameterSetName = "Area", HelpMessage = "Retrieve Area entries.")]
        public SwitchParameter Area { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether to retrieve Group entries.
        /// </summary>
        [Parameter(ParameterSetName = "Group", HelpMessage = "Retrieve Group entries.")]
        public SwitchParameter Group { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether to retrieve SubArea entries.
        /// </summary>
        [Parameter(ParameterSetName = "SubArea", HelpMessage = "Retrieve SubArea entries.")]
        public SwitchParameter SubArea { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether to retrieve Privilege entries.
        /// </summary>
        [Parameter(ParameterSetName = "Privilege", HelpMessage = "Retrieve Privilege entries.")]
        public SwitchParameter Privilege { get; set; }

        /// <summary>
        /// Gets or sets the ID of a specific entry to retrieve.
        /// </summary>
        [Parameter(ParameterSetName = "Default", HelpMessage = "The ID of a specific entry to retrieve.")]
        [Parameter(ParameterSetName = "Area", HelpMessage = "The ID of a specific entry to retrieve.")]
        [Parameter(ParameterSetName = "Group", HelpMessage = "The ID of a specific entry to retrieve.")]
        [Parameter(ParameterSetName = "SubArea", HelpMessage = "The ID of a specific entry to retrieve.")]
        [Parameter(ParameterSetName = "Privilege", HelpMessage = "The ID of a specific entry to retrieve.")]
        public string EntryId { get; set; }

        /// <summary>
        /// Gets or sets the parent Area ID to filter by (optional for Groups and SubAreas).
        /// </summary>
        [Parameter(ParameterSetName = "Group", HelpMessage = "Filter entries by parent Area ID (optional for Groups).")]
        [Parameter(ParameterSetName = "SubArea", HelpMessage = "Filter entries by parent Area ID (optional for SubAreas).")]
        public string ParentAreaId { get; set; }

        /// <summary>
        /// Gets or sets the parent Group ID to filter by (optional for SubAreas).
        /// </summary>
        [Parameter(ParameterSetName = "SubArea", HelpMessage = "Filter SubAreas by parent Group ID (optional for SubAreas).")]
        public string ParentGroupId { get; set; }

        /// <summary>
        /// Gets or sets the parent SubArea ID to filter by (required for Privileges).
        /// </summary>
        [Parameter(ParameterSetName = "Privilege", Mandatory = true, HelpMessage = "Filter Privileges by parent SubArea ID (required for Privileges).")]
        public string ParentSubAreaId { get; set; }

        /// <summary>
        /// Processes the cmdlet request.
        /// </summary>
        protected override void ProcessRecord()
        {
            base.ProcessRecord();

            // Get sitemap name and ID from Sitemap object if provided
            string sitemapUniqueName = SitemapUniqueName;
            Guid? sitemapId = SitemapId;

            if (Sitemap != null)
            {
                if (string.IsNullOrEmpty(sitemapUniqueName))
                    sitemapUniqueName = Sitemap.UniqueName;
                if (!sitemapId.HasValue || sitemapId.Value == Guid.Empty)
                    sitemapId = Sitemap.Id;
            }

            if (string.IsNullOrEmpty(sitemapUniqueName) && (!sitemapId.HasValue || sitemapId.Value == Guid.Empty))
            {
                ThrowTerminatingError(new ErrorRecord(
                    new ArgumentException("Either SitemapUniqueName, SitemapId, or a Sitemap object must be provided."),
                    "MissingSitemapIdentifier",
                    ErrorCategory.InvalidArgument,
                    null));
                return;
            }

            // Determine entry type from parameter set
            SitemapEntryType? entryType = null;
            if (Area.IsPresent)
            {
                entryType = SitemapEntryType.Area;
            }
            else if (Group.IsPresent)
            {
                entryType = SitemapEntryType.Group;
            }
            else if (SubArea.IsPresent)
            {
                entryType = SitemapEntryType.SubArea;
            }
            else if (Privilege.IsPresent)
            {
                entryType = SitemapEntryType.Privilege;
            }

            // Get local copies of parameters for use in helper methods
            string parentAreaId = ParentAreaId;
            string parentGroupId = ParentGroupId;
            string parentSubAreaId = ParentSubAreaId;
            string entryId = EntryId;

            // Retrieve the sitemap, preferring unpublished
            var query = new QueryExpression("sitemap")
            {
                ColumnSet = new ColumnSet("sitemapid", "sitemapname", "sitemapnameunique", "sitemapxml"),
                TopCount = 1
            };

            if (sitemapId.HasValue && sitemapId.Value != Guid.Empty)
            {
                query.Criteria.AddCondition("sitemapid", ConditionOperator.Equal, sitemapId.Value);
            }
            else
            {
                query.Criteria.AddCondition("sitemapnameunique", ConditionOperator.Equal, sitemapUniqueName);
            }

            Entity sitemap = null;

            // First try unpublished
            try
            {
                var request = new Microsoft.Crm.Sdk.Messages.RetrieveUnpublishedMultipleRequest { Query = query };
                var response = (Microsoft.Crm.Sdk.Messages.RetrieveUnpublishedMultipleResponse)Connection.Execute(request);
                if (response.EntityCollection.Entities.Count > 0)
                {
                    sitemap = response.EntityCollection.Entities[0];
                    WriteVerbose("Found sitemap in unpublished records");
                }
            }
            catch (Exception ex)
            {
                WriteVerbose($"Failed to retrieve unpublished sitemap: {ex.Message}");
            }

            // If not found in unpublished, try published
            if (sitemap == null)
            {
                var sitemaps = Connection.RetrieveMultiple(query);
                if (sitemaps.Entities.Count > 0)
                {
                    sitemap = sitemaps.Entities[0];
                    WriteVerbose("Found sitemap in published records");
                }
            }

            if (sitemap == null)
            {
                ThrowTerminatingError(new ErrorRecord(
                    new InvalidOperationException($"Sitemap '{sitemapUniqueName ?? sitemapId.ToString()}' not found."),
                    "SitemapNotFound",
                    ErrorCategory.ObjectNotFound,
                    sitemapUniqueName ?? sitemapId.ToString()));
                return;
            }
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
                    if (!entryType.HasValue || entryType.Value == SitemapEntryType.Area)
                    {
                        if (string.IsNullOrEmpty(entryId) || areaId == entryId)
                        {
                            var entry = new SitemapEntryInfo
                            {
                                EntryType = SitemapEntryType.Area,
                                Id = areaId,
                                ResourceId = area.Attribute("ResourceId")?.Value,
                                Titles = ParseTitles(area),
                                Descriptions = ParseDescriptions(area),
                                DescriptionResourceId = area.Attribute("DescriptionResourceId")?.Value,
                                ToolTipResourceId = area.Attribute("ToolTipResourceId")?.Value ?? area.Attribute("ToolTipResourseId")?.Value,
                                Icon = area.Attribute("Icon")?.Value,
                                ShowInAppNavigation = ParseBool(area.Attribute("ShowInAppNavigation")?.Value)
                            };
                            entries.Add(entry);
                        }
                    }

                    // Process Groups within this Area
                    if ((!entryType.HasValue || entryType.Value == SitemapEntryType.Group || entryType.Value == SitemapEntryType.SubArea || entryType.Value == SitemapEntryType.Privilege) &&
                        (string.IsNullOrEmpty(parentAreaId) || parentAreaId == areaId))
                    {
                        ProcessGroups(area, areaId, entries, entryType, entryId, parentGroupId, parentSubAreaId);
                    }
                }
            }

            WriteVerbose($"Found {entries.Count} sitemap entry(ies)");

            foreach (var entry in entries)
            {
                WriteObject(CreatePSObject(entry));
            }
        }

        private void ProcessGroups(XElement areaElement, string areaId, List<SitemapEntryInfo> entries, SitemapEntryType? entryType, string entryId, string parentGroupId, string parentSubAreaId)
        {
            var groups = areaElement.Elements("Group");
            foreach (var group in groups)
            {
                var groupId = group.Attribute("Id")?.Value;

                // Add Group if it matches the filter
                if (!entryType.HasValue || entryType.Value == SitemapEntryType.Group)
                {
                    if (string.IsNullOrEmpty(entryId) || groupId == entryId)
                    {
                        var entry = new SitemapEntryInfo
                        {
                            EntryType = SitemapEntryType.Group,
                            Id = groupId,
                            ParentAreaId = areaId,
                            ResourceId = group.Attribute("ResourceId")?.Value,
                            Titles = ParseTitles(group),
                            Descriptions = ParseDescriptions(group),
                            DescriptionResourceId = group.Attribute("DescriptionResourceId")?.Value,
                            ToolTipResourceId = group.Attribute("ToolTipResourceId")?.Value ?? group.Attribute("ToolTipResourseId")?.Value,
                            IsDefault = ParseBool(group.Attribute("IsProfile")?.Value)
                        };
                        entries.Add(entry);
                    }
                }

                // Process SubAreas within this Group
                if ((!entryType.HasValue || entryType.Value == SitemapEntryType.SubArea || entryType.Value == SitemapEntryType.Privilege) &&
                    (string.IsNullOrEmpty(parentGroupId) || parentGroupId == groupId))
                {
                    ProcessSubAreas(group, areaId, groupId, entries, entryType, entryId, parentSubAreaId);
                }
            }
        }

        private void ProcessSubAreas(XElement groupElement, string areaId, string groupId, List<SitemapEntryInfo> entries, SitemapEntryType? entryType, string entryId, string parentSubAreaId)
        {
            var subAreas = groupElement.Elements("SubArea");
            foreach (var subArea in subAreas)
            {
                var subAreaId = subArea.Attribute("Id")?.Value;

                // Add SubArea if it matches the filter
                if ((!entryType.HasValue || entryType.Value == SitemapEntryType.SubArea) &&
                    (string.IsNullOrEmpty(entryId) || subAreaId == entryId))
                {
                    var entry = new SitemapEntryInfo
                    {
                        EntryType = SitemapEntryType.SubArea,
                        Id = subAreaId,
                        ParentAreaId = areaId,
                        ParentGroupId = groupId,
                        ResourceId = subArea.Attribute("ResourceId")?.Value,
                        Titles = ParseTitles(subArea),
                        Descriptions = ParseDescriptions(subArea),
                        DescriptionResourceId = subArea.Attribute("DescriptionResourceId")?.Value,
                        ToolTipResourceId = subArea.Attribute("ToolTipResourceId")?.Value ?? subArea.Attribute("ToolTipResourseId")?.Value,
                        Icon = subArea.Attribute("Icon")?.Value,
                        Entity = subArea.Attribute("Entity")?.Value,
                        Url = subArea.Attribute("Url")?.Value,
                        IsDefault = ParseBool(subArea.Attribute("IsDefault")?.Value)
                    };
                    
                    // Populate privileges collection for SubArea
                    entry.Privileges = new List<PrivilegeInfo>();
                    PopulatePrivileges(subArea, entry);
                    
                    entries.Add(entry);
                }

                // Process Privileges within this SubArea
                if ((entryType.HasValue && entryType.Value == SitemapEntryType.Privilege) &&
                    (string.IsNullOrEmpty(parentSubAreaId) || parentSubAreaId == subAreaId))
                {
                    ProcessPrivileges(subArea, subAreaId, entries, entryId);
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

        private Hashtable ParseTitles(XElement element)
        {
            var titles = new Hashtable();
            
            // First check for new format: <Titles><Title LCID="..." Title="..." /></Titles>
            var titlesElement = element.Element("Titles");
            if (titlesElement != null)
            {
                foreach (var titleElement in titlesElement.Elements("Title"))
                {
                    var lcidAttr = titleElement.Attribute("LCID")?.Value;
                    var titleAttr = titleElement.Attribute("Title")?.Value;
                    
                    if (int.TryParse(lcidAttr, out int lcid) && !string.IsNullOrEmpty(titleAttr))
                    {
                        titles[lcid] = titleAttr;
                    }
                }
            }
            
            // Fallback to old format: Title attribute
            var oldTitle = element.Attribute("Title")?.Value;
            if (!string.IsNullOrEmpty(oldTitle) && titles.Count == 0)
            {
                titles[1033] = oldTitle; // Default to English LCID
            }
            
            return titles.Count > 0 ? titles : null;
        }

        private Hashtable ParseDescriptions(XElement element)
        {
            var descriptions = new Hashtable();
            
            // First check for new format: <Descriptions><Description LCID="..." Description="..." /></Descriptions>
            var descriptionsElement = element.Element("Descriptions");
            if (descriptionsElement != null)
            {
                foreach (var descElement in descriptionsElement.Elements("Description"))
                {
                    var lcidAttr = descElement.Attribute("LCID")?.Value;
                    var descAttr = descElement.Attribute("Description")?.Value;
                    
                    if (int.TryParse(lcidAttr, out int lcid) && !string.IsNullOrEmpty(descAttr))
                    {
                        descriptions[lcid] = descAttr;
                    }
                }
            }
            
            // Fallback to old format: Description attribute
            var oldDescription = element.Attribute("Description")?.Value;
            if (!string.IsNullOrEmpty(oldDescription) && descriptions.Count == 0)
            {
                descriptions[1033] = oldDescription; // Default to English LCID
            }
            
            return descriptions.Count > 0 ? descriptions : null;
        }

        private void PopulatePrivileges(XElement subAreaElement, SitemapEntryInfo subAreaEntry)
        {
            var privileges = subAreaElement.Elements("Privilege");
            foreach (var privilege in privileges)
            {
                var entity = privilege.Attribute("Entity")?.Value;
                var name = privilege.Attribute("Privilege")?.Value;

                if (!string.IsNullOrEmpty(entity) && !string.IsNullOrEmpty(name))
                {
                    subAreaEntry.Privileges.Add(new PrivilegeInfo
                    {
                        Entity = entity,
                        Privilege = name
                    });
                }
            }
        }

        private void ProcessPrivileges(XElement subAreaElement, string subAreaId, List<SitemapEntryInfo> entries, string entryId)
        {
            var privileges = subAreaElement.Elements("Privilege");
            foreach (var privilege in privileges)
            {
                var entity = privilege.Attribute("Entity")?.Value;
                var name = privilege.Attribute("Privilege")?.Value;
                var privilegeId = $"{entity}_{name}";

                // Add Privilege if it matches the filter
                if (string.IsNullOrEmpty(entryId) || privilegeId == entryId)
                {
                    var entry = new SitemapEntryInfo
                    {
                        EntryType = SitemapEntryType.Privilege,
                        Id = privilegeId,
                        ParentSubAreaId = subAreaId,
                        PrivilegeEntity = entity,
                        PrivilegeName = name,
                    };
                    entries.Add(entry);
                }
            }
        }

        private PSObject CreatePSObject(SitemapEntryInfo entry)
        {
            PSObject psObject = new PSObject();
            psObject.Properties.Add(new PSNoteProperty("EntryType", entry.EntryType));
            psObject.Properties.Add(new PSNoteProperty("Id", entry.Id));

            if (!string.IsNullOrEmpty(entry.ResourceId)) psObject.Properties.Add(new PSNoteProperty("ResourceId", entry.ResourceId));
            
            // Add Titles and Descriptions as dictionaries
            if (entry.Titles != null && entry.Titles.Count > 0)
                psObject.Properties.Add(new PSNoteProperty("Titles", entry.Titles));
            if (entry.Descriptions != null && entry.Descriptions.Count > 0)
                psObject.Properties.Add(new PSNoteProperty("Descriptions", entry.Descriptions));
            
            if (!string.IsNullOrEmpty(entry.DescriptionResourceId)) psObject.Properties.Add(new PSNoteProperty("DescriptionResourceId", entry.DescriptionResourceId));
            if (!string.IsNullOrEmpty(entry.ToolTipResourceId)) psObject.Properties.Add(new PSNoteProperty("ToolTipResourceId", entry.ToolTipResourceId));
            if (!string.IsNullOrEmpty(entry.Icon)) psObject.Properties.Add(new PSNoteProperty("Icon", entry.Icon));
            if (!string.IsNullOrEmpty(entry.Entity)) psObject.Properties.Add(new PSNoteProperty("Entity", entry.Entity));
            if (!string.IsNullOrEmpty(entry.Url)) psObject.Properties.Add(new PSNoteProperty("Url", entry.Url));
            if (entry.IsDefault.HasValue) psObject.Properties.Add(new PSNoteProperty("IsDefault", entry.IsDefault.Value));
            if (!string.IsNullOrEmpty(entry.ParentAreaId)) psObject.Properties.Add(new PSNoteProperty("ParentAreaId", entry.ParentAreaId));
            if (!string.IsNullOrEmpty(entry.ParentGroupId)) psObject.Properties.Add(new PSNoteProperty("ParentGroupId", entry.ParentGroupId));
            if (entry.ShowInAppNavigation.HasValue) psObject.Properties.Add(new PSNoteProperty("ShowInAppNavigation", entry.ShowInAppNavigation.Value));
            if (entry.Privileges != null) psObject.Properties.Add(new PSNoteProperty("Privileges", entry.Privileges));
            if (!string.IsNullOrEmpty(entry.PrivilegeEntity)) psObject.Properties.Add(new PSNoteProperty("PrivilegeEntity", entry.PrivilegeEntity));
            if (!string.IsNullOrEmpty(entry.PrivilegeName)) psObject.Properties.Add(new PSNoteProperty("PrivilegeName", entry.PrivilegeName));
            if (!string.IsNullOrEmpty(entry.ParentSubAreaId)) psObject.Properties.Add(new PSNoteProperty("ParentSubAreaId", entry.ParentSubAreaId));

            return psObject;
        }
    }
}
