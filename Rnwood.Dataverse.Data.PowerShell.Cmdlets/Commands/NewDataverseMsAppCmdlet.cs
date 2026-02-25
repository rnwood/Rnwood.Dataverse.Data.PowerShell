using ICSharpCode.SharpZipLib.Zip;
using MsAppToolkit;
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

            byte[] msappBytes = YamlFirstPackaging.CreateDefaultMsApp();
            File.WriteAllBytes(filePath, msappBytes);

            WriteVerbose($"Created new .msapp file at: {filePath}");
            WriteObject(new FileInfo(filePath));
        }

    }
}
