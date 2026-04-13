using System;
using Microsoft.Xrm.Sdk;

namespace Fake4Dataverse.Pipeline
{
    /// <summary>
    /// In-memory implementation of <see cref="IOrganizationServiceFactory"/> that delegates to
    /// a factory delegate supplied by <see cref="PipelineManager"/>.
    /// </summary>
    internal sealed class FakeOrganizationServiceFactory : IOrganizationServiceFactory
    {
        private readonly Func<Guid?, IOrganizationService> _factory;

        internal FakeOrganizationServiceFactory(Func<Guid?, IOrganizationService> factory)
        {
            _factory = factory;
        }

        /// <inheritdoc />
        public IOrganizationService CreateOrganizationService(Guid? userId) => _factory(userId);
    }
}
