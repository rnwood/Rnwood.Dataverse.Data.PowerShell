using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Management.Automation;
using System.Net.Http;
using System.Runtime.InteropServices;
using System.Threading.Tasks;

namespace Rnwood.Dataverse.Data.PowerShell.Commands
{
    /// <summary>
    /// Helper class for managing and executing the Power Apps CLI (PAC).
    /// </summary>
    internal static class PacCliHelper
    {
        private static string _cachedPacPath = null;
        private static readonly object _lockObject = new object();

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

                // Check if dotnet tool is installed
                string pacFromDotnetTool = FindPacFromDotnetTool(cmdlet);
                if (pacFromDotnetTool != null)
                {
                    cmdlet.WriteVerbose($"Found PAC CLI as .NET tool: {pacFromDotnetTool}");
                    _cachedPacPath = pacFromDotnetTool;
                    return _cachedPacPath;
                }

                // If not found, install it as a .NET local tool
                cmdlet.WriteWarning("PAC CLI not found. Installing Microsoft.PowerApps.CLI.Tool as a .NET tool...");
                InstallPacCli(cmdlet);

                // Try finding it again
                pacFromDotnetTool = FindPacFromDotnetTool(cmdlet);
                if (pacFromDotnetTool != null)
                {
                    cmdlet.WriteVerbose($"PAC CLI installed successfully: {pacFromDotnetTool}");
                    _cachedPacPath = pacFromDotnetTool;
                    return _cachedPacPath;
                }

                throw new InvalidOperationException("Failed to find or install PAC CLI.");
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

        private static string FindPacFromDotnetTool(PSCmdlet cmdlet)
        {
            try
            {
                // Check if pac is available as a dotnet tool
                var startInfo = new ProcessStartInfo
                {
                    FileName = "dotnet",
                    Arguments = "tool list",
                    UseShellExecute = false,
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    CreateNoWindow = true
                };

                using (var process = Process.Start(startInfo))
                {
                    string output = process.StandardOutput.ReadToEnd();
                    process.WaitForExit();

                    if (output.Contains("microsoft.powerapps.cli.tool"))
                    {
                        // Tool is installed, try to find its location
                        string userProfile = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);
                        string dotnetToolsPath = Path.Combine(userProfile, ".dotnet", "tools");
                        string pacExecutable = RuntimeInformation.IsOSPlatform(OSPlatform.Windows) ? "pac.exe" : "pac";
                        string pacPath = Path.Combine(dotnetToolsPath, pacExecutable);

                        if (File.Exists(pacPath))
                        {
                            return pacPath;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                cmdlet.WriteVerbose($"Error checking for dotnet tool: {ex.Message}");
            }

            return null;
        }

        private static void InstallPacCli(PSCmdlet cmdlet)
        {
            cmdlet.WriteVerbose("Installing Microsoft.PowerApps.CLI.Tool...");

            var startInfo = new ProcessStartInfo
            {
                FileName = "dotnet",
                Arguments = "tool install --global Microsoft.PowerApps.CLI.Tool",
                UseShellExecute = false,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                CreateNoWindow = true
            };

            using (var process = new Process { StartInfo = startInfo })
            {
                process.OutputDataReceived += (sender, e) =>
                {
                    if (!string.IsNullOrEmpty(e.Data))
                    {
                        cmdlet.WriteVerbose($"dotnet tool: {e.Data}");
                    }
                };

                process.ErrorDataReceived += (sender, e) =>
                {
                    if (!string.IsNullOrEmpty(e.Data))
                    {
                        cmdlet.WriteWarning($"dotnet tool: {e.Data}");
                    }
                };

                process.Start();
                process.BeginOutputReadLine();
                process.BeginErrorReadLine();
                process.WaitForExit();

                if (process.ExitCode != 0)
                {
                    throw new InvalidOperationException($"Failed to install PAC CLI. Exit code: {process.ExitCode}");
                }
            }

            cmdlet.WriteVerbose("PAC CLI installed successfully.");
        }
    }
}
