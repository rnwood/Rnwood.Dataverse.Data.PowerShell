using System;
using System.Management.Automation;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Query;
using Microsoft.Crm.Sdk.Messages;

namespace Rnwood.Dataverse.Data.PowerShell.Commands
{
    /// <summary>
    /// Creates or updates a sitemap in Dataverse.
    /// </summary>
    [Cmdlet(VerbsCommon.Set, "DataverseSitemap", SupportsShouldProcess = true, ConfirmImpact = ConfirmImpact.Medium)]
    [OutputType(typeof(Guid))]
    public class SetDataverseSitemapCmdlet : OrganizationServiceCmdlet
    {
        private const string DefaultSitemapXml = @"<SiteMap IntroducedVersion=""7.0.0.0""><Area Id=""area_f25442ad""
             ResourceId=""SitemapDesigner.NewTitle"" DescriptionResourceId=""SitemapDesigner.NewTitle""       
             ShowGroups=""true"" IntroducedVersion=""7.0.0.0""><Titles><Title LCID=""1033"" Title=""Area1""       
             /></Titles><Group Id=""group_3d08e7b2"" ResourceId=""SitemapDesigner.NewGroup""
             DescriptionResourceId=""SitemapDesigner.NewGroup"" IntroducedVersion=""7.0.0.0""
             IsProfile=""false"" ToolTipResourseId=""SitemapDesigner.Unknown""><SubArea
             Id=""subarea_6b3ec540"" Icon=""/_imgs/imagestrips/transparent_spacer.gif""
             Entity=""contact""
             Client=""All,Outlook,OutlookLaptopClient,OutlookWorkstationClient,Web""
             AvailableOffline=""true"" PassParams=""false"" Sku=""All,OnPremise,Live,SPLA"" 
             /></Group></Area></SiteMap>";
        /// <summary>
        /// Gets or sets the name property of the sitemap. Required when creating a new sitemap, optional when updating.
        /// </summary>
        [Parameter(Mandatory = false, Position = 0, HelpMessage = "The name property of the sitemap. Required when creating a new sitemap, optional when updating.", ValueFromPipelineByPropertyName = true)]
        public string Name { get; set; }

        /// <summary>
        /// Gets or sets the unique identifier (key) of the sitemap to update. If not specified along with UniqueName, a new sitemap is created.
        /// </summary>
        [Parameter(HelpMessage = "The unique identifier (key) of the sitemap to update. If not specified along with UniqueName, a new sitemap is created.", ValueFromPipelineByPropertyName = true)]
        public Guid? Id { get; set; }

        /// <summary>
        /// Gets or sets the unique name (key) of the sitemap to update. If a sitemap with this unique name exists, it will be updated; otherwise, a new sitemap is created with this unique name.
        /// </summary>
        [Parameter(HelpMessage = "The unique name (key) of the sitemap to update. If a sitemap with this unique name exists, it will be updated; otherwise, a new sitemap is created with this unique name.", ValueFromPipelineByPropertyName = true)]
        public string UniqueName { get; set; }

        /// <summary>
        /// Gets or sets the XML definition of the sitemap.
        /// </summary>
        [Parameter(HelpMessage = "The XML definition of the sitemap. If not specified, a default sitemap will be used.", ValueFromPipelineByPropertyName = true)]
        public string SitemapXml { get; set; }

        /// <summary>
        /// Gets or sets whether to return the sitemap ID after creation/update.
        /// </summary>
        [Parameter(HelpMessage = "If specified, the cmdlet returns the ID of the created or updated sitemap.")]
        public SwitchParameter PassThru { get; set; }

        /// <summary>
        /// If specified, publishes the sitemap after creating or updating.
        /// </summary>
        [Parameter(HelpMessage = "If specified, publishes the sitemap after creating or updating")]
        public SwitchParameter Publish { get; set; }

        /// <summary>
        /// Processes the cmdlet request.
        /// </summary>
        protected override void ProcessRecord()
        {
            base.ProcessRecord();

            Guid sitemapId = Guid.Empty;
            bool isUpdate = false;
            Entity existingSitemap = null;

            // Determine if we're updating or creating
            if (Id.HasValue)
            {
                // Update by ID
                isUpdate = true;
                sitemapId = Id.Value;
            }
            else if (!string.IsNullOrEmpty(UniqueName))
            {
                // Try to find existing sitemap by UniqueName
                var query = new QueryExpression("sitemap")
                {
                    ColumnSet = new ColumnSet("sitemapid", "sitemapname"),
                    Criteria = new FilterExpression
                    {
                        Conditions =
                        {
                            new ConditionExpression("sitemapnameunique", ConditionOperator.Equal, UniqueName)
                        }
                    },
                    TopCount = 1
                };

                // Try unpublished first
                var retrieveUnpublishedMultipleRequest = new RetrieveUnpublishedMultipleRequest
                {
                    Query = query
                };
                var unpublishedResponse = (RetrieveUnpublishedMultipleResponse)Connection.Execute(retrieveUnpublishedMultipleRequest);
                var results = unpublishedResponse.EntityCollection;
                
                if (results.Entities.Count == 0)
                {
                    // If not found in unpublished, try published
                    results = Connection.RetrieveMultiple(query);
                }

                if (results.Entities.Count > 0)
                {
                    // Found existing sitemap, update it
                    isUpdate = true;
                    existingSitemap = results.Entities[0];
                    sitemapId = existingSitemap.Id;
                }
                // If not found, we'll create a new one (isUpdate remains false)
            }

            if (isUpdate)
            {
                // Update existing sitemap
                string sitemapDescription = !string.IsNullOrEmpty(Name) ? $"Sitemap '{Name}' (ID: {sitemapId})" : $"Sitemap (ID: {sitemapId})";
                if (!ShouldProcess(sitemapDescription, "Update"))
                {
                    return;
                }

                WriteVerbose($"Checking if sitemap with ID {sitemapId} exists...");

                // If we don't have the existing sitemap yet (when updating by ID), fetch it
                if (existingSitemap == null)
                {
                    var existingQuery = new QueryExpression("sitemap")
                    {
                        ColumnSet = new ColumnSet("sitemapid", "sitemapname"),
                        Criteria = new FilterExpression
                        {
                            Conditions =
                            {
                                new ConditionExpression("sitemapid", ConditionOperator.Equal, sitemapId)
                            }
                        },
                        TopCount = 1
                    };

                    // Try unpublished first
                    var retrieveUnpublishedMultipleRequest = new RetrieveUnpublishedMultipleRequest
                    {
                        Query = existingQuery
                    };
                    var unpublishedResponse = (RetrieveUnpublishedMultipleResponse)Connection.Execute(retrieveUnpublishedMultipleRequest);
                    var existingSitemaps = unpublishedResponse.EntityCollection;

                    if (existingSitemaps.Entities.Count == 0)
                    {
                        // If not found in unpublished, try published
                        existingSitemaps = Connection.RetrieveMultiple(existingQuery);
                    }

                    if (existingSitemaps.Entities.Count == 0)
                    {
                        ThrowTerminatingError(new ErrorRecord(
                            new InvalidOperationException($"Sitemap with ID '{sitemapId}' not found."),
                            "SitemapNotFound",
                            ErrorCategory.ObjectNotFound,
                            sitemapId));
                        return;
                    }

                    existingSitemap = existingSitemaps.Entities[0];
                }

                WriteVerbose($"Updating sitemap...");

                var updateEntity = new Entity("sitemap", sitemapId);
                if (!string.IsNullOrEmpty(Name))
                {
                    updateEntity["sitemapname"] = Name;
                }
                if (!string.IsNullOrEmpty(UniqueName))
                {
                    updateEntity["sitemapnameunique"] = UniqueName;
                }
                if (!string.IsNullOrEmpty(SitemapXml))
                {
                    // Validate XML format before updating
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
                    updateEntity["sitemapxml"] = SitemapXml;
                }

                Connection.Update(updateEntity);

                WriteVerbose("Sitemap updated successfully.");
            }
            else
            {
                // Create new sitemap - Name is required
                if (string.IsNullOrEmpty(Name))
                {
                    ThrowTerminatingError(new ErrorRecord(
                        new ArgumentException("Name is required when creating a new sitemap."),
                        "NameRequired",
                        ErrorCategory.InvalidArgument,
                        null));
                    return;
                }

                if (!ShouldProcess($"Sitemap '{Name}'", "Create"))
                {
                    return;
                }

                WriteVerbose($"Creating new sitemap '{Name}'...");

                // Use default XML if not provided
                if (string.IsNullOrEmpty(SitemapXml))
                {
                    SitemapXml = DefaultSitemapXml;
                }

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

                var createEntity = new Entity("sitemap");
                createEntity["sitemapname"] = Name;
                if (!string.IsNullOrEmpty(UniqueName))
                {
                    createEntity["sitemapnameunique"] = UniqueName;
                }
                createEntity["sitemapxml"] = SitemapXml;

                sitemapId = Connection.Create(createEntity);

                WriteVerbose($"Sitemap created successfully with ID: {sitemapId}");
            }

            // Publish the sitemap if specified
            if (Publish && ShouldProcess($"Sitemap with ID '{sitemapId}'", "Publish"))
            {
                var publishRequest = new PublishXmlRequest
                {
                    ParameterXml = $"<importexportxml><sitemaps><sitemap>{sitemapId}</sitemap></sitemaps></importexportxml>"
                };
                Connection.Execute(publishRequest);
                WriteVerbose($"Published sitemap with ID: {sitemapId}");
            }

            if (PassThru.IsPresent)
            {
                WriteObject(sitemapId);
            }
        }
    }
}
