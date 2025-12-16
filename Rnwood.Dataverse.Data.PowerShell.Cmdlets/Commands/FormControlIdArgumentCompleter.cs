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
    /// Provides tab-completion for form control IDs.
    /// If SectionName/TabName are bound, returns controls from that context.
    /// If only FormId is bound, returns all controls with their location context.
    /// </summary>
    public class FormControlIdArgumentCompleter : IArgumentCompleter
    {
        /// <summary>
        /// Returns completion candidates for control IDs.
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
                string sectionNameFilter = null;

                // Look for Connection, FormId, TabName, and SectionName parameters
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
                    else if (string.Equals(key, "SectionName", StringComparison.OrdinalIgnoreCase))
                    {
                        if (entry.Value is string s && !string.IsNullOrWhiteSpace(s))
                        {
                            sectionNameFilter = s;
                        }
                    }
                }

                // FormId is required to query controls
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
                
                // Get controls using the FormXmlHelper.GetControls method
                var controlInfos = new List<dynamic>();
                foreach (var (tabName, sectionName, control) in FormXmlHelper.GetControls(systemForm, tabNameFilter, sectionNameFilter, null, null))
                {
                    var controlId = control.Attribute("id")?.Value;
                    if (string.IsNullOrEmpty(controlId)) continue;

                    var dataField = control.Attribute("datafieldname")?.Value;
                    var classId = control.Attribute("classid")?.Value;
                    var disabled = control.Attribute("disabled")?.Value == "true";
                    var visible = control.Attribute("visible")?.Value != "false";

                    controlInfos.Add(new
                    {
                        Id = controlId,
                        TabName = tabName,
                        SectionName = sectionName,
                        DataField = dataField,
                        ClassId = classId,
                        Disabled = disabled,
                        Visible = visible
                    });
                }

                if (!controlInfos.Any())
                {
                    return Enumerable.Empty<CompletionResult>();
                }

                // Filter by word to complete
                IEnumerable<dynamic> filtered = controlInfos;
                if (!string.IsNullOrEmpty(wordToComplete))
                {
                    string wc = wordToComplete.Trim().ToLowerInvariant();
                    filtered = controlInfos.Where(c => 
                        c.Id.IndexOf(wc, StringComparison.OrdinalIgnoreCase) >= 0 ||
                        (!string.IsNullOrEmpty(c.DataField) && c.DataField.IndexOf(wc, StringComparison.OrdinalIgnoreCase) >= 0));
                }

                return filtered.Select(c =>
                {
                    // Build display text based on what filters are active
                    string displayText;
                    if (string.IsNullOrEmpty(tabNameFilter) && string.IsNullOrEmpty(sectionNameFilter))
                    {
                        // Show full context
                        displayText = string.IsNullOrEmpty(c.DataField)
                            ? $"{c.Id} (Tab: {c.TabName}, Section: {c.SectionName})"
                            : $"{c.Id} — {c.DataField} (Tab: {c.TabName}, Section: {c.SectionName})";
                    }
                    else if (string.IsNullOrEmpty(sectionNameFilter))
                    {
                        // Show section context
                        displayText = string.IsNullOrEmpty(c.DataField)
                            ? $"{c.Id} (Section: {c.SectionName})"
                            : $"{c.Id} — {c.DataField} (Section: {c.SectionName})";
                    }
                    else
                    {
                        // Just show the ID and data field
                        displayText = string.IsNullOrEmpty(c.DataField)
                            ? c.Id
                            : $"{c.Id} — {c.DataField}";
                    }

                    string toolTip = $"Control ID: {c.Id}\nData Field: {c.DataField ?? "N/A"}\nTab: {c.TabName}\nSection: {c.SectionName}\nVisible: {c.Visible}\nDisabled: {c.Disabled}";
                    return new CompletionResult(c.Id, displayText, CompletionResultType.ParameterValue, toolTip);
                });
            }
            catch
            {
                return Enumerable.Empty<CompletionResult>();
            }
        }
    }
}
