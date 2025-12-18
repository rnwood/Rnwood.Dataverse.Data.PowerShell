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
        /// <returns>The full path to the PAC CLI executable.</returns>
        public static string GetPacCliPath(PSCmdlet cmdlet)
        {
            lock (_lockObject)
            {
                if (_cachedPacPath != null)
                {
                    return _cachedPacPath;
                }

                cmdlet.WriteVerbose("Searching for PAC CLI...");

                // First, check if pac is in PATH
                string pacInPath = FindPacInPath();
                if (pacInPath != null)
                {
                    cmdlet.WriteVerbose($"Found PAC CLI in PATH: {pacInPath}");
                    _cachedPacPath = pacInPath;
                    return _cachedPacPath;
                }

                cmdlet.WriteVerbose("PAC CLI not found in PATH.");

                // Check if we have a local cached installation
                string localPacPath = GetLocalPacPath();
                if (localPacPath != null && File.Exists(localPacPath))
                {
                    cmdlet.WriteVerbose($"Found locally cached PAC CLI: {localPacPath}");
                    _cachedPacPath = localPacPath;
                    return _cachedPacPath;
                }

                // Download and install PAC CLI from NuGet
                cmdlet.WriteWarning("PAC CLI not found. Downloading from NuGet...");
                DownloadAndExtractPacCli(cmdlet);

                // Try finding it again
                localPacPath = GetLocalPacPath();
                if (localPacPath != null && File.Exists(localPacPath))
                {
                    cmdlet.WriteVerbose($"PAC CLI downloaded successfully: {localPacPath}");
                    _cachedPacPath = localPacPath;
                    return _cachedPacPath;
                }

                throw new InvalidOperationException("Failed to find or download PAC CLI.");
            }
        }

        /// <summary>
        /// Executes a PAC CLI command and returns the exit code.
        /// </summary>
        /// <param name="cmdlet">The cmdlet executing the command (for output).</param>
        /// <param name="arguments">The arguments to pass to PAC CLI.</param>
        /// <param name="workingDirectory">The working directory for the process.</param>
        /// <returns>The exit code from the PAC CLI process.</returns>
        public static int ExecutePacCli(PSCmdlet cmdlet, string arguments, string workingDirectory = null)
        {
            string pacPath = GetPacCliPath(cmdlet);

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

        private static string FindPacInPath()
        {
            string pathEnv = Environment.GetEnvironmentVariable("PATH");
            if (string.IsNullOrEmpty(pathEnv))
            {
                return null;
            }

            string[] pathDirs = pathEnv.Split(Path.PathSeparator);
            string pacExecutable = RuntimeInformation.IsOSPlatform(OSPlatform.Windows) ? "pac.exe" : "pac";

            foreach (string dir in pathDirs)
            {
                try
                {
                    string pacPath = Path.Combine(dir, pacExecutable);
                    if (File.Exists(pacPath))
                    {
                        return pacPath;
                    }
                }
                catch
                {
                    // Ignore invalid paths
                }
            }

            return null;
        }

        private static string GetLocalPacPath()
        {
            // Get the local cache directory for PAC CLI
            string localAppData = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
            string pacCacheDir = Path.Combine(localAppData, "Rnwood.Dataverse.Data.PowerShell", "pac-cli");
            
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

        private static void DownloadAndExtractPacCli(PSCmdlet cmdlet)
        {
            try
            {
                // Create cache directory
                string localAppData = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
                string pacCacheDir = Path.Combine(localAppData, "Rnwood.Dataverse.Data.PowerShell", "pac-cli");
                
                if (Directory.Exists(pacCacheDir))
                {
                    cmdlet.WriteVerbose($"Cleaning existing cache directory: {pacCacheDir}");
                    Directory.Delete(pacCacheDir, recursive: true);
                }
                
                Directory.CreateDirectory(pacCacheDir);
                cmdlet.WriteVerbose($"Created cache directory: {pacCacheDir}");

                // Download the NuGet package
                var task = Task.Run(async () => await DownloadPacPackageAsync(cmdlet, pacCacheDir));
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

        private static async Task<string> DownloadPacPackageAsync(PSCmdlet cmdlet, string cacheDir)
        {
            var logger = NullLogger.Instance;
            var cancellationToken = System.Threading.CancellationToken.None;

            var cache = new SourceCacheContext();
            var repository = Repository.Factory.GetCoreV3("https://api.nuget.org/v3/index.json");
            var resource = await repository.GetResourceAsync<FindPackageByIdResource>();

            // Get all versions
            var versions = await resource.GetAllVersionsAsync(
                PAC_PACKAGE_ID,
                cache,
                logger,
                cancellationToken);

            if (versions == null || !versions.Any())
            {
                throw new InvalidOperationException($"No versions found for package {PAC_PACKAGE_ID}");
            }

            // Get the latest version
            var latestVersion = versions.OrderByDescending(v => v).First();
            cmdlet.WriteVerbose($"Latest PAC CLI version: {latestVersion}");

            // Download the package
            string nupkgPath = Path.Combine(cacheDir, $"{PAC_PACKAGE_ID}.{latestVersion}.nupkg");
            
            using (var packageStream = File.Create(nupkgPath))
            {
                bool success = await resource.CopyNupkgToStreamAsync(
                    PAC_PACKAGE_ID,
                    latestVersion,
                    packageStream,
                    cache,
                    logger,
                    cancellationToken);

                if (!success)
                {
                    throw new InvalidOperationException($"Failed to download package {PAC_PACKAGE_ID}");
                }
            }

            return nupkgPath;
        }
    }
}
