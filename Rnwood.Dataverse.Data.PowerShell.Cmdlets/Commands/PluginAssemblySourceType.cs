namespace Rnwood.Dataverse.Data.PowerShell.Commands
{
    /// <summary>
    /// Specifies the source type for a plugin assembly.
    /// </summary>
    public enum PluginAssemblySourceType
    {
        /// <summary>
        /// Database - The assembly is stored in the database.
        /// </summary>
        Database = 0,

        /// <summary>
        /// Disk - The assembly is stored on disk.
        /// </summary>
        Disk = 1,

        /// <summary>
        /// Normal - Standard assembly source.
        /// </summary>
        Normal = 2,

        /// <summary>
        /// AzureWebApp - The assembly is deployed as an Azure Web App.
        /// </summary>
        AzureWebApp = 3,

        /// <summary>
        /// FileStore - The assembly is stored in a file store.
        /// </summary>
        FileStore = 4
    }
}
