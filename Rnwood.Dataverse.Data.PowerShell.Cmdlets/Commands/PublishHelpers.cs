using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Query;
using System;
using System.Linq;
using System.Threading;

namespace Rnwood.Dataverse.Data.PowerShell.Commands
{
    /// <summary>
    /// Helper methods for publish operations.
    /// </summary>
    internal static class PublishHelpers
    {
        /// <summary>
        /// Waits for any in-progress publish or solution operations to complete.
        /// Polls the msdyn_solutionhistory table for the most recent record by msdyn_starttime
        /// and checks if msdyn_status is Started (0) or Queued (2), indicating in-progress operations.
        /// </summary>
        /// <param name="connection">The organization service connection</param>
        /// <param name="writeVerbose">Action to write verbose messages</param>
        /// <param name="maxWaitSeconds">Maximum time to wait in seconds (default 300 = 5 minutes)</param>
        /// <param name="pollIntervalSeconds">Interval between polls in seconds (default 2)</param>
        public static void WaitForPublishComplete(IOrganizationService connection, Action<string> writeVerbose, int maxWaitSeconds = 300, int pollIntervalSeconds = 2)
        {
            if (connection == null)
            {
                throw new ArgumentNullException(nameof(connection));
            }

            writeVerbose?.Invoke("Waiting for any in-progress publish or solution operations to complete...");

            var startTime = DateTime.UtcNow;
            var maxWaitTime = TimeSpan.FromSeconds(maxWaitSeconds);
            var pollInterval = TimeSpan.FromSeconds(pollIntervalSeconds);

            while (true)
            {
                // Check if max wait time exceeded
                if (DateTime.UtcNow - startTime > maxWaitTime)
                {
                    writeVerbose?.Invoke($"Warning: Publish wait timeout after {maxWaitSeconds} seconds. Operation may still be in progress.");
                    break;
                }

                // Query for the most recent operation by msdyn_starttime and check its status
                // msdyn_status: 0 = Started, 1 = Completed, 2 = Queued
                var query = new QueryExpression("msdyn_solutionhistory")
                {
                    ColumnSet = new ColumnSet("msdyn_solutionhistoryid", "msdyn_starttime", "msdyn_status"),
                    Orders =
                    {
                        new OrderExpression("msdyn_starttime", OrderType.Descending)
                    },
                    TopCount = 1 // Get only the most recent record
                };

                try
                {
                    var results = QueryHelpers.RetrieveMultipleWithThrottlingRetry(connection, query, writeVerbose);
                    
                    if (results.Entities == null || !results.Entities.Any())
                    {
                        writeVerbose?.Invoke("No solution history records found. Continuing...");
                        break;
                    }

                    var mostRecentRecord = results.Entities.First();
                    
                    // Check if the most recent operation is still in progress
                    if (mostRecentRecord.Contains("msdyn_status"))
                    {
                        var status = mostRecentRecord.GetAttributeValue<Microsoft.Xrm.Sdk.OptionSetValue>("msdyn_status");
                        var statusValue = status?.Value ?? -1;
                        
                        // 0 = Started, 2 = Queued (both indicate in-progress)
                        // 1 = Completed
                        if (statusValue == 0 || statusValue == 2)
                        {
                            var statusText = statusValue == 0 ? "Started" : "Queued";
                            writeVerbose?.Invoke($"Most recent operation status: {statusText}. Waiting {pollIntervalSeconds} seconds before checking again...");
                            Thread.Sleep(pollInterval);
                            continue;
                        }
                        else if (statusValue == 1)
                        {
                            writeVerbose?.Invoke("Most recent operation completed. Continuing...");
                            break;
                        }
                        else
                        {
                            writeVerbose?.Invoke($"Unknown status value: {statusValue}. Continuing...");
                            break;
                        }
                    }
                    else
                    {
                        writeVerbose?.Invoke("No status information available in most recent record. Continuing...");
                        break;
                    }
                }
                catch (Exception ex)
                {
                    // If the table doesn't exist or we don't have permissions, just continue
                    writeVerbose?.Invoke($"Warning: Unable to check publish status (this may be expected in some environments): {ex.Message}");
                    break;
                }
            }
        }
    }
}
