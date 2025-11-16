namespace Rnwood.Dataverse.Data.PowerShell.Commands
{
    /// <summary>
    /// Specifies the execution mode for a plugin step.
    /// </summary>
    public enum PluginStepMode
    {
        /// <summary>
        /// Synchronous - The plugin executes synchronously.
        /// </summary>
        Synchronous = 0,

        /// <summary>
        /// Asynchronous - The plugin executes asynchronously.
        /// </summary>
        Asynchronous = 1
    }
}
