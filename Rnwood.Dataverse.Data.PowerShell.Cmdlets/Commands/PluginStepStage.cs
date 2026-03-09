namespace Rnwood.Dataverse.Data.PowerShell.Commands
{
    /// <summary>
    /// Specifies the stage of execution for a plugin step.
    /// </summary>
    public enum PluginStepStage
    {
        /// <summary>
        /// PreValidation - Executes before the main operation and outside the database transaction.
        /// </summary>
        PreValidation = 10,

        /// <summary>
        /// PreOperation - Executes before the main operation and within the database transaction.
        /// </summary>
        PreOperation = 20,

        /// <summary>
        /// PostOperation - Executes after the main operation and within the database transaction.
        /// </summary>
        PostOperation = 40,

        /// <summary>
        /// PostOperationDeprecated - Deprecated post-operation stage (use PostOperation instead).
        /// </summary>
        PostOperationDeprecated = 50
    }
}
