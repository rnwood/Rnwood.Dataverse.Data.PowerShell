using System;
using System.IO;
using System.Management.Automation;
using System.Text.Json.Nodes;

namespace Rnwood.Dataverse.Data.PowerShell.Commands
{
    /// <summary>
    /// Gets data sources from a .msapp Canvas app file.
    /// </summary>
    [Cmdlet(VerbsCommon.Get, "DataverseMsAppDataSource")]
    [OutputType(typeof(PSObject))]
    public class GetDataverseMsAppDataSourceCmdlet : PSCmdlet
    {
        [Parameter(Mandatory = true, Position = 0, ParameterSetName = "FromPath", HelpMessage = "Path to the .msapp file")]
        [ValidateNotNullOrEmpty]
        public string MsAppPath { get; set; }

        [Parameter(Mandatory = true, Position = 0, ParameterSetName = "FromObject", ValueFromPipeline = true, HelpMessage = "Canvas app PSObject from Get-DataverseCanvasApp -IncludeDocument")]
        [ValidateNotNull]
        public PSObject CanvasApp { get; set; }

        [Parameter(HelpMessage = "Name pattern of data sources to retrieve. Supports wildcards (* and ?)")]
        public string DataSourceName { get; set; }

        protected override void ProcessRecord()
        {
            base.ProcessRecord();

            string tempMsappPath = null;
            string workMsappPath;

            try
            {
                if (ParameterSetName == "FromObject")
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
                    workMsappPath = tempMsappPath;
                }
                else
                {
                    workMsappPath = GetUnresolvedProviderPathFromPSPath(MsAppPath);
                    if (!File.Exists(workMsappPath))
                    {
                        ThrowTerminatingError(new ErrorRecord(
                            new FileNotFoundException($"MsApp file not found: {workMsappPath}"),
                            "FileNotFound",
                            ErrorCategory.ObjectNotFound,
                            workMsappPath));
                        return;
                    }
                }

                var tempUnpackDir = Path.Combine(Path.GetTempPath(), $"msapp_unpack_{Guid.NewGuid():N}");
                try
                {
                    MsAppPackagingHelper.UnpackMsApp(workMsappPath, tempUnpackDir);
                    var dataSources = MsAppToolkit.YamlFirstPackaging.GetDataSources(tempUnpackDir);

                    foreach (var ds in dataSources)
                    {
                        var name = ds["Name"]?.GetValue<string>() ?? string.Empty;

                        bool nameMatches = string.IsNullOrEmpty(DataSourceName) ||
                            (WildcardPattern.ContainsWildcardCharacters(DataSourceName)
                                ? new WildcardPattern(DataSourceName, WildcardOptions.IgnoreCase).IsMatch(name)
                                : string.Equals(name, DataSourceName, StringComparison.OrdinalIgnoreCase));

                        if (!nameMatches)
                        {
                            continue;
                        }

                        var obj = new PSObject();
                        obj.Properties.Add(new PSNoteProperty("Name", name));
                        obj.Properties.Add(new PSNoteProperty("Type", ds["Type"]?.GetValue<string>() ?? string.Empty));
                        obj.Properties.Add(new PSNoteProperty("LogicalName", ds["LogicalName"]?.GetValue<string>()));
                        obj.Properties.Add(new PSNoteProperty("DatasetName", ds["DatasetName"]?.GetValue<string>()));
                        obj.Properties.Add(new PSNoteProperty("IsWritable", ds["IsWritable"] is JsonValue writableVal ? writableVal.GetValue<bool>() : (bool?)null));
                        obj.Properties.Add(new PSNoteProperty("IsSampleData", ds["IsSampleData"] is JsonValue sampleVal ? sampleVal.GetValue<bool>() : (bool?)null));
                        WriteObject(obj);
                    }
                }
                finally
                {
                    if (Directory.Exists(tempUnpackDir))
                    {
                        try { Directory.Delete(tempUnpackDir, true); } catch { /* Ignore cleanup errors */ }
                    }
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
    }
}
