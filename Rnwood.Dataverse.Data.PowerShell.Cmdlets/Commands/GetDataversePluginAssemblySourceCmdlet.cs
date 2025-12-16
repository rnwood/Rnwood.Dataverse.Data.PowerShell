using Rnwood.Dataverse.Data.PowerShell.Model;
using System;
using System.IO;
using System.Management.Automation;
using System.Text;
using System.Text.Json;

namespace Rnwood.Dataverse.Data.PowerShell.Commands
{
    /// <summary>
    /// Extracts source code and build metadata from a plugin assembly created by New-DataversePluginAssembly.
    /// </summary>
    [Cmdlet(VerbsCommon.Get, "DataversePluginAssemblySource")]
    [OutputType(typeof(PSObject))]
    public class GetDataversePluginAssemblySourceCmdlet : PSCmdlet
    {
        /// <summary>
        /// Gets or sets the assembly bytes to extract from.
        /// </summary>
        [Parameter(Mandatory = true, ParameterSetName = "Bytes", ValueFromPipeline = true, HelpMessage = "Assembly bytes")]
        public byte[] AssemblyBytes { get; set; }

        /// <summary>
        /// Gets or sets the path to the assembly file.
        /// </summary>
        [Parameter(Mandatory = true, ParameterSetName = "FilePath", HelpMessage = "Path to assembly file")]
        public string FilePath { get; set; }

        /// <summary>
        /// If specified, also outputs the source code to a file.
        /// </summary>
        [Parameter(HelpMessage = "Output path for extracted source code")]
        public string OutputSourceFile { get; set; }

        /// <summary>
        /// Process the cmdlet.
        /// </summary>
        protected override void ProcessRecord()
        {
            base.ProcessRecord();

            try
            {
                byte[] assemblyBytes = AssemblyBytes;
                if (ParameterSetName == "FilePath")
                {
                    if (!File.Exists(FilePath))
                    {
                        throw new FileNotFoundException($"Assembly file not found: {FilePath}");
                    }
                    assemblyBytes = File.ReadAllBytes(FilePath);
                }

                WriteVerbose("Extracting metadata from assembly");

                PluginAssemblyMetadata metadata = ExtractMetadata(assemblyBytes);

                if (metadata == null)
                {
                    WriteWarning("No embedded metadata found in assembly. This assembly was not created with New-DataversePluginAssembly or metadata is missing.");
                    return;
                }

                WriteVerbose($"Extracted metadata for assembly: {metadata.AssemblyName}");

                // Write source to file if requested
                if (!string.IsNullOrEmpty(OutputSourceFile) && !string.IsNullOrEmpty(metadata.SourceCode))
                {
                    string directory = Path.GetDirectoryName(OutputSourceFile);
                    if (!string.IsNullOrEmpty(directory) && !Directory.Exists(directory))
                    {
                        Directory.CreateDirectory(directory);
                    }
                    File.WriteAllText(OutputSourceFile, metadata.SourceCode);
                    WriteVerbose($"Source code written to: {OutputSourceFile}");
                }

                // Create PSObject with metadata
                PSObject result = new PSObject();
                result.Properties.Add(new PSNoteProperty("AssemblyName", metadata.AssemblyName));
                result.Properties.Add(new PSNoteProperty("Version", metadata.Version));
                result.Properties.Add(new PSNoteProperty("Culture", metadata.Culture));
                result.Properties.Add(new PSNoteProperty("PublicKeyToken", metadata.PublicKeyToken));
                result.Properties.Add(new PSNoteProperty("SourceCode", metadata.SourceCode));
                result.Properties.Add(new PSNoteProperty("FrameworkReferences", metadata.FrameworkReferences.ToArray()));
                result.Properties.Add(new PSNoteProperty("PackageReferences", metadata.PackageReferences.ToArray()));

                WriteObject(result);
            }
            catch (Exception ex)
            {
                WriteError(new ErrorRecord(ex, "ExtractionError", ErrorCategory.InvalidOperation, null));
            }
        }

        private PluginAssemblyMetadata ExtractMetadata(byte[] assemblyBytes)
        {
            try
            {
                // Look for our marker at the end: "DPLM" (4 bytes)
                if (assemblyBytes.Length < 8)
                {
                    return null;
                }

                // Check for marker at the end
                byte[] marker = new byte[4];
                Array.Copy(assemblyBytes, assemblyBytes.Length - 4, marker, 0, 4);
                string markerString = Encoding.ASCII.GetString(marker);

                if (markerString != "DPLM")
                {
                    return null;
                }

                // Read length (4 bytes before marker)
                byte[] lengthBytes = new byte[4];
                Array.Copy(assemblyBytes, assemblyBytes.Length - 8, lengthBytes, 0, 4);
                int metadataLength = BitConverter.ToInt32(lengthBytes, 0);

                if (metadataLength <= 0 || metadataLength > assemblyBytes.Length - 8)
                {
                    return null;
                }

                // Extract metadata bytes
                byte[] metadataBytes = new byte[metadataLength];
                Array.Copy(assemblyBytes, assemblyBytes.Length - 8 - metadataLength, metadataBytes, 0, metadataLength);

                // Deserialize metadata
                string metadataJson = Encoding.UTF8.GetString(metadataBytes);
                PluginAssemblyMetadata metadata = JsonSerializer.Deserialize<PluginAssemblyMetadata>(metadataJson);

                return metadata;
            }
            catch (Exception ex)
            {
                WriteWarning($"Failed to extract metadata: {ex.Message}");
                return null;
            }
        }
    }
}
