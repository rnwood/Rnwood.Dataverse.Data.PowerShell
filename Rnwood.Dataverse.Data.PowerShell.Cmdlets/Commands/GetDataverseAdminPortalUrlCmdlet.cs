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

			// Build the admin portal URL for the specific environment
			string url = $"https://admin.powerplatform.microsoft.com/environments/{orgId:D}/hub";
			
			WriteObject(url);
		}
	}
}
