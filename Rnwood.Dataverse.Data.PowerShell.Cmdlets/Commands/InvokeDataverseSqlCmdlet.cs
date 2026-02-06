using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Management.Automation;
using Microsoft.Crm.Sdk.Messages;

using Microsoft.Xrm.Sdk;
using Microsoft.PowerPlatform.Dataverse.Client;
using MarkMpn.Sql4Cds.Engine;
using System.Data.Common;
using System.Data;
using System.Collections;
using System.Threading.Tasks;
using System.Collections.Concurrent;

namespace Rnwood.Dataverse.Data.PowerShell.Commands
{
	///<summary>Invokes a Dataverse request.</summary>
	[Cmdlet("Invoke", "DataverseSql", SupportsShouldProcess = true, ConfirmImpact = ConfirmImpact.Medium)]
	public class InvokeDataverseSqlCmdlet : OrganizationServiceCmdlet
	{
		private Sql4CdsConnection _sqlConnection;
		private Sql4CdsCommand _command;
		private bool _commandPrepared;
		private int _progressPercentage;
		private string _progressMessage;
		private ConcurrentQueue<string> _infoMessages;
		private ConcurrentQueue<Task> _pendingConfirmations;
		private System.Threading.CancellationTokenSource _userCancellationCts;
		/// <summary>
		/// SQL to execute. See Sql4Cds docs.
		/// </summary>
		[Parameter(Mandatory = true, Position = 0, HelpMessage = "SQL to execute. See Sql4Cds docs.", ValueFromRemainingArguments = true)]
		public string Sql { get; set; }
		/// <summary>
		/// Uses the TDS endpoint for supported queries. See Sql4Cds docs
		/// </summary>
		[Parameter(HelpMessage = "Uses the TDS endpoint for supported queries. See Sql4Cds docs")]
		public SwitchParameter UseTdsEndpoint { get; set; }
		/// <summary>
		/// Sets the command timeout. See Sql4Cds docs
		/// </summary>
		[Parameter(HelpMessage = "Sets the command timeout. See Sql4Cds docs")]
		public int Timeout { get; set; } = 600;
		/// <summary>
		/// Specifies the values to use as parameters for the query. When reading from the pipelines, each input object will execute the query once.
		/// </summary>
		[Parameter(ValueFromPipeline = true, HelpMessage ="Specifies the values to use as parameters for the query. When reading from the pipelines, each input object will execute the query once.")]
		public PSObject Parameters { get; set; }
		/// <summary>
		/// Sets the max batch size. See Sql4Cds docs
		/// </summary>
		[Parameter(HelpMessage = "Sets the max batch size. See Sql4Cds docs")]
		public int? BatchSize { get; set; }
		/// <summary>
		/// Sets the max degree of paralleism. See Sql4Cds docs
		/// </summary>
		[Parameter(HelpMessage = "Sets the max degree of paralleism. See Sql4Cds docs")]
		public int? MaxDegreeOfParallelism { get; set; }
		/// <summary>
		/// Bypasses custom plugins. See Sql4Cds docs.
		/// </summary>
		[Parameter(HelpMessage = "Bypasses custom plugins. See Sql4Cds docs.")]
		public SwitchParameter BypassCustomPluginExecution { get; set; }
		/// <summary>
		/// Uses bulk delete for supported DELETE operations. See Sql4Cds docs.
		/// </summary>
		[Parameter(HelpMessage = "Uses bulk delete for supported DELETE operations. See Sql4Cds docs.")]
		public SwitchParameter UseBulkDelete { get; set; }
		/// <summary>
		/// Returns lookup column values as simple Guid as opposed to SqlEntityReference type. See Sql4Cds docs.
		/// </summary>
		[Parameter(HelpMessage = "Returns lookup column values as simple Guid as opposed to SqlEntityReference type. See Sql4Cds docs.")]
		public SwitchParameter ReturnEntityReferenceAsGuid { get; set; }
		/// <summary>
		/// When working with date values, this property indicates the local time zone should be used. See Sql4Cds docs.
		/// </summary>
		[Parameter(HelpMessage = "When working with date values, this property indicates the local time zone should be used. See Sql4Cds docs.")]
		public SwitchParameter UseLocalTimezone { get; set; }
		/// <summary>
		/// Additional data sources to register with Sql4Cds, allowing queries across multiple connections. Hashtable where keys are data source names and values are ServiceClient connections.
		/// </summary>
		[Parameter(HelpMessage = "Additional data sources to register with Sql4Cds, allowing queries across multiple connections. Hashtable where keys are data source names and values are ServiceClient connections.")]
		public Hashtable AdditionalConnections { get; set; }
		/// <summary>
		/// Specifies the name for the primary data source. If not specified, defaults to the organization unique name. Use this parameter to ensure consistent data source names across different environments for repeatable queries.
		/// </summary>
		[Parameter(HelpMessage = "Specifies the name for the primary data source. If not specified, defaults to the organization unique name. Use this parameter to ensure consistent data source names across different environments for repeatable queries.")]
		public string DataSourceName { get; set; }

		/// <summary>
		/// Initializes the cmdlet.
		/// </summary>
		protected override void BeginProcessing()
		{
			base.BeginProcessing();

			// Initialize cancellation token source for this pipeline invocation
			_userCancellationCts = new System.Threading.CancellationTokenSource();

			// Create a DataSource with AccessTokenProvider to enable TDS endpoint support
			// when CurrentAccessToken is not available (e.g., with token provider authentication)
			var dataSource = new DataSource(Connection);
			
			// Set AccessTokenProvider to retrieve access token from ServiceClient
			// This is used as a fallback when CurrentAccessToken is null
			if (Connection is ServiceClient serviceClient)
			{
				dataSource.AccessTokenProvider = () => GetAccessToken(serviceClient);
			}

			// Override the data source name if specified by the user
			// This ensures consistent naming across different environments for repeatable queries
			if (!string.IsNullOrEmpty(DataSourceName))
			{
				dataSource.Name = DataSourceName;
			}

			// Create connection using the DataSource with AccessTokenProvider configured
			var dataSources = new Dictionary<string, DataSource>(StringComparer.OrdinalIgnoreCase)
			{
				[dataSource.Name] = dataSource
			};

			// Add any additional connections provided by the user
			if (AdditionalConnections != null)
			{
				foreach (DictionaryEntry entry in AdditionalConnections)
				{
					string name = entry.Key.ToString();
					if (entry.Value is ServiceClient additionalServiceClient)
					{
						var additionalDataSource = new DataSource(additionalServiceClient);
						additionalDataSource.Name = name;
						
						// Set AccessTokenProvider for TDS endpoint support
						additionalDataSource.AccessTokenProvider = () => GetAccessToken(additionalServiceClient);
						
						dataSources[name] = additionalDataSource;
					}
					else if (entry.Value is IOrganizationService additionalOrgService)
					{
						var additionalDataSource = new DataSource(additionalOrgService);
						additionalDataSource.Name = name;
						
						// Set AccessTokenProvider if it's a ServiceClient
						if (additionalOrgService is ServiceClient additionalSvcClient)
						{
							additionalDataSource.AccessTokenProvider = () => GetAccessToken(additionalSvcClient);
						}
						
						dataSources[name] = additionalDataSource;
					}
					else
					{
						throw new ArgumentException($"AdditionalConnections value for key '{name}' must be a ServiceClient or IOrganizationService instance.");
					}
				}
			}
			
			_sqlConnection = new Sql4CdsConnection(dataSources);
			_sqlConnection.UseTDSEndpoint = false;
			_sqlConnection.Progress += OnSqlConnection_Progress;
			_sqlConnection.UseTDSEndpoint = UseTdsEndpoint;
			_sqlConnection.UseBulkDelete = UseBulkDelete;
			_sqlConnection.ReturnEntityReferenceAsGuid = ReturnEntityReferenceAsGuid;
			_sqlConnection.UseLocalTimeZone = UseLocalTimezone;
			_sqlConnection.InfoMessage += OnSqlConnection_InfoMessage;

			this._sqlConnection.PreInsert += GetOnSqlConnectionConfirmatonRequiredHandler("Create");
			this._sqlConnection.PreDelete += GetOnSqlConnectionConfirmatonRequiredHandler("Delete");
			this._sqlConnection.PreUpdate += GetOnSqlConnectionConfirmatonRequiredHandler("Update");

			if (BypassCustomPluginExecution)
			{
				_sqlConnection.BypassCustomPlugins = true;
			}

			if (BatchSize.HasValue)
			{
				_sqlConnection.BatchSize = BatchSize.Value;
			}

			if (MaxDegreeOfParallelism.HasValue)
			{
				_sqlConnection.MaxDegreeOfParallelism = MaxDegreeOfParallelism.Value;
			}


			_command = _sqlConnection.CreateCommand();
			_command.CommandText = Sql;
			_command.CommandTimeout = Timeout;
			_commandPrepared = false;

			_progressPercentage = 0;
			_progressMessage = "Initialising";
			_infoMessages = new ConcurrentQueue<string>();
			_pendingConfirmations = new ConcurrentQueue<Task>();
		}

		private EventHandler<ConfirmDmlStatementEventArgs> GetOnSqlConnectionConfirmatonRequiredHandler(string operation)
		{
			return (s, ea) =>
			{
				Task task = new Task(() =>
				{
					ea.Cancel = !ShouldProcess($"{operation} {ea.Count} rows in table '{ea.Metadata.LogicalName}?'");
				});
				_pendingConfirmations.Enqueue(task);
				task.Wait();
			};
		}

		private void OnSqlConnection_InfoMessage(object sender, InfoMessageEventArgs e)
		{
			_infoMessages.Enqueue(e.Statement.ToString() + "\n" + e.Message.ToString());
		}

		private void OnSqlConnection_Progress(object sender, ProgressEventArgs e)
		{
			_progressPercentage = Math.Min(100, (int)(100.0 * (e.Progress ?? 0)) );
			_progressMessage = e.Message;
		}

		/// <summary>
		/// Completes cmdlet processing.
		/// </summary>
		protected override void EndProcessing()
		{
			base.EndProcessing();

			_sqlConnection.Dispose();
			_sqlConnection = null;

			_command.Dispose();
			_command = null;
			_commandPrepared = false;

			// Cleanup cancellation token source
			_userCancellationCts?.Dispose();
			_userCancellationCts = null;
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
		/// Processes each record in the pipeline.
		/// </summary>
		protected override void ProcessRecord()
		{
			WriteProgressAndVerbose(new ProgressRecord(1, $"Running query '{Sql}'", _progressMessage) { PercentComplete = 0, RecordType = ProgressRecordType.Processing });


			base.ProcessRecord();

			_command.Parameters.Clear();
			if (Parameters != null)
			{
				if (Parameters.BaseObject is Hashtable parametersHashtable)
				{
					foreach (string key in parametersHashtable.Keys)
					{
						Sql4CdsParameter cmdParam = _command.CreateParameter();
						cmdParam.ParameterName = $"@{key}";
						cmdParam.Value = parametersHashtable[key] ?? DBNull.Value;
						_command.Parameters.Add(cmdParam);
					}
				}
				else
				{

					foreach (PSPropertyInfo p in Parameters.Properties)
					{
						Sql4CdsParameter cmdParam = _command.CreateParameter();
						cmdParam.ParameterName = $"@{p.Name}";
						cmdParam.Value = p.Value ?? DBNull.Value;
						_command.Parameters.Add(cmdParam);
					}
				}
			}

			if (!_commandPrepared)
			{
				_command.Prepare();
				_commandPrepared = true;
			}


			Task<DbDataReader> task = Task.Run(() =>
			{
				//Despite the Async, seems to block until completion
				return _command.ExecuteReaderAsync();
			});

			while (!task.IsCompleted)
			{
				// Check for cancellation
				if (Stopping || (_userCancellationCts != null && _userCancellationCts.IsCancellationRequested))
				{
					WriteVerbose("SQL query execution cancelled by user");
					// Note: Sql4Cds doesn't support cancellation tokens directly, so the task may continue
					// but we'll stop processing results
					return;
				}

				//Wait for either the original task to complete, or a fixed wait
				Task.WaitAny(task, Task.Delay(100));

				WriteProgressAndVerbose(new ProgressRecord(1, $"Running query '{Sql}'", _progressMessage) { PercentComplete = _progressPercentage, RecordType = ProgressRecordType.Processing });

				while (_infoMessages.TryDequeue(out var infoMessage))
				{
					WriteVerbose(infoMessage);
				}

				while(_pendingConfirmations.TryDequeue(out var pendingConfirmation))
				{
					pendingConfirmation.RunSynchronously();
				}
			}

			using (DbDataReader reader = task.Result)
			{
				if (reader.HasRows)
				{
					while (reader.Read())
					{
						// Check for cancellation during result reading
						if (Stopping || (_userCancellationCts != null && _userCancellationCts.IsCancellationRequested))
						{
							WriteVerbose("SQL query result reading cancelled by user");
							break;
						}

						PSObject output = new PSObject();

						for (int f = 0; f < reader.VisibleFieldCount; f++)
						{
							string fieldName = reader.GetName(f);
							if (string.IsNullOrEmpty(fieldName))
							{
								fieldName = $"field{f}";
                            } 
							output.Properties.Add(new PSNoteProperty(fieldName, reader.GetValue(f)));
						}

						WriteObject(output);
					}
				}

				if (reader.RecordsAffected != -1)
				{
					WriteVerbose($"{reader.RecordsAffected} records affected");
				}
			}

			WriteProgressAndVerbose(new ProgressRecord(1, @"Running query '{Sql}'", "Done") { PercentComplete = 100, RecordType = ProgressRecordType.Completed });

		}

		private void WriteProgressAndVerbose(ProgressRecord progressRecord)
		{
			WriteProgress(progressRecord);
			WriteVerbose($"{progressRecord.Activity}: {progressRecord.StatusDescription} {progressRecord.PercentComplete}%");
		}

		/// <summary>
		/// Gets an access token from the ServiceClient for TDS endpoint authentication.
		/// </summary>
		/// <param name="serviceClient">The ServiceClient instance to retrieve the token from</param>
		/// <returns>The access token, or null if unavailable</returns>
		private string GetAccessToken(ServiceClient serviceClient)
		{
			// If using ServiceClientWithTokenProvider, use the TokenProviderFunction
			if (serviceClient is ServiceClientWithTokenProvider clientWithProvider && clientWithProvider.TokenProviderFunction != null)
			{
				try
				{
					// Call the token provider function with the service URL
					// The function expects a URL parameter for scope resolution
					var tokenTask = clientWithProvider.TokenProviderFunction(serviceClient.ConnectedOrgUriActual?.ToString() ?? string.Empty);
					return tokenTask.GetAwaiter().GetResult();
				}
				catch
				{
					// If token retrieval fails, fall back to checking CurrentAccessToken
				}
			}

			// Fallback: Try to get CurrentAccessToken using reflection
			// This property is available but may be null when using external token management
			try
			{
				var currentAccessTokenProperty = serviceClient.GetType().GetProperty("CurrentAccessToken",
					System.Reflection.BindingFlags.Instance |
					System.Reflection.BindingFlags.Public |
					System.Reflection.BindingFlags.NonPublic);

				if (currentAccessTokenProperty != null)
				{
					var token = currentAccessTokenProperty.GetValue(serviceClient) as string;
					if (!string.IsNullOrEmpty(token))
					{
						return token;
					}
				}
			}
			catch
			{
				// If reflection fails, we'll return null and fall back to non-TDS mode
			}

			return null;
		}
	}
}
