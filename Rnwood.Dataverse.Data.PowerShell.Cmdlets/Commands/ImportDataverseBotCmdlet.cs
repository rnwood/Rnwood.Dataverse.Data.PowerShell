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
    /// Imports a Copilot Studio bot and its components from a backup directory.
    /// </summary>
    [Cmdlet(VerbsData.Import, "DataverseBot", SupportsShouldProcess = true, ConfirmImpact = ConfirmImpact.Medium)]
    [OutputType(typeof(PSObject))]
    public class ImportDataverseBotCmdlet : OrganizationServiceCmdlet
    {
        /// <summary>
        /// Gets or sets the path to the backup directory.
        /// </summary>
        [Parameter(Mandatory = true, Position = 0, HelpMessage = "Path to the backup directory created by Export-DataverseBot.")]
        [ValidateNotNullOrEmpty]
        public string Path { get; set; }

        /// <summary>
        /// Gets or sets the new name for the bot. If not specified, uses the name from the backup.
        /// </summary>
        [Parameter(HelpMessage = "New name for the bot. If not specified, uses the name from the backup.")]
        public string Name { get; set; }

        /// <summary>
        /// Gets or sets the new schema name for the bot. If not specified, uses the schema name from the backup (or generates one if creating new bot).
        /// </summary>
        [Parameter(HelpMessage = "New schema name for the bot. If not specified, uses the schema name from the backup.")]
        public string SchemaName { get; set; }

        /// <summary>
        /// Gets or sets the existing bot ID to restore components to. If not specified, creates a new bot.
        /// </summary>
        [Parameter(HelpMessage = "Existing bot ID to restore components to. If not specified, creates a new bot.")]
        public Guid? TargetBotId { get; set; }

        /// <summary>
        /// If specified, overwrites existing components with matching schema names.
        /// </summary>
        [Parameter(HelpMessage = "If specified, overwrites existing components with matching schema names.")]
        public SwitchParameter Overwrite { get; set; }

        /// <summary>
        /// If specified, returns information about the import.
        /// </summary>
        [Parameter(HelpMessage = "If specified, returns information about the import.")]
        public SwitchParameter PassThru { get; set; }

        /// <summary>
        /// Processes the cmdlet.
        /// </summary>
        protected override void ProcessRecord()
        {
            base.ProcessRecord();

            // Validate backup directory
            string fullPath = System.IO.Path.GetFullPath(Path);
            if (!Directory.Exists(fullPath))
            {
                ThrowTerminatingError(new ErrorRecord(
                    new DirectoryNotFoundException($"Backup directory not found: {fullPath}"),
                    "BackupDirectoryNotFound",
                    ErrorCategory.ObjectNotFound,
                    fullPath));
                return;
            }

            string manifestPath = System.IO.Path.Combine(fullPath, "manifest.json");
            if (!File.Exists(manifestPath))
            {
                ThrowTerminatingError(new ErrorRecord(
                    new FileNotFoundException($"Manifest file not found: {manifestPath}"),
                    "ManifestNotFound",
                    ErrorCategory.ObjectNotFound,
                    manifestPath));
                return;
            }

            // Read manifest
            string manifestJson = File.ReadAllText(manifestPath);
            var manifest = JsonSerializer.Deserialize<Dictionary<string, JsonElement>>(manifestJson);

            var botInfo = manifest["bot"].Deserialize<Dictionary<string, JsonElement>>();
            string backupBotName = botInfo["name"].GetString();
            string backupSchemaName = botInfo["schemaname"].GetString();
            int backupLanguage = botInfo.ContainsKey("language") ? botInfo["language"].GetInt32() : 1033;

            string finalBotName = Name ?? backupBotName;
            string finalSchemaName = SchemaName ?? backupSchemaName;

            if (!ShouldProcess($"Import bot '{finalBotName}' from {fullPath}", "Import"))
            {
                return;
            }

            try
            {
                Guid botId;
                bool botCreated = false;

                // Determine or create target bot
                if (TargetBotId.HasValue)
                {
                    botId = TargetBotId.Value;
                    WriteVerbose($"Using existing bot ID: {botId}");

                    // Verify bot exists
                    try
                    {
                        Entity existingBot = Connection.Retrieve("bot", botId, new ColumnSet("name"));
                        WriteVerbose($"Target bot found: {existingBot.GetAttributeValue<string>("name")}");
                    }
                    catch
                    {
                        ThrowTerminatingError(new ErrorRecord(
                            new InvalidOperationException($"Target bot with ID {botId} not found"),
                            "TargetBotNotFound",
                            ErrorCategory.ObjectNotFound,
                            botId));
                        return;
                    }
                }
                else
                {
                    // Create new bot
                    botId = ImportBotConfiguration(fullPath, finalBotName, finalSchemaName, backupLanguage);
                    botCreated = true;
                    WriteVerbose($"Created new bot with ID: {botId}");
                }

                // Import components
                int importedCount = ImportBotComponents(fullPath, botId, finalSchemaName);

                WriteVerbose($"Successfully imported bot '{finalBotName}' with {importedCount} component(s)");

                if (PassThru)
                {
                    var importInfo = new PSObject();
                    importInfo.Properties.Add(new PSNoteProperty("BotId", botId));
                    importInfo.Properties.Add(new PSNoteProperty("BotName", finalBotName));
                    importInfo.Properties.Add(new PSNoteProperty("BotSchemaName", finalSchemaName));
                    importInfo.Properties.Add(new PSNoteProperty("ComponentsImported", importedCount));
                    importInfo.Properties.Add(new PSNoteProperty("BotCreated", botCreated));
                    importInfo.Properties.Add(new PSNoteProperty("SourcePath", fullPath));
                    importInfo.Properties.Add(new PSNoteProperty("ImportDate", DateTime.Now));
                    WriteObject(importInfo);
                }
            }
            catch (Exception ex)
            {
                ThrowTerminatingError(new ErrorRecord(
                    new InvalidOperationException($"Failed to import bot: {ex.Message}", ex),
                    "ImportFailed",
                    ErrorCategory.WriteError,
                    fullPath));
            }
        }

        private Guid ImportBotConfiguration(string backupPath, string botName, string schemaName, int language)
        {
            string configPath = System.IO.Path.Combine(backupPath, "bot_config.json");

            Entity bot = new Entity("bot");
            bot["name"] = botName;
            bot["schemaname"] = schemaName;
            bot["language"] = language;

            if (File.Exists(configPath))
            {
                string configJson = File.ReadAllText(configPath);
                var config = JsonSerializer.Deserialize<Dictionary<string, JsonElement>>(configJson);

                // Import configuration attributes (excluding system fields)
                string[] excludeAttrs = new[] { "botid", "componentidunique", "componentstate", "overwritetime", "solutionid", "publishedon", "publishedby", "synchronizationstatus" };

                foreach (var kvp in config)
                {
                    if (excludeAttrs.Contains(kvp.Key.ToLower()))
                        continue;

                    if (kvp.Key == "name" || kvp.Key == "schemaname" || kvp.Key == "language")
                        continue; // Already set above

                    try
                    {
                        object value = ConvertJsonElementToValue(kvp.Value);
                        if (value != null)
                        {
                            bot[kvp.Key] = value;
                        }
                    }
                    catch (Exception ex)
                    {
                        WriteWarning($"Could not set attribute {kvp.Key}: {ex.Message}");
                    }
                }
            }

            Guid botId = Connection.Create(bot);
            WriteVerbose($"Created bot: {botName}");

            return botId;
        }

        private int ImportBotComponents(string backupPath, Guid botId, string botSchemaName)
        {
            var yamlFiles = Directory.GetFiles(backupPath, "*.yaml");
            int importedCount = 0;

            foreach (var yamlFile in yamlFiles)
            {
                string baseName = System.IO.Path.GetFileNameWithoutExtension(yamlFile);
                string metaFile = System.IO.Path.Combine(backupPath, $"{baseName}.meta.json");

                if (!File.Exists(metaFile))
                {
                    WriteWarning($"No metadata file for {yamlFile}, skipping");
                    continue;
                }

                try
                {
                    // Read metadata
                    string metaJson = File.ReadAllText(metaFile);
                    var metadata = JsonSerializer.Deserialize<Dictionary<string, JsonElement>>(metaJson);

                    string componentName = metadata["name"].GetString();
                    string originalSchemaName = metadata["schemaname"].GetString();
                    int componentType = metadata["componenttype"].GetInt32();
                    string description = metadata.ContainsKey("description") ? metadata["description"].GetString() : "";
                    int language = metadata.ContainsKey("language") ? metadata["language"].GetInt32() : 1033;

                    // Generate new schema name if needed
                    string newSchemaName = originalSchemaName;
                    if (!string.IsNullOrEmpty(botSchemaName) && originalSchemaName.Contains("."))
                    {
                        var parts = originalSchemaName.Split('.');
                        if (parts.Length > 1)
                        {
                            newSchemaName = botSchemaName + "." + string.Join(".", parts.Skip(1));
                        }
                    }

                    // Read component data
                    string data = File.ReadAllText(yamlFile);

                    // Check if component exists
                    Guid? existingComponentId = null;
                    if (Overwrite)
                    {
                        var query = new QueryExpression("botcomponent")
                        {
                            ColumnSet = new ColumnSet("botcomponentid"),
                            TopCount = 1
                        };
                        query.Criteria.AddCondition("schemaname", ConditionOperator.Equal, newSchemaName);
                        query.Criteria.AddCondition("parentbotid", ConditionOperator.Equal, botId);

                        var results = Connection.RetrieveMultiple(query);
                        if (results.Entities.Count > 0)
                        {
                            existingComponentId = results.Entities[0].Id;
                        }
                    }

                    Entity component;
                    if (existingComponentId.HasValue)
                    {
                        // Update existing component
                        component = new Entity("botcomponent", existingComponentId.Value);
                        component["name"] = componentName;
                        component["data"] = data;
                        component["description"] = description;

                        Connection.Update(component);
                        WriteVerbose($"Updated component: {componentName}");
                    }
                    else
                    {
                        // Create new component
                        component = new Entity("botcomponent");
                        component["name"] = componentName;
                        component["schemaname"] = newSchemaName;
                        component["componenttype"] = new OptionSetValue(componentType);
                        component["data"] = data;
                        component["description"] = description;
                        component["language"] = language;
                        component["parentbotid"] = new EntityReference("bot", botId);

                        Connection.Create(component);
                        WriteVerbose($"Created component: {componentName}");
                    }

                    importedCount++;
                }
                catch (Exception ex)
                {
                    WriteWarning($"Failed to import component from {yamlFile}: {ex.Message}");
                }
            }

            return importedCount;
        }

        private object ConvertJsonElementToValue(JsonElement element)
        {
            switch (element.ValueKind)
            {
                case JsonValueKind.String:
                    return element.GetString();
                case JsonValueKind.Number:
                    if (element.TryGetInt32(out int intVal))
                        return intVal;
                    if (element.TryGetInt64(out long longVal))
                        return longVal;
                    if (element.TryGetDouble(out double doubleVal))
                        return doubleVal;
                    return element.GetDecimal();
                case JsonValueKind.True:
                    return true;
                case JsonValueKind.False:
                    return false;
                case JsonValueKind.Object:
                    // Handle special object types (e.g., EntityReference)
                    var dict = JsonSerializer.Deserialize<Dictionary<string, JsonElement>>(element.GetRawText());
                    if (dict.ContainsKey("LogicalName") && dict.ContainsKey("Id"))
                    {
                        return new EntityReference(
                            dict["LogicalName"].GetString(),
                            Guid.Parse(dict["Id"].GetString())
                        );
                    }
                    return null;
                default:
                    return null;
            }
        }
    }
}
