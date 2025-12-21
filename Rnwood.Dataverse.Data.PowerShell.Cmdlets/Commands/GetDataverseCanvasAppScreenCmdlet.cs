using ICSharpCode.SharpZipLib.Zip;
using Microsoft.PowerPlatform.Dataverse.Client;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Query;
using System;
using System.IO;
using System.Linq;
using System.Management.Automation;

namespace Rnwood.Dataverse.Data.PowerShell.Commands
{
    /// <summary>
    /// Retrieves screens from a Canvas app's .msapp file.
    /// </summary>
    [Cmdlet(VerbsCommon.Get, "DataverseCanvasAppScreen")]
    [OutputType(typeof(PSObject))]
    public class GetDataverseCanvasAppScreenCmdlet : OrganizationServiceCmdlet
    {
        private const string PARAMSET_FROM_DATAVERSE = "FromDataverse";
        private const string PARAMSET_FROM_FILE = "FromFile";

        /// <summary>
        /// Gets or sets the Canvas app ID to retrieve screens from.
        /// </summary>
        [Parameter(ParameterSetName = PARAMSET_FROM_DATAVERSE, Mandatory = true, HelpMessage = "ID of the Canvas app to retrieve screens from")]
        public Guid CanvasAppId { get; set; }

        /// <summary>
        /// Gets or sets the Canvas app name to retrieve screens from.
        /// </summary>
        [Parameter(ParameterSetName = PARAMSET_FROM_DATAVERSE, HelpMessage = "Name of the Canvas app to retrieve screens from")]
        public string CanvasAppName { get; set; }

        /// <summary>
        /// Gets or sets the path to an .msapp file to read screens from.
        /// </summary>
        [Parameter(ParameterSetName = PARAMSET_FROM_FILE, Mandatory = true, HelpMessage = "Path to an .msapp file to read screens from")]
        public string MsAppPath { get; set; }

        /// <summary>
        /// Gets or sets the name pattern of screens to retrieve. Supports wildcards (* and ?).
        /// </summary>
        [Parameter(HelpMessage = "Name pattern of screens to retrieve. Supports wildcards (* and ?)")]
        public string ScreenName { get; set; }

        /// <inheritdoc />
        protected override void ProcessRecord()
        {
            base.ProcessRecord();

            byte[] msappBytes;

            if (ParameterSetName == PARAMSET_FROM_DATAVERSE)
            {
                // Retrieve .msapp file from Dataverse
                msappBytes = RetrieveMsAppFromDataverse();
            }
            else
            {
                // Read .msapp file from disk
                var filePath = GetUnresolvedProviderPathFromPSPath(MsAppPath);
                if (!File.Exists(filePath))
                {
                    ThrowTerminatingError(new ErrorRecord(
                        new FileNotFoundException($"MsApp file not found: {filePath}"),
                        "FileNotFound",
                        ErrorCategory.ObjectNotFound,
                        filePath));
                    return;
                }

                msappBytes = File.ReadAllBytes(filePath);
            }

            // Extract screens from .msapp file
            ExtractScreens(msappBytes);
        }

        private byte[] RetrieveMsAppFromDataverse()
        {
            Guid appId = CanvasAppId;

            if (appId == Guid.Empty && !string.IsNullOrEmpty(CanvasAppName))
            {
                // Find Canvas app by name
                var query = new QueryExpression("canvasapp")
                {
                    ColumnSet = new ColumnSet("canvasappid"),
                    Criteria = new FilterExpression()
                };
                query.Criteria.AddCondition("name", ConditionOperator.Equal, CanvasAppName);

                var results = Connection.RetrieveMultiple(query);
                var app = results.Entities.FirstOrDefault();

                if (app == null)
                {
                    ThrowTerminatingError(new ErrorRecord(
                        new Exception($"Canvas app '{CanvasAppName}' not found"),
                        "CanvasAppNotFound",
                        ErrorCategory.ObjectNotFound,
                        CanvasAppName));
                    return null;
                }

                appId = app.Id;
            }

            // Retrieve Canvas app with document
            var canvasApp = Connection.Retrieve("canvasapp", appId, new ColumnSet("document"));

            if (!canvasApp.Contains("document"))
            {
                WriteWarning("Canvas app does not contain a document (.msapp file)");
                return null;
            }

            // The document is stored as base64-encoded bytes
            string base64Document = canvasApp.GetAttributeValue<string>("document");
            if (string.IsNullOrEmpty(base64Document))
            {
                WriteWarning("Canvas app document is empty");
                return null;
            }

            return Convert.FromBase64String(base64Document);
        }

        private void ExtractScreens(byte[] msappBytes)
        {
            using (var memoryStream = new MemoryStream(msappBytes))
            using (var zipInputStream = new ZipInputStream(memoryStream))
            {
                ZipEntry entry;
                while ((entry = zipInputStream.GetNextEntry()) != null)
                {
                    // Screen YAML files are in the Src/ folder and end with .pa.yaml
                    // Typically named like Screen1.pa.yaml, Screen2.pa.yaml, etc.
                    // We skip App.pa.yaml and _EditorState.pa.yaml
                    if (entry.Name.StartsWith("Src/") && 
                        entry.Name.EndsWith(".pa.yaml") && 
                        !entry.Name.EndsWith("/App.pa.yaml") &&
                        !entry.Name.EndsWith("/_EditorState.pa.yaml"))
                    {
                        string screenName = Path.GetFileNameWithoutExtension(entry.Name);

                        // Apply name filter if specified
                        if (!string.IsNullOrEmpty(ScreenName))
                        {
                            if (!MatchesPattern(screenName, ScreenName))
                            {
                                continue;
                            }
                        }

                        // Read YAML content
                        byte[] entryBytes = ReadZipEntryBytes(zipInputStream);
                        string yamlContent = System.Text.Encoding.UTF8.GetString(entryBytes);

                        // Create PSObject with screen information
                        var psObject = new PSObject();
                        psObject.Properties.Add(new PSNoteProperty("ScreenName", screenName));
                        psObject.Properties.Add(new PSNoteProperty("FilePath", entry.Name));
                        psObject.Properties.Add(new PSNoteProperty("YamlContent", yamlContent));
                        psObject.Properties.Add(new PSNoteProperty("Size", entryBytes.Length));

                        WriteObject(psObject);
                    }
                }
            }
        }

        private byte[] ReadZipEntryBytes(ZipInputStream zipInputStream)
        {
            using (var entryStream = new MemoryStream())
            {
                byte[] buffer = new byte[4096];
                int count;
                while ((count = zipInputStream.Read(buffer, 0, buffer.Length)) > 0)
                {
                    entryStream.Write(buffer, 0, count);
                }
                return entryStream.ToArray();
            }
        }

        private bool MatchesPattern(string value, string pattern)
        {
            if (string.IsNullOrEmpty(pattern))
            {
                return true;
            }

            // Convert wildcard pattern to regex pattern
            string regexPattern = "^" + System.Text.RegularExpressions.Regex.Escape(pattern)
                .Replace("\\*", ".*")
                .Replace("\\?", ".") + "$";

            return System.Text.RegularExpressions.Regex.IsMatch(value, regexPattern, 
                System.Text.RegularExpressions.RegexOptions.IgnoreCase);
        }
    }
}
