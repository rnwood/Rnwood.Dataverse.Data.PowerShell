using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Query;
using System;
using System.Management.Automation;

namespace Rnwood.Dataverse.Data.PowerShell.Commands
{
    /// <summary>
    /// Creates or updates a Copilot Studio bot in Dataverse.
    /// </summary>
    [Cmdlet(VerbsCommon.Set, "DataverseBot", SupportsShouldProcess = true, ConfirmImpact = ConfirmImpact.Medium)]
    [OutputType(typeof(PSObject))]
    public class SetDataverseBotCmdlet : OrganizationServiceCmdlet
    {
        /// <summary>
        /// Gets or sets the bot ID. If provided, updates the existing bot. If not provided, creates a new bot.
        /// </summary>
        [Parameter(ValueFromPipelineByPropertyName = true, HelpMessage = "Bot ID (GUID). If provided, updates the existing bot. If not provided, creates a new bot.")]
        public Guid? BotId { get; set; }

        /// <summary>
        /// Gets or sets the bot name.
        /// </summary>
        [Parameter(Mandatory = true, HelpMessage = "Bot name.")]
        public string Name { get; set; }

        /// <summary>
        /// Gets or sets the bot schema name. Required for new bots.
        /// </summary>
        [Parameter(HelpMessage = "Bot schema name. Required when creating a new bot.")]
        public string SchemaName { get; set; }

        /// <summary>
        /// Gets or sets the language code. Default is 1033 (English US).
        /// </summary>
        [Parameter(HelpMessage = "Language code. Default is 1033 (English US).")]
        public int Language { get; set; } = 1033;

        /// <summary>
        /// Gets or sets the bot configuration JSON.
        /// </summary>
        [Parameter(HelpMessage = "Bot configuration JSON string.")]
        public string Configuration { get; set; }

        /// <summary>
        /// Gets or sets the authentication mode.
        /// </summary>
        [Parameter(HelpMessage = "Authentication mode (0=None, 1=Generic, 2=Integrated).")]
        public int? AuthenticationMode { get; set; }

        /// <summary>
        /// Gets or sets the runtime provider.
        /// </summary>
        [Parameter(HelpMessage = "Runtime provider (0=PowerVirtualAgents).")]
        public int? RuntimeProvider { get; set; }

        /// <summary>
        /// Gets or sets the template.
        /// </summary>
        [Parameter(HelpMessage = "Template name.")]
        public string Template { get; set; }

        /// <summary>
        /// If specified, returns the bot after creation/update.
        /// </summary>
        [Parameter(HelpMessage = "If specified, returns the bot after creation/update.")]
        public SwitchParameter PassThru { get; set; }

        /// <summary>
        /// Processes the cmdlet.
        /// </summary>
        protected override void ProcessRecord()
        {
            base.ProcessRecord();

            bool isUpdate = BotId.HasValue && BotId.Value != Guid.Empty;
            string operation = isUpdate ? "update" : "create";
            string target = isUpdate ? $"bot '{Name}' (ID: {BotId})" : $"new bot '{Name}'";

            if (!ShouldProcess(target, operation))
            {
                return;
            }

            Entity bot;

            if (isUpdate)
            {
                // Update existing bot
                bot = new Entity("bot", BotId.Value);
            }
            else
            {
                // Create new bot
                if (string.IsNullOrEmpty(SchemaName))
                {
                    ThrowTerminatingError(new ErrorRecord(
                        new ArgumentException("SchemaName is required when creating a new bot."),
                        "MissingSchemaName",
                        ErrorCategory.InvalidArgument,
                        null));
                    return;
                }

                bot = new Entity("bot");
                bot["schemaname"] = SchemaName;
            }

            Guid botId = isUpdate ? BotId.Value : Guid.Empty;

            // Set common attributes
            bot["name"] = Name;
            bot["language"] = Language;

            if (!string.IsNullOrEmpty(Configuration))
            {
                bot["configuration"] = Configuration;
            }

            if (AuthenticationMode.HasValue)
            {
                bot["authenticationmode"] = new OptionSetValue(AuthenticationMode.Value);
            }

            if (RuntimeProvider.HasValue)
            {
                bot["runtimeprovider"] = new OptionSetValue(RuntimeProvider.Value);
            }

            if (!string.IsNullOrEmpty(Template))
            {
                bot["template"] = Template;
            }

            if (isUpdate)
            {
                Connection.Update(bot);
                WriteVerbose($"Updated bot '{Name}' (ID: {botId})");
            }
            else
            {
                botId = Connection.Create(bot);
                WriteVerbose($"Created bot '{Name}' with ID: {botId}");
            }

            if (PassThru)
            {
                // Retrieve and return the bot
                Entity retrievedBot = Connection.Retrieve("bot", botId, new ColumnSet(true));
                var entityMetadataFactory = new EntityMetadataFactory(Connection);
                var converter = new DataverseEntityConverter(Connection, entityMetadataFactory);
                var psObject = converter.ConvertToPSObject(retrievedBot, new ColumnSet(true), _ => ValueType.Raw);
                WriteObject(psObject);
            }
        }
    }
}
