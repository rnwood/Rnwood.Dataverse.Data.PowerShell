using System;
using System.Management.Automation;
using Microsoft.PowerPlatform.Dataverse.Client;
using Microsoft.Crm.Sdk.Messages;

namespace Rnwood.Dataverse.Data.PowerShell.Commands
{
	/// <summary>
	/// Generates a URL to open the Power Apps Maker Portal for the current environment.
	/// </summary>
	[Cmdlet(VerbsCommon.Get, "DataverseMakerPortalUrl")]
	[OutputType(typeof(string))]
	public class GetDataverseMakerPortalUrlCmdlet : OrganizationServiceCmdlet
	{
		/// <summary>
		/// Gets or sets the specific page to navigate to in the maker portal.
		/// </summary>
		[Parameter(Mandatory = false, HelpMessage = "Specific page to navigate to in the maker portal (e.g., 'solutions', 'tables', 'apps').")]
		[ValidateSet("home", "solutions", "tables", "apps", "flows", "chatbots", "connections", "dataflows", "entities")]
		public string Page { get; set; } = "home";

		/// <summary>
		/// Processes the cmdlet to generate the maker portal URL.
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

			// Build the maker portal URL
			string baseUrl = "https://make.powerapps.com";
			string url = $"{baseUrl}/environments/{orgId:D}";

			// Add page-specific path
			switch (Page.ToLowerInvariant())
			{
				case "solutions":
					url += "/solutions";
					break;
				case "tables":
				case "entities":
					url += "/entities";
					break;
				case "apps":
					url += "/apps";
					break;
				case "flows":
					url += "/flows";
					break;
				case "chatbots":
					url += "/chatbots";
					break;
				case "connections":
					url += "/connections";
					break;
				case "dataflows":
					url += "/dataflows";
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
