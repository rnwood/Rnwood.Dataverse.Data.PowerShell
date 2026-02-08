using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Text;

namespace Rnwood.Dataverse.Data.PowerShell.Tests.Infrastructure
{
    /// <summary>
    /// Helper class for running PowerShell scripts in a child process with the full module loaded.
    /// This is used for tests that require the complete PowerShell module environment,
    /// similar to how Pester tests spawned child processes.
    /// </summary>
    public static class PowerShellProcessRunner
    {
        /// <summary>
        /// Runs a PowerShell script in a child process with the module loaded.
        /// Uses the TESTMODULEPATH environment variable to find the module.
        /// </summary>
        /// <param name="script">The PowerShell script to execute.</param>
        /// <param name="usePowerShellCore">
        /// If true, uses pwsh.exe (PowerShell Core). If false, uses powershell.exe (Windows PowerShell).
        /// Default is true for cross-platform compatibility.
        /// </param>
        /// <param name="timeoutSeconds">Maximum time to wait for the script to complete.</param>
        /// <returns>A result containing stdout, stderr, and exit code.</returns>
        public static PowerShellProcessResult Run(string script, bool usePowerShellCore = true, int timeoutSeconds = 60, bool importModule = true)
        {
            var modulePath = GetModulePath();
            
            // Find the .psd1 manifest file in the module path
            var manifestPath = Path.Combine(modulePath, "Rnwood.Dataverse.Data.PowerShell.psd1");
            if (!File.Exists(manifestPath))
            {
                // Try to find any .psd1 file
                var psd1Files = Directory.GetFiles(modulePath, "*.psd1");
                if (psd1Files.Length == 0)
                {
                    throw new FileNotFoundException($"No .psd1 module manifest found in {modulePath}");
                }
                manifestPath = psd1Files[0];
            }
            
            var manifestLiteral = manifestPath.Replace("'", "''");

            // Build the full script that optionally imports the module before running the user script
            var prelude = "\n$ErrorActionPreference = 'Stop'\n$VerbosePreference = 'SilentlyContinue'\n";
            var moduleVariables = $"$moduleManifest = '{manifestLiteral}'\n$modulePath = Split-Path -Parent $moduleManifest\n";

            var importSection = importModule
                ? $"    Import-Module '{manifestLiteral}' -Force -ErrorAction Stop\n"
                : string.Empty;

            var fullScript = $@"{prelude}{moduleVariables}try {{
{importSection}    {script}
}} catch {{
    Write-Error $_.Exception.Message
    Write-Error $_.ScriptStackTrace
    exit 1
}}
";

            // Use a temp file to avoid command-line escaping issues with complex scripts
            var tempScriptPath = Path.Combine(Path.GetTempPath(), $"ps_test_{Guid.NewGuid():N}.ps1");
            try
            {
                File.WriteAllText(tempScriptPath, fullScript, Encoding.UTF8);
                
                var executable = usePowerShellCore ? "pwsh" : "powershell";
                var arguments = $"-NoProfile -NonInteractive -ExecutionPolicy Bypass -File \"{tempScriptPath}\"";

                var startInfo = new ProcessStartInfo
                {
                    FileName = executable,
                    Arguments = arguments,
                    UseShellExecute = false,
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    CreateNoWindow = true,
                    StandardOutputEncoding = Encoding.UTF8,
                    StandardErrorEncoding = Encoding.UTF8
                };

                var stdout = new StringBuilder();
                var stderr = new StringBuilder();

                using var process = new Process { StartInfo = startInfo };
                
                process.OutputDataReceived += (sender, e) =>
                {
                    if (e.Data != null)
                        stdout.AppendLine(e.Data);
                };
                
                process.ErrorDataReceived += (sender, e) =>
                {
                    if (e.Data != null)
                        stderr.AppendLine(e.Data);
                };

                process.Start();
                process.BeginOutputReadLine();
                process.BeginErrorReadLine();

                if (!process.WaitForExit(timeoutSeconds * 1000))
                {
                    process.Kill();
                    throw new TimeoutException($"PowerShell script timed out after {timeoutSeconds} seconds");
                }

                return new PowerShellProcessResult
                {
                    ExitCode = process.ExitCode,
                    StandardOutput = stdout.ToString().TrimEnd(),
                    StandardError = stderr.ToString().TrimEnd(),
                    Success = process.ExitCode == 0
                };
            }
            finally
            {
                // Clean up temp script file
                try { File.Delete(tempScriptPath); } catch { /* Ignore cleanup errors */ }
            }
        }

        /// <summary>
        /// Gets the module path from environment variable or default location.
        /// </summary>
        public static string GetModulePath()
        {
            var modulePath = Environment.GetEnvironmentVariable("TESTMODULEPATH");
            
            if (string.IsNullOrEmpty(modulePath))
            {
                // Try to find it relative to the test assembly
                var assemblyLocation = typeof(PowerShellProcessRunner).Assembly.Location;
                var assemblyDir = Path.GetDirectoryName(assemblyLocation)!;
                
                // Navigate up to solution root and find module
                var solutionRoot = Path.GetFullPath(Path.Combine(assemblyDir, "..", "..", "..", ".."));
                var defaultPath = Path.Combine(solutionRoot, "Rnwood.Dataverse.Data.PowerShell", "bin", "Debug", "netstandard2.0");
                
                if (Directory.Exists(defaultPath))
                {
                    modulePath = defaultPath;
                }
                else
                {
                    throw new InvalidOperationException(
                        "TESTMODULEPATH environment variable is not set and module not found at default location. " +
                        "Set TESTMODULEPATH to the built module directory (e.g., Rnwood.Dataverse.Data.PowerShell/bin/Debug/netstandard2.0)");
                }
            }

            if (!Directory.Exists(modulePath))
            {
                throw new DirectoryNotFoundException($"Module path does not exist: {modulePath}");
            }

            return modulePath;
        }
    }

    /// <summary>
    /// Result of running a PowerShell script in a child process.
    /// </summary>
    public class PowerShellProcessResult
    {
        /// <summary>
        /// The exit code of the PowerShell process.
        /// </summary>
        public int ExitCode { get; set; }

        /// <summary>
        /// The standard output from the script.
        /// </summary>
        public string StandardOutput { get; set; } = "";

        /// <summary>
        /// The standard error from the script.
        /// </summary>
        public string StandardError { get; set; } = "";

        /// <summary>
        /// True if the script completed successfully (exit code 0).
        /// </summary>
        public bool Success { get; set; }

        /// <summary>
        /// Gets the combined output (stdout + stderr if there were errors).
        /// </summary>
        public string GetFullOutput()
        {
            if (string.IsNullOrEmpty(StandardError))
                return StandardOutput;
            
            return $"{StandardOutput}\n\nErrors:\n{StandardError}";
        }

        /// <summary>
        /// Throws an exception if the script failed.
        /// </summary>
        public void ThrowIfFailed()
        {
            if (!Success)
            {
                throw new Exception($"PowerShell script failed with exit code {ExitCode}:\n{GetFullOutput()}");
            }
        }
    }
}
