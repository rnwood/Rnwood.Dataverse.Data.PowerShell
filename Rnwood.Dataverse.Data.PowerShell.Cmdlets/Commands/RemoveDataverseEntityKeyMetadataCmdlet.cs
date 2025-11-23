using Microsoft.Xrm.Sdk.Messages;
using System.Management.Automation;

namespace Rnwood.Dataverse.Data.PowerShell.Commands
{
    /// <summary>
    /// Deletes an alternate key from an entity (table) in Dataverse.
    /// </summary>
    [Cmdlet(VerbsCommon.Remove, "DataverseEntityKeyMetadata", SupportsShouldProcess = true, ConfirmImpact = ConfirmImpact.High)]
    public class RemoveDataverseEntityKeyMetadataCmdlet : OrganizationServiceCmdlet
    {
        /// <summary>
        /// Gets or sets the logical name of the entity.
        /// </summary>
        [Parameter(Mandatory = true, Position = 0, ValueFromPipeline = true, ValueFromPipelineByPropertyName = true, HelpMessage = "Logical name of the entity (table)")]
        [ArgumentCompleter(typeof(TableNameArgumentCompleter))]
        [Alias("TableName")]
        public string EntityName { get; set; }

        /// <summary>
        /// Gets or sets the logical name of the key to delete.
        /// </summary>
        [Parameter(Mandatory = true, Position = 1, ValueFromPipelineByPropertyName = true, HelpMessage = "Logical name of the alternate key to delete")]
        public string KeyName { get; set; }

        /// <summary>
        /// Processes the cmdlet.
        /// </summary>
        protected override void ProcessRecord()
        {
            base.ProcessRecord();

            if (!ShouldProcess($"Entity '{EntityName}', Key '{KeyName}'", "Delete alternate key"))
            {
                return;
            }

            var request = new DeleteEntityKeyRequest
            {
                EntityLogicalName = EntityName,
                Name = KeyName
            };

            WriteVerbose($"Deleting alternate key '{KeyName}' from entity '{EntityName}'");

            Connection.Execute(request);

            WriteVerbose($"Alternate key '{KeyName}' deleted successfully from entity '{EntityName}'");

            // Invalidate cache for this entity
            var connectionKey = MetadataCache.GetConnectionKey(Connection as Microsoft.PowerPlatform.Dataverse.Client.ServiceClient);
            if (connectionKey != null)
            {
                MetadataCache.InvalidateEntity(connectionKey, EntityName);
                WriteVerbose($"Invalidated metadata cache for entity '{EntityName}'");
            }
        }
    }
}
