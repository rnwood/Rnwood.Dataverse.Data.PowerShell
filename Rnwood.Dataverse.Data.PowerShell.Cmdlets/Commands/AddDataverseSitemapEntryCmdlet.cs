using System;
using System.Linq;
using System.Management.Automation;
using System.Xml.Linq;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Query;

namespace Rnwood.Dataverse.Data.PowerShell.Commands
{
    /// <summary>
    /// Adds a new entry (Area, Group, or SubArea) to a Dataverse sitemap.
    /// </summary>
    [Cmdlet(VerbsCommon.Add, "DataverseSitemapEntry", SupportsShouldProcess = true, ConfirmImpact = ConfirmImpact.Medium)]
    [OutputType(typeof(SitemapEntryInfo))]
    public class AddDataverseSitemapEntryCmdlet : OrganizationServiceCmdlet
    {
        /// <summary>
        /// Gets or sets the sitemap object from pipeline.
        /// </summary>
        [Parameter(ValueFromPipeline = true, HelpMessage = "Sitemap object from Get-DataverseSitemap.")]
        public SitemapInfo Sitemap { get; set; }

        /// <summary>
        /// Gets or sets the name of the sitemap to add the entry to.
        /// </summary>
        [Parameter(Position = 0, ValueFromPipelineByPropertyName = true, HelpMessage = "The name of the sitemap to add the entry to.")]
        [Alias("Name")]
        public string SitemapName { get; set; }

        /// <summary>
        /// Gets or sets the ID of the sitemap to add the entry to.
        /// </summary>
        [Parameter(ValueFromPipelineByPropertyName = true, HelpMessage = "The ID of the sitemap to add the entry to.")]
        public Guid? SitemapId { get; set; }

        /// <summary>
        /// Gets or sets the type of entry to add.
        /// </summary>
        [Parameter(Mandatory = true, HelpMessage = "The type of entry to add (Area, Group, SubArea).")]
        public SitemapEntryType EntryType { get; set; }

        /// <summary>
        /// Gets or sets the ID for the new entry.
        /// </summary>
        [Parameter(Mandatory = true, HelpMessage = "The ID for the new entry.")]
        [ValidateNotNullOrEmpty]
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
        /// Gets or sets the parent Area ID (required for Groups and SubAreas).
        /// </summary>
        [Parameter(HelpMessage = "The parent Area ID (required for Groups and SubAreas).")]
        public string ParentAreaId { get; set; }

        /// <summary>
        /// Gets or sets the parent Group ID (required for SubAreas).
        /// </summary>
        [Parameter(HelpMessage = "The parent Group ID (required for SubAreas).")]
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
        /// Gets or sets whether to return the created entry.
        /// </summary>
        [Parameter(HelpMessage = "If specified, returns the created entry.")]
        public SwitchParameter PassThru { get; set; }

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

            // Validate parameters based on entry type
            if (EntryType == SitemapEntryType.Group || EntryType == SitemapEntryType.SubArea)
            {
                if (string.IsNullOrEmpty(ParentAreaId))
                {
                    ThrowTerminatingError(new ErrorRecord(
                        new ArgumentException("ParentAreaId is required for Group and SubArea entries."),
                        "MissingParentAreaId",
                        ErrorCategory.InvalidArgument,
                        null));
                    return;
                }
            }

            if (EntryType == SitemapEntryType.SubArea && string.IsNullOrEmpty(ParentGroupId))
            {
                ThrowTerminatingError(new ErrorRecord(
                    new ArgumentException("ParentGroupId is required for SubArea entries."),
                    "MissingParentGroupId",
                    ErrorCategory.InvalidArgument,
                    null));
                return;
            }

            if (!ShouldProcess($"Sitemap '{sitemapName ?? sitemapId.ToString()}'", $"Add {EntryType} entry '{EntryId}'"))
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
                    new InvalidOperationException("Cannot add entries to a managed sitemap."),
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

            // Create the new entry element
            var newElement = CreateEntryElement(doc);

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
                    var parentArea = FindElement(doc.Root, "Area", ParentAreaId);
                    if (parentArea != null)
                    {
                        parentArea.Add(newElement);
                        added = true;
                        WriteVerbose($"Added Group '{EntryId}' to Area '{ParentAreaId}'");
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
                    var area = FindElement(doc.Root, "Area", ParentAreaId);
                    if (area != null)
                    {
                        var parentGroup = FindElement(area, "Group", ParentGroupId);
                        if (parentGroup != null)
                        {
                            parentGroup.Add(newElement);
                            added = true;
                            WriteVerbose($"Added SubArea '{EntryId}' to Group '{ParentGroupId}' in Area '{ParentAreaId}'");
                        }
                        else
                        {
                            ThrowTerminatingError(new ErrorRecord(
                                new InvalidOperationException($"Parent Group '{ParentGroupId}' not found in Area '{ParentAreaId}'."),
                                "ParentGroupNotFound",
                                ErrorCategory.ObjectNotFound,
                                ParentGroupId));
                            return;
                        }
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

            // Update the sitemap
            var updateEntity = new Entity("sitemap", retrievedSitemapId);
            updateEntity["sitemapxml"] = doc.ToString();

            WriteVerbose("Updating sitemap in Dataverse...");
            Connection.Update(updateEntity);

            WriteVerbose($"{EntryType} entry '{EntryId}' added successfully.");
            WriteObject($"{EntryType} entry '{EntryId}' added to sitemap '{sitemapName ?? sitemapId.ToString()}' successfully.");

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
                    ParentAreaId = ParentAreaId,
                    ParentGroupId = ParentGroupId,
                    IsDefault = IsDefault.IsPresent ? true : (bool?)null,
                    Privilege = Privilege
                };
                WriteObject(entry);
            }
        }

        private XElement CreateEntryElement(XDocument doc)
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
