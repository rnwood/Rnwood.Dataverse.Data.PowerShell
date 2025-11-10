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
        public Guid FormId { get; set; }

        /// <summary>
        /// Gets or sets the tab name to filter controls. Use '[Header]' to get controls from the form header.
        /// </summary>
        [Parameter(HelpMessage = "Name of the tab containing the section. Use '[Header]' to get header controls.")]
        public string TabName { get; set; }

        /// <summary>
        /// Gets or sets the section name to filter controls. TabName must be provided when using SectionName (except for header).
        /// </summary>
        [Parameter(HelpMessage = "Name of the section containing the controls. TabName is required when using SectionName (not applicable for header).")]
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

            // Special handling for header controls
            bool isHeaderRequest = !string.IsNullOrEmpty(TabName) && TabName.Equals("[Header]", StringComparison.OrdinalIgnoreCase);
            
            // Validate that TabName is provided when SectionName is specified (unless it's header)
            if (!string.IsNullOrEmpty(SectionName) && string.IsNullOrEmpty(TabName))
            {
                throw new ArgumentException("TabName is required when SectionName is specified. Section names are only unique within a tab.");
            }

            Entity form = FormXmlHelper.RetrieveForm(Connection, FormId, new ColumnSet("formxml"));
            var (doc, systemForm) = FormXmlHelper.ParseFormXml(form);

            foreach (var (tabName, sectionName, control) in FormXmlHelper.GetControls(systemForm, TabName, SectionName, ControlId, DataField))
            {
                // Find the parent cell for cell attributes
                XElement parentCell = control.Parent;
                if (parentCell == null || parentCell.Name.LocalName != "cell")
                {
                    parentCell = null;
                }
                
                PSObject controlObj = FormXmlHelper.ParseControl(control, parentCell);
                // Add form and location context
                controlObj.Properties.Add(new PSNoteProperty("FormId", FormId));
                controlObj.Properties.Add(new PSNoteProperty("TabName", tabName));
                controlObj.Properties.Add(new PSNoteProperty("SectionName", sectionName));

                // Calculate Row and Column
                if (parentCell != null)
                {
                    XElement row = parentCell.Parent;
                    if (row != null && row.Name.LocalName == "row")
                    {
                        XElement rows = row.Parent;
                        if (rows != null && rows.Name.LocalName == "rows")
                        {
                            var rowList = rows.Elements("row").ToList();
                            int rowIndex = rowList.IndexOf(row);
                            var cellList = row.Elements("cell").ToList();
                            int columnIndex = cellList.IndexOf(parentCell);
                            controlObj.Properties.Add(new PSNoteProperty("Row", rowIndex));
                            controlObj.Properties.Add(new PSNoteProperty("Column", columnIndex));
                        }
                    }
                }

                WriteObject(controlObj);
            }
        }
    }
}
