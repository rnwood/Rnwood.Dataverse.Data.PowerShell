using System;
using System.Diagnostics;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Management.Automation;
using System.Net.Http;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using NuGet.Protocol;
using NuGet.Protocol.Core.Types;
using NuGet.Common;
using NuGet.Versioning;

namespace Rnwood.Dataverse.Data.PowerShell.Commands
{
    /// <summary>
    /// Helper class for managing and executing the Power Apps CLI (PAC).
    /// </summary>
    internal static class PacCliHelper
    {
        private static string _cachedPacPath = null;
        private static readonly object _lockObject = new object();
        private const string PAC_PACKAGE_ID = "Microsoft.PowerApps.CLI.Tool";

        /// <summary>
        /// Gets the path to the PAC CLI executable, downloading it if necessary.
        /// </summary>
        /// <param name="cmdlet">The cmdlet requesting the PAC CLI (for verbose/warning output).</param>
        /// <param name="version">The version of PAC CLI to use. If null, uses the latest version.</param>
        /// <returns>The full path to the PAC CLI executable.</returns>
        public static string GetPacCliPath(PSCmdlet cmdlet, string version = null)
        {
            lock (_lockObject)
            {
                // Always use our downloaded version, ignore PATH
                cmdlet.WriteVerbose("Searching for locally cached PAC CLI...");

                // Check if we have a local cached installation for the requested version
                string localPacPath = GetLocalPacPath(version);
                if (localPacPath != null && File.Exists(localPacPath))
                {
                    cmdlet.WriteVerbose($"Found locally cached PAC CLI: {localPacPath}");
                    return localPacPath;
                }

                // Download and install PAC CLI from NuGet
                string versionInfo = version != null ? $" version {version}" : " (latest version)";
                cmdlet.WriteWarning($"PAC CLI{versionInfo} not found. Downloading from NuGet...");
                DownloadAndExtractPacCli(cmdlet, version);

                // Try finding it again
                localPacPath = GetLocalPacPath(version);
                if (localPacPath != null && File.Exists(localPacPath))
                {
                    cmdlet.WriteVerbose($"PAC CLI downloaded successfully: {localPacPath}");
                    return localPacPath;
                }

                throw new InvalidOperationException("Failed to download PAC CLI.");
            }
        }

        /// <summary>
        /// Executes a PAC CLI command and returns the exit code.
        /// </summary>
        /// <param name="cmdlet">The cmdlet executing the command (for output).</param>
        /// <param name="arguments">The arguments to pass to PAC CLI.</param>
        /// <param name="workingDirectory">The working directory for the process.</param>
        /// <param name="version">The version of PAC CLI to use. If null, uses the latest version.</param>
        /// <returns>The exit code from the PAC CLI process.</returns>
        public static int ExecutePacCli(PSCmdlet cmdlet, string arguments, string workingDirectory = null, string version = null)
        {
            string pacPath = GetPacCliPath(cmdlet, version);

            cmdlet.WriteVerbose($"Executing: {pacPath} {arguments}");

            var startInfo = new ProcessStartInfo
            {
                FileName = pacPath,
                Arguments = arguments,
                UseShellExecute = false,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                CreateNoWindow = true,
                WorkingDirectory = workingDirectory ?? Directory.GetCurrentDirectory()
            };

            using (var process = new Process { StartInfo = startInfo })
            {
                process.OutputDataReceived += (sender, e) =>
                {
                    if (!string.IsNullOrEmpty(e.Data))
                    {
                        cmdlet.WriteVerbose($"PAC: {e.Data}");
                    }
                };

                process.ErrorDataReceived += (sender, e) =>
                {
                    if (!string.IsNullOrEmpty(e.Data))
                    {
                        cmdlet.WriteWarning($"PAC: {e.Data}");
                    }
                };

                process.Start();
                process.BeginOutputReadLine();
                process.BeginErrorReadLine();
                process.WaitForExit();

                return process.ExitCode;
            }
        }

        private static string GetLocalPacPath(string version = null)
        {
            // Get the local cache directory for PAC CLI
            string localAppData = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
            string versionSuffix = version != null ? $"-{version}" : "";
            string pacCacheDir = Path.Combine(localAppData, "Rnwood.Dataverse.Data.PowerShell", $"pac-cli{versionSuffix}");
            
            if (!Directory.Exists(pacCacheDir))
            {
                return null;
            }

            // Find the executable in tools folder
            string toolsDir = Path.Combine(pacCacheDir, "tools");
            if (!Directory.Exists(toolsDir))
            {
                return null;
            }

            string pacExecutable = RuntimeInformation.IsOSPlatform(OSPlatform.Windows) ? "pac.exe" : "pac";
            
            // Search for pac executable in tools directory
            string[] pacFiles = Directory.GetFiles(toolsDir, pacExecutable, SearchOption.AllDirectories);
            if (pacFiles.Length > 0)
            {
                return pacFiles[0];
            }

            return null;
        }

        private static void DownloadAndExtractPacCli(PSCmdlet cmdlet, string version = null)
        {
            try
            {
                // Create cache directory
                string localAppData = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
                string versionSuffix = version != null ? $"-{version}" : "";
                string pacCacheDir = Path.Combine(localAppData, "Rnwood.Dataverse.Data.PowerShell", $"pac-cli{versionSuffix}");
                
                if (Directory.Exists(pacCacheDir))
                {
                    cmdlet.WriteVerbose($"Cleaning existing cache directory: {pacCacheDir}");
                    Directory.Delete(pacCacheDir, recursive: true);
                }
                
                Directory.CreateDirectory(pacCacheDir);
                cmdlet.WriteVerbose($"Created cache directory: {pacCacheDir}");

                // Download the NuGet package
                var task = Task.Run(async () => await DownloadPacPackageAsync(cmdlet, pacCacheDir, version));
                task.Wait();
                string nupkgPath = task.Result;

                cmdlet.WriteVerbose($"Downloaded package to: {nupkgPath}");

                // Extract the NuGet package (it's a ZIP file)
                string extractDir = Path.Combine(pacCacheDir, "extracted");
                Directory.CreateDirectory(extractDir);
                ZipFile.ExtractToDirectory(nupkgPath, extractDir);
                cmdlet.WriteVerbose($"Extracted package to: {extractDir}");

                // Move the tools folder to the cache root
                string toolsSource = Path.Combine(extractDir, "tools");
                string toolsDest = Path.Combine(pacCacheDir, "tools");
                
                if (Directory.Exists(toolsSource))
                {
                    Directory.Move(toolsSource, toolsDest);
                    cmdlet.WriteVerbose($"Moved tools to: {toolsDest}");
                }

                // Clean up
                File.Delete(nupkgPath);
                Directory.Delete(extractDir, recursive: true);
                
                // Make executable on Unix systems
                if (!RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
                {
                    string pacPath = GetLocalPacPath();
                    if (pacPath != null)
                    {
                        var chmodInfo = new ProcessStartInfo
                        {
                            FileName = "chmod",
                            Arguments = $"+x \"{pacPath}\"",
                            UseShellExecute = false,
                            CreateNoWindow = true
                        };
                        Process.Start(chmodInfo)?.WaitForExit();
                    }
                }

                cmdlet.WriteVerbose("PAC CLI extracted successfully.");
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"Failed to download and extract PAC CLI: {ex.Message}", ex);
            }
        }

        private static async Task<string> DownloadPacPackageAsync(PSCmdlet cmdlet, string cacheDir, string version = null)
        {
            var logger = NullLogger.Instance;
            var cancellationToken = System.Threading.CancellationToken.None;

            var cache = new SourceCacheContext();
            var repository = Repository.Factory.GetCoreV3("https://api.nuget.org/v3/index.json");
            var resource = await repository.GetResourceAsync<FindPackageByIdResource>();

            NuGetVersion selectedVersion;

            if (version != null)
            {
                // Use the specified version
                if (!NuGetVersion.TryParse(version, out selectedVersion))
                {
                    throw new InvalidOperationException($"Invalid version format: {version}");
                }
                cmdlet.WriteVerbose($"Using specified PAC CLI version: {selectedVersion}");
            }
            else
            {
                // Get all versions and select the latest
                var versions = await resource.GetAllVersionsAsync(
                    PAC_PACKAGE_ID,
                    cache,
                    logger,
                    cancellationToken);

                if (versions == null || !versions.Any())
                {
                    throw new InvalidOperationException($"No versions found for package {PAC_PACKAGE_ID}");
                }

                selectedVersion = versions.OrderByDescending(v => v).First();
                cmdlet.WriteVerbose($"Using latest PAC CLI version: {selectedVersion}");
            }

            // Download the package
            string nupkgPath = Path.Combine(cacheDir, $"{PAC_PACKAGE_ID}.{selectedVersion}.nupkg");
            
            using (var packageStream = File.Create(nupkgPath))
            {
                bool success = await resource.CopyNupkgToStreamAsync(
                    PAC_PACKAGE_ID,
                    selectedVersion,
                    packageStream,
                    cache,
                    logger,
                    cancellationToken);

                if (!success)
                {
                    throw new InvalidOperationException($"Failed to download package {PAC_PACKAGE_ID} version {selectedVersion}");
                }
            }

            return nupkgPath;
        }
    }
}
