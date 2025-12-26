using ICSharpCode.SharpZipLib.Zip;
using SharpYaml.Serialization;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Management.Automation;

namespace Rnwood.Dataverse.Data.PowerShell.Commands
{
    /// <summary>
    /// Retrieves components from a .msapp file.
    /// </summary>
    [Cmdlet(VerbsCommon.Get, "DataverseMsAppComponent")]
    [OutputType(typeof(PSObject))]
    public class GetDataverseMsAppComponentCmdlet : PSCmdlet
    {
        /// <summary>
        /// Gets or sets the path to the .msapp file.
        /// </summary>
        [Parameter(Mandatory = true, Position = 0, ParameterSetName = "FromPath", HelpMessage = "Path to the .msapp file")]
        [ValidateNotNullOrEmpty]
        public string MsAppPath { get; set; }

        /// <summary>
        /// Gets or sets the Canvas app PSObject returned by Get-DataverseCanvasApp with -IncludeDocument.
        /// </summary>
        [Parameter(Mandatory = true, Position = 0, ParameterSetName = "FromObject", ValueFromPipeline = true, HelpMessage = "Canvas app PSObject from Get-DataverseCanvasApp -IncludeDocument")]
        [ValidateNotNull]
        public PSObject CanvasApp { get; set; }

        /// <summary>
        /// Gets or sets the name pattern of components to retrieve. Supports wildcards (* and ?).
        /// </summary>
        [Parameter(HelpMessage = "Name pattern of components to retrieve. Supports wildcards (* and ?)")]
        public string ComponentName { get; set; }

        /// <inheritdoc />
        protected override void ProcessRecord()
        {
            base.ProcessRecord();

            byte[] msappBytes;

            if (ParameterSetName == "FromObject")
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
                var filePath = GetUnresolvedProviderPathFromPSPath(MsAppPath);
                if (!File.Exists(filePath))
                {
                    ThrowTerminatingError(new ErrorRecord(
                        new FileNotFoundException($"MsApp file not found: {filePath}"),
                        "FileNotFound",
                        ErrorCategory.ObjectNotFound,
                        filePath));
                    return;
                }

                msappBytes = File.ReadAllBytes(filePath);
            }

            ExtractComponents(msappBytes);
        }

        private void ExtractComponents(byte[] msappBytes)
        {
            using (var memoryStream = new MemoryStream(msappBytes))
            using (var zipInputStream = new ZipInputStream(memoryStream))
            {
                ZipEntry entry;
                while ((entry = zipInputStream.GetNextEntry()) != null)
                {
                    // Component YAML files can be in Components/ folder or Src/ folder
                    bool isComponent = false;
                    string componentName = null;

                    if (entry.Name.StartsWith("Components/") && entry.Name.EndsWith(".pa.yaml"))
                    {
                        // Components in Components/ folder
                        isComponent = true;
                        componentName = Path.GetFileNameWithoutExtension(entry.Name);

                        // Apply name filter
                        if (!string.IsNullOrEmpty(ComponentName) && !MatchesPattern(componentName, ComponentName))
                        {
                            continue;
                        }

                        // Read content
                        byte[] entryBytes = ReadZipEntryBytes(zipInputStream);
                        string yamlContent = System.Text.Encoding.UTF8.GetString(entryBytes);

                        // Strip the component header and unindent the content
                        string processedYaml = StripComponentHeader(yamlContent, componentName);

                        // Create PSObject
                        var psObject = new PSObject();
                        psObject.Properties.Add(new PSNoteProperty("ComponentName", componentName));
                        psObject.Properties.Add(new PSNoteProperty("FilePath", entry.Name));
                        psObject.Properties.Add(new PSNoteProperty("YamlContent", processedYaml));
                        psObject.Properties.Add(new PSNoteProperty("Size", entryBytes.Length));

                        WriteObject(psObject);
                    }
                    else if (entry.Name.StartsWith("Src/") && 
                             entry.Name.EndsWith(".pa.yaml") &&
                             !entry.Name.EndsWith("/App.pa.yaml") &&
                             !entry.Name.EndsWith("/_EditorState.pa.yaml") &&
                             !entry.Name.Contains("/Screen"))
                    {
                        // Check if it's a component in Src/ folder
                        string fileName = Path.GetFileNameWithoutExtension(entry.Name);
                        
                        // Read content to check
                        byte[] entryBytes = ReadZipEntryBytes(zipInputStream);
                        string yamlContent = System.Text.Encoding.UTF8.GetString(entryBytes);

                        // Components start with "Component:" or "Components:" in YAML
                        if (yamlContent.TrimStart().StartsWith("Component:") || 
                            yamlContent.TrimStart().StartsWith("Components:"))
                        {
                            componentName = fileName;

                            // Apply name filter
                            if (!string.IsNullOrEmpty(ComponentName) && !MatchesPattern(componentName, ComponentName))
                            {
                                continue;
                            }

                            // Strip the component header and unindent the content
                            string processedYaml = StripComponentHeader(yamlContent, componentName);

                            // Create PSObject
                            var psObject = new PSObject();
                            psObject.Properties.Add(new PSNoteProperty("ComponentName", componentName));
                            psObject.Properties.Add(new PSNoteProperty("FilePath", entry.Name));
                            psObject.Properties.Add(new PSNoteProperty("YamlContent", processedYaml));
                            psObject.Properties.Add(new PSNoteProperty("Size", entryBytes.Length));

                            WriteObject(psObject);
                        }
                    }
                }
            }
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

        private bool MatchesPattern(string value, string pattern)
        {
            if (string.IsNullOrEmpty(pattern))
            {
                return true;
            }

            // Convert wildcard pattern to regex pattern
            string regexPattern = "^" + System.Text.RegularExpressions.Regex.Escape(pattern)
                .Replace("\\*", ".*")
                .Replace("\\?", ".") + "$";

            return System.Text.RegularExpressions.Regex.IsMatch(value, regexPattern, 
                System.Text.RegularExpressions.RegexOptions.IgnoreCase);
        }

        /// <summary>
        /// Strips the component header (e.g., "Components:\n  ComponentName:") using SharpYaml to parse and extract the component content.
        /// </summary>
        private string StripComponentHeader(string yamlContent, string componentName)
        {
            try
            {
                // Parse YAML using SharpYaml
                var serializer = new Serializer();
                var yamlObject = serializer.Deserialize(yamlContent);

                // Expected structure: { Components: { ComponentName: { ... properties ... } } }
                if (yamlObject is Dictionary<object, object> root &&
                    root.TryGetValue("Components", out var componentsObj) &&
                    componentsObj is Dictionary<object, object> components &&
                    components.TryGetValue(componentName, out var componentContent))
                {
                    // Serialize just the component content without the header
                    var contentYaml = serializer.Serialize(componentContent);
                    return contentYaml;
                }

                // Fallback: return original if structure doesn't match
                WriteWarning($"Could not parse component YAML structure for '{componentName}'. Returning original content.");
                return yamlContent;
            }
            catch (Exception ex)
            {
                WriteWarning($"Error parsing YAML for component '{componentName}': {ex.Message}. Returning original content.");
                return yamlContent;
            }
        }
    }
}
