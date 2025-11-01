using Microsoft.PowerPlatform.Dataverse.Client;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Query;
using System;
using System.Collections;
using System.Linq;
using System.Management.Automation;
using System.Xml.Linq;

namespace Rnwood.Dataverse.Data.PowerShell.Commands
{
    /// <summary>
    /// Retrieves forms from a Dataverse environment.
    /// </summary>
    [Cmdlet(VerbsCommon.Get, "DataverseForm")]
    [OutputType(typeof(PSObject))]
    public class GetDataverseFormCmdlet : OrganizationServiceCmdlet
    {
        /// <summary>
        /// Gets or sets the form ID to retrieve a specific form.
        /// </summary>
        [Parameter(ParameterSetName = "ById", Mandatory = true, ValueFromPipelineByPropertyName = true, HelpMessage = "ID of the form to retrieve")]
        [Alias("formid")]
        public Guid Id { get; set; }

        /// <summary>
        /// Gets or sets the logical name of the entity/table for which to retrieve forms.
        /// </summary>
        [Parameter(ParameterSetName = "ByEntity", Mandatory = true, HelpMessage = "Logical name of the entity/table for which to retrieve forms")]
        [Parameter(ParameterSetName = "ByName", Mandatory = true, HelpMessage = "Logical name of the entity/table")]
        [ArgumentCompleter(typeof(TableNameArgumentCompleter))]
        [Alias("EntityName", "TableName")]
        public string Entity { get; set; }

        /// <summary>
        /// Gets or sets the name of the form to retrieve.
        /// </summary>
        [Parameter(ParameterSetName = "ByName", Mandatory = true, HelpMessage = "Name of the form to retrieve")]
        public string Name { get; set; }

        /// <summary>
        /// Gets or sets the form type filter. Valid values: Main (2), QuickCreate (5), QuickView (6), Card (11), Dashboard (0), MainInteractionCentric (63), Other (100), MainBackup (101), AppointmentBook (102), Dialog (103).
        /// </summary>
        [Parameter(ParameterSetName = "ByEntity", HelpMessage = "Form type filter: Main (2), QuickCreate (5), QuickView (6), Card (11), Dashboard (0), MainInteractionCentric (63), Other (100), MainBackup (101), AppointmentBook (102), Dialog (103)")]
        [ValidateSet("Main", "QuickCreate", "QuickView", "Card", "Dashboard", "MainInteractionCentric", "Other", "MainBackup", "AppointmentBook", "Dialog")]
        public string FormType { get; set; }

        /// <summary>
        /// Gets or sets whether to include the FormXml in the output. Default is false for performance.
        /// </summary>
        [Parameter(HelpMessage = "Include the FormXml in the output (default: false for performance)")]
        public SwitchParameter IncludeFormXml { get; set; }

        /// <summary>
        /// Gets or sets whether to parse the FormXml and include structured tab/section/control information.
        /// </summary>
        [Parameter(HelpMessage = "Parse FormXml and include structured tab/section/control information")]
        public SwitchParameter ParseFormXml { get; set; }

        /// <summary>
        /// Processes the cmdlet request.
        /// </summary>
        protected override void ProcessRecord()
        {
            base.ProcessRecord();

            QueryExpression query = new QueryExpression("systemform")
            {
                ColumnSet = new ColumnSet("formid", "name", "objecttypecode", "type", "description", 
                    "formactivationstate", "formpresentation", "isdefault", "iscustomizable")
            };

            if (IncludeFormXml.IsPresent || ParseFormXml.IsPresent)
            {
                query.ColumnSet.AddColumn("formxml");
            }

            switch (ParameterSetName)
            {
                case "ById":
                    query.Criteria.AddCondition("formid", ConditionOperator.Equal, Id);
                    break;

                case "ByEntity":
                    query.Criteria.AddCondition("objecttypecode", ConditionOperator.Equal, Entity);
                    if (!string.IsNullOrEmpty(FormType))
                    {
                        query.Criteria.AddCondition("type", ConditionOperator.Equal, GetFormTypeValue(FormType));
                    }
                    break;

                case "ByName":
                    query.Criteria.AddCondition("objecttypecode", ConditionOperator.Equal, Entity);
                    query.Criteria.AddCondition("name", ConditionOperator.Equal, Name);
                    break;
            }

            EntityCollection results = Connection.RetrieveMultiple(query);

            foreach (Entity form in results.Entities)
            {
                PSObject output = new PSObject();
                output.Properties.Add(new PSNoteProperty("FormId", form.GetAttributeValue<Guid>("formid")));
                output.Properties.Add(new PSNoteProperty("Name", form.GetAttributeValue<string>("name")));
                output.Properties.Add(new PSNoteProperty("Entity", form.GetAttributeValue<string>("objecttypecode")));
                output.Properties.Add(new PSNoteProperty("Type", GetFormTypeName(form.GetAttributeValue<OptionSetValue>("type")?.Value ?? 0)));
                output.Properties.Add(new PSNoteProperty("TypeCode", form.GetAttributeValue<OptionSetValue>("type")?.Value));
                output.Properties.Add(new PSNoteProperty("Description", form.GetAttributeValue<string>("description")));
                output.Properties.Add(new PSNoteProperty("IsActive", form.GetAttributeValue<OptionSetValue>("formactivationstate")?.Value == 1));
                output.Properties.Add(new PSNoteProperty("Presentation", GetFormPresentationName(form.GetAttributeValue<OptionSetValue>("formpresentation")?.Value ?? 0)));
                output.Properties.Add(new PSNoteProperty("IsDefault", form.GetAttributeValue<bool?>("isdefault") ?? false));
                output.Properties.Add(new PSNoteProperty("IsCustomizable", form.GetAttributeValue<BooleanManagedProperty>("iscustomizable")?.Value ?? false));

                if (IncludeFormXml.IsPresent && form.Contains("formxml"))
                {
                    output.Properties.Add(new PSNoteProperty("FormXml", form.GetAttributeValue<string>("formxml")));
                }

                if (ParseFormXml.IsPresent && form.Contains("formxml"))
                {
                    try
                    {
                        string formXml = form.GetAttributeValue<string>("formxml");
                        if (!string.IsNullOrWhiteSpace(formXml))
                        {
                            XDocument doc = XDocument.Parse(formXml);
                            output.Properties.Add(new PSNoteProperty("ParsedForm", ParseFormStructure(doc)));
                        }
                    }
                    catch (Exception ex)
                    {
                        WriteWarning($"Failed to parse FormXml for form '{form.GetAttributeValue<string>("name")}': {ex.Message}");
                    }
                }

                WriteObject(output);
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
                case "MainInteractionCentric": return 63;
                case "Other": return 100;
                case "MainBackup": return 101;
                case "AppointmentBook": return 102;
                case "Dialog": return 103;
                default: return 2;
            }
        }

        private string GetFormTypeName(int typeCode)
        {
            switch (typeCode)
            {
                case 2: return "Main";
                case 5: return "QuickCreate";
                case 6: return "QuickView";
                case 11: return "Card";
                case 0: return "Dashboard";
                case 63: return "MainInteractionCentric";
                case 100: return "Other";
                case 101: return "MainBackup";
                case 102: return "AppointmentBook";
                case 103: return "Dialog";
                default: return $"Unknown ({typeCode})";
            }
        }

        private string GetFormPresentationName(int presentationCode)
        {
            switch (presentationCode)
            {
                case 0: return "ClassicForm";
                case 1: return "AirForm";
                case 2: return "ConvertedICForm";
                default: return $"Unknown ({presentationCode})";
            }
        }

        private PSObject ParseFormStructure(XDocument doc)
        {
            PSObject parsed = new PSObject();
            XElement systemForm = doc.Root?.Element("SystemForm");
            
            if (systemForm == null)
            {
                return parsed;
            }

            // Parse tabs
            var tabsElement = systemForm.Element("tabs");
            if (tabsElement != null)
            {
                var tabs = tabsElement.Elements("tab").Select(tab => ParseTab(tab)).ToArray();
                parsed.Properties.Add(new PSNoteProperty("Tabs", tabs));
            }

            // Parse events
            var eventsElement = systemForm.Element("events");
            if (eventsElement != null)
            {
                parsed.Properties.Add(new PSNoteProperty("Events", ParseEvents(eventsElement)));
            }

            // Parse navigation
            var navElement = systemForm.Element("Navigation");
            if (navElement != null)
            {
                parsed.Properties.Add(new PSNoteProperty("Navigation", ParseNavigation(navElement)));
            }

            return parsed;
        }

        private PSObject ParseTab(XElement tab)
        {
            PSObject tabObj = new PSObject();
            tabObj.Properties.Add(new PSNoteProperty("Id", tab.Attribute("id")?.Value));
            tabObj.Properties.Add(new PSNoteProperty("Name", tab.Attribute("name")?.Value));
            tabObj.Properties.Add(new PSNoteProperty("Expanded", tab.Attribute("expanded")?.Value == "true"));
            tabObj.Properties.Add(new PSNoteProperty("Visible", tab.Attribute("visible")?.Value != "false"));

            // Parse labels
            var labelsElement = tab.Element("labels");
            if (labelsElement != null)
            {
                var labels = labelsElement.Elements("label").Select(l => new PSObject(new
                {
                    Description = l.Attribute("description")?.Value,
                    LanguageCode = l.Attribute("languagecode")?.Value
                })).ToArray();
                tabObj.Properties.Add(new PSNoteProperty("Labels", labels));
            }

            // Parse columns/sections
            var columnsElement = tab.Element("columns");
            if (columnsElement != null)
            {
                var sections = columnsElement.Elements("column")
                    .SelectMany(col => col.Element("sections")?.Elements("section") ?? Enumerable.Empty<XElement>())
                    .Select(sec => ParseSection(sec))
                    .ToArray();
                tabObj.Properties.Add(new PSNoteProperty("Sections", sections));
            }

            return tabObj;
        }

        private PSObject ParseSection(XElement section)
        {
            PSObject secObj = new PSObject();
            secObj.Properties.Add(new PSNoteProperty("Id", section.Attribute("id")?.Value));
            secObj.Properties.Add(new PSNoteProperty("Name", section.Attribute("name")?.Value));
            secObj.Properties.Add(new PSNoteProperty("ShowLabel", section.Attribute("showlabel")?.Value != "false"));
            secObj.Properties.Add(new PSNoteProperty("Visible", section.Attribute("visible")?.Value != "false"));

            // Parse labels
            var labelsElement = section.Element("labels");
            if (labelsElement != null)
            {
                var labels = labelsElement.Elements("label").Select(l => new PSObject(new
                {
                    Description = l.Attribute("description")?.Value,
                    LanguageCode = l.Attribute("languagecode")?.Value
                })).ToArray();
                secObj.Properties.Add(new PSNoteProperty("Labels", labels));
            }

            // Parse controls
            var rowsElement = section.Element("rows");
            if (rowsElement != null)
            {
                var controls = rowsElement.Elements("row")
                    .SelectMany(row => row.Elements("cell"))
                    .SelectMany(cell => cell.Elements("control"))
                    .Select(ctrl => ParseControl(ctrl))
                    .ToArray();
                secObj.Properties.Add(new PSNoteProperty("Controls", controls));
            }

            return secObj;
        }

        private PSObject ParseControl(XElement control)
        {
            PSObject ctrlObj = new PSObject();
            ctrlObj.Properties.Add(new PSNoteProperty("Id", control.Attribute("id")?.Value));
            ctrlObj.Properties.Add(new PSNoteProperty("DataField", control.Attribute("datafieldname")?.Value));
            ctrlObj.Properties.Add(new PSNoteProperty("ClassId", control.Attribute("classid")?.Value));
            ctrlObj.Properties.Add(new PSNoteProperty("Disabled", control.Attribute("disabled")?.Value == "true"));
            ctrlObj.Properties.Add(new PSNoteProperty("Visible", control.Attribute("visible")?.Value != "false"));

            return ctrlObj;
        }

        private PSObject ParseEvents(XElement events)
        {
            PSObject eventsObj = new PSObject();
            
            // Parse form events (OnLoad, OnSave, etc.)
            var formEvents = events.Elements("event").Select(evt => new PSObject(new
            {
                Name = evt.Attribute("name")?.Value,
                Application = evt.Attribute("application")?.Value,
                Active = evt.Attribute("active")?.Value != "false",
                Handlers = evt.Elements("Handler").Select(h => new PSObject(new
                {
                    FunctionName = h.Attribute("functionName")?.Value,
                    LibraryName = h.Attribute("libraryName")?.Value,
                    HandlerUniqueId = h.Attribute("handlerUniqueId")?.Value,
                    Enabled = h.Attribute("enabled")?.Value != "false",
                    Parameters = h.Attribute("parameters")?.Value
                })).ToArray()
            })).ToArray();
            
            eventsObj.Properties.Add(new PSNoteProperty("FormEvents", formEvents));
            
            return eventsObj;
        }

        private PSObject ParseNavigation(XElement navigation)
        {
            PSObject navObj = new PSObject();
            
            var navItems = navigation.Elements("NavBarItem").Select(item => new PSObject(new
            {
                Id = item.Attribute("Id")?.Value,
                Area = item.Attribute("Area")?.Value,
                Sequence = item.Attribute("Sequence")?.Value
            })).ToArray();
            
            navObj.Properties.Add(new PSNoteProperty("Items", navItems));
            
            return navObj;
        }
    }
}
