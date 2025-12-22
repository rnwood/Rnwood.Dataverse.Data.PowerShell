using ICSharpCode.SharpZipLib.Zip;
using System;
using System.IO;
using System.Linq;
using System.Management.Automation;

namespace Rnwood.Dataverse.Data.PowerShell.Commands
{
    /// <summary>
    /// Retrieves the App properties from a .msapp file (App.pa.yaml).
    /// </summary>
    [Cmdlet(VerbsCommon.Get, "DataverseMsAppProperties")]
    [OutputType(typeof(PSObject))]
    public class GetDataverseMsAppPropertiesCmdlet : PSCmdlet
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

            ExtractAppProperties(msappBytes);
        }

        private void ExtractAppProperties(byte[] msappBytes)
        {
            using (var memoryStream = new MemoryStream(msappBytes))
            using (var zipInputStream = new ZipInputStream(memoryStream))
            {
                ZipEntry entry;
                while ((entry = zipInputStream.GetNextEntry()) != null)
                {
                    // Look for Src/App.pa.yaml
                    if (entry.Name == "Src/App.pa.yaml")
                    {
                        // Read YAML content
                        byte[] entryBytes = ReadZipEntryBytes(zipInputStream);
                        string yamlContent = System.Text.Encoding.UTF8.GetString(entryBytes);

                        // Strip the "App:" header and unindent the content
                        string processedYaml = StripAppHeader(yamlContent);

                        // Create PSObject with app properties information
                        var psObject = new PSObject();
                        psObject.Properties.Add(new PSNoteProperty("FilePath", entry.Name));
                        psObject.Properties.Add(new PSNoteProperty("YamlContent", processedYaml));
                        psObject.Properties.Add(new PSNoteProperty("Size", entryBytes.Length));

                        WriteObject(psObject);
                        return;
                    }
                }
            }

            WriteWarning("App.pa.yaml not found in .msapp file");
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
        /// Strips the "App:" header and unindents the content.
        /// </summary>
        private string StripAppHeader(string yamlContent)
        {
            // Expected format:
            // App:
            //   Properties:
            //     ...
            
            // We need to remove "App:" line and unindent by 2 spaces
            var lines = yamlContent.Split(new[] { "\r\n", "\r", "\n" }, StringSplitOptions.None);
            
            if (lines.Length < 1)
            {
                return yamlContent; // Not enough lines, return as-is
            }

            // Skip first line (App:) and unindent the rest
            var contentLines = lines.Skip(1).ToList();
            
            // Unindent by removing leading spaces (typically 2 spaces for app content)
            var result = string.Join(Environment.NewLine, contentLines.Select(line =>
            {
                // Remove up to 2 leading spaces
                if (line.StartsWith("  "))
                {
                    return line.Substring(2);
                }
                return line;
            }));

            return result;
        }
    }
}
