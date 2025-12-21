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
    /// Retrieves components from a Canvas app's .msapp file.
    /// </summary>
    [Cmdlet(VerbsCommon.Get, "DataverseCanvasAppComponent")]
    [OutputType(typeof(PSObject))]
    public class GetDataverseCanvasAppComponentCmdlet : OrganizationServiceCmdlet
    {
        private const string PARAMSET_FROM_DATAVERSE = "FromDataverse";
        private const string PARAMSET_FROM_FILE = "FromFile";

        /// <summary>
        /// Gets or sets the Canvas app ID to retrieve components from.
        /// </summary>
        [Parameter(ParameterSetName = PARAMSET_FROM_DATAVERSE, Mandatory = true, HelpMessage = "ID of the Canvas app to retrieve components from")]
        public Guid CanvasAppId { get; set; }

        /// <summary>
        /// Gets or sets the Canvas app name to retrieve components from.
        /// </summary>
        [Parameter(ParameterSetName = PARAMSET_FROM_DATAVERSE, HelpMessage = "Name of the Canvas app to retrieve components from")]
        public string CanvasAppName { get; set; }

        /// <summary>
        /// Gets or sets the path to an .msapp file to read components from.
        /// </summary>
        [Parameter(ParameterSetName = PARAMSET_FROM_FILE, Mandatory = true, HelpMessage = "Path to an .msapp file to read components from")]
        public string MsAppPath { get; set; }

        /// <summary>
        /// Gets or sets the name pattern of components to retrieve. Supports wildcards (* and ?).
        /// </summary>
        [Parameter(HelpMessage = "Name pattern of components to retrieve. Supports wildcards (* and ?)")]
        public string ComponentName { get; set; }

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

            // Extract components from .msapp file
            ExtractComponents(msappBytes);
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

        private void ExtractComponents(byte[] msappBytes)
        {
            using (var memoryStream = new MemoryStream(msappBytes))
            using (var zipInputStream = new ZipInputStream(memoryStream))
            {
                ZipEntry entry;
                while ((entry = zipInputStream.GetNextEntry()) != null)
                {
                    // Component YAML files are in the Src/ folder with naming pattern ComponentX.pa.yaml
                    // or in Components/ folder (newer Canvas app format)
                    // We need to identify components vs screens
                    // Components typically have "Component" in the name or are in Components/ folder
                    bool isComponent = false;
                    string componentName = null;

                    if (entry.Name.StartsWith("Components/") && entry.Name.EndsWith(".pa.yaml"))
                    {
                        // Components in Components/ folder
                        isComponent = true;
                        componentName = Path.GetFileNameWithoutExtension(entry.Name);
                        
                        // Apply name filter if specified
                        if (!string.IsNullOrEmpty(ComponentName))
                        {
                            if (!MatchesPattern(componentName, ComponentName))
                            {
                                continue;
                            }
                        }

                        // Read YAML content
                        byte[] entryBytes = ReadZipEntryBytes(zipInputStream);
                        string yamlContent = System.Text.Encoding.UTF8.GetString(entryBytes);

                        // Create PSObject with component information
                        var psObject = new PSObject();
                        psObject.Properties.Add(new PSNoteProperty("ComponentName", componentName));
                        psObject.Properties.Add(new PSNoteProperty("FilePath", entry.Name));
                        psObject.Properties.Add(new PSNoteProperty("YamlContent", yamlContent));
                        psObject.Properties.Add(new PSNoteProperty("Size", entryBytes.Length));

                        WriteObject(psObject);
                    }
                    else if (entry.Name.StartsWith("Src/") && 
                             entry.Name.EndsWith(".pa.yaml") &&
                             !entry.Name.EndsWith("/App.pa.yaml") &&
                             !entry.Name.EndsWith("/_EditorState.pa.yaml") &&
                             !entry.Name.Contains("/Screen"))
                    {
                        // Check if it's a component in Src/ folder (contains "Component" in YAML content)
                        string fileName = Path.GetFileNameWithoutExtension(entry.Name);
                        
                        // Read content to check if it's a component
                        byte[] entryBytes = ReadZipEntryBytes(zipInputStream);
                        string yamlContent = System.Text.Encoding.UTF8.GetString(entryBytes);

                        // Components start with "Component:" or "Components:" in YAML
                        if (yamlContent.TrimStart().StartsWith("Component:") || 
                            yamlContent.TrimStart().StartsWith("Components:"))
                        {
                            isComponent = true;
                            componentName = fileName;

                            // Apply name filter if specified
                            if (!string.IsNullOrEmpty(ComponentName))
                            {
                                if (!MatchesPattern(componentName, ComponentName))
                                {
                                    continue;
                                }
                            }

                            // Create PSObject with component information
                            var psObject = new PSObject();
                            psObject.Properties.Add(new PSNoteProperty("ComponentName", componentName));
                            psObject.Properties.Add(new PSNoteProperty("FilePath", entry.Name));
                            psObject.Properties.Add(new PSNoteProperty("YamlContent", yamlContent));
                            psObject.Properties.Add(new PSNoteProperty("Size", entryBytes.Length));

                            WriteObject(psObject);
                        }
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
