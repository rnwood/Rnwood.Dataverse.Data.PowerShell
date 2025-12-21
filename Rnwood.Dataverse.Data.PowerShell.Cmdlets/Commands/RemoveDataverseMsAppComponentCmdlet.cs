using ICSharpCode.SharpZipLib.Zip;
using System;
using System.IO;
using System.Management.Automation;

namespace Rnwood.Dataverse.Data.PowerShell.Commands
{
    /// <summary>
    /// Removes a component from a .msapp file.
    /// </summary>
    [Cmdlet(VerbsCommon.Remove, "DataverseMsAppComponent", SupportsShouldProcess = true, ConfirmImpact = ConfirmImpact.High)]
    public class RemoveDataverseMsAppComponentCmdlet : PSCmdlet
    {
        /// <summary>
        /// Gets or sets the path to the .msapp file.
        /// </summary>
        [Parameter(Mandatory = true, Position = 0, HelpMessage = "Path to the .msapp file")]
        [ValidateNotNullOrEmpty]
        public string MsAppPath { get; set; }

        /// <summary>
        /// Gets or sets the component name to remove.
        /// </summary>
        [Parameter(Mandatory = true, Position = 1, HelpMessage = "Name of the component to remove")]
        [ValidateNotNullOrEmpty]
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

            string action = $"Remove component '{ComponentName}' from .msapp file '{filePath}'";
            if (!ShouldProcess(action, action, "Remove Component"))
            {
                return;
            }

            byte[] msappBytes = File.ReadAllBytes(filePath);
            byte[] modifiedBytes = RemoveMsAppComponent(msappBytes);
            File.WriteAllBytes(filePath, modifiedBytes);

            WriteVerbose($"Component '{ComponentName}' removed successfully from .msapp file");
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
                        new Exception($"Component '{ComponentName}' not found in .msapp file"),
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
