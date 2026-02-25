using ICSharpCode.SharpZipLib.Zip;
using System;
using System.IO;
using System.Management.Automation;

namespace Rnwood.Dataverse.Data.PowerShell.Commands
{
    /// <summary>
    /// Removes a screen from a .msapp file.
    /// </summary>
    [Cmdlet(VerbsCommon.Remove, "DataverseMsAppScreen", SupportsShouldProcess = true, ConfirmImpact = ConfirmImpact.High)]
    public class RemoveDataverseMsAppScreenCmdlet : PSCmdlet
    {
        /// <summary>
        /// Gets or sets the path to the .msapp file.
        /// </summary>
        [Parameter(Mandatory = true, Position = 0, ParameterSetName = "FromPath", HelpMessage = "Path to the .msapp file")]
        [ValidateNotNullOrEmpty]
        public string MsAppPath { get; set; }

        /// <summary>
        /// Gets or sets the Canvas app PSObject returned by Get-DataverseCanvasApp with -IncludeDocument.
        /// The document property will be updated with the modified .msapp.
        /// </summary>
        [Parameter(Mandatory = true, Position = 0, ParameterSetName = "FromObject", ValueFromPipeline = true, HelpMessage = "Canvas app PSObject from Get-DataverseCanvasApp -IncludeDocument")]
        [ValidateNotNull]
        public PSObject CanvasApp { get; set; }

        /// <summary>
        /// Gets or sets the screen name to remove.
        /// </summary>
        [Parameter(Mandatory = true, Position = 1, HelpMessage = "Name of the screen to remove")]
        [ValidateNotNullOrEmpty]
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

            bool isFromObject = ParameterSetName == "FromObject";
            byte[] msappBytes;
            string targetPath = null;

            if (isFromObject)
            {
                // Get .msapp from the PSObject's document property
                var documentProp = CanvasApp.Properties["document"];
                if (documentProp == null || documentProp.Value == null)
                {
                    ThrowTerminatingError(new ErrorRecord(
                        new InvalidOperationException("CanvasApp object does not have a 'document' property. Use Get-DataverseCanvasApp with -IncludeDocument."),
                        "DocumentPropertyMissing",
                        ErrorCategory.InvalidArgument,
                        CanvasApp));
                    return;
                }

                string base64Document = documentProp.Value.ToString();
                msappBytes = Convert.FromBase64String(base64Document);
            }
            else
            {
                targetPath = GetUnresolvedProviderPathFromPSPath(MsAppPath);
                if (!File.Exists(targetPath))
                {
                    ThrowTerminatingError(new ErrorRecord(
                        new FileNotFoundException($"MsApp file not found: {targetPath}"),
                        "FileNotFound",
                        ErrorCategory.ObjectNotFound,
                        targetPath));
                    return;
                }

                msappBytes = File.ReadAllBytes(targetPath);
            }

            string action = isFromObject
                ? $"Remove screen '{ScreenName}' from Canvas app"
                : $"Remove screen '{ScreenName}' from .msapp file '{targetPath}'";
            
            if (!ShouldProcess(action, action, "Remove Screen"))
            {
                return;
            }

            byte[] modifiedBytes = RemoveMsAppScreen(msappBytes);
            
            if (isFromObject)
            {
                // Update the document property in the PSObject
                string base64 = Convert.ToBase64String(modifiedBytes);
                CanvasApp.Properties["document"].Value = base64;
                WriteVerbose($"Screen '{ScreenName}' removed successfully from Canvas app object");
            }
            else
            {
                File.WriteAllBytes(targetPath, modifiedBytes);
                WriteVerbose($"Screen '{ScreenName}' removed successfully from .msapp file");
            }
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
                        string entryName = entry.Name.Replace('\\', '/');
                        if (entryName == screenFileName)
                        {
                            screenFound = true;
                            WriteVerbose($"Removing screen '{ScreenName}'");
                            // Skip this entry (don't add it to output)
                            continue;
                        }

                        // Copy entry as-is
                        byte[] entryBytes = ReadZipEntryBytes(zipInputStream);
                        AddZipEntry(zipOutputStream, entryName, entryBytes);
                    }

                    zipOutputStream.Finish();
                }

                if (!screenFound && !IfExists.IsPresent)
                {
                    ThrowTerminatingError(new ErrorRecord(
                        new Exception($"Screen '{ScreenName}' not found in .msapp file"),
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
