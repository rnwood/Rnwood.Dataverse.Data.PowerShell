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
        [ArgumentCompleter(typeof(FormIdArgumentCompleter))]
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
        /// Gets or sets whether the section is hidden.
        /// </summary>
        [Parameter(HelpMessage = "Whether the section is hidden")]
        public SwitchParameter Hidden { get; set; } = true;

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
        /// Zero-based index of the column within the tab where the section should be placed.
        /// </summary>
        [Parameter(HelpMessage = "Zero-based index of the column within the tab where the section should be placed")]
        public int? ColumnIndex { get; set; }

        /// <summary>
        /// Gets or sets the cell label alignment.
        /// </summary>
        [Parameter(HelpMessage = "Cell label alignment (Center, Left, Right)")]
        public CellLabelAlignment? CellLabelAlignment { get; set; }

        /// <summary>
        /// Gets or sets the cell label position.
        /// </summary>
        [Parameter(HelpMessage = "Cell label position (Top, Left)")]
        public CellLabelPosition? CellLabelPosition { get; set; }

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

            // Determine tab columns count from tab attribute if present
            int? tabColumnsAttr = null;
            var tabColumnsAttrVal = targetTab.Attribute("columns")?.Value;
            if (!string.IsNullOrEmpty(tabColumnsAttrVal) && int.TryParse(tabColumnsAttrVal, out int parsedTabCols))
            {
                tabColumnsAttr = parsedTabCols;
            }

            // Ensure there are column elements; if none exist, create defaults based on tabColumnsAttr or single
            var columnsList = columnsElement.Elements("column").ToList();
            if (columnsList.Count == 0)
            {
                int createCount = tabColumnsAttr ?? 1;
                for (int i = 0; i < createCount; i++)
                {
                    columnsElement.Add(new XElement("column",
                        new XAttribute("width", $"{(100 / createCount)}%"),
                        new XElement("sections")
                    ));
                }
                columnsList = columnsElement.Elements("column").ToList();
            }

            // Validate ColumnIndex against tab column count (preference: tabColumnsAttr when present)
            if (ColumnIndex.HasValue)
            {
                if (tabColumnsAttr.HasValue)
                {
                    if (ColumnIndex.Value < 0 || ColumnIndex.Value >= tabColumnsAttr.Value)
                        throw new InvalidOperationException($"ColumnIndex {ColumnIndex.Value} is out of range. Valid range is 0-{tabColumnsAttr.Value - 1}");
                }
                else
                {
                    if (ColumnIndex.Value < 0 || ColumnIndex.Value >= columnsList.Count)
                        throw new InvalidOperationException($"ColumnIndex {ColumnIndex.Value} is out of range. Valid range is 0-{columnsList.Count - 1}");
                }
            }

            // Find existing section across all columns
            XElement section = null;
            string sectionId = null;
            bool isUpdate = false;
            XElement currentColumn = null;

            // Search by ID or name across columns
            foreach (var col in columnsList)
            {
                var secs = col.Element("sections");
                if (secs == null) continue;

                if (!string.IsNullOrEmpty(SectionId))
                {
                    var s = secs.Elements("section").FirstOrDefault(sx => sx.Attribute("id")?.Value == SectionId);
                    if (s != null)
                    {
                        section = s;
                        sectionId = SectionId;
                        isUpdate = true;
                        currentColumn = col;
                        break;
                    }
                }

                var byName = secs.Elements("section").FirstOrDefault(sx => sx.Attribute("name")?.Value == Name);
                if (byName != null)
                {
                    section = byName;
                    sectionId = section.Attribute("id")?.Value;
                    isUpdate = true;
                    currentColumn = col;
                    break;
                }
            }

            // Determine target column element based on ColumnIndex or existing/current column or default first
            XElement targetColumn = null;
            if (ColumnIndex.HasValue)
            {
                targetColumn = columnsList[ColumnIndex.Value];
            }
            else if (isUpdate && currentColumn != null)
            {
                targetColumn = currentColumn;
            }
            else
            {
                targetColumn = columnsList.First();
            }

            // Ensure target column has sections element
            var targetSectionsElement = targetColumn.Element("sections");
            if (targetSectionsElement == null)
            {
                targetSectionsElement = new XElement("sections");
                targetColumn.Add(targetSectionsElement);
            }

            if (isUpdate)
            {
                // If ColumnIndex specified and different to current, move the section
                if (ColumnIndex.HasValue && currentColumn != null && !object.ReferenceEquals(currentColumn, targetColumn))
                {
                    // Remove from current
                    var currentSections = currentColumn.Element("sections");
                    section.Remove();

                    // Insert into target - respect Index/InsertBefore/InsertAfter if provided, else append
                    if (Index.HasValue)
                    {
                        var targetSecs = targetSectionsElement.Elements("section").ToList();
                        if (Index.Value >= 0 && Index.Value <= targetSecs.Count)
                        {
                            if (Index.Value == targetSecs.Count)
                                targetSectionsElement.Add(section);
                            else
                                targetSecs[Index.Value].AddBeforeSelf(section);
                        }
                        else
                        {
                            throw new InvalidOperationException($"Index {Index.Value} is out of range. Valid range is 0-{targetSecs.Count}");
                        }
                    }
                    else if (!string.IsNullOrEmpty(InsertBefore))
                    {
                        var refSec = targetSectionsElement.Elements("section")
                            .FirstOrDefault(s => s.Attribute("name")?.Value == InsertBefore || s.Attribute("id")?.Value == InsertBefore);
                        if (refSec == null)
                            throw new InvalidOperationException($"Reference section '{InsertBefore}' not found in target column");
                        refSec.AddBeforeSelf(section);
                    }
                    else if (!string.IsNullOrEmpty(InsertAfter))
                    {
                        var refSec = targetSectionsElement.Elements("section")
                            .FirstOrDefault(s => s.Attribute("name")?.Value == InsertAfter || s.Attribute("id")?.Value == InsertAfter);
                        if (refSec == null)
                            throw new InvalidOperationException($"Reference section '{InsertAfter}' not found in target column");
                        refSec.AddAfterSelf(section);
                    }
                    else
                    {
                        targetSectionsElement.Add(section);
                    }

                    // update currentColumn reference
                    currentColumn = targetColumn;
                }
                // else update attributes only
            }
            else
            {
                // Section does not exist - create new
                sectionId = !string.IsNullOrEmpty(SectionId) ? SectionId : Guid.NewGuid().ToString();
                section = new XElement("section");

                // Handle positioning for new section within targetSectionsElement
                if (Index.HasValue)
                {
                    var targetSecs = targetSectionsElement.Elements("section").ToList();
                    if (Index.Value >= 0 && Index.Value <= targetSecs.Count)
                    {
                        if (Index.Value == targetSecs.Count)
                        {
                            targetSectionsElement.Add(section);
                        }
                        else
                        {
                            targetSecs[Index.Value].AddBeforeSelf(section);
                        }
                    }
                    else
                    {
                        throw new InvalidOperationException($"Index {Index.Value} is out of range. Valid range is 0-{targetSecs.Count}");
                    }
                }
                else if (!string.IsNullOrEmpty(InsertBefore))
                {
                    var referenceSection = targetSectionsElement.Elements("section")
                        .FirstOrDefault(s => s.Attribute("name")?.Value == InsertBefore || 
                                            s.Attribute("id")?.Value == InsertBefore);
                    if (referenceSection == null)
                    {
                        throw new InvalidOperationException($"Reference section '{InsertBefore}' not found in target column");
                    }
                    referenceSection.AddBeforeSelf(section);
                }
                else if (!string.IsNullOrEmpty(InsertAfter))
                {
                    var referenceSection = targetSectionsElement.Elements("section")
                        .FirstOrDefault(s => s.Attribute("name")?.Value == InsertAfter || 
                                            s.Attribute("id")?.Value == InsertAfter);
                    if (referenceSection == null)
                    {
                        throw new InvalidOperationException($"Reference section '{InsertAfter}' not found in target column");
                    }
                    referenceSection.AddAfterSelf(section);
                }
                else
                {
                    // Add at the end (default)
                    targetSectionsElement.Add(section);
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
            
            if (MyInvocation.BoundParameters.ContainsKey(nameof(Hidden)))
            {
                section.SetAttributeValue("visible", Hidden.IsPresent ? "false" : "true");
            }
            else if (!isUpdate)
            {
                section.SetAttributeValue("visible", "true");
            }

            if (Columns.HasValue)
            {
                section.SetAttributeValue("columns", Columns.Value);
            }

            if (LabelWidth.HasValue)
            {
                section.SetAttributeValue("labelwidth", LabelWidth.Value);
            }

            if (CellLabelAlignment.HasValue)
            {
                section.SetAttributeValue("celllabelalignment", CellLabelAlignment.Value.ToString());
            }

            if (CellLabelPosition.HasValue)
            {
                section.SetAttributeValue("celllabelposition", CellLabelPosition.Value.ToString());
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
