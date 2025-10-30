using System.Management.Automation;

namespace Rnwood.Dataverse.Data.PowerShell.Commands
{
    /// <summary>
    /// Enables or disables the global metadata cache and returns the current state.
    /// </summary>
    [Cmdlet(VerbsCommon.Set, "DataverseMetadataCache")]
    [OutputType(typeof(bool))]
    public class SetDataverseMetadataCacheCmdlet : PSCmdlet
    {
        /// <summary>
        /// Gets or sets whether to enable the metadata cache.
        /// </summary>
        [Parameter(Position = 0, Mandatory = true, HelpMessage = "Enable or disable the global metadata cache")]
        public bool Enabled { get; set; }

        /// <summary>
        /// Processes the cmdlet.
        /// </summary>
        protected override void ProcessRecord()
        {
            base.ProcessRecord();

            MetadataCache.IsEnabled = Enabled;

            WriteVerbose($"Metadata cache {(Enabled ? "enabled" : "disabled")}");

            WriteObject(MetadataCache.IsEnabled);
        }
    }

    /// <summary>
    /// Gets the current state of the global metadata cache.
    /// </summary>
    [Cmdlet(VerbsCommon.Get, "DataverseMetadataCache")]
    [OutputType(typeof(bool))]
    public class GetDataverseMetadataCacheCmdlet : PSCmdlet
    {
        /// <summary>
        /// Processes the cmdlet.
        /// </summary>
        protected override void ProcessRecord()
        {
            base.ProcessRecord();

            WriteObject(MetadataCache.IsEnabled);
        }
    }
}
