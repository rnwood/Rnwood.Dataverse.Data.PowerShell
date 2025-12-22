using ICSharpCode.SharpZipLib.Zip;
using Microsoft.Crm.Sdk.Messages;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Query;
using System;
using System.IO;
using System.Linq;
using System.Management.Automation;
using System.Text;
using System.Xml.Linq;

namespace Rnwood.Dataverse.Data.PowerShell.Commands
{
    /// <summary>
    /// Creates or updates a Canvas app in a Dataverse environment (upsert operation) using solution import.
    /// Automatically determines whether to create or update based on ID or Name.
    /// </summary>
    [Cmdlet(VerbsCommon.Set, "DataverseCanvasApp", SupportsShouldProcess = true, ConfirmImpact = ConfirmImpact.Medium)]
    [OutputType(typeof(Guid))]
    public class SetDataverseCanvasAppCmdlet : OrganizationServiceCmdlet
    {
        /// <summary>
        /// Gets or sets the Canvas app ID. If provided and exists, updates the app. If not exists, creates with this ID.
        /// </summary>
        [Parameter(Position = 0, ValueFromPipelineByPropertyName = true, HelpMessage = "ID of the Canvas app (used as unique key for upsert)")]
        public Guid? Id { get; set; }

        /// <summary>
        /// Gets or sets the name for the Canvas app. Used as unique key if ID not provided.
        /// </summary>
        [Parameter(Position = 1, HelpMessage = "Name for the Canvas app (logical name, used as unique key if ID not provided)")]
        public string Name { get; set; }

        /// <summary>
        /// Gets or sets the display name for the Canvas app.
        /// </summary>
        [Parameter(HelpMessage = "Display name for the Canvas app")]
        public string DisplayName { get; set; }

        /// <summary>
        /// Gets or sets the description for the Canvas app.
        /// </summary>
        [Parameter(HelpMessage = "Description for the Canvas app")]
        public string Description { get; set; }

        /// <summary>
        /// Gets or sets the path to an .msapp file to use as the document content.
        /// </summary>
        [Parameter(Mandatory = true, HelpMessage = "Path to an .msapp file to use as the document content")]
        [ValidateNotNullOrEmpty]
        public string MsAppPath { get; set; }

        /// <summary>
        /// Gets or sets the publisher prefix for new Canvas apps.
        /// </summary>
        [Parameter(HelpMessage = "Publisher prefix for new Canvas apps (e.g., 'new'). Required for creation.")]
        public string PublisherPrefix { get; set; } = "new";

        /// <summary>
        /// Gets or sets whether to return the Canvas app ID.
        /// </summary>
        [Parameter(HelpMessage = "If set, returns the Canvas app ID")]
        public SwitchParameter PassThru { get; set; }

        /// <inheritdoc />
        protected override void ProcessRecord()
        {
            base.ProcessRecord();

            // Validate MsAppPath
            var msappFilePath = GetUnresolvedProviderPathFromPSPath(MsAppPath);
            if (!File.Exists(msappFilePath))
            {
                ThrowTerminatingError(new ErrorRecord(
                    new FileNotFoundException($"MsApp file not found: {msappFilePath}"),
                    "FileNotFound",
                    ErrorCategory.ObjectNotFound,
                    msappFilePath));
                return;
            }

            // Read .msapp file
            byte[] msappBytes = File.ReadAllBytes(msappFilePath);

            // Determine if we're creating or updating
            Guid canvasAppId;
            bool isUpdate = false;
            Entity existingApp = null;
            string canvasAppName = null;

            if (Id.HasValue && Id.Value != Guid.Empty)
            {
                // Try to find by ID
                try
                {
                    existingApp = Connection.Retrieve("canvasapp", Id.Value, new ColumnSet("canvasappid", "name", "displayname"));
                    canvasAppId = Id.Value;
                    canvasAppName = existingApp.GetAttributeValue<string>("name");
                    isUpdate = true;
                    WriteVerbose($"Found existing Canvas app by ID: {canvasAppId}, Name: {canvasAppName}");
                }
                catch
                {
                    // Doesn't exist, need a name to create
                    if (string.IsNullOrEmpty(Name))
                    {
                        ThrowTerminatingError(new ErrorRecord(
                            new ArgumentException("Name is required when creating a new Canvas app with a specific ID"),
                            "MissingName",
                            ErrorCategory.InvalidArgument,
                            null));
                        return;
                    }
                    canvasAppId = Id.Value;
                    canvasAppName = Name;
                    isUpdate = false;
                    WriteVerbose($"Canvas app with ID {canvasAppId} not found, will create new with name {canvasAppName}");
                }
            }
            else if (!string.IsNullOrEmpty(Name))
            {
                // Try to find by name
                var foundId = FindCanvasAppByName(Name);
                if (foundId.HasValue)
                {
                    canvasAppId = foundId.Value;
                    existingApp = Connection.Retrieve("canvasapp", canvasAppId, new ColumnSet("canvasappid", "name", "displayname"));
                    canvasAppName = Name;
                    isUpdate = true;
                    WriteVerbose($"Found existing Canvas app by name '{Name}': {canvasAppId}");
                }
                else
                {
                    canvasAppId = Guid.NewGuid();
                    canvasAppName = Name;
                    isUpdate = false;
                    WriteVerbose($"Canvas app '{Name}' not found, will create new with ID {canvasAppId}");
                }
            }
            else
            {
                ThrowTerminatingError(new ErrorRecord(
                    new ArgumentException("Either Id or Name must be provided"),
                    "MissingIdentifier",
                    ErrorCategory.InvalidArgument,
                    null));
                return;
            }

            // Determine display name
            string displayName = DisplayName;
            if (string.IsNullOrEmpty(displayName) && existingApp != null)
            {
                displayName = existingApp.GetAttributeValue<string>("displayname");
            }
            if (string.IsNullOrEmpty(displayName))
            {
                displayName = canvasAppName;
            }

            // Perform upsert using solution import
            string action = isUpdate ? $"Update Canvas app '{canvasAppName}'" : $"Create Canvas app '{canvasAppName}'";
            if (!ShouldProcess(action, action, isUpdate ? "Update Canvas App" : "Create Canvas App"))
            {
                return;
            }

            if (isUpdate)
            {
                PerformUpdate(canvasAppId, canvasAppName, displayName, msappBytes);
            }
            else
            {
                PerformCreate(canvasAppId, canvasAppName, displayName, msappBytes);
            }

            if (PassThru)
            {
                WriteObject(canvasAppId);
            }
        }

        private void PerformCreate(Guid canvasAppId, string canvasAppName, string displayName, byte[] msappBytes)
        {
            WriteVerbose($"Creating new Canvas app: {canvasAppName} (ID: {canvasAppId})");

            // Create a solution with the Canvas app
            byte[] solutionBytes = CreateSolutionWithCanvasApp(canvasAppName, displayName, msappBytes);

            // Import the solution
            ImportSolution(solutionBytes);

            WriteVerbose($"Canvas app created successfully with ID: {canvasAppId}");
        }

        private void PerformUpdate(Guid canvasAppId, string canvasAppName, string displayName, byte[] msappBytes)
        {
            WriteVerbose($"Updating Canvas app: {canvasAppName} (ID: {canvasAppId})");

            // Export the current app in a solution, update it, and re-import
            // For now, we'll create a new solution with the updated app
            byte[] solutionBytes = CreateSolutionWithCanvasApp(canvasAppName, displayName, msappBytes);

            // Import the solution (this will update the existing app)
            ImportSolution(solutionBytes);

            WriteVerbose($"Canvas app updated successfully: {canvasAppId}");
        }

        private byte[] CreateSolutionWithCanvasApp(string canvasAppName, string displayName, byte[] msappBytes)
        {
            using (var memoryStream = new MemoryStream())
            using (var zipOutputStream = new ZipOutputStream(memoryStream))
            {
                zipOutputStream.SetLevel(6);

                // Generate unique solution name based on app name
                string solutionUniqueName = $"TempSolution_{canvasAppName}_{Guid.NewGuid().ToString("N").Substring(0, 8)}";

                // Create solution.xml
                string solutionXml = CreateSolutionXml(solutionUniqueName, displayName ?? canvasAppName);
                AddZipEntry(zipOutputStream, "solution.xml", solutionXml);

                // Create customizations.xml with Canvas app metadata
                string customizationsXml = CreateCustomizationsXml(canvasAppName, displayName);
                AddZipEntry(zipOutputStream, "customizations.xml", customizationsXml);

                // Add .msapp file to CanvasApps folder
                string msappFileName = $"{canvasAppName}_DocumentUri.msapp";
                AddZipEntry(zipOutputStream, $"CanvasApps/{msappFileName}", msappBytes);

                // Create [Content_Types].xml
                string contentTypesXml = CreateContentTypesXml();
                AddZipEntry(zipOutputStream, "[Content_Types].xml", contentTypesXml);

                zipOutputStream.Finish();
                return memoryStream.ToArray();
            }
        }

        private string CreateSolutionXml(string solutionUniqueName, string displayName)
        {
            XNamespace ns = "http://www.w3.org/2001/XMLSchema-instance";
            var doc = new XDocument(
                new XElement("ImportExportXml",
                    new XAttribute("version", "9.2"),
                    new XAttribute("SolutionPackageVersion", "9.2"),
                    new XAttribute("languagecode", "1033"),
                    new XAttribute("generatedBy", "PowerShell"),
                    new XAttribute(XNamespace.Xmlns + "xsi", ns),
                    new XElement("SolutionManifest",
                        new XElement("UniqueName", solutionUniqueName),
                        new XElement("LocalizedNames",
                            new XElement("LocalizedName",
                                new XAttribute("description", displayName),
                                new XAttribute("languagecode", "1033")
                            )
                        ),
                        new XElement("Descriptions"),
                        new XElement("Version", "1.0.0.0"),
                        new XElement("Managed", "0"),
                        new XElement("Publisher",
                            new XElement("UniqueName", $"Publisher{PublisherPrefix}"),
                            new XElement("LocalizedNames",
                                new XElement("LocalizedName",
                                    new XAttribute("description", "PowerShell Publisher"),
                                    new XAttribute("languagecode", "1033")
                                )
                            ),
                            new XElement("Descriptions"),
                            new XElement("EMailAddress", new XAttribute(ns + "nil", "true")),
                            new XElement("SupportingWebsiteUrl", new XAttribute(ns + "nil", "true")),
                            new XElement("CustomizationPrefix", PublisherPrefix),
                            new XElement("CustomizationOptionValuePrefix", "10000"),
                            new XElement("Addresses")
                        ),
                        new XElement("RootComponents"),
                        new XElement("MissingDependencies")
                    )
                )
            );

            return doc.ToString();
        }

        private string CreateCustomizationsXml(string canvasAppName, string displayName)
        {
            XNamespace ns = "http://www.w3.org/2001/XMLSchema-instance";
            string msappPath = $"/CanvasApps/{canvasAppName}_DocumentUri.msapp";

            var doc = new XDocument(
                new XElement("ImportExportXml",
                    new XAttribute(XNamespace.Xmlns + "xsi", ns),
                    new XElement("Entities"),
                    new XElement("Roles"),
                    new XElement("Workflows"),
                    new XElement("FieldSecurityProfiles"),
                    new XElement("Templates"),
                    new XElement("EntityMaps"),
                    new XElement("EntityRelationships"),
                    new XElement("OrganizationSettings"),
                    new XElement("optionsets"),
                    new XElement("CustomControls"),
                    new XElement("EntityDataProviders"),
                    new XElement("CanvasApps",
                        new XElement("CanvasApp",
                            new XElement("Name", canvasAppName),
                            new XElement("AppVersion", DateTime.UtcNow.ToString("yyyy-MM-ddTHH:mm:ssZ")),
                            new XElement("Status", "Ready"),
                            new XElement("CreatedByClientVersion", "3.25122.8.0"),
                            new XElement("MinClientVersion", "3.25122.8.0"),
                            new XElement("Tags", "{\"primaryDeviceWidth\":\"1366\",\"primaryDeviceHeight\":\"768\",\"supportsPortrait\":\"true\",\"supportsLandscape\":\"true\",\"primaryFormFactor\":\"Tablet\",\"showStatusBar\":\"false\",\"publisherVersion\":\"3.25122.8\",\"minimumRequiredApiVersion\":\"2.2.0\",\"hasComponent\":\"false\",\"hasUnlockedComponent\":\"false\",\"isUnifiedRootApp\":\"false\"}"),
                            new XElement("IsCdsUpgraded", "0"),
                            new XElement("GalleryItemId", new XAttribute(ns + "nil", "true")),
                            new XElement("BackgroundColor", "RGBA(0,176,240,1)"),
                            new XElement("DisplayName", displayName ?? canvasAppName),
                            string.IsNullOrEmpty(Description) 
                                ? new XElement("Description", new XAttribute(ns + "nil", "true"))
                                : new XElement("Description", Description),
                            new XElement("CommitMessage", new XAttribute(ns + "nil", "true")),
                            new XElement("Publisher", new XAttribute(ns + "nil", "true")),
                            new XElement("AuthorizationReferences", "[]"),
                            new XElement("ConnectionReferences", "{}"),
                            new XElement("DatabaseReferences", "{}"),
                            new XElement("AppComponents", "[]"),
                            new XElement("AppComponentDependencies", "[]"),
                            new XElement("CanConsumeAppPass", "1"),
                            new XElement("CanvasAppType", "0"),
                            new XElement("BypassConsent", "0"),
                            new XElement("AdminControlBypassConsent", "0"),
                            new XElement("EmbeddedApp", new XAttribute(ns + "nil", "true")),
                            new XElement("IntroducedVersion", "1.0"),
                            new XElement("CdsDependencies", "{\"cdsdependencies\":[]}"),
                            new XElement("IsCustomizable", "1"),
                            new XElement("DocumentUri", msappPath)
                        )
                    ),
                    new XElement("Languages",
                        new XElement("Language", "1033")
                    )
                )
            );

            return doc.ToString();
        }

        private string CreateContentTypesXml()
        {
            return @"<?xml version=""1.0"" encoding=""utf-8""?>
<Types xmlns=""http://schemas.openxmlformats.org/package/2006/content-types"">
  <Default Extension=""xml"" ContentType=""application/octet-stream"" />
  <Default Extension=""msapp"" ContentType=""application/octet-stream"" />
</Types>";
        }

        private void AddZipEntry(ZipOutputStream zipStream, string entryName, string content)
        {
            AddZipEntry(zipStream, entryName, Encoding.UTF8.GetBytes(content));
        }

        private void AddZipEntry(ZipOutputStream zipStream, string entryName, byte[] content)
        {
            var entry = new ZipEntry(entryName)
            {
                DateTime = DateTime.Now,
                Size = content.Length
            };

            zipStream.PutNextEntry(entry);
            zipStream.Write(content, 0, content.Length);
            zipStream.CloseEntry();
        }

        private void ImportSolution(byte[] solutionBytes)
        {
            // Use ImportSolutionRequest to import the solution
            var importRequest = new ImportSolutionRequest
            {
                CustomizationFile = solutionBytes,
                OverwriteUnmanagedCustomizations = true,
                PublishWorkflows = false,
                ImportJobId = Guid.NewGuid()
            };

            WriteVerbose("Importing solution...");
            Connection.Execute(importRequest);
            WriteVerbose("Solution imported successfully");
        }

        private Guid? FindCanvasAppByName(string name)
        {
            var query = new QueryExpression("canvasapp")
            {
                ColumnSet = new ColumnSet("canvasappid"),
                Criteria = new FilterExpression()
            };
            query.Criteria.AddCondition("name", ConditionOperator.Equal, name);

            var results = Connection.RetrieveMultiple(query);
            return results.Entities.FirstOrDefault()?.Id;
        }
    }
}
