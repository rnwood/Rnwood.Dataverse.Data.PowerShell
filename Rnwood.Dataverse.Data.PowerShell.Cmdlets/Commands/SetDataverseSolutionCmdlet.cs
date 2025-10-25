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
        [Parameter(Mandatory = true, Position = 0, HelpMessage = "The unique name of the solution to update.")]
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

            // Query for the solution
            var query = new QueryExpression("solution")
            {
                ColumnSet = new ColumnSet("solutionid", "uniquename", "friendlyname", "ismanaged"),
                Criteria = new FilterExpression
                {
                    Conditions =
                    {
                        new ConditionExpression("uniquename", ConditionOperator.Equal, UniqueName)
                    }
                },
                TopCount = 1
            };

            var solutions = Connection.RetrieveMultiple(query);

            if (solutions.Entities.Count == 0)
            {
                ThrowTerminatingError(new ErrorRecord(
                    new InvalidOperationException($"Solution '{UniqueName}' not found."),
                    "SolutionNotFound",
                    ErrorCategory.ObjectNotFound,
                    UniqueName));
                return;
            }

            var solution = solutions.Entities[0];
            var solutionId = solution.Id;
            var friendlyName = solution.GetAttributeValue<string>("friendlyname");
            var isManaged = solution.GetAttributeValue<bool>("ismanaged");

            WriteVerbose($"Found solution: {friendlyName} (ID: {solutionId}, Managed: {isManaged})");

            if (isManaged)
            {
                WriteWarning("Solution is managed. Only the description can be updated for managed solutions.");
            }

            // Create update entity
            var updateEntity = new Entity("solution", solutionId);
            bool hasUpdates = false;

            // Update friendly name (only for unmanaged solutions)
            if (!string.IsNullOrEmpty(Name))
            {
                if (isManaged)
                {
                    WriteWarning("Cannot update name of managed solution. Skipping name update.");
                }
                else
                {
                    updateEntity["friendlyname"] = Name;
                    hasUpdates = true;
                    WriteVerbose($"Setting friendly name to: {Name}");
                }
            }

            // Update description (allowed for both managed and unmanaged)
            if (!string.IsNullOrEmpty(Description))
            {
                updateEntity["description"] = Description;
                hasUpdates = true;
                WriteVerbose($"Setting description to: {Description}");
            }

            // Update version (only for unmanaged solutions)
            if (!string.IsNullOrEmpty(Version))
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

            if (!hasUpdates)
            {
                WriteWarning("No updates to apply. Please specify at least one property to update (Name, Description, or Version).");
                return;
            }

            WriteVerbose("Updating solution...");
            Connection.Update(updateEntity);

            WriteVerbose("Solution updated successfully.");
            WriteObject($"Solution '{UniqueName}' updated successfully.");
        }
    }
}
