using Microsoft.PowerPlatform.Dataverse.Client;
using NuGet.Common;
using NuGet.Packaging.Core;
using NuGet.Protocol;
using NuGet.Protocol.Core.Types;
using NuGet.Versioning;
using Rnwood.Dataverse.Data.PowerShell.Cmdlets.Commands.Model;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Management.Automation;
using System.Threading;
using System.Threading.Tasks;

namespace Rnwood.Dataverse.Data.PowerShell.Commands
{
    /// <summary>
    /// Invokes an XrmToolbox plugin downloaded from NuGet with the current Dataverse connection injected.
    /// Searches for packages with XrmToolBox tag filter.
    /// </summary>
    [Cmdlet(VerbsLifecycle.Invoke, "DataverseXrmToolbox")]
    [OutputType(typeof(void))]
    public class InvokeDataverseXrmToolboxCmdlet : OrganizationServiceCmdlet
    {
        /// <summary>
        /// The NuGet package ID of the XrmToolbox plugin to execute. Searches for packages with XrmToolBox tag filter, first checking for exact match by package ID.
        /// </summary>
        [Parameter(Mandatory = true, Position = 0, HelpMessage = "The NuGet package ID of the XrmToolbox plugin to execute. Searches for packages with XrmToolBox tag filter, first checking for exact match by package ID.")]
        [ValidateNotNullOrEmpty]
        public string PackageName { get; set; }

        /// <summary>
        /// The version of the NuGet package to download. If not specified, the latest version will be used.
        /// </summary>
        [Parameter(Mandatory = false, HelpMessage = "The version of the NuGet package to download. If not specified, the latest version will be used.")]
        public string Version { get; set; }

        /// <summary>
        /// The directory where NuGet packages should be cached. Defaults to %LOCALAPPDATA%\Rnwood.Dataverse.Data.PowerShell\XrmToolboxPlugins.
        /// </summary>
        [Parameter(Mandatory = false, HelpMessage = "The directory where NuGet packages should be cached.")]
        public string CacheDirectory { get; set; }

        /// <summary>
        /// Force re-download of the package even if it's already cached.
        /// </summary>
        [Parameter(Mandatory = false, HelpMessage = "Force re-download of the package even if it's already cached.")]
        public SwitchParameter Force { get; set; }

        /// <summary>
        /// The name of the plugin to load if the assembly contains multiple plugins.
        /// </summary>
        [Parameter(Mandatory = false, HelpMessage = "The name of the plugin to load if the assembly contains multiple plugins.")]
        public string Name { get; set; }

        private const int RuntimeDirectoryCleanupHours = 1;
        
        private string _defaultCacheDirectory;
        private SourceCacheContext _cache;
        private SourceRepository _repository;
        private ILogger _logger;
        private readonly ConcurrentQueue<LogMessage> _pendingMessages = new ConcurrentQueue<LogMessage>();
        private volatile bool _stopTokenServer;

        private struct LogMessage
        {
            public LogLevel Level;
            public string Message;
        }

        private enum LogLevel
        {
            Verbose,
            Warning,
            Error,
            Progress
        }

        /// <summary>
        /// Initializes the cmdlet and sets up the cache directory.
        /// </summary>
        protected override void BeginProcessing()
        {
            base.BeginProcessing();

            // Set default cache directory
            _defaultCacheDirectory = Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
                "Rnwood.Dataverse.Data.PowerShell",
                "XrmToolboxPlugins"
            );

            if (string.IsNullOrEmpty(CacheDirectory))
            {
                CacheDirectory = _defaultCacheDirectory;
            }

            // Ensure cache directory exists
            Directory.CreateDirectory(CacheDirectory);

            // Initialize NuGet components
            _cache = new SourceCacheContext();
            _repository = Repository.Factory.GetCoreV3("https://api.nuget.org/v3/index.json");
            _logger = new NuGetLogger(this);
        }

        /// <summary>
        /// Processes the cmdlet by downloading and launching the XrmToolbox plugin.
        /// </summary>
        protected override void ProcessRecord()
        {
            try
            {
                WriteVerbose($"Searching for XrmToolbox plugin package: {PackageName}");

                // Run async operations synchronously, flushing messages on the main thread
                var searchResult = RunAsyncWithMainThreadFlush(() => SearchPackagesAsync());

                if (searchResult == null)
                {
                    // Error was queued, flush it
                    FlushPendingMessages();
                    return;
                }

                WriteVerbose($"Using package: {searchResult.Identity.Id} v{searchResult.Identity.Version}");

                // Download and extract the package
                string packagePath = RunAsyncWithMainThreadFlush(() => DownloadPackageAsync(searchResult));

                WriteVerbose($"Package downloaded to: {packagePath}");

                // Extract plugin files (synchronous)
                string pluginDirectory = ExtractPluginFiles(packagePath, searchResult.Identity);

                WriteVerbose($"Plugin extracted to: {pluginDirectory}");

                // Launch the plugin host process
                LaunchPluginHost(pluginDirectory);
            }
            catch (Exception ex)
            {
                WriteError(new ErrorRecord(ex, "InvokeXrmToolboxPluginFailed", ErrorCategory.InvalidOperation, PackageName));
            }
            finally
            {
                _stopTokenServer = true;
                FlushPendingMessages();
            }
        }

        /// <summary>
        /// Flushes any queued messages to the PowerShell output streams.
        /// MUST be called from the main cmdlet thread only.
        /// </summary>
        private void FlushPendingMessages()
        {
            while (_pendingMessages.TryDequeue(out LogMessage message))
            {
                switch (message.Level)
                {
                    case LogLevel.Verbose:
                        WriteVerbose(message.Message);
                        break;
                    case LogLevel.Warning:
                        WriteWarning(message.Message);
                        break;
                    case LogLevel.Error:
                        WriteError(new ErrorRecord(
                            new InvalidOperationException(message.Message),
                            "BackgroundTaskError",
                            ErrorCategory.InvalidOperation,
                            null));
                        break;
                    case LogLevel.Progress:
                        // Progress messages are formatted as "id|activity|status|percentComplete"
                        var parts = message.Message.Split('|');
                        if (parts.Length >= 3)
                        {
                            int id = int.Parse(parts[0]);
                            string activity = parts[1];
                            string status = parts[2];
                            var progressRecord = new ProgressRecord(id, activity, status);

                            if (parts.Length > 3 && int.TryParse(parts[3], out int percentComplete))
                            {
                                progressRecord.PercentComplete = percentComplete;
                            }

                            if (status.IndexOf("complete", StringComparison.OrdinalIgnoreCase) >= 0)
                            {
                                progressRecord.RecordType = ProgressRecordType.Completed;
                            }

                            WriteProgress(progressRecord);
                        }
                        break;
                }
            }
        }

        /// <summary>
        /// Queues a message to be written on the main thread later.
        /// Safe to call from any thread.
        /// </summary>
        private void QueueMessage(LogLevel level, string message)
        {
            _pendingMessages.Enqueue(new LogMessage { Level = level, Message = message });
        }

        /// <summary>
        /// Queues a progress message to be written on the main thread.
        /// </summary>
        private void QueueProgress(int id, string activity, string status, int percentComplete = -1)
        {
            string message = percentComplete >= 0
                ? $"{id}|{activity}|{status}|{percentComplete}"
                : $"{id}|{activity}|{status}";
            QueueMessage(LogLevel.Progress, message);
        }

        /// <summary>
        /// Runs an async task while polling and flushing messages on the main thread.
        /// This ensures all Write* calls happen on the pipeline thread.
        /// </summary>
        private T RunAsyncWithMainThreadFlush<T>(Func<Task<T>> taskFunc)
        {
            var task = taskFunc();

            // Poll until complete, flushing messages on this (main) thread
            while (!task.IsCompleted)
            {
                FlushPendingMessages();
                Thread.Sleep(50); // Brief sleep to avoid busy-waiting
            }

            // Final flush
            FlushPendingMessages();

            // Propagate any exception
            return task.GetAwaiter().GetResult();
        }

        private async Task<IPackageSearchMetadata> SearchPackagesAsync()
        {
            var searchResource = await _repository.GetResourceAsync<PackageSearchResource>();
            var searchFilter = new SearchFilter(includePrerelease: false);

            QueueProgress(1, "Searching NuGet", $"Searching with XrmToolBox tag: {PackageName}");

            // Search for packages with XrmToolBox tag
            var results = await searchResource.SearchAsync(
                $"{PackageName} tag:XrmToolBox",
                searchFilter,
                skip: 0,
                take: 20,
                _logger,
                CancellationToken.None);

            var packageList = results.ToList();

            QueueProgress(1, "Searching NuGet", "Search complete");

            if (packageList.Count == 0)
            {
                QueueMessage(LogLevel.Error, $"No packages found matching '{PackageName}' with XrmToolBox tag");
                return null;
            }

            // Check for exact match
            var exactMatch = packageList.FirstOrDefault(p =>
                p.Identity.Id.Equals(PackageName, StringComparison.OrdinalIgnoreCase));

            if (exactMatch != null)
            {
                QueueMessage(LogLevel.Verbose, $"Found exact match: {exactMatch.Identity.Id}");
                return exactMatch;
            }

            // If only one result, use it
            if (packageList.Count == 1)
            {
                QueueMessage(LogLevel.Verbose, $"Found single match with XrmToolBox tag: {packageList[0].Identity.Id}");
                return packageList[0];
            }

            // Multiple matches - list them
            QueueMessage(LogLevel.Warning, $"Multiple packages match '{PackageName}' with XrmToolBox tag. Please specify the exact package ID:");
            foreach (var pkg in packageList.Take(10))
            {
                QueueMessage(LogLevel.Warning, $"  - {pkg.Identity.Id}: {pkg.Description}");
            }

            if (packageList.Count > 10)
            {
                QueueMessage(LogLevel.Warning, $"  ... and {packageList.Count - 10} more. Refine your search.");
            }

            throw new InvalidOperationException($"Multiple packages match '{PackageName}' with XrmToolBox tag. Please specify the exact package ID.");
        }

        private async Task<string> DownloadPackageAsync(IPackageSearchMetadata package)
        {
            var packageIdentity = package.Identity;
            string packageFileName = $"{packageIdentity.Id}.{packageIdentity.Version}.nupkg";
            string packagePath = Path.Combine(CacheDirectory, packageIdentity.Id, packageIdentity.Version.ToString(), packageFileName);

            // Check if package is already cached
            if (File.Exists(packagePath) && !Force.IsPresent)
            {
                QueueMessage(LogLevel.Verbose, $"Using cached package: {packagePath}");
                return packagePath;
            }

            Directory.CreateDirectory(Path.GetDirectoryName(packagePath));

            QueueProgress(2, "Downloading Package", $"Downloading {packageIdentity.Id} v{packageIdentity.Version}");

            // Get download resource
            var downloadResource = await _repository.GetResourceAsync<DownloadResource>();

            // Download the package
            using (var downloadResult = await downloadResource.GetDownloadResourceResultAsync(
                packageIdentity,
                new PackageDownloadContext(_cache),
                Path.GetTempPath(),
                _logger,
                CancellationToken.None))
            {
                if (downloadResult.Status != DownloadResourceResultStatus.Available)
                {
                    throw new InvalidOperationException($"Package download failed: {downloadResult.Status}");
                }

                // Copy to cache with progress
                using (var sourceStream = downloadResult.PackageStream)
                using (var targetStream = new FileStream(packagePath, FileMode.Create, FileAccess.Write, FileShare.None))
                {
                    var buffer = new byte[81920]; // 80KB buffer
                    long totalBytes = sourceStream.Length;
                    long totalRead = 0;
                    int bytesRead;

                    while ((bytesRead = await sourceStream.ReadAsync(buffer, 0, buffer.Length)) > 0)
                    {
                        await targetStream.WriteAsync(buffer, 0, bytesRead);
                        totalRead += bytesRead;

                        if (totalBytes > 0)
                        {
                            int percentComplete = (int)((totalRead * 100) / totalBytes);
                            QueueProgress(2, "Downloading Package",
                                $"Downloaded {totalRead / 1024}KB / {totalBytes / 1024}KB",
                                percentComplete);
                        }
                    }
                }
            }

            QueueProgress(2, "Downloading Package", "Download complete");
            QueueMessage(LogLevel.Verbose, $"Package downloaded successfully to: {packagePath}");
            return packagePath;
        }

        private string ExtractPluginFiles(string packagePath, PackageIdentity identity)
        {
            // Extract the .nupkg (which is a ZIP file) to get the plugin files
            string extractPath = Path.Combine(
                Path.GetDirectoryName(packagePath),
                "extracted"
            );

            if (Directory.Exists(extractPath) && !Force.IsPresent)
            {
                WriteVerbose($"Using cached extraction: {extractPath}");
                return extractPath;
            }

            if (Directory.Exists(extractPath))
            {
                WriteProgress(new ProgressRecord(3, "Extracting Package", "Cleaning previous extraction"));
                Directory.Delete(extractPath, true);
            }

            WriteProgress(new ProgressRecord(3, "Extracting Package", $"Extracting {identity.Id}"));
            WriteVerbose($"Extracting package to: {extractPath}");

            ZipFile.ExtractToDirectory(packagePath, extractPath, entryNameEncoding: new ZipFileNameEncoding());

            WriteProgress(new ProgressRecord(3, "Extracting Package", "Extraction complete") { RecordType = ProgressRecordType.Completed });

            return extractPath;
        }

        private void LaunchPluginHost(string pluginDirectory)
        {
            // Find the XrmToolbox plugin DLL in the extracted files
            // XrmToolbox plugins are typically in lib/net*/Plugins/ directory
            string sourcePluginsPath = null;

            var topLevelPluginsPath = Path.Combine(pluginDirectory, "Plugins");
            if (Directory.Exists(topLevelPluginsPath))
            {
                sourcePluginsPath = topLevelPluginsPath;
            }
            else
            {

                string libPath = Path.Combine(pluginDirectory, "lib");
                if (Directory.Exists(libPath))
                {


                    var netDirs = Directory.GetDirectories(libPath, "net*", SearchOption.TopDirectoryOnly);

                    foreach (var netDir in netDirs)
                    {
                        string potentialPluginsPath = Path.Combine(netDir, "Plugins");
                        if (Directory.Exists(potentialPluginsPath))
                        {
                            sourcePluginsPath = potentialPluginsPath;
                            break;
                        } else
                        {
                            sourcePluginsPath = netDir;
                            break;
                        }
                    }
                }
            }

            if (string.IsNullOrEmpty(sourcePluginsPath))
            {
                throw new InvalidOperationException($"Could not find Plugins directory in package. Searched for lib/net*/Plugins in {pluginDirectory}");
            }

            WriteVerbose($"Found plugins directory: {sourcePluginsPath}");

            // Find the plugin loader DLL (the one that implements IXrmToolBoxPlugin)
            var pluginFiles = Directory.GetFiles(sourcePluginsPath, "*.dll", SearchOption.AllDirectories);

            if (pluginFiles.Length == 0)
            {
                throw new InvalidOperationException($"No plugin DLLs found in {sourcePluginsPath}");
            }

            WriteVerbose($"Found {pluginFiles.Length} plugin files");

            // Create a unique runtime directory for this invocation
            // This ensures each invocation has its own isolated XrmToolBox folder structure
            string runtimeRootPath = CreateRuntimeDirectory(sourcePluginsPath);

            WriteVerbose($"Created runtime directory: {runtimeRootPath}");

            // Find the host executable
            string hostExePath = FindPluginHost();

            if (string.IsNullOrEmpty(hostExePath) || !File.Exists(hostExePath))
            {
                throw new InvalidOperationException(
                    "XrmToolbox plugin host executable not found. " +
                    "The host process is required to run XrmToolbox plugins on .NET Core."
                );
            }

            WriteVerbose($"Using plugin host: {hostExePath}");

            // Create a named pipe name for connection
            string pipeName = $"DataversePowerShell_{Guid.NewGuid():N}";

            // Get connection string from current connection
            string url = GetUrl();

            WriteProgress(new ProgressRecord(4, "Launching Plugin", "Starting XrmToolbox plugin host"));
            WriteVerbose($"Launching plugin host...");
            WriteVerbose($"  Runtime root directory: {runtimeRootPath}");
            WriteVerbose($"  Pipe name: {pipeName}");

            // Start token provider server in background (does not call Write* methods)
            Task.Run(() => StartTokenServer(pipeName));

            // Launch the host process with runtime root directory
            var processStartInfo = new System.Diagnostics.ProcessStartInfo
            {
                FileName = hostExePath,
                Arguments = $"\"{runtimeRootPath}\" \"{pipeName}\" \"{url}\" \"{Name ?? ""}\"",
                UseShellExecute = false,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                CreateNoWindow = false
            };

            using (var process = System.Diagnostics.Process.Start(processStartInfo))
            {
                if (process != null)
                {
                    WriteVerbose("Plugin host process started");
                    WriteProgress(new ProgressRecord(4, "Launching Plugin", "Plugin host started") { RecordType = ProgressRecordType.Completed });

                    // Start background tasks to read output and queue messages (no Write* calls)
                    Task.Run(() => ReadProcessOutput(process));
                    Task.Run(() => ReadProcessError(process));

                    // Poll on main thread, flushing queued messages
                    while (!process.HasExited)
                    {
                        FlushPendingMessages();
                        Thread.Sleep(100);
                    }

                    // Wait a moment for output tasks to finish queueing, then flush
                    Thread.Sleep(200);
                    FlushPendingMessages();

                    if (process.ExitCode != 0)
                    {
                        throw new InvalidOperationException($"Plugin host exited with code: {process.ExitCode}");
                    }
                }
                else
                {
                    throw new InvalidOperationException("Failed to start plugin host process");
                }
            }
        }

        /// <summary>
        /// Reads stdout from the process and queues messages. Does NOT call Write* methods.
        /// </summary>
        private void ReadProcessOutput(System.Diagnostics.Process process)
        {
            try
            {
                string line;
                while ((line = process.StandardOutput.ReadLine()) != null)
                {
                    QueueMessage(LogLevel.Verbose, $"Host: {line}");
                }
            }
            catch (Exception ex)
            {
                QueueMessage(LogLevel.Error, $"Error reading host output: {ex.Message}");
            }
        }

        /// <summary>
        /// Reads stderr from the process and queues messages. Does NOT call Write* methods.
        /// </summary>
        private void ReadProcessError(System.Diagnostics.Process process)
        {
            try
            {
                string line;
                while ((line = process.StandardError.ReadLine()) != null)
                {
                    QueueMessage(LogLevel.Warning, $"Host: {line}");
                }
            }
            catch (Exception ex)
            {
                QueueMessage(LogLevel.Error, $"Error reading host error output: {ex.Message}");
            }
        }

        /// <summary>
        /// Runs a token server on a background thread. Only queues messages, does NOT call Write* methods.
        /// </summary>
        private void StartTokenServer(string pipeName)
        {
            try
            {
                QueueMessage(LogLevel.Verbose, $"Starting token server on pipe: {pipeName}");

                // Create named pipe server that provides tokens on demand
                while (!_stopTokenServer)
                {
                    try
                    {
                        using (var pipeServer = new System.IO.Pipes.NamedPipeServerStream(
                            pipeName,
                            System.IO.Pipes.PipeDirection.Out,
                            System.IO.Pipes.NamedPipeServerStream.MaxAllowedServerInstances,
                            System.IO.Pipes.PipeTransmissionMode.Message,
                            System.IO.Pipes.PipeOptions.Asynchronous))
                        {
                            QueueMessage(LogLevel.Verbose, "Token server: Waiting for connection...");

                            pipeServer.WaitForConnection();

                            QueueMessage(LogLevel.Verbose, "Token server: Client connected, providing token");

                            try
                            {
                                // Get fresh token from connection
                                string token = Connection?.CurrentAccessToken;

                                if (string.IsNullOrEmpty(token) && Connection is ServiceClientWithTokenProvider serviceClientWithTokenProvider)
                                {
                                    token =
                                        serviceClientWithTokenProvider.TokenProviderFunction(Connection.ConnectedOrgUriActual.Host).Result;
                                }

                                if (!string.IsNullOrEmpty(token))
                                {
                                    using (var writer = new System.IO.StreamWriter(pipeServer))
                                    {
                                        writer.AutoFlush = true;
                                        writer.Write(token);
                                    }
                                    QueueMessage(LogLevel.Verbose, $"Token server: Token sent ({token.Length} characters)");
                                }
                                else
                                {
                                    QueueMessage(LogLevel.Warning, "Token server: No token available");
                                }
                            }
                            catch (Exception ex)
                            {
                                QueueMessage(LogLevel.Verbose, $"Token server: Error sending token: {ex.Message}");
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        QueueMessage(LogLevel.Verbose, $"Token server: Connection error: {ex.Message}");
                        break;
                    }
                }

                QueueMessage(LogLevel.Verbose, "Token server stopped");
            }
            catch (Exception ex)
            {
                QueueMessage(LogLevel.Warning, $"Token server failed: {ex.Message}");
            }
        }

        private string CreateRuntimeDirectory(string sourcePluginsPath)
        {
            // Create a unique runtime directory for this invocation
            // This ensures each invocation gets its own isolated XrmToolBox folder structure
            // Plugins expect: root/Plugins, root/Settings, root/Logs, root/Connections
            
            string runtimeBasePath = Path.Combine(CacheDirectory, "runtime");
            Directory.CreateDirectory(runtimeBasePath);
            
            // Use a GUID for uniqueness to avoid conflicts between concurrent invocations
            string runtimeId = Guid.NewGuid().ToString("N");
            string runtimeRootPath = Path.Combine(runtimeBasePath, runtimeId);
            
            WriteVerbose($"Creating runtime directory: {runtimeRootPath}");
            
            // Create the XrmToolBox expected folder structure
            string pluginsFolder = Path.Combine(runtimeRootPath, "Plugins");
            string settingsFolder = Path.Combine(runtimeRootPath, "Settings");
            string logsFolder = Path.Combine(runtimeRootPath, "Logs");
            string connectionsFolder = Path.Combine(runtimeRootPath, "Connections");
            
            Directory.CreateDirectory(pluginsFolder);
            Directory.CreateDirectory(settingsFolder);
            Directory.CreateDirectory(logsFolder);
            Directory.CreateDirectory(connectionsFolder);
            
            WriteVerbose($"Copying plugin files from {sourcePluginsPath} to {pluginsFolder}");
            
            // Copy all files and subdirectories from the source plugins path to the runtime Plugins folder
            CopyDirectory(sourcePluginsPath, pluginsFolder);
            
            // Clean up old runtime directories (older than 1 hour)
            CleanupOldRuntimeDirectories(runtimeBasePath);
            
            return runtimeRootPath;
        }

        private void CopyDirectory(string sourceDir, string targetDir)
        {
            if (string.IsNullOrEmpty(sourceDir))
            {
                throw new ArgumentNullException(nameof(sourceDir));
            }
            
            if (string.IsNullOrEmpty(targetDir))
            {
                throw new ArgumentNullException(nameof(targetDir));
            }
            
            if (!Directory.Exists(sourceDir))
            {
                throw new DirectoryNotFoundException($"Source directory not found: {sourceDir}");
            }
            
            // Copy all files
            foreach (string file in Directory.GetFiles(sourceDir))
            {
                string fileName = Path.GetFileName(file);
                string targetFile = Path.Combine(targetDir, fileName);
                File.Copy(file, targetFile, true);
            }
            
            // Copy all subdirectories recursively
            foreach (string subDir in Directory.GetDirectories(sourceDir))
            {
                string dirName = Path.GetFileName(subDir);
                string targetSubDir = Path.Combine(targetDir, dirName);
                Directory.CreateDirectory(targetSubDir);
                CopyDirectory(subDir, targetSubDir);
            }
        }

        private void CleanupOldRuntimeDirectories(string runtimeBasePath)
        {
            try
            {
                if (!Directory.Exists(runtimeBasePath))
                {
                    return;
                }
                
                var cutoffTime = DateTime.UtcNow.AddHours(-RuntimeDirectoryCleanupHours);
                
                foreach (string dir in Directory.GetDirectories(runtimeBasePath))
                {
                    try
                    {
                        var dirInfo = new DirectoryInfo(dir);
                        if (dirInfo.CreationTimeUtc < cutoffTime)
                        {
                            WriteVerbose($"Cleaning up old runtime directory: {dir}");
                            Directory.Delete(dir, true);
                        }
                    }
                    catch
                    {
                        // Ignore errors when cleaning up - directory might be in use
                    }
                }
            }
            catch
            {
                // Ignore errors in cleanup - not critical
            }
        }

        private string FindPluginHost()
        {
            // Look for the host executable in the module directory
            string moduleDirectory = Path.GetDirectoryName(this.GetType().Assembly.Location);

            // Check in the module's bin directory
            string hostExe = Path.Combine(moduleDirectory, "XrmToolboxPluginHost", "Rnwood.Dataverse.Data.PowerShell.XrmToolboxPluginHost.exe");

            if (File.Exists(hostExe))
            {
                return hostExe;
            }

            // Check relative to cmdlets assembly
            hostExe = Path.Combine(moduleDirectory, "..", "..", "XrmToolboxPluginHost", "Rnwood.Dataverse.Data.PowerShell.XrmToolboxPluginHost.exe");

            if (File.Exists(hostExe))
            {
                return Path.GetFullPath(hostExe);
            }

            return null;
        }

        private string GetUrl()
        {
            // Get the URL from the current connection

            if (Connection == null)
            {
                throw new InvalidOperationException("No connection available. Use Get-DataverseConnection first.");
            }

            // Extract URL - token will be provided through named pipe on demand
            var url = Connection.ConnectedOrgUriActual?.ToString() ?? "unknown";

            return url;
        }

        /// <summary>
        /// NuGet logger that queues messages for the main thread.
        /// </summary>
        private class NuGetLogger : ILogger
        {
            private readonly InvokeDataverseXrmToolboxCmdlet _cmdlet;

            public NuGetLogger(InvokeDataverseXrmToolboxCmdlet cmdlet)
            {
                _cmdlet = cmdlet;
            }

            public void LogDebug(string data)
            {
                _cmdlet.QueueMessage(LogLevel.Verbose, data);
            }

            public void LogVerbose(string data)
            {
                _cmdlet.QueueMessage(LogLevel.Verbose, data);
            }

            public void LogInformation(string data)
            {
                _cmdlet.QueueMessage(LogLevel.Verbose, data);
            }

            public void LogMinimal(string data)
            {
                _cmdlet.QueueMessage(LogLevel.Verbose, data);
            }

            public void LogWarning(string data)
            {
                _cmdlet.QueueMessage(LogLevel.Warning, data);
            }

            public void LogError(string data)
            {
                _cmdlet.QueueMessage(LogLevel.Warning, $"NuGet Error: {data}");
            }

            public void LogInformationSummary(string data)
            {
                _cmdlet.QueueMessage(LogLevel.Verbose, data);
            }

            public void Log(NuGet.Common.LogLevel level, string data)
            {
                switch (level)
                {
                    case NuGet.Common.LogLevel.Error:
                        LogError(data);
                        break;
                    case NuGet.Common.LogLevel.Warning:
                        LogWarning(data);
                        break;
                    case NuGet.Common.LogLevel.Information:
                        LogInformation(data);
                        break;
                    case NuGet.Common.LogLevel.Verbose:
                        LogVerbose(data);
                        break;
                    case NuGet.Common.LogLevel.Debug:
                        LogDebug(data);
                        break;
                    case NuGet.Common.LogLevel.Minimal:
                        LogMinimal(data);
                        break;
                }
            }

            public Task LogAsync(NuGet.Common.LogLevel level, string data)
            {
                Log(level, data);
                return Task.CompletedTask;
            }

            public void Log(ILogMessage message)
            {
                Log(message.Level, message.Message);
            }

            public Task LogAsync(ILogMessage message)
            {
                Log(message);
                return Task.CompletedTask;
            }
        }

        protected override void EndProcessing()
        {
            base.EndProcessing();
            _cache?.Dispose();
        }
    }
}
