using Azure;
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
    /// <summary>
    /// Interface defining parameters for set operations that can be shared between cmdlet and operation context.
    /// </summary>
    internal interface ISetOperationParameters
    {
        /// <summary>
        /// Gets the business logic types to bypass.
        /// </summary>
        CustomLogicBypassableOrganizationServiceCmdlet.BusinessLogicTypes[] BypassBusinessLogicExecution { get; }

        /// <summary>
        /// Gets the business logic execution step IDs to bypass.
        /// </summary>
        Guid[] BypassBusinessLogicExecutionStepIds { get; }

        /// <summary>
        /// Gets the number of retries for failed operations.
        /// </summary>
        int Retries { get; }

        /// <summary>
        /// Gets the initial retry delay in seconds.
        /// </summary>
        int InitialRetryDelay { get; }

        /// <summary>
        /// Gets a value indicating whether to skip updating existing records.
        /// </summary>
        bool NoUpdate { get; }

        /// <summary>
        /// Gets a value indicating whether to skip creating new records.
        /// </summary>
        bool NoCreate { get; }

        /// <summary>
        /// Gets a value indicating whether to use create-only mode.
        /// </summary>
        bool CreateOnly { get; }

        /// <summary>
        /// Gets a value indicating whether to use upsert mode.
        /// </summary>
        bool Upsert { get; }

        /// <summary>
        /// Gets a value indicating whether to pass through the input object with Id set.
        /// </summary>
        bool PassThru { get; }

        /// <summary>
        /// Gets a value indicating whether to update all columns without comparison.
        /// </summary>
        bool UpdateAllColumns { get; }

        /// <summary>
        /// Gets the list of columns that should not be updated.
        /// </summary>
        string[] NoUpdateColumns { get; }

        /// <summary>
        /// Gets the match-on column lists for finding existing records.
        /// </summary>
        string[][] MatchOn { get; }

        /// <summary>
        /// Gets the ID of the record.
        /// </summary>
        Guid Id { get; }
    }

    /// <summary>
    /// Handles the complete lifecycle of a set operation for a single record, including
    /// request creation, execution, error handling, and retry logic.
    /// </summary>
    internal class SetOperationContext : ISetOperationParameters
    {
        private readonly Action<string> _writeVerbose;
        private readonly Action<ErrorRecord> _writeError;
        private readonly Action<object> _writeObject;
        private readonly Func<string, bool> _shouldProcess;

        public SetOperationContext(
            PSObject inputObject,
            string tableName,
            Guid? callerId,
            ISetOperationParameters parameters,
            EntityMetadataFactory metadataFactory,
            DataverseEntityConverter entityConverter,
            IOrganizationService connection,
            ConvertToDataverseEntityOptions conversionOptions,
            Action<string> writeVerbose,
            Action<ErrorRecord> writeError,
            Action<object> writeObject,
            Func<string, bool> shouldProcess)
        {
            InputObject = inputObject;
            TableName = tableName;
            CallerId = callerId;
            BypassBusinessLogicExecution = parameters.BypassBusinessLogicExecution;
            BypassBusinessLogicExecutionStepIds = parameters.BypassBusinessLogicExecutionStepIds;
            NoUpdate = parameters.NoUpdate;
            NoCreate = parameters.NoCreate;
            CreateOnly = parameters.CreateOnly;
            Upsert = parameters.Upsert;
            PassThru = parameters.PassThru;
            UpdateAllColumns = parameters.UpdateAllColumns;
            NoUpdateColumns = parameters.NoUpdateColumns;
            MatchOn = parameters.MatchOn;
            Id = parameters.Id;
            Retries = parameters.Retries;
            InitialRetryDelay = parameters.InitialRetryDelay;
            RetriesRemaining = parameters.Retries;
            NextRetryTime = DateTime.MinValue;
            MetadataFactory = metadataFactory;
            EntityConverter = entityConverter;
            Connection = connection;
            ConversionOptions = conversionOptions;
            _writeVerbose = writeVerbose;
            _writeError = writeError;
            _writeObject = writeObject;
            _shouldProcess = shouldProcess;
            Requests = new List<OrganizationRequest>();
        }

        public PSObject InputObject { get; }
        public string TableName { get; }
        public Guid? CallerId { get; }
        public CustomLogicBypassableOrganizationServiceCmdlet.BusinessLogicTypes[] BypassBusinessLogicExecution { get; }
        public Guid[] BypassBusinessLogicExecutionStepIds { get; }
        public bool NoUpdate { get; }
        public bool NoCreate { get; }
        public bool CreateOnly { get; }
        public bool Upsert { get; }
        public bool PassThru { get; }
        public bool UpdateAllColumns { get; }
        public string[] NoUpdateColumns { get; }
        public string[][] MatchOn { get; }
        public Guid Id { get; }
        public int Retries { get; }
        public int InitialRetryDelay { get; }
        public int RetriesRemaining { get; set; }
        public DateTime NextRetryTime { get; set; }
        public List<OrganizationRequest> Requests { get; set; }
        public EntityMetadataFactory MetadataFactory { get; }
        public DataverseEntityConverter EntityConverter { get; }
        public IOrganizationService Connection { get; }
        public ConvertToDataverseEntityOptions ConversionOptions { get; }
        public Entity Target { get; set; }
        public EntityMetadata EntityMetadata { get; set; }
        public Entity ExistingRecord { get; set; }
        
        /// <summary>
        /// Callback to invoke when the operation completes successfully.
        /// </summary>
        public Action<OrganizationResponse> ResponseCompletion { get; set; }
        
        /// <summary>
        /// Callback to invoke when the operation encounters a fault.
        /// Returns true if the fault was handled, false otherwise.
        /// </summary>
        public Func<OrganizationServiceFault, bool> ResponseExceptionCompletion { get; set; }

        /// <summary>
        /// Schedules this operation for retry after a failure.
        /// </summary>
        public void ScheduleRetry(Exception e)
        {
            // Schedule for retry with exponential backoff
            int attemptNumber = Retries - RetriesRemaining + 1;
            int delayS = InitialRetryDelay * (int)Math.Pow(2, attemptNumber - 1);
            NextRetryTime = DateTime.UtcNow.AddSeconds(delayS);
            RetriesRemaining--;

            _writeVerbose($"Request failed, will retry in {delayS}s (attempt {attemptNumber} of {Retries + 1}): {this}\n{e}");
        }

        /// <summary>
        /// Reports an error for this operation.
        /// </summary>
        public void ReportError(Exception e)
        {
            _writeError(new ErrorRecord(e, null, ErrorCategory.InvalidResult, InputObject));
        }

        /// <summary>
        /// Gets a summary of the key for an entity (ID or alternate keys).
        /// </summary>
        public static string GetKeySummary(Entity record)
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

        /// <summary>
        /// Truncates a string value to 100 characters with ellipsis.
        /// </summary>
        public static string Ellipsis(string value)
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

        /// <summary>
        /// Gets a summary of values, handling collections.
        /// </summary>
        public static object GetValueSummary(object value)
        {
            if ((!(value is string)) && value is IEnumerable enumerable)
            {
                return "[" + string.Join(", ", enumerable.Cast<object>().Select(i => GetValueSummary(i))) + "]";
            }

            return value ?? "<null>";
        }

        /// <summary>
        /// Gets a formatted summary of all columns in an entity.
        /// </summary>
        public static string GetColumnSummary(Entity entity, DataverseEntityConverter converter)
        {
            PSObject psObject = converter.ConvertToPSObject(entity, new ColumnSet(entity.Attributes.Select(a => a.Key).ToArray()), a => ValueType.Raw);
            return string.Join("\n", psObject.Properties.Select(a => a.Name + " = " + Ellipsis((GetValueSummary(a.Value)).ToString())));
        }

        /// <summary>
        /// Column names that should not be updated directly - they require special handling.
        /// </summary>
        public static readonly string[] DontUpdateDirectlyColumnNames = new[] { "statuscode", "statecode", "ownerid" };

        /// <summary>
        /// Removes unchanged columns from target entity by comparing with existing record.
        /// Also sets target.Id to existing record's Id.
        /// </summary>
        public static void RemoveUnchangedColumns(Entity target, Entity existingRecord)
        {
            foreach (KeyValuePair<string, object> column in target.Attributes.ToArray())
            {
                if ((existingRecord.Contains(column.Key) && Equals(column.Value, existingRecord[column.Key]))
                    ||
                    //Dataverse seems to consider that null and "" string are equal and doesn't include the attribute in retrieve records if the value is either
                    ((column.Value == null || column.Value as string == "") && !existingRecord.Contains(column.Key)))
                {
                    target.Attributes.Remove(column.Key);
                }
                else if (existingRecord.GetAttributeValue<object>(column.Key) is OptionSetValueCollection existingCollection && target.GetAttributeValue<object>(column.Key) is OptionSetValueCollection targetCollection)
                {
                    if (existingCollection.Count == targetCollection.Count && targetCollection.All(existingCollection.Contains))
                    {
                        target.Attributes.Remove(column.Key);
                    }
                }
            }

            target.Id = existingRecord.Id;
        }

        /// <summary>
        /// Applies bypass business logic execution parameters to a request.
        /// </summary>
        public void ApplyBypassBusinessLogicExecution(OrganizationRequest request)
        {
            if (BypassBusinessLogicExecution?.Length > 0)
            {
                request.Parameters["BypassBusinessLogicExecution"] = string.Join(",", BypassBusinessLogicExecution.Select(o => o.ToString()));
            }
            else
            {
                request.Parameters.Remove("BypassBusinessLogicExecution");
            }

            if (BypassBusinessLogicExecutionStepIds?.Length > 0)
            {
                request.Parameters["BypassBusinessLogicExecutionStepIds"] = string.Join(",", BypassBusinessLogicExecutionStepIds.Select(id => id.ToString()));
            }
            else
            {
                request.Parameters.Remove("BypassBusinessLogicExecutionStepIds");
            }
        }

        /// <summary>
        /// Sets the Id property on a PSObject, removing any existing Id property first.
        /// </summary>
        public static void SetIdProperty(PSObject inputObject, Guid id)
        {
            if (inputObject.Properties.Any(p => p.Name.Equals("Id", StringComparison.OrdinalIgnoreCase)))
            {
                inputObject.Properties.Remove("Id");
            }
            inputObject.Properties.Add(new PSNoteProperty("Id", id));
        }

        /// <summary>
        /// Determines if a record needs to be retrieved from the server before processing.
        /// </summary>
        public bool NeedsRetrieval(EntityMetadata entityMetadata, Entity target)
        {
            if (CreateOnly || Upsert)
            {
                return false;
            }

            if (!entityMetadata.IsIntersect.GetValueOrDefault())
            {
                if (Id != Guid.Empty && UpdateAllColumns)
                {
                    return false;
                }
            }

            return true;
        }

        /// <summary>
        /// Gets an existing record from the server if it exists, using Id or MatchOn criteria.
        /// </summary>
        public Entity GetExistingRecord(EntityMetadata entityMetadata, Entity target)
        {
            Entity existingRecord = null;

            if (CreateOnly || Upsert)
            {
                return null;
            }

            if (!entityMetadata.IsIntersect.GetValueOrDefault())
            {
                if (Id != Guid.Empty)
                {
                    if (UpdateAllColumns)
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

        /// <summary>
        /// Completion handler for create operations.
        /// </summary>
        public void CreateCompletion(Entity target, Entity targetCreate, string columnSummary, CreateResponse response)
        {
            SetIdProperty(InputObject, response.id);
            _writeVerbose(string.Format("Created new record {0}:{1} columns:\n{2}", target.LogicalName, response.id, columnSummary));

            if (PassThru)
            {
                _writeObject(InputObject);
            }
        }

        /// <summary>
        /// Completion handler for update operations.
        /// </summary>
        public void UpdateCompletion(Entity target, Entity existingRecord, string updatedColumnSummary)
        {
            _writeVerbose(string.Format("Updated existing record {0}:{1} columns:\n{2}", target.LogicalName, existingRecord.Id, updatedColumnSummary));

            if (PassThru)
            {
                _writeObject(InputObject);
            }
        }

        /// <summary>
        /// Completion handler for upsert operations.
        /// </summary>
        public void UpsertCompletion(Entity targetUpdate, UpsertResponse response)
        {
            targetUpdate.Id = response.Target.Id;

            SetIdProperty(InputObject, targetUpdate.Id);

            string columnSummary = GetColumnSummary(targetUpdate, EntityConverter);

            if (response.RecordCreated)
            {
                _writeVerbose(string.Format("Upsert created new record {0}:{1} columns:\n{2}", TableName, GetKeySummary(targetUpdate), columnSummary));
            }
            else
            {
                _writeVerbose(string.Format("Upsert updated existing record {0}:{1} columns:\n{2}", TableName, GetKeySummary(targetUpdate), columnSummary));
            }

            if (PassThru)
            {
                _writeObject(InputObject);
            }
        }

        /// <summary>
        /// Completion handler for M:M association operations.
        /// </summary>
        public void AssociateCompletion(Entity target, ManyToManyRelationshipMetadata manyToManyRelationshipMetadata, EntityReference record1, EntityReference record2)
        {
            QueryExpression getIdQuery = new QueryExpression(TableName);
            getIdQuery.Criteria.AddCondition(manyToManyRelationshipMetadata.Entity1IntersectAttribute, ConditionOperator.Equal, record1.Id);
            getIdQuery.Criteria.AddCondition(manyToManyRelationshipMetadata.Entity2IntersectAttribute, ConditionOperator.Equal, record2.Id);
            Guid id = Connection.RetrieveMultiple(getIdQuery).Entities.Single().Id;

            SetIdProperty(InputObject, id);
            _writeVerbose(string.Format("Created new intersect record {0}:{1}", target.LogicalName, id));

            if (PassThru)
            {
                _writeObject(InputObject);
            }
        }

        /// <summary>
        /// Completion handler for M:M association with upsert semantics.
        /// </summary>
        public void AssociateUpsertCompletion(bool recordWasCreated, Entity target, ManyToManyRelationshipMetadata manyToManyRelationshipMetadata, EntityReference record1, EntityReference record2)
        {
            if (recordWasCreated)
            {
                _writeVerbose(string.Format("Created intersect record {0}:{1}:{2}", target.LogicalName, record1.Id, record2.Id));
            }
            else
            {
                _writeVerbose(string.Format("Skipped creating (upsert) intersect record as already exists {0}:{1}:{2}", target.LogicalName, record1.Id, record2.Id));
            }
        }

        /// <summary>
        /// Error handler for M:M association with upsert semantics.
        /// Returns true if the error was handled (record already exists).
        /// </summary>
        public bool AssociateUpsertError(OrganizationServiceFault fault, Entity target, ManyToManyRelationshipMetadata manyToManyRelationshipMetadata, EntityReference record1, EntityReference record2)
        {
            if (fault.ErrorCode != -2147220937)
            {
                return false;
            }

            AssociateUpsertCompletion(false, target, manyToManyRelationshipMetadata, record1, record2);

            return true;
        }

        /// <summary>
        /// Completion handler for getting the ID of a newly associated record.
        /// </summary>
        public void AssociateUpsertGetIdCompletion(OrganizationResponse response)
        {
            SetIdProperty(InputObject, ((UpsertResponse)response).Target.Id);
        }

        /// <summary>
        /// Creates a new record (regular entity or M:M association).
        /// Sets up the create request and completion callbacks.
        /// </summary>
        public void CreateNewRecord()
        {
            if (NoCreate)
            {
                _writeVerbose($"Skipped creating new record {TableName}:{Id} - NoCreate enabled");
                return;
            }

            if (EntityMetadata.IsIntersect.GetValueOrDefault())
            {
                // Handle M:M association creation
                ManyToManyRelationshipMetadata manyToManyRelationshipMetadata = EntityMetadata.ManyToManyRelationships[0];

                EntityReference record1 = new EntityReference(manyToManyRelationshipMetadata.Entity1LogicalName,
                         Target.GetAttributeValue<Guid>(
                             manyToManyRelationshipMetadata.Entity1IntersectAttribute));
                EntityReference record2 = new EntityReference(manyToManyRelationshipMetadata.Entity2LogicalName,
                         Target.GetAttributeValue<Guid>(
                             manyToManyRelationshipMetadata.Entity2IntersectAttribute));

                AssociateRequest request = new AssociateRequest()
                {
                    Target = record1,
                    RelatedEntities = new EntityReferenceCollection() { record2 },
                    Relationship = new Relationship(manyToManyRelationshipMetadata.SchemaName)
                    {
                        PrimaryEntityRole = EntityRole.Referencing
                    },
                };
                ApplyBypassBusinessLogicExecution(request);
                Requests.Add(request);

                _writeVerbose($"Added create of new intersect record {TableName}:{record1.Id},{record2.Id} to batch");
                
                // Set up completion callback
                ResponseCompletion = (response) => {
                    AssociateCompletion(Target, manyToManyRelationshipMetadata, record1, record2);
                };
            }
            else
            {
                // Handle regular entity creation
                Entity targetCreate = new Entity(Target.LogicalName) { Id = Target.Id };
                targetCreate.Attributes.AddRange(Target.Attributes.Where(a => 
                    !DontUpdateDirectlyColumnNames.Contains(a.Key, StringComparer.OrdinalIgnoreCase)));

                string columnSummary = GetColumnSummary(targetCreate, EntityConverter);

                CreateRequest request = new CreateRequest() { Target = targetCreate };
                ApplyBypassBusinessLogicExecution(request);
                Requests.Add(request);

                _writeVerbose($"Added created of new record {TableName}:{targetCreate.Id} to batch - columns:\n{columnSummary}");
                
                // Set up completion callback
                ResponseCompletion = (response) => {
                    CreateCompletion(Target, targetCreate, columnSummary, (CreateResponse)response);
                };
            }
        }

        public override string ToString()
        {
            return $"Set {TableName}:{Id}";
        }
    }

    /// <summary>
    /// Manages batching and retry logic for set operations.
    /// </summary>
    internal class SetBatchProcessor
    {
        private readonly List<SetOperationContext> _nextBatchItems;
        private readonly List<SetOperationContext> _pendingRetries;
        private readonly uint _batchSize;
        private readonly IOrganizationService _connection;
        private readonly Action<string> _writeVerbose;
        private readonly Func<string, bool> _shouldProcess;
        private readonly Func<bool> _isStopping;
        private readonly CancellationToken _cancellationToken;
        private readonly Action<Guid?> _setCallerId;
        private readonly Func<Guid> _getCallerId;
        private Guid? _nextBatchCallerId;

        public SetBatchProcessor(
            uint batchSize,
            IOrganizationService connection,
            Action<string> writeVerbose,
            Func<string, bool> shouldProcess,
            Func<bool> isStopping,
            CancellationToken cancellationToken,
            Action<Guid?> setCallerId,
            Func<Guid> getCallerId)
        {
            _batchSize = batchSize;
            _connection = connection;
            _writeVerbose = writeVerbose;
            _shouldProcess = shouldProcess;
            _isStopping = isStopping;
            _cancellationToken = cancellationToken;
            _setCallerId = setCallerId;
            _getCallerId = getCallerId;
            _nextBatchItems = new List<SetOperationContext>();
            _pendingRetries = new List<SetOperationContext>();
        }

        /// <summary>
        /// Adds an operation to the batch, executing the batch if it reaches the batch size or caller ID changes.
        /// </summary>
        public void QueueOperation(SetOperationContext context)
        {
            if (_nextBatchItems.Any() && _nextBatchCallerId != context.CallerId)
            {
                ExecuteBatch();
            }

            _nextBatchCallerId = context.CallerId;
            _nextBatchItems.Add(context);

            if (_nextBatchItems.Count >= _batchSize)
            {
                ExecuteBatch();
            }
        }

        /// <summary>
        /// Executes any remaining operations in the batch.
        /// </summary>
        public void Flush()
        {
            if (_nextBatchItems.Count > 0)
            {
                ExecuteBatch();
            }
        }

        /// <summary>
        /// Processes all pending retries.
        /// </summary>
        public void ProcessRetries()
        {
            while (!_isStopping() && !_cancellationToken.IsCancellationRequested && _pendingRetries.Count > 0)
            {
                DateTime now = DateTime.UtcNow;
                var readyForRetry = _pendingRetries.Where(r => r.NextRetryTime <= now).ToList();

                if (readyForRetry.Count == 0)
                {
                    // Calculate wait time for next retry
                    var nextRetryTime = _pendingRetries.Min(r => r.NextRetryTime);
                    var waitTimeMs = (int)Math.Max(100, (nextRetryTime - now).TotalMilliseconds);

                    _writeVerbose($"Waiting {waitTimeMs / 1000.0:F1}s for next retry batch...");
                    Thread.Sleep(waitTimeMs);

                    continue;
                }

                // Remove from pending and add to batch for retry
                foreach (var item in readyForRetry)
                {
                    _pendingRetries.Remove(item);
                    _nextBatchItems.Add(item);

                    if (_nextBatchItems.Count >= _batchSize)
                    {
                        ExecuteBatch();
                    }
                }

                // Process any remaining items in batch
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

            // Apply bypass logic from first item (they should all be the same within a batch)
            if (_nextBatchItems.Count > 0)
            {
                var firstContext = _nextBatchItems[0];
                if (firstContext.BypassBusinessLogicExecution?.Length > 0)
                {
                    batchRequest.Parameters["BypassBusinessLogicExecution"] = string.Join(",", firstContext.BypassBusinessLogicExecution.Select(o => o.ToString()));
                }
                if (firstContext.BypassBusinessLogicExecutionStepIds?.Length > 0)
                {
                    batchRequest.Parameters["BypassBusinessLogicExecutionStepIds"] = string.Join(",", firstContext.BypassBusinessLogicExecutionStepIds.Select(id => id.ToString()));
                }
            }

            // Add all requests from all contexts to the batch
            foreach (var context in _nextBatchItems)
            {
                batchRequest.Requests.AddRange(context.Requests);
            }

            Guid oldCallerId = _getCallerId();
            _setCallerId(_nextBatchCallerId);

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
            finally
            {
                _setCallerId(oldCallerId);
            }

            // Process responses - note that we need to map back to contexts
            // Since each context may have multiple requests, we need to track which requests belong to which context
            int requestIndex = 0;
            foreach (var context in _nextBatchItems)
            {
                bool hasError = false;
                bool handledError = false;
                Exception firstError = null;
                OrganizationServiceFault firstFault = null;

                // Process all requests for this context
                for (int i = 0; i < context.Requests.Count; i++)
                {
                    if (requestIndex < response.Responses.Count)
                    {
                        var itemResponse = response.Responses[requestIndex];
                        
                        if (itemResponse.Fault != null)
                        {
                            // Try to handle fault with context's exception handler
                            if (context.ResponseExceptionCompletion != null && !handledError)
                            {
                                handledError = context.ResponseExceptionCompletion(itemResponse.Fault);
                            }
                            
                            if (!handledError)
                            {
                                // Build fault details for failed request
                                StringBuilder details = new StringBuilder();
                                AppendFaultDetails(itemResponse.Fault, details);
                                if (firstError == null)
                                {
                                    firstError = new Exception(details.ToString());
                                    firstFault = itemResponse.Fault;
                                }
                                hasError = true;
                            }
                        }
                        else if (context.ResponseCompletion != null && i == 0)
                        {
                            // Call success completion callback for the first (typically only) response
                            context.ResponseCompletion(itemResponse.Response);
                        }
                    }
                    requestIndex++;
                }

                if (hasError && !handledError)
                {
                    if (context.RetriesRemaining > 0)
                    {
                        context.ScheduleRetry(firstError);
                        _pendingRetries.Add(context);
                    }
                    else
                    {
                        context.ReportError(firstError);
                    }
                }
            }

            _nextBatchItems.Clear();
        }

        public static void AppendFaultDetails(OrganizationServiceFault fault, StringBuilder output)
        {
            output.AppendLine("OrganizationServiceFault " + fault.ErrorCode + ": " + fault.Message);
            output.AppendLine(fault.TraceText);

            if (fault.InnerFault != null)
            {
                output.AppendLine("---");
                AppendFaultDetails(fault.InnerFault, output);
            }
        }
    }

    /// <summary>Creates or updates records in a Dataverse environment. If a matching record is found then it will be updated, otherwise a new record is created (some options can override this).
    /// This command can also handle creation/update of intersect records (many to many relationships).</summary>
    [Cmdlet(VerbsCommon.Set, "DataverseRecord", SupportsShouldProcess = true, ConfirmImpact = ConfirmImpact.Medium)]
    public class SetDataverseRecordCmdlet : CustomLogicBypassableOrganizationServiceCmdlet, ISetOperationParameters
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
        [Parameter(Mandatory = false, HelpMessage = "List of properties on the input object which are ignored and not attempted to be mapped to the record. Default is none.")]
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

        // Explicit interface implementations for ISetOperationParameters
        bool ISetOperationParameters.NoUpdate => NoUpdate.IsPresent;
        bool ISetOperationParameters.NoCreate => NoCreate.IsPresent;
        bool ISetOperationParameters.CreateOnly => CreateOnly.IsPresent;
        bool ISetOperationParameters.Upsert => Upsert.IsPresent;
        bool ISetOperationParameters.PassThru => PassThru.IsPresent;
        bool ISetOperationParameters.UpdateAllColumns => UpdateAllColumns.IsPresent;
        CustomLogicBypassableOrganizationServiceCmdlet.BusinessLogicTypes[] ISetOperationParameters.BypassBusinessLogicExecution => BypassBusinessLogicExecution;
        Guid[] ISetOperationParameters.BypassBusinessLogicExecutionStepIds => BypassBusinessLogicExecutionStepIds;
        int ISetOperationParameters.Retries => Retries;
        int ISetOperationParameters.InitialRetryDelay => InitialRetryDelay;
        string[] ISetOperationParameters.NoUpdateColumns => NoUpdateColumns;
        string[][] ISetOperationParameters.MatchOn => MatchOn;
        Guid ISetOperationParameters.Id => Id;

        // Track records that need to be retried with full processing
        private class RetryRecord
        {
            public PSObject InputObject { get; set; }
            public int RetriesRemaining { get; set; }
            public DateTime NextRetryTime { get; set; }
            public Exception LastError { get; set; }
            public bool RetryInProgress { get; set; }
            public string TableName { get; internal set; }
            public Guid? CallerId { get; internal set; }
        }

        private List<RetryRecord> _pendingRetries;

        private class BatchItem
        {
            public BatchItem(PSObject inputObject, string tableName, Guid? callerId, OrganizationRequest request, Action<OrganizationResponse> responseCompletion) : this(inputObject, tableName, callerId, request, responseCompletion, null)
            {
            }

            public BatchItem(PSObject inputObject, string tableName, Guid? callerId, OrganizationRequest request, Action<OrganizationResponse> responseCompletion, Func<OrganizationServiceFault, bool> responseExceptionCompletion)
            {
                InputObject = inputObject;
                Request = request;
                ResponseCompletion = responseCompletion;
                ResponseExceptionCompletion = responseExceptionCompletion;
                TableName = tableName;
                CallerId = callerId;
            }

            public string TableName { get; set; }

            public Guid? CallerId { get; set; }

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
        private SetBatchProcessor _setBatchProcessor;
        private CancellationTokenSource _userCancellationCts;

        // Retrieval batching support
        private class RecordProcessingItem
        {
            public PSObject InputObject { get; set; }
            public Entity Target { get; set; }
            public EntityMetadata EntityMetadata { get; set; }
            public Entity ExistingRecord { get; set; }
            public string TableName { get; internal set; }
            public Guid? CallerId { get; internal set; }
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

            // Initialize cancellation token source for this pipeline invocation
            _userCancellationCts = new CancellationTokenSource();

            recordCount = 0;
            entityMetadataFactory = new EntityMetadataFactory(Connection);
            entityConverter = new DataverseEntityConverter(Connection, entityMetadataFactory);

            _retrievalBatchQueue = new List<RecordProcessingItem>();
            _pendingRetries = new List<RetryRecord>();

            if (BatchSize > 1)
            {
                _nextBatchItems = new List<BatchItem>();

                // Initialize the batch processor (not yet fully used but wired up)
                _setBatchProcessor = new SetBatchProcessor(
                    BatchSize,
                    Connection,
                    WriteVerbose,
                    ShouldProcess,
                    () => Stopping,
                    _userCancellationCts.Token,
                    callerId => Connection.CallerId = callerId.GetValueOrDefault(),
                    () => Connection.CallerId);
            }
        }

        private void AppendFaultDetails(OrganizationServiceFault fault, StringBuilder output)
        {
            SetBatchProcessor.AppendFaultDetails(fault, output);
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

                ExecuteMultipleResponse response = null;
                try
                {
                    response = (ExecuteMultipleResponse)Connection.Execute(batchRequest);
                }
                catch (Exception e)
                {
                    foreach (var batchItem in _nextBatchItems)
                    {
                        
                        this.ScheduleRecordRetry(batchItem.InputObject, batchItem.TableName, batchItem.CallerId, e);
                    }
                    _nextBatchItems.Clear();
                    return;
                }
                finally
                {
                    Connection.CallerId = oldCallerId;
                }

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
                                ScheduleRecordRetry(batchItem.InputObject, batchItem.TableName, batchItem.CallerId, new Exception($"OrganizationServiceFault {itemResponse.Fault.ErrorCode}: {itemResponse.Fault.Message}"));
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


            }

            _nextBatchItems.Clear();
            _nextBatchCallerId = null;
        }

        private void ScheduleRecordRetry(PSObject inputObject, string tableName, Guid? callerId, Exception error)
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
                    LastError = error,
                    TableName = tableName,
                    CallerId = callerId
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

            // Flush new batch processor if it was used
            if (_setBatchProcessor != null)
            {
                _setBatchProcessor.Flush();
            }

            // Also process old batch system (for methods not yet migrated)
            if (_nextBatchItems != null)
            {
                ProcessBatch();
            }

            // Process any pending retries from both systems
            if (_setBatchProcessor != null)
            {
                _setBatchProcessor.ProcessRetries();
            }
            ProcessRetries();

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

        private void ProcessRetries()
        {
            while (!Stopping && _pendingRetries.Where(r => !r.RetryInProgress).Any())
            {
                DateTime now = DateTime.UtcNow;
                var readyForRetry = _pendingRetries.Where(r => !r.RetryInProgress && r.NextRetryTime <= now).ToList();

                if (readyForRetry.Count == 0)
                {
                    // Calculate wait time for next retry
                    var nextRetryTime = _pendingRetries.Where(r => !r.RetryInProgress).Min(r => r.NextRetryTime);
                    var waitTime = (nextRetryTime - now).TotalSeconds;

                    if (waitTime > 0)
                    {
                        WriteVerbose($"Waiting {waitTime:F0}s for next retry...");
                        Thread.Sleep((int)waitTime * 1000);
                    }

                    continue;
                }

                // Remove from pending and reprocess
                foreach (var item in readyForRetry)
                {
                    item.RetryInProgress = true;
                    WriteVerbose($"Retrying record processing...");
                    ProcessSingleRecord(item.InputObject, item.TableName, item.CallerId);
                }

                // Process any accumulated batches after retries
                if (_retrievalBatchQueue != null && _retrievalBatchQueue.Count > 0)
                {
                    ProcessQueuedRecords();
                }

                // Flush new batch processor if it was used
                if (_setBatchProcessor != null)
                {
                    _setBatchProcessor.Flush();
                }

                // Also process old batch system (for methods not yet migrated)
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


            recordCount++;
            WriteVerbose("Processing record #" + recordCount);

            ProcessSingleRecord(InputObject, TableName, CallerId);
        }

        private void ProcessSingleRecord(PSObject inputObject, string tableName, Guid? callerId)
        {
            if (callerId.HasValue)
            {
                if (BatchSize > 1)
                {
                    throw new ArgumentException("CreatedOnBehalfBy not supported with BatchSize > 1");
                }
            }

            Entity target;

            try
            {
                target = entityConverter.ConvertToDataverseEntity(inputObject, tableName, GetConversionOptions());
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
                    ScheduleRecordRetry(inputObject, tableName, callerId, e);
                }
                else
                {
                    RecordRetryDone(inputObject);
                    WriteError(new ErrorRecord(e, null, ErrorCategory.InvalidOperation, inputObject));
                }
                return;
            }

            EntityMetadata entityMetadata = entityMetadataFactory.GetMetadata(TableName);

            // Create a context to access utility methods
            var context = new SetOperationContext(
                inputObject,
                tableName,
                callerId,
                this,
                entityMetadataFactory,
                entityConverter,
                Connection,
                GetConversionOptions(),
                WriteVerbose,
                WriteError,
                WriteObject,
                ShouldProcess);
            context.Target = target;
            context.EntityMetadata = entityMetadata;

            // Check if this record needs retrieval
            if (context.NeedsRetrieval(entityMetadata, target))
            {
                // Queue for batched retrieval
                _retrievalBatchQueue.Add(new RecordProcessingItem
                {
                    InputObject = inputObject,
                    Target = target,
                    EntityMetadata = entityMetadata,
                    ExistingRecord = null,
                    TableName = tableName,
                    CallerId = callerId
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
                    existingRecord = context.GetExistingRecord(entityMetadata, target);
                }
                catch (Exception e)
                {
                    if (Retries > 0)
                    {
                        ScheduleRecordRetry(inputObject, tableName, callerId, e);
                    }
                    else
                    {
                        RecordRetryDone(inputObject);
                        WriteError(new ErrorRecord(e, null, ErrorCategory.InvalidOperation, inputObject));
                    }
                    return;
                }

                ProcessRecordWithExistingRecord(inputObject, tableName, callerId, target, entityMetadata, existingRecord);
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

        private bool UpsertRecord(PSObject inputObject, string tableName, Guid? callerId, EntityMetadata entityMetadata, Entity target)
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
                    QueueBatchItem(new BatchItem(inputObject, tableName, callerId, request, (response) => { AssociateUpsertCompletion(true, target, inputObject, manyToManyRelationshipMetadata, record1, record2); }, fault => { return AssociateUpsertError(fault, target, inputObject, manyToManyRelationshipMetadata, record1, record2); }), CallerId);

                    if (PassThru.IsPresent)
                    {
                        QueryExpression getIdQuery = new QueryExpression(TableName);
                        getIdQuery.Criteria.AddCondition(manyToManyRelationshipMetadata.Entity1IntersectAttribute, ConditionOperator.Equal, record1.Id);
                        getIdQuery.Criteria.AddCondition(manyToManyRelationshipMetadata.Entity2IntersectAttribute, ConditionOperator.Equal, record2.Id);
                        QueueBatchItem(new BatchItem(inputObject, tableName, callerId, new RetrieveMultipleRequest() { Query = getIdQuery }, response =>
                        {
                            AssociateUpsertGetIdCompletion(response, inputObject);
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

                targetUpdate.Attributes.AddRange(target.Attributes.Where(a => !SetOperationContext.DontUpdateDirectlyColumnNames.Contains(a.Key, StringComparer.OrdinalIgnoreCase)));

                string columnSummary = SetOperationContext.GetColumnSummary(targetUpdate, entityConverter);

                UpsertRequest request = new UpsertRequest()
                {
                    Target = targetUpdate
                };

                if (_nextBatchItems != null)
                {
                    WriteVerbose(string.Format("Added upsert of new record {0}:{1} to batch - columns:\n{2}", TableName, SetOperationContext.GetKeySummary(targetUpdate), columnSummary));
                    QueueBatchItem(new BatchItem(inputObject, tableName, callerId, request, (response) => { UpsertCompletion(targetUpdate, inputObject, (UpsertResponse)response); }), CallerId);
                }
                else
                {
                    if (targetUpdate.Id == Guid.Empty && targetUpdate.KeyAttributes.Count == 0)
                    {
                        targetUpdate.Id = Guid.NewGuid();
                    }

                    if (ShouldProcess(string.Format("Upsert record {0}:{1} columns:\n{2}", TableName, SetOperationContext.GetKeySummary(targetUpdate), columnSummary)))
                    {
                        try
                        {
                            UpsertResponse response = (UpsertResponse)Connection.Execute(request);
                            UpsertCompletion(targetUpdate, inputObject, response);
                        }
                        catch (Exception e)
                        {
                            result = false;
                            WriteError(new ErrorRecord(new Exception(string.Format("Error creating record {0}:{1} {2}, columns: {3}", TableName, SetOperationContext.GetKeySummary(targetUpdate), e.Message, columnSummary), e), null, ErrorCategory.InvalidResult, inputObject));
                        }
                    }
                }
            }

            return result;
        }



        private void AssociateUpsertGetIdCompletion(OrganizationResponse response, PSObject inputObject)
        {
            Guid id = ((RetrieveMultipleResponse)response).EntityCollection.Entities.Single().Id;

            SetOperationContext.SetIdProperty(inputObject, id);

            WriteObject(inputObject);
        }

        private void UpsertCompletion(Entity targetUpdate, PSObject inputObject, UpsertResponse response)
        {
            // Delegate to context method
            var context = new SetOperationContext(inputObject, TableName, null, this, entityMetadataFactory, entityConverter, Connection, GetConversionOptions(), WriteVerbose, WriteError, WriteObject, ShouldProcess);
            context.Target = targetUpdate;
            context.UpsertCompletion(targetUpdate, response);
        }

        private void CreateNewRecord(PSObject inputObject, string tableName, Guid? callerId, EntityMetadata entityMetadata, Entity target)
        {
            // Create context for this operation
            var context = new SetOperationContext(
                inputObject,
                tableName,
                callerId,
                this,
                entityMetadataFactory,
                entityConverter,
                Connection,
                GetConversionOptions(),
                WriteVerbose,
                WriteError,
                WriteObject,
                ShouldProcess);
            context.Target = target;
            context.EntityMetadata = entityMetadata;

            // Let context build the request and set up callbacks
            context.CreateNewRecord();

            // If no requests were created (e.g., NoCreate is set), return early
            if (context.Requests.Count == 0)
            {
                return;
            }

            // Queue for batch processing if available, otherwise execute immediately
            if (_setBatchProcessor != null)
            {
                _setBatchProcessor.QueueOperation(context);
            }
            else
            {
                // Non-batch mode: execute immediately
                var request = context.Requests[0];
                if (ShouldProcess(request.RequestName))
                {
                    try
                    {
                        if (callerId.HasValue)
                        {
                            Connection.CallerId = callerId.Value;
                        }

                        var response = Connection.Execute(request);
                        
                        if (callerId.HasValue)
                        {
                            Connection.CallerId = Guid.Empty;
                        }

                        // Call completion callback
                        if (context.ResponseCompletion != null)
                        {
                            context.ResponseCompletion(response);
                        }
                    }
                    catch (Exception e)
                    {
                        if (callerId.HasValue)
                        {
                            Connection.CallerId = Guid.Empty;
                        }
                        WriteError(new ErrorRecord(e, null, ErrorCategory.WriteError, inputObject));
                    }
                }
            }
        }

        private void CreateCompletion(Entity target, PSObject inputObject, Entity targetCreate, string columnSummary, CreateResponse response)
        {
            // Delegate to context method
            var context = new SetOperationContext(inputObject, TableName, null, this, entityMetadataFactory, entityConverter, Connection, GetConversionOptions(), WriteVerbose, WriteError, WriteObject, ShouldProcess);
            context.Target = target;
            context.CreateCompletion(target, targetCreate, columnSummary, response);
        }

        private bool AssociateUpsertError(OrganizationServiceFault fault, Entity target, PSObject inputObject, ManyToManyRelationshipMetadata manyToManyRelationshipMetadata, EntityReference record1, EntityReference record2)
        {
            // Delegate to context method
            var context = new SetOperationContext(inputObject, TableName, null, this, entityMetadataFactory, entityConverter, Connection, GetConversionOptions(), WriteVerbose, WriteError, WriteObject, ShouldProcess);
            context.Target = target;
            return context.AssociateUpsertError(fault, target, manyToManyRelationshipMetadata, record1, record2);
        }

        private void AssociateUpsertCompletion(bool recordWasCreated, Entity target, PSObject inputObject, ManyToManyRelationshipMetadata manyToManyRelationshipMetadata, EntityReference record1, EntityReference record2)
        {
            // Delegate to context method
            var context = new SetOperationContext(inputObject, TableName, null, this, entityMetadataFactory, entityConverter, Connection, GetConversionOptions(), WriteVerbose, WriteError, WriteObject, ShouldProcess);
            context.Target = target;
            context.AssociateUpsertCompletion(recordWasCreated, target, manyToManyRelationshipMetadata, record1, record2);
        }

        private void AssociateCompletion(Entity target, PSObject inputObject, ManyToManyRelationshipMetadata manyToManyRelationshipMetadata, EntityReference record1, EntityReference record2)
        {
            // Delegate to context method
            var context = new SetOperationContext(inputObject, TableName, null, this, entityMetadataFactory, entityConverter, Connection, GetConversionOptions(), WriteVerbose, WriteError, WriteObject, ShouldProcess);
            context.Target = target;
            context.AssociateCompletion(target, manyToManyRelationshipMetadata, record1, record2);
        }



        private void UpdateExistingRecord(PSObject inputObject, string tableName, Guid? callerId, EntityMetadata entityMetadata, Entity target, Entity existingRecord)
        {
            if (NoUpdate.IsPresent)
            {
                WriteVerbose(string.Format("Skipped updated existing record {0}:{1} - NoUpdate enabled", TableName, Id));
                return;
            }

            target.Id = existingRecord.Id;
            target[entityMetadata.PrimaryIdAttribute] = existingRecord[entityMetadata.PrimaryIdAttribute];

            SetOperationContext.SetIdProperty(inputObject, existingRecord.Id);

            SetOperationContext.RemoveUnchangedColumns(target, existingRecord);

            if (NoUpdateColumns != null)
            {
                foreach (string noUpdateColumns in NoUpdateColumns)
                {
                    target.Attributes.Remove(noUpdateColumns);
                }
            }

            Entity targetUpdate = new Entity(target.LogicalName);
            targetUpdate.Attributes.AddRange(target.Attributes.Where(a => !SetOperationContext.DontUpdateDirectlyColumnNames.Contains(a.Key, StringComparer.OrdinalIgnoreCase)));

            if (entityMetadata.IsIntersect.GetValueOrDefault())
            {
                if (PassThru.IsPresent)
                {
                    WriteObject(inputObject);
                }
            }
            else if (targetUpdate.Attributes.Any())
            {
                DataverseEntityConverter converter = new DataverseEntityConverter(Connection, entityMetadataFactory);

                UpdateRequest request = new UpdateRequest() { Target = target };
                ApplyBypassBusinessLogicExecution(request);
                string updatedColumnSummary = SetOperationContext.GetColumnSummary(targetUpdate, entityConverter);

                if (_nextBatchItems != null)
                {
                    WriteVerbose(string.Format("Added updated of existing record {0}:{1} to batch - columns:\n{2}", TableName, existingRecord.Id, updatedColumnSummary));
                    QueueBatchItem(new BatchItem(inputObject, tableName, callerId, request, (response) => { UpdateCompletion(target, inputObject, existingRecord, updatedColumnSummary); }), CallerId);
                }
                else
                {
                    if (ShouldProcess(string.Format("Update existing record {0}:{1} columns:\n{2}", TableName, existingRecord.Id, updatedColumnSummary)))
                    {
                        try
                        {
                            Connection.Execute(request);
                            UpdateCompletion(target, inputObject, existingRecord, updatedColumnSummary);
                        }
                        catch (Exception e)
                        {
                            WriteError(new ErrorRecord(new Exception(string.Format("Error updating record {0}:{1}, {2} columns: {3}", TableName, existingRecord.Id, e.Message, updatedColumnSummary), e), null, ErrorCategory.InvalidResult, inputObject));
                        }
                    }
                }
            }
            else
            {
                WriteVerbose(string.Format("Skipped updated existing record {0}:{1} - nothing changed", TableName, Id));

                if (PassThru.IsPresent)
                {
                    WriteObject(inputObject);
                }
            }
        }

        private void UpdateCompletion(Entity target, PSObject inputObject, Entity existingRecord, string updatedColumnSummary)
        {
            // Delegate to context method
            var context = new SetOperationContext(inputObject, TableName, null, this, entityMetadataFactory, entityConverter, Connection, GetConversionOptions(), WriteVerbose, WriteError, WriteObject, ShouldProcess);
            context.Target = target;
            context.ExistingRecord = existingRecord;
            context.UpdateCompletion(target, existingRecord, updatedColumnSummary);
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
                            ScheduleRecordRetry(item.InputObject, item.TableName, item.CallerId, e);
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
                            ScheduleRecordRetry(item.InputObject, item.TableName, item.CallerId, e);
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
                            ScheduleRecordRetry(item.InputObject, item.TableName, item.CallerId, e);
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
                ProcessRecordWithExistingRecord(item.InputObject, item.TableName, item.CallerId, item.Target, item.EntityMetadata, item.ExistingRecord);
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

        private void ProcessRecordWithExistingRecord(PSObject inputObject, string tableName, Guid? callerId, Entity target, EntityMetadata entityMetadata, Entity existingRecord)
        {
            // This is the original processing logic from ProcessRecord, after GetExistingRecord
            if (Upsert.IsPresent)
            {
                UpsertRecord(inputObject, tableName, callerId, entityMetadata, target);
            }
            else
            {
                if (existingRecord != null)
                {
                    UpdateExistingRecord(inputObject, tableName, callerId, entityMetadata, target, existingRecord);
                }
                else
                {
                    CreateNewRecord(inputObject, tableName, callerId, entityMetadata, target);
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
                        QueueBatchItem(new BatchItem(inputObject, tableName, callerId, request, (response) => { AssignRecordCompletion(target, ownerid); }), CallerId);
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
                        QueueBatchItem(new BatchItem(inputObject, tableName, callerId, request, (response) => { SetStateCompletion(target, statuscode, stateCode); }), CallerId);
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




    }
}
