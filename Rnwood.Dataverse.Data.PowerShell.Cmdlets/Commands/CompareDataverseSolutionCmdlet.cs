using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Management.Automation;
using System.Xml.Linq;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Query;

namespace Rnwood.Dataverse.Data.PowerShell.Commands
{
    /// <summary>
    /// Compares a solution file with the state of that solution in the target environment.
    /// </summary>
    [Cmdlet(VerbsData.Compare, "DataverseSolution")]
    [OutputType(typeof(PSObject))]
    public class CompareDataverseSolutionCmdlet : OrganizationServiceCmdlet
    {
        /// <summary>
        /// Gets or sets the path to the solution file to compare.
        /// </summary>
        [Parameter(Mandatory = true, Position = 0, ParameterSetName = "FromFile", HelpMessage = "Path to the solution file (.zip) to compare.")]
        [ValidateNotNullOrEmpty]
        public string SolutionFile { get; set; }

        /// <summary>
        /// Gets or sets the solution file bytes to compare.
        /// </summary>
        [Parameter(Mandatory = true, ValueFromPipeline = true, ParameterSetName = "FromBytes", HelpMessage = "Solution file bytes to compare.")]
        public byte[] SolutionBytes { get; set; }

        /// <summary>
        /// Processes the cmdlet request.
        /// </summary>
        protected override void ProcessRecord()
        {
            base.ProcessRecord();

            // Load solution file
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

            // Extract solution info and components from file
            var (solutionUniqueName, fileComponents) = ExtractSolutionComponents(solutionBytes);
            WriteVerbose($"Extracted solution: {solutionUniqueName} with {fileComponents.Count} root components");

            // Query target environment for the solution
            var solutionQuery = new QueryExpression("solution")
            {
                ColumnSet = new ColumnSet("solutionid", "uniquename", "friendlyname"),
                Criteria = new FilterExpression
                {
                    Conditions =
                    {
                        new ConditionExpression("uniquename", ConditionOperator.Equal, solutionUniqueName)
                    }
                },
                TopCount = 1
            };

            var solutions = Connection.RetrieveMultiple(solutionQuery);
            if (solutions.Entities.Count == 0)
            {
                WriteWarning($"Solution '{solutionUniqueName}' not found in target environment. All components will be marked as 'Added'.");
                
                // Output all file components as Added
                foreach (var component in fileComponents)
                {
                    OutputComparisonResult(component.ComponentType, component.ObjectId, component.RootComponentBehavior, 
                        null, "Added", solutionUniqueName);
                }
                return;
            }

            var solutionId = solutions.Entities[0].Id;
            WriteVerbose($"Found solution in target environment: {solutionId}");

            // Query target environment for solution components
            var envComponents = GetEnvironmentComponents(solutionId);
            WriteVerbose($"Found {envComponents.Count} components in target environment");

            // Compare components
            CompareComponents(fileComponents, envComponents, solutionUniqueName);
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

        private void CompareComponents(List<SolutionComponentInfo> fileComponents, 
            List<SolutionComponentInfo> envComponents, string solutionName)
        {
            // Create lookup dictionaries for efficient comparison
            var fileComponentDict = fileComponents
                .GroupBy(c => new { c.ComponentType, c.ObjectId })
                .ToDictionary(g => g.Key, g => g.First());

            var envComponentDict = envComponents
                .GroupBy(c => new { c.ComponentType, c.ObjectId })
                .ToDictionary(g => g.Key, g => g.First());

            // Find added components (in file but not in environment)
            foreach (var fileComponent in fileComponents)
            {
                var key = new { fileComponent.ComponentType, fileComponent.ObjectId };
                
                if (!envComponentDict.ContainsKey(key))
                {
                    OutputComparisonResult(fileComponent.ComponentType, fileComponent.ObjectId, 
                        fileComponent.RootComponentBehavior, null, "Added", solutionName);
                }
                else
                {
                    var envComponent = envComponentDict[key];
                    
                    // Check if behavior changed
                    if (fileComponent.RootComponentBehavior != envComponent.RootComponentBehavior)
                    {
                        // Behavior changed - consider it modified
                        OutputComparisonResult(fileComponent.ComponentType, fileComponent.ObjectId,
                            fileComponent.RootComponentBehavior, envComponent.RootComponentBehavior, 
                            "Modified", solutionName);
                    }
                    else
                    {
                        // Component exists in both with same behavior - assume modified
                        // (We can't detect actual changes without inspecting the component itself)
                        OutputComparisonResult(fileComponent.ComponentType, fileComponent.ObjectId,
                            fileComponent.RootComponentBehavior, envComponent.RootComponentBehavior, 
                            "Modified", solutionName);
                    }
                }
            }

            // Find removed components (in environment but not in file)
            foreach (var envComponent in envComponents)
            {
                var key = new { envComponent.ComponentType, envComponent.ObjectId };
                
                if (!fileComponentDict.ContainsKey(key))
                {
                    OutputComparisonResult(envComponent.ComponentType, envComponent.ObjectId,
                        null, envComponent.RootComponentBehavior, "Removed", solutionName);
                }
            }
        }

        private void OutputComparisonResult(int componentType, Guid objectId, 
            int? fileBehavior, int? envBehavior, string status, string solutionName)
        {
            var result = new PSObject();
            result.Properties.Add(new PSNoteProperty("SolutionName", solutionName));
            result.Properties.Add(new PSNoteProperty("ComponentType", componentType));
            result.Properties.Add(new PSNoteProperty("ComponentTypeName", GetComponentTypeName(componentType)));
            result.Properties.Add(new PSNoteProperty("ObjectId", objectId));
            result.Properties.Add(new PSNoteProperty("Status", status));
            result.Properties.Add(new PSNoteProperty("FileBehavior", fileBehavior.HasValue ? GetBehaviorName(fileBehavior.Value) : null));
            result.Properties.Add(new PSNoteProperty("EnvironmentBehavior", envBehavior.HasValue ? GetBehaviorName(envBehavior.Value) : null));
            
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
