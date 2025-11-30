using System;
using System.Management.Automation;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Query;
using Microsoft.Crm.Sdk.Messages;

namespace Rnwood.Dataverse.Data.PowerShell.Commands
{
    /// <summary>
    /// Removes (deletes) a sitemap from Dataverse.
    /// </summary>
    [Cmdlet(VerbsCommon.Remove, "DataverseSitemap", SupportsShouldProcess = true, ConfirmImpact = ConfirmImpact.High)]
    public class RemoveDataverseSitemapCmdlet : OrganizationServiceCmdlet
    {
        /// <summary>
        /// Gets or sets the name of the sitemap to remove.
        /// </summary>
        [Parameter(Mandatory = true, Position = 0, ParameterSetName = "ByName", HelpMessage = "The name of the sitemap to remove.", ValueFromPipelineByPropertyName = true)]
        [ValidateNotNullOrEmpty]
        public string Name { get; set; }

        /// <summary>
        /// Gets or sets the unique identifier of the sitemap to remove.
        /// </summary>
        [Parameter(Mandatory = true, ParameterSetName = "ById", HelpMessage = "The unique identifier of the sitemap to remove.", ValueFromPipelineByPropertyName = true)]
        public Guid Id { get; set; }

        /// <summary>
        /// Gets or sets the unique name of the sitemap to remove.
        /// </summary>
        [Parameter(Mandatory = true, ParameterSetName = "ByUniqueName", HelpMessage = "The unique name of the sitemap to remove.", ValueFromPipelineByPropertyName = true)]
        [ValidateNotNullOrEmpty]
        public string UniqueName { get; set; }

        /// <summary>
        /// Gets or sets whether to suppress errors if the sitemap does not exist.
        /// </summary>
        [Parameter(HelpMessage = "If specified, the cmdlet will not raise an error if the sitemap does not exist.")]
        public SwitchParameter IfExists { get; set; }

        /// <summary>
        /// Processes the cmdlet request.
        /// </summary>
        protected override void ProcessRecord()
        {
            base.ProcessRecord();

            Guid sitemapId = Guid.Empty;
            string sitemapName = string.Empty;

            if (ParameterSetName == "ByName")
            {
                WriteVerbose($"Querying for sitemap with name '{Name}'...");

                // Query for the sitemap by name
                var query = new QueryExpression("sitemap")
                {
                    ColumnSet = new ColumnSet("sitemapid", "sitemapname"),
                    Criteria = new FilterExpression
                    {
                        Conditions =
                        {
                            new ConditionExpression("sitemapname", ConditionOperator.Equal, Name)
                        }
                    },
                    TopCount = 1
                };

                var sitemaps = QueryHelpers.RetrieveMultipleWithThrottlingRetry(Connection, query);

                if (sitemaps.Entities.Count == 0)
                {
                    // Check unpublished sitemaps
                    var retrieveUnpublishedMultipleRequest = new RetrieveUnpublishedMultipleRequest
                    {
                        Query = query
                    };
                    var unpublishedResponse = (RetrieveUnpublishedMultipleResponse)QueryHelpers.ExecuteWithThrottlingRetry(Connection, retrieveUnpublishedMultipleRequest);
                    sitemaps = unpublishedResponse.EntityCollection;
                }

                if (sitemaps.Entities.Count == 0)
                {
                    if (IfExists.IsPresent)
                    {
                        WriteVerbose($"Sitemap '{Name}' not found. Skipping deletion.");
                        return;
                    }

                    ThrowTerminatingError(new ErrorRecord(
                        new InvalidOperationException($"Sitemap '{Name}' not found."),
                        "SitemapNotFound",
                        ErrorCategory.ObjectNotFound,
                        Name));
                    return;
                }

                var sitemap = sitemaps.Entities[0];
                sitemapId = sitemap.Id;
                sitemapName = sitemap.GetAttributeValue<string>("sitemapname");
            }
            else if (ParameterSetName == "ByUniqueName")
            {
                WriteVerbose($"Querying for sitemap with unique name '{UniqueName}'...");

                // Query for the sitemap by unique name
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

                var sitemaps = QueryHelpers.RetrieveMultipleWithThrottlingRetry(Connection, query);

                if (sitemaps.Entities.Count == 0)
                {
                    // Check unpublished sitemaps
                    var retrieveUnpublishedMultipleRequest = new RetrieveUnpublishedMultipleRequest
                    {
                        Query = query
                    };
                    var unpublishedResponse = (RetrieveUnpublishedMultipleResponse)QueryHelpers.ExecuteWithThrottlingRetry(Connection, retrieveUnpublishedMultipleRequest);
                    sitemaps = unpublishedResponse.EntityCollection;
                }

                if (sitemaps.Entities.Count == 0)
                {
                    if (IfExists.IsPresent)
                    {
                        WriteVerbose($"Sitemap with unique name '{UniqueName}' not found. Skipping deletion.");
                        return;
                    }

                    ThrowTerminatingError(new ErrorRecord(
                        new InvalidOperationException($"Sitemap with unique name '{UniqueName}' not found."),
                        "SitemapNotFound",
                        ErrorCategory.ObjectNotFound,
                        UniqueName));
                    return;
                }

                var sitemap = sitemaps.Entities[0];
                sitemapId = sitemap.Id;
                sitemapName = sitemap.GetAttributeValue<string>("sitemapname");
            }
            else // ById
            {
                WriteVerbose($"Checking if sitemap with ID {Id} exists...");

                // Query to check if it exists and get info
                var query = new QueryExpression("sitemap")
                {
                    ColumnSet = new ColumnSet("sitemapid", "sitemapname"),
                    Criteria = new FilterExpression
                    {
                        Conditions =
                        {
                            new ConditionExpression("sitemapid", ConditionOperator.Equal, Id)
                        }
                    },
                    TopCount = 1
                };

                var sitemaps = QueryHelpers.RetrieveMultipleWithThrottlingRetry(Connection, query);

                if (sitemaps.Entities.Count == 0)
                {
                    // Check unpublished sitemaps
                    var retrieveUnpublishedMultipleRequest = new RetrieveUnpublishedMultipleRequest
                    {
                        Query = query
                    };
                    var unpublishedResponse = (RetrieveUnpublishedMultipleResponse)QueryHelpers.ExecuteWithThrottlingRetry(Connection, retrieveUnpublishedMultipleRequest);
                    sitemaps = unpublishedResponse.EntityCollection;
                }

                if (sitemaps.Entities.Count == 0)
                {
                    if (IfExists.IsPresent)
                    {
                        WriteVerbose($"Sitemap with ID {Id} not found. Skipping deletion.");
                        return;
                    }

                    ThrowTerminatingError(new ErrorRecord(
                        new InvalidOperationException($"Sitemap with ID '{Id}' not found."),
                        "SitemapNotFound",
                        ErrorCategory.ObjectNotFound,
                        Id));
                    return;
                }

                var sitemap = sitemaps.Entities[0];
                sitemapId = Id;
                sitemapName = sitemap.GetAttributeValue<string>("sitemapname");
            }

            if (!ShouldProcess($"Sitemap '{sitemapName}' (ID: {sitemapId})", "Delete"))
            {
                return;
            }

            WriteVerbose($"Deleting sitemap '{sitemapName}' (ID: {sitemapId})...");
            QueryHelpers.DeleteWithThrottlingRetry(Connection, "sitemap", sitemapId);

            WriteVerbose("Sitemap deleted successfully.");
            WriteObject($"Sitemap '{sitemapName}' deleted successfully.");
        }
    }
}
