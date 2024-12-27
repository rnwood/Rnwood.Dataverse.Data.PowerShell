using FakeItEasy;
using Microsoft.Extensions.Logging;
using Microsoft.Identity.Client;
using Microsoft.PowerPlatform.Dataverse.Client;
using Microsoft.PowerPlatform.Dataverse.Client.Model;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Metadata;
using System;
using System.Collections.Generic;
using System.IO;
using System.Management.Automation;
using System.Net;
using System.Net.Http;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Threading.Tasks;


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
		private const string PARAMSET_MOCK = "Return a mock connection";

		[Parameter(Mandatory =true, ParameterSetName =PARAMSET_MOCK) ]
		public EntityMetadata[] Mock { get; set; }

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
					case PARAMSET_MOCK:

						FakeXrmEasy.XrmFakedContext xrmFakeContext = new FakeXrmEasy.XrmFakedContext();
						xrmFakeContext.InitializeMetadata(Mock);

						ConstructorInfo contructor = typeof(ServiceClient).GetConstructor(BindingFlags.NonPublic | BindingFlags.Instance, null, new[] { typeof(IOrganizationService), typeof(HttpClient), typeof(string), typeof(Version), typeof(ILogger) }, null);
						result = (ServiceClient)contructor.Invoke(new object[] { xrmFakeContext.GetOrganizationService(), new HttpClient(GetFakeHttpHandler()), "https://fakeorg.crm.dynamics.com", new Version(9, 2), A.Fake<ILogger>() });
						break;

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
							var confApp = ConfidentialClientApplicationBuilder
							.Create(ClientId.ToString())
							.WithRedirectUri("http://localhost")
							.WithClientSecret(ClientSecret)
							.WithAuthority(AadAuthorityAudience.AzureAdMultipleOrgs)
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
			} catch (Exception e)
			{
				WriteError(new ErrorRecord(e, "dataverse-failed-connect", ErrorCategory.ConnectionError, null) { ErrorDetails = new ErrorDetails($"Failed to connect to Dataverse: {e}") });
			}
		}

		private static HttpMessageHandler GetFakeHttpHandler()
		{
			return new FakeHttpMessageHandler();
		}

		class FakeHttpMessageHandler : HttpMessageHandler
		{
			protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
			{
				return Task.FromResult(new HttpResponseMessage(HttpStatusCode.OK) { Content = new StringContent("{}")});
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

			if (authResult == null) {
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
