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
using System.Threading;

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
		/// Controls the maximum number of records to retrieve in a single query when checking for existing records. Default is 500. Specify 1 to retrieve one record at a time.
		/// </summary>
		[Parameter(HelpMessage = "Controls the maximum number of records to retrieve in a single query when checking for existing records. Default is 500. Specify 1 to retrieve one record at a time.")]
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

		/// <summary>
		/// Number of times to retry each record on failure. Default is 0 (no retries).
		/// </summary>
		[Parameter(HelpMessage = "Number of times to retry each record on failure. Default is 0 (no retries).")]
		public int Retries { get; set; } = 0;

		/// <summary>
		/// Initial delay in seconds before first retry. Subsequent retries use exponential backoff. Default is 5s.
		/// </summary>
		[Parameter(HelpMessage = "Initial delay in seconds before first retry. Subsequent retries use exponential backoff. Default is 5s.")]
		public int InitialRetryDelay { get; set; } = 5;

		// Track records that need to be retried with full processing
		private class RetryRecord
		{
			public PSObject InputObject { get; set; }
			public int RetriesRemaining { get; set; }
			public DateTime NextRetryTime { get; set; }
			public Exception LastError { get; set; }
            public bool RetryInProgress { get; private set; }
        }

		private List<RetryRecord> _pendingRetries;

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

		// Retrieval batching support
		private class RecordProcessingItem
		{
			public PSObject InputObject { get; set; }
			public Entity Target { get; set; }
			public EntityMetadata EntityMetadata { get; set; }
			public Entity ExistingRecord { get; set; }
		}

		private List<RecordProcessingItem> _retrievalBatchQueue;

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

			_retrievalBatchQueue = new List<RecordProcessingItem>();
			_pendingRetries = new List<RetryRecord>();

			if (BatchSize > 1)
			{
				_nextBatchItems = new List<BatchItem>();
			}
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
							bool handledByCompletion = batchItem.ResponseExceptionCompletion != null && batchItem.ResponseExceptionCompletion(itemResponse.Fault);

							if (!handledByCompletion)
							{
								// Schedule the full record for retry (not just the batch item)
								if (Retries > 0)
								{
									ScheduleRecordRetry(batchItem.InputObject, new Exception($"OrganizationServiceFault {itemResponse.Fault.ErrorCode}: {itemResponse.Fault.Message}"));
								}
								else
								{
                                    RecordRetryDone(batchItem.InputObject);
                                    // No retries configured - write error immediately
                                    StringBuilder details = new StringBuilder();
									AppendFaultDetails(itemResponse.Fault, details);
									WriteError(new ErrorRecord(new Exception(details.ToString()), null, ErrorCategory.InvalidResult, batchItem.InputObject));
								}
							}
						}
						else
						{
							this.RecordRetryDone(batchItem.InputObject);
                            batchItem.ResponseCompletion(itemResponse.Response);
						}
					}

				} catch (Exception e)
				{
					foreach (var batchItem in _nextBatchItems) {
						this.ScheduleRecordRetry(batchItem.InputObject, e);
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

		private void ScheduleRecordRetry(PSObject inputObject, Exception error)
		{
			// Check if this record is already scheduled for retry
			var existing = _pendingRetries.FirstOrDefault(r => ReferenceEquals(r.InputObject, inputObject));

			if (existing != null)
			{
				// Already retrying - decrement remaining count
				if (existing.RetriesRemaining > 0)
				{
					int attemptNumber = Retries - existing.RetriesRemaining + 1;
					int delayS = InitialRetryDelay * (int)Math.Pow(2, attemptNumber - 1);
					existing.NextRetryTime = DateTime.UtcNow.AddSeconds(delayS);
					existing.RetriesRemaining--;
					existing.LastError = error;
					existing.RetryInProgress = false;

					WriteVerbose($"Record processing failed, will retry in {delayS}s (attempt {attemptNumber + 1} of {Retries + 1})");
				}
				else
				{
					// No more retries - write final error
					_pendingRetries.Remove(existing);
					WriteError(new ErrorRecord(existing.LastError, null, ErrorCategory.InvalidResult, inputObject));
				}
			}
			else
			{
				// First failure - schedule for retry
				int delayS = InitialRetryDelay;
				_pendingRetries.Add(new RetryRecord
				{
					InputObject = inputObject,
					RetriesRemaining = Retries - 1,
					NextRetryTime = DateTime.UtcNow.AddSeconds(delayS),
					LastError = error
				});

				WriteVerbose($"Record processing failed, will retry in {delayS}s (attempt 2 of {Retries + 1})");
			}
		}

	/// <summary>
	/// Completes cmdlet processing.
	/// </summary>


		protected override void EndProcessing()
		{
			base.EndProcessing();

			// Process any remaining queued records
			if (_retrievalBatchQueue != null && _retrievalBatchQueue.Count > 0)
			{
				ProcessQueuedRecords();
			}

			if (_nextBatchItems != null)
			{
				ProcessBatch();
			}

			// Process any pending retries
			ProcessRetries();
		}

		private void ProcessRetries()
		{
			while (!Stopping && _pendingRetries.Where(r => !r.RetryInProgress).Any())
			{
				DateTime now = DateTime.UtcNow;
				var readyForRetry = _pendingRetries.Where(r => !r.RetryInProgress && r.NextRetryTime <= now).ToList();

				if (readyForRetry.Count == 0)
				{
					// Calculate wait time for next retry
					var nextRetryTime = _pendingRetries.Where(r=> !r.RetryInProgress).Min(r => r.NextRetryTime);
					var waitTime = (nextRetryTime - now).TotalSeconds;

					if (waitTime > 0)
					{
						WriteVerbose($"Waiting {waitTime:F0}s for next retry...");
						Thread.Sleep((int)waitTime*1000);
					}

					continue;
				}

				// Remove from pending and reprocess
				foreach (var item in readyForRetry)
				{
					item.RetryInProgress = true;
                    WriteVerbose($"Retrying record processing...");
					ProcessSingleRecord(item.InputObject);
				}

				// Process any accumulated batches after retries
				if (_retrievalBatchQueue != null && _retrievalBatchQueue.Count > 0)
				{
					ProcessQueuedRecords();
				}

				if (_nextBatchItems != null && _nextBatchItems.Count > 0)
				{
					ProcessBatch();
				}
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

			ProcessSingleRecord(InputObject);
		}

		private void ProcessSingleRecord(PSObject inputObject)
		{
			Entity target;

			try
			{
				target = entityConverter.ConvertToDataverseEntity(inputObject, TableName, GetConversionOptions());
            }
			catch (FormatException e)
			{
				// Conversion errors are not retryable
				WriteError(new ErrorRecord(new Exception("Error converting input object: " + e.Message, e), null, ErrorCategory.InvalidData, inputObject));
				return;
			}
			catch (Exception e)
			{
				// Other errors during conversion may be retryable
				if (Retries > 0)
				{
					ScheduleRecordRetry(inputObject, e);
				}
				else
				{
                    RecordRetryDone(inputObject);
                    WriteError(new ErrorRecord(e, null, ErrorCategory.InvalidOperation, inputObject));
				}
				return;
			}

			EntityMetadata entityMetadata = entityMetadataFactory.GetMetadata(TableName);

			// Check if this record needs retrieval
			if (NeedsRetrieval(entityMetadata, target))
			{
				// Queue for batched retrieval
				_retrievalBatchQueue.Add(new RecordProcessingItem
				{
					InputObject = inputObject,
					Target = target,
					EntityMetadata = entityMetadata,
					ExistingRecord = null
				});

				// Process the batch if it's full
				if (_retrievalBatchQueue.Count >= RetrievalBatchSize)
				{
					ProcessQueuedRecords();
				}
			}
			else
			{
				// Process immediately without retrieval
				Entity existingRecord;

				try
				{
					existingRecord = GetExistingRecord(entityMetadata, target);
                }
				catch (Exception e)
				{
					if (Retries > 0)
					{
						ScheduleRecordRetry(inputObject, e);
					}
					else
					{
						RecordRetryDone(inputObject);
                        WriteError(new ErrorRecord(e, null, ErrorCategory.InvalidOperation, inputObject));
					}
					return;
				}

				ProcessRecordWithExistingRecord(inputObject, target, entityMetadata, existingRecord);
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
					if (targetUpdate.Id == Guid.Empty && targetUpdate.KeyAttributes.Count == 0)
					{
						targetUpdate.Id = Guid.NewGuid();
					}

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
					if (ShouldProcess(string.Format("Update existing record {0}:{1} columns:\n{2}", TableName, existingRecord.Id, updatedColumnSummary)))
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

		private bool NeedsRetrieval(EntityMetadata entityMetadata, Entity target)
		{
			if (CreateOnly.IsPresent || Upsert.IsPresent)
			{
				return false;
			}

			if (!entityMetadata.IsIntersect.GetValueOrDefault())
			{
				if (Id != Guid.Empty && UpdateAllColumns.IsPresent)
				{
					return false;
				}
			}

			return true;
		}

		private void ProcessQueuedRecords()
		{
			if (_retrievalBatchQueue.Count == 0)
			{
				return;
			}

			WriteVerbose($"Processing retrieval batch of {_retrievalBatchQueue.Count} record(s)");

			// Group records by retrieval type for efficient batching
			var recordsById = _retrievalBatchQueue.Where(r => 
				!r.EntityMetadata.IsIntersect.GetValueOrDefault() && 
				(Id != Guid.Empty || r.Target.Id != Guid.Empty) && 
				MatchOn == null).ToList();

			var recordsByMatchOn = _retrievalBatchQueue.Where(r => 
				!r.EntityMetadata.IsIntersect.GetValueOrDefault() && 
				MatchOn != null).ToList();

			var recordsIntersect = _retrievalBatchQueue.Where(r => 
				r.EntityMetadata.IsIntersect.GetValueOrDefault()).ToList();

			// Batch retrieve by ID
			if (recordsById.Any())
			{
				try
				{
					RetrieveRecordsBatchById(recordsById);
				}
				catch (Exception e)
				{
					// Retrieval failed - schedule all records in this batch for retry
					if (Retries > 0)
					{
						WriteVerbose($"Retrieval batch by ID failed, scheduling {recordsById.Count} record(s) for retry: {e.Message}");
						foreach (var item in recordsById)
						{
							ScheduleRecordRetry(item.InputObject, e);
						}
						// Remove from queue so they're not processed below
						recordsById.Clear();
					}
					else
					{
						// No retries - write errors and remove from queue
						foreach (var item in recordsById)
						{
							RecordRetryDone(item.InputObject);
							WriteError(new ErrorRecord(new Exception($"Error retrieving existing record: {e.Message}", e), null, ErrorCategory.InvalidOperation, item.InputObject));
						}
						recordsById.Clear();
					}
				}
			}

			// Batch retrieve by MatchOn
			if (recordsByMatchOn.Any())
			{
				try
				{
					RetrieveRecordsBatchByMatchOn(recordsByMatchOn);
                }
				catch (Exception e)
				{
					// Retrieval failed - schedule all records in this batch for retry
					if (Retries > 0)
					{
						WriteVerbose($"Retrieval batch by MatchOn failed, scheduling {recordsByMatchOn.Count} record(s) for retry: {e.Message}");
						foreach (var item in recordsByMatchOn)
						{
							ScheduleRecordRetry(item.InputObject, e);
						}
						// Remove from queue so they're not processed below
						recordsByMatchOn.Clear();
					}
					else
					{
						// No retries - write errors and remove from queue
						foreach (var item in recordsByMatchOn)
						{
                                RecordRetryDone(item.InputObject);
                                WriteError(new ErrorRecord(new Exception($"Error retrieving existing record: {e.Message}", e), null, ErrorCategory.InvalidOperation, item.InputObject));
						}
						recordsByMatchOn.Clear();
					}
				}
			}

			// Batch retrieve intersect entities
			if (recordsIntersect.Any())
			{
				try
				{
					RetrieveRecordsBatchIntersect(recordsIntersect);
                }
				catch (Exception e)
				{
					// Retrieval failed - schedule all records in this batch for retry
					if (Retries > 0)
					{
						WriteVerbose($"Retrieval batch for intersect entities failed, scheduling {recordsIntersect.Count} record(s) for retry: {e.Message}");
						foreach (var item in recordsIntersect)
						{
							ScheduleRecordRetry(item.InputObject, e);
						}
						// Remove from queue so they're not processed below
						recordsIntersect.Clear();
					}
					else
					{
						// No retries - write errors and remove from queue
						foreach (var item in recordsIntersect)
						{
                            RecordRetryDone(item.InputObject);
                            WriteError(new ErrorRecord(new Exception($"Error retrieving existing record: {e.Message}", e), null, ErrorCategory.InvalidOperation, item.InputObject));
						}
						recordsIntersect.Clear();
					}
				}
			}

			// Process all successfully queued records with their retrieved existing records
			foreach (var item in _retrievalBatchQueue)
			{
				ProcessRecordWithExistingRecord(item.InputObject, item.Target, item.EntityMetadata, item.ExistingRecord);
			}

			_retrievalBatchQueue.Clear();
		}

        private void RecordRetryDone(PSObject inputObject)
        {
            _pendingRetries.RemoveAll(r => r.InputObject == inputObject);
        }

        private void RetrieveRecordsBatchById(List<RecordProcessingItem> records)
		{
			if (records.Count == 0) return;

			var entityName = records[0].Target.LogicalName;
			var primaryIdAttribute = records[0].EntityMetadata.PrimaryIdAttribute;
			
			// Get all IDs to retrieve
			var ids = records.Select(r => r.Target.Id != Guid.Empty ? r.Target.Id : Id).Distinct().ToList();

			if (ids.Count == 0) return;

			// Build query with In operator for efficient batching
			var query = new QueryExpression(entityName)
			{
				ColumnSet = new ColumnSet(records[0].Target.Attributes.Select(a => a.Key).ToArray())
			};

			if (ids.Count == 1)
			{
				query.Criteria.AddCondition(primaryIdAttribute, ConditionOperator.Equal, ids[0]);
			}
			else
			{
				query.Criteria.AddCondition(primaryIdAttribute, ConditionOperator.In, ids.Cast<object>().ToArray());
			}

			WriteVerbose($"Retrieving {ids.Count} record(s) by ID in retrieval batch");

			var retrievedRecords = Connection.RetrieveMultiple(query).Entities;
			var retrievedDict = retrievedRecords.ToDictionary(e => e.Id);

			// Match retrieved records back to processing items
			foreach (var item in records)
			{
				var recordId = item.Target.Id != Guid.Empty ? item.Target.Id : Id;
				if (retrievedDict.TryGetValue(recordId, out var existingRecord))
				{
					item.ExistingRecord = existingRecord;
				}
			}
		}

		private void RetrieveRecordsBatchByMatchOn(List<RecordProcessingItem> records)
		{
			if (records.Count == 0) return;

			var entityName = records[0].Target.LogicalName;

			foreach (string[] matchOnColumnList in MatchOn)
			{
				var recordsNeedingMatch = records.Where(r => r.ExistingRecord == null).ToList();
				if (recordsNeedingMatch.Count == 0) break;

				if (matchOnColumnList.Length == 1)
				{
					// Single column - use In operator for efficiency
					var matchColumn = matchOnColumnList[0];
					var matchValues = recordsNeedingMatch.Select(r =>
					{
						var val = r.Target.GetAttributeValue<object>(matchColumn);
						if (val is EntityReference er) return (object)er.Id;
						if (val is OptionSetValue osv) return (object)osv.Value;
						return val;
					}).Distinct().ToList();

					var query = new QueryExpression(entityName)
					{
						ColumnSet = new ColumnSet(recordsNeedingMatch[0].Target.Attributes.Select(a => a.Key).ToArray())
					};

					if (matchValues.Count == 1)
					{
						query.Criteria.AddCondition(matchColumn, ConditionOperator.Equal, matchValues[0]);
					}
					else
					{
						query.Criteria.AddCondition(matchColumn, ConditionOperator.In, matchValues.ToArray());
					}

					WriteVerbose($"Retrieving records by MatchOn ({matchColumn}) in retrieval batch");

					var retrievedRecords = Connection.RetrieveMultiple(query).Entities;

					// Match back to items
					foreach (var item in recordsNeedingMatch)
					{
						var itemValue = item.Target.GetAttributeValue<object>(matchColumn);
						if (itemValue is EntityReference er) itemValue = er.Id;
						if (itemValue is OptionSetValue osv) itemValue = osv.Value;

						var matches = retrievedRecords.Where(e =>
						{
							var recValue = e.GetAttributeValue<object>(matchColumn);
							if (recValue is EntityReference er2) recValue = er2.Id;
							if (recValue is OptionSetValue osv2) recValue = osv2.Value;
							return Equals(itemValue, recValue);
						}).ToList();

						if (matches.Count == 1)
						{
							item.ExistingRecord = matches[0];
						}
						else if (matches.Count > 1)
						{
							WriteError(new ErrorRecord(new Exception($"Match on {matchColumn} resulted in more than one record"), null, ErrorCategory.InvalidOperation, item.InputObject));
						}
					}
				}
				else
				{
					// Multi-column - use Or with And conditions
					var query = new QueryExpression(entityName)
					{
						ColumnSet = new ColumnSet(recordsNeedingMatch[0].Target.Attributes.Select(a => a.Key).ToArray())
					};

					var orFilter = new FilterExpression(LogicalOperator.Or);
					
					foreach (var item in recordsNeedingMatch)
					{
						var andFilter = new FilterExpression(LogicalOperator.And);
						foreach (var matchColumn in matchOnColumnList)
						{
							var queryValue = item.Target.GetAttributeValue<object>(matchColumn);
							if (queryValue is EntityReference er1) queryValue = er1.Id;
							if (queryValue is OptionSetValue osv1) queryValue = osv1.Value;
							
							andFilter.AddCondition(matchColumn, ConditionOperator.Equal, queryValue);
						}
						orFilter.AddFilter(andFilter);
					}

					query.Criteria.AddFilter(orFilter);

					WriteVerbose($"Retrieving records by MatchOn ({string.Join(",", matchOnColumnList)}) in retrieval batch");

					var retrievedRecords = Connection.RetrieveMultiple(query).Entities;

					// Match back to items
					foreach (var item in recordsNeedingMatch)
					{
						var matches = retrievedRecords.Where(e =>
						{
							return matchOnColumnList.All(col =>
							{
								var itemValue = item.Target.GetAttributeValue<object>(col);
								var recValue = e.GetAttributeValue<object>(col);
								
								if (itemValue is EntityReference er1) itemValue = er1.Id;
								if (itemValue is OptionSetValue osv1) itemValue = osv1.Value;
								if (recValue is EntityReference er2) recValue = er2.Id;
								if (recValue is OptionSetValue osv2) recValue = osv2.Value;
								
								return Equals(itemValue, recValue);
							});
						}).ToList();

						if (matches.Count == 1)
						{
							item.ExistingRecord = matches[0];
						}
						else if (matches.Count > 1)
						{
							var matchOnSummary = string.Join(", ", matchOnColumnList.Select(c => $"{c}='{item.Target.GetAttributeValue<object>(c)}'"));
							WriteError(new ErrorRecord(new Exception($"Match on values {matchOnSummary} resulted in more than one record"), null, ErrorCategory.InvalidOperation, item.InputObject));
						}
					}
				}
			}
		}

		private void RetrieveRecordsBatchIntersect(List<RecordProcessingItem> records)
		{
			if (records.Count == 0) return;

			var entityName = records[0].Target.LogicalName;
			var manyToManyRelationshipMetadata = records[0].EntityMetadata.ManyToManyRelationships[0];
			
			var query = new QueryExpression(entityName)
			{
				ColumnSet = new ColumnSet(records[0].Target.Attributes.Select(a => a.Key).ToArray())
			};

			var orFilter = new FilterExpression(LogicalOperator.Or);
			
			foreach (var item in records)
			{
				var entity1Value = item.Target.GetAttributeValue<Guid?>(manyToManyRelationshipMetadata.Entity1IntersectAttribute);
				var entity2Value = item.Target.GetAttributeValue<Guid?>(manyToManyRelationshipMetadata.Entity2IntersectAttribute);

				if (entity1Value.HasValue && entity2Value.HasValue)
				{
					var andFilter = new FilterExpression(LogicalOperator.And);
					andFilter.AddCondition(manyToManyRelationshipMetadata.Entity1IntersectAttribute, ConditionOperator.Equal, entity1Value.Value);
					andFilter.AddCondition(manyToManyRelationshipMetadata.Entity2IntersectAttribute, ConditionOperator.Equal, entity2Value.Value);
					orFilter.AddFilter(andFilter);
				}
			}

			if (orFilter.Filters.Count > 0)
			{
				query.Criteria.AddFilter(orFilter);

				WriteVerbose($"Retrieving {records.Count} intersect record(s) in retrieval batch");

				var retrievedRecords = Connection.RetrieveMultiple(query).Entities;

				// Match back to items
				foreach (var item in records)
				{
					var entity1Value = item.Target.GetAttributeValue<Guid?>(manyToManyRelationshipMetadata.Entity1IntersectAttribute);
					var entity2Value = item.Target.GetAttributeValue<Guid?>(manyToManyRelationshipMetadata.Entity2IntersectAttribute);

					var match = retrievedRecords.FirstOrDefault(e =>
						e.GetAttributeValue<Guid>(manyToManyRelationshipMetadata.Entity1IntersectAttribute) == entity1Value &&
						e.GetAttributeValue<Guid>(manyToManyRelationshipMetadata.Entity2IntersectAttribute) == entity2Value);

					if (match != null)
					{
						item.ExistingRecord = match;
					}
				}
			}
		}

		private void ProcessRecordWithExistingRecord(PSObject inputObject, Entity target, EntityMetadata entityMetadata, Entity existingRecord)
		{
			// This is the original processing logic from ProcessRecord, after GetExistingRecord
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

			// Handle assignment and status changes
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
						QueueBatchItem(new BatchItem(inputObject, request, (response) => { AssignRecordCompletion(target, ownerid); }), CallerId);
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
								WriteError(new ErrorRecord(e, null, ErrorCategory.WriteError, inputObject));
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
						QueueBatchItem(new BatchItem(inputObject, request, (response) => { SetStateCompletion(target, statuscode, stateCode); }), CallerId);
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
								WriteError(new ErrorRecord(e, null, ErrorCategory.WriteError, inputObject));
							}
						}
					}
				}
			}
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
						QueryByAttribute existingRecordQuery = new QueryByAttribute(TableName);
						existingRecordQuery.AddAttributeValue(entityMetadata.PrimaryIdAttribute, Id);
						existingRecordQuery.ColumnSet = target.LogicalName.Equals("calendar", StringComparison.OrdinalIgnoreCase) ? new ColumnSet(true) : new ColumnSet(target.Attributes.Select(a => a.Key).ToArray());

						existingRecord = Connection.RetrieveMultiple(existingRecordQuery
						).Entities.FirstOrDefault();
					}
				}

				if (existingRecord == null && MatchOn != null)
				{
					foreach (string[] matchOnColumnList in MatchOn)
					{
						QueryByAttribute matchOnQuery = new QueryByAttribute(TableName);
						matchOnQuery.TopCount = 2;

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

							matchOnQuery.AddAttributeValue(matchOnColumn, queryValue);
						}

						matchOnQuery.ColumnSet = new ColumnSet(target.Attributes.Select(a => a.Key).ToArray());

						var existingRecords = Connection.RetrieveMultiple(matchOnQuery
							).Entities;

						if (existingRecords.Count == 1)
						{
							existingRecord = existingRecords[0];
							break;
						}
						else if (existingRecords.Count > 1)
						{
							string matchOnSummary = string.Join("\n", matchOnColumnList.Select(c => c + "='" +
							matchOnQuery.Values[matchOnQuery.Attributes.IndexOf(c)] + "'" ?? "<null>").ToArray());

							throw new Exception(string.Format("Match on values {0} resulted in more than one record to update. Match on values:\n", matchOnSummary));
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

				QueryByAttribute existingRecordQuery = new QueryByAttribute(TableName);
				existingRecordQuery.AddAttributeValue(manyToManyRelationshipMetadata.Entity1IntersectAttribute, entity1Value.Value);
				existingRecordQuery.AddAttributeValue(manyToManyRelationshipMetadata.Entity2IntersectAttribute, entity2Value.Value);
				existingRecordQuery.ColumnSet = new ColumnSet(target.Attributes.Select(a => a.Key).ToArray());

				existingRecord = Connection.RetrieveMultiple(existingRecordQuery
					).Entities.FirstOrDefault();
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
