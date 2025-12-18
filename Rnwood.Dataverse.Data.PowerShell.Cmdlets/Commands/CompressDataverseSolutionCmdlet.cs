using System;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Management.Automation;

namespace Rnwood.Dataverse.Data.PowerShell.Commands
{
    /// <summary>
    /// Packs a Dataverse solution folder using the Power Apps CLI.
    /// </summary>
    [Cmdlet(VerbsData.Compress, "DataverseSolution", SupportsShouldProcess = true, ConfirmImpact = ConfirmImpact.Medium)]
    [Alias("Pack-DataverseSolution")]
    public class CompressDataverseSolutionCmdlet : PSCmdlet
    {
        /// <summary>
        /// Gets or sets the path to the solution folder to pack.
        /// </summary>
        [Parameter(Mandatory = true, Position = 0, HelpMessage = "Path to the solution folder to pack.")]
        [ValidateNotNullOrEmpty]
        public string Path { get; set; }

        /// <summary>
        /// Gets or sets the output path for the packed solution file (.zip).
        /// </summary>
        [Parameter(Mandatory = true, Position = 1, HelpMessage = "Output path for the packed solution file (.zip).")]
        [ValidateNotNullOrEmpty]
        public string OutputPath { get; set; }

        /// <summary>
        /// Gets or sets whether to pack .msapp folders into .msapp files.
        /// </summary>
        [Parameter(HelpMessage = "Pack .msapp folders found in the solution into .msapp zip files (same name with .msapp extension).")]
        public SwitchParameter PackMsapp { get; set; }

        /// <summary>
        /// Processes the cmdlet request.
        /// </summary>
        protected override void ProcessRecord()
        {
            base.ProcessRecord();

            // Resolve paths
            string resolvedPath = GetUnresolvedProviderPathFromPSPath(Path);
            string resolvedOutputPath = GetUnresolvedProviderPathFromPSPath(OutputPath);

            if (!Directory.Exists(resolvedPath))
            {
                ThrowTerminatingError(new ErrorRecord(
                    new DirectoryNotFoundException($"Solution folder not found: {resolvedPath}"),
                    "FolderNotFound",
                    ErrorCategory.ObjectNotFound,
                    resolvedPath));
                return;
            }

            if (!ShouldProcess($"Solution folder '{resolvedPath}' to '{resolvedOutputPath}'", "Pack"))
            {
                return;
            }

            WriteVerbose($"Packing solution from '{resolvedPath}' to '{resolvedOutputPath}'");

            string workingPath = resolvedPath;
            string tempPath = null;

            try
            {
                // If PackMsapp is specified, create a temp copy and pack .msapp folders
                if (PackMsapp.IsPresent)
                {
                    WriteVerbose("PackMsapp flag is set. Creating temporary copy to pack .msapp folders...");
                    tempPath = System.IO.Path.Combine(System.IO.Path.GetTempPath(), $"dataverse_solution_{Guid.NewGuid():N}");
                    Directory.CreateDirectory(tempPath);

                    WriteVerbose($"Copying solution to temporary location: {tempPath}");
                    CopyDirectory(resolvedPath, tempPath);

                    WriteVerbose("Packing .msapp folders...");
                    PackMsappFolders(tempPath);

                    workingPath = tempPath;
                }

                // Build PAC CLI arguments
                var args = $"solution pack --zipfile \"{resolvedOutputPath}\" --folder \"{workingPath}\"";

                // Execute PAC CLI
                int exitCode = PacCliHelper.ExecutePacCli(this, args);

                if (exitCode != 0)
                {
                    ThrowTerminatingError(new ErrorRecord(
                        new InvalidOperationException($"PAC CLI pack failed with exit code {exitCode}"),
                        "PacCliFailed",
                        ErrorCategory.InvalidOperation,
                        resolvedPath));
                    return;
                }

                WriteVerbose("Solution packed successfully.");
            }
            finally
            {
                // Clean up temp directory
                if (tempPath != null && Directory.Exists(tempPath))
                {
                    WriteVerbose($"Cleaning up temporary directory: {tempPath}");
                    try
                    {
                        Directory.Delete(tempPath, recursive: true);
                    }
                    catch (Exception ex)
                    {
                        WriteWarning($"Failed to delete temporary directory '{tempPath}': {ex.Message}");
                    }
                }
            }
        }

        private void PackMsappFolders(string basePath)
        {
            // Find all directories with .msapp extension
            var msappFolders = Directory.GetDirectories(basePath, "*.msapp", SearchOption.AllDirectories);

            if (msappFolders.Length == 0)
            {
                WriteVerbose("No .msapp folders found.");
                return;
            }

            WriteVerbose($"Found {msappFolders.Length} .msapp folder(s) to pack.");

            foreach (string msappFolder in msappFolders)
            {
                string parentDirectory = System.IO.Path.GetDirectoryName(msappFolder);
                string folderName = System.IO.Path.GetFileName(msappFolder);
                // The folder already has .msapp extension, so use it as-is for the file name
                string msappFile = System.IO.Path.Combine(parentDirectory, folderName);

                WriteVerbose($"Packing '{msappFolder}' to '{msappFile}'");

                try
                {
                    // Delete existing .msapp file if it exists (shouldn't normally)
                    if (File.Exists(msappFile))
                    {
                        WriteVerbose($"Deleting existing .msapp file: {msappFile}");
                        File.Delete(msappFile);
                    }

                    // Create the .msapp file (which is a zip)
                    ZipFile.CreateFromDirectory(msappFolder, msappFile, CompressionLevel.Optimal, includeBaseDirectory: false);

                    WriteVerbose($"Successfully packed '{msappFolder}'");

                    // Delete the folder after successful packing
                    WriteVerbose($"Deleting source folder: {msappFolder}");
                    Directory.Delete(msappFolder, recursive: true);
                }
                catch (Exception ex)
                {
                    WriteWarning($"Failed to pack '{msappFolder}': {ex.Message}");
                }
            }

            WriteVerbose($"Finished packing {msappFolders.Length} .msapp folder(s).");
        }

        private void CopyDirectory(string sourceDir, string destDir)
        {
            // Create destination directory
            Directory.CreateDirectory(destDir);

            // Copy files
            foreach (string file in Directory.GetFiles(sourceDir))
            {
                string destFile = System.IO.Path.Combine(destDir, System.IO.Path.GetFileName(file));
                File.Copy(file, destFile, overwrite: true);
            }

            // Copy subdirectories
            foreach (string subDir in Directory.GetDirectories(sourceDir))
            {
                string destSubDir = System.IO.Path.Combine(destDir, System.IO.Path.GetFileName(subDir));
                CopyDirectory(subDir, destSubDir);
            }
        }
    }
}
