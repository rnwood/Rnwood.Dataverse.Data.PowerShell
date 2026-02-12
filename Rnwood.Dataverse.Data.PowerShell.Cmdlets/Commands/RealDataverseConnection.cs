using System;
using Microsoft.PowerPlatform.Dataverse.Client;

namespace Rnwood.Dataverse.Data.PowerShell.Commands
{
    /// <summary>
    /// Implementation of IDataverseConnection for real (non-mock) ServiceClient instances.
    /// Delegates Clone() to the underlying ServiceClient's Clone() method.
    /// </summary>
    internal class RealDataverseConnection : IDataverseConnection
    {
        private readonly ServiceClient _serviceClient;

        public RealDataverseConnection(ServiceClient serviceClient)
        {
            _serviceClient = serviceClient ?? throw new ArgumentNullException(nameof(serviceClient));
        }

        public ServiceClient ServiceClient => _serviceClient;

        public IDataverseConnection Clone()
        {
            var cloned = _serviceClient.Clone();
            return new RealDataverseConnection(cloned);
        }
    }
}
