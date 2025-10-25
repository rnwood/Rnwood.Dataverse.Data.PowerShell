using System;
using System.Management.Automation;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Query;

namespace Rnwood.Dataverse.Data.PowerShell.Commands
{
    /// <summary>
    /// Retrieves solution information from a Dataverse environment.
    /// </summary>
    [Cmdlet(VerbsCommon.Get, "DataverseSolution")]
    [OutputType(typeof(SolutionInfo))]
    public class GetDataverseSolutionCmdlet : OrganizationServiceCmdlet
    {
        /// <summary>
        /// Gets or sets the unique name of the solution to retrieve.
        /// </summary>
        [Parameter(Position = 0, HelpMessage = "The unique name of the solution to retrieve. If not specified, all solutions are returned.")]
        public string UniqueName { get; set; }

        /// <summary>
        /// Gets or sets whether to filter for managed solutions only.
        /// </summary>
        [Parameter(HelpMessage = "Filter to return only managed solutions.")]
        public SwitchParameter Managed { get; set; }

        /// <summary>
        /// Gets or sets whether to filter for unmanaged solutions only.
        /// </summary>
        [Parameter(HelpMessage = "Filter to return only unmanaged solutions.")]
        public SwitchParameter Unmanaged { get; set; }

        /// <summary>
        /// Gets or sets whether to exclude default system solutions.
        /// </summary>
        [Parameter(HelpMessage = "Exclude default system solutions (Default, Active, and Basic).")]
        public SwitchParameter ExcludeSystemSolutions { get; set; }

        /// <summary>
        /// Processes the cmdlet request.
        /// </summary>
        protected override void ProcessRecord()
        {
            base.ProcessRecord();

            WriteVerbose("Querying solutions from Dataverse environment...");

            // Build query
            var query = new QueryExpression("solution")
            {
                ColumnSet = new ColumnSet(
                    "solutionid",
                    "uniquename",
                    "friendlyname",
                    "version",
                    "ismanaged",
                    "description",
                    "publisherid"
                )
            };

            // Add filters
            if (!string.IsNullOrEmpty(UniqueName))
            {
                query.Criteria.AddCondition("uniquename", ConditionOperator.Equal, UniqueName);
                WriteVerbose($"Filtering by unique name: {UniqueName}");
            }

            if (Managed.IsPresent && !Unmanaged.IsPresent)
            {
                query.Criteria.AddCondition("ismanaged", ConditionOperator.Equal, true);
                WriteVerbose("Filtering for managed solutions only");
            }
            else if (Unmanaged.IsPresent && !Managed.IsPresent)
            {
                query.Criteria.AddCondition("ismanaged", ConditionOperator.Equal, false);
                WriteVerbose("Filtering for unmanaged solutions only");
            }

            if (ExcludeSystemSolutions.IsPresent)
            {
                query.Criteria.AddCondition("uniquename", ConditionOperator.NotIn, 
                    new[] { "Default", "Active", "Basic" });
                WriteVerbose("Excluding system solutions");
            }

            // Add link to publisher for publisher information
            var publisherLink = query.AddLink("publisher", "publisherid", "publisherid");
            publisherLink.EntityAlias = "publisher";
            publisherLink.Columns = new ColumnSet("friendlyname", "uniquename", "customizationprefix");

            // Execute query
            var solutions = Connection.RetrieveMultiple(query);

            WriteVerbose($"Found {solutions.Entities.Count} solution(s)");

            // Convert to SolutionInfo objects
            foreach (var solution in solutions.Entities)
            {
                var solutionInfo = new SolutionInfo
                {
                    Id = solution.Id,
                    UniqueName = solution.GetAttributeValue<string>("uniquename"),
                    Name = solution.GetAttributeValue<string>("friendlyname"),
                    Version = ParseVersion(solution.GetAttributeValue<string>("version")),
                    IsManaged = solution.GetAttributeValue<bool>("ismanaged"),
                    Description = solution.GetAttributeValue<string>("description")
                };

                // Extract publisher information from the linked entity
                if (solution.Contains("publisher.friendlyname"))
                {
                    solutionInfo.PublisherName = solution.GetAttributeValue<AliasedValue>("publisher.friendlyname")?.Value as string;
                }
                if (solution.Contains("publisher.uniquename"))
                {
                    solutionInfo.PublisherUniqueName = solution.GetAttributeValue<AliasedValue>("publisher.uniquename")?.Value as string;
                }
                if (solution.Contains("publisher.customizationprefix"))
                {
                    solutionInfo.PublisherPrefix = solution.GetAttributeValue<AliasedValue>("publisher.customizationprefix")?.Value as string;
                }

                WriteObject(solutionInfo);
            }
        }

        private Version ParseVersion(string versionString)
        {
            if (string.IsNullOrEmpty(versionString))
            {
                return null;
            }

            if (Version.TryParse(versionString, out var version))
            {
                return version;
            }

            return null;
        }
    }
}
