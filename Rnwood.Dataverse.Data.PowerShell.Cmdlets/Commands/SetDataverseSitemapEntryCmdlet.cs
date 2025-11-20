using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Management.Automation;
using System.Xml.Linq;
using Microsoft.Crm.Sdk.Messages;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Messages;
using Microsoft.Xrm.Sdk.Metadata;
using Microsoft.Xrm.Sdk.Query;

namespace Rnwood.Dataverse.Data.PowerShell.Commands
{
    /// <summary>
    /// Creates or updates an entry (Area, Group, or SubArea) in a Dataverse sitemap.
    /// </summary>
    [Cmdlet(VerbsCommon.Set, "DataverseSitemapEntry", SupportsShouldProcess = true, ConfirmImpact = ConfirmImpact.Medium)]
    [OutputType(typeof(SitemapEntryInfo))]
    public class SetDataverseSitemapEntryCmdlet : OrganizationServiceCmdlet
    {
        /// <summary>
        /// Gets or sets the entry object from pipeline.
        /// </summary>
        [Parameter(ValueFromPipeline = true, HelpMessage = "Entry object from Get-DataverseSitemapEntry.")]
        public SitemapEntryInfo InputObject { get; set; }

        /// <summary>
        /// Gets or sets the sitemap object from pipeline.
        /// </summary>
        [Parameter(ValueFromPipelineByPropertyName = true, HelpMessage = "Sitemap object from Get-DataverseSitemap.")]
        public SitemapInfo Sitemap { get; set; }

        /// <summary>
        /// Gets or sets the unique name of the sitemap containing the entry.
        /// </summary>
        [Parameter(Position = 0, ValueFromPipelineByPropertyName = true, HelpMessage = "The unique name of the sitemap containing the entry.")]
        [Alias("Name")]
        public string SitemapUniqueName { get; set; }

        /// <summary>
        /// Gets or sets the ID of the sitemap containing the entry.
        /// </summary>
        [Parameter(ValueFromPipelineByPropertyName = true, HelpMessage = "The ID of the sitemap containing the entry.")]
        public Guid? SitemapId { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether to create or update an Area entry.
        /// </summary>
        [Parameter(ParameterSetName = "Area", Mandatory = true, HelpMessage = "Create or update an Area entry.")]
        public SwitchParameter Area { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether to create or update a Group entry.
        /// </summary>
        [Parameter(ParameterSetName = "Group", Mandatory = true, HelpMessage = "Create or update a Group entry.")]
        public SwitchParameter Group { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether to create or update a SubArea entry.
        /// </summary>
        [Parameter(ParameterSetName = "SubArea", Mandatory = true, HelpMessage = "Create or update a SubArea entry.")]
        public SwitchParameter SubArea { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether to create or update a Privilege entry.
        /// </summary>
        [Parameter(ParameterSetName = "Privilege", Mandatory = true, HelpMessage = "Create or update a Privilege entry.")]
        public SwitchParameter Privilege { get; set; }

        /// <summary>
        /// Gets or sets the ID of the entry to create or update.
        /// </summary>
        [Parameter(ParameterSetName = "Area", Mandatory = true, ValueFromPipelineByPropertyName = true, HelpMessage = "The ID of the entry to create or update.")]
        [Parameter(ParameterSetName = "Group", Mandatory = true, ValueFromPipelineByPropertyName = true, HelpMessage = "The ID of the entry to create or update.")]
        [Parameter(ParameterSetName = "SubArea", Mandatory = true, ValueFromPipelineByPropertyName = true, HelpMessage = "The ID of the entry to create or update.")]
        [Parameter(ParameterSetName = "Privilege", ValueFromPipelineByPropertyName = true, HelpMessage = "The ID of the entry to create or update (auto-generated for privileges if not provided).")]
        [ValidateNotNullOrEmpty]
        [Alias("Id")]
        public string EntryId { get; set; }

        /// <summary>
        /// Gets or sets the resource ID for localized titles.
        /// </summary>
        [Parameter(HelpMessage = "The resource ID for localized titles.")]
        public string ResourceId { get; set; }

        /// <summary>
        /// Gets or sets the resource ID for localized descriptions.
        /// </summary>
        [Parameter(HelpMessage = "The resource ID for localized descriptions.")]
        public string DescriptionResourceId { get; set; }

        /// <summary>
        /// Gets or sets the resource ID for localized tooltips.
        /// </summary>
        [Parameter(HelpMessage = "The resource ID for localized tooltips.")]
        public string ToolTipResourceId { get; set; }

        /// <summary>
        /// Gets or sets the titles of the entry keyed by LCID.
        /// Null values for a specific LCID will remove that LCID's title.
        /// Keys can be integers or strings that represent integers.
        /// </summary>
        [Parameter(HelpMessage = "The titles of the entry as a hashtable keyed by LCID (integer or string). Null values for a specific LCID will remove that LCID's title.")]
        public Hashtable Titles { get; set; }

        /// <summary>
        /// Gets or sets the descriptions of the entry keyed by LCID.
        /// Null values for a specific LCID will remove that LCID's description.
        /// Keys can be integers or strings that represent integers.
        /// </summary>
        [Parameter(HelpMessage = "The descriptions of the entry as a hashtable keyed by LCID (integer or string). Null values for a specific LCID will remove that LCID's description.")]
        public Hashtable Descriptions { get; set; }

        /// <summary>
        /// Gets or sets the icon path (for Areas and SubAreas).
        /// </summary>
        [Parameter(HelpMessage = "The icon path (for Areas and SubAreas).")]
        public string Icon { get; set; }

        /// <summary>
        /// Gets or sets the entity logical name (for SubAreas).
        /// </summary>
        [Parameter(HelpMessage = "The entity logical name (for SubAreas).")]
        [ArgumentCompleter(typeof(TableNameArgumentCompleter))]
        public string Entity { get; set; }

        /// <summary>
        /// Gets or sets the URL (for SubAreas).
        /// </summary>
        [Parameter(HelpMessage = "The URL (for SubAreas).")]
        public string Url { get; set; }

        /// <summary>
        /// Gets or sets the parent Area ID (required for Groups and SubAreas when creating).
        /// </summary>
        [Parameter(ParameterSetName = "Group", HelpMessage = "The parent Area ID (required for Groups when creating).")]
        [Parameter(ParameterSetName = "SubArea", HelpMessage = "The parent Area ID (required for SubAreas when creating).")]
        public string ParentAreaId { get; set; }

        /// <summary>
        /// Gets or sets the parent Group ID (required for SubAreas when creating).
        /// </summary>
        [Parameter(ParameterSetName = "SubArea", HelpMessage = "The parent Group ID (required for SubAreas when creating).")]
        public string ParentGroupId { get; set; }

        /// <summary>
        /// Gets or sets the parent SubArea ID (required for Privileges when creating).
        /// </summary>
        [Parameter(ParameterSetName = "Privilege", Mandatory = true, HelpMessage = "The parent SubArea ID (required for Privileges when creating).")]
        public string ParentSubAreaId { get; set; }

        /// <summary>
        /// Gets or sets the entity name for privilege entries.
        /// </summary>
        [Parameter(ParameterSetName = "Privilege", Mandatory = true, HelpMessage = "The entity name for privilege entries.")]
        [ArgumentCompleter(typeof(TableNameArgumentCompleter))]
        public string PrivilegeEntity { get; set; }

        /// <summary>
        /// Gets or sets the privilege name for privilege entries (e.g., Read, Write, Create, Delete).
        /// </summary>
        [Parameter(ParameterSetName = "Privilege", Mandatory = true, HelpMessage = "The privilege name for privilege entries (e.g., Read, Write, Create, Delete).")]
        [ValidateSet("Read", "Write", "Create", "Delete", "Append", "AppendTo", "Share", "Assign")]
        public string PrivilegeName { get; set; }

        /// <summary>
        /// Gets or sets the zero-based index position where the entry should be inserted or moved to.
        /// If not specified, the entry is added at the end of its parent container.
        /// </summary>
        [Parameter(HelpMessage = "The zero-based index position where the entry should be inserted or moved to.")]
        public int? Index { get; set; }

        /// <summary>
        /// Gets or sets the ID of the sibling entry before which this entry should be inserted or moved.
        /// </summary>
        [Parameter(HelpMessage = "The ID of the sibling entry before which this entry should be inserted or moved.")]
        public string Before { get; set; }

        /// <summary>
        /// Gets or sets the ID of the sibling entry after which this entry should be inserted or moved.
        /// </summary>
        [Parameter(HelpMessage = "The ID of the sibling entry after which this entry should be inserted or moved.")]
        public string After { get; set; }

        /// <summary>
        /// Gets or sets whether the entry is a default entry.
        /// </summary>
        [Parameter(HelpMessage = "Whether the entry is a default entry.")]
        public SwitchParameter IsDefault { get; set; }



        /// <summary>
        /// Gets or sets whether to return the created or updated entry.
        /// </summary>
        [Parameter(HelpMessage = "If specified, returns the created or updated entry.")]
        public SwitchParameter PassThru { get; set; }

        /// <summary>
        /// Processes the cmdlet request.
        /// </summary>
        protected override void ProcessRecord()
        {
            base.ProcessRecord();

            // Get values from InputObject if provided
            string sitemapUniqueName = SitemapUniqueName;
            Guid? sitemapId = SitemapId;
            string parentAreaId = ParentAreaId;
            string parentGroupId = ParentGroupId;

            if (InputObject != null)
            {
                if (string.IsNullOrEmpty(parentAreaId))
                    parentAreaId = InputObject.ParentAreaId;
                if (string.IsNullOrEmpty(parentGroupId))
                    parentGroupId = InputObject.ParentGroupId;
            }

            // Determine entry type from parameter set
            SitemapEntryType entryType;
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
                
                // Auto-generate EntryId for privileges if not provided
                if (string.IsNullOrEmpty(EntryId))
                {
                    EntryId = $"{PrivilegeEntity}_{PrivilegeName}";
                }
            }
            else
            {
                // This should not happen due to parameter set validation
                throw new InvalidOperationException("No entry type specified.");
            }

            if (Sitemap != null)
            {
                if (string.IsNullOrEmpty(sitemapUniqueName))
                    sitemapUniqueName = Sitemap.UniqueName;
                if (!sitemapId.HasValue || sitemapId.Value == Guid.Empty)
                    sitemapId = Sitemap.Id;
            }

            // Validate required parameters
            if (string.IsNullOrEmpty(sitemapUniqueName) && (!sitemapId.HasValue || sitemapId.Value == Guid.Empty))
            {
                ThrowTerminatingError(new ErrorRecord(
                    new ArgumentException("Either SitemapUniqueName, SitemapId, Sitemap, or InputObject must be provided."),
                    "MissingSitemapIdentifier",
                    ErrorCategory.InvalidArgument,
                    null));
                return;
            }

            // Validate entity names exist
            if (!string.IsNullOrEmpty(Entity))
            {
                ValidateEntityExists(Entity);
            }

            if (!string.IsNullOrEmpty(PrivilegeEntity))
            {
                ValidateEntityExists(PrivilegeEntity);
            }

            // Convert Hashtable parameters to Dictionary<int, string>
            Dictionary<int, string> titlesDict = null;
            Dictionary<int, string> descriptionsDict = null;

            if (Titles != null)
            {
                titlesDict = ConvertHashtableToIntDictionary(Titles, "Titles");
            }

            if (Descriptions != null)
            {
                descriptionsDict = ConvertHashtableToIntDictionary(Descriptions, "Descriptions");
            }

            WriteVerbose($"Retrieving sitemap '{sitemapUniqueName ?? sitemapId.ToString()}'...");

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
                var request = new RetrieveUnpublishedMultipleRequest { Query = query };
                var response = (RetrieveUnpublishedMultipleResponse)Connection.Execute(request);
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

            var retrievedSitemapId = sitemap.Id;

            var sitemapXml = sitemap.GetAttributeValue<string>("sitemapxml");

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

            // Try to find existing entry
            XElement entryElement = null;
            switch (entryType)
            {
                case SitemapEntryType.Area:
                    entryElement = FindElement(doc.Root, "Area", EntryId);
                    break;

                case SitemapEntryType.Group:
                    if (!string.IsNullOrEmpty(parentAreaId))
                    {
                        var area = FindElement(doc.Root, "Area", parentAreaId);
                        if (area != null)
                        {
                            entryElement = FindElement(area, "Group", EntryId);
                        }
                    }
                    else
                    {
                        // Search all areas for the group
                        foreach (var area in doc.Root?.Elements("Area") ?? Enumerable.Empty<XElement>())
                        {
                            entryElement = FindElement(area, "Group", EntryId);
                            if (entryElement != null)
                                break;
                        }
                    }
                    break;

                case SitemapEntryType.SubArea:
                    if (!string.IsNullOrEmpty(parentAreaId) && !string.IsNullOrEmpty(parentGroupId))
                    {
                        var area = FindElement(doc.Root, "Area", parentAreaId);
                        if (area != null)
                        {
                            var group = FindElement(area, "Group", parentGroupId);
                            if (group != null)
                            {
                                entryElement = FindElement(group, "SubArea", EntryId);
                            }
                        }
                    }
                    else
                    {
                        // Search all areas and groups for the subarea
                        foreach (var area in doc.Root?.Elements("Area") ?? Enumerable.Empty<XElement>())
                        {
                            foreach (var group in area.Elements("Group"))
                            {
                                entryElement = FindElement(group, "SubArea", EntryId);
                                if (entryElement != null)
                                    break;
                            }
                            if (entryElement != null)
                                break;
                        }
                    }
                    break;

                case SitemapEntryType.Privilege:
                    if (!string.IsNullOrEmpty(ParentSubAreaId))
                    {
                        // Search for the SubArea containing this privilege
                        foreach (var area in doc.Root?.Elements("Area") ?? Enumerable.Empty<XElement>())
                        {
                            foreach (var group in area.Elements("Group"))
                            {
                                var subArea = FindElement(group, "SubArea", ParentSubAreaId);
                                if (subArea != null)
                                {
                                    entryElement = subArea.Elements("Privilege")
                                        .FirstOrDefault(p => p.Attribute("Entity")?.Value == PrivilegeEntity &&
                                                           p.Attribute("Privilege")?.Value == PrivilegeName);
                                    break;
                                }
                            }
                            if (entryElement != null)
                                break;
                        }
                    }
                    break;
            }

            bool isUpdate = (entryElement != null);
            string operation = isUpdate ? "Update" : "Create";

            if (!ShouldProcess($"Sitemap '{sitemapUniqueName ?? sitemapId.ToString()}'", $"{operation} {entryType} entry '{EntryId}'"))
            {
                return;
            }

            bool needsMove = false;

            if (isUpdate)
            {
                string currentParentAreaId = null;
                string currentParentGroupId = null;

                // Determine current parent information
                switch (entryType)
                {
                    case SitemapEntryType.Group:
                        // Find the current parent area
                        foreach (var area in doc.Root?.Elements("Area") ?? Enumerable.Empty<XElement>())
                        {
                            if (FindElement(area, "Group", EntryId) != null)
                            {
                                currentParentAreaId = area.Attribute("Id")?.Value;
                                break;
                            }
                        }
                        // Check if parent area has changed
                        if (!string.IsNullOrEmpty(ParentAreaId) && ParentAreaId != currentParentAreaId)
                        {
                            needsMove = true;
                        }
                        break;

                    case SitemapEntryType.SubArea:
                        // Find the current parent area and group
                        foreach (var area in doc.Root?.Elements("Area") ?? Enumerable.Empty<XElement>())
                        {
                            foreach (var group in area.Elements("Group"))
                            {
                                if (FindElement(group, "SubArea", EntryId) != null)
                                {
                                    currentParentAreaId = area.Attribute("Id")?.Value;
                                    currentParentGroupId = group.Attribute("Id")?.Value;
                                    break;
                                }
                            }
                            if (!string.IsNullOrEmpty(currentParentAreaId))
                                break;
                        }
                        // Check if parent area or group has changed
                        if ((!string.IsNullOrEmpty(ParentAreaId) && ParentAreaId != currentParentAreaId) ||
                            (!string.IsNullOrEmpty(ParentGroupId) && ParentGroupId != currentParentGroupId))
                        {
                            needsMove = true;
                        }
                        break;
                }

                if (needsMove)
                {
                    WriteVerbose($"Moving {entryType} entry '{EntryId}' to new parent location...");

                    // Remove from current location
                    entryElement.Remove();

                    // Add to new location
                    bool moved = false;
                    switch (entryType)
                    {
                        case SitemapEntryType.Group:
                            var newParentArea = FindElement(doc.Root, "Area", ParentAreaId);
                            if (newParentArea != null)
                            {
                                AddElementAtPosition(newParentArea, entryElement, "Group");
                                moved = true;
                                WriteVerbose($"Moved Group '{EntryId}' to Area '{ParentAreaId}'");
                            }
                            else
                            {
                                ThrowTerminatingError(new ErrorRecord(
                                    new InvalidOperationException($"Parent Area '{ParentAreaId}' not found."),
                                    "ParentAreaNotFound",
                                    ErrorCategory.ObjectNotFound,
                                    ParentAreaId));
                                return;
                            }
                            break;

                        case SitemapEntryType.SubArea:
                            var newArea = FindElement(doc.Root, "Area", ParentAreaId ?? currentParentAreaId);
                            if (newArea != null)
                            {
                                var newGroup = FindElement(newArea, "Group", ParentGroupId ?? currentParentGroupId);
                                if (newGroup != null)
                                {
                                    AddElementAtPosition(newGroup, entryElement, "SubArea");
                                    moved = true;
                                    WriteVerbose($"Moved SubArea '{EntryId}' to Group '{ParentGroupId ?? currentParentGroupId}' in Area '{ParentAreaId ?? currentParentAreaId}'");
                                }
                                else
                                {
                                    ThrowTerminatingError(new ErrorRecord(
                                        new InvalidOperationException($"Parent Group '{ParentGroupId ?? currentParentGroupId}' not found in Area '{ParentAreaId ?? currentParentAreaId}'."),
                                        "ParentGroupNotFound",
                                        ErrorCategory.ObjectNotFound,
                                        ParentGroupId ?? currentParentGroupId));
                                    return;
                                }
                            }
                            else
                            {
                                ThrowTerminatingError(new ErrorRecord(
                                    new InvalidOperationException($"Parent Area '{ParentAreaId ?? currentParentAreaId}' not found."),
                                    "ParentAreaNotFound",
                                    ErrorCategory.ObjectNotFound,
                                    ParentAreaId ?? currentParentAreaId));
                                return;
                            }
                            break;
                    }

                    if (!moved)
                    {
                        ThrowTerminatingError(new ErrorRecord(
                            new InvalidOperationException("Failed to move entry to new location."),
                            "MoveEntryFailed",
                            ErrorCategory.InvalidOperation,
                            null));
                        return;
                    }
                }

                bool updated = false;

                if (!string.IsNullOrEmpty(ResourceId))
                {
                    entryElement.SetAttributeValue("ResourceId", ResourceId);
                    updated = true;
                }

                if (!string.IsNullOrEmpty(DescriptionResourceId))
                {
                    entryElement.SetAttributeValue("DescriptionResourceId", DescriptionResourceId);
                    updated = true;
                }

                if (!string.IsNullOrEmpty(ToolTipResourceId))
                {
                    // Note: Dataverse uses "ToolTipResourseId" with a typo in the XML
                    entryElement.SetAttributeValue("ToolTipResourseId", ToolTipResourceId);
                    updated = true;
                }

                // Handle Titles (new format) - merge with existing and update
                if (titlesDict != null)
                {
                    var mergedTitles = MergeTitles(entryElement, titlesDict);
                    UpdateTitlesElement(entryElement, mergedTitles);
                    // Remove old Title attribute if it exists
                    entryElement.Attribute("Title")?.Remove();
                    updated = true;
                }

                // Handle Descriptions (new format) - merge with existing and update
                if (descriptionsDict != null)
                {
                    var mergedDescriptions = MergeDescriptions(entryElement, descriptionsDict);
                    UpdateDescriptionsElement(entryElement, mergedDescriptions);
                    // Remove old Description attribute if it exists
                    entryElement.Attribute("Description")?.Remove();
                    updated = true;
                }

                if (!string.IsNullOrEmpty(Icon))
                {
                    entryElement.SetAttributeValue("Icon", Icon);
                    updated = true;
                }

                if (entryType == SitemapEntryType.SubArea)
                {
                    if (!string.IsNullOrEmpty(Entity))
                    {
                        entryElement.SetAttributeValue("Entity", Entity);
                        updated = true;
                    }

                    if (!string.IsNullOrEmpty(Url))
                    {
                        entryElement.SetAttributeValue("Url", Url);
                        updated = true;
                    }

                    if (IsDefault.IsPresent)
                    {
                        entryElement.SetAttributeValue("IsDefault", "true");
                        updated = true;
                    }


                }

                if (!updated)
                {
                    WriteWarning("No attributes specified to update. Specify at least one attribute to update.");
                    return;
                }

                WriteVerbose($"{entryType} entry '{EntryId}' updated successfully.");
            }
            else
            {
                // Create new entry
                WriteVerbose($"Creating new {entryType} entry '{EntryId}'...");

                // Validate parent requirements for creation
                if (entryType == SitemapEntryType.Group && string.IsNullOrEmpty(parentAreaId))
                {
                    ThrowTerminatingError(new ErrorRecord(
                        new ArgumentException("ParentAreaId is required when creating Group entries."),
                        "MissingParentAreaId",
                        ErrorCategory.InvalidArgument,
                        null));
                    return;
                }

                if (entryType == SitemapEntryType.SubArea && string.IsNullOrEmpty(parentGroupId))
                {
                    ThrowTerminatingError(new ErrorRecord(
                        new ArgumentException("ParentGroupId is required when creating SubArea entries."),
                        "MissingParentGroupId",
                        ErrorCategory.InvalidArgument,
                        null));
                    return;
                }

                // Create the new entry element
                var newElement = CreateEntryElement(entryType);

                // Add the entry to the appropriate parent
                bool added = false;
                switch (entryType)
                {
                    case SitemapEntryType.Area:
                        if (doc.Root != null)
                        {
                            AddElementAtPosition(doc.Root, newElement, "Area");
                            added = true;
                            WriteVerbose($"Added Area '{EntryId}' to sitemap root");
                        }
                        break;

                    case SitemapEntryType.Group:
                        var parentArea = FindElement(doc.Root, "Area", parentAreaId);
                        if (parentArea != null)
                        {
                            AddElementAtPosition(parentArea, newElement, "Group");
                            added = true;
                            WriteVerbose($"Added Group '{EntryId}' to Area '{parentAreaId}'");
                        }
                        else
                        {
                            ThrowTerminatingError(new ErrorRecord(
                                new InvalidOperationException($"Parent Area '{parentAreaId}' not found."),
                                "ParentAreaNotFound",
                                ErrorCategory.ObjectNotFound,
                                parentAreaId));
                            return;
                        }
                        break;

                    case SitemapEntryType.SubArea:
                        // Find the parent group across all areas (groups are unique)
                        XElement parentGroup = null;
                        XElement containingArea = null;
                        
                        foreach (var area in doc.Root?.Elements("Area") ?? Enumerable.Empty<XElement>())
                        {
                            parentGroup = FindElement(area, "Group", parentGroupId);
                            if (parentGroup != null)
                            {
                                containingArea = area;
                                break;
                            }
                        }
                        
                        if (parentGroup != null)
                        {
                            AddElementAtPosition(parentGroup, newElement, "SubArea");
                            added = true;
                            WriteVerbose($"Added SubArea '{EntryId}' to Group '{parentGroupId}' in Area '{containingArea.Attribute("Id")?.Value}'");
                        }
                        else
                        {
                            ThrowTerminatingError(new ErrorRecord(
                                new InvalidOperationException($"Parent Group '{parentGroupId}' not found."),
                                "ParentGroupNotFound",
                                ErrorCategory.ObjectNotFound,
                                parentGroupId));
                            return;
                        }
                        break;

                    case SitemapEntryType.Privilege:
                        // Find the parent SubArea
                        XElement parentSubArea = null;
                        foreach (var area in doc.Root?.Elements("Area") ?? Enumerable.Empty<XElement>())
                        {
                            foreach (var group in area.Elements("Group"))
                            {
                                parentSubArea = FindElement(group, "SubArea", ParentSubAreaId);
                                if (parentSubArea != null)
                                    break;
                            }
                            if (parentSubArea != null)
                                break;
                        }

                        if (parentSubArea != null)
                        {
                            // Remove all existing privileges for this entity (replace case)
                            var existingPrivileges = parentSubArea.Elements("Privilege")
                                .Where(p => string.Equals(p.Attribute("Entity")?.Value, PrivilegeEntity, StringComparison.OrdinalIgnoreCase))
                                .ToList();

                            foreach (var existingPrivilege in existingPrivileges)
                            {
                                existingPrivilege.Remove();
                            }

                            // Add new privilege
                            parentSubArea.Add(newElement);
                            WriteVerbose($"Added Privilege '{PrivilegeEntity}_{PrivilegeName}' to SubArea '{ParentSubAreaId}'");
                            added = true;
                        }
                        else
                        {
                            ThrowTerminatingError(new ErrorRecord(
                                new InvalidOperationException($"Parent SubArea '{ParentSubAreaId}' not found."),
                                "ParentSubAreaNotFound",
                                ErrorCategory.ObjectNotFound,
                                ParentSubAreaId));
                            return;
                        }
                        break;
                }

                if (!added)
                {
                    ThrowTerminatingError(new ErrorRecord(
                        new InvalidOperationException("Failed to add entry to sitemap."),
                        "AddEntryFailed",
                        ErrorCategory.InvalidOperation,
                        null));
                    return;
                }

                WriteVerbose($"{entryType} entry '{EntryId}' created successfully.");
            }

            // Update the sitemap
            var updateEntity = new Entity("sitemap", retrievedSitemapId);
            updateEntity["sitemapxml"] = doc.ToString();

            WriteVerbose("Updating sitemap in Dataverse...");
            Connection.Update(updateEntity);

            WriteVerbose($"{entryType} entry '{EntryId}' {(isUpdate ? "updated" : "created")} in sitemap '{sitemapUniqueName ?? sitemapId.ToString()}' successfully.");

            if (PassThru.IsPresent)
            {
                // Determine the final parent IDs after any move
                string finalParentAreaId = parentAreaId;
                string finalParentGroupId = parentGroupId;

                // For SubArea creation with only ParentGroupId, determine the area
                if (!isUpdate && entryType == SitemapEntryType.SubArea && string.IsNullOrEmpty(finalParentAreaId) && !string.IsNullOrEmpty(finalParentGroupId))
                {
                    foreach (var area in doc.Root?.Elements("Area") ?? Enumerable.Empty<XElement>())
                    {
                        if (FindElement(area, "Group", finalParentGroupId) != null)
                        {
                            finalParentAreaId = area.Attribute("Id")?.Value;
                            break;
                        }
                    }
                }

                if (isUpdate && !needsMove)
                {
                    // For updates without move, determine current parent IDs from the final XML
                    switch (entryType)
                    {
                        case SitemapEntryType.Group:
                            foreach (var area in doc.Root?.Elements("Area") ?? Enumerable.Empty<XElement>())
                            {
                                if (FindElement(area, "Group", EntryId) != null)
                                {
                                    finalParentAreaId = area.Attribute("Id")?.Value;
                                    break;
                                }
                            }
                            break;

                        case SitemapEntryType.SubArea:
                            foreach (var area in doc.Root?.Elements("Area") ?? Enumerable.Empty<XElement>())
                            {
                                foreach (var group in area.Elements("Group"))
                                {
                                    if (FindElement(group, "SubArea", EntryId) != null)
                                    {
                                        finalParentAreaId = area.Attribute("Id")?.Value;
                                        finalParentGroupId = group.Attribute("Id")?.Value;
                                        break;
                                    }
                                }
                                if (!string.IsNullOrEmpty(finalParentAreaId))
                                    break;
                            }
                            break;
                    }
                }
                else if (needsMove)
                {
                    // For moves, use the new parent IDs
                    finalParentAreaId = ParentAreaId ?? finalParentAreaId;
                    finalParentGroupId = ParentGroupId ?? finalParentGroupId;
                }

                var entry = new SitemapEntryInfo
                {
                    EntryType = entryType,
                    Id = EntryId,
                    ResourceId = ResourceId,
                    DescriptionResourceId = DescriptionResourceId,
                    ToolTipResourceId = ToolTipResourceId,
                    Titles = titlesDict,
                    Descriptions = descriptionsDict,
                    Icon = Icon,
                    Entity = Entity,
                    Url = Url,
                    ParentAreaId = finalParentAreaId,
                    ParentGroupId = finalParentGroupId,
                    IsDefault = IsDefault.IsPresent ? true : (bool?)null,
                    PrivilegeEntity = PrivilegeEntity,
                    PrivilegeName = PrivilegeName,
                    ParentSubAreaId = ParentSubAreaId,
                };
                WriteObject(entry);
            }
        }

        private XElement CreateEntryElement(SitemapEntryType entryType)
        {
            XElement element;
            
            if (entryType == SitemapEntryType.Privilege)
            {
                // For privileges, create a Privilege element with Entity and Privilege attributes
                element = new XElement("Privilege");
                element.SetAttributeValue("Entity", PrivilegeEntity);
                element.SetAttributeValue("Privilege", PrivilegeName);
                return element;
            }
            
            var elementName = entryType.ToString();
            element = new XElement(elementName);

            element.SetAttributeValue("Id", EntryId);

            if (!string.IsNullOrEmpty(ResourceId))
                element.SetAttributeValue("ResourceId", ResourceId);

            if (!string.IsNullOrEmpty(DescriptionResourceId))
                element.SetAttributeValue("DescriptionResourceId", DescriptionResourceId);

            if (!string.IsNullOrEmpty(ToolTipResourceId))
                // Note: Dataverse uses "ToolTipResourseId" with a typo in the XML
                element.SetAttributeValue("ToolTipResourseId", ToolTipResourceId);

            // Handle Titles (new format)
            if (Titles != null)
            {
                var titlesToSet = new Dictionary<int, string>();
                
                // Add titles from Titles hashtable (converted to dictionary)
                var titlesDict = ConvertHashtableToIntDictionary(Titles, "Titles");
                foreach (var kvp in titlesDict)
                {
                    if (kvp.Value != null) // Skip null values
                        titlesToSet[kvp.Key] = kvp.Value;
                }
                
                UpdateTitlesElement(element, titlesToSet);
            }

            // Handle Descriptions (new format)
            if (Descriptions != null)
            {
                var descriptionsToSet = new Dictionary<int, string>();
                
                // Add descriptions from Descriptions hashtable (converted to dictionary)
                var descriptionsDict = ConvertHashtableToIntDictionary(Descriptions, "Descriptions");
                foreach (var kvp in descriptionsDict)
                {
                    if (kvp.Value != null) // Skip null values
                        descriptionsToSet[kvp.Key] = kvp.Value;
                }
                
                UpdateDescriptionsElement(element, descriptionsToSet);
            }

            if (!string.IsNullOrEmpty(Icon))
                element.SetAttributeValue("Icon", Icon);

            // Set required attributes for Area elements
            if (entryType == SitemapEntryType.Area)
            {
                element.SetAttributeValue("ShowGroups", "true");
                element.SetAttributeValue("IntroducedVersion", "7.0.0.0");
            }

            // Set required attributes for Group elements
            if (entryType == SitemapEntryType.Group)
            {
                element.SetAttributeValue("IntroducedVersion", "7.0.0.0");
                element.SetAttributeValue("IsProfile", "false");
            }

            if (entryType == SitemapEntryType.SubArea)
            {
                if (!string.IsNullOrEmpty(Entity))
                    element.SetAttributeValue("Entity", Entity);

                if (!string.IsNullOrEmpty(Url))
                    element.SetAttributeValue("Url", Url);

                if (IsDefault.IsPresent)
                    element.SetAttributeValue("IsDefault", "true");


            }

            return element;
        }

        private XElement FindElement(XElement parent, string elementName, string id)
        {
            if (parent == null)
                return null;

            return parent.Elements(elementName)
                .FirstOrDefault(e => e.Attribute("Id")?.Value == id);
        }

        private void AddElementAtPosition(XElement parent, XElement newElement, string elementType)
        {
            // If no position is specified, add at the end
            if (!Index.HasValue && string.IsNullOrEmpty(Before) && string.IsNullOrEmpty(After))
            {
                parent.Add(newElement);
                return;
            }

            // Get all child elements of the same type
            var siblings = parent.Elements(elementType).ToList();

            // Handle Index position
            if (Index.HasValue)
            {
                int index = Index.Value;
                if (index < 0 || index > siblings.Count)
                {
                    WriteWarning($"Index {index} is out of range (0-{siblings.Count}). Adding at the end.");
                    parent.Add(newElement);
                }
                else if (index == siblings.Count)
                {
                    parent.Add(newElement);
                }
                else
                {
                    siblings[index].AddBeforeSelf(newElement);
                }
                return;
            }

            // Handle Before position
            if (!string.IsNullOrEmpty(Before))
            {
                var beforeElement = siblings.FirstOrDefault(e => e.Attribute("Id")?.Value == Before);
                if (beforeElement != null)
                {
                    beforeElement.AddBeforeSelf(newElement);
                }
                else
                {
                    WriteWarning($"Entry with ID '{Before}' not found. Adding at the end.");
                    parent.Add(newElement);
                }
                return;
            }

            // Handle After position
            if (!string.IsNullOrEmpty(After))
            {
                var afterElement = siblings.FirstOrDefault(e => e.Attribute("Id")?.Value == After);
                if (afterElement != null)
                {
                    afterElement.AddAfterSelf(newElement);
                }
                else
                {
                    WriteWarning($"Entry with ID '{After}' not found. Adding at the end.");
                    parent.Add(newElement);
                }
                return;
            }
        }

        private void UpdateTitlesElement(XElement entryElement, Dictionary<int, string> titles)
        {
            // Remove existing Titles element
            entryElement.Element("Titles")?.Remove();
            
            // If titles is null or empty, we're done (removal only)
            if (titles == null || titles.Count == 0)
                return;
                
            // Create new Titles element
            var titlesElement = new XElement("Titles");
            foreach (var kvp in titles.OrderBy(x => x.Key))
            {
                if (kvp.Value != null) // Skip null values (they indicate removal)
                {
                    titlesElement.Add(new XElement("Title",
                        new XAttribute("LCID", kvp.Key),
                        new XAttribute("Title", kvp.Value)));
                }
            }
            
            // Only add Titles element if it has children
            if (titlesElement.HasElements)
                entryElement.Add(titlesElement);
        }

        private void UpdateDescriptionsElement(XElement entryElement, Dictionary<int, string> descriptions)
        {
            // Remove existing Descriptions element
            entryElement.Element("Descriptions")?.Remove();
            
            // If descriptions is null or empty, we're done (removal only)
            if (descriptions == null || descriptions.Count == 0)
                return;
                
            // Create new Descriptions element
            var descriptionsElement = new XElement("Descriptions");
            foreach (var kvp in descriptions.OrderBy(x => x.Key))
            {
                if (kvp.Value != null) // Skip null values (they indicate removal)
                {
                    descriptionsElement.Add(new XElement("Description",
                        new XAttribute("LCID", kvp.Key),
                        new XAttribute("Description", kvp.Value)));
                }
            }
            
            // Only add Descriptions element if it has children
            if (descriptionsElement.HasElements)
                entryElement.Add(descriptionsElement);
        }

        private Dictionary<int, string> MergeTitles(XElement entryElement, Dictionary<int, string> newTitles)
        {
            var result = new Dictionary<int, string>();
            
            // First, load existing titles from the element
            var existingTitlesElement = entryElement.Element("Titles");
            if (existingTitlesElement != null)
            {
                foreach (var titleElement in existingTitlesElement.Elements("Title"))
                {
                    if (int.TryParse(titleElement.Attribute("LCID")?.Value, out int lcid))
                    {
                        var titleValue = titleElement.Attribute("Title")?.Value;
                        if (!string.IsNullOrEmpty(titleValue))
                            result[lcid] = titleValue;
                    }
                }
            }
            else
            {
                // Fallback to old Title attribute
                var oldTitle = entryElement.Attribute("Title")?.Value;
                if (!string.IsNullOrEmpty(oldTitle))
                    result[1033] = oldTitle;
            }
            
            // Apply new titles (additive, null removes)
            if (newTitles != null)
            {
                foreach (var kvp in newTitles)
                {
                    if (kvp.Value == null)
                        result.Remove(kvp.Key);
                    else
                        result[kvp.Key] = kvp.Value;
                }
            }
            
            return result.Count > 0 ? result : null;
        }

        private Dictionary<int, string> MergeDescriptions(XElement entryElement, Dictionary<int, string> newDescriptions)
        {
            var result = new Dictionary<int, string>();
            
            // First, load existing descriptions from the element
            var existingDescriptionsElement = entryElement.Element("Descriptions");
            if (existingDescriptionsElement != null)
            {
                foreach (var descElement in existingDescriptionsElement.Elements("Description"))
                {
                    if (int.TryParse(descElement.Attribute("LCID")?.Value, out int lcid))
                    {
                        var descValue = descElement.Attribute("Description")?.Value;
                        if (!string.IsNullOrEmpty(descValue))
                            result[lcid] = descValue;
                    }
                }
            }
            else
            {
                // Fallback to old Description attribute
                var oldDescription = entryElement.Attribute("Description")?.Value;
                if (!string.IsNullOrEmpty(oldDescription))
                    result[1033] = oldDescription;
            }
            
            // Apply new descriptions (additive, null removes)
            if (newDescriptions != null)
            {
                foreach (var kvp in newDescriptions)
                {
                    if (kvp.Value == null)
                        result.Remove(kvp.Key);
                    else
                        result[kvp.Key] = kvp.Value;
                }
            }
            
            return result.Count > 0 ? result : null;
        }

        /// <summary>
        /// Converts a Hashtable with integer or string keys to a Dictionary with integer keys.
        /// </summary>
        /// <param name="hashtable">The hashtable to convert.</param>
        /// <param name="parameterName">The name of the parameter for error messages.</param>
        /// <returns>A dictionary with integer keys and string values.</returns>
        private Dictionary<int, string> ConvertHashtableToIntDictionary(Hashtable hashtable, string parameterName)
        {
            if (hashtable == null)
                return null;

            var result = new Dictionary<int, string>();

            foreach (DictionaryEntry entry in hashtable)
            {
                int key;

                // Try to convert the key to an integer
                if (entry.Key is int intKey)
                {
                    key = intKey;
                }
                else if (entry.Key is string strKey)
                {
                    if (!int.TryParse(strKey, out key))
                    {
                        ThrowTerminatingError(new ErrorRecord(
                            new ArgumentException($"Invalid key '{strKey}' in {parameterName}. Keys must be integers or strings that can be parsed as integers (LCID values)."),
                            "InvalidHashtableKey",
                            ErrorCategory.InvalidArgument,
                            strKey));
                        return null;
                    }
                }
                else
                {
                    ThrowTerminatingError(new ErrorRecord(
                        new ArgumentException($"Invalid key type '{entry.Key?.GetType().Name}' in {parameterName}. Keys must be integers or strings that can be parsed as integers (LCID values)."),
                        "InvalidHashtableKeyType",
                        ErrorCategory.InvalidArgument,
                        entry.Key));
                    return null;
                }

                // The value should be a string or null
                if (entry.Value == null)
                {
                    result[key] = null;
                }
                else if (entry.Value is string strValue)
                {
                    result[key] = strValue;
                }
                else
                {
                    ThrowTerminatingError(new ErrorRecord(
                        new ArgumentException($"Invalid value type '{entry.Value.GetType().Name}' for key '{key}' in {parameterName}. Values must be strings or null."),
                        "InvalidHashtableValueType",
                        ErrorCategory.InvalidArgument,
                        entry.Value));
                    return null;
                }
            }

            return result;
        }

        /// <summary>
        /// Validates that an entity with the given logical name exists in Dataverse.
        /// Checks both published and unpublished entities.
        /// </summary>
        /// <param name="entityLogicalName">The logical name of the entity to validate.</param>
        private void ValidateEntityExists(string entityLogicalName)
        {
            WriteVerbose($"Validating entity '{entityLogicalName}' exists...");

            try
            {
                // Try to retrieve entity metadata (includes unpublished entities)
                var request = new RetrieveEntityRequest
                {
                    LogicalName = entityLogicalName,
                    EntityFilters = EntityFilters.Entity,
                    RetrieveAsIfPublished = true  // Include unpublished entities
                };

                var response = (RetrieveEntityResponse)Connection.Execute(request);

                if (response.EntityMetadata != null)
                {
                    WriteVerbose($"Entity '{entityLogicalName}' found (MetadataId: {response.EntityMetadata.MetadataId}).");
                }
            }
            catch (Exception ex)
            {
                ThrowTerminatingError(new ErrorRecord(
                    new InvalidOperationException($"Entity '{entityLogicalName}' does not exist or could not be retrieved. Ensure the entity name is correct and includes both published and unpublished entities. Error: {ex.Message}"),
                    "EntityNotFound",
                    ErrorCategory.ObjectNotFound,
                    entityLogicalName));
            }
        }
    }
}
