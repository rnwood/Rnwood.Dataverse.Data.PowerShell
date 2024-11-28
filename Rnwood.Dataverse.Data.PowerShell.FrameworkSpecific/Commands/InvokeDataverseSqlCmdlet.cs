﻿using System;
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
	[Cmdlet("Invoke", "DataverseSql", SupportsShouldProcess = true, ConfirmImpact = ConfirmImpact.Medium)]
	///<summary>Invokes a Dataverse request.</summary>
	public class InvokeDataverseSqlCmdlet : OrganizationServiceCmdlet
	{
		private Sql4CdsConnection _sqlConnection;
		private Sql4CdsCommand _command;
		private bool _commandPrepared;
		private int _progressPercentage;
		private string _progressMessage;
		private ConcurrentQueue<string> _infoMessages;
		private ConcurrentQueue<Task> _pendingConfirmations;

		[Parameter(Mandatory = true)]
		public override ServiceClient Connection { get; set; }

		[Parameter(Mandatory = true, HelpMessage = "SQL to execute. See Sql4Cds docs.", ValueFromRemainingArguments = true)]
		public string Sql { get; set; }

		[Parameter]
		public bool UseTdsEndpoint { get; set; }

		[Parameter(ValueFromPipeline = true)]
		public PSObject Parameters { get; set; }

		[Parameter(ValueFromPipeline = true)]
		public int? BatchSize { get; private set; }

		protected override void BeginProcessing()
		{
			base.BeginProcessing();

			_sqlConnection = new Sql4CdsConnection(Connection);
			_sqlConnection.UseTDSEndpoint = false;
			_sqlConnection.Progress += OnSqlConnection_Progress;
			_sqlConnection.UseTDSEndpoint = UseTdsEndpoint;
			_sqlConnection.InfoMessage += OnSqlConnection_InfoMessage;

			this._sqlConnection.PreInsert += GetOnSqlConnectionConfirmatonRequiredHandler("Create");
			this._sqlConnection.PreDelete += GetOnSqlConnectionConfirmatonRequiredHandler("Delete");
			this._sqlConnection.PreUpdate += GetOnSqlConnectionConfirmatonRequiredHandler("Update");

			if (BatchSize.HasValue)
			{
				_sqlConnection.BatchSize = BatchSize.Value;
			}

			_command = _sqlConnection.CreateCommand();
			_command.CommandText = Sql;
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
					ea.Cancel = !ShouldContinue($"{operation} {ea.Count} rows in table '{ea.Metadata.LogicalName}?'", "Continue?");
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
			_progressPercentage = (int)(100.0 * (e.Progress ?? 0));
			_progressMessage = e.Message;
		}

		protected override void EndProcessing()
		{
			base.EndProcessing();

			_sqlConnection.Dispose();
			_sqlConnection = null;

			_command.Dispose();
			_command = null;
			_commandPrepared = false;

		}

		protected override void ProcessRecord()
		{
			WriteProgress(new ProgressRecord(1, $"Running query '{Sql}'", _progressMessage) { PercentComplete = 0, RecordType = ProgressRecordType.Processing });


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
				//Wait for either the original task to complete, or a fixed wait
				Task.WaitAny(task, Task.Delay(100));

				WriteProgress(new ProgressRecord(1, $"Running query '{Sql}'", _progressMessage) { PercentComplete = _progressPercentage, RecordType = ProgressRecordType.Processing });

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
						PSObject output = new PSObject();

						for (int f = 0; f < reader.VisibleFieldCount; f++)
						{
							string fieldName = reader.GetName(f) ?? $"field{f}";
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

			WriteProgress(new ProgressRecord(1, @"Running query '{Sql}'", "Done") { PercentComplete = 100, RecordType = ProgressRecordType.Completed });

		}
	}
}