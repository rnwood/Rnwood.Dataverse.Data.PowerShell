using System;
using System.Management.Automation;
using System.Threading;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Query;

namespace Rnwood.Dataverse.Data.PowerShell.Commands
{
    /// <summary>
    /// Removes (uninstalls) a solution from Dataverse.
    /// </summary>
    [Cmdlet(VerbsCommon.Remove, "DataverseSolution", SupportsShouldProcess = true, ConfirmImpact = ConfirmImpact.High)]
    public class RemoveDataverseSolutionCmdlet : OrganizationServiceCmdlet
    {
        /// <summary>
        /// Gets or sets the unique name of the solution to remove.
        /// </summary>
        [Parameter(Mandatory = true, Position = 0, HelpMessage = "The unique name of the solution to remove.")]
        [ValidateNotNullOrEmpty]
        public string UniqueName { get; set; }

        /// <summary>
        /// Gets or sets the polling interval in seconds for checking deletion status. Default is 5 seconds.
        /// </summary>
        [Parameter(HelpMessage = "Polling interval in seconds for checking deletion status. Default is 5.")]
        public int PollingIntervalSeconds { get; set; } = 5;

        /// <summary>
        /// Gets or sets the timeout in seconds for the deletion operation. Default is 600 seconds (10 minutes).
        /// </summary>
        [Parameter(HelpMessage = "Timeout in seconds for the deletion operation. Default is 600 (10 minutes).")]
        public int TimeoutSeconds { get; set; } = 600;

        /// <summary>
        /// Processes the cmdlet request.
        /// </summary>
        protected override void ProcessRecord()
        {
            base.ProcessRecord();

            if (!ShouldProcess($"Solution '{UniqueName}'", "Remove"))
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
            WriteVerbose("Deleting solution...");

            // Create progress record
            var progressRecord = new ProgressRecord(1, "Removing Solution", $"Removing solution '{friendlyName}'")
            {
                PercentComplete = 0
            };
            WriteProgress(progressRecord);

            // Delete the solution
            Connection.Delete("solution", solutionId);

            WriteVerbose("Delete request submitted. Monitoring deletion progress...");

            // Monitor the deletion by checking if the solution still exists
            var startTime = DateTime.UtcNow;
            var timeout = TimeSpan.FromSeconds(TimeoutSeconds);
            var pollingInterval = TimeSpan.FromSeconds(PollingIntervalSeconds);

            while (true)
            {
                // Check for timeout
                if (DateTime.UtcNow - startTime > timeout)
                {
                    progressRecord.RecordType = ProgressRecordType.Completed;
                    WriteProgress(progressRecord);
                    ThrowTerminatingError(new ErrorRecord(
                        new TimeoutException($"Solution deletion timed out after {TimeoutSeconds} seconds."),
                        "DeletionTimeout",
                        ErrorCategory.OperationTimeout,
                        UniqueName));
                    return;
                }

                // Check if stopping has been requested
                if (Stopping)
                {
                    progressRecord.RecordType = ProgressRecordType.Completed;
                    WriteProgress(progressRecord);
                    WriteWarning("Solution deletion was stopped by user.");
                    return;
                }

                // Check if solution still exists
                var checkQuery = new QueryExpression("solution")
                {
                    ColumnSet = new ColumnSet("solutionid"),
                    Criteria = new FilterExpression
                    {
                        Conditions =
                        {
                            new ConditionExpression("solutionid", ConditionOperator.Equal, solutionId)
                        }
                    },
                    TopCount = 1
                };

                var checkResults = Connection.RetrieveMultiple(checkQuery);

                if (checkResults.Entities.Count == 0)
                {
                    // Solution has been deleted
                    progressRecord.PercentComplete = 100;
                    progressRecord.RecordType = ProgressRecordType.Completed;
                    WriteProgress(progressRecord);
                    WriteVerbose("Solution deleted successfully.");
                    WriteObject($"Solution '{friendlyName}' removed successfully.");
                    return;
                }

                // Solution still exists, update progress and continue polling
                progressRecord.StatusDescription = "Deleting...";
                progressRecord.PercentComplete = 50;
                WriteProgress(progressRecord);

                WriteVerbose("Solution still exists. Waiting before checking again...");

                // Wait before polling again
                Thread.Sleep(pollingInterval);
            }
        }
    }
}
