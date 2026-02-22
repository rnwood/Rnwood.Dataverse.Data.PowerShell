using Microsoft.PowerPlatform.Dataverse.Client;
using System;
using System.IO;
using System.Management.Automation;

namespace Rnwood.Dataverse.Data.PowerShell.Commands
{
    /// <summary>
    /// Adds or updates a data source in a .msapp Canvas app file.
    /// Supports Dataverse table data sources (using an existing connection) and static collection data sources (from a JSON example).
    /// </summary>
    [Cmdlet(VerbsCommon.Set, "DataverseMsAppDataSource", SupportsShouldProcess = true, ConfirmImpact = ConfirmImpact.Medium)]
    public class SetDataverseMsAppDataSourceCmdlet : PSCmdlet
    {
        [Parameter(Mandatory = true, Position = 0, ParameterSetName = "Dataverse-FromPath", HelpMessage = "Path to the .msapp file")]
        [Parameter(Mandatory = true, Position = 0, ParameterSetName = "Collection-FromPath", HelpMessage = "Path to the .msapp file")]
        [ValidateNotNullOrEmpty]
        public string MsAppPath { get; set; }

        [Parameter(Mandatory = true, Position = 0, ParameterSetName = "Dataverse-FromObject", ValueFromPipeline = true, HelpMessage = "Canvas app PSObject from Get-DataverseCanvasApp -IncludeDocument")]
        [Parameter(Mandatory = true, Position = 0, ParameterSetName = "Collection-FromObject", ValueFromPipeline = true, HelpMessage = "Canvas app PSObject from Get-DataverseCanvasApp -IncludeDocument")]
        [ValidateNotNull]
        public PSObject CanvasApp { get; set; }

        [Parameter(Mandatory = true, Position = 1, ParameterSetName = "Dataverse-FromPath", HelpMessage = "Logical name of the Dataverse table to add as a data source")]
        [Parameter(Mandatory = true, Position = 1, ParameterSetName = "Dataverse-FromObject", HelpMessage = "Logical name of the Dataverse table to add as a data source")]
        [ValidateNotNullOrEmpty]
        public string TableLogicalName { get; set; }

        [Parameter(Mandatory = true, Position = 1, ParameterSetName = "Collection-FromPath", HelpMessage = "Name of the collection data source")]
        [Parameter(Mandatory = true, Position = 1, ParameterSetName = "Collection-FromObject", HelpMessage = "Name of the collection data source")]
        [ValidateNotNullOrEmpty]
        public string DataSourceName { get; set; }

        [Parameter(Mandatory = true, Position = 2, ParameterSetName = "Collection-FromPath", HelpMessage = "Path to a JSON file containing example data for the collection")]
        [Parameter(Mandatory = true, Position = 2, ParameterSetName = "Collection-FromObject", HelpMessage = "Path to a JSON file containing example data for the collection")]
        [ValidateNotNullOrEmpty]
        public string JsonExamplePath { get; set; }

        [Parameter(Mandatory = false, ParameterSetName = "Dataverse-FromPath", HelpMessage = "DataverseConnection instance obtained from Get-DataverseConnection cmdlet. If not provided, uses the default connection.")]
        [Parameter(Mandatory = false, ParameterSetName = "Dataverse-FromObject", HelpMessage = "DataverseConnection instance obtained from Get-DataverseConnection cmdlet. If not provided, uses the default connection.")]
        public ServiceClient Connection { get; set; }

        [Parameter(Mandatory = false, ParameterSetName = "Dataverse-FromPath", HelpMessage = "Optional display name to use for the data source. Defaults to the table display name.")]
        [Parameter(Mandatory = false, ParameterSetName = "Dataverse-FromObject", HelpMessage = "Optional display name to use for the data source. Defaults to the table display name.")]
        public string DisplayName { get; set; }

        [Parameter(Mandatory = false, ParameterSetName = "Dataverse-FromPath", HelpMessage = "Optional dataset name. Defaults to 'default.cds'.")]
        [Parameter(Mandatory = false, ParameterSetName = "Dataverse-FromObject", HelpMessage = "Optional dataset name. Defaults to 'default.cds'.")]
        public string DatasetName { get; set; }

        protected override void BeginProcessing()
        {
            base.BeginProcessing();

            if (ParameterSetName.StartsWith("Dataverse-", StringComparison.Ordinal) && Connection == null)
            {
                Connection = DefaultConnectionManager.DefaultConnection;
                if (Connection == null)
                {
                    ThrowTerminatingError(new ErrorRecord(
                        new InvalidOperationException("No connection provided and no default connection is set. Either provide a -Connection parameter or set a default connection using: Get-DataverseConnection -SetAsDefault <parameters>"),
                        "NoConnection",
                        ErrorCategory.InvalidOperation,
                        null));
                }
            }
        }

        protected override void ProcessRecord()
        {
            base.ProcessRecord();

            WriteWarning("YAML-first Canvas app modification is experimental. The Power Apps YAML format may change between releases and the results may need to be validated in Power Apps Studio.");

            bool isFromObject = ParameterSetName.Contains("FromObject");
            string targetPath = null;
            string tempMsappPath = null;

            try
            {
                if (isFromObject)
                {
                    var documentProp = CanvasApp.Properties["document"];
                    if (documentProp == null || documentProp.Value == null)
                    {
                        ThrowTerminatingError(new ErrorRecord(
                            new InvalidOperationException("CanvasApp object does not have a 'document' property. Use Get-DataverseCanvasApp with -IncludeDocument."),
                            "DocumentPropertyMissing",
                            ErrorCategory.InvalidArgument,
                            CanvasApp));
                        return;
                    }

                    tempMsappPath = Path.Combine(Path.GetTempPath(), $"msapp_{Guid.NewGuid():N}.msapp");
                    File.WriteAllBytes(tempMsappPath, Convert.FromBase64String(documentProp.Value.ToString()));
                    targetPath = tempMsappPath;
                }
                else
                {
                    targetPath = GetUnresolvedProviderPathFromPSPath(MsAppPath);
                    if (!File.Exists(targetPath))
                    {
                        ThrowTerminatingError(new ErrorRecord(
                            new FileNotFoundException($"MsApp file not found: {targetPath}"),
                            "FileNotFound",
                            ErrorCategory.ObjectNotFound,
                            targetPath));
                        return;
                    }
                }

                bool isDataverse = ParameterSetName.StartsWith("Dataverse-", StringComparison.Ordinal);
                string action = isDataverse
                    ? $"Set Dataverse table data source '{TableLogicalName}' in {(isFromObject ? "Canvas app" : $".msapp file '{targetPath}'")}"
                    : $"Set collection data source '{DataSourceName}' in {(isFromObject ? "Canvas app" : $".msapp file '{targetPath}'")}";

                if (!ShouldProcess(action, action, "Set Data Source"))
                {
                    return;
                }

                WriteVerbose("Using YAML-first packaging to update data sources in msapp");

                if (isDataverse)
                {
                    string environmentUrl = Connection.ConnectedOrgUriActual?.ToString()
                        ?? throw new InvalidOperationException("Cannot determine environment URL from connection.");
                    string accessToken = GetAccessToken();

                    MsAppPackagingHelper.ModifyMsApp(targetPath, unpackDir =>
                    {
                        MsAppToolkit.YamlFirstPackaging.UpsertDataverseTableDataSourceAsync(
                            unpackDir,
                            environmentUrl,
                            TableLogicalName,
                            accessToken,
                            DisplayName,
                            DatasetName).GetAwaiter().GetResult();
                    });

                    WriteVerbose($"Dataverse table data source '{TableLogicalName}' set successfully");
                }
                else
                {
                    var jsonExampleResolved = GetUnresolvedProviderPathFromPSPath(JsonExamplePath);
                    if (!File.Exists(jsonExampleResolved))
                    {
                        ThrowTerminatingError(new ErrorRecord(
                            new FileNotFoundException($"JSON example file not found: {jsonExampleResolved}"),
                            "FileNotFound",
                            ErrorCategory.ObjectNotFound,
                            jsonExampleResolved));
                        return;
                    }

                    MsAppPackagingHelper.ModifyMsApp(targetPath, unpackDir =>
                    {
                        MsAppToolkit.YamlFirstPackaging.GenerateCollectionDataSourceFromJson(
                            unpackDir, DataSourceName, jsonExampleResolved);
                    });

                    WriteVerbose($"Collection data source '{DataSourceName}' set successfully");
                }

                if (isFromObject)
                {
                    byte[] modifiedBytes = File.ReadAllBytes(tempMsappPath);
                    CanvasApp.Properties["document"].Value = Convert.ToBase64String(modifiedBytes);
                }
            }
            finally
            {
                if (tempMsappPath != null && File.Exists(tempMsappPath))
                {
                    try { File.Delete(tempMsappPath); } catch { /* Ignore cleanup errors */ }
                }
            }
        }

        private string GetAccessToken()
        {
            if (Connection is ServiceClientWithTokenProvider clientWithProvider && clientWithProvider.TokenProviderFunction != null)
            {
                try
                {
                    var tokenTask = clientWithProvider.TokenProviderFunction(
                        Connection.ConnectedOrgUriActual?.ToString() ?? string.Empty);
                    return tokenTask.GetAwaiter().GetResult();
                }
                catch
                {
                    // Fall through to reflection approach
                }
            }

            try
            {
                var prop = Connection.GetType().GetProperty("CurrentAccessToken",
                    System.Reflection.BindingFlags.Instance |
                    System.Reflection.BindingFlags.Public |
                    System.Reflection.BindingFlags.NonPublic);
                if (prop != null)
                {
                    var token = prop.GetValue(Connection) as string;
                    if (!string.IsNullOrEmpty(token))
                    {
                        return token;
                    }
                }
            }
            catch
            {
                // Fall through
            }

            throw new InvalidOperationException("Unable to retrieve access token from connection. Ensure you are authenticated.");
        }
    }
}
