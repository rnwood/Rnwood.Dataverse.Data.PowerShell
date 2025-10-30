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
    /// Adds a control to a Dataverse form section.
    /// </summary>
    [Cmdlet(VerbsCommon.Add, "DataverseFormControl", SupportsShouldProcess = true, ConfirmImpact = ConfirmImpact.Medium)]
    [OutputType(typeof(string))]
    public class AddDataverseFormControlCmdlet : OrganizationServiceCmdlet
    {
        /// <summary>
        /// Gets or sets the form ID.
        /// </summary>
        [Parameter(Mandatory = true, ValueFromPipelineByPropertyName = true, HelpMessage = "ID of the form")]
        [Alias("formid")]
        public Guid FormId { get; set; }

        /// <summary>
        /// Gets or sets the section name where the control will be added.
        /// </summary>
        [Parameter(Mandatory = true, HelpMessage = "Name of the section where the control will be added")]
        public string SectionName { get; set; }

        /// <summary>
        /// Gets or sets the data field name (attribute) for the control.
        /// </summary>
        [Parameter(Mandatory = true, HelpMessage = "Data field name (attribute) for the control")]
        public string DataField { get; set; }

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
        /// Gets or sets whether to return the control ID.
        /// </summary>
        [Parameter(HelpMessage = "Return the control ID")]
        public SwitchParameter PassThru { get; set; }

        /// <summary>
        /// Gets or sets whether to publish the form after adding the control.
        /// </summary>
        [Parameter(HelpMessage = "Publish the form after adding the control")]
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

            // Create control
            string controlId = DataField;
            XElement control = new XElement("control",
                new XAttribute("id", controlId),
                new XAttribute("datafieldname", DataField),
                new XAttribute("classid", GetClassId(ControlType))
            );

            if (Disabled.IsPresent)
            {
                control.Add(new XAttribute("disabled", "true"));
            }

            if (!Visible.IsPresent)
            {
                control.Add(new XAttribute("visible", "false"));
            }

            if (!ShowLabel.IsPresent)
            {
                control.Add(new XAttribute("showlabel", "false"));
            }

            if (IsRequired.IsPresent)
            {
                control.Add(new XAttribute("isrequired", "true"));
            }

            // Add label if provided
            if (!string.IsNullOrEmpty(Label))
            {
                XElement labels = new XElement("labels",
                    new XElement("label",
                        new XAttribute("description", Label),
                        new XAttribute("languagecode", LanguageCode)
                    )
                );
                control.Add(labels);
            }

            // Add parameters for special controls
            if (Parameters != null && Parameters.Count > 0)
            {
                XElement paramsElement = new XElement("parameters");
                foreach (System.Collections.DictionaryEntry param in Parameters)
                {
                    paramsElement.Add(new XElement(param.Key.ToString(), param.Value?.ToString()));
                }
                control.Add(paramsElement);
            }

            // Create cell and row
            XElement cell = new XElement("cell");
            
            if (ColSpan.HasValue)
            {
                cell.Add(new XAttribute("colspan", ColSpan.Value));
            }

            if (RowSpan.HasValue)
            {
                cell.Add(new XAttribute("rowspan", RowSpan.Value));
            }

            cell.Add(control);

            XElement row = new XElement("row", cell);

            // Add row to section
            XElement rowsElement = targetSection.Element("rows");
            if (rowsElement == null)
            {
                rowsElement = new XElement("rows");
                targetSection.Add(rowsElement);
            }

            // Confirm action
            if (!ShouldProcess($"form '{FormId}', section '{SectionName}'", $"Add control '{DataField}'"))
            {
                return;
            }

            rowsElement.Add(row);

            // Update form
            Entity updateForm = new Entity("systemform", FormId);
            updateForm["formxml"] = doc.ToString();
            Connection.Update(updateForm);

            WriteVerbose($"Added control '{DataField}' to section '{SectionName}' in form '{FormId}'");

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
