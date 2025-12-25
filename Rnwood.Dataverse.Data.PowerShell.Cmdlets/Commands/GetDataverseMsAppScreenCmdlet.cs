using ICSharpCode.SharpZipLib.Zip;
using SharpYaml.Serialization;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Management.Automation;
using System.Text.RegularExpressions;

namespace Rnwood.Dataverse.Data.PowerShell.Commands
{
    /// <summary>
    /// Retrieves screens from a .msapp file.
    /// </summary>
    [Cmdlet(VerbsCommon.Get, "DataverseMsAppScreen")]
    [OutputType(typeof(PSObject))]
    public class GetDataverseMsAppScreenCmdlet : PSCmdlet
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
        /// Gets or sets the name pattern of screens to retrieve. Supports wildcards (* and ?).
        /// </summary>
        [Parameter(HelpMessage = "Name pattern of screens to retrieve. Supports wildcards (* and ?)")]
        public string ScreenName { get; set; }

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

            ExtractScreens(msappBytes);
        }

        private void ExtractScreens(byte[] msappBytes)
        {
            using (var memoryStream = new MemoryStream(msappBytes))
            using (var zipInputStream = new ZipInputStream(memoryStream))
            {
                ZipEntry entry;
                while ((entry = zipInputStream.GetNextEntry()) != null)
                {
                    // Screen YAML files are in the Src/ folder and end with .pa.yaml
                    // Skip App.pa.yaml and _EditorState.pa.yaml
                    if (entry.Name.StartsWith("Src/") && 
                        entry.Name.EndsWith(".pa.yaml") && 
                        !entry.Name.EndsWith("/App.pa.yaml") &&
                        !entry.Name.EndsWith("/_EditorState.pa.yaml"))
                    {
                        string screenName = Path.GetFileNameWithoutExtension(entry.Name);

                        // Apply name filter if specified
                        if (!string.IsNullOrEmpty(ScreenName))
                        {
                            if (!MatchesPattern(screenName, ScreenName))
                            {
                                continue;
                            }
                        }

                        // Read YAML content
                        byte[] entryBytes = ReadZipEntryBytes(zipInputStream);
                        string yamlContent = System.Text.Encoding.UTF8.GetString(entryBytes);

                        // Strip the screen header and unindent the content
                        string processedYaml = StripScreenHeader(yamlContent, screenName);

                        // Create PSObject with screen information
                        var psObject = new PSObject();
                        psObject.Properties.Add(new PSNoteProperty("ScreenName", screenName));
                        psObject.Properties.Add(new PSNoteProperty("FilePath", entry.Name));
                        psObject.Properties.Add(new PSNoteProperty("YamlContent", processedYaml));
                        psObject.Properties.Add(new PSNoteProperty("Size", entryBytes.Length));

                        WriteObject(psObject);
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
        /// Strips the screen header (e.g., "Screens:\n  ScreenName:") using SharpYaml to parse and extract the screen content.
        /// </summary>
        private string StripScreenHeader(string yamlContent, string screenName)
        {
            try
            {
                // Parse YAML using SharpYaml
                var serializer = new Serializer();
                var yamlObject = serializer.Deserialize(yamlContent);

                // Expected structure: { Screens: { ScreenName: { ... properties ... } } }
                if (yamlObject is Dictionary<object, object> root &&
                    root.TryGetValue("Screens", out var screensObj) &&
                    screensObj is Dictionary<object, object> screens &&
                    screens.TryGetValue(screenName, out var screenContent))
                {
                    // Serialize just the screen content without the header
                    var contentYaml = serializer.Serialize(screenContent);
                    return contentYaml;
                }

                // Fallback: return original if structure doesn't match
                WriteWarning($"Could not parse screen YAML structure for '{screenName}'. Returning original content.");
                return yamlContent;
            }
            catch (Exception ex)
            {
                WriteWarning($"Error parsing YAML for screen '{screenName}': {ex.Message}. Returning original content.");
                return yamlContent;
            }
        }
    }
}
