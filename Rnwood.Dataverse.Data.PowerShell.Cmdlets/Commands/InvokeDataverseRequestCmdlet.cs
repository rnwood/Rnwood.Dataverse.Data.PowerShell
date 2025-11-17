using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Management.Automation;
using Microsoft.Crm.Sdk.Messages;

using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Messages;
using Microsoft.PowerPlatform.Dataverse.Client;
using System.Collections;
using Newtonsoft.Json;
using System.Threading;

namespace Rnwood.Dataverse.Data.PowerShell.Commands
{
	///<summary>Invokes a Dataverse request.</summary>
	[Cmdlet("Invoke", "DataverseRequest")]
	public partial class InvokeDataverseRequestCmdlet : RetryableOrganizationServiceCmdlet
	{
		/// <summary>
		/// Request to execute
		/// </summary>
		[Parameter(ParameterSetName = "Request", Mandatory = true, HelpMessage = "Request to execute", ValueFromRemainingArguments = true, ValueFromPipeline = true)]
		public OrganizationRequest Request { get; set; }
		/// <summary>
		/// Name of the Dataverse request to execute. This should be the message name (e.g., WhoAmI, RetrieveMultiple, or a custom action name like myapi_EscalateCase).
		/// </summary>
		[Parameter(ParameterSetName = "NameAndInputs", Mandatory = true, Position = 0, HelpMessage = "Name of the Dataverse request to execute. This should be the message name (e.g., WhoAmI, RetrieveMultiple, or a custom action name like myapi_EscalateCase).")]
		public string RequestName { get; set; }
		/// <summary>
		/// Hashtable of parameters to pass to the request. Keys are parameter names and values are parameter values.
		/// </summary>
		[Parameter(ParameterSetName = "NameAndInputs", Mandatory = false, Position = 1, HelpMessage = "Hashtable of parameters to pass to the request. Keys are parameter names and values are parameter values.")]
		public Hashtable Parameters { get; set; } = new Hashtable();

		/// <summary>
		/// Return the raw OrganizationResponse instead of converting to a PSObject.
		/// </summary>
		[Parameter(ParameterSetName = "NameAndInputs", HelpMessage = "Return the raw OrganizationResponse instead of converting to a PSObject.")]
		public SwitchParameter Raw { get; set; }

		/// <summary>
		/// HTTP method to use for the REST API call (e.g., GET, POST, PATCH, DELETE).
		/// </summary>
		[Parameter(ParameterSetName = "REST", Mandatory = true, Position = 0, HelpMessage = "HTTP method to use for the REST API call (e.g., GET, POST, PATCH, DELETE).")]
		public System.Net.Http.HttpMethod Method { get; set; }
		/// <summary>
		/// Resource name for the REST API call (e.g., 'accounts' or 'myapi_Example'). Do not include the full path like '/api/data/v9.2/accounts' as the organization URL and API version are automatically added.
		/// </summary>
		[Parameter(ParameterSetName = "REST", Mandatory = true, Position = 1, HelpMessage = "Resource name for the REST API call (e.g., 'accounts' or 'myapi_Example'). Do not include the full path - the organization URL and API version are automatically added.")]
		public string Path { get; set; }
		/// <summary>
		/// Body of the REST API request. Can be a string (JSON) or a PSObject which will be converted to JSON.
		/// </summary>
		[Parameter(ParameterSetName = "REST", Mandatory = false, Position = 2, HelpMessage = "Body of the REST API request. Can be a string (JSON) or a PSObject which will be converted to JSON.")]
		public PSObject Body { get; set; } = "";
		/// <summary>
		/// Hashtable of custom HTTP headers to include in the REST API request.
		/// </summary>
		[Parameter(ParameterSetName = "REST", Mandatory = false, HelpMessage = "Hashtable of custom HTTP headers to include in the REST API request.")]
		public Hashtable CustomHeaders { get; set; } = new Hashtable();

        /// <summary>
        /// Controls the maximum number of requests sent to Dataverse in one batch (where possible) to improve throughput. Specify 1 to disable.
        /// </summary>
        [Parameter(ParameterSetName = "Request", HelpMessage = "Controls the maximum number of requests sent to Dataverse in one batch (where possible) to improve throughput. Specify 1 to disable.")]
        [Parameter(ParameterSetName = "NameAndInputs", HelpMessage = "Controls the maximum number of requests sent to Dataverse in one batch (where possible) to improve throughput. Specify 1 to disable.")]
		public uint BatchSize { get; set; } = 1;

		/// <summary>
		/// Specifies the types of business logic (for example plugins) to bypass
		/// </summary>
		[Parameter(ParameterSetName = "Request", HelpMessage = "Specifies the types of business logic (for example plugins) to bypass")]
		[Parameter(ParameterSetName = "NameAndInputs", HelpMessage = "Specifies the types of business logic (for example plugins) to bypass")]
		public CustomLogicBypassableOrganizationServiceCmdlet.BusinessLogicTypes[] BypassBusinessLogicExecution { get; set; }

		/// <summary>
		/// Specifies the IDs of plugin steps to bypass
		/// </summary>
		[Parameter(ParameterSetName = "Request", HelpMessage = "Specifies the IDs of plugin steps to bypass")]
		[Parameter(ParameterSetName = "NameAndInputs", HelpMessage = "Specifies the IDs of plugin steps to bypass")]
		public Guid[] BypassBusinessLogicExecutionStepIds { get; set; }

		private RequestBatchProcessor _batchProcessor;
		private CancellationTokenSource _userCancellationCts;
		private EntityMetadataFactory _entityMetadataFactory;
		private DataverseEntityConverter _entityConverter;

		/// <summary>
		/// Initializes the cmdlet.
		/// </summary>
		protected override void BeginProcessing()
		{
			base.BeginProcessing();

			// Initialize metadata factory and converter only for NameAndInputs parameter set when Raw is not specified
			if (ParameterSetName == "NameAndInputs" && !Raw)
			{
				_entityMetadataFactory = new EntityMetadataFactory(Connection);
				_entityConverter = new DataverseEntityConverter(Connection, _entityMetadataFactory);
			}

			// initialize cancellation token source for this pipeline invocation
			_userCancellationCts = new CancellationTokenSource();

			if (BatchSize > 1)
			{
				_batchProcessor = new RequestBatchProcessor(
					BatchSize,
					Connection,
					WriteVerbose,
					(obj) => WriteObject(obj),
					(err) => WriteError(err),
					ShouldProcess,
					() => Stopping,
					_userCancellationCts.Token,
					BypassBusinessLogicExecution,
					BypassBusinessLogicExecutionStepIds);
			}
		}

		/// <summary>
		/// Processes the cmdlet request and writes the response to the pipeline.
		/// </summary>
		protected override void ProcessRecord()
		{
			base.ProcessRecord();

			if (ParameterSetName == "REST")
			{
				// Validate that the path portion (before query string) does not contain '/' as the SDK does not support full paths
				// Query strings (after '?') may contain '/' characters
				string pathPortion = Path.Split('?')[0];
				if (pathPortion.Contains("/"))
				{
					throw new ArgumentException(
						$"The Path parameter should not contain '/' characters in the resource name portion. " +
						$"Provide only the resource name (e.g., 'accounts' or 'WhoAmI') rather than a full path like '/api/data/v9.2/accounts'. " +
						$"The organization URL and API version are automatically added by the connection. " +
						$"Query strings may contain '/' characters. " +
						$"Current value: '{Path}'",
						nameof(Path));
				}

				var headers = new Dictionary<string, List<string>>();
				foreach (DictionaryEntry kvp in CustomHeaders.Cast<DictionaryEntry>())
				{
					headers[(string)kvp.Key] = new List<string>(new[] { (string)kvp.Value });
				}

				string bodyString = "";
				if (Body != null)
				{
					if (Body.ImmediateBaseObject is string bs)
					{
						bodyString = bs;
					} else
					{
						bodyString = (string)InvokeCommand.NewScriptBlock("param($body); $body | ConvertTo-Json -Depth 100").Invoke(Body).First().ImmediateBaseObject;
					}
				}

				System.Net.Http.HttpResponseMessage response = Connection.ExecuteWebRequest(Method, Path, bodyString, headers);
				response.EnsureSuccessStatusCode();
				string responseBody = response.Content.ReadAsStringAsync().Result;
				var result = InvokeCommand.NewScriptBlock("param($response); $response | ConvertFrom-Json -Depth 100").Invoke(responseBody);
				WriteObject(result);
			}
			else
			{
				if (ParameterSetName == "NameAndInputs")
				{
					Request = new OrganizationRequest(RequestName);
					Request.Parameters.AddRange(Parameters.Cast<DictionaryEntry>().Select(e => new KeyValuePair<string, object>((string)e.Key, e.Value)));
				}

				// Apply bypass parameters to the request
				QueryHelpers.ApplyBypassBusinessLogicExecution(Request, BypassBusinessLogicExecution, BypassBusinessLogicExecutionStepIds);

				// If batching is enabled, queue the request for batch execution
				if (_batchProcessor != null)
				{
					var inputObj = Request != null ? PSObject.AsPSObject(Request) : null;
					var context = new RequestOperationContext(
						inputObj,
						Request,
						Retries,
						InitialRetryDelay,
						WriteVerbose,
						(err) => WriteError(err),
						(obj) => WriteObject(obj),
						ShouldProcess,
						_entityConverter);

					context.Request = Request;
					context.RetriesRemaining = Retries;

					WriteVerbose($"Added request {Request?.GetType().Name ?? RequestName} to batch");
					_batchProcessor.QueueOperation(context);
				}
				else
				{
					// Non-batched: execute with retry wrapper already provided by base class
					var response = ExecuteWithRetry(Request);
					
					// Convert response to PSObject if we have a converter
					if (_entityConverter != null)
					{
						var convertedResponse = _entityConverter.ConvertResponseToPSObject(response);
						WriteObject(convertedResponse);
					}
					else
					{
						WriteObject(response);
					}
				}
			}
		}

		/// <summary>Completes cmdlet processing.</summary>
		protected override void EndProcessing()
		{
			base.EndProcessing();

			if (_batchProcessor != null)
			{
				_batchProcessor.Flush();
				_batchProcessor.ProcessRetries();
			}

			_userCancellationCts?.Dispose();
			_userCancellationCts = null;
		}

		/// <summary>
		/// Called when the user cancels the cmdlet.
		/// </summary>
		protected override void StopProcessing()
		{
			try
			{
				_userCancellationCts?.Cancel();
			}
			catch { }
			base.StopProcessing();
		}
	}
}
