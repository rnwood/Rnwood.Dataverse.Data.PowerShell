using Microsoft.Crm.Sdk.Messages;
using Microsoft.PowerPlatform.Dataverse.Client;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Query;
using System;
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
        /// Gets or sets the form type. Valid values: Main (2), QuickCreate (5), QuickView (6), Card (11), Dashboard (0).
        /// </summary>
        [Parameter(ParameterSetName = "Create", Mandatory = true, HelpMessage = "Form type: Main, QuickCreate, QuickView, Card, Dashboard")]
        [Parameter(ParameterSetName = "CreateWithXml", Mandatory = true, HelpMessage = "Form type: Main, QuickCreate, QuickView, Card, Dashboard")]
        [Parameter(ParameterSetName = "Update", HelpMessage = "Form type: Main, QuickCreate, QuickView, Card, Dashboard")]
        [Parameter(ParameterSetName = "UpdateWithXml", HelpMessage = "Form type: Main, QuickCreate, QuickView, Card, Dashboard")]
        [ValidateSet("Main", "QuickCreate", "QuickView", "Card", "Dashboard")]
        public string FormType { get; set; }

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
        [Parameter(HelpMessage = "Form presentation: ClassicForm (0), AirForm (1), ConvertedICForm (2)")]
        [ValidateSet("ClassicForm", "AirForm", "ConvertedICForm")]
        public string FormPresentation { get; set; }

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

            if (!string.IsNullOrEmpty(FormType))
            {
                form["type"] = new OptionSetValue(GetFormTypeValue(FormType));
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

            if (!string.IsNullOrEmpty(FormPresentation))
            {
                form["formpresentation"] = new OptionSetValue(GetFormPresentationValue(FormPresentation));
            }

            // Handle FormXml
            if (!string.IsNullOrEmpty(FormXmlContent))
            {
                // Validate and update FormXml
                try
                {
                    XDocument doc = XDocument.Parse(FormXmlContent);
                    
                    // Ensure formid in XML matches the entity formid
                    XElement systemForm = doc.Root?.Element("SystemForm");
                    if (systemForm != null)
                    {
                        XElement formIdElement = systemForm.Element("formid");
                        if (formIdElement != null)
                        {
                            formIdElement.Value = formId.ToString("B").ToUpper();
                        }
                        else
                        {
                            systemForm.Add(new XElement("formid", formId.ToString("B").ToUpper()));
                        }
                    }
                    
                    form["formxml"] = doc.ToString();
                }
                catch (Exception ex)
                {
                    throw new ArgumentException($"Invalid FormXml: {ex.Message}", nameof(FormXmlContent), ex);
                }
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

        private int GetFormTypeValue(string formType)
        {
            switch (formType)
            {
                case "Main": return 2;
                case "QuickCreate": return 5;
                case "QuickView": return 6;
                case "Card": return 11;
                case "Dashboard": return 0;
                default: return 2;
            }
        }

        private int GetFormPresentationValue(string presentation)
        {
            switch (presentation)
            {
                case "ClassicForm": return 0;
                case "AirForm": return 1;
                case "ConvertedICForm": return 2;
                default: return 0;
            }
        }

        private string GenerateMinimalFormXml(Guid formId, string entityName, string formName)
        {
            string formIdString = formId.ToString("B").ToUpper();
            
            return $@"<forms type=""main"">
  <SystemForm>
    <formid>{formIdString}</formid>
    <FormPresentation>0</FormPresentation>
    <IsCustomizable>1</IsCustomizable>
    <tabs>
      <tab name=""general"" id=""{Guid.NewGuid():B}"" expanded=""true"" showlabel=""false"" verticallayout=""true"">
        <labels>
          <label description=""General"" languagecode=""1033"" />
        </labels>
        <columns>
          <column width=""100%"">
            <sections>
              <section name=""section_1"" showlabel=""false"" showbar=""false"" id=""{Guid.NewGuid():B}"" columns=""1"" labelwidth=""115"" celllabelalignment=""Left"" celllabelposition=""Left"">
                <labels>
                  <label description=""Section"" languagecode=""1033"" />
                </labels>
                <rows />
              </section>
            </sections>
          </column>
        </columns>
      </tab>
    </tabs>
    <Navigation />
    <footer />
    <events />
  </SystemForm>
</forms>";
        }
    }
}
