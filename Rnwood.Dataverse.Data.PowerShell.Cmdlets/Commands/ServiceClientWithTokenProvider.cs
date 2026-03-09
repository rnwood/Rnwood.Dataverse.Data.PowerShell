using System;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.PowerPlatform.Dataverse.Client;

namespace Rnwood.Dataverse.Data.PowerShell.Commands
{
/// <summary>
/// Wrapper for ServiceClient that exposes the token provider function used for authentication.
/// This is needed to support TDS endpoint with external token management.
/// </summary>
internal class ServiceClientWithTokenProvider : ServiceClient
{
/// <summary>
/// Initializes a new instance of ServiceClientWithTokenProvider with a token provider function.
/// </summary>
/// <param name="instanceUrl">The URL of the Dataverse instance</param>
/// <param name="tokenProviderFunction">Function that provides access tokens</param>
public ServiceClientWithTokenProvider(Uri instanceUrl, Func<string, Task<string>> tokenProviderFunction) 
: base(instanceUrl, tokenProviderFunction)
{
TokenProviderFunction = tokenProviderFunction;
}

/// <summary>
/// Gets the token provider function used for authentication.
/// This can be used to retrieve access tokens for TDS endpoint or other purposes.
/// </summary>
public Func<string, Task<string>> TokenProviderFunction { get; }
}
}
