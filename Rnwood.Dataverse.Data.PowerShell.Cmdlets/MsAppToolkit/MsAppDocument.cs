using System.Globalization;
using System.IO.Compression;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Xml.Linq;
using YamlDotNet.Serialization;

namespace MsAppToolkit;

public sealed class MsAppDocument
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        WriteIndented = true,
    };

    private readonly Dictionary<string, byte[]> _entries;
    private readonly Dictionary<string, JsonObject> _controlFiles;
    private readonly Dictionary<string, JsonObject> _componentFiles;
    private readonly JsonObject _referencesTemplates;
    private readonly JsonObject _referencesThemes;

    private MsAppDocument(
        Dictionary<string, byte[]> entries,
        Dictionary<string, JsonObject> controlFiles,
        Dictionary<string, JsonObject> componentFiles,
        JsonObject referencesTemplates,
        JsonObject referencesThemes)
    {
        _entries = entries;
        _controlFiles = controlFiles;
        _componentFiles = componentFiles;
        _referencesTemplates = referencesTemplates;
        _referencesThemes = referencesThemes;
    }

    public IReadOnlyDictionary<string, JsonObject> ControlFiles => _controlFiles;
    public IReadOnlyDictionary<string, JsonObject> ComponentFiles => _componentFiles;

    public static MsAppDocument Load(string msappPath)
    {
        var entries = new Dictionary<string, byte[]>(StringComparer.OrdinalIgnoreCase);

        using (var archive = ZipFile.OpenRead(msappPath))
        {
            foreach (var entry in archive.Entries)
            {
                using var stream = entry.Open();
                using var memory = new MemoryStream();
                stream.CopyTo(memory);
                entries[entry.FullName.Replace('\\', '/')] = memory.ToArray();
            }
        }

        var controlFiles = new Dictionary<string, JsonObject>(StringComparer.OrdinalIgnoreCase);
        foreach (var pair in entries.Where(e => e.Key.StartsWith("Controls/", StringComparison.OrdinalIgnoreCase) && e.Key.EndsWith(".json", StringComparison.OrdinalIgnoreCase)))
        {
            var parsed = JsonNode.Parse(pair.Value)?.AsObject() ?? throw new InvalidDataException($"Invalid control file: {pair.Key}");
            controlFiles[pair.Key] = parsed;
        }

        var componentFiles = new Dictionary<string, JsonObject>(StringComparer.OrdinalIgnoreCase);
        foreach (var pair in entries.Where(e => e.Key.StartsWith("Components/", StringComparison.OrdinalIgnoreCase) && e.Key.EndsWith(".json", StringComparison.OrdinalIgnoreCase)))
        {
            var parsed = JsonNode.Parse(pair.Value)?.AsObject() ?? throw new InvalidDataException($"Invalid component file: {pair.Key}");
            componentFiles[pair.Key] = parsed;
        }

        var templates = TryReadJsonObject(entries, "References/Templates.json") ?? new JsonObject();
        var themes = TryReadJsonObject(entries, "References/Themes.json") ?? new JsonObject();

        return new MsAppDocument(entries, controlFiles, componentFiles, templates, themes);
    }

    public void Save(string msappPath)
    {
        foreach (var (entryName, json) in _controlFiles)
        {
            _entries[entryName] = Encoding.UTF8.GetBytes(json.ToJsonString(JsonOptions));
        }

        foreach (var (entryName, json) in _componentFiles)
        {
            _entries[entryName] = Encoding.UTF8.GetBytes(json.ToJsonString(JsonOptions));
        }

        _entries["References/Templates.json"] = Encoding.UTF8.GetBytes(_referencesTemplates.ToJsonString(JsonOptions));
        _entries["References/Themes.json"] = Encoding.UTF8.GetBytes(_referencesThemes.ToJsonString(JsonOptions));

        if (File.Exists(msappPath))
        {
            File.Delete(msappPath);
        }

        using var archive = ZipFile.Open(msappPath, ZipArchiveMode.Create);
        foreach (var (path, data) in _entries.OrderBy(k => k.Key, StringComparer.OrdinalIgnoreCase))
        {
            var entry = archive.CreateEntry(path, CompressionLevel.Optimal);
            using var stream = entry.Open();
            stream.Write(data, 0, data.Length);
        }
    }

    public IReadOnlyList<ControlNode> GetScreens()
    {
        return GetTopParents()
            .Where(n => string.Equals(n.TemplateName, "screen", StringComparison.OrdinalIgnoreCase))
            .ToList();
    }

    public IReadOnlyList<ControlNode> GetTopParents()
    {
        var list = new List<ControlNode>();
        foreach (var (fileName, json) in _controlFiles)
        {
            var top = json["TopParent"] as JsonObject;
            if (top is null)
            {
                continue;
            }

            list.Add(ControlNode.FromJson(fileName, top));
        }

        foreach (var (fileName, json) in _componentFiles)
        {
            var top = json["TopParent"] as JsonObject;
            if (top is null)
            {
                continue;
            }

            list.Add(ControlNode.FromJson(fileName, top));
        }

        return list;
    }

    public IReadOnlyList<ControlNode> GetComponentDefinitions()
    {
        return GetTopParents()
            .Where(n => n.IsComponentDefinition)
            .ToList();
    }

    public ControlNode? FindControlByName(string name)
    {
        foreach (var root in GetTopParents())
        {
            var hit = root.SelfAndDescendants().FirstOrDefault(c => string.Equals(c.Name, name, StringComparison.OrdinalIgnoreCase));
            if (hit is not null)
            {
                return hit;
            }
        }

        return null;
    }

    public IReadOnlyDictionary<string, string?> ResolveEffectiveProperties(ControlNode control)
    {
        var templateCatalog = TemplateCatalog.FromTemplatesJson(_referencesTemplates);
        var themeCatalog = ThemeCatalog.FromThemesJson(_referencesThemes);

        var templateDefaults = templateCatalog.ResolveDefaults(control.TemplateName, control.TemplateVersion);
        var styleDefaults = themeCatalog.ResolveStyle(control.StyleName);
        var explicitRules = control.Rules.ToDictionary(k => k.Property, v => v.InvariantScript, StringComparer.OrdinalIgnoreCase);

        var merged = new Dictionary<string, string?>(StringComparer.OrdinalIgnoreCase);
        foreach (var (k, v) in templateDefaults)
        {
            merged[k] = v;
        }

        foreach (var (k, v) in styleDefaults)
        {
            merged[k] = v;
        }

        foreach (var (k, v) in explicitRules)
        {
            merged[k] = v;
        }

        return merged;
    }

    public string ExportScreenYaml(string screenName, bool includeHeader = true, bool includeDefaults = false)
    {
        var screen = GetScreens().FirstOrDefault(s => string.Equals(s.Name, screenName, StringComparison.OrdinalIgnoreCase));
        if (screen is null)
        {
            throw new KeyNotFoundException($"Screen '{screenName}' was not found.");
        }

        var serializer = new SerializerBuilder().ConfigureDefaultValuesHandling(DefaultValuesHandling.OmitNull).Build();

        var root = new Dictionary<string, object?>
        {
            ["Screens"] = new Dictionary<string, object?>
            {
                [screen.Name] = ToYamlScreenPayload(screen, includeDefaults),
            },
        };

        var yaml = serializer.Serialize(root);
        if (!includeHeader)
        {
            return yaml;
        }

        var header = string.Join(Environment.NewLine, HeaderLines) + Environment.NewLine;
        return header + yaml;
    }

    public void ImportScreenYaml(string yaml)
    {
        var deserializer = new DeserializerBuilder().IgnoreUnmatchedProperties().Build();
        var parsed = deserializer.Deserialize<Dictionary<object, object?>>(yaml)
            ?? throw new InvalidDataException("YAML content is empty or invalid.");

        if (!TryGetMap(parsed, "Screens", out var screensMap))
        {
            throw new InvalidDataException("Expected top-level 'Screens' mapping.");
        }

        foreach (var kv in screensMap)
        {
            var screenName = kv.Key?.ToString() ?? string.Empty;
            if (string.IsNullOrWhiteSpace(screenName))
            {
                continue;
            }

            var screenNode = GetScreens().FirstOrDefault(s => string.Equals(s.Name, screenName, StringComparison.OrdinalIgnoreCase));
            if (screenNode is null)
            {
                continue;
            }

            if (kv.Value is not Dictionary<object, object?> screenPayload)
            {
                continue;
            }

            ApplyScreenPayload(screenNode, screenPayload);
            UpsertScreenYamlSource(screenNode.Name, yaml);
        }
    }

    private static JsonObject? TryReadJsonObject(Dictionary<string, byte[]> entries, string key)
    {
        if (!entries.TryGetValue(key, out var bytes))
        {
            return null;
        }

        return JsonNode.Parse(bytes)?.AsObject();
    }

    private static object ToYamlScreenPayload(ControlNode screen, bool includeDefaults)
    {
        var payload = new Dictionary<string, object?>();

        var properties = screen.Rules.ToDictionary(
            r => r.Property,
            r => PrefixFormula(r.InvariantScript),
            StringComparer.OrdinalIgnoreCase);

        if (properties.Count > 0)
        {
            payload["Properties"] = properties;
        }

        var children = new List<object>();
        foreach (var child in screen.Children)
        {
            children.Add(new Dictionary<string, object?> { [child.Name] = ToYamlControlPayload(child, includeDefaults) });
        }

        if (children.Count > 0)
        {
            payload["Children"] = children;
        }

        return payload;
    }

    private static object ToYamlControlPayload(ControlNode node, bool includeDefaults)
    {
        var payload = new Dictionary<string, object?>();

        payload["Control"] = FormatControlType(node.TemplateName, node.TemplateVersion);
        if (!string.IsNullOrWhiteSpace(node.VariantName))
        {
            payload["Variant"] = node.VariantName;
        }

        var properties = node.Rules.ToDictionary(
            r => r.Property,
            r => PrefixFormula(r.InvariantScript),
            StringComparer.OrdinalIgnoreCase);

        if (properties.Count > 0)
        {
            payload["Properties"] = properties;
        }

        if (node.Children.Count > 0)
        {
            payload["Children"] = node.Children
                .Select(c => (object)new Dictionary<string, object?> { [c.Name] = ToYamlControlPayload(c, includeDefaults) })
                .ToList();
        }

        return payload;
    }

    private static string PrefixFormula(string? fx)
    {
        if (string.IsNullOrWhiteSpace(fx))
        {
            return "=";
        }

        return (fx.Length > 0 && fx[0] == '=') ? fx : $"={fx}";
    }

    private static string StripFormulaPrefix(string? fx)
    {
        if (string.IsNullOrWhiteSpace(fx))
        {
            return string.Empty;
        }

        return (fx.Length > 0 && fx[0] == '=') ? fx.Substring(1) : fx;
    }

    private static string FormatControlType(string templateName, string templateVersion)
    {
        if (string.IsNullOrWhiteSpace(templateName))
        {
            return "Unknown";
        }

        var title = char.ToUpperInvariant(templateName[0]) + templateName.Substring(1);
        return string.IsNullOrWhiteSpace(templateVersion) ? title : $"{title}@{templateVersion}";
    }

    private static (string name, string version) ParseControlType(string controlType)
    {
        if (string.IsNullOrWhiteSpace(controlType))
        {
            return ("unknown", "1.0.0");
        }

        var parts = controlType.Split(new[] { '@' }, 2).Select(p => p.Trim()).ToArray();
        var name = parts[0].ToLowerInvariant();
        var version = parts.Length > 1 ? parts[1] : "1.0.0";
        return (name, version);
    }

    private static bool TryGetMap(Dictionary<object, object?> source, string key, out Dictionary<object, object?> map)
    {
        map = new Dictionary<object, object?>();
        if (!source.TryGetValue(key, out var value) || value is not Dictionary<object, object?> dict)
        {
            return false;
        }

        map = dict;
        return true;
    }

    private void ApplyScreenPayload(ControlNode screenNode, Dictionary<object, object?> screenPayload)
    {
        if (screenPayload.TryGetValue("Properties", out var propertiesObj) && propertiesObj is Dictionary<object, object?> props)
        {
            ApplyProperties(screenNode.Json, props);
        }

        if (screenPayload.TryGetValue("Children", out var childrenObj) && childrenObj is List<object?> children)
        {
            ApplyChildren(screenNode.Json, children, screenNode.Name);
        }
    }

    private void ApplyChildren(JsonObject parentJson, List<object?> yamlChildren, string parentName)
    {
        var existingChildren = (parentJson["Children"] as JsonArray) ?? [];
        var existingByName = existingChildren
            .OfType<JsonObject>()
            .ToDictionary(c => c["Name"]?.GetValue<string>() ?? string.Empty, c => c, StringComparer.OrdinalIgnoreCase);

        var newChildren = new JsonArray();
        var nextIndex = 0;
        foreach (var child in yamlChildren)
        {
            if (child is not Dictionary<object, object?> singleton || singleton.Count != 1)
            {
                continue;
            }

            var pair = singleton.First();
            var childName = pair.Key?.ToString() ?? string.Empty;
            if (string.IsNullOrWhiteSpace(childName) || pair.Value is not Dictionary<object, object?> payload)
            {
                continue;
            }

            JsonObject childJson;
            if (!existingByName.TryGetValue(childName, out childJson!))
            {
                childJson = BuildNewControl(childName, payload, parentName, nextIndex);
            }

            if (payload.TryGetValue("Properties", out var propsObj) && propsObj is Dictionary<object, object?> props)
            {
                ApplyProperties(childJson, props);
            }

            if (payload.TryGetValue("Variant", out var variantObj) && variantObj is not null)
            {
                childJson["VariantName"] = variantObj.ToString();
            }

            childJson["Parent"] = parentName;
            childJson["Index"] = nextIndex;
            childJson["PublishOrderIndex"] = nextIndex;

            if (payload.TryGetValue("Children", out var nestedObj) && nestedObj is List<object?> nestedList)
            {
                ApplyChildren(childJson, nestedList, childName);
            }
            else if (childJson["Children"] is null)
            {
                childJson["Children"] = new JsonArray();
            }

            newChildren.Add(childJson);
            nextIndex++;
        }

        parentJson["Children"] = newChildren;
    }

    private void ApplyProperties(JsonObject controlJson, Dictionary<object, object?> yamlProps)
    {
        var newRules = new JsonArray();
        foreach (var prop in yamlProps)
        {
            var propertyName = prop.Key?.ToString();
            if (string.IsNullOrWhiteSpace(propertyName))
            {
                continue;
            }

            var raw = prop.Value?.ToString() ?? string.Empty;
            var rule = new JsonObject
            {
                ["Property"] = propertyName,
                ["Category"] = "Data",
                ["InvariantScript"] = StripFormulaPrefix(raw),
                ["RuleProviderType"] = "Unknown",
            };

            newRules.Add(rule);
        }

        controlJson["Rules"] = newRules;
        controlJson["ControlPropertyState"] = new JsonArray(yamlProps.Keys.Select(k => JsonValue.Create(k.ToString())).ToArray());
    }

    private JsonObject BuildNewControl(string name, Dictionary<object, object?> payload, string parentName, int index)
    {
        var controlType = payload.TryGetValue("Control", out var ctObj) ? ctObj?.ToString() ?? "Label" : "Label";
        var (templateName, templateVersion) = ParseControlType(controlType);
        var templateId = $"http://microsoft.com/appmagic/{templateName}";

        var control = new JsonObject
        {
            ["Type"] = "ControlInfo",
            ["Name"] = name,
            ["Template"] = new JsonObject
            {
                ["Id"] = templateId,
                ["Version"] = templateVersion,
                ["LastModifiedTimestamp"] = "0",
                ["Name"] = templateName,
                ["FirstParty"] = true,
                ["IsPremiumPcfControl"] = false,
                ["IsCustomGroupControlTemplate"] = false,
                ["CustomGroupControlTemplateName"] = "",
                ["IsComponentDefinition"] = false,
                ["OverridableProperties"] = new JsonObject(),
            },
            ["Index"] = index,
            ["PublishOrderIndex"] = index,
            ["VariantName"] = payload.TryGetValue("Variant", out var variantObj) ? variantObj?.ToString() ?? string.Empty : string.Empty,
            ["LayoutName"] = string.Empty,
            ["MetaDataIDKey"] = string.Empty,
            ["PersistMetaDataIDKey"] = false,
            ["IsFromScreenLayout"] = false,
            ["StyleName"] = string.Empty,
            ["Parent"] = parentName,
            ["IsDataControl"] = false,
            ["AllowAccessToGlobals"] = true,
            ["OptimizeForDevices"] = "Off",
            ["IsGroupControl"] = false,
            ["IsAutoGenerated"] = false,
            ["Rules"] = new JsonArray(),
            ["ControlPropertyState"] = new JsonArray(),
            ["IsLocked"] = false,
            ["ControlUniqueId"] = NextUniqueId(),
            ["Children"] = new JsonArray(),
        };

        return control;
    }

    private string NextUniqueId()
    {
        var max = 0;
        foreach (var top in GetTopParents())
        {
            foreach (var node in top.SelfAndDescendants())
            {
                if (int.TryParse(node.ControlUniqueId, NumberStyles.Integer, CultureInfo.InvariantCulture, out var id) && id > max)
                {
                    max = id;
                }
            }
        }

        return (max + 1).ToString(CultureInfo.InvariantCulture);
    }

    private void UpsertScreenYamlSource(string screenName, string yaml)
    {
        var key = $"Src/{screenName}.pa.yaml";
        _entries[key] = Encoding.UTF8.GetBytes(yaml);
    }

    private static readonly string[] HeaderLines =
    [
        "# ************************************************************************************************",
        "# Warning: YAML source code for Canvas Apps should only be used to review changes made within Power Apps Studio and for minor edits (Preview).",
        "# Use the maker portal to create and edit your Power Apps.",
        "# ",
        "# The schema file for Canvas Apps is available at https://go.microsoft.com/fwlink/?linkid=2304907",
        "# ",
        "# For more information, visit https://go.microsoft.com/fwlink/?linkid=2292623",
        "# ************************************************************************************************",
    ];
}

public sealed class ControlNode
{
    private ControlNode(string sourceControlFile, JsonObject json)
    {
        SourceControlFile = sourceControlFile;
        Json = json;
        Name = json["Name"]?.GetValue<string>() ?? string.Empty;
        StyleName = json["StyleName"]?.GetValue<string>() ?? string.Empty;
        VariantName = json["VariantName"]?.GetValue<string>() ?? string.Empty;
        ControlUniqueId = json["ControlUniqueId"]?.GetValue<string>() ?? string.Empty;

        var template = json["Template"] as JsonObject;
        TemplateName = template?["Name"]?.GetValue<string>() ?? string.Empty;
        TemplateVersion = template?["Version"]?.GetValue<string>() ?? string.Empty;
        IsComponentDefinition = template?["IsComponentDefinition"]?.GetValue<bool>() ?? false;

        Rules = ((json["Rules"] as JsonArray) ?? [])
            .OfType<JsonObject>()
            .Select(o => new ControlRule(
                o["Property"]?.GetValue<string>() ?? string.Empty,
                o["InvariantScript"]?.GetValue<string>() ?? string.Empty,
                o["Category"]?.GetValue<string>() ?? "Unknown"))
            .Where(r => !string.IsNullOrWhiteSpace(r.Property))
            .ToList();

        Children = ((json["Children"] as JsonArray) ?? [])
            .OfType<JsonObject>()
            .Select(c => FromJson(sourceControlFile, c))
            .ToList();
    }

    public string SourceControlFile { get; }
    public JsonObject Json { get; }
    public string Name { get; }
    public string StyleName { get; }
    public string VariantName { get; }
    public string ControlUniqueId { get; }
    public string TemplateName { get; }
    public string TemplateVersion { get; }
    public bool IsComponentDefinition { get; }
    public IReadOnlyList<ControlRule> Rules { get; }
    public IReadOnlyList<ControlNode> Children { get; }

    public static ControlNode FromJson(string sourceControlFile, JsonObject json) => new(sourceControlFile, json);

    public IEnumerable<ControlNode> SelfAndDescendants()
    {
        yield return this;
        foreach (var child in Children)
        {
            foreach (var nested in child.SelfAndDescendants())
            {
                yield return nested;
            }
        }
    }
}

public sealed record ControlRule(string Property, string InvariantScript, string Category);

public sealed class TemplateCatalog
{
    private readonly Dictionary<(string Name, string Version), Dictionary<string, string?>> _defaultsByTemplate =
        new();

    private TemplateCatalog()
    {
    }

    public static TemplateCatalog FromTemplatesJson(JsonObject templates)
    {
        var catalog = new TemplateCatalog();
        var used = templates["UsedTemplates"] as JsonArray;
        if (used is null)
        {
            return catalog;
        }

        foreach (var entry in used.OfType<JsonObject>())
        {
            var name = entry["Name"]?.GetValue<string>() ?? string.Empty;
            var version = entry["Version"]?.GetValue<string>() ?? string.Empty;
            var xml = entry["Template"]?.GetValue<string>() ?? string.Empty;
            if (string.IsNullOrWhiteSpace(name) || string.IsNullOrWhiteSpace(version) || string.IsNullOrWhiteSpace(xml))
            {
                continue;
            }

            var defaults = ParseTemplateDefaults(xml);
            catalog._defaultsByTemplate[(name.ToLowerInvariant(), version)] = defaults;
        }

        return catalog;
    }

    public IReadOnlyDictionary<string, string?> ResolveDefaults(string templateName, string templateVersion)
    {
        if (_defaultsByTemplate.TryGetValue((templateName.ToLowerInvariant(), templateVersion), out var exact))
        {
            return exact;
        }

        var match = _defaultsByTemplate
            .Where(k => string.Equals(k.Key.Name, templateName, StringComparison.OrdinalIgnoreCase))
            .OrderByDescending(k => SemverTuple(k.Key.Version))
            .FirstOrDefault();

        return match.Value ?? new Dictionary<string, string?>();
    }

    private static Dictionary<string, string?> ParseTemplateDefaults(string templateXml)
    {
        var defaults = new Dictionary<string, string?>(StringComparer.OrdinalIgnoreCase);
        try
        {
            var root = XDocument.Parse(templateXml).Root;
            if (root is null)
            {
                return defaults;
            }

            foreach (var prop in root.Descendants().Where(e => e.Name.LocalName is "property" or "includeProperty"))
            {
                var name = prop.Attribute("name")?.Value;
                var defaultValue = prop.Attribute("defaultValue")?.Value;
                if (string.IsNullOrWhiteSpace(name) || defaultValue is null)
                {
                    continue;
                }

                defaults[name] = defaultValue;
            }
        }
        catch
        {
            // Ignore invalid template XML and continue.
        }

        return defaults;
    }

    private static (int major, int minor, int patch) SemverTuple(string version)
    {
        var tokens = version.Split(new[] { '.' }, StringSplitOptions.RemoveEmptyEntries).Select(s => s.Trim()).ToArray();
        var nums = new[] { 0, 0, 0 };
        for (var i = 0; i < Math.Min(3, tokens.Length); i++)
        {
            var digits = new string(tokens[i].TakeWhile(char.IsDigit).ToArray());
            nums[i] = int.TryParse(digits, NumberStyles.Integer, CultureInfo.InvariantCulture, out var parsed) ? parsed : 0;
        }

        return (nums[0], nums[1], nums[2]);
    }
}

public sealed class ThemeCatalog
{
    private readonly Dictionary<string, string> _palette = new(StringComparer.OrdinalIgnoreCase);
    private readonly Dictionary<string, Dictionary<string, string>> _styleProperties = new(StringComparer.OrdinalIgnoreCase);

    private ThemeCatalog()
    {
    }

    public static ThemeCatalog FromThemesJson(JsonObject themes)
    {
        var catalog = new ThemeCatalog();
        var current = themes["CurrentTheme"]?.GetValue<string>() ?? string.Empty;
        var customThemes = themes["CustomThemes"] as JsonArray;
        if (customThemes is null)
        {
            return catalog;
        }

        var selected = customThemes
            .OfType<JsonObject>()
            .FirstOrDefault(t => string.Equals(t["name"]?.GetValue<string>(), current, StringComparison.OrdinalIgnoreCase))
            ?? customThemes.OfType<JsonObject>().FirstOrDefault();

        if (selected is null)
        {
            return catalog;
        }

        var palette = selected["palette"] as JsonArray;
        if (palette is not null)
        {
            foreach (var item in palette.OfType<JsonObject>())
            {
                var name = item["name"]?.GetValue<string>() ?? string.Empty;
                var value = item["value"]?.GetValue<string>() ?? string.Empty;
                if (!string.IsNullOrWhiteSpace(name))
                {
                    catalog._palette[name] = value;
                }
            }
        }

        var styles = selected["styles"] as JsonArray;
        if (styles is not null)
        {
            foreach (var style in styles.OfType<JsonObject>())
            {
                var name = style["name"]?.GetValue<string>() ?? string.Empty;
                if (string.IsNullOrWhiteSpace(name))
                {
                    continue;
                }

                var mapped = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
                var pmap = style["propertyValuesMap"] as JsonArray;
                if (pmap is not null)
                {
                    foreach (var row in pmap.OfType<JsonObject>())
                    {
                        var property = row["property"]?.GetValue<string>() ?? string.Empty;
                        var value = row["value"]?.GetValue<string>() ?? string.Empty;
                        if (!string.IsNullOrWhiteSpace(property))
                        {
                            mapped[property] = ResolvePaletteTokens(value, catalog._palette);
                        }
                    }
                }

                catalog._styleProperties[name] = mapped;
            }
        }

        return catalog;
    }

    public IReadOnlyDictionary<string, string> ResolveStyle(string? styleName)
    {
        if (string.IsNullOrWhiteSpace(styleName))
        {
            return new Dictionary<string, string>();
        }

        if (_styleProperties.TryGetValue(styleName, out var style))
        {
            return style;
        }

        return new Dictionary<string, string>();
    }

    private static string ResolvePaletteTokens(string value, IReadOnlyDictionary<string, string> palette)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return value;
        }

        const string prefix = "%Palette.";
        const string suffix = "%";

        if (value.StartsWith(prefix, StringComparison.OrdinalIgnoreCase) && value.EndsWith(suffix, StringComparison.OrdinalIgnoreCase))
        {
            var key = value.Substring(prefix.Length, value.Length - prefix.Length - suffix.Length);
            if (palette.TryGetValue(key, out var resolved))
            {
                return resolved;
            }
        }

        return value;
    }
}