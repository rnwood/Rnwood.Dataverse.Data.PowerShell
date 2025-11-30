using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Management.Automation;
using Microsoft.PowerPlatform.Dataverse.Client;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Query;
using Rnwood.Dataverse.Data.PowerShell.Commands.Model;
using System.IO.Compression;
using System.Xml.Linq;

namespace Rnwood.Dataverse.Data.PowerShell.Commands
{
    /// <summary>
    /// Compares a solution file with the state of that solution in the target environment.
    /// </summary>
    [Cmdlet(VerbsData.Compare, "DataverseSolutionComponents")]
    [OutputType(typeof(PSObject))]
    public class CompareDataverseSolutionComponentsCmdlet : OrganizationServiceCmdlet
    {
        /// <summary>
        /// Gets or sets the Dataverse connection.
        /// </summary>
        [Parameter(Mandatory = false, ParameterSetName = "FileToEnvironment", HelpMessage = "Dataverse connection for comparing with environment.")]
        [Parameter(Mandatory = false, ParameterSetName = "BytesToEnvironment", HelpMessage = "Dataverse connection for comparing with environment.")]
        public override ServiceClient Connection { get => base.Connection; set => base.Connection = value; }

        /// <summary>
        /// Gets or sets the path to the solution file to compare.
        /// </summary>
        [Parameter(Mandatory = true, Position = 0, ParameterSetName = "FileToEnvironment", HelpMessage = "Path to the solution file (.zip) to compare with environment.")]
        [ValidateNotNullOrEmpty]
        public string SolutionFile { get; set; }

        /// <summary>
        /// Gets or sets the solution file bytes to compare.
        /// </summary>
        [Parameter(Mandatory = true, ValueFromPipeline = true, ParameterSetName = "BytesToEnvironment", HelpMessage = "Solution file bytes to compare with environment.")]
        public byte[] SolutionBytes { get; set; }

        /// <summary>
        /// Switch to compare a solution file with the environment.
        /// </summary>
        [Parameter(Mandatory = true, ParameterSetName = "FileToEnvironment", HelpMessage = "Compare a solution file with the environment.")]
        public SwitchParameter FileToEnvironment { get; set; }

        /// <summary>
        /// Switch to compare solution bytes with the environment.
        /// </summary>
        [Parameter(Mandatory = true, ParameterSetName = "BytesToEnvironment", HelpMessage = "Compare solution bytes with the environment.")]
        public SwitchParameter BytesToEnvironment { get; set; }

        /// <summary>
        /// Gets or sets whether to reverse the comparison direction (compare environment to solution instead of solution to environment).
        /// </summary>
        [Parameter(Mandatory = false, ParameterSetName = "FileToEnvironment", HelpMessage = "Reverse the comparison direction.")]
        [Parameter(Mandatory = false, ParameterSetName = "BytesToEnvironment", HelpMessage = "Reverse the comparison direction.")]
        public SwitchParameter ReverseComparison { get; set; }

        // Private fields to store solution file bytes for subcomponent extraction
        private byte[] _sourceSolutionBytes;

        /// <summary>
        /// Processes the cmdlet request.
        /// </summary>
        protected override void ProcessRecord()
        {
            base.ProcessRecord();

            // Load solution file (source)
            byte[] sourceSolutionBytes;
            if (ParameterSetName == "BytesToEnvironment")
            {
                sourceSolutionBytes = SolutionBytes;
            }
            else
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
                sourceSolutionBytes = File.ReadAllBytes(filePath);
            }

            WriteVerbose($"Solution file size: {sourceSolutionBytes.Length} bytes");

            // Store the source bytes for subcomponent extraction
            _sourceSolutionBytes = sourceSolutionBytes;

            // Extract solution info and components from source file
            string sourceSolutionName = ExtractSolutionName(sourceSolutionBytes);
            var sourceExtractor = new FileComponentExtractor(Connection, this, sourceSolutionBytes);

            // Query target environment for the solution
            var solutionQuery = new QueryExpression("solution")
            {
                ColumnSet = new ColumnSet("solutionid", "uniquename", "friendlyname"),
                Criteria = new FilterExpression
                {
                    Conditions =
 {
new ConditionExpression("uniquename", ConditionOperator.Equal, sourceSolutionName)
   }
                },
                TopCount = 1
            };

            var solutions = QueryHelpers.RetrieveMultipleWithThrottlingRetry(Connection, solutionQuery);

            var solutionId = solutions.Entities[0].Id;
            WriteVerbose($"Found solution in target environment: {solutionId}");


            // Compare components
            CompareComponents(sourceSolutionName, solutionId);
        }

        private void OutputComparisonResult(SolutionComponent sourceComponent, SolutionComponent targetComponent,
       SolutionComponentStatus status, string solutionName, bool isReversed)
        {
            var result = new PSObject();
            result.Properties.Add(new PSNoteProperty("SolutionName", solutionName));

            int componentType = sourceComponent?.ComponentType ?? targetComponent?.ComponentType ?? 0;
            var dummyComponent = new SolutionComponent
            {
                ComponentType = componentType,
                UniqueName = sourceComponent?.UniqueName ?? targetComponent?.UniqueName
            };
            result.Properties.Add(new PSNoteProperty("ComponentType", componentType));
            result.Properties.Add(new PSNoteProperty("ComponentTypeName", ComponentTypeResolver.GetComponentTypeName(Connection, dummyComponent)));

            // Display the logical name if available, otherwise the ObjectId
            string displayIdentifier = GetDisplayIdentifier(sourceComponent) ?? GetDisplayIdentifier(targetComponent) ?? "Unknown";
            result.Properties.Add(new PSNoteProperty("DisplayIdentifier", displayIdentifier));

            // Add source and target ObjectIds
            result.Properties.Add(new PSNoteProperty("SourceObjectId", (object)sourceComponent?.ObjectId));
            result.Properties.Add(new PSNoteProperty("TargetObjectId", (object)targetComponent?.ObjectId));

            result.Properties.Add(new PSNoteProperty("Status", status.ToString()));
            
            var sourceBehaviorEnum = RootComponentBehaviorExtensions.FromInt(sourceComponent?.RootComponentBehavior);
            var targetBehaviorEnum = RootComponentBehaviorExtensions.FromInt(targetComponent?.RootComponentBehavior);
            
            result.Properties.Add(new PSNoteProperty("SourceBehavior", sourceBehaviorEnum));
            result.Properties.Add(new PSNoteProperty("TargetBehavior", targetBehaviorEnum));
            
            result.Properties.Add(new PSNoteProperty("IsSubcomponent", sourceComponent?.IsSubcomponent ?? targetComponent?.IsSubcomponent ?? false));

            if ((sourceComponent?.IsSubcomponent ?? targetComponent?.IsSubcomponent ?? false) &&
                (sourceComponent?.ParentComponentType.HasValue ?? targetComponent?.ParentComponentType.HasValue ?? false))
            {
                int? parentComponentType = sourceComponent?.ParentComponentType ?? targetComponent?.ParentComponentType;
                result.Properties.Add(new PSNoteProperty("ParentComponentType", parentComponentType.Value));
                result.Properties.Add(new PSNoteProperty("ParentComponentTypeName", ComponentTypeResolver.GetComponentTypeName(Connection, new SolutionComponent { ComponentType = parentComponentType.Value })));
                result.Properties.Add(new PSNoteProperty("ParentTableName", sourceComponent?.ParentTableName ?? targetComponent?.ParentTableName));
            }

            WriteObject(result);
        }

        private string ExtractSolutionName(byte[] solutionBytes)
        {
            using (var memoryStream = new MemoryStream(solutionBytes))
            using (var archive = new ZipArchive(memoryStream, ZipArchiveMode.Read))
            {
                var solutionXmlEntry = archive.Entries.FirstOrDefault(e =>
                   e.FullName.Equals("solution.xml", StringComparison.OrdinalIgnoreCase));

                if (solutionXmlEntry != null)
                {
                    using (var stream = solutionXmlEntry.Open())
                    using (var reader = new StreamReader(stream))
                    {
                        var xmlContent = reader.ReadToEnd();
                        var xdoc = XDocument.Parse(xmlContent);
                        var solutionManifest = xdoc.Root?.Element("SolutionManifest");
                        return solutionManifest?.Element("UniqueName")?.Value;
                    }
                }
            }
            return null;
        }

        private void CompareComponents(string solutionName, Guid? targetSolutionId)
        {
            // Create extractors
            IComponentExtractor sourceExtractor = new FileComponentExtractor(Connection, this, _sourceSolutionBytes);
            IComponentExtractor targetExtractor = new EnvironmentComponentExtractor(Connection, this, targetSolutionId.Value);

            // Compare components
            var comparer = new SolutionComponentComparer(sourceExtractor, targetExtractor, this);
            var comparisonResults = comparer.CompareComponents();

            // Output results
            foreach (var result in comparisonResults)
            {
                OutputComparisonResult(result.SourceComponent, result.TargetComponent, result.Status, solutionName, isReversed: false);
            }
        }

        /// <summary>
        /// Gets the display identifier for a component, including parent table name if available.
        /// </summary>
        private string GetDisplayIdentifier(SolutionComponent component)
        {
            if (component == null) return null;

            if (!string.IsNullOrEmpty(component.UniqueName))
            {
                string id = component.UniqueName;
                if (!string.IsNullOrEmpty(component.ParentTableName))
                {
                    id = $"{component.ParentTableName}.{id}";
                }
                return id;
            }

            return component.ObjectId?.ToString();
        }
    }
}
