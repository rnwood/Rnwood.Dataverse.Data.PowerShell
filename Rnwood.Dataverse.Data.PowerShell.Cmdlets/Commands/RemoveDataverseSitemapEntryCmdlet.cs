using System;
using System.Linq;
using System.Management.Automation;
using System.Xml.Linq;
using Microsoft.Crm.Sdk.Messages;
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
        /// Gets or sets a value indicating whether to remove an Area entry.
        /// </summary>
        [Parameter(ParameterSetName = "Area", Mandatory = true, HelpMessage = "Remove an Area entry.")]
        public SwitchParameter Area { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether to remove a Group entry.
        /// </summary>
        [Parameter(ParameterSetName = "Group", Mandatory = true, HelpMessage = "Remove a Group entry.")]
        public SwitchParameter Group { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether to remove a SubArea entry.
        /// </summary>
        [Parameter(ParameterSetName = "SubArea", Mandatory = true, HelpMessage = "Remove a SubArea entry.")]
        public SwitchParameter SubArea { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether to remove a Privilege entry.
        /// </summary>
        [Parameter(ParameterSetName = "Privilege", Mandatory = true, HelpMessage = "Remove a Privilege entry.")]
        public SwitchParameter Privilege { get; set; }

        /// <summary>
        /// Gets or sets the ID of the entry to remove.
        /// </summary>
        [Parameter(ParameterSetName = "Area", Mandatory = true, ValueFromPipelineByPropertyName = true, HelpMessage = "The ID of the entry to remove.")]
        [Parameter(ParameterSetName = "Group", Mandatory = true, ValueFromPipelineByPropertyName = true, HelpMessage = "The ID of the entry to remove.")]
        [Parameter(ParameterSetName = "SubArea", Mandatory = true, ValueFromPipelineByPropertyName = true, HelpMessage = "The ID of the entry to remove.")]
        [ValidateNotNullOrEmpty]
        [Alias("Id")]
        public string EntryId { get; set; }

        /// <summary>
        /// Gets or sets the entity name for privilege entries.
        /// </summary>
        [Parameter(ParameterSetName = "Privilege", Mandatory = true, ValueFromPipelineByPropertyName = true, HelpMessage = "The entity name for the privilege to remove.")]
        [ValidateNotNullOrEmpty]
        public string PrivilegeEntity { get; set; }

        /// <summary>
        /// Gets or sets the privilege name for privilege entries.
        /// </summary>
        [Parameter(ParameterSetName = "Privilege", Mandatory = true, ValueFromPipelineByPropertyName = true, HelpMessage = "The privilege name to remove.")]
        [ValidateNotNullOrEmpty]
        public string PrivilegeName { get; set; }

        /// <summary>
        /// Gets or sets the parent SubArea ID for privilege entries.
        /// </summary>
        [Parameter(ParameterSetName = "Privilege", Mandatory = true, ValueFromPipelineByPropertyName = true, HelpMessage = "The parent SubArea ID containing the privilege.")]
        [ValidateNotNullOrEmpty]
        public string ParentSubAreaId { get; set; }

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

            // Get values from InputObject if provided
            string sitemapUniqueName = SitemapUniqueName;
            Guid? sitemapId = SitemapId;
            SitemapEntryType? entryType = null;
            string entryId = EntryId;

            // Determine entry type from parameter set
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

            if (InputObject != null)
            {
                if (!entryType.HasValue)
                    entryType = InputObject.EntryType;
                if (string.IsNullOrEmpty(entryId))
                    entryId = InputObject.Id;
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

            if (!entryType.HasValue)
            {
                ThrowTerminatingError(new ErrorRecord(
                    new ArgumentException("EntryType could not be determined from parameters or InputObject."),
                    "MissingEntryType",
                    ErrorCategory.InvalidArgument,
                    null));
                return;
            }

            if (string.IsNullOrEmpty(entryId) && entryType != SitemapEntryType.Privilege)
            {
                ThrowTerminatingError(new ErrorRecord(
                    new ArgumentException("entryId is required. Provide it directly or via InputObject."),
                    "MissingEntryId",
                    ErrorCategory.InvalidArgument,
                    null));
                return;
            }

            string targetDescription = entryType.Value == SitemapEntryType.Privilege
                ? $"Remove {entryType.Value} '{PrivilegeEntity}.{PrivilegeName}' from SubArea '{ParentSubAreaId}'"
                : $"Remove {entryType.Value} entry '{entryId}'";

            if (!ShouldProcess($"Sitemap '{sitemapUniqueName ?? sitemapId.ToString()}'", targetDescription))
            {
                return;
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
                var response = (RetrieveUnpublishedMultipleResponse)QueryHelpers.ExecuteWithThrottlingRetry(Connection, request);
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
                var sitemaps = QueryHelpers.RetrieveMultipleWithThrottlingRetry(Connection, query);
                if (sitemaps.Entities.Count > 0)
                {
                    sitemap = sitemaps.Entities[0];
                    WriteVerbose("Found sitemap in published records");
                }
            }

            if (sitemap == null)
            {
                if (IfExists.IsPresent)
                {
                    WriteVerbose($"Sitemap '{sitemapUniqueName ?? sitemapId.ToString()}' not found. Skipping removal.");
                    return;
                }

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

            // Find and remove the entry
            XElement entryElement = null;
            switch (entryType.Value)
            {
                case SitemapEntryType.Area:
                case SitemapEntryType.Group:
                case SitemapEntryType.SubArea:
                    entryElement = FindElement(doc.Root, entryType.Value.ToString(), entryId);
                    break;

                case SitemapEntryType.Privilege:
                    // Find the SubArea first
                    var subArea = FindElement(doc.Root, "SubArea", ParentSubAreaId);
                    if (subArea != null)
                    {
                        // Find the Privilege element with matching Entity and Privilege attributes
                        entryElement = subArea.Elements("Privilege")
                            .FirstOrDefault(p => p.Attribute("Entity")?.Value == PrivilegeEntity && p.Attribute("Privilege")?.Value == PrivilegeName);
                    }
                    break;
            }

            if (entryElement == null)
            {
                string notFoundMessage = entryType.Value == SitemapEntryType.Privilege
                    ? $"{entryType} '{PrivilegeEntity}.{PrivilegeName}' not found in SubArea '{ParentSubAreaId}'"
                    : $"{entryType} entry '{entryId}' not found in sitemap.";

                if (IfExists.IsPresent)
                {
                    WriteVerbose($"{notFoundMessage}. Skipping removal.");
                    return;
                }

                ThrowTerminatingError(new ErrorRecord(
                    new InvalidOperationException(notFoundMessage),
                    "EntryNotFound",
                    ErrorCategory.ObjectNotFound,
                    entryType.Value == SitemapEntryType.Privilege ? $"{PrivilegeEntity}.{PrivilegeName}" : entryId));
                return;
            }

            // Remove the entry
            entryElement.Remove();
            string removedDescription = entryType.Value == SitemapEntryType.Privilege
                ? $"{entryType.Value} '{PrivilegeEntity}.{PrivilegeName}'"
                : $"{entryType.Value} entry '{entryId}'";
            WriteVerbose($"{removedDescription} removed from sitemap XML");

            // Update the sitemap
            var updateEntity = new Entity("sitemap", retrievedSitemapId);
            updateEntity["sitemapxml"] = doc.ToString();

            WriteVerbose("Updating sitemap in Dataverse...");
            QueryHelpers.UpdateWithThrottlingRetry(Connection, updateEntity);

            WriteVerbose($"{removedDescription} removed successfully.");
            string successMessage = entryType.Value == SitemapEntryType.Privilege
                ? $"{entryType.Value} '{PrivilegeEntity}.{PrivilegeName}' removed from SubArea '{ParentSubAreaId}' in sitemap '{sitemapUniqueName ?? sitemapId.ToString()}' successfully."
                : $"{entryType.Value} entry '{entryId}' removed from sitemap '{sitemapUniqueName ?? sitemapId.ToString()}' successfully.";
            WriteObject(successMessage);
        }

        private XElement FindElement(XElement parent, string elementName, string id)
        {
            if (parent == null)
                return null;

            return parent.Descendants(elementName)
                .FirstOrDefault(e => e.Attribute("Id")?.Value == id);
        }
    }
}
