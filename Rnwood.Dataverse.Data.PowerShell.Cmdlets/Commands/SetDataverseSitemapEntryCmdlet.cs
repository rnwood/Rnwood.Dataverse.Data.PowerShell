using System;
using System.Linq;
using System.Management.Automation;
using System.Xml.Linq;
using Microsoft.Xrm.Sdk;
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
        /// Gets or sets the name of the sitemap containing the entry.
        /// </summary>
        [Parameter(Position = 0, ValueFromPipelineByPropertyName = true, HelpMessage = "The name of the sitemap containing the entry.")]
        [Alias("Name")]
        public string SitemapName { get; set; }

        /// <summary>
        /// Gets or sets the ID of the sitemap containing the entry.
        /// </summary>
        [Parameter(ValueFromPipelineByPropertyName = true, HelpMessage = "The ID of the sitemap containing the entry.")]
        public Guid? SitemapId { get; set; }

        /// <summary>
        /// Gets or sets the type of entry to create or update.
        /// </summary>
        [Parameter(Mandatory = true, HelpMessage = "The type of entry to create or update (Area, Group, SubArea).")]
        public SitemapEntryType EntryType { get; set; }

        /// <summary>
        /// Gets or sets the ID of the entry to create or update.
        /// </summary>
        [Parameter(Mandatory = true, ValueFromPipelineByPropertyName = true, HelpMessage = "The ID of the entry to create or update.")]
        [ValidateNotNullOrEmpty]
        [Alias("Id")]
        public string EntryId { get; set; }

        /// <summary>
        /// Gets or sets the resource ID for localized titles.
        /// </summary>
        [Parameter(HelpMessage = "The resource ID for localized titles.")]
        public string ResourceId { get; set; }

        /// <summary>
        /// Gets or sets the title/label of the entry.
        /// </summary>
        [Parameter(HelpMessage = "The title/label of the entry.")]
        public string Title { get; set; }

        /// <summary>
        /// Gets or sets the description of the entry.
        /// </summary>
        [Parameter(HelpMessage = "The description of the entry.")]
        public string Description { get; set; }

        /// <summary>
        /// Gets or sets the icon path (for Areas and SubAreas).
        /// </summary>
        [Parameter(HelpMessage = "The icon path (for Areas and SubAreas).")]
        public string Icon { get; set; }

        /// <summary>
        /// Gets or sets the entity logical name (for SubAreas).
        /// </summary>
        [Parameter(HelpMessage = "The entity logical name (for SubAreas).")]
        public string Entity { get; set; }

        /// <summary>
        /// Gets or sets the URL (for SubAreas).
        /// </summary>
        [Parameter(HelpMessage = "The URL (for SubAreas).")]
        public string Url { get; set; }

        /// <summary>
        /// Gets or sets the parent Area ID (required for Groups and SubAreas when creating).
        /// </summary>
        [Parameter(HelpMessage = "The parent Area ID (required for Groups and SubAreas when creating).")]
        public string ParentAreaId { get; set; }

        /// <summary>
        /// Gets or sets the parent Group ID (required for SubAreas when creating).
        /// </summary>
        [Parameter(HelpMessage = "The parent Group ID (required for SubAreas when creating).")]
        public string ParentGroupId { get; set; }

        /// <summary>
        /// Gets or sets whether the entry is a default entry.
        /// </summary>
        [Parameter(HelpMessage = "Whether the entry is a default entry.")]
        public SwitchParameter IsDefault { get; set; }

        /// <summary>
        /// Gets or sets the privilege required to view this entry.
        /// </summary>
        [Parameter(HelpMessage = "The privilege required to view this entry.")]
        public string Privilege { get; set; }

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
            string sitemapName = SitemapName;
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

            if (Sitemap != null)
            {
                if (string.IsNullOrEmpty(sitemapName))
                    sitemapName = Sitemap.Name;
                if (!sitemapId.HasValue || sitemapId.Value == Guid.Empty)
                    sitemapId = Sitemap.Id;
            }

            // Validate required parameters
            if (string.IsNullOrEmpty(sitemapName) && (!sitemapId.HasValue || sitemapId.Value == Guid.Empty))
            {
                ThrowTerminatingError(new ErrorRecord(
                    new ArgumentException("Either SitemapName, SitemapId, Sitemap, or InputObject must be provided."),
                    "MissingSitemapIdentifier",
                    ErrorCategory.InvalidArgument,
                    null));
                return;
            }

            WriteVerbose($"Retrieving sitemap '{sitemapName ?? sitemapId.ToString()}'...");

            // Retrieve the sitemap
            var query = new QueryExpression("sitemap")
            {
                ColumnSet = new ColumnSet("sitemapid", "sitemapname", "sitemapxml", "ismanaged"),
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
            var retrievedSitemapId = sitemap.Id;
            var isManaged = sitemap.GetAttributeValue<bool>("ismanaged");

            if (isManaged)
            {
                ThrowTerminatingError(new ErrorRecord(
                    new InvalidOperationException("Cannot modify entries in a managed sitemap."),
                    "ManagedSitemapModificationNotAllowed",
                    ErrorCategory.InvalidOperation,
                    sitemapName ?? sitemapId.ToString()));
                return;
            }

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
            switch (EntryType)
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
            }

            bool isUpdate = (entryElement != null);
            string operation = isUpdate ? "Update" : "Create";

            if (!ShouldProcess($"Sitemap '{sitemapName ?? sitemapId.ToString()}'", $"{operation} {EntryType} entry '{EntryId}'"))
            {
                return;
            }

            if (isUpdate)
            {
                // Update existing entry
                WriteVerbose($"Updating existing {EntryType} entry '{EntryId}'...");

                bool updated = false;

                if (!string.IsNullOrEmpty(ResourceId))
                {
                    entryElement.SetAttributeValue("ResourceId", ResourceId);
                    updated = true;
                }

                if (!string.IsNullOrEmpty(Title))
                {
                    entryElement.SetAttributeValue("Title", Title);
                    updated = true;
                }

                if (!string.IsNullOrEmpty(Description))
                {
                    entryElement.SetAttributeValue("Description", Description);
                    updated = true;
                }

                if (!string.IsNullOrEmpty(Icon))
                {
                    entryElement.SetAttributeValue("Icon", Icon);
                    updated = true;
                }

                if (EntryType == SitemapEntryType.SubArea)
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

                    if (!string.IsNullOrEmpty(Privilege))
                    {
                        entryElement.SetAttributeValue("Privilege", Privilege);
                        updated = true;
                    }
                }

                if (!updated)
                {
                    WriteWarning("No attributes specified to update. Specify at least one attribute to update.");
                    return;
                }

                WriteVerbose($"{EntryType} entry '{EntryId}' updated successfully.");
            }
            else
            {
                // Create new entry
                WriteVerbose($"Creating new {EntryType} entry '{EntryId}'...");

                // Validate parent requirements for creation
                if (EntryType == SitemapEntryType.Group || EntryType == SitemapEntryType.SubArea)
                {
                    if (string.IsNullOrEmpty(parentAreaId))
                    {
                        ThrowTerminatingError(new ErrorRecord(
                            new ArgumentException("ParentAreaId is required when creating Group and SubArea entries."),
                            "MissingParentAreaId",
                            ErrorCategory.InvalidArgument,
                            null));
                        return;
                    }
                }

                if (EntryType == SitemapEntryType.SubArea && string.IsNullOrEmpty(parentGroupId))
                {
                    ThrowTerminatingError(new ErrorRecord(
                        new ArgumentException("ParentGroupId is required when creating SubArea entries."),
                        "MissingParentGroupId",
                        ErrorCategory.InvalidArgument,
                        null));
                    return;
                }

                // Create the new entry element
                var newElement = CreateEntryElement();

                // Add the entry to the appropriate parent
                bool added = false;
                switch (EntryType)
                {
                    case SitemapEntryType.Area:
                        doc.Root?.Add(newElement);
                        added = true;
                        WriteVerbose($"Added Area '{EntryId}' to sitemap root");
                        break;

                    case SitemapEntryType.Group:
                        var parentArea = FindElement(doc.Root, "Area", parentAreaId);
                        if (parentArea != null)
                        {
                            parentArea.Add(newElement);
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
                        var area = FindElement(doc.Root, "Area", parentAreaId);
                        if (area != null)
                        {
                            var parentGroup = FindElement(area, "Group", parentGroupId);
                            if (parentGroup != null)
                            {
                                parentGroup.Add(newElement);
                                added = true;
                                WriteVerbose($"Added SubArea '{EntryId}' to Group '{parentGroupId}' in Area '{parentAreaId}'");
                            }
                            else
                            {
                                ThrowTerminatingError(new ErrorRecord(
                                    new InvalidOperationException($"Parent Group '{parentGroupId}' not found in Area '{parentAreaId}'."),
                                    "ParentGroupNotFound",
                                    ErrorCategory.ObjectNotFound,
                                    parentGroupId));
                                return;
                            }
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

                WriteVerbose($"{EntryType} entry '{EntryId}' created successfully.");
            }

            // Update the sitemap
            var updateEntity = new Entity("sitemap", retrievedSitemapId);
            updateEntity["sitemapxml"] = doc.ToString();

            WriteVerbose("Updating sitemap in Dataverse...");
            Connection.Update(updateEntity);

            WriteObject($"{EntryType} entry '{EntryId}' {(isUpdate ? "updated" : "created")} in sitemap '{sitemapName ?? sitemapId.ToString()}' successfully.");

            if (PassThru.IsPresent)
            {
                var entry = new SitemapEntryInfo
                {
                    EntryType = EntryType,
                    Id = EntryId,
                    ResourceId = ResourceId,
                    Title = Title,
                    Description = Description,
                    Icon = Icon,
                    Entity = Entity,
                    Url = Url,
                    ParentAreaId = parentAreaId,
                    ParentGroupId = parentGroupId,
                    IsDefault = IsDefault.IsPresent ? true : (bool?)null,
                    Privilege = Privilege
                };
                WriteObject(entry);
            }
        }

        private XElement CreateEntryElement()
        {
            var elementName = EntryType.ToString();
            var element = new XElement(elementName);

            element.SetAttributeValue("Id", EntryId);

            if (!string.IsNullOrEmpty(ResourceId))
                element.SetAttributeValue("ResourceId", ResourceId);

            if (!string.IsNullOrEmpty(Title))
                element.SetAttributeValue("Title", Title);

            if (!string.IsNullOrEmpty(Description))
                element.SetAttributeValue("Description", Description);

            if (!string.IsNullOrEmpty(Icon))
                element.SetAttributeValue("Icon", Icon);

            if (EntryType == SitemapEntryType.SubArea)
            {
                if (!string.IsNullOrEmpty(Entity))
                    element.SetAttributeValue("Entity", Entity);

                if (!string.IsNullOrEmpty(Url))
                    element.SetAttributeValue("Url", Url);

                if (IsDefault.IsPresent)
                    element.SetAttributeValue("IsDefault", "true");

                if (!string.IsNullOrEmpty(Privilege))
                    element.SetAttributeValue("Privilege", Privilege);
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
    }
}
