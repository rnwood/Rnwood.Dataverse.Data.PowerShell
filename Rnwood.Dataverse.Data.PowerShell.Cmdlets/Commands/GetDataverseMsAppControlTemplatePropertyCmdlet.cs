using System;
using System.IO;
using System.Management.Automation;

namespace Rnwood.Dataverse.Data.PowerShell.Commands
{
    /// <summary>
    /// Lists properties for a Canvas control template from embedded all-controls resources,
    /// optionally augmented by templates in a provided .msapp file.
    /// When the same template exists in both sources, the latest version is selected unless -TemplateVersion is specified.
    /// </summary>
    [Cmdlet(VerbsCommon.Get, "DataverseMsAppControlTemplateProperty", DefaultParameterSetName = "Embedded")]
    [OutputType(typeof(PSObject))]
    public class GetDataverseMsAppControlTemplatePropertyCmdlet : PSCmdlet
    {
        /// <summary>
        /// Gets or sets the template name to describe.
        /// </summary>
        [Parameter(Mandatory = true, Position = 0, HelpMessage = "Template name to describe")]
        [ValidateNotNullOrEmpty]
        public string TemplateName { get; set; }

        /// <summary>
        /// Gets or sets the optional template version. If omitted, the latest version is used.
        /// </summary>
        [Parameter(HelpMessage = "Optional template version. If omitted, the latest version is used")]
        public string TemplateVersion { get; set; }

        /// <summary>
        /// Gets or sets the path to the .msapp file used to augment embedded control template metadata.
        /// </summary>
        [Parameter(Mandatory = true, Position = 1, ParameterSetName = "FromPath", HelpMessage = "Path to the .msapp file used to augment embedded control template metadata")]
        [ValidateNotNullOrEmpty]
        public string MsAppPath { get; set; }

        /// <summary>
        /// Gets or sets the Canvas app PSObject returned by Get-DataverseCanvasApp with -IncludeDocument.
        /// </summary>
        [Parameter(Mandatory = true, Position = 1, ParameterSetName = "FromObject", ValueFromPipeline = true, HelpMessage = "Canvas app PSObject from Get-DataverseCanvasApp -IncludeDocument")]
        [ValidateNotNull]
        public PSObject CanvasApp { get; set; }

        /// <summary>
        /// Gets or sets whether contextual auto-layout child properties should also be returned.
        /// </summary>
        [Parameter(HelpMessage = "Include contextual auto-layout child properties that are injected by runtime")]
        public SwitchParameter IncludeContextualDynamicProperties { get; set; }

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

                var details = MsAppToolkit.YamlFirstPackaging.DescribeTemplate(TemplateName, TemplateVersion, effectiveMsappPath);

                foreach (var prop in details.Properties)
                {
                    var obj = new PSObject();
                    obj.Properties.Add(new PSNoteProperty("TemplateName", details.Name));
                    obj.Properties.Add(new PSNoteProperty("TemplateVersion", details.Version));
                    obj.Properties.Add(new PSNoteProperty("TemplateId", details.TemplateId));
                    obj.Properties.Add(new PSNoteProperty("YamlControlName", details.YamlControlName));
                    obj.Properties.Add(new PSNoteProperty("PropertyName", prop.PropertyName));
                    obj.Properties.Add(new PSNoteProperty("DefaultValue", prop.DefaultValue));
                    obj.Properties.Add(new PSNoteProperty("IsContextualDynamicProperty", false));
                    obj.Properties.Add(new PSNoteProperty("RequiresVariantKeyword", details.RequiresVariantKeyword));
                    obj.Properties.Add(new PSNoteProperty("AvailableVariants", details.AvailableVariants));
                    obj.Properties.Add(new PSNoteProperty("AppFlagRequirements", details.AppFlagRequirements));
                    WriteObject(obj);
                }

                if (IncludeContextualDynamicProperties)
                {
                    foreach (var prop in details.ContextualDynamicProperties)
                    {
                        var obj = new PSObject();
                        obj.Properties.Add(new PSNoteProperty("TemplateName", details.Name));
                        obj.Properties.Add(new PSNoteProperty("TemplateVersion", details.Version));
                        obj.Properties.Add(new PSNoteProperty("TemplateId", details.TemplateId));
                        obj.Properties.Add(new PSNoteProperty("YamlControlName", details.YamlControlName));
                        obj.Properties.Add(new PSNoteProperty("PropertyName", prop.PropertyName));
                        obj.Properties.Add(new PSNoteProperty("DefaultValue", prop.DefaultValue));
                        obj.Properties.Add(new PSNoteProperty("IsContextualDynamicProperty", true));
                        obj.Properties.Add(new PSNoteProperty("RequiresVariantKeyword", details.RequiresVariantKeyword));
                        obj.Properties.Add(new PSNoteProperty("AvailableVariants", details.AvailableVariants));
                        obj.Properties.Add(new PSNoteProperty("AppFlagRequirements", details.AppFlagRequirements));
                        WriteObject(obj);
                    }
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
