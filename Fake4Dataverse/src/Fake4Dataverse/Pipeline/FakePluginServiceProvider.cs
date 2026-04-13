using System;
using Microsoft.Xrm.Sdk;

namespace Fake4Dataverse.Pipeline
{
    /// <summary>
    /// <see cref="IServiceProvider"/> passed to <see cref="IPlugin.Execute"/> during pipeline
    /// execution.  Resolves <see cref="IPluginExecutionContext"/>,
    /// <see cref="IOrganizationServiceFactory"/>, and <see cref="ITracingService"/>.
    /// </summary>
    internal sealed class FakePluginServiceProvider : IServiceProvider
    {
        private readonly IPluginExecutionContext _context;
        private readonly IOrganizationServiceFactory _factory;
        private readonly ITracingService _tracingService;

        internal FakePluginServiceProvider(
            IPluginExecutionContext context,
            IOrganizationServiceFactory factory,
            ITracingService tracingService)
        {
            _context = context;
            _factory = factory;
            _tracingService = tracingService;
        }

        /// <inheritdoc />
        public object? GetService(Type serviceType)
        {
            if (serviceType == typeof(IPluginExecutionContext)) return _context;
            if (serviceType == typeof(IOrganizationServiceFactory)) return _factory;
            if (serviceType == typeof(ITracingService)) return _tracingService;
            return null;
        }
    }
}
