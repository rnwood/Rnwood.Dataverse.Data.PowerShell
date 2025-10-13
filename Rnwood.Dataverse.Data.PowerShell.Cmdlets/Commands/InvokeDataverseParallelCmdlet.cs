using Microsoft.PowerPlatform.Dataverse.Client;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Management.Automation;
using System.Management.Automation.Runspaces;
using System.Threading;

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

		private List<PSObject> _currentChunk = new List<PSObject>();
		private List<System.Management.Automation.PowerShell> _activeTasks = new List<System.Management.Automation.PowerShell>();
		private List<IAsyncResult> _activeWaitHandles = new List<IAsyncResult>();
		private RunspacePool _runspacePool;
		private int _chunkNumber = 0;

		/// <summary>
		/// Initializes the runspace pool for parallel processing.
		/// </summary>
		protected override void BeginProcessing()
		{
			base.BeginProcessing();

			// Initialize runspace pool for streaming chunk processing
			var initialSessionState = InitialSessionState.CreateDefault();
			_runspacePool = RunspaceFactory.CreateRunspacePool(1, MaxDegreeOfParallelism, initialSessionState, Host);
			_runspacePool.Open();

			WriteVerbose($"Starting parallel processing with MaxDOP={MaxDegreeOfParallelism}, ChunkSize={ChunkSize}");
		}

		/// <summary>
		/// Processes input objects from the pipeline, streaming chunks as they are filled.
		/// </summary>
		protected override void ProcessRecord()
		{
			base.ProcessRecord();
			
			_currentChunk.Add(InputObject);

			// When chunk is full, process it immediately
			if (_currentChunk.Count >= ChunkSize)
			{
				// Create a copy of the chunk to avoid reference issues
				// Convert to array to ensure a complete snapshot
				var chunkToProcess = _currentChunk.ToArray();
				ProcessChunk(new List<PSObject>(chunkToProcess));
				_currentChunk.Clear();
			}
		}

		/// <summary>
		/// Processes any remaining partial chunk and waits for all tasks to complete.
		/// </summary>
		protected override void EndProcessing()
		{
			base.EndProcessing();

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

			// Clean up runspace pool
			if (_runspacePool != null)
			{
				_runspacePool.Close();
				_runspacePool.Dispose();
			}
		}

		private void ProcessChunk(List<PSObject> chunk)
		{
			_chunkNumber++;
			WriteVerbose($"Processing chunk {_chunkNumber} with {chunk.Count} items");

			var ps = System.Management.Automation.PowerShell.Create();
			ps.RunspacePool = _runspacePool;

			// Try to clone the connection for this runspace
			ServiceClient connectionToUse;
			try
			{
				connectionToUse = Connection.Clone();
				WriteVerbose($"Cloned connection for chunk {_chunkNumber}");
			}
			catch (Exception ex) when (ex is NotImplementedException || 
			                            ex.Message.Contains("On-Premises Connections are not supported") ||
			                            ex.InnerException is NotImplementedException)
			{
				// Mock connections don't support cloning - use the original connection
				// With thread-safe proxy, this is now safe for mock connections
				connectionToUse = Connection;
				WriteVerbose($"Connection cloning not supported - using thread-safe proxy (mock mode)");
			}

			// Create script that sets up the default connection and runs the user script
			ps.AddScript(@"
				param($chunk, $scriptBlock, $connection)
				# Set default connection for Dataverse cmdlets in this runspace
				if (-not $global:PSDefaultParameterValues) {
					$global:PSDefaultParameterValues = @{}
				}
				$global:PSDefaultParameterValues['*-Dataverse*:Connection'] = $connection
				
				# Process each item in the chunk using ForEach-Object
				$chunk | ForEach-Object -Process $scriptBlock
			");
			ps.AddParameter("chunk", chunk);
			ps.AddParameter("scriptBlock", ScriptBlock);
			ps.AddParameter("connection", connectionToUse);

			var asyncResult = ps.BeginInvoke();
			_activeTasks.Add(ps);
			_activeWaitHandles.Add(asyncResult);
		}

		private void WaitForAllTasks()
		{
			WriteVerbose($"Waiting for {_activeTasks.Count} active tasks to complete");

			// Wait for all tasks to complete and collect results
			for (int i = 0; i < _activeTasks.Count; i++)
			{
				var task = _activeTasks[i];
				var waitHandle = _activeWaitHandles[i];
				
				var results = task.EndInvoke(waitHandle);
				foreach (var result in results)
				{
					WriteObject(result);
				}

				if (task.HadErrors)
				{
					foreach (var error in task.Streams.Error)
					{
						WriteError(error);
					}
				}

				task.Dispose();
			}

			_activeTasks.Clear();
			_activeWaitHandles.Clear();
		}
	}
}
