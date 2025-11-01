using Microsoft.PowerPlatform.Dataverse.Client;
using System.Management.Automation;

namespace Rnwood.Dataverse.Data.PowerShell.Commands
{
    /// <summary>
    /// Clears the global metadata cache.
    /// </summary>
    [Cmdlet(VerbsCommon.Clear, "DataverseMetadataCache")]
    public class ClearDataverseMetadataCacheCmdlet : PSCmdlet
    {
        /// <summary>
        /// Gets or sets the connection to clear cache for. If not specified, clears all cached metadata.
        /// </summary>
        [Parameter(HelpMessage = "Connection to clear cache for. If not specified, clears all cached metadata.")]
        public ServiceClient Connection { get; set; }

        /// <summary>
        /// Processes the cmdlet.
        /// </summary>
        protected override void ProcessRecord()
        {
            base.ProcessRecord();

            if (Connection != null)
            {
                var connectionKey = MetadataCache.GetConnectionKey(Connection);
                WriteVerbose($"Clearing metadata cache for connection: {connectionKey}");
                MetadataCache.ClearConnection(connectionKey);
            }
            else
            {
                WriteVerbose("Clearing all metadata cache");
                MetadataCache.ClearAll();
            }

            WriteVerbose("Metadata cache cleared successfully");
        }
    }
}
