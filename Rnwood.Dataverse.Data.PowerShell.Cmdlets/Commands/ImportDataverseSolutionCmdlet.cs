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

namespace Rnwood.Dataverse.Data.PowerShell.Commands
{
    /// <summary>
    /// Imports a solution to Dataverse using an asynchronous job with progress reporting.
    /// </summary>
    [Cmdlet(VerbsData.Import, "DataverseSolution", SupportsShouldProcess = true, ConfirmImpact = ConfirmImpact.High, DefaultParameterSetName = "FromFile")]
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
        /// Gets or sets whether to import as a holding solution staged for upgrade.
        /// </summary>
        [Parameter(HelpMessage = "Import the solution as a holding solution staged for upgrade. Automatically falls back to regular import if solution doesn't exist.")]
        public SwitchParameter HoldingSolution { get; set; }

        /// <summary>
        /// Gets or sets connection references as a hashtable.
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
        /// Gets or sets the polling interval in seconds for checking job status. Default is 5 seconds.
        /// </summary>
        [Parameter(HelpMessage = "Polling interval in seconds for checking job status. Default is 5.")]
        public int PollingIntervalSeconds { get; set; } = 5;

        /// <summary>
        /// Gets or sets the timeout in seconds for the import operation. Default is 1800 seconds (30 minutes).
        /// </summary>
        [Parameter(HelpMessage = "Timeout in seconds for the import operation. Default is 1800 (30 minutes).")]
        public int TimeoutSeconds { get; set; } = 1800;

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

                WriteVerbose($"Loading solution file from: {filePath}");
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

            // Check if this is an upgrade scenario and if the solution already exists
            bool shouldFallbackToRegularImport = false;
            if (HoldingSolution.IsPresent)
            {
                // Extract solution unique name from the solution file (this is a simplified approach)
                // In a real scenario, you might want to parse the solution XML
                WriteVerbose("HoldingSolution specified - checking if solution already exists...");
                
                // Try to detect if solution exists by attempting to query for it
                // We'll catch the exception if it doesn't exist and fallback
                shouldFallbackToRegularImport = !DoesSolutionExist(solutionBytes);
                
                if (shouldFallbackToRegularImport)
                {
                    WriteWarning("Solution does not exist in the target environment. Falling back to regular import instead of upgrade.");
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
                    foreach (DictionaryEntry entry in EnvironmentVariables)
                    {
                        var envVarName = entry.Key.ToString();
                        var envVarValue = entry.Value.ToString();

                        WriteVerbose($"  Setting environment variable '{envVarName}' to value '{envVarValue}'");

                        var componentParam = new Entity("environmentvariablevalue");
                        componentParam["schemaname"] = envVarName;
                        componentParam["value"] = envVarValue;
                        
                        componentParameters.Entities.Add(componentParam);
                    }
                }
            }

            // Create the async import request
            var importRequest = new ImportSolutionAsyncRequest
            {
                CustomizationFile = solutionBytes,
                OverwriteUnmanagedCustomizations = OverwriteUnmanagedCustomizations.IsPresent,
                PublishWorkflows = PublishWorkflows.IsPresent,
                SkipProductUpdateDependencies = SkipProductUpdateDependencies.IsPresent,
                HoldingSolution = HoldingSolution.IsPresent && !shouldFallbackToRegularImport,
                ConvertToManaged = ConvertToManaged.IsPresent,
                SkipQueueRibbonJob = SkipQueueRibbonJob.IsPresent,
                AsyncRibbonProcessing = AsyncRibbonProcessing.IsPresent,
                ComponentParameters = componentParameters
            };

            if (LayerDesiredOrder != null)
            {
                importRequest.LayerDesiredOrder = LayerDesiredOrder;
            }

            WriteVerbose($"Starting async import (HoldingSolution={importRequest.HoldingSolution}, OverwriteUnmanagedCustomizations={importRequest.OverwriteUnmanagedCustomizations})");

            // Execute the async import request
            var importResponse = (ImportSolutionAsyncResponse)Connection.Execute(importRequest);
            var importJobId = (Guid)importResponse.Results["ImportJobId"];
            var asyncOperationId = (Guid)importResponse.Results["AsyncOperationId"];

            WriteVerbose($"Import job started. ImportJobId: {importJobId}, AsyncOperationId: {asyncOperationId}");

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

                WriteVerbose($"Import status: {statusDescription} (StatusCode={statusCode})");

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
                    var errorMessage = friendlyMessage ?? message ?? "Unknown error";
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
        }

        private bool DoesSolutionExist(byte[] solutionBytes)
        {
            try
            {
                // Extract the solution unique name from customizations.xml inside the zip
                string solutionUniqueName = ExtractSolutionUniqueName(solutionBytes);
                
                if (string.IsNullOrEmpty(solutionUniqueName))
                {
                    WriteVerbose("Could not extract solution unique name from ZIP. Assuming solution doesn't exist.");
                    return false;
                }

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

        private string ExtractSolutionUniqueName(byte[] solutionBytes)
        {
            try
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

                            // Extract the UniqueName from the solution XML
                            var uniqueNameElement = xdoc.Descendants()
                                .FirstOrDefault(e => e.Name.LocalName == "UniqueName");
                            
                            if (uniqueNameElement != null)
                            {
                                var uniqueName = uniqueNameElement.Value;
                                WriteVerbose($"Extracted solution unique name: {uniqueName}");
                                return uniqueName;
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                WriteVerbose($"Error extracting solution unique name: {ex.Message}");
            }

            return null;
        }

        private void ValidateSolutionComponents(byte[] solutionBytes)
        {
            try
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
            catch (Exception ex)
            {
                WriteVerbose($"Error during solution component validation: {ex.Message}");
                // If we can't parse the solution, we'll let the import proceed and let Dataverse handle any issues
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

                            // Extract environment variables
                            // Environment variables are stored with their schema names
                            var envVarElements = xdoc.Descendants()
                                .Where(e => e.Name.LocalName == "environmentvariabledefinition");
                            
                            foreach (var envVar in envVarElements)
                            {
                                var schemaName = envVar.Element(XName.Get("schemaname", envVar.Name.NamespaceName))?.Value;
                                if (!string.IsNullOrEmpty(schemaName))
                                {
                                    environmentVariables.Add(schemaName);
                                    WriteVerbose($"Found environment variable in solution: {schemaName}");
                                }
                            }
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
            try
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
                    var connectionId = connRef.GetAttributeValue<Guid?>("connectionid");
                    // Connection reference exists and has a value set
                    return connectionId.HasValue && connectionId.Value != Guid.Empty;
                }

                return false;
            }
            catch (Exception ex)
            {
                WriteVerbose($"Error checking connection reference '{connectionRefLogicalName}': {ex.Message}");
                // If we can't query, assume it doesn't exist
                return false;
            }
        }

        private bool CheckEnvironmentVariableExistsInTarget(string envVarSchemaName)
        {
            try
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
            catch (Exception ex)
            {
                WriteVerbose($"Error checking environment variable '{envVarSchemaName}': {ex.Message}");
                // If we can't query, assume it doesn't exist
                return false;
            }
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
