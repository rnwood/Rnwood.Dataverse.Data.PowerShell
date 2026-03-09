using System;
using System.Management.Automation;
using System.Threading;
using Microsoft.Crm.Sdk.Messages;
using Microsoft.PowerPlatform.Dataverse.Client;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Messages;
using Microsoft.Xrm.Sdk.Query;

namespace Rnwood.Dataverse.Data.PowerShell.Commands
{
    /// <summary>
    /// Applies a staged solution upgrade by deleting the original solution and promoting the holding solution.
    /// </summary>
    [Cmdlet(VerbsLifecycle.Invoke, "DataverseSolutionUpgrade", SupportsShouldProcess = true, ConfirmImpact = ConfirmImpact.Medium)]
    public class InvokeDataverseSolutionUpgradeCmdlet : OrganizationServiceCmdlet
    {
        /// <summary>
        /// Gets or sets the unique name of the solution to upgrade.
        /// </summary>
        [Parameter(Mandatory = true, Position = 0, HelpMessage = "The unique name of the solution to upgrade (e.g., 'MySolution'). The holding solution 'MySolution_Upgrade' must exist.")]
        [ValidateNotNullOrEmpty]
        public string SolutionName { get; set; }

        /// <summary>
        /// Gets or sets whether to check if the holding solution exists before attempting to apply the upgrade.
        /// </summary>
        [Parameter(HelpMessage = "Check if the holding solution (SolutionName_Upgrade) exists before attempting to apply the upgrade. If it doesn't exist, skip the operation.")]
        public SwitchParameter IfExists { get; set; }

        /// <summary>
        /// Gets or sets the polling interval in seconds for checking upgrade status. Default is 5 seconds.
        /// </summary>
        [Parameter(HelpMessage = "Polling interval in seconds for checking upgrade status. Default is 5.")]
        [ValidateRange(1, int.MaxValue)]
        public int PollingIntervalSeconds { get; set; } = 5;

        /// <summary>
        /// Gets or sets the timeout in seconds for the upgrade operation. Default is 3600 seconds (1 hour).
        /// </summary>
        [Parameter(HelpMessage = "Timeout in seconds for the upgrade operation. Default is 3600 (1 hour).")]
        [ValidateRange(1, int.MaxValue)]
        public int TimeoutSeconds { get; set; } = 3600;

        /// <summary>
        /// Processes the cmdlet request.
        /// </summary>
        protected override void ProcessRecord()
        {
            base.ProcessRecord();

            string holdingSolutionName = $"{SolutionName}_Upgrade";

            // Check if holding solution exists when IfExists is specified
            if (IfExists.IsPresent)
            {
                WriteVerbose($"Checking if holding solution '{holdingSolutionName}' exists...");
                
                if (!DoesHoldingSolutionExist(holdingSolutionName))
                {
                    WriteWarning($"Holding solution '{holdingSolutionName}' does not exist. Skipping upgrade operation.");
                    return;
                }

                WriteVerbose($"Holding solution '{holdingSolutionName}' exists.");
            }

            if (!ShouldProcess($"Solution '{SolutionName}'", "Apply upgrade"))
            {
                return;
            }

            WriteVerbose($"Starting asynchronous upgrade for solution '{SolutionName}' using holding solution '{holdingSolutionName}'...");

            // Create progress record
            var progressRecord = new ProgressRecord(1, "Applying Solution Upgrade", $"Applying upgrade for solution '{SolutionName}'")
            {
                PercentComplete = 0
            };
            WriteProgress(progressRecord);

            // Create DeleteAndPromoteRequest wrapped in ExecuteAsyncRequest
            var deleteAndPromoteRequest = new DeleteAndPromoteRequest
            {
                UniqueName = SolutionName
            };

            var asyncRequest = new ExecuteAsyncRequest
            {
                Request = deleteAndPromoteRequest
            };

            var asyncResponse = (ExecuteAsyncResponse)Connection.Execute(asyncRequest);
            var asyncOperationId = asyncResponse.AsyncJobId;

            WriteVerbose($"Upgrade request submitted. AsyncOperationId: {asyncOperationId}. Monitoring upgrade progress...");

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
                        new TimeoutException($"Solution upgrade timed out after {TimeoutSeconds} seconds."),
                        "UpgradeTimeout",
                        ErrorCategory.OperationTimeout,
                        SolutionName));
                    return;
                }

                // Check if stopping has been requested
                if (Stopping)
                {
                    progressRecord.RecordType = ProgressRecordType.Completed;
                    WriteProgress(progressRecord);
                    WriteWarning("Solution upgrade was stopped by user.");
                    return;
                }

                // Retrieve the async operation status
                var asyncOp = Connection.Retrieve("asyncoperation", asyncOperationId, new ColumnSet("statecode", "statuscode", "message"));
                var stateCode = asyncOp.GetAttributeValue<OptionSetValue>("statecode")?.Value ?? -1;

                if (stateCode == 3) // Completed
                {
                    var statusCode = asyncOp.GetAttributeValue<OptionSetValue>("statuscode")?.Value ?? -1;
                    if (statusCode == 30) // Succeeded
                    {
                        progressRecord.PercentComplete = 100;
                        progressRecord.RecordType = ProgressRecordType.Completed;
                        WriteProgress(progressRecord);
                        WriteVerbose($"Successfully applied upgrade for solution '{SolutionName}'.");

                        return;
                    }
                    else
                    {
                        // Failed or other status
                        var message = asyncOp.GetAttributeValue<string>("message") ?? "Unknown error during upgrade.";
                        progressRecord.RecordType = ProgressRecordType.Completed;
                        WriteProgress(progressRecord);
                        ThrowTerminatingError(new ErrorRecord(
                            new InvalidOperationException($"Solution upgrade failed: {message}"),
                            "UpgradeFailed",
                            ErrorCategory.OperationStopped,
                            SolutionName));
                        return;
                    }
                }

                // Operation still in progress
                progressRecord.StatusDescription = "Applying upgrade...";
                progressRecord.PercentComplete = 50;
                WriteProgress(progressRecord);

                WriteVerbose("Upgrade operation in progress. Waiting before checking again...");

                // Wait before polling again
                Thread.Sleep(pollingInterval);
            }
        }

        private bool DoesHoldingSolutionExist(string holdingSolutionName)
        {
            var query = new QueryExpression("solution")
            {
                ColumnSet = new ColumnSet("uniquename"),
                Criteria = new FilterExpression
                {
                    Conditions =
                    {
                        new ConditionExpression("uniquename", ConditionOperator.Equal, holdingSolutionName)
                    }
                },
                TopCount = 1
            };

            var solutions = Connection.RetrieveMultiple(query);
            return solutions.Entities.Count > 0;
        }
    }
}
