



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
    }

    /// <summary>
    /// Wraps a delete operation with its input record and cmdlet parameters at the time of invocation.
    /// </summary>
    internal class DeleteOperationContext : IDeleteOperationParameters
    {
        public DeleteOperationContext(
            PSObject inputObject,
            string tableName,
            Guid id,
            IDeleteOperationParameters parameters)
        {
            InputObject = inputObject;
            TableName = tableName;
            Id = id;
            BypassBusinessLogicExecution = parameters.BypassBusinessLogicExecution;
            BypassBusinessLogicExecutionStepIds = parameters.BypassBusinessLogicExecutionStepIds;
            IfExists = parameters.IfExists;
            Retries = parameters.Retries;
            InitialRetryDelay = parameters.InitialRetryDelay;
            RetriesRemaining = parameters.Retries;
            NextRetryTime = DateTime.MinValue;
        }

        public PSObject InputObject { get; }
        public string TableName { get; }
        public Guid Id { get; }
        public CustomLogicBypassableOrganizationServiceCmdlet.BusinessLogicTypes[] BypassBusinessLogicExecution { get; }
        public Guid[] BypassBusinessLogicExecutionStepIds { get; }
        public bool IfExists { get; }
        public int Retries { get; }
        public int InitialRetryDelay { get; }
        public int RetriesRemaining { get; set; }
        public DateTime NextRetryTime { get; set; }
        public OrganizationRequest Request { get; set; }

        public override string ToString()
        {
            return $"Delete {TableName}:{Id}";
        }
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
        [Parameter(Mandatory = true, ValueFromPipelineByPropertyName = true, HelpMessage = "Id of record to process")]
        public Guid Id { get; set; }
        /// <summary>
        /// Controls the maximum number of requests sent to Dataverse in one batch (where possible) to improve throughput. Specify 1 to disable. When value is 1, requests are sent to Dataverse one request at a time. When > 1, batching is used. Note that the batch will continue on error and any errors will be returned once the batch has completed. The error contains the input record to allow correlation.
        /// </summary>
        [Parameter(HelpMessage = "Controls the maximum number of requests sent to Dataverse in one batch (where possible) to improve throughput. Specify 1 to disable. When value is 1, requests are sent to Dataverse one request at a time. When > 1, batching is used. Note that the batch will continue on error and any errors will be returned once the batch has completed. The error contains the input record to allow correlation.")]
        public uint BatchSize { get; set; } = 100;
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

        private List<DeleteOperationContext> _nextBatchItems;
        private List<DeleteOperationContext> _pendingRetries;

        /// <summary>Processes each record in the pipeline.</summary>


        protected override void ProcessRecord()
        {
            base.ProcessRecord();

            Delete(TableName, Id);
        }

        private void Delete(string entityName, Guid id)
        {
            EntityMetadata metadata = metadataFactory.GetMetadata(entityName);

            if (metadata.IsIntersect.GetValueOrDefault())
            {
                if (ShouldProcess(string.Format("Delete intersect record {0}:{1}", entityName, id)))
                {
                    ManyToManyRelationshipMetadata manyToManyRelationshipMetadata = metadata.ManyToManyRelationships[0];

                    QueryExpression getRecordWithMMColumns = new QueryExpression(entityName);
                    getRecordWithMMColumns.ColumnSet = new ColumnSet(manyToManyRelationshipMetadata.Entity1IntersectAttribute, manyToManyRelationshipMetadata.Entity2IntersectAttribute);
                    getRecordWithMMColumns.Criteria.AddCondition(metadata.PrimaryIdAttribute, ConditionOperator.Equal, id);

                    Entity record = Connection.RetrieveMultiple(getRecordWithMMColumns).Entities.Single();

                    DisassociateRequest request = new DisassociateRequest()
                    {
                        Target = new EntityReference(manyToManyRelationshipMetadata.Entity1LogicalName,
                                                                         record.GetAttributeValue<Guid>(
                                                                             manyToManyRelationshipMetadata.Entity1IntersectAttribute)),
                        RelatedEntities =
                                                new EntityReferenceCollection()
                                                    {
                                    new EntityReference(manyToManyRelationshipMetadata.Entity2LogicalName,
                                                        record.GetAttributeValue<Guid>(
                                                            manyToManyRelationshipMetadata.Entity2IntersectAttribute))
                                                    },
                        Relationship = new Relationship(manyToManyRelationshipMetadata.SchemaName) { PrimaryEntityRole = EntityRole.Referencing }
                    };
                    ApplyBypassBusinessLogicExecution(request);
                    Connection.Execute(request);

                    WriteVerbose(string.Format("Deleted intersect record {0}:{1}", entityName, id));
                }
            }
            else
            {
                DeleteRequest request = new DeleteRequest { Target = new EntityReference(entityName, id) };
                ApplyBypassBusinessLogicExecution(request);

                if (_nextBatchItems != null)
                {
                    WriteVerbose(string.Format("Added delete of {0}:{1} to batch", entityName, id));
                    var context = new DeleteOperationContext(InputObject, entityName, id, this);
                    context.Request = request;
                    QueueBatchItem(context);
                }
                else
                {
                    if (ShouldProcess(string.Format("Delete record {0}:{1}", entityName, id)))
                    {
                        try
                        {
                            DeleteRequest deleteRequest = new DeleteRequest { Target = new EntityReference(entityName, id) };
                            ApplyBypassBusinessLogicExecution(deleteRequest);
                            Connection.Execute(deleteRequest);
                        }
                        catch (FaultException ex)
                        {
                            if (IfExists.IsPresent && ex.HResult == -2147220969)
                            {
                                WriteVerbose(string.Format("Record {0}:{1} was not present", entityName, id));
                            }
                            else
                            {
                                throw;
                            }
                        }
                    }
                }
            }
        }

        private void QueueBatchItem(DeleteOperationContext context)
        {
            _nextBatchItems.Add(context);

            if (_nextBatchItems.Count == BatchSize)
            {
                ProcessBatch();
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

                ExecuteMultipleResponse response = null;

                try
                {
                    response = (ExecuteMultipleResponse)Connection.Execute(batchRequest);
                }
                catch (Exception e)
                {
                    foreach (var context in _nextBatchItems)
                    {
                        if (context.RetriesRemaining > 0)
                        {
                            ScheduleRetry(context, e);
                        }
                        else
                        {
                            WriteError(new ErrorRecord(e, null, ErrorCategory.InvalidResult, context.InputObject));
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

                        bool handled = HandleDeleteFault(context, itemResponse.Fault);

                        if (!handled && context.RetriesRemaining > 0)
                        {
                            ScheduleRetry(context, e);
                        }
                        else if (!handled)
                        {
                            WriteError(new ErrorRecord(e, null, ErrorCategory.InvalidResult, context.InputObject));
                        }
                    }
                    else
                    {
                        CompleteDelete(context, (DeleteResponse)itemResponse.Response);
                    }
                }
            }

            _nextBatchItems.Clear();
        }

        private bool HandleDeleteFault(DeleteOperationContext context, OrganizationServiceFault fault)
        {
            // Handle specific error codes that should be ignored
            if (context.IfExists && fault.ErrorCode == -2147220969)
            {
                WriteVerbose(string.Format("Record {0}:{1} was not present", context.TableName, context.Id));
                return true;
            }

            return false;
        }

        private void CompleteDelete(DeleteOperationContext context, DeleteResponse response)
        {
            WriteVerbose(string.Format("Deleted record {0}:{1}", context.TableName, context.Id));
        }

        private void ScheduleRetry(DeleteOperationContext context, Exception e)
        {
            // Schedule for retry with exponential backoff
            int attemptNumber = context.Retries - context.RetriesRemaining + 1;
            int delayS = context.InitialRetryDelay * (int)Math.Pow(2, attemptNumber - 1);
            context.NextRetryTime = DateTime.UtcNow.AddSeconds(delayS);
            context.RetriesRemaining--;

            WriteVerbose($"Request failed, will retry in {delayS}s (attempt {attemptNumber} of {context.Retries + 1}): {context}\n{e}");
            _pendingRetries.Add(context);
        }

        /// <summary>Initializes the cmdlet.</summary>


        protected override void BeginProcessing()
        {
            base.BeginProcessing();

            // initialize cancellation token source for this pipeline invocation
            _userCancellationCts = new CancellationTokenSource();

            metadataFactory = new EntityMetadataFactory(Connection);
            _pendingRetries = new List<DeleteOperationContext>();

            if (BatchSize > 1)
            {
                _nextBatchItems = new List<DeleteOperationContext>();
            }
        }

        private EntityMetadataFactory metadataFactory;

        /// <summary>Completes cmdlet processing.</summary>


        protected override void EndProcessing()
        {
            base.EndProcessing();

            if (_nextBatchItems != null)
            {
                ProcessBatch();
            }

            // Process any pending retries
            ProcessRetries();

            _userCancellationCts?.Dispose();
            _userCancellationCts = null;
        }

        // Cancellation token source that is cancelled when the user hits Ctrl+C (StopProcessing)
        private CancellationTokenSource _userCancellationCts;

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
            while (!Stopping && !_userCancellationCts.IsCancellationRequested && _pendingRetries.Count > 0)
            {
                DateTime now = DateTime.UtcNow;
                var readyForRetry = _pendingRetries.Where(r => r.NextRetryTime <= now).ToList();

                if (readyForRetry.Count == 0)
                {
                    // Calculate wait time for next retry
                    var nextRetryTime = _pendingRetries.Min(r => r.NextRetryTime);
                    var waitTimeMs = (int)Math.Max(100, (nextRetryTime - now).TotalMilliseconds);

                    WriteVerbose($"Waiting {waitTimeMs / 1000.0:F1}s for next retry batch...");
                    Thread.Sleep(waitTimeMs);

                    continue;
                }

                // Remove from pending and add to batch for retry
                foreach (var item in readyForRetry)
                {
                    _pendingRetries.Remove(item);
                    _nextBatchItems.Add(item);

                    if (_nextBatchItems.Count == BatchSize)
                    {
                        ProcessBatch();
                    }
                }

                // Process any remaining items in batch
                if (_nextBatchItems.Count > 0)
                {
                    ProcessBatch();
                }
            }
        }
    }
}
