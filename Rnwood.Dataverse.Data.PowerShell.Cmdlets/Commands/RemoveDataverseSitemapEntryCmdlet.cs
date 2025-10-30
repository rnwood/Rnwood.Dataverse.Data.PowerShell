using System;
using System.Linq;
using System.Management.Automation;
using System.Xml.Linq;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Query;

namespace Rnwood.Dataverse.Data.PowerShell.Commands
{
    /// <summary>
    /// Removes an entry (Area, Group, or SubArea) from a Dataverse sitemap.
    /// </summary>
    [Cmdlet(VerbsCommon.Remove, "DataverseSitemapEntry", SupportsShouldProcess = true, ConfirmImpact = ConfirmImpact.High)]
    public class RemoveDataverseSitemapEntryCmdlet : OrganizationServiceCmdlet
    {
        /// <summary>
        /// Gets or sets the name of the sitemap containing the entry.
        /// </summary>
        [Parameter(Mandatory = true, Position = 0, ValueFromPipelineByPropertyName = true, HelpMessage = "The name of the sitemap containing the entry.")]
        [ValidateNotNullOrEmpty]
        public string SitemapName { get; set; }

        /// <summary>
        /// Gets or sets the ID of the sitemap containing the entry.
        /// </summary>
        [Parameter(ValueFromPipelineByPropertyName = true, HelpMessage = "The ID of the sitemap containing the entry.")]
        public Guid? SitemapId { get; set; }

        /// <summary>
        /// Gets or sets the type of entry to remove.
        /// </summary>
        [Parameter(Mandatory = true, HelpMessage = "The type of entry to remove (Area, Group, SubArea).")]
        public SitemapEntryType EntryType { get; set; }

        /// <summary>
        /// Gets or sets the ID of the entry to remove.
        /// </summary>
        [Parameter(Mandatory = true, ValueFromPipelineByPropertyName = true, HelpMessage = "The ID of the entry to remove.")]
        [ValidateNotNullOrEmpty]
        [Alias("Id")]
        public string EntryId { get; set; }

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
        /// Gets or sets whether to suppress errors if the entry does not exist.
        /// </summary>
        [Parameter(HelpMessage = "If specified, the cmdlet will not raise an error if the entry does not exist.")]
        public SwitchParameter IfExists { get; set; }

        /// <summary>
        /// Processes the cmdlet request.
        /// </summary>
        protected override void ProcessRecord()
        {
            base.ProcessRecord();

            if (!ShouldProcess($"Sitemap '{SitemapName}'", $"Remove {EntryType} entry '{EntryId}'"))
            {
                return;
            }

            WriteVerbose($"Retrieving sitemap '{SitemapName}'...");

            // Retrieve the sitemap
            var query = new QueryExpression("sitemap")
            {
                ColumnSet = new ColumnSet("sitemapid", "sitemapname", "sitemapxml", "ismanaged"),
                TopCount = 1
            };

            if (SitemapId.HasValue && SitemapId.Value != Guid.Empty)
            {
                query.Criteria.AddCondition("sitemapid", ConditionOperator.Equal, SitemapId.Value);
            }
            else
            {
                query.Criteria.AddCondition("sitemapname", ConditionOperator.Equal, SitemapName);
            }

            var sitemaps = Connection.RetrieveMultiple(query);

            if (sitemaps.Entities.Count == 0)
            {
                if (IfExists.IsPresent)
                {
                    WriteVerbose($"Sitemap '{SitemapName}' not found. Skipping removal.");
                    return;
                }

                ThrowTerminatingError(new ErrorRecord(
                    new InvalidOperationException($"Sitemap '{SitemapName}' not found."),
                    "SitemapNotFound",
                    ErrorCategory.ObjectNotFound,
                    SitemapName));
                return;
            }

            var sitemap = sitemaps.Entities[0];
            var sitemapId = sitemap.Id;
            var isManaged = sitemap.GetAttributeValue<bool>("ismanaged");

            if (isManaged)
            {
                ThrowTerminatingError(new ErrorRecord(
                    new InvalidOperationException("Cannot remove entries from a managed sitemap."),
                    "ManagedSitemapModificationNotAllowed",
                    ErrorCategory.InvalidOperation,
                    SitemapName));
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

            // Find and remove the entry
            XElement entryElement = null;
            switch (EntryType)
            {
                case SitemapEntryType.Area:
                    entryElement = FindElement(doc.Root, "Area", EntryId);
                    break;

                case SitemapEntryType.Group:
                    if (!string.IsNullOrEmpty(ParentAreaId))
                    {
                        var area = FindElement(doc.Root, "Area", ParentAreaId);
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
                    if (!string.IsNullOrEmpty(ParentAreaId) && !string.IsNullOrEmpty(ParentGroupId))
                    {
                        var area = FindElement(doc.Root, "Area", ParentAreaId);
                        if (area != null)
                        {
                            var group = FindElement(area, "Group", ParentGroupId);
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

            if (entryElement == null)
            {
                if (IfExists.IsPresent)
                {
                    WriteVerbose($"{EntryType} entry '{EntryId}' not found. Skipping removal.");
                    return;
                }

                ThrowTerminatingError(new ErrorRecord(
                    new InvalidOperationException($"{EntryType} entry '{EntryId}' not found in sitemap."),
                    "EntryNotFound",
                    ErrorCategory.ObjectNotFound,
                    EntryId));
                return;
            }

            // Remove the entry
            entryElement.Remove();
            WriteVerbose($"Removed {EntryType} entry '{EntryId}' from sitemap XML");

            // Update the sitemap
            var updateEntity = new Entity("sitemap", sitemapId);
            updateEntity["sitemapxml"] = doc.ToString();

            WriteVerbose("Updating sitemap in Dataverse...");
            Connection.Update(updateEntity);

            WriteVerbose($"{EntryType} entry '{EntryId}' removed successfully.");
            WriteObject($"{EntryType} entry '{EntryId}' removed from sitemap '{SitemapName}' successfully.");
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
