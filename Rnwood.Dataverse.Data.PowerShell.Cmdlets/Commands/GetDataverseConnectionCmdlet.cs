using Azure.Core;
using Azure.Identity;
using FakeItEasy;
using FakeXrmEasy.Core;
using FakeXrmEasy.Middleware;
using FakeXrmEasy;
using FakeXrmEasy.Abstractions;
using FakeXrmEasy.Abstractions.Enums;
using FakeXrmEasy.Middleware.Crud;
using FakeXrmEasy.Middleware.Messages;
using FakeXrmEasy.FakeMessageExecutors;
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
using System.Security;


namespace Rnwood.Dataverse.Data.PowerShell.Commands
{
	/// <summary>
	/// Establishes a connection to a Dataverse environment using various authentication methods.
	/// </summary>
	[Cmdlet(VerbsCommon.Get, "DataverseConnection")]
	[Alias("Connect-DataverseConnection")]
	[OutputType(typeof(ServiceClient))]
	public class GetDataverseConnectionCmdlet : PSCmdlet
	{
		/// <summary>
		/// Initializes a new instance of the <see cref="GetDataverseConnectionCmdlet"/> class.
		/// </summary>
		public GetDataverseConnectionCmdlet()
		{
		}

		private const string PARAMSET_CLIENTSECRET = "Authenticate with client secret";
		private const string PARAMSET_INTERACTIVE = "Authenticate interactively";
		private const string PARAMSET_DEVICECODE = "Authenticate using the device code flow";
		private const string PARAMSET_USERNAMEPASSWORD = "Authenticate with username and password";
		private const string PARAMSET_CONNECTIONSTRING = "Authenticate with Dataverse SDK connection string.";
		private const string PARAMSET_DEFAULTAZURECREDENTIAL = "Authenticate with DefaultAzureCredential";
		private const string PARAMSET_MANAGEDIDENTITY = "Authenticate with ManagedIdentityCredential";

		private const string PARAMSET_MOCK = "Return a mock connection";
		private const string PARAMSET_GETDEFAULT = "Get default connection";

		/// <summary>
		/// Gets or sets a value indicating whether to get the current default connection.
		/// </summary>
		[Parameter(Mandatory = true, ParameterSetName = PARAMSET_GETDEFAULT, HelpMessage = "Gets the current default connection. Returns an error if no default connection is set.")]
		public SwitchParameter GetDefault { get; set; }

		/// <summary>
		/// Gets or sets a value indicating whether to set this connection as the default.
		/// </summary>
		[Parameter(Mandatory = false, HelpMessage = "When set, this connection will be used as the default for cmdlets that don't have a connection parameter specified.")]
		public SwitchParameter SetAsDefault { get; set; }

		/// <summary>
		/// Gets or sets the entity metadata for creating a mock connection.
		/// </summary>
		[Parameter(Mandatory =true, ParameterSetName =PARAMSET_MOCK, HelpMessage = "Entity metadata for mock connection. Used for testing purposes. Provide entity metadata objects to configure the mock connection with.")] 
		public EntityMetadata[] Mock { get; set; }

		/// <summary>
		/// Gets or sets the client ID to use for authentication.
		/// </summary>
		[Parameter(Mandatory = true, ParameterSetName = PARAMSET_CLIENTSECRET, HelpMessage = "Client ID to use for authentication. By default the MS provided ID for PAC CLI is used to make it easy to get started.")]
		[Parameter(Mandatory = false, ParameterSetName = PARAMSET_INTERACTIVE, HelpMessage = "Client ID to use for authentication. By default the MS provided ID for PAC CLI is used to make it easy to get started.")]
		[Parameter(Mandatory = false, ParameterSetName = PARAMSET_DEVICECODE, HelpMessage = "Client ID to use for authentication. By default the MS provided ID for PAC CLI is used to make it easy to get started.")]
		[Parameter(Mandatory = false, ParameterSetName = PARAMSET_USERNAMEPASSWORD, HelpMessage = "Client ID to use for authentication. By default the MS provided ID for PAC CLI is used to make it easy to get started.")]
		public Guid ClientId { get; set; } = new Guid("9cee029c-6210-4654-90bb-17e6e9d36617");

		/// <summary>
		/// Gets or sets the URL of the Dataverse environment to connect to.
		/// </summary>
		[Parameter(Mandatory = true, ParameterSetName = PARAMSET_CLIENTSECRET, HelpMessage = "URL of the Dataverse environment to connect to. For example https://myorg.crm11.dynamics.com")]
		[Parameter(Mandatory = false, ParameterSetName = PARAMSET_INTERACTIVE, HelpMessage = "URL of the Dataverse environment to connect to. For example https://myorg.crm11.dynamics.com. If not specified, you will be prompted to select from available environments.")]
		[Parameter(Mandatory = false, ParameterSetName = PARAMSET_DEVICECODE, HelpMessage = "URL of the Dataverse environment to connect to. For example https://myorg.crm11.dynamics.com. If not specified, you will be prompted to select from available environments.")]
		[Parameter(Mandatory = false, ParameterSetName = PARAMSET_USERNAMEPASSWORD, HelpMessage = "URL of the Dataverse environment to connect to. For example https://myorg.crm11.dynamics.com. If not specified, you will be prompted to select from available environments.")]
		[Parameter(Mandatory = true, ParameterSetName = PARAMSET_CONNECTIONSTRING, HelpMessage = "URL of the Dataverse environment to connect to. For example https://myorg.crm11.dynamics.com")]
		[Parameter(Mandatory = false, ParameterSetName = PARAMSET_DEFAULTAZURECREDENTIAL, HelpMessage = "URL of the Dataverse environment to connect to. For example https://myorg.crm11.dynamics.com. If not specified, you will be prompted to select from available environments.")]
		[Parameter(Mandatory = false, ParameterSetName = PARAMSET_MANAGEDIDENTITY, HelpMessage = "URL of the Dataverse environment to connect to. For example https://myorg.crm11.dynamics.com. If not specified, you will be prompted to select from available environments.")]
		[Parameter(Mandatory = true, ParameterSetName = PARAMSET_MOCK, HelpMessage = "URL of the Dataverse environment to connect to. For example https://myorg.crm11.dynamics.com")]
		public Uri Url { get; set; }

		/// <summary>
		/// Gets or sets the client secret for authentication.
		/// </summary>
		[Parameter(Mandatory = true, ParameterSetName = PARAMSET_CLIENTSECRET, HelpMessage = "Client secret to authenticate with, as registered for the Entra ID application.")]
		public string ClientSecret { get; set; }

		/// <summary>
		/// Gets or sets the username for authentication.
		/// </summary>
		[Parameter(Mandatory = false, ParameterSetName = PARAMSET_INTERACTIVE, HelpMessage = "Username to authenticate with.")]
		[Parameter(Mandatory = false, ParameterSetName = PARAMSET_DEVICECODE, HelpMessage = "Username to authenticate with.")]
		[Parameter(Mandatory = true, ParameterSetName = PARAMSET_USERNAMEPASSWORD, HelpMessage = "Username to authenticate with.")]
		public string Username { get; set; }

		/// <summary>
		/// Gets or sets the password for authentication.
		/// </summary>
		[Parameter(Mandatory = true, ParameterSetName = PARAMSET_USERNAMEPASSWORD, HelpMessage = "Password to authenticate with.")]
		public string Password { get; set; }

		/// <summary>
		/// Gets or sets a value indicating whether to use interactive authentication.
		/// </summary>
		[Parameter(Mandatory = true, ParameterSetName = PARAMSET_INTERACTIVE, HelpMessage = "Triggers interactive authentication, where browser will be opened for user to interactively log in.")]
		public SwitchParameter Interactive { get; set; }

		/// <summary>
		/// Gets or sets a value indicating whether to use device code authentication.
		/// </summary>
		[Parameter(Mandatory = true, ParameterSetName = PARAMSET_DEVICECODE, HelpMessage = "Triggers device code authentication where you will be given a URL to visit and a code to complete authentication in web browser.")]
		public SwitchParameter DeviceCode { get; set; }

		/// <summary>
		/// Gets or sets the connection string for authentication.
		/// </summary>
		[Parameter(Mandatory = true, ParameterSetName = PARAMSET_CONNECTIONSTRING, HelpMessage = "Specifies the connection string to authenticate with - see https://learn.microsoft.com/en-us/power-apps/developer/data-platform/xrm-tooling/use-connection-strings-xrm-tooling-connect")]
		public string ConnectionString { get; set; }

		/// <summary>
		/// Gets or sets a value indicating whether to use DefaultAzureCredential for authentication.
		/// </summary>
		[Parameter(Mandatory = true, ParameterSetName = PARAMSET_DEFAULTAZURECREDENTIAL, HelpMessage = "Use DefaultAzureCredential for authentication. This will try multiple authentication methods in order: environment variables, managed identity, Visual Studio, Azure CLI, Azure PowerShell, and interactive browser.")]
		public SwitchParameter DefaultAzureCredential { get; set; }

		/// <summary>
		/// Gets or sets a value indicating whether to use managed identity for authentication.
		/// </summary>
		[Parameter(Mandatory = true, ParameterSetName = PARAMSET_MANAGEDIDENTITY, HelpMessage = "Use ManagedIdentityCredential for authentication. Authenticates using the managed identity assigned to the Azure resource.")]
		public SwitchParameter ManagedIdentity { get; set; }

		/// <summary>
		/// Gets or sets the client ID of the user-assigned managed identity.
		/// </summary>
		[Parameter(Mandatory = false, ParameterSetName = PARAMSET_MANAGEDIDENTITY, HelpMessage = "Client ID of the user-assigned managed identity. If not specified, the system-assigned managed identity will be used.")]
		public string ManagedIdentityClientId { get; set; }

		/// <summary>
		/// Gets or sets the timeout for authentication operations in seconds.
		/// </summary>
		[Parameter(Mandatory = false, HelpMessage = "Timeout for authentication operations. Defaults to 5 minutes.")]
		public uint Timeout { get; set; } = 5*60;

		// Cancellation token source that is cancelled when the user hits Ctrl+C (StopProcessing)
		private CancellationTokenSource _userCancellationCts;

		/// <summary>
		/// Initializes the cmdlet processing.
		/// </summary>
		protected override void BeginProcessing()
		{
			base.BeginProcessing();
			// initialize cancellation token source for this pipeline invocation
			_userCancellationCts = new CancellationTokenSource();
		}

		/// <summary>
		/// Called when the user cancels the cmdlet.
		/// </summary>
		protected override void StopProcessing()
		{
			// Called when user presses Ctrl+C. Signal cancellation to any ongoing operations.
			try
			{
				_userCancellationCts?.Cancel();
			}
			catch { }
			base.StopProcessing();
		}

		/// <summary>
		/// Completes cmdlet processing.
		/// </summary>
		protected override void EndProcessing()
		{
			base.EndProcessing();
			_userCancellationCts?.Dispose();
			_userCancellationCts = null;
		}

		private CancellationTokenSource CreateLinkedCts(TimeSpan timeout)
		{
			var timeoutCts = new CancellationTokenSource(timeout);
			return CancellationTokenSource.CreateLinkedTokenSource(_userCancellationCts?.Token ?? CancellationToken.None, timeoutCts.Token);
		}

		/// <summary>
		/// Processes each record in the pipeline.
		/// </summary>
		protected override void ProcessRecord()
		{
			// Handle GetDefault specially to avoid wrapping its error
			if (ParameterSetName == PARAMSET_GETDEFAULT)
			{
				base.ProcessRecord();
				ServiceClient result = DefaultConnectionManager.DefaultConnection;
				if (result == null)
				{
					ThrowTerminatingError(new ErrorRecord(
						new InvalidOperationException("No default connection has been set. Use Get-DataverseConnection with -SetAsDefault or provide a -Connection parameter to cmdlets."),
						"NoDefaultConnection",
						ErrorCategory.InvalidOperation,
						null));
				}
				WriteObject(result);
				return;
			}

			try
			{
				base.ProcessRecord();

				

				ServiceClient result;

				switch (ParameterSetName)
				{
					case PARAMSET_MOCK:

						IXrmFakedContext xrmFakeContext = MiddlewareBuilder
                        .New()
                        .AddCrud()
						.AddFakeMessageExecutors(Assembly.GetAssembly(typeof(FakeXrmEasy.FakeMessageExecutors.RetrieveEntityRequestExecutor)))
						.UseMessages()
                        .UseCrud()
                        .SetLicense(FakeXrmEasyLicense.RPL_1_5)
                        .Build();
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

							// If URL is not provided, discover and select environment
							if (Url == null)
							{
								var discoveryUrl = DiscoverAndSelectEnvironment(publicClient).GetAwaiter().GetResult();
								Url = new Uri(discoveryUrl);
							}

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

							// If URL is not provided, discover and select environment
							if (Url == null)
							{
								var discoveryUrl = DiscoverAndSelectEnvironment(publicClient).GetAwaiter().GetResult();
								Url = new Uri(discoveryUrl);
							}

							result = new ServiceClient(Url, url => GetTokenWithUsernamePassword(publicClient, url));

							break;
						}

					case PARAMSET_DEVICECODE:
						{
							var publicClient = PublicClientApplicationBuilder
								.Create(ClientId.ToString())
								.WithRedirectUri("http://localhost")
								.Build();

							// If URL is not provided, discover and select environment
							if (Url == null)
							{
								var discoveryUrl = DiscoverAndSelectEnvironment(publicClient).GetAwaiter().GetResult();
								Url = new Uri(discoveryUrl);
							}

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

					case PARAMSET_DEFAULTAZURECREDENTIAL:
						{
							var credential = new Azure.Identity.DefaultAzureCredential();
							
							// If URL is not provided, discover and select environment
							if (Url == null)
							{
								// For DefaultAzureCredential, we need to use interactive flow for discovery
								var publicClient = PublicClientApplicationBuilder
									.Create(ClientId.ToString())
									.WithRedirectUri("http://localhost")
									.Build();
								var discoveryUrl = DiscoverAndSelectEnvironment(publicClient).GetAwaiter().GetResult();
								Url = new Uri(discoveryUrl);
							}
							
							result = new ServiceClient(Url, url => GetTokenWithAzureCredential(credential, url));

							break;
						}

					case PARAMSET_MANAGEDIDENTITY:
						{
							TokenCredential credential;
							if (!string.IsNullOrEmpty(ManagedIdentityClientId))
							{
								credential = new ManagedIdentityCredential(ManagedIdentityClientId);
							}
							else
							{
								credential = new ManagedIdentityCredential();
							}
							
							// If URL is not provided, discover and select environment
							if (Url == null)
							{
								// For ManagedIdentity, we need to use interactive flow for discovery
								var publicClient = PublicClientApplicationBuilder
									.Create(ClientId.ToString())
									.WithRedirectUri("http://localhost")
									.Build();
								var discoveryUrl = DiscoverAndSelectEnvironment(publicClient).GetAwaiter().GetResult();
								Url = new Uri(discoveryUrl);
							}
							
							result = new ServiceClient(Url, url => GetTokenWithAzureCredential(credential, url));

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

				// Set as default if requested
				if (SetAsDefault)
				{
					DefaultConnectionManager.DefaultConnection = result;
				}

				WriteObject(result);
			}
			catch (Exception e)
			{
				// If cancellation was requested, throw a terminating PipelineStoppedException so PowerShell treats this as an interrupted operation
				if (e is OperationCanceledException || e is TaskCanceledException || (_userCancellationCts != null && _userCancellationCts.IsCancellationRequested))
				{
					ThrowTerminatingError(new ErrorRecord(new PipelineStoppedException(), "OperationStopped", ErrorCategory.OperationStopped, null));
				}

				// If it's already a PipelineStoppedException (from ThrowTerminatingError), rethrow it
				if (e is PipelineStoppedException)
				{
					throw;
				}

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

				using (var cts = CreateLinkedCts(TimeSpan.FromSeconds(Timeout)))
				{
					HttpResponseMessage httpResponse = httpClient.SendAsync(httpRequestMessage, cts.Token).GetAwaiter().GetResult();
					var header = httpResponse.Headers.GetValues("WWW-Authenticate").First();

					//Bearer authorization_uri=https://login.microsoftonline.com/bd6c851f-e0dc-4d6d-ab4c-99452fe28387/oauth2/authorize, resource_id=https://orgxyz.crm11.dynamics.com/
					var match = Regex.Match(header, ".*authorization_uri=([^ ,]+).*");
					var authUri = match.Groups[1].Value;

					authority = authUri.Replace("/oauth2/authorize", "");
				}

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
				if (cancellationToken.IsCancellationRequested)
				{
					return Task.FromCanceled<HttpResponseMessage>(cancellationToken);
				}

				return Task.FromResult(new HttpResponseMessage(HttpStatusCode.OK) { Content = new StringContent("{}") });
			}
		}

		private async Task<string> GetTokenWithClientSecret(IConfidentialClientApplication app, string url)
		{
			Uri scope = new Uri(Url, "/.default");
			string[] scopes = new[] { scope.ToString() };

			using (var cts = CreateLinkedCts(TimeSpan.FromSeconds(Timeout)))
			{
				AuthenticationResult authResult = await app.AcquireTokenForClient(scopes).ExecuteAsync(cts.Token);
				return authResult.AccessToken;
			}
		}

		private async Task<string> GetTokenWithUsernamePassword(IPublicClientApplication app, string url)
		{
			Uri scope = new Uri(Url, "/.default");
			string[] scopes = new[] { scope.ToString() };

			using (var cts = CreateLinkedCts(TimeSpan.FromSeconds(Timeout)))
			{
				AuthenticationResult authResult = null;
				
				// Try to get token silently from cache first
				if (!string.IsNullOrEmpty(Username))
				{
					try
					{
						authResult = await app.AcquireTokenSilent(scopes, Username).ExecuteAsync(cts.Token);
					}
					catch (MsalUiRequiredException)
					{
						// Token cache miss or expired, need to acquire new token
					}
					catch (MsalServiceException)
					{
						// Service error during silent acquisition, need to acquire new token
					}
				}

				// If silent acquisition failed, acquire new token with username/password
				if (authResult == null)
				{
					authResult = await app.AcquireTokenByUsernamePassword(scopes, Username, new NetworkCredential("", Password).SecurePassword).ExecuteAsync(cts.Token);
				}

				return authResult.AccessToken;
			}
		}

		private async Task<string> GetTokenWithDeviceCode(IPublicClientApplication app, string url)
		{
			Uri scope = new Uri(Url, "/.default");
			string[] scopes = new[] { scope.ToString() };

			using (var cts = CreateLinkedCts(TimeSpan.FromSeconds(Timeout)))
			{
				AuthenticationResult authResult = null;
				if (!string.IsNullOrEmpty(Username))
				{
					try
					{
						authResult = await app.AcquireTokenSilent(scopes, Username).ExecuteAsync(cts.Token);
					}
					catch (MsalUiRequiredException)
					{

					}
				}

				if (authResult == null)
				{
					authResult = await app.AcquireTokenWithDeviceCode(scopes, (dcr) =>
					{
						if (cts.Token.IsCancellationRequested)
						{
							return Task.FromCanceled(cts.Token);
						}
						Host.UI.WriteLine(dcr.Message);
						return Task.CompletedTask;
					}).ExecuteAsync(cts.Token);
				}

				Username = authResult.Account.Username;

				return authResult.AccessToken;
			}
		}

		private async Task<string> GetTokenInteractive(IPublicClientApplication app, string url)
		{
			Uri scope = new Uri(Url, "/.default");
			string[] scopes = new[] { scope.ToString() };

			using (var cts = CreateLinkedCts(TimeSpan.FromSeconds(Timeout)))
			{
				AuthenticationResult authResult = null;

				if (!string.IsNullOrEmpty(Username))
				{
					try
					{
						authResult = await app.AcquireTokenSilent(scopes, Username).ExecuteAsync(cts.Token);
					}
					catch (MsalUiRequiredException)
					{

					}
				}

				if (authResult == null)
				{
					authResult = await app.AcquireTokenInteractive(scopes).ExecuteAsync(cts.Token);
					Username = authResult.Account.Username;
				}

				return authResult.AccessToken;
			}
		}

		private async Task<string> GetTokenWithAzureCredential(TokenCredential credential, string url)
		{
			Uri scope = new Uri(Url, "/.default");
			var tokenRequestContext = new TokenRequestContext(new[] { scope.ToString() });

			using (var cts = CreateLinkedCts(TimeSpan.FromSeconds(Timeout)))
			{
				var token = await credential.GetTokenAsync(tokenRequestContext, cts.Token);
				return token.Token;
			}
		}

		private async Task<string> DiscoverAndSelectEnvironment(IPublicClientApplication app)
		{
			// Authenticate interactively to get access token for discovery
			Uri discoveryScope = new Uri("https://globaldisco.crm.dynamics.com/.default");
			string[] scopes = new[] { discoveryScope.ToString() };

			using (var cts = CreateLinkedCts(TimeSpan.FromSeconds(Timeout)))
			{
				AuthenticationResult authResult = null;

				if (!string.IsNullOrEmpty(Username))
				{
					try
					{
						authResult = await app.AcquireTokenSilent(scopes, Username).ExecuteAsync(cts.Token);
					}
					catch (MsalUiRequiredException)
					{
					}
				}

				if (authResult == null)
				{
					authResult = await app.AcquireTokenInteractive(scopes).ExecuteAsync(cts.Token);
					Username = authResult.Account.Username;
				}

				// Use ServiceClient.DiscoverOnlineOrganizationsAsync to get list of environments
				var orgDetails = await ServiceClient.DiscoverOnlineOrganizationsAsync(
					(uri) => Task.FromResult(authResult.AccessToken),
					new Uri("https://globaldisco.crm.dynamics.com"),
					null,
					null,
					cts.Token);

				if (orgDetails == null || orgDetails.Count == 0)
				{
					throw new Exception("No Dataverse environments found for this user.");
				}

				// Display available environments and let user select
				Host.UI.WriteLine("Available Dataverse environments:");
				Host.UI.WriteLine("");

				var orgList = orgDetails.ToList();
				for (int i = 0; i < orgList.Count; i++)
				{
					var org = orgList[i];
					Host.UI.WriteLine($"  {i + 1}. {org.FriendlyName} ({org.UniqueName})");
					Host.UI.WriteLine($"      URL: {org.Endpoints[Microsoft.Xrm.Sdk.Discovery.EndpointType.WebApplication]}");
					Host.UI.WriteLine("");
				}

				// Prompt for selection
				int selection = -1;
				while (selection < 1 || selection > orgList.Count)
				{
					try
					{
						Host.UI.Write("Select environment (1-" + orgList.Count + "): ");
						var input = Host.UI.ReadLine();
						selection = int.Parse(input);

						if (selection < 1 || selection > orgList.Count)
						{
							Host.UI.WriteLine("Invalid selection. Please enter a number between 1 and " + orgList.Count);
						}
					}
					catch
					{
						Host.UI.WriteLine("Invalid input. Please enter a number.");
						selection = -1;
					}
				}

				var selectedOrg = orgList[selection - 1];
				var url = selectedOrg.Endpoints[Microsoft.Xrm.Sdk.Discovery.EndpointType.WebApplication];

				Host.UI.WriteLine($"Selected environment: {selectedOrg.FriendlyName} ({url})");

				return url;
			}
		}
	}

}
