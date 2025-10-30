using System;
using System.Management.Automation;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Query;

namespace Rnwood.Dataverse.Data.PowerShell.Commands
{
    /// <summary>
    /// Creates or updates a sitemap in Dataverse.
    /// </summary>
    [Cmdlet(VerbsCommon.Set, "DataverseSitemap", SupportsShouldProcess = true, ConfirmImpact = ConfirmImpact.Medium)]
    [OutputType(typeof(Guid))]
    public class SetDataverseSitemapCmdlet : OrganizationServiceCmdlet
    {
        /// <summary>
        /// Gets or sets the name of the sitemap to create or update.
        /// </summary>
        [Parameter(Mandatory = true, Position = 0, HelpMessage = "The name of the sitemap to create or update.", ValueFromPipelineByPropertyName = true)]
        [ValidateNotNullOrEmpty]
        public string Name { get; set; }

        /// <summary>
        /// Gets or sets the unique identifier of the sitemap to update. If not specified, a new sitemap is created.
        /// </summary>
        [Parameter(HelpMessage = "The unique identifier of the sitemap to update. If not specified, a new sitemap is created.", ValueFromPipelineByPropertyName = true)]
        public Guid? Id { get; set; }

        /// <summary>
        /// Gets or sets the XML definition of the sitemap.
        /// </summary>
        [Parameter(Mandatory = true, HelpMessage = "The XML definition of the sitemap.", ValueFromPipelineByPropertyName = true)]
        [ValidateNotNullOrEmpty]
        public string SitemapXml { get; set; }

        /// <summary>
        /// Gets or sets whether to return the sitemap ID after creation/update.
        /// </summary>
        [Parameter(HelpMessage = "If specified, the cmdlet returns the ID of the created or updated sitemap.")]
        public SwitchParameter PassThru { get; set; }

        /// <summary>
        /// Processes the cmdlet request.
        /// </summary>
        protected override void ProcessRecord()
        {
            base.ProcessRecord();

            // Validate XML format
            try
            {
                System.Xml.Linq.XDocument.Parse(SitemapXml);
            }
            catch (System.Xml.XmlException ex)
            {
                ThrowTerminatingError(new ErrorRecord(
                    new ArgumentException($"Invalid XML format: {ex.Message}"),
                    "InvalidXmlFormat",
                    ErrorCategory.InvalidArgument,
                    SitemapXml));
                return;
            }

            Guid sitemapId;
            bool isUpdate = Id.HasValue && Id.Value != Guid.Empty;

            if (isUpdate)
            {
                // Update existing sitemap
                if (!ShouldProcess($"Sitemap '{Name}' (ID: {Id.Value})", "Update"))
                {
                    return;
                }

                WriteVerbose($"Checking if sitemap with ID {Id.Value} exists...");

                // Verify the sitemap exists
                var existingQuery = new QueryExpression("sitemap")
                {
                    ColumnSet = new ColumnSet("sitemapid", "sitemapname", "ismanaged"),
                    Criteria = new FilterExpression
                    {
                        Conditions =
                        {
                            new ConditionExpression("sitemapid", ConditionOperator.Equal, Id.Value)
                        }
                    },
                    TopCount = 1
                };

                var existingSitemaps = Connection.RetrieveMultiple(existingQuery);

                if (existingSitemaps.Entities.Count == 0)
                {
                    ThrowTerminatingError(new ErrorRecord(
                        new InvalidOperationException($"Sitemap with ID '{Id.Value}' not found."),
                        "SitemapNotFound",
                        ErrorCategory.ObjectNotFound,
                        Id.Value));
                    return;
                }

                var existingSitemap = existingSitemaps.Entities[0];
                var isManaged = existingSitemap.GetAttributeValue<bool>("ismanaged");

                if (isManaged)
                {
                    ThrowTerminatingError(new ErrorRecord(
                        new InvalidOperationException("Cannot update a managed sitemap. Only unmanaged sitemaps can be modified."),
                        "ManagedSitemapUpdateNotAllowed",
                        ErrorCategory.InvalidOperation,
                        Id.Value));
                    return;
                }

                WriteVerbose($"Updating sitemap '{Name}'...");

                var updateEntity = new Entity("sitemap", Id.Value);
                updateEntity["sitemapname"] = Name;
                updateEntity["sitemapxml"] = SitemapXml;

                Connection.Update(updateEntity);
                sitemapId = Id.Value;

                WriteVerbose("Sitemap updated successfully.");
                WriteObject($"Sitemap '{Name}' updated successfully.");
            }
            else
            {
                // Create new sitemap
                if (!ShouldProcess($"Sitemap '{Name}'", "Create"))
                {
                    return;
                }

                WriteVerbose($"Creating new sitemap '{Name}'...");

                var createEntity = new Entity("sitemap");
                createEntity["sitemapname"] = Name;
                createEntity["sitemapxml"] = SitemapXml;

                sitemapId = Connection.Create(createEntity);

                WriteVerbose($"Sitemap created successfully with ID: {sitemapId}");
                WriteObject($"Sitemap '{Name}' created successfully with ID: {sitemapId}");
            }

            if (PassThru.IsPresent)
            {
                WriteObject(sitemapId);
            }
        }
    }
}
