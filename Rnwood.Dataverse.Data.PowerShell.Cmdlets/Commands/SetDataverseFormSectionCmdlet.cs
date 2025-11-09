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
    /// Creates or updates a section in a Dataverse form tab.
    /// </summary>
    [Cmdlet(VerbsCommon.Set, "DataverseFormSection", SupportsShouldProcess = true, ConfirmImpact = ConfirmImpact.Medium)]
    [OutputType(typeof(string))]
    public class SetDataverseFormSectionCmdlet : OrganizationServiceCmdlet
    {
        /// <summary>
        /// Gets or sets the form ID.
        /// </summary>
        [Parameter(Mandatory = true, ValueFromPipelineByPropertyName = true, HelpMessage = "ID of the form")]
        public Guid FormId { get; set; }

        /// <summary>
        /// Gets or sets the tab name where the section is located.
        /// </summary>
        [Parameter(Mandatory = true, HelpMessage = "Name of the tab containing the section")]
        public string TabName { get; set; }

        /// <summary>
        /// Gets or sets the section ID for updating an existing section.
        /// </summary>
        [Parameter(Mandatory = false, HelpMessage = "ID of the section to create or update")]
        public string SectionId { get; set; }

        /// <summary>
        /// Gets or sets the name of the section.
        /// </summary>
        [Parameter(Mandatory = true, HelpMessage = "Name of the section")]
        public string Name { get; set; }

        /// <summary>
        /// Gets or sets the label for the section.
        /// </summary>
        [Parameter(HelpMessage = "Label text for the section")]
        public string Label { get; set; }

        /// <summary>
        /// Gets or sets the language code for the label (default: 1033 for English).
        /// </summary>
        [Parameter(HelpMessage = "Language code for the label (default: 1033)")]
        public int LanguageCode { get; set; } = 1033;

        /// <summary>
        /// Gets or sets whether to show the section label.
        /// </summary>
        [Parameter(HelpMessage = "Whether to show the section label")]
        public SwitchParameter ShowLabel { get; set; } = true;

        /// <summary>
        /// Gets or sets whether to show the section bar.
        /// </summary>
        [Parameter(HelpMessage = "Whether to show the section bar")]
        public SwitchParameter ShowBar { get; set; } = true;

        /// <summary>
        /// Gets or sets whether the section is visible.
        /// </summary>
        [Parameter(HelpMessage = "Whether the section is visible")]
        public SwitchParameter Visible { get; set; } = true;

        /// <summary>
        /// Gets or sets the number of columns in the section.
        /// </summary>
        [Parameter(HelpMessage = "Number of columns in the section (1-4)")]
        [ValidateRange(1, 4)]
        public int? Columns { get; set; }

        /// <summary>
        /// Gets or sets the label width.
        /// </summary>
        [Parameter(HelpMessage = "Width of labels in pixels")]
        public int? LabelWidth { get; set; }

        /// <summary>
        /// Gets or sets the zero-based index position where the section should be inserted.
        /// </summary>
        [Parameter(HelpMessage = "Zero-based index position to insert the section")]
        public int? Index { get; set; }

        /// <summary>
        /// Gets or sets the name or ID of the section before which to insert this section.
        /// </summary>
        [Parameter(HelpMessage = "Name or ID of the section before which to insert")]
        public string InsertBefore { get; set; }

        /// <summary>
        /// Gets or sets the name or ID of the section after which to insert this section.
        /// </summary>
        [Parameter(HelpMessage = "Name or ID of the section after which to insert")]
        public string InsertAfter { get; set; }

        /// <summary>
        /// Gets or sets whether to return the section ID.
        /// </summary>
        [Parameter(HelpMessage = "Return the section ID")]
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

            // Find the tab
            var tabsElement = systemForm.Element("tabs");
            if (tabsElement == null)
            {
                throw new InvalidOperationException("Form has no tabs");
            }

            XElement targetTab = tabsElement.Elements("tab")
                .FirstOrDefault(t => t.Attribute("name")?.Value == TabName);

            if (targetTab == null)
            {
                throw new InvalidOperationException($"Tab '{TabName}' not found in form");
            }

            // Get or create columns element
            XElement columnsElement = targetTab.Element("columns");
            if (columnsElement == null)
            {
                columnsElement = new XElement("columns");
                targetTab.Add(columnsElement);
            }

            // Get or create first column
            XElement column = columnsElement.Elements("column").FirstOrDefault();
            if (column == null)
            {
                column = new XElement("column", new XAttribute("width", "100%"));
                columnsElement.Add(column);
            }

            // Get or create sections element
            XElement sectionsElement = column.Element("sections");
            if (sectionsElement == null)
            {
                sectionsElement = new XElement("sections");
                column.Add(sectionsElement);
            }

            XElement section = null;
            string sectionId = null;
            bool isUpdate = false;

            // Check if section exists by ID or Name
            if (!string.IsNullOrEmpty(SectionId))
            {
                // Find by ID
                section = sectionsElement.Elements("section").FirstOrDefault(s => s.Attribute("id")?.Value == SectionId);
                if (section != null)
                {
                    isUpdate = true;
                    sectionId = SectionId;
                }
            }
            else
            {
                // Find by Name
                section = sectionsElement.Elements("section").FirstOrDefault(s => s.Attribute("name")?.Value == Name);
                if (section != null)
                {
                    isUpdate = true;
                    sectionId = section.Attribute("id")?.Value;
                }
            }

            if (isUpdate)
            {
                // Update existing section - no positioning needed
            }
            else
            {
                // Section does not exist - create new
                sectionId = !string.IsNullOrEmpty(SectionId) ? SectionId : Guid.NewGuid().ToString();
                section = new XElement("section");
                
                // Handle positioning for new section
                if (Index.HasValue)
                {
                    // Insert at specific index
                    var sections = sectionsElement.Elements("section").ToList();
                    if (Index.Value >= 0 && Index.Value <= sections.Count)
                    {
                        if (Index.Value == sections.Count)
                        {
                            sectionsElement.Add(section);
                        }
                        else
                        {
                            sections[Index.Value].AddBeforeSelf(section);
                        }
                    }
                    else
                    {
                        throw new InvalidOperationException($"Index {Index.Value} is out of range. Valid range is 0-{sections.Count}");
                    }
                }
                else if (!string.IsNullOrEmpty(InsertBefore))
                {
                    // Insert before specified section
                    var referenceSection = sectionsElement.Elements("section")
                        .FirstOrDefault(s => s.Attribute("name")?.Value == InsertBefore || 
                                            s.Attribute("id")?.Value == InsertBefore);
                    if (referenceSection == null)
                    {
                        throw new InvalidOperationException($"Reference section '{InsertBefore}' not found");
                    }
                    referenceSection.AddBeforeSelf(section);
                }
                else if (!string.IsNullOrEmpty(InsertAfter))
                {
                    // Insert after specified section
                    var referenceSection = sectionsElement.Elements("section")
                        .FirstOrDefault(s => s.Attribute("name")?.Value == InsertAfter || 
                                            s.Attribute("id")?.Value == InsertAfter);
                    if (referenceSection == null)
                    {
                        throw new InvalidOperationException($"Reference section '{InsertAfter}' not found");
                    }
                    referenceSection.AddAfterSelf(section);
                }
                else
                {
                    // Add at the end (default)
                    sectionsElement.Add(section);
                }
            }

            // Update section attributes
            section.SetAttributeValue("name", Name);
            section.SetAttributeValue("id", sectionId);
            
            if (MyInvocation.BoundParameters.ContainsKey(nameof(ShowLabel)))
            {
                section.SetAttributeValue("showlabel", ShowLabel.IsPresent ? "true" : "false");
            }
            
            if (MyInvocation.BoundParameters.ContainsKey(nameof(ShowBar)))
            {
                section.SetAttributeValue("showbar", ShowBar.IsPresent ? "true" : "false");
            }
            
            if (MyInvocation.BoundParameters.ContainsKey(nameof(Visible)))
            {
                section.SetAttributeValue("visible", Visible.IsPresent ? "true" : "false");
            }

            if (Columns.HasValue)
            {
                section.SetAttributeValue("columns", Columns.Value);
            }

            if (LabelWidth.HasValue)
            {
                section.SetAttributeValue("labelwidth", LabelWidth.Value);
            }

            // Update or add labels
            if (!string.IsNullOrEmpty(Label))
            {
                XElement labelsElement = section.Element("labels");
                if (labelsElement == null)
                {
                    labelsElement = new XElement("labels");
                    section.Add(labelsElement);
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

            // Ensure rows element exists
            if (section.Element("rows") == null)
            {
                section.Add(new XElement("rows"));
            }

            // Confirm action
            string action = isUpdate ? "Update" : "Create";
            if (!ShouldProcess($"form '{FormId}', tab '{TabName}'", $"{action} section '{Name}'"))
            {
                return;
            }

            // Update the form
            FormXmlHelper.UpdateFormXml(Connection, FormId, doc);

            // Return the section ID if PassThru is specified
            if (PassThru.IsPresent)
            {
                WriteObject(sectionId);
            }

            WriteVerbose($"{action}d section '{Name}' (ID: {sectionId}) in form '{FormId}'");
        }
    }
}
