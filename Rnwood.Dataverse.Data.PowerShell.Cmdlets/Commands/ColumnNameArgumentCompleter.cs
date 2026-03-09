using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Management.Automation;
using System.Management.Automation.Language;
using Microsoft.PowerPlatform.Dataverse.Client;

namespace Rnwood.Dataverse.Data.PowerShell.Commands
{
    /// <summary>
    /// Provides tab-completion for parameter values that are column/attribute logical names.
    /// It attempts to find an associated TableName parameter in the bound parameters to
    /// determine which entity's attributes to list. If a Connection parameter is bound it
    /// will be used to retrieve entity metadata.
    /// </summary>
    public class ColumnNameArgumentCompleter : IArgumentCompleter  
    {
    /// <summary>
    /// Provide completion results for a parameter that expects a Dataverse column/logical name.
    /// The completer attempts to determine the correct entity (table) from bound parameters
    /// such as &lt;Name&gt;TableName, &lt;Name&gt;EntityName or from bound PSObjects containing
    /// a TableName/LogicalName property or an EntityReference. If no explicit entity can be
    /// resolved, it may attempt a conservative inference from the cmdlet name.
    /// </summary>
    /// <param name="commandName">The name of the cmdlet being completed.</param>
    /// <param name="parameterName">The name of the parameter being completed.</param>
    /// <param name="wordToComplete">The current partial word to complete.</param>
    /// <param name="commandAst">The AST for the command being completed.</param>
    /// <param name="fakeBoundParameters">A dictionary of bound parameters provided by PowerShell.</param>
    /// <returns>A sequence of <see cref="CompletionResult"/> values for the completion UI.</returns>
    public IEnumerable<CompletionResult> CompleteArgument(string commandName, string parameterName, string wordToComplete, CommandAst commandAst, IDictionary fakeBoundParameters)
        {
            try
            {
                if (fakeBoundParameters == null)
                {
                    return Enumerable.Empty<CompletionResult>();
                }

                ServiceClient connectionObj = null;
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

                // If no explicit connection was provided, try to use the default connection
                if (connectionObj == null)
                {
                    connectionObj = DefaultConnectionManager.DefaultConnection;
                }

                if (connectionObj == null)
                {
                    return Enumerable.Empty<CompletionResult>();
                }

                // Determine candidate prefix based on the parameter being completed.
                // For *IgnoreProperties helpers we prefer to suggest property names from the associated PSObject parameter
                // (e.g. ExtraPropertiesIgnoreProperties -> ExtraProperties). If no explicit prefix is present we fall back to
                // common parameter names such as InputObject or Target.
                string baseName = parameterName;
                if (baseName.EndsWith("IgnoreProperties", StringComparison.OrdinalIgnoreCase))
                {
                    baseName = baseName.Substring(0, baseName.Length - "IgnoreProperties".Length);
                }
                else if (baseName.EndsWith("ColumnName", StringComparison.OrdinalIgnoreCase))
                {
                    baseName = baseName.Substring(0, baseName.Length - "ColumnName".Length);
                }
                else if (baseName.EndsWith("ColumnNames", StringComparison.OrdinalIgnoreCase))
                {
                    baseName = baseName.Substring(0, baseName.Length - "ColumnNames".Length);
                }
                else if (baseName.EndsWith("Columns", StringComparison.OrdinalIgnoreCase))
                {
                    baseName = baseName.Substring(0, baseName.Length - "Columns".Length);
                }
                else
                {
                    baseName = "";
                }

                var candidates = new List<string>();
                candidates.Add(baseName + "TableName");
                candidates.Add(baseName + "EntityName");
                candidates.Add(baseName + "LogicalName");

                string tableName = null;

                // Check bound parameters for an explicit table name
                foreach (DictionaryEntry entry in fakeBoundParameters)
                {
                    var key = entry.Key as string;
                    if (key == null) continue;

                    if (candidates.Any(c => string.Equals(c, key, StringComparison.OrdinalIgnoreCase)))
                    {
                        if (entry.Value is string s && !string.IsNullOrWhiteSpace(s))
                        {
                            tableName = s;
                            break;
                        }
                        if (entry.Value is PSObject pso)
                        {
                            // If user bound a PSObject, try common property names inside it
                            try
                            {
                                var prop = pso.Properties["TableName"] ?? pso.Properties["LogicalName"] ?? pso.Properties["EntityName"];
                                if (prop != null && prop.Value is string sval && !string.IsNullOrWhiteSpace(sval))
                                {
                                    tableName = sval;
                                    break;
                                }
                            }
                            catch { }
                        }
                    }
                }

                if (string.IsNullOrEmpty(tableName))
                {
                    // No explicit candidate table parameter was bound. Try to discover a table name from any bound PSObject
                    // (e.g. a Target PSObject that includes a TableName/LogicalName property) or from an EntityReference base object.
                    foreach (DictionaryEntry entry in fakeBoundParameters)
                    {
                        if (entry.Value is PSObject pso)
                        {
                            try
                            {
                                var prop = pso.Properties["TableName"] ?? pso.Properties["LogicalName"] ?? pso.Properties["EntityName"];
                                if (prop != null && prop.Value is string sval && !string.IsNullOrWhiteSpace(sval))
                                {
                                    tableName = sval;
                                    break;
                                }
                                // If underlying base object is an EntityReference, use its LogicalName
                                if (pso.BaseObject is Microsoft.Xrm.Sdk.EntityReference er && !string.IsNullOrWhiteSpace(er.LogicalName))
                                {
                                    tableName = er.LogicalName;
                                    break;
                                }
                            }
                            catch { }
                        }
                        else if (entry.Value is Microsoft.Xrm.Sdk.EntityReference er2 && !string.IsNullOrWhiteSpace(er2.LogicalName))
                        {
                            tableName = er2.LogicalName;
                            break;
                        }
                    }
                }

                if (string.IsNullOrEmpty(tableName))
                {
                    // As a last resort, attempt to infer the entity from the cmdlet name. This is conservative: we only
                    // use an inferred match when it yields a single, unambiguous entity match.
                    try
                    {
                        var metadataFactoryAll = new EntityMetadataFactory(connectionObj);
                        var allMetadata = metadataFactoryAll.GetAllEntityMetadata();
                        if (allMetadata != null && allMetadata.Any())
                        {
                            // Break the command name into tokens (dash-separated and PascalCase tokens)
                            var tokens = new List<string>();
                            if (!string.IsNullOrEmpty(commandName))
                            {
                                tokens.AddRange(commandName.Split('-').Where(t => !string.IsNullOrWhiteSpace(t)));
                                // Also split PascalCase words from the first token
                                var first = tokens.FirstOrDefault() ?? commandName;
                                var pascalTokens = System.Text.RegularExpressions.Regex.Matches(first, "([A-Z][a-z0-9]+|[0-9]+)")
                                    .Cast<System.Text.RegularExpressions.Match>()
                                    .Select(m => m.Value).Where(s => !string.IsNullOrWhiteSpace(s)).ToArray();
                                tokens.AddRange(pascalTokens);
                            }

                            var matches = new List<Microsoft.Xrm.Sdk.Metadata.EntityMetadata>();
                            foreach (var t in tokens.Where(x => !string.IsNullOrWhiteSpace(x)))
                            {
                                var token = t.Trim();
                                var found = allMetadata.Where(md =>
                                    (!string.IsNullOrEmpty(md.LogicalName) && md.LogicalName.IndexOf(token, StringComparison.OrdinalIgnoreCase) >= 0) ||
                                    (md.DisplayName?.UserLocalizedLabel?.Label != null && md.DisplayName.UserLocalizedLabel.Label.IndexOf(token, StringComparison.OrdinalIgnoreCase) >= 0) ||
                                    (md.DisplayName?.LocalizedLabels?.FirstOrDefault()?.Label != null && md.DisplayName.LocalizedLabels.FirstOrDefault().Label.IndexOf(token, StringComparison.OrdinalIgnoreCase) >= 0)
                                ).ToList();

                                if (found != null && found.Count == 1)
                                {
                                    matches.Add(found[0]);
                                }
                            }

                            // Use the first unique match if exactly one distinct entity was found
                            var distinct = matches.Distinct().ToList();
                            if (distinct.Count == 1)
                            {
                                tableName = distinct[0].LogicalName;
                            }
                        }
                    }
                    catch
                    {
                        // Ignore any failures in the inference stage
                    }
                }

                if (string.IsNullOrEmpty(tableName))
                {
                    // Could not determine an entity for column-name completion — return no suggestions.
                    return Enumerable.Empty<CompletionResult>();
                }

                var metadataFactory = new EntityMetadataFactory(connectionObj);
                var em = metadataFactory.GetLimitedMetadata(tableName);
                if (em == null)
                {
                    return Enumerable.Empty<CompletionResult>();
                }

                var allColumns = DataverseEntityConverter.GetAllColumnNames(em, false, null, true);

                if (string.IsNullOrEmpty(wordToComplete))
                {
                    return allColumns.Take(200).Select(c => new CompletionResult(c, c, CompletionResultType.ParameterValue, tableName));
                }

                string wc = wordToComplete.Trim();
                var startsWith = allColumns.Where(c => c.StartsWith(wc, StringComparison.OrdinalIgnoreCase));
                var contains = allColumns.Where(c => !c.StartsWith(wc, StringComparison.OrdinalIgnoreCase) && c.IndexOf(wc, StringComparison.OrdinalIgnoreCase) >= 0);

                return startsWith.Concat(contains).Select(c => new CompletionResult(c, c, CompletionResultType.ParameterValue, tableName));
            }
            catch
            {
                return Enumerable.Empty<CompletionResult>();
            }
        }
    }
}
