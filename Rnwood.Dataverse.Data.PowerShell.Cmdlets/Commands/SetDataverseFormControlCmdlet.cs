using Microsoft.Crm.Sdk.Messages;
using Microsoft.PowerPlatform.Dataverse.Client;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Query;
using System;
using System.Linq;
using System.Management.Automation;
using System.Xml.Linq;

namespace Rnwood.Dataverse.Data.PowerShell.Commands
{
    /// <summary>
    /// Creates or updates a control in a Dataverse form section.
    /// </summary>
    [Cmdlet(VerbsCommon.Set, "DataverseFormControl", SupportsShouldProcess = true, ConfirmImpact = ConfirmImpact.Medium)]
    [OutputType(typeof(string))]
    public class SetDataverseFormControlCmdlet : OrganizationServiceCmdlet
    {
        /// <summary>
        /// Gets or sets the form ID.
        /// </summary>
        [Parameter(Mandatory = true, ValueFromPipelineByPropertyName = true, HelpMessage = "ID of the form")]
        [Alias("formid")]
        public Guid FormId { get; set; }

        /// <summary>
        /// Gets or sets the section name where the control is located.
        /// </summary>
        [Parameter(Mandatory = true, HelpMessage = "Name of the section containing the control")]
        public string SectionName { get; set; }

        /// <summary>
        /// Gets or sets the control ID for updating an existing control.
        /// </summary>
        [Parameter(ParameterSetName = "Update", Mandatory = true, HelpMessage = "ID of the control to update")]
        public string ControlId { get; set; }

        /// <summary>
        /// Gets or sets the data field name (attribute) for the control.
        /// </summary>
        [Parameter(ParameterSetName = "Create", Mandatory = true, HelpMessage = "Data field name (attribute) for the control")]
        [Parameter(ParameterSetName = "Update", Mandatory = true, HelpMessage = "Data field name (attribute) for the control")]
        public string DataField { get; set; }

        /// <summary>
        /// Gets or sets the raw XML for the control element.
        /// </summary>
        [Parameter(ParameterSetName = "RawXml", Mandatory = true, HelpMessage = "Raw XML for the control element")]
        public string ControlXml { get; set; }

        /// <summary>
        /// Gets or sets the control type.
        /// </summary>
        [Parameter(HelpMessage = "Control type (Standard, Lookup, OptionSet, DateTime, Boolean, Subgrid, WebResource, QuickForm, Spacer, IFrame, Timer, KBSearch, Notes)")]
        [ValidateSet("Standard", "Lookup", "OptionSet", "DateTime", "Boolean", "Subgrid", "WebResource", "QuickForm", "Spacer", "IFrame", "Timer", "KBSearch", "Notes")]
        public string ControlType { get; set; } = "Standard";

        /// <summary>
        /// Gets or sets the label for the control.
        /// </summary>
        [Parameter(HelpMessage = "Label text for the control")]
        public string Label { get; set; }

        /// <summary>
        /// Gets or sets the language code for the label (default: 1033 for English).
        /// </summary>
        [Parameter(HelpMessage = "Language code for the label (default: 1033)")]
        public int LanguageCode { get; set; } = 1033;

        /// <summary>
        /// Gets or sets whether the control is disabled.
        /// </summary>
        [Parameter(HelpMessage = "Whether the control is disabled")]
        public SwitchParameter Disabled { get; set; }

        /// <summary>
        /// Gets or sets whether the control is visible.
        /// </summary>
        [Parameter(HelpMessage = "Whether the control is visible")]
        public SwitchParameter Visible { get; set; } = true;

        /// <summary>
        /// Gets or sets the number of rows (for multiline text).
        /// </summary>
        [Parameter(HelpMessage = "Number of rows for multiline text controls")]
        public int? Rows { get; set; }

        /// <summary>
        /// Gets or sets the colspan for the control.
        /// </summary>
        [Parameter(HelpMessage = "Number of columns the control spans")]
        public int? ColSpan { get; set; }

        /// <summary>
        /// Gets or sets the rowspan for the control.
        /// </summary>
        [Parameter(HelpMessage = "Number of rows the control spans")]
        public int? RowSpan { get; set; }

        /// <summary>
        /// Gets or sets whether to show the label.
        /// </summary>
        [Parameter(HelpMessage = "Whether to show the control label")]
        public SwitchParameter ShowLabel { get; set; } = true;

        /// <summary>
        /// Gets or sets whether the control is required.
        /// </summary>
        [Parameter(HelpMessage = "Whether the control is required")]
        public SwitchParameter IsRequired { get; set; }

        /// <summary>
        /// Gets or sets custom parameters for special controls (WebResource URL, Subgrid settings, etc.).
        /// </summary>
        [Parameter(HelpMessage = "Custom parameters as a hashtable for special controls")]
        public System.Collections.Hashtable Parameters { get; set; }

        /// <summary>
        /// Gets or sets the zero-based index position where the control should be inserted in the section.
        /// </summary>
        [Parameter(ParameterSetName = "Create", HelpMessage = "Zero-based index position to insert the control")]
        [Parameter(ParameterSetName = "RawXml", HelpMessage = "Zero-based index position to insert the control")]
        public int? Index { get; set; }

        /// <summary>
        /// Gets or sets the ID or data field name of the control before which to insert this control.
        /// </summary>
        [Parameter(ParameterSetName = "Create", HelpMessage = "ID or data field name of the control before which to insert")]
        [Parameter(ParameterSetName = "RawXml", HelpMessage = "ID or data field name of the control before which to insert")]
        public string InsertBefore { get; set; }

        /// <summary>
        /// Gets or sets the ID or data field name of the control after which to insert this control.
        /// </summary>
        [Parameter(ParameterSetName = "Create", HelpMessage = "ID or data field name of the control after which to insert")]
        [Parameter(ParameterSetName = "RawXml", HelpMessage = "ID or data field name of the control after which to insert")]
        public string InsertAfter { get; set; }

        /// <summary>
        /// Gets or sets whether to return the control ID.
        /// </summary>
        [Parameter(HelpMessage = "Return the control ID")]
        public SwitchParameter PassThru { get; set; }

        /// <summary>
        /// Gets or sets whether to publish the form after modifying the control.
        /// </summary>
        [Parameter(HelpMessage = "Publish the form after modifying the control")]
        public SwitchParameter Publish { get; set; }

        /// <summary>
        /// Processes the cmdlet request.
        /// </summary>
        protected override void ProcessRecord()
        {
            base.ProcessRecord();

            // Retrieve the form
            Entity form = Connection.Retrieve("systemform", FormId, new ColumnSet("formxml", "objecttypecode"));
            
            if (!form.Contains("formxml"))
            {
                throw new InvalidOperationException($"Form '{FormId}' does not contain FormXml");
            }

            string formXml = form.GetAttributeValue<string>("formxml");
            XDocument doc = XDocument.Parse(formXml);
            XElement systemForm = doc.Root?.Element("SystemForm");
            
            if (systemForm == null)
            {
                throw new InvalidOperationException("Invalid FormXml structure");
            }

            // Find the section
            var tabsElement = systemForm.Element("tabs");
            if (tabsElement == null)
            {
                throw new InvalidOperationException("Form has no tabs");
            }

            XElement targetSection = tabsElement.Elements("tab")
                .SelectMany(t => t.Element("columns")?.Elements("column") ?? Enumerable.Empty<XElement>())
                .SelectMany(col => col.Element("sections")?.Elements("section") ?? Enumerable.Empty<XElement>())
                .FirstOrDefault(s => s.Attribute("name")?.Value == SectionName);

            if (targetSection == null)
            {
                throw new InvalidOperationException($"Section '{SectionName}' not found in form");
            }

            XElement rowsElement = targetSection.Element("rows");
            if (rowsElement == null)
            {
                rowsElement = new XElement("rows");
                targetSection.Add(rowsElement);
            }

            XElement control = null;
            XElement cell = null;
            string controlId = null;
            bool isUpdate = ParameterSetName == "Update";
            bool isRawXml = ParameterSetName == "RawXml";

            if (isRawXml)
            {
                // Parse the raw XML
                try
                {
                    control = XElement.Parse(ControlXml);
                }
                catch (Exception ex)
                {
                    throw new InvalidOperationException($"Failed to parse ControlXml: {ex.Message}", ex);
                }

                // Extract control ID from the XML
                controlId = control.Attribute("id")?.Value;
                if (string.IsNullOrEmpty(controlId))
                {
                    throw new InvalidOperationException("ControlXml must have an 'id' attribute");
                }

                // Check if control with same ID already exists (for update)
                foreach (var row in rowsElement.Elements("row"))
                {
                    foreach (var c in row.Elements("cell"))
                    {
                        var ctrl = c.Elements("control").FirstOrDefault(ct => ct.Attribute("id")?.Value == controlId);
                        if (ctrl != null)
                        {
                            // Update existing control by replacing it
                            ctrl.ReplaceWith(control);
                            cell = c;
                            isUpdate = true;
                            break;
                        }
                    }
                    if (cell != null) break;
                }

                if (!isUpdate)
                {
                    // Create new control with positioning
                    cell = new XElement("cell", control);
                    XElement row = new XElement("row", cell);
                    
                    // Handle positioning (same logic as before)
                    if (Index.HasValue)
                    {
                        var rows = rowsElement.Elements("row").ToList();
                        if (Index.Value >= 0 && Index.Value <= rows.Count)
                        {
                            if (Index.Value == rows.Count)
                            {
                                rowsElement.Add(row);
                            }
                            else
                            {
                                rows[Index.Value].AddBeforeSelf(row);
                            }
                        }
                        else
                        {
                            throw new InvalidOperationException($"Index {Index.Value} is out of range. Valid range is 0-{rows.Count}");
                        }
                    }
                    else if (!string.IsNullOrEmpty(InsertBefore))
                    {
                        XElement referenceRow = null;
                        foreach (var r in rowsElement.Elements("row"))
                        {
                            foreach (var c in r.Elements("cell"))
                            {
                                var ctrl = c.Elements("control").FirstOrDefault(ct => 
                                    ct.Attribute("id")?.Value == InsertBefore || 
                                    ct.Attribute("datafieldname")?.Value == InsertBefore);
                                if (ctrl != null)
                                {
                                    referenceRow = r;
                                    break;
                                }
                            }
                            if (referenceRow != null) break;
                        }
                        
                        if (referenceRow == null)
                        {
                            throw new InvalidOperationException($"Reference control '{InsertBefore}' not found");
                        }
                        referenceRow.AddBeforeSelf(row);
                    }
                    else if (!string.IsNullOrEmpty(InsertAfter))
                    {
                        XElement referenceRow = null;
                        foreach (var r in rowsElement.Elements("row"))
                        {
                            foreach (var c in r.Elements("cell"))
                            {
                                var ctrl = c.Elements("control").FirstOrDefault(ct => 
                                    ct.Attribute("id")?.Value == InsertAfter || 
                                    ct.Attribute("datafieldname")?.Value == InsertAfter);
                                if (ctrl != null)
                                {
                                    referenceRow = r;
                                    break;
                                }
                            }
                            if (referenceRow != null) break;
                        }
                        
                        if (referenceRow == null)
                        {
                            throw new InvalidOperationException($"Reference control '{InsertAfter}' not found");
                        }
                        referenceRow.AddAfterSelf(row);
                    }
                    else
                    {
                        rowsElement.Add(row);
                    }
                }
            }
            else if (isUpdate)
            {
                // Find existing control
                foreach (var row in rowsElement.Elements("row"))
                {
                    foreach (var c in row.Elements("cell"))
                    {
                        var ctrl = c.Elements("control").FirstOrDefault(ct => ct.Attribute("id")?.Value == ControlId);
                        if (ctrl != null)
                        {
                            control = ctrl;
                            cell = c;
                            break;
                        }
                    }
                    if (control != null) break;
                }

                if (control == null)
                {
                    throw new InvalidOperationException($"Control with ID '{ControlId}' not found in section '{SectionName}'");
                }
                controlId = ControlId;
            }
            else
            {
                // Check if control with same data field exists
                foreach (var row in rowsElement.Elements("row"))
                {
                    foreach (var c in row.Elements("cell"))
                    {
                        var ctrl = c.Elements("control").FirstOrDefault(ct => ct.Attribute("datafieldname")?.Value == DataField);
                        if (ctrl != null)
                        {
                            control = ctrl;
                            cell = c;
                            isUpdate = true;
                            controlId = ctrl.Attribute("id")?.Value ?? DataField;
                            break;
                        }
                    }
                    if (control != null) break;
                }

                if (control == null)
                {
                    // Create new control
                    controlId = DataField;
                    control = new XElement("control");
                    cell = new XElement("cell", control);
                    XElement row = new XElement("row", cell);
                    
                    // Handle positioning
                    if (Index.HasValue)
                    {
                        // Insert at specific index (row-level)
                        var rows = rowsElement.Elements("row").ToList();
                        if (Index.Value >= 0 && Index.Value <= rows.Count)
                        {
                            if (Index.Value == rows.Count)
                            {
                                rowsElement.Add(row);
                            }
                            else
                            {
                                rows[Index.Value].AddBeforeSelf(row);
                            }
                        }
                        else
                        {
                            throw new InvalidOperationException($"Index {Index.Value} is out of range. Valid range is 0-{rows.Count}");
                        }
                    }
                    else if (!string.IsNullOrEmpty(InsertBefore))
                    {
                        // Find reference control and insert before its row
                        XElement referenceRow = null;
                        foreach (var r in rowsElement.Elements("row"))
                        {
                            foreach (var c in r.Elements("cell"))
                            {
                                var ctrl = c.Elements("control").FirstOrDefault(ct => 
                                    ct.Attribute("id")?.Value == InsertBefore || 
                                    ct.Attribute("datafieldname")?.Value == InsertBefore);
                                if (ctrl != null)
                                {
                                    referenceRow = r;
                                    break;
                                }
                            }
                            if (referenceRow != null) break;
                        }
                        
                        if (referenceRow == null)
                        {
                            throw new InvalidOperationException($"Reference control '{InsertBefore}' not found");
                        }
                        referenceRow.AddBeforeSelf(row);
                    }
                    else if (!string.IsNullOrEmpty(InsertAfter))
                    {
                        // Find reference control and insert after its row
                        XElement referenceRow = null;
                        foreach (var r in rowsElement.Elements("row"))
                        {
                            foreach (var c in r.Elements("cell"))
                            {
                                var ctrl = c.Elements("control").FirstOrDefault(ct => 
                                    ct.Attribute("id")?.Value == InsertAfter || 
                                    ct.Attribute("datafieldname")?.Value == InsertAfter);
                                if (ctrl != null)
                                {
                                    referenceRow = r;
                                    break;
                                }
                            }
                            if (referenceRow != null) break;
                        }
                        
                        if (referenceRow == null)
                        {
                            throw new InvalidOperationException($"Reference control '{InsertAfter}' not found");
                        }
                        referenceRow.AddAfterSelf(row);
                    }
                    else
                    {
                        // Add at the end (default)
                        rowsElement.Add(row);
                    }
                }
            }

            // Update control attributes (only for non-RawXml parameter sets)
            if (!isRawXml)
            {
                control.SetAttributeValue("id", controlId);
                control.SetAttributeValue("datafieldname", DataField);
                
                if (MyInvocation.BoundParameters.ContainsKey(nameof(ControlType)))
                {
                    control.SetAttributeValue("classid", GetClassId(ControlType));
                }

                if (MyInvocation.BoundParameters.ContainsKey(nameof(Disabled)))
                {
                    if (Disabled.IsPresent)
                    {
                        control.SetAttributeValue("disabled", "true");
                    }
                    else
                    {
                        control.SetAttributeValue("disabled", null);
                    }
                }

                if (MyInvocation.BoundParameters.ContainsKey(nameof(Visible)))
                {
                    if (!Visible.IsPresent)
                    {
                        control.SetAttributeValue("visible", "false");
                    }
                    else
                    {
                        control.SetAttributeValue("visible", null);
                    }
                }

                if (MyInvocation.BoundParameters.ContainsKey(nameof(ShowLabel)))
                {
                    if (!ShowLabel.IsPresent)
                    {
                        control.SetAttributeValue("showlabel", "false");
                    }
                    else
                    {
                        control.SetAttributeValue("showlabel", null);
                    }
                }

                if (MyInvocation.BoundParameters.ContainsKey(nameof(IsRequired)))
                {
                    if (IsRequired.IsPresent)
                    {
                        control.SetAttributeValue("isrequired", "true");
                    }
                    else
                    {
                        control.SetAttributeValue("isrequired", null);
                    }
                }

                // Update cell attributes
                if (ColSpan.HasValue)
                {
                    cell.SetAttributeValue("colspan", ColSpan.Value);
                }

                if (RowSpan.HasValue)
                {
                    cell.SetAttributeValue("rowspan", RowSpan.Value);
                }

                // Update or add labels
                if (!string.IsNullOrEmpty(Label))
                {
                    XElement labelsElement = control.Element("labels");
                    if (labelsElement == null)
                    {
                        labelsElement = new XElement("labels");
                        control.Add(labelsElement);
                    }
                    else
                    {
                        labelsElement.RemoveAll();
                    }
                    
                    labelsElement.Add(new XElement("label",
                        new XAttribute("description", Label),
                        new XAttribute("languagecode", LanguageCode)
                    ));
                }

                // Add parameters for special controls
                if (Parameters != null && Parameters.Count > 0)
                {
                    XElement paramsElement = control.Element("parameters");
                    if (paramsElement == null)
                    {
                        paramsElement = new XElement("parameters");
                        control.Add(paramsElement);
                    }
                    else
                    {
                        paramsElement.RemoveAll();
                    }

                    foreach (System.Collections.DictionaryEntry param in Parameters)
                    {
                        paramsElement.Add(new XElement(param.Key.ToString(), param.Value?.ToString()));
                    }
                }
            }

            // Confirm action
            string action = isUpdate ? "Update" : "Create";
            string controlDescriptor = isRawXml ? $"control (from XML)" : $"control '{DataField}'";
            if (!ShouldProcess($"form '{FormId}', section '{SectionName}'", $"{action} {controlDescriptor}"))
            {
                return;
            }

            // Update form
            Entity updateForm = new Entity("systemform", FormId);
            updateForm["formxml"] = doc.ToString();
            Connection.Update(updateForm);

            WriteVerbose($"{action}d {controlDescriptor} in section '{SectionName}' on form '{FormId}'");

            // Publish if requested
            if (Publish.IsPresent)
            {
                string entityName = form.GetAttributeValue<string>("objecttypecode");
                WriteVerbose($"Publishing entity '{entityName}'...");
                var publishRequest = new PublishXmlRequest
                {
                    ParameterXml = $"<importexportxml><entities><entity>{entityName}</entity></entities></importexportxml>"
                };
                Connection.Execute(publishRequest);
                WriteVerbose("Entity published successfully");
            }

            if (PassThru.IsPresent)
            {
                WriteObject(controlId);
            }
        }

        private string GetClassId(string controlType)
        {
            // Class IDs for different control types
            switch (controlType)
            {
                case "Standard":
                    return "{4273EDBD-AC1D-40D3-9FB2-095C621B552D}"; // Standard text field
                case "Lookup":
                    return "{270BD3DB-D9AF-4782-9025-509E298DEC0A}"; // Lookup control
                case "OptionSet":
                    return "{3EF39988-22BB-4F0B-BBBE-64B5A3748AEE}"; // Option set (dropdown)
                case "DateTime":
                    return "{5B773807-9FB2-42DB-97C3-7A91EFF8ADFF}"; // Date and time picker
                case "Boolean":
                    return "{67FAC785-CD58-4F9F-ABB3-4B7DDC6ED5ED}"; // Two option (boolean)
                case "Subgrid":
                    return "{E7A81278-8635-4d9e-8D4D-59480B391C5B}"; // Subgrid
                case "WebResource":
                    return "{9FDF5F91-88B1-47f4-AD53-C11EFC01A01D}"; // Web resource
                case "QuickForm":
                    return "{5C5600E0-1D6E-4205-A272-BE80DA87FD42}"; // Quick view form
                case "Spacer":
                    return "{5546E6CD-394C-4BEE-94A8-4B09EB3A5C4A}"; // Spacer
                case "IFrame":
                    return "{5C5600E0-1D6E-4205-A272-BE80DA87FD42}"; // IFrame
                case "Timer":
                    return "{B0C6723A-8503-4FD7-BB28-C8A06AC933C2}"; // Timer control
                case "KBSearch":
                    return "{5C5600E0-1D6E-4205-A272-BE80DA87FD42}"; // Knowledge base search
                case "Notes":
                    return "{06375649-c143-495e-a496-c962e5b4488e}"; // Notes control
                default:
                    return "{4273EDBD-AC1D-40D3-9FB2-095C621B552D}"; // Default to standard
            }
        }
    }
}
