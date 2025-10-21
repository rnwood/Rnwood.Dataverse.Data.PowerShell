



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
    ///<summary>Deletes records from a Dataverse organization.</summary>
    [Cmdlet(VerbsCommon.Remove, "DataverseRecord", SupportsShouldProcess = true, ConfirmImpact = ConfirmImpact.Medium)]
    public class RemoveDataverseRecordCmdlet : CustomLogicBypassableOrganizationServiceCmdlet
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
                RetriesRemaining = 0;
                NextRetryTime = DateTime.MinValue;
            }

            public OrganizationRequest Request { get; set; }

            public Action<OrganizationResponse> ResponseCompletion { get; set; }

            public Func<OrganizationServiceFault, bool> ResponseExceptionCompletion { get; set; }

            public PSObject InputObject { get; set; }

            public int RetriesRemaining { get; set; }

            public DateTime NextRetryTime { get; set; }

            public override string ToString()
            {
                return Request.RequestName + " " + string.Join(", ", Request.Parameters.Select(p => $"{p.Key}='{FormatValue(p.Value)}'"));
            }

            private object FormatValue(object value)
            {
                return value is EntityReference er ? $"{er.LogicalName}:{er.Id}" : value?.ToString();
            }
        }

        private List<BatchItem> _nextBatchItems;
        private List<BatchItem> _pendingRetries;

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
                    var inputObject = InputObject;
                    QueueBatchItem(new BatchItem(inputObject, request, (response) => { DeleteCompletion(inputObject, entityName, id, (DeleteResponse)response); }, ex =>
                    {
                        if (IfExists.IsPresent && ex.ErrorCode == -2147220969)
                        {
                            WriteVerbose(string.Format("Record {0}:{1} was not present", entityName, id));
                            return true;
                        }

                        return false;
                    }));
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

        private void DeleteCompletion(PSObject inputObject, string entityName, Guid id, DeleteResponse response)
        {
            WriteVerbose(string.Format("Deleted record {0}:{1}", entityName, id));
        }

        private void QueueBatchItem(BatchItem item)
        {
            item.RetriesRemaining = Retries;
            _nextBatchItems.Add(item);

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
                    foreach (var batchItem in _nextBatchItems)
                    {
                        if (batchItem.RetriesRemaining > 0)
                        {
                            ScheduleRetry(batchItem, e);
                        }
                        else
                        {
                            WriteError(new ErrorRecord(e, null, ErrorCategory.InvalidResult, batchItem.InputObject));
                        }
                    }

                    _nextBatchItems.Clear();
                    return;
                }

                foreach (var itemResponse in response.Responses)
                {
                    BatchItem batchItem = _nextBatchItems[itemResponse.RequestIndex];

                    if (itemResponse.Fault != null)
                    {
                        // Build fault details for failed request
                        StringBuilder details = new StringBuilder();
                        AppendFaultDetails(itemResponse.Fault, details);
                        var e = new Exception(details.ToString());

                        bool handledByCompletion = batchItem.ResponseExceptionCompletion != null && batchItem.ResponseExceptionCompletion(itemResponse.Fault);

                        if (!handledByCompletion && batchItem.RetriesRemaining > 0)
                        {
                            ScheduleRetry(batchItem, e);
                        }
                        else if (!handledByCompletion)
                        {


                            WriteError(new ErrorRecord(e, null, ErrorCategory.InvalidResult, batchItem.InputObject));
                        }
                    }
                    else
                    {
                        batchItem.ResponseCompletion(itemResponse.Response);
                    }
                }
            }

            _nextBatchItems.Clear();
        }

        private void ScheduleRetry(BatchItem batchItem, Exception e)
        {
            // Schedule for retry with exponential backoff
            int attemptNumber = Retries - batchItem.RetriesRemaining + 1;
            int delayS = InitialRetryDelay * (int)Math.Pow(2, attemptNumber - 1);
            batchItem.NextRetryTime = DateTime.UtcNow.AddSeconds(delayS);
            batchItem.RetriesRemaining--;

            WriteVerbose($"Request failed, will retry in {delayS}s (attempt {attemptNumber} of {Retries + 1}): {batchItem}\n{e}");
            _pendingRetries.Add(batchItem);
        }

        /// <summary>Initializes the cmdlet.</summary>


        protected override void BeginProcessing()
        {
            base.BeginProcessing();

            // initialize cancellation token source for this pipeline invocation
            _userCancellationCts = new CancellationTokenSource();

            metadataFactory = new EntityMetadataFactory(Connection);
            _pendingRetries = new List<BatchItem>();

            if (BatchSize > 1)
            {
                _nextBatchItems = new List<BatchItem>();
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
                    var waitTime = (nextRetryTime - now).TotalSeconds;

                    if (waitTime > 0)
                    {
                        WriteVerbose($"Waiting {waitTime:F0}s for next retry batch...");
                        Thread.Sleep((int)waitTime * 1000);
                    }

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
