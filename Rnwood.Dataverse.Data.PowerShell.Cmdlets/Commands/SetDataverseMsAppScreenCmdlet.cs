using ICSharpCode.SharpZipLib.Zip;
using System;
using System.IO;
using System.Management.Automation;
using System.Text;

namespace Rnwood.Dataverse.Data.PowerShell.Commands
{
    /// <summary>
    /// Adds or updates a screen in a .msapp file.
    /// </summary>
    [Cmdlet(VerbsCommon.Set, "DataverseMsAppScreen", SupportsShouldProcess = true, ConfirmImpact = ConfirmImpact.Medium)]
    public class SetDataverseMsAppScreenCmdlet : PSCmdlet
    {
        /// <summary>
        /// Gets or sets the path to the .msapp file.
        /// </summary>
        [Parameter(Mandatory = true, Position = 0, HelpMessage = "Path to the .msapp file")]
        [ValidateNotNullOrEmpty]
        public string MsAppPath { get; set; }

        /// <summary>
        /// Gets or sets the screen name.
        /// </summary>
        [Parameter(Mandatory = true, Position = 1, HelpMessage = "Name of the screen")]
        [ValidateNotNullOrEmpty]
        public string ScreenName { get; set; }

        /// <summary>
        /// Gets or sets the YAML content for the screen.
        /// </summary>
        [Parameter(Mandatory = true, Position = 2, ParameterSetName = "YamlContent", HelpMessage = "YAML content for the screen")]
        [ValidateNotNullOrEmpty]
        public string YamlContent { get; set; }

        /// <summary>
        /// Gets or sets the path to a YAML file containing the screen definition.
        /// </summary>
        [Parameter(Mandatory = true, ParameterSetName = "YamlFile", HelpMessage = "Path to a YAML file containing the screen definition")]
        [ValidateNotNullOrEmpty]
        public string YamlFilePath { get; set; }

        /// <inheritdoc />
        protected override void ProcessRecord()
        {
            base.ProcessRecord();

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

            // Get YAML content
            string yaml;
            if (!string.IsNullOrEmpty(YamlFilePath))
            {
                var yamlPath = GetUnresolvedProviderPathFromPSPath(YamlFilePath);
                if (!File.Exists(yamlPath))
                {
                    ThrowTerminatingError(new ErrorRecord(
                        new FileNotFoundException($"YAML file not found: {yamlPath}"),
                        "FileNotFound",
                        ErrorCategory.ObjectNotFound,
                        yamlPath));
                    return;
                }
                yaml = File.ReadAllText(yamlPath);
            }
            else
            {
                yaml = YamlContent;
            }

            string action = $"Set screen '{ScreenName}' in .msapp file '{filePath}'";
            if (!ShouldProcess(action, action, "Set Screen"))
            {
                return;
            }

            byte[] msappBytes = File.ReadAllBytes(filePath);
            byte[] modifiedBytes = ModifyMsAppScreen(msappBytes, yaml);
            File.WriteAllBytes(filePath, modifiedBytes);

            WriteVerbose($"Screen '{ScreenName}' set successfully in .msapp file");
        }

        private byte[] ModifyMsAppScreen(byte[] originalBytes, string yaml)
        {
            using (var memoryStream = new MemoryStream(originalBytes))
            using (var resultStream = new MemoryStream())
            {
                bool screenExists = false;
                string screenFileName = $"Src/{ScreenName}.pa.yaml";

                using (var zipInputStream = new ZipInputStream(memoryStream))
                using (var zipOutputStream = new ZipOutputStream(resultStream))
                {
                    zipOutputStream.SetLevel(6);

                    ZipEntry entry;
                    while ((entry = zipInputStream.GetNextEntry()) != null)
                    {
                        if (entry.Name == screenFileName)
                        {
                            screenExists = true;
                            WriteVerbose($"Updating existing screen '{ScreenName}'");
                            AddZipEntry(zipOutputStream, entry.Name, yaml);
                        }
                        else
                        {
                            // Copy entry as-is
                            byte[] entryBytes = ReadZipEntryBytes(zipInputStream);
                            AddZipEntry(zipOutputStream, entry.Name, entryBytes);
                        }
                    }

                    // Add new screen if it doesn't exist
                    if (!screenExists)
                    {
                        WriteVerbose($"Creating new screen '{ScreenName}'");
                        AddZipEntry(zipOutputStream, screenFileName, yaml);
                    }

                    zipOutputStream.Finish();
                }

                return resultStream.ToArray();
            }
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
    }
}
