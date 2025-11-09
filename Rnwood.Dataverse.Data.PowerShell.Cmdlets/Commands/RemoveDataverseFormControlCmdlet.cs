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
        /// Gets or sets the tab name where the section is located.
        /// </summary>
        [Parameter(Mandatory = true, ParameterSetName = "ByDataField", HelpMessage = "Name of the tab containing the section")]
        public string TabName { get; set; }

        /// <summary>
        /// Processes the cmdlet request.
        /// </summary>
        protected override void ProcessRecord()
        {
            base.ProcessRecord();

            // Retrieve the form
            Entity form = FormXmlHelper.RetrieveForm(Connection, FormId, new ColumnSet("formxml", "objecttypecode"));
            var (doc, systemForm) = FormXmlHelper.ParseFormXml(form);

            // Find the control to remove
            XElement controlToRemove = null;
            XElement parentRow = null;
            if (ParameterSetName == "ById")
            {
                (controlToRemove, parentRow) = FormXmlHelper.FindControl(systemForm, controlId: ControlId);
            }
            else
            {
                (controlToRemove, parentRow) = FormXmlHelper.FindControlByDataField(systemForm, TabName, SectionName, DataField);
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
            FormXmlHelper.UpdateFormXml(Connection, FormId, doc);
            WriteVerbose($"Removed control '{targetName}' from form '{FormId}'");
        }
    }
}
