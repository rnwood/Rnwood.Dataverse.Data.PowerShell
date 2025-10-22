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
	public class InvokeDataverseRequestCmdlet : RetryableOrganizationServiceCmdlet
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
		/// HTTP method to use for the REST API call (e.g., GET, POST, PATCH, DELETE).
		/// </summary>
		[Parameter(ParameterSetName = "REST", Mandatory = true, Position = 0, HelpMessage = "HTTP method to use for the REST API call (e.g., GET, POST, PATCH, DELETE).")]
		public System.Net.Http.HttpMethod Method { get; set; }
		/// <summary>
		/// Path portion of the REST API URL (e.g., 'api/data/v9.2/contacts' or 'myapi_Example').
		/// </summary>
		[Parameter(ParameterSetName = "REST", Mandatory = true, Position = 1, HelpMessage = "Path portion of the REST API URL (e.g., 'api/data/v9.2/contacts' or 'myapi_Example').")]
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



		/// <summary>
		/// Initializes the cmdlet.
		/// </summary>
		protected override void BeginProcessing()
		{
			base.BeginProcessing();

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
						ShouldProcess);

					context.Request = Request;
					context.RetriesRemaining = Retries;

					WriteVerbose($"Added request {Request?.GetType().Name ?? RequestName} to batch");
					_batchProcessor.QueueOperation(context);
				}
				else
				{
					// Non-batched: execute with retry wrapper already provided by base class
					var response = ExecuteWithRetry(Request);
					WriteObject(response);
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

		/// <summary>
		/// Holds state for a single request operation in a batch or retry queue.
		/// </summary>
		internal class RequestOperationContext
		{
			private readonly Action<string> _writeVerbose;
			private readonly Action<ErrorRecord> _writeError;
			private readonly Action<object> _writeObject;
			private readonly Func<string, bool> _shouldProcess;

			public RequestOperationContext(PSObject inputObject, OrganizationRequest request, int retries, int initialRetryDelay, Action<string> writeVerbose, Action<ErrorRecord> writeError, Action<object> writeObject, Func<string, bool> shouldProcess)
			{
				InputObject = inputObject;
				Request = request;
				Retries = retries;
				InitialRetryDelay = initialRetryDelay;
				RetriesRemaining = retries;
				NextRetryTime = DateTime.MinValue;
				_writeVerbose = writeVerbose;
				_writeError = writeError;
				_writeObject = writeObject;
				_shouldProcess = shouldProcess;
			}

			public PSObject InputObject { get; }
			public OrganizationRequest Request { get; set; }
			public int Retries { get; }
			public int InitialRetryDelay { get; }
			public int RetriesRemaining { get; set; }
			public DateTime NextRetryTime { get; set; }

			public void Complete(OrganizationResponse response)
			{
				_writeVerbose($"Request completed: {Request?.GetType().Name}");
				try { _writeObject(response); } catch { }
			}

			public bool HandleFault(OrganizationServiceFault fault)
			{
				// No special handling for generic requests by default
				return false;
			}

			public void ScheduleRetry(Exception e)
			{
				int attemptNumber = Retries - RetriesRemaining + 1;
				int delayS = InitialRetryDelay * (int)Math.Pow(2, attemptNumber - 1);
				NextRetryTime = DateTime.UtcNow.AddSeconds(delayS);
				RetriesRemaining--;

				_writeVerbose($"Request failed, will retry in {delayS}s (attempt {attemptNumber} of {Retries + 1}): {this}\n{e}");
			}

			public void ReportError(Exception e)
			{
				_writeError(new ErrorRecord(e, null, ErrorCategory.InvalidResult, (object)InputObject ?? (object)Request));
			}

			public override string ToString()
			{
				return Request == null ? "Request" : Request.GetType().Name;
			}
		}

		/// <summary>
		/// Batch processor for generic OrganizationRequest batching (uses ExecuteMultipleRequest).
		/// </summary>
		internal class RequestBatchProcessor
		{
			private readonly List<RequestOperationContext> _nextBatchItems;
			private readonly List<RequestOperationContext> _pendingRetries;
			private readonly uint _batchSize;
			private readonly IOrganizationService _connection;
			private readonly Action<string> _writeVerbose;
			private readonly Action<object> _writeObject;
			private readonly Action<ErrorRecord> _writeError;
			private readonly Func<string, bool> _shouldProcess;
			private readonly Func<bool> _isStopping;
			private readonly CancellationToken _cancellationToken;
			private readonly CustomLogicBypassableOrganizationServiceCmdlet.BusinessLogicTypes[] _bypassBusinessLogicExecution;
			private readonly Guid[] _bypassBusinessLogicExecutionStepIds;

			public RequestBatchProcessor(uint batchSize, IOrganizationService connection, Action<string> writeVerbose, Action<object> writeObject, Action<ErrorRecord> writeError, Func<string, bool> shouldProcess, Func<bool> isStopping, CancellationToken cancellationToken, CustomLogicBypassableOrganizationServiceCmdlet.BusinessLogicTypes[] bypassBusinessLogicExecution, Guid[] bypassBusinessLogicExecutionStepIds)
			{
				_batchSize = batchSize;
				_connection = connection;
				_writeVerbose = writeVerbose;
				_writeObject = writeObject;
				_writeError = writeError;
				_shouldProcess = shouldProcess;
				_isStopping = isStopping;
				_cancellationToken = cancellationToken;
				_bypassBusinessLogicExecution = bypassBusinessLogicExecution;
				_bypassBusinessLogicExecutionStepIds = bypassBusinessLogicExecutionStepIds;
				_nextBatchItems = new List<RequestOperationContext>();
				_pendingRetries = new List<RequestOperationContext>();
			}

			public void QueueOperation(RequestOperationContext context)
			{
				_nextBatchItems.Add(context);

				if (_nextBatchItems.Count >= _batchSize)
				{
					ExecuteBatch();
				}
			}

			public void Flush()
			{
				if (_nextBatchItems.Count > 0)
				{
					ExecuteBatch();
				}
			}

			public void ProcessRetries()
			{
				while (!_isStopping() && !_cancellationToken.IsCancellationRequested && _pendingRetries.Count > 0)
				{
					DateTime now = DateTime.UtcNow;
					var readyForRetry = _pendingRetries.Where(r => r.NextRetryTime <= now).ToList();

					if (readyForRetry.Count == 0)
					{
						var nextRetryTime = _pendingRetries.Min(r => r.NextRetryTime);
						var waitTimeMs = (int)Math.Max(100, (nextRetryTime - now).TotalMilliseconds);

						_writeVerbose($"Waiting {waitTimeMs / 1000.0:F1}s for next retry batch...");
						Thread.Sleep(waitTimeMs);

						continue;
					}

					foreach (var item in readyForRetry)
					{
						_pendingRetries.Remove(item);
						_nextBatchItems.Add(item);

						if (_nextBatchItems.Count >= _batchSize)
						{
							ExecuteBatch();
						}
					}

					if (_nextBatchItems.Count > 0)
					{
						ExecuteBatch();
					}
				}
			}

			private void ExecuteBatch()
			{
				if (_nextBatchItems.Count == 0)
				{
					return;
				}

				if (!_shouldProcess("Execute batch of requests:\n" + string.Join("\n", _nextBatchItems.Select(r => r.ToString()))))
				{
					_nextBatchItems.Clear();
					return;
				}

				ExecuteMultipleRequest batchRequest = new ExecuteMultipleRequest()
				{
					Settings = new ExecuteMultipleSettings()
					{
						ReturnResponses = true,
						ContinueOnError = true
					},
					Requests = new OrganizationRequestCollection(),
					RequestId = Guid.NewGuid()
				};

				// Apply bypass logic at batch level
				if (_bypassBusinessLogicExecution?.Length > 0)
				{
					batchRequest.Parameters["BypassBusinessLogicExecution"] = string.Join(",", _bypassBusinessLogicExecution.Select(o => o.ToString()));
				}
				if (_bypassBusinessLogicExecutionStepIds?.Length > 0)
				{
					batchRequest.Parameters["BypassBusinessLogicExecutionStepIds"] = string.Join(",", _bypassBusinessLogicExecutionStepIds.Select(id => id.ToString()));
				}

				batchRequest.Requests.AddRange(_nextBatchItems.Select(i => i.Request));

				ExecuteMultipleResponse response = null;

				try
				{
					response = (ExecuteMultipleResponse)_connection.Execute(batchRequest);
				}
				catch (Exception e)
				{
					foreach (var context in _nextBatchItems)
					{
						if (context.RetriesRemaining > 0)
						{
							context.ScheduleRetry(e);
							_pendingRetries.Add(context);
						}
						else
						{
							context.ReportError(e);
						}
					}

					_nextBatchItems.Clear();
					return;
				}

				for (int i = 0; i < response.Responses.Count; i++)
				{
					var itemResponse = response.Responses[i];
					var context = _nextBatchItems[itemResponse.RequestIndex];

					if (itemResponse.Fault != null)
					{
						StringBuilder details = new StringBuilder();
						QueryHelpers.AppendFaultDetails(itemResponse.Fault, details);
						var e = new Exception(details.ToString());

						bool handled = context.HandleFault(itemResponse.Fault);

						if (!handled && context.RetriesRemaining > 0)
						{
							context.ScheduleRetry(e);
							_pendingRetries.Add(context);
						}
						else if (!handled)
						{
							context.ReportError(e);
						}
					}
					else
					{
						context.Complete(itemResponse.Response);
					}
				}

				_nextBatchItems.Clear();
			}


		}
	}
}
