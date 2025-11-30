using Microsoft.Xrm.Sdk;
using System;
using System.IO;
using System.Management.Automation;

namespace Rnwood.Dataverse.Data.PowerShell.Commands
{
    /// <summary>
    /// Creates or updates a plugin package in a Dataverse environment.
    /// </summary>
    [Cmdlet(VerbsCommon.Set, "DataversePluginPackage", SupportsShouldProcess = true, ConfirmImpact = ConfirmImpact.Medium)]
    [OutputType(typeof(PSObject))]
    public class SetDataversePluginPackageCmdlet : OrganizationServiceCmdlet
    {
        /// <summary>
        /// Gets or sets the ID of the plugin package to update.
        /// </summary>
        [Parameter(ValueFromPipelineByPropertyName = true, HelpMessage = "ID of the plugin package to update. If not specified, a new package is created.")]
        public Guid? Id { get; set; }

        /// <summary>
        /// Gets or sets the unique name of the plugin package.
        /// </summary>
        [Parameter(Mandatory = true, HelpMessage = "Unique name of the plugin package")]
        public string UniqueName { get; set; }

        /// <summary>
        /// Gets or sets the content of the package as a byte array.
        /// </summary>
        [Parameter(ParameterSetName = "Content", Mandatory = true, HelpMessage = "Content of the package as a byte array")]
        public byte[] Content { get; set; }

        /// <summary>
        /// Gets or sets the path to the package file to upload.
        /// </summary>
        [Parameter(ParameterSetName = "FilePath", Mandatory = true, HelpMessage = "Path to the package file (NuGet .nupkg) to upload")]
        public string FilePath { get; set; }

        /// <summary>
        /// Gets or sets the version of the package.
        /// </summary>
        [Parameter(HelpMessage = "Version of the package")]
        public string Version { get; set; }

        /// <summary>
        /// Gets or sets the description of the package.
        /// </summary>
        [Parameter(HelpMessage = "Description of the package")]
        public string Description { get; set; }

        /// <summary>
        /// If specified, the created/updated package is written to the pipeline as a PSObject.
        /// </summary>
        [Parameter(HelpMessage = "If specified, the created/updated package is written to the pipeline as a PSObject")]
        public SwitchParameter PassThru { get; set; }

        /// <summary>
        /// Process the cmdlet.
        /// </summary>
        protected override void ProcessRecord()
        {
            base.ProcessRecord();

            byte[] packageContent = Content;
            if (ParameterSetName == "FilePath")
            {
                if (!File.Exists(FilePath))
                {
                    throw new FileNotFoundException($"Package file not found: {FilePath}");
                }
                packageContent = File.ReadAllBytes(FilePath);
            }

            Entity package = new Entity("pluginpackage");
            if (Id.HasValue)
            {
                package.Id = Id.Value;
            }

            package["uniquename"] = UniqueName;
            package["content"] = Convert.ToBase64String(packageContent);

            if (!string.IsNullOrEmpty(Version))
            {
                package["version"] = Version;
            }

            if (!string.IsNullOrEmpty(Description))
            {
                package["description"] = Description;
            }

            if (ShouldProcess($"Plugin Package: {UniqueName}", Id.HasValue ? "Update" : "Create"))
            {
                Guid packageId;
                if (Id.HasValue)
                {
                    QueryHelpers.UpdateWithThrottlingRetry(Connection, package);
                    packageId = Id.Value;
                    WriteVerbose($"Updated plugin package: {UniqueName} (ID: {packageId})");
                }
                else
                {
                    packageId = QueryHelpers.CreateWithThrottlingRetry(Connection, package);
                    WriteVerbose($"Created plugin package: {UniqueName} (ID: {packageId})");
                }

                if (PassThru)
                {
                    Entity retrieved = QueryHelpers.RetrieveWithThrottlingRetry(Connection, "pluginpackage", packageId, new Microsoft.Xrm.Sdk.Query.ColumnSet(true));
                    EntityMetadataFactory entityMetadataFactory = new EntityMetadataFactory(Connection);
                    DataverseEntityConverter converter = new DataverseEntityConverter(Connection, entityMetadataFactory);
                    PSObject psObject = converter.ConvertToPSObject(retrieved, new Microsoft.Xrm.Sdk.Query.ColumnSet(true), _ => ValueType.Display);
                    WriteObject(psObject);
                }
            }
        }
    }
}
