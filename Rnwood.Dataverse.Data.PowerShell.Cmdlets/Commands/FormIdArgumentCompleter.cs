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
    /// Provides tab-completion for parameters that accept form IDs (Guid).
    /// Optionally filters forms by entity/table name if found in bound parameters.
    /// </summary>
    public class FormIdArgumentCompleter : IArgumentCompleter
    {
        /// <summary>
        /// Returns completion candidates for form IDs with optional entity filtering.
        /// If a Connection parameter is bound it will be used to retrieve forms.
        /// If an Entity/TableName parameter is bound, only forms for that entity are returned.
        /// </summary>
        /// <param name="commandName">The name of the command.</param>
        /// <param name="parameterName">The name of the parameter being completed.</param>
        /// <param name="wordToComplete">The partial word being completed.</param>
        /// <param name="commandAst">The AST of the command.</param>
        /// <param name="fakeBoundParameters">Dictionary of bound parameters.</param>
        /// <returns>Sequence of CompletionResult objects for matching form IDs.</returns>
        public IEnumerable<CompletionResult> CompleteArgument(string commandName, string parameterName, string wordToComplete, CommandAst commandAst, IDictionary fakeBoundParameters)
        {
            try
            {
                if (fakeBoundParameters == null)
                {
                    return Enumerable.Empty<CompletionResult>();
                }

                ServiceClient connectionObj = null;
                string entityFilter = null;

                // Look for Connection and Entity parameters in bound parameters
                foreach (DictionaryEntry entry in fakeBoundParameters)
                {
                    var key = entry.Key as string;
                    if (string.IsNullOrEmpty(key))
                        continue;

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
                    }
                    else if (string.Equals(key, "Entity", StringComparison.OrdinalIgnoreCase) ||
                             string.Equals(key, "TableName", StringComparison.OrdinalIgnoreCase) ||
                             string.Equals(key, "EntityName", StringComparison.OrdinalIgnoreCase))
                    {
                        if (entry.Value is string s && !string.IsNullOrWhiteSpace(s))
                        {
                            entityFilter = s;
                        }
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

                // Query forms from Dataverse
                var query = new QueryExpression("systemform")
                {
                    ColumnSet = new ColumnSet("formid", "name", "uniquename", "objecttypecode", "type"),
                    TopCount = 200 // Limit to top 200 for performance
                };

                // Apply entity filter if available
                if (!string.IsNullOrEmpty(entityFilter))
                {
                    query.Criteria.AddCondition("objecttypecode", ConditionOperator.Equal, entityFilter);
                }

                // Filter to only published forms by default for better UX
                // (users typically work with published forms, but include unpublished if needed)
                
                // Execute query (use RetrieveUnpublishedMultiple to include unpublished forms)
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

                // Extract form information
                var forms = results.Entities.Select(e => new
                {
                    FormId = e.GetAttributeValue<Guid>("formid"),
                    Name = e.GetAttributeValue<string>("name"),
                    UniqueName = e.GetAttributeValue<string>("uniquename"),
                    Entity = e.GetAttributeValue<string>("objecttypecode"),
                    Type = e.GetAttributeValue<OptionSetValue>("type")?.Value ?? 0
                }).Where(f => f.FormId != Guid.Empty).ToList();

                // If word to complete is a partial GUID, try to filter by it
                Guid partialGuid;
                bool isGuidSearch = Guid.TryParse(wordToComplete, out partialGuid);

                IEnumerable<CompletionResult> completionResults;

                if (isGuidSearch)
                {
                    // Filter by exact or partial GUID match
                    var guidMatches = forms.Where(f => 
                        f.FormId.ToString().StartsWith(wordToComplete, StringComparison.OrdinalIgnoreCase) ||
                        f.FormId.ToString().IndexOf(wordToComplete, StringComparison.OrdinalIgnoreCase) >= 0);
                    
                    completionResults = guidMatches.Select(f =>
                    {
                        string formType = GetFormTypeDescription(f.Type);
                        string displayText = string.IsNullOrEmpty(f.Name)
                            ? $"{f.FormId}"
                            : $"{f.FormId} — {f.Name}";
                        string toolTip = $"{formType}: {f.Name ?? "Unnamed"}\nEntity: {f.Entity}\nUnique Name: {f.UniqueName ?? "N/A"}";
                        return new CompletionResult(f.FormId.ToString(), displayText, CompletionResultType.ParameterValue, toolTip);
                    });
                }
                else
                {
                    // Filter by form name or unique name if word provided
                    IEnumerable<dynamic> filteredForms = forms;
                    
                    if (!string.IsNullOrEmpty(wordToComplete))
                    {
                        string wcLower = wordToComplete.ToLowerInvariant();
                        filteredForms = forms.Where(f =>
                            (!string.IsNullOrEmpty(f.Name) && f.Name.IndexOf(wcLower, StringComparison.OrdinalIgnoreCase) >= 0) ||
                            (!string.IsNullOrEmpty(f.UniqueName) && f.UniqueName.IndexOf(wcLower, StringComparison.OrdinalIgnoreCase) >= 0));
                    }

                    // Order by entity, then by type, then by name
                    var orderedForms = filteredForms.OrderBy(f => f.Entity).ThenBy(f => f.Type).ThenBy(f => f.Name);

                    completionResults = orderedForms.Select(f =>
                    {
                        string formType = GetFormTypeDescription(f.Type);
                        string displayText = string.IsNullOrEmpty(f.Name)
                            ? $"{f.FormId} ({f.Entity} — {formType})"
                            : $"{f.Name} ({f.Entity} — {formType})";
                        string completionText = f.FormId.ToString();
                        string toolTip = $"{formType}: {f.Name ?? "Unnamed"}\nEntity: {f.Entity}\nUnique Name: {f.UniqueName ?? "N/A"}\nForm ID: {f.FormId}";
                        return new CompletionResult(completionText, displayText, CompletionResultType.ParameterValue, toolTip);
                    });
                }

                return completionResults;
            }
            catch
            {
                // If completion fails for any reason, return no results rather than throwing
                return Enumerable.Empty<CompletionResult>();
            }
        }

        /// <summary>
        /// Gets a human-readable description of a form type code.
        /// </summary>
        private string GetFormTypeDescription(int typeCode)
        {
            switch (typeCode)
            {
                case 0: return "Dashboard";
                case 1: return "AppointmentBook";
                case 2: return "Main";
                case 3: return "MiniCampaignBO";
                case 4: return "Preview";
                case 5: return "Mobile-Express";
                case 6: return "Quick View";
                case 7: return "Quick Create";
                case 8: return "Dialog";
                case 9: return "Task Flow";
                case 10: return "InteractionCentric Dashboard";
                case 11: return "Card";
                case 12: return "Main Interactive";
                case 13: return "Contextual Dashboard";
                case 100: return "Other";
                case 101: return "MainBackup";
                case 102: return "AppointmentBookBackup";
                case 103: return "Power BI Dashboard";
                default: return "Unknown";
            }
        }
    }
}
