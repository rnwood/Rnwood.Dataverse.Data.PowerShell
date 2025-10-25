using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Management.Automation;
using System.Xml.Linq;
using Microsoft.PowerPlatform.Dataverse.Client;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Query;

namespace Rnwood.Dataverse.Data.PowerShell.Commands
{
    /// <summary>
    /// Compares a solution file with the state of that solution in the target environment or with another solution file.
    /// </summary>
    [Cmdlet(VerbsData.Compare, "DataverseSolution")]
    [OutputType(typeof(PSObject))]
    public class CompareDataverseSolutionCmdlet : PSCmdlet
    {
        /// <summary>
        /// Gets or sets the Dataverse connection.
        /// </summary>
        [Parameter(Mandatory = true, ParameterSetName = "FileToEnvironment", HelpMessage = "Dataverse connection for comparing with environment.")]
        [Parameter(Mandatory = true, ParameterSetName = "BytesToEnvironment", HelpMessage = "Dataverse connection for comparing with environment.")]
        public ServiceClient Connection { get; set; }

        /// <summary>
        /// Gets or sets the path to the first solution file (source/reference) to compare.
        /// </summary>
        [Parameter(Mandatory = true, Position = 0, ParameterSetName = "FileToEnvironment", HelpMessage = "Path to the solution file (.zip) to compare with environment.")]
        [Parameter(Mandatory = true, Position = 0, ParameterSetName = "FileToFile", HelpMessage = "Path to the first solution file (.zip) to compare.")]
        [ValidateNotNullOrEmpty]
        public string SolutionFile { get; set; }

        /// <summary>
        /// Gets or sets the solution file bytes to compare.
        /// </summary>
        [Parameter(Mandatory = true, ValueFromPipeline = true, ParameterSetName = "BytesToEnvironment", HelpMessage = "Solution file bytes to compare with environment.")]
        public byte[] SolutionBytes { get; set; }

        /// <summary>
        /// Gets or sets the path to the second solution file (target) to compare against.
        /// </summary>
        [Parameter(Mandatory = true, Position = 1, ParameterSetName = "FileToFile", HelpMessage = "Path to the second solution file (.zip) to compare against.")]
        [ValidateNotNullOrEmpty]
        public string TargetSolutionFile { get; set; }

        /// <summary>
        /// Gets or sets whether to reverse the comparison direction (compare environment to file instead of file to environment).
        /// </summary>
        [Parameter(ParameterSetName = "FileToEnvironment", HelpMessage = "Compare environment to file instead of file to environment.")]
        [Parameter(ParameterSetName = "BytesToEnvironment", HelpMessage = "Compare environment to file instead of file to environment.")]
        public SwitchParameter ReverseComparison { get; set; }

        /// <summary>
        /// Processes the cmdlet request.
        /// </summary>
        protected override void ProcessRecord()
        {
            base.ProcessRecord();

            // Determine comparison mode
            bool isFileToFile = ParameterSetName == "FileToFile";
            bool isFileToEnvironment = ParameterSetName == "FileToEnvironment" || ParameterSetName == "BytesToEnvironment";

            // Load first solution file (source)
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

            // Extract solution info and components from source file
            var (sourceSolutionName, sourceComponents) = ExtractSolutionComponents(sourceSolutionBytes);
            WriteVerbose($"Extracted source solution: {sourceSolutionName} with {sourceComponents.Count} root components");

            List<SolutionComponentInfo> targetComponents;
            string targetSolutionName;

            if (isFileToFile)
            {
                // Load target solution file
                var targetFilePath = GetUnresolvedProviderPathFromPSPath(TargetSolutionFile);
                if (!File.Exists(targetFilePath))
                {
                    ThrowTerminatingError(new ErrorRecord(
                        new FileNotFoundException($"Target solution file not found: {targetFilePath}"),
                        "FileNotFound",
                        ErrorCategory.ObjectNotFound,
                        targetFilePath));
                    return;
                }

                WriteVerbose($"Loading target solution file from: {targetFilePath}");
                var targetSolutionBytes = File.ReadAllBytes(targetFilePath);
                WriteVerbose($"Target solution file size: {targetSolutionBytes.Length} bytes");

                (targetSolutionName, targetComponents) = ExtractSolutionComponents(targetSolutionBytes);
                WriteVerbose($"Extracted target solution: {targetSolutionName} with {targetComponents.Count} root components");
            }
            else
            {
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

                var solutions = Connection.RetrieveMultiple(solutionQuery);
                if (solutions.Entities.Count == 0)
                {
                    WriteWarning($"Solution '{sourceSolutionName}' not found in target environment. All components will be marked as 'Added'.");
                    
                    // Output all source components as Added
                    foreach (var component in sourceComponents)
                    {
                        OutputComparisonResult(component.ComponentType, component.ObjectId, component.RootComponentBehavior, 
                            null, "Added", sourceSolutionName, isReversed: ReverseComparison.IsPresent);
                    }
                    return;
                }

                var solutionId = solutions.Entities[0].Id;
                WriteVerbose($"Found solution in target environment: {solutionId}");

                // Query target environment for solution components
                targetComponents = GetEnvironmentComponents(solutionId);
                targetSolutionName = sourceSolutionName;
                WriteVerbose($"Found {targetComponents.Count} components in target environment");
            }

            // Apply reverse comparison if requested
            if (ReverseComparison.IsPresent && isFileToEnvironment)
            {
                WriteVerbose("Reversing comparison direction (environment to file)");
                var temp = sourceComponents;
                sourceComponents = targetComponents;
                targetComponents = temp;
            }

            // Compare components
            CompareComponents(sourceComponents, targetComponents, targetSolutionName);
        }

        private (string UniqueName, List<SolutionComponentInfo> Components) ExtractSolutionComponents(byte[] solutionBytes)
        {
            var components = new List<SolutionComponentInfo>();
            string uniqueName = null;

            using (var memoryStream = new MemoryStream(solutionBytes))
            using (var archive = new ZipArchive(memoryStream, ZipArchiveMode.Read))
            {
                // First, get the solution unique name from solution.xml
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
                        uniqueName = solutionManifest?.Element("UniqueName")?.Value;
                    }
                }

                if (string.IsNullOrEmpty(uniqueName))
                {
                    ThrowTerminatingError(new ErrorRecord(
                        new InvalidOperationException("Could not extract solution unique name from solution file."),
                        "InvalidSolutionFile",
                        ErrorCategory.InvalidData,
                        null));
                    return (null, components);
                }

                // Now parse customizations.xml for root components
                var customizationsEntry = archive.Entries.FirstOrDefault(e =>
                    e.FullName.Equals("customizations.xml", StringComparison.OrdinalIgnoreCase));

                if (customizationsEntry != null)
                {
                    using (var stream = customizationsEntry.Open())
                    using (var reader = new StreamReader(stream))
                    {
                        var xmlContent = reader.ReadToEnd();
                        var xdoc = XDocument.Parse(xmlContent);

                        // Find RootComponents section
                        var rootComponents = xdoc.Descendants()
                            .FirstOrDefault(e => e.Name.LocalName == "RootComponents");

                        if (rootComponents != null)
                        {
                            foreach (var rootComponent in rootComponents.Elements())
                            {
                                if (rootComponent.Name.LocalName == "RootComponent")
                                {
                                    var idAttr = rootComponent.Attribute("id");
                                    var typeAttr = rootComponent.Attribute("type");
                                    var behaviorAttr = rootComponent.Attribute("behavior");

                                    if (idAttr != null && typeAttr != null)
                                    {
                                        var component = new SolutionComponentInfo
                                        {
                                            ObjectId = Guid.Parse(idAttr.Value),
                                            ComponentType = int.Parse(typeAttr.Value),
                                            RootComponentBehavior = behaviorAttr != null ? int.Parse(behaviorAttr.Value) : 0
                                        };
                                        components.Add(component);
                                    }
                                }
                            }
                        }
                    }
                }
            }

            return (uniqueName, components);
        }

        private List<SolutionComponentInfo> GetEnvironmentComponents(Guid solutionId)
        {
            var components = new List<SolutionComponentInfo>();

            var query = new QueryExpression("solutioncomponent")
            {
                ColumnSet = new ColumnSet("objectid", "componenttype", "rootcomponentbehavior"),
                Criteria = new FilterExpression
                {
                    Conditions =
                    {
                        new ConditionExpression("solutionid", ConditionOperator.Equal, solutionId)
                    }
                }
            };

            var results = Connection.RetrieveMultiple(query);

            foreach (var entity in results.Entities)
            {
                var component = new SolutionComponentInfo
                {
                    ObjectId = entity.GetAttributeValue<Guid>("objectid"),
                    ComponentType = entity.GetAttributeValue<int>("componenttype"),
                    RootComponentBehavior = entity.Contains("rootcomponentbehavior") 
                        ? entity.GetAttributeValue<int>("rootcomponentbehavior") 
                        : 0
                };
                components.Add(component);
            }

            return components;
        }

        private void CompareComponents(List<SolutionComponentInfo> sourceComponents, 
            List<SolutionComponentInfo> targetComponents, string solutionName)
        {
            // Create lookup dictionaries for efficient comparison
            var sourceComponentDict = sourceComponents
                .GroupBy(c => new { c.ComponentType, c.ObjectId })
                .ToDictionary(g => g.Key, g => g.First());

            var targetComponentDict = targetComponents
                .GroupBy(c => new { c.ComponentType, c.ObjectId })
                .ToDictionary(g => g.Key, g => g.First());

            // Find added components (in source but not in target)
            foreach (var sourceComponent in sourceComponents)
            {
                var key = new { sourceComponent.ComponentType, sourceComponent.ObjectId };
                
                if (!targetComponentDict.ContainsKey(key))
                {
                    OutputComparisonResult(sourceComponent.ComponentType, sourceComponent.ObjectId, 
                        sourceComponent.RootComponentBehavior, null, "Added", solutionName, isReversed: false);
                }
                else
                {
                    var targetComponent = targetComponentDict[key];
                    
                    // Check if behavior changed
                    if (sourceComponent.RootComponentBehavior != targetComponent.RootComponentBehavior)
                    {
                        // Determine if this is an inclusion or exclusion based on behavior change
                        string status = DetermineBehaviorChangeStatus(sourceComponent.RootComponentBehavior, targetComponent.RootComponentBehavior);
                        
                        OutputComparisonResult(sourceComponent.ComponentType, sourceComponent.ObjectId,
                            sourceComponent.RootComponentBehavior, targetComponent.RootComponentBehavior, 
                            status, solutionName, isReversed: false);
                    }
                    else
                    {
                        // Component exists in both with same behavior - assume modified
                        // (We can't detect actual changes without inspecting the component itself)
                        OutputComparisonResult(sourceComponent.ComponentType, sourceComponent.ObjectId,
                            sourceComponent.RootComponentBehavior, targetComponent.RootComponentBehavior, 
                            "Modified", solutionName, isReversed: false);
                    }
                }
            }

            // Find removed components (in target but not in source)
            foreach (var targetComponent in targetComponents)
            {
                var key = new { targetComponent.ComponentType, targetComponent.ObjectId };
                
                if (!sourceComponentDict.ContainsKey(key))
                {
                    OutputComparisonResult(targetComponent.ComponentType, targetComponent.ObjectId,
                        null, targetComponent.RootComponentBehavior, "Removed", solutionName, isReversed: false);
                }
            }
        }

        private string DetermineBehaviorChangeStatus(int sourceBehavior, int targetBehavior)
        {
            // Behavior levels: 0 (Full/Include Subcomponents) > 1 (Do Not Include Subcomponents) > 2 (Shell)
            // Going from higher number to lower number = including more data (BehaviorIncluded)
            // Going from lower number to higher number = excluding data (BehaviorExcluded)
            
            if (sourceBehavior < targetBehavior)
            {
                // e.g., 0 (Full) -> 2 (Shell): excluding/removing data
                return "BehaviorExcluded";
            }
            else if (sourceBehavior > targetBehavior)
            {
                // e.g., 2 (Shell) -> 0 (Full): including more data
                return "BehaviorIncluded";
            }
            
            return "Modified";
        }

        private void OutputComparisonResult(int componentType, Guid objectId, 
            int? sourceBehavior, int? targetBehavior, string status, string solutionName, bool isReversed)
        {
            var result = new PSObject();
            result.Properties.Add(new PSNoteProperty("SolutionName", solutionName));
            result.Properties.Add(new PSNoteProperty("ComponentType", componentType));
            result.Properties.Add(new PSNoteProperty("ComponentTypeName", GetComponentTypeName(componentType)));
            result.Properties.Add(new PSNoteProperty("ObjectId", objectId));
            result.Properties.Add(new PSNoteProperty("Status", status));
            result.Properties.Add(new PSNoteProperty("SourceBehavior", sourceBehavior.HasValue ? GetBehaviorName(sourceBehavior.Value) : null));
            result.Properties.Add(new PSNoteProperty("TargetBehavior", targetBehavior.HasValue ? GetBehaviorName(targetBehavior.Value) : null));
            
            WriteObject(result);
        }

        private string GetComponentTypeName(int componentType)
        {
            // Map common component types to names
            // Full list: https://learn.microsoft.com/en-us/power-apps/developer/data-platform/reference/entities/solutioncomponent
            switch (componentType)
            {
                case 1: return "Entity";
                case 2: return "Attribute";
                case 3: return "Relationship";
                case 4: return "Attribute Picklist Value";
                case 5: return "Attribute Lookup Value";
                case 6: return "View Query";
                case 7: return "Localized Label";
                case 8: return "Relationship Extra Condition";
                case 9: return "Option Set";
                case 10: return "Entity Relationship";
                case 11: return "Entity Relationship Role";
                case 12: return "Entity Relationship Relationships";
                case 13: return "Managed Property";
                case 14: return "Entity Key";
                case 16: return "Privilege";
                case 17: return "Privilege Object Type Code";
                case 18: return "Index";
                case 20: return "Role";
                case 21: return "Role Privilege";
                case 22: return "Display String";
                case 23: return "Display String Map";
                case 24: return "Form";
                case 25: return "Organization";
                case 26: return "Saved Query";
                case 29: return "Workflow";
                case 31: return "Report";
                case 32: return "Report Entity";
                case 33: return "Report Category";
                case 34: return "Report Visibility";
                case 35: return "Attachment";
                case 36: return "Email Template";
                case 37: return "Contract Template";
                case 38: return "KB Article Template";
                case 39: return "Mail Merge Template";
                case 44: return "Duplicate Rule";
                case 45: return "Duplicate Rule Condition";
                case 46: return "Entity Map";
                case 47: return "Attribute Map";
                case 48: return "Ribbon Command";
                case 49: return "Ribbon Context Group";
                case 50: return "Ribbon Customization";
                case 52: return "Ribbon Rule";
                case 53: return "Ribbon Tab To Command Map";
                case 55: return "Ribbon Diff";
                case 59: return "Saved Query Visualization";
                case 60: return "System Form";
                case 61: return "Web Resource";
                case 62: return "Site Map";
                case 63: return "Connection Role";
                case 64: return "Complex Control";
                case 65: return "Hierarchy Rule";
                case 66: return "Custom Control";
                case 68: return "Custom Control Default Config";
                case 70: return "Field Security Profile";
                case 71: return "Field Permission";
                case 90: return "Plugin Type";
                case 91: return "Plugin Assembly";
                case 92: return "SDK Message Processing Step";
                case 93: return "SDK Message Processing Step Image";
                case 95: return "Service Endpoint";
                case 150: return "Routing Rule";
                case 151: return "Routing Rule Item";
                case 152: return "SLA";
                case 153: return "SLA Item";
                case 154: return "Convert Rule";
                case 155: return "Convert Rule Item";
                case 161: return "Mobile Offline Profile";
                case 162: return "Mobile Offline Profile Item";
                case 165: return "Similarity Rule";
                case 166: return "Data Source Mapping";
                case 201: return "SDKMessage";
                case 202: return "SDKMessageFilter";
                case 203: return "SdkMessagePair";
                case 204: return "SdkMessageRequest";
                case 205: return "SdkMessageRequestField";
                case 206: return "SdkMessageResponse";
                case 207: return "SdkMessageResponseField";
                case 208: return "Import Map";
                case 210: return "WebWizard";
                case 300: return "Canvas App";
                case 371: return "Connector";
                case 372: return "Connector";
                case 380: return "Environment Variable Definition";
                case 381: return "Environment Variable Value";
                case 400: return "AI Project Type";
                case 401: return "AI Project";
                case 402: return "AI Configuration";
                case 430: return "Model-Driven App";
                default: return $"Unknown ({componentType})";
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

        private class SolutionComponentInfo
        {
            public Guid ObjectId { get; set; }
            public int ComponentType { get; set; }
            public int RootComponentBehavior { get; set; }
        }
    }
}
