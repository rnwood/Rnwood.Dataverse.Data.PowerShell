﻿using Azure.Core;
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
using System.Collections;
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
using System.Security.Cryptography.X509Certificates;
using System.IO;
using System.Text.Json;


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
		private const string PARAMSET_CLIENTCERTIFICATE = "Authenticate with client certificate";
		private const string PARAMSET_INTERACTIVE = "Authenticate interactively";
		private const string PARAMSET_DEVICECODE = "Authenticate using the device code flow";
		private const string PARAMSET_USERNAMEPASSWORD = "Authenticate with username and password";
		private const string PARAMSET_CONNECTIONSTRING = "Authenticate with Dataverse SDK connection string.";
		private const string PARAMSET_DEFAULTAZURECREDENTIAL = "Authenticate with DefaultAzureCredential";
		private const string PARAMSET_MANAGEDIDENTITY = "Authenticate with ManagedIdentityCredential";
		private const string PARAMSET_ACCESSTOKEN = "Authenticate with access token script block";

		private const string PARAMSET_MOCK = "Return a mock connection";
		private const string PARAMSET_GETDEFAULT = "Get default connection";
		private const string PARAMSET_LOADNAMED = "Load a saved named connection";
		private const string PARAMSET_LISTNAMED = "List saved named connections";
		private const string PARAMSET_DELETENAMED = "Delete a saved named connection";
		private const string PARAMSET_CLEARALL = "Clear all saved connections";
		private const string PARAMSET_FROMPAC = "Load connection from PAC CLI profile";

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
		/// Gets or sets a value indicating whether to save credentials/secrets with the connection (NOT RECOMMENDED).
		/// </summary>
		[Parameter(Mandatory = false, ParameterSetName = PARAMSET_USERNAMEPASSWORD, HelpMessage = "WARNING: Saves the password with the connection (encrypted). This is NOT RECOMMENDED for security reasons. Only use for testing or non-production scenarios.")]
		[Parameter(Mandatory = false, ParameterSetName = PARAMSET_CLIENTSECRET, HelpMessage = "WARNING: Saves the client secret with the connection (encrypted). This is NOT RECOMMENDED for security reasons. Only use for testing or non-production scenarios.")]
		[Parameter(Mandatory = false, ParameterSetName = PARAMSET_CLIENTCERTIFICATE, HelpMessage = "WARNING: Saves certificate path and password with the connection (encrypted). This is NOT RECOMMENDED for security reasons. Only use for testing or non-production scenarios.")]
		public SwitchParameter SaveCredentials { get; set; }

		/// <summary>
		/// Gets or sets the name of the connection to save or load.
		/// </summary>
		[Parameter(Mandatory = false, ParameterSetName = PARAMSET_INTERACTIVE, HelpMessage = "Name to save this connection under for later retrieval. Allows you to persist and reuse connections.")]
		[Parameter(Mandatory = false, ParameterSetName = PARAMSET_DEVICECODE, HelpMessage = "Name to save this connection under for later retrieval. Allows you to persist and reuse connections.")]
		[Parameter(Mandatory = false, ParameterSetName = PARAMSET_USERNAMEPASSWORD, HelpMessage = "Name to save this connection under for later retrieval. Allows you to persist and reuse connections.")]
		[Parameter(Mandatory = false, ParameterSetName = PARAMSET_CLIENTSECRET, HelpMessage = "Name to save this connection under for later retrieval. Allows you to persist and reuse connections.")]
		[Parameter(Mandatory = false, ParameterSetName = PARAMSET_CLIENTCERTIFICATE, HelpMessage = "Name to save this connection under for later retrieval. Allows you to persist and reuse connections.")]
		[Parameter(Mandatory = false, ParameterSetName = PARAMSET_DEFAULTAZURECREDENTIAL, HelpMessage = "Name to save this connection under for later retrieval. Allows you to persist and reuse connections.")]
		[Parameter(Mandatory = false, ParameterSetName = PARAMSET_MANAGEDIDENTITY, HelpMessage = "Name to save this connection under for later retrieval. Allows you to persist and reuse connections.")]
		[Parameter(Mandatory = true, ParameterSetName = PARAMSET_LOADNAMED, HelpMessage = "Name of a saved connection to load. The connection will be restored with cached credentials.")]
		[Parameter(Mandatory = true, ParameterSetName = PARAMSET_DELETENAMED, HelpMessage = "Name of the saved connection to delete.")]
		public string Name { get; set; }

		/// <summary>
		/// Gets or sets a value indicating whether to delete the named connection.
		/// </summary>
		[Parameter(Mandatory = true, ParameterSetName = PARAMSET_DELETENAMED, HelpMessage = "Deletes a saved named connection. Use with -Name to specify which connection to delete.")]
		public SwitchParameter DeleteConnection { get; set; }

		/// <summary>
		/// Gets or sets a value indicating whether to clear all saved connections.
		/// </summary>
		[Parameter(Mandatory = true, ParameterSetName = PARAMSET_CLEARALL, HelpMessage = "Clears all saved named connections and cached tokens.")]
		public SwitchParameter ClearAllConnections { get; set; }

		/// <summary>
		/// Gets or sets a value indicating whether to list all saved connections.
		/// </summary>
		[Parameter(Mandatory = true, ParameterSetName = PARAMSET_LISTNAMED, HelpMessage = "Lists all saved named connections.")]
		public SwitchParameter ListConnections { get; set; }

		/// <summary>
		/// Gets or sets the entity metadata for creating a mock connection.
		/// </summary>
		[Parameter(Mandatory =true, ParameterSetName =PARAMSET_MOCK, HelpMessage = "Entity metadata for mock connection. Used for testing purposes. Provide entity metadata objects to configure the mock connection with.")] 
		public EntityMetadata[] Mock { get; set; }

		/// <summary>
		/// Gets or sets the ScriptBlock to intercept requests for testing purposes.
		/// </summary>
		[Parameter(Mandatory = false, ParameterSetName = PARAMSET_MOCK, HelpMessage = "ScriptBlock to intercept and modify requests. The ScriptBlock receives the OrganizationRequest and can throw exceptions or return modified responses.")]
		public ScriptBlock RequestInterceptor { get; set; }

		/// <summary>
		/// Gets or sets the client ID to use for authentication.
		/// </summary>
		[Parameter(Mandatory = true, ParameterSetName = PARAMSET_CLIENTSECRET, HelpMessage = "Client ID to use for authentication. By default the MS provided ID for PAC CLI is used to make it easy to get started.")]
		[Parameter(Mandatory = true, ParameterSetName = PARAMSET_CLIENTCERTIFICATE, HelpMessage = "Client ID to use for authentication. Required for client certificate authentication.")]
		[Parameter(Mandatory = false, ParameterSetName = PARAMSET_INTERACTIVE, HelpMessage = "Client ID to use for authentication. By default the MS provided ID for PAC CLI is used to make it easy to get started.")]
		[Parameter(Mandatory = false, ParameterSetName = PARAMSET_DEVICECODE, HelpMessage = "Client ID to use for authentication. By default the MS provided ID for PAC CLI is used to make it easy to get started.")]
		[Parameter(Mandatory = false, ParameterSetName = PARAMSET_USERNAMEPASSWORD, HelpMessage = "Client ID to use for authentication. By default the MS provided ID for PAC CLI is used to make it easy to get started.")]
		public Guid ClientId { get; set; } = new Guid("9cee029c-6210-4654-90bb-17e6e9d36617");

		/// <summary>
		/// Gets or sets the URL of the Dataverse environment to connect to.
		/// </summary>
		[Parameter(Mandatory = true, ParameterSetName = PARAMSET_CLIENTSECRET, HelpMessage = "URL of the Dataverse environment to connect to. For example https://myorg.crm11.dynamics.com")]
		[Parameter(Mandatory = true, ParameterSetName = PARAMSET_CLIENTCERTIFICATE, HelpMessage = "URL of the Dataverse environment to connect to. For example https://myorg.crm11.dynamics.com")]
		[Parameter(Mandatory = false, ParameterSetName = PARAMSET_INTERACTIVE, HelpMessage = "URL of the Dataverse environment to connect to. For example https://myorg.crm11.dynamics.com. If not specified, you will be prompted to select from available environments.")]
		[Parameter(Mandatory = false, ParameterSetName = PARAMSET_DEVICECODE, HelpMessage = "URL of the Dataverse environment to connect to. For example https://myorg.crm11.dynamics.com. If not specified, you will be prompted to select from available environments.")]
		[Parameter(Mandatory = false, ParameterSetName = PARAMSET_USERNAMEPASSWORD, HelpMessage = "URL of the Dataverse environment to connect to. For example https://myorg.crm11.dynamics.com. If not specified, you will be prompted to select from available environments.")]
		[Parameter(Mandatory = true, ParameterSetName = PARAMSET_CONNECTIONSTRING, HelpMessage = "URL of the Dataverse environment to connect to. For example https://myorg.crm11.dynamics.com")]
		[Parameter(Mandatory = false, ParameterSetName = PARAMSET_DEFAULTAZURECREDENTIAL, HelpMessage = "URL of the Dataverse environment to connect to. For example https://myorg.crm11.dynamics.com. If not specified, you will be prompted to select from available environments.")]
		[Parameter(Mandatory = false, ParameterSetName = PARAMSET_MANAGEDIDENTITY, HelpMessage = "URL of the Dataverse environment to connect to. For example https://myorg.crm11.dynamics.com. If not specified, you will be prompted to select from available environments.")]
		[Parameter(Mandatory = true, ParameterSetName = PARAMSET_MOCK, HelpMessage = "URL of the Dataverse environment to connect to. For example https://myorg.crm11.dynamics.com")]
		[Parameter(Mandatory = true, ParameterSetName = PARAMSET_ACCESSTOKEN, HelpMessage = "URL of the Dataverse environment to connect to. For example https://myorg.crm11.dynamics.com")]
		public Uri Url { get; set; }

		/// <summary>
		/// Gets or sets the client secret for authentication.
		/// </summary>
		[Parameter(Mandatory = true, ParameterSetName = PARAMSET_CLIENTSECRET, HelpMessage = "Client secret to authenticate with, as registered for the Entra ID application.")]
		public string ClientSecret { get; set; }

		/// <summary>
		/// Gets or sets the path to the client certificate file (.pfx or .p12).
		/// </summary>
		[Parameter(Mandatory = true, ParameterSetName = PARAMSET_CLIENTCERTIFICATE, HelpMessage = "Path to the client certificate file (.pfx or .p12) for authentication.")]
		public string CertificatePath { get; set; }

		/// <summary>
		/// Gets or sets the password for the client certificate file.
		/// </summary>
		[Parameter(Mandatory = false, ParameterSetName = PARAMSET_CLIENTCERTIFICATE, HelpMessage = "Password for the client certificate file. If not provided, the certificate is assumed to be unencrypted.")]
		public string CertificatePassword { get; set; }

		/// <summary>
		/// Gets or sets the thumbprint of the certificate in the certificate store.
		/// </summary>
		[Parameter(Mandatory = false, ParameterSetName = PARAMSET_CLIENTCERTIFICATE, HelpMessage = "Thumbprint of the certificate in the certificate store. Used to load certificate from the Windows certificate store instead of a file.")]
		public string CertificateThumbprint { get; set; }

		/// <summary>
		/// Gets or sets the certificate store location to search for the certificate.
		/// </summary>
		[Parameter(Mandatory = false, ParameterSetName = PARAMSET_CLIENTCERTIFICATE, HelpMessage = "Certificate store location to search for the certificate. Default is CurrentUser.")]
		public StoreLocation CertificateStoreLocation { get; set; } = StoreLocation.CurrentUser;

		/// <summary>
		/// Gets or sets the certificate store name to search for the certificate.
		/// </summary>
		[Parameter(Mandatory = false, ParameterSetName = PARAMSET_CLIENTCERTIFICATE, HelpMessage = "Certificate store name to search for the certificate. Default is My (Personal).")]
		public StoreName CertificateStoreName { get; set; } = StoreName.My;

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
		/// Gets or sets the script block to provide access tokens.
		/// </summary>
		[Parameter(Mandatory = true, ParameterSetName = PARAMSET_ACCESSTOKEN, HelpMessage = "Script block that returns an access token string. Called whenever a new access token is needed.")]
		public ScriptBlock AccessToken { get; set; }

		/// <summary>
		/// Gets or sets a value indicating whether to load connection from PAC CLI profile.
		/// </summary>
		[Parameter(Mandatory = true, ParameterSetName = PARAMSET_FROMPAC, HelpMessage = "Load connection from a Power Platform CLI (PAC) authentication profile.")]
		public SwitchParameter FromPac { get; set; }

		/// <summary>
		/// Gets or sets the PAC CLI profile name to use.
		/// </summary>
		[Parameter(Mandatory = false, ParameterSetName = PARAMSET_FROMPAC, HelpMessage = "Name of the PAC CLI profile to use. If not specified, uses the current/active profile.")]
		public string ProfileName { get; set; }

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
			// Handle special parameter sets first
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

			if (ParameterSetName == PARAMSET_LISTNAMED)
			{
				base.ProcessRecord();
				var store = new ConnectionStore();
				var connections = store.ListConnections();
				foreach (var connName in connections)
				{
					var metadata = store.LoadConnection(connName);
					WriteObject(new
					{
						Name = connName,
						Url = metadata.Url,
						AuthMethod = metadata.AuthMethod,
						Username = metadata.Username,
						SavedAt = metadata.SavedAt
					});
				}
				return;
			}

			if (ParameterSetName == PARAMSET_CLEARALL)
			{
				base.ProcessRecord();
				var store = new ConnectionStore();
				store.ClearAllConnections();
				WriteObject("All saved connections and cached tokens have been cleared.");
				return;
			}

			if (ParameterSetName == PARAMSET_DELETENAMED)
			{
				base.ProcessRecord();
				var store = new ConnectionStore();
				if (store.DeleteConnection(Name))
				{
					WriteObject($"Connection '{Name}' deleted successfully.");
				}
				else
				{
					ThrowTerminatingError(new ErrorRecord(
						new InvalidOperationException($"Connection '{Name}' not found."),
						"ConnectionNotFound",
						ErrorCategory.ObjectNotFound,
						Name));
				}
				return;
			}

			if (ParameterSetName == PARAMSET_LOADNAMED)
			{
				base.ProcessRecord();
				var store = new ConnectionStore();
				var metadata = store.LoadConnection(Name);
				if (metadata == null)
				{
					ThrowTerminatingError(new ErrorRecord(
						new InvalidOperationException($"Connection '{Name}' not found."),
						"ConnectionNotFound",
						ErrorCategory.ObjectNotFound,
						Name));
					return;
				}

				// Restore connection parameters from metadata
				Url = new Uri(metadata.Url);
				ClientId = string.IsNullOrEmpty(metadata.ClientId) ? ClientId : new Guid(metadata.ClientId);
				Username = metadata.Username;
				ManagedIdentityClientId = metadata.ManagedIdentityClientId;

				// Restore saved credentials if available
				if (!string.IsNullOrEmpty(metadata.Password))
				{
					Password = metadata.Password;
					WriteVerbose("Restored saved password");
				}
				if (!string.IsNullOrEmpty(metadata.ClientSecret))
				{
					ClientSecret = metadata.ClientSecret;
					WriteVerbose("Restored saved client secret");
				}
				if (!string.IsNullOrEmpty(metadata.CertificatePath))
				{
					CertificatePath = metadata.CertificatePath;
					CertificatePassword = metadata.CertificatePassword;
					WriteVerbose("Restored saved certificate path and password");
				}
				if (!string.IsNullOrEmpty(metadata.CertificateThumbprint))
				{
					CertificateThumbprint = metadata.CertificateThumbprint;
					if (!string.IsNullOrEmpty(metadata.CertificateStoreLocation))
					{
						CertificateStoreLocation = (StoreLocation)Enum.Parse(typeof(StoreLocation), metadata.CertificateStoreLocation);
					}
					if (!string.IsNullOrEmpty(metadata.CertificateStoreName))
					{
						CertificateStoreName = (StoreName)Enum.Parse(typeof(StoreName), metadata.CertificateStoreName);
					}
					WriteVerbose("Restored certificate thumbprint and store location");
				}

				// Set the appropriate parameter set name based on the auth method
				// and continue with authentication - the MSAL cache will be used
				WriteVerbose($"Loading connection '{Name}' using {metadata.AuthMethod} authentication");
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

						// Wrap the fake service with a thread-safe proxy since FakeXrmEasy is not thread-safe
						var fakeService = xrmFakeContext.GetOrganizationService();
						IOrganizationService threadSafeService = new ThreadSafeOrganizationServiceProxy(fakeService);

						// If RequestInterceptor is provided, wrap with script block interceptor
						if (RequestInterceptor != null)
						{
							threadSafeService = new MockOrganizationServiceWithScriptBlock(threadSafeService, RequestInterceptor);
						}

						ConstructorInfo contructor = typeof(ServiceClient).GetConstructor(BindingFlags.NonPublic | BindingFlags.Instance, null, new[] { typeof(IOrganizationService), typeof(HttpClient), typeof(string), typeof(Version), typeof(ILogger) }, null);
						result = (ServiceClient)contructor.Invoke(new object[] { threadSafeService, new HttpClient(GetFakeHttpHandler()), "https://fakeorg.crm.dynamics.com", new Version(9, 2), A.Fake<ILogger>() });
						break;

					case PARAMSET_CONNECTIONSTRING:
						result = new ServiceClient(ConnectionString);
						break;

					case PARAMSET_INTERACTIVE:
					case PARAMSET_LOADNAMED:
						{
							var publicClient = PublicClientApplicationBuilder
								.Create(ClientId.ToString())
								.WithRedirectUri("http://localhost")
								.Build();

							// Register MSAL cache if saving or loading a named connection
							if (!string.IsNullOrEmpty(Name))
							{
								var store = new ConnectionStore();
								store.RegisterCache(publicClient);
							}

							// If URL is not provided, discover and select environment
							if (Url == null)
							{
								var discoveryUrl = DiscoverAndSelectEnvironment(publicClient).GetAwaiter().GetResult();
								Url = new Uri(discoveryUrl);
							}

							result = new ServiceClient(Url, url => GetTokenInteractive(publicClient, url));

							// Save connection metadata if a name was provided
							if (!string.IsNullOrEmpty(Name) && ParameterSetName != PARAMSET_LOADNAMED)
							{
								var store = new ConnectionStore();
								store.SaveConnection(Name, new ConnectionMetadata
								{
									Url = Url.ToString(),
									AuthMethod = "Interactive",
									ClientId = ClientId.ToString(),
									Username = Username,
									SavedAt = DateTime.UtcNow
								});
								WriteVerbose($"Connection saved as '{Name}'");
							}

							break;
						}

					case PARAMSET_USERNAMEPASSWORD:
						{
							var publicClient = PublicClientApplicationBuilder
								.Create(ClientId.ToString())
								.WithRedirectUri("http://localhost")
								.WithAuthority(AadAuthorityAudience.AzureAdMultipleOrgs)
								.Build();

							// Register MSAL cache if saving a named connection
							if (!string.IsNullOrEmpty(Name))
							{
								var store = new ConnectionStore();
								store.RegisterCache(publicClient);
							}

							// If URL is not provided, discover and select environment
							if (Url == null)
							{
								var discoveryUrl = DiscoverAndSelectEnvironment(publicClient).GetAwaiter().GetResult();
								Url = new Uri(discoveryUrl);
							}

							result = new ServiceClient(Url, url => GetTokenWithUsernamePassword(publicClient, url));

							// Save connection metadata if a name was provided
							if (!string.IsNullOrEmpty(Name))
							{
								var store = new ConnectionStore();
								var metadata = new ConnectionMetadata
								{
									Url = Url.ToString(),
									AuthMethod = "UsernamePassword",
									ClientId = ClientId.ToString(),
									Username = Username,
									SavedAt = DateTime.UtcNow
								};

								// Save password if SaveCredentials is specified (NOT RECOMMENDED)
								if (SaveCredentials)
								{
									metadata.Password = Password;
									WriteWarning("SECURITY WARNING: Password has been saved (encrypted). This is NOT RECOMMENDED for production use.");
								}
								else
								{
									WriteVerbose("Password is not saved. You will need to provide it again when loading this connection.");
								}

								store.SaveConnection(Name, metadata);
								WriteVerbose($"Connection saved as '{Name}'");
							}

							break;
						}

					case PARAMSET_DEVICECODE:
						{
							var publicClient = PublicClientApplicationBuilder
								.Create(ClientId.ToString())
								.WithRedirectUri("http://localhost")
								.Build();

							// Register MSAL cache if saving a named connection
							if (!string.IsNullOrEmpty(Name))
							{
								var store = new ConnectionStore();
								store.RegisterCache(publicClient);
							}

							// If URL is not provided, discover and select environment
							if (Url == null)
							{
								var discoveryUrl = DiscoverAndSelectEnvironment(publicClient).GetAwaiter().GetResult();
								Url = new Uri(discoveryUrl);
							}

							result = new ServiceClient(Url, url => GetTokenWithDeviceCode(publicClient, url));

							// Save connection metadata if a name was provided
							if (!string.IsNullOrEmpty(Name))
							{
								var store = new ConnectionStore();
								store.SaveConnection(Name, new ConnectionMetadata
								{
									Url = Url.ToString(),
									AuthMethod = "DeviceCode",
									ClientId = ClientId.ToString(),
									Username = Username,
									SavedAt = DateTime.UtcNow
								});
								WriteVerbose($"Connection saved as '{Name}'");
							}

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

							// Register MSAL cache if saving a named connection
							if (!string.IsNullOrEmpty(Name))
							{
								var store = new ConnectionStore();
								store.RegisterCache(confApp);
							}

							result = new ServiceClient(Url, url => GetTokenWithClientSecret(confApp, url));

							// Save connection metadata if a name was provided
							if (!string.IsNullOrEmpty(Name))
							{
								var store = new ConnectionStore();
								var metadata = new ConnectionMetadata
								{
									Url = Url.ToString(),
									AuthMethod = "ClientSecret",
									ClientId = ClientId.ToString(),
									SavedAt = DateTime.UtcNow
								};

								// Save client secret if SaveCredentials is specified (NOT RECOMMENDED)
								if (SaveCredentials)
								{
									metadata.ClientSecret = ClientSecret;
									WriteWarning("SECURITY WARNING: Client secret has been saved (encrypted). This is NOT RECOMMENDED for production use.");
								}
								else
								{
									WriteVerbose("Client secret is not saved. You will need to provide it again when loading this connection.");
								}

								store.SaveConnection(Name, metadata);
								WriteVerbose($"Connection saved as '{Name}'");
							}

							break;
						}

					case PARAMSET_CLIENTCERTIFICATE:
						{
							string authority = GetAuthority();

							X509Certificate2 certificate = LoadCertificate();

							var confApp = ConfidentialClientApplicationBuilder
							.Create(ClientId.ToString())
							.WithRedirectUri("http://localhost")
							.WithCertificate(certificate)
							.WithAuthority(authority)
							.Build();

							// Register MSAL cache if saving a named connection
							if (!string.IsNullOrEmpty(Name))
							{
								var store = new ConnectionStore();
								store.RegisterCache(confApp);
							}

							result = new ServiceClient(Url, url => GetTokenWithClientCertificate(confApp, url));

							// Save connection metadata if a name was provided
							if (!string.IsNullOrEmpty(Name))
							{
								var store = new ConnectionStore();
								var metadata = new ConnectionMetadata
								{
									Url = Url.ToString(),
									AuthMethod = "ClientCertificate",
									ClientId = ClientId.ToString(),
									SavedAt = DateTime.UtcNow
								};

								// Save certificate details if SaveCredentials is specified (NOT RECOMMENDED)
								if (SaveCredentials)
								{
									metadata.CertificatePath = CertificatePath;
									metadata.CertificatePassword = CertificatePassword;
									metadata.CertificateThumbprint = CertificateThumbprint;
									metadata.CertificateStoreLocation = CertificateStoreLocation.ToString();
									metadata.CertificateStoreName = CertificateStoreName.ToString();
									WriteWarning("SECURITY WARNING: Certificate details (including password) have been saved (encrypted). This is NOT RECOMMENDED for production use.");
								}
								else
								{
									// Still save thumbprint and store location as they are not secrets
									metadata.CertificateThumbprint = CertificateThumbprint;
									metadata.CertificateStoreLocation = CertificateStoreLocation.ToString();
									metadata.CertificateStoreName = CertificateStoreName.ToString();
									WriteVerbose("Certificate path and password are not saved. You will need to provide the certificate again when loading this connection.");
								}

								store.SaveConnection(Name, metadata);
								WriteVerbose($"Connection saved as '{Name}'");
							}

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

							// Save connection metadata if a name was provided
							if (!string.IsNullOrEmpty(Name))
							{
								var store = new ConnectionStore();
								store.SaveConnection(Name, new ConnectionMetadata
								{
									Url = Url.ToString(),
									AuthMethod = "DefaultAzureCredential",
									ClientId = ClientId.ToString(),
									SavedAt = DateTime.UtcNow
								});
								WriteVerbose($"Connection saved as '{Name}'");
							}

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

							// Save connection metadata if a name was provided
							if (!string.IsNullOrEmpty(Name))
							{
								var store = new ConnectionStore();
								store.SaveConnection(Name, new ConnectionMetadata
								{
									Url = Url.ToString(),
									AuthMethod = "ManagedIdentity",
									ClientId = ClientId.ToString(),
									ManagedIdentityClientId = ManagedIdentityClientId,
									SavedAt = DateTime.UtcNow
								});
								WriteVerbose($"Connection saved as '{Name}'");
							}

							break;
						}

					case PARAMSET_ACCESSTOKEN:
						result = new ServiceClient(Url, url => GetTokenWithScriptBlock());
						break;

					case PARAMSET_FROMPAC:
						{
							// Read PAC CLI profiles
							var profilesPath = Path.Combine(
								Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
								"Microsoft",
								"PowerPlatform",
								"Cli",
								"authprofiles_v2.json");

							if (!File.Exists(profilesPath))
							{
								ThrowTerminatingError(new ErrorRecord(
									new FileNotFoundException($"PAC CLI profiles file not found at: {profilesPath}. Please run 'pac auth create' first to authenticate with PAC CLI."),
									"PacProfilesNotFound",
									ErrorCategory.ObjectNotFound,
									profilesPath));
								return;
							}

							WriteVerbose($"Reading PAC profiles from: {profilesPath}");

							// Read and parse the profiles JSON
							string json = File.ReadAllText(profilesPath);
							var jsonOptions = new JsonSerializerOptions
							{
								PropertyNameCaseInsensitive = true
							};
							
							PacProfilesData pacProfiles;
							try
							{
								pacProfiles = JsonSerializer.Deserialize<PacProfilesData>(json, jsonOptions);
							}
							catch (Exception ex)
							{
								ThrowTerminatingError(new ErrorRecord(
									new InvalidDataException($"Failed to parse PAC CLI profiles file: {ex.Message}", ex),
									"PacProfilesParseError",
									ErrorCategory.ParserError,
									profilesPath));
								return;
							}

							if (pacProfiles == null || pacProfiles.Profiles == null || pacProfiles.Profiles.Count == 0)
							{
								ThrowTerminatingError(new ErrorRecord(
									new InvalidOperationException("No profiles found in PAC CLI. Please run 'pac auth create' first to authenticate with PAC CLI."),
									"NoPacProfiles",
									ErrorCategory.ObjectNotFound,
									null));
								return;
							}

							// Find the profile to use
							PacProfileData profile = null;
							if (!string.IsNullOrEmpty(ProfileName))
							{
								// Find profile by name
								profile = pacProfiles.Profiles.FirstOrDefault(p => 
									p.Name != null && 
									string.Equals(p.Name.Value, ProfileName, StringComparison.OrdinalIgnoreCase));
								
								if (profile == null)
								{
									var availableProfiles = string.Join(", ", pacProfiles.Profiles.Where(p => p.Name != null).Select(p => p.Name.Value));
									ThrowTerminatingError(new ErrorRecord(
										new InvalidOperationException($"PAC CLI profile '{ProfileName}' not found. Available profiles: {availableProfiles}"),
										"PacProfileNotFound",
										ErrorCategory.ObjectNotFound,
										ProfileName));
									return;
								}
							}
							else
							{
								// Use the first profile
								profile = pacProfiles.Profiles.FirstOrDefault();
								
								if (profile == null)
								{
									ThrowTerminatingError(new ErrorRecord(
										new InvalidOperationException("No current PAC CLI profile found."),
										"NoCurrentPacProfile",
										ErrorCategory.ObjectNotFound,
										null));
									return;
								}
								
								var profileName = profile.Name?.Value ?? "(unnamed)";
								WriteVerbose($"Using PAC CLI profile: {profileName}");
							}

							// Extract connection details from the profile
							if (profile.ActiveEnvironmentUrl == null && profile.LegacyResource?.Resource == null)
							{
								ThrowTerminatingError(new ErrorRecord(
									new InvalidOperationException($"The selected PAC CLI profile does not have an active environment URL. Please select an environment with 'pac org select'."),
									"PacProfileNoEnvironment",
									ErrorCategory.InvalidData,
									profile));
								return;
							}

							string environmentUrlString = profile.ActiveEnvironmentUrl ?? profile.LegacyResource.Resource;
							Uri environmentUrl;
							if (!Uri.TryCreate(environmentUrlString, UriKind.Absolute, out environmentUrl))
							{
								ThrowTerminatingError(new ErrorRecord(
									new InvalidOperationException($"Invalid environment URL in PAC CLI profile: {environmentUrlString}"),
									"PacProfileInvalidUrl",
									ErrorCategory.InvalidData,
									environmentUrlString));
								return;
							}

							WriteVerbose($"Environment URL: {environmentUrl}");

							// Use MSAL with the same client ID that PAC CLI uses
							var pacClientId = new Guid("04b07795-8ddb-461a-bbee-02f9e1bf7b46"); // PAC CLI's default client ID
							
							var publicClient = PublicClientApplicationBuilder
								.Create(pacClientId.ToString())
								.WithRedirectUri("http://localhost")
								.Build();

							// PAC CLI stores tokens in the same MSAL cache location, so this should find cached tokens
							var store = new ConnectionStore();
							store.RegisterCache(publicClient);

							// Create ServiceClient with token provider that uses MSAL
							result = new ServiceClient(environmentUrl, url => GetTokenFromMsal(publicClient, environmentUrl));

							WriteVerbose($"Connected to Dataverse using PAC CLI profile");
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

		private async Task<string> GetTokenWithClientCertificate(IConfidentialClientApplication app, string url)
		{
			Uri scope = new Uri(Url, "/.default");
			string[] scopes = new[] { scope.ToString() };

			using (var cts = CreateLinkedCts(TimeSpan.FromSeconds(Timeout)))
			{
				AuthenticationResult authResult = await app.AcquireTokenForClient(scopes).ExecuteAsync(cts.Token);
				return authResult.AccessToken;
			}
		}

		private X509Certificate2 LoadCertificate()
		{
			// If thumbprint is provided, load from certificate store
			if (!string.IsNullOrEmpty(CertificateThumbprint))
			{
				using (X509Store store = new X509Store(CertificateStoreName, CertificateStoreLocation))
				{
					store.Open(OpenFlags.ReadOnly);
					X509Certificate2Collection certificates = store.Certificates.Find(
						X509FindType.FindByThumbprint,
						CertificateThumbprint,
						validOnly: false);

					if (certificates.Count == 0)
					{
						throw new InvalidOperationException(
							$"Certificate with thumbprint '{CertificateThumbprint}' not found in {CertificateStoreLocation}\\{CertificateStoreName} store.");
					}

					return certificates[0];
				}
			}
			// Otherwise load from file path
			else if (!string.IsNullOrEmpty(CertificatePath))
			{
				string resolvedPath = GetUnresolvedProviderPathFromPSPath(CertificatePath);
				
				if (!System.IO.File.Exists(resolvedPath))
				{
					throw new System.IO.FileNotFoundException($"Certificate file not found: {resolvedPath}");
				}

				if (!string.IsNullOrEmpty(CertificatePassword))
				{
					return new X509Certificate2(resolvedPath, CertificatePassword);
				}
				else
				{
					return new X509Certificate2(resolvedPath);
				}
			}
			else
			{
				throw new InvalidOperationException(
					"Either CertificatePath or CertificateThumbprint must be provided for certificate authentication.");
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
						// Token cache miss or expired, need to acquire new token
					}
					catch (MsalServiceException)
					{
						// Service error during silent acquisition, need to acquire new token
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
						// Token cache miss or expired, need to acquire new token
					}
					catch (MsalServiceException)
					{
						// Service error during silent acquisition, need to acquire new token
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

		private async Task<string> GetTokenWithScriptBlock()
		{
			using (var cts = CreateLinkedCts(TimeSpan.FromSeconds(Timeout)))
			{
				var results = await Task.Run(() => AccessToken.Invoke(), cts.Token);
				if (results.Count == 0)
				{
					throw new InvalidOperationException("AccessToken script block did not return a value.");
				}
				return results[0].BaseObject.ToString();
			}
		}

		private async Task<string> GetTokenFromMsal(IPublicClientApplication app, Uri environmentUrl)
		{
			Uri scope = new Uri(environmentUrl, "/.default");
			string[] scopes = new[] { scope.ToString() };

			using (var cts = CreateLinkedCts(TimeSpan.FromSeconds(Timeout)))
			{
				// Try to get token silently from cache (PAC CLI should have cached it)
				try
				{
					var accounts = await app.GetAccountsAsync();
					if (accounts.Any())
					{
						var authResult = await app.AcquireTokenSilent(scopes, accounts.First()).ExecuteAsync(cts.Token);
						return authResult.AccessToken;
					}
				}
				catch (MsalUiRequiredException)
				{
					// Token cache miss or expired, need to acquire new token interactively
					WriteVerbose("Cached token not found or expired, acquiring new token interactively...");
				}
				catch (MsalServiceException ex)
				{
					WriteVerbose($"Service error during silent acquisition: {ex.Message}");
				}

				// If silent acquisition failed, acquire new token interactively
				var result = await app.AcquireTokenInteractive(scopes).ExecuteAsync(cts.Token);
				return result.AccessToken;
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

	// Simple classes for parsing PAC CLI profile JSON without depending on bolt.authentication types
	internal class PacProfileData
	{
		public PacProfileName Name { get; set; }
		public string ActiveEnvironmentUrl { get; set; }
		public PacProfileLegacyResource LegacyResource { get; set; }
	}

	internal class PacProfileName
	{
		public string Value { get; set; }
	}

	internal class PacProfileLegacyResource
	{
		public string Resource { get; set; }
	}

	internal class PacProfilesData
	{
		public List<PacProfileData> Profiles { get; set; }
	}

}
