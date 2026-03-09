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
    /// Removes a control from a Dataverse form.
    /// </summary>
    [Cmdlet(VerbsCommon.Remove, "DataverseFormControl", SupportsShouldProcess = true, ConfirmImpact = ConfirmImpact.High)]
    public class RemoveDataverseFormControlCmdlet : OrganizationServiceCmdlet
    {
        /// <summary>
        /// Gets or sets the form ID.
        /// </summary>
        [Parameter(Mandatory = true, ValueFromPipelineByPropertyName = true, HelpMessage = "ID of the form")]
        [ArgumentCompleter(typeof(FormIdArgumentCompleter))]
        public Guid FormId { get; set; }

        /// <summary>
        /// Gets or sets the control ID to remove.
        /// </summary>
        [Parameter(Mandatory = true, ParameterSetName = "ById", HelpMessage = "ID of the control to remove")]
        [ArgumentCompleter(typeof(FormControlIdArgumentCompleter))]
        public string ControlId { get; set; }

        /// <summary>
        /// Gets or sets the data field name of the control to remove.
        /// </summary>
        [Parameter(Mandatory = true, ParameterSetName = "ByDataField", HelpMessage = "Data field name of the control to remove")]
        [ArgumentCompleter(typeof(ColumnNameArgumentCompleter))]
        public string DataField { get; set; }

        /// <summary>
        /// Gets or sets the section name to limit the search.
        /// </summary>
        [Parameter(ParameterSetName = "ByDataField", HelpMessage = "Section name to limit the search. Not used for header controls.")]
        [Parameter(ParameterSetName = "ById", HelpMessage = "Section name to limit the search. Not used for header controls.")]
        [ArgumentCompleter(typeof(FormSectionNameArgumentCompleter))]
        public string SectionName { get; set; }

        /// <summary>
        /// Gets or sets the tab name where the section is located. Use '[Header]' for header controls.
        /// </summary>
        [Parameter(ParameterSetName = "ByDataField", HelpMessage = "Name of the tab containing the section. Use '[Header]' for header controls.")]
        [Parameter(ParameterSetName = "ById", HelpMessage = "Name of the tab containing the section. Use '[Header]' for header controls.")]
        [ArgumentCompleter(typeof(FormTabNameArgumentCompleter))]
        public string TabName { get; set; }

        /// <summary>
        /// Processes the cmdlet request.
        /// </summary>
        protected override void ProcessRecord()
        {
            base.ProcessRecord();

            // Check if this is a header control operation
            bool isHeaderControl = TabName != null && TabName.Equals("[Header]", StringComparison.OrdinalIgnoreCase);

            // Retrieve the form
            Entity form = FormXmlHelper.RetrieveForm(Connection, FormId, new ColumnSet("formxml", "objecttypecode"));
            var (doc, systemForm) = FormXmlHelper.ParseFormXml(form);

            // Find the control to remove
            XElement controlToRemove = null;
            XElement parentCell = null;
            
            if (ParameterSetName == "ById")
            {
                // Try header first
                (controlToRemove, parentCell) = FormXmlHelper.FindControlInHeader(systemForm, controlId: ControlId);
                
                // If not in header, search in regular sections
                if (controlToRemove == null)
                {
                    (controlToRemove, parentCell) = FormXmlHelper.FindControl(systemForm, controlId: ControlId);
                }
            }
            else
            {
                // ByDataField parameter set
                if (isHeaderControl)
                {
                    // Search in header
                    (controlToRemove, parentCell) = FormXmlHelper.FindControlInHeader(systemForm, dataField: DataField);
                }
                else if (!string.IsNullOrEmpty(SectionName))
                {
                    // Search in specific section
                    (controlToRemove, parentCell) = FormXmlHelper.FindControlByDataField(systemForm, TabName, SectionName, DataField);
                }
                else
                {
                    // Search in all sections of the tab
                    var tab = FormXmlHelper.FindTab(systemForm, tabName: TabName);
                    if (tab == null)
                    {
                        throw new InvalidOperationException($"Tab '{TabName}' not found in form");
                    }
                    
                    // Search through all sections in the tab
                    foreach (var section in tab.Descendants("section"))
                    {
                        var rows = section.Element("rows");
                        if (rows != null)
                        {
                            foreach (var row in rows.Elements("row"))
                            {
                                foreach (var cell in row.Elements("cell"))
                                {
                                    var control = cell.Elements("control").FirstOrDefault(c => c.Attribute("datafieldname")?.Value == DataField);
                                    if (control != null)
                                    {
                                        controlToRemove = control;
                                        parentCell = cell;
                                        break;
                                    }
                                }
                                if (controlToRemove != null) break;
                            }
                            if (controlToRemove != null) break;
                        }
                    }
                }
            }

            if (controlToRemove == null)
            {
                string identifier = ParameterSetName == "ById" ? ControlId : DataField;
                string location = isHeaderControl ? "header" : (string.IsNullOrEmpty(SectionName) ? $"tab '{TabName}'" : $"section '{SectionName}'");
                throw new InvalidOperationException($"Control '{identifier}' not found in {location}");
            }

            string targetName = ParameterSetName == "ById" ? ControlId : DataField;
            string locationDescriptor = isHeaderControl ? "header" : (string.IsNullOrEmpty(SectionName) ? $"tab '{TabName}'" : $"section '{SectionName}'");

            // Confirm action
            if (!ShouldProcess($"form '{FormId}', {locationDescriptor}", $"Remove control '{targetName}'"))
            {
                return;
            }

            // Remove the control's parent cell
            if (parentCell != null)
            {
                var parentRow = parentCell.Parent; // Get the row containing the cell
                parentCell.Remove();
                
                // If row is now empty, remove it too
                if (parentRow != null && !parentRow.Elements("cell").Any())
                {
                    parentRow.Remove();
                }
            }

            // Update form
            FormXmlHelper.UpdateFormXml(Connection, FormId, doc);
            WriteVerbose($"Removed control '{targetName}' from form '{FormId}' {locationDescriptor}");
        }
    }
}
