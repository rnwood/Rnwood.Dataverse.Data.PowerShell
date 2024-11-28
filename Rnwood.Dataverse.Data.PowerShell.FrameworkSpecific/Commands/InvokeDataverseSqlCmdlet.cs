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

namespace Rnwood.Dataverse.Data.PowerShell.Commands
{
	[Cmdlet("Invoke", "DataverseSql")]
	///<summary>Invokes a Dataverse request.</summary>
	public class InvokeDataverseSqlCmdlet : OrganizationServiceCmdlet
	{
		private Sql4CdsConnection _sqlConnection;
		private Sql4CdsCommand _command;

		[Parameter(Mandatory = true)]
		public override ServiceClient Connection { get; set; }

		[Parameter(Mandatory = true, HelpMessage = "SQL to execute. See Sql4Cds docs.", ValueFromRemainingArguments = true)]
		public string Sql { get; set; }

		[Parameter(ValueFromPipeline =true)]
		public PSObject Parameters { get; set; }

		protected override void BeginProcessing()
		{
			base.BeginProcessing();

			_sqlConnection = new Sql4CdsConnection(Connection);
			_sqlConnection.UseTDSEndpoint = false;

			_command = _sqlConnection.CreateCommand();
			_command.CommandText = Sql;
		}

		protected override void EndProcessing()
		{
			base.EndProcessing();

			_sqlConnection.Dispose();
			_sqlConnection = null;

			_command.Dispose();
			_command = null;
		}

		protected override void ProcessRecord()
		{
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

			_command.Prepare();

			using (DbDataReader reader = _command.ExecuteReader())
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


		}
	}
}
