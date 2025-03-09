using Microsoft.Extensions.Logging;
using Microsoft.Identity.Client;
using Microsoft.PowerPlatform.Dataverse.Client;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Metadata;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Management.Automation;
using System.Net;
using System.Net.Http;
using System.Reflection;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using AuthenticationResult = Microsoft.Identity.Client.AuthenticationResult;


namespace Rnwood.Dataverse.Data.PowerShell.Commands
{
	[Cmdlet(VerbsCommon.Get, "DataverseConnection")]
	[OutputType(typeof(ServiceClient))]
	public class GetDataverseConnectionCmdlet : PSCmdlet
	{
		public GetDataverseConnectionCmdlet()
		{
		}

		private const string PARAMSET_CLIENTSECRET = "Authenticate with client secret";
		private const string PARAMSET_INTERACTIVE = "Authenticate interactively";
		private const string PARAMSET_DEVICECODE = "Authenticate using the device code flow";
		private const string PARAMSET_USERNAMEPASSWORD = "Authenticate with username and password";
		private const string PARAMSET_CONNECTIONSTRING = "Authenticate with Dataverse SDK connection string.";

		[Parameter(Mandatory = true, ParameterSetName = PARAMSET_CLIENTSECRET)]
		[Parameter(Mandatory = false, ParameterSetName = PARAMSET_INTERACTIVE)]
		[Parameter(Mandatory = false, ParameterSetName = PARAMSET_DEVICECODE)]
		[Parameter(Mandatory = false, ParameterSetName = PARAMSET_USERNAMEPASSWORD)]
		public Guid ClientId { get; set; } = new Guid("9cee029c-6210-4654-90bb-17e6e9d36617");

		[Parameter(Mandatory = true)]
		public Uri Url { get; set; }

		[Parameter(Mandatory = true, ParameterSetName = PARAMSET_CLIENTSECRET)]
		public string ClientSecret { get; set; }

		[Parameter(Mandatory = false, ParameterSetName = PARAMSET_INTERACTIVE)]
		[Parameter(Mandatory = false, ParameterSetName = PARAMSET_DEVICECODE)]
		[Parameter(Mandatory = true, ParameterSetName = PARAMSET_USERNAMEPASSWORD)]
		public string Username { get; set; }

		[Parameter(Mandatory = true, ParameterSetName = PARAMSET_USERNAMEPASSWORD)]
		public string Password { get; set; }

		[Parameter(Mandatory = true, ParameterSetName = PARAMSET_INTERACTIVE)]
		public SwitchParameter Interactive { get; set; }

		[Parameter(Mandatory = true, ParameterSetName = PARAMSET_DEVICECODE)]
		public SwitchParameter DeviceCode { get; set; }

		[Parameter(Mandatory = true, ParameterSetName = PARAMSET_CONNECTIONSTRING)]
		public string ConnectionString { get; set; }


		protected override void BeginProcessing()
		{
			try
			{
				base.BeginProcessing();


				ServiceClient result;

				switch (ParameterSetName)
				{

					case PARAMSET_CONNECTIONSTRING:
						result = new ServiceClient(ConnectionString);
						break;

					case PARAMSET_INTERACTIVE:
						{
							var publicClient = PublicClientApplicationBuilder
								.Create(ClientId.ToString())
								.WithRedirectUri("http://localhost")
								.Build();

							result = new ServiceClient(Url, url => GetTokenInteractive(publicClient, url));

							break;
						}

					case PARAMSET_USERNAMEPASSWORD:
						{
							var publicClient = PublicClientApplicationBuilder
								.Create(ClientId.ToString())
								.WithRedirectUri("http://localhost")
								.WithAuthority(AadAuthorityAudience.AzureAdMultipleOrgs)
								.Build();

							result = new ServiceClient(Url, url => GetTokenWithUsernamePassword(publicClient, url));

							break;
						}

					case PARAMSET_DEVICECODE:
						{
							var publicClient = PublicClientApplicationBuilder
								.Create(ClientId.ToString())
								.WithRedirectUri("http://localhost")
								.Build();

							result = new ServiceClient(Url, url => GetTokenWithDeviceCode(publicClient, url));

							break;
						}

					case PARAMSET_CLIENTSECRET:
						{
							string authority = GetAuthority();

							var confApp = ConfidentialClientApplicationBuilder
							.Create(ClientId.ToString())
							.WithRedirectUri("http://localhost")
							.WithClientSecret(ClientSecret)
							.WithAuthority(authority)
							.Build();

							result = new ServiceClient(Url, url => GetTokenWithClientSecret(confApp, url));

							break;
						}

					default:
						throw new NotImplementedException(ParameterSetName);
				}

				result.EnableAffinityCookie = false;

				// Bump up the min threads reserved for this app to ramp connections faster - minWorkerThreads defaults to 4, minIOCP defaults to 4 
				ThreadPool.SetMinThreads(100, 100);
				// Change max connections from .NET to a remote service default: 2
				System.Net.ServicePointManager.DefaultConnectionLimit = 65000;
				// Turn off the Expect 100 to continue message - 'true' will cause the caller to wait until it round-trip confirms a connection to the server 
				System.Net.ServicePointManager.Expect100Continue = false;
				// Can decrease overall transmission overhead but can cause delay in data packet arrival
				System.Net.ServicePointManager.UseNagleAlgorithm = false;

				WriteObject(result);
			}
			catch (Exception e)
			{
				WriteError(new ErrorRecord(e, "dataverse-failed-connect", ErrorCategory.ConnectionError, null) { ErrorDetails = new ErrorDetails($"Failed to connect to Dataverse: {e}") });
			}
		}

		private string GetAuthority()
		{
			string authority;
			using (HttpClient httpClient = new HttpClient())
			{

				HttpRequestMessage httpRequestMessage = new HttpRequestMessage(HttpMethod.Get,
Url + "/api/data/v9.2/");

				HttpResponseMessage httpResponse = httpClient.SendAsync(httpRequestMessage).Result;
				var header = httpResponse.Headers.GetValues("WWW-Authenticate").First();

				//Bearer authorization_uri=https://login.microsoftonline.com/bd6c851f-e0dc-4d6d-ab4c-99452fe28387/oauth2/authorize, resource_id=https://orgxyz.crm11.dynamics.com/
				var match = Regex.Match(header, ".*authorization_uri=([^ ,]+).*");
				var authUri = match.Groups[1].Value;

				authority = authUri.Replace("/oauth2/authorize", "");

			}

			return authority;
		}

		private static HttpMessageHandler GetFakeHttpHandler()
		{
			return new FakeHttpMessageHandler();
		}

		class FakeHttpMessageHandler : HttpMessageHandler
		{
			protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
			{
				return Task.FromResult(new HttpResponseMessage(HttpStatusCode.OK) { Content = new StringContent("{}") });
			}
		}

		private async Task<string> GetTokenWithClientSecret(IConfidentialClientApplication app, string url)
		{
			Uri scope = new Uri(Url, "/.default");
			string[] scopes = new[] { scope.ToString() };


			AuthenticationResult authResult = await app.AcquireTokenForClient(scopes).ExecuteAsync();
			return authResult.AccessToken;
		}

		private async Task<string> GetTokenWithUsernamePassword(IPublicClientApplication app, string url)
		{
			Uri scope = new Uri(Url, "/.default");
			string[] scopes = new[] { scope.ToString() };

			AuthenticationResult authResult = await app.AcquireTokenByUsernamePassword(scopes, Username, Password).ExecuteAsync();


			return authResult.AccessToken;

		}

		private async Task<string> GetTokenWithDeviceCode(IPublicClientApplication app, string url)
		{
			Uri scope = new Uri(Url, "/.default");
			string[] scopes = new[] { scope.ToString() };

			AuthenticationResult authResult = null;
			if (!string.IsNullOrEmpty(Username))
			{
				try
				{

					authResult = await app.AcquireTokenSilent(scopes, Username).ExecuteAsync();
				}
				catch (MsalUiRequiredException)
				{

				}
			}

			if (authResult == null)
			{
				authResult = await app.AcquireTokenWithDeviceCode(scopes, (dcr) =>
				{
					Host.UI.WriteLine(dcr.Message);
					return Task.FromResult(0);
				}).ExecuteAsync();
			}

			Username = authResult.Account.Username;

			return authResult.AccessToken;

		}

		private async Task<string> GetTokenInteractive(IPublicClientApplication app, string url)
		{
			Uri scope = new Uri(Url, "/.default");
			string[] scopes = new[] { scope.ToString() };


			AuthenticationResult authResult = null;

			if (!string.IsNullOrEmpty(Username))
			{
				try
				{

					authResult = await app.AcquireTokenSilent(scopes, Username).ExecuteAsync();
				}
				catch (MsalUiRequiredException)
				{

				}
			}

			if (authResult == null)
			{
				authResult = await app.AcquireTokenInteractive(scopes).ExecuteAsync();
				Username = authResult.Account.Username;
			}


			return authResult.AccessToken;

		}
	}


}
