using System;
using System.IO;
using System.Management.Automation;

namespace Rnwood.Dataverse.Data.PowerShell.Commands
{
    /// <summary>
    /// Lists Canvas control templates from embedded all-controls resources, optionally augmented by templates in a provided .msapp file.
    /// When the same template exists in both sources, the latest version is returned.
    /// </summary>
    [Cmdlet(VerbsCommon.Get, "DataverseMsAppControlTemplate", DefaultParameterSetName = "Embedded")]
    [OutputType(typeof(PSObject))]
    public class GetDataverseMsAppControlTemplateCmdlet : PSCmdlet
    {
        /// <summary>
        /// Gets or sets the path to the .msapp file used to augment embedded control template metadata.
        /// </summary>
        [Parameter(Mandatory = true, Position = 0, ParameterSetName = "FromPath", HelpMessage = "Path to the .msapp file used to augment embedded control template metadata")]
        [ValidateNotNullOrEmpty]
        public string MsAppPath { get; set; }

        /// <summary>
        /// Gets or sets the Canvas app PSObject returned by Get-DataverseCanvasApp with -IncludeDocument.
        /// </summary>
        [Parameter(Mandatory = true, Position = 0, ParameterSetName = "FromObject", ValueFromPipeline = true, HelpMessage = "Canvas app PSObject from Get-DataverseCanvasApp -IncludeDocument")]
        [ValidateNotNull]
        public PSObject CanvasApp { get; set; }

        /// <summary>
        /// Gets or sets the template name filter pattern. Supports wildcards.
        /// </summary>
        [Parameter(HelpMessage = "Template name pattern to filter by. Supports wildcards (* and ?)")]
        public string TemplateName { get; set; }

        /// <inheritdoc />
        protected override void ProcessRecord()
        {
            base.ProcessRecord();

            string tempMsappPath = null;
            string effectiveMsappPath = null;

            try
            {
                if (ParameterSetName == "FromObject")
                {
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

                    tempMsappPath = Path.Combine(Path.GetTempPath(), $"msapp_{Guid.NewGuid():N}.msapp");
                    File.WriteAllBytes(tempMsappPath, Convert.FromBase64String(documentProp.Value.ToString()));
                    effectiveMsappPath = tempMsappPath;
                }
                else if (ParameterSetName == "FromPath")
                {
                    effectiveMsappPath = GetUnresolvedProviderPathFromPSPath(MsAppPath);
                    if (!File.Exists(effectiveMsappPath))
                    {
                        ThrowTerminatingError(new ErrorRecord(
                            new FileNotFoundException($"MsApp file not found: {effectiveMsappPath}"),
                            "FileNotFound",
                            ErrorCategory.ObjectNotFound,
                            effectiveMsappPath));
                        return;
                    }
                }

                var templates = MsAppToolkit.YamlFirstPackaging.ListControlTemplates(effectiveMsappPath);
                WildcardPattern namePattern = null;
                if (!string.IsNullOrWhiteSpace(TemplateName))
                {
                    namePattern = new WildcardPattern(TemplateName, WildcardOptions.IgnoreCase);
                }

                foreach (var template in templates)
                {
                    if (namePattern != null && !namePattern.IsMatch(template.Name))
                    {
                        continue;
                    }

                    var obj = new PSObject();
                    obj.Properties.Add(new PSNoteProperty("Name", template.Name));
                    obj.Properties.Add(new PSNoteProperty("Version", template.Version));
                    obj.Properties.Add(new PSNoteProperty("TemplateId", template.TemplateId));
                    obj.Properties.Add(new PSNoteProperty("YamlControlName", template.YamlControlName));
                    obj.Properties.Add(new PSNoteProperty("RequiresVariantKeyword", template.RequiresVariantKeyword));
                    obj.Properties.Add(new PSNoteProperty("AvailableVariants", template.AvailableVariants));
                    obj.Properties.Add(new PSNoteProperty("AppFlagRequirements", template.AppFlagRequirements));
                    WriteObject(obj);
                }
            }
            finally
            {
                if (tempMsappPath != null && File.Exists(tempMsappPath))
                {
                    try { File.Delete(tempMsappPath); } catch { /* Ignore cleanup errors */ }
                }
            }
        }
    }
}
