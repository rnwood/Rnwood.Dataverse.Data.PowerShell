namespace Fake4Dataverse.Pipeline
{
    /// <summary>
    /// Defines the stages in the plugin-like execution pipeline.
    /// </summary>
    public enum PipelineStage
    {
        /// <summary>Fires before validation. Can abort the operation.</summary>
        PreValidation = 10,

        /// <summary>Fires before the core operation. Can abort the operation.</summary>
        PreOperation = 20,

        /// <summary>Fires after the core operation completes.</summary>
        PostOperation = 40
    }
}
