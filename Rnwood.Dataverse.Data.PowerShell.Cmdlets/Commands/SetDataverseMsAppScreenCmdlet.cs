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

                using (var zipInputStream = new ZipInputStream(memoryStream))
                using (var zipOutputStream = new ZipOutputStream(resultStream))
                {
                    zipOutputStream.SetLevel(6);

                    ZipEntry entry;
                    while ((entry = zipInputStream.GetNextEntry()) != null)
                    {
                        if (entry.Name == screenFileName)
                        {
                            screenExists = true;
                            WriteVerbose($"Updating existing screen '{ScreenName}'");
                            AddZipEntry(zipOutputStream, entry.Name, yaml);
                        }
                        else
                        {
                            // Copy entry as-is
                            byte[] entryBytes = ReadZipEntryBytes(zipInputStream);
                            AddZipEntry(zipOutputStream, entry.Name, entryBytes);
                        }
                    }

                    // Add new screen if it doesn't exist
                    if (!screenExists)
                    {
                        WriteVerbose($"Creating new screen '{ScreenName}'");
                        AddZipEntry(zipOutputStream, screenFileName, yaml);
                    }

                    zipOutputStream.Finish();
                }

                return resultStream.ToArray();
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
