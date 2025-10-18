using Microsoft.Crm.Sdk.Messages;
using Microsoft.PowerPlatform.Dataverse.Client;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Messages;
using Microsoft.Xrm.Sdk.Metadata;
using Microsoft.Xrm.Sdk.Query;

using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Management.Automation;
using System.Text;

namespace Rnwood.Dataverse.Data.PowerShell.Commands
{
/// <summary>Creates or updates records in a Dataverse environment. If a matching record is found then it will be updated, otherwise a new record is created (some options can override this).
/// This command can also handle creation/update of intersect records (many to many relationships).</summary>
	[Cmdlet(VerbsCommon.Set, "DataverseRecord", SupportsShouldProcess = true, ConfirmImpact = ConfirmImpact.Medium)]
	public class SetDataverseRecordCmdlet : CustomLogicBypassableOrganizationServiceCmdlet
	{
	/// <summary>
	/// Object containing values to be used. Property names must match the logical names of Dataverse columns in the specified table and the property values are used to set the values of the Dataverse record being created/updated. The properties may include ownerid, statecode and statuscode which will assign and change the record state/status.
	/// </summary>
		[Parameter(Mandatory = true, ValueFromPipeline = true, ValueFromRemainingArguments = true,
			HelpMessage = "Object containing values to be used. Property names must match the logical names of Dataverse columns in the specified table and the property values are used to set the values of the Dataverse record being created/updated. The properties may include ownerid, statecode and statuscode which will assign and change the record state/status.")]
		public PSObject InputObject { get; set; }
	/// <summary>
	/// The logical name of the table to operate on.
		/// </summary>
		[Parameter(Mandatory = true, ValueFromPipelineByPropertyName = true, HelpMessage = "Logical name of table")]
		[Alias("EntityName", "LogicalName")]
		[ArgumentCompleter(typeof(TableNameArgumentCompleter))]
		public string TableName { get; set; }

		/// <summary>
		/// Controls the maximum number of requests sent to Dataverse in one batch (where possible) to improve throughput. Specify 1 to disable batching.
		/// </summary>
		[Parameter(HelpMessage = "Controls the maximum number of requests sent to Dataverse in one batch (where possible) to improve throughput. Specify 1 to disable.")]
		public uint BatchSize { get; set; } = 100;

		/// <summary>
		/// Controls the maximum number of records to retrieve in one batch when checking for existing records or resolving lookups. Specify 1 to disable batching. Default is 500.
		/// </summary>
		[Parameter(HelpMessage = "Controls the maximum number of records to retrieve in one batch when checking for existing records or resolving lookups. Specify 1 to disable batching. Default is 500.")]
		public uint RetrievalBatchSize { get; set; } = 500;

		/// <summary>
		/// List of properties on the input object which are ignored and not attempted to be mapped to the record. Default is none.
		/// </summary>
		[Parameter(Mandatory = false, ValueFromPipelineByPropertyName = true, HelpMessage = "List of properties on the input object which are ignored and not attempted to be mapped to the record. Default is none.")]
		[ArgumentCompleter(typeof(Rnwood.Dataverse.Data.PowerShell.Commands.PSObjectPropertyNameArgumentCompleter))]
		public string[] IgnoreProperties { get; set; }

		/// <summary>
		/// ID of record to be created or updated.
		/// </summary>
		[Parameter(Mandatory = false, ValueFromPipelineByPropertyName = true, HelpMessage = "ID of record to be created or updated.")]
		public Guid Id { get; set; }
	/// <summary>
	/// List of list of column names that identify an existing record to update based on the values of those columns in the InputObject. For update/create these are used if a record with an Id matching the value of the Id cannot be found. The first list that returns a match is used.
	/// </summary>
		[Parameter(Mandatory = false, HelpMessage = "List of list of column names that identify an existing record to update based on the values of those columns in the InputObject. For update/create these are used if a record with an Id matching the value of the Id cannot be found. The first list that returns a match is used.")]
		public string[][] MatchOn { get; set; }
	/// <summary>
	/// If specified, the InputObject is written to the pipeline with an Id property set indicating the primary key of the affected record (even if nothing was updated).
	/// </summary>
		[Parameter(HelpMessage = "If specified, the InputObject is written to the pipeline with an Id property set indicating the primary key of the affected record (even if nothing was updated).")]
		public SwitchParameter PassThru { get; set; }
	/// <summary>
	/// If specified, existing records matching the ID and or MatchOn columns will not be updated.
	/// </summary>
		[Parameter(HelpMessage = "If specified, existing records matching the ID and or MatchOn columns will not be updated.")]
		public SwitchParameter NoUpdate { get; set; }
	/// <summary>
	/// If specified, then no records will be created even if no existing records matching the ID and or MatchOn columns is found.
	/// </summary>
		[Parameter(HelpMessage = "If specified, then no records will be created even if no existing records matching the ID and or MatchOn columns is found.")]
		public SwitchParameter NoCreate { get; set; }
	/// <summary>
	/// List of column names which will not be included when updating existing records.
	/// </summary>
	[Parameter(HelpMessage = "List of column names which will not be included when updating existing records.")]
	[ArgumentCompleter(typeof(Rnwood.Dataverse.Data.PowerShell.Commands.ColumnNamesArgumentCompleter))]
	public string[] NoUpdateColumns { get; set; }
		/// <summary>
		/// If specified, the creation/updates will be done on behalf of the user with the specified ID. For best performance, sort the records using this value since a new batch request is needed each time this value changes.
		/// </summary>
		[Parameter(ValueFromPipelineByPropertyName = true, HelpMessage = "If specified, the creation/updates will be done on behalf of the user with the specified ID. For best performance, sort the records using this value since a new batch request is needed each time this value changes.")]
		public Guid? CallerId { get; set; }
	/// <summary>
	/// If specified, an update containing all supplied columns will be issued without retrieving the existing record for comparison (default is to remove unchanged columns). Id must be provided
	/// </summary>
		[Parameter(HelpMessage = "If specified, an update containing all supplied columns will be issued without retrieving the existing record for comparison (default is to remove unchanged columns). Id must be provided")]
		public SwitchParameter UpdateAllColumns { get; set; }
	/// <summary>
	/// If specified, no check for existing record is made and records will always be attempted to be created. Use this option when it's known that no existing matching records will exist to improve performance. See the -noupdate option for an alternative.
	/// </summary>
		[Parameter(HelpMessage = "If specified, no check for existing record is made and records will always be attempted to be created. Use this option when it's known that no existing matching records will exist to improve performance. See the -noupdate option for an alternative.")]
		public SwitchParameter CreateOnly { get; set; }
	/// <summary>
	/// If specified, upsert request will be used to create/update existing records as appropriate. -MatchOn is not supported with this option
	/// </summary>
		[Parameter(HelpMessage = "If specified, upsert request will be used to create/update existing records as appropriate. -MatchOn is not supported with this option")]
		public SwitchParameter Upsert { get; set; }
	/// <summary>
	/// Hashset mapping lookup column names in the target entity to column names in the referred-to table for resolving lookup references.
	/// </summary>
		[Parameter(Mandatory = false, HelpMessage = "Hashset of lookup column name in the target entity to column name in the referred to table with which to find the records.")]
		[ArgumentCompleter(typeof(Rnwood.Dataverse.Data.PowerShell.Commands.ColumnNamesArgumentCompleter))]
		public Hashtable LookupColumns
		{
			get;
			set;
		}
	/// <summary>
	/// Specifies the types of business logic (for example plugins) to bypass
	/// </summary>
		[Parameter(HelpMessage = "Specifies the types of business logic (for example plugins) to bypass")]
		public override BusinessLogicTypes[] BypassBusinessLogicExecution { get; set; }
	/// <summary>
	/// Specifies the IDs of plugin steps to bypass
	/// </summary>
		[Parameter(HelpMessage = "Specifies the IDs of plugin steps to bypass")]
		public override Guid[] BypassBusinessLogicExecutionStepIds { get; set; }

		private class BatchItem
		{
			public BatchItem(PSObject inputObject, OrganizationRequest request, Action<OrganizationResponse> responseCompletion) : this(inputObject, request, responseCompletion, null)
			{
			}

			public BatchItem(PSObject inputObject, OrganizationRequest request, Action<OrganizationResponse> responseCompletion, Func<OrganizationServiceFault, bool> responseExceptionCompletion)
			{
				InputObject = inputObject;
				Request = request;
				ResponseCompletion = responseCompletion;
				ResponseExceptionCompletion = responseExceptionCompletion;
			}

			public OrganizationRequest Request { get; set; }

			public Action<OrganizationResponse> ResponseCompletion { get; set; }

			public Func<OrganizationServiceFault, bool> ResponseExceptionCompletion { get; set; }

			public PSObject InputObject { get; set; }

			private object FormatValue(object value)
			{
				if (value is EntityReference er)
				{
					return $"{er.LogicalName}:{er.Id}";
				}

				if (value is Entity en)
				{
					string attributeValues = string.Join(", ", en.Attributes.Select(a => $"{a.Key}='{a.Value}'"));
					return $"{en.LogicalName} : {{{attributeValues}}}";
				}

				return value?.ToString();
			}

			public override string ToString()
			{
				return Request.RequestName + " " + string.Join(", ", Request.Parameters.Select(p => $"{p.Key}='{FormatValue(p.Value)}'"));
			}
		}

		private List<BatchItem> _nextBatchItems;
		private Guid? _nextBatchCallerId;
		
		// Batched retrieval support
		private class RetrievalBatchItem
		{
			public Guid Id { get; set; }
			public string TableName { get; set; }
			public ColumnSet ColumnSet { get; set; }
		}
		
		private class MatchOnRetrievalBatchItem
		{
			public string[] Columns { get; set; }
			public Dictionary<string, object> Values { get; set; }
			public Entity Target { get; set; }
			public ColumnSet ColumnSet { get; set; }
		}
		
		private class IntersectRetrievalBatchItem
		{
			public Guid Entity1Value { get; set; }
			public Guid Entity2Value { get; set; }
			public Entity Target { get; set; }
			public ColumnSet ColumnSet { get; set; }
			public ManyToManyRelationshipMetadata Metadata { get; set; }
		}
		
		private List<RetrievalBatchItem> _retrievalBatch;
		private List<MatchOnRetrievalBatchItem> _matchOnRetrievalBatch;
		private List<IntersectRetrievalBatchItem> _intersectRetrievalBatch;

		private void QueueBatchItem(BatchItem item, Guid? callerId)
		{
			if (_nextBatchItems.Any() && _nextBatchCallerId != callerId)
			{
				ProcessBatch();
			}

			_nextBatchCallerId = callerId;
			_nextBatchItems.Add(item);

			if (_nextBatchItems.Count == BatchSize)
			{
				ProcessBatch();
			}
		}
	/// <summary>
	/// Initializes the cmdlet.
	/// </summary>


		protected override void BeginProcessing()
		{
			base.BeginProcessing();

			recordCount = 0;
			entityMetadataFactory = new EntityMetadataFactory(Connection);
			entityConverter = new DataverseEntityConverter(Connection, entityMetadataFactory);



			if (BatchSize > 1)
			{
				_nextBatchItems = new List<BatchItem>();
			}
			
			// Initialize batched retrieval support
			_retrievalBatch = new List<RetrievalBatchItem>();
			_matchOnRetrievalBatch = new List<MatchOnRetrievalBatchItem>();
			_intersectRetrievalBatch = new List<IntersectRetrievalBatchItem>();
		}

		private void AppendFaultDetails(OrganizationServiceFault fault, StringBuilder output)
		{
			output.AppendLine("OrganizationServiceFault " + fault.ErrorCode + ": " + fault.Message);
			output.AppendLine(fault.TraceText);

			if (fault.InnerFault != null)
			{
				output.AppendLine("---");
				AppendFaultDetails(fault.InnerFault, output);
			}
		}

		private string GetMatchOnCacheKey(string[] columns, Dictionary<string, object> values)
		{
			// Create a cache key from the column names and values
			return string.Join("|", columns.OrderBy(c => c).Select(c => $"{c}={values[c]}"));
		}

		private Dictionary<string, Entity> ProcessMatchOnRetrievalBatch()
		{
			var results = new Dictionary<string, Entity>();
			
			if (_matchOnRetrievalBatch == null || _matchOnRetrievalBatch.Count == 0)
			{
				return results;
			}

			WriteVerbose(string.Format("Processing MatchOn retrieval batch of {0} records", _matchOnRetrievalBatch.Count));

			// Group by column combination to batch similar queries
			var groupedByColumns = _matchOnRetrievalBatch.GroupBy(r => string.Join(",", r.Columns.OrderBy(c => c)));

			foreach (var columnGroup in groupedByColumns)
			{
				var items = columnGroup.ToArray();
				var columns = items[0].Columns;

				// For single-column MatchOn, we can use In operator for efficient batching
				if (columns.Length == 1)
				{
					string columnName = columns[0];
					var values = items.Select(i => i.Values[columnName]).Distinct().ToArray();

					QueryExpression query = new QueryExpression(TableName);
					query.Criteria.AddCondition(columnName, ConditionOperator.In, values);
					query.ColumnSet = items[0].ColumnSet;
					query.TopCount = values.Length + 1; // Add 1 to detect duplicates

					WriteVerbose(string.Format("Retrieving {0} {1} records by {2} in batch", values.Length, TableName, columnName));

					EntityCollection entities = Connection.RetrieveMultiple(query);

					// Return results by the match value
					foreach (Entity entity in entities.Entities)
					{
						object matchValue = entity.GetAttributeValue<object>(columnName);
						if (matchValue is EntityReference er)
						{
							matchValue = er.Id;
						}
						else if (matchValue is OptionSetValue osv)
						{
							matchValue = osv.Value;
						}

						string cacheKey = $"{columnName}={matchValue}";
						
						// Only add if unique
						if (!results.ContainsKey(cacheKey))
						{
							results[cacheKey] = entity;
						}
					}
				}
				else
				{
					// For multi-column MatchOn, use Or conditions to batch multiple queries
					QueryExpression query = new QueryExpression(TableName);
					query.ColumnSet = items[0].ColumnSet;

					FilterExpression orFilter = new FilterExpression(LogicalOperator.Or);

					foreach (var item in items)
					{
						FilterExpression andFilter = new FilterExpression(LogicalOperator.And);
						foreach (string column in columns)
						{
							object value = item.Values[column];
							andFilter.AddCondition(column, ConditionOperator.Equal, value);
						}
						orFilter.AddFilter(andFilter);
					}

					query.Criteria = orFilter;

					WriteVerbose(string.Format("Retrieving {0} {1} records by {2} in batch", items.Length, TableName, string.Join(",", columns)));

					EntityCollection entities = Connection.RetrieveMultiple(query);

					// Return results
					foreach (Entity entity in entities.Entities)
					{
						var valueDict = new Dictionary<string, object>();
						foreach (string column in columns)
						{
							object value = entity.GetAttributeValue<object>(column);
							if (value is EntityReference er)
							{
								value = er.Id;
							}
							else if (value is OptionSetValue osv)
							{
								value = osv.Value;
							}
							valueDict[column] = value;
						}

						string cacheKey = GetMatchOnCacheKey(columns, valueDict);
						results[cacheKey] = entity;
					}
				}
			}

			_matchOnRetrievalBatch.Clear();
			return results;
		}

		private Dictionary<string, Entity> ProcessIntersectRetrievalBatch()
		{
			var results = new Dictionary<string, Entity>();
			
			if (_intersectRetrievalBatch == null || _intersectRetrievalBatch.Count == 0)
			{
				return results;
			}

			WriteVerbose(string.Format("Processing intersect retrieval batch of {0} records", _intersectRetrievalBatch.Count));

			// All items should have the same metadata
			var firstItem = _intersectRetrievalBatch[0];
			var metadata = firstItem.Metadata;

			// Build a query with Or conditions for all entity pairs
			QueryExpression query = new QueryExpression(TableName);
			query.ColumnSet = firstItem.ColumnSet;

			FilterExpression orFilter = new FilterExpression(LogicalOperator.Or);

			foreach (var item in _intersectRetrievalBatch)
			{
				FilterExpression andFilter = new FilterExpression(LogicalOperator.And);
				andFilter.AddCondition(metadata.Entity1IntersectAttribute, ConditionOperator.Equal, item.Entity1Value);
				andFilter.AddCondition(metadata.Entity2IntersectAttribute, ConditionOperator.Equal, item.Entity2Value);
				orFilter.AddFilter(andFilter);
			}

			query.Criteria = orFilter;

			WriteVerbose(string.Format("Retrieving {0} {1} intersect records in batch", _intersectRetrievalBatch.Count, TableName));

			EntityCollection entities = Connection.RetrieveMultiple(query);

			// Return results
			foreach (Entity entity in entities.Entities)
			{
				Guid entity1Value = entity.GetAttributeValue<Guid>(metadata.Entity1IntersectAttribute);
				Guid entity2Value = entity.GetAttributeValue<Guid>(metadata.Entity2IntersectAttribute);
				string cacheKey = $"{entity1Value}|{entity2Value}";
				results[cacheKey] = entity;
			}

			_intersectRetrievalBatch.Clear();
			return results;
		}

		private Dictionary<Guid, Entity> ProcessRetrievalBatch()
		{
			var results = new Dictionary<Guid, Entity>();
			
			if (_retrievalBatch == null || _retrievalBatch.Count == 0)
			{
				return results;
			}

			WriteVerbose(string.Format("Processing retrieval batch of {0} records", _retrievalBatch.Count));

			// Group by table name for efficient querying
			var groupedByTable = _retrievalBatch.GroupBy(r => r.TableName);

			foreach (var tableGroup in groupedByTable)
			{
				string tableName = tableGroup.Key;
				var ids = tableGroup.Select(r => r.Id).Distinct().ToArray();

				// Build a query to retrieve all records by ID using In operator
				QueryExpression query = new QueryExpression(tableName);
				query.Criteria.AddCondition(entityMetadataFactory.GetMetadata(tableName).PrimaryIdAttribute, ConditionOperator.In, ids.Cast<object>().ToArray());
				
				// Use the column set from the first record of this table (they should all be similar for efficiency)
				var firstRecord = tableGroup.First();
				query.ColumnSet = firstRecord.ColumnSet;

				WriteVerbose(string.Format("Retrieving {0} {1} records in batch", ids.Length, tableName));

				EntityCollection entities = Connection.RetrieveMultiple(query);

				foreach (Entity entity in entities.Entities)
				{
					results[entity.Id] = entity;
				}
			}

			_retrievalBatch.Clear();
			return results;
		}

		private void ProcessBatch()
		{
			if (_nextBatchItems.Count > 0 &&
				ShouldProcess("Execute batch of requests:\n" + string.Join("\n", _nextBatchItems.Select(r => r.ToString()))))
			{

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
				ApplyBypassBusinessLogicExecution(batchRequest);

				batchRequest.Requests.AddRange(_nextBatchItems.Select(i => i.Request));

				Guid oldCallerId = Connection.CallerId;
				Connection.CallerId = _nextBatchCallerId.GetValueOrDefault();
				try
				{
					ExecuteMultipleResponse response = (ExecuteMultipleResponse)Connection.Execute(batchRequest);

					foreach (var itemResponse in response.Responses)
					{
						BatchItem batchItem = _nextBatchItems[itemResponse.RequestIndex];

						if (itemResponse.Fault != null)
						{
							if (batchItem.ResponseExceptionCompletion != null && batchItem.ResponseExceptionCompletion(itemResponse.Fault))
							{
								//Handled error
							}
							else
							{
								StringBuilder details = new StringBuilder();
								AppendFaultDetails(itemResponse.Fault, details);

								WriteError(new ErrorRecord(new Exception(details.ToString()), null, ErrorCategory.InvalidResult, batchItem.InputObject));
							}
						}
						else
						{
							batchItem.ResponseCompletion(itemResponse.Response);
						}
					}

				}
				finally
				{
					Connection.CallerId = oldCallerId;
				}
			}

			_nextBatchItems.Clear();
			_nextBatchCallerId = null;
		}
	/// <summary>
	/// Completes cmdlet processing.
	/// </summary>


		protected override void EndProcessing()
		{
			base.EndProcessing();

			// Flush any remaining retrieval batches
			if (_retrievalBatch != null)
			{
				ProcessRetrievalBatch();
			}

			if (_matchOnRetrievalBatch != null)
			{
				ProcessMatchOnRetrievalBatch();
			}

			if (_intersectRetrievalBatch != null)
			{
				ProcessIntersectRetrievalBatch();
			}

			if (_nextBatchItems != null)
			{
				ProcessBatch();
			}
		}

		private int recordCount;
		private EntityMetadataFactory entityMetadataFactory;
		private DataverseEntityConverter entityConverter;

		private ConvertToDataverseEntityOptions GetConversionOptions()
		{
			ConvertToDataverseEntityOptions options = new ConvertToDataverseEntityOptions();


			options.IgnoredPropertyName.Add("TableName");
			options.IgnoredPropertyName.Add("EntityName");
			options.IgnoredPropertyName.Add("Id");

			if (IgnoreProperties != null)
			{
				foreach (string ignoreProperty in IgnoreProperties)
				{
					options.IgnoredPropertyName.Add(ignoreProperty);
				}
			}

			if (LookupColumns != null)
			{
				foreach (DictionaryEntry lookupColumn in LookupColumns)
				{
					options.ColumnOptions[(string)lookupColumn.Key] = new ConvertToDataverseEntityColumnOptions() { LookupColumn = (string)lookupColumn.Value };
				}
			}

			return options;
		}
	/// <summary>
	/// Processes each record in the pipeline.
	/// </summary>


		protected override void ProcessRecord()
		{
			base.ProcessRecord();

			if (CallerId.HasValue)
			{
				if (BatchSize > 1)
				{
					throw new ArgumentException("CreatedOnBehalfBy not supported with BatchSize > 1");
				}
			}

			recordCount++;
			WriteVerbose("Processing record #" + recordCount);

			Entity target;

			try
			{
				target = entityConverter.ConvertToDataverseEntity(InputObject, TableName, GetConversionOptions());
			}
			catch (FormatException e)
			{
				WriteError(new ErrorRecord(new Exception("Error converting input object: " + e.Message, e), null, ErrorCategory.InvalidData, InputObject));
				return;
			}

			EntityMetadata entityMetadata = entityMetadataFactory.GetMetadata(TableName);

			Entity existingRecord;

			try
			{
				existingRecord = GetExistingRecord(entityMetadata, target);
			}
			catch (Exception e)
			{
				WriteError(new ErrorRecord(e, null, ErrorCategory.InvalidOperation, InputObject)); ;
				return;
			}

			if (Upsert.IsPresent)
			{
				UpsertRecord(entityMetadata, target);
			}
			else
			{
				if (existingRecord != null)
				{
					UpdateExistingRecord(entityMetadata, target, existingRecord);
				}
				else
				{
					CreateNewRecord(entityMetadata, target);
				}
			}

			//Skip assignment/status if record creation failed.
			if ((existingRecord != null && !NoUpdate) || (existingRecord == null && !NoCreate))
			{
				if (target.Contains("ownerid"))
				{
					EntityReference ownerid = target.GetAttributeValue<EntityReference>("ownerid");
					AssignRequest request = new AssignRequest()
					{
						Assignee = ownerid,
						Target = target.ToEntityReference()
					};
					ApplyBypassBusinessLogicExecution(request);

					if (_nextBatchItems != null)
					{
						WriteVerbose(string.Format("Added assignment of record {0}:{1} to {2} to batch", TableName, target.Id, ownerid.Name));
						QueueBatchItem(new BatchItem(InputObject, request, (response) => { AssignRecordCompletion(target, ownerid); }), CallerId);
					}
					else
					{
						if (ShouldProcess(string.Format("Assign record {0}:{1} to {2}", TableName, target.Id, ownerid.Name)))
						{
							try
							{
								Connection.Execute(request);
								AssignRecordCompletion(target, ownerid);
							}
							catch (Exception e)
							{
								WriteError(new ErrorRecord(e, null, ErrorCategory.WriteError, InputObject));
							}
						}
					}
				}

				if (target.Contains("statecode") || target.Contains("statuscode"))
				{
					OptionSetValue statuscode = target.GetAttributeValue<OptionSetValue>("statuscode") ?? new OptionSetValue(-1);

					OptionSetValue stateCode = target.GetAttributeValue<OptionSetValue>("statecode");

					if (stateCode == null)
					{
						StatusAttributeMetadata statusMetadata =
							(StatusAttributeMetadata)entityMetadata.Attributes.First(
								a => a.LogicalName.Equals("statuscode", StringComparison.OrdinalIgnoreCase));

						stateCode = new OptionSetValue(statusMetadata.OptionSet.Options.Cast<StatusOptionMetadata>()
								.First(o => o.Value.Value == statuscode.Value)
								.State.Value);
					}

					SetStateRequest request = new SetStateRequest()
					{
						EntityMoniker = target.ToEntityReference(),
						State = stateCode,
						Status = statuscode
					};
					ApplyBypassBusinessLogicExecution(request);

					if (_nextBatchItems != null)
					{
						WriteVerbose(string.Format("Added set record {0}:{1} status to State:{2} Status: {3} to batch", TableName, Id, stateCode.Value, statuscode.Value));
						QueueBatchItem(new BatchItem(InputObject, request, (response) => { SetStateCompletion(target, statuscode, stateCode); }), CallerId);
					}
					else
					{
						if (ShouldProcess(string.Format("Set record {0}:{1} status to State:{2} Status: {3}", TableName, Id, stateCode.Value, statuscode.Value)))
						{
							try
							{
								Connection.Execute(request);
								SetStateCompletion(target, statuscode, stateCode);
							}
							catch (Exception e)
							{
								WriteError(new ErrorRecord(e, null, ErrorCategory.WriteError, InputObject));
							}
						}
					}
				}
			}
		}

		private void SetStateCompletion(Entity target, OptionSetValue statuscode, OptionSetValue stateCode)
		{
			WriteVerbose(string.Format("Record {0}:{1} status set to State:{2} Status: {3}", target.LogicalName, target.Id, stateCode.Value, statuscode.Value));
		}

		private void AssignRecordCompletion(Entity target, EntityReference ownerid)
		{
			WriteVerbose(string.Format("Record {0}:{1} assigned to {2}", target.LogicalName, target.Id, ownerid.Name));
		}

		private bool UpsertRecord(EntityMetadata entityMetadata, Entity target)
		{
			bool result = true;

			if (NoCreate || NoUpdate)
			{
				throw new ArgumentException("-NoCreate and -NoUpdate are not supported with -Upsert");
			}

			if (entityMetadata.IsIntersect.GetValueOrDefault())
			{
				if (MatchOn != null)
				{
					throw new ArgumentException("-MatchOn is not supported for -Upsert of M:M");
				}

				ManyToManyRelationshipMetadata manyToManyRelationshipMetadata = entityMetadata.ManyToManyRelationships[0];

				EntityReference record1 = new EntityReference(manyToManyRelationshipMetadata.Entity1LogicalName,
						 target.GetAttributeValue<Guid>(
							 manyToManyRelationshipMetadata.Entity1IntersectAttribute));
				EntityReference record2 = new EntityReference(manyToManyRelationshipMetadata.Entity2LogicalName,
						 target.GetAttributeValue<Guid>(
							 manyToManyRelationshipMetadata.Entity2IntersectAttribute));

				AssociateRequest request = new AssociateRequest()
				{
					Target = record1,
					RelatedEntities =
							new EntityReferenceCollection()
					{
								record2
					},
					Relationship = new Relationship(manyToManyRelationshipMetadata.SchemaName)
					{
						PrimaryEntityRole = EntityRole.Referencing
					},
				};
				ApplyBypassBusinessLogicExecution(request);

				if (_nextBatchItems != null)
				{
					WriteVerbose(string.Format("Added create of new intersect record {0}:{1},{2} to batch", TableName, record1.Id, record2.Id));
					QueueBatchItem(new BatchItem(InputObject, request, (response) => { AssociateUpsertCompletion(true, target, InputObject, manyToManyRelationshipMetadata, record1, record2); }, fault => { return AssociateUpsertError(fault, target, InputObject, manyToManyRelationshipMetadata, record1, record2); }), CallerId);

					if (PassThru.IsPresent)
					{
						QueryExpression getIdQuery = new QueryExpression(TableName);
						getIdQuery.Criteria.AddCondition(manyToManyRelationshipMetadata.Entity1IntersectAttribute, ConditionOperator.Equal, record1.Id);
						getIdQuery.Criteria.AddCondition(manyToManyRelationshipMetadata.Entity2IntersectAttribute, ConditionOperator.Equal, record2.Id);
						QueueBatchItem(new BatchItem(InputObject, new RetrieveMultipleRequest() { Query = getIdQuery }, response =>
						{
							AssociateUpsertGetIdCompletion(response, InputObject);
						}), CallerId);
					}
				}
				else
				{
					throw new NotSupportedException("Upsert not supported for insertsect entities except in batch mode");
				}
			}
			else
			{
				Entity targetUpdate = new Entity(target.LogicalName) { Id = target.Id };

				if (MatchOn != null)
				{
					if (MatchOn.Length > 1)
					{
						throw new NotSupportedException("MatchOn must only have a single array when used with Upsert");
					}

					var key = entityMetadataFactory.GetMetadata(target.LogicalName).Keys.FirstOrDefault(k => k.KeyAttributes.Length == MatchOn[0].Length && k.KeyAttributes.All(a => MatchOn[0].Contains(a)));
					if (key == null)
					{
						throw new ArgumentException($"MatchOn must match a key that is defined on the table");
					}

					targetUpdate.KeyAttributes = new KeyAttributeCollection();

					foreach (var matchOnField in MatchOn[0])
					{
						targetUpdate.KeyAttributes.Add(matchOnField, target.GetAttributeValue<object>(matchOnField));
					}
				}

				targetUpdate.Attributes.AddRange(target.Attributes.Where(a => !dontUpdateDirectlyColumnNames.Contains(a.Key, StringComparer.OrdinalIgnoreCase)));

				string columnSummary = GetColumnSummary(targetUpdate);

				UpsertRequest request = new UpsertRequest()
				{
					Target = targetUpdate
				};

				if (_nextBatchItems != null)
				{
					if (targetUpdate.Id == Guid.Empty && targetUpdate.KeyAttributes.Count == 0)
					{
						targetUpdate.Id = Guid.NewGuid();
					}

					var inputObject = InputObject;

					WriteVerbose(string.Format("Added upsert of new record {0}:{1} to batch - columns:\n{2}", TableName, GetKeySummary(targetUpdate), columnSummary));
					QueueBatchItem(new BatchItem(InputObject, request, (response) => { UpsertCompletion(targetUpdate, inputObject, (UpsertResponse)response); }), CallerId);
				}
				else
				{
					if (ShouldProcess(string.Format("Upsert record {0}:{1} columns:\n{2}", TableName, GetKeySummary(targetUpdate), columnSummary)))
					{
						try
						{
							UpsertResponse response = (UpsertResponse)Connection.Execute(request);
							UpsertCompletion(targetUpdate, InputObject, response);
						}
						catch (Exception e)
						{
							result = false;
							WriteError(new ErrorRecord(new Exception(string.Format("Error creating record {0}:{1} {2}, columns: {3}", TableName, GetKeySummary(targetUpdate), e.Message, columnSummary), e), null, ErrorCategory.InvalidResult, InputObject));
						}
					}
				}
			}

			return result;
		}

		private static string GetKeySummary(Entity record)
		{
			if (record.Id != Guid.Empty)
			{
				return record.Id.ToString();
			}

			if (record.KeyAttributes.Any())
			{
				return string.Join(",", record.KeyAttributes.Select(kvp => $"{kvp.Key}={kvp.Value}"));
			}

			return "<No ID>"; 
		}

		private void AssociateUpsertGetIdCompletion(OrganizationResponse response, PSObject inputObject)
		{
			Guid id = ((RetrieveMultipleResponse)response).EntityCollection.Entities.Single().Id;

			if (inputObject.Properties.Any(p => p.Name.Equals("Id", StringComparison.OrdinalIgnoreCase)))
			{
				inputObject.Properties.Remove("Id");
			}
			inputObject.Properties.Add(new PSNoteProperty("Id", id));

			WriteObject(inputObject);
		}

		private void UpsertCompletion(Entity targetUpdate, PSObject inputObject, UpsertResponse response)
		{
			targetUpdate.Id = response.Target.Id;

			if (inputObject.Properties.Any(p => p.Name.Equals("Id", StringComparison.OrdinalIgnoreCase)))
			{
				inputObject.Properties.Remove("Id");
			}

			inputObject.Properties.Add(new PSNoteProperty("Id", targetUpdate.Id));

			string columnSummary = GetColumnSummary(targetUpdate);

			if (response.RecordCreated)
			{
				WriteVerbose(string.Format("Upsert created new record {0}:{1} columns:\n{2}", TableName, GetKeySummary(targetUpdate), columnSummary));
			}
			else
			{
				WriteVerbose(string.Format("Upsert updated existing record {0}:{1} columns:\n{2}", TableName, GetKeySummary(targetUpdate), columnSummary));
			}

			if (PassThru.IsPresent)
			{
				WriteObject(inputObject);
			}
		}

		private void CreateNewRecord(EntityMetadata entityMetadata, Entity target)
		{
			if (NoCreate.IsPresent)
			{
				WriteVerbose(string.Format("Skipped creating new record {0}:{1} - NoCreate enabled", TableName, Id));
				return;
			}

			if (entityMetadata.IsIntersect.GetValueOrDefault())
			{
				ManyToManyRelationshipMetadata manyToManyRelationshipMetadata = entityMetadata.ManyToManyRelationships[0];

				EntityReference record1 = new EntityReference(manyToManyRelationshipMetadata.Entity1LogicalName,
						 target.GetAttributeValue<Guid>(
							 manyToManyRelationshipMetadata.Entity1IntersectAttribute));
				EntityReference record2 = new EntityReference(manyToManyRelationshipMetadata.Entity2LogicalName,
						 target.GetAttributeValue<Guid>(
							 manyToManyRelationshipMetadata.Entity2IntersectAttribute));

				AssociateRequest request = new AssociateRequest()
				{
					Target = record1,
					RelatedEntities =
							new EntityReferenceCollection()
					{
								record2
					},
					Relationship = new Relationship(manyToManyRelationshipMetadata.SchemaName)
					{
						PrimaryEntityRole = EntityRole.Referencing
					},
				};
				ApplyBypassBusinessLogicExecution(request);

				if (_nextBatchItems != null)
				{
					WriteVerbose(string.Format("Added create of new intersect record {0}:{1},{2} to batch", TableName, record1.Id, record2.Id));
					QueueBatchItem(new BatchItem(InputObject, request, (response) => { AssociateCompletion(target, InputObject, manyToManyRelationshipMetadata, record1, record2); }), CallerId);
				}
				else
				{
					if (ShouldProcess(string.Format("Create new intersect record {0}", TableName)))
					{
						try
						{
							if (CallerId.HasValue)
							{
								Connection.CallerId = CallerId.Value;
							}
							Connection.Execute(request);
							if (CallerId.HasValue)
							{
								Connection.CallerId = Guid.Empty;
							}

							AssociateCompletion(target, InputObject, manyToManyRelationshipMetadata, record1, record2);
						}
						catch (Exception e)
						{
							if (CallerId.HasValue)
							{
								Connection.CallerId = Guid.Empty;
							}
							WriteError(new ErrorRecord(e, null, ErrorCategory.WriteError, InputObject));
						}
					}
				}
			}
			else
			{
				Entity targetCreate = new Entity(target.LogicalName) { Id = target.Id };
				targetCreate.Attributes.AddRange(target.Attributes.Where(a => !dontUpdateDirectlyColumnNames.Contains(a.Key, StringComparer.OrdinalIgnoreCase)));

				string columnSummary = GetColumnSummary(targetCreate);

				CreateRequest request = new CreateRequest()
				{
					Target = targetCreate
				};
				ApplyBypassBusinessLogicExecution(request);

				if (_nextBatchItems != null)
				{
					WriteVerbose(string.Format("Added created of new record {0}:{1} to batch - columns:\n{2}", TableName, targetCreate.Id, columnSummary));
					var inputObject = InputObject;
					QueueBatchItem(new BatchItem(InputObject, request, (response) => { CreateCompletion(target, inputObject, targetCreate, columnSummary, (CreateResponse)response); }), CallerId);
				}
				else
				{
					if (ShouldProcess(string.Format("Create new record {0}:{1} columns:\n{2}", TableName, targetCreate.Id, columnSummary)))
					{
						try
						{
							if (CallerId.HasValue)
							{
								Connection.CallerId = CallerId.Value;
							}

							CreateResponse response = (CreateResponse)Connection.Execute(request);

							if (CallerId.HasValue)
							{
								Connection.CallerId = Guid.Empty;
							}

							CreateCompletion(target, InputObject, targetCreate, columnSummary, response);
						}
						catch (Exception e)
						{
							if (CallerId.HasValue)
							{
								Connection.CallerId = Guid.Empty;
							}

							WriteError(new ErrorRecord(new Exception(string.Format("Error creating record {0}:{1} {2}, columns: {3}", TableName, targetCreate.Id, e.Message, columnSummary), e), null, ErrorCategory.InvalidResult, InputObject));
						}
					}
				}
			}
		}

		private void CreateCompletion(Entity target, PSObject inputObject, Entity targetCreate, string columnSummary, CreateResponse response)
		{
			if (inputObject.Properties.Any(p => p.Name.Equals("Id", StringComparison.OrdinalIgnoreCase)))
			{
				inputObject.Properties.Remove("Id");
			}

			inputObject.Properties.Add(new PSNoteProperty("Id", response.id));
			WriteVerbose(string.Format("Created new record {0}:{1} columns:\n{2}", target.LogicalName, targetCreate.Id, columnSummary));

			if (PassThru.IsPresent)
			{
				WriteObject(inputObject);
			}
		}

		private bool AssociateUpsertError(OrganizationServiceFault fault, Entity target, PSObject inputObject, ManyToManyRelationshipMetadata manyToManyRelationshipMetadata, EntityReference record1, EntityReference record2)
		{
			if (fault.ErrorCode != -2147220937)
			{
				return false;
			}

			AssociateUpsertCompletion(false, target, inputObject, manyToManyRelationshipMetadata, record1, record2);

			return true;
		}

		private void AssociateUpsertCompletion(bool recordWasCreated, Entity target, PSObject inputObject, ManyToManyRelationshipMetadata manyToManyRelationshipMetadata, EntityReference record1, EntityReference record2)
		{
			if (recordWasCreated)
			{
				WriteVerbose(string.Format("Created intersect record {0}:{1}:{2}", target.LogicalName, record1.Id, record2.Id));
			}
			else
			{
				WriteVerbose(string.Format("Skipped creating (upsert) intersect record as already exists {0}:{1}:{2}", target.LogicalName, record1.Id, record2.Id));
			}
		}

		private void AssociateCompletion(Entity target, PSObject inputObject, ManyToManyRelationshipMetadata manyToManyRelationshipMetadata, EntityReference record1, EntityReference record2)
		{
			QueryExpression getIdQuery = new QueryExpression(TableName);
			getIdQuery.Criteria.AddCondition(manyToManyRelationshipMetadata.Entity1IntersectAttribute, ConditionOperator.Equal, record1.Id);
			getIdQuery.Criteria.AddCondition(manyToManyRelationshipMetadata.Entity2IntersectAttribute, ConditionOperator.Equal, record2.Id);
			Guid id = Connection.RetrieveMultiple(getIdQuery).Entities.Single().Id;

			if (inputObject.Properties.Any(p => p.Name.Equals("Id", StringComparison.OrdinalIgnoreCase)))
			{
				inputObject.Properties.Remove("Id");
			}
			inputObject.Properties.Add(new PSNoteProperty("Id", id));
			WriteVerbose(string.Format("Created new intersect record {0}:{1}", target.LogicalName, id));

			if (PassThru.IsPresent)
			{
				WriteObject(inputObject);
			}
		}

		private string Ellipsis(string value)
		{
			if (value == null)
			{
				return null;
			}

			if (value.Length <= 100)
			{
				return value;
			}

			return value.Substring(0, 100) + "...";
		}

		private string GetColumnSummary(Entity entity)
        {
            DataverseEntityConverter converter = new DataverseEntityConverter(Connection, entityMetadataFactory);
            PSObject psObject = converter.ConvertToPSObject(entity, new ColumnSet(entity.Attributes.Select(a => a.Key).ToArray()), a => ValueType.Raw);

            return string.Join("\n", psObject.Properties.Select(a => a.Name + " = " + Ellipsis((GetValueSummary(a.Value)).ToString())));
        }

        private static object GetValueSummary(object value)
        {
			if ((!(value is string)) && value is IEnumerable enumberable)
			{
				return "[" + string.Join(", ", enumberable.Cast<object>().Select( i=>GetValueSummary(i))) + "]";
			}

            return value ?? "<null>";
        }

        private string[] dontUpdateDirectlyColumnNames = new[] { "statuscode", "statecode", "ownerid" };

		private void UpdateExistingRecord(EntityMetadata entityMetadata, Entity target, Entity existingRecord)
		{
			if (NoUpdate.IsPresent)
			{
				WriteVerbose(string.Format("Skipped updated existing record {0}:{1} - NoUpdate enabled", TableName, Id));
				return;
			}

			target.Id = existingRecord.Id;
			target[entityMetadata.PrimaryIdAttribute] = existingRecord[entityMetadata.PrimaryIdAttribute];

			if (InputObject.Properties.Any(p => p.Name.Equals("Id", StringComparison.OrdinalIgnoreCase)))
			{
				InputObject.Properties.Remove("Id");
			}
			InputObject.Properties.Add(new PSNoteProperty("Id", existingRecord.Id));

			RemoveUnchangedColumns(target, existingRecord);

			if (NoUpdateColumns != null)
			{
				foreach (string noUpdateColumns in NoUpdateColumns)
				{
					target.Attributes.Remove(noUpdateColumns);
				}
			}

			Entity targetUpdate = new Entity(target.LogicalName);
			targetUpdate.Attributes.AddRange(target.Attributes.Where(a => !dontUpdateDirectlyColumnNames.Contains(a.Key, StringComparer.OrdinalIgnoreCase)));

			if (entityMetadata.IsIntersect.GetValueOrDefault())
			{
				if (PassThru.IsPresent)
				{
					WriteObject(InputObject);
				}
			}
			else if (targetUpdate.Attributes.Any())
			{
				DataverseEntityConverter converter = new DataverseEntityConverter(Connection, entityMetadataFactory);

				UpdateRequest request = new UpdateRequest() { Target = target };
				ApplyBypassBusinessLogicExecution(request);
				string updatedColumnSummary = GetColumnSummary(targetUpdate);

				if (_nextBatchItems != null)
				{
					WriteVerbose(string.Format("Added updated of existing record {0}:{1} to batch - columns:\n{2}", TableName, existingRecord.Id, updatedColumnSummary));
					var inputObject = InputObject;
					QueueBatchItem(new BatchItem(InputObject, request, (response) => { UpdateCompletion(target, inputObject, existingRecord, updatedColumnSummary); }), CallerId);
				}
				else
				{
					if (this.ShouldProcess(string.Format("Update existing record {0}:{1} columns:\n{2}", TableName, existingRecord.Id, updatedColumnSummary)))
					{
						try
						{
							Connection.Execute(request);
							UpdateCompletion(target, InputObject, existingRecord, updatedColumnSummary);
						}
						catch (Exception e)
						{
							WriteError(new ErrorRecord(new Exception(string.Format("Error updating record {0}:{1}, {2} columns: {3}", TableName, existingRecord.Id, e.Message, updatedColumnSummary), e), null, ErrorCategory.InvalidResult, InputObject));
						}
					}
				}
			}
			else
			{
				WriteVerbose(string.Format("Skipped updated existing record {0}:{1} - nothing changed", TableName, Id));

				if (PassThru.IsPresent)
				{
					WriteObject(InputObject);
				}
			}
		}

		private void UpdateCompletion(Entity target, PSObject inputObject, Entity existingRecord, string updatedColumnSummary)
		{
			WriteVerbose(string.Format("Updated existing record {0}:{1} columns:\n{2}", target.LogicalName, existingRecord.Id, updatedColumnSummary));

			if (PassThru.IsPresent)
			{
				WriteObject(inputObject);
			}
		}

		private void QueueRetrievalIfNeeded(Guid id, string tableName, ColumnSet columnSet)
		{
			_retrievalBatch.Add(new RetrievalBatchItem
			{
				Id = id,
				TableName = tableName,
				ColumnSet = columnSet
			});
		}

		private Entity GetExistingRecord(EntityMetadata entityMetadata, Entity target)
		{
			Entity existingRecord = null;

			if (CreateOnly.IsPresent || Upsert.IsPresent)
			{
				return null;
			}

			if (!entityMetadata.IsIntersect.GetValueOrDefault())
			{
				if (Id != Guid.Empty)
				{
					if (UpdateAllColumns.IsPresent)
					{
						existingRecord = new Entity(target.LogicalName) { Id = Id };
						existingRecord[entityMetadata.PrimaryIdAttribute] = Id;
					}
					else
					{
						ColumnSet columnSet = target.LogicalName.Equals("calendar", StringComparison.OrdinalIgnoreCase) ? new ColumnSet(true) : new ColumnSet(target.Attributes.Select(a => a.Key).ToArray());

						// Queue for batched retrieval
						QueueRetrievalIfNeeded(Id, TableName, columnSet);
						
						// Process batch when full or immediately if batch size is 1
						if (_retrievalBatch.Count >= RetrievalBatchSize)
						{
							var results = ProcessRetrievalBatch();
							results.TryGetValue(Id, out existingRecord);
							return existingRecord;
						}
						
						// If we haven't hit the batch size yet, process immediately anyway
						// (This ensures we get the record for this call)
						var immediateResults = ProcessRetrievalBatch();
						immediateResults.TryGetValue(Id, out existingRecord);
						return existingRecord;
					}
				}

				if (existingRecord == null && MatchOn != null)
				{
					foreach (string[] matchOnColumnList in MatchOn)
					{
						var values = new Dictionary<string, object>();
						foreach (string matchOnColumn in matchOnColumnList)
						{
							object queryValue = target.GetAttributeValue<object>(matchOnColumn);

							if (queryValue is EntityReference)
							{
								queryValue = ((EntityReference)queryValue).Id;
							}

							if (queryValue is OptionSetValue)
							{
								queryValue = ((OptionSetValue)queryValue).Value;
							}

							values[matchOnColumn] = queryValue;
						}

						string cacheKey = GetMatchOnCacheKey(matchOnColumnList, values);
						ColumnSet columnSet = new ColumnSet(target.Attributes.Select(a => a.Key).ToArray());

						// Queue for batched retrieval
						_matchOnRetrievalBatch.Add(new MatchOnRetrievalBatchItem
						{
							Columns = matchOnColumnList,
							Values = values,
							Target = target,
							ColumnSet = columnSet
						});

						// Process batch when full or immediately
						Dictionary<string, Entity> results;
						if (_matchOnRetrievalBatch.Count >= RetrievalBatchSize)
						{
							results = ProcessMatchOnRetrievalBatch();
						}
						else
						{
							// Process immediately to get the result
							results = ProcessMatchOnRetrievalBatch();
						}

						// Check results
						if (results.TryGetValue(cacheKey, out existingRecord))
						{
							break;
						}
					}
				}
			}
			else
			{
				if (MatchOn != null)
				{
					throw new ArgumentException("MatchOn not supported for intersect entities", "MatchOn");
				}

				ManyToManyRelationshipMetadata manyToManyRelationshipMetadata = entityMetadata.ManyToManyRelationships[0];

				Guid? entity1Value =
					target.GetAttributeValue<Guid?>(manyToManyRelationshipMetadata.Entity1IntersectAttribute);
				Guid? entity2Value =
					target.GetAttributeValue<Guid?>(manyToManyRelationshipMetadata.Entity2IntersectAttribute);

				if (entity1Value == null || entity2Value == null)
				{
					throw new Exception("For intersect entities (many to many relationships), The input object must contain values for both attributes involved in the relationship.");
				}

				string cacheKey = $"{entity1Value.Value}|{entity2Value.Value}";
				ColumnSet columnSet = new ColumnSet(target.Attributes.Select(a => a.Key).ToArray());

				// Queue for batched retrieval
				_intersectRetrievalBatch.Add(new IntersectRetrievalBatchItem
				{
					Entity1Value = entity1Value.Value,
					Entity2Value = entity2Value.Value,
					Target = target,
					ColumnSet = columnSet,
					Metadata = manyToManyRelationshipMetadata
				});

				// Process batch when full or immediately
				Dictionary<string, Entity> results;
				if (_intersectRetrievalBatch.Count >= RetrievalBatchSize)
				{
					results = ProcessIntersectRetrievalBatch();
				}
				else
				{
					// Process immediately to get the result
					results = ProcessIntersectRetrievalBatch();
				}

				// Check results
				results.TryGetValue(cacheKey, out existingRecord);
			}
			return existingRecord;
		}

		private void RemoveUnchangedColumns(Entity target, Entity existingRecord)
		{
			foreach (KeyValuePair<string, object> column in target.Attributes.ToArray())
			{
				if ((existingRecord.Contains(column.Key) && Equals(column.Value, existingRecord[column.Key]))
					||
					//Dataverse seems to consider that null and "" string are equal and doesn't include the attribute in retrieve records if the value is either
					((column.Value == null || column.Value as string == "") && !existingRecord.Contains(column.Key)))
				{
					target.Attributes.Remove(column.Key);
				} else if (existingRecord.GetAttributeValue<object>(column.Key) is OptionSetValueCollection existingCollection && target.GetAttributeValue<object>(column.Key) is OptionSetValueCollection targetCollection)
				{
					if (existingCollection.Count == targetCollection.Count && targetCollection.All(existingCollection.Contains))
					{
						target.Attributes.Remove(column.Key);
					}
				}
			}

			target.Id = existingRecord.Id;
		}
	}
}
