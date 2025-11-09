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
    /// Creates or updates a form in a Dataverse environment.
    /// </summary>
    [Cmdlet(VerbsCommon.Set, "DataverseForm", SupportsShouldProcess = true, ConfirmImpact = ConfirmImpact.Medium)]
    [OutputType(typeof(Guid))]
    public class SetDataverseFormCmdlet : OrganizationServiceCmdlet
    {
        /// <summary>
        /// Gets or sets the form ID for updating an existing form.
        /// </summary>
        [Parameter(ParameterSetName = "Update", Mandatory = true, ValueFromPipelineByPropertyName = true, HelpMessage = "ID of the form to update")]
        [Parameter(ParameterSetName = "UpdateWithXml", Mandatory = true, ValueFromPipelineByPropertyName = true, HelpMessage = "ID of the form to update")]
        [Alias("formid")]
        public Guid Id { get; set; }

        /// <summary>
        /// Gets or sets the logical name of the entity/table for the form.
        /// </summary>
        [Parameter(ParameterSetName = "Create", Mandatory = true, HelpMessage = "Logical name of the entity/table for the form")]
        [Parameter(ParameterSetName = "CreateWithXml", Mandatory = true, HelpMessage = "Logical name of the entity/table for the form")]
        [Parameter(ParameterSetName = "Update", HelpMessage = "Logical name of the entity/table for the form")]
        [Parameter(ParameterSetName = "UpdateWithXml", HelpMessage = "Logical name of the entity/table for the form")]
        [ArgumentCompleter(typeof(TableNameArgumentCompleter))]
        [Alias("EntityName", "TableName", "ObjectTypeCode")]
        public string Entity { get; set; }

        /// <summary>
        /// Gets or sets the name of the form.
        /// </summary>
        [Parameter(ParameterSetName = "Create", Mandatory = true, HelpMessage = "Name of the form")]
        [Parameter(ParameterSetName = "CreateWithXml", Mandatory = true, HelpMessage = "Name of the form")]
        [Parameter(ParameterSetName = "Update", HelpMessage = "Name of the form")]
        [Parameter(ParameterSetName = "UpdateWithXml", HelpMessage = "Name of the form")]
        public string Name { get; set; }

        /// <summary>
        /// Gets or sets the form type.
        /// </summary>
        [Parameter(ParameterSetName = "Create", Mandatory = true, HelpMessage = "Form type")]
        [Parameter(ParameterSetName = "CreateWithXml", Mandatory = true, HelpMessage = "Form type")]
        [Parameter(ParameterSetName = "Update", HelpMessage = "Form type")]
        [Parameter(ParameterSetName = "UpdateWithXml", HelpMessage = "Form type")]
        public FormType? FormType { get; set; }

        /// <summary>
        /// Gets or sets the complete FormXml. When provided, this takes precedence over individual parameters.
        /// </summary>
        [Parameter(ParameterSetName = "CreateWithXml", Mandatory = true, HelpMessage = "Complete FormXml content")]
        [Parameter(ParameterSetName = "UpdateWithXml", Mandatory = true, HelpMessage = "Complete FormXml content")]
        [Alias("FormXml", "Xml")]
        public string FormXmlContent { get; set; }

        /// <summary>
        /// Gets or sets the description of the form.
        /// </summary>
        [Parameter(HelpMessage = "Description of the form")]
        public string Description { get; set; }

        /// <summary>
        /// Gets or sets whether the form is active.
        /// </summary>
        [Parameter(HelpMessage = "Whether the form is active (default: true)")]
        public SwitchParameter IsActive { get; set; } = true;

        /// <summary>
        /// Gets or sets whether this form is the default form for the entity.
        /// </summary>
        [Parameter(HelpMessage = "Whether this form is the default form for the entity")]
        public SwitchParameter IsDefault { get; set; }

        /// <summary>
        /// Gets or sets the form presentation type.
        /// </summary>
        [Parameter(HelpMessage = "Form presentation type")]
        public FormPresentation? FormPresentation { get; set; }

        /// <summary>
        /// Gets or sets whether to return the form ID after creation/update.
        /// </summary>
        [Parameter(HelpMessage = "Return the form ID after creation/update")]
        public SwitchParameter PassThru { get; set; }

        /// <summary>
        /// Gets or sets whether to publish the form after creation/update.
        /// </summary>
        [Parameter(HelpMessage = "Publish the form after creation/update")]
        public SwitchParameter Publish { get; set; }

        /// <summary>
        /// Processes the cmdlet request.
        /// </summary>
        protected override void ProcessRecord()
        {
            base.ProcessRecord();

            Entity form;
            bool isUpdate = ParameterSetName == "Update" || ParameterSetName == "UpdateWithXml";
            Guid formId;

            if (isUpdate)
            {
                formId = Id;
                form = new Entity("systemform", formId);
            }
            else
            {
                formId = Guid.NewGuid();
                form = new Entity("systemform")
                {
                    Id = formId
                };
                form["formid"] = formId;
            }

            // Set basic properties
            if (!string.IsNullOrEmpty(Name))
            {
                form["name"] = Name;
            }

            if (!string.IsNullOrEmpty(Entity))
            {
                form["objecttypecode"] = Entity;
            }

            if (FormType.HasValue)
            {
                form["type"] = new OptionSetValue((int)FormType.Value);
            }

            if (!string.IsNullOrEmpty(Description))
            {
                form["description"] = Description;
            }

            if (MyInvocation.BoundParameters.ContainsKey(nameof(IsActive)))
            {
                form["formactivationstate"] = new OptionSetValue(IsActive.IsPresent ? 1 : 0);
            }

            if (MyInvocation.BoundParameters.ContainsKey(nameof(IsDefault)))
            {
                form["isdefault"] = IsDefault.IsPresent;
            }

            if (FormPresentation.HasValue)
            {
                form["formpresentation"] = new OptionSetValue((int)FormPresentation.Value);
            }

            // Handle FormXml
            if (!string.IsNullOrEmpty(FormXmlContent))
            { 
                form["formxml"] = FormXmlContent;  
            }
            else if (!isUpdate)
            {
                // Create minimal FormXml for new forms
                form["formxml"] = GenerateMinimalFormXml(formId, Entity, Name);
            }

            // Confirm action
            string action = isUpdate ? "Update" : "Create";
            string target = isUpdate ? $"form '{Id}'" : $"form '{Name}' for entity '{Entity}'";
            
            if (!ShouldProcess(target, action))
            {
                return;
            }

            // Create or update the form
            try
            {
                if (isUpdate)
                {
                    Connection.Update(form);
                    WriteVerbose($"Updated form '{formId}'");
                }
                else
                {
                    formId = Connection.Create(form);
                    WriteVerbose($"Created form '{formId}'");
                }

                // Publish if requested
                if (Publish.IsPresent)
                {
                    WriteVerbose($"Publishing form '{formId}'...");
                    var publishRequest = new PublishXmlRequest
                    {
                        ParameterXml = $"<importexportxml><entities><entity>{Entity}</entity></entities></importexportxml>"
                    };
                    Connection.Execute(publishRequest);
                    WriteVerbose("Form published successfully");
                }

                if (PassThru.IsPresent)
                {
                    WriteObject(formId);
                }
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"Failed to {action.ToLower()} form: {ex.Message}", ex);
            }
        }



        private string GenerateMinimalFormXml(Guid formId, string entityName, string formName)
        {
            string tabId = Guid.NewGuid().ToString("D").ToUpper();
            string sectionId = Guid.NewGuid().ToString("D").ToUpper();
            
            return $@"<form>
  <tabs>
    <tab id=""{{{tabId}}}"" name=""General"" IsUserDefined=""0"" visible=""true"">
      <labels>
        <label description=""General"" languagecode=""1033"" />
      </labels>
      <columns>
        <column width=""100%"">
          <sections>
            <section id=""{{{sectionId}}}"" name=""GeneralSection"" showlabel=""true"">
              <labels>
                <label description=""General"" languagecode=""1033"" />
              </labels>
              <rows>
                <row>
                </row>
              </rows>
            </section>
          </sections>
        </column>
      </columns>
    </tab>
  </tabs>
</form>";
        }
    }
}
