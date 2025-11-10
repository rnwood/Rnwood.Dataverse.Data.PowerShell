using System;
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
        [Parameter(HelpMessage = "Allows unpublished records to be retrieved instead of the default published")]
        public SwitchParameter Unpublished { get; set; }

        /// <summary>
        /// Processes the cmdlet request.
        /// </summary>
        protected override void ProcessRecord()
        {
            base.ProcessRecord();

            WriteVerbose("Querying sitemaps from Dataverse environment...");

            // Build query
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
            var sitemaps = QueryHelpers.ExecuteQueryWithPaging(query, Connection, WriteVerbose, Unpublished.IsPresent).ToList();

            WriteVerbose($"Found {sitemaps.Count} sitemap(s)");

            // Convert to SitemapInfo objects
            foreach (var sitemap in sitemaps)
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
