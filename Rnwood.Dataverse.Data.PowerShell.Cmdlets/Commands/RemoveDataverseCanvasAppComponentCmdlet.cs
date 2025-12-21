using ICSharpCode.SharpZipLib.Zip;
using Microsoft.PowerPlatform.Dataverse.Client;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Query;
using System;
using System.IO;
using System.Management.Automation;

namespace Rnwood.Dataverse.Data.PowerShell.Commands
{
    /// <summary>
    /// Removes a component from a Canvas app's .msapp file.
    /// </summary>
    [Cmdlet(VerbsCommon.Remove, "DataverseCanvasAppComponent", SupportsShouldProcess = true, ConfirmImpact = ConfirmImpact.High)]
    public class RemoveDataverseCanvasAppComponentCmdlet : OrganizationServiceCmdlet
    {
        /// <summary>
        /// Gets or sets the Canvas app ID to remove component from.
        /// </summary>
        [Parameter(Mandatory = true, HelpMessage = "ID of the Canvas app to remove component from")]
        public Guid CanvasAppId { get; set; }

        /// <summary>
        /// Gets or sets the component name to remove.
        /// </summary>
        [Parameter(Mandatory = true, Position = 0, HelpMessage = "Name of the component to remove")]
        public string ComponentName { get; set; }

        /// <summary>
        /// Gets or sets whether to skip the error if the component doesn't exist.
        /// </summary>
        [Parameter(HelpMessage = "If set, no error is thrown if the component doesn't exist")]
        public SwitchParameter IfExists { get; set; }

        /// <inheritdoc />
        protected override void ProcessRecord()
        {
            base.ProcessRecord();

            string action = $"Remove component '{ComponentName}' from Canvas app '{CanvasAppId}'";
            if (!ShouldProcess(action, action, "Remove Component"))
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

            // Modify .msapp file to remove component
            byte[] modifiedMsappBytes = RemoveMsAppComponent(msappBytes);

            // Update Canvas app with modified .msapp file
            var updateEntity = new Entity("canvasapp")
            {
                Id = CanvasAppId,
                ["document"] = Convert.ToBase64String(modifiedMsappBytes)
            };

            Connection.Update(updateEntity);

            WriteVerbose($"Component '{ComponentName}' removed successfully from Canvas app");
        }

        private byte[] RemoveMsAppComponent(byte[] originalBytes)
        {
            using (var memoryStream = new MemoryStream(originalBytes))
            using (var resultStream = new MemoryStream())
            {
                bool componentFound = false;
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
                            componentFound = true;
                            WriteVerbose($"Removing component '{ComponentName}'");
                            // Skip this entry (don't add it to output)
                            continue;
                        }

                        // Copy entry as-is
                        byte[] entryBytes = ReadZipEntryBytes(zipInputStream);
                        AddZipEntry(zipOutputStream, entry.Name, entryBytes);
                    }

                    zipOutputStream.Finish();
                }

                if (!componentFound && !IfExists.IsPresent)
                {
                    ThrowTerminatingError(new ErrorRecord(
                        new Exception($"Component '{ComponentName}' not found in Canvas app"),
                        "ComponentNotFound",
                        ErrorCategory.ObjectNotFound,
                        ComponentName));
                }
                else if (!componentFound)
                {
                    WriteVerbose($"Component '{ComponentName}' not found (IfExists flag set, skipping)");
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
