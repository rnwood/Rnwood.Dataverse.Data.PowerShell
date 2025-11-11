using System;
using System.Management.Automation;
using Microsoft.PowerPlatform.Dataverse.Client;
using Microsoft.Crm.Sdk.Messages;

namespace Rnwood.Dataverse.Data.PowerShell.Commands
{
	/// <summary>
	/// Generates a URL to open the Power Platform Admin Center for the current environment.
	/// </summary>
	[Cmdlet(VerbsCommon.Get, "DataverseAdminPortalUrl")]
	[OutputType(typeof(string))]
	public class GetDataverseAdminPortalUrlCmdlet : OrganizationServiceCmdlet
	{
		/// <summary>
		/// Gets or sets the specific page to navigate to in the admin portal.
		/// </summary>
		[Parameter(Mandatory = false, HelpMessage = "Specific page to navigate to in the admin portal (e.g., 'environments', 'analytics', 'resources').")]
		[ValidateSet("home", "environments", "analytics", "resources", "dataintegration", "datapolicies", "helpandsupport")]
		public string Page { get; set; } = "environments";

		/// <summary>
		/// Processes the cmdlet to generate the admin portal URL.
		/// </summary>
		protected override void ProcessRecord()
		{
			base.ProcessRecord();

			if (Connection == null)
			{
				ThrowTerminatingError(new ErrorRecord(
					new InvalidOperationException("No connection provided. Use -Connection parameter or set a default connection."),
					"NoConnection",
					ErrorCategory.InvalidOperation,
					null));
				return;
			}

			// Get the environment ID from the organization
			WhoAmIRequest whoAmIRequest = new WhoAmIRequest();
			WhoAmIResponse whoAmIResponse = (WhoAmIResponse)Connection.Execute(whoAmIRequest);
			Guid orgId = whoAmIResponse.OrganizationId;

			// Build the admin portal URL
			string baseUrl = "https://admin.powerplatform.microsoft.com";
			
			// For environments page, include the specific environment
			if (Page.ToLowerInvariant() == "environments")
			{
				string url = $"{baseUrl}/environments/{orgId:D}/hub";
				WriteObject(url);
			}
			else
			{
				// For other pages, navigate to the general section
				string url = baseUrl;
				switch (Page.ToLowerInvariant())
				{
					case "analytics":
						url += "/analytics";
						break;
					case "resources":
						url += "/resources";
						break;
					case "dataintegration":
						url += "/dataintegration";
						break;
					case "datapolicies":
						url += "/datapolicies";
						break;
					case "helpandsupport":
						url += "/support";
						break;
					case "home":
					default:
						url += "/home";
						break;
				}
				WriteObject(url);
			}
		}
	}
}
