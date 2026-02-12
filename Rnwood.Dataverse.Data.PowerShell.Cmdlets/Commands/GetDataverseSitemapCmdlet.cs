using System;
using System.Collections.Generic;
using System.Linq;
using System.Management.Automation;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Query;

namespace Rnwood.Dataverse.Data.PowerShell.Commands
{
    /// <summary>
    /// Retrieves sitemap information from a Dataverse environment.
    /// </summary>
    [Cmdlet(VerbsCommon.Get, "DataverseSitemap")]
    [OutputType(typeof(SitemapInfo))]
    public class GetDataverseSitemapCmdlet : OrganizationServiceCmdlet
    {
        /// <summary>
        /// Gets or sets the name of the sitemap to retrieve.
        /// </summary>
        [Parameter(Position = 0, HelpMessage = "The name of the sitemap to retrieve. If not specified, all sitemaps are returned.")]
        public string Name { get; set; }

        /// <summary>
        /// Gets or sets the unique name of the sitemap to retrieve.
        /// </summary>
        [Parameter(HelpMessage = "The unique name of the sitemap to retrieve.")]
        public string UniqueName { get; set; }

        /// <summary>
        /// Gets or sets the unique identifier of the sitemap to retrieve.
        /// </summary>
        [Parameter(HelpMessage = "The unique identifier of the sitemap to retrieve.")]
        public Guid? Id { get; set; }

        /// <summary>
        /// Gets or sets whether to retrieve unpublished sitemaps instead of the default published ones.
        /// </summary>
        [Parameter(HelpMessage = "Allows published records to be retrieved instead of the default published")]
        public SwitchParameter Published { get; set; }

        /// <summary>
        /// Processes the cmdlet request.
        /// </summary>
        protected override void ProcessRecord()
        {
            base.ProcessRecord();

            WriteVerbose("Querying sitemaps from Dataverse environment...");

            // Optimization: When retrieving by ID or UniqueName and not forcing Published, use RetrieveUnpublishedRequest
            // for better support of unpublished sitemap XML (similar to forms)
            if (string.IsNullOrEmpty(Name))
            {
                Guid? sitemapId = Id;
                
                // If retrieving by UniqueName, first find the sitemap ID
                if (!sitemapId.HasValue && !string.IsNullOrEmpty(UniqueName))
                {
                    WriteVerbose($"Looking up sitemap by unique name: {UniqueName}");
                    var lookupQuery = new QueryExpression("sitemap")
                    {
                        ColumnSet = new ColumnSet("sitemapid"),
                        Criteria = new FilterExpression
                        {
                            Conditions =
                            {
                                new ConditionExpression("sitemapnameunique", ConditionOperator.Equal, UniqueName)
                            }
                        },
                        TopCount = 1
                    };
                    
                    EntityCollection results;
                    if (!Published.IsPresent)
                    {
                        var retrieveUnpublishedMultipleRequest = new Microsoft.Crm.Sdk.Messages.RetrieveUnpublishedMultipleRequest
                        {
                            Query = lookupQuery
                        };
                        var unpublishedResponse = (Microsoft.Crm.Sdk.Messages.RetrieveUnpublishedMultipleResponse)Connection.Execute(retrieveUnpublishedMultipleRequest);
                        results = unpublishedResponse.EntityCollection;
                        
                        if (results.Entities.Count == 0)
                        {
                            // Try published
                            results = Connection.RetrieveMultiple(lookupQuery);
                        }
                    }
                    else
                    {
                        results = Connection.RetrieveMultiple(lookupQuery);
                    }
                    
                    if (results.Entities.Count > 0)
                    {
                        sitemapId = results.Entities[0].Id;
                    }
                }
                
                // If we have a sitemap ID, retrieve it using RetrieveUnpublishedRequest
                if (sitemapId.HasValue)
                {
                Entity sitemap;
                
                if (!Published.IsPresent)
                {
                    WriteVerbose($"Retrieving unpublished sitemap by ID: {sitemapId.Value}");
                    try
                    {
                        var retrieveUnpublishedRequest = new Microsoft.Crm.Sdk.Messages.RetrieveUnpublishedRequest
                        {
                            Target = new EntityReference("sitemap", sitemapId.Value),
                            ColumnSet = new ColumnSet(true) // Use all columns to ensure sitemapxml is retrieved
                        };
                        var response = (Microsoft.Crm.Sdk.Messages.RetrieveUnpublishedResponse)Connection.Execute(retrieveUnpublishedRequest);
                        sitemap = response.Entity;
                    }
                    catch (System.ServiceModel.FaultException<Microsoft.Xrm.Sdk.OrganizationServiceFault> ex)
                    {
                        if (QueryHelpers.IsNotFoundException(ex))
                        {
                            // Try published version as fallback
                            WriteVerbose($"Sitemap not found in unpublished layer, trying published version...");
                            sitemap = Connection.Retrieve("sitemap", sitemapId.Value, new ColumnSet(true)); // Use all columns
                        }
                        else
                        {
                            throw;
                        }
                    }
                }
                else
                {
                    WriteVerbose($"Retrieving published sitemap by ID: {sitemapId.Value}");
                    sitemap = Connection.Retrieve("sitemap", sitemapId.Value, new ColumnSet(true)); // Use all columns
                }

                var sitemapInfo = new SitemapInfo
                {
                    Id = sitemap.Id,
                    Name = sitemap.GetAttributeValue<string>("sitemapname"),
                    UniqueName = sitemap.GetAttributeValue<string>("sitemapnameunique"),
                    SitemapXml = sitemap.GetAttributeValue<string>("sitemapxml"),
                    CreatedOn = sitemap.GetAttributeValue<DateTime?>("createdon"),
                    ModifiedOn = sitemap.GetAttributeValue<DateTime?>("modifiedon")
                };

                WriteObject(sitemapInfo);
                return;
                }
            }

            // Build query for other cases
            var query = new QueryExpression("sitemap")
            {
                ColumnSet = new ColumnSet(
                    "sitemapid",
                    "sitemapname",
                    "sitemapnameunique",
                    "sitemapxml",
                    "createdby",
                    "createdon",
                    "modifiedby",
                    "modifiedon",
                    "solutionid"
                )
            };

            // Add filters
            if (Id.HasValue)
            {
                query.Criteria.AddCondition("sitemapid", ConditionOperator.Equal, Id.Value);
                WriteVerbose($"Filtering by ID: {Id.Value}");
            }

            if (!string.IsNullOrEmpty(Name))
            {
                query.Criteria.AddCondition("sitemapname", ConditionOperator.Equal, Name);
                WriteVerbose($"Filtering by name: {Name}");
            }

            if (!string.IsNullOrEmpty(UniqueName))
            {
                query.Criteria.AddCondition("sitemapnameunique", ConditionOperator.Equal, UniqueName);
                WriteVerbose($"Filtering by unique name: {UniqueName}");
            }

            // Execute query with paging
            IEnumerable<Entity> sitemaps;
            if (!Published.IsPresent)
            {
                // Get both unpublished and published, with deduplication (unpublished preferred)
                sitemaps = QueryHelpers.ExecuteQueryWithPublishedAndUnpublished(query, Connection, WriteVerbose);
            }
            else
            {
                // Get only published records
                sitemaps = QueryHelpers.ExecuteQueryWithPaging(query, Connection, WriteVerbose);
            }

            var sitemapsList = sitemaps.ToList();
            WriteVerbose($"Found {sitemapsList.Count} sitemap(s)");

            // Convert to SitemapInfo objects
            foreach (var sitemap in sitemapsList)
            {
                var sitemapInfo = new SitemapInfo
                {
                    Id = sitemap.Id,
                    Name = sitemap.GetAttributeValue<string>("sitemapname"),
                    UniqueName = sitemap.GetAttributeValue<string>("sitemapnameunique"),
                    SitemapXml = sitemap.GetAttributeValue<string>("sitemapxml"),
                    CreatedOn = sitemap.GetAttributeValue<DateTime?>("createdon"),
                    ModifiedOn = sitemap.GetAttributeValue<DateTime?>("modifiedon")
                };

                WriteObject(sitemapInfo);
            }
        }
    }
}
