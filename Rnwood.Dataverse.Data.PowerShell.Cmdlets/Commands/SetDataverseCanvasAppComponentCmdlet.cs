using ICSharpCode.SharpZipLib.Zip;
using Microsoft.PowerPlatform.Dataverse.Client;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Query;
using System;
using System.IO;
using System.Management.Automation;
using System.Text;

namespace Rnwood.Dataverse.Data.PowerShell.Commands
{
    /// <summary>
    /// Adds or updates a component in a Canvas app's .msapp file.
    /// </summary>
    [Cmdlet(VerbsCommon.Set, "DataverseCanvasAppComponent", SupportsShouldProcess = true, ConfirmImpact = ConfirmImpact.Medium)]
    [OutputType(typeof(Guid))]
    public class SetDataverseCanvasAppComponentCmdlet : OrganizationServiceCmdlet
    {
        /// <summary>
        /// Gets or sets the Canvas app ID to add/update component in.
        /// </summary>
        [Parameter(Mandatory = true, HelpMessage = "ID of the Canvas app to add/update component in")]
        public Guid CanvasAppId { get; set; }

        /// <summary>
        /// Gets or sets the component name.
        /// </summary>
        [Parameter(Mandatory = true, Position = 0, HelpMessage = "Name of the component")]
        public string ComponentName { get; set; }

        /// <summary>
        /// Gets or sets the YAML content for the component.
        /// </summary>
        [Parameter(Mandatory = true, Position = 1, HelpMessage = "YAML content for the component")]
        public string YamlContent { get; set; }

        /// <summary>
        /// Gets or sets the path to a YAML file containing the component definition.
        /// </summary>
        [Parameter(HelpMessage = "Path to a YAML file containing the component definition")]
        public string YamlFilePath { get; set; }

        /// <summary>
        /// Gets or sets whether to skip creating a new component.
        /// </summary>
        [Parameter(HelpMessage = "If set, skips creating new components (only updates existing ones)")]
        public SwitchParameter NoCreate { get; set; }

        /// <summary>
        /// Gets or sets whether to skip updating existing components.
        /// </summary>
        [Parameter(HelpMessage = "If set, skips updating existing components (only creates new ones)")]
        public SwitchParameter NoUpdate { get; set; }

        /// <summary>
        /// Gets or sets whether to return the Canvas app ID.
        /// </summary>
        [Parameter(HelpMessage = "If set, returns the Canvas app ID")]
        public SwitchParameter PassThru { get; set; }

        /// <inheritdoc />
        protected override void ProcessRecord()
        {
            base.ProcessRecord();

            // Get YAML content
            string yaml;
            if (!string.IsNullOrEmpty(YamlFilePath))
            {
                var filePath = GetUnresolvedProviderPathFromPSPath(YamlFilePath);
                if (!File.Exists(filePath))
                {
                    ThrowTerminatingError(new ErrorRecord(
                        new FileNotFoundException($"YAML file not found: {filePath}"),
                        "FileNotFound",
                        ErrorCategory.ObjectNotFound,
                        filePath));
                    return;
                }
                yaml = File.ReadAllText(filePath);
            }
            else
            {
                yaml = YamlContent;
            }

            string action = $"Set component '{ComponentName}' in Canvas app '{CanvasAppId}'";
            if (!ShouldProcess(action, action, "Set Component"))
            {
                return;
            }

            // Retrieve existing Canvas app
            var canvasApp = Connection.Retrieve("canvasapp", CanvasAppId, new ColumnSet("document", "name"));
            if (canvasApp == null)
            {
                ThrowTerminatingError(new ErrorRecord(
                    new Exception($"Canvas app with ID {CanvasAppId} not found"),
                    "CanvasAppNotFound",
                    ErrorCategory.ObjectNotFound,
                    CanvasAppId));
                return;
            }

            // Get document bytes
            string base64Document = canvasApp.GetAttributeValue<string>("document");
            if (string.IsNullOrEmpty(base64Document))
            {
                ThrowTerminatingError(new ErrorRecord(
                    new Exception("Canvas app does not contain a document (.msapp file)"),
                    "NoDocument",
                    ErrorCategory.InvalidData,
                    CanvasAppId));
                return;
            }

            byte[] msappBytes = Convert.FromBase64String(base64Document);

            // Modify .msapp file to add/update component
            byte[] modifiedMsappBytes = ModifyMsAppComponent(msappBytes, yaml);

            // Update Canvas app with modified .msapp file
            var updateEntity = new Entity("canvasapp")
            {
                Id = CanvasAppId,
                ["document"] = Convert.ToBase64String(modifiedMsappBytes)
            };

            Connection.Update(updateEntity);

            WriteVerbose($"Component '{ComponentName}' set successfully in Canvas app");

            if (PassThru)
            {
                WriteObject(CanvasAppId);
            }
        }

        private byte[] ModifyMsAppComponent(byte[] originalBytes, string yaml)
        {
            using (var memoryStream = new MemoryStream(originalBytes))
            using (var resultStream = new MemoryStream())
            {
                bool componentExists = false;
                string componentFileName = $"Src/{ComponentName}.pa.yaml";

                using (var zipInputStream = new ZipInputStream(memoryStream))
                using (var zipOutputStream = new ZipOutputStream(resultStream))
                {
                    zipOutputStream.SetLevel(6);

                    ZipEntry entry;
                    while ((entry = zipInputStream.GetNextEntry()) != null)
                    {
                        if (entry.Name == componentFileName)
                        {
                            componentExists = true;

                            if (NoUpdate.IsPresent)
                            {
                                WriteVerbose($"Component '{ComponentName}' exists, skipping update (NoUpdate flag set)");
                                // Copy existing entry
                                byte[] entryBytes = ReadZipEntryBytes(zipInputStream);
                                AddZipEntry(zipOutputStream, entry.Name, entryBytes);
                            }
                            else
                            {
                                // Replace with new YAML content
                                WriteVerbose($"Updating existing component '{ComponentName}'");
                                AddZipEntry(zipOutputStream, entry.Name, yaml);
                            }
                        }
                        else
                        {
                            // Copy entry as-is
                            byte[] entryBytes = ReadZipEntryBytes(zipInputStream);
                            AddZipEntry(zipOutputStream, entry.Name, entryBytes);
                        }
                    }

                    // Add new component if it doesn't exist
                    if (!componentExists)
                    {
                        if (NoCreate.IsPresent)
                        {
                            WriteVerbose($"Component '{ComponentName}' does not exist, skipping creation (NoCreate flag set)");
                        }
                        else
                        {
                            WriteVerbose($"Creating new component '{ComponentName}'");
                            AddZipEntry(zipOutputStream, componentFileName, yaml);
                        }
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
