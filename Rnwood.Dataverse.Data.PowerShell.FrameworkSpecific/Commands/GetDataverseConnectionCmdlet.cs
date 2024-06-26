﻿using Microsoft.Identity.Client;
using Microsoft.PowerPlatform.Dataverse.Client;
using System;
using System.Collections.Generic;
using System.IO;
using System.Management.Automation;
using System.Reflection;
using System.Text;
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

		static GetDataverseConnectionCmdlet()
		{
			AppDomain.CurrentDomain.AssemblyResolve += CurrentDomain_AssemblyResolve;
		}

		private static System.Reflection.Assembly CurrentDomain_AssemblyResolve(object sender, ResolveEventArgs args)
		{
			string assyDir = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);

			string assyName = args.Name.Split(',')[0];
			string assyFile = assyDir + "/" + assyName + ".dll";

			if (File.Exists(assyFile))
			{
				return Assembly.LoadFrom(assyFile);
			}

			return null;
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
			base.BeginProcessing();

			


			switch (ParameterSetName)
			{
				case PARAMSET_CONNECTIONSTRING:
					WriteObject(new ServiceClient(ConnectionString));
					break;

				case PARAMSET_INTERACTIVE:
					{
						var publicClient = PublicClientApplicationBuilder
							.Create(ClientId.ToString())
							.WithRedirectUri("http://localhost")
							.Build();

						WriteObject(new ServiceClient(Url, url => GetTokenInteractive(publicClient, url)));

						break;
					}

				case PARAMSET_USERNAMEPASSWORD:
					{
						var publicClient = PublicClientApplicationBuilder
							.Create(ClientId.ToString())
							.WithRedirectUri("http://localhost")
							.Build();

						WriteObject(new ServiceClient(Url, url => GetTokenWithUsernamePassword(publicClient, url)));

						break;
					}

				case PARAMSET_DEVICECODE:
					{
						var publicClient = PublicClientApplicationBuilder
							.Create(ClientId.ToString())
							.WithRedirectUri("http://localhost")
							.Build();

						WriteObject(new ServiceClient(Url, url => GetTokenWithDeviceCode(publicClient, url)));

						break;
					}

				case PARAMSET_CLIENTSECRET:
					{
						var confApp = ConfidentialClientApplicationBuilder
						.Create(ClientId.ToString())
						.WithRedirectUri("http://localhost")
						.WithClientSecret(ClientSecret)
						.WithTenantId("bd6c851f-e0dc-4d6d-ab4c-99452fe28387")
						.Build();

						WriteObject(new ServiceClient(Url, url => GetTokenWithClientSecret(confApp, url)));

						break;
					}

				default:
					throw new NotImplementedException(ParameterSetName);
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
