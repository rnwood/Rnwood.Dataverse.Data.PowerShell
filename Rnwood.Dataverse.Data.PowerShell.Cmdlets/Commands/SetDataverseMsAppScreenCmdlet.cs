using ICSharpCode.SharpZipLib.Zip;
using SharpYaml.Serialization;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Management.Automation;
using System.Text;

namespace Rnwood.Dataverse.Data.PowerShell.Commands
{
    /// <summary>
    /// Adds or updates a screen in a .msapp file.
    /// </summary>
    [Cmdlet(VerbsCommon.Set, "DataverseMsAppScreen", SupportsShouldProcess = true, ConfirmImpact = ConfirmImpact.Medium)]
    public class SetDataverseMsAppScreenCmdlet : PSCmdlet
    {
        /// <summary>
        /// Gets or sets the path to the .msapp file.
        /// </summary>
        [Parameter(Mandatory = true, Position = 0, ParameterSetName = "YamlContent-FromPath", HelpMessage = "Path to the .msapp file")]
        [Parameter(Mandatory = true, Position = 0, ParameterSetName = "YamlFile-FromPath", HelpMessage = "Path to the .msapp file")]
        [ValidateNotNullOrEmpty]
        public string MsAppPath { get; set; }

        /// <summary>
        /// Gets or sets the Canvas app PSObject returned by Get-DataverseCanvasApp with -IncludeDocument.
        /// The document property will be updated with the modified .msapp.
        /// </summary>
        [Parameter(Mandatory = true, Position = 0, ParameterSetName = "YamlContent-FromObject", ValueFromPipeline = true, HelpMessage = "Canvas app PSObject from Get-DataverseCanvasApp -IncludeDocument")]
        [Parameter(Mandatory = true, Position = 0, ParameterSetName = "YamlFile-FromObject", ValueFromPipeline = true, HelpMessage = "Canvas app PSObject from Get-DataverseCanvasApp -IncludeDocument")]
        [ValidateNotNull]
        public PSObject CanvasApp { get; set; }

        /// <summary>
        /// Gets or sets the screen name.
        /// </summary>
        [Parameter(Mandatory = true, Position = 1, HelpMessage = "Name of the screen")]
        [ValidateNotNullOrEmpty]
        public string ScreenName { get; set; }

        /// <summary>
        /// Gets or sets the YAML content for the screen (without header - will be added automatically).
        /// </summary>
        [Parameter(Mandatory = true, Position = 2, ParameterSetName = "YamlContent-FromPath", HelpMessage = "YAML content for the screen (without header)")]
        [Parameter(Mandatory = true, Position = 2, ParameterSetName = "YamlContent-FromObject", HelpMessage = "YAML content for the screen (without header)")]
        [ValidateNotNullOrEmpty]
        public string YamlContent { get; set; }

        /// <summary>
        /// Gets or sets the path to a YAML file containing the screen definition (without header).
        /// </summary>
        [Parameter(Mandatory = true, ParameterSetName = "YamlFile-FromPath", HelpMessage = "Path to a YAML file containing the screen definition (without header)")]
        [Parameter(Mandatory = true, ParameterSetName = "YamlFile-FromObject", HelpMessage = "Path to a YAML file containing the screen definition (without header)")]
        [ValidateNotNullOrEmpty]
        public string YamlFilePath { get; set; }

        /// <inheritdoc />
        protected override void ProcessRecord()
        {
            base.ProcessRecord();

            bool isFromObject = ParameterSetName.Contains("FromObject");
            byte[] msappBytes;
            string targetPath = null;

            if (isFromObject)
            {
                // Get .msapp from the PSObject's document property
                var documentProp = CanvasApp.Properties["document"];
                if (documentProp == null || documentProp.Value == null)
                {
                    ThrowTerminatingError(new ErrorRecord(
                        new InvalidOperationException("CanvasApp object does not have a 'document' property. Use Get-DataverseCanvasApp with -IncludeDocument."),
                        "DocumentPropertyMissing",
                        ErrorCategory.InvalidArgument,
                        CanvasApp));
                    return;
                }

                string base64Document = documentProp.Value.ToString();
                msappBytes = Convert.FromBase64String(base64Document);
            }
            else
            {
                targetPath = GetUnresolvedProviderPathFromPSPath(MsAppPath);
                if (!File.Exists(targetPath))
                {
                    ThrowTerminatingError(new ErrorRecord(
                        new FileNotFoundException($"MsApp file not found: {targetPath}"),
                        "FileNotFound",
                        ErrorCategory.ObjectNotFound,
                        targetPath));
                    return;
                }

                msappBytes = File.ReadAllBytes(targetPath);
            }

            // Get YAML content
            string yaml;
            if (!string.IsNullOrEmpty(YamlFilePath))
            {
                var yamlPath = GetUnresolvedProviderPathFromPSPath(YamlFilePath);
                if (!File.Exists(yamlPath))
                {
                    ThrowTerminatingError(new ErrorRecord(
                        new FileNotFoundException($"YAML file not found: {yamlPath}"),
                        "FileNotFound",
                        ErrorCategory.ObjectNotFound,
                        yamlPath));
                    return;
                }
                yaml = File.ReadAllText(yamlPath);
            }
            else
            {
                yaml = YamlContent;
            }

            string action = isFromObject 
                ? $"Set screen '{ScreenName}' in Canvas app" 
                : $"Set screen '{ScreenName}' in .msapp file '{targetPath}'";
            
            if (!ShouldProcess(action, action, "Set Screen"))
            {
                return;
            }

            // Add header and indent the YAML content
            string fullYaml = AddScreenHeader(yaml, ScreenName);

            byte[] modifiedBytes = ModifyMsAppScreen(msappBytes, fullYaml);
            
            if (isFromObject)
            {
                // Update the document property in the PSObject
                string base64 = Convert.ToBase64String(modifiedBytes);
                CanvasApp.Properties["document"].Value = base64;
                WriteVerbose($"Screen '{ScreenName}' set successfully in Canvas app object");
            }
            else
            {
                File.WriteAllBytes(targetPath, modifiedBytes);
                WriteVerbose($"Screen '{ScreenName}' set successfully in .msapp file");
            }
        }

        private byte[] ModifyMsAppScreen(byte[] originalBytes, string yaml)
        {
            using (var memoryStream = new MemoryStream(originalBytes))
            using (var resultStream = new MemoryStream())
            {
                bool screenExists = false;
                string screenFileName = $"Src/{ScreenName}.pa.yaml";
                var screenFiles = new Dictionary<string, string>(); // screenName -> yamlContent
                string appYaml = null;

                using (var zipInputStream = new ZipInputStream(memoryStream))
                using (var zipOutputStream = new ZipOutputStream(resultStream))
                {
                    zipOutputStream.SetLevel(6);

                    ZipEntry entry;
                    while ((entry = zipInputStream.GetNextEntry()) != null)
                    {
                        // Normalize path separators to forward slashes
                        string normalizedName = entry.Name.Replace('\\', '/');
                        string normalizedScreenFileName = screenFileName.Replace('\\', '/');
                        
                        if (normalizedName == normalizedScreenFileName)
                        {
                            screenExists = true;
                            WriteVerbose($"Updating existing screen '{ScreenName}'");
                            ReadZipEntryBytes(zipInputStream); // Read to advance stream
                            AddZipEntry(zipOutputStream, entry.Name, yaml);
                            screenFiles[ScreenName] = yaml;
                        }
                        else if (normalizedName.StartsWith("Controls/") && normalizedName.EndsWith(".json"))
                        {
                            // Skip existing Controls JSON files - they will be regenerated
                            WriteVerbose($"Skipping existing control file: {entry.Name}");
                            ReadZipEntryBytes(zipInputStream); // Read to advance stream
                        }
                        else
                        {
                            // Copy entry as-is
                            byte[] entryBytes = ReadZipEntryBytes(zipInputStream);
                            AddZipEntry(zipOutputStream, entry.Name, entryBytes);

                            // Collect App YAML for Controls generation
                            if (normalizedName == "Src/App.pa.yaml")
                            {
                                appYaml = Encoding.UTF8.GetString(entryBytes);
                            }
                            // Collect all screen YAML files for Controls generation
                            else if (normalizedName.StartsWith("Src/") && 
                                normalizedName.EndsWith(".pa.yaml") && 
                                !normalizedName.EndsWith("/App.pa.yaml") &&
                                !normalizedName.EndsWith("/_EditorState.pa.yaml"))
                            {
                                string screenName = Path.GetFileNameWithoutExtension(Path.GetFileNameWithoutExtension(entry.Name));
                                string yamlContent = Encoding.UTF8.GetString(entryBytes);
                                screenFiles[screenName] = yamlContent;
                            }
                        }
                    }

                    // Add new screen if it doesn't exist
                    if (!screenExists)
                    {
                        WriteVerbose($"Creating new screen '{ScreenName}'");
                        AddZipEntry(zipOutputStream, screenFileName, yaml);
                        screenFiles[ScreenName] = yaml;
                    }

                    // Generate Controls JSON files
                    WriteVerbose("Generating Controls JSON files");
                    if (appYaml != null)
                    {
                        RegenerateControlsFiles(zipOutputStream, appYaml, screenFiles);
                    }
                    else
                    {
                        WriteWarning("App.pa.yaml not found - cannot generate Controls JSON files");
                    }

                    zipOutputStream.Finish();
                }

                return resultStream.ToArray();
            }
        }

        private void RegenerateControlsFiles(ZipOutputStream zipOutputStream, string appYaml, Dictionary<string, string> screenFiles)
        {
            // Generate Controls/1.json for App
            WriteVerbose("Generating Controls/1.json for App");
            string appControlJson = MsAppControlsGenerator.GenerateAppControlJson(appYaml);
            AddZipEntry(zipOutputStream, "Controls/1.json", appControlJson);

            // Generate Controls/*.json for screens (sorted alphabetically by screen name)
            // ControlUniqueId starts at 4 for first screen (1 is App, 3 is Host)
            int controlUniqueId = 4;
            var sortedScreens = screenFiles.OrderBy(kvp => kvp.Key).ToList();
            
            foreach (var screen in sortedScreens)
            {
                string screenName = screen.Key;
                string yamlContent = screen.Value;
                
                WriteVerbose($"Generating Controls/{controlUniqueId}.json for screen '{screenName}'");
                string screenControlJson = MsAppControlsGenerator.GenerateScreenControlJson(screenName, yamlContent, controlUniqueId);
                AddZipEntry(zipOutputStream, $"Controls/{controlUniqueId}.json", screenControlJson);
                
                controlUniqueId++;
            }
        }

        private void AddZipEntry(ZipOutputStream zipStream, string entryName, byte[] content)
        {
            var entry = new ZipEntry(entryName)
            {
                DateTime = DateTime.Now,
                Size = content.Length
            };

            zipStream.PutNextEntry(entry);
            zipStream.Write(content, 0, content.Length);
            zipStream.CloseEntry();
        }

        private void AddZipEntry(ZipOutputStream zipStream, string entryName, string content)
        {
            AddZipEntry(zipStream, entryName, Encoding.UTF8.GetBytes(content));
        }

        private byte[] ReadZipEntryBytes(ZipInputStream zipInputStream)
        {
            using (var entryStream = new MemoryStream())
            {
                byte[] buffer = new byte[4096];
                int count;
                while ((count = zipInputStream.Read(buffer, 0, buffer.Length)) > 0)
                {
                    entryStream.Write(buffer, 0, count);
                }
                return entryStream.ToArray();
            }
        }

        /// <summary>
        /// Adds the screen header using SharpYaml to properly serialize the YAML structure.
        /// Transforms the provided content into: "Screens:\n  ScreenName:\n    ..."
        /// </summary>
        private string AddScreenHeader(string yamlContent, string screenName)
        {
            try
            {
                // Parse the screen content YAML
                var serializer = new Serializer();
                var screenContent = serializer.Deserialize(yamlContent);

                // Create the full structure: Screens -> ScreenName -> content
                var fullStructure = new Dictionary<object, object>
                {
                    ["Screens"] = new Dictionary<object, object>
                    {
                        [screenName] = screenContent
                    }
                };

                // Serialize with proper indentation
                var result = serializer.Serialize(fullStructure);
                return result;
            }
            catch (Exception ex)
            {
                ThrowTerminatingError(new ErrorRecord(
                    new InvalidOperationException($"Failed to parse YAML content: {ex.Message}", ex),
                    "YamlParseError",
                    ErrorCategory.InvalidData,
                    yamlContent));
                return null; // Never reached due to ThrowTerminatingError
            }
        }
    }
}
