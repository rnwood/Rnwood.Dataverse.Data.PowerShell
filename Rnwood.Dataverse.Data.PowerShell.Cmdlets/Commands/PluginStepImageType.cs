namespace Rnwood.Dataverse.Data.PowerShell.Commands
{
    /// <summary>
    /// Specifies the type of plugin step image.
    /// </summary>
    public enum PluginStepImageType
    {
        /// <summary>
        /// PreImage - A snapshot of the entity before the operation.
        /// </summary>
        PreImage = 0,

        /// <summary>
        /// PostImage - A snapshot of the entity after the operation.
        /// </summary>
        PostImage = 1,

        /// <summary>
        /// Both - Both pre-image and post-image.
        /// </summary>
        Both = 2
    }
}
