using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Management.Automation;
using System.Management.Automation.Language;
using Microsoft.PowerPlatform.Dataverse.Client;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Query;
using System.Xml.Linq;
using Rnwood.Dataverse.Data.PowerShell.Model;

namespace Rnwood.Dataverse.Data.PowerShell.Commands
{
    /// <summary>
    /// Provides tab-completion for form tab names.
    /// Queries the form specified by FormId parameter to retrieve available tabs.
    /// </summary>
    public class FormTabNameArgumentCompleter : IArgumentCompleter
    {
        /// <summary>
        /// Returns completion candidates for tab names from a form.
        /// Requires FormId parameter to be bound.
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
                Guid? formId = null;

                // Look for Connection and FormId parameters
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
                    else if (string.Equals(key, "FormId", StringComparison.OrdinalIgnoreCase))
                    {
                        if (entry.Value is Guid guid)
                        {
                            formId = guid;
                        }
                        else if (entry.Value is PSObject pso && pso.BaseObject is Guid guid2)
                        {
                            formId = guid2;
                        }
                        else if (entry.Value is string s && Guid.TryParse(s, out Guid parsedGuid))
                        {
                            formId = parsedGuid;
                        }
                    }
                }

                // FormId is required to query tabs
                if (!formId.HasValue)
                {
                    return Enumerable.Empty<CompletionResult>();
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

                // Retrieve the form
                Entity form = FormXmlHelper.RetrieveForm(connectionObj, formId.Value, new ColumnSet("formxml"));
                if (form == null || !form.Contains("formxml"))
                {
                    return Enumerable.Empty<CompletionResult>();
                }

                var (doc, systemForm) = FormXmlHelper.ParseFormXml(form);
                var tabsElement = systemForm.Element("tabs");
                if (tabsElement == null)
                {
                    return Enumerable.Empty<CompletionResult>();
                }

                var tabs = tabsElement.Elements("tab").ToList();
                if (!tabs.Any())
                {
                    return Enumerable.Empty<CompletionResult>();
                }

                // Extract tab information
                var tabInfos = tabs.Select(t => new
                {
                    Name = t.Attribute("name")?.Value,
                    Id = t.Attribute("id")?.Value,
                    Visible = t.Attribute("visible")?.Value != "false",
                    Label = t.Element("labels")?.Element("label")?.Attribute("description")?.Value
                }).Where(t => !string.IsNullOrEmpty(t.Name)).ToList();

                // Filter by word to complete
                IEnumerable<dynamic> filtered = tabInfos;
                if (!string.IsNullOrEmpty(wordToComplete))
                {
                    string wc = wordToComplete.Trim().ToLowerInvariant();
                    filtered = tabInfos.Where(t => 
                        t.Name.IndexOf(wc, StringComparison.OrdinalIgnoreCase) >= 0 ||
                        (!string.IsNullOrEmpty(t.Label) && t.Label.IndexOf(wc, StringComparison.OrdinalIgnoreCase) >= 0));
                }

                return filtered.Select(t =>
                {
                    string displayText = string.IsNullOrEmpty(t.Label)
                        ? t.Name
                        : $"{t.Name} — {t.Label}";
                    string toolTip = $"Tab: {t.Name}\nLabel: {t.Label ?? "N/A"}\nVisible: {t.Visible}";
                    return new CompletionResult(t.Name, displayText, CompletionResultType.ParameterValue, toolTip);
                });
            }
            catch
            {
                return Enumerable.Empty<CompletionResult>();
            }
        }
    }
}
