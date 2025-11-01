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
    /// Retrieves controls from a Dataverse form.
    /// </summary>
    [Cmdlet(VerbsCommon.Get, "DataverseFormControl")]
    [OutputType(typeof(PSObject))]
    public class GetDataverseFormControlCmdlet : OrganizationServiceCmdlet
    {
        /// <summary>
        /// Gets or sets the form ID.
        /// </summary>
        [Parameter(Mandatory = true, ValueFromPipelineByPropertyName = true, HelpMessage = "ID of the form")]
        [Alias("formid")]
        public Guid FormId { get; set; }

        /// <summary>
        /// Gets or sets the section name to filter controls.
        /// </summary>
        [Parameter(HelpMessage = "Name of the section containing the controls")]
        public string SectionName { get; set; }

        /// <summary>
        /// Gets or sets the control ID to retrieve a specific control.
        /// </summary>
        [Parameter(HelpMessage = "ID of a specific control to retrieve")]
        public string ControlId { get; set; }

        /// <summary>
        /// Gets or sets the data field name to filter controls.
        /// </summary>
        [Parameter(HelpMessage = "Data field name to filter controls")]
        public string DataField { get; set; }

        /// <summary>
        /// Processes the cmdlet request.
        /// </summary>
        protected override void ProcessRecord()
        {
            base.ProcessRecord();

            // Retrieve the form
            Entity form = Connection.Retrieve("systemform", FormId, new ColumnSet("formxml"));
            
            if (!form.Contains("formxml"))
            {
                WriteWarning($"Form '{FormId}' does not contain FormXml");
                return;
            }

            string formXml = form.GetAttributeValue<string>("formxml");
            XDocument doc = XDocument.Parse(formXml);
            XElement systemForm = doc.Root?.Element("SystemForm");
            
            if (systemForm == null)
            {
                WriteWarning("Invalid FormXml structure");
                return;
            }

            var tabsElement = systemForm.Element("tabs");
            if (tabsElement == null)
            {
                return;
            }

            foreach (var tab in tabsElement.Elements("tab"))
            {
                string tabName = tab.Attribute("name")?.Value;
                var columnsElement = tab.Element("columns");
                if (columnsElement == null) continue;

                foreach (var column in columnsElement.Elements("column"))
                {
                    var sectionsElement = column.Element("sections");
                    if (sectionsElement == null) continue;

                    var sections = sectionsElement.Elements("section");
                    
                    if (!string.IsNullOrEmpty(SectionName))
                    {
                        sections = sections.Where(s => s.Attribute("name")?.Value == SectionName);
                    }

                    foreach (var section in sections)
                    {
                        string sectionName = section.Attribute("name")?.Value;
                        var rowsElement = section.Element("rows");
                        if (rowsElement == null) continue;

                        var controls = rowsElement.Elements("row")
                            .SelectMany(row => row.Elements("cell"))
                            .SelectMany(cell => cell.Elements("control"));

                        if (!string.IsNullOrEmpty(ControlId))
                        {
                            controls = controls.Where(c => c.Attribute("id")?.Value == ControlId);
                        }

                        if (!string.IsNullOrEmpty(DataField))
                        {
                            controls = controls.Where(c => c.Attribute("datafieldname")?.Value == DataField);
                        }

                        foreach (var control in controls)
                        {
                            PSObject controlObj = new PSObject();
                            controlObj.Properties.Add(new PSNoteProperty("FormId", FormId));
                            controlObj.Properties.Add(new PSNoteProperty("TabName", tabName));
                            controlObj.Properties.Add(new PSNoteProperty("SectionName", sectionName));
                            controlObj.Properties.Add(new PSNoteProperty("Id", control.Attribute("id")?.Value));
                            controlObj.Properties.Add(new PSNoteProperty("DataField", control.Attribute("datafieldname")?.Value));
                            controlObj.Properties.Add(new PSNoteProperty("ClassId", control.Attribute("classid")?.Value));
                            controlObj.Properties.Add(new PSNoteProperty("Disabled", control.Attribute("disabled")?.Value == "true"));
                            controlObj.Properties.Add(new PSNoteProperty("Visible", control.Attribute("visible")?.Value != "false"));
                            controlObj.Properties.Add(new PSNoteProperty("ShowLabel", control.Attribute("showlabel")?.Value != "false"));
                            controlObj.Properties.Add(new PSNoteProperty("IsRequired", control.Attribute("isrequired")?.Value == "true"));

                            // Parse labels
                            var labelsElement = control.Element("labels");
                            if (labelsElement != null)
                            {
                                var labels = labelsElement.Elements("label").Select(l => new PSObject(new
                                {
                                    Description = l.Attribute("description")?.Value,
                                    LanguageCode = l.Attribute("languagecode")?.Value
                                })).ToArray();
                                controlObj.Properties.Add(new PSNoteProperty("Labels", labels));
                            }

                            WriteObject(controlObj);
                        }
                    }
                }
            }
        }
    }
}
