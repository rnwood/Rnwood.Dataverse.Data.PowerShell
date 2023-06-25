using Microsoft.Identity.Client;
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


		[Parameter]
		public Guid ClientId { get; set; } = new Guid("9cee029c-6210-4654-90bb-17e6e9d36617");

		[Parameter(Mandatory = true)]
		public Uri Url { get; set; }

		[Parameter(Mandatory = true, ParameterSetName = PARAMSET_CLIENTSECRET)]
		public string ClientSecret { get; set; }

		[Parameter(Mandatory = false, ParameterSetName = PARAMSET_INTERACTIVE)]
		public string Username { get; set; }

		protected override void BeginProcessing()
		{
			base.BeginProcessing();

			switch (ParameterSetName)
			{
				case PARAMSET_INTERACTIVE:
					var publicClient = PublicClientApplicationBuilder
						.Create(ClientId.ToString())
						.WithRedirectUri("http://localhost")
						.Build();

					WriteObject(new ServiceClient(Url, url => GetTokenInteractive(publicClient, url)));

					break;

				case PARAMSET_CLIENTSECRET:
					var confApp = ConfidentialClientApplicationBuilder
					.Create(ClientId.ToString())
					.WithRedirectUri("http://localhost")
					.WithClientSecret(ClientSecret)
					.WithTenantId("bd6c851f-e0dc-4d6d-ab4c-99452fe28387")
					.Build();

					WriteObject(new ServiceClient(Url, url => GetTokenWithClientSecret(confApp, url)));


					break;

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


		private async Task<string> GetTokenInteractive(IPublicClientApplication app, string url)
		{
			Uri scope = new Uri(Url, "/.default");
			string[] scopes = new[] { scope.ToString() };


			AuthenticationResult authResult;

			try
			{

				authResult = await app.AcquireTokenSilent(scopes, Username).ExecuteAsync();
			}
			catch (MsalUiRequiredException)
			{
				authResult = await app.AcquireTokenInteractive(scopes).ExecuteAsync();

			}

			return authResult.AccessToken;



		}
	}


}
