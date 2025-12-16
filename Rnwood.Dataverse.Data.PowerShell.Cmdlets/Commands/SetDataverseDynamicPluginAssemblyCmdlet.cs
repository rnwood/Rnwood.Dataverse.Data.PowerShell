using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Emit;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Query;
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
    /// Compiles C# source code into a plugin assembly, uploads to Dataverse, and manages plugin types automatically.
    /// </summary>
    [Cmdlet(VerbsCommon.Set, "DataverseDynamicPluginAssembly", SupportsShouldProcess = true, ConfirmImpact = ConfirmImpact.Medium)]
    [OutputType(typeof(PSObject))]
    public class SetDataverseDynamicPluginAssemblyCmdlet : OrganizationServiceCmdlet
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
        public string Name { get; set; }

        /// <summary>
        /// Gets or sets the framework references.
        /// </summary>
        [Parameter(HelpMessage = "Framework assembly references. If not specified, existing references are reused.")]
        public string[] FrameworkReferences { get; set; }

        /// <summary>
        /// Gets or sets the NuGet package references.
        /// </summary>
        [Parameter(HelpMessage = "NuGet package references with versions (e.g., 'Microsoft.Xrm.Sdk@9.0.2'). If not specified, existing references are reused.")]
        public string[] PackageReferences { get; set; }

        /// <summary>
        /// Gets or sets the path to a strong name key file (.snk).
        /// </summary>
        [Parameter(HelpMessage = "Path to strong name key file (.snk). If not specified, an existing key is reused or a new one is generated.")]
        public string StrongNameKeyFile { get; set; }

        /// <summary>
        /// Gets or sets the assembly version.
        /// </summary>
        [Parameter(HelpMessage = "Assembly version (e.g., '1.0.0.0'). If not specified, existing version is incremented or '1.0.0.0' is used.")]
        public string Version { get; set; }

        /// <summary>
        /// Gets or sets the assembly culture.
        /// </summary>
        [Parameter(HelpMessage = "Assembly culture. If not specified, existing culture is reused or 'neutral' is used.")]
        public string Culture { get; set; }

        /// <summary>
        /// Gets or sets the isolation mode.
        /// </summary>
        [Parameter(HelpMessage = "Isolation mode for the plugin assembly. Default is Sandbox.")]
        public PluginAssemblyIsolationMode IsolationMode { get; set; } = PluginAssemblyIsolationMode.Sandbox;

        /// <summary>
        /// Gets or sets the description of the assembly.
        /// </summary>
        [Parameter(HelpMessage = "Description of the assembly")]
        public string Description { get; set; }

        /// <summary>
        /// If specified, the created/updated assembly is written to the pipeline as a PSObject.
        /// </summary>
        [Parameter(HelpMessage = "If specified, the created/updated assembly is written to the pipeline as a PSObject")]
        public SwitchParameter PassThru { get; set; }

        private string _tempKeyFilePath;

        /// <summary>
        /// Process the cmdlet.
        /// </summary>
        protected override void ProcessRecord()
        {
            base.ProcessRecord();

            try
            {
                // Read source code
                string sourceCode = SourceCode;
                if (ParameterSetName == "SourceFile")
                {
                    if (!File.Exists(SourceFile))
                    {
                        throw new FileNotFoundException($"Source file not found: {SourceFile}");
                    }
                    sourceCode = File.ReadAllText(SourceFile);
                }

                WriteVerbose($"Compiling assembly: {Name}");

                // Check if assembly already exists
                Entity existingAssembly = FindExistingAssembly(Name);
                PluginAssemblyMetadata existingMetadata = null;

                if (existingAssembly != null)
                {
                    WriteVerbose($"Found existing assembly: {Name}");
                    
                    // Extract metadata from existing assembly
                    if (existingAssembly.Contains("content"))
                    {
                        string base64Content = existingAssembly.GetAttributeValue<string>("content");
                        byte[] existingBytes = Convert.FromBase64String(base64Content);
                        existingMetadata = ExtractMetadata(existingBytes);
                    }
                }

                // Resolve parameters using existing metadata or defaults
                string effectiveVersion = Version ?? existingMetadata?.Version ?? "1.0.0.0";
                string effectiveCulture = Culture ?? existingMetadata?.Culture ?? "neutral";
                string[] effectiveFrameworkRefs = FrameworkReferences ?? existingMetadata?.FrameworkReferences?.ToArray();
                string[] effectivePackageRefs = PackageReferences ?? existingMetadata?.PackageReferences?.ToArray();

                // Handle strong name key
                string effectiveKeyPath = StrongNameKeyFile;
                if (string.IsNullOrEmpty(effectiveKeyPath))
                {
                    if (existingMetadata != null && !string.IsNullOrEmpty(existingMetadata.PublicKeyToken))
                    {
                        WriteVerbose("Reusing existing strong name key");
                        // Would need to extract key from existing assembly - for now, generate new
                        effectiveKeyPath = GenerateStrongNameKey();
                    }
                    else
                    {
                        WriteVerbose("Generating new strong name key");
                        effectiveKeyPath = GenerateStrongNameKey();
                    }
                }

                // Detect plugin classes
                List<string> pluginTypeNames = DetectPluginTypes(sourceCode);
                
                if (pluginTypeNames.Count == 0)
                {
                    throw new InvalidOperationException("No plugin types found in source code. Plugin classes must implement Microsoft.Xrm.Sdk.IPlugin interface.");
                }

                WriteVerbose($"Detected {pluginTypeNames.Count} plugin type(s): {string.Join(", ", pluginTypeNames)}");

                // Compile assembly
                byte[] assemblyBytes = CompileAssembly(
                    sourceCode, 
                    Name, 
                    effectiveKeyPath,
                    effectiveFrameworkRefs,
                    effectivePackageRefs);

                // Create metadata
                PluginAssemblyMetadata metadata = new PluginAssemblyMetadata
                {
                    AssemblyName = Name,
                    SourceCode = sourceCode,
                    FrameworkReferences = effectiveFrameworkRefs?.ToList() ?? new List<string>(),
                    PackageReferences = effectivePackageRefs?.ToList() ?? new List<string>(),
                    Version = effectiveVersion,
                    Culture = effectiveCulture
                };

                // Extract public key token
                Assembly loadedAssembly = Assembly.Load(assemblyBytes);
                byte[] publicKeyToken = loadedAssembly.GetName().GetPublicKeyToken();
                if (publicKeyToken != null && publicKeyToken.Length > 0)
                {
                    metadata.PublicKeyToken = BitConverter.ToString(publicKeyToken).Replace("-", "").ToLowerInvariant();
                }

                // Embed metadata
                byte[] assemblyWithMetadata = EmbedMetadataInAssembly(assemblyBytes, metadata);

                // Upload to Dataverse
                if (ShouldProcess($"Plugin Assembly: {Name}", existingAssembly != null ? "Update" : "Create"))
                {
                    Guid assemblyId = UploadToDataverse(existingAssembly, Name, assemblyWithMetadata, effectiveVersion, effectiveCulture, metadata.PublicKeyToken);

                    // Manage plugin types
                    ManagePluginTypes(assemblyId, pluginTypeNames, loadedAssembly);

                    if (PassThru)
                    {
                        Entity retrieved = Connection.Retrieve("pluginassembly", assemblyId, new ColumnSet(true));
                        EntityMetadataFactory entityMetadataFactory = new EntityMetadataFactory(Connection);
                        DataverseEntityConverter converter = new DataverseEntityConverter(Connection, entityMetadataFactory);
                        PSObject psObject = converter.ConvertToPSObject(retrieved, new ColumnSet(true), _ => ValueType.Display);
                        WriteObject(psObject);
                    }
                }
            }
            finally
            {
                // Clean up temp key file
                if (!string.IsNullOrEmpty(_tempKeyFilePath) && File.Exists(_tempKeyFilePath))
                {
                    try
                    {
                        File.Delete(_tempKeyFilePath);
                    }
                    catch
                    {
                        // Ignore cleanup errors
                    }
                }
            }
        }

        private Entity FindExistingAssembly(string name)
        {
            QueryExpression query = new QueryExpression("pluginassembly")
            {
                ColumnSet = new ColumnSet(true),
                Criteria = new FilterExpression()
            };
            query.Criteria.AddCondition("name", ConditionOperator.Equal, name);

            EntityCollection results = Connection.RetrieveMultiple(query);
            return results.Entities.FirstOrDefault();
        }

        private string GenerateStrongNameKey()
        {
            _tempKeyFilePath = Path.Combine(Path.GetTempPath(), $"{Guid.NewGuid()}.snk");
            
            using (RSACryptoServiceProvider rsa = new RSACryptoServiceProvider(2048))
            {
                byte[] keyPair = rsa.ExportCspBlob(true);
                File.WriteAllBytes(_tempKeyFilePath, keyPair);
            }

            WriteVerbose($"Generated strong name key: {_tempKeyFilePath}");
            return _tempKeyFilePath;
        }

        private List<string> DetectPluginTypes(string sourceCode)
        {
            List<string> pluginTypes = new List<string>();

            SyntaxTree syntaxTree = CSharpSyntaxTree.ParseText(sourceCode);
            CompilationUnitSyntax root = syntaxTree.GetCompilationUnitRoot();

            // Find all classes that implement IPlugin
            var classDeclarations = root.DescendantNodes().OfType<ClassDeclarationSyntax>();

            foreach (var classDecl in classDeclarations)
            {
                // Check if class implements IPlugin
                if (classDecl.BaseList != null)
                {
                    foreach (var baseType in classDecl.BaseList.Types)
                    {
                        string baseTypeName = baseType.Type.ToString();
                        if (baseTypeName.Contains("IPlugin"))
                        {
                            // Get full type name including namespace
                            string fullTypeName = GetFullTypeName(classDecl, root);
                            pluginTypes.Add(fullTypeName);
                            break;
                        }
                    }
                }
            }

            return pluginTypes;
        }

        private string GetFullTypeName(ClassDeclarationSyntax classDecl, CompilationUnitSyntax root)
        {
            // Get namespace
            var namespaceDecl = classDecl.Ancestors().OfType<NamespaceDeclarationSyntax>().FirstOrDefault();
            
            if (namespaceDecl != null)
            {
                return $"{namespaceDecl.Name}.{classDecl.Identifier.Text}";
            }

            return classDecl.Identifier.Text;
        }

        private byte[] CompileAssembly(string sourceCode, string assemblyName, string keyFilePath, string[] frameworkRefs, string[] packageRefs)
        {
            SyntaxTree syntaxTree = CSharpSyntaxTree.ParseText(sourceCode);

            List<MetadataReference> references = GetMetadataReferences(frameworkRefs, packageRefs);

            CSharpCompilationOptions compilationOptions = new CSharpCompilationOptions(
                OutputKind.DynamicallyLinkedLibrary,
                optimizationLevel: OptimizationLevel.Release,
                assemblyIdentityComparer: DesktopAssemblyIdentityComparer.Default);

            if (!string.IsNullOrEmpty(keyFilePath) && File.Exists(keyFilePath))
            {
                compilationOptions = compilationOptions
                    .WithStrongNameProvider(new DesktopStrongNameProvider())
                    .WithCryptoKeyFile(keyFilePath);
            }

            CSharpCompilation compilation = CSharpCompilation.Create(
                assemblyName,
                new[] { syntaxTree },
                references,
                compilationOptions);

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

                return assemblyStream.ToArray();
            }
        }

private List<MetadataReference> GetMetadataReferences(string[] frameworkRefs, string[] packageRefs)
        {
            List<MetadataReference> references = new List<MetadataReference>();

            // Core references
            string[] coreReferences = new[]
            {
                typeof(object).Assembly.Location,
                typeof(Console).Assembly.Location,
                typeof(System.Runtime.AssemblyTargetedPatchBandAttribute).Assembly.Location,
                typeof(System.Linq.Enumerable).Assembly.Location,
                typeof(System.Collections.Generic.List<>).Assembly.Location,
            };

            foreach (string reference in coreReferences)
            {
                if (!string.IsNullOrEmpty(reference) && File.Exists(reference))
                {
                    references.Add(MetadataReference.CreateFromFile(reference));
                }
            }

            // Add System.Runtime explicitly (needed for Type and other runtime types)
            try
            {
                Assembly runtimeAssembly = Assembly.Load("System.Runtime");
                if (!string.IsNullOrEmpty(runtimeAssembly.Location) && File.Exists(runtimeAssembly.Location))
                {
                    references.Add(MetadataReference.CreateFromFile(runtimeAssembly.Location));
                    WriteVerbose($"Added required reference: System.Runtime from {runtimeAssembly.Location}");
                }
            }
            catch (Exception ex)
            {
                WriteWarning($"Could not load System.Runtime: {ex.Message}");
            }

            // Add Microsoft.Xrm.Sdk as a required reference for plugin assemblies
            try
            {
                Assembly xrmSdkAssembly = typeof(Microsoft.Xrm.Sdk.IPlugin).Assembly;
                if (!string.IsNullOrEmpty(xrmSdkAssembly.Location) && File.Exists(xrmSdkAssembly.Location))
                {
                    references.Add(MetadataReference.CreateFromFile(xrmSdkAssembly.Location));
                    WriteVerbose($"Added required reference: Microsoft.Xrm.Sdk from {xrmSdkAssembly.Location}");
                }
            }
            catch (Exception ex)
            {
                WriteWarning($"Could not load Microsoft.Xrm.Sdk: {ex.Message}");
            }

            // Add System.ComponentModel for IServiceProvider (needed by plugin interface)
            try
            {
                Assembly componentModelAssembly = Assembly.Load("System.ComponentModel, Version=8.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a");
                if (!string.IsNullOrEmpty(componentModelAssembly.Location) && File.Exists(componentModelAssembly.Location))
                {
                    references.Add(MetadataReference.CreateFromFile(componentModelAssembly.Location));
                    WriteVerbose($"Added required reference: System.ComponentModel from {componentModelAssembly.Location}");
                }
            }
            catch (Exception ex)
            {
                WriteWarning($"Could not load System.ComponentModel: {ex.Message}");
            }

            // Framework references
            if (frameworkRefs != null)
            {
                foreach (string frameworkRef in frameworkRefs)
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
                    catch
                    {
                        WriteWarning($"Could not load framework reference '{frameworkRef}'");
                    }
                }
            }

            // Package references
            if (packageRefs != null)
            {
                foreach (string packageRef in packageRefs)
                {
                    string packageName = packageRef.Split('@')[0];
                    try
                    {
                        Assembly assembly = Assembly.Load(packageName);
                        if (!string.IsNullOrEmpty(assembly.Location) && File.Exists(assembly.Location))
                        {
                            references.Add(MetadataReference.CreateFromFile(assembly.Location));
                            WriteVerbose($"Added package reference: {packageRef}");
                        }
                    }
                    catch
                    {
                        WriteWarning($"Could not load package reference '{packageRef}'");
                    }
                }
            }

            return references;
        }
        private byte[] EmbedMetadataInAssembly(byte[] assemblyBytes, PluginAssemblyMetadata metadata)
        {
            string metadataJson = JsonSerializer.Serialize(metadata);
            byte[] metadataBytes = Encoding.UTF8.GetBytes(metadataJson);
            
            byte[] marker = Encoding.ASCII.GetBytes("DPLM");
            byte[] lengthBytes = BitConverter.GetBytes(metadataBytes.Length);
            
            byte[] result = new byte[assemblyBytes.Length + metadataBytes.Length + lengthBytes.Length + marker.Length];
            Array.Copy(assemblyBytes, 0, result, 0, assemblyBytes.Length);
            Array.Copy(metadataBytes, 0, result, assemblyBytes.Length, metadataBytes.Length);
            Array.Copy(lengthBytes, 0, result, assemblyBytes.Length + metadataBytes.Length, lengthBytes.Length);
            Array.Copy(marker, 0, result, assemblyBytes.Length + metadataBytes.Length + lengthBytes.Length, marker.Length);
            
            return result;
        }

        private PluginAssemblyMetadata ExtractMetadata(byte[] assemblyBytes)
        {
            try
            {
                if (assemblyBytes.Length < 8) return null;

                byte[] marker = new byte[4];
                Array.Copy(assemblyBytes, assemblyBytes.Length - 4, marker, 0, 4);
                
                if (Encoding.ASCII.GetString(marker) != "DPLM") return null;

                byte[] lengthBytes = new byte[4];
                Array.Copy(assemblyBytes, assemblyBytes.Length - 8, lengthBytes, 0, 4);
                int metadataLength = BitConverter.ToInt32(lengthBytes, 0);

                if (metadataLength <= 0 || metadataLength > assemblyBytes.Length - 8) return null;

                byte[] metadataBytes = new byte[metadataLength];
                Array.Copy(assemblyBytes, assemblyBytes.Length - 8 - metadataLength, metadataBytes, 0, metadataLength);

                string metadataJson = Encoding.UTF8.GetString(metadataBytes);
                return JsonSerializer.Deserialize<PluginAssemblyMetadata>(metadataJson);
            }
            catch
            {
                return null;
            }
        }

        private Guid UploadToDataverse(Entity existingAssembly, string name, byte[] assemblyBytes, string version, string culture, string publicKeyToken)
        {
            Entity assembly = new Entity("pluginassembly");
            
            if (existingAssembly != null)
            {
                assembly.Id = existingAssembly.Id;
            }

            assembly["name"] = name;
            assembly["content"] = Convert.ToBase64String(assemblyBytes);
            assembly["isolationmode"] = new OptionSetValue((int)IsolationMode);
            assembly["sourcetype"] = new OptionSetValue((int)PluginAssemblySourceType.Database);
            assembly["version"] = version;
            assembly["culture"] = culture ?? "neutral";
            
            if (!string.IsNullOrEmpty(publicKeyToken))
            {
                assembly["publickeytoken"] = publicKeyToken;
            }

            if (!string.IsNullOrEmpty(Description))
            {
                assembly["description"] = Description;
            }

            if (existingAssembly != null)
            {
                Connection.Update(assembly);
                WriteVerbose($"Updated plugin assembly: {name}");
                return existingAssembly.Id;
            }
            else
            {
                Guid id = Connection.Create(assembly);
                WriteVerbose($"Created plugin assembly: {name} (ID: {id})");
                return id;
            }
        }

        private void ManagePluginTypes(Guid assemblyId, List<string> pluginTypeNames, Assembly loadedAssembly)
        {
            // Get existing plugin types
            QueryExpression query = new QueryExpression("plugintype")
            {
                ColumnSet = new ColumnSet("typename", "plugintypeid"),
                Criteria = new FilterExpression()
            };
            query.Criteria.AddCondition("pluginassemblyid", ConditionOperator.Equal, assemblyId);

            EntityCollection existingTypes = Connection.RetrieveMultiple(query);
            HashSet<string> existingTypeNames = new HashSet<string>(
                existingTypes.Entities.Select(e => e.GetAttributeValue<string>("typename")),
                StringComparer.OrdinalIgnoreCase);

            // Add new plugin types
            foreach (string typeName in pluginTypeNames)
            {
                if (!existingTypeNames.Contains(typeName))
                {
                    WriteVerbose($"Creating plugin type: {typeName}");
                    CreatePluginType(assemblyId, typeName, loadedAssembly);
                }
                else
                {
                    WriteVerbose($"Plugin type already exists: {typeName}");
                }
            }

            // Remove plugin types that no longer exist
            foreach (Entity existingType in existingTypes.Entities)
            {
                string typeName = existingType.GetAttributeValue<string>("typename");
                if (!pluginTypeNames.Contains(typeName, StringComparer.OrdinalIgnoreCase))
                {
                    WriteVerbose($"Removing plugin type: {typeName}");
                    Connection.Delete("plugintype", existingType.Id);
                }
            }
        }

        private void CreatePluginType(Guid assemblyId, string typeName, Assembly loadedAssembly)
        {
            Entity pluginType = new Entity("plugintype");
            pluginType["pluginassemblyid"] = new EntityReference("pluginassembly", assemblyId);
            pluginType["typename"] = typeName;
            pluginType["friendlyname"] = typeName.Split('.').Last();
            pluginType["name"] = typeName;

            Connection.Create(pluginType);
        }
    }
}
