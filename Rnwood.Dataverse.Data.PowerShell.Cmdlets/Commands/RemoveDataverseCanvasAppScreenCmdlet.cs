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
    /// Removes a screen from a Canvas app's .msapp file.
    /// </summary>
    [Cmdlet(VerbsCommon.Remove, "DataverseCanvasAppScreen", SupportsShouldProcess = true, ConfirmImpact = ConfirmImpact.High)]
    public class RemoveDataverseCanvasAppScreenCmdlet : OrganizationServiceCmdlet
    {
        /// <summary>
        /// Gets or sets the Canvas app ID to remove screen from.
        /// </summary>
        [Parameter(Mandatory = true, HelpMessage = "ID of the Canvas app to remove screen from")]
        public Guid CanvasAppId { get; set; }

        /// <summary>
        /// Gets or sets the screen name to remove.
        /// </summary>
        [Parameter(Mandatory = true, Position = 0, HelpMessage = "Name of the screen to remove")]
        public string ScreenName { get; set; }

        /// <summary>
        /// Gets or sets whether to skip the error if the screen doesn't exist.
        /// </summary>
        [Parameter(HelpMessage = "If set, no error is thrown if the screen doesn't exist")]
        public SwitchParameter IfExists { get; set; }

        /// <inheritdoc />
        protected override void ProcessRecord()
        {
            base.ProcessRecord();

            string action = $"Remove screen '{ScreenName}' from Canvas app '{CanvasAppId}'";
            if (!ShouldProcess(action, action, "Remove Screen"))
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

            // Modify .msapp file to remove screen
            byte[] modifiedMsappBytes = RemoveMsAppScreen(msappBytes);

            // Update Canvas app with modified .msapp file
            var updateEntity = new Entity("canvasapp")
            {
                Id = CanvasAppId,
                ["document"] = Convert.ToBase64String(modifiedMsappBytes)
            };

            Connection.Update(updateEntity);

            WriteVerbose($"Screen '{ScreenName}' removed successfully from Canvas app");
        }

        private byte[] RemoveMsAppScreen(byte[] originalBytes)
        {
            using (var memoryStream = new MemoryStream(originalBytes))
            using (var resultStream = new MemoryStream())
            {
                bool screenFound = false;
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
                            screenFound = true;
                            WriteVerbose($"Removing screen '{ScreenName}'");
                            // Skip this entry (don't add it to output)
                            continue;
                        }

                        // Copy entry as-is
                        byte[] entryBytes = ReadZipEntryBytes(zipInputStream);
                        AddZipEntry(zipOutputStream, entry.Name, entryBytes);
                    }

                    zipOutputStream.Finish();
                }

                if (!screenFound && !IfExists.IsPresent)
                {
                    ThrowTerminatingError(new ErrorRecord(
                        new Exception($"Screen '{ScreenName}' not found in Canvas app"),
                        "ScreenNotFound",
                        ErrorCategory.ObjectNotFound,
                        ScreenName));
                }
                else if (!screenFound)
                {
                    WriteVerbose($"Screen '{ScreenName}' not found (IfExists flag set, skipping)");
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
