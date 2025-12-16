using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Emit;
using Rnwood.Dataverse.Data.PowerShell.Model;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Management.Automation;
using System.Reflection;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;

namespace Rnwood.Dataverse.Data.PowerShell.Commands
{
    /// <summary>
    /// Compiles C# source code into a plugin assembly with embedded metadata for reconstruction.
    /// </summary>
    [Cmdlet(VerbsCommon.New, "DataversePluginAssembly")]
    [OutputType(typeof(byte[]))]
    public class NewDataversePluginAssemblyCmdlet : PSCmdlet
    {
        /// <summary>
        /// Gets or sets the C# source code to compile.
        /// </summary>
        [Parameter(Mandatory = true, ParameterSetName = "SourceCode", HelpMessage = "C# source code to compile")]
        public string SourceCode { get; set; }

        /// <summary>
        /// Gets or sets the path to the C# source file.
        /// </summary>
        [Parameter(Mandatory = true, ParameterSetName = "SourceFile", HelpMessage = "Path to C# source file")]
        public string SourceFile { get; set; }

        /// <summary>
        /// Gets or sets the assembly name.
        /// </summary>
        [Parameter(Mandatory = true, HelpMessage = "Name of the assembly")]
        public string AssemblyName { get; set; }

        /// <summary>
        /// Gets or sets the framework references.
        /// </summary>
        [Parameter(HelpMessage = "Framework assembly references (e.g., 'System', 'System.Core')")]
        public string[] FrameworkReferences { get; set; }

        /// <summary>
        /// Gets or sets the NuGet package references.
        /// </summary>
        [Parameter(HelpMessage = "NuGet package references with versions (e.g., 'Microsoft.Xrm.Sdk@9.0.2')")]
        public string[] PackageReferences { get; set; }

        /// <summary>
        /// Gets or sets the path to a strong name key file (.snk).
        /// </summary>
        [Parameter(HelpMessage = "Path to strong name key file (.snk)")]
        public string StrongNameKeyFile { get; set; }

        /// <summary>
        /// Gets or sets the assembly version.
        /// </summary>
        [Parameter(HelpMessage = "Assembly version (e.g., '1.0.0.0')")]
        public string Version { get; set; } = "1.0.0.0";

        /// <summary>
        /// Gets or sets the assembly culture.
        /// </summary>
        [Parameter(HelpMessage = "Assembly culture (e.g., 'neutral')")]
        public string Culture { get; set; } = "neutral";

        /// <summary>
        /// Gets or sets the output path for the compiled assembly.
        /// </summary>
        [Parameter(HelpMessage = "Output path for the compiled assembly")]
        public string OutputPath { get; set; }

        /// <summary>
        /// Process the cmdlet.
        /// </summary>
        protected override void ProcessRecord()
        {
            base.ProcessRecord();

            try
            {
                string sourceCode = SourceCode;
                if (ParameterSetName == "SourceFile")
                {
                    if (!File.Exists(SourceFile))
                    {
                        throw new FileNotFoundException($"Source file not found: {SourceFile}");
                    }
                    sourceCode = File.ReadAllText(SourceFile);
                }

                WriteVerbose($"Compiling assembly: {AssemblyName}");

                // Parse the source code
                SyntaxTree syntaxTree = CSharpSyntaxTree.ParseText(sourceCode);

                // Get metadata references
                List<MetadataReference> references = GetMetadataReferences();

                // Compilation options
                CSharpCompilationOptions compilationOptions = new CSharpCompilationOptions(
                    OutputKind.DynamicallyLinkedLibrary,
                    optimizationLevel: OptimizationLevel.Release,
                    assemblyIdentityComparer: DesktopAssemblyIdentityComparer.Default);

                // Add strong name key if provided
                if (!string.IsNullOrEmpty(StrongNameKeyFile))
                {
                    if (!File.Exists(StrongNameKeyFile))
                    {
                        throw new FileNotFoundException($"Strong name key file not found: {StrongNameKeyFile}");
                    }
                    
                    byte[] keyFileBytes = File.ReadAllBytes(StrongNameKeyFile);
                    compilationOptions = compilationOptions.WithStrongNameProvider(new DesktopStrongNameProvider())
                        .WithCryptoKeyFile(StrongNameKeyFile);
                }

                // Create compilation
                CSharpCompilation compilation = CSharpCompilation.Create(
                    AssemblyName,
                    new[] { syntaxTree },
                    references,
                    compilationOptions);

                // Emit to memory stream
                using (MemoryStream assemblyStream = new MemoryStream())
                {
                    EmitResult result = compilation.Emit(assemblyStream);

                    if (!result.Success)
                    {
                        StringBuilder errorMessage = new StringBuilder("Compilation failed:");
                        foreach (Diagnostic diagnostic in result.Diagnostics.Where(d => d.Severity == DiagnosticSeverity.Error))
                        {
                            errorMessage.AppendLine();
                            errorMessage.Append($"  {diagnostic.Id}: {diagnostic.GetMessage()}");
                            if (diagnostic.Location.IsInSource)
                            {
                                errorMessage.Append($" at line {diagnostic.Location.GetLineSpan().StartLinePosition.Line + 1}");
                            }
                        }
                        throw new InvalidOperationException(errorMessage.ToString());
                    }

                    WriteVerbose("Compilation successful");

                    byte[] assemblyBytes = assemblyStream.ToArray();

                    // Create metadata
                    PluginAssemblyMetadata metadata = new PluginAssemblyMetadata
                    {
                        AssemblyName = AssemblyName,
                        SourceCode = sourceCode,
                        FrameworkReferences = FrameworkReferences?.ToList() ?? new List<string>(),
                        PackageReferences = PackageReferences?.ToList() ?? new List<string>(),
                        Version = Version,
                        Culture = Culture
                    };

                    // Extract public key token if strong named
                    if (!string.IsNullOrEmpty(StrongNameKeyFile))
                    {
                        Assembly assembly = Assembly.Load(assemblyBytes);
                        byte[] publicKeyToken = assembly.GetName().GetPublicKeyToken();
                        if (publicKeyToken != null && publicKeyToken.Length > 0)
                        {
                            metadata.PublicKeyToken = BitConverter.ToString(publicKeyToken).Replace("-", "").ToLowerInvariant();
                        }
                    }

                    // Embed metadata as a resource
                    WriteVerbose("Embedding metadata in assembly");
                    byte[] assemblyWithMetadata = EmbedMetadataInAssembly(assemblyBytes, metadata);

                    // Write to output path if specified
                    if (!string.IsNullOrEmpty(OutputPath))
                    {
                        string directory = Path.GetDirectoryName(OutputPath);
                        if (!string.IsNullOrEmpty(directory) && !Directory.Exists(directory))
                        {
                            Directory.CreateDirectory(directory);
                        }
                        File.WriteAllBytes(OutputPath, assemblyWithMetadata);
                        WriteVerbose($"Assembly written to: {OutputPath}");
                    }

                    // Write to pipeline
                    WriteObject(assemblyWithMetadata);
                }
            }
            catch (Exception ex)
            {
                WriteError(new ErrorRecord(ex, "CompilationError", ErrorCategory.InvalidOperation, null));
            }
        }

        private List<MetadataReference> GetMetadataReferences()
        {
            List<MetadataReference> references = new List<MetadataReference>();

            // Add core framework references
            string[] coreReferences = new[]
            {
                typeof(object).Assembly.Location,                          // System.Private.CoreLib
                typeof(Console).Assembly.Location,                         // System.Console
                typeof(System.Runtime.AssemblyTargetedPatchBandAttribute).Assembly.Location, // System.Runtime
                typeof(System.Linq.Enumerable).Assembly.Location,          // System.Linq
                typeof(System.Collections.Generic.List<>).Assembly.Location, // System.Collections
            };

            foreach (string reference in coreReferences)
            {
                if (!string.IsNullOrEmpty(reference) && File.Exists(reference))
                {
                    references.Add(MetadataReference.CreateFromFile(reference));
                }
            }

            // Add framework references
            if (FrameworkReferences != null)
            {
                foreach (string frameworkRef in FrameworkReferences)
                {
                    try
                    {
                        Assembly assembly = Assembly.Load(frameworkRef);
                        if (!string.IsNullOrEmpty(assembly.Location) && File.Exists(assembly.Location))
                        {
                            references.Add(MetadataReference.CreateFromFile(assembly.Location));
                            WriteVerbose($"Added framework reference: {frameworkRef}");
                        }
                    }
                    catch (Exception ex)
                    {
                        WriteWarning($"Could not load framework reference '{frameworkRef}': {ex.Message}");
                    }
                }
            }

            // Add package references (for now, just try to load them)
            if (PackageReferences != null)
            {
                foreach (string packageRef in PackageReferences)
                {
                    string[] parts = packageRef.Split('@');
                    string packageName = parts[0];
                    
                    try
                    {
                        Assembly assembly = Assembly.Load(packageName);
                        if (!string.IsNullOrEmpty(assembly.Location) && File.Exists(assembly.Location))
                        {
                            references.Add(MetadataReference.CreateFromFile(assembly.Location));
                            WriteVerbose($"Added package reference: {packageRef}");
                        }
                    }
                    catch (Exception ex)
                    {
                        WriteWarning($"Could not load package reference '{packageRef}': {ex.Message}");
                    }
                }
            }

            return references;
        }

        private byte[] EmbedMetadataInAssembly(byte[] assemblyBytes, PluginAssemblyMetadata metadata)
        {
            // For now, we'll use a simple approach: append metadata as JSON at the end
            // with a marker to find it. In production, this would use proper resource embedding.
            
            string metadataJson = JsonSerializer.Serialize(metadata);
            byte[] metadataBytes = Encoding.UTF8.GetBytes(metadataJson);
            
            // Create marker: "DPLM" (Dataverse Plugin Metadata) + length (4 bytes)
            byte[] marker = Encoding.ASCII.GetBytes("DPLM");
            byte[] lengthBytes = BitConverter.GetBytes(metadataBytes.Length);
            
            // Combine: assembly + metadata + length + marker
            byte[] result = new byte[assemblyBytes.Length + metadataBytes.Length + lengthBytes.Length + marker.Length];
            Array.Copy(assemblyBytes, 0, result, 0, assemblyBytes.Length);
            Array.Copy(metadataBytes, 0, result, assemblyBytes.Length, metadataBytes.Length);
            Array.Copy(lengthBytes, 0, result, assemblyBytes.Length + metadataBytes.Length, lengthBytes.Length);
            Array.Copy(marker, 0, result, assemblyBytes.Length + metadataBytes.Length + lengthBytes.Length, marker.Length);
            
            return result;
        }
    }
}
