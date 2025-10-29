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
    /// Manages parallel processing and batching for delete operations.
    /// </summary>
    internal class ParallelDeleteProcessor
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
        private readonly IDeleteOperationParameters _parameters;

        private readonly ConcurrentQueue<DeleteOperationContext> _workQueue;
        private readonly ConcurrentQueue<string> _verboseQueue;
        private readonly ConcurrentQueue<ErrorRecord> _errorQueue;
        private readonly ConcurrentQueue<ProgressRecord> _progressQueue;
        private readonly List<Task> _activeTasks;
        private readonly object _tasksLock = new object();
        private int _totalQueuedCount = 0;
        private int _totalCompletedCount = 0;
        private volatile bool _inputComplete = false;

        public ParallelDeleteProcessor(
            int maxDegreeOfParallelism,
            uint batchSize,
            ServiceClient connection,
            Action<string> writeVerbose,
            Action<ErrorRecord> writeError,
            Action<ProgressRecord> writeProgress,
            Func<string, bool> shouldProcess,
            Func<bool> isStopping,
            CancellationToken cancellationToken,
            IDeleteOperationParameters parameters)
        {
            _maxDegreeOfParallelism = maxDegreeOfParallelism;
            _batchSize = batchSize;
            _connection = connection;
            _writeVerbose = writeVerbose;
            _writeError = writeError;
            _writeProgress = writeProgress;
            _shouldProcess = shouldProcess;
            _isStopping = isStopping;
            _cancellationToken = cancellationToken;
            _parameters = parameters;
            _workQueue = new ConcurrentQueue<DeleteOperationContext>();
            _verboseQueue = new ConcurrentQueue<string>();
            _errorQueue = new ConcurrentQueue<ErrorRecord>();
            _progressQueue = new ConcurrentQueue<ProgressRecord>();
            _activeTasks = new List<Task>();

            _writeVerbose($"Initialized parallel delete processor with MaxDOP={_maxDegreeOfParallelism}, BatchSize={_batchSize}");
        }

        /// <summary>
        /// Queues a delete operation for parallel processing.
        /// </summary>
        public void QueueOperation(DeleteOperationContext context)
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

            _verboseQueue.Enqueue($"All parallel delete operations completed. Total processed: {_totalCompletedCount}");
            DrainQueues();

            if (_totalQueuedCount > 0)
            {
                _writeProgress(new ProgressRecord(1, "Deleting Records", "All records processed")
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
                DeleteBatchProcessor batchProcessor = null;
                if (_batchSize > 1)
                {
                    batchProcessor = new DeleteBatchProcessor(
                        _batchSize,
                        workerConnection,
                        (msg) => _verboseQueue.Enqueue(msg),
                        (target) => true, // Always return true - ShouldProcess was already called
                        _isStopping,
                        _cancellationToken);
                }

                // Process items from the queue
                List<DeleteOperationContext> batchedContexts = new List<DeleteOperationContext>();
                
                while (!_isStopping() && !_cancellationToken.IsCancellationRequested)
                {
                    if (_workQueue.TryDequeue(out DeleteOperationContext originalContext))
                    {
                        try
                        {
                            // Create a new context with queued output methods for thread safety
                            var workerContext = new DeleteOperationContext(
                                originalContext.InputObject,
                                originalContext.TableName,
                                originalContext.Id,
                                _parameters,
                                originalContext.MetadataFactory,
                                workerConnection,
                                (msg) => _verboseQueue.Enqueue(msg),
                                (err) => _errorQueue.Enqueue(err),
                                (target) => true); // Always return true - ShouldProcess was already called
                            
                            // Copy the request from the original context
                            workerContext.Request = originalContext.Request;
                            
                            // Update the connection in the context's request if needed
                            // The request was already created by the original context
                            if (batchProcessor != null)
                            {
                                batchProcessor.QueueOperation(workerContext);
                                batchedContexts.Add(workerContext);
                            }
                            else
                            {
                                // For non-batched execution, execute request directly with retry support
                                bool success = false;
                                
                                while (!success && workerContext.RetriesRemaining >= 0)
                                {
                                    try
                                    {
                                        workerConnection.Execute(workerContext.Request);
                                        _verboseQueue.Enqueue(string.Format("Deleted record {0}:{1}", workerContext.TableName, workerContext.Id));
                                        
                                        success = true;
                                        
                                        int completed = Interlocked.Increment(ref _totalCompletedCount);

                                        // Update progress
                                        if (_totalQueuedCount > 0)
                                        {
                                            int percent = (int)((completed * 100L) / _totalQueuedCount);
                                            _progressQueue.Enqueue(new ProgressRecord(1, "Deleting Records", $"Deleted {completed} of {_totalQueuedCount} records")
                                            {
                                                PercentComplete = percent,
                                                RecordType = ProgressRecordType.Processing
                                            });
                                        }
                                    }
                                    catch (System.ServiceModel.FaultException ex)
                                    {
                                        // Check for "record not found" error code OR if message contains "Does Not Exist"
                                        // Different versions of FakeXrmEasy may set HResult differently (sometimes 0, sometimes -2147220969)
                                        if (workerContext.IfExists && (ex.HResult == -2147220969 || (ex.Message != null && ex.Message.Contains("Does Not Exist"))))
                                        {
                                            _verboseQueue.Enqueue(string.Format("Record {0}:{1} was not present", workerContext.TableName, workerContext.Id));
                                            success = true; // Not an error, record wasn't there
                                        }
                                        else
                                        {
                                            if (workerContext.RetriesRemaining > 0)
                                            {
                                                workerContext.ScheduleRetry(ex);
                                                
                                                // Wait for retry delay
                                                int waitMs = (int)(workerContext.NextRetryTime - DateTime.UtcNow).TotalMilliseconds;
                                                if (waitMs > 0)
                                                {
                                                    Thread.Sleep(waitMs);
                                                }
                                            }
                                            else
                                            {
                                                // No more retries, queue error and exit loop
                                                _errorQueue.Enqueue(new ErrorRecord(ex, null, ErrorCategory.InvalidOperation, originalContext.InputObject));
                                                break;
                                            }
                                        }
                                    }
                                    catch (Exception ex)
                                    {
                                        if (workerContext.RetriesRemaining > 0)
                                        {
                                            workerContext.ScheduleRetry(ex);
                                            
                                            // Wait for retry delay
                                            int waitMs = (int)(workerContext.NextRetryTime - DateTime.UtcNow).TotalMilliseconds;
                                            if (waitMs > 0)
                                            {
                                                Thread.Sleep(waitMs);
                                            }
                                        }
                                        else
                                        {
                                            // No more retries, queue error and exit loop
                                            _errorQueue.Enqueue(new ErrorRecord(ex, null, ErrorCategory.InvalidOperation, originalContext.InputObject));
                                            break;
                                        }
                                    }
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
                if (batchProcessor != null)
                {
                    batchProcessor.Flush();
                    batchProcessor.ProcessRetries();
                    
                    // Count all batched contexts as completed
                    int completed = Interlocked.Add(ref _totalCompletedCount, batchedContexts.Count);
                    
                    // Update progress
                    if (_totalQueuedCount > 0 && batchedContexts.Count > 0)
                    {
                        int percent = (int)((completed * 100L) / _totalQueuedCount);
                        _progressQueue.Enqueue(new ProgressRecord(1, "Deleting Records", $"Deleted {completed} of {_totalQueuedCount} records")
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
        }
    }
}
