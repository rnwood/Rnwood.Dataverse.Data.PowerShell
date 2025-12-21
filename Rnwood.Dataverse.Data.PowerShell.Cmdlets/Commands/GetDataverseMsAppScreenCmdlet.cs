using MsAppToolkit;
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
            var doc = MsAppDocument.Load(msappBytes);
            var screens = doc.GetScreens();

            foreach (var screen in screens)
            {
                // Apply name filter if specified
                if (!string.IsNullOrEmpty(ScreenName))
                {
                    if (!MatchesPattern(screen.Name, ScreenName))
                    {
                        continue;
                    }
                }

                // Use MsAppDocument.ExportScreenYaml which produces YAML with MetadataKey,
                // IsLocked, Layout, and other structural fields that Studio-generated YAML omits.
                var fullYaml = doc.ExportScreenYaml(screen.Name, includeHeader: true);

                // Strip the Screens: / ScreenName: wrapper to return just the screen content
                var processedYaml = StripScreenHeader(fullYaml, screen.Name);

                var psObject = new PSObject();
                psObject.Properties.Add(new PSNoteProperty("ScreenName", screen.Name));
                psObject.Properties.Add(new PSNoteProperty("FilePath", $"Src/{screen.Name}.pa.yaml"));
                psObject.Properties.Add(new PSNoteProperty("YamlContent", processedYaml));
                psObject.Properties.Add(new PSNoteProperty("Size", System.Text.Encoding.UTF8.GetByteCount(fullYaml)));

                WriteObject(psObject);
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
        /// Strips the screen header (e.g., "Screens:\n  ScreenName:") using YamlDotNet to parse and extract the screen content.
        /// </summary>
        private string StripScreenHeader(string yamlContent, string screenName)
        {
            try
            {
                var deserializer = new YamlDotNet.Serialization.DeserializerBuilder().IgnoreUnmatchedProperties().Build();
                var serializer = new YamlDotNet.Serialization.SerializerBuilder()
                    .ConfigureDefaultValuesHandling(YamlDotNet.Serialization.DefaultValuesHandling.OmitNull)
                    .Build();
                var yamlObject = deserializer.Deserialize<Dictionary<object, object?>>(yamlContent);

                if (yamlObject is not null &&
                    yamlObject.TryGetValue("Screens", out var screensObj) &&
                    screensObj is Dictionary<object, object?> screens &&
                    screens.TryGetValue(screenName, out var screenContent))
                {
                    return serializer.Serialize(screenContent);
                }

                var errorRecord = new ErrorRecord(
                    new InvalidDataException($"Could not parse screen YAML structure for '{screenName}'."),
                    "ScreenYamlStructureInvalid",
                    ErrorCategory.InvalidData,
                    screenName);
                ThrowTerminatingError(errorRecord);
                return null;
            }
            catch (Exception ex) when (ex is not PipelineStoppedException)
            {
                ThrowTerminatingError(new ErrorRecord(
                    ex,
                    "ScreenYamlParseError",
                    ErrorCategory.InvalidData,
                    screenName));
                return null;
            }
        }
    }
}
