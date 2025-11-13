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
        /// Polls the msdyn_solutionhistory table for records with null msdyn_endtime (indicating in-progress operations).
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

                // Query for in-progress operations (where msdyn_endtime is null)
                var query = new QueryExpression("msdyn_solutionhistory")
                {
                    ColumnSet = new ColumnSet("msdyn_solutionhistoryid", "msdyn_starttime"),
                    Criteria = new FilterExpression
                    {
                        Conditions =
                        {
                            new ConditionExpression("msdyn_endtime", ConditionOperator.Null)
                        }
                    },
                    TopCount = 1 // We only need to know if any exist
                };

                try
                {
                    var results = connection.RetrieveMultiple(query);
                    
                    if (results.Entities == null || !results.Entities.Any())
                    {
                        writeVerbose?.Invoke("No in-progress publish or solution operations detected. Continuing...");
                        break;
                    }

                    var inProgressCount = results.TotalRecordCount;
                    writeVerbose?.Invoke($"Found {inProgressCount} in-progress operation(s). Waiting {pollIntervalSeconds} seconds before checking again...");
                    Thread.Sleep(pollInterval);
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
