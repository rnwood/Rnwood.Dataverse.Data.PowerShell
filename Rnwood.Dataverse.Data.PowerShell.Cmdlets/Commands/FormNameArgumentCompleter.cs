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
    /// Provides tab-completion for form names.
    /// Optionally filters forms by entity/table name if found in bound parameters.
    /// </summary>
    public class FormNameArgumentCompleter : IArgumentCompleter
    {
        /// <summary>
        /// Returns completion candidates for form names with optional entity filtering.
        /// If a Connection parameter is bound it will be used to retrieve forms.
        /// If an Entity/TableName parameter is bound, only forms for that entity are returned.
        /// </summary>
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

                // Look for Connection and Entity parameters
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

                // If no explicit connection, try default
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
                    ColumnSet = new ColumnSet("name", "uniquename", "objecttypecode", "type"),
                    TopCount = 500 // Limit for performance
                };

                // Apply entity filter if available
                if (!string.IsNullOrEmpty(entityFilter))
                {
                    query.Criteria.AddCondition("objecttypecode", ConditionOperator.Equal, entityFilter);
                }

                // Apply name filter if word to complete is provided
                if (!string.IsNullOrEmpty(wordToComplete))
                {
                    query.Criteria.AddCondition("name", ConditionOperator.Like, $"%{wordToComplete}%");
                }

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
                    Name = e.GetAttributeValue<string>("name"),
                    UniqueName = e.GetAttributeValue<string>("uniquename"),
                    Entity = e.GetAttributeValue<string>("objecttypecode"),
                    Type = e.GetAttributeValue<OptionSetValue>("type")?.Value ?? 0
                }).Where(f => !string.IsNullOrEmpty(f.Name)).ToList();

                // Order by entity, then by type, then by name
                var orderedForms = forms.OrderBy(f => f.Entity).ThenBy(f => f.Type).ThenBy(f => f.Name);

                return orderedForms.Select(f =>
                {
                    string formType = GetFormTypeDescription(f.Type);
                    string displayText = string.IsNullOrEmpty(entityFilter)
                        ? $"{f.Name} ({f.Entity} — {formType})"
                        : $"{f.Name} ({formType})";
                    string toolTip = $"{formType}: {f.Name}\nEntity: {f.Entity}\nUnique Name: {f.UniqueName ?? "N/A"}";
                    return new CompletionResult(f.Name, displayText, CompletionResultType.ParameterValue, toolTip);
                });
            }
            catch
            {
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
