using System;

namespace Fake4Dataverse.Pipeline
{
    /// <summary>
    /// Defines a pre-image or post-image registered on a pipeline step.
    /// </summary>
    internal sealed class ImageDefinition
    {
        /// <summary>Gets the image alias name.</summary>
        public string Name { get; }

        /// <summary>
        /// Gets the attribute logical names to include in the image.
        /// An empty array means all attributes.
        /// </summary>
        public string[] Attributes { get; }

        public ImageDefinition(string name, string[] attributes)
        {
            Name = name ?? throw new ArgumentNullException(nameof(name));
            Attributes = attributes ?? Array.Empty<string>();
        }
    }
}
