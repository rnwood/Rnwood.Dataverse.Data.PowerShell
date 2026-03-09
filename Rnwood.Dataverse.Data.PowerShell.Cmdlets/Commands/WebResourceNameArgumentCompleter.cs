using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Management.Automation;
using System.Management.Automation.Language;
using Microsoft.PowerPlatform.Dataverse.Client;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Query;

namespace Rnwood.Dataverse.Data.PowerShell.Commands
{
    /// <summary>
    /// Provides tab-completion for parameters that accept web resource names.
    /// Supports partial name matching and optional filtering by web resource type
    /// based on parameter name patterns (e.g., icon parameters filter to image types).
    /// </summary>
    public class WebResourceNameArgumentCompleter : IArgumentCompleter
    {
        /// <summary>
        /// Returns completion candidates for web resource names with optional type filtering.
        /// If a Connection parameter is bound it will be used to retrieve web resources.
        /// Supports partial name matching using startsWith and contains logic.
        /// </summary>
        /// <param name="commandName">The name of the command.</param>
        /// <param name="parameterName">The name of the parameter being completed.</param>
        /// <param name="wordToComplete">The partial word being completed.</param>
        /// <param name="commandAst">The AST of the command.</param>
        /// <param name="fakeBoundParameters">Dictionary of bound parameters.</param>
        /// <returns>Sequence of CompletionResult objects for matching web resource names.</returns>
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

                // Determine which web resource types to filter by based on parameter name
                var typeFilter = DetermineTypeFilter(parameterName);

                // Query web resources from Dataverse
                var query = new QueryExpression("webresource")
                {
                    ColumnSet = new ColumnSet("name", "webresourcetype", "displayname"),
                    TopCount = 500 // Limit to top 500 for performance
                };

                // Apply type filter if determined from parameter name
                if (typeFilter != null && typeFilter.Length > 0)
                {
                    if (typeFilter.Length == 1)
                    {
                        query.Criteria.AddCondition("webresourcetype", ConditionOperator.Equal, typeFilter[0]);
                    }
                    else
                    {
                        query.Criteria.AddCondition("webresourcetype", ConditionOperator.In, typeFilter.Cast<object>().ToArray());
                    }
                }

                // Add partial name filter if provided
                if (!string.IsNullOrEmpty(wordToComplete))
                {
                    string wc = wordToComplete.Trim();
                    // Use Like operator for partial matching
                    string likePattern = $"%{wc}%";
                    query.Criteria.AddCondition("name", ConditionOperator.Like, likePattern);
                }

                // Execute query (use RetrieveUnpublishedMultiple to include unpublished web resources)
                EntityCollection results;
                try
                {
                    var request = new Microsoft.Crm.Sdk.Messages.RetrieveUnpublishedMultipleRequest { Query = query };
                    var response = (Microsoft.Crm.Sdk.Messages.RetrieveUnpublishedMultipleResponse)connectionObj.Execute(request);
                    results = response.EntityCollection;
                }
                catch
                {
                    // Fallback to regular retrieve if unpublished retrieve fails
                    results = connectionObj.RetrieveMultiple(query);
                }

                if (results.Entities.Count == 0)
                {
                    return Enumerable.Empty<CompletionResult>();
                }

                // Extract web resource information
                var webResources = results.Entities.Select(e => new
                {
                    Name = e.GetAttributeValue<string>("name"),
                    Type = e.GetAttributeValue<OptionSetValue>("webresourcetype")?.Value ?? 0,
                    DisplayName = e.GetAttributeValue<string>("displayname")
                }).Where(wr => !string.IsNullOrEmpty(wr.Name)).ToList();

                // If no word to complete, return all results (already limited by query)
                if (string.IsNullOrEmpty(wordToComplete))
                {
                    return webResources.Select(wr =>
                    {
                        string typeDesc = GetTypeDescription(wr.Type);
                        string displayText = string.IsNullOrEmpty(wr.DisplayName) 
                            ? $"{wr.Name} ({typeDesc})" 
                            : $"{wr.Name} — {wr.DisplayName} ({typeDesc})";
                        string toolTip = string.IsNullOrEmpty(wr.DisplayName) 
                            ? $"{typeDesc}: {wr.Name}" 
                            : $"{typeDesc}: {wr.Name}\n{wr.DisplayName}";
                        return new CompletionResult(wr.Name, displayText, CompletionResultType.ParameterValue, toolTip);
                    });
                }

                string wcLower = wordToComplete.Trim().ToLowerInvariant();

                // Prioritize results: exact match > starts-with > contains
                var exactMatches = webResources.Where(wr => wr.Name.Equals(wordToComplete, StringComparison.OrdinalIgnoreCase));
                var startsWithMatches = webResources.Where(wr => 
                    wr.Name.StartsWith(wcLower, StringComparison.OrdinalIgnoreCase) && 
                    !wr.Name.Equals(wordToComplete, StringComparison.OrdinalIgnoreCase));
                var containsMatches = webResources.Where(wr => 
                    !wr.Name.StartsWith(wcLower, StringComparison.OrdinalIgnoreCase) && 
                    wr.Name.IndexOf(wcLower, StringComparison.OrdinalIgnoreCase) >= 0);

                var orderedResults = exactMatches.Concat(startsWithMatches).Concat(containsMatches);

                return orderedResults.Select(wr =>
                {
                    string typeDesc = GetTypeDescription(wr.Type);
                    string displayText = string.IsNullOrEmpty(wr.DisplayName)
                        ? $"{wr.Name} ({typeDesc})"
                        : $"{wr.Name} — {wr.DisplayName} ({typeDesc})";
                    string toolTip = string.IsNullOrEmpty(wr.DisplayName)
                        ? $"{typeDesc}: {wr.Name}"
                        : $"{typeDesc}: {wr.Name}\n{wr.DisplayName}";
                    return new CompletionResult(wr.Name, displayText, CompletionResultType.ParameterValue, toolTip);
                });
            }
            catch
            {
                // If completion fails for any reason, return no results rather than throwing
                return Enumerable.Empty<CompletionResult>();
            }
        }

        /// <summary>
        /// Determines which web resource types to filter by based on the parameter name.
        /// Returns null for no filtering, or an array of type codes to filter.
        /// </summary>
        private int[] DetermineTypeFilter(string parameterName)
        {
            if (string.IsNullOrEmpty(parameterName))
            {
                return null;
            }

            string paramLower = parameterName.ToLowerInvariant();

            // Icon parameters - filter to image types (PNG, JPG, GIF) for Large/Medium/Small icons
            if (paramLower.Contains("iconlarge") || paramLower.Contains("iconmedium") || paramLower.Contains("iconsmall"))
            {
                return new[] { 5, 6, 7 }; // PNG, JPG, GIF
            }

            // Vector icon parameters - filter to SVG
            if (paramLower.Contains("iconvector"))
            {
                return new[] { 11 }; // SVG
            }

            // General icon parameters without specific size - include all image types
            if (paramLower.Contains("icon"))
            {
                return new[] { 5, 6, 7, 10, 11 }; // PNG, JPG, GIF, ICO, SVG
            }

            // Event handler library parameters - filter to JavaScript
            if (paramLower.Contains("library") || paramLower.Contains("handler") || paramLower.Contains("event") || paramLower.Contains("script"))
            {
                return new[] { 3 }; // JavaScript
            }

            // No specific filtering
            return null;
        }

        /// <summary>
        /// Gets a human-readable description of a web resource type code.
        /// </summary>
        private string GetTypeDescription(int typeCode)
        {
            switch (typeCode)
            {
                case 1: return "HTML";
                case 2: return "CSS";
                case 3: return "JavaScript";
                case 4: return "XML";
                case 5: return "PNG";
                case 6: return "JPG";
                case 7: return "GIF";
                case 8: return "XAP";
                case 9: return "XSL";
                case 10: return "ICO";
                case 11: return "SVG";
                case 12: return "RESX";
                default: return "Unknown";
            }
        }
    }
}
