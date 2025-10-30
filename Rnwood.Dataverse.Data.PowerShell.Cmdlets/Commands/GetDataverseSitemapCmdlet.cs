using System;
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
        /// Gets or sets the unique identifier of the sitemap to retrieve.
        /// </summary>
        [Parameter(HelpMessage = "The unique identifier of the sitemap to retrieve.")]
        public Guid? Id { get; set; }

        /// <summary>
        /// Gets or sets the solution unique name to filter sitemaps by.
        /// </summary>
        [Parameter(HelpMessage = "Filter sitemaps by solution unique name.")]
        public string SolutionUniqueName { get; set; }

        /// <summary>
        /// Gets or sets the app unique name to filter sitemaps by.
        /// </summary>
        [Parameter(HelpMessage = "Filter sitemaps associated with a specific app unique name.")]
        public string AppUniqueName { get; set; }

        /// <summary>
        /// Gets or sets whether to filter for managed sitemaps only.
        /// </summary>
        [Parameter(HelpMessage = "Filter to return only managed sitemaps.")]
        public SwitchParameter Managed { get; set; }

        /// <summary>
        /// Gets or sets whether to filter for unmanaged sitemaps only.
        /// </summary>
        [Parameter(HelpMessage = "Filter to return only unmanaged sitemaps.")]
        public SwitchParameter Unmanaged { get; set; }

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
                    "sitemapxml",
                    "ismanaged",
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

            if (Managed.IsPresent && !Unmanaged.IsPresent)
            {
                query.Criteria.AddCondition("ismanaged", ConditionOperator.Equal, true);
                WriteVerbose("Filtering for managed sitemaps only");
            }
            else if (Unmanaged.IsPresent && !Managed.IsPresent)
            {
                query.Criteria.AddCondition("ismanaged", ConditionOperator.Equal, false);
                WriteVerbose("Filtering for unmanaged sitemaps only");
            }

            // Add link to solution for solution information
            if (!string.IsNullOrEmpty(SolutionUniqueName))
            {
                var solutionLink = query.AddLink("solution", "solutionid", "solutionid");
                solutionLink.EntityAlias = "solution";
                solutionLink.Columns = new ColumnSet("friendlyname", "uniquename");
                solutionLink.LinkCriteria.AddCondition("uniquename", ConditionOperator.Equal, SolutionUniqueName);
                WriteVerbose($"Filtering by solution unique name: {SolutionUniqueName}");
            }
            else
            {
                // Still link to solution to get solution name
                var solutionLink = query.AddLink("solution", "solutionid", "solutionid", JoinOperator.LeftOuter);
                solutionLink.EntityAlias = "solution";
                solutionLink.Columns = new ColumnSet("friendlyname", "uniquename");
            }

            // Add link to appmodule if filtering by app
            if (!string.IsNullOrEmpty(AppUniqueName))
            {
                var appLink = query.AddLink("appmodule", "sitemapid", "appmoduleid");
                appLink.EntityAlias = "app";
                appLink.Columns = new ColumnSet("uniquename", "name");
                appLink.LinkCriteria.AddCondition("uniquename", ConditionOperator.Equal, AppUniqueName);
                WriteVerbose($"Filtering by app unique name: {AppUniqueName}");
            }

            // Execute query
            var sitemaps = Connection.RetrieveMultiple(query);

            WriteVerbose($"Found {sitemaps.Entities.Count} sitemap(s)");

            // Convert to SitemapInfo objects
            foreach (var sitemap in sitemaps.Entities)
            {
                var sitemapInfo = new SitemapInfo
                {
                    Id = sitemap.Id,
                    Name = sitemap.GetAttributeValue<string>("sitemapname"),
                    SitemapXml = sitemap.GetAttributeValue<string>("sitemapxml"),
                    IsManaged = sitemap.GetAttributeValue<bool>("ismanaged"),
                    CreatedOn = sitemap.GetAttributeValue<DateTime?>("createdon"),
                    ModifiedOn = sitemap.GetAttributeValue<DateTime?>("modifiedon")
                };

                // Extract solution information from the linked entity
                if (sitemap.Contains("solution.friendlyname"))
                {
                    sitemapInfo.SolutionName = sitemap.GetAttributeValue<AliasedValue>("solution.friendlyname")?.Value as string;
                }

                // Extract app information from the linked entity if present
                if (sitemap.Contains("app.uniquename"))
                {
                    sitemapInfo.AppUniqueName = sitemap.GetAttributeValue<AliasedValue>("app.uniquename")?.Value as string;
                }

                WriteObject(sitemapInfo);
            }
        }
    }
}
