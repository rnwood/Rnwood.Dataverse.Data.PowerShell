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
    /// Argument completer for the -FilterValues parameter on Get-DataverseRecord.
    /// Completes column logical names for the table specified by the TableName parameter when a Connection is available.
    /// </summary>
    public class FilterValuesArgumentCompleter : IArgumentCompleter
    {
    /// <summary>
    /// Returns completion suggestions for a partial column name based on the bound TableName and Connection.
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

                // Strip common surrounding characters a user might have typed in a hashtable context
                prefix = prefix.Trim();
                prefix = prefix.TrimStart('@', '{');
                if (prefix.Contains('='))
                {
                    prefix = prefix.Substring(0, prefix.IndexOf('='));
                }
                if (prefix.Contains(':'))
                {
                    prefix = prefix.Substring(0, prefix.IndexOf(':'));
                }

                string tableName = null;
                ServiceClient connectionObj = null;

                if (fakeBoundParameters != null)
                {
                    if (fakeBoundParameters.Contains("TableName") && fakeBoundParameters["TableName"] is string tn)
                    {
                        tableName = tn;
                    }

                    // Copy connection extraction logic from ColumnNameArgumentCompleter so we handle both ServiceClient
                    // and PSObject wrappers containing a ServiceClient.
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

                // If no explicit connection was provided, try to use the default connection
                if (connectionObj == null)
                {
                    connectionObj = DefaultConnectionManager.DefaultConnection;
                }

                if (string.IsNullOrEmpty(tableName) || connectionObj == null)
                {
                    // We can't fetch metadata without a connection and a table name; return no suggestions.
                    return Enumerable.Empty<CompletionResult>();
                }

                var metadataFactory = new EntityMetadataFactory(connectionObj);
                EntityMetadata metadata = null;
                try
                {
                    metadata = metadataFactory.GetLimitedMetadata(tableName);
                }
                catch
                {
                    // If retrieving metadata fails, silently return no completions.
                    return Enumerable.Empty<CompletionResult>();
                }

                if (metadata == null)
                {
                    return Enumerable.Empty<CompletionResult>();
                }

                // Use attribute metadata so we can include the display name in the list item text
                var attributes = metadata.Attributes
                    .GroupBy(a => a.LogicalName, StringComparer.OrdinalIgnoreCase)
                    .Select(g => g.First());

                // Match when prefix is contained in either logical name or display name (case-insensitive)
                var results = attributes
                    .Select(a => new { Attr = a, DisplayName = a.DisplayName?.UserLocalizedLabel?.Label ?? string.Empty })
                    .Where(x =>
                        x.Attr.LogicalName.IndexOf(prefix, StringComparison.OrdinalIgnoreCase) >= 0
                        || (!string.IsNullOrWhiteSpace(x.DisplayName) && x.DisplayName.IndexOf(prefix, StringComparison.OrdinalIgnoreCase) >= 0)
                    )
                    .Select(x =>
                    {
                    var listItemText = string.IsNullOrWhiteSpace(x.DisplayName) ? x.Attr.LogicalName : $"{x.Attr.LogicalName} - {x.DisplayName}";
                    var toolTip = string.IsNullOrWhiteSpace(x.DisplayName) ? "Column name" : x.DisplayName;
                    return new CompletionResult($"@{{\"{ x.Attr.LogicalName }\"=@{{operator=\"Equal\"; value=\"\"}}}}", listItemText, CompletionResultType.ParameterValue, toolTip);

            })
                    .ToList();

                return results;
             }
             catch
             {
                 return Enumerable.Empty<CompletionResult>();
             }
         }
     }
 }
