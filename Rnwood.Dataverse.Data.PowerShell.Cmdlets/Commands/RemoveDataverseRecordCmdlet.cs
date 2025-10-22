



using Azure;
using Microsoft.PowerPlatform.Dataverse.Client;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Messages;
using Microsoft.Xrm.Sdk.Metadata;
using Microsoft.Xrm.Sdk.Query;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Management.Automation;
using System.ServiceModel;
using System.Text;
using System.Threading;

namespace Rnwood.Dataverse.Data.PowerShell.Commands
{
    /// <summary>
    /// Interface defining parameters for delete operations that can be shared between cmdlet and operation context.
    /// </summary>
    internal interface IDeleteOperationParameters
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
        /// Gets a value indicating whether to suppress errors if the record doesn't exist.
        /// </summary>
        bool IfExists { get; }

        /// <summary>
        /// Gets the number of retries for failed operations.
        /// </summary>
        int Retries { get; }

        /// <summary>
        /// Gets the initial retry delay in seconds.
        /// </summary>
        int InitialRetryDelay { get; }

        /// <summary>
        /// Gets the match-on column lists for finding existing records.
        /// </summary>
        string[][] MatchOn { get; }

        /// <summary>
        /// Gets a value indicating whether to allow processing multiple matching records.
        /// </summary>
        bool AllowMultipleMatches { get; }
    }

    /// <summary>
    /// Handles the complete lifecycle of a delete operation for a single record, including
    /// request creation, execution, error handling, and retry logic.
    /// </summary>
    internal class DeleteOperationContext : IDeleteOperationParameters
    {
        private readonly Action<string> _writeVerbose;
        private readonly Action<ErrorRecord> _writeError;
        private readonly Func<string, bool> _shouldProcess;

        public DeleteOperationContext(
            PSObject inputObject,
            string tableName,
            Guid id,
            IDeleteOperationParameters parameters,
            EntityMetadataFactory metadataFactory,
            IOrganizationService connection,
            Action<string> writeVerbose,
            Action<ErrorRecord> writeError,
            Func<string, bool> shouldProcess)
        {
            InputObject = inputObject;
            TableName = tableName;
            Id = id;
            BypassBusinessLogicExecution = parameters.BypassBusinessLogicExecution;
            BypassBusinessLogicExecutionStepIds = parameters.BypassBusinessLogicExecutionStepIds;
            IfExists = parameters.IfExists;
            Retries = parameters.Retries;
            InitialRetryDelay = parameters.InitialRetryDelay;
            MatchOn = parameters.MatchOn;
            AllowMultipleMatches = parameters.AllowMultipleMatches;
            RetriesRemaining = parameters.Retries;
            NextRetryTime = DateTime.MinValue;
            MetadataFactory = metadataFactory;
            Connection = connection;
            _writeVerbose = writeVerbose;
            _writeError = writeError;
            _shouldProcess = shouldProcess;
        }

        public PSObject InputObject { get; }
        public string TableName { get; }
        public Guid Id { get; }
        public CustomLogicBypassableOrganizationServiceCmdlet.BusinessLogicTypes[] BypassBusinessLogicExecution { get; }
        public Guid[] BypassBusinessLogicExecutionStepIds { get; }
        public bool IfExists { get; }
        public int Retries { get; }
        public int InitialRetryDelay { get; }
        public string[][] MatchOn { get; }
        public bool AllowMultipleMatches { get; }
        public int RetriesRemaining { get; set; }
        public DateTime NextRetryTime { get; set; }
        public OrganizationRequest Request { get; set; }
        public EntityMetadataFactory MetadataFactory { get; }
        public IOrganizationService Connection { get; }

        /// <summary>
        /// Creates the delete request for this operation, handling both regular and intersect records.
        /// </summary>
        public void CreateRequest()
        {
            EntityMetadata metadata = MetadataFactory.GetMetadata(TableName);

            if (metadata.IsIntersect.GetValueOrDefault())
            {
                // For intersect (many-to-many) records, we need to use DisassociateRequest
                ManyToManyRelationshipMetadata manyToManyRelationshipMetadata = metadata.ManyToManyRelationships[0];

                QueryExpression getRecordWithMMColumns = new QueryExpression(TableName);
                getRecordWithMMColumns.ColumnSet = new ColumnSet(
                    manyToManyRelationshipMetadata.Entity1IntersectAttribute,
                    manyToManyRelationshipMetadata.Entity2IntersectAttribute);
                getRecordWithMMColumns.Criteria.AddCondition(metadata.PrimaryIdAttribute, ConditionOperator.Equal, Id);

                Entity record = Connection.RetrieveMultiple(getRecordWithMMColumns).Entities.Single();

                DisassociateRequest request = new DisassociateRequest()
                {
                    Target = new EntityReference(manyToManyRelationshipMetadata.Entity1LogicalName,
                        record.GetAttributeValue<Guid>(manyToManyRelationshipMetadata.Entity1IntersectAttribute)),
                    RelatedEntities = new EntityReferenceCollection()
                    {
                        new EntityReference(manyToManyRelationshipMetadata.Entity2LogicalName,
                            record.GetAttributeValue<Guid>(manyToManyRelationshipMetadata.Entity2IntersectAttribute))
                    },
                    Relationship = new Relationship(manyToManyRelationshipMetadata.SchemaName) { PrimaryEntityRole = EntityRole.Referencing }
                };
                ApplyBypassBusinessLogicExecution(request);
                Request = request;
            }
            else
            {
                DeleteRequest request = new DeleteRequest { Target = new EntityReference(TableName, Id) };
                ApplyBypassBusinessLogicExecution(request);
                Request = request;
            }
        }

        /// <summary>
        /// Executes the delete operation without batching.
        /// </summary>
        public void ExecuteNonBatched()
        {
            if (_shouldProcess(string.Format("Delete record {0}:{1}", TableName, Id)))
            {
                try
                {
                    Connection.Execute(Request);
                    _writeVerbose(string.Format("Deleted record {0}:{1}", TableName, Id));
                }
                catch (FaultException ex)
                {
                    if (IfExists && ex.HResult == -2147220969)
                    {
                        _writeVerbose(string.Format("Record {0}:{1} was not present", TableName, Id));
                    }
                    else
                    {
                        throw;
                    }
                }
            }
        }

        /// <summary>
        /// Handles a fault that occurred during batch execution.
        /// </summary>
        /// <returns>True if the fault was handled and should not be reported as an error.</returns>
        public bool HandleFault(OrganizationServiceFault fault)
        {
            // Handle specific error codes that should be ignored
            if (IfExists && fault.ErrorCode == -2147220969)
            {
                _writeVerbose(string.Format("Record {0}:{1} was not present", TableName, Id));
                return true;
            }

            return false;
        }

        /// <summary>
        /// Called when the delete operation completes successfully.
        /// </summary>
        public void Complete()
        {
            _writeVerbose(string.Format("Deleted record {0}:{1}", TableName, Id));
        }

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

        private void ApplyBypassBusinessLogicExecution(OrganizationRequest request)
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

        public override string ToString()
        {
            return $"Delete {TableName}:{Id}";
        }
    }

    /// <summary>
    /// Manages batching and retry logic for delete operations.
    /// </summary>
    internal class DeleteBatchProcessor
    {
        private readonly List<DeleteOperationContext> _nextBatchItems;
        private readonly List<DeleteOperationContext> _pendingRetries;
        private readonly uint _batchSize;
        private readonly IOrganizationService _connection;
        private readonly Action<string> _writeVerbose;
        private readonly Func<string, bool> _shouldProcess;
        private readonly Func<bool> _isStopping;
        private readonly CancellationToken _cancellationToken;

        public DeleteBatchProcessor(
            uint batchSize,
            IOrganizationService connection,
            Action<string> writeVerbose,
            Func<string, bool> shouldProcess,
            Func<bool> isStopping,
            CancellationToken cancellationToken)
        {
            _batchSize = batchSize;
            _connection = connection;
            _writeVerbose = writeVerbose;
            _shouldProcess = shouldProcess;
            _isStopping = isStopping;
            _cancellationToken = cancellationToken;
            _nextBatchItems = new List<DeleteOperationContext>();
            _pendingRetries = new List<DeleteOperationContext>();
        }

        /// <summary>
        /// Adds an operation to the batch, executing the batch if it reaches the batch size.
        /// </summary>
        public void QueueOperation(DeleteOperationContext context)
        {
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

            foreach (var itemResponse in response.Responses)
            {
                DeleteOperationContext context = _nextBatchItems[itemResponse.RequestIndex];

                if (itemResponse.Fault != null)
                {
                    // Build fault details for failed request
                    StringBuilder details = new StringBuilder();
                    AppendFaultDetails(itemResponse.Fault, details);
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
                    context.Complete();
                }
            }

            _nextBatchItems.Clear();
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
    }

    /// <summary>
    /// Represents an InputObject queued for MatchOn resolution in RemoveDataverseRecordCmdlet.
    /// </summary>
    internal class MatchOnResolveItem
    {
        public PSObject InputObject { get; set; }
        public Entity InputEntity { get; set; }
        public List<Guid> ResolvedIds { get; set; }
    }

    ///<summary>Deletes records from a Dataverse organization.</summary>
    [Cmdlet(VerbsCommon.Remove, "DataverseRecord", SupportsShouldProcess = true, ConfirmImpact = ConfirmImpact.Medium)]
    public class RemoveDataverseRecordCmdlet : CustomLogicBypassableOrganizationServiceCmdlet, IDeleteOperationParameters
    {
        /// <summary>
        /// Record from pipeline. This allows piping in record to delete.
        /// </summary>
        [Parameter(ValueFromPipeline = true, HelpMessage = "Record from pipeline. This allows piping in record to delete.")]
        public PSObject InputObject { get; set; }
        /// <summary>
        /// The logical name of the table to operate on.
        /// </summary>
        [Parameter(Mandatory = true, ValueFromPipelineByPropertyName = true, HelpMessage = "Logical name of table")]
        [Alias("EntityName")]
        [ArgumentCompleter(typeof(TableNameArgumentCompleter))]
        public string TableName { get; set; }
        /// <summary>
        /// Id of record to process
        /// </summary>
        [Parameter(Mandatory = false, ValueFromPipelineByPropertyName = true, HelpMessage = "Id of record to process")]
        public Guid Id { get; set; }
        /// <summary>
        /// List of list of column names that identify records to delete based on the values of those columns in the InputObject. The first list that returns a match is used. If AllowMultipleMatches is not specified, an error will be raised if more than one record matches.
        /// </summary>
        [Parameter(Mandatory = false, HelpMessage = "List of list of column names that identify records to delete based on the values of those columns in the InputObject. The first list that returns a match is used. If AllowMultipleMatches is not specified, an error will be raised if more than one record matches.")]
        public string[][] MatchOn { get; set; }
        /// <summary>
        /// If specified, allows deletion of multiple records when MatchOn criteria matches more than one record. Without this switch, an error is raised if MatchOn finds multiple matches.
        /// </summary>
        [Parameter(HelpMessage = "If specified, allows deletion of multiple records when MatchOn criteria matches more than one record. Without this switch, an error is raised if MatchOn finds multiple matches.")]
        public SwitchParameter AllowMultipleMatches { get; set; }
        /// <summary>
        /// Controls the maximum number of requests sent to Dataverse in one batch (where possible) to improve throughput. Specify 1 to disable. When value is 1, requests are sent to Dataverse one request at a time. When > 1, batching is used. Note that the batch will continue on error and any errors will be returned once the batch has completed. The error contains the input record to allow correlation.
        /// </summary>
        [Parameter(HelpMessage = "Controls the maximum number of requests sent to Dataverse in one batch (where possible) to improve throughput. Specify 1 to disable. When value is 1, requests are sent to Dataverse one request at a time. When > 1, batching is used. Note that the batch will continue on error and any errors will be returned once the batch has completed. The error contains the input record to allow correlation.")]
        public uint BatchSize { get; set; } = 100;
        
        /// <summary>
        /// Controls the maximum number of records to resolve in a single query when using MatchOn. Default is 500. Specify 1 to resolve one record at a time.
        /// </summary>
        [Parameter(HelpMessage = "Controls the maximum number of records to resolve in a single query when using MatchOn. Default is 500. Specify 1 to resolve one record at a time.")]
        public uint RetrievalBatchSize { get; set; } = 500;
        /// <summary>
        /// If specified, the cmdlet will not raise an error if the record does not exist.
        /// </summary>
        [Parameter(HelpMessage = "If specified, the cmdlet will not raise an error if the record does not exist.")]
        public SwitchParameter IfExists { get; set; }

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
        /// Number of times to retry each batch item on failure. Default is 0 (no retries).
        /// </summary>
        [Parameter(HelpMessage = "Number of times to retry each batch item on failure. Default is 0 (no retries).")]
        public int Retries { get; set; } = 0;

        /// <summary>
        /// Initial delay in seconds before first retry. Subsequent retries use exponential backoff. Default is 5s.
        /// </summary>
        [Parameter(HelpMessage = "Initial delay in seconds before first retry. Subsequent retries use exponential backoff. Default is 5s.")]
        public int InitialRetryDelay { get; set; } = 5;

        // Explicit interface implementations for IDeleteOperationParameters
        bool IDeleteOperationParameters.IfExists => IfExists.IsPresent;
        CustomLogicBypassableOrganizationServiceCmdlet.BusinessLogicTypes[] IDeleteOperationParameters.BypassBusinessLogicExecution => BypassBusinessLogicExecution;
        Guid[] IDeleteOperationParameters.BypassBusinessLogicExecutionStepIds => BypassBusinessLogicExecutionStepIds;
        int IDeleteOperationParameters.Retries => Retries;
        int IDeleteOperationParameters.InitialRetryDelay => InitialRetryDelay;
        string[][] IDeleteOperationParameters.MatchOn => MatchOn;
        bool IDeleteOperationParameters.AllowMultipleMatches => AllowMultipleMatches.IsPresent;

        private EntityMetadataFactory _metadataFactory;
        private DeleteBatchProcessor _batchProcessor;
        private CancellationTokenSource _userCancellationCts;
        private DataverseEntityConverter _entityConverter;
        private List<MatchOnResolveItem> _matchOnResolveQueue;

        /// <summary>Initializes the cmdlet.</summary>
        protected override void BeginProcessing()
        {
            base.BeginProcessing();

            // initialize cancellation token source for this pipeline invocation
            _userCancellationCts = new CancellationTokenSource();

            _metadataFactory = new EntityMetadataFactory(Connection);
            _entityConverter = new DataverseEntityConverter(Connection, _metadataFactory);
            _matchOnResolveQueue = new List<MatchOnResolveItem>();

            if (BatchSize > 1)
            {
                _batchProcessor = new DeleteBatchProcessor(
                    BatchSize,
                    Connection,
                    WriteVerbose,
                    ShouldProcess,
                    () => Stopping,
                    _userCancellationCts.Token);
            }
        }

        /// <summary>Processes each record in the pipeline.</summary>
        protected override void ProcessRecord()
        {
            base.ProcessRecord();

            // Validate parameters
            if (Id == Guid.Empty && MatchOn == null)
            {
                WriteError(new ErrorRecord(
                    new ArgumentException("Either Id or MatchOn must be specified"),
                    null,
                    ErrorCategory.InvalidArgument,
                    InputObject));
                return;
            }

            // If MatchOn is specified, queue for batched resolution
            if (MatchOn != null)
            {
                try
                {
                    // Convert InputObject to Entity to get values for matching
                    // Use the existing entity converter instance for efficiency
                    var conversionOptions = new ConvertToDataverseEntityOptions();
                    Entity inputEntity = _entityConverter.ConvertToDataverseEntity(InputObject, TableName, conversionOptions);

                    _matchOnResolveQueue.Add(new MatchOnResolveItem
                    {
                        InputObject = InputObject,
                        InputEntity = inputEntity,
                        ResolvedIds = new List<Guid>()
                    });

                    // Process batch if it's full
                    if (_matchOnResolveQueue.Count >= RetrievalBatchSize)
                    {
                        ProcessMatchOnBatch();
                    }
                }
                catch (Exception e)
                {
                    WriteError(new ErrorRecord(e, null, ErrorCategory.InvalidOperation, InputObject));
                }
                return;
            }

            // If Id is specified, process immediately
            if (Id != Guid.Empty)
            {
                ProcessDeleteById(InputObject, Id);
            }
        }

        /// <summary>
        /// Resolves record IDs based on MatchOn criteria.
        /// </summary>
        private List<Guid> ResolveRecordsByMatchOn()
        {
            List<Guid> resolvedIds = new List<Guid>();
            EntityMetadata entityMetadata = _metadataFactory.GetMetadata(TableName);

            // Convert InputObject to Entity to get values for matching
            var entityConverter = new DataverseEntityConverter(Connection, _metadataFactory);
            var conversionOptions = new ConvertToDataverseEntityOptions();
            Entity inputEntity = entityConverter.ConvertToDataverseEntity(InputObject, TableName, conversionOptions);

            foreach (string[] matchOnColumnList in MatchOn)
            {
                QueryByAttribute matchOnQuery = new QueryByAttribute(TableName);
                matchOnQuery.ColumnSet = new ColumnSet(entityMetadata.PrimaryIdAttribute);

                foreach (string matchOnColumn in matchOnColumnList)
                {
                    object queryValue = inputEntity.GetAttributeValue<object>(matchOnColumn);

                    if (queryValue is EntityReference er)
                    {
                        queryValue = er.Id;
                    }

                    if (queryValue is OptionSetValue osv)
                    {
                        queryValue = osv.Value;
                    }

                    matchOnQuery.AddAttributeValue(matchOnColumn, queryValue);
                }

                var matchingRecords = Connection.RetrieveMultiple(matchOnQuery).Entities;

                if (matchingRecords.Count > 0)
                {
                    resolvedIds.AddRange(matchingRecords.Select(r => r.Id));
                    break; // First match set wins
                }
            }

            return resolvedIds;
        }

        /// <summary>
        /// Processes a batch of MatchOn resolutions using efficient batched queries.
        /// </summary>
        private void ProcessMatchOnBatch()
        {
            if (_matchOnResolveQueue == null || _matchOnResolveQueue.Count == 0)
            {
                return;
            }

            WriteVerbose($"Processing MatchOn batch of {_matchOnResolveQueue.Count} record(s)");

            EntityMetadata entityMetadata = _metadataFactory.GetMetadata(TableName);

            // Try each MatchOn column list in order
            foreach (string[] matchOnColumnList in MatchOn)
            {
                var itemsNeedingMatch = _matchOnResolveQueue.Where(item => item.ResolvedIds.Count == 0).ToList();
                if (itemsNeedingMatch.Count == 0)
                {
                    break;
                }

                if (matchOnColumnList.Length == 1)
                {
                    // Single column - use In operator for efficiency
                    var matchColumn = matchOnColumnList[0];
                    var matchValues = itemsNeedingMatch.Select(item =>
                    {
                        var val = item.InputEntity.GetAttributeValue<object>(matchColumn);
                        if (val is EntityReference er) return (object)er.Id;
                        if (val is OptionSetValue osv) return (object)osv.Value;
                        return val;
                    }).Distinct().ToList();

                    // Check if any values are null - this indicates a conversion problem
                    if (matchValues.Any(v => v == null))
                    {
                        WriteError(new ErrorRecord(
                            new Exception($"MatchOn column '{matchColumn}' has null value in InputObject - conversion may have failed"),
                            null,
                            ErrorCategory.InvalidData,
                            null));
                        continue;
                    }

                    // For single-column MatchOn with single value, use QueryByAttribute for compatibility
                    if (matchValues.Count == 1)
                    {
                        // Use QueryByAttribute - same as original non-batched code
                        QueryByAttribute matchOnQuery = new QueryByAttribute(TableName);
                        // Include both the primary ID and the match column for matching back
                        matchOnQuery.ColumnSet = new ColumnSet(new[] { entityMetadata.PrimaryIdAttribute, matchColumn });
                        matchOnQuery.AddAttributeValue(matchColumn, matchValues[0]);

                        WriteVerbose($"Retrieving records by MatchOn ({matchColumn}) using QueryByAttribute");

                        var retrievedRecords = Connection.RetrieveMultiple(matchOnQuery).Entities;

                        // Match back to items
                        foreach (var item in itemsNeedingMatch)
                        {
                            var itemValue = item.InputEntity.GetAttributeValue<object>(matchColumn);
                            if (itemValue is EntityReference er) itemValue = er.Id;
                            if (itemValue is OptionSetValue osv) itemValue = osv.Value;

                            var matches = retrievedRecords.Where(e =>
                            {
                                var recValue = e.GetAttributeValue<object>(matchColumn);
                                if (recValue is EntityReference er2) recValue = er2.Id;
                                if (recValue is OptionSetValue osv2) recValue = osv2.Value;
                                return Equals(itemValue, recValue);
                            }).ToList();

                            if (matches.Count > 0)
                            {
                                item.ResolvedIds.AddRange(matches.Select(m => m.Id));
                            }
                        }
                    }
                    else
                    {
                        // For multiple values, use QueryExpression with In operator
                        var query = new QueryExpression(TableName)
                        {
                            // Include both the primary ID and the match column for matching back
                            ColumnSet = new ColumnSet(new[] { entityMetadata.PrimaryIdAttribute, matchColumn })
                        };

                        query.Criteria.AddCondition(matchColumn, ConditionOperator.In, matchValues.ToArray());

                        WriteVerbose($"Retrieving records by MatchOn ({matchColumn}) in batch");

                        var retrievedRecords = Connection.RetrieveMultiple(query).Entities;

                        // Match back to items
                        foreach (var item in itemsNeedingMatch)
                        {
                            var itemValue = item.InputEntity.GetAttributeValue<object>(matchColumn);
                            if (itemValue is EntityReference er) itemValue = er.Id;
                            if (itemValue is OptionSetValue osv) itemValue = osv.Value;

                            var matches = retrievedRecords.Where(e =>
                            {
                                var recValue = e.GetAttributeValue<object>(matchColumn);
                                if (recValue is EntityReference er2) recValue = er2.Id;
                                if (recValue is OptionSetValue osv2) recValue = osv2.Value;
                                return Equals(itemValue, recValue);
                            }).ToList();

                            if (matches.Count > 0)
                            {
                                item.ResolvedIds.AddRange(matches.Select(m => m.Id));
                            }
                        }
                    }
                }
                else
                {
                    // Multi-column - use Or with And conditions
                    var query = new QueryExpression(TableName)
                    {
                        // Include the primary ID and all match columns for matching back
                        ColumnSet = new ColumnSet(new[] { entityMetadata.PrimaryIdAttribute }.Concat(matchOnColumnList).ToArray())
                    };

                    var orFilter = new FilterExpression(LogicalOperator.Or);

                    foreach (var item in itemsNeedingMatch)
                    {
                        var andFilter = new FilterExpression(LogicalOperator.And);
                        foreach (var matchColumn in matchOnColumnList)
                        {
                            var queryValue = item.InputEntity.GetAttributeValue<object>(matchColumn);
                            if (queryValue is EntityReference er1) queryValue = er1.Id;
                            if (queryValue is OptionSetValue osv1) queryValue = osv1.Value;

                            andFilter.AddCondition(matchColumn, ConditionOperator.Equal, queryValue);
                        }
                        orFilter.AddFilter(andFilter);
                    }

                    query.Criteria.AddFilter(orFilter);

                    WriteVerbose($"Retrieving records by MatchOn ({string.Join(",", matchOnColumnList)}) in batch");

                    var retrievedRecords = Connection.RetrieveMultiple(query).Entities;

                    // Match back to items
                    foreach (var item in itemsNeedingMatch)
                    {
                        var matches = retrievedRecords.Where(e =>
                        {
                            return matchOnColumnList.All(col =>
                            {
                                var itemValue = item.InputEntity.GetAttributeValue<object>(col);
                                var recValue = e.GetAttributeValue<object>(col);

                                if (itemValue is EntityReference er1) itemValue = er1.Id;
                                if (itemValue is OptionSetValue osv1) itemValue = osv1.Value;
                                if (recValue is EntityReference er2) recValue = er2.Id;
                                if (recValue is OptionSetValue osv2) recValue = osv2.Value;

                                return Equals(itemValue, recValue);
                            });
                        }).ToList();

                        if (matches.Count > 0)
                        {
                            item.ResolvedIds.AddRange(matches.Select(m => m.Id));
                        }
                    }
                }
            }

            // Process all items in the batch - validate and delete
            foreach (var item in _matchOnResolveQueue)
            {
                if (item.ResolvedIds.Count == 0)
                {
                    if (!IfExists.IsPresent)
                    {
                        WriteError(new ErrorRecord(
                            new Exception("No records found matching the MatchOn criteria"),
                            null,
                            ErrorCategory.ObjectNotFound,
                            item.InputObject));
                    }
                    else
                    {
                        WriteVerbose("No records found matching the MatchOn criteria");
                    }
                }
                else if (item.ResolvedIds.Count > 1 && !AllowMultipleMatches.IsPresent)
                {
                    WriteError(new ErrorRecord(
                        new Exception($"MatchOn criteria matched {item.ResolvedIds.Count} records. Use -AllowMultipleMatches to delete all matching records."),
                        null,
                        ErrorCategory.InvalidOperation,
                        item.InputObject));
                }
                else
                {
                    // Process deletions for this item
                    foreach (var idToDelete in item.ResolvedIds)
                    {
                        ProcessDeleteById(item.InputObject, idToDelete);
                    }
                }
            }

            _matchOnResolveQueue.Clear();
        }

        /// <summary>
        /// Processes deletion of a single record by ID.
        /// </summary>
        private void ProcessDeleteById(PSObject inputObject, Guid idToDelete)
        {
            var context = new DeleteOperationContext(
                inputObject,
                TableName,
                idToDelete,
                this,
                _metadataFactory,
                Connection,
                WriteVerbose,
                WriteError,
                ShouldProcess);

            context.CreateRequest();

            if (_batchProcessor != null)
            {
                WriteVerbose(string.Format("Added delete of {0}:{1} to batch", TableName, idToDelete));
                _batchProcessor.QueueOperation(context);
            }
            else
            {
                context.ExecuteNonBatched();
            }
        }

        /// <summary>Completes cmdlet processing.</summary>
        protected override void EndProcessing()
        {
            base.EndProcessing();

            // Process any remaining queued MatchOn items
            if (_matchOnResolveQueue != null && _matchOnResolveQueue.Count > 0)
            {
                ProcessMatchOnBatch();
            }

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
            // Called when user presses Ctrl+C. Signal cancellation to any ongoing operations.
            try
            {
                _userCancellationCts?.Cancel();
            }
            catch { }
            base.StopProcessing();
        }
    }
}
