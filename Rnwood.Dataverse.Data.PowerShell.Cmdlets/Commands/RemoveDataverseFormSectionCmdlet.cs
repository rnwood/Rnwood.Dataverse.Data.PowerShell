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
    /// Removes a section from a Dataverse form tab.
    /// </summary>
    [Cmdlet(VerbsCommon.Remove, "DataverseFormSection", SupportsShouldProcess = true, ConfirmImpact = ConfirmImpact.High)]
    public class RemoveDataverseFormSectionCmdlet : OrganizationServiceCmdlet
    {
        /// <summary>
        /// Gets or sets the form ID.
        /// </summary>
        [Parameter(Mandatory = true, ValueFromPipelineByPropertyName = true, HelpMessage = "ID of the form")]
        public Guid FormId { get; set; }

        /// <summary>
        /// Gets or sets the tab name where the section is located (used when removing by section name).
        /// </summary>
        [Parameter(Mandatory = false, HelpMessage = "Name of the tab containing the section (required when using SectionName)")]
        public string TabName { get; set; }

        /// <summary>
        /// Gets or sets the section name to remove.
        /// </summary>
        [Parameter(Mandatory = false, HelpMessage = "Name of the section to remove")]
        public string SectionName { get; set; }

        /// <summary>
        /// Gets or sets the section ID to remove.
        /// </summary>
        [Parameter(Mandatory = false, HelpMessage = "ID of the section to remove")]
        public string SectionId { get; set; }

        /// <summary>
        /// Processes the cmdlet request.
        /// </summary>
        protected override void ProcessRecord()
        {
            base.ProcessRecord();

            // Retrieve the form
            Entity form = FormXmlHelper.RetrieveForm(Connection, FormId, new ColumnSet("formxml", "objecttypecode"));
            var (doc, systemForm) = FormXmlHelper.ParseFormXml(form);

            var tabsElement = systemForm.Element("tabs");
            if (tabsElement == null)
            {
                WriteWarning("Form has no tabs");
                return;
            }

            XElement sectionToRemove = null;
            if (!string.IsNullOrEmpty(SectionId))
            {
                sectionToRemove = FormXmlHelper.FindSection(systemForm, sectionId: SectionId);
            }
            else if (!string.IsNullOrEmpty(SectionName))
            {
                // If TabName is provided, search within that tab; otherwise search globally
                if (!string.IsNullOrEmpty(TabName))
                {
                    var tab = FormXmlHelper.FindTab(systemForm, TabName);
                    if (tab == null)
                    {
                        throw new InvalidOperationException($"Tab '{TabName}' not found in form");
                    }
                    sectionToRemove = FormXmlHelper.FindSectionInTab(tab, sectionName: SectionName);
                }
                else
                {
                    sectionToRemove = FormXmlHelper.FindSection(systemForm, sectionName: SectionName);
                }
            }
            else
            {
                throw new ArgumentException("Either SectionName or SectionId must be provided");
            }

            if (sectionToRemove == null)
            {
                string identifier = !string.IsNullOrEmpty(SectionId) ? SectionId : SectionName;
                throw new InvalidOperationException($"Section '{identifier}' not found in form");
            }

            string targetName = !string.IsNullOrEmpty(SectionId) ? SectionId : SectionName;

            // Confirm action
            if (!ShouldProcess($"form '{FormId}'", $"Remove section '{targetName}'"))
            {
                return;
            }

            // Remove section
            sectionToRemove.Remove();

            // Update form
            FormXmlHelper.UpdateFormXml(Connection, FormId, doc);
            WriteVerbose($"Removed section '{targetName}' from form '{FormId}'");
        }
    }
}
