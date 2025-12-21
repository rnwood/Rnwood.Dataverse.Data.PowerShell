using ICSharpCode.SharpZipLib.Zip;
using Microsoft.Crm.Sdk.Messages;
using Microsoft.PowerPlatform.Dataverse.Client;
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
    /// Creates or updates a Canvas app in a Dataverse environment by generating and importing a solution.
    /// </summary>
    [Cmdlet(VerbsCommon.Set, "DataverseCanvasApp", SupportsShouldProcess = true, ConfirmImpact = ConfirmImpact.Medium)]
    [OutputType(typeof(Guid))]
    public class SetDataverseCanvasAppCmdlet : OrganizationServiceCmdlet
    {
        private const string PARAMSET_NEW = "New";
        private const string PARAMSET_UPDATE = "Update";
        private const string PARAMSET_INPUTOBJECT = "InputObject";

        /// <summary>
        /// Gets or sets the Canvas app ID to update.
        /// </summary>
        [Parameter(ParameterSetName = PARAMSET_UPDATE, Mandatory = true, HelpMessage = "ID of the Canvas app to update")]
        public Guid Id { get; set; }

        /// <summary>
        /// Gets or sets the name for the Canvas app. Required for new apps.
        /// </summary>
        [Parameter(ParameterSetName = PARAMSET_NEW, Mandatory = true, Position = 0, HelpMessage = "Name for the Canvas app (logical name)")]
        [Parameter(ParameterSetName = PARAMSET_UPDATE, HelpMessage = "Name for the Canvas app (logical name)")]
        public string Name { get; set; }

        /// <summary>
        /// Gets or sets the display name for the Canvas app.
        /// </summary>
        [Parameter(ParameterSetName = PARAMSET_NEW, HelpMessage = "Display name for the Canvas app")]
        [Parameter(ParameterSetName = PARAMSET_UPDATE, HelpMessage = "Display name for the Canvas app")]
        public string DisplayName { get; set; }

        /// <summary>
        /// Gets or sets the description for the Canvas app.
        /// </summary>
        [Parameter(ParameterSetName = PARAMSET_NEW, HelpMessage = "Description for the Canvas app")]
        [Parameter(ParameterSetName = PARAMSET_UPDATE, HelpMessage = "Description for the Canvas app")]
        public string Description { get; set; }

        /// <summary>
        /// Gets or sets the publisher prefix for the new Canvas app.
        /// </summary>
        [Parameter(ParameterSetName = PARAMSET_NEW, Mandatory = true, HelpMessage = "Publisher prefix for the new Canvas app (e.g., 'new')")]
        public string PublisherPrefix { get; set; }

        /// <summary>
        /// Gets or sets the publisher display name for the new Canvas app.
        /// </summary>
        [Parameter(ParameterSetName = PARAMSET_NEW, HelpMessage = "Publisher display name for the new Canvas app")]
        public string PublisherName { get; set; } = "Default Publisher";

        /// <summary>
        /// Gets or sets the path to an .msapp file to use as the document content.
        /// </summary>
        [Parameter(ParameterSetName = PARAMSET_NEW, HelpMessage = "Path to an .msapp file to use as the document content")]
        [Parameter(ParameterSetName = PARAMSET_UPDATE, HelpMessage = "Path to an .msapp file to use as the document content")]
        public string MsAppPath { get; set; }

        /// <summary>
        /// Gets or sets the background color for the Canvas app.
        /// </summary>
        [Parameter(ParameterSetName = PARAMSET_NEW, HelpMessage = "Background color for the Canvas app (e.g., 'RGBA(0,176,240,1)')")]
        [Parameter(ParameterSetName = PARAMSET_UPDATE, HelpMessage = "Background color for the Canvas app (e.g., 'RGBA(0,176,240,1)')")]
        public string BackgroundColor { get; set; } = "RGBA(0,176,240,1)";

        /// <summary>
        /// Gets or sets whether to return the Canvas app ID.
        /// </summary>
        [Parameter(HelpMessage = "If set, returns the Canvas app ID")]
        public SwitchParameter PassThru { get; set; }

        /// <summary>
        /// Gets or sets whether to skip creating a new Canvas app.
        /// </summary>
        [Parameter(HelpMessage = "If set, skips creating new Canvas apps (only updates existing ones)")]
        public SwitchParameter NoCreate { get; set; }

        /// <summary>
        /// Gets or sets whether to skip updating existing Canvas apps.
        /// </summary>
        [Parameter(HelpMessage = "If set, skips updating existing Canvas apps (only creates new ones)")]
        public SwitchParameter NoUpdate { get; set; }

        /// <inheritdoc />
        protected override void ProcessRecord()
        {
            base.ProcessRecord();

            Guid canvasAppId;
            bool isUpdate = false;

            // Determine if we're updating or creating
            if (ParameterSetName == PARAMSET_UPDATE)
            {
                canvasAppId = Id;
                isUpdate = true;
            }
            else
            {
                // Check if Canvas app exists by name
                var existingApp = FindCanvasAppByName(Name);
                if (existingApp.HasValue)
                {
                    canvasAppId = existingApp.Value;
                    isUpdate = true;
                }
                else
                {
                    canvasAppId = Guid.NewGuid();
                    isUpdate = false;
                }
            }

            // Check NoCreate/NoUpdate flags
            if (isUpdate && NoUpdate.IsPresent)
            {
                WriteVerbose($"Skipping update of existing Canvas app: {Name ?? canvasAppId.ToString()}");
                if (PassThru)
                {
                    WriteObject(canvasAppId);
                }
                return;
            }

            if (!isUpdate && NoCreate.IsPresent)
            {
                WriteVerbose($"Skipping creation of new Canvas app: {Name}");
                return;
            }

            // For updates, we need to export the existing app, modify it, and re-import
            if (isUpdate)
            {
                ProcessUpdate(canvasAppId);
            }
            else
            {
                ProcessCreate(canvasAppId);
            }

            if (PassThru)
            {
                WriteObject(canvasAppId);
            }
        }

        private void ProcessCreate(Guid canvasAppId)
        {
            string action = $"Create Canvas app '{Name}'";
            if (!ShouldProcess(action, action, "Create Canvas App"))
            {
                return;
            }

            WriteVerbose($"Creating new Canvas app: {Name}");

            // Create solution with Canvas app
            byte[] solutionBytes = CreateSolutionWithCanvasApp(canvasAppId);

            // Import the solution
            ImportSolution(solutionBytes, $"Creating Canvas app '{Name}'");

            WriteVerbose($"Canvas app created successfully with ID: {canvasAppId}");
        }

        private void ProcessUpdate(Guid canvasAppId)
        {
            string action = $"Update Canvas app '{canvasAppId}'";
            if (!ShouldProcess(action, action, "Update Canvas App"))
            {
                return;
            }

            WriteVerbose($"Updating Canvas app: {canvasAppId}");

            // Retrieve existing Canvas app
            var existingApp = Connection.Retrieve("canvasapp", canvasAppId, new ColumnSet(true));
            if (existingApp == null)
            {
                ThrowTerminatingError(new ErrorRecord(
                    new Exception($"Canvas app with ID {canvasAppId} not found"),
                    "CanvasAppNotFound",
                    ErrorCategory.ObjectNotFound,
                    canvasAppId));
                return;
            }

            // Export the Canvas app in a solution, modify it, and re-import
            byte[] solutionBytes = ExportAndModifyCanvasApp(existingApp);

            // Import the modified solution
            ImportSolution(solutionBytes, $"Updating Canvas app '{canvasAppId}'");

            WriteVerbose($"Canvas app updated successfully: {canvasAppId}");
        }

        private byte[] CreateSolutionWithCanvasApp(Guid canvasAppId)
        {
            // Generate unique solution name
            string solutionName = $"TempCanvasApp_{Guid.NewGuid():N}";
            string canvasAppName = Name;

            // Read or create default .msapp content
            byte[] msappBytes;
            if (!string.IsNullOrEmpty(MsAppPath))
            {
                var filePath = GetUnresolvedProviderPathFromPSPath(MsAppPath);
                if (!File.Exists(filePath))
                {
                    ThrowTerminatingError(new ErrorRecord(
                        new FileNotFoundException($"MsApp file not found: {filePath}"),
                        "FileNotFound",
                        ErrorCategory.ObjectNotFound,
                        filePath));
                    return null;
                }
                msappBytes = File.ReadAllBytes(filePath);
            }
            else
            {
                msappBytes = CreateDefaultMsApp();
            }

            // Create solution zip in memory
            using (var memoryStream = new MemoryStream())
            {
                using (var zipStream = new ZipOutputStream(memoryStream))
                {
                    zipStream.SetLevel(6); // Compression level

                    // Add [Content_Types].xml
                    AddZipEntry(zipStream, "[Content_Types].xml", CreateContentTypesXml());

                    // Add solution.xml
                    AddZipEntry(zipStream, "solution.xml", CreateSolutionXml(solutionName));

                    // Add customizations.xml with Canvas app definition
                    AddZipEntry(zipStream, "customizations.xml", CreateCustomizationsXml(canvasAppName));

                    // Add Canvas app files
                    string canvasAppFolder = $"CanvasApps/{canvasAppName}";
                    
                    // Add document (.msapp file)
                    AddZipEntry(zipStream, $"{canvasAppFolder}_DocumentUri.msapp", msappBytes);

                    // Add background image (empty PNG)
                    AddZipEntry(zipStream, $"{canvasAppFolder}_BackgroundImageUri", CreateDefaultBackgroundImage());

                    // Add identity.json
                    AddZipEntry(zipStream, $"{canvasAppFolder}_AdditionalUris0_identity.json", CreateIdentityJson(canvasAppId));

                    zipStream.Finish();
                    zipStream.Close();
                }

                return memoryStream.ToArray();
            }
        }

        private byte[] ExportAndModifyCanvasApp(Entity existingApp)
        {
            // Get the Canvas app name
            string canvasAppName = existingApp.GetAttributeValue<string>("name");

            // Create a temporary solution containing this Canvas app
            string tempSolutionName = $"TempCanvasAppExport_{Guid.NewGuid():N}";

            WriteVerbose($"Creating temporary solution '{tempSolutionName}' to export Canvas app...");

            // Create solution
            var solution = new Entity("solution")
            {
                ["uniquename"] = tempSolutionName,
                ["friendlyname"] = $"Temp Canvas App Export {canvasAppName}",
                ["version"] = "1.0.0.0",
                ["publisherid"] = GetDefaultPublisher()
            };
            var solutionId = Connection.Create(solution);

            try
            {
                // Add Canvas app to solution as a component
                var addRequest = new AddSolutionComponentRequest
                {
                    ComponentType = 300, // Canvas App
                    ComponentId = existingApp.Id,
                    SolutionUniqueName = tempSolutionName,
                    AddRequiredComponents = false
                };
                Connection.Execute(addRequest);

                WriteVerbose("Exporting temporary solution...");

                // Export the solution
                var exportRequest = new ExportSolutionRequest
                {
                    SolutionName = tempSolutionName,
                    Managed = false
                };
                var exportResponse = (ExportSolutionResponse)Connection.Execute(exportRequest);
                byte[] solutionBytes = exportResponse.ExportSolutionFile;

                // Modify the solution bytes (update properties as specified)
                solutionBytes = ModifySolutionBytes(solutionBytes, canvasAppName);

                return solutionBytes;
            }
            finally
            {
                // Clean up temporary solution
                WriteVerbose($"Deleting temporary solution '{tempSolutionName}'...");
                try
                {
                    Connection.Delete("solution", solutionId);
                }
                catch (Exception ex)
                {
                    WriteWarning($"Failed to delete temporary solution: {ex.Message}");
                }
            }
        }

        private byte[] ModifySolutionBytes(byte[] originalBytes, string canvasAppName)
        {
            using (var memoryStream = new MemoryStream(originalBytes))
            using (var resultStream = new MemoryStream())
            {
                using (var zipInputStream = new ZipInputStream(memoryStream))
                using (var zipOutputStream = new ZipOutputStream(resultStream))
                {
                    zipOutputStream.SetLevel(6);

                    ZipEntry entry;
                    while ((entry = zipInputStream.GetNextEntry()) != null)
                    {
                        if (entry.Name == "customizations.xml")
                        {
                            // Read and modify customizations.xml
                            byte[] entryBytes = ReadZipEntryBytes(zipInputStream);
                            string xmlContent = Encoding.UTF8.GetString(entryBytes);
                            XDocument doc = XDocument.Parse(xmlContent);

                            // Find and update Canvas app properties
                            var canvasAppElement = doc.Descendants("CanvasApp").FirstOrDefault();
                            if (canvasAppElement != null)
                            {
                                if (!string.IsNullOrEmpty(DisplayName))
                                {
                                    var displayNameElement = canvasAppElement.Element("DisplayName");
                                    if (displayNameElement != null)
                                    {
                                        displayNameElement.Value = DisplayName;
                                    }
                                }

                                if (!string.IsNullOrEmpty(Description))
                                {
                                    var descElement = canvasAppElement.Element("Description");
                                    if (descElement != null)
                                    {
                                        descElement.SetAttributeValue(XNamespace.Xmlns + "nil", "false");
                                        descElement.Value = Description;
                                    }
                                }

                                if (!string.IsNullOrEmpty(BackgroundColor))
                                {
                                    var bgColorElement = canvasAppElement.Element("BackgroundColor");
                                    if (bgColorElement != null)
                                    {
                                        bgColorElement.Value = BackgroundColor;
                                    }
                                }
                            }

                            // Write modified XML
                            byte[] modifiedBytes = Encoding.UTF8.GetBytes(doc.ToString());
                            AddZipEntry(zipOutputStream, entry.Name, modifiedBytes);
                        }
                        else if (!string.IsNullOrEmpty(MsAppPath) && entry.Name.EndsWith("_DocumentUri.msapp"))
                        {
                            // Replace with new .msapp file
                            var filePath = GetUnresolvedProviderPathFromPSPath(MsAppPath);
                            byte[] newMsappBytes = File.ReadAllBytes(filePath);
                            AddZipEntry(zipOutputStream, entry.Name, newMsappBytes);
                        }
                        else
                        {
                            // Copy entry as-is
                            byte[] entryBytes = ReadZipEntryBytes(zipInputStream);
                            AddZipEntry(zipOutputStream, entry.Name, entryBytes);
                        }
                    }

                    zipOutputStream.Finish();
                }

                return resultStream.ToArray();
            }
        }

        private void ImportSolution(byte[] solutionBytes, string operationDescription)
        {
            WriteVerbose($"{operationDescription} - importing solution...");

            var importRequest = new ImportSolutionRequest
            {
                CustomizationFile = solutionBytes,
                OverwriteUnmanagedCustomizations = true,
                PublishWorkflows = false
            };

            var importResponse = (ImportSolutionResponse)Connection.Execute(importRequest);

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

        private EntityReference GetDefaultPublisher()
        {
            var query = new QueryExpression("publisher")
            {
                ColumnSet = new ColumnSet("publisherid"),
                Criteria = new FilterExpression()
            };
            query.Criteria.AddCondition("isreadonly", ConditionOperator.Equal, false);
            query.TopCount = 1;

            var results = Connection.RetrieveMultiple(query);
            var publisher = results.Entities.FirstOrDefault();

            if (publisher != null)
            {
                return new EntityReference("publisher", publisher.Id);
            }

            throw new InvalidOperationException("No writable publisher found in the environment");
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

        private void AddZipEntry(ZipOutputStream zipStream, string entryName, string content)
        {
            AddZipEntry(zipStream, entryName, Encoding.UTF8.GetBytes(content));
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

        private string CreateContentTypesXml()
        {
            return @"<?xml version=""1.0"" encoding=""utf-8""?><Types xmlns=""http://schemas.openxmlformats.org/package/2006/content-types""><Default Extension=""xml"" ContentType=""application/octet-stream"" /><Default Extension=""msapp"" ContentType=""application/octet-stream"" /><Default Extension=""json"" ContentType=""application/octet-stream"" /><Override PartName=""/CanvasApps/" + Name + @"_BackgroundImageUri"" ContentType=""application/octet-stream"" /></Types>";
        }

        private string CreateSolutionXml(string solutionName)
        {
            string publisherUniqueName = $"{PublisherPrefix}publisher";
            string publisherDisplayName = PublisherName ?? "Default Publisher";

            return $@"<?xml version=""1.0"" encoding=""utf-8""?>
<ImportExportXml version=""9.2"" SolutionPackageVersion=""9.2"" languagecode=""1033"" generatedBy=""PowerShell"" xmlns:xsi=""http://www.w3.org/2001/XMLSchema-instance"">
  <SolutionManifest>
    <UniqueName>{solutionName}</UniqueName>
    <LocalizedNames>
      <LocalizedName description=""Temp Canvas App Solution"" languagecode=""1033"" />
    </LocalizedNames>
    <Descriptions />
    <Version>1.0.0.0</Version>
    <Managed>0</Managed>
    <Publisher>
      <UniqueName>{publisherUniqueName}</UniqueName>
      <LocalizedNames>
        <LocalizedName description=""{publisherDisplayName}"" languagecode=""1033"" />
      </LocalizedNames>
      <Descriptions />
      <EMailAddress xsi:nil=""true""></EMailAddress>
      <SupportingWebsiteUrl xsi:nil=""true""></SupportingWebsiteUrl>
      <CustomizationPrefix>{PublisherPrefix}</CustomizationPrefix>
      <CustomizationOptionValuePrefix>10000</CustomizationOptionValuePrefix>
      <Addresses>
        <Address>
          <AddressNumber>1</AddressNumber>
          <AddressTypeCode xsi:nil=""true""></AddressTypeCode>
          <City xsi:nil=""true""></City>
        </Address>
      </Addresses>
    </Publisher>
    <RootComponents>
      <RootComponent type=""300"" schemaName=""{Name}"" behavior=""0"" />
    </RootComponents>
    <MissingDependencies />
  </SolutionManifest>
</ImportExportXml>";
        }

        private string CreateCustomizationsXml(string canvasAppName)
        {
            string displayNameValue = DisplayName ?? Name;
            string descriptionValue = Description ?? "";
            string descriptionAttr = string.IsNullOrEmpty(descriptionValue) ? @" xsi:nil=""true""" : "";

            return $@"<?xml version=""1.0"" encoding=""utf-8""?>
<ImportExportXml xmlns:xsi=""http://www.w3.org/2001/XMLSchema-instance"" OrganizationVersion=""9.2"" OrganizationSchemaType=""Standard"">
  <Entities></Entities>
  <Roles></Roles>
  <Workflows></Workflows>
  <FieldSecurityProfiles></FieldSecurityProfiles>
  <Templates />
  <EntityMaps />
  <EntityRelationships />
  <OrganizationSettings />
  <optionsets />
  <CustomControls />
  <EntityDataProviders />
  <CanvasApps>
    <CanvasApp>
      <Name>{canvasAppName}</Name>
      <AppVersion>{DateTime.UtcNow:yyyy-MM-ddTHH:mm:ssZ}</AppVersion>
      <Status>Ready</Status>
      <CreatedByClientVersion>3.25122.8.0</CreatedByClientVersion>
      <MinClientVersion>3.25122.8.0</MinClientVersion>
      <Tags>{{""primaryDeviceWidth"":""1366"",""primaryDeviceHeight"":""768"",""supportsPortrait"":""true"",""supportsLandscape"":""true"",""primaryFormFactor"":""Tablet"",""showStatusBar"":""false"",""publisherVersion"":""3.25122.8"",""minimumRequiredApiVersion"":""2.2.0"",""hasComponent"":""false"",""hasUnlockedComponent"":""false"",""isUnifiedRootApp"":""false""}}</Tags>
      <IsCdsUpgraded>0</IsCdsUpgraded>
      <GalleryItemId xsi:nil=""true""></GalleryItemId>
      <BackgroundColor>{BackgroundColor}</BackgroundColor>
      <DisplayName>{displayNameValue}</DisplayName>
      <Description{descriptionAttr}>{descriptionValue}</Description>
      <CommitMessage xsi:nil=""true""></CommitMessage>
      <Publisher xsi:nil=""true""></Publisher>
      <AuthorizationReferences>[]</AuthorizationReferences>
      <ConnectionReferences>{{}}</ConnectionReferences>
      <DatabaseReferences>{{}}</DatabaseReferences>
      <AppComponents>[]</AppComponents>
      <AppComponentDependencies>[]</AppComponentDependencies>
      <CanConsumeAppPass>1</CanConsumeAppPass>
      <CanvasAppType>0</CanvasAppType>
      <BypassConsent>0</BypassConsent>
      <AdminControlBypassConsent>0</AdminControlBypassConsent>
      <EmbeddedApp xsi:nil=""true""></EmbeddedApp>
      <IntroducedVersion>1.0</IntroducedVersion>
      <CdsDependencies>{{""cdsdependencies"":[]}}</CdsDependencies>
      <IsCustomizable>1</IsCustomizable>
      <BackgroundImageUri>/CanvasApps/{canvasAppName}_BackgroundImageUri</BackgroundImageUri>
      <DocumentUri>/CanvasApps/{canvasAppName}_DocumentUri.msapp</DocumentUri>
      <AdditionalUris>
        <AdditionalUri>/CanvasApps/{canvasAppName}_AdditionalUris0_identity.json</AdditionalUri>
      </AdditionalUris>
    </CanvasApp>
  </CanvasApps>
  <Languages>
    <Language>1033</Language>
  </Languages>
</ImportExportXml>";
        }

        private byte[] CreateDefaultMsApp()
        {
            // Create a minimal .msapp file (it's a zip file)
            using (var memoryStream = new MemoryStream())
            {
                using (var zipStream = new ZipOutputStream(memoryStream))
                {
                    zipStream.SetLevel(6);

                    // Add minimal files required for a Canvas app
                    AddZipEntry(zipStream, ".gitignore", "");
                    AddZipEntry(zipStream, "Header.json", @"{""DocVersion"":""1.33"",""MinVersionToLoad"":""1.33"",""MSAppStructureVersion"":""2.0""}");
                    AddZipEntry(zipStream, "Properties.json", @"{""InstrumentationKey"":"""",""AppCreationSource"":""AppFromScratch"",""AppDescription"":"""",""AppPreviewFlagsKey"":""""}");
                    AddZipEntry(zipStream, "Src/App.pa.yaml", "App:\n  Properties:\n    Theme: =PowerAppsTheme\n");
                    AddZipEntry(zipStream, "Src/Screen1.pa.yaml", "Screens:\n  Screen1:\n    Properties:\n      LoadingSpinnerColor: =RGBA(56, 96, 178, 1)\n");
                    AddZipEntry(zipStream, "References/DataSources.json", "[]");
                    AddZipEntry(zipStream, "References/Resources.json", "[]");
                    AddZipEntry(zipStream, "References/Templates.json", "[]");
                    AddZipEntry(zipStream, "References/Themes.json", "{}");

                    zipStream.Finish();
                    zipStream.Close();
                }

                return memoryStream.ToArray();
            }
        }

        private byte[] CreateDefaultBackgroundImage()
        {
            // Return minimal 1x1 transparent PNG
            return Convert.FromBase64String("iVBORw0KGgoAAAANSUhEUgAAAAEAAAABCAYAAAAfFcSJAAAADUlEQVR42mNk+M9QDwADhgGAWjR9awAAAABJRU5ErkJggg==");
        }

        private string CreateIdentityJson(Guid canvasAppId)
        {
            return $@"{{""__Version"":""0.1"",""App"":""{canvasAppId}"",""Host"":""{Guid.NewGuid()}"",""Screen1"":""{Guid.NewGuid()}""}}";
        }
    }
}
