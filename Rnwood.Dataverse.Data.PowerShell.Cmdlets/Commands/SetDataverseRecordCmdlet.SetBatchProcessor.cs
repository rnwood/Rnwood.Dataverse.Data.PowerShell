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
                                QueryHelpers.AppendFaultDetails(itemResponse.Fault, details);
                                if (firstError == null)
                                {
                                    firstError = new Exception(details.ToString());
                                    firstFault = itemResponse.Fault;
                                }
                                hasError = true;
                            }
                        }
                        else if (context.ResponseCompletions != null && i < context.ResponseCompletions.Count && context.ResponseCompletions[i] != null)
                        {
                            // Call the completion callback for this request
                            context.ResponseCompletions[i](itemResponse.Response);
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

}
    }
