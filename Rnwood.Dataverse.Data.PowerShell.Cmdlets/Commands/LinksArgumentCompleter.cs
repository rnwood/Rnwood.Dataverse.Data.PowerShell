using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Management.Automation;
using System.Management.Automation.Language;
using Microsoft.PowerPlatform.Dataverse.Client;
using Microsoft.Xrm.Sdk.Metadata;

namespace Rnwood.Dataverse.Data.PowerShell.Commands
{
    /// <summary>
    /// Argument completer for the -Links parameter on Get-DataverseRecord.
    /// Provides completion for table names in simplified link syntax based on:
    /// - The current table name (from TableName parameter)
    /// - Lookup relationships from the current table
    /// - Other links already in the array
    /// </summary>
    public class LinksArgumentCompleter : IArgumentCompleter
    {
        /// <summary>
        /// Returns completion suggestions for link entity syntax.
        /// </summary>
        /// <param name="commandName">Name of the command being completed.</param>
        /// <param name="parameterName">Name of the parameter being completed.</param>
        /// <param name="wordToComplete">The current partial word to complete.</param>
        /// <param name="commandAst">The AST for the command being completed.</param>
        /// <param name="fakeBoundParameters">Currently bound parameters and their values.</param>
        /// <returns>Enumeration of possible completion results.</returns>
        public IEnumerable<CompletionResult> CompleteArgument(string commandName, string parameterName, string wordToComplete, CommandAst commandAst, IDictionary fakeBoundParameters)
        {
            try
            {
                string prefix = wordToComplete ?? string.Empty;

                // Strip common surrounding characters
                prefix = prefix.Trim();
                prefix = prefix.TrimStart('@', '{', '\'', '"');

                string tableName = null;
                ServiceClient connectionObj = null;

                if (fakeBoundParameters != null)
                {
                    if (fakeBoundParameters.Contains("TableName") && fakeBoundParameters["TableName"] is string tn)
                    {
                        tableName = tn;
                    }

                    foreach (DictionaryEntry entry in fakeBoundParameters)
                    {
                        var key = entry.Key as string;
                        if (string.Equals(key, "Connection", StringComparison.OrdinalIgnoreCase))
                        {
                            if (entry.Value is ServiceClient sc)
                            {
                                connectionObj = sc;
                            }
                            else if (entry.Value is PSObject pso && pso.BaseObject is ServiceClient sc2)
                            {
                                connectionObj = sc2;
                            }

                            break;
                        }
                    }
                }

                if (string.IsNullOrEmpty(tableName) || connectionObj == null)
                {
                    return Enumerable.Empty<CompletionResult>();
                }

                var metadataFactory = new EntityMetadataFactory(connectionObj);
                EntityMetadata metadata = null;
                try
                {
                    metadata = metadataFactory.GetMetadata(tableName);
                }
                catch
                {
                    return Enumerable.Empty<CompletionResult>();
                }

                if (metadata == null)
                {
                    return Enumerable.Empty<CompletionResult>();
                }

                var results = new List<CompletionResult>();

                // Get all lookup attributes from the current table
                var lookupAttributes = metadata.Attributes
                    .Where(a => a.AttributeType == AttributeTypeCode.Lookup || 
                               a.AttributeType == AttributeTypeCode.Customer || 
                               a.AttributeType == AttributeTypeCode.Owner)
                    .OfType<LookupAttributeMetadata>()
                    .ToList();

                // For each lookup, suggest link syntax
                foreach (var lookupAttr in lookupAttributes)
                {
                    // Get target entities
                    if (lookupAttr.Targets != null && lookupAttr.Targets.Length > 0)
                    {
                        foreach (var targetEntity in lookupAttr.Targets)
                        {
                            try
                            {
                                // Get metadata for target entity to find its primary key
                                var targetMetadata = metadataFactory.GetMetadata(targetEntity);
                                if (targetMetadata != null && !string.IsNullOrEmpty(targetMetadata.PrimaryIdAttribute))
                                {
                                    string fromSpec = $"{tableName}.{lookupAttr.LogicalName}";
                                    string toSpec = $"{targetEntity}.{targetMetadata.PrimaryIdAttribute}";
                                    string linkSpec = $"'{fromSpec}' = '{toSpec}'";

                                    // Check if this matches the prefix
                                    if (string.IsNullOrEmpty(prefix) || 
                                        fromSpec.IndexOf(prefix, StringComparison.OrdinalIgnoreCase) >= 0 ||
                                        toSpec.IndexOf(prefix, StringComparison.OrdinalIgnoreCase) >= 0 ||
                                        targetEntity.IndexOf(prefix, StringComparison.OrdinalIgnoreCase) >= 0)
                                    {
                                        var displayName = lookupAttr.DisplayName?.UserLocalizedLabel?.Label;
                                        var listItemText = string.IsNullOrWhiteSpace(displayName) 
                                            ? $"{lookupAttr.LogicalName} → {targetEntity}"
                                            : $"{lookupAttr.LogicalName} ({displayName}) → {targetEntity}";

                                        var completionText = $"@{{ {linkSpec} }}";
                                        var toolTip = $"Link from {tableName}.{lookupAttr.LogicalName} to {targetEntity}.{targetMetadata.PrimaryIdAttribute}";

                                        results.Add(new CompletionResult(completionText, listItemText, CompletionResultType.ParameterValue, toolTip));
                                    }
                                }
                            }
                            catch
                            {
                                // Skip this target if we can't get metadata
                                continue;
                            }
                        }
                    }
                }

                return results.Take(50); // Limit to 50 results to avoid overwhelming the user
            }
            catch
            {
                return Enumerable.Empty<CompletionResult>();
            }
        }
    }
}
