using Microsoft.PowerPlatform.Dataverse.Client;
using Microsoft.Xrm.Sdk;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Rnwood.Dataverse.Data.PowerShell.FrameworkSpecific
{
	public class DataverseConnection
	{ 

        internal DataverseConnection(IOrganizationService service, string info)
		{
			this.Service = service;
			this.Info = info;
		}
		public IOrganizationService Service { get; private set; }

		internal string Info { get; private set; }

		public override string ToString()
		{
			return Info;
		}

		internal IDisposable WithCallerId(Guid callerId)
		{
			if (callerId == Guid.Empty)
			{
				return new NoimpersonationScope();
			}

			if (!(Service is ServiceClient serviceClient))
			{
				throw new NotSupportedException("The current connection does not support impersonation");
			}

			return new ImpersonationScope(serviceClient,callerId);
		}

		class NoimpersonationScope : IDisposable
		{
			public void Dispose()
			{
				
			}
		}

		class ImpersonationScope : IDisposable
		{
			internal ImpersonationScope(ServiceClient serviceClient, Guid callerId)
			{
				this.ServiceClient = serviceClient;
				this._lastCallerId = serviceClient.CallerId;
				serviceClient.CallerId = callerId;
			}

			private Guid _lastCallerId;

			public ServiceClient ServiceClient { get; private set; }

            public void Dispose()
			{
				ServiceClient.CallerId = _lastCallerId;
			}
		}

	}
}
