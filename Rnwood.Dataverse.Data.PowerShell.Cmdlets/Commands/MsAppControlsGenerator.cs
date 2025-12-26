using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using SharpYaml.Serialization;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Rnwood.Dataverse.Data.PowerShell.Commands
{
    /// <summary>
    /// Helper class to generate Controls JSON files from YAML content in msapp files.
    /// </summary>
    internal static class MsAppControlsGenerator
    {
        /// <summary>
        /// Generates the App control JSON (Controls/1.json) from App.pa.yaml content.
        /// </summary>
        /// <param name="appYamlContent">The YAML content from App.pa.yaml</param>
        /// <returns>JSON string for Controls/1.json</returns>
        public static string GenerateAppControlJson(string appYamlContent)
        {
            var serializer = new Serializer();
            var yamlObject = serializer.Deserialize(appYamlContent);

            // Expected structure: { App: { Properties: { ... } } }
            Dictionary<object, object> properties = null;
            if (yamlObject is Dictionary<object, object> root &&
                root.TryGetValue("App", out var appObj) &&
                appObj is Dictionary<object, object> app &&
                app.TryGetValue("Properties", out var propsObj) &&
                propsObj is Dictionary<object, object> props)
            {
                properties = props;
            }

            var control = CreateAppControl(properties);
            var wrapper = new JObject
            {
                ["TopParent"] = control
            };

            return wrapper.ToString(Formatting.Indented);
        }

        /// <summary>
        /// Generates a screen control JSON from screen YAML content.
        /// </summary>
        /// <param name="screenName">Name of the screen</param>
        /// <param name="screenYamlContent">The YAML content from {screenName}.pa.yaml</param>
        /// <param name="controlUniqueId">Unique ID for the control</param>
        /// <returns>JSON string for Controls/{controlUniqueId}.json</returns>
        public static string GenerateScreenControlJson(string screenName, string screenYamlContent, int controlUniqueId)
        {
            var serializer = new Serializer();
            var yamlObject = serializer.Deserialize(screenYamlContent);

            // Expected structure: { Screens: { ScreenName: { Properties: { ... } } } }
            Dictionary<object, object> properties = null;
            if (yamlObject is Dictionary<object, object> root &&
                root.TryGetValue("Screens", out var screensObj) &&
                screensObj is Dictionary<object, object> screens &&
                screens.TryGetValue(screenName, out var screenObj) &&
                screenObj is Dictionary<object, object> screen &&
                screen.TryGetValue("Properties", out var propsObj) &&
                propsObj is Dictionary<object, object> props)
            {
                properties = props;
            }

            var control = CreateScreenControl(screenName, properties, controlUniqueId);
            var wrapper = new JObject
            {
                ["TopParent"] = control
            };

            return wrapper.ToString(Formatting.Indented);
        }

        private static JObject CreateAppControl(Dictionary<object, object> properties)
        {
            var rules = new JArray();
            var controlPropertyState = new JArray();

            if (properties != null)
            {
                foreach (var prop in properties)
                {
                    string propertyName = prop.Key.ToString();
                    string value = prop.Value?.ToString() ?? "";
                    
                    // Remove leading '=' if present
                    string invariantScript = value.StartsWith("=") ? value.Substring(1) : value;

                    rules.Add(new JObject
                    {
                        ["Property"] = propertyName,
                        ["Category"] = DetermineCategory(propertyName),
                        ["InvariantScript"] = invariantScript,
                        ["RuleProviderType"] = "Unknown"
                    });

                    controlPropertyState.Add(propertyName);
                }
            }

            var control = new JObject
            {
                ["Type"] = "ControlInfo",
                ["Name"] = "App",
                ["Template"] = new JObject
                {
                    ["Id"] = "http://microsoft.com/appmagic/appinfo",
                    ["Version"] = "1.0",
                    ["LastModifiedTimestamp"] = "0",
                    ["Name"] = "appinfo",
                    ["FirstParty"] = true,
                    ["IsPremiumPcfControl"] = false,
                    ["IsCustomGroupControlTemplate"] = false,
                    ["CustomGroupControlTemplateName"] = "",
                    ["IsComponentDefinition"] = false,
                    ["OverridableProperties"] = new JObject()
                },
                ["Index"] = 0,
                ["PublishOrderIndex"] = 0,
                ["VariantName"] = "",
                ["LayoutName"] = "",
                ["MetaDataIDKey"] = "",
                ["PersistMetaDataIDKey"] = false,
                ["IsFromScreenLayout"] = false,
                ["StyleName"] = "",
                ["Parent"] = "",
                ["IsDataControl"] = true,
                ["AllowAccessToGlobals"] = true,
                ["OptimizeForDevices"] = "Off",
                ["IsGroupControl"] = false,
                ["IsAutoGenerated"] = false,
                ["Rules"] = rules,
                ["ControlPropertyState"] = controlPropertyState,
                ["IsLocked"] = false,
                ["ControlUniqueId"] = "1",
                ["Children"] = new JArray
                {
                    new JObject
                    {
                        ["Type"] = "ControlInfo",
                        ["Name"] = "Host",
                        ["HasDynamicProperties"] = false,
                        ["Template"] = new JObject
                        {
                            ["Id"] = "http://microsoft.com/appmagic/hostcontrol",
                            ["Version"] = "1.6.0",
                            ["LastModifiedTimestamp"] = "0",
                            ["Name"] = "hostControl",
                            ["FirstParty"] = true,
                            ["IsPremiumPcfControl"] = false,
                            ["IsCustomGroupControlTemplate"] = false,
                            ["CustomGroupControlTemplateName"] = "",
                            ["IsComponentDefinition"] = false,
                            ["HostType"] = "Default",
                            ["OverridableProperties"] = new JObject()
                        },
                        ["Index"] = 0,
                        ["PublishOrderIndex"] = 0,
                        ["VariantName"] = "DefaultHostControlVariant",
                        ["LayoutName"] = "",
                        ["MetaDataIDKey"] = "",
                        ["PersistMetaDataIDKey"] = false,
                        ["IsFromScreenLayout"] = false,
                        ["StyleName"] = "",
                        ["Parent"] = "App",
                        ["IsDataControl"] = true,
                        ["AllowAccessToGlobals"] = true,
                        ["OptimizeForDevices"] = "Off",
                        ["IsGroupControl"] = false,
                        ["IsAutoGenerated"] = false,
                        ["Rules"] = new JArray(),
                        ["ControlPropertyState"] = new JArray(),
                        ["IsLocked"] = false,
                        ["ControlUniqueId"] = "3",
                        ["Children"] = new JArray()
                    }
                }
            };

            return control;
        }

        private static JObject CreateScreenControl(string screenName, Dictionary<object, object> properties, int controlUniqueId)
        {
            var rules = new JArray();
            var controlPropertyState = new JArray();

            if (properties != null)
            {
                foreach (var prop in properties)
                {
                    string propertyName = prop.Key.ToString();
                    string value = prop.Value?.ToString() ?? "";
                    
                    // Remove leading '=' if present
                    string invariantScript = value.StartsWith("=") ? value.Substring(1) : value;

                    rules.Add(new JObject
                    {
                        ["Property"] = propertyName,
                        ["Category"] = DetermineCategory(propertyName),
                        ["InvariantScript"] = invariantScript,
                        ["RuleProviderType"] = "Unknown"
                    });

                    controlPropertyState.Add(propertyName);
                }
            }

            var control = new JObject
            {
                ["Type"] = "ControlInfo",
                ["Name"] = screenName,
                ["Template"] = new JObject
                {
                    ["Id"] = "http://microsoft.com/appmagic/screen",
                    ["Version"] = "1.0",
                    ["LastModifiedTimestamp"] = "0",
                    ["Name"] = "screen",
                    ["FirstParty"] = true,
                    ["IsPremiumPcfControl"] = false,
                    ["IsCustomGroupControlTemplate"] = false,
                    ["CustomGroupControlTemplateName"] = "",
                    ["IsComponentDefinition"] = false,
                    ["OverridableProperties"] = new JObject()
                },
                ["Index"] = 0,
                ["PublishOrderIndex"] = 0,
                ["VariantName"] = "",
                ["LayoutName"] = "",
                ["MetaDataIDKey"] = "",
                ["PersistMetaDataIDKey"] = false,
                ["IsFromScreenLayout"] = false,
                ["StyleName"] = "defaultScreenStyle",
                ["Parent"] = "",
                ["IsDataControl"] = false,
                ["AllowAccessToGlobals"] = true,
                ["OptimizeForDevices"] = "Off",
                ["IsGroupControl"] = false,
                ["IsAutoGenerated"] = false,
                ["Rules"] = rules,
                ["ControlPropertyState"] = controlPropertyState,
                ["IsLocked"] = false,
                ["ControlUniqueId"] = controlUniqueId.ToString(),
                ["Children"] = new JArray()
            };

            return control;
        }

        /// <summary>
        /// Determines the category for a property based on its name.
        /// </summary>
        private static string DetermineCategory(string propertyName)
        {
            // Common categorizations based on Power Apps conventions
            var designProperties = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
            {
                "Fill", "ImagePosition", "Height", "Width", "Size", "Orientation",
                "LoadingSpinner", "LoadingSpinnerColor", "MinScreenHeight", "MinScreenWidth", "Theme"
            };

            var dataProperties = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
            {
                "ConfirmExit", "BackEnabled"
            };

            var behaviorProperties = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
            {
                "OnVisible", "OnHidden", "OnNew", "OnEdit", "OnView", "OnSave", "OnCancel"
            };

            if (designProperties.Contains(propertyName))
                return "Design";
            if (dataProperties.Contains(propertyName))
                return "Data";
            if (behaviorProperties.Contains(propertyName))
                return "Behavior";
            if (propertyName.Equals("SizeBreakpoints", StringComparison.OrdinalIgnoreCase))
                return "ConstantData";

            // Default to Design
            return "Design";
        }
    }
}
