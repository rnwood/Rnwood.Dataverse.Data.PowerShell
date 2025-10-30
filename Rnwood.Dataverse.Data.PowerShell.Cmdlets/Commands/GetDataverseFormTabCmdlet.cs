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
    /// Retrieves tabs from a Dataverse form.
    /// </summary>
    [Cmdlet(VerbsCommon.Get, "DataverseFormTab")]
    [OutputType(typeof(PSObject))]
    public class GetDataverseFormTabCmdlet : OrganizationServiceCmdlet
    {
        /// <summary>
        /// Gets or sets the form ID from which to retrieve tabs.
        /// </summary>
        [Parameter(Mandatory = true, ValueFromPipelineByPropertyName = true, HelpMessage = "ID of the form")]
        [Alias("formid")]
        public Guid FormId { get; set; }

        /// <summary>
        /// Gets or sets the tab name to retrieve a specific tab.
        /// </summary>
        [Parameter(HelpMessage = "Name of a specific tab to retrieve")]
        public string TabName { get; set; }

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
                PSObject tabObj = new PSObject();
                tabObj.Properties.Add(new PSNoteProperty("FormId", FormId));
                tabObj.Properties.Add(new PSNoteProperty("Id", tab.Attribute("id")?.Value));
                tabObj.Properties.Add(new PSNoteProperty("Name", tab.Attribute("name")?.Value));
                tabObj.Properties.Add(new PSNoteProperty("Expanded", tab.Attribute("expanded")?.Value == "true"));
                tabObj.Properties.Add(new PSNoteProperty("Visible", tab.Attribute("visible")?.Value != "false"));
                tabObj.Properties.Add(new PSNoteProperty("ShowLabel", tab.Attribute("showlabel")?.Value != "false"));
                tabObj.Properties.Add(new PSNoteProperty("VerticalLayout", tab.Attribute("verticallayout")?.Value == "true"));

                // Parse labels
                var labelsElement = tab.Element("labels");
                if (labelsElement != null)
                {
                    var labels = labelsElement.Elements("label").Select(l => new PSObject(new
                    {
                        Description = l.Attribute("description")?.Value,
                        LanguageCode = l.Attribute("languagecode")?.Value
                    })).ToArray();
                    tabObj.Properties.Add(new PSNoteProperty("Labels", labels));
                }

                WriteObject(tabObj);
            }
        }
    }
}
