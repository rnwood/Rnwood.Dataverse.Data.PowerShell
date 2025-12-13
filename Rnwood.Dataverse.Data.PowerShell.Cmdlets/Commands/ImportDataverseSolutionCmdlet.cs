using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Management.Automation;
using System.Text;
using System.Threading;
using System.Xml.Linq;
using Microsoft.Crm.Sdk.Messages;
using Microsoft.PowerPlatform.Dataverse.Client;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Query;
using Rnwood.Dataverse.Data.PowerShell.Commands.Model;

namespace Rnwood.Dataverse.Data.PowerShell.Commands
{
    /// <summary>
    /// Specifies the import mode for solution imports.
    /// </summary>
    public enum ImportMode
    {
        /// <summary>
        /// Automatically determine the best import method based on solution existence and managed status.
        /// </summary>
        Auto,

        /// <summary>
        /// Skip upgrade logic and perform regular import.
        /// </summary>
        NoUpgrade,

        /// <summary>
        /// Import the solution using Stage and Upgrade mode.
        /// </summary>
        StageAndUpgrade,

        /// <summary>
        /// Import the solution as a holding solution staged for upgrade.
        /// </summary>
        HoldingSolution
    }

    /// <summary>
    /// Imports a solution to Dataverse using an asynchronous job with progress reporting.
    /// </summary>
    [Cmdlet(VerbsData.Import, "DataverseSolution", SupportsShouldProcess = true, ConfirmImpact = ConfirmImpact.Medium)]
    public class ImportDataverseSolutionCmdlet : OrganizationServiceCmdlet
    {
        /// <summary>
        /// Gets or sets the path to the solution file to import.
        /// </summary>
        [Parameter(Mandatory = true, Position = 0, ParameterSetName = "FromFile", HelpMessage = "Path to the solution file (.zip) to import.")]
        [ValidateNotNullOrEmpty]
        public string InFile { get; set; }

        /// <summary>
        /// Gets or sets the solution file bytes to import.
        /// </summary>
        [Parameter(Mandatory = true, ValueFromPipeline = true, ParameterSetName = "FromBytes", HelpMessage = "Solution file bytes to import.")]
        public byte[] SolutionFile { get; set; }

        /// <summary>
        /// Gets or sets whether any unmanaged customizations should be overwritten.
        /// </summary>
        [Parameter(HelpMessage = "Overwrite any unmanaged customizations that have been applied over existing managed solution components.")]
        public SwitchParameter OverwriteUnmanagedCustomizations { get; set; }

        /// <summary>
        /// Gets or sets whether workflows should be activated after import.
        /// </summary>
        [Parameter(HelpMessage = "Activate any processes (workflows) included in the solution after import.")]
        public SwitchParameter PublishWorkflows { get; set; }

        /// <summary>
        /// Gets or sets whether to skip product update dependencies.
        /// </summary>
        [Parameter(HelpMessage = "Skip enforcement of dependencies related to product updates.")]
        public SwitchParameter SkipProductUpdateDependencies { get; set; }

        /// <summary>
        /// Gets or sets the import mode.
        /// </summary>
        [Parameter(HelpMessage = "The import mode to use. Auto (default) automatically determines the best method based on solution existence and managed status.")]
        public ImportMode Mode { get; set; } = ImportMode.Auto;

        /// <summary>
        /// Gets or sets the connection references.
        /// </summary>
        [Parameter(HelpMessage = "Hashtable of connection reference schema names to connection IDs (e.g., @{'new_sharedconnectionref' = '00000000-0000-0000-0000-000000000000'}).")]
        public Hashtable ConnectionReferences { get; set; }

        /// <summary>
        /// Gets or sets environment variable values as a hashtable.
        /// </summary>
        [Parameter(HelpMessage = "Hashtable of environment variable schema names to values (e.g., @{'new_apiurl' = 'https://api.example.com'}).")]
        public Hashtable EnvironmentVariables { get; set; }

        /// <summary>
        /// Gets or sets whether to convert to managed (obsolete).
        /// </summary>
        [Parameter(HelpMessage = "Obsolete. The system will convert unmanaged solution components to managed when you import a managed solution.")]
        public SwitchParameter ConvertToManaged { get; set; }

        /// <summary>
        /// Gets or sets whether to skip queue ribbon job.
        /// </summary>
        [Parameter(HelpMessage = "For internal use only.")]
        public SwitchParameter SkipQueueRibbonJob { get; set; }

        /// <summary>
        /// Gets or sets the layer desired order.
        /// </summary>
        [Parameter(HelpMessage = "For internal use only.")]
        public LayerDesiredOrder LayerDesiredOrder { get; set; }

        /// <summary>
        /// Gets or sets whether to use async ribbon processing.
        /// </summary>
        [Parameter(HelpMessage = "For internal use only.")]
        public SwitchParameter AsyncRibbonProcessing { get; set; }

        /// <summary>
        /// Gets or sets whether to use update if additive mode (experimental and incomplete).
        /// </summary>
        [Parameter(HelpMessage = "Use update if additive mode (experimental and incomplete). Only valid with Auto (default) mode. If the solution already exists in the target environment, compares the solution file with the target environment. If there are zero items removed ('TargetOnly' or 'InSourceAndTarget_BehaviourLessInclusiveInSource' status), uses simple install mode (no stage and upgrade) for best performance.")]
        public SwitchParameter UseUpdateIfAdditive { get; set; }

        /// <summary>
        /// Gets or sets the polling interval in seconds for checking job status. Default is 5 seconds.
        /// </summary>
        [Parameter(HelpMessage = "Polling interval in seconds for checking job status. Default is 5.")]
        public int PollingIntervalSeconds { get; set; } = 5;

        /// <summary>
        /// Gets or sets the timeout in seconds for the import operation. Default is 7200 seconds (2 hours).
        /// </summary>
        [Parameter(HelpMessage = "Timeout in seconds for the import operation. Default is 7200 (2 hours).")]
        public int TimeoutSeconds { get; set; } = 7200;

        /// <summary>
        /// Gets or sets whether to skip validation of connection references.
        /// </summary>
        [Parameter(HelpMessage = "Skip validation that all required connection references are provided.")]
        public SwitchParameter SkipConnectionReferenceValidation { get; set; }

        /// <summary>
        /// Gets or sets whether to skip validation of environment variables.
        /// </summary>
        [Parameter(HelpMessage = "Skip validation that all required environment variables are provided.")]
        public SwitchParameter SkipEnvironmentVariableValidation { get; set; }

        /// <summary>
        /// Gets or sets whether to skip import if the solution version in the file is the same as the installed version.
        /// </summary>
        [Parameter(HelpMessage = "Skip import if the solution version in the file is the same as the installed version in the target environment.")]
        public SwitchParameter SkipIfSameVersion { get; set; }

        /// <summary>
        /// Gets or sets whether to skip import if the solution version in the file is lower than the installed version.
        /// </summary>
        [Parameter(HelpMessage = "Skip import if the solution version in the file is lower than the installed version in the target environment.")]
        public SwitchParameter SkipIfLowerVersion { get; set; }

        /// <summary>
        /// Processes the cmdlet request.
        /// </summary>
        protected override void ProcessRecord()
        {
            base.ProcessRecord();

            // Load solution file
            byte[] solutionBytes;
            if (ParameterSetName == "FromFile")
            {
                if (!ShouldProcess($"Solution file '{InFile}'", "Import"))
                {
                    return;
                }

                var filePath = GetUnresolvedProviderPathFromPSPath(InFile);
                if (!File.Exists(filePath))
                {
                    ThrowTerminatingError(new ErrorRecord(
                        new FileNotFoundException($"Solution file not found: {filePath}"),
                        "FileNotFound",
                        ErrorCategory.ObjectNotFound,
                        filePath));
                    return;
                }

                WriteVerbose($"{filePath}");
                solutionBytes = File.ReadAllBytes(filePath);
            }
            else
            {
                if (!ShouldProcess("Solution bytes", "Import"))
                {
                    return;
                }

                solutionBytes = SolutionFile;
            }

            WriteVerbose($"Solution file size: {solutionBytes.Length} bytes");

            // Validate solution components (connection references and environment variables)
            ValidateSolutionComponents(solutionBytes);

            // Validate parameter combinations
            if (UseUpdateIfAdditive.IsPresent && Mode != ImportMode.Auto)
            {
                ThrowTerminatingError(new ErrorRecord(
                    new InvalidOperationException("-UseUpdateIfAdditive is only valid with Auto (default) mode."),
                    "InvalidParameterCombination",
                    ErrorCategory.InvalidArgument,
                    null));
            }

            // Determine import mode based on Mode parameter
            bool useNoUpgrade = Mode == ImportMode.NoUpgrade;
            bool useHoldingSolution = Mode == ImportMode.HoldingSolution;
            bool useStageAndUpgrade = Mode == ImportMode.Auto || Mode == ImportMode.StageAndUpgrade;

            // Extract solution info including version
            var (solutionUniqueName, isManaged, sourceSolutionVersion) = ExtractSolutionInfo(solutionBytes);
            WriteVerbose($"Source solution '{solutionUniqueName}' is {(isManaged ? "managed" : "unmanaged")}");
            
            if (sourceSolutionVersion != null)
            {
                WriteVerbose($"Source solution version: {sourceSolutionVersion}");
            }

            // Check if this is an upgrade scenario and if the solution already exists
            bool shouldUseStageAndUpgrade = false;
            bool shouldUseHoldingSolution = false;
            bool solutionExists = DoesSolutionExist(solutionBytes);

            // Version checking logic
            if (solutionExists && (SkipIfSameVersion.IsPresent || SkipIfLowerVersion.IsPresent))
            {
                Version installedVersion = GetInstalledSolutionVersion(solutionUniqueName);
                if (installedVersion != null && sourceSolutionVersion != null)
                {
                    WriteVerbose($"Installed solution version: {installedVersion}");
                    
                    int versionComparison = sourceSolutionVersion.CompareTo(installedVersion);
                    
                    if (SkipIfSameVersion.IsPresent && versionComparison == 0)
                    {
                        WriteWarning($"Skipping import: Solution '{solutionUniqueName}' version {sourceSolutionVersion} is already installed (same version).");
                        
                        // Check and update connection references and environment variables if provided
                        CheckAndUpdateSolutionComponents(solutionBytes);
                        
                        return;
                    }
                    
                    if (SkipIfLowerVersion.IsPresent && versionComparison < 0)
                    {
                        WriteWarning($"Skipping import: Solution '{solutionUniqueName}' version {sourceSolutionVersion} is lower than installed version {installedVersion}.");
                        
                        // Check and update connection references and environment variables if provided
                        CheckAndUpdateSolutionComponents(solutionBytes);
                        
                        return;
                    }
                    
                    WriteVerbose($"Version check passed: source version {sourceSolutionVersion} vs installed version {installedVersion}");
                }
                else
                {
                    if (sourceSolutionVersion == null)
                    {
                        WriteWarning($"Unable to extract source solution version for '{solutionUniqueName}'. Skipping version check.");
                    }
                    if (installedVersion == null)
                    {
                        WriteWarning($"Unable to retrieve installed solution version for '{solutionUniqueName}'. Skipping version check.");
                    }
                }
            }

            if (useHoldingSolution)
            {
                // Extract solution unique name from the solution file (this is a simplified approach)
                // In a real scenario, you might want to parse the solution XML
                WriteVerbose("HoldingSolution mode specified - checking if solution already exists...");

                // Try to detect if solution exists by attempting to query for it
                // We'll catch the exception if it doesn't exist and fallback
                if (solutionExists)
                {
                    shouldUseHoldingSolution = true;
                }
                else
                {
                    WriteWarning("Solution does not exist in the target environment. Falling back to regular import instead of upgrade.");
                }
            }
            else if (useStageAndUpgrade)
            {
                if (Mode == ImportMode.Auto)
                {
                    WriteVerbose("Auto mode - checking if solution already EXISTS and is managed...");
                }
                else
                {
                    WriteVerbose("StageAndUpgrade mode specified - checking if solution already exists...");
                }

                if (solutionExists && (Mode == ImportMode.StageAndUpgrade || isManaged))
                {
                    shouldUseStageAndUpgrade = true;
                    WriteVerbose("Solution exists and source is managed - using StageAndUpgradeAsyncRequest");
                }
                else
                {
                    if (!solutionExists)
                    {
                        WriteWarning("Solution does not exist in the target environment. Falling back to regular import instead of upgrade.");
                    }
                    else if (Mode == ImportMode.Auto && !isManaged)
                    {
                        WriteWarning("Source solution is unmanaged. Falling back to regular import instead of upgrade.");
                    }
                }
            }

            // Handle UseUpdateIfAdditive logic
            if (UseUpdateIfAdditive.IsPresent && solutionExists)
            {
                WriteWarning("UseUpdateIfAdditive is experimental and incomplete. Behavior may be incorrect and may change in future versions.");

                WriteVerbose("UseUpdateIfAdditive specified - comparing solution components...");

                // Extract source components
                string sourceSolutionName = ExtractSolutionName(solutionBytes);
                var sourceExtractor = new FileComponentExtractor(Connection, this, solutionBytes);
                var sourceComponents = sourceExtractor.GetComponents(includeSubcomponents: false);
                WriteVerbose($"Extracted source solution: {sourceSolutionName} with {sourceComponents.Count} root components");

                // Query target environment for solution id
                var solutionQuery = new QueryExpression("solution")
                {
                    ColumnSet = new ColumnSet("solutionid"),
                    Criteria = new FilterExpression
                    {
                        Conditions =
                        {
                            new ConditionExpression("uniquename", ConditionOperator.Equal, sourceSolutionName)
                        }
                    },
                    TopCount = 1
                };

                var solutions = Connection.RetrieveMultiple(solutionQuery);
                if (solutions.Entities.Count > 0)
                {
                    var solutionId = solutions.Entities[0].Id;
               

                    // Compare components
                    var sourceExtractor2 = new FileComponentExtractor(Connection, this, solutionBytes);
                    var targetExtractor = new EnvironmentComponentExtractor(Connection, this, solutionId);
                    var comparer = new SolutionComponentComparer(sourceExtractor2, targetExtractor, this);
                    var comparisonResults = comparer.CompareComponents();

                    // Count problematic statuses
                    int targetOnlyCount = comparisonResults.Count(r => r.Status == SolutionComponentStatus.InTargetOnly);
                    int lessInclusiveCount = comparisonResults.Count(r => r.Status == SolutionComponentStatus.InSourceAndTarget_BehaviourLessInclusiveInSource);

                    WriteVerbose($"Comparison results: {targetOnlyCount} TargetOnly, {lessInclusiveCount} LessInclusiveInSource");

                    if (targetOnlyCount == 0 && lessInclusiveCount == 0)
                    {
                        WriteVerbose("No removed components found - using simple install mode (no stage and upgrade)");
                        shouldUseStageAndUpgrade = false;
                        shouldUseHoldingSolution = false;
                    }
                    else
                    {
                        WriteVerbose("Removed components found - proceeding with full upgrade logic to ensure they are removed correctly.");
                        // List the problematic components
                        foreach (var result in comparisonResults.Where(r => r.Status == SolutionComponentStatus.InTargetOnly || r.Status == SolutionComponentStatus.InSourceAndTarget_BehaviourLessInclusiveInSource))
                        {
                            string componentName = result.SourceComponent?.UniqueName ?? result.TargetComponent?.UniqueName ?? "Unknown";
                            int componentType = result.SourceComponent?.ComponentType ?? result.TargetComponent?.ComponentType ?? 0;
                            WriteVerbose($"  Removed component: Type {componentType} '{componentName}' - {result.Status}");
                        }
                        // Keep the existing logic for shouldUseStageAndUpgrade
                    }
                }
            }

            // Build ComponentParameters from ConnectionReferences and EnvironmentVariables hashtables
            EntityCollection componentParameters = null;

            int totalParams = (ConnectionReferences?.Count ?? 0) + (EnvironmentVariables?.Count ?? 0);

            if (totalParams > 0)
            {
                WriteVerbose($"Processing {totalParams} component parameter(s)...");
                componentParameters = new EntityCollection();

                // Process connection references
                if (ConnectionReferences != null && ConnectionReferences.Count > 0)
                {
                    WriteVerbose($"Processing {ConnectionReferences.Count} connection reference(s)...");
                    foreach (DictionaryEntry entry in ConnectionReferences)
                    {
                        var connectionRefName = entry.Key.ToString();
                        var connectionId = entry.Value.ToString();

                        WriteVerbose($"  Setting connection reference '{connectionRefName}' to connection '{connectionId}'");

                        var componentParam = new Entity("connectionreference");
                        componentParam["connectionreferencelogicalname"] = connectionRefName;
                        componentParam["connectionid"] = connectionId;

                        componentParameters.Entities.Add(componentParam);
                    }
                }

                // Process environment variables
                if (EnvironmentVariables != null && EnvironmentVariables.Count > 0)
                {
                    WriteVerbose($"Processing {EnvironmentVariables.Count} environment variable(s)...");

                    // Query for existing environment variable values by schema name
                    var existingEnvVarValuesBySchemaName = GetExistingEnvironmentVariableValueIds(EnvironmentVariables.Keys.Cast<object>().Select(k => k.ToString()).ToList());

                    foreach (DictionaryEntry entry in EnvironmentVariables)
                    {
                        var envVarSchemaName = entry.Key.ToString();
                        var envVarValue = entry.Value.ToString();

                        WriteVerbose($"  Setting environment variable '{envVarSchemaName}' to value '{envVarValue}'");

                        var componentParam = new Entity("environmentvariablevalue");
                        componentParam["schemaname"] = envVarSchemaName;
                        componentParam["value"] = envVarValue;

                        // If there's an existing value record, include its ID for update
                        if (existingEnvVarValuesBySchemaName.TryGetValue(envVarSchemaName, out var existingValueId))
                        {
                            componentParam["environmentvariablevalueid"] = existingValueId;
                            WriteVerbose($"    Found existing value record with ID: {existingValueId}");
                        }

                        componentParameters.Entities.Add(componentParam);
                    }
                }
            }

            // Create the async import request
            OrganizationRequest importRequest;
            if (shouldUseStageAndUpgrade)
            {
                var stageRequest = new StageAndUpgradeAsyncRequest
                {
                    CustomizationFile = solutionBytes,
                    OverwriteUnmanagedCustomizations = OverwriteUnmanagedCustomizations.IsPresent,
                    PublishWorkflows = PublishWorkflows.IsPresent,
                    SkipProductUpdateDependencies = SkipProductUpdateDependencies.IsPresent,
                    ConvertToManaged = ConvertToManaged.IsPresent,
                    SkipQueueRibbonJob = SkipQueueRibbonJob.IsPresent,
                    AsyncRibbonProcessing = AsyncRibbonProcessing.IsPresent,
                    ComponentParameters = componentParameters
                };

                if (LayerDesiredOrder != null)
                {
                    stageRequest.LayerDesiredOrder = LayerDesiredOrder;
                }

                importRequest = stageRequest;
                WriteVerbose("Using StageAndUpgradeAsyncRequest");
            }
            else
            {
                var importSolutionRequest = new ImportSolutionAsyncRequest
                {
                    CustomizationFile = solutionBytes,
                    OverwriteUnmanagedCustomizations = OverwriteUnmanagedCustomizations.IsPresent,
                    PublishWorkflows = PublishWorkflows.IsPresent,
                    SkipProductUpdateDependencies = SkipProductUpdateDependencies.IsPresent,
                    HoldingSolution = shouldUseHoldingSolution,
                    ConvertToManaged = ConvertToManaged.IsPresent,
                    SkipQueueRibbonJob = SkipQueueRibbonJob.IsPresent,
                    AsyncRibbonProcessing = AsyncRibbonProcessing.IsPresent,
                    ComponentParameters = componentParameters
                };

                if (LayerDesiredOrder != null)
                {
                    importSolutionRequest.LayerDesiredOrder = LayerDesiredOrder;
                }

                importRequest = importSolutionRequest;
                WriteVerbose($"Using ImportSolutionAsyncRequest (HoldingSolution={shouldUseHoldingSolution})");
            }

            // Execute the async import request
            string importJobId;
            Guid asyncOperationId;
            if (importRequest is StageAndUpgradeAsyncRequest)
            {
                var response = (StageAndUpgradeAsyncResponse)Connection.Execute(importRequest);
                importJobId = response.ImportJobKey;
                asyncOperationId = response.AsyncOperationId;
            }
            else
            {
                var response = (ImportSolutionAsyncResponse)Connection.Execute(importRequest);
                importJobId = response.ImportJobKey;
                asyncOperationId = response.AsyncOperationId;
            }

            WriteVerbose($"Import job started. ImportJobKey: {importJobId}, AsyncOperationId: {asyncOperationId}");

            // Monitor the async operation
            var progressRecord = new ProgressRecord(1, "Importing Solution", "Importing solution...")
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
                        new TimeoutException($"Solution import timed out after {TimeoutSeconds} seconds."),
                        "ImportTimeout",
                        ErrorCategory.OperationTimeout,
                        null));
                    return;
                }

                // Check if stopping has been requested
                if (Stopping)
                {
                    progressRecord.RecordType = ProgressRecordType.Completed;
                    WriteProgress(progressRecord);
                    WriteWarning("Import operation was stopped by user.");
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

                // Query the importjob record to get progress
                var jobQuery = new QueryExpression("importjob")
                {
                    ColumnSet = new ColumnSet("progress"),
                    Criteria = new FilterExpression
                    {
                        Conditions =
                        {
                            new ConditionExpression("importjobid", ConditionOperator.Equal, importJobId)
                        }
                    }
                };

                var jobResults = Connection.RetrieveMultiple(jobQuery);
                int progress = 0;
                if (jobResults.Entities.Count > 0)
                {
                    progress = (int) jobResults.Entities[0].GetAttributeValue<double>("progress");
                }

                var statusDescription = GetStatusDescription(statusCode);
                progressRecord.StatusDescription = $"{statusDescription}";
                progressRecord.PercentComplete = progress;

                if (!string.IsNullOrEmpty(friendlyMessage))
                {
                    progressRecord.CurrentOperation = friendlyMessage;
                }
                else if (!string.IsNullOrEmpty(message))
                {
                    progressRecord.CurrentOperation = message;
                }

                WriteVerbose($"Import status: {statusDescription} (StatusCode={statusCode}, Progress={progress}%)");

                if (statusCode == 30) // Succeeded
                {
                    progressRecord.PercentComplete = 100;
                    progressRecord.RecordType = ProgressRecordType.Completed;
                    WriteProgress(progressRecord);
                    WriteVerbose("Import completed successfully.");

                    // Output the import job ID
                    var result = new PSObject();
                    result.Properties.Add(new PSNoteProperty("ImportJobId", importJobId));
                    result.Properties.Add(new PSNoteProperty("AsyncOperationId", asyncOperationId));
                    result.Properties.Add(new PSNoteProperty("Status", "Succeeded"));
                    WriteObject(result);
                    return;
                }
                else if (statusCode == 31) // Failed
                {
                    progressRecord.RecordType = ProgressRecordType.Completed;
                    WriteProgress(progressRecord);
                    var errorMessage = (friendlyMessage ?? "Unknown error") + " - " + message;
                    ThrowTerminatingError(new ErrorRecord(
                        new InvalidOperationException($"Solution import failed: {errorMessage}"),
                        "ImportFailed",
                        ErrorCategory.InvalidOperation,
                        null));
                    return;
                }
                else if (statusCode == 32) // Canceled
                {
                    progressRecord.RecordType = ProgressRecordType.Completed;
                    WriteProgress(progressRecord);
                    WriteWarning("Solution import was canceled.");
                    return;
                }
                else
                {
                    // In progress - progress is set from importjob above
                    WriteProgress(progressRecord);
                }

                // Wait before polling again
                Thread.Sleep(pollingInterval);
            }
        }

        private bool DoesSolutionExist(byte[] solutionBytes)
        {
            try
            {
                // Extract the solution unique name from customizations.xml inside the zip
                var (solutionUniqueName, _, _) = ExtractSolutionInfo(solutionBytes);

                WriteVerbose($"Checking if solution '{solutionUniqueName}' exists in target environment...");

                var query = new QueryExpression("solution")
                {
                    ColumnSet = new ColumnSet("uniquename"),
                    Criteria = new FilterExpression
                    {
                        Conditions =
                        {
                            new ConditionExpression("uniquename", ConditionOperator.Equal, solutionUniqueName)
                        }
                    },
                    TopCount = 1
                };

                var solutions = Connection.RetrieveMultiple(query);

                bool exists = solutions.Entities.Count > 0;
                WriteVerbose($"Solution '{solutionUniqueName}' {(exists ? "exists" : "does not exist")} in target environment.");

                return exists;
            }
            catch (Exception ex)
            {
                WriteVerbose($"Error checking for existing solution: {ex.Message}");
                // If we can't determine, assume it doesn't exist and do regular import
                return false;
            }
        }

        private (string UniqueName, bool IsManaged, Version Version) ExtractSolutionInfo(byte[] solutionBytes)
        {
                using (var memoryStream = new MemoryStream(solutionBytes))
                using (var archive = new ZipArchive(memoryStream, ZipArchiveMode.Read))
                {
                    // Find the solution.xml file in the solution
                    var solutionXmlEntry = archive.Entries.FirstOrDefault(e =>
                        e.FullName.Equals("solution.xml", StringComparison.OrdinalIgnoreCase));

                    if (solutionXmlEntry != null)
                    {
                        using (var stream = solutionXmlEntry.Open())
                        using (var reader = new StreamReader(stream))
                        {
                            var xmlContent = reader.ReadToEnd();
                            var xdoc = XDocument.Parse(xmlContent);

                            // Navigate to the SolutionManifest element
                            var solutionManifest = xdoc.Root.Element("SolutionManifest");
                            if (solutionManifest == null)
                            {
                                ThrowTerminatingError(new ErrorRecord(new Exception("SolutionManifest element not found in solution.xml"), "SolutionManifestNotFound", ErrorCategory.InvalidData, null));
                                return (null, false, null);
                            }

                            // Extract the UniqueName from the SolutionManifest
                            var uniqueNameElement = solutionManifest.Element("UniqueName");

                            string uniqueName = null;
                            if (uniqueNameElement == null)
                            {
                                ThrowTerminatingError(new ErrorRecord(new Exception("UniqueName element not found in solution.xml"), "UniqueNameNotFound", ErrorCategory.InvalidData, null));
                                return (null, false, null);
                            }
                            uniqueName = uniqueNameElement.Value;

                            // Extract the Managed flag from the SolutionManifest
                            var managedElement = solutionManifest.Element("Managed");

                            bool isManaged = false;
                            if (managedElement != null && !string.IsNullOrEmpty(managedElement.Value))
                            {
                                isManaged = managedElement.Value == "1";
                                WriteVerbose($"Solution is {(isManaged ? "managed" : "unmanaged")}");
                            }
                            else
                            {
                                ThrowTerminatingError(new ErrorRecord(new Exception("Could not determine if solution is managed, assuming unmanaged"), "SolutionManagedStatusUnknown", ErrorCategory.InvalidData, null));
                            }

                            // Extract the Version from the SolutionManifest
                            var versionElement = solutionManifest.Element("Version");
                            Version version = null;
                            if (versionElement != null && !string.IsNullOrEmpty(versionElement.Value))
                            {
                                Version.TryParse(versionElement.Value, out version);
                            }

                            return (uniqueName, isManaged, version);
                        }
                    }
                }

            return (null, false, null);
        }

        private void ValidateSolutionComponents(byte[] solutionBytes)
        {
            WriteVerbose("Validating solution components...");

            // Extract connection references and environment variables from the solution
            var solutionComponents = ExtractSolutionComponents(solutionBytes);

            // Validate connection references if not skipped
            if (!SkipConnectionReferenceValidation.IsPresent)
            {
                ValidateConnectionReferences(solutionComponents.ConnectionReferences);
            }

            // Validate environment variables if not skipped
            if (!SkipEnvironmentVariableValidation.IsPresent)
            {
                ValidateEnvironmentVariables(solutionComponents.EnvironmentVariables);
            }
        }

        private (List<string> ConnectionReferences, List<string> EnvironmentVariables) ExtractSolutionComponents(byte[] solutionBytes)
        {
            var connectionReferences = new List<string>();
            var environmentVariables = new List<string>();

            try
            {
                using (var memoryStream = new MemoryStream(solutionBytes))
                using (var archive = new ZipArchive(memoryStream, ZipArchiveMode.Read))
                {
                    // Find the customizations.xml file in the solution
                    var customizationsEntry = archive.Entries.FirstOrDefault(e =>
                        e.FullName.Equals("customizations.xml", StringComparison.OrdinalIgnoreCase));

                    if (customizationsEntry != null)
                    {
                        using (var stream = customizationsEntry.Open())
                        using (var reader = new StreamReader(stream))
                        {
                            var xmlContent = reader.ReadToEnd();
                            var xdoc = XDocument.Parse(xmlContent);

                            // Extract connection references
                            // Connection references are stored in the solution XML with specific schema
                            var connRefElements = xdoc.Descendants()
                                .Where(e => e.Name.LocalName == "connectionreference");

                            foreach (var connRef in connRefElements)
                            {
                                var logicalName = connRef.Attribute("connectionreferencelogicalname")?.Value;
                                if (!string.IsNullOrEmpty(logicalName))
                                {
                                    connectionReferences.Add(logicalName);
                                    WriteVerbose($"Found connection reference in solution: {logicalName}");
                                }
                            }
                        }
                    }

                    // Extract environment variables from separate files
                    var envVarEntries = archive.Entries.Where(e =>
                        e.FullName.Contains("environmentvariabledefinitions/") &&
                        e.FullName.EndsWith("environmentvariabledefinition.xml", StringComparison.OrdinalIgnoreCase));

                    foreach (var entry in envVarEntries)
                    {
                        try
                        {
                            using (var stream = entry.Open())
                            using (var reader = new StreamReader(stream))
                            {
                                var xmlContent = reader.ReadToEnd();
                                var xdoc = XDocument.Parse(xmlContent);

                                // Get the schemaname from the root element attribute
                                var root = xdoc.Root;
                                var schemaName = root?.Attribute("schemaname")?.Value;

                                if (!string.IsNullOrEmpty(schemaName))
                                {
                                    environmentVariables.Add(schemaName);
                                    WriteVerbose($"Found environment variable in solution: {schemaName}");
                                }
                            }
                        }
                        catch (Exception ex)
                        {
                            WriteVerbose($"Error parsing environment variable file {entry.FullName}: {ex.Message}");
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                WriteVerbose($"Error extracting solution components: {ex.Message}");
            }

            return (connectionReferences, environmentVariables);
        }

        private void ValidateConnectionReferences(List<string> requiredConnectionRefs)
        {
            if (requiredConnectionRefs == null || requiredConnectionRefs.Count == 0)
            {
                WriteVerbose("No connection references found in solution.");
                return;
            }

            WriteVerbose($"Validating {requiredConnectionRefs.Count} connection reference(s)...");

            var missingConnectionRefs = new List<string>();

            foreach (var connRefName in requiredConnectionRefs)
            {
                // Check if this connection reference is provided in the parameters
                bool isProvided = ConnectionReferences != null && ConnectionReferences.ContainsKey(connRefName);

                if (!isProvided)
                {
                    // Check if it exists in the target environment with a value
                    bool existsInTarget = CheckConnectionReferenceExistsInTarget(connRefName);

                    if (!existsInTarget)
                    {
                        missingConnectionRefs.Add(connRefName);
                        WriteVerbose($"Connection reference '{connRefName}' is not provided and does not exist in target environment.");
                    }
                    else
                    {
                        WriteVerbose($"Connection reference '{connRefName}' exists in target environment.");
                    }
                }
                else
                {
                    WriteVerbose($"Connection reference '{connRefName}' is provided in parameters.");
                }
            }

            if (missingConnectionRefs.Count > 0)
            {
                var errorMessage = new StringBuilder();
                errorMessage.AppendLine($"The following connection reference(s) are required but not provided:");
                foreach (var connRef in missingConnectionRefs)
                {
                    errorMessage.AppendLine($"  - {connRef}");
                }
                errorMessage.AppendLine();
                errorMessage.AppendLine("Please provide values using the -ConnectionReferences parameter, or use -SkipConnectionReferenceValidation to skip this check.");

                ThrowTerminatingError(new ErrorRecord(
                    new InvalidOperationException(errorMessage.ToString()),
                    "MissingConnectionReferences",
                    ErrorCategory.InvalidArgument,
                    ConnectionReferences));
            }
        }

        private void ValidateEnvironmentVariables(List<string> requiredEnvVars)
        {
            if (requiredEnvVars == null || requiredEnvVars.Count == 0)
            {
                WriteVerbose("No environment variables found in solution.");
                return;
            }

            WriteVerbose($"Validating {requiredEnvVars.Count} environment variable(s)...");

            var missingEnvVars = new List<string>();

            foreach (var envVarName in requiredEnvVars)
            {
                // Check if this environment variable is provided in the parameters
                bool isProvided = EnvironmentVariables != null && EnvironmentVariables.ContainsKey(envVarName);

                if (!isProvided)
                {
                    // Check if it exists in the target environment with a value
                    bool existsInTarget = CheckEnvironmentVariableExistsInTarget(envVarName);

                    if (!existsInTarget)
                    {
                        missingEnvVars.Add(envVarName);
                        WriteVerbose($"Environment variable '{envVarName}' is not provided and does not exist in target environment.");
                    }
                    else
                    {
                        WriteVerbose($"Environment variable '{envVarName}' exists in target environment.");
                    }
                }
                else
                {
                    WriteVerbose($"Environment variable '{envVarName}' is provided in parameters.");
                }
            }

            if (missingEnvVars.Count > 0)
            {
                var errorMessage = new StringBuilder();
                errorMessage.AppendLine($"The following environment variable(s) are required but not provided:");
                foreach (var envVar in missingEnvVars)
                {
                    errorMessage.AppendLine($"  - {envVar}");
                }
                errorMessage.AppendLine();
                errorMessage.AppendLine("Please provide values using the -EnvironmentVariables parameter, or use -SkipEnvironmentVariableValidation to skip this check.");

                ThrowTerminatingError(new ErrorRecord(
                    new InvalidOperationException(errorMessage.ToString()),
                    "MissingEnvironmentVariables",
                    ErrorCategory.InvalidArgument,
                    EnvironmentVariables));
            }
        }

        private bool CheckConnectionReferenceExistsInTarget(string connectionRefLogicalName)
        {
            var query = new QueryExpression("connectionreference")
            {
                ColumnSet = new ColumnSet("connectionreferencelogicalname", "connectionid"),
                Criteria = new FilterExpression
                {
                    Conditions =
                        {
                            new ConditionExpression("connectionreferencelogicalname", ConditionOperator.Equal, connectionRefLogicalName)
                        }
                },
                TopCount = 1
            };

            var results = Connection.RetrieveMultiple(query);

            if (results.Entities.Count > 0)
            {
                var connRef = results.Entities[0];
                var connectionId = connRef.GetAttributeValue<string>("connectionid");
                // Connection reference exists and has a value set
                return !string.IsNullOrEmpty(connectionId);
            }

            return false;
        }

        private bool CheckEnvironmentVariableExistsInTarget(string envVarSchemaName)
        {
            // Query for environment variable definition
            var defQuery = new QueryExpression("environmentvariabledefinition")
            {
                ColumnSet = new ColumnSet("environmentvariabledefinitionid", "schemaname"),
                Criteria = new FilterExpression
                {
                    Conditions =
                        {
                            new ConditionExpression("schemaname", ConditionOperator.Equal, envVarSchemaName)
                        }
                },
                TopCount = 1
            };

            var defResults = Connection.RetrieveMultiple(defQuery);

            if (defResults.Entities.Count > 0)
            {
                var envVarDef = defResults.Entities[0];
                var envVarDefId = envVarDef.Id;

                // Check if there's a value set for this environment variable
                var valueQuery = new QueryExpression("environmentvariablevalue")
                {
                    ColumnSet = new ColumnSet("value"),
                    Criteria = new FilterExpression
                    {
                        Conditions =
                            {
                                new ConditionExpression("environmentvariabledefinitionid", ConditionOperator.Equal, envVarDefId)
                            }
                    },
                    TopCount = 1
                };

                var valueResults = Connection.RetrieveMultiple(valueQuery);

                if (valueResults.Entities.Count > 0)
                {
                    var envVarValue = valueResults.Entities[0];
                    var value = envVarValue.GetAttributeValue<string>("value");
                    // Environment variable exists and has a value set
                    return !string.IsNullOrEmpty(value);
                }
            }

            return false;
        }

        private Dictionary<string, Guid> GetExistingEnvironmentVariableValueIds(List<string> schemaNames)
        {
            var result = new Dictionary<string, Guid>();

            if (schemaNames == null || schemaNames.Count == 0)
            {
                return result;
            }

            WriteVerbose($"Querying for existing environment variable values for {schemaNames.Count} schema name(s)...");

            // Query for environment variable values by schema name
            var query = new QueryExpression("environmentvariablevalue")
            {
                ColumnSet = new ColumnSet("environmentvariablevalueid", "schemaname"),
                Criteria = new FilterExpression
                {
                    Conditions =
                        {
                            new ConditionExpression("schemaname", ConditionOperator.In, schemaNames.ToArray())
                        }
                }
            };

            var allResults = new List<Entity>();
            EntityCollection ec;
            do
            {
                ec = Connection.RetrieveMultiple(query);
                allResults.AddRange(ec.Entities);
                if (ec.MoreRecords)
                {
                    query.PageInfo.PageNumber++;
                    query.PageInfo.PagingCookie = ec.PagingCookie;
                }
            } while (ec.MoreRecords);

            foreach (var entity in allResults)
            {
                if (entity.Contains("schemaname"))
                {
                    var schemaName = entity.GetAttributeValue<string>("schemaname");
                    result[schemaName] = entity.Id;
                    WriteVerbose($"  Found existing value for '{schemaName}': {entity.Id}");
                }
            }

            WriteVerbose($"Found {result.Count} existing environment variable value(s).");
            return result;

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

        private string ExtractSolutionName(byte[] solutionBytes)
        {
            using (var memoryStream = new MemoryStream(solutionBytes))
            using (var archive = new ZipArchive(memoryStream, ZipArchiveMode.Read))
            {
                var solutionXmlEntry = archive.Entries.FirstOrDefault(e =>
                   e.FullName.Equals("solution.xml", StringComparison.OrdinalIgnoreCase));

                if (solutionXmlEntry != null)
                {
                    using (var stream = solutionXmlEntry.Open())
                    using (var reader = new StreamReader(stream))
                    {
                        var xmlContent = reader.ReadToEnd();
                        var xdoc = XDocument.Parse(xmlContent);
                        var solutionManifest = xdoc.Root?.Element("SolutionManifest");
                        return solutionManifest?.Element("UniqueName")?.Value;
                    }
                }
            }
            return null;
        }

        private Version GetInstalledSolutionVersion(string solutionUniqueName)
        {
            try
            {
                var query = new QueryExpression("solution")
                {
                    ColumnSet = new ColumnSet("version"),
                    Criteria = new FilterExpression
                    {
                        Conditions =
                        {
                            new ConditionExpression("uniquename", ConditionOperator.Equal, solutionUniqueName)
                        }
                    },
                    TopCount = 1
                };

                var solutions = Connection.RetrieveMultiple(query);

                if (solutions.Entities.Count > 0)
                {
                    var versionString = solutions.Entities[0].GetAttributeValue<string>("version");
                    if (!string.IsNullOrEmpty(versionString) && Version.TryParse(versionString, out var version))
                    {
                        return version;
                    }
                }
            }
            catch (Exception ex)
            {
                WriteVerbose($"Error retrieving installed solution version: {ex.Message}");
            }

            return null;
        }

        private void CheckAndUpdateSolutionComponents(byte[] solutionBytes)
        {
            WriteVerbose("Checking and updating connection references and environment variables from solution...");

            // Extract connection references and environment variables from the solution
            var solutionComponents = ExtractSolutionComponents(solutionBytes);

            // If user provided values, filter to only those that are in the solution
            Dictionary<string, string> connectionReferencesToCheck = new Dictionary<string, string>();
            Dictionary<string, string> environmentVariablesToCheck = new Dictionary<string, string>();

            // Build lists of items to check - only those in the solution and provided by user
            if (ConnectionReferences != null && ConnectionReferences.Count > 0)
            {
                foreach (DictionaryEntry entry in ConnectionReferences)
                {
                    var connRefName = entry.Key.ToString();
                    var connectionId = entry.Value.ToString();

                    // Only check if it's in the solution
                    if (solutionComponents.ConnectionReferences.Contains(connRefName))
                    {
                        connectionReferencesToCheck[connRefName] = connectionId;
                    }
                    else
                    {
                        WriteVerbose($"Connection reference '{connRefName}' is not in the solution, skipping.");
                    }
                }
            }

            if (EnvironmentVariables != null && EnvironmentVariables.Count > 0)
            {
                foreach (DictionaryEntry entry in EnvironmentVariables)
                {
                    var envVarName = entry.Key.ToString();
                    var envVarValue = entry.Value.ToString();

                    // Only check if it's in the solution
                    if (solutionComponents.EnvironmentVariables.Contains(envVarName))
                    {
                        environmentVariablesToCheck[envVarName] = envVarValue;
                    }
                    else
                    {
                        WriteVerbose($"Environment variable '{envVarName}' is not in the solution, skipping.");
                    }
                }
            }

            // Update connection references if needed
            if (connectionReferencesToCheck.Count > 0)
            {
                WriteVerbose($"Checking {connectionReferencesToCheck.Count} connection reference(s) from solution...");
                UpdateConnectionReferencesIfDifferent(connectionReferencesToCheck);
            }

            // Update environment variables if needed
            if (environmentVariablesToCheck.Count > 0)
            {
                WriteVerbose($"Checking {environmentVariablesToCheck.Count} environment variable(s) from solution...");
                UpdateEnvironmentVariablesIfDifferent(environmentVariablesToCheck);
            }
        }

        private void UpdateConnectionReferencesIfDifferent(Dictionary<string, string> connectionReferencesToSet)
        {
            foreach (var kvp in connectionReferencesToSet)
            {
                var logicalName = kvp.Key;
                var desiredConnectionId = kvp.Value;

                WriteVerbose($"Checking connection reference '{logicalName}'...");

                // Query for the connection reference by logical name
                var query = new QueryExpression("connectionreference")
                {
                    ColumnSet = new ColumnSet("connectionreferenceid", "connectionreferencelogicalname", "connectionid"),
                    Criteria = new FilterExpression
                    {
                        Conditions =
                        {
                            new ConditionExpression("connectionreferencelogicalname", ConditionOperator.Equal, logicalName)
                        }
                    },
                    TopCount = 1
                };

                var results = Connection.RetrieveMultiple(query);

                if (results.Entities.Count == 0)
                {
                    WriteVerbose($"  Connection reference '{logicalName}' not found in target environment. Skipping update (will be created during solution import).");
                    continue;
                }

                var connRef = results.Entities[0];
                var connRefId = connRef.Id;
                var currentConnectionId = connRef.GetAttributeValue<string>("connectionid");

                WriteVerbose($"  Current connection ID: {currentConnectionId ?? "(none)"}");
                WriteVerbose($"  Desired connection ID: {desiredConnectionId}");

                // Update only if different
                if (desiredConnectionId != currentConnectionId)
                {
                    WriteVerbose($"  Connection reference '{logicalName}' has different value. Updating...");

                    var updateEntity = new Entity("connectionreference", connRefId);
                    updateEntity["connectionid"] = desiredConnectionId;

                    Connection.Update(updateEntity);
                    WriteVerbose($"  Successfully updated connection reference '{logicalName}'");
                }
                else
                {
                    WriteVerbose($"  Connection reference '{logicalName}' already has the desired value. No update needed.");
                }
            }
        }

        private void UpdateEnvironmentVariablesIfDifferent(Dictionary<string, string> environmentVariablesToSet)
        {
            // Query for existing environment variable values by schema name
            var existingEnvVarValuesBySchemaName = GetExistingEnvironmentVariableValueIds(environmentVariablesToSet.Keys.ToList());

            foreach (var kvp in environmentVariablesToSet)
            {
                var schemaName = kvp.Key;
                var desiredValue = kvp.Value;

                WriteVerbose($"Checking environment variable '{schemaName}'...");

                // Query for the environment variable definition by schema name
                var defQuery = new QueryExpression("environmentvariabledefinition")
                {
                    ColumnSet = new ColumnSet("environmentvariabledefinitionid", "schemaname"),
                    Criteria = new FilterExpression
                    {
                        Conditions =
                        {
                            new ConditionExpression("schemaname", ConditionOperator.Equal, schemaName)
                        }
                    },
                    TopCount = 1
                };

                var defResults = Connection.RetrieveMultiple(defQuery);

                if (defResults.Entities.Count == 0)
                {
                    WriteVerbose($"  Environment variable definition '{schemaName}' not found in target environment. Skipping update (will be created during solution import).");
                    continue;
                }

                var envVarDef = defResults.Entities[0];
                var envVarDefId = envVarDef.Id;

                // Check if there's an existing value record
                if (existingEnvVarValuesBySchemaName.TryGetValue(schemaName, out var existingValueId))
                {
                    // Query the current value
                    var valueQuery = new QueryExpression("environmentvariablevalue")
                    {
                        ColumnSet = new ColumnSet("value"),
                        Criteria = new FilterExpression
                        {
                            Conditions =
                            {
                                new ConditionExpression("environmentvariablevalueid", ConditionOperator.Equal, existingValueId)
                            }
                        },
                        TopCount = 1
                    };

                    var valueResults = Connection.RetrieveMultiple(valueQuery);
                    if (valueResults.Entities.Count > 0)
                    {
                        var currentValue = valueResults.Entities[0].GetAttributeValue<string>("value");
                        
                        WriteVerbose($"  Current value: {currentValue ?? "(none)"}");
                        WriteVerbose($"  Desired value: {desiredValue}");

                        // Update only if different
                        if (desiredValue != currentValue)
                        {
                            WriteVerbose($"  Environment variable '{schemaName}' has different value. Updating...");

                            var updateEntity = new Entity("environmentvariablevalue", existingValueId);
                            updateEntity["value"] = desiredValue;

                            Connection.Update(updateEntity);
                            WriteVerbose($"  Successfully updated environment variable value for '{schemaName}'");
                        }
                        else
                        {
                            WriteVerbose($"  Environment variable '{schemaName}' already has the desired value. No update needed.");
                        }
                    }
                }
                else
                {
                    // No existing value record - create one
                    WriteVerbose($"  Environment variable '{schemaName}' has no value record. Creating...");

                    var createEntity = new Entity("environmentvariablevalue");
                    createEntity["schemaname"] = schemaName;
                    createEntity["value"] = desiredValue;
                    createEntity["environmentvariabledefinitionid"] = new EntityReference("environmentvariabledefinition", envVarDefId);

                    var newValueId = Connection.Create(createEntity);
                    WriteVerbose($"  Successfully created environment variable value for '{schemaName}' (ID: {newValueId})");
                }
            }
        }
    }
}
