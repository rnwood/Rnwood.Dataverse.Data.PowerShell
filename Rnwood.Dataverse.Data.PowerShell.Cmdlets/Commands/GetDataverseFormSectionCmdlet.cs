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
    /// Retrieves sections from a Dataverse form tab.
    /// </summary>
    [Cmdlet(VerbsCommon.Get, "DataverseFormSection")]
    [OutputType(typeof(PSObject))]
    public class GetDataverseFormSectionCmdlet : OrganizationServiceCmdlet
    {
        /// <summary>
        /// Gets or sets the form ID.
        /// </summary>
        [Parameter(Mandatory = true, ValueFromPipelineByPropertyName = true, HelpMessage = "ID of the form")]
        [Alias("formid")]
        public Guid FormId { get; set; }

        /// <summary>
        /// Gets or sets the tab name to filter sections.
        /// </summary>
        [Parameter(HelpMessage = "Name of the tab containing the sections")]
        public string TabName { get; set; }

        /// <summary>
        /// Gets or sets the section name to retrieve a specific section.
        /// </summary>
        [Parameter(HelpMessage = "Name of a specific section to retrieve")]
        public string SectionName { get; set; }

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

            var tabs = tabsElement.Elements("tab");
            
            if (!string.IsNullOrEmpty(TabName))
            {
                tabs = tabs.Where(t => t.Attribute("name")?.Value == TabName);
            }

            foreach (var tab in tabs)
            {
                string currentTabName = tab.Attribute("name")?.Value;
                var columnsElement = tab.Element("columns");
                if (columnsElement == null) continue;

                var sections = columnsElement.Elements("column")
                    .SelectMany(col => col.Element("sections")?.Elements("section") ?? Enumerable.Empty<XElement>());

                if (!string.IsNullOrEmpty(SectionName))
                {
                    sections = sections.Where(s => s.Attribute("name")?.Value == SectionName);
                }

                foreach (var section in sections)
                {
                    PSObject sectionObj = new PSObject();
                    sectionObj.Properties.Add(new PSNoteProperty("FormId", FormId));
                    sectionObj.Properties.Add(new PSNoteProperty("TabName", currentTabName));
                    sectionObj.Properties.Add(new PSNoteProperty("Id", section.Attribute("id")?.Value));
                    sectionObj.Properties.Add(new PSNoteProperty("Name", section.Attribute("name")?.Value));
                    sectionObj.Properties.Add(new PSNoteProperty("ShowLabel", section.Attribute("showlabel")?.Value != "false"));
                    sectionObj.Properties.Add(new PSNoteProperty("ShowBar", section.Attribute("showbar")?.Value != "false"));
                    sectionObj.Properties.Add(new PSNoteProperty("Visible", section.Attribute("visible")?.Value != "false"));
                    sectionObj.Properties.Add(new PSNoteProperty("Columns", section.Attribute("columns")?.Value));
                    sectionObj.Properties.Add(new PSNoteProperty("LabelWidth", section.Attribute("labelwidth")?.Value));

                    // Parse labels
                    var labelsElement = section.Element("labels");
                    if (labelsElement != null)
                    {
                        var labels = labelsElement.Elements("label").Select(l => new PSObject(new
                        {
                            Description = l.Attribute("description")?.Value,
                            LanguageCode = l.Attribute("languagecode")?.Value
                        })).ToArray();
                        sectionObj.Properties.Add(new PSNoteProperty("Labels", labels));
                    }

                    WriteObject(sectionObj);
                }
            }
        }
    }
}
