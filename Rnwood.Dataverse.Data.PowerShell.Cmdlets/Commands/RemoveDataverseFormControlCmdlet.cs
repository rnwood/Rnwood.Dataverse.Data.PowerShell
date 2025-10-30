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
    /// Removes a control from a Dataverse form.
    /// </summary>
    [Cmdlet(VerbsCommon.Remove, "DataverseFormControl", SupportsShouldProcess = true, ConfirmImpact = ConfirmImpact.High)]
    public class RemoveDataverseFormControlCmdlet : OrganizationServiceCmdlet
    {
        /// <summary>
        /// Gets or sets the form ID.
        /// </summary>
        [Parameter(Mandatory = true, ValueFromPipelineByPropertyName = true, HelpMessage = "ID of the form")]
        [Alias("formid")]
        public Guid FormId { get; set; }

        /// <summary>
        /// Gets or sets the control ID to remove.
        /// </summary>
        [Parameter(Mandatory = true, ParameterSetName = "ById", HelpMessage = "ID of the control to remove")]
        public string ControlId { get; set; }

        /// <summary>
        /// Gets or sets the data field name of the control to remove.
        /// </summary>
        [Parameter(Mandatory = true, ParameterSetName = "ByDataField", HelpMessage = "Data field name of the control to remove")]
        public string DataField { get; set; }

        /// <summary>
        /// Gets or sets the section name to limit the search.
        /// </summary>
        [Parameter(ParameterSetName = "ByDataField", HelpMessage = "Section name to limit the search")]
        public string SectionName { get; set; }

        /// <summary>
        /// Gets or sets whether to publish the form after removing the control.
        /// </summary>
        [Parameter(HelpMessage = "Publish the form after removing the control")]
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

            var tabsElement = systemForm.Element("tabs");
            if (tabsElement == null)
            {
                WriteWarning("Form has no tabs");
                return;
            }

            XElement controlToRemove = null;
            XElement parentRow = null;

            // Find the control
            foreach (var tab in tabsElement.Elements("tab"))
            {
                var columnsElement = tab.Element("columns");
                if (columnsElement == null) continue;

                foreach (var column in columnsElement.Elements("column"))
                {
                    var sectionsElement = column.Element("sections");
                    if (sectionsElement == null) continue;

                    var sections = sectionsElement.Elements("section");

                    if (ParameterSetName == "ByDataField" && !string.IsNullOrEmpty(SectionName))
                    {
                        sections = sections.Where(s => s.Attribute("name")?.Value == SectionName);
                    }

                    foreach (var section in sections)
                    {
                        var rowsElement = section.Element("rows");
                        if (rowsElement == null) continue;

                        foreach (var row in rowsElement.Elements("row"))
                        {
                            foreach (var cell in row.Elements("cell"))
                            {
                                var control = cell.Elements("control").FirstOrDefault(c =>
                                {
                                    if (ParameterSetName == "ById")
                                    {
                                        return c.Attribute("id")?.Value == ControlId;
                                    }
                                    else
                                    {
                                        return c.Attribute("datafieldname")?.Value == DataField;
                                    }
                                });

                                if (control != null)
                                {
                                    controlToRemove = control;
                                    parentRow = row;
                                    break;
                                }
                            }

                            if (controlToRemove != null) break;
                        }

                        if (controlToRemove != null) break;
                    }

                    if (controlToRemove != null) break;
                }

                if (controlToRemove != null) break;
            }

            if (controlToRemove == null)
            {
                string identifier = ParameterSetName == "ById" ? ControlId : DataField;
                throw new InvalidOperationException($"Control '{identifier}' not found in form");
            }

            string targetName = ParameterSetName == "ById" ? ControlId : DataField;

            // Confirm action
            if (!ShouldProcess($"form '{FormId}'", $"Remove control '{targetName}'"))
            {
                return;
            }

            // Remove the control's parent cell
            var parentCell = controlToRemove.Parent;
            if (parentCell != null)
            {
                parentCell.Remove();
                
                // If row is now empty, remove it too
                if (parentRow != null && !parentRow.Elements("cell").Any())
                {
                    parentRow.Remove();
                }
            }

            // Update form
            Entity updateForm = new Entity("systemform", FormId);
            updateForm["formxml"] = doc.ToString();
            Connection.Update(updateForm);

            WriteVerbose($"Removed control '{targetName}' from form '{FormId}'");

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
        }
    }
}
