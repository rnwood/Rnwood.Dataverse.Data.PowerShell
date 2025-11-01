using Microsoft.Xrm.Sdk;
using System;
using System.IO;
using System.Management.Automation;

namespace Rnwood.Dataverse.Data.PowerShell.Commands
{
    /// <summary>
    /// Creates or updates a plugin assembly in a Dataverse environment.
    /// </summary>
    [Cmdlet(VerbsCommon.Set, "DataversePluginAssembly", SupportsShouldProcess = true, ConfirmImpact = ConfirmImpact.Medium)]
    [OutputType(typeof(PSObject))]
    public class SetDataversePluginAssemblyCmdlet : OrganizationServiceCmdlet
    {
        /// <summary>
        /// Gets or sets the ID of the plugin assembly to update.
        /// </summary>
        [Parameter(ValueFromPipelineByPropertyName = true, HelpMessage = "ID of the plugin assembly to update. If not specified, a new assembly is created.")]
        public Guid? Id { get; set; }

        /// <summary>
        /// Gets or sets the name of the plugin assembly.
        /// </summary>
        [Parameter(Mandatory = true, HelpMessage = "Name of the plugin assembly")]
        public string Name { get; set; }

        /// <summary>
        /// Gets or sets the content of the assembly as a byte array.
        /// </summary>
        [Parameter(ParameterSetName = "Content", Mandatory = true, HelpMessage = "Content of the assembly as a byte array")]
        public byte[] Content { get; set; }

        /// <summary>
        /// Gets or sets the path to the assembly file to upload.
        /// </summary>
        [Parameter(ParameterSetName = "FilePath", Mandatory = true, HelpMessage = "Path to the assembly file to upload")]
        public string FilePath { get; set; }

        /// <summary>
        /// Gets or sets the isolation mode. 0=None, 1=Sandbox, 2=External
        /// </summary>
        [Parameter(HelpMessage = "Isolation mode: 0=None, 1=Sandbox, 2=External. Default is 2 (External).")]
        public int IsolationMode { get; set; } = 2;

        /// <summary>
        /// Gets or sets the source type. 0=Database, 1=Disk, 2=Normal, 3=AzureWebApp
        /// </summary>
        [Parameter(HelpMessage = "Source type: 0=Database, 1=Disk, 2=Normal, 3=AzureWebApp. Default is 0 (Database).")]
        public int SourceType { get; set; } = 0;

        /// <summary>
        /// Gets or sets the version of the assembly.
        /// </summary>
        [Parameter(HelpMessage = "Version of the assembly")]
        public string Version { get; set; }

        /// <summary>
        /// Gets or sets the culture of the assembly.
        /// </summary>
        [Parameter(HelpMessage = "Culture of the assembly")]
        public string Culture { get; set; }

        /// <summary>
        /// Gets or sets the public key token of the assembly.
        /// </summary>
        [Parameter(HelpMessage = "Public key token of the assembly")]
        public string PublicKeyToken { get; set; }

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

        /// <summary>
        /// Process the cmdlet.
        /// </summary>
        protected override void ProcessRecord()
        {
            base.ProcessRecord();

            byte[] assemblyContent = Content;
            if (ParameterSetName == "FilePath")
            {
                if (!File.Exists(FilePath))
                {
                    throw new FileNotFoundException($"Assembly file not found: {FilePath}");
                }
                assemblyContent = File.ReadAllBytes(FilePath);
            }

            Entity assembly = new Entity("pluginassembly");
            if (Id.HasValue)
            {
                assembly.Id = Id.Value;
            }

            assembly["name"] = Name;
            assembly["content"] = Convert.ToBase64String(assemblyContent);
            assembly["isolationmode"] = new OptionSetValue(IsolationMode);
            assembly["sourcetype"] = new OptionSetValue(SourceType);

            if (!string.IsNullOrEmpty(Version))
            {
                assembly["version"] = Version;
            }

            if (!string.IsNullOrEmpty(Culture))
            {
                assembly["culture"] = Culture;
            }

            if (!string.IsNullOrEmpty(PublicKeyToken))
            {
                assembly["publickeytoken"] = PublicKeyToken;
            }

            if (!string.IsNullOrEmpty(Description))
            {
                assembly["description"] = Description;
            }

            if (ShouldProcess($"Plugin Assembly: {Name}", Id.HasValue ? "Update" : "Create"))
            {
                Guid assemblyId;
                if (Id.HasValue)
                {
                    Connection.Update(assembly);
                    assemblyId = Id.Value;
                    WriteVerbose($"Updated plugin assembly: {Name} (ID: {assemblyId})");
                }
                else
                {
                    assemblyId = Connection.Create(assembly);
                    WriteVerbose($"Created plugin assembly: {Name} (ID: {assemblyId})");
                }

                if (PassThru)
                {
                    Entity retrieved = Connection.Retrieve("pluginassembly", assemblyId, new Microsoft.Xrm.Sdk.Query.ColumnSet(true));
                    EntityMetadataFactory entityMetadataFactory = new EntityMetadataFactory(Connection);
                    DataverseEntityConverter converter = new DataverseEntityConverter(Connection, entityMetadataFactory);
                    PSObject psObject = converter.ConvertToPSObject(retrieved, new Microsoft.Xrm.Sdk.Query.ColumnSet(true), _ => ValueType.Display);
                    WriteObject(psObject);
                }
            }
        }
    }
}
