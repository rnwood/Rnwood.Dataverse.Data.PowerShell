using System.Globalization;
using System.IO.Compression;
using System.Net.Http.Headers;
using System.Reflection;
using System.Text.Encodings.Web;
using System.Text.RegularExpressions;
using System.Text;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Xml.Linq;
using Azure.Core;
using Azure.Identity;
using YamlDotNet.Serialization;

namespace MsAppToolkit;

public static partial class YamlFirstPackaging
{    private static void NormalizeThemeMetadata(Dictionary<string, byte[]> entries)
    {
        JsonObject root;
        if (entries.TryGetValue("References/Themes.json", out var bytes)
            && JsonNode.Parse(bytes) is JsonObject parsed)
        {
            root = parsed;
        }
        else
        {
            root = new JsonObject();
        }

        if ((root["CustomThemes"] as JsonArray)?.Count is null or 0)
        {
            var embeddedThemes = TryReadEmbeddedReferenceJson("References/Themes.json");
            if (embeddedThemes?["CustomThemes"] is JsonArray embeddedCustomThemes && embeddedCustomThemes.Count > 0)
            {
                root["CustomThemes"] = embeddedCustomThemes.DeepClone();
            }
        }

        // Studio-authored modern apps default to this pointer.
        root["CurrentTheme"] = "v9WebLightTheme";
        entries["References/Themes.json"] = Encoding.UTF8.GetBytes(root.ToJsonString(JsonOptions));
    }

    private static void NormalizeModernThemeMetadata(Dictionary<string, byte[]> entries)
    {
        JsonObject root;
        if (entries.TryGetValue("References/ModernThemes.json", out var bytes)
            && JsonNode.Parse(bytes) is JsonObject parsed)
        {
            root = parsed;
        }
        else
        {
            root = new JsonObject();
        }

        if (root["Themes"] is not JsonArray themes || themes.Count == 0)
        {
            var embedded = TryReadEmbeddedReferenceJson("References/ModernThemes.json");
            if (embedded?["Themes"] is JsonArray embeddedThemes && embeddedThemes.Count > 0)
            {
                root["Themes"] = embeddedThemes.DeepClone();
            }
            else
            {
                root["Themes"] = new JsonArray
                {
                    new JsonObject
                    {
                        ["EntityName"] = "PowerAppsTheme",
                        ["ThemeName"] = "PowerAppsTheme",
                    },
                };
            }
        }

        if (root["$schema"] is not null)
        {
            root.Remove("$schema");
        }

        entries["References/ModernThemes.json"] = Encoding.UTF8.GetBytes(root.ToJsonString(JsonOptions));
    }

    private static JsonObject? TryReadEmbeddedReferenceJson(string entryPath)
    {
        if (!ReadEmbeddedAllControlsEntries(out var embedded)
            || !embedded.TryGetValue(entryPath, out var bytes)
            || JsonNode.Parse(bytes) is not JsonObject parsed)
        {
            return null;
        }

        return parsed;
    }

    private static void NormalizeHostControlMetadata(Dictionary<string, byte[]> entries)
    {
        foreach (var path in entries.Keys
                     .Where(k => (k.StartsWith("Controls/", StringComparison.OrdinalIgnoreCase)
                                  || k.StartsWith("Components/", StringComparison.OrdinalIgnoreCase))
                                 && k.EndsWith(".json", StringComparison.OrdinalIgnoreCase))
                     .ToList())
        {
            if (JsonNode.Parse(entries[path]) is not JsonObject doc
                || doc["TopParent"] is not JsonObject topParent)
            {
                continue;
            }

            var changed = false;
            foreach (var node in Flatten(topParent))
            {
                var template = node["Template"] as JsonObject;
                if (template is null)
                {
                    continue;
                }

                var templateName = template["Name"]?.GetValue<string>() ?? string.Empty;
                var templateId = template["Id"]?.GetValue<string>() ?? string.Empty;
                var isHost = string.Equals(templateName, "hostControl", StringComparison.OrdinalIgnoreCase)
                             || templateId.EndsWith("/hostcontrol", StringComparison.OrdinalIgnoreCase);
                if (!isHost)
                {
                    continue;
                }

                if (template["HostType"] is null)
                {
                    template["HostType"] = "Default";
                    changed = true;
                }

                if (node["HasDynamicProperties"] is null)
                {
                    node["HasDynamicProperties"] = false;
                    changed = true;
                }
            }

            if (changed)
            {
                entries[path] = Encoding.UTF8.GetBytes(doc.ToJsonString(JsonOptions));
            }
        }
    }

    private static void NormalizeAppInfoRuleProviders(Dictionary<string, byte[]> entries)
    {
        foreach (var path in entries.Keys
                     .Where(k => (k.StartsWith("Controls/", StringComparison.OrdinalIgnoreCase)
                                  || k.StartsWith("Components/", StringComparison.OrdinalIgnoreCase))
                                 && k.EndsWith(".json", StringComparison.OrdinalIgnoreCase))
                     .ToList())
        {
            if (JsonNode.Parse(entries[path]) is not JsonObject doc
                || doc["TopParent"] is not JsonObject topParent)
            {
                continue;
            }

            var changed = false;
            foreach (var node in Flatten(topParent))
            {
                if (!IsAppInfoControl(node))
                {
                    continue;
                }

                var rules = (node["Rules"] as JsonArray) ?? new JsonArray();
                foreach (var rule in rules.OfType<JsonObject>())
                {
                    var property = rule["Property"]?.GetValue<string>() ?? string.Empty;
                    if (string.Equals(property, "OnStart", StringComparison.OrdinalIgnoreCase)
                        || string.Equals(property, "StartScreen", StringComparison.OrdinalIgnoreCase))
                    {
                        if (!string.Equals(rule["RuleProviderType"]?.GetValue<string>() ?? string.Empty, "User", StringComparison.OrdinalIgnoreCase))
                        {
                            rule["RuleProviderType"] = "User";
                            changed = true;
                        }
                    }
                }

                if (NormalizeAppInfoStateAndRuleOrder(node))
                {
                    changed = true;
                }
            }

            if (changed)
            {
                entries[path] = Encoding.UTF8.GetBytes(doc.ToJsonString(JsonOptions));
            }
        }
    }

    private static bool NormalizeAppInfoStateAndRuleOrder(JsonObject appNode)
    {
        var changed = false;

        var preferredRuleOrder = new[]
        {
            "ConfirmExit", "BackEnabled", "MinScreenHeight", "MinScreenWidth", "Theme", "OnStart", "SizeBreakpoints", "StartScreen",
        };

        var existingRules = ((appNode["Rules"] as JsonArray) ?? new JsonArray()).OfType<JsonObject>().ToList();
        var ruleByProperty = new Dictionary<string, JsonObject>(StringComparer.OrdinalIgnoreCase);
        foreach (var rule in existingRules)
        {
            var property = rule["Property"]?.GetValue<string>() ?? string.Empty;
            if (!string.IsNullOrWhiteSpace(property) && !ruleByProperty.ContainsKey(property))
            {
                ruleByProperty[property] = rule;
            }
        }

        var reorderedRules = new JsonArray();
        foreach (var property in preferredRuleOrder)
        {
            if (ruleByProperty.TryGetValue(property, out var found))
            {
                reorderedRules.Add(found.DeepClone());
            }
        }

        foreach (var rule in existingRules)
        {
            var property = rule["Property"]?.GetValue<string>() ?? string.Empty;
            if (string.IsNullOrWhiteSpace(property)
                || preferredRuleOrder.Contains(property, StringComparer.OrdinalIgnoreCase))
            {
                continue;
            }

            reorderedRules.Add(rule.DeepClone());
        }

        var oldRules = ((appNode["Rules"] as JsonArray) ?? new JsonArray()).ToJsonString(JsonOptions);
        var newRules = reorderedRules.ToJsonString(JsonOptions);
        if (!string.Equals(oldRules, newRules, StringComparison.Ordinal))
        {
            appNode["Rules"] = reorderedRules;
            changed = true;
        }

        var preferredStateOrder = new[]
        {
            "MinScreenHeight", "MinScreenWidth", "ConfirmExit", "SizeBreakpoints", "BackEnabled", "Theme", "StartScreen", "OnStart",
        };

        var stateByProperty = new Dictionary<string, JsonNode>(StringComparer.OrdinalIgnoreCase);
        foreach (var stateEntry in ((appNode["ControlPropertyState"] as JsonArray) ?? new JsonArray()))
        {
            var property = GetStatePropertyName(stateEntry);
            if (string.IsNullOrWhiteSpace(property) || stateByProperty.ContainsKey(property))
            {
                continue;
            }

            stateByProperty[property] = stateEntry?.DeepClone() ?? JsonValue.Create(property)!;
        }

        var reorderedState = new JsonArray();
        foreach (var property in preferredStateOrder)
        {
            if (!stateByProperty.TryGetValue(property, out var stateNode))
            {
                stateNode = string.Equals(property, "StartScreen", StringComparison.OrdinalIgnoreCase)
                            || string.Equals(property, "OnStart", StringComparison.OrdinalIgnoreCase)
                    ? CreateInvariantPropertyStateEntry(property)
                    : JsonValue.Create(property)!;
            }

            reorderedState.Add(stateNode.DeepClone());
        }

        var oldState = ((appNode["ControlPropertyState"] as JsonArray) ?? new JsonArray()).ToJsonString(JsonOptions);
        var newState = reorderedState.ToJsonString(JsonOptions);
        if (!string.Equals(oldState, newState, StringComparison.Ordinal))
        {
            appNode["ControlPropertyState"] = reorderedState;
            changed = true;
        }

        return changed;
    }

    private static void NormalizePropertiesMetadata(Dictionary<string, byte[]> entries)
    {
        JsonObject root;
        if (entries.TryGetValue("Properties.json", out var bytes)
            && JsonNode.Parse(bytes) is JsonObject parsed)
        {
            root = parsed;
        }
        else
        {
            root = new JsonObject();
        }

        var appName = TryReadPublishInfoAppName(entries);
        if (string.IsNullOrWhiteSpace(appName))
        {
            appName = "App";
        }

        EnsureString(root, "Author", "");
        EnsureName(root, appName);
        EnsureGuidString(root, "Id");
        EnsureGuidString(root, "FileID");
        EnsureString(root, "LocalConnectionReferences", "{}");
        EnsureString(root, "LocalDatabaseReferences", "");
        EnsureString(root, "LibraryDependencies", "[]");
        MergeIntoObject(root, "AppPreviewFlagsMap", BuildDefaultAppPreviewFlagsMap());
        EnsureNumber(root, "DocumentLayoutWidth", 1366);
        EnsureNumber(root, "DocumentLayoutHeight", 768);
        EnsureString(root, "DocumentLayoutOrientation", "landscape");
        EnsureBool(root, "DocumentLayoutMaintainAspectRatio", false);
        EnsureBool(root, "DocumentLayoutScaleToFit", false);
        EnsureString(root, "AppCreationSource", "AppFromScreenTemplate");
        EnsureBool(root, "DocumentLayoutLockOrientation", false);
        EnsureBool(root, "ShowStatusBar", false);
        EnsureString(root, "AppCopilotSchemaName", "");
        EnsureString(root, "DocumentAppType", "DesktopOrTablet");
        EnsureString(root, "DocumentType", "App");
        EnsureString(root, "AppDescription", "");
        EnsureString(root, "ManualOfflineProfileId", "");
        EnsureNumber(root, "DefaultConnectedDataSourceMaxGetRowsCount", 500);
        EnsureBool(root, "ContainsThirdPartyPcfControls", false);
        EnsureNumber(root, "ParserErrorCount", 0);
        EnsureNumber(root, "BindingErrorCount", 0);
        EnsureString(root, "ConnectionString", "");
        EnsureBool(root, "EnableInstrumentation", false);

        if ((root["OriginatingVersion"]?.GetValue<string>() ?? string.Empty) is var ov
            && string.IsNullOrWhiteSpace(ov))
        {
            root["OriginatingVersion"] = root["DocVersion"]?.GetValue<string>() ?? "1.348";
        }

        var hasLocalDbRefs = !string.IsNullOrWhiteSpace(root["LocalDatabaseReferences"]?.GetValue<string>() ?? string.Empty);
        if (!hasLocalDbRefs)
        {
            var localDbRefs = BuildLocalDatabaseReferences(entries);
            if (!string.IsNullOrWhiteSpace(localDbRefs))
            {
                root["LocalDatabaseReferences"] = localDbRefs;
            }
        }

        entries["Properties.json"] = Encoding.UTF8.GetBytes(root.ToJsonString(JsonOptions));
    }

    private static string TryReadPublishInfoAppName(Dictionary<string, byte[]> entries)
    {
        if (!entries.TryGetValue("Resources/PublishInfo.json", out var bytes)
            || JsonNode.Parse(bytes) is not JsonObject root)
        {
            return string.Empty;
        }

        return root["AppName"]?.GetValue<string>() ?? string.Empty;
    }

    private static void EnsureString(JsonObject root, string property, string defaultValue)
    {
        if (string.IsNullOrWhiteSpace(root[property]?.GetValue<string>() ?? string.Empty))
        {
            root[property] = defaultValue;
        }
    }

    private static void EnsureName(JsonObject root, string appName)
    {
        var current = root["Name"]?.GetValue<string>() ?? string.Empty;
        if (string.IsNullOrWhiteSpace(current))
        {
            root["Name"] = appName;
        }
    }

    private static void EnsureGuidString(JsonObject root, string property)
    {
        var value = root[property]?.GetValue<string>() ?? string.Empty;
        if (!Guid.TryParse(value, out _))
        {
            root[property] = Guid.NewGuid().ToString();
        }
    }

    private static void EnsureNumber(JsonObject root, string property, int defaultValue)
    {
        if (root[property] is not JsonValue existing
            || !(existing.TryGetValue<int>(out _) || existing.TryGetValue<long>(out _)))
        {
            root[property] = defaultValue;
        }
    }

    private static void EnsureBool(JsonObject root, string property, bool defaultValue)
    {
        if (root[property] is not JsonValue existing || !existing.TryGetValue<bool>(out _))
        {
            root[property] = defaultValue;
        }
    }

    private static void EnsureObject(JsonObject root, string property, JsonObject defaultValue)
    {
        if (root[property] is not JsonObject)
        {
            root[property] = defaultValue.DeepClone();
        }
    }

    /// <summary>
    /// Ensures <paramref name="property"/> exists as a JsonObject in <paramref name="root"/>.
    /// If the key is missing the full <paramref name="defaults"/> object is inserted.
    /// If it already exists, any keys present in <paramref name="defaults"/> but absent in
    /// the existing object are added — existing values are never overwritten.
    /// </summary>
    private static void MergeIntoObject(JsonObject root, string property, JsonObject defaults)
    {
        if (root[property] is not JsonObject existing)
        {
            root[property] = defaults.DeepClone();
            return;
        }
        foreach (var (key, value) in defaults)
        {
            if (!existing.ContainsKey(key))
            {
                existing[key] = value?.DeepClone();
            }
        }
    }

    /// <summary>
    /// Full AppPreviewFlagsMap baseline derived from a real Power Apps Studio session (v1.348).
    /// These defaults match what Studio writes when creating a new DesktopOrTablet canvas app
    /// with Dataverse connected. Individual values are never overwritten for apps that already
    /// have an AppPreviewFlagsMap — only missing keys are added.
    /// </summary>
    private static JsonObject BuildDefaultAppPreviewFlagsMap()
    {
        return new JsonObject
        {
            // --- feature flags (alphabetical) ---
            ["adaptivepaging"] = false,
            ["aibuilderserviceenrollment"] = false,
            ["allowmultiplescreensincanvaspages"] = false,
            ["appinstrumentationcorrelationtracing"] = false,
            ["appinsightserrortracing"] = false,
            ["autocreateenvironmentvariables"] = false,
            ["behaviorpropertyui"] = true,
            ["blockmovingcontrol"] = true,
            ["cdsdataformatting"] = false,
            ["classiccontrols"] = false,
            ["commentgeneratedformulasv2"] = true,
            ["consistentreturnschemafortabularfunctions"] = true,
            ["copyandmerge"] = false,
            ["dataflowanalysisenabled"] = true,
            ["datatablev2control"] = true,
            ["dataverseactionsenabled"] = true,
            ["delaycontrolrendering"] = true,
            ["delayloadscreens"] = true,
            ["disablebehaviorreturntypecheck"] = false,
            ["disableruntimepolicies"] = false,
            ["dynamicschema"] = false,
            ["enableappembeddingux"] = false,
            ["enablecanvasappruntimecopilot"] = true,
            ["enablecomponentnamemaps"] = false,
            ["enablecomponentscopeoldbehavior"] = false,
            ["enablecopilotanswercontrol"] = false,
            ["enablecopilotcontrol"] = false,
            ["enablecreateaformula"] = true,
            ["enabledataverseoffline"] = false,
            ["enableeditinmcs"] = true,
            ["enableexcelonlinebusinessv2connector"] = true,
            ["enablelegacybarcodescanner"] = false,
            ["enableonstartnavigate"] = false,
            ["enableonstart"] = true,
            ["enablepcfmoderndatasets"] = true,
            ["enablerowscopeonetonexpand"] = false,
            ["enablerpawarecomponentdependency"] = true,
            ["enablesaveloadcleardataonweb"] = true,
            ["enableupdateifdelegation"] = true,
            ["errorhandling"] = true,
            ["expandedsavedatasupport"] = true,
            ["exportimportcomponents2"] = true,
            ["externalmessage"] = false,
            ["fluentv9controls"] = false,
            ["fluentv9controlspreview"] = false,
            ["formuladataprefetch"] = true,
            ["generatedebugpublishedapp"] = false,
            ["herocontrols"] = false,
            ["improvedtabstopbehavior"] = false,
            ["isemptyrequirestableargument"] = true,
            ["keeprecentscreensloaded"] = false,
            ["loadcomponentdefinitionsondemand"] = true,
            ["longlivingcache"] = false,
            ["mobilenativerendering"] = false,
            ["nativecdsexperimental"] = true,
            ["offlineprofilegenerationemitscolumns"] = false,
            ["onegrid"] = false,
            ["optimizestartscreenpublishedappload"] = true,
            ["packagemodernruntime"] = false,
            ["pdffunction"] = false,
            ["powerfxdecimal"] = false,
            ["powerfxv1"] = false,
            ["primaryoutputpropertycoerciondeprecated"] = true,
            ["proactivecontrolrename"] = true,
            ["projectionmapping"] = true,
            ["reservedkeywords"] = false,
            ["rtlinstudiopreview"] = false,
            ["rtlsupport"] = false,
            ["sharepointselectsenabled"] = true,
            ["showclassicthemes"] = false,
            ["smartemaildatacard"] = false,
            ["supportcolumnnamesasidentifiers"] = true,
            ["tabledoesntwraprecords"] = true,
            ["useexperimentalcdsconnector"] = true,
            ["useexperimentalsqlconnector"] = true,
            ["usedisplaynamemetadata"] = true,
            ["useenforcesavedatalimits"] = true,
            ["useguiddatatypes"] = true,
            ["usenonblockingonstartrule"] = true,
            ["userdefinedtypes"] = false,
            ["webbarcodescanner"] = false,
            ["zeroalltabindexes"] = true,
        };
    }

    private static void NormalizeHeaderMetadata(Dictionary<string, byte[]> entries)
    {
        JsonObject root;
        if (entries.TryGetValue("Header.json", out var bytes)
            && JsonNode.Parse(bytes) is JsonObject parsed)
        {
            root = parsed;
        }
        else
        {
            root = new JsonObject();
        }

        // Fill in the required version fields when missing; Power Apps Studio refuses to import
        // a package with an empty or version-less Header.json.
        const string currentVersion = "1.348";
        const string structureVersion = "2.4.0";

        if (root["DocVersion"]?.GetValue<string>() is not { Length: > 0 })
            root["DocVersion"] = currentVersion;
        if (root["MinVersionToLoad"]?.GetValue<string>() is not { Length: > 0 })
            root["MinVersionToLoad"] = currentVersion;
        if (root["MSAppStructureVersion"]?.GetValue<string>() is not { Length: > 0 })
            root["MSAppStructureVersion"] = structureVersion;

        // AnalysisOptions block: ensure it exists
        if (root["AnalysisOptions"] is not JsonObject)
        {
            root["AnalysisOptions"] = new JsonObject
            {
                ["DataflowAnalysisEnabled"] = true,
                ["DataflowAnalysisFlagStateToggledByUser"] = false
            };
        }

        entries["Header.json"] = Encoding.UTF8.GetBytes(root.ToJsonString(JsonOptions));
    }

    private static void EnsurePublishInfoResource(Dictionary<string, byte[]> entries)
    {
        const string publishInfoPath = "Resources/PublishInfo.json";
        if (entries.ContainsKey(publishInfoPath))
        {
            return;
        }

        var appName = "App";
        if (entries.TryGetValue("Properties.json", out var propertiesBytes)
            && JsonNode.Parse(propertiesBytes) is JsonObject propertiesRoot)
        {
            appName = propertiesRoot["Name"]?.GetValue<string>() ?? appName;
            if (appName.EndsWith(".msapp", StringComparison.OrdinalIgnoreCase))
            {
                appName = Path.GetFileNameWithoutExtension(appName);
            }

            if (string.IsNullOrWhiteSpace(appName))
            {
                appName = "App";
            }
        }

        var publishInfo = new JsonObject
        {
            ["AppName"] = appName,
            ["BackgroundColor"] = "RGBA(0,176,240,1)",
            ["PublishTarget"] = "player",
            ["LogoFileName"] = string.Empty,
            ["IconColor"] = "RGBA(255,255,255,1)",
            ["IconName"] = "Edit",
            ["PublishResourcesLocally"] = false,
            ["PublishDataLocally"] = false,
            ["UserLocale"] = "en-US",
        };

        entries[publishInfoPath] = Encoding.UTF8.GetBytes(publishInfo.ToJsonString(JsonOptions));
    }

    private static string BuildLocalDatabaseReferences(Dictionary<string, byte[]> entries)
    {
        if (!entries.TryGetValue("References/DataSources.json", out var dsBytes))
        {
            return string.Empty;
        }

        if (JsonNode.Parse(dsBytes) is not JsonObject dsRoot || dsRoot["DataSources"] is not JsonArray dsArray)
        {
            return string.Empty;
        }

        var native = dsArray
            .OfType<JsonObject>()
            .Where(ds => string.Equals(ds["Type"]?.GetValue<string>() ?? string.Empty, "NativeCDSDataSourceInfo", StringComparison.OrdinalIgnoreCase))
            .ToList();
        if (native.Count == 0)
        {
            return string.Empty;
        }

        string instanceUrl = string.Empty;
        foreach (var ds in native)
        {
            // TableDefinition may be a JSON-encoded string (new format) or an inline
            // JsonObject (older export format). Normalise both into a JsonObject.
            JsonObject? tableDefinitionObj = null;
            if (ds["TableDefinition"] is JsonValue tdVal)
            {
                var tdStr = tdVal.TryGetValue<string>(out var s) ? s : string.Empty;
                if (!string.IsNullOrWhiteSpace(tdStr))
                {
                    tableDefinitionObj = JsonNode.Parse(tdStr) as JsonObject;
                }
            }
            else if (ds["TableDefinition"] is JsonObject tdObj)
            {
                tableDefinitionObj = tdObj;
            }

            if (tableDefinitionObj is not null)
            {
                // Views may itself be a JSON-encoded string or an inline node.
                var views = tableDefinitionObj["Views"] is JsonValue vVal
                    ? (vVal.TryGetValue<string>(out var vs) ? vs : string.Empty)
                    : tableDefinitionObj["Views"]?.ToJsonString() ?? string.Empty;
                if (!string.IsNullOrWhiteSpace(views))
                {
                    var m = Regex.Match(views, @"https://[^/]+", RegexOptions.IgnoreCase);
                    if (m.Success)
                    {
                        instanceUrl = m.Value.TrimEnd('/') + "/";
                        break;
                    }
                }
            }

            // Legacy format: CdsFCBContext was a nested object with Views
            var views2 = (ds["CdsFCBContext"] as JsonObject)?["Views"]?.GetValue<string>() ?? string.Empty;
            if (!string.IsNullOrWhiteSpace(views2))
            {
                var m = Regex.Match(views2, @"https://[^/]+", RegexOptions.IgnoreCase);
                if (m.Success)
                {
                    instanceUrl = m.Value.TrimEnd('/') + "/";
                    break;
                }
            }
        }

        if (string.IsNullOrWhiteSpace(instanceUrl))
        {
            return string.Empty;
        }

        var dataSources = new JsonObject();
        foreach (var ds in native)
        {
            var name = ds["Name"]?.GetValue<string>() ?? string.Empty;
            var entitySetName = ds["EntitySetName"]?.GetValue<string>() ?? string.Empty;
            var logicalName = ds["LogicalName"]?.GetValue<string>() ?? string.Empty;
            if (string.IsNullOrWhiteSpace(name) || string.IsNullOrWhiteSpace(entitySetName) || string.IsNullOrWhiteSpace(logicalName))
            {
                continue;
            }

            dataSources[name] = new JsonObject
            {
                ["entitySetName"] = entitySetName,
                ["logicalName"] = logicalName,
            };
        }

        if (dataSources.Count == 0)
        {
            return string.Empty;
        }

        var payload = new JsonObject
        {
            ["default.cds"] = new JsonObject
            {
                ["state"] = "Configured",
                ["instanceUrl"] = instanceUrl,
                ["webApiVersion"] = "v9.0",
                ["environmentVariableName"] = string.Empty,
                ["dataSources"] = dataSources,
            },
        };

        return payload.ToJsonString();
    }

    private static void NormalizeDataSourceMetadata(Dictionary<string, byte[]> entries)
    {
        if (!entries.TryGetValue("References/DataSources.json", out var bytes))
        {
            return;
        }

        if (JsonNode.Parse(bytes) is not JsonObject root || root["DataSources"] is not JsonArray dataSources)
        {
            return;
        }

        var changed = false;
        foreach (var ds in dataSources.OfType<JsonObject>())
        {
            var type = ds["Type"]?.GetValue<string>() ?? string.Empty;
            var name = ds["Name"]?.GetValue<string>() ?? string.Empty;

            if (string.Equals(type, "NativeCDSDataSourceInfo", StringComparison.OrdinalIgnoreCase)
                && ds["MaxGetRowsCount"] is null)
            {
                ds["MaxGetRowsCount"] = 500;
                changed = true;
            }

            if (string.Equals(type, "StaticDataSourceInfo", StringComparison.OrdinalIgnoreCase)
                && name.EndsWith("Sample", StringComparison.OrdinalIgnoreCase)
                && !(ds["IsSampleData"]?.GetValue<bool>() ?? false))
            {
                ds["IsSampleData"] = true;
                changed = true;
            }
        }

        if (!changed)
        {
            return;
        }

        root["DataSources"] = dataSources;
        entries["References/DataSources.json"] = Encoding.UTF8.GetBytes(root.ToJsonString(JsonOptions));
    }
}