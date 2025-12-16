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
        [ArgumentCompleter(typeof(FormIdArgumentCompleter))]
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
                PSObject tabObj = FormXmlHelper.ParseTab(tab);
                // Add form context
                tabObj.Properties.Add(new PSNoteProperty("FormId", FormId));
                tabObj.Properties.Add(new PSNoteProperty("VerticalLayout", tab.Attribute("verticallayout")?.Value == "true"));
                tabObj.Properties.Add(new PSNoteProperty("ShowLabel", tab.Attribute("showlabel")?.Value != "false"));

                WriteObject(tabObj);
            }
        }
    }
}
