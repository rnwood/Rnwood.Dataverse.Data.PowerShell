using System;
using System.Linq;
using System.Management.Automation;
using System.Xml.Linq;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Query;

namespace Rnwood.Dataverse.Data.PowerShell.Commands
{
    /// <summary>
    /// Updates an existing entry (Area, Group, or SubArea) in a Dataverse sitemap.
    /// </summary>
    [Cmdlet(VerbsCommon.Set, "DataverseSitemapEntry", SupportsShouldProcess = true, ConfirmImpact = ConfirmImpact.Medium)]
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
        /// Gets or sets the type of entry to update.
        /// </summary>
        [Parameter(HelpMessage = "The type of entry to update (Area, Group, SubArea).")]
        public SitemapEntryType? EntryType { get; set; }

        /// <summary>
        /// Gets or sets the ID of the entry to update.
        /// </summary>
        [Parameter(ValueFromPipelineByPropertyName = true, HelpMessage = "The ID of the entry to update.")]
        [Alias("Id")]
        public string EntryId { get; set; }

        /// <summary>
        /// Gets or sets the new resource ID for localized titles.
        /// </summary>
        [Parameter(HelpMessage = "The new resource ID for localized titles.")]
        public string ResourceId { get; set; }

        /// <summary>
        /// Gets or sets the new title/label of the entry.
        /// </summary>
        [Parameter(HelpMessage = "The new title/label of the entry.")]
        public string Title { get; set; }

        /// <summary>
        /// Gets or sets the new description of the entry.
        /// </summary>
        [Parameter(HelpMessage = "The new description of the entry.")]
        public string Description { get; set; }

        /// <summary>
        /// Gets or sets the new icon path (for Areas and SubAreas).
        /// </summary>
        [Parameter(HelpMessage = "The new icon path (for Areas and SubAreas).")]
        public string Icon { get; set; }

        /// <summary>
        /// Gets or sets the new entity logical name (for SubAreas).
        /// </summary>
        [Parameter(HelpMessage = "The new entity logical name (for SubAreas).")]
        public string Entity { get; set; }

        /// <summary>
        /// Gets or sets the new URL (for SubAreas).
        /// </summary>
        [Parameter(HelpMessage = "The new URL (for SubAreas).")]
        public string Url { get; set; }

        /// <summary>
        /// Gets or sets the parent Area ID (for locating Groups and SubAreas).
        /// </summary>
        [Parameter(HelpMessage = "The parent Area ID (for locating Groups and SubAreas).")]
        public string ParentAreaId { get; set; }

        /// <summary>
        /// Gets or sets the parent Group ID (for locating SubAreas).
        /// </summary>
        [Parameter(HelpMessage = "The parent Group ID (for locating SubAreas).")]
        public string ParentGroupId { get; set; }

        /// <summary>
        /// Processes the cmdlet request.
        /// </summary>
        protected override void ProcessRecord()
        {
            base.ProcessRecord();

            // Get values from InputObject if provided
            string sitemapName = SitemapName;
            Guid? sitemapId = SitemapId;
            SitemapEntryType? entryType = EntryType;
            string entryId = EntryId;
            string parentAreaId = ParentAreaId;
            string parentGroupId = ParentGroupId;

            if (InputObject != null)
            {
                if (!entryType.HasValue)
                    entryType = InputObject.EntryType;
                if (string.IsNullOrEmpty(entryId))
                    entryId = InputObject.Id;
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

            if (!entryType.HasValue)
            {
                ThrowTerminatingError(new ErrorRecord(
                    new ArgumentException("EntryType is required. Provide it directly or via InputObject."),
                    "MissingEntryType",
                    ErrorCategory.InvalidArgument,
                    null));
                return;
            }

            if (string.IsNullOrEmpty(entryId))
            {
                ThrowTerminatingError(new ErrorRecord(
                    new ArgumentException("EntryId is required. Provide it directly or via InputObject."),
                    "MissingEntryId",
                    ErrorCategory.InvalidArgument,
                    null));
                return;
            }

            if (!ShouldProcess($"Sitemap '{sitemapName ?? sitemapId.ToString()}'", $"Update {entryType} entry '{entryId}'"))
            {
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
                    new InvalidOperationException($"Sitemap '{sitemapName}' not found."),
                    "SitemapNotFound",
                    ErrorCategory.ObjectNotFound,
                    sitemapName));
                return;
            }

            var sitemap = sitemaps.Entities[0];
            var retrievedSitemapId = sitemap.Id;
            var isManaged = sitemap.GetAttributeValue<bool>("ismanaged");

            if (isManaged)
            {
                ThrowTerminatingError(new ErrorRecord(
                    new InvalidOperationException("Cannot update entries in a managed sitemap."),
                    "ManagedSitemapModificationNotAllowed",
                    ErrorCategory.InvalidOperation,
                    sitemapName));
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

            // Find the entry to update
            XElement entryElement = null;
            switch (entryType.Value)
            {
                case SitemapEntryType.Area:
                    entryElement = FindElement(doc.Root, "Area", entryId);
                    break;

                case SitemapEntryType.Group:
                    if (!string.IsNullOrEmpty(parentAreaId))
                    {
                        var area = FindElement(doc.Root, "Area", parentAreaId);
                        if (area != null)
                        {
                            entryElement = FindElement(area, "Group", entryId);
                        }
                    }
                    else
                    {
                        // Search all areas for the group
                        foreach (var area in doc.Root?.Elements("Area") ?? Enumerable.Empty<XElement>())
                        {
                            entryElement = FindElement(area, "Group", entryId);
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
                                entryElement = FindElement(group, "SubArea", entryId);
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
                                entryElement = FindElement(group, "SubArea", entryId);
                                if (entryElement != null)
                                    break;
                            }
                            if (entryElement != null)
                                break;
                        }
                    }
                    break;
            }

            if (entryElement == null)
            {
                ThrowTerminatingError(new ErrorRecord(
                    new InvalidOperationException($"{entryType.Value} entry '{entryId}' not found in sitemap."),
                    "EntryNotFound",
                    ErrorCategory.ObjectNotFound,
                    entryId));
                return;
            }

            // Update attributes
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

            if (entryType.Value == SitemapEntryType.SubArea)
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
            }

            if (!updated)
            {
                WriteWarning("No attributes specified to update. Specify at least one attribute (ResourceId, Title, Description, Icon, Entity, or Url).");
                return;
            }

            // Update the sitemap
            var updateEntity = new Entity("sitemap", retrievedSitemapId);
            updateEntity["sitemapxml"] = doc.ToString();

            WriteVerbose("Updating sitemap in Dataverse...");
            Connection.Update(updateEntity);

            WriteVerbose($"{entryType.Value} entry '{entryId}' updated successfully.");
            WriteObject($"{entryType.Value} entry '{entryId}' in sitemap '{sitemapName}' updated successfully.");
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
