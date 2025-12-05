using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Management.Automation;
using System.Net.Http;
using System.Threading.Tasks;
using System.Xml.Linq;
using Microsoft.PowerPlatform.Dataverse.Client;

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
        /// The NuGet package ID of the XrmToolbox plugin to execute (e.g., "Cinteros.Xrm.FetchXMLBuilder").
        /// </summary>
        [Parameter(Mandatory = true, Position = 0, HelpMessage = "The NuGet package ID of the XrmToolbox plugin to execute (e.g., \"Cinteros.Xrm.FetchXMLBuilder\").")]
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

        private static readonly HttpClient _httpClient = new HttpClient();
        private string _defaultCacheDirectory;

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
        }

        /// <summary>
        /// Processes the cmdlet by downloading and launching the XrmToolbox plugin.
        /// </summary>
        protected override void ProcessRecord()
        {
            try
            {
                WriteVerbose($"Looking for XrmToolbox plugin package: {PackageName}");

                // Download and extract the package
                string packagePath = DownloadPackageAsync().GetAwaiter().GetResult();
                
                WriteVerbose($"Package downloaded to: {packagePath}");

                // Extract plugin files
                string pluginDirectory = ExtractPluginFiles(packagePath);
                
                WriteVerbose($"Plugin extracted to: {pluginDirectory}");

                // Launch the plugin host process
                LaunchPluginHost(pluginDirectory);
            }
            catch (Exception ex)
            {
                WriteError(new ErrorRecord(ex, "InvokeXrmToolboxPluginFailed", ErrorCategory.InvalidOperation, PackageName));
            }
        }

        private async Task<string> DownloadPackageAsync()
        {
            string effectiveVersion = Version;

            // If no version specified, get the latest
            if (string.IsNullOrEmpty(effectiveVersion))
            {
                WriteVerbose($"No version specified, finding latest version for {PackageName}");
                effectiveVersion = await GetLatestVersionAsync(PackageName);
                WriteVerbose($"Latest version: {effectiveVersion}");
            }

            // Check if package is already cached
            string packageFileName = $"{PackageName}.{effectiveVersion}.nupkg";
            string packagePath = Path.Combine(CacheDirectory, PackageName, effectiveVersion, packageFileName);

            if (File.Exists(packagePath) && !Force.IsPresent)
            {
                WriteVerbose($"Using cached package: {packagePath}");
                return packagePath;
            }

            // Download the package
            WriteVerbose($"Downloading package: {PackageName} version {effectiveVersion}");
            
            string downloadUrl = $"https://www.nuget.org/api/v2/package/{PackageName}/{effectiveVersion}";
            
            Directory.CreateDirectory(Path.GetDirectoryName(packagePath));

            using (var response = await _httpClient.GetAsync(downloadUrl))
            {
                response.EnsureSuccessStatusCode();
                
                using (var fileStream = new FileStream(packagePath, FileMode.Create, FileAccess.Write, FileShare.None))
                {
                    await response.Content.CopyToAsync(fileStream);
                }
            }

            WriteVerbose($"Package downloaded successfully to: {packagePath}");
            return packagePath;
        }

        private async Task<string> GetLatestVersionAsync(string packageName)
        {
            // Query NuGet API to get latest version
            string apiUrl = $"https://api.nuget.org/v3-flatcontainer/{packageName.ToLowerInvariant()}/index.json";
            
            using (var response = await _httpClient.GetAsync(apiUrl))
            {
                response.EnsureSuccessStatusCode();
                
                string json = await response.Content.ReadAsStringAsync();
                
                // Parse JSON to get versions array
                // Simple JSON parsing - look for "versions" array
                var doc = System.Text.Json.JsonDocument.Parse(json);
                var versions = doc.RootElement.GetProperty("versions");
                
                string latestVersion = null;
                foreach (var version in versions.EnumerateArray())
                {
                    latestVersion = version.GetString();
                }
                
                if (string.IsNullOrEmpty(latestVersion))
                {
                    throw new InvalidOperationException($"Could not determine latest version for package {packageName}");
                }
                
                return latestVersion;
            }
        }

        private string ExtractPluginFiles(string packagePath)
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
                Directory.Delete(extractPath, true);
            }

            WriteVerbose($"Extracting package to: {extractPath}");
            ZipFile.ExtractToDirectory(packagePath, extractPath);

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
    }
}
