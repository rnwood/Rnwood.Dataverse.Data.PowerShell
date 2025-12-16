using Microsoft.PowerPlatform.Dataverse.Client;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Query;
using Rnwood.Dataverse.Data.PowerShell.Model;
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
        [ArgumentCompleter(typeof(FormIdArgumentCompleter))]
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

            Entity form = FormXmlHelper.RetrieveForm(Connection, FormId, new ColumnSet("formxml"));
            var (doc, systemForm) = FormXmlHelper.ParseFormXml(form);

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

                // Iterate columns with index so we can report which column each section belongs to
                var columnsWithIndex = columnsElement.Elements("column").Select((col, idx) => new { Column = col, Index = idx }).ToList();

                foreach (var colInfo in columnsWithIndex)
                {
                    var sections = colInfo.Column.Element("sections")?.Elements("section") ?? Enumerable.Empty<XElement>();

                    if (!string.IsNullOrEmpty(SectionName))
                    {
                        sections = sections.Where(s => s.Attribute("name")?.Value == SectionName);
                    }

                    foreach (var section in sections)
                    {
                        PSObject sectionObj = FormXmlHelper.ParseSection(section);
                        // Add form and tab context
                        sectionObj.Properties.Add(new PSNoteProperty("FormId", FormId));
                        sectionObj.Properties.Add(new PSNoteProperty("TabName", currentTabName));
                        sectionObj.Properties.Add(new PSNoteProperty("ShowBar", section.Attribute("showbar")?.Value != "false"));
                        sectionObj.Properties.Add(new PSNoteProperty("LabelWidth", section.Attribute("labelwidth")?.Value));

                        // Add Columns attribute if present (parse to int when possible)
                        var columnsAttr = section.Attribute("columns")?.Value;
                        if (!string.IsNullOrEmpty(columnsAttr) && int.TryParse(columnsAttr, out int cols))
                        {
                            sectionObj.Properties.Add(new PSNoteProperty("Columns", cols));
                        }
                        else
                        {
                            sectionObj.Properties.Add(new PSNoteProperty("Columns", columnsAttr));
                        }


                        // Add the zero-based index of the parent column within the tab
                        sectionObj.Properties.Add(new PSNoteProperty("ColumnIndex", colInfo.Index));

                        // Add CellLabelAlignment and CellLabelPosition attributes if present
                        var cellLabelAlignment = section.Attribute("celllabelalignment")?.Value;
                        sectionObj.Properties.Add(new PSNoteProperty("CellLabelAlignment", cellLabelAlignment));
                        
                        var cellLabelPosition = section.Attribute("celllabelposition")?.Value;
                        sectionObj.Properties.Add(new PSNoteProperty("CellLabelPosition", cellLabelPosition));

                        WriteObject(sectionObj);
                    }
                }
            }
        }
    }
}
