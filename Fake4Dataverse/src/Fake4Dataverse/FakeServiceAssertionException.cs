using System;

namespace Fake4Dataverse
{
    /// <summary>
    /// Exception thrown when a fluent assertion on <see cref="FakeOrganizationService"/> fails.
    /// </summary>
    public sealed class FakeServiceAssertionException : Exception
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="FakeServiceAssertionException"/> class.
        /// </summary>
        public FakeServiceAssertionException(string message) : base(message)
        {
        }
    }
}
