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
        public Guid FormId { get; set; }

        /// <summary>
        /// Gets or sets the tab ID for updating an existing tab.
        /// </summary>
        [Parameter(Mandatory = false, HelpMessage = "ID of the tab to create or update")]
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
        /// Gets or sets the column layout for the tab.
        /// </summary>
        [Parameter(HelpMessage = "Column layout for the tab (OneColumn, TwoColumns, ThreeColumns)")]
        [ValidateSet("OneColumn", "TwoColumns", "ThreeColumns")]
        public string Layout { get; set; }

        /// <summary>
        /// Gets or sets the width of the first column (when using TwoColumns or ThreeColumns layout).
        /// </summary>
        [Parameter(HelpMessage = "Width of the first column as percentage (0-100)")]
        [ValidateRange(0, 100)]
        public int? Column1Width { get; set; }

        /// <summary>
        /// Gets or sets the width of the second column (when using TwoColumns or ThreeColumns layout).
        /// </summary>
        [Parameter(HelpMessage = "Width of the second column as percentage (0-100)")]
        [ValidateRange(0, 100)]
        public int? Column2Width { get; set; }

        /// <summary>
        /// Gets or sets the width of the third column (when using ThreeColumns layout).
        /// </summary>
        [Parameter(HelpMessage = "Width of the third column as percentage (0-100)")]
        [ValidateRange(0, 100)]
        public int? Column3Width { get; set; }

        /// <summary>
        /// Gets or sets the zero-based index position where the tab should be inserted.
        /// </summary>
        [Parameter(HelpMessage = "Zero-based index position to insert the tab")]
        public int? Index { get; set; }

        /// <summary>
        /// Gets or sets the name or ID of the tab before which to insert this tab.
        /// </summary>
        [Parameter(HelpMessage = "Name or ID of the tab before which to insert")]
        public string InsertBefore { get; set; }

        /// <summary>
        /// Gets or sets the name or ID of the tab after which to insert this tab.
        /// </summary>
        [Parameter(HelpMessage = "Name or ID of the tab after which to insert")]
        public string InsertAfter { get; set; }

        /// <summary>
        /// Gets or sets whether to return the tab ID.
        /// </summary>
        [Parameter(HelpMessage = "Return the tab ID")]
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

            XElement tabsElement = systemForm.Element("tabs");
            if (tabsElement == null)
            {
                tabsElement = new XElement("tabs");
                systemForm.Add(tabsElement);
            }

            XElement tab = null;
            string tabId = null;
            bool isUpdate = false;

            // Check if tab exists by ID or Name
            if (!string.IsNullOrEmpty(TabId))
            {
                // Find by ID
                tab = tabsElement.Elements("tab").FirstOrDefault(t => t.Attribute("id")?.Value == TabId);
                if (tab != null)
                {
                    isUpdate = true;
                    tabId = TabId;
                }
            }
            else
            {
                // Find by Name
                tab = tabsElement.Elements("tab").FirstOrDefault(t => t.Attribute("name")?.Value == Name);
                if (tab != null)
                {
                    isUpdate = true;
                    tabId = tab.Attribute("id")?.Value;
                }
            }

            if (isUpdate)
            {
                // Update existing tab - no positioning needed
            }
            else
            {
                // Tab does not exist - create new
                tabId = !string.IsNullOrEmpty(TabId) ? TabId : Guid.NewGuid().ToString();
                tab = new XElement("tab");
                
                // Handle positioning for new tab
                if (Index.HasValue)
                {
                    // Insert at specific index
                    var tabs = tabsElement.Elements("tab").ToList();
                    if (Index.Value >= 0 && Index.Value <= tabs.Count)
                    {
                        if (Index.Value == tabs.Count)
                        {
                            tabsElement.Add(tab);
                        }
                        else
                        {
                            tabs[Index.Value].AddBeforeSelf(tab);
                        }
                    }
                    else
                    {
                        throw new InvalidOperationException($"Index {Index.Value} is out of range. Valid range is 0-{tabs.Count}");
                    }
                }
                else if (!string.IsNullOrEmpty(InsertBefore))
                {
                    // Insert before specified tab
                    var referenceTab = tabsElement.Elements("tab")
                        .FirstOrDefault(t => t.Attribute("name")?.Value == InsertBefore || 
                                            t.Attribute("id")?.Value == InsertBefore);
                    if (referenceTab == null)
                    {
                        throw new InvalidOperationException($"Reference tab '{InsertBefore}' not found");
                    }
                    referenceTab.AddBeforeSelf(tab);
                }
                else if (!string.IsNullOrEmpty(InsertAfter))
                {
                    // Insert after specified tab
                    var referenceTab = tabsElement.Elements("tab")
                        .FirstOrDefault(t => t.Attribute("name")?.Value == InsertAfter || 
                                            t.Attribute("id")?.Value == InsertAfter);
                    if (referenceTab == null)
                    {
                        throw new InvalidOperationException($"Reference tab '{InsertAfter}' not found");
                    }
                    referenceTab.AddAfterSelf(tab);
                }
                else
                {
                    // Add at the end (default)
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

            // Handle column layout
            UpdateColumnLayout(tab, isUpdate);

            // Confirm action
            string action = isUpdate ? "Update" : "Create";
            if (!ShouldProcess($"form '{FormId}'", $"{action} tab '{Name}'"))
            {
                return;
            }

            // Update the form
            FormXmlHelper.UpdateFormXml(Connection, FormId, doc);

            // Return the tab ID if PassThru is specified
            if (PassThru.IsPresent)
            {
                WriteObject(tabId);
            }

            WriteVerbose($"{action}d tab '{Name}' (ID: {tabId}) in form '{FormId}'");
        }

        private void UpdateColumnLayout(XElement tab, bool isUpdate)
        {
            // Handle layout and column width changes
            if (!string.IsNullOrEmpty(Layout) || 
                !string.IsNullOrEmpty(Column1Width?.ToString()) || 
                !string.IsNullOrEmpty(Column2Width?.ToString()) || 
                !string.IsNullOrEmpty(Column3Width?.ToString()))
            {
                XElement columnsElement = tab.Element("columns");
                var existingColumns = columnsElement?.Elements("column").ToArray() ?? new XElement[0];
                
                // Determine target layout
                string targetLayout = Layout;
                if (string.IsNullOrEmpty(targetLayout))
                {
                    // Infer layout from column width parameters
                    if (!string.IsNullOrEmpty(Column3Width?.ToString()))
                        targetLayout = "ThreeColumns";
                    else if (!string.IsNullOrEmpty(Column2Width?.ToString()))
                        targetLayout = "TwoColumns";
                    else
                        targetLayout = "OneColumn";
                }

                // Create new columns structure
                if (columnsElement == null)
                {
                    columnsElement = new XElement("columns");
                    tab.Add(columnsElement);

                    // No existing columns - create columns for the requested layout
                    CreateColumnsForLayout(columnsElement, targetLayout);
                }
                else
                {
                    // Preserve sections from existing columns
                    var existingSections = existingColumns
                        .SelectMany(col => col.Element("sections")?.Elements("section") ?? Enumerable.Empty<XElement>())
                        .ToArray();
                    
                    columnsElement.RemoveAll();
                    
                    // Create columns based on layout
                    CreateColumnsForLayout(columnsElement, targetLayout);
                    
                    // Redistribute sections across new columns
                    RedistributeSections(columnsElement, existingSections);
                }
            }
            else if (!isUpdate)
            {
                // For new tabs without layout specified, create default single column
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
        }

        private void CreateColumnsForLayout(XElement columnsElement, string layout)
        {
            switch (layout)
            {
                case "OneColumn":
                    columnsElement.Add(new XElement("column",
                        new XAttribute("width", $"{Column1Width ?? 100}%"),
                        new XElement("sections")));
                    break;
                
                case "TwoColumns":
                    columnsElement.Add(new XElement("column",
                        new XAttribute("width", $"{Column1Width ?? 50}%"),
                        new XElement("sections")));
                    columnsElement.Add(new XElement("column",
                        new XAttribute("width", $"{Column2Width ?? 50}%"),
                        new XElement("sections")));
                    break;
                
                case "ThreeColumns":
                    columnsElement.Add(new XElement("column",
                        new XAttribute("width", $"{Column1Width ?? 33}%"),
                        new XElement("sections")));
                    columnsElement.Add(new XElement("column",
                        new XAttribute("width", $"{Column2Width ?? 33}%"),
                        new XElement("sections")));
                    columnsElement.Add(new XElement("column",
                        new XAttribute("width", $"{Column3Width ?? 34}%"),
                        new XElement("sections")));
                    break;
            }
        }

        private void RedistributeSections(XElement columnsElement, XElement[] existingSections)
        {
            if (existingSections.Length == 0)
                return;

            var columns = columnsElement.Elements("column").ToArray();
            if (columns.Length == 0)
                return;

            // Distribute sections evenly across columns
            for (int i = 0; i < existingSections.Length; i++)
            {
                int columnIndex = i % columns.Length;
                var targetColumn = columns[columnIndex];
                var sectionsElement = targetColumn.Element("sections");
                
                // Create a copy of the section to avoid moving it
                var sectionCopy = new XElement(existingSections[i]);
                sectionsElement.Add(sectionCopy);
            }
        }
    }
}
