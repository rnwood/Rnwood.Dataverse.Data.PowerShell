using System.Collections.Generic;

namespace Rnwood.Dataverse.Data.PowerShell.Model
{
    /// <summary>
    /// Metadata for a plugin assembly containing source code and build information.
    /// </summary>
    public class PluginAssemblyMetadata
    {
        /// <summary>
        /// Gets or sets the C# source code for the assembly.
        /// </summary>
        public string SourceCode { get; set; }

        /// <summary>
        /// Gets or sets the framework references (e.g., "System.dll", "System.Core.dll").
        /// </summary>
        public List<string> FrameworkReferences { get; set; } = new List<string>();

        /// <summary>
        /// Gets or sets the NuGet package references with versions (e.g., "Microsoft.Xrm.Sdk@9.0.0").
        /// </summary>
        public List<string> PackageReferences { get; set; } = new List<string>();

        /// <summary>
        /// Gets or sets the public key token used for strong naming (hex string).
        /// </summary>
        public string PublicKeyToken { get; set; }

        /// <summary>
        /// Gets or sets the assembly version.
        /// </summary>
        public string Version { get; set; }

        /// <summary>
        /// Gets or sets the assembly culture.
        /// </summary>
        public string Culture { get; set; }

        /// <summary>
        /// Gets or sets the assembly name.
        /// </summary>
        public string AssemblyName { get; set; }

        /// <summary>
        /// Gets or sets the strong name key pair (base64 encoded).
        /// This allows the same key to be reused when updating the assembly.
        /// </summary>
        public string StrongNameKey { get; set; }
    }
}
