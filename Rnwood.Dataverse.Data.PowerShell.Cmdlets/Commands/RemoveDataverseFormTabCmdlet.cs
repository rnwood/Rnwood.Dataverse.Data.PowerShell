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
    /// Removes a tab from a Dataverse form.
    /// </summary>
    [Cmdlet(VerbsCommon.Remove, "DataverseFormTab", SupportsShouldProcess = true, ConfirmImpact = ConfirmImpact.High)]
    public class RemoveDataverseFormTabCmdlet : OrganizationServiceCmdlet
    {
        /// <summary>
        /// Gets or sets the form ID from which to remove the tab.
        /// </summary>
        [Parameter(Mandatory = true, ValueFromPipelineByPropertyName = true, HelpMessage = "ID of the form")]
        [Alias("formid")]
        public Guid FormId { get; set; }

        /// <summary>
        /// Gets or sets the name of the tab to remove.
        /// </summary>
        [Parameter(Mandatory = true, ParameterSetName = "ByName", HelpMessage = "Name of the tab to remove")]
        public string TabName { get; set; }

        /// <summary>
        /// Gets or sets the ID of the tab to remove.
        /// </summary>
        [Parameter(Mandatory = true, ParameterSetName = "ById", HelpMessage = "ID of the tab to remove")]
        public string TabId { get; set; }

        /// <summary>
        /// Gets or sets whether to publish the form after removing the tab.
        /// </summary>
        [Parameter(HelpMessage = "Publish the form after removing the tab")]
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
                WriteWarning("Form has no tabs");
                return;
            }

            XElement tabToRemove = null;
            if (ParameterSetName == "ByName")
            {
                tabToRemove = tabsElement.Elements("tab")
                    .FirstOrDefault(t => t.Attribute("name")?.Value == TabName);
                
                if (tabToRemove == null)
                {
                    throw new InvalidOperationException($"Tab '{TabName}' not found in form");
                }
            }
            else
            {
                tabToRemove = tabsElement.Elements("tab")
                    .FirstOrDefault(t => t.Attribute("id")?.Value == TabId);
                
                if (tabToRemove == null)
                {
                    throw new InvalidOperationException($"Tab with ID '{TabId}' not found in form");
                }
            }

            string targetName = ParameterSetName == "ByName" ? TabName : TabId;
            
            // Confirm action
            if (!ShouldProcess($"form '{FormId}'", $"Remove tab '{targetName}'"))
            {
                return;
            }

            // Remove tab
            tabToRemove.Remove();

            // Update form
            Entity updateForm = new Entity("systemform", FormId);
            updateForm["formxml"] = doc.ToString();
            Connection.Update(updateForm);

            WriteVerbose($"Removed tab '{targetName}' from form '{FormId}'");

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
