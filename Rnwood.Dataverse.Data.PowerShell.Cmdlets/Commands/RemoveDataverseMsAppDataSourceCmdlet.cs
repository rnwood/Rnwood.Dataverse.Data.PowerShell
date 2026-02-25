using System;
using System.IO;
using System.Management.Automation;

namespace Rnwood.Dataverse.Data.PowerShell.Commands
{
    /// <summary>
    /// Removes a data source from a .msapp Canvas app file.
    /// </summary>
    [Cmdlet(VerbsCommon.Remove, "DataverseMsAppDataSource", SupportsShouldProcess = true, ConfirmImpact = ConfirmImpact.High)]
    public class RemoveDataverseMsAppDataSourceCmdlet : PSCmdlet
    {
        [Parameter(Mandatory = true, Position = 0, ParameterSetName = "FromPath", HelpMessage = "Path to the .msapp file")]
        [ValidateNotNullOrEmpty]
        public string MsAppPath { get; set; }

        [Parameter(Mandatory = true, Position = 0, ParameterSetName = "FromObject", ValueFromPipeline = true, HelpMessage = "Canvas app PSObject from Get-DataverseCanvasApp -IncludeDocument")]
        [ValidateNotNull]
        public PSObject CanvasApp { get; set; }

        [Parameter(Mandatory = true, Position = 1, HelpMessage = "Name of the data source to remove")]
        [ValidateNotNullOrEmpty]
        public string DataSourceName { get; set; }

        [Parameter(HelpMessage = "If set, no error is thrown if the data source does not exist")]
        public SwitchParameter IfExists { get; set; }

        protected override void ProcessRecord()
        {
            base.ProcessRecord();

            WriteWarning("YAML-first Canvas app modification is experimental. The Power Apps MSAPP format it not fully understood/supported and the results may need to be validated in Power Apps Studio.");

            bool isFromObject = ParameterSetName == "FromObject";
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

                string action = isFromObject
                    ? $"Remove data source '{DataSourceName}' from Canvas app"
                    : $"Remove data source '{DataSourceName}' from .msapp file '{targetPath}'";

                if (!ShouldProcess(action, action, "Remove Data Source"))
                {
                    return;
                }

                var removedResult = new bool[1];
                MsAppPackagingHelper.ModifyMsApp(targetPath, unpackDir =>
                {
                    removedResult[0] = MsAppToolkit.YamlFirstPackaging.RemoveDataSource(unpackDir, DataSourceName);
                });
                bool removed = removedResult[0];

                if (!removed && !IfExists.IsPresent)
                {
                    ThrowTerminatingError(new ErrorRecord(
                        new InvalidOperationException($"Data source '{DataSourceName}' was not found in the .msapp file."),
                        "DataSourceNotFound",
                        ErrorCategory.ObjectNotFound,
                        DataSourceName));
                    return;
                }

                if (isFromObject)
                {
                    byte[] modifiedBytes = File.ReadAllBytes(tempMsappPath);
                    CanvasApp.Properties["document"].Value = Convert.ToBase64String(modifiedBytes);
                }

                WriteVerbose(removed
                    ? $"Data source '{DataSourceName}' removed successfully"
                    : $"Data source '{DataSourceName}' not found (IfExists flag set, skipping)");
            }
            finally
            {
                if (tempMsappPath != null && File.Exists(tempMsappPath))
                {
                    try { File.Delete(tempMsappPath); } catch { /* Ignore cleanup errors */ }
                }
            }
        }
    }
}
