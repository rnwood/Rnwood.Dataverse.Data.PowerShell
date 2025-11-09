using Microsoft.Crm.Sdk.Messages;
using Microsoft.PowerPlatform.Dataverse.Client;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Query;
using Rnwood.Dataverse.Data.PowerShell.Model;
using System;
using System.Linq;
using System.Management.Automation;
using System.ServiceModel;
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
        public Guid FormId { get; set; }

        /// <summary>
        /// Gets or sets the section name where the control is located.
        /// </summary>
        [Parameter(Mandatory = true, HelpMessage = "Name of the section containing the control")]
        public string SectionName { get; set; }

        /// <summary>
        /// Gets or sets the tab name where the section is located.
        /// </summary>
        [Parameter(Mandatory = true, HelpMessage = "Name of the tab containing the section")]
        public string TabName { get; set; }

        /// <summary>
        /// Gets or sets the control ID for updating an existing control.
        /// </summary>
        [Parameter(Mandatory = false, HelpMessage = "ID of the control to create or update")]
        public string ControlId { get; set; }

        /// <summary>
        /// Gets or sets the data field name (attribute) for the control.
        /// </summary>
        [Parameter(Mandatory = true, ParameterSetName = "Default", HelpMessage = "Data field name (attribute) for the control")]
        [Parameter(Mandatory = false, ParameterSetName = "RawXml", HelpMessage = "Data field name (attribute) for the control")]
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
        /// Gets or sets whether the control is hidden.
        /// </summary>
        [Parameter(HelpMessage = "Whether the control is hidden")]
        public SwitchParameter Hidden { get; set; } = true;

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
        [Parameter(HelpMessage = "Zero-based index position to insert the control")]
        public int? Index { get; set; }

        /// <summary>
        /// Gets or sets the ID or data field name of the control before which to insert this control.
        /// </summary>
        [Parameter(HelpMessage = "ID or data field name of the control before which to insert")]
        public string InsertBefore { get; set; }

        /// <summary>
        /// Gets or sets the ID or data field name of the control after which to insert this control.
        /// </summary>
        [Parameter(HelpMessage = "ID or data field name of the control after which to insert")]
        public string InsertAfter { get; set; }

        /// <summary>
        /// Gets or sets whether to return the control ID.
        /// </summary>
        [Parameter(HelpMessage = "Return the control ID")]
        public SwitchParameter PassThru { get; set; }

        /// <summary>
        /// Processes the cmdlet request.
        /// </summary>
        protected override void ProcessRecord()
        {
            base.ProcessRecord();

            // Retrieve the form
            Entity form = FormXmlHelper.RetrieveForm(Connection, FormId, new ColumnSet("formxml", "objecttypecode"));
            var (doc, systemForm) = FormXmlHelper.ParseFormXml(form);

            // Find the section
            var sectionResult = FormXmlHelper.FindTargetSection(systemForm, TabName, SectionName);
            if (sectionResult.Section == null)
            {
                throw new InvalidOperationException($"Section '{SectionName}' not found in form");
            }

            XElement targetSection = sectionResult.Section;
            XElement rowsElement = FormXmlHelper.GetOrCreateRowsElement(targetSection);

            XElement control = null;
            XElement cell = null;
            string controlId = ControlId;
            bool isUpdate = false;

            if (ParameterSetName == "RawXml")
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

                // Determine control ID
                if (!string.IsNullOrEmpty(ControlId))
                {
                    controlId = ControlId;
                    control.SetAttributeValue("id", controlId);
                    
                    // Check if control with same ID already exists
                    var (existingControl, existingCell) = FormXmlHelper.FindControlById(systemForm, TabName, SectionName, controlId);
                    if (existingControl != null)
                    {
                        // Update existing control by replacing it
                        existingControl.ReplaceWith(control);
                        cell = existingCell;
                        isUpdate = true;
                    }
                    else
                    {
                        // Create new control
                        cell = new XElement("cell", control);
                        XElement row = new XElement("row", cell);
                        
                        // Handle positioning
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
                            // Insert before specified control
                            var referenceRow = FormXmlHelper.FindControlRowForPositioning(systemForm, TabName, SectionName, InsertBefore);
                            if (referenceRow == null)
                            {
                                throw new InvalidOperationException($"Reference control '{InsertBefore}' not found");
                            }
                            referenceRow.AddBeforeSelf(row);
                        }
                        else if (!string.IsNullOrEmpty(InsertAfter))
                        {
                            // Insert after specified control
                            var referenceRow = FormXmlHelper.FindControlRowForPositioning(systemForm, TabName, SectionName, InsertAfter);
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
                else
                {
                    // ControlId not provided, use DataField + SectionName as key
                    string dataFieldName = control.Attribute("datafieldname")?.Value;
                    if (!string.IsNullOrEmpty(dataFieldName))
                    {
                        // Check if control with same DataField already exists in section
                        var (existingControl, existingCell) = FormXmlHelper.FindControlByDataField(systemForm, TabName, SectionName, dataFieldName);
                        if (existingControl != null)
                        {
                            // Update existing control by replacing it
                            controlId = existingControl.Attribute("id")?.Value;
                            control.SetAttributeValue("id", controlId);
                            existingControl.ReplaceWith(control);
                            cell = existingCell;
                            isUpdate = true;
                        }
                        else
                        {
                            // Create new control
                            controlId = Guid.NewGuid().ToString();
                            control.SetAttributeValue("id", controlId);
                            cell = new XElement("cell", control);
                            XElement row = new XElement("row", cell);
                            
                            // Handle positioning
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
                                // Insert before specified control
                                var referenceRow = FormXmlHelper.FindControlRowForPositioning(systemForm, TabName, SectionName, InsertBefore);
                                if (referenceRow == null)
                                {
                                    throw new InvalidOperationException($"Reference control '{InsertBefore}' not found");
                                }
                                referenceRow.AddBeforeSelf(row);
                            }
                            else if (!string.IsNullOrEmpty(InsertAfter))
                            {
                                // Insert after specified control
                                var referenceRow = FormXmlHelper.FindControlRowForPositioning(systemForm, TabName, SectionName, InsertAfter);
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
                    else
                    {
                        // No datafieldname in XML, generate new ID
                        controlId = Guid.NewGuid().ToString();
                        control.SetAttributeValue("id", controlId);
                        cell = new XElement("cell", control);
                        XElement row = new XElement("row", cell);
                        
                        // Handle positioning
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
                            // Insert before specified control
                            var referenceRow = FormXmlHelper.FindControlRowForPositioning(systemForm, TabName, SectionName, InsertBefore);
                            if (referenceRow == null)
                            {
                                throw new InvalidOperationException($"Reference control '{InsertBefore}' not found");
                            }
                            referenceRow.AddBeforeSelf(row);
                        }
                        else if (!string.IsNullOrEmpty(InsertAfter))
                        {
                            // Insert after specified control
                            var referenceRow = FormXmlHelper.FindControlRowForPositioning(systemForm, TabName, SectionName, InsertAfter);
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
            }
            else
            {
                // Determine control ID and check if exists
                if (!string.IsNullOrEmpty(ControlId))
                {
                    // Find by ID
                    var (existingControl, existingCell) = FormXmlHelper.FindControlById(systemForm, TabName, SectionName, ControlId);
                    if (existingControl != null)
                    {
                        control = existingControl;
                        cell = existingCell;
                        isUpdate = true;
                        controlId = ControlId;
                    }
                }
                else
                {
                    // Find by DataField
                    var (existingControl, existingCell) = FormXmlHelper.FindControlByDataField(systemForm, TabName, SectionName, DataField);
                    if (existingControl != null)
                    {
                        control = existingControl;
                        cell = existingCell;
                        isUpdate = true;
                        controlId = existingControl.Attribute("id")?.Value;
                    }
                }

                if (!isUpdate)
                {
                    // Create new control
                    controlId = !string.IsNullOrEmpty(ControlId) ? ControlId : Guid.NewGuid().ToString();
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
                        var referenceRow = FormXmlHelper.FindControlRowForPositioning(systemForm, TabName, SectionName, InsertBefore);
                        if (referenceRow == null)
                        {
                            throw new InvalidOperationException($"Reference control '{InsertBefore}' not found");
                        }
                        referenceRow.AddBeforeSelf(row);
                    }
                    else if (!string.IsNullOrEmpty(InsertAfter))
                    {
                        // Find reference control and insert after its row
                        var referenceRow = FormXmlHelper.FindControlRowForPositioning(systemForm, TabName, SectionName, InsertAfter);
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
            if (!isUpdate)
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

                if (MyInvocation.BoundParameters.ContainsKey(nameof(Hidden)))
                {
                    control.SetAttributeValue("visible", Hidden.IsPresent ? "false" : "true");
                }
                else
                {
                    control.SetAttributeValue("visible", "true");
                }

                if (MyInvocation.BoundParameters.ContainsKey(nameof(ShowLabel)))
                {
                    control.SetAttributeValue("showlabel", ShowLabel.IsPresent ? "true" : "false");
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
            string controlDescriptor = isUpdate ? $"control '{ControlId}'" : $"control '{DataField}'";
            if (!ShouldProcess($"form '{FormId}', section '{SectionName}'", $"{action} {controlDescriptor}"))
            {
                return;
            }

            // Update the form
            FormXmlHelper.UpdateFormXml(Connection, FormId, doc);

            // Return the control ID if PassThru is specified
            if (PassThru.IsPresent)
            {
                WriteObject(controlId);
            }

            WriteVerbose($"{action}d {controlDescriptor} (ID: {controlId}) in form '{FormId}'");
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
