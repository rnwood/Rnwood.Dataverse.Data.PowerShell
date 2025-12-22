using ICSharpCode.SharpZipLib.Zip;
using Microsoft.Crm.Sdk.Messages;
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
    /// Retrieves Canvas apps from a Dataverse environment.
    /// </summary>
    [Cmdlet(VerbsCommon.Get, "DataverseCanvasApp")]
    [OutputType(typeof(PSObject))]
    public class GetDataverseCanvasAppCmdlet : OrganizationServiceCmdlet
    {
        private const string PARAMSET_QUERY = "Query";
        private const string PARAMSET_ID = "Id";

        /// <summary>
        /// Gets or sets the ID of the Canvas app to retrieve.
        /// </summary>
        [Parameter(ParameterSetName = PARAMSET_ID, Mandatory = true, HelpMessage = "ID of the Canvas app to retrieve")]
        public Guid Id { get; set; }

        /// <summary>
        /// Gets or sets the name or name pattern of the Canvas app to retrieve. Supports wildcards (* and ?).
        /// </summary>
        [Parameter(ParameterSetName = PARAMSET_QUERY, Position = 0, HelpMessage = "Name or name pattern of the Canvas app. Supports wildcards (* and ?)")]
        public string Name { get; set; }

        /// <summary>
        /// Gets or sets the display name or pattern to filter by. Supports wildcards (* and ?).
        /// </summary>
        [Parameter(ParameterSetName = PARAMSET_QUERY, HelpMessage = "Display name or pattern to filter by. Supports wildcards (* and ?)")]
        public string DisplayName { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether to filter to only unmanaged Canvas apps.
        /// </summary>
        [Parameter(ParameterSetName = PARAMSET_QUERY, HelpMessage = "If set, filters to only unmanaged Canvas apps")]
        public SwitchParameter Unmanaged { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether to include the document content (.msapp file) in the results.
        /// This will export the Canvas app via solution export to retrieve the .msapp file.
        /// </summary>
        [Parameter(HelpMessage = "If set, includes the document content (.msapp file bytes) in the results via solution export")]
        public SwitchParameter IncludeDocument { get; set; }

        /// <inheritdoc />
        protected override void ProcessRecord()
        {
            base.ProcessRecord();

            QueryExpression query;

            if (ParameterSetName == PARAMSET_ID)
            {
                query = new QueryExpression("canvasapp")
                {
                    ColumnSet = new ColumnSet(true),
                    Criteria = new FilterExpression()
                };
                query.Criteria.AddCondition("canvasappid", ConditionOperator.Equal, Id);
            }
            else
            {
                query = new QueryExpression("canvasapp");

                // Get all columns except we'll fetch document separately if needed
                query.ColumnSet = new ColumnSet(
                    "canvasappid",
                    "name",
                    "displayname",
                    "description",
                    "commitmessage",
                    "publisher",
                    "authorizationreferences",
                    "connectionreferences",
                    "databasereferences",
                    "appcomponents",
                    "appcomponentdependencies",
                    "status",
                    "tags",
                    "createdtime",
                    "lastmodifiedtime",
                    "iscustomizable",
                    "canvasapptype",
                    "bypassconsent",
                    "admincontrolbypassconsent",
                    "canconsumeapppass",
                    "iscdsupgraded",
                    "isfeaturedapp",
                    "isheroapp",
                    "cdsdependencies",
                    "minclientversion",
                    "createdbyclientversion",
                    "appversion"
                );

                query.Criteria = new FilterExpression();

                // Add filters based on parameters
                if (!string.IsNullOrEmpty(Name))
                {
                    if (Name.Contains("*") || Name.Contains("?"))
                    {
                        string likePattern = Name.Replace("*", "%").Replace("?", "_");
                        query.Criteria.AddCondition("name", ConditionOperator.Like, likePattern);
                        WriteVerbose($"Filtering by name pattern: {Name}");
                    }
                    else
                    {
                        query.Criteria.AddCondition("name", ConditionOperator.Equal, Name);
                        WriteVerbose($"Filtering by name: {Name}");
                    }
                }

                if (!string.IsNullOrEmpty(DisplayName))
                {
                    if (DisplayName.Contains("*") || DisplayName.Contains("?"))
                    {
                        string likePattern = DisplayName.Replace("*", "%").Replace("?", "_");
                        query.Criteria.AddCondition("displayname", ConditionOperator.Like, likePattern);
                        WriteVerbose($"Filtering by display name pattern: {DisplayName}");
                    }
                    else
                    {
                        query.Criteria.AddCondition("displayname", ConditionOperator.Equal, DisplayName);
                        WriteVerbose($"Filtering by display name: {DisplayName}");
                    }
                }

                if (Unmanaged.IsPresent)
                {
                    query.Criteria.AddCondition("ismanaged", ConditionOperator.Equal, false);
                    WriteVerbose("Filtering for unmanaged Canvas apps only");
                }
            }

            WriteVerbose("Retrieving Canvas apps...");
            var results = Connection.RetrieveMultiple(query);
            WriteVerbose($"Found {results.Entities.Count} Canvas app(s)");

            // Convert to PSObjects
            var metadataFactory = new EntityMetadataFactory(Connection);
            var converter = new DataverseEntityConverter(Connection, metadataFactory);

            foreach (var entity in results.Entities)
            {
                var psObject = converter.ConvertToPSObject(entity, query.ColumnSet, _ => ValueType.Display);

                // If IncludeDocument is specified, export via solution to get .msapp
                if (IncludeDocument.IsPresent)
                {
                    string appName = entity.GetAttributeValue<string>("name");
                    Guid appId = entity.Id;

                    WriteVerbose($"Exporting Canvas app '{appName}' to retrieve .msapp file...");
                    byte[] msappBytes = ExportCanvasAppDocument(appId, appName);

                    if (msappBytes != null)
                    {
                        // Add document as base64 string to match expected format
                        psObject.Properties.Add(new PSNoteProperty("document", Convert.ToBase64String(msappBytes)));
                        WriteVerbose($"Retrieved .msapp file ({msappBytes.Length} bytes)");
                    }
                    else
                    {
                        WriteWarning($"Could not retrieve .msapp file for Canvas app '{appName}'");
                    }
                }

                WriteObject(psObject);
            }
        }

        private byte[] ExportCanvasAppDocument(Guid canvasAppId, string canvasAppName)
        {
            try
            {
                // Create a temporary solution containing just this Canvas app
                string tempSolutionName = $"TempExport_{canvasAppName}_{Guid.NewGuid().ToString("N").Substring(0, 8)}";

                // Check if a solution already exists with this Canvas app
                // For simplicity, we'll create a temp solution, add the app, export, then delete
                WriteVerbose($"Creating temporary solution '{tempSolutionName}' for export...");

                // Create temporary solution
                var solution = new Entity("solution")
                {
                    ["uniquename"] = tempSolutionName,
                    ["friendlyname"] = "Temporary Export Solution",
                    ["version"] = "1.0.0.0",
                    ["publisherid"] = GetDefaultPublisher()
                };
                var solutionId = Connection.Create(solution);
                WriteVerbose($"Created temporary solution with ID: {solutionId}");

                try
                {
                    // Add Canvas app to solution
                    var addRequest = new AddSolutionComponentRequest
                    {
                        ComponentType = 300, // Canvas App component type
                        ComponentId = canvasAppId,
                        SolutionUniqueName = tempSolutionName
                    };
                    Connection.Execute(addRequest);
                    WriteVerbose("Added Canvas app to solution");

                    // Export solution
                    var exportRequest = new ExportSolutionRequest
                    {
                        SolutionName = tempSolutionName,
                        Managed = false
                    };
                    var exportResponse = (ExportSolutionResponse)Connection.Execute(exportRequest);
                    byte[] solutionBytes = exportResponse.ExportSolutionFile;
                    WriteVerbose($"Exported solution ({solutionBytes.Length} bytes)");

                    // Extract .msapp from the solution zip
                    byte[] msappBytes = ExtractMsAppFromSolution(solutionBytes, canvasAppName);

                    return msappBytes;
                }
                finally
                {
                    // Delete temporary solution
                    try
                    {
                        Connection.Delete("solution", solutionId);
                        WriteVerbose("Deleted temporary solution");
                    }
                    catch (Exception ex)
                    {
                        WriteWarning($"Failed to delete temporary solution: {ex.Message}");
                    }
                }
            }
            catch (Exception ex)
            {
                WriteWarning($"Failed to export Canvas app document: {ex.Message}");
                return null;
            }
        }

        private byte[] ExtractMsAppFromSolution(byte[] solutionBytes, string canvasAppName)
        {
            using (var memoryStream = new MemoryStream(solutionBytes))
            using (var zipInputStream = new ZipInputStream(memoryStream))
            {
                ZipEntry entry;
                while ((entry = zipInputStream.GetNextEntry()) != null)
                {
                    // Look for the .msapp file in the CanvasApps folder
                    if (entry.Name.StartsWith("CanvasApps/") && entry.Name.EndsWith(".msapp"))
                    {
                        WriteVerbose($"Found .msapp file: {entry.Name}");

                        using (var msappStream = new MemoryStream())
                        {
                            byte[] buffer = new byte[4096];
                            int count;
                            while ((count = zipInputStream.Read(buffer, 0, buffer.Length)) > 0)
                            {
                                msappStream.Write(buffer, 0, count);
                            }
                            return msappStream.ToArray();
                        }
                    }
                }
            }

            WriteWarning($"No .msapp file found in solution for Canvas app '{canvasAppName}'");
            return null;
        }

        private EntityReference GetDefaultPublisher()
        {
            // Get the default publisher for the organization
            var query = new QueryExpression("publisher")
            {
                ColumnSet = new ColumnSet("publisherid"),
                Criteria = new FilterExpression()
            };
            query.Criteria.AddCondition("isreadonly", ConditionOperator.Equal, false);
            query.TopCount = 1;

            var results = Connection.RetrieveMultiple(query);
            if (results.Entities.Count > 0)
            {
                return results.Entities[0].ToEntityReference();
            }

            // Fallback: query for any publisher
            query = new QueryExpression("publisher")
            {
                ColumnSet = new ColumnSet("publisherid"),
                TopCount = 1
            };
            results = Connection.RetrieveMultiple(query);
            if (results.Entities.Count > 0)
            {
                return results.Entities[0].ToEntityReference();
            }

            throw new Exception("No publisher found in the organization");
        }
    }
}
