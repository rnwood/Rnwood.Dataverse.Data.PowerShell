using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace Rnwood.Dataverse.Data.PowerShell.Tests.Infrastructure
{
    /// <summary>
    /// Fake HTTP message handler that returns empty responses for any HTTP requests.
    /// Used to create mock ServiceClient instances.
    /// </summary>
    internal class FakeHttpMessageHandler : HttpMessageHandler
    {
        protected override Task<HttpResponseMessage> SendAsync(
            HttpRequestMessage request,
            CancellationToken cancellationToken)
        {
            return Task.FromResult(new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent("{}")
            });
        }
    }
}
