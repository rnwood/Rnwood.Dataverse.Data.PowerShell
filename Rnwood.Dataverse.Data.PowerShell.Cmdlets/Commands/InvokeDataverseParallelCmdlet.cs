using Microsoft.PowerPlatform.Dataverse.Client;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Management.Automation;
using System.Management.Automation.Runspaces;
using System.Threading;
using System.Threading.Tasks;
using System.Collections.Concurrent;

namespace Rnwood.Dataverse.Data.PowerShell.Commands
{
    /// <summary>
    /// Processes input objects in parallel using chunked batches with cloned Dataverse connections.
    /// </summary>
    [Cmdlet(VerbsLifecycle.Invoke, "DataverseParallel")]
    [OutputType(typeof(PSObject))]
    public class InvokeDataverseParallelCmdlet : OrganizationServiceCmdlet
    {
        /// <summary>
        /// Gets or sets the script block to execute for each chunk of input objects.
        /// </summary>
        [Parameter(Mandatory = true, Position = 0, HelpMessage = "Script block to execute for each chunk. The chunk is available as $_ and a cloned connection is set as the default connection.")]
        public ScriptBlock ScriptBlock { get; set; }

        /// <summary>
        /// Gets or sets the input objects to process.
        /// </summary>
        [Parameter(Mandatory = true, ValueFromPipeline = true, HelpMessage = "Input objects to process in parallel.")]
        public PSObject InputObject { get; set; }

        /// <summary>
        /// Gets or sets the chunk size for batching input objects.
        /// </summary>
        [Parameter(Mandatory = false, HelpMessage = "Number of input objects to process in each parallel batch. Default is 50.")]
        public int ChunkSize { get; set; } = 50;

        /// <summary>
        /// Gets or sets the maximum degree of parallelism.
        /// </summary>
        [Parameter(Mandatory = false, HelpMessage = "Maximum number of parallel operations. Default is the number of processors.")]
        public int MaxDegreeOfParallelism { get; set; } = Environment.ProcessorCount;

        /// <summary>
        /// Gets or sets the modules to exclude when setting up parallel runspaces.
        /// </summary>
        [Parameter(Mandatory = false, HelpMessage = "Array of module name patterns (supports wildcards) to exclude from parallel runspaces. Pester is always excluded. Example: @('PSReadLine', 'Test*')")]
        public string[] ExcludeModule { get; set; } = new string[0];

        /// <summary>
        /// Helper class to track task information including chunk number.
        /// </summary>
        private class TaskInfo
        {
            public System.Management.Automation.PowerShell PowerShell { get; set; }
            public IAsyncResult AsyncResult { get; set; }
            public PSDataCollection<PSObject> OutputCollection { get; set; }
            public int ChunkNumber { get; set; }
            public int RecordCount { get; set; }
            public int LastOutputIndex { get; set; } = -1;
            public int LastErrorIndex { get; set; } = -1;
            public int LastVerboseIndex { get; set; } = -1;
            public int LastWarningIndex { get; set; } = -1;
            public int LastDebugIndex { get; set; } = -1;
            public int LastInformationIndex { get; set; } = -1;
            public EventHandler<DataAddedEventArgs> OutputHandler { get; set; }
            public EventHandler<DataAddedEventArgs> ErrorHandler { get; set; }
            public EventHandler<DataAddedEventArgs> VerboseHandler { get; set; }
            public EventHandler<DataAddedEventArgs> WarningHandler { get; set; }
            public EventHandler<DataAddedEventArgs> DebugHandler { get; set; }
            public EventHandler<DataAddedEventArgs> InformationHandler { get; set; }
        }

        private List<PSObject> _currentChunk = new List<PSObject>();
        private List<TaskInfo> _activeTasks = new List<TaskInfo>();
        private RunspacePool _runspacePool;
        private int _chunkNumber = 0;
        private volatile bool _stopping = false;
        private CancellationTokenSource _cancellationTokenSource;
        private Task _outputWriterTask;
        private object _tasksLock = new object();
        private ConcurrentQueue<PSObject> _outputObjectQueue = new ConcurrentQueue<PSObject>();
        private ConcurrentQueue<ErrorRecord> _errorQueue = new ConcurrentQueue<ErrorRecord>();
        private ConcurrentQueue<string> _verboseQueue = new ConcurrentQueue<string>();
        private ConcurrentQueue<string> _warningQueue = new ConcurrentQueue<string>();
        private ConcurrentQueue<string> _debugQueue = new ConcurrentQueue<string>();
        private ConcurrentQueue<InformationRecord> _informationQueue = new ConcurrentQueue<InformationRecord>();
        private ConcurrentQueue<ProgressRecord> _progressQueue = new ConcurrentQueue<ProgressRecord>();
        private int _totalCompletedRecords = 0;
        private volatile int _totalInputRecords = 0;

        /// <summary>
        /// Initializes the runspace pool for parallel processing.
        /// </summary>
        protected override void BeginProcessing()
        {
            base.BeginProcessing();


            // Initialize runspace pool for streaming chunk processing
            var initialSessionState = InitialSessionState.CreateDefault();
            initialSessionState.ThreadOptions = PSThreadOptions.UseNewThread;

            // Build list of module patterns to exclude (always include Pester)
            var excludePatterns = new List<string> { "Pester" };
            if (ExcludeModule != null && ExcludeModule.Length > 0)
            {
                excludePatterns.AddRange(ExcludeModule);
            }

            var loadedModules = this.InvokeCommand.InvokeScript("Get-Module | Where-Object { $_.Path } | Select-Object Name, Path");

            foreach (var moduleInfo in loadedModules)
            {
                var moduleName = moduleInfo.Properties["Name"].Value.ToString();
                var modulePath = moduleInfo.Properties["Path"].Value.ToString();

                // Check if module matches any exclude pattern
                bool shouldExclude = false;
                foreach (var pattern in excludePatterns)
                {
                    var wildcardPattern = new WildcardPattern(pattern, WildcardOptions.IgnoreCase);
                    if (wildcardPattern.IsMatch(moduleName))
                    {
                        WriteVerbose($"Excluding module '{moduleName}' from parallel runspaces (matched pattern '{pattern}')");
                        shouldExclude = true;
                        break;
                    }
                }

                if (!shouldExclude)
                {
                    WriteVerbose($"Including module '{moduleName}' in parallel runspaces");
                    initialSessionState.ImportPSModule(new[] { modulePath });
                }
            }

            _runspacePool = RunspaceFactory.CreateRunspacePool(initialSessionState);
            _runspacePool.SetMinRunspaces(1);
            _runspacePool.SetMaxRunspaces(MaxDegreeOfParallelism);
            _runspacePool.Open();

            _cancellationTokenSource = new CancellationTokenSource();
            _outputWriterTask = Task.Run(() => WriteCompletedOutputs());

            WriteVerbose($"Starting parallel processing with MaxDOP={MaxDegreeOfParallelism}, ChunkSize={ChunkSize}");
            WriteProgress(new ProgressRecord(1, "Processing Records", $"Completed {_totalCompletedRecords} of {_totalInputRecords} records") { PercentComplete = 0, RecordType = ProgressRecordType.Processing });
        }

        /// <summary>
        /// Processes input objects from the pipeline, streaming chunks as they are filled.
        /// </summary>
        protected override void ProcessRecord()
        {
            base.ProcessRecord();

            // Check if stopping has been requested
            if (_stopping)
            {
                return;
            }

            _currentChunk.Add(InputObject);
            _totalInputRecords++;

            // When chunk is full, process it immediately
            if (_currentChunk.Count >= ChunkSize)
            {
                // Create a copy of the chunk to avoid reference issues
                // Convert to array to ensure a complete snapshot
                var chunkToProcess = _currentChunk.ToArray();
                ProcessChunk(new List<PSObject>(chunkToProcess));
                _currentChunk.Clear();
            }

            // Drain the output queues to write any pending outputs
            DrainQueues();
        }

        /// <summary>
        /// Processes any remaining partial chunk and waits for all tasks to complete.
        /// </summary>
        protected override void EndProcessing()
        {
            base.EndProcessing();

            // Check if stopping has been requested
            if (_stopping)
            {
                // Clean up runspace pool
                if (_runspacePool != null)
                {
                    _runspacePool.Close();
                    _runspacePool.Dispose();
                }
                return;
            }

            // Process any remaining partial chunk
            if (_currentChunk.Count > 0)
            {
                // Create a copy of the chunk to avoid reference issues
                // Convert to array to ensure a complete snapshot
                var chunkToProcess = _currentChunk.ToArray();
                ProcessChunk(new List<PSObject>(chunkToProcess));
            }


            // Wait for all active tasks to complete
            WaitForAllTasks();

            // Drain any remaining outputs
            DrainQueues();

            WriteProgress(new ProgressRecord(1, "Processing Records", "All records processed") { PercentComplete = 100, RecordType = ProgressRecordType.Completed });

            // Cancel the output writer task
            _cancellationTokenSource.Cancel();
            _outputWriterTask.Wait();

            // Clean up runspace pool
            if (_runspacePool != null)
            {
                _runspacePool.Close();
                _runspacePool.Dispose();
            }
        }

        /// <summary>
        /// Stops processing and cleans up resources.
        /// </summary>
        protected override void StopProcessing()
        {
            _stopping = true;

            WriteVerbose("StopProcessing called - stopping parallel operations");

            // Cancel the output writer task
            _cancellationTokenSource.Cancel();

            // Stop the runspace pool to prevent new operations
            if (_runspacePool != null)
            {
                try
                {
                    _runspacePool.Close();
                }
                catch (Exception ex)
                {
                    WriteVerbose($"Error closing runspace pool: {ex.Message}");
                }
            }

            // Stop all active PowerShell tasks
            lock (_tasksLock)
            {
                foreach (var taskInfo in _activeTasks)
                {
                    try
                    {
                        taskInfo.PowerShell.Stop();
                    }
                    catch (Exception ex)
                    {
                        WriteVerbose($"Error stopping PowerShell task: {ex.Message}");
                    }
                }
                _activeTasks.Clear();
            }

            // Dispose output collections
            lock (_tasksLock)
            {
                foreach (var taskInfo in _activeTasks)
                {
                    try
                    {
                        taskInfo.OutputCollection.Dispose();
                    }
                    catch (Exception ex)
                    {
                        WriteVerbose($"Error disposing output collection: {ex.Message}");
                    }
                }
                _activeTasks.Clear();
            }
            // Clear current chunk to prevent further processing
            _currentChunk.Clear();
        }

        private void ProcessChunk(List<PSObject> chunk)
        {
            // Check if stopping has been requested
            if (_stopping)
            {
                return;
            }

            _chunkNumber++;
            var currentChunkNum = _chunkNumber;
            _verboseQueue.Enqueue($"Queuing chunk {currentChunkNum} with {chunk.Count} items");

            var ps = System.Management.Automation.PowerShell.Create();
            ps.RunspacePool = _runspacePool;

            // Try to clone the connection for this runspace
            ServiceClient connectionToUse;
            try
            {
                connectionToUse = Connection.Clone();
            }
            catch (Exception ex) when (ex is NotImplementedException ||
                                        ex.Message.Contains("On-Premises Connections are not supported") ||
                                        ex.InnerException is NotImplementedException)
            {
                // Mock connections don't support cloning - use the original connection
                // With thread-safe proxy, this is now safe for mock connections
                connectionToUse = Connection;
                _verboseQueue.Enqueue($"Connection cloning not supported");
            }

            ps.AddScript(@"param($Chunk, $connection ); 
            Set-DataverseConnectionAsDefault -Connection $connection
            $psVar = New-Object System.Management.Automation.PSVariable -ArgumentList '_', $Chunk
            $varList = New-Object 'System.Collections.Generic.List[System.Management.Automation.PSVariable]'
            $varList.Add($psVar)
            {" + ScriptBlock + @"}.InvokeWithContext($null, $varList)
");
            // Convert chunk to array to ensure it's fully materialized before passing to PowerShell
            var chunkArray = chunk.ToArray();

            ps.AddParameter("Chunk", chunkArray);
            ps.AddParameter("connection", connectionToUse);

            // Create output collection for streaming results
            var outputCollection = new PSDataCollection<PSObject>();

            var taskInfo = new TaskInfo
            {
                PowerShell = ps,
                OutputCollection = outputCollection,
                ChunkNumber = currentChunkNum,
                RecordCount = chunk.Count
            };

            taskInfo.OutputHandler = (sender, e) => { _outputObjectQueue.Enqueue(outputCollection[e.Index]); taskInfo.LastOutputIndex = e.Index; };
            outputCollection.DataAdded += taskInfo.OutputHandler;

            // Subscribe to stream events to write as they arrive
            taskInfo.ErrorHandler = (sender, e) => { _errorQueue.Enqueue(ps.Streams.Error[e.Index]); taskInfo.LastErrorIndex = e.Index; };
            ps.Streams.Error.DataAdded += taskInfo.ErrorHandler;
            taskInfo.VerboseHandler = (sender, e) => { _verboseQueue.Enqueue(ps.Streams.Verbose[e.Index].Message); taskInfo.LastVerboseIndex = e.Index; };
            ps.Streams.Verbose.DataAdded += taskInfo.VerboseHandler;
            taskInfo.WarningHandler = (sender, e) => { _warningQueue.Enqueue(ps.Streams.Warning[e.Index].Message); taskInfo.LastWarningIndex = e.Index; };
            ps.Streams.Warning.DataAdded += taskInfo.WarningHandler;
            taskInfo.DebugHandler = (sender, e) => { _debugQueue.Enqueue(ps.Streams.Debug[e.Index].Message); taskInfo.LastDebugIndex = e.Index; };
            ps.Streams.Debug.DataAdded += taskInfo.DebugHandler;
            taskInfo.InformationHandler = (sender, e) => { _informationQueue.Enqueue(ps.Streams.Information[e.Index]); taskInfo.LastInformationIndex = e.Index; };
            ps.Streams.Information.DataAdded += taskInfo.InformationHandler;

            var asyncResult = ps.BeginInvoke(new PSDataCollection<PSObject>(), outputCollection);
            taskInfo.AsyncResult = asyncResult;
            
            lock (_tasksLock)
            {
                _activeTasks.Add(taskInfo);
            }
        }

        private void WriteCompletedOutputs()
        {
            while (!_cancellationTokenSource.Token.IsCancellationRequested)
            {
                TaskInfo taskToComplete = null;

                lock (_tasksLock)
                {
                    if (_activeTasks.Count == 0)
                    {
                        Thread.Sleep(50);
                        continue;
                    }

                    // Check each task to see if any are completed
                    for (int i = 0; i < _activeTasks.Count; i++)
                    {
                        if (_activeTasks[i].AsyncResult.IsCompleted)
                        {
                            taskToComplete = _activeTasks[i];
                            _activeTasks.RemoveAt(i);
                            break;
                        }
                    }
                }

                // If we found a completed task, process it outside the lock
                if (taskToComplete != null)
                {
                    try
                    {
                        taskToComplete.PowerShell.EndInvoke(taskToComplete.AsyncResult);
                        int newTotal = Interlocked.Add(ref _totalCompletedRecords, taskToComplete.RecordCount);
                        _verboseQueue.Enqueue($"Chunk {taskToComplete.ChunkNumber} completed - {newTotal} of {_totalInputRecords} records completed");

                        if (_totalInputRecords > 0)
                        {
                            int percent = (int)((_totalCompletedRecords * 100L) / _totalInputRecords);
                            _progressQueue.Enqueue(new ProgressRecord(1, "Processing Records", $"Completed {_totalCompletedRecords} of {_totalInputRecords} records") { PercentComplete = percent, RecordType = ProgressRecordType.Processing });
                        }
                    }
                    catch (Exception ex)
                    {
                        _errorQueue.Enqueue(new ErrorRecord(ex, "TaskError", ErrorCategory.OperationStopped, taskToComplete.PowerShell));
                    }
                    finally
                    {
                        // Process remaining output items
                        for (int i = taskToComplete.LastOutputIndex + 1; i < taskToComplete.OutputCollection.Count; i++)
                        {
                            _outputObjectQueue.Enqueue(taskToComplete.OutputCollection[i]);
                        }

                        // Process remaining stream items
                        for (int i = taskToComplete.LastErrorIndex + 1; i < taskToComplete.PowerShell.Streams.Error.Count; i++)
                        {
                            _errorQueue.Enqueue(taskToComplete.PowerShell.Streams.Error[i]);
                        }
                        for (int i = taskToComplete.LastVerboseIndex + 1; i < taskToComplete.PowerShell.Streams.Verbose.Count; i++)
                        {
                            _verboseQueue.Enqueue(taskToComplete.PowerShell.Streams.Verbose[i].Message);
                        }
                        for (int i = taskToComplete.LastWarningIndex + 1; i < taskToComplete.PowerShell.Streams.Warning.Count; i++)
                        {
                            _warningQueue.Enqueue(taskToComplete.PowerShell.Streams.Warning[i].Message);
                        }
                        for (int i = taskToComplete.LastDebugIndex + 1; i < taskToComplete.PowerShell.Streams.Debug.Count; i++)
                        {
                            _debugQueue.Enqueue(taskToComplete.PowerShell.Streams.Debug[i].Message);
                        }
                        for (int i = taskToComplete.LastInformationIndex + 1; i < taskToComplete.PowerShell.Streams.Information.Count; i++)
                        {
                            _informationQueue.Enqueue(taskToComplete.PowerShell.Streams.Information[i]);
                        }

                        // Unsubscribe events
                        taskToComplete.OutputCollection.DataAdded -= taskToComplete.OutputHandler;
                        taskToComplete.PowerShell.Streams.Error.DataAdded -= taskToComplete.ErrorHandler;
                        taskToComplete.PowerShell.Streams.Verbose.DataAdded -= taskToComplete.VerboseHandler;
                        taskToComplete.PowerShell.Streams.Warning.DataAdded -= taskToComplete.WarningHandler;
                        taskToComplete.PowerShell.Streams.Debug.DataAdded -= taskToComplete.DebugHandler;
                        taskToComplete.PowerShell.Streams.Information.DataAdded -= taskToComplete.InformationHandler;

                        taskToComplete.PowerShell.Dispose();
                        taskToComplete.OutputCollection.Dispose();
                    }
                }
                else
                {
                    // No completed tasks found, wait a bit before checking again
                    Thread.Sleep(50);
                }
            }
        }

        private void WaitForAllTasks()
        {
            lock (_tasksLock)
            {
                _verboseQueue.Enqueue($"Waiting for {_activeTasks.Count} active tasks to complete");
            }

            // If stopping, don't wait for tasks - just dispose them
            if (_stopping)
            {
                lock (_tasksLock)
                {
                    foreach (var taskInfo in _activeTasks)
                    {
                        try
                        {
                            taskInfo.PowerShell.Dispose();
                        }
                        catch (Exception ex)
                        {
                            _verboseQueue.Enqueue($"Error disposing PowerShell task: {ex.Message}");
                        }
                    }
                    foreach (var taskInfo in _activeTasks)
                    {
                        try
                        {
                            taskInfo.OutputCollection.Dispose();
                        }
                        catch (Exception ex)
                        {
                            _verboseQueue.Enqueue($"Error disposing output collection: {ex.Message}");
                        }
                    }
                    _activeTasks.Clear();
                }
                return;
            }

            // Wait for all active tasks to complete
            while (true)
            {
                lock (_tasksLock)
                {
                    if (_activeTasks.Count == 0)
                        break;
                }
                Thread.Sleep(100);
                DrainQueues();
            }
        }


        private void DrainQueues()
        {
            while (_outputObjectQueue.TryDequeue(out var obj)) WriteObject(obj);
            while (_errorQueue.TryDequeue(out var err)) WriteError(err);
            while (_verboseQueue.TryDequeue(out var msg)) WriteVerbose(msg);
            while (_warningQueue.TryDequeue(out var msg)) WriteWarning(msg);
            while (_debugQueue.TryDequeue(out var msg)) WriteDebug(msg);
            while (_informationQueue.TryDequeue(out var info)) WriteVerbose(info.MessageData.ToString());
            while (_progressQueue.TryDequeue(out var pr)) WriteProgress(pr);
        }
    }
}
