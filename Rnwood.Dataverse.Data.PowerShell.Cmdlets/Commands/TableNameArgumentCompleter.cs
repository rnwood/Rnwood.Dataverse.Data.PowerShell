using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Management.Automation;
using System.Management.Automation.Language;
using Microsoft.Xrm.Sdk;
using Microsoft.PowerPlatform.Dataverse.Client;

namespace Rnwood.Dataverse.Data.PowerShell.Commands
{
    /// <summary>
    /// Provides tab-completion for parameters that accept a table/entity logical name.
    /// It looks for a bound 'Connection' parameter and uses it to query metadata.
    /// </summary>
    public class TableNameArgumentCompleter : IArgumentCompleter
    {
        /// <summary>
        /// Returns completion candidates for table logical names. If a Connection parameter
        /// is bound it will be used to retrieve entity metadata.
        /// </summary>
        /// <returns>Sequence of CompletionResult objects for matching logical names.</returns>
        public IEnumerable<CompletionResult> CompleteArgument(string commandName, string parameterName, string wordToComplete, CommandAst commandAst, IDictionary fakeBoundParameters)
        {
            try
            {
                if (fakeBoundParameters == null)
                {
                    return Enumerable.Empty<CompletionResult>();
                }

                ServiceClient connectionObj = null;
                // Look for Connection parameter in bound parameters (case-insensitive)
                foreach (DictionaryEntry entry in fakeBoundParameters)
                {
                    var key = entry.Key as string;
                    if (string.Equals(key, "Connection", StringComparison.OrdinalIgnoreCase))
                    {
                        if (entry.Value is ServiceClient)
                        {
                            connectionObj = (ServiceClient)entry.Value;
                        }
                        else if (entry.Value is PSObject pso && pso.BaseObject is ServiceClient)
                        {
                            connectionObj = (ServiceClient)pso.BaseObject;
                        }

                        break;
                    }
                }

                // If no explicit connection was provided, try to use the default connection
                if (connectionObj == null)
                {
                    connectionObj = DefaultConnectionManager.DefaultConnection;
                }

                if (connectionObj == null)
                {
                    return Enumerable.Empty<CompletionResult>();
                }

                var metadataFactory = new EntityMetadataFactory(connectionObj);
                var allMetadata = metadataFactory.GetAllEntityMetadata();

                // Build a list of candidates: logical name and display label
                var candidates = allMetadata.Select(em => new
                {
                    LogicalName = em.LogicalName,
                    DisplayName = em.DisplayName?.UserLocalizedLabel?.Label ?? em.DisplayName?.LocalizedLabels?.FirstOrDefault()?.Label ?? string.Empty
                }).ToList();

                // If nothing typed, return top N logical names (format display as: "logicalName — DisplayName (primaryAttribute)")
                if (string.IsNullOrEmpty(wordToComplete))
                {
                    return candidates.Take(200).Select(c =>
                    {
                        string displayText = string.IsNullOrEmpty(c.DisplayName) ? c.LogicalName : $"{c.LogicalName} — {c.DisplayName} ({c.DisplayName})";
                        string toolTip = string.IsNullOrEmpty(c.DisplayName) ? "Entity logical name" : c.DisplayName;
                        return new CompletionResult(displayText, c.LogicalName, CompletionResultType.ParameterValue, toolTip);
                    });
                }

                string wc = wordToComplete.Trim();

                // Prioritize starts-with matches then contains matches
                var startsWithMatches = candidates.Where(c => c.LogicalName.StartsWith(wc, StringComparison.OrdinalIgnoreCase) || (!string.IsNullOrEmpty(c.DisplayName) && c.DisplayName.StartsWith(wc, StringComparison.OrdinalIgnoreCase)));
                var containsMatches = candidates.Where(c => (!c.LogicalName.StartsWith(wc, StringComparison.OrdinalIgnoreCase) && c.LogicalName.IndexOf(wc, StringComparison.OrdinalIgnoreCase) >= 0) || (!string.IsNullOrEmpty(c.DisplayName) && !c.DisplayName.StartsWith(wc, StringComparison.OrdinalIgnoreCase) && c.DisplayName.IndexOf(wc, StringComparison.OrdinalIgnoreCase) >= 0));

                var results = startsWithMatches.Concat(containsMatches)
                    .Select(c =>
                    {
                        string displayText = string.IsNullOrEmpty(c.DisplayName) ? c.LogicalName : $"{c.LogicalName} ({c.DisplayName})";
                        string toolTip = string.IsNullOrEmpty(c.DisplayName) ? "Entity logical name" : c.DisplayName;
                        return new CompletionResult(c.LogicalName, displayText, CompletionResultType.ParameterValue, toolTip);
                    });

                return results;
            }
            catch
            {
                // If completion fails for any reason, return no results rather than throwing.
                return Enumerable.Empty<CompletionResult>();
            }
        }
    }
}
