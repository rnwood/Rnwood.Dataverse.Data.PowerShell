using System;
using System.Management.Automation;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Query;

namespace Rnwood.Dataverse.Data.PowerShell.Commands
{
    /// <summary>
    /// Updates properties of a solution in Dataverse.
    /// </summary>
    [Cmdlet(VerbsCommon.Set, "DataverseSolution", SupportsShouldProcess = true, ConfirmImpact = ConfirmImpact.Medium)]
    public class SetDataverseSolutionCmdlet : OrganizationServiceCmdlet
    {
        /// <summary>
        /// Gets or sets the unique name of the solution to update.
        /// </summary>
        [Parameter(Mandatory = true, Position = 0, HelpMessage = "The unique name of the solution to update.", ValueFromPipelineByPropertyName =true)]
        [ValidateNotNullOrEmpty]
        public string UniqueName { get; set; }

        /// <summary>
        /// Gets or sets the new friendly name for the solution.
        /// </summary>
        [Parameter(HelpMessage = "The new friendly name for the solution.")]
        public string Name { get; set; }

        /// <summary>
        /// Gets or sets the new description for the solution.
        /// </summary>
        [Parameter(HelpMessage = "The new description for the solution.")]
        public string Description { get; set; }

        /// <summary>
        /// Gets or sets the new version for the solution.
        /// </summary>
        [Parameter(HelpMessage = "The new version for the solution (e.g., '1.0.0.0').")]
        public string Version { get; set; }

        /// <summary>
        /// Gets or sets the unique name of the new publisher for the solution.
        /// </summary>
        [Parameter(HelpMessage = "The unique name of the new publisher for the solution.")]
        public string PublisherUniqueName { get; set; }

        /// <summary>
        /// Processes the cmdlet request.
        /// </summary>
        protected override void ProcessRecord()
        {
            base.ProcessRecord();

            if (!ShouldProcess($"Solution '{UniqueName}'", "Update"))
            {
                return;
            }

            WriteVerbose($"Querying for solution '{UniqueName}'...");

            // Query for the solution with publisher link
            var query = new QueryExpression("solution")
            {
                ColumnSet = new ColumnSet(true),
                Criteria = new FilterExpression
                {
                    Conditions =
                    {
                        new ConditionExpression("uniquename", ConditionOperator.Equal, UniqueName)
                    }
                },
                TopCount =1
            };

            // Link to publisher to get publisher.uniquename
            var publisherLink = new LinkEntity("solution", "publisher", "publisherid", "publisherid", JoinOperator.Inner)
            {
                Columns = new ColumnSet("uniquename"),
                EntityAlias = "publisher"
            };
            query.LinkEntities.Add(publisherLink);

            var solutions = QueryHelpers.RetrieveMultipleWithThrottlingRetry(Connection, query);

            if (solutions.Entities.Count ==0)
            {
                // Create new solution
                WriteVerbose($"Solution '{UniqueName}' not found. Creating new solution...");

                var createEntity = new Entity("solution");
                createEntity["uniquename"] = UniqueName;
                createEntity["friendlyname"] = !string.IsNullOrEmpty(Name) ? Name : UniqueName;
                if (!string.IsNullOrEmpty(Description))
                {
                    createEntity["description"] = Description;
                }
                createEntity["version"] = !string.IsNullOrEmpty(Version) ? Version : "1.0.0.0";

                if (!string.IsNullOrEmpty(PublisherUniqueName))
                {
                    WriteVerbose($"Querying for publisher '{PublisherUniqueName}'...");

                    // Query for the publisher
                    var publisherQuery = new QueryExpression("publisher")
                    {
                        ColumnSet = new ColumnSet("publisherid"),
                        Criteria = new FilterExpression
                        {
                            Conditions =
                            {
                                new ConditionExpression("uniquename", ConditionOperator.Equal, PublisherUniqueName)
                            }
                        },
                        TopCount = 1
                    };

                    var publishers = QueryHelpers.RetrieveMultipleWithThrottlingRetry(Connection, publisherQuery);

                    if (publishers.Entities.Count == 0)
                    {
                        ThrowTerminatingError(new ErrorRecord(
                            new InvalidOperationException($"Publisher '{PublisherUniqueName}' not found."),
                            "PublisherNotFound",
                            ErrorCategory.ObjectNotFound,
                            PublisherUniqueName));
                        return;
                    }

                    var publisherId = publishers.Entities[0].Id;
                    createEntity["publisherid"] = new EntityReference("publisher", publisherId);
                }
                else
                {
                    ThrowTerminatingError(new ErrorRecord(
                        new InvalidOperationException("PublisherUniqueName is required when creating a new solution."),
                        "PublisherRequired",
                        ErrorCategory.InvalidArgument,
                        null));
                    return;
                }

                // Validate version if provided
                if (!string.IsNullOrEmpty(Version) && !System.Version.TryParse(Version, out var _))
                {
                    ThrowTerminatingError(new ErrorRecord(
                        new ArgumentException($"Invalid version format: {Version}. Expected format: 'major.minor.build.revision' (e.g., '1.0.0.0')"),
                        "InvalidVersionFormat",
                        ErrorCategory.InvalidArgument,
                        Version));
                    return;
                }

                WriteVerbose("Creating solution...");
                QueryHelpers.CreateWithThrottlingRetry(Connection, createEntity);
                WriteVerbose("Solution created successfully.");
                WriteObject($"Solution '{UniqueName}' created successfully.");
                return;
            }

            var solution = solutions.Entities[0];
            var solutionId = solution.Id;
            var friendlyName = solution.GetAttributeValue<string>("friendlyname");
            var isManaged = solution.GetAttributeValue<bool>("ismanaged");
            var description = solution.GetAttributeValue<string>("description");
            var version = solution.GetAttributeValue<string>("version");

            // Retrieve current publisher unique name from linked entity
            string currentPublisherUniqueName = null;
            if (solution.Contains("publisher.uniquename"))
            {
                currentPublisherUniqueName = solution.GetAttributeValue<AliasedValue>("publisher.uniquename")?.Value as string;
            }

            WriteVerbose($"Found solution: {friendlyName} (ID: {solutionId}, Managed: {isManaged})");

            // Create update entity
            var updateEntity = new Entity("solution", solutionId);
            bool hasUpdates = false;

            // Update friendly name (only for unmanaged solutions)
            if (!string.IsNullOrEmpty(Name) && Name != friendlyName)
            {
                updateEntity["friendlyname"] = Name;
                hasUpdates = true;
                WriteVerbose($"Setting friendly name to: {Name}");
            }

            // Update description (allowed for both managed and unmanaged)
            if (!string.IsNullOrEmpty(Description) && Description != description)
            {
                updateEntity["description"] = Description;
                hasUpdates = true;
                WriteVerbose($"Setting description to: {Description}");
            }

            // Update version (only for unmanaged solutions)
            if (!string.IsNullOrEmpty(Version) && Version != version)
            {
                if (isManaged)
                {
                    WriteWarning("Cannot update version of managed solution. Skipping version update.");
                }
                else
                {
                    // Validate version format
                    if (System.Version.TryParse(Version, out var _))
                    {
                        updateEntity["version"] = Version;
                        hasUpdates = true;
                        WriteVerbose($"Setting version to: {Version}");
                    }
                    else
                    {
                        ThrowTerminatingError(new ErrorRecord(
                            new ArgumentException($"Invalid version format: {Version}. Expected format: 'major.minor.build.revision' (e.g., '1.0.0.0')"),
                            "InvalidVersionFormat",
                            ErrorCategory.InvalidArgument,
                            Version));
                        return;
                    }
                }
            }

            // Update publisher (only for unmanaged solutions and if changed)
            if (!string.IsNullOrEmpty(PublisherUniqueName))
            {
                if (isManaged)
                {
                    WriteWarning("Cannot update publisher of managed solution. Skipping publisher update.");
                }
                else if (PublisherUniqueName != currentPublisherUniqueName)
                {
                    WriteVerbose($"Querying for publisher '{PublisherUniqueName}'...");

                    // Query for the publisher
                    var publisherQuery = new QueryExpression("publisher")
                    {
                        ColumnSet = new ColumnSet("publisherid"),
                        Criteria = new FilterExpression
                        {
                            Conditions =
                            {
                                new ConditionExpression("uniquename", ConditionOperator.Equal, PublisherUniqueName)
                            }
                        },
                        TopCount =1
                    };

                    var publishers = QueryHelpers.RetrieveMultipleWithThrottlingRetry(Connection, publisherQuery);

                    if (publishers.Entities.Count ==0)
                    {
                        ThrowTerminatingError(new ErrorRecord(
                            new InvalidOperationException($"Publisher '{PublisherUniqueName}' not found."),
                            "PublisherNotFound",
                            ErrorCategory.ObjectNotFound,
                            PublisherUniqueName));
                        return;
                    }

                    var publisherId = publishers.Entities[0].Id;
                    updateEntity["publisherid"] = new EntityReference("publisher", publisherId);
                    hasUpdates = true;
                    WriteVerbose($"Setting publisher to: {PublisherUniqueName}");
                }
                else
                {
                    WriteVerbose($"Publisher unique name is already '{PublisherUniqueName}'. Skipping publisher update.");
                }
            }

            if (!hasUpdates)
            {
                WriteWarning("No updates to apply. Please specify at least one property to update (Name, Description, Version, or PublisherUniqueName).");
                return;
            }

            WriteVerbose("Updating solution...");
            QueryHelpers.UpdateWithThrottlingRetry(Connection, updateEntity);

            WriteVerbose("Solution updated successfully.");
            WriteObject($"Solution '{UniqueName}' updated successfully.");
        }
    }
}
