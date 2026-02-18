using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Query;
using System;
using System.Management.Automation;

namespace Rnwood.Dataverse.Data.PowerShell.Commands
{
    /// <summary>
    /// Creates or updates a Copilot Studio bot component in Dataverse.
    /// </summary>
    [Cmdlet(VerbsCommon.Set, "DataverseBotComponent", SupportsShouldProcess = true, ConfirmImpact = ConfirmImpact.Medium)]
    [OutputType(typeof(PSObject))]
    public class SetDataverseBotComponentCmdlet : OrganizationServiceCmdlet
    {
        /// <summary>
        /// Gets or sets the bot component ID. If provided, updates the existing component. If not provided, creates a new component.
        /// </summary>
        [Parameter(ValueFromPipelineByPropertyName = true, HelpMessage = "Bot component ID (GUID). If provided, updates the existing component. If not provided, creates a new component.")]
        public Guid? BotComponentId { get; set; }

        /// <summary>
        /// Gets or sets the component name.
        /// </summary>
        [Parameter(Mandatory = true, HelpMessage = "Component name.")]
        public string Name { get; set; }

        /// <summary>
        /// Gets or sets the component schema name. Required for new components.
        /// </summary>
        [Parameter(HelpMessage = "Component schema name. Required when creating a new component.")]
        public string SchemaName { get; set; }

        /// <summary>
        /// Gets or sets the parent bot ID. Required for new components.
        /// </summary>
        [Parameter(HelpMessage = "Parent bot ID (GUID). Required when creating a new component.")]
        public Guid? ParentBotId { get; set; }

        /// <summary>
        /// Gets or sets the component type. Required for new components.
        /// </summary>
        [Parameter(HelpMessage = "Component type (10=Topic, 11=Skill, etc.). Required when creating a new component.")]
        public int? ComponentType { get; set; }

        /// <summary>
        /// Gets or sets the component data (e.g., YAML content).
        /// </summary>
        [Parameter(HelpMessage = "Component data (e.g., YAML content for topics).")]
        public string Data { get; set; }

        /// <summary>
        /// Gets or sets the component content.
        /// </summary>
        [Parameter(HelpMessage = "Component content.")]
        public string Content { get; set; }

        /// <summary>
        /// Gets or sets the description.
        /// </summary>
        [Parameter(HelpMessage = "Component description.")]
        public string Description { get; set; }

        /// <summary>
        /// Gets or sets the category.
        /// </summary>
        [Parameter(HelpMessage = "Component category.")]
        public string Category { get; set; }

        /// <summary>
        /// Gets or sets the language code.
        /// </summary>
        [Parameter(HelpMessage = "Language code (e.g., 1033 for English US).")]
        public int? Language { get; set; }

        /// <summary>
        /// Gets or sets the help link.
        /// </summary>
        [Parameter(HelpMessage = "Help link URL.")]
        public string HelpLink { get; set; }

        /// <summary>
        /// If specified, returns the component after creation/update.
        /// </summary>
        [Parameter(HelpMessage = "If specified, returns the component after creation/update.")]
        public SwitchParameter PassThru { get; set; }

        /// <summary>
        /// Processes the cmdlet.
        /// </summary>
        protected override void ProcessRecord()
        {
            base.ProcessRecord();

            bool isUpdate = BotComponentId.HasValue && BotComponentId.Value != Guid.Empty;
            string operation = isUpdate ? "update" : "create";
            string target = isUpdate ? $"bot component '{Name}' (ID: {BotComponentId})" : $"new bot component '{Name}'";

            if (!ShouldProcess(target, operation))
            {
                return;
            }

            Entity component;

            if (isUpdate)
            {
                // Update existing component
                component = new Entity("botcomponent", BotComponentId.Value);
            }
            else
            {
                // Create new component
                if (string.IsNullOrEmpty(SchemaName))
                {
                    ThrowTerminatingError(new ErrorRecord(
                        new ArgumentException("SchemaName is required when creating a new bot component."),
                        "MissingSchemaName",
                        ErrorCategory.InvalidArgument,
                        null));
                    return;
                }

                if (!ParentBotId.HasValue)
                {
                    ThrowTerminatingError(new ErrorRecord(
                        new ArgumentException("ParentBotId is required when creating a new bot component."),
                        "MissingParentBotId",
                        ErrorCategory.InvalidArgument,
                        null));
                    return;
                }

                if (!ComponentType.HasValue)
                {
                    ThrowTerminatingError(new ErrorRecord(
                        new ArgumentException("ComponentType is required when creating a new bot component."),
                        "MissingComponentType",
                        ErrorCategory.InvalidArgument,
                        null));
                    return;
                }

                component = new Entity("botcomponent");
                component["schemaname"] = SchemaName;
                component["parentbotid"] = new EntityReference("bot", ParentBotId.Value);
                component["componenttype"] = new OptionSetValue(ComponentType.Value);
            }

            Guid componentId = isUpdate ? BotComponentId.Value : Guid.Empty;

            // Set common attributes
            component["name"] = Name;

            if (!string.IsNullOrEmpty(Data))
            {
                component["data"] = Data;
            }

            if (!string.IsNullOrEmpty(Content))
            {
                component["content"] = Content;
            }

            if (!string.IsNullOrEmpty(Description))
            {
                component["description"] = Description;
            }

            if (!string.IsNullOrEmpty(Category))
            {
                component["category"] = Category;
            }

            if (Language.HasValue)
            {
                component["language"] = Language.Value;
            }

            if (!string.IsNullOrEmpty(HelpLink))
            {
                component["helplink"] = HelpLink;
            }

            if (isUpdate)
            {
                Connection.Update(component);
                WriteVerbose($"Updated bot component '{Name}' (ID: {componentId})");
            }
            else
            {
                componentId = Connection.Create(component);
                WriteVerbose($"Created bot component '{Name}' with ID: {componentId}");
            }

            if (PassThru)
            {
                // Retrieve and return the component
                Entity retrievedComponent = Connection.Retrieve("botcomponent", componentId, new ColumnSet(true));
                var entityMetadataFactory = new EntityMetadataFactory(Connection);
                var converter = new DataverseEntityConverter(Connection, entityMetadataFactory);
                var psObject = converter.ConvertToPSObject(retrievedComponent, new ColumnSet(true), _ => ValueType.Raw);
                WriteObject(psObject);
            }
        }
    }
}
