using System;
using System.Diagnostics;
using System.IO;

namespace Rnwood.Dataverse.Data.PowerShell.XrmToolboxPlugin
{
    /// <summary>
    /// Utility class for detecting and managing PowerShell installations.
    /// </summary>
    public static class PowerShellDetector
    {
        /// <summary>
        /// Checks if Windows PowerShell (powershell.exe) is available in PATH.
        /// </summary>
        public static bool IsDesktopAvailable()
        {
            return IsExecutableInPath("powershell.exe");
        }

        /// <summary>
        /// Checks if PowerShell Core (pwsh.exe) is available in PATH.
        /// </summary>
        public static bool IsCoreAvailable()
        {
            return IsExecutableInPath("pwsh.exe");
        }

        /// <summary>
        /// Gets the executable name for the specified PowerShell version.
        /// </summary>
        public static string GetExecutableName(PowerShellVersion version)
        {
            return version == PowerShellVersion.Desktop ? "powershell.exe" : "pwsh.exe";
        }

        /// <summary>
        /// Gets the display name for the specified PowerShell version.
        /// </summary>
        public static string GetDisplayName(PowerShellVersion version)
        {
            return version == PowerShellVersion.Desktop ? "PowerShell 5.1" : "PowerShell 7+";
        }

        /// <summary>
        /// Gets installation instructions for PowerShell Core.
        /// </summary>
        public static string GetInstallInstructions()
        {
            return @"PowerShell 7+ (pwsh) is not installed or not found in PATH.

To install PowerShell 7+:
1. Visit: https://github.com/PowerShell/PowerShell/releases/latest
2. Download the installer for your platform (Windows, macOS, or Linux)
3. Run the installer and follow the instructions
4. Restart XrmToolbox after installation

Alternatively, use Windows PowerShell 5.1 (already installed on Windows).";
        }

        /// <summary>
        /// Checks if an executable is available in the system PATH.
        /// </summary>
        private static bool IsExecutableInPath(string executableName)
        {
            try
            {
                // Try using where command (cross-platform approach)
                var processStartInfo = new ProcessStartInfo
                {
                    FileName = executableName,
                    Arguments = "--version",
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    UseShellExecute = false,
                    CreateNoWindow = true
                };

                using (var process = Process.Start(processStartInfo))
                {
                    if (process == null) return false;

                    process.WaitForExit(2000); // 2 second timeout
                    return process.ExitCode == 0;
                }
            }
            catch
            {
                return false;
            }
        }

        /// <summary>
        /// Gets the default PowerShell version to use (prefers Core if available, falls back to Desktop).
        /// </summary>
        public static PowerShellVersion GetDefaultVersion()
        {
            return IsCoreAvailable() ? PowerShellVersion.Core : PowerShellVersion.Desktop;
        }

        /// <summary>
        /// Validates that the specified PowerShell version is available.
        /// </summary>
        public static bool IsAvailable(PowerShellVersion version)
        {
            return version == PowerShellVersion.Desktop ? IsDesktopAvailable() : IsCoreAvailable();
        }
    }
}
