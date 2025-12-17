using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Management.Automation;
using System.Management.Automation.Language;

namespace Rnwood.Dataverse.Data.PowerShell.Commands
{
    /// <summary>
    /// Provides tab-completion for ComponentType parameters with friendly names and numeric values.
    /// </summary>
    public class ComponentTypeArgumentCompleter : IArgumentCompleter
    {
        // Standard component types with their friendly names
        private static readonly Dictionary<int, ComponentTypeInfo> StandardComponentTypes = new Dictionary<int, ComponentTypeInfo>
        {
            { 1, new ComponentTypeInfo(1, "Entity", "Table") },
            { 2, new ComponentTypeInfo(2, "Attribute", "Column") },
            { 3, new ComponentTypeInfo(3, "Relationship", "Relationship") },
            { 4, new ComponentTypeInfo(4, "AttributePicklistValue", "Choice Option") },
            { 5, new ComponentTypeInfo(5, "AttributeLookupValue", "Lookup Value") },
            { 6, new ComponentTypeInfo(6, "ViewAttribute", "View Column") },
            { 7, new ComponentTypeInfo(7, "LocalizedLabel", "Localized Label") },
            { 8, new ComponentTypeInfo(8, "RelationshipExtraCondition", "Relationship Extra Condition") },
            { 9, new ComponentTypeInfo(9, "OptionSet", "Choice") },
            { 10, new ComponentTypeInfo(10, "EntityRelationship", "Entity Relationship") },
            { 11, new ComponentTypeInfo(11, "EntityRelationshipRole", "Entity Relationship Role") },
            { 12, new ComponentTypeInfo(12, "EntityRelationshipRelationships", "Entity Relationship Relationships") },
            { 13, new ComponentTypeInfo(13, "ManagedProperty", "Managed Property") },
            { 14, new ComponentTypeInfo(14, "EntityKey", "Alternate Key") },
            { 16, new ComponentTypeInfo(16, "Role", "Security Role") },
            { 17, new ComponentTypeInfo(17, "RolePrivilege", "Role Privilege") },
            { 18, new ComponentTypeInfo(18, "DisplayString", "Display String") },
            { 19, new ComponentTypeInfo(19, "DisplayStringMap", "Display String Map") },
            { 20, new ComponentTypeInfo(20, "Form", "Form (legacy)") },
            { 21, new ComponentTypeInfo(21, "Organization", "Organization") },
            { 22, new ComponentTypeInfo(22, "SavedQuery", "View (legacy)") },
            { 23, new ComponentTypeInfo(23, "Workflow", "Workflow (legacy)") },
            { 24, new ComponentTypeInfo(24, "Report", "Report") },
            { 25, new ComponentTypeInfo(25, "ReportEntity", "Report Entity") },
            { 26, new ComponentTypeInfo(26, "ReportCategory", "Report Category") },
            { 27, new ComponentTypeInfo(27, "ReportVisibility", "Report Visibility") },
            { 28, new ComponentTypeInfo(28, "Attachment", "Attachment") },
            { 29, new ComponentTypeInfo(29, "EmailTemplate", "Email Template") },
            { 30, new ComponentTypeInfo(30, "ContractTemplate", "Contract Template") },
            { 31, new ComponentTypeInfo(31, "KBArticleTemplate", "KB Article Template") },
            { 32, new ComponentTypeInfo(32, "MailMergeTemplate", "Mail Merge Template") },
            { 33, new ComponentTypeInfo(33, "DuplicateRule", "Duplicate Rule") },
            { 34, new ComponentTypeInfo(34, "DuplicateRuleCondition", "Duplicate Rule Condition") },
            { 35, new ComponentTypeInfo(35, "EntityMap", "Entity Map") },
            { 36, new ComponentTypeInfo(36, "AttributeMap", "Attribute Map") },
            { 37, new ComponentTypeInfo(37, "RibbonCommand", "Ribbon Command") },
            { 38, new ComponentTypeInfo(38, "RibbonContextGroup", "Ribbon Context Group") },
            { 39, new ComponentTypeInfo(39, "RibbonCustomization", "Ribbon Customization") },
            { 40, new ComponentTypeInfo(40, "RibbonRule", "Ribbon Rule") },
            { 41, new ComponentTypeInfo(41, "RibbonTabToCommandMap", "Ribbon Tab To Command Map") },
            { 42, new ComponentTypeInfo(42, "RibbonDiff", "Ribbon Diff") },
            { 44, new ComponentTypeInfo(44, "HierarchyRule", "Hierarchy Rule") },
            { 45, new ComponentTypeInfo(45, "CustomControl", "Custom Control") },
            { 46, new ComponentTypeInfo(46, "CustomControlDefaultConfig", "Custom Control Default Config") },
            { 48, new ComponentTypeInfo(48, "SystemForm", "Form") },
            { 49, new ComponentTypeInfo(49, "ImportMap", "Data Map") },
            { 50, new ComponentTypeInfo(50, "WebResource", "Web Resource") },
            { 52, new ComponentTypeInfo(52, "SiteMap", "Sitemap") },
            { 53, new ComponentTypeInfo(53, "ConnectionRole", "Connection Role") },
            { 55, new ComponentTypeInfo(55, "FieldSecurityProfile", "Field Security Profile") },
            { 59, new ComponentTypeInfo(59, "PluginType", "Plugin Type") },
            { 60, new ComponentTypeInfo(60, "PluginAssembly", "Plugin Assembly") },
            { 61, new ComponentTypeInfo(61, "SDKMessageProcessingStep", "SDK Message Processing Step") },
            { 62, new ComponentTypeInfo(62, "SDKMessageProcessingStepImage", "SDK Message Processing Step Image") },
            { 63, new ComponentTypeInfo(63, "ServiceEndpoint", "Service Endpoint") },
            { 64, new ComponentTypeInfo(64, "RoutingRule", "Routing Rule") },
            { 65, new ComponentTypeInfo(65, "RoutingRuleItem", "Routing Rule Item") },
            { 66, new ComponentTypeInfo(66, "SLA", "SLA") },
            { 67, new ComponentTypeInfo(67, "SLAItem", "SLA Item") },
            { 68, new ComponentTypeInfo(68, "ConvertRule", "Convert Rule") },
            { 69, new ComponentTypeInfo(69, "ConvertRuleItem", "Convert Rule Item") },
            { 70, new ComponentTypeInfo(70, "MobileOfflineProfile", "Mobile Offline Profile") },
            { 71, new ComponentTypeInfo(71, "MobileOfflineProfileItem", "Mobile Offline Profile Item") },
            { 72, new ComponentTypeInfo(72, "SimilarityRule", "Similarity Rule") },
            { 73, new ComponentTypeInfo(73, "DataSourceMapping", "Virtual Entity Data Source") },
            { 80, new ComponentTypeInfo(80, "SDKMessage", "SDK Message") },
            { 81, new ComponentTypeInfo(81, "SDKMessageFilter", "SDK Message Filter") },
            { 82, new ComponentTypeInfo(82, "SdkMessagePair", "SDK Message Pair") },
            { 83, new ComponentTypeInfo(83, "SdkMessageRequest", "SDK Message Request") },
            { 84, new ComponentTypeInfo(84, "SdkMessageRequestField", "SDK Message Request Field") },
            { 85, new ComponentTypeInfo(85, "SdkMessageResponse", "SDK Message Response") },
            { 86, new ComponentTypeInfo(86, "SdkMessageResponseField", "SDK Message Response Field") },
            { 90, new ComponentTypeInfo(90, "WebWizard", "Web Wizard") },
            { 91, new ComponentTypeInfo(91, "Index", "Entity Index") },
            { 92, new ComponentTypeInfo(92, "Article", "Article") },
            { 93, new ComponentTypeInfo(93, "ChannelAccessProfile", "Channel Access Profile") },
            { 95, new ComponentTypeInfo(95, "ChannelAccessProfileEntityAccessLevel", "Channel Access Profile Entity Access Level") },
            { 152, new ComponentTypeInfo(152, "AppModule", "Model-Driven App") },
            { 153, new ComponentTypeInfo(153, "AppModuleRoles", "App Module Roles") },
            { 154, new ComponentTypeInfo(154, "PluginPackage", "Plugin Package") },
            { 161, new ComponentTypeInfo(161, "Connector", "Connector") },
            { 162, new ComponentTypeInfo(162, "EnvironmentVariableDefinition", "Environment Variable Definition") },
            { 163, new ComponentTypeInfo(163, "EnvironmentVariableValue", "Environment Variable Value") },
            { 165, new ComponentTypeInfo(165, "AIProjectType", "AI Project Type") },
            { 166, new ComponentTypeInfo(166, "AIProject", "AI Project") },
            { 167, new ComponentTypeInfo(167, "AIConfiguration", "AI Configuration") },
            { 168, new ComponentTypeInfo(168, "EntityAnalyticsConfig", "Entity Analytics Config") },
            { 175, new ComponentTypeInfo(175, "CanvasApp", "Canvas App") },
            { 300, new ComponentTypeInfo(300, "SavedQueryVisualization", "Chart") },
            { 371, new ComponentTypeInfo(371, "ConnectionReference", "Connection Reference") },
            { 380, new ComponentTypeInfo(380, "CustomAPI", "Custom API") },
            { 381, new ComponentTypeInfo(381, "CustomAPIRequestParameter", "Custom API Request Parameter") },
            { 382, new ComponentTypeInfo(382, "CustomAPIResponseProperty", "Custom API Response Property") }
        };

        // Name-to-value mappings for common friendly names
        private static readonly Dictionary<string, int> NameMappings = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase)
        {
            { "Entity", 1 },
            { "Table", 1 },
            { "Attribute", 2 },
            { "Column", 2 },
            { "Relationship", 3 },
            { "OptionSet", 9 },
            { "Choice", 9 },
            { "EntityRelationship", 10 },
            { "AlternateKey", 14 },
            { "EntityKey", 14 },
            { "Role", 16 },
            { "SecurityRole", 16 },
            { "Form", 48 },
            { "SystemForm", 48 },
            { "DataMap", 49 },
            { "ImportMap", 49 },
            { "WebResource", 50 },
            { "SiteMap", 52 },
            { "Sitemap", 52 },
            { "ConnectionRole", 53 },
            { "FieldSecurityProfile", 55 },
            { "PluginType", 59 },
            { "PluginAssembly", 60 },
            { "Plugin", 60 },
            { "SDKMessageProcessingStep", 61 },
            { "PluginStep", 61 },
            { "Step", 61 },
            { "SDKMessageProcessingStepImage", 62 },
            { "StepImage", 62 },
            { "ServiceEndpoint", 63 },
            { "SLA", 66 },
            { "MobileOfflineProfile", 70 },
            { "VirtualEntityDataSource", 73 },
            { "DataSourceMapping", 73 },
            { "SDKMessage", 80 },
            { "Index", 91 },
            { "EntityIndex", 91 },
            { "AppModule", 152 },
            { "App", 152 },
            { "ModelDrivenApp", 152 },
            { "PluginPackage", 154 },
            { "Connector", 161 },
            { "EnvironmentVariableDefinition", 162 },
            { "EnvVarDefinition", 162 },
            { "EnvironmentVariableValue", 163 },
            { "EnvVarValue", 163 },
            { "CanvasApp", 175 },
            { "Chart", 300 },
            { "SavedQueryVisualization", 300 },
            { "ConnectionReference", 371 },
            { "CustomAPI", 380 },
            { "CustomAPIRequestParameter", 381 },
            { "CustomAPIResponseProperty", 382 }
        };

        /// <summary>
        /// Returns completion candidates for component type values with friendly names.
        /// </summary>
        /// <param name="commandName">The name of the command</param>
        /// <param name="parameterName">The name of the parameter</param>
        /// <param name="wordToComplete">The partial input to complete</param>
        /// <param name="commandAst">The command AST</param>
        /// <param name="fakeBoundParameters">The bound parameters</param>
        /// <returns>Sequence of CompletionResult objects for matching component types</returns>
        public IEnumerable<CompletionResult> CompleteArgument(
            string commandName,
            string parameterName,
            string wordToComplete,
            CommandAst commandAst,
            IDictionary fakeBoundParameters)
        {
            try
            {
                var results = new List<CompletionResult>();

                // If nothing typed, return most common component types
                if (string.IsNullOrEmpty(wordToComplete))
                {
                    var commonTypes = new[] { 1, 2, 9, 10, 14, 48, 50, 52, 60, 61, 152, 162, 163, 175, 300, 371, 380 };
                    foreach (var typeValue in commonTypes)
                    {
                        if (StandardComponentTypes.TryGetValue(typeValue, out var info))
                        {
                            results.Add(CreateCompletionResult(info));
                        }
                    }
                    return results;
                }

                string wc = wordToComplete.Trim();

                // Check if input is numeric
                if (int.TryParse(wc, out int numericValue))
                {
                    // Find matching numeric values
                    var matches = StandardComponentTypes.Values
                        .Where(t => t.Value.ToString().StartsWith(wc))
                        .OrderBy(t => t.Value);

                    foreach (var info in matches)
                    {
                        results.Add(CreateCompletionResult(info));
                    }
                }
                else
                {
                    // Search by friendly name
                    var nameMatches = StandardComponentTypes.Values
                        .Where(t => 
                            t.Name.StartsWith(wc, StringComparison.OrdinalIgnoreCase) ||
                            t.DisplayName.StartsWith(wc, StringComparison.OrdinalIgnoreCase) ||
                            t.Name.IndexOf(wc, StringComparison.OrdinalIgnoreCase) >= 0 ||
                            t.DisplayName.IndexOf(wc, StringComparison.OrdinalIgnoreCase) >= 0)
                        .OrderBy(t => t.Name.StartsWith(wc, StringComparison.OrdinalIgnoreCase) ? 0 : 1)
                        .ThenBy(t => t.Value);

                    foreach (var info in nameMatches.Take(50))
                    {
                        results.Add(CreateCompletionResult(info));
                    }
                }

                return results;
            }
            catch
            {
                return Enumerable.Empty<CompletionResult>();
            }
        }

        private CompletionResult CreateCompletionResult(ComponentTypeInfo info)
        {
            string listItemText = $"{info.Value} ({info.DisplayName})";
            string toolTip = $"{info.DisplayName} - Component Type {info.Value}";
            return new CompletionResult(
                info.Value.ToString(),
                listItemText,
                CompletionResultType.ParameterValue,
                toolTip);
        }

        /// <summary>
        /// Attempts to parse a component type value from either a numeric string or a friendly name.
        /// </summary>
        /// <param name="value">The value to parse (can be numeric or friendly name)</param>
        /// <param name="componentType">The parsed component type value</param>
        /// <returns>True if parsing succeeded, false otherwise</returns>
        public static bool TryParse(string value, out int componentType)
        {
            if (string.IsNullOrWhiteSpace(value))
            {
                componentType = 0;
                return false;
            }

            // Try numeric first
            if (int.TryParse(value.Trim(), out componentType))
            {
                return true;
            }

            // Try friendly name lookup
            if (NameMappings.TryGetValue(value.Trim(), out componentType))
            {
                return true;
            }

            componentType = 0;
            return false;
        }

        private class ComponentTypeInfo
        {
            public int Value { get; }
            public string Name { get; }
            public string DisplayName { get; }

            public ComponentTypeInfo(int value, string name, string displayName)
            {
                Value = value;
                Name = name;
                DisplayName = displayName;
            }
        }
    }
}
