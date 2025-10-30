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
    /// Removes a section from a Dataverse form tab.
    /// </summary>
    [Cmdlet(VerbsCommon.Remove, "DataverseFormSection", SupportsShouldProcess = true, ConfirmImpact = ConfirmImpact.High)]
    public class RemoveDataverseFormSectionCmdlet : OrganizationServiceCmdlet
    {
        /// <summary>
        /// Gets or sets the form ID.
        /// </summary>
        [Parameter(Mandatory = true, ValueFromPipelineByPropertyName = true, HelpMessage = "ID of the form")]
        [Alias("formid")]
        public Guid FormId { get; set; }

        /// <summary>
        /// Gets or sets the section name to remove.
        /// </summary>
        [Parameter(Mandatory = true, ParameterSetName = "ByName", HelpMessage = "Name of the section to remove")]
        public string SectionName { get; set; }

        /// <summary>
        /// Gets or sets the section ID to remove.
        /// </summary>
        [Parameter(Mandatory = true, ParameterSetName = "ById", HelpMessage = "ID of the section to remove")]
        public string SectionId { get; set; }

        /// <summary>
        /// Gets or sets whether to publish the form after removing the section.
        /// </summary>
        [Parameter(HelpMessage = "Publish the form after removing the section")]
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

            XElement sectionToRemove = null;

            // Find the section
            foreach (var tab in tabsElement.Elements("tab"))
            {
                var columnsElement = tab.Element("columns");
                if (columnsElement == null) continue;

                foreach (var column in columnsElement.Elements("column"))
                {
                    var sectionsElement = column.Element("sections");
                    if (sectionsElement == null) continue;

                    if (ParameterSetName == "ByName")
                    {
                        sectionToRemove = sectionsElement.Elements("section")
                            .FirstOrDefault(s => s.Attribute("name")?.Value == SectionName);
                    }
                    else
                    {
                        sectionToRemove = sectionsElement.Elements("section")
                            .FirstOrDefault(s => s.Attribute("id")?.Value == SectionId);
                    }

                    if (sectionToRemove != null) break;
                }

                if (sectionToRemove != null) break;
            }

            if (sectionToRemove == null)
            {
                string identifier = ParameterSetName == "ByName" ? SectionName : SectionId;
                throw new InvalidOperationException($"Section '{identifier}' not found in form");
            }

            string targetName = ParameterSetName == "ByName" ? SectionName : SectionId;

            // Confirm action
            if (!ShouldProcess($"form '{FormId}'", $"Remove section '{targetName}'"))
            {
                return;
            }

            // Remove section
            sectionToRemove.Remove();

            // Update form
            Entity updateForm = new Entity("systemform", FormId);
            updateForm["formxml"] = doc.ToString();
            Connection.Update(updateForm);

            WriteVerbose($"Removed section '{targetName}' from form '{FormId}'");

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
