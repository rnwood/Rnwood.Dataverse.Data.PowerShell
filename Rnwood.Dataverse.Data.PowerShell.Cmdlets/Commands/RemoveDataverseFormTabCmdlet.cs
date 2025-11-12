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
    /// Removes a tab from a Dataverse form.
    /// </summary>
    [Cmdlet(VerbsCommon.Remove, "DataverseFormTab", SupportsShouldProcess = true, ConfirmImpact = ConfirmImpact.High)]
    public class RemoveDataverseFormTabCmdlet : OrganizationServiceCmdlet
    {
        /// <summary>
        /// Gets or sets the form ID from which to remove the tab.
        /// </summary>
        [Parameter(Mandatory = true, ValueFromPipelineByPropertyName = true, HelpMessage = "ID of the form")]
        public Guid FormId { get; set; }

        /// <summary>
        /// Gets or sets the name of the tab to remove.
        /// </summary>
        [Parameter(Mandatory = false, HelpMessage = "Name of the tab to remove")]
        public string TabName { get; set; }

        /// <summary>
        /// Gets or sets the ID of the tab to remove.
        /// </summary>
        [Parameter(Mandatory = false, HelpMessage = "ID of the tab to remove")]
        public string TabId { get; set; }

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
                WriteWarning("Form has no tabs");
                return;
            }

            XElement tabToRemove = null;
            if (!string.IsNullOrEmpty(TabId))
            {
                tabToRemove = FormXmlHelper.FindTab(systemForm, tabId: TabId);
                if (tabToRemove == null)
                {
                    throw new InvalidOperationException($"Tab with ID '{TabId}' not found in form");
                }
            }
            else if (!string.IsNullOrEmpty(TabName))
            {
                tabToRemove = FormXmlHelper.FindTab(systemForm, tabName: TabName);
                if (tabToRemove == null)
                {
                    throw new InvalidOperationException($"Tab '{TabName}' not found in form");
                }
            }
            else
            {
                throw new ArgumentException("Either TabName or TabId must be provided");
            }

            string targetName = !string.IsNullOrEmpty(TabId) ? TabId : TabName;
            
            // Confirm action
            if (!ShouldProcess($"form '{FormId}'", $"Remove tab '{targetName}'"))
            {
                return;
            }

            // Remove tab
            tabToRemove.Remove();

            // Update form
            FormXmlHelper.UpdateFormXml(Connection, FormId, doc);
            WriteVerbose($"Removed tab '{targetName}' from form '{FormId}'");
        }
    }
}
