using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Query;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Management.Automation;
using System.Text.Json;

namespace Rnwood.Dataverse.Data.PowerShell.Commands
{
    /// <summary>
    /// Exports a Copilot Studio bot and all its components to a backup directory.
    /// </summary>
    [Cmdlet(VerbsData.Export, "DataverseBot", SupportsShouldProcess = true)]
    [OutputType(typeof(PSObject))]
    public class ExportDataverseBotCmdlet : OrganizationServiceCmdlet
    {
        /// <summary>
        /// Gets or sets the bot ID to export.
        /// </summary>
        [Parameter(Mandatory = true, Position = 0, ValueFromPipelineByPropertyName = true, HelpMessage = "Bot ID (GUID) to export.")]
        public Guid BotId { get; set; }

        /// <summary>
        /// Gets or sets the output directory for the backup.
        /// </summary>
        [Parameter(Position = 1, HelpMessage = "Output directory for the backup. If not specified, creates a timestamped directory in the current location.")]
        public string OutputPath { get; set; }

        /// <summary>
        /// If specified, returns information about the export.
        /// </summary>
        [Parameter(HelpMessage = "If specified, returns information about the export.")]
        public SwitchParameter PassThru { get; set; }

        /// <summary>
        /// Processes the cmdlet.
        /// </summary>
        protected override void ProcessRecord()
        {
            base.ProcessRecord();

            // Retrieve the bot
            Entity bot;
            try
            {
                bot = Connection.Retrieve("bot", BotId, new ColumnSet(true));
            }
            catch (Exception ex)
            {
                ThrowTerminatingError(new ErrorRecord(
                    new InvalidOperationException($"Failed to retrieve bot with ID {BotId}: {ex.Message}", ex),
                    "BotNotFound",
                    ErrorCategory.ObjectNotFound,
                    BotId));
                return;
            }

            string botName = bot.GetAttributeValue<string>("name");
            string botSchemaName = bot.GetAttributeValue<string>("schemaname");

            // Create output directory
            if (string.IsNullOrEmpty(OutputPath))
            {
                string timestamp = DateTime.Now.ToString("yyyyMMdd_HHmmss");
                string safeName = MakeSafeFileName(botSchemaName ?? "bot");
                OutputPath = Path.Combine(Directory.GetCurrentDirectory(), $"{safeName}_backup_{timestamp}");
            }

            OutputPath = Path.GetFullPath(OutputPath);

            if (!ShouldProcess($"Export bot '{botName}' to {OutputPath}", "Export"))
            {
                return;
            }

            try
            {
                Directory.CreateDirectory(OutputPath);
                WriteVerbose($"Created backup directory: {OutputPath}");

                // Export bot configuration
                ExportBotConfiguration(bot, OutputPath);

                // Export bot components
                int componentCount = ExportBotComponents(BotId, OutputPath);

                // Create manifest
                CreateManifest(bot, componentCount, OutputPath);

                WriteVerbose($"Successfully exported bot '{botName}' with {componentCount} component(s) to {OutputPath}");

                if (PassThru)
                {
                    var exportInfo = new PSObject();
                    exportInfo.Properties.Add(new PSNoteProperty("BotId", BotId));
                    exportInfo.Properties.Add(new PSNoteProperty("BotName", botName));
                    exportInfo.Properties.Add(new PSNoteProperty("BotSchemaName", botSchemaName));
                    exportInfo.Properties.Add(new PSNoteProperty("ComponentCount", componentCount));
                    exportInfo.Properties.Add(new PSNoteProperty("OutputPath", OutputPath));
                    exportInfo.Properties.Add(new PSNoteProperty("ExportDate", DateTime.Now));
                    WriteObject(exportInfo);
                }
            }
            catch (Exception ex)
            {
                ThrowTerminatingError(new ErrorRecord(
                    new InvalidOperationException($"Failed to export bot: {ex.Message}", ex),
                    "ExportFailed",
                    ErrorCategory.WriteError,
                    BotId));
            }
        }

        private void ExportBotConfiguration(Entity bot, string outputPath)
        {
            var botConfig = new Dictionary<string, object>();

            // Export key bot attributes
            foreach (var attr in bot.Attributes)
            {
                if (attr.Key == "botid") continue; // Skip ID, will be assigned on import

                object value = attr.Value;

                // Convert special types to serializable formats
                if (value is EntityReference entityRef)
                {
                    botConfig[attr.Key] = new Dictionary<string, object>
                    {
                        { "LogicalName", entityRef.LogicalName },
                        { "Id", entityRef.Id },
                        { "Name", entityRef.Name }
                    };
                }
                else if (value is OptionSetValue optionSet)
                {
                    botConfig[attr.Key] = optionSet.Value;
                }
                else if (value is Money money)
                {
                    botConfig[attr.Key] = money.Value;
                }
                else if (value != null)
                {
                    botConfig[attr.Key] = value;
                }
            }

            string configPath = Path.Combine(outputPath, "bot_config.json");
            string json = JsonSerializer.Serialize(botConfig, new JsonSerializerOptions { WriteIndented = true });
            File.WriteAllText(configPath, json);

            WriteVerbose($"Exported bot configuration to: {configPath}");
        }

        private int ExportBotComponents(Guid botId, string outputPath)
        {
            // Query all components for this bot
            var query = new QueryExpression("botcomponent")
            {
                ColumnSet = new ColumnSet(true),
                Criteria = new FilterExpression()
            };
            query.Criteria.AddCondition("parentbotid", ConditionOperator.Equal, botId);

            EntityCollection components = Connection.RetrieveMultiple(query);

            WriteVerbose($"Found {components.Entities.Count} component(s) to export");

            int exportedCount = 0;

            foreach (var component in components.Entities)
            {
                try
                {
                    string componentName = component.GetAttributeValue<string>("name");
                    string schemaName = component.GetAttributeValue<string>("schemaname");
                    string data = component.GetAttributeValue<string>("data");

                    string safeName = MakeSafeFileName(componentName ?? schemaName ?? $"component_{component.Id}");

                    // Save component data (YAML format)
                    if (!string.IsNullOrEmpty(data))
                    {
                        string dataPath = Path.Combine(outputPath, $"{safeName}.yaml");
                        File.WriteAllText(dataPath, data);
                    }

                    // Save component metadata
                    var metadata = new Dictionary<string, object>
                    {
                        { "name", componentName },
                        { "schemaname", schemaName },
                        { "componenttype", component.Contains("componenttype") ? component.GetAttributeValue<OptionSetValue>("componenttype").Value : 0 },
                        { "description", component.GetAttributeValue<string>("description") ?? "" },
                        { "category", component.GetAttributeValue<string>("category") ?? "" },
                        { "language", component.Contains("language") ? (component.GetAttributeValue<object>("language") is OptionSetValue langOsv ? langOsv.Value : component.GetAttributeValue<int>("language")) : 1033 }
                    };

                    string metaPath = Path.Combine(outputPath, $"{safeName}.meta.json");
                    string metaJson = JsonSerializer.Serialize(metadata, new JsonSerializerOptions { WriteIndented = true });
                    File.WriteAllText(metaPath, metaJson);

                    exportedCount++;
                    WriteVerbose($"Exported component: {componentName}");
                }
                catch (Exception ex)
                {
                    WriteWarning($"Failed to export component {component.Id}: {ex.Message}");
                }
            }

            return exportedCount;
        }

        private void CreateManifest(Entity bot, int componentCount, string outputPath)
        {
            var manifest = new Dictionary<string, object>
            {
                { "version", "1.0" },
                { "exportDate", DateTime.Now.ToString("o") },
                { "bot", new Dictionary<string, object>
                    {
                        { "name", bot.GetAttributeValue<string>("name") },
                        { "schemaname", bot.GetAttributeValue<string>("schemaname") },
                        { "language", bot.Contains("language") ? (bot.GetAttributeValue<object>("language") is OptionSetValue langOsv ? langOsv.Value : bot.GetAttributeValue<int>("language")) : 1033 }
                    }
                },
                { "componentCount", componentCount }
            };

            string manifestPath = Path.Combine(outputPath, "manifest.json");
            string json = JsonSerializer.Serialize(manifest, new JsonSerializerOptions { WriteIndented = true });
            File.WriteAllText(manifestPath, json);

            WriteVerbose($"Created manifest: {manifestPath}");
        }

        private string MakeSafeFileName(string name)
        {
            char[] invalids = Path.GetInvalidFileNameChars();
            string safe = new string(name.Select(c => invalids.Contains(c) ? '_' : c).ToArray());
            return safe.Replace(" ", "_");
        }
    }
}
