using System;
using System.Diagnostics;
using System.IO;
using System.Management.Automation;
using System.Runtime.InteropServices;

namespace Rnwood.Dataverse.Data.PowerShell.Commands
{
    /// <summary>
    /// Helper class for managing and executing the Power Apps CLI (PAC).
    /// </summary>
    internal static class PacCliHelper
    {
        /// <summary>
        /// Gets the path to the PAC CLI executable from PATH.
        /// </summary>
        /// <param name="cmdlet">The cmdlet requesting the PAC CLI (for verbose/warning output).</param>
        /// <returns>The full path to the PAC CLI executable.</returns>
        public static string GetPacCliPath(PSCmdlet cmdlet)
        {
            cmdlet.WriteVerbose("Looking for PAC CLI in system PATH...");
            string pacInPath = FindPacInPath();
            if (pacInPath != null)
            {
                cmdlet.WriteVerbose($"Found PAC CLI in PATH: {pacInPath}");
                return pacInPath;
            }
            
            throw new InvalidOperationException("PAC CLI not found in PATH. Please install Power Apps CLI from https://aka.ms/PowerAppsCLI");
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
    }
}
