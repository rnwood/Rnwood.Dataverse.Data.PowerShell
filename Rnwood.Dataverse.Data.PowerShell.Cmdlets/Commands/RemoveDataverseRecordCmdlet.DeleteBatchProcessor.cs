using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Messages;
using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel;
using System.Text;
using System.Threading;

namespace Rnwood.Dataverse.Data.PowerShell.Commands
{
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

                        // Write verbose message once and sleep for the full duration
                        _writeVerbose($"Waiting {waitTimeMs / 1000.0:F1}s for next retry batch...");
                        
                        // Sleep for the full wait time, checking cancellation periodically
                        int sleptMs = 0;
                        while (sleptMs < waitTimeMs && !_isStopping() && !_cancellationToken.IsCancellationRequested)
                        {
                            int sleepChunkMs = Math.Min(100, waitTimeMs - sleptMs);
                            Thread.Sleep(sleepChunkMs);
                            sleptMs += sleepChunkMs;
                        }

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
                catch (FaultException<OrganizationServiceFault> ex) when (QueryHelpers.IsThrottlingException(ex, out TimeSpan retryDelay))
                {
                    // Throttling exception - schedule all items for retry with the specified delay
                    _writeVerbose($"Batch throttled by service protection. Waiting {retryDelay.TotalSeconds:F1}s before retry...");
                    foreach (var context in _nextBatchItems)
                    {
                        context.NextRetryTime = DateTime.UtcNow.Add(retryDelay);
                        _pendingRetries.Add(context);
                    }

                    _nextBatchItems.Clear();
                    return;
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
                        // Check if this is a throttling fault
                        if (QueryHelpers.IsThrottlingException(itemResponse.Fault, out TimeSpan retryDelay))
                        {
                            _writeVerbose($"Request in batch throttled by service protection. Will retry after {retryDelay.TotalSeconds:F1}s...");
                            context.NextRetryTime = DateTime.UtcNow.Add(retryDelay);
                            _pendingRetries.Add(context);
                        }
                        else
                        {
                            // Build fault details for failed request
                            StringBuilder details = new StringBuilder();
                            QueryHelpers.AppendFaultDetails(itemResponse.Fault, details);
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
                    }
                    else
                    {
                        context.Complete();
                    }
                }

                _nextBatchItems.Clear();
            }


        }
}
