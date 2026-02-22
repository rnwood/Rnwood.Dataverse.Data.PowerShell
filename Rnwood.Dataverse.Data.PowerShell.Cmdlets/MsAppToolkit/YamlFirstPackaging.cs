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
{    public sealed record EmbeddedTemplateSummary(string Name, string Version, string TemplateId, string YamlControlName, bool RequiresVariantKeyword, IReadOnlyList<string> AvailableVariants, IReadOnlyList<TemplateAppFlagRequirement> AppFlagRequirements);
    public sealed record EmbeddedTemplateProperty(string PropertyName, string? DefaultValue);
    /// <summary>Describes a control template's properties, variants, flag requirements, and the
    /// universal auto-layout child properties that are injected by the runtime when this control
    /// is placed inside a horizontal or vertical auto-layout GroupContainer.</summary>
    public sealed record EmbeddedTemplateDetails(string Name, string Version, string TemplateId, string YamlControlName, IReadOnlyList<EmbeddedTemplateProperty> Properties, bool RequiresVariantKeyword, IReadOnlyList<string> AvailableVariants, IReadOnlyList<TemplateAppFlagRequirement> AppFlagRequirements, IReadOnlyList<EmbeddedTemplateProperty> ContextualDynamicProperties);

    /// <summary>Properties that are injected onto any child control placed inside an auto-layout
    /// (horizontal or vertical) GroupContainer.  They are not declared in any individual template
    /// but are universally available as parent-context properties.</summary>
    public static readonly IReadOnlyList<EmbeddedTemplateProperty> AutoLayoutChildProperties = new List<EmbeddedTemplateProperty>
    {
        new("AlignInContainer",  "AlignInContainer.SetByContainer"),
        new("FillPortions",      "0"),
        new("LayoutMaxHeight",   "0"),
        new("LayoutMaxWidth",    "0"),
        new("LayoutMinHeight",   "32"),
        new("LayoutMinWidth",    "320"),
    }.AsReadOnly();
    /// <summary>Describes an AppPreviewFlagsMap requirement for a template (e.g. classiccontrols must be true).</summary>
    public sealed record TemplateAppFlagRequirement(string FlagName, bool RequiredValue, string Reason);

    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        WriteIndented = true,
        Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping,
    };


    public static IReadOnlyList<EmbeddedTemplateSummary> ListEmbeddedControlTemplates()
    {
        ReadEmbeddedAllControlsEntries(out var embeddedEntries);
        var templates = ReadUsedTemplates(embeddedEntries);
        var variantMap = CollectVariantsByTemplateName(embeddedEntries);

        return templates
            .Select(t =>
            {
                var requiresVariant = RequiresVariantKeyword(t.Name, t.Id, t.TemplateXml, variantMap);
                var variants = MergeVariants(t.Name, t.Id, t.TemplateXml, variantMap);
                var flagReqs = BuildAppFlagRequirements(t.TemplateXml);
                return new EmbeddedTemplateSummary(t.Name, t.Version, t.Id, ToYamlControlName(t.Name), requiresVariant, variants, flagReqs);
            })
            .OrderBy(t => t.Name, StringComparer.OrdinalIgnoreCase)
            .ThenBy(t => t.Version, StringComparer.OrdinalIgnoreCase)
            .ToList();
    }

    public static EmbeddedTemplateDetails DescribeEmbeddedTemplate(string templateName, string? templateVersion = null)
    {
        if (string.IsNullOrWhiteSpace(templateName))
        {
            throw new ArgumentException("Template name must be provided.", nameof(templateName));
        }

        ReadEmbeddedAllControlsEntries(out var embeddedEntries);
        var templates = ReadUsedTemplates(embeddedEntries);

        var matchingByName = templates
            .Where(t => string.Equals(t.Name, templateName, StringComparison.OrdinalIgnoreCase))
            .ToList();

        if (matchingByName.Count == 0)
        {
            throw new InvalidDataException($"Template '{templateName}' was not found in embedded all-controls templates.");
        }

        UsedTemplate selected;
        if (!string.IsNullOrWhiteSpace(templateVersion))
        {
            selected = matchingByName.FirstOrDefault(t => string.Equals(t.Version, templateVersion, StringComparison.OrdinalIgnoreCase));
            if (string.IsNullOrWhiteSpace(selected.Name))
            {
                throw new InvalidDataException($"Template '{templateName}' with version '{templateVersion}' was not found in embedded all-controls templates.");
            }
        }
        else
        {
            selected = matchingByName
                .OrderByDescending(t => SemverTuple(t.Version))
                .First();
        }

        var properties = ParseTemplateProperties(selected.TemplateXml)
            .OrderBy(p => p.PropertyName, StringComparer.OrdinalIgnoreCase)
            .ToList();

        var variantMap = CollectVariantsByTemplateName(embeddedEntries);
        var requiresVariant = RequiresVariantKeyword(selected.Name, selected.Id, selected.TemplateXml, variantMap);
        var variants = MergeVariants(selected.Name, selected.Id, selected.TemplateXml, variantMap);
        var flagReqs = BuildAppFlagRequirements(selected.TemplateXml);

        return new EmbeddedTemplateDetails(selected.Name, selected.Version, selected.Id, ToYamlControlName(selected.Name), properties, requiresVariant, variants, flagReqs, AutoLayoutChildProperties);
    }


    public static void UnpackToDirectory(string msappPath, string outputDirectory)
    {
        if (Directory.Exists(outputDirectory))
        {
            Directory.Delete(outputDirectory, true);
        }

        Directory.CreateDirectory(outputDirectory);
        ZipFile.ExtractToDirectory(msappPath, outputDirectory);
    }

    public static void InitEmptyAppDirectory(string outputDirectory, string screenName = "Screen1")
    {
        if (string.IsNullOrWhiteSpace(outputDirectory))
        {
            throw new ArgumentException("Output directory must be provided.", nameof(outputDirectory));
        }

        if (string.IsNullOrWhiteSpace(screenName))
        {
            throw new ArgumentException("Screen name must be provided.", nameof(screenName));
        }

        var normalizedScreenName = screenName.Trim();
        if (!IdentifierRegex.IsMatch(normalizedScreenName))
        {
            throw new InvalidDataException($"Invalid screen name '{normalizedScreenName}'. Use a valid Power Fx identifier.");
        }

        if (Directory.Exists(outputDirectory))
        {
            var hasExistingContent = Directory.EnumerateFileSystemEntries(outputDirectory).Any();
            if (hasExistingContent)
            {
                throw new InvalidOperationException($"Output directory '{outputDirectory}' already exists and is not empty.");
            }
        }

        Directory.CreateDirectory(outputDirectory);

        var serializer = new SerializerBuilder().Build();
        var entries = new Dictionary<string, byte[]>(StringComparer.OrdinalIgnoreCase);

        var appTop = CreateDefaultApp(normalizedScreenName);
        var screenTop = CreateDefaultScreen(normalizedScreenName);
        screenTop["ControlUniqueId"] = "4";
        screenTop["Rules"] = new JsonArray
        {
            new JsonObject
            {
                ["Property"] = "LoadingSpinnerColor",
                ["Category"] = "Design",
                ["InvariantScript"] = "RGBA(56, 96, 178, 1)",
                ["RuleProviderType"] = "Unknown",
            },
        };
        screenTop["ControlPropertyState"] = new JsonArray("LoadingSpinnerColor");

        entries["Controls/1.json"] = Encoding.UTF8.GetBytes(WrapTopParent(appTop).ToJsonString(JsonOptions));
        entries["Controls/4.json"] = Encoding.UTF8.GetBytes(WrapTopParent(screenTop).ToJsonString(JsonOptions));

        var appYamlPayload = new Dictionary<string, object?>
        {
            ["App"] = new Dictionary<string, object?>
            {
                ["Properties"] = new Dictionary<string, object?>
                {
                    ["StartScreen"] = "=" + normalizedScreenName,
                    ["Theme"] = "=PowerAppsTheme",
                },
            },
        };

        var screenYamlPayload = new Dictionary<string, object?>
        {
            ["Screens"] = new Dictionary<string, object?>
            {
                [normalizedScreenName] = new Dictionary<string, object?>
                {
                    ["Children"] = new List<object?>(),
                },
            },
        };

        var editorStatePayload = new Dictionary<string, object?>
        {
            ["EditorState"] = new Dictionary<string, object?>
            {
                ["ScreensOrder"] = new List<string> { normalizedScreenName },
                ["ComponentDefinitionsOrder"] = new List<string>(),
            },
        };

        static string WithYamlHeader(string body)
            => string.Join(Environment.NewLine, HeaderLines) + Environment.NewLine + body;

        entries["Src/App.pa.yaml"] = Encoding.UTF8.GetBytes(WithYamlHeader(serializer.Serialize(appYamlPayload)));
        entries[$"Src/{normalizedScreenName}.pa.yaml"] = Encoding.UTF8.GetBytes(WithYamlHeader(serializer.Serialize(screenYamlPayload)));
        entries["Src/_EditorState.pa.yaml"] = Encoding.UTF8.GetBytes(WithYamlHeader(serializer.Serialize(editorStatePayload)));

        entries["References/DataSources.json"] = Encoding.UTF8.GetBytes("{\n  \"DataSources\": []\n}");
        entries["References/QualifiedValues.json"] = Encoding.UTF8.GetBytes("{\n  \"QualifiedValues\": []\n}");
        entries["References/Templates.json"] = Encoding.UTF8.GetBytes("{\n  \"UsedTemplates\": []\n}");
        entries["References/Themes.json"] = Encoding.UTF8.GetBytes("{\n  \"CurrentTheme\": \"v9WebLightTheme\"\n}");
        entries["References/ModernThemes.json"] = Encoding.UTF8.GetBytes("{\n  \"$schema\": \"\"\n}");
        entries["References/Resources.json"] = Encoding.UTF8.GetBytes("{\n  \"Resources\": []\n}");

        entries["Properties.json"] = Encoding.UTF8.GetBytes("{\n  \"Name\": \"App\",\n  \"ControlCount\": {\n    \"screen\": 1\n  }\n}");
        entries["Header.json"] = Encoding.UTF8.GetBytes("{}\n");

        WriteEntriesToDirectory(entries, outputDirectory);
    }

    public static void PackFromDirectory(string unpackDirectory, string outputMsappPath, bool ignoreMissingDataSources = false)
    {
        var entries = ReadEntries(unpackDirectory);

        var schemaIssues = ValidatePaYamlFilesAgainstSchema(entries);
        if (schemaIssues.Count > 0)
        {
            Console.Error.WriteLine($"PA YAML schema validation: {schemaIssues.Count} issue(s) found:");
            foreach (var issue in schemaIssues)
            {
                Console.Error.WriteLine($"  {issue}");
            }

            throw new InvalidDataException($"PA YAML schema validation failed with {schemaIssues.Count} issue(s). See error output for details.");
        }

        var deserializer = new DeserializerBuilder().IgnoreUnmatchedProperties().Build();
        var serializer = new SerializerBuilder().Build();
        var templateResolver = BuildTemplateResolver(entries, unpackDirectory);
        var metadataResolver = BuildControlMetadataResolver(entries);
        var propertyResolver = BuildTemplatePropertyResolver(entries);
        var keywordResolver = BuildTemplateKeywordResolver(entries);
        var appFlags = ReadAppPreviewFlagsMap(entries);
        var flagRequirementsMap = BuildTemplateFlagRequirementsMap();

        var existingControlFiles = ParseTopParentFiles(entries, "Controls/");
        var existingComponentFiles = ParseTopParentFiles(entries, "Components/");

        var existingScreenByName = existingControlFiles.Values
            .Select(v => v["TopParent"] as JsonObject)
            .Where(v => v is not null)
            .Cast<JsonObject>()
            .Where(v => string.Equals(v["Template"]?["Name"]?.GetValue<string>(), "screen", StringComparison.OrdinalIgnoreCase))
            .ToDictionary(v => v["Name"]?.GetValue<string>() ?? string.Empty, v => v, StringComparer.OrdinalIgnoreCase);

        var existingComponentByName = existingComponentFiles.Values
            .Select(v => v["TopParent"] as JsonObject)
            .Where(v => v is not null)
            .Cast<JsonObject>()
            .ToDictionary(v => v["Name"]?.GetValue<string>() ?? string.Empty, v => v, StringComparer.OrdinalIgnoreCase);

        var allExistingNodes = new Dictionary<string, JsonObject>(StringComparer.OrdinalIgnoreCase);
        foreach (var top in existingControlFiles.Values.Select(v => v["TopParent"] as JsonObject).Where(v => v is not null).Cast<JsonObject>())
        {
            IndexByName(top, allExistingNodes);
        }

        foreach (var top in existingComponentFiles.Values.Select(v => v["TopParent"] as JsonObject).Where(v => v is not null).Cast<JsonObject>())
        {
            IndexByName(top, allExistingNodes);
        }

        var componentMetadataByName = ReadComponentMetadata(entries);

        var appTop = existingControlFiles.Values
            .Select(v => v["TopParent"] as JsonObject)
            .FirstOrDefault(v => string.Equals(v?["Name"]?.GetValue<string>(), "App", StringComparison.OrdinalIgnoreCase));

        if (appTop is null)
        {
            throw new InvalidDataException("Could not find App top parent in Controls/*.json.");
        }

        var appControlFileName = existingControlFiles.FirstOrDefault(kv => string.Equals(kv.Value["TopParent"]?["Name"]?.GetValue<string>(), "App", StringComparison.OrdinalIgnoreCase)).Key;
        if (string.IsNullOrWhiteSpace(appControlFileName))
        {
            appControlFileName = "Controls/1.json";
        }

        if (entries.TryGetValue("Src/App.pa.yaml", out var appYamlBytes))
        {
            var appYaml = Encoding.UTF8.GetString(appYamlBytes);
            if (TryParseYamlMap(deserializer, appYaml, out var appRoot)
                && TryGetMap(appRoot, "App", out var appMap)
                && TryGetMap(appMap, "Properties", out var appProps))
            {
                UpsertProperties(appTop, appProps);
            }
        }

        var screenPayloads = new Dictionary<string, Dictionary<object, object?>>(StringComparer.OrdinalIgnoreCase);
        var componentPayloads = new Dictionary<string, Dictionary<object, object?>>(StringComparer.OrdinalIgnoreCase);
        var screenOrderFromFiles = new List<string>();
        var componentOrderFromFiles = new List<string>();

        foreach (var (path, bytes) in entries.Where(e => e.Key.StartsWith("Src/", StringComparison.OrdinalIgnoreCase) && e.Key.EndsWith(".pa.yaml", StringComparison.OrdinalIgnoreCase)).OrderBy(e => e.Key, StringComparer.OrdinalIgnoreCase))
        {
            var fileName = Path.GetFileName(path);
            if (string.Equals(fileName, "App.pa.yaml", StringComparison.OrdinalIgnoreCase)
                || string.Equals(fileName, "_EditorState.pa.yaml", StringComparison.OrdinalIgnoreCase))
            {
                continue;
            }

            var yaml = Encoding.UTF8.GetString(bytes);
            if (!TryParseYamlMap(deserializer, yaml, out var root))
            {
                continue;
            }

            if (path.StartsWith("Src/Components/", StringComparison.OrdinalIgnoreCase))
            {
                if (TryGetMap(root, "ComponentDefinitions", out var defs))
                {
                    foreach (var kv in defs)
                    {
                        var n = kv.Key?.ToString() ?? string.Empty;
                        if (string.IsNullOrWhiteSpace(n) || kv.Value is not Dictionary<object, object?> payload)
                        {
                            continue;
                        }

                        componentPayloads[n] = payload;
                        if (!componentOrderFromFiles.Contains(n, StringComparer.OrdinalIgnoreCase))
                        {
                            componentOrderFromFiles.Add(n);
                        }
                    }
                }

                continue;
            }

            if (TryGetMap(root, "Screens", out var screens))
            {
                foreach (var kv in screens)
                {
                    var n = kv.Key?.ToString() ?? string.Empty;
                    if (string.IsNullOrWhiteSpace(n) || kv.Value is not Dictionary<object, object?> payload)
                    {
                        continue;
                    }

                    screenPayloads[n] = payload;
                    if (!screenOrderFromFiles.Contains(n, StringComparer.OrdinalIgnoreCase))
                    {
                        screenOrderFromFiles.Add(n);
                    }
                }
            }
        }

        var idAllocator = new ControlIdAllocator(existingControlFiles.Values
            .Concat(existingComponentFiles.Values)
            .Select(v => v["TopParent"] as JsonObject)
            .Where(v => v is not null)
            .Cast<JsonObject>()
            .SelectMany(t => Flatten(t))
            .Select(n => n["ControlUniqueId"]?.GetValue<string>() ?? string.Empty));

        var topControlFiles = new Dictionary<string, JsonObject>(StringComparer.OrdinalIgnoreCase)
        {
            [appControlFileName] = WrapTopParent(appTop),
        };

        var screenIndex = 0;
        foreach (var screenName in screenPayloads.Keys.OrderBy(n => n, StringComparer.OrdinalIgnoreCase))
        {
            var payload = screenPayloads[screenName];
            var existing = existingScreenByName.TryGetValue(screenName, out var e) ? e : null;
            var root = existing is null ? CreateDefaultScreen(screenName) : (JsonObject)existing.DeepClone();

            root["Name"] = screenName;
            root["Parent"] = string.Empty;
            root["Template"] ??= new JsonObject();
            (root["Template"] as JsonObject)!["Name"] = "screen";
            (root["Template"] as JsonObject)!["Id"] ??= "http://microsoft.com/appmagic/screen";
            (root["Template"] as JsonObject)!["Version"] ??= "1.0";
            root["Index"] = screenIndex;
            root["PublishOrderIndex"] = 0;
            root["ControlUniqueId"] = idAllocator.GetOrCreate(screenName, existing);

            ApplyFromYaml(root, payload, screenName, allExistingNodes, idAllocator, templateResolver, metadataResolver, propertyResolver, keywordResolver, appFlags, flagRequirementsMap);

            var fileName = existingControlFiles.FirstOrDefault(kv => string.Equals(kv.Value["TopParent"]?["Name"]?.GetValue<string>(), screenName, StringComparison.OrdinalIgnoreCase)).Key;
            if (string.IsNullOrWhiteSpace(fileName))
            {
                fileName = $"Controls/{root["ControlUniqueId"]?.GetValue<string>()}.json";
            }

            topControlFiles[fileName] = WrapTopParent(root);
            screenIndex++;
        }

        var topComponentFiles = new Dictionary<string, JsonObject>(StringComparer.OrdinalIgnoreCase);
        foreach (var componentName in componentPayloads.Keys.OrderBy(n => n, StringComparer.OrdinalIgnoreCase))
        {
            var payload = componentPayloads[componentName];
            var existing = existingComponentByName.TryGetValue(componentName, out var e) ? e : null;
            var root = existing is null ? CreateDefaultComponent(componentName) : (JsonObject)existing.DeepClone();

            root["Name"] = componentName;
            root["Parent"] = string.Empty;
            root["Template"] ??= new JsonObject();
            var template = (JsonObject)root["Template"]!;
            template["Id"] = "http://microsoft.com/appmagic/Component";
            template["Version"] ??= "1.0";
            template["IsComponentDefinition"] = true;
            template["Name"] = ResolveComponentTemplateName(componentName, template, componentMetadataByName);

            root["ControlUniqueId"] = idAllocator.GetOrCreate(componentName, existing);

            ApplyFromYaml(root, payload, componentName, allExistingNodes, idAllocator, templateResolver, metadataResolver, propertyResolver, keywordResolver, appFlags, flagRequirementsMap);

            var fileName = existingComponentFiles.FirstOrDefault(kv => string.Equals(kv.Value["TopParent"]?["Name"]?.GetValue<string>(), componentName, StringComparison.OrdinalIgnoreCase)).Key;
            if (string.IsNullOrWhiteSpace(fileName))
            {
                fileName = $"Components/{root["ControlUniqueId"]?.GetValue<string>()}.json";
            }

            topComponentFiles[fileName] = WrapTopParent(root);
        }

        UpdateUsedTemplates(entries, topControlFiles, topComponentFiles);
        var galleryTemplateDescriptor = BuildGalleryTemplateDescriptor(entries);
        ApplyLegacyControlParityTransforms(topControlFiles, topComponentFiles, galleryTemplateDescriptor);
        metadataResolver.NormalizeReservedEnumTokensInTopFiles(topControlFiles);
        metadataResolver.NormalizeReservedEnumTokensInTopFiles(topComponentFiles);
        FixAutoLayoutChildDynamicProperties(topControlFiles, topComponentFiles);

        ReplaceEntries(entries, "Controls/", topControlFiles);
        ReplaceEntries(entries, "Components/", topComponentFiles);

        UpdateComponentMetadata(entries, componentPayloads, topComponentFiles);
        UpdatePropertiesControlCount(entries, topControlFiles, topComponentFiles);
        var discoveredScreens = topControlFiles.Values
            .Select(v => v["TopParent"] as JsonObject)
            .Where(v => v is not null)
            .Cast<JsonObject>()
            .Where(v => string.Equals(v["Template"]?["Name"]?.GetValue<string>(), "screen", StringComparison.OrdinalIgnoreCase))
            .Select(v => v["Name"]?.GetValue<string>() ?? string.Empty)
            .Where(n => !string.IsNullOrWhiteSpace(n))
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToList();

        var discoveredComponents = topComponentFiles.Values
            .Select(v => v["TopParent"] as JsonObject)
            .Where(v => v is not null)
            .Cast<JsonObject>()
            .Select(v => v["Name"]?.GetValue<string>() ?? string.Empty)
            .Where(n => !string.IsNullOrWhiteSpace(n))
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToList();

        UpdateEditorState(entries, serializer, deserializer, screenOrderFromFiles, componentOrderFromFiles, discoveredScreens, discoveredComponents);
        NormalizeHostControlMetadata(entries);
        NormalizeAppInfoRuleProviders(entries);
        NormalizeThemeMetadata(entries);
        NormalizeModernThemeMetadata(entries);
        NormalizePropertiesMetadata(entries);
        NormalizeHeaderMetadata(entries);
        EnsurePublishInfoResource(entries);
        NormalizeDataSourceMetadata(entries);
        RemoveStaticCollectionDataSources(entries, topControlFiles, topComponentFiles);
        if (!ignoreMissingDataSources)
            ValidateDataSourcesContainCollections(entries, topControlFiles, topComponentFiles);

        ValidateUniqueControlNames(topControlFiles, topComponentFiles);

        WriteMsApp(entries, outputMsappPath);
    }

    /// <summary>
    /// Validates that all control names (including screen names) are globally unique
    /// across all screens and components. Power Apps requires unique control names;
    /// duplicate names cause generic load errors in Studio.
    /// </summary>
    private static void ValidateUniqueControlNames(
        Dictionary<string, JsonObject> topControlFiles,
        Dictionary<string, JsonObject> topComponentFiles)
    {
        // Map: control name -> list of (screen/component name, control name) for error reporting
        var nameLocations = new Dictionary<string, List<string>>(StringComparer.OrdinalIgnoreCase);

        void CollectNames(Dictionary<string, JsonObject> files, string kind)
        {
            foreach (var (fileName, wrapper) in files)
            {
                var topParent = wrapper["TopParent"] as JsonObject;
                if (topParent is null)
                    continue;

                var topName = topParent["Name"]?.GetValue<string>() ?? fileName;
                var templateName = topParent["Template"]?["Name"]?.GetValue<string>() ?? string.Empty;

                // Skip the App control itself — it's special
                if (string.Equals(topName, "App", StringComparison.OrdinalIgnoreCase))
                    continue;

                foreach (var node in Flatten(topParent))
                {
                    var controlName = node["Name"]?.GetValue<string>();
                    if (string.IsNullOrWhiteSpace(controlName))
                        continue;

                    if (!nameLocations.TryGetValue(controlName, out var locations))
                    {
                        locations = new List<string>();
                        nameLocations[controlName] = locations;
                    }

                    locations.Add($"{kind} '{topName}'");
                }
            }
        }

        CollectNames(topControlFiles, "Screen");
        CollectNames(topComponentFiles, "Component");

        var duplicates = nameLocations
            .Where(kv => kv.Value.Count > 1)
            .OrderBy(kv => kv.Key, StringComparer.OrdinalIgnoreCase)
            .ToList();

        if (duplicates.Count > 0)
        {
            var messages = new List<string>();
            foreach (var dup in duplicates)
            {
                var locations = string.Join(", ", dup.Value.Distinct(StringComparer.OrdinalIgnoreCase));
                messages.Add($"  Control '{dup.Key}' appears in: {locations}");
            }

            throw new InvalidDataException(
                $"Duplicate control names detected ({duplicates.Count}). " +
                $"Power Apps requires globally unique control names across all screens and components.\n" +
                string.Join("\n", messages));
        }
    }

}
