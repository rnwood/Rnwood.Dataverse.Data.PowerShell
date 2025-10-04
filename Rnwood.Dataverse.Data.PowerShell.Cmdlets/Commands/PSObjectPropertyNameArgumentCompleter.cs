using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Management.Automation;
using System.Management.Automation.Language;

namespace Rnwood.Dataverse.Data.PowerShell.Commands
{
    /// <summary>
    /// Provides tab-completion for helper parameters that list property names on a bound PSObject
    /// (for example &lt;Prefix&gt;IgnoreProperties). This completer finds an associated PSObject
    /// parameter (by prefix or common names such as InputObject/Target) and suggests its property
    /// names as completion candidates.
    /// </summary>
    public class PSObjectPropertyNameArgumentCompleter : IArgumentCompleter
    {
        /// <summary>
        /// Provide completion results based on PSObject property names for the related input object.
        /// </summary>
        public IEnumerable<CompletionResult> CompleteArgument(string commandName, string parameterName, string wordToComplete, CommandAst commandAst, IDictionary fakeBoundParameters)
        {
            try
            {
                if (fakeBoundParameters == null)
                {
                    return Enumerable.Empty<CompletionResult>();
                }

                // Determine prefix by stripping the suffix 'IgnoreProperties' if present
                string baseName = parameterName ?? "";
                if (baseName.EndsWith("IgnoreProperties", StringComparison.OrdinalIgnoreCase))
                {
                    baseName = baseName.Substring(0, baseName.Length - "IgnoreProperties".Length);
                }
                else
                {
                    baseName = "";
                }

                var possiblePrefixes = new List<string>();
                if (!string.IsNullOrEmpty(baseName)) possiblePrefixes.Add(baseName);
                possiblePrefixes.AddRange(new[] { "InputObject", "Target" });

                PSObject sourcePso = null;
                foreach (DictionaryEntry entry in fakeBoundParameters)
                {
                    var key = entry.Key as string;
                    if (key == null) continue;

                    if (possiblePrefixes.Any(p => string.Equals(p, key, StringComparison.OrdinalIgnoreCase)))
                    {
                        if (entry.Value is PSObject psoCandidate)
                        {
                            sourcePso = psoCandidate;
                            break;
                        }
                        if (entry.Value is IEnumerable enumerable)
                        {
                            foreach (var item in enumerable)
                            {
                                if (item is PSObject psoItem)
                                {
                                    sourcePso = psoItem;
                                    break;
                                }
                            }
                            if (sourcePso != null) break;
                        }
                    }
                }

                if (sourcePso == null)
                {
                    return Enumerable.Empty<CompletionResult>();
                }

                var propNames = sourcePso.Properties.Select(p => p.Name).Where(n => !string.IsNullOrWhiteSpace(n)).Distinct().ToList();
                if (!propNames.Any()) return Enumerable.Empty<CompletionResult>();

                if (string.IsNullOrEmpty(wordToComplete))
                {
                    return propNames.Take(200).Select(n => new CompletionResult(n, n, CompletionResultType.ParameterValue, baseName ?? ""));
                }

                var token = wordToComplete.Trim();
                var starts = propNames.Where(c => c.StartsWith(token, StringComparison.OrdinalIgnoreCase));
                var contains = propNames.Where(c => !c.StartsWith(token, StringComparison.OrdinalIgnoreCase) && c.IndexOf(token, StringComparison.OrdinalIgnoreCase) >= 0);
                return starts.Concat(contains).Select(c => new CompletionResult(c, c, CompletionResultType.ParameterValue, baseName ?? ""));
            }
            catch
            {
                return Enumerable.Empty<CompletionResult>();
            }
        }
    }
}
