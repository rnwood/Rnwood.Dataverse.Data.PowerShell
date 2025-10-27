using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Management.Automation;
using System.Xml.Linq;
using Microsoft.PowerPlatform.Dataverse.Client;
using Microsoft.Xrm.Sdk;
using Rnwood.Dataverse.Data.PowerShell.Commands.Model;

namespace Rnwood.Dataverse.Data.PowerShell.Commands
{
    /// <summary>
    /// Retrieves the root components from a solution file (.zip).
    /// </summary>
    [Cmdlet(VerbsCommon.Get, "DataverseSolutionFileComponent")]
    [OutputType(typeof(PSObject))]
    public class GetDataverseSolutionFileComponentCmdlet : OrganizationServiceCmdlet
    {
        /// <summary>
        /// Gets or sets the path to the solution file to analyze.
        /// </summary>
        [Parameter(Mandatory = true, Position = 0, ParameterSetName = "FromFile", HelpMessage = "Path to the solution file (.zip) to analyze.")]
        [ValidateNotNullOrEmpty]
        public string SolutionFile { get; set; }

        /// <summary>
        /// Gets or sets the solution file bytes to analyze.
        /// </summary>
        [Parameter(Mandatory = true, ValueFromPipeline = true, ParameterSetName = "FromBytes", HelpMessage = "Solution file bytes to analyze.")]
        public byte[] SolutionBytes { get; set; }

        /// <summary>
        /// Gets or sets whether to include subcomponents in the output.
        /// </summary>
        [Parameter(Mandatory = false, HelpMessage = "Include subcomponents (attributes, relationships, forms, views, etc.) from the solution file.")]
        public SwitchParameter IncludeSubcomponents { get; set; }

        /// <summary>
        /// Processes the cmdlet request.
        /// </summary>
        protected override void ProcessRecord()
        {
            base.ProcessRecord();

            // Load solution bytes
            byte[] solutionBytes;
            if (ParameterSetName == "FromFile")
            {
                var filePath = GetUnresolvedProviderPathFromPSPath(SolutionFile);
                if (!File.Exists(filePath))
                {
                    ThrowTerminatingError(new ErrorRecord(
                      new FileNotFoundException($"Solution file not found: {filePath}"),
                  "FileNotFound",
               ErrorCategory.ObjectNotFound,
                    filePath));
                    return;
                }

                WriteVerbose($"Loading solution file from: {filePath}");
                solutionBytes = File.ReadAllBytes(filePath);
            }
            else
            {
                solutionBytes = SolutionBytes;
            }

            WriteVerbose($"Solution file size: {solutionBytes.Length} bytes");

            // Extract components from the solution file
            var extractor = new FileComponentExtractor(Connection, this, solutionBytes);
            var components = extractor.GetComponents(IncludeSubcomponents.IsPresent);

            WriteVerbose($"Found {components.Count} components in the solution file.");

            // Output components
            foreach (var component in components)
            {
                OutputComponentAsObject(component);
            }
        }

        private string GetBehaviorName(int behavior)
        {
            switch (behavior)
            {
                case 0: return "Include Subcomponents";
                case 1: return "Do Not Include Subcomponents";
                case 2: return "Include As Shell";
                default: return $"Unknown ({behavior})";
            }
        }

        private void OutputComponentAsObject(SolutionComponent component)
        {
            var result = new PSObject();
            var displayIdentifier = component.UniqueName ?? component.ObjectId?.ToString() ?? "Unknown";
            result.Properties.Add(new PSNoteProperty("ObjectId", displayIdentifier));
            result.Properties.Add(new PSNoteProperty("ComponentType", component.ComponentType));
            result.Properties.Add(new PSNoteProperty("ComponentTypeName", ComponentTypeResolver.GetComponentTypeName(Connection, component)));
            result.Properties.Add(new PSNoteProperty("Behavior", GetBehaviorName(component.RootComponentBehavior ?? 0)));
            result.Properties.Add(new PSNoteProperty("IsSubcomponent", component.IsSubcomponent));

            if (component.IsSubcomponent)
            {
                result.Properties.Add(new PSNoteProperty("ParentComponentType", component.ParentComponentType));
                result.Properties.Add(new PSNoteProperty("ParentComponentTypeName", ComponentTypeResolver.GetComponentTypeName(Connection, new SolutionComponent { ComponentType = component.ParentComponentType.GetValueOrDefault() })));
                result.Properties.Add(new PSNoteProperty("ParentTableName", component.ParentTableName));
            }

            WriteObject(result);
        }
    }
}
