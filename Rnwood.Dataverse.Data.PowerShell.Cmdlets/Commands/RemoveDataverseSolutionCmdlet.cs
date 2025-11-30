using System;
using System.Management.Automation;
using System.Threading;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Query;
using Microsoft.Crm.Sdk.Messages;

namespace Rnwood.Dataverse.Data.PowerShell.Commands
{
    /// <summary>
    /// Removes (uninstalls) a solution from Dataverse.
    /// </summary>
    [Cmdlet(VerbsCommon.Remove, "DataverseSolution", SupportsShouldProcess = true, ConfirmImpact = ConfirmImpact.Medium)]
    public class RemoveDataverseSolutionCmdlet : OrganizationServiceCmdlet
    {
        /// <summary>
        /// Gets or sets the unique name of the solution to remove.
        /// </summary>
        [Parameter(Mandatory = true, Position = 0, HelpMessage = "The unique name of the solution to remove.", ValueFromPipelineByPropertyName =true)]
        [ValidateNotNullOrEmpty]
        public string UniqueName { get; set; }

        /// <summary>
        /// Gets or sets the polling interval in seconds for checking uninstall status. Default is 5 seconds.
        /// </summary>
        [Parameter(HelpMessage = "Polling interval in seconds for checking uninstall status. Default is 5.")]
        public int PollingIntervalSeconds { get; set; } = 5;

        /// <summary>
        /// Gets or sets the timeout in seconds for the uninstall operation. Default is 3600 seconds (1 hour).
        /// </summary>
        [Parameter(HelpMessage = "Timeout in seconds for the uninstall operation. Default is 3600 (1 hour).")]
        public int TimeoutSeconds { get; set; } = 3600;

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

            var solutions = QueryHelpers.RetrieveMultipleWithThrottlingRetry(Connection, query);

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
            WriteVerbose("Starting asynchronous uninstall of solution...");

            // Create progress record
            var progressRecord = new ProgressRecord(1, "Removing Solution", $"Removing solution '{friendlyName}'")
            {
                PercentComplete = 0
            };
            WriteProgress(progressRecord);

            // Execute asynchronous uninstall request
            var request = new UninstallSolutionAsyncRequest { SolutionUniqueName = UniqueName };
            var response = (UninstallSolutionAsyncResponse)QueryHelpers.ExecuteWithThrottlingRetry(Connection, request);
            var asyncOperationId = response.AsyncOperationId;

            WriteVerbose($"Uninstall request submitted. AsyncOperationId: {asyncOperationId}. Monitoring uninstall progress...");

            // Monitor the asynchronous operation
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
                        new TimeoutException($"Solution uninstall timed out after {TimeoutSeconds} seconds."),
                        "UninstallTimeout",
                        ErrorCategory.OperationTimeout,
                        UniqueName));
                    return;
                }

                // Check if stopping has been requested
                if (Stopping)
                {
                    progressRecord.RecordType = ProgressRecordType.Completed;
                    WriteProgress(progressRecord);
                    WriteWarning("Solution uninstall was stopped by user.");
                    return;
                }

                // Retrieve the async operation status
                var asyncOp = QueryHelpers.RetrieveWithThrottlingRetry(Connection, "asyncoperation", asyncOperationId, new ColumnSet("statecode", "statuscode", "message"));
                var stateCode = asyncOp.GetAttributeValue<OptionSetValue>("statecode")?.Value ?? -1;

                if (stateCode == 3) // Completed
                {
                    var statusCode = asyncOp.GetAttributeValue<OptionSetValue>("statuscode")?.Value ?? -1;
                    if (statusCode == 30) // Succeeded
                    {
                        progressRecord.PercentComplete = 100;
                        progressRecord.RecordType = ProgressRecordType.Completed;
                        WriteProgress(progressRecord);
                        WriteVerbose("Solution uninstalled successfully.");
                        WriteObject($"Solution '{friendlyName}' removed successfully.");
                        return;
                    }
                    else
                    {
                        // Failed or other status
                        var message = asyncOp.GetAttributeValue<string>("message") ?? "Unknown error during uninstall.";
                        progressRecord.RecordType = ProgressRecordType.Completed;
                        WriteProgress(progressRecord);
                        ThrowTerminatingError(new ErrorRecord(
                            new InvalidOperationException($"Solution uninstall failed: {message}"),
                            "UninstallFailed",
                            ErrorCategory.OperationStopped,
                            UniqueName));
                        return;
                    }
                }

                // Operation still in progress
                progressRecord.StatusDescription = "Uninstalling...";
                progressRecord.PercentComplete = 50;
                WriteProgress(progressRecord);

                WriteVerbose("Uninstall operation in progress. Waiting before checking again...");

                // Wait before polling again
                Thread.Sleep(pollingInterval);
            }
        }
    }
}
