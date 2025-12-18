using System;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Management.Automation;

namespace Rnwood.Dataverse.Data.PowerShell.Commands
{
    /// <summary>
    /// Unpacks a Dataverse solution file using the Power Apps CLI.
    /// </summary>
    [Cmdlet(VerbsData.Expand, "DataverseSolution", SupportsShouldProcess = true, ConfirmImpact = ConfirmImpact.Medium)]
    [Alias("Unpack-DataverseSolution")]
    public class ExpandDataverseSolutionCmdlet : PSCmdlet
    {
        /// <summary>
        /// Gets or sets the path to the solution file (.zip) to unpack.
        /// </summary>
        [Parameter(Mandatory = true, Position = 0, HelpMessage = "Path to the solution file (.zip) to unpack.")]
        [ValidateNotNullOrEmpty]
        public string Path { get; set; }

        /// <summary>
        /// Gets or sets the output path where the solution will be unpacked.
        /// </summary>
        [Parameter(Mandatory = true, Position = 1, HelpMessage = "Output path where the solution will be unpacked.")]
        [ValidateNotNullOrEmpty]
        public string OutputPath { get; set; }

        /// <summary>
        /// Gets or sets whether to unpack .msapp files into folders.
        /// </summary>
        [Parameter(HelpMessage = "Unpack .msapp files found in the solution into folders (same name without extension).")]
        public SwitchParameter UnpackMsapp { get; set; }

        /// <summary>
        /// Gets or sets the PAC CLI version to use. If not specified, uses the latest version.
        /// </summary>
        [Parameter(HelpMessage = "PAC CLI version to use (e.g., '1.31.6'). If not specified, uses the latest version.")]
        public string PacVersion { get; set; }

        /// <summary>
        /// Processes the cmdlet request.
        /// </summary>
        protected override void ProcessRecord()
        {
            base.ProcessRecord();

            // Resolve paths
            string resolvedPath = GetUnresolvedProviderPathFromPSPath(Path);
            string resolvedOutputPath = GetUnresolvedProviderPathFromPSPath(OutputPath);

            if (!File.Exists(resolvedPath))
            {
                ThrowTerminatingError(new ErrorRecord(
                    new FileNotFoundException($"Solution file not found: {resolvedPath}"),
                    "FileNotFound",
                    ErrorCategory.ObjectNotFound,
                    resolvedPath));
                return;
            }

            if (!ShouldProcess($"Solution file '{resolvedPath}' to '{resolvedOutputPath}'", "Unpack"))
            {
                return;
            }

            WriteVerbose($"Unpacking solution from '{resolvedPath}' to '{resolvedOutputPath}'");

            // Build PAC CLI arguments (always use clobber and allowDelete)
            var args = $"solution unpack --zipfile \"{resolvedPath}\" --folder \"{resolvedOutputPath}\" --clobber --allowDelete";

            // Execute PAC CLI with specified version
            int exitCode = PacCliHelper.ExecutePacCli(this, args, version: PacVersion);

            if (exitCode != 0)
            {
                ThrowTerminatingError(new ErrorRecord(
                    new InvalidOperationException($"PAC CLI unpack failed with exit code {exitCode}"),
                    "PacCliFailed",
                    ErrorCategory.InvalidOperation,
                    resolvedPath));
                return;
            }

            WriteVerbose("Solution unpacked successfully.");

            // If UnpackMsapp is specified, find and unpack .msapp files
            if (UnpackMsapp.IsPresent)
            {
                WriteVerbose("Searching for .msapp files to unpack...");
                UnpackMsappFiles(resolvedOutputPath);
            }
        }

        private void UnpackMsappFiles(string basePath)
        {
            // Find all .msapp files in the unpacked solution
            string[] msappFiles = Directory.GetFiles(basePath, "*.msapp", SearchOption.AllDirectories);

            if (msappFiles.Length == 0)
            {
                WriteVerbose("No .msapp files found.");
                return;
            }

            WriteVerbose($"Found {msappFiles.Length} .msapp file(s) to unpack.");

            foreach (string msappFile in msappFiles)
            {
                string msappDirectory = System.IO.Path.GetDirectoryName(msappFile);
                string msappFileName = System.IO.Path.GetFileNameWithoutExtension(msappFile);
                string outputFolder = System.IO.Path.Combine(msappDirectory, msappFileName);

                WriteVerbose($"Unpacking '{msappFile}' to '{outputFolder}'");

                try
                {
                    // Always overwrite - delete existing folder if present
                    if (Directory.Exists(outputFolder))
                    {
                        Directory.Delete(outputFolder, recursive: true);
                    }
                    
                    Directory.CreateDirectory(outputFolder);

                    // Unzip the .msapp file
                    ZipFile.ExtractToDirectory(msappFile, outputFolder);

                    WriteVerbose($"Successfully unpacked '{msappFile}'");
                }
                catch (Exception ex)
                {
                    WriteWarning($"Failed to unpack '{msappFile}': {ex.Message}");
                }
            }

            WriteVerbose($"Finished unpacking {msappFiles.Length} .msapp file(s).");
        }
    }
}
