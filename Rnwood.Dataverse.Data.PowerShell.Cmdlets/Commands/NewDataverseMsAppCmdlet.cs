using ICSharpCode.SharpZipLib.Zip;
using System;
using System.IO;
using System.Management.Automation;
using System.Text;

namespace Rnwood.Dataverse.Data.PowerShell.Commands
{
    /// <summary>
    /// Creates a new default .msapp file for a Canvas app.
    /// </summary>
    [Cmdlet(VerbsCommon.New, "DataverseMsApp", SupportsShouldProcess = true, ConfirmImpact = ConfirmImpact.Low)]
    [OutputType(typeof(FileInfo))]
    public class NewDataverseMsAppCmdlet : PSCmdlet
    {
        /// <summary>
        /// Gets or sets the path where the .msapp file will be created.
        /// </summary>
        [Parameter(Mandatory = true, Position = 0, HelpMessage = "Path where the .msapp file will be created")]
        [ValidateNotNullOrEmpty]
        public string Path { get; set; }

        /// <summary>
        /// Gets or sets whether to overwrite an existing file.
        /// </summary>
        [Parameter(HelpMessage = "If set, overwrites the file if it already exists")]
        public SwitchParameter Force { get; set; }

        /// <inheritdoc />
        protected override void ProcessRecord()
        {
            base.ProcessRecord();

            var filePath = GetUnresolvedProviderPathFromPSPath(Path);
            
            if (File.Exists(filePath) && !Force.IsPresent)
            {
                ThrowTerminatingError(new ErrorRecord(
                    new InvalidOperationException($"File already exists: {filePath}. Use -Force to overwrite."),
                    "FileExists",
                    ErrorCategory.ResourceExists,
                    filePath));
                return;
            }

            string action = $"Create new .msapp file at '{filePath}'";
            if (!ShouldProcess(action, action, "Create MsApp"))
            {
                return;
            }

            byte[] msappBytes = CreateDefaultMsApp();
            File.WriteAllBytes(filePath, msappBytes);

            WriteVerbose($"Created new .msapp file at: {filePath}");
            WriteObject(new FileInfo(filePath));
        }

        private byte[] CreateDefaultMsApp()
        {
            using (var memoryStream = new MemoryStream())
            {
                using (var zipOutputStream = new ZipOutputStream(memoryStream))
                {
                    zipOutputStream.SetLevel(6);

                    // Add .gitignore
                    AddTextEntry(zipOutputStream, ".gitignore", @"# Power Apps temp files
*.cdsproj.user
");

                    // Add Header.json
                    string headerJson = "{\"DocVersion\":\"1.347\",\"MinVersionToLoad\":\"1.331\",\"MSAppStructureVersion\":\"2.4.0\",\"LastSavedDateTimeUTC\":\"" + DateTime.UtcNow.ToString("M/d/yyyy HH:mm:ss") + "\",\"AnalysisOptions\":{\"DataflowAnalysisEnabled\":true,\"DataflowAnalysisFlagStateToggledByUser\":false}}";
                    AddTextEntry(zipOutputStream, "Header.json", headerJson);

                    // Add Properties.json (simplified version)
                    string propertiesJson = @"{
  ""LocalDatabaseReferencesAsEmpty"": true,
  ""Name"": ""NewCanvasApp"",
  ""AppCreationSource"": ""Portal"",
  ""AppDescription"": """",
  ""AppPreviewFlagsMap"": {},
  ""BackgroundColor"": ""RGBA(255,255,255,1)"",
  ""DocumentLayoutHeight"": 768,
  ""DocumentLayoutWidth"": 1366,
  ""DocumentLayoutOrientation"": ""landscape"",
  ""DocumentLayoutScaleToFit"": false,
  ""DocumentLayoutMaintainAspectRatio"": true,
  ""DocumentAppType"": ""Phone"",
  ""DocumentType"": ""App"",
  ""EnableInstrumentation"": false,
  ""FileID"": """ + Guid.NewGuid().ToString() + @""",
  ""Id"": """ + Guid.NewGuid().ToString() + @""",
  ""InstrumentationKey"": """",
  ""Name"": ""NewCanvasApp"",
  ""OriginatingVersion"": ""1.347""
}";
                    AddTextEntry(zipOutputStream, "Properties.json", propertiesJson);

                    // Add Src/Screen1.pa.yaml
                    string screen1Yaml = @"Screens:
  Screen1:
    Properties:
      LoadingSpinnerColor: =RGBA(56, 96, 178, 1)
";
                    AddTextEntry(zipOutputStream, @"Src\Screen1.pa.yaml", screen1Yaml);

                    // Add Src/App.pa.yaml
                    string appYaml = @"App:
  Properties:
    Theme: =PowerAppsTheme
";
                    AddTextEntry(zipOutputStream, @"Src\App.pa.yaml", appYaml);

                    // Add Src/_EditorState.pa.yaml
                    string editorStateYaml = @"{}
";
                    AddTextEntry(zipOutputStream, @"Src\_EditorState.pa.yaml", editorStateYaml);

                    // Add References/Themes.json (minimal)
                    string themesJson = "{\"CurrentTheme\":\"PowerAppsTheme\",\"ThemesGallery\":[]}";
                    AddTextEntry(zipOutputStream, @"References\Themes.json", themesJson);

                    // Add References/DataSources.json
                    AddTextEntry(zipOutputStream, @"References\DataSources.json", "[]");

                    // Add References/ModernThemes.json
                    AddTextEntry(zipOutputStream, @"References\ModernThemes.json", "{\"CurrentTheme\":{},\"ThemesGallery\":[]}");

                    // Add References/Resources.json
                    AddTextEntry(zipOutputStream, @"References\Resources.json", "[]");

                    // Add References/Templates.json
                    AddTextEntry(zipOutputStream, @"References\Templates.json", "[]");

                    // Add AppCheckerResult.sarif (empty)
                    string sarifJson = @"{
  ""version"": ""2.1.0"",
  ""runs"": [
    {
      ""tool"": {
        ""driver"": {
          ""name"": ""PowerAppsChecker"",
          ""version"": ""1.0""
        }
      },
      ""results"": []
    }
  ]
}";
                    AddTextEntry(zipOutputStream, "AppCheckerResult.sarif", sarifJson);

                    // Add Resources/PublishInfo.json
                    string publishInfoJson = @"{
  ""PublishTimestamp"": """ + DateTime.UtcNow.ToString("o") + @""",
  ""PublishedAppVersion"": ""1.0.0""
}";
                    AddTextEntry(zipOutputStream, @"Resources\PublishInfo.json", publishInfoJson);

                    zipOutputStream.Finish();
                }

                return memoryStream.ToArray();
            }
        }

        private void AddTextEntry(ZipOutputStream zipStream, string entryName, string content)
        {
            byte[] buffer = Encoding.UTF8.GetBytes(content);
            var entry = new ZipEntry(entryName)
            {
                DateTime = DateTime.Now,
                Size = buffer.Length
            };

            zipStream.PutNextEntry(entry);
            zipStream.Write(buffer, 0, buffer.Length);
            zipStream.CloseEntry();
        }
    }
}
