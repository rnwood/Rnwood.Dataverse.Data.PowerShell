using Rnwood.Dataverse.Data.PowerShell.Model;
using System;
using System.IO;
using System.Linq;
using System.Management.Automation;
using System.Text;
using System.Text.Json;

namespace Rnwood.Dataverse.Data.PowerShell.Commands
{
    /// <summary>
    /// Extracts source code and build metadata from a plugin assembly created by New-DataversePluginAssembly.
    /// </summary>
    [Cmdlet(VerbsCommon.Get, "DataverseDynamicPluginAssembly")]
    [OutputType(typeof(PSObject))]
    public class GetDataverseDynamicPluginAssemblyCmdlet : PSCmdlet
    {
        /// <summary>
        /// Gets or sets the assembly bytes to extract from.
        /// </summary>
        [Parameter(Mandatory = true, ParameterSetName = "Bytes", ValueFromPipeline = true, HelpMessage = "Assembly bytes")]
        [Parameter(Mandatory = true, ParameterSetName = "VSProjectFromBytes", ValueFromPipeline = true, HelpMessage = "Assembly bytes")]
        public byte[] AssemblyBytes { get; set; }

        /// <summary>
        /// Gets or sets the path to the assembly file.
        /// </summary>
        [Parameter(Mandatory = true, ParameterSetName = "FilePath", HelpMessage = "Path to assembly file")]
        [Parameter(Mandatory = true, ParameterSetName = "VSProjectFromFile", HelpMessage = "Path to assembly file")]
        public string FilePath { get; set; }

        /// <summary>
        /// If specified, also outputs the source code to a file.
        /// </summary>
        [Parameter(HelpMessage = "Output path for extracted source code")]
        public string OutputSourceFile { get; set; }

        /// <summary>
        /// Gets or sets the output directory for a complete Visual Studio project.
        /// </summary>
        [Parameter(Mandatory = true, ParameterSetName = "VSProjectFromBytes", HelpMessage = "Output directory for Visual Studio project")]
        [Parameter(Mandatory = true, ParameterSetName = "VSProjectFromFile", HelpMessage = "Output directory for Visual Studio project")]
        public string OutputProjectPath { get; set; }

        /// <summary>
        /// Process the cmdlet.
        /// </summary>
        protected override void ProcessRecord()
        {
            base.ProcessRecord();

            try
            {
                byte[] assemblyBytes = null;
                
                // Determine which parameter set and load assembly bytes
                if (ParameterSetName == "Bytes" || ParameterSetName == "VSProjectFromBytes")
                {
                    assemblyBytes = AssemblyBytes;
                }
                else if (ParameterSetName == "FilePath" || ParameterSetName == "VSProjectFromFile")
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

                // Handle VS Project generation
                if (ParameterSetName == "VSProjectFromBytes" || ParameterSetName == "VSProjectFromFile")
                {
                    GenerateVSProject(metadata, OutputProjectPath);
                    return;
                }

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

        private void GenerateVSProject(PluginAssemblyMetadata metadata, string outputPath)
        {
            // Create output directory
            if (!Directory.Exists(outputPath))
            {
                Directory.CreateDirectory(outputPath);
            }

            WriteVerbose($"Generating Visual Studio project in: {outputPath}");

            // Write source code file
            string sourceFileName = $"{metadata.AssemblyName}.cs";
            string sourceFilePath = Path.Combine(outputPath, sourceFileName);
            File.WriteAllText(sourceFilePath, metadata.SourceCode);
            WriteVerbose($"Created source file: {sourceFileName}");

            // Write strong name key file if available
            string snkFileName = $"{metadata.AssemblyName}.snk";
            string snkFilePath = Path.Combine(outputPath, snkFileName);
            if (!string.IsNullOrEmpty(metadata.StrongNameKey))
            {
                byte[] keyBytes = Convert.FromBase64String(metadata.StrongNameKey);
                File.WriteAllBytes(snkFilePath, keyBytes);
                WriteVerbose($"Created strong name key file: {snkFileName}");
            }

            // Generate .csproj file
            string csprojFileName = $"{metadata.AssemblyName}.csproj";
            string csprojFilePath = Path.Combine(outputPath, csprojFileName);
            string csprojContent = GenerateCsprojContent(metadata, sourceFileName, snkFileName);
            File.WriteAllText(csprojFilePath, csprojContent);
            WriteVerbose($"Created project file: {csprojFileName}");

            WriteObject($"Visual Studio project generated successfully in: {outputPath}");
            WriteObject($"  - {sourceFileName}");
            if (!string.IsNullOrEmpty(metadata.StrongNameKey))
            {
                WriteObject($"  - {snkFileName}");
            }
            WriteObject($"  - {csprojFileName}");
            WriteVerbose($"You can now open {csprojFileName} in Visual Studio and build the project.");
        }

        private string GenerateCsprojContent(PluginAssemblyMetadata metadata, string sourceFileName, string snkFileName)
        {
            StringBuilder csproj = new StringBuilder();
            
            csproj.AppendLine("<Project Sdk=\"Microsoft.NET.Sdk\">");
            csproj.AppendLine("  <PropertyGroup>");
            csproj.AppendLine("    <TargetFramework>net462</TargetFramework>");
            csproj.AppendLine($"    <AssemblyName>{metadata.AssemblyName}</AssemblyName>");
            csproj.AppendLine($"    <AssemblyVersion>{metadata.Version}</AssemblyVersion>");
            csproj.AppendLine($"    <FileVersion>{metadata.Version}</FileVersion>");
            
            if (!string.IsNullOrEmpty(metadata.Culture) && metadata.Culture != "neutral")
            {
                csproj.AppendLine($"    <NeutralLanguage>{metadata.Culture}</NeutralLanguage>");
            }
            
            // Add strong name signing if key is available
            if (!string.IsNullOrEmpty(metadata.StrongNameKey))
            {
                csproj.AppendLine("    <SignAssembly>true</SignAssembly>");
                csproj.AppendLine($"    <AssemblyOriginatorKeyFile>{snkFileName}</AssemblyOriginatorKeyFile>");
            }
            
            csproj.AppendLine("  </PropertyGroup>");
            csproj.AppendLine();
            
            // Add package references
            if (metadata.PackageReferences != null && metadata.PackageReferences.Count > 0)
            {
                csproj.AppendLine("  <ItemGroup>");
                foreach (string packageRef in metadata.PackageReferences)
                {
                    string[] parts = packageRef.Split('@');
                    string packageName = parts[0];
                    string version = parts.Length > 1 ? parts[1] : "*";
                    csproj.AppendLine($"    <PackageReference Include=\"{packageName}\" Version=\"{version}\" />");
                }
                csproj.AppendLine("  </ItemGroup>");
                csproj.AppendLine();
            }
            
            // Always add Microsoft.CrmSdk.CoreAssemblies as it's required for plugins
            bool hasCrmSdk = metadata.PackageReferences?.Any(p => p.StartsWith("Microsoft.CrmSdk.CoreAssemblies", StringComparison.OrdinalIgnoreCase)) ?? false;
            if (!hasCrmSdk)
            {
                csproj.AppendLine("  <ItemGroup>");
                csproj.AppendLine("    <PackageReference Include=\"Microsoft.CrmSdk.CoreAssemblies\" Version=\"9.*\" />");
                csproj.AppendLine("  </ItemGroup>");
                csproj.AppendLine();
            }
            
            // Add framework references if needed
            if (metadata.FrameworkReferences != null && metadata.FrameworkReferences.Count > 0)
            {
                csproj.AppendLine("  <ItemGroup>");
                foreach (string frameworkRef in metadata.FrameworkReferences)
                {
                    // Remove .dll extension if present
                    string refName = frameworkRef.EndsWith(".dll", StringComparison.OrdinalIgnoreCase) 
                        ? frameworkRef.Substring(0, frameworkRef.Length - 4) 
                        : frameworkRef;
                    csproj.AppendLine($"    <Reference Include=\"{refName}\" />");
                }
                csproj.AppendLine("  </ItemGroup>");
                csproj.AppendLine();
            }
            
            csproj.AppendLine("</Project>");
            
            return csproj.ToString();
        }
    }
}
