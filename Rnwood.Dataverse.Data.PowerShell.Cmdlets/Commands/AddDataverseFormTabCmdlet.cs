using Microsoft.Crm.Sdk.Messages;
using Microsoft.PowerPlatform.Dataverse.Client;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Query;
using System;
using System.Management.Automation;
using System.Xml.Linq;

namespace Rnwood.Dataverse.Data.PowerShell.Commands
{
    /// <summary>
    /// Adds a new tab to a Dataverse form.
    /// </summary>
    [Cmdlet(VerbsCommon.Add, "DataverseFormTab", SupportsShouldProcess = true, ConfirmImpact = ConfirmImpact.Medium)]
    [OutputType(typeof(string))]
    public class AddDataverseFormTabCmdlet : OrganizationServiceCmdlet
    {
        /// <summary>
        /// Gets or sets the form ID to which to add the tab.
        /// </summary>
        [Parameter(Mandatory = true, ValueFromPipelineByPropertyName = true, HelpMessage = "ID of the form")]
        [Alias("formid")]
        public Guid FormId { get; set; }

        /// <summary>
        /// Gets or sets the name of the tab.
        /// </summary>
        [Parameter(Mandatory = true, HelpMessage = "Name of the tab")]
        public string Name { get; set; }

        /// <summary>
        /// Gets or sets the label for the tab.
        /// </summary>
        [Parameter(Mandatory = true, HelpMessage = "Label text for the tab")]
        public string Label { get; set; }

        /// <summary>
        /// Gets or sets the language code for the label (default: 1033 for English).
        /// </summary>
        [Parameter(HelpMessage = "Language code for the label (default: 1033)")]
        public int LanguageCode { get; set; } = 1033;

        /// <summary>
        /// Gets or sets whether the tab is expanded by default.
        /// </summary>
        [Parameter(HelpMessage = "Whether the tab is expanded by default")]
        public SwitchParameter Expanded { get; set; }

        /// <summary>
        /// Gets or sets whether the tab is visible.
        /// </summary>
        [Parameter(HelpMessage = "Whether the tab is visible")]
        public SwitchParameter Visible { get; set; } = true;

        /// <summary>
        /// Gets or sets whether to show the tab label.
        /// </summary>
        [Parameter(HelpMessage = "Whether to show the tab label")]
        public SwitchParameter ShowLabel { get; set; } = true;

        /// <summary>
        /// Gets or sets whether to use vertical layout.
        /// </summary>
        [Parameter(HelpMessage = "Whether to use vertical layout")]
        public SwitchParameter VerticalLayout { get; set; }

        /// <summary>
        /// Gets or sets whether to return the tab ID.
        /// </summary>
        [Parameter(HelpMessage = "Return the tab ID")]
        public SwitchParameter PassThru { get; set; }

        /// <summary>
        /// Gets or sets whether to publish the form after adding the tab.
        /// </summary>
        [Parameter(HelpMessage = "Publish the form after adding the tab")]
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

            XElement tabsElement = systemForm.Element("tabs");
            if (tabsElement == null)
            {
                tabsElement = new XElement("tabs");
                systemForm.Add(tabsElement);
            }

            // Create new tab
            string tabId = Guid.NewGuid().ToString("B");
            XElement newTab = new XElement("tab",
                new XAttribute("name", Name),
                new XAttribute("id", tabId),
                new XAttribute("expanded", Expanded.IsPresent ? "true" : "false"),
                new XAttribute("visible", Visible.IsPresent ? "true" : "false"),
                new XAttribute("showlabel", ShowLabel.IsPresent ? "true" : "false")
            );

            if (VerticalLayout.IsPresent)
            {
                newTab.Add(new XAttribute("verticallayout", "true"));
            }

            // Add labels
            XElement labelsElement = new XElement("labels",
                new XElement("label",
                    new XAttribute("description", Label),
                    new XAttribute("languagecode", LanguageCode)
                )
            );
            newTab.Add(labelsElement);

            // Add columns element with one column
            XElement columnsElement = new XElement("columns",
                new XElement("column",
                    new XAttribute("width", "100%"),
                    new XElement("sections")
                )
            );
            newTab.Add(columnsElement);

            // Confirm action
            if (!ShouldProcess($"form '{FormId}'", $"Add tab '{Name}'"))
            {
                return;
            }

            // Add tab to form
            tabsElement.Add(newTab);

            // Update form
            Entity updateForm = new Entity("systemform", FormId);
            updateForm["formxml"] = doc.ToString();
            Connection.Update(updateForm);

            WriteVerbose($"Added tab '{Name}' to form '{FormId}'");

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
                WriteObject(tabId);
            }
        }
    }
}
