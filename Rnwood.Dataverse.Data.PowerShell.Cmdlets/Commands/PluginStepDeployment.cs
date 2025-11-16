namespace Rnwood.Dataverse.Data.PowerShell.Commands
{
    /// <summary>
    /// Specifies the supported deployment for a plugin step.
    /// </summary>
    public enum PluginStepDeployment
    {
        /// <summary>
        /// ServerOnly - The plugin runs only on the server.
        /// </summary>
        ServerOnly = 0,

        /// <summary>
        /// MicrosoftDynamics365Client - The plugin runs on the Microsoft Dynamics 365 client.
        /// </summary>
        MicrosoftDynamics365Client = 1,

        /// <summary>
        /// Both - The plugin runs on both server and client.
        /// </summary>
        Both = 2
    }
}
