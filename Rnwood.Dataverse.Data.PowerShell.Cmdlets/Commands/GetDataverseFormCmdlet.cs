using Microsoft.PowerPlatform.Dataverse.Client;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Query;
using Rnwood.Dataverse.Data.PowerShell.Model;
using System;
using System.Collections;
using System.Linq;
using System.Management.Automation;
using System.Xml.Linq;

namespace Rnwood.Dataverse.Data.PowerShell.Commands
{
    /// <summary>
    /// Form type enumeration.
    /// </summary>
    public enum FormType
    {
        /// <summary>Dashboard form type.</summary>
        Dashboard = 0,
        /// <summary>AppointmentBook form type.</summary>
        AppointmentBook = 1,
        /// <summary>Main form type.</summary>
        Main = 2,
        /// <summary>MiniCampaignBO form type.</summary>
        MiniCampaignBO = 3,
        /// <summary>Preview form type.</summary>
        Preview = 4,
        /// <summary>Mobile - Express form type.</summary>
        MobileExpress = 5,
        /// <summary>Quick View Form form type.</summary>
        QuickViewForm = 6,
        /// <summary>Quick Create form type.</summary>
        QuickCreate = 7,
        /// <summary>Dialog form type.</summary>
        Dialog = 8,
        /// <summary>Task Flow Form form type.</summary>
        TaskFlowForm = 9,
        /// <summary>InteractionCentricDashboard form type.</summary>
        InteractionCentricDashboard = 10,
        /// <summary>Card form type.</summary>
        Card = 11,
        /// <summary>Main - Interactive experience form type.</summary>
        MainInteractiveExperience = 12,
        /// <summary>Contextual Dashboard form type.</summary>
        ContextualDashboard = 13,
        /// <summary>Other form type.</summary>
        Other = 100,
        /// <summary>MainBackup form type.</summary>
        MainBackup = 101,
        /// <summary>AppointmentBookBackup form type.</summary>
        AppointmentBookBackup = 102,
        /// <summary>Power BI Dashboard form type.</summary>
        PowerBIDashboard = 103
    }

    /// <summary>
    /// Form presentation enumeration.
    /// </summary>
    public enum FormPresentation
    {
        /// <summary>Classic form presentation.</summary>
        ClassicForm = 0,
        /// <summary>Air form presentation.</summary>
        AirForm = 1,
        /// <summary>Converted IC form presentation.</summary>
        ConvertedICForm = 2
    }

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
        [Parameter(ParameterSetName = "ByUniqueName", Mandatory = true, HelpMessage = "Logical name of the entity/table")]
        [ArgumentCompleter(typeof(TableNameArgumentCompleter))]
        [Alias("EntityName", "TableName")]
        public string Entity { get; set; }

        /// <summary>
        /// Gets or sets the name of the form to retrieve.
        /// </summary>
        [Parameter(ParameterSetName = "ByName", Mandatory = true, HelpMessage = "Name of the form to retrieve")]
        public string Name { get; set; }

        /// <summary>
        /// Gets or sets the unique name of the form to retrieve.
        /// </summary>
        [Parameter(ParameterSetName = "ByUniqueName", Mandatory = true, HelpMessage = "Unique name of the form to retrieve")]
        public string UniqueName { get; set; }

        /// <summary>
        /// Gets or sets the form type filter.
        /// </summary>
        [Parameter(ParameterSetName = "ByEntity", HelpMessage = "Form type filter")]
        public FormType? FormType { get; set; }

        /// <summary>
        /// Gets or sets the unique name filter for forms.
        /// </summary>
        [Parameter(ParameterSetName = "ByEntity", HelpMessage = "Unique name filter for forms")]
        public string UniqueNameFilter { get; set; }

        /// <summary>
        /// Gets or sets whether to include the FormXml in the output. Default is false for performance.
        /// </summary>
        [Parameter(HelpMessage = "Include the FormXml in the output (default: false for performance)")]
        public SwitchParameter IncludeFormXml { get; set; }

        /// <summary>
        /// Gets or sets whether to include unpublished forms in the results.
        /// </summary>
        [Parameter(HelpMessage = "Include unpublished forms in the results")]
        public SwitchParameter Unpublished { get; set; }

        /// <summary>
        /// Processes the cmdlet request.
        /// </summary>
        protected override void ProcessRecord()
        {
            base.ProcessRecord();

            QueryExpression query = new QueryExpression("systemform")
            {
                ColumnSet = new ColumnSet("formid", "name", "uniquename", "objecttypecode", "type", "description",
                    "formactivationstate", "formpresentation", "isdefault")
            };

            if (IncludeFormXml.IsPresent)
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
                    if (FormType.HasValue)
                    {
                        query.Criteria.AddCondition("type", ConditionOperator.Equal, (int)FormType.Value);
                    }
                    if (!string.IsNullOrEmpty(UniqueNameFilter))
                    {
                        query.Criteria.AddCondition("uniquename", ConditionOperator.Equal, UniqueNameFilter);
                    }
                    break;

                case "ByName":
                    query.Criteria.AddCondition("objecttypecode", ConditionOperator.Equal, Entity);
                    query.Criteria.AddCondition("name", ConditionOperator.Equal, Name);
                    break;

                case "ByUniqueName":
                    query.Criteria.AddCondition("objecttypecode", ConditionOperator.Equal, Entity);
                    query.Criteria.AddCondition("uniquename", ConditionOperator.Equal, UniqueName);
                    break;
            }

            var results = QueryHelpers.ExecuteQueryWithPaging(query, Connection, WriteVerbose, Unpublished.IsPresent);

            foreach (Entity form in results)
            {
                PSObject output = new PSObject();
                output.Properties.Add(new PSNoteProperty("FormId", form.GetAttributeValue<Guid>("formid")));
                output.Properties.Add(new PSNoteProperty("Name", form.GetAttributeValue<string>("name")));
                output.Properties.Add(new PSNoteProperty("UniqueName", form.GetAttributeValue<string>("uniquename")));
                output.Properties.Add(new PSNoteProperty("Entity", form.GetAttributeValue<string>("objecttypecode")));
                output.Properties.Add(new PSNoteProperty("Type", (FormType)Enum.ToObject(typeof(FormType), form.GetAttributeValue<OptionSetValue>("type")?.Value ?? 0)));
                output.Properties.Add(new PSNoteProperty("Description", form.GetAttributeValue<string>("description")));
                output.Properties.Add(new PSNoteProperty("IsActive", form.GetAttributeValue<OptionSetValue>("formactivationstate")?.Value == 1));
                output.Properties.Add(new PSNoteProperty("Presentation", (FormPresentation)Enum.ToObject(typeof(FormPresentation), form.GetAttributeValue<OptionSetValue>("formpresentation")?.Value ?? 0)));
                output.Properties.Add(new PSNoteProperty("IsDefault", form.GetAttributeValue<bool?>("isdefault") ?? false));

                if (IncludeFormXml.IsPresent && form.Contains("formxml"))
                {
                    output.Properties.Add(new PSNoteProperty("FormXml", form.GetAttributeValue<string>("formxml")));
                }

                WriteObject(output);
            }
        }


    }
}
