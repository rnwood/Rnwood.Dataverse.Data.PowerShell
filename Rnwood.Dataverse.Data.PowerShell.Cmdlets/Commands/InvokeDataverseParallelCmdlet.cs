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

		private List<PSObject> _inputBuffer = new List<PSObject>();

		/// <summary>
		/// Collects input objects from the pipeline.
		/// </summary>
		protected override void ProcessRecord()
		{
			base.ProcessRecord();
			_inputBuffer.Add(InputObject);
		}

		/// <summary>
		/// Processes all collected input objects in parallel chunks.
		/// </summary>
		protected override void EndProcessing()
		{
			base.EndProcessing();

			if (_inputBuffer.Count == 0)
			{
				return;
			}

			// Split input into chunks
			var chunks = ChunkInputObjects(_inputBuffer, ChunkSize);

			WriteVerbose($"Processing {_inputBuffer.Count} objects in {chunks.Count} chunks with MaxDOP={MaxDegreeOfParallelism}");
			for (int i = 0; i < chunks.Count; i++)
			{
				WriteVerbose($"Chunk {i + 1}: {chunks[i].Count} items");
			}

			ProcessChunksWithRunspacePool(chunks);
		}

		private List<List<PSObject>> ChunkInputObjects(List<PSObject> objects, int chunkSize)
		{
			var chunks = new List<List<PSObject>>();
			for (int i = 0; i < objects.Count; i += chunkSize)
			{
				chunks.Add(objects.Skip(i).Take(chunkSize).ToList());
			}
			return chunks;
		}

		private void ProcessChunksWithRunspacePool(List<List<PSObject>> chunks)
		{
			// Use RunspacePool for parallel execution
			var initialSessionState = InitialSessionState.CreateDefault();

			using (var runspacePool = RunspaceFactory.CreateRunspacePool(1, MaxDegreeOfParallelism, initialSessionState, Host))
			{
				runspacePool.Open();

				var tasks = new List<System.Management.Automation.PowerShell>();
				var waitHandles = new List<IAsyncResult>();

				foreach (var chunk in chunks)
				{
					var ps = System.Management.Automation.PowerShell.Create();
					ps.RunspacePool = runspacePool;

					// Try to clone the connection for this runspace
					ServiceClient connectionToUse;
					try
					{
						connectionToUse = Connection.Clone();
						WriteVerbose($"Cloned connection for chunk processing");
					}
					catch (Exception ex) when (ex is NotImplementedException || 
					                            ex.Message.Contains("On-Premises Connections are not supported") ||
					                            ex.InnerException is NotImplementedException)
					{
						// Mock connections don't support cloning - use the original connection
						// This is safe for mock connections since they're not making real network calls
						connectionToUse = Connection;
						WriteVerbose($"Connection cloning not supported - using original connection (mock mode): {ex.Message}");
					}

					// Create script that sets up the default connection and runs the user script
					ps.AddScript(@"
						param($chunk, $scriptBlock, $connection)
						# Set default connection for Dataverse cmdlets in this runspace
						if (-not $global:PSDefaultParameterValues) {
							$global:PSDefaultParameterValues = @{}
						}
						$global:PSDefaultParameterValues['*-Dataverse*:Connection'] = $connection
						
						# Process each item in the chunk
						# Use ForEach-Object to properly set $_ for the script block
						foreach ($item in $chunk) {
							$item | ForEach-Object -Process $scriptBlock
						}
					");
					ps.AddParameter("chunk", chunk);
					ps.AddParameter("scriptBlock", ScriptBlock);
					ps.AddParameter("connection", connectionToUse);

					var asyncResult = ps.BeginInvoke();
					tasks.Add(ps);
					waitHandles.Add(asyncResult);
				}

				// Wait for all tasks to complete
				for (int i = 0; i < tasks.Count; i++)
				{
					var task = tasks[i];
					var waitHandle = waitHandles[i];
					
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

				runspacePool.Close();
			}
		}
	}
}
