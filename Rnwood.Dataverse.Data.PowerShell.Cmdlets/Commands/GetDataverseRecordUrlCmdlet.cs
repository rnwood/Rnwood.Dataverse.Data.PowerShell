using System;
using System.Management.Automation;
using Microsoft.PowerPlatform.Dataverse.Client;
using Microsoft.Xrm.Sdk;

namespace Rnwood.Dataverse.Data.PowerShell.Commands
{
	/// <summary>
	/// Generates a URL to open a record in the Dataverse web interface.
	/// </summary>
	[Cmdlet(VerbsCommon.Get, "DataverseRecordUrl")]
	[OutputType(typeof(string))]
	public class GetDataverseRecordUrlCmdlet : OrganizationServiceCmdlet
	{
		/// <summary>
		/// Gets or sets the logical name of the table (entity).
		/// </summary>
		[Parameter(Mandatory = true, Position = 0, ValueFromPipelineByPropertyName = true, HelpMessage = "Logical name of the table (e.g., 'account', 'contact').")]
		[Alias("EntityName", "LogicalName")]
		public string TableName { get; set; }

		/// <summary>
		/// Gets or sets the ID of the record. If not provided, generates a URL for creating a new record.
		/// </summary>
		[Parameter(Mandatory = false, Position = 1, ValueFromPipelineByPropertyName = true, HelpMessage = "ID of the record. If not provided, generates a URL for creating a new record.")]
		[Alias("RecordId")]
		public Guid? Id { get; set; }

		/// <summary>
		/// Gets or sets the app ID to open the record in a specific app.
		/// </summary>
		[Parameter(Mandatory = false, HelpMessage = "App ID to open the record in a specific model-driven app.")]
		public Guid? AppId { get; set; }

		/// <summary>
		/// Gets or sets the form ID to open a specific form.
		/// </summary>
		[Parameter(Mandatory = false, HelpMessage = "Form ID to open a specific form for the record.")]
		public Guid? FormId { get; set; }

		/// <summary>
		/// Processes the cmdlet to generate the record URL.
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

			// Extract the base URL from the connection
			string baseUrl = Connection.ConnectedOrgUriActual?.ToString();
			if (string.IsNullOrEmpty(baseUrl))
			{
				ThrowTerminatingError(new ErrorRecord(
					new InvalidOperationException("Unable to determine organization URL from connection."),
					"InvalidConnection",
					ErrorCategory.InvalidOperation,
					null));
				return;
			}

			// Remove trailing slash
			baseUrl = baseUrl.TrimEnd('/');

			// Build the URL
			string url;
			if (Id.HasValue)
			{
				// URL for existing record
				url = $"{baseUrl}/main.aspx?etn={TableName}&id={Id.Value:D}&pagetype=entityrecord";
			}
			else
			{
				// URL for creating new record
				url = $"{baseUrl}/main.aspx?etn={TableName}&pagetype=entityrecord";
			}

			// Add optional parameters
			if (AppId.HasValue)
			{
				url += $"&appid={AppId.Value:D}";
			}

			if (FormId.HasValue)
			{
				url += $"&formid={FormId.Value:D}";
			}

			WriteObject(url);
		}
	}
}
