using System;
using System.IO;
using System.IO.Compression;
using System.Management.Automation;
using System.Threading;
using Microsoft.Crm.Sdk.Messages;
using Microsoft.PowerPlatform.Dataverse.Client;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Query;

namespace Rnwood.Dataverse.Data.PowerShell.Commands
{
    /// <summary>
    /// Exports a solution from Dataverse using an asynchronous job with progress reporting.
    /// </summary>
    [Cmdlet(VerbsData.Export, "DataverseSolution", SupportsShouldProcess = true, ConfirmImpact = ConfirmImpact.Medium, DefaultParameterSetName = "ToFile")]
    [OutputType(typeof(byte[]))]
    public class ExportDataverseSolutionCmdlet : OrganizationServiceCmdlet
    {
        /// <summary>
        /// Gets or sets the unique name of the solution to be exported. Required.
        /// </summary>
        [Parameter(Mandatory = true, Position = 0, ParameterSetName = "ToFile", HelpMessage = "The unique name of the solution to be exported.")]
        [Parameter(Mandatory = true, Position = 0, ParameterSetName = "ToFolder", HelpMessage = "The unique name of the solution to be exported.")]
        [ValidateNotNullOrEmpty]
        public string SolutionName { get; set; }

        /// <summary>
        /// Gets or sets whether the solution should be exported as a managed solution.
        /// </summary>
        [Parameter(ParameterSetName = "ToFile", HelpMessage = "Export as a managed solution. Default is unmanaged (false).")]
        [Parameter(ParameterSetName = "ToFolder", HelpMessage = "Export as a managed solution. Default is unmanaged (false).")]
        public SwitchParameter Managed { get; set; }

        /// <summary>
        /// Get or set a value indicating the version that the exported solution will support.
        /// </summary>
        [Parameter(HelpMessage = "The version that the exported solution will support.")]
        public string TargetVersion { get; set; }

        /// <summary>
        /// Gets or sets whether auto numbering settings should be included in the solution being exported.
        /// </summary>
        [Parameter(HelpMessage = "Include auto numbering settings in the exported solution.")]
        public SwitchParameter ExportAutoNumberingSettings { get; set; }

        /// <summary>
        /// Gets or sets whether calendar settings should be included in the solution being exported.
        /// </summary>
        [Parameter(HelpMessage = "Include calendar settings in the exported solution.")]
        public SwitchParameter ExportCalendarSettings { get; set; }

        /// <summary>
        /// Gets or sets whether customization settings should be included in the solution being exported.
        /// </summary>
        [Parameter(HelpMessage = "Include customization settings in the exported solution.")]
        public SwitchParameter ExportCustomizationSettings { get; set; }

        /// <summary>
        /// Gets or sets whether email tracking settings should be included in the solution being exported.
        /// </summary>
        [Parameter(HelpMessage = "Include email tracking settings in the exported solution.")]
        public SwitchParameter ExportEmailTrackingSettings { get; set; }

        /// <summary>
        /// Gets or sets whether general settings should be included in the solution being exported.
        /// </summary>
        [Parameter(HelpMessage = "Include general settings in the exported solution.")]
        public SwitchParameter ExportGeneralSettings { get; set; }

        /// <summary>
        /// Gets or sets whether marketing settings should be included in the solution being exported.
        /// </summary>
        [Parameter(HelpMessage = "Include marketing settings in the exported solution.")]
        public SwitchParameter ExportMarketingSettings { get; set; }

        /// <summary>
        /// Gets or sets whether outlook synchronization settings should be included in the solution being exported.
        /// </summary>
        [Parameter(HelpMessage = "Include Outlook synchronization settings in the exported solution.")]
        public SwitchParameter ExportOutlookSynchronizationSettings { get; set; }

        /// <summary>
        /// Gets or sets whether relationship role settings should be included in the solution being exported.
        /// </summary>
        [Parameter(HelpMessage = "Include relationship role settings in the exported solution.")]
        public SwitchParameter ExportRelationshipRoles { get; set; }

        /// <summary>
        /// Gets or sets whether ISV.Config settings should be included in the solution being exported.
        /// </summary>
        [Parameter(HelpMessage = "Include ISV.Config settings in the exported solution.")]
        public SwitchParameter ExportIsvConfig { get; set; }

        /// <summary>
        /// Gets or sets whether sales settings should be included in the solution being exported.
        /// </summary>
        [Parameter(HelpMessage = "Include sales settings in the exported solution.")]
        public SwitchParameter ExportSales { get; set; }

        /// <summary>
        /// For internal use only.
        /// </summary>
        [Parameter(HelpMessage = "For internal use only.")]
        public SwitchParameter ExportExternalApplications { get; set; }

        /// <summary>
        /// Gets or sets the path to save the exported solution file.
        /// </summary>
        [Parameter(ParameterSetName = "ToFile", HelpMessage = "Path where the exported solution file should be saved.")]
        public string OutFile { get; set; }

        /// <summary>
        /// Gets or sets the path to save and unpack the exported solution.
        /// </summary>
        [Parameter(Mandatory = true, ParameterSetName = "ToFolder", HelpMessage = "Path where the exported solution will be unpacked.")]
        [ValidateNotNullOrEmpty]
        public string OutFolder { get; set; }

        /// <summary>
        /// Gets or sets whether to unpack .msapp files when using OutFolder.
        /// </summary>
        [Parameter(ParameterSetName = "ToFolder", HelpMessage = "Unpack .msapp files found in the solution into folders.")]
        public SwitchParameter UnpackCanvas { get; set; }

        /// <summary>
        /// Gets or sets the package type for unpacking when using OutFolder. Can be 'Unmanaged', 'Managed', or 'Both'.
        /// </summary>
        [Parameter(ParameterSetName = "ToFolder", HelpMessage = "Package type: 'Unmanaged' (default), 'Managed', or 'Both' for dual Managed and Unmanaged operation.")]
        [ValidateSet("Unmanaged", "Managed", "Both", IgnoreCase = true)]
        public string PackageType { get; set; } = "Unmanaged";

        /// <summary>
        /// Gets or sets whether to output the solution file bytes to the pipeline.
        /// </summary>
        [Parameter(ParameterSetName = "ToFile", HelpMessage = "Output the solution file bytes to the pipeline.")]
        public SwitchParameter PassThru { get; set; }

        /// <summary>
        /// Gets or sets the polling interval in seconds for checking job status. Default is 5 seconds.
        /// </summary>
        [Parameter(HelpMessage = "Polling interval in seconds for checking job status. Default is 5.")]
        public int PollingIntervalSeconds { get; set; } = 5;

        /// <summary>
        /// Gets or sets the timeout in seconds for the export operation. Default is 600 seconds (10 minutes).
        /// </summary>
        [Parameter(HelpMessage = "Timeout in seconds for the export operation. Default is 600 (10 minutes).")]
        public int TimeoutSeconds { get; set; } = 600;

        /// <summary>
        /// Processes the cmdlet request and writes the response to the pipeline.
        /// </summary>
        protected override void ProcessRecord()
        {
            base.ProcessRecord();

            if (!ShouldProcess($"Solution '{SolutionName}'", "Export"))
            {
                return;
            }

            WriteVerbose($"Starting async export of solution '{SolutionName}' (Managed={Managed})");

            // Create the async export request
            var exportRequest = new ExportSolutionAsyncRequest
            {
                SolutionName = SolutionName,
                Managed = Managed.IsPresent,
                TargetVersion = TargetVersion,
                ExportAutoNumberingSettings = ExportAutoNumberingSettings.IsPresent,
                ExportCalendarSettings = ExportCalendarSettings.IsPresent,
                ExportCustomizationSettings = ExportCustomizationSettings.IsPresent,
                ExportEmailTrackingSettings = ExportEmailTrackingSettings.IsPresent,
                ExportGeneralSettings = ExportGeneralSettings.IsPresent,
                ExportMarketingSettings = ExportMarketingSettings.IsPresent,
                ExportOutlookSynchronizationSettings = ExportOutlookSynchronizationSettings.IsPresent,
                ExportRelationshipRoles = ExportRelationshipRoles.IsPresent,
                ExportIsvConfig = ExportIsvConfig.IsPresent,
                ExportSales = ExportSales.IsPresent,
                ExportExternalApplications = ExportExternalApplications.IsPresent
            };

            // Execute the async export request
            var exportResponse = (ExportSolutionAsyncResponse)Connection.Execute(exportRequest);
            var exportJobId = exportResponse.ExportJobId;
            var asyncOperationId = exportResponse.AsyncOperationId;

            WriteVerbose($"Export job started. ExportJobId: {exportJobId}, AsyncOperationId: {asyncOperationId}");

            // Monitor the async operation
            var progressRecord = new ProgressRecord(1, "Exporting Solution", $"Exporting solution '{SolutionName}'")
            {
                PercentComplete = 0
            };
            WriteProgress(progressRecord);

            var startTime = DateTime.UtcNow;
            var timeout = TimeSpan.FromSeconds(TimeoutSeconds);
            var pollingInterval = TimeSpan.FromSeconds(PollingIntervalSeconds);

            while (true)
            {
                // Check for timeout
                if (DateTime.UtcNow - startTime > timeout)
                {
                    progressRecord.RecordType = ProgressRecordType.Completed;
                    WriteProgress(progressRecord);
                    ThrowTerminatingError(new ErrorRecord(
                        new TimeoutException($"Solution export timed out after {TimeoutSeconds} seconds."),
                        "ExportTimeout",
                        ErrorCategory.OperationTimeout,
                        SolutionName));
                    return;
                }

                // Check if stopping has been requested
                if (Stopping)
                {
                    progressRecord.RecordType = ProgressRecordType.Completed;
                    WriteProgress(progressRecord);
                    WriteWarning("Export operation was stopped by user.");
                    return;
                }

                // Query the asyncoperation record to check status
                var query = new QueryExpression("asyncoperation")
                {
                    ColumnSet = new ColumnSet("statuscode", "message", "friendlymessage"),
                    Criteria = new FilterExpression
                    {
                        Conditions =
                        {
                            new ConditionExpression("asyncoperationid", ConditionOperator.Equal, asyncOperationId)
                        }
                    }
                };

                var asyncOperations = Connection.RetrieveMultiple(query);
                if (asyncOperations.Entities.Count == 0)
                {
                    progressRecord.RecordType = ProgressRecordType.Completed;
                    WriteProgress(progressRecord);
                    ThrowTerminatingError(new ErrorRecord(
                        new InvalidOperationException($"Async operation {asyncOperationId} not found."),
                        "AsyncOperationNotFound",
                        ErrorCategory.ObjectNotFound,
                        asyncOperationId));
                    return;
                }

                var asyncOperation = asyncOperations.Entities[0];
                var statusCode = asyncOperation.GetAttributeValue<OptionSetValue>("statuscode")?.Value ?? 0;
                var message = asyncOperation.GetAttributeValue<string>("message");
                var friendlyMessage = asyncOperation.GetAttributeValue<string>("friendlymessage");

                // Status codes for asyncoperation:
                // 0 = WaitingForResources
                // 10 = Waiting
                // 20 = InProgress
                // 21 = Pausing
                // 22 = Canceling
                // 30 = Succeeded
                // 31 = Failed
                // 32 = Canceled

                var statusDescription = GetStatusDescription(statusCode);
                progressRecord.StatusDescription = $"{statusDescription}";
                
                if (!string.IsNullOrEmpty(friendlyMessage))
                {
                    progressRecord.CurrentOperation = friendlyMessage;
                }
                else if (!string.IsNullOrEmpty(message))
                {
                    progressRecord.CurrentOperation = message;
                }

                WriteVerbose($"Export status: {statusDescription} (StatusCode={statusCode})");

                if (statusCode == 30) // Succeeded
                {
                    progressRecord.PercentComplete = 100;
                    WriteProgress(progressRecord);
                    WriteVerbose("Export completed successfully. Downloading solution data...");
                    break;
                }
                else if (statusCode == 31) // Failed
                {
                    progressRecord.RecordType = ProgressRecordType.Completed;
                    WriteProgress(progressRecord);
                    var errorMessage = friendlyMessage ?? message ?? "Unknown error";
                    ThrowTerminatingError(new ErrorRecord(
                        new InvalidOperationException($"Solution export failed: {errorMessage}"),
                        "ExportFailed",
                        ErrorCategory.InvalidOperation,
                        SolutionName));
                    return;
                }
                else if (statusCode == 32) // Canceled
                {
                    progressRecord.RecordType = ProgressRecordType.Completed;
                    WriteProgress(progressRecord);
                    WriteWarning("Solution export was canceled.");
                    return;
                }
                else
                {
                    // In progress - update progress percentage based on status
                    if (statusCode == 20) // InProgress
                    {
                        progressRecord.PercentComplete = 50;
                    }
                    else if (statusCode == 0 || statusCode == 10) // Waiting
                    {
                        progressRecord.PercentComplete = 10;
                    }

                    WriteProgress(progressRecord);
                }

                // Wait before polling again
                Thread.Sleep(pollingInterval);
            }

            // Download the solution file
            var downloadRequest = new DownloadSolutionExportDataRequest
            {
                ExportJobId = exportJobId
            };

            WriteVerbose("Downloading exported solution data...");
            var downloadResponse = (DownloadSolutionExportDataResponse)Connection.Execute(downloadRequest);
            var solutionFileBytes = downloadResponse.ExportSolutionFile;

            progressRecord.RecordType = ProgressRecordType.Completed;
            WriteProgress(progressRecord);

            WriteVerbose($"Solution exported successfully. Size: {solutionFileBytes.Length} bytes");

            // Handle ToFolder parameter set - unpack the solution
            if (ParameterSetName == "ToFolder")
            {
                string tempZipPath = null;
                try
                {
                    // Save to temp file first
                    tempZipPath = System.IO.Path.Combine(System.IO.Path.GetTempPath(), $"{SolutionName}_{Guid.NewGuid():N}.zip");
                    WriteVerbose($"Saving solution to temporary file: {tempZipPath}");
                    File.WriteAllBytes(tempZipPath, solutionFileBytes);

                    // Unpack using PAC CLI
                    var resolvedOutputPath = GetUnresolvedProviderPathFromPSPath(OutFolder);
                    WriteVerbose($"Unpacking solution to '{resolvedOutputPath}'");

                    // Build PAC CLI arguments (always use clobber and allowDelete)
                    var args = $"solution unpack --zipfile \"{tempZipPath}\" --folder \"{resolvedOutputPath}\" --packagetype {PackageType} --clobber --allowDelete";

                    // Execute PAC CLI
                    int exitCode = PacCliHelper.ExecutePacCli(this, args);

                    if (exitCode != 0)
                    {
                        ThrowTerminatingError(new ErrorRecord(
                            new InvalidOperationException($"PAC CLI unpack failed with exit code {exitCode}"),
                            "PacCliFailed",
                            ErrorCategory.InvalidOperation,
                            tempZipPath));
                        return;
                    }

                    WriteVerbose("Solution unpacked successfully.");

                    // If UnpackCanvas is specified, find and unpack .msapp files
                    if (UnpackCanvas.IsPresent)
                    {
                        WriteVerbose("Searching for .msapp files to unpack...");
                        UnpackMsappFiles(resolvedOutputPath);
                    }
                }
                finally
                {
                    // Clean up temp file
                    if (tempZipPath != null && File.Exists(tempZipPath))
                    {
                        WriteVerbose($"Cleaning up temporary file: {tempZipPath}");
                        try
                        {
                            File.Delete(tempZipPath);
                        }
                        catch (Exception ex)
                        {
                            WriteWarning($"Failed to delete temporary file '{tempZipPath}': {ex.Message}");
                        }
                    }
                }
            }
            else
            {
                // Handle ToFile parameter set
                // Save to file if OutFile is specified
                if (!string.IsNullOrEmpty(OutFile))
                {
                    var outFilePath = GetUnresolvedProviderPathFromPSPath(OutFile);
                    WriteVerbose($"Saving solution to file: {outFilePath}");
                    File.WriteAllBytes(outFilePath, solutionFileBytes);
                    WriteVerbose("Solution file saved successfully.");
                }

                // Output to pipeline if PassThru is specified
                if (PassThru.IsPresent)
                {
                    WriteObject(solutionFileBytes);
                }
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

        private string GetStatusDescription(int statusCode)
        {
            switch (statusCode)
            {
                case 0:
                    return "Waiting for resources";
                case 10:
                    return "Waiting";
                case 20:
                    return "In progress";
                case 21:
                    return "Pausing";
                case 22:
                    return "Canceling";
                case 30:
                    return "Succeeded";
                case 31:
                    return "Failed";
                case 32:
                    return "Canceled";
                default:
                    return $"Unknown status ({statusCode})";
            }
        }
    }
}
