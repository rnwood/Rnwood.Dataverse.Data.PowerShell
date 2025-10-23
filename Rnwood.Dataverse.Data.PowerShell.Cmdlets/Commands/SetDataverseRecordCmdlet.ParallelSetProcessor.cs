using Microsoft.PowerPlatform.Dataverse.Client;
using Microsoft.Xrm.Sdk;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Management.Automation;
using System.Threading;
using System.Threading.Tasks;

namespace Rnwood.Dataverse.Data.PowerShell.Commands
{
    /// <summary>
    /// Manages parallel processing and batching for set operations.
    /// </summary>
    internal class ParallelSetProcessor
    {
        private readonly int _maxDegreeOfParallelism;
        private readonly uint _batchSize;
        private readonly ServiceClient _connection;
        private readonly Action<string> _writeVerbose;
        private readonly Action<ErrorRecord> _writeError;
        private readonly Action<ProgressRecord> _writeProgress;
        private readonly Func<string, bool> _shouldProcess;
        private readonly Func<bool> _isStopping;
        private readonly CancellationToken _cancellationToken;
        private readonly Action<Guid?> _setCallerId;
        private readonly Func<Guid> _getCallerId;

        private readonly ConcurrentQueue<SetOperationContext> _workQueue;
        private readonly ConcurrentQueue<string> _verboseQueue;
        private readonly ConcurrentQueue<ErrorRecord> _errorQueue;
        private readonly ConcurrentQueue<ProgressRecord> _progressQueue;
        private readonly ConcurrentQueue<object> _outputQueue;
        private readonly Action<object> _writeObject;
        private readonly List<Task> _activeTasks;
        private readonly object _tasksLock = new object();
        private int _totalQueuedCount = 0;
        private int _totalCompletedCount = 0;
        private volatile bool _inputComplete = false;

        public ParallelSetProcessor(
            int maxDegreeOfParallelism,
            uint batchSize,
            ServiceClient connection,
            Action<string> writeVerbose,
            Action<ErrorRecord> writeError,
            Action<ProgressRecord> writeProgress,
            Action<object> writeObject,
            Func<string, bool> shouldProcess,
            Func<bool> isStopping,
            CancellationToken cancellationToken,
            Action<Guid?> setCallerId,
            Func<Guid> getCallerId)
        {
            _maxDegreeOfParallelism = maxDegreeOfParallelism;
            _batchSize = batchSize;
            _connection = connection;
            _writeVerbose = writeVerbose;
            _writeError = writeError;
            _writeProgress = writeProgress;
            _writeObject = writeObject;
            _shouldProcess = shouldProcess;
            _isStopping = isStopping;
            _cancellationToken = cancellationToken;
            _setCallerId = setCallerId;
            _getCallerId = getCallerId;
            _workQueue = new ConcurrentQueue<SetOperationContext>();
            _verboseQueue = new ConcurrentQueue<string>();
            _errorQueue = new ConcurrentQueue<ErrorRecord>();
            _progressQueue = new ConcurrentQueue<ProgressRecord>();
            _outputQueue = new ConcurrentQueue<object>();
            _activeTasks = new List<Task>();

            _writeVerbose($"Initialized parallel set processor with MaxDOP={_maxDegreeOfParallelism}, BatchSize={_batchSize}");
        }

        /// <summary>
        /// Queues a set operation for parallel processing.
        /// </summary>
        public void QueueOperation(SetOperationContext context)
        {
            _workQueue.Enqueue(context);
            Interlocked.Increment(ref _totalQueuedCount);

            // Start worker tasks if we have work and capacity
            EnsureWorkersStarted();

            // Drain queues to write any pending outputs
            DrainQueues();
        }

        /// <summary>
        /// Waits for all queued operations to complete.
        /// </summary>
        public void WaitForCompletion()
        {
            _inputComplete = true;

            // Give workers a chance to pick up remaining work
            while (!_workQueue.IsEmpty && !_isStopping() && !_cancellationToken.IsCancellationRequested)
            {
                Thread.Sleep(50);
                DrainQueues();
            }

            List<Task> tasksToWait;
            lock (_tasksLock)
            {
                tasksToWait = new List<Task>(_activeTasks);
            }

            _verboseQueue.Enqueue($"Waiting for {tasksToWait.Count} parallel tasks to complete...");
            DrainQueues();

            // Wait for all active tasks to complete
            if (tasksToWait.Count > 0)
            {
                Task.WaitAll(tasksToWait.ToArray());
            }

            // Drain any remaining outputs
            DrainQueues();

            _verboseQueue.Enqueue($"All parallel set operations completed. Total processed: {_totalCompletedCount}");
            DrainQueues();

            if (_totalQueuedCount > 0)
            {
                _writeProgress(new ProgressRecord(1, "Setting Records", "All records processed")
                {
                    PercentComplete = 100,
                    RecordType = ProgressRecordType.Completed
                });
            }
        }

        /// <summary>
        /// Ensures worker tasks are started up to the maximum degree of parallelism.
        /// </summary>
        private void EnsureWorkersStarted()
        {
            lock (_tasksLock)
            {
                // Remove completed tasks
                _activeTasks.RemoveAll(t => t.IsCompleted);

                // Start new tasks if we have capacity and work to do
                while (_activeTasks.Count < _maxDegreeOfParallelism && !_workQueue.IsEmpty)
                {
                    var task = Task.Run(() => ProcessWorkItems());
                    _activeTasks.Add(task);
                    _verboseQueue.Enqueue($"Started worker task {_activeTasks.Count}/{_maxDegreeOfParallelism}");
                }
            }
        }

        /// <summary>
        /// Worker task that processes items from the work queue.
        /// </summary>
        private void ProcessWorkItems()
        {
            ServiceClient workerConnection = null;

            try
            {
                // Clone the connection for this worker
                try
                {
                    workerConnection = _connection.Clone();
                    _verboseQueue.Enqueue("Created cloned connection for parallel worker");
                }
                catch (Exception ex) when (ex is NotImplementedException ||
                                            ex.Message.Contains("On-Premises Connections are not supported") ||
                                            ex.InnerException is NotImplementedException)
                {
                    // Mock connections don't support cloning - use the original connection
                    workerConnection = _connection;
                    _verboseQueue.Enqueue("Connection cloning not supported, using shared connection");
                }

                // Create a batch processor for this worker
                SetBatchProcessor batchProcessor = null;
                if (_batchSize > 1)
                {
                    batchProcessor = new SetBatchProcessor(
                        _batchSize,
                        workerConnection,
                        (msg) => _verboseQueue.Enqueue(msg),
                        (target) => true, // Always return true - ShouldProcess was already called
                        _isStopping,
                        _cancellationToken,
                        (callerId) => workerConnection.CallerId = callerId.GetValueOrDefault(),
                        () => workerConnection.CallerId);
                }

                // Process items from the queue
                List<SetOperationContext> batchedContexts = new List<SetOperationContext>();
                
                while (!_isStopping() && !_cancellationToken.IsCancellationRequested)
                {
                    if (_workQueue.TryDequeue(out SetOperationContext originalContext))
                    {
                        try
                        {
                            // Create a new context with queued output methods for thread safety
                            var workerContext = new SetOperationContext(
                                originalContext.InputObject,
                                originalContext.TableName,
                                originalContext.CallerId,
                                originalContext,
                                originalContext.MetadataFactory,
                                originalContext.EntityConverter,
                                workerConnection,
                                originalContext.ConversionOptions,
                                (msg) => _verboseQueue.Enqueue(msg),
                                (err) => _errorQueue.Enqueue(err),
                                (obj) => _outputQueue.Enqueue(obj),
                                (target) => true); // Always return true - ShouldProcess was already called
                            
                            // Disable retries in parallel mode to avoid deadlocks
                            // Retries with shared connections can cause workers to wait indefinitely
                            workerContext.RetriesRemaining = 0;
                            
                            // Copy the context data from the original context
                            workerContext.Target = originalContext.Target;
                            workerContext.EntityMetadata = originalContext.EntityMetadata;
                            workerContext.ExistingRecord = originalContext.ExistingRecord;
                            
                            // Rebuild the requests and callbacks with the worker's output methods
                            // This ensures callbacks use the queued write methods instead of the original ones
                            if (originalContext.Requests.Count > 0)
                            {
                                var firstRequest = originalContext.Requests[0];
                                if (firstRequest is Microsoft.Xrm.Sdk.Messages.CreateRequest || firstRequest is Microsoft.Xrm.Sdk.Messages.AssociateRequest)
                                {
                                    workerContext.CreateNewRecord();
                                }
                                else if (firstRequest is Microsoft.Xrm.Sdk.Messages.UpdateRequest)
                                {
                                    workerContext.UpdateExistingRecord();
                                }
                                else if (firstRequest is Microsoft.Xrm.Sdk.Messages.UpsertRequest)
                                {
                                    workerContext.UpsertRecord();
                                }
                                else
                                {
                                    // For other request types (Assign, SetState), just copy them
                                    workerContext.Requests = new List<OrganizationRequest>(originalContext.Requests);
                                    workerContext.ResponseCompletions = new List<Action<OrganizationResponse>>(originalContext.ResponseCompletions);
                                    workerContext.ResponseExceptionCompletion = originalContext.ResponseExceptionCompletion;
                                }
                            }
                            
                            if (batchProcessor != null)
                            {
                                batchProcessor.QueueOperation(workerContext);
                                batchedContexts.Add(workerContext);
                            }
                            else
                            {
                                // For non-batched execution, execute requests directly
                                Guid oldCallerId = workerConnection.CallerId;
                                try
                                {
                                    if (workerContext.CallerId.HasValue)
                                    {
                                        workerConnection.CallerId = workerContext.CallerId.Value;
                                    }

                                    // Execute each request and call its completion callback
                                    for (int i = 0; i < workerContext.Requests.Count; i++)
                                    {
                                        var request = workerContext.Requests[i];
                                        var response = workerConnection.Execute(request);

                                        // Call completion callback for this request
                                        if (workerContext.ResponseCompletions != null && i < workerContext.ResponseCompletions.Count && workerContext.ResponseCompletions[i] != null)
                                        {
                                            workerContext.ResponseCompletions[i](response);
                                        }
                                    }
                                    
                                    int completed = Interlocked.Increment(ref _totalCompletedCount);

                                    // Update progress
                                    if (_totalQueuedCount > 0)
                                    {
                                        int percent = (int)((completed * 100L) / _totalQueuedCount);
                                        _progressQueue.Enqueue(new ProgressRecord(1, "Setting Records", $"Processed {completed} of {_totalQueuedCount} records")
                                        {
                                            PercentComplete = percent,
                                            RecordType = ProgressRecordType.Processing
                                        });
                                    }
                                }
                                catch (Exception ex)
                                {
                                    _errorQueue.Enqueue(new ErrorRecord(ex, null, ErrorCategory.InvalidOperation, originalContext.InputObject));
                                }
                                finally
                                {
                                    workerConnection.CallerId = oldCallerId;
                                }
                            }
                        }
                        catch (Exception ex)
                        {
                            _errorQueue.Enqueue(new ErrorRecord(ex, null, ErrorCategory.InvalidOperation, originalContext.InputObject));
                        }
                    }
                    else if (_inputComplete)
                    {
                        // No more work and input is complete
                        break;
                    }
                    else
                    {
                        // Queue is empty but input may not be complete, wait a bit
                        Thread.Sleep(10);
                    }
                }

                // Flush any remaining batched operations
                // NOTE: Do NOT call ProcessRetries() here - retries in parallel mode can cause deadlocks
                // when multiple workers share the same connection (e.g., mock connections).
                // Workers should only flush their batches. Any retries will fail and be reported as errors.
                if (batchProcessor != null)
                {
                    try
                    {
                        batchProcessor.Flush();
                    }
                    catch (Exception ex)
                    {
                        _verboseQueue.Enqueue($"Error flushing batch: {ex.Message}");
                    }
                    
                    // Count all batched contexts as completed
                    int completed = Interlocked.Add(ref _totalCompletedCount, batchedContexts.Count);
                    
                    // Update progress
                    if (_totalQueuedCount > 0 && batchedContexts.Count > 0)
                    {
                        int percent = (int)((completed * 100L) / _totalQueuedCount);
                        _progressQueue.Enqueue(new ProgressRecord(1, "Setting Records", $"Processed {completed} of {_totalQueuedCount} records")
                        {
                            PercentComplete = percent,
                            RecordType = ProgressRecordType.Processing
                        });
                    }
                }
            }
            catch (Exception ex)
            {
                _verboseQueue.Enqueue($"Worker task error: {ex.Message}");
            }
            finally
            {
                // Dispose the cloned connection if it was created
                if (workerConnection != null && workerConnection != _connection)
                {
                    try
                    {
                        workerConnection.Dispose();
                    }
                    catch (Exception ex)
                    {
                        _verboseQueue.Enqueue($"Error disposing worker connection: {ex.Message}");
                    }
                }
            }
        }

        /// <summary>
        /// Drains all queued messages and writes them to the cmdlet's output streams.
        /// </summary>
        private void DrainQueues()
        {
            while (_verboseQueue.TryDequeue(out var msg)) _writeVerbose(msg);
            while (_errorQueue.TryDequeue(out var err)) _writeError(err);
            while (_progressQueue.TryDequeue(out var pr)) _writeProgress(pr);
            while (_outputQueue.TryDequeue(out var obj)) _writeObject(obj);
        }
    }
}
