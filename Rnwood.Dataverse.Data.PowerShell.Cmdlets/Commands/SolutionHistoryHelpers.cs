using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Query;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;

namespace Rnwood.Dataverse.Data.PowerShell.Commands
{
    /// <summary>
    /// Helper methods for waiting on Dataverse solution history operations.
    /// </summary>
    internal static class SolutionHistoryHelpers
    {
        private const string UpgradeSuffix = "_Upgrade";

        /// <summary>
        /// Waits for any in-progress solution history operations for the supplied solution names.
        /// </summary>
        /// <param name="connection">The organization service connection.</param>
        /// <param name="solutionNames">The solution names to check.</param>
        /// <param name="writeVerbose">Action to write verbose messages.</param>
        /// <param name="maxWaitSeconds">Maximum time to wait in seconds.</param>
        /// <param name="pollIntervalSeconds">Interval between polls in seconds.</param>
        public static void WaitForSolutionOperationsToComplete(
            IOrganizationService connection,
            IEnumerable<string> solutionNames,
            Action<string> writeVerbose,
            int maxWaitSeconds = 30,
            int pollIntervalSeconds = 2)
        {
            if (connection == null)
            {
                throw new ArgumentNullException(nameof(connection));
            }

            var namesToCheck = solutionNames?
                .Where(name => !string.IsNullOrWhiteSpace(name))
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .ToArray() ?? Array.Empty<string>();

            if (namesToCheck.Length == 0)
            {
                return;
            }

            writeVerbose?.Invoke($"Checking solution history for in-progress operations for: {string.Join(", ", namesToCheck)}");

            var startTime = DateTime.UtcNow;
            var maxWait = TimeSpan.FromSeconds(maxWaitSeconds);
            var pollInterval = TimeSpan.FromSeconds(pollIntervalSeconds);

            while (true)
            {
                IReadOnlyCollection<string> inProgressSolutionNames;

                try
                {
                    inProgressSolutionNames = GetInProgressSolutionNames(connection, namesToCheck);
                }
                catch (Exception ex)
                {
                    writeVerbose?.Invoke($"Warning: Unable to check solution history (this may be expected in some environments): {ex.Message}");
                    return;
                }

                if (inProgressSolutionNames.Count == 0)
                {
                    writeVerbose?.Invoke("No in-progress solution history operations detected.");
                    return;
                }

                if (DateTime.UtcNow - startTime > maxWait)
                {
                    throw new TimeoutException(
                        $"Timed out after {maxWaitSeconds} seconds waiting for solution history operations to complete for: {string.Join(", ", inProgressSolutionNames)}");
                }

                writeVerbose?.Invoke(
                    $"Solution history operation still in progress for: {string.Join(", ", inProgressSolutionNames)}. Waiting {pollIntervalSeconds} seconds before checking again...");
                Thread.Sleep(pollInterval);
            }
        }

        /// <summary>
        /// Gets the solution names to check, including the related upgrade or base solution.
        /// </summary>
        /// <param name="solutionName">The solution unique name.</param>
        /// <returns>The solution names to check.</returns>
        public static IReadOnlyCollection<string> GetSolutionNamesToCheck(string solutionName)
        {
            if (string.IsNullOrWhiteSpace(solutionName))
            {
                return Array.Empty<string>();
            }

            var names = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
            {
                solutionName
            };

            if (solutionName.EndsWith(UpgradeSuffix, StringComparison.OrdinalIgnoreCase))
            {
                names.Add(solutionName.Substring(0, solutionName.Length - UpgradeSuffix.Length));
            }
            else
            {
                names.Add($"{solutionName}{UpgradeSuffix}");
            }

            return names.ToArray();
        }

        private static IReadOnlyCollection<string> GetInProgressSolutionNames(IOrganizationService connection, IEnumerable<string> solutionNames)
        {
            var solutionNameValues = solutionNames.Cast<object>().ToArray();

            var query = new QueryExpression("msdyn_solutionhistory")
            {
                ColumnSet = new ColumnSet("msdyn_name", "msdyn_status", "msdyn_starttime"),
                Criteria = new FilterExpression
                {
                    Conditions =
                    {
                        new ConditionExpression("msdyn_name", ConditionOperator.In, solutionNameValues)
                    }
                },
                Orders =
                {
                    new OrderExpression("msdyn_starttime", OrderType.Descending)
                }
            };

            var results = connection.RetrieveMultiple(query);
            var latestResultsBySolution = new Dictionary<string, Entity>(StringComparer.OrdinalIgnoreCase);

            foreach (var entity in results.Entities)
            {
                var solutionName = entity.GetAttributeValue<string>("msdyn_name");

                if (string.IsNullOrWhiteSpace(solutionName) || latestResultsBySolution.ContainsKey(solutionName))
                {
                    continue;
                }

                latestResultsBySolution[solutionName] = entity;
            }

            return latestResultsBySolution
                .Where(kvp =>
                {
                    var status = kvp.Value.GetAttributeValue<OptionSetValue>("msdyn_status")?.Value ?? -1;
                    return status == 0 || status == 2;
                })
                .Select(kvp => kvp.Key)
                .ToArray();
        }
    }
}
