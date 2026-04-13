using System;
using System.Collections.Generic;

namespace Fake4Dataverse.Pipeline
{
    /// <summary>
    /// Represents a registered pipeline step. Dispose to unregister.
    /// Supports pre/post image definitions and execution mode configuration.
    /// </summary>
    public sealed class PipelineStepRegistration : IDisposable
    {
        private readonly Action _unregister;
        private bool _disposed;
        private readonly List<ImageDefinition> _preImageDefinitions = new List<ImageDefinition>();
        private readonly List<ImageDefinition> _postImageDefinitions = new List<ImageDefinition>();

        /// <summary>
        /// Gets or sets the execution mode for this step (0 = Synchronous, 1 = Asynchronous).
        /// </summary>
        public int Mode { get; set; }

        internal IReadOnlyList<ImageDefinition> PreImageDefinitions => _preImageDefinitions;
        internal IReadOnlyList<ImageDefinition> PostImageDefinitions => _postImageDefinitions;

        internal PipelineStepRegistration(Action unregister)
        {
            _unregister = unregister;
        }

        /// <summary>
        /// Registers a pre-image definition on this step.
        /// Pre-images capture the entity state before the core operation.
        /// </summary>
        /// <param name="imageName">The alias name for the image.</param>
        /// <param name="attributes">
        /// The attribute logical names to include. If empty, all attributes are included.
        /// </param>
        public PipelineStepRegistration AddPreImage(string imageName, params string[] attributes)
        {
            _preImageDefinitions.Add(new ImageDefinition(imageName, attributes));
            return this;
        }

        /// <summary>
        /// Registers a post-image definition on this step.
        /// Post-images capture the entity state after the core operation (post-operation stage only).
        /// </summary>
        /// <param name="imageName">The alias name for the image.</param>
        /// <param name="attributes">
        /// The attribute logical names to include. If empty, all attributes are included.
        /// </param>
        public PipelineStepRegistration AddPostImage(string imageName, params string[] attributes)
        {
            _postImageDefinitions.Add(new ImageDefinition(imageName, attributes));
            return this;
        }

        /// <summary>
        /// Marks this step as asynchronous (Mode = 1).
        /// </summary>
        public PipelineStepRegistration SetAsynchronous()
        {
            Mode = 1;
            return this;
        }

        /// <summary>
        /// Unregisters this pipeline step.
        /// </summary>
        public void Dispose()
        {
            if (!_disposed)
            {
                _disposed = true;
                _unregister();
            }
        }
    }
}
