using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Messages;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Management.Automation;
using System.ServiceModel;
using System.Text;
using System.Threading;

namespace Rnwood.Dataverse.Data.PowerShell.Commands
{
    public partial class InvokeDataverseRequestCmdlet
    {
        /// <summary>
        /// Batch processor for generic OrganizationRequest batching (uses ExecuteMultipleRequest).
        /// </summary>
        internal class RequestBatchProcessor
        {
            private readonly List<RequestOperationContext> _nextBatchItems;
            private readonly List<RequestOperationContext> _pendingRetries;
            private readonly uint _batchSize;
            private readonly IOrganizationService _connection;
            private readonly Action<string> _writeVerbose;
            private readonly Action<object> _writeObject;
            private readonly Action<ErrorRecord> _writeError;
            private readonly Func<string, bool> _shouldProcess;
            private readonly Func<bool> _isStopping;
            private readonly CancellationToken _cancellationToken;
            private readonly CustomLogicBypassableOrganizationServiceCmdlet.BusinessLogicTypes[] _bypassBusinessLogicExecution;
            private readonly Guid[] _bypassBusinessLogicExecutionStepIds;

            public RequestBatchProcessor(uint batchSize, IOrganizationService connection, Action<string> writeVerbose, Action<object> writeObject, Action<ErrorRecord> writeError, Func<string, bool> shouldProcess, Func<bool> isStopping, CancellationToken cancellationToken, CustomLogicBypassableOrganizationServiceCmdlet.BusinessLogicTypes[] bypassBusinessLogicExecution, Guid[] bypassBusinessLogicExecutionStepIds)
            {
                _batchSize = batchSize;
                _connection = connection;
                _writeVerbose = writeVerbose;
                _writeObject = writeObject;
                _writeError = writeError;
                _shouldProcess = shouldProcess;
                _isStopping = isStopping;
                _cancellationToken = cancellationToken;
                _bypassBusinessLogicExecution = bypassBusinessLogicExecution;
                _bypassBusinessLogicExecutionStepIds = bypassBusinessLogicExecutionStepIds;
                _nextBatchItems = new List<RequestOperationContext>();
                _pendingRetries = new List<RequestOperationContext>();
            }

            public void QueueOperation(RequestOperationContext context)
            {
                _nextBatchItems.Add(context);

                if (_nextBatchItems.Count >= _batchSize)
                {
                    ExecuteBatch();
                }
            }

            public void Flush()
            {
                if (_nextBatchItems.Count > 0)
                {
                    ExecuteBatch();
                }
            }

            public void ProcessRetries()
            {
                while (!_isStopping() && !_cancellationToken.IsCancellationRequested && _pendingRetries.Count > 0)
                {
                    DateTime now = DateTime.UtcNow;
                    var readyForRetry = _pendingRetries.Where(r => r.NextRetryTime <= now).ToList();

                    if (readyForRetry.Count == 0)
                    {
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

                    foreach (var item in readyForRetry)
                    {
                        _pendingRetries.Remove(item);
                        _nextBatchItems.Add(item);

                        if (_nextBatchItems.Count >= _batchSize)
                        {
                            ExecuteBatch();
                        }
                    }

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

                // Apply bypass logic at batch level
                if (_bypassBusinessLogicExecution?.Length > 0)
                {
                    batchRequest.Parameters["BypassBusinessLogicExecution"] = string.Join(",", _bypassBusinessLogicExecution.Select(o => o.ToString()));
                }
                if (_bypassBusinessLogicExecutionStepIds?.Length > 0)
                {
                    batchRequest.Parameters["BypassBusinessLogicExecutionStepIds"] = string.Join(",", _bypassBusinessLogicExecutionStepIds.Select(id => id.ToString()));
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

                for (int i = 0; i < response.Responses.Count; i++)
                {
                    var itemResponse = response.Responses[i];
                    var context = _nextBatchItems[itemResponse.RequestIndex];

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
                        context.Complete(itemResponse.Response);
                    }
                }

                _nextBatchItems.Clear();
            }


        }
    }
}
