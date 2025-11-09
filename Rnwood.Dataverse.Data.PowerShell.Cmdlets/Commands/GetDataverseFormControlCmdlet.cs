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
        /// Gets or sets the section name to filter controls.
        /// </summary>
        [Parameter(HelpMessage = "Name of the section containing the controls")]
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
        /// Gets or sets the tab name to filter controls.
        /// </summary>
        [Parameter(HelpMessage = "Name of the tab containing the section")]
        public string TabName { get; set; }

        /// <summary>
        /// Processes the cmdlet request.
        /// </summary>
        protected override void ProcessRecord()
        {
            base.ProcessRecord();

            Entity form = FormXmlHelper.RetrieveForm(Connection, FormId, new ColumnSet("formxml"));
            var (doc, systemForm) = FormXmlHelper.ParseFormXml(form);

            foreach (var (tabName, sectionName, control) in FormXmlHelper.GetControls(systemForm, TabName, SectionName, ControlId, DataField))
            {
                PSObject controlObj = FormXmlHelper.ParseControl(control);
                // Add form and location context
                controlObj.Properties.Add(new PSNoteProperty("FormId", FormId));
                controlObj.Properties.Add(new PSNoteProperty("TabName", tabName));
                controlObj.Properties.Add(new PSNoteProperty("SectionName", sectionName));

                WriteObject(controlObj);
            }
        }
    }
}
