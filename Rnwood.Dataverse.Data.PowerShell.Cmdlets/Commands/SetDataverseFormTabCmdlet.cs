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
    /// Creates or updates a tab on a Dataverse form.
    /// </summary>
    [Cmdlet(VerbsCommon.Set, "DataverseFormTab", SupportsShouldProcess = true, ConfirmImpact = ConfirmImpact.Medium)]
    [OutputType(typeof(string))]
    public class SetDataverseFormTabCmdlet : OrganizationServiceCmdlet
    {
        /// <summary>
        /// Gets or sets the form ID.
        /// </summary>
        [Parameter(Mandatory = true, ValueFromPipelineByPropertyName = true, HelpMessage = "ID of the form")]
        [Alias("formid")]
        public Guid FormId { get; set; }

        /// <summary>
        /// Gets or sets the tab ID for updating an existing tab.
        /// </summary>
        [Parameter(ParameterSetName = "Update", Mandatory = true, HelpMessage = "ID of the tab to update")]
        public string TabId { get; set; }

        /// <summary>
        /// Gets or sets the name of the tab.
        /// </summary>
        [Parameter(Mandatory = true, HelpMessage = "Name of the tab")]
        public string Name { get; set; }

        /// <summary>
        /// Gets or sets the label for the tab.
        /// </summary>
        [Parameter(HelpMessage = "Label text for the tab")]
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
        /// Gets or sets whether to publish the form after modifying the tab.
        /// </summary>
        [Parameter(HelpMessage = "Publish the form after modifying the tab")]
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

            XElement tab = null;
            string tabId = null;
            bool isUpdate = ParameterSetName == "Update";

            if (isUpdate)
            {
                // Find existing tab
                tab = tabsElement.Elements("tab").FirstOrDefault(t => t.Attribute("id")?.Value == TabId);
                if (tab == null)
                {
                    throw new InvalidOperationException($"Tab with ID '{TabId}' not found in form");
                }
                tabId = TabId;
            }
            else
            {
                // Check if tab with same name exists
                tab = tabsElement.Elements("tab").FirstOrDefault(t => t.Attribute("name")?.Value == Name);
                if (tab != null)
                {
                    // Update existing tab
                    isUpdate = true;
                    tabId = tab.Attribute("id")?.Value;
                }
                else
                {
                    // Create new tab
                    tabId = Guid.NewGuid().ToString("B");
                    tab = new XElement("tab");
                    tabsElement.Add(tab);
                }
            }

            // Update tab attributes
            tab.SetAttributeValue("name", Name);
            tab.SetAttributeValue("id", tabId);
            
            if (MyInvocation.BoundParameters.ContainsKey(nameof(Expanded)))
            {
                tab.SetAttributeValue("expanded", Expanded.IsPresent ? "true" : "false");
            }
            
            if (MyInvocation.BoundParameters.ContainsKey(nameof(Visible)))
            {
                tab.SetAttributeValue("visible", Visible.IsPresent ? "true" : "false");
            }
            
            if (MyInvocation.BoundParameters.ContainsKey(nameof(ShowLabel)))
            {
                tab.SetAttributeValue("showlabel", ShowLabel.IsPresent ? "true" : "false");
            }

            if (VerticalLayout.IsPresent)
            {
                tab.SetAttributeValue("verticallayout", "true");
            }

            // Update or add labels
            if (!string.IsNullOrEmpty(Label))
            {
                XElement labelsElement = tab.Element("labels");
                if (labelsElement == null)
                {
                    labelsElement = new XElement("labels");
                    tab.Add(labelsElement);
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

            // Ensure columns element exists for new tabs
            if (!isUpdate || tab.Element("columns") == null)
            {
                if (tab.Element("columns") == null)
                {
                    XElement columnsElement = new XElement("columns",
                        new XElement("column",
                            new XAttribute("width", "100%"),
                            new XElement("sections")
                        )
                    );
                    tab.Add(columnsElement);
                }
            }

            // Confirm action
            string action = isUpdate ? "Update" : "Create";
            if (!ShouldProcess($"form '{FormId}'", $"{action} tab '{Name}'"))
            {
                return;
            }

            // Update form
            Entity updateForm = new Entity("systemform", FormId);
            updateForm["formxml"] = doc.ToString();
            Connection.Update(updateForm);

            WriteVerbose($"{action}d tab '{Name}' on form '{FormId}'");

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
