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
    /// Sets the App properties in a .msapp file (App.pa.yaml).
    /// </summary>
    [Cmdlet(VerbsCommon.Set, "DataverseMsAppProperties", SupportsShouldProcess = true, ConfirmImpact = ConfirmImpact.Medium)]
    public class SetDataverseMsAppPropertiesCmdlet : PSCmdlet
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
        /// Gets or sets the YAML content for the app properties (without "App:" header - will be added automatically).
        /// </summary>
        [Parameter(Mandatory = true, Position = 1, ParameterSetName = "YamlContent-FromPath", HelpMessage = "YAML content for the app properties (without 'App:' header)")]
        [Parameter(Mandatory = true, Position = 1, ParameterSetName = "YamlContent-FromObject", HelpMessage = "YAML content for the app properties (without 'App:' header)")]
        [ValidateNotNullOrEmpty]
        public string YamlContent { get; set; }

        /// <summary>
        /// Gets or sets the path to a YAML file containing the app properties definition (without "App:" header).
        /// </summary>
        [Parameter(Mandatory = true, ParameterSetName = "YamlFile-FromPath", HelpMessage = "Path to a YAML file containing the app properties definition (without 'App:' header)")]
        [Parameter(Mandatory = true, ParameterSetName = "YamlFile-FromObject", HelpMessage = "Path to a YAML file containing the app properties definition (without 'App:' header)")]
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
                ? "Set app properties in Canvas app" 
                : $"Set app properties in .msapp file '{targetPath}'";
            
            if (!ShouldProcess(action, action, "Set App Properties"))
            {
                return;
            }

            // Add header and indent the YAML content
            string fullYaml = AddAppHeader(yaml);

            byte[] modifiedBytes = ModifyMsAppProperties(msappBytes, fullYaml);
            
            if (isFromObject)
            {
                // Update the document property in the PSObject
                string base64 = Convert.ToBase64String(modifiedBytes);
                CanvasApp.Properties["document"].Value = base64;
                WriteVerbose("App properties set successfully in Canvas app object");
            }
            else
            {
                File.WriteAllBytes(targetPath, modifiedBytes);
                WriteVerbose("App properties set successfully in .msapp file");
            }
        }

        private byte[] ModifyMsAppProperties(byte[] originalBytes, string yaml)
        {
            using (var memoryStream = new MemoryStream(originalBytes))
            using (var resultStream = new MemoryStream())
            {
                using (var zipInputStream = new ZipInputStream(memoryStream))
                using (var zipOutputStream = new ZipOutputStream(resultStream))
                {
                    zipOutputStream.SetLevel(6);

                    ZipEntry entry;
                    bool appPropertiesFound = false;
                    var screenFiles = new Dictionary<string, string>(); // screenName -> yamlContent

                    // First pass: copy all files except Controls/*.json and collect screen YAML files
                    while ((entry = zipInputStream.GetNextEntry()) != null)
                    {
                        // Normalize path separators to forward slashes
                        string normalizedName = entry.Name.Replace('\\', '/');
                        
                        if (normalizedName == "Src/App.pa.yaml")
                        {
                            appPropertiesFound = true;
                            WriteVerbose("Updating App.pa.yaml");
                            ReadZipEntryBytes(zipInputStream); // Read to advance stream
                            AddZipEntry(zipOutputStream, entry.Name, yaml);
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

                            // Collect screen YAML files for Controls generation
                            if (normalizedName.StartsWith("Src/") && 
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

                    if (!appPropertiesFound)
                    {
                        WriteWarning("App.pa.yaml not found in .msapp file - creating it");
                        AddZipEntry(zipOutputStream, "Src/App.pa.yaml", yaml);
                    }

                    // Generate Controls JSON files
                    WriteVerbose("Generating Controls JSON files");
                    RegenerateControlsFiles(zipOutputStream, yaml, screenFiles);

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
        /// Adds the "App:" header using SharpYaml to properly serialize the YAML structure.
        /// Transforms the provided content into: "App:\n  ..."
        /// </summary>
        private string AddAppHeader(string yamlContent)
        {
            try
            {
                // Parse the app properties content YAML
                var serializer = new Serializer();
                var appContent = serializer.Deserialize(yamlContent);

                // Create the full structure: App -> content
                var fullStructure = new Dictionary<object, object>
                {
                    ["App"] = appContent
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
