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
    /// Provides tab-completion for form section names.
    /// If TabName is bound, returns sections from that tab only.
    /// If TabName is not bound but FormId is, returns all sections with their tab names.
    /// </summary>
    public class FormSectionNameArgumentCompleter : IArgumentCompleter
    {
        /// <summary>
        /// Returns completion candidates for section names.
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
                string tabNameFilter = null;

                // Look for Connection, FormId, and TabName parameters
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
                    }
                    else if (string.Equals(key, "TabName", StringComparison.OrdinalIgnoreCase))
                    {
                        if (entry.Value is string s && !string.IsNullOrWhiteSpace(s))
                        {
                            tabNameFilter = s;
                        }
                    }
                }

                // FormId is required to query sections
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

                var tabs = tabsElement.Elements("tab");
                
                // Filter by TabName if specified
                if (!string.IsNullOrEmpty(tabNameFilter))
                {
                    tabs = tabs.Where(t => t.Attribute("name")?.Value == tabNameFilter);
                }

                // Extract section information from all relevant tabs
                var sectionInfos = new List<dynamic>();
                foreach (var tab in tabs)
                {
                    string currentTabName = tab.Attribute("name")?.Value;
                    var columnsElement = tab.Element("columns");
                    if (columnsElement == null) continue;

                    foreach (var column in columnsElement.Elements("column"))
                    {
                        var sectionsElement = column.Element("sections");
                        if (sectionsElement == null) continue;

                        foreach (var section in sectionsElement.Elements("section"))
                        {
                            var sectionName = section.Attribute("name")?.Value;
                            if (string.IsNullOrEmpty(sectionName)) continue;

                            var sectionLabel = section.Element("labels")?.Element("label")?.Attribute("description")?.Value;
                            var visible = section.Attribute("visible")?.Value != "false";

                            sectionInfos.Add(new
                            {
                                Name = sectionName,
                                TabName = currentTabName,
                                Label = sectionLabel,
                                Visible = visible,
                                Id = section.Attribute("id")?.Value
                            });
                        }
                    }
                }

                if (!sectionInfos.Any())
                {
                    return Enumerable.Empty<CompletionResult>();
                }

                // Filter by word to complete
                IEnumerable<dynamic> filtered = sectionInfos;
                if (!string.IsNullOrEmpty(wordToComplete))
                {
                    string wc = wordToComplete.Trim().ToLowerInvariant();
                    filtered = sectionInfos.Where(s => 
                        s.Name.IndexOf(wc, StringComparison.OrdinalIgnoreCase) >= 0 ||
                        (!string.IsNullOrEmpty(s.Label) && s.Label.IndexOf(wc, StringComparison.OrdinalIgnoreCase) >= 0));
                }

                return filtered.Select(s =>
                {
                    // If TabName wasn't specified, include it in the display
                    string displayText;
                    if (string.IsNullOrEmpty(tabNameFilter))
                    {
                        displayText = string.IsNullOrEmpty(s.Label)
                            ? $"{s.Name} (Tab: {s.TabName})"
                            : $"{s.Name} — {s.Label} (Tab: {s.TabName})";
                    }
                    else
                    {
                        displayText = string.IsNullOrEmpty(s.Label)
                            ? s.Name
                            : $"{s.Name} — {s.Label}";
                    }

                    string toolTip = $"Section: {s.Name}\nTab: {s.TabName}\nLabel: {s.Label ?? "N/A"}\nVisible: {s.Visible}";
                    return new CompletionResult(s.Name, displayText, CompletionResultType.ParameterValue, toolTip);
                });
            }
            catch
            {
                return Enumerable.Empty<CompletionResult>();
            }
        }
    }
}
