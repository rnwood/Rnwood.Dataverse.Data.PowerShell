namespace Rnwood.Dataverse.Data.PowerShell.Commands
{
    /// <summary>
    /// Specifies the isolation mode for a plugin assembly.
    /// </summary>
    public enum PluginAssemblyIsolationMode
    {
        /// <summary>
        /// None - The assembly runs without isolation.
        /// </summary>
        None = 0,

        /// <summary>
        /// Sandbox - The assembly runs in a sandboxed environment (recommended for security).
        /// </summary>
        Sandbox = 1,

        /// <summary>
        /// External - The assembly runs as an external process.
        /// </summary>
        External = 2
    }
}
