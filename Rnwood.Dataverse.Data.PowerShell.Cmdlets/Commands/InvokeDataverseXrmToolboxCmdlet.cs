using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Management.Automation;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.PowerPlatform.Dataverse.Client;
using NuGet.Common;
using NuGet.Packaging.Core;
using NuGet.Protocol;
using NuGet.Protocol.Core.Types;
using NuGet.Versioning;

namespace Rnwood.Dataverse.Data.PowerShell.Commands
{
    /// <summary>
    /// Invokes an XrmToolbox plugin downloaded from NuGet with the current Dataverse connection injected.
    /// </summary>
    [Cmdlet(VerbsLifecycle.Invoke, "DataverseXrmToolbox")]
    [OutputType(typeof(void))]
    public class InvokeDataverseXrmToolboxCmdlet : OrganizationServiceCmdlet
    {
        /// <summary>
        /// The NuGet package ID of the XrmToolbox plugin to execute (e.g., "Cinteros.Xrm.FetchXMLBuilder"). Supports partial matching.
        /// </summary>
        [Parameter(Mandatory = true, Position = 0, HelpMessage = "The NuGet package ID of the XrmToolbox plugin to execute (e.g., \"Cinteros.Xrm.FetchXMLBuilder\"). Supports partial matching.")]
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

        private string _defaultCacheDirectory;
        private SourceCacheContext _cache;
        private SourceRepository _repository;
        private ILogger _logger;

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

                // Search for packages
                var searchResult = SearchPackagesAsync().GetAwaiter().GetResult();
                
                if (searchResult == null)
                {
                    return; // Error already written
                }

                WriteVerbose($"Using package: {searchResult.Identity.Id} v{searchResult.Identity.Version}");

                // Download and extract the package
                string packagePath = DownloadPackageAsync(searchResult).GetAwaiter().GetResult();
                
                WriteVerbose($"Package downloaded to: {packagePath}");

                // Extract plugin files
                string pluginDirectory = ExtractPluginFiles(packagePath, searchResult.Identity);
                
                WriteVerbose($"Plugin extracted to: {pluginDirectory}");

                // Launch the plugin host process
                LaunchPluginHost(pluginDirectory);
            }
            catch (Exception ex)
            {
                WriteError(new ErrorRecord(ex, "InvokeXrmToolboxPluginFailed", ErrorCategory.InvalidOperation, PackageName));
            }
        }

        private async Task<IPackageSearchMetadata> SearchPackagesAsync()
        {
            var searchResource = await _repository.GetResourceAsync<PackageSearchResource>();
            var searchFilter = new SearchFilter(includePrerelease: false);

            WriteProgress(new ProgressRecord(1, "Searching NuGet", $"Searching for: {PackageName}"));

            // Search for packages
            var results = await searchResource.SearchAsync(
                PackageName,
                searchFilter,
                skip: 0,
                take: 20,
                _logger,
                CancellationToken.None);

            var packageList = results.ToList();

            WriteProgress(new ProgressRecord(1, "Searching NuGet", "Search complete") { RecordType = ProgressRecordType.Completed });

            if (packageList.Count == 0)
            {
                WriteError(new ErrorRecord(
                    new InvalidOperationException($"No packages found matching '{PackageName}'"),
                    "PackageNotFound",
                    ErrorCategory.ObjectNotFound,
                    PackageName));
                return null;
            }

            // Check for exact match
            var exactMatch = packageList.FirstOrDefault(p => 
                p.Identity.Id.Equals(PackageName, StringComparison.OrdinalIgnoreCase));

            if (exactMatch != null)
            {
                WriteVerbose($"Found exact match: {exactMatch.Identity.Id}");
                return exactMatch;
            }

            // If only one result, use it
            if (packageList.Count == 1)
            {
                WriteVerbose($"Found single match: {packageList[0].Identity.Id}");
                return packageList[0];
            }

            // Multiple matches - list them
            WriteWarning($"Multiple packages match '{PackageName}'. Please specify the exact package ID:");
            foreach (var pkg in packageList.Take(10))
            {
                WriteWarning($"  - {pkg.Identity.Id}: {pkg.Description}");
            }

            if (packageList.Count > 10)
            {
                WriteWarning($"  ... and {packageList.Count - 10} more. Refine your search.");
            }

            throw new InvalidOperationException($"Multiple packages match '{PackageName}'. Please specify the exact package ID.");
        }

        private async Task<string> DownloadPackageAsync(IPackageSearchMetadata package)
        {
            var packageIdentity = package.Identity;
            string packageFileName = $"{packageIdentity.Id}.{packageIdentity.Version}.nupkg";
            string packagePath = Path.Combine(CacheDirectory, packageIdentity.Id, packageIdentity.Version.ToString(), packageFileName);

            // Check if package is already cached
            if (File.Exists(packagePath) && !Force.IsPresent)
            {
                WriteVerbose($"Using cached package: {packagePath}");
                return packagePath;
            }

            Directory.CreateDirectory(Path.GetDirectoryName(packagePath));

            WriteProgress(new ProgressRecord(2, "Downloading Package", $"Downloading {packageIdentity.Id} v{packageIdentity.Version}"));

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
                            WriteProgress(new ProgressRecord(2, "Downloading Package", 
                                $"Downloaded {totalRead / 1024}KB / {totalBytes / 1024}KB")
                            {
                                PercentComplete = percentComplete
                            });
                        }
                    }
                }
            }

            WriteProgress(new ProgressRecord(2, "Downloading Package", "Download complete") { RecordType = ProgressRecordType.Completed });
            WriteVerbose($"Package downloaded successfully to: {packagePath}");
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
            
            ZipFile.ExtractToDirectory(packagePath, extractPath);

            WriteProgress(new ProgressRecord(3, "Extracting Package", "Extraction complete") { RecordType = ProgressRecordType.Completed });

            return extractPath;
        }

        private void LaunchPluginHost(string pluginDirectory)
        {
            // Find the XrmToolbox plugin DLL in the extracted files
            // XrmToolbox plugins are typically in lib/net48/Plugins/ directory
            string pluginsPath = Path.Combine(pluginDirectory, "lib", "net48", "Plugins");
            
            if (!Directory.Exists(pluginsPath))
            {
                // Try alternate location
                pluginsPath = Path.Combine(pluginDirectory, "lib", "net462", "Plugins");
            }

            if (!Directory.Exists(pluginsPath))
            {
                throw new InvalidOperationException($"Could not find Plugins directory in package. Expected location: {pluginsPath}");
            }

            WriteVerbose($"Found plugins directory: {pluginsPath}");

            // Find the plugin loader DLL (the one that implements IXrmToolBoxPlugin)
            var pluginFiles = Directory.GetFiles(pluginsPath, "*.dll", SearchOption.AllDirectories);
            
            if (pluginFiles.Length == 0)
            {
                throw new InvalidOperationException($"No plugin DLLs found in {pluginsPath}");
            }

            WriteVerbose($"Found {pluginFiles.Length} plugin files");

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
            string connectionString = BuildConnectionString();

            WriteProgress(new ProgressRecord(4, "Launching Plugin", "Starting XrmToolbox plugin host"));
            WriteVerbose($"Launching plugin host...");
            WriteVerbose($"  Plugin directory: {pluginsPath}");
            WriteVerbose($"  Pipe name: {pipeName}");

            // Launch the host process
            var processStartInfo = new System.Diagnostics.ProcessStartInfo
            {
                FileName = hostExePath,
                Arguments = $"\"{pluginsPath}\" \"{pipeName}\" \"{connectionString}\"",
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
                    
                    // Read output
                    string output = process.StandardOutput.ReadToEnd();
                    string error = process.StandardError.ReadToEnd();
                    
                    if (!string.IsNullOrEmpty(output))
                    {
                        WriteVerbose($"Host output: {output}");
                    }
                    
                    if (!string.IsNullOrEmpty(error))
                    {
                        WriteWarning($"Host errors: {error}");
                    }
                    
                    process.WaitForExit();
                    
                    if (process.ExitCode != 0)
                    {
                        WriteWarning($"Plugin host exited with code: {process.ExitCode}");
                    }
                }
                else
                {
                    throw new InvalidOperationException("Failed to start plugin host process");
                }
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

        private string BuildConnectionString()
        {
            // Build a connection string from the current connection
            // This is a simplified version - in production, you'd want to extract
            // the actual connection details from the ServiceClient
            
            if (Connection == null)
            {
                throw new InvalidOperationException("No connection available. Use Get-DataverseConnection first.");
            }

            // For now, return a placeholder
            // In a full implementation, you would extract:
            // - Organization URL
            // - Authentication type
            // - Credentials (if applicable)
            // And build a proper connection string
            
            var url = Connection.ConnectedOrgUriActual?.ToString() ?? "unknown";
            return $"Url={url};";
        }

        /// <summary>
        /// NuGet logger that writes to PowerShell
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
                _cmdlet.WriteVerbose(data);
            }

            public void LogVerbose(string data)
            {
                _cmdlet.WriteVerbose(data);
            }

            public void LogInformation(string data)
            {
                _cmdlet.WriteVerbose(data);
            }

            public void LogMinimal(string data)
            {
                _cmdlet.WriteVerbose(data);
            }

            public void LogWarning(string data)
            {
                _cmdlet.WriteWarning(data);
            }

            public void LogError(string data)
            {
                _cmdlet.WriteWarning($"NuGet Error: {data}");
            }

            public void LogInformationSummary(string data)
            {
                _cmdlet.WriteVerbose(data);
            }

            public void Log(LogLevel level, string data)
            {
                switch (level)
                {
                    case LogLevel.Error:
                        LogError(data);
                        break;
                    case LogLevel.Warning:
                        LogWarning(data);
                        break;
                    case LogLevel.Information:
                        LogInformation(data);
                        break;
                    case LogLevel.Verbose:
                        LogVerbose(data);
                        break;
                    case LogLevel.Debug:
                        LogDebug(data);
                        break;
                    case LogLevel.Minimal:
                        LogMinimal(data);
                        break;
                }
            }

            public Task LogAsync(LogLevel level, string data)
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
