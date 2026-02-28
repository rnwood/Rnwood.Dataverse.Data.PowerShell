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
{    private static TemplateResolver BuildTemplateResolver(Dictionary<string, byte[]> entries, string unpackDirectory)
    {
        var resolver = new TemplateResolver();
        PopulateTemplateResolverFromEntries(entries, resolver);

        ReadEmbeddedAllControlsEntries(out var embeddedEntries);
        PopulateTemplateResolverFromEntries(embeddedEntries, resolver);
        

        return resolver;
    }

    private static ControlMetadataResolver BuildControlMetadataResolver(Dictionary<string, byte[]> entries)
    {
        var resolver = new ControlMetadataResolver();
        PopulateMetadataResolverFromEntries(entries, resolver);

        ReadEmbeddedAllControlsEntries(out var embeddedEntries);
        PopulateMetadataResolverFromEntries(embeddedEntries, resolver);

        return resolver;
    }

    private static TemplatePropertyResolver BuildTemplatePropertyResolver(Dictionary<string, byte[]> entries)
    {
        var resolver = new TemplatePropertyResolver();
        PopulateTemplatePropertyResolverFromEntries(entries, resolver);

        ReadEmbeddedAllControlsEntries(out var embeddedEntries);
        PopulateTemplatePropertyResolverFromEntries(embeddedEntries, resolver);

        return resolver;
    }

    private static TemplateKeywordResolver BuildTemplateKeywordResolver(Dictionary<string, byte[]> entries)
    {
        var resolver = new TemplateKeywordResolver();
        PopulateTemplateKeywordResolverFromEntries(entries, resolver);

        ReadEmbeddedAllControlsEntries(out var embeddedEntries);
        PopulateTemplateKeywordResolverFromEntries(embeddedEntries, resolver);

        return resolver;
    }

    private static void PopulateTemplateKeywordResolverFromEntries(Dictionary<string, byte[]> entries, TemplateKeywordResolver resolver)
    {
        if (!entries.TryGetValue("References/Templates.json", out var templatesBytes))
        {
            return;
        }

        var templatesRoot = JsonNode.Parse(templatesBytes)?.AsObject();
        var usedTemplates = templatesRoot?["UsedTemplates"] as JsonArray;
        if (usedTemplates is null)
        {
            return;
        }

        foreach (var t in usedTemplates.OfType<JsonObject>())
        {
            var name = t["Name"]?.GetValue<string>() ?? string.Empty;
            var id = t["Id"]?.GetValue<string>() ?? string.Empty;
            var templateXml = t["Template"]?.GetValue<string>() ?? string.Empty;
            if (string.IsNullOrWhiteSpace(name))
            {
                continue;
            }

            if (string.IsNullOrWhiteSpace(id))
            {
                id = ParseTemplateId(templateXml);
            }

            resolver.RegisterTemplateKeywords(name, id, ParseRequiredYamlKeywords(templateXml));
        }

        var topParents = ParseTopParentFiles(entries, "Controls/")
            .Concat(ParseTopParentFiles(entries, "Components/"))
            .Select(kv => kv.Value["TopParent"] as JsonObject)
            .Where(v => v is not null)
            .Cast<JsonObject>()
            .ToList();

        foreach (var top in topParents)
        {
            foreach (var node in Flatten(top))
            {
                resolver.RegisterFromControlNode(node);
            }
        }
    }


    private static void PopulateTemplatePropertyResolverFromEntries(Dictionary<string, byte[]> entries, TemplatePropertyResolver resolver)
    {
        var allTopParentFiles = ParseTopParentFiles(entries, "Controls/")
            .Concat(ParseTopParentFiles(entries, "Components/"))
            .Select(kv => kv.Value["TopParent"] as JsonObject)
            .Where(v => v is not null)
            .Cast<JsonObject>()
            .ToList();

        foreach (var top in allTopParentFiles)
        {
            foreach (var node in Flatten(top))
            {
                resolver.RegisterFromControlNode(node);
            }
        }

        if (entries.TryGetValue("References/Templates.json", out var templatesBytes))
        {
            var templatesRoot = JsonNode.Parse(templatesBytes)?.AsObject();
            var usedTemplates = templatesRoot?["UsedTemplates"] as JsonArray;
            if (usedTemplates is not null)
            {
                foreach (var t in usedTemplates.OfType<JsonObject>())
                {
                    var name = t["Name"]?.GetValue<string>() ?? string.Empty;
                    var id = t["Id"]?.GetValue<string>() ?? string.Empty;
                    var xml = t["Template"]?.GetValue<string>() ?? string.Empty;

                    if (string.IsNullOrWhiteSpace(id))
                    {
                        id = ParseTemplateId(xml);
                    }

                    if (string.IsNullOrWhiteSpace(name) || string.IsNullOrWhiteSpace(id))
                    {
                        continue;
                    }

                    resolver.RegisterTemplateProperties(name, id, ParseTemplatePropertyNames(xml));
                }
            }

            var pcfTemplates = templatesRoot?["PcfTemplates"] as JsonArray;
            if (pcfTemplates is not null)
            {
                foreach (var pcf in pcfTemplates.OfType<JsonObject>())
                {
                    var name = pcf["Name"]?.GetValue<string>() ?? string.Empty;
                    if (string.IsNullOrWhiteSpace(name))
                    {
                        continue;
                    }

                    resolver.RegisterTemplateProperties(name, string.Empty, ParsePcfTemplatePropertyNames(pcf));
                }
            }
        }
    }

    private static HashSet<string> ParsePcfTemplatePropertyNames(JsonObject pcfTemplate)
    {
        var names = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        if (pcfTemplate["PcfConversions"] is not JsonArray conversions)
        {
            return names;
        }

        foreach (var conversion in conversions.OfType<JsonObject>())
        {
            if (conversion["Action"] is not JsonArray actions)
            {
                continue;
            }

            foreach (var action in actions.OfType<JsonObject>())
            {
                var name = action["Name"]?.GetValue<string>() ?? string.Empty;
                if (!string.IsNullOrWhiteSpace(name))
                {
                    names.Add(name);
                }
            }
        }

        return names;
    }


    private static void PopulateMetadataResolverFromEntries(Dictionary<string, byte[]> entries, ControlMetadataResolver resolver)
    {
        var allTopParentFiles = ParseTopParentFiles(entries, "Controls/")
            .Concat(ParseTopParentFiles(entries, "Components/"))
            .Select(kv => kv.Value["TopParent"] as JsonObject)
            .Where(v => v is not null)
            .Cast<JsonObject>()
            .ToList();

        foreach (var top in allTopParentFiles)
        {
            foreach (var node in Flatten(top))
            {
                resolver.RegisterFromControlNode(node);
            }
        }

        if (entries.TryGetValue("References/Templates.json", out var templatesBytes)
            && JsonNode.Parse(templatesBytes) is JsonObject templatesRoot
            && templatesRoot["UsedTemplates"] is JsonArray usedTemplates)
        {
            foreach (var t in usedTemplates.OfType<JsonObject>())
            {
                var name = t["Name"]?.GetValue<string>() ?? string.Empty;
                var id = t["Id"]?.GetValue<string>() ?? string.Empty;
                var templateXml = t["Template"]?.GetValue<string>() ?? string.Empty;
                if (string.IsNullOrWhiteSpace(id))
                {
                    id = ParseTemplateId(templateXml);
                }

                resolver.RegisterTemplateDefaultsFromXml(name, id, templateXml);
                resolver.RegisterTemplatePropertyDatatypesFromXml(name, id, templateXml);
            }
        }

        if (entries.TryGetValue("References/Themes.json", out var themesBytes)
            && JsonNode.Parse(themesBytes) is JsonObject themesRoot)
        {
            resolver.RegisterThemeStyleDefaultsFromThemesJson(themesRoot);
        }
    }

    private static void PopulateTemplateResolverFromEntries(Dictionary<string, byte[]> entries, TemplateResolver resolver)
    {
        var allTopParentFiles = ParseTopParentFiles(entries, "Controls/")
            .Concat(ParseTopParentFiles(entries, "Components/"))
            .Select(kv => kv.Value["TopParent"] as JsonObject)
            .Where(v => v is not null)
            .Cast<JsonObject>()
            .ToList();

        var indexByName = new Dictionary<string, JsonObject>(StringComparer.OrdinalIgnoreCase);
        foreach (var top in allTopParentFiles)
        {
            IndexByName(top, indexByName);

            foreach (var node in Flatten(top))
            {
                resolver.RegisterTemplateFromControlNode(node);
            }
        }

        if (entries.TryGetValue("References/Templates.json", out var templatesBytes))
        {
            var templatesRoot = JsonNode.Parse(templatesBytes)?.AsObject();
            var usedTemplates = templatesRoot?["UsedTemplates"] as JsonArray;
            if (usedTemplates is not null)
            {
                foreach (var t in usedTemplates.OfType<JsonObject>())
                {
                    var name = t["Name"]?.GetValue<string>() ?? string.Empty;
                    var id = t["Id"]?.GetValue<string>() ?? string.Empty;
                    var version = t["Version"]?.GetValue<string>() ?? string.Empty;
                    resolver.RegisterTemplate(name, id, version);
                }
            }
        }

        var deserializer = new DeserializerBuilder().IgnoreUnmatchedProperties().Build();
        foreach (var (path, bytes) in entries
                     .Where(e => e.Key.StartsWith("Src/", StringComparison.OrdinalIgnoreCase)
                                 && e.Key.EndsWith(".pa.yaml", StringComparison.OrdinalIgnoreCase)))
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

            if (TryGetMap(root, "Screens", out var screens))
            {
                foreach (var kv in screens)
                {
                    var rootName = kv.Key?.ToString() ?? string.Empty;
                    if (string.IsNullOrWhiteSpace(rootName) || kv.Value is not Dictionary<object, object?> payload)
                    {
                        continue;
                    }

                    RegisterControlMappingsFromYamlPayload(payload, rootName, indexByName, resolver);
                }
            }

            if (TryGetMap(root, "ComponentDefinitions", out var components))
            {
                foreach (var kv in components)
                {
                    var rootName = kv.Key?.ToString() ?? string.Empty;
                    if (string.IsNullOrWhiteSpace(rootName) || kv.Value is not Dictionary<object, object?> payload)
                    {
                        continue;
                    }

                    RegisterControlMappingsFromYamlPayload(payload, rootName, indexByName, resolver);
                }
            }
        }
    }

    private static void RegisterControlMappingsFromYamlPayload(
        Dictionary<object, object?> payload,
        string controlName,
        IReadOnlyDictionary<string, JsonObject> indexByName,
        TemplateResolver resolver)
    {
        if (payload.TryGetValue("Control", out var controlTypeObj)
            && controlTypeObj is not null
            && indexByName.TryGetValue(controlName, out var controlNode))
        {
            resolver.RegisterControlType(controlTypeObj.ToString() ?? string.Empty, controlNode);
        }

        if (!payload.TryGetValue("Children", out var childrenObj) || childrenObj is not List<object?> children)
        {
            return;
        }

        foreach (var child in children)
        {
            if (child is not Dictionary<object, object?> singleton || singleton.Count != 1)
            {
                continue;
            }

            var kv = singleton.First();
            var childName = kv.Key?.ToString() ?? string.Empty;
            if (string.IsNullOrWhiteSpace(childName) || kv.Value is not Dictionary<object, object?> childPayload)
            {
                continue;
            }

            RegisterControlMappingsFromYamlPayload(childPayload, childName, indexByName, resolver);
        }
    }


    private readonly record struct ResolvedTemplate(string TemplateId, string TemplateName, string Version, JsonObject? TemplatePrototype);

    private readonly record struct UsedTemplate(string Name, string Version, string Id, string TemplateXml);

    private readonly record struct FunctionCall(string FunctionName, List<string> Arguments);

    private sealed class TemplateResolver
    {
        private readonly Dictionary<string, ResolvedTemplate> _byAlias = new(StringComparer.OrdinalIgnoreCase);

        public void RegisterControlType(string controlType, JsonObject controlNode)
        {
            var template = controlNode["Template"] as JsonObject;
            if (template is null)
            {
                return;
            }

            RegisterAlias(
                NormalizeControlType(controlType),
                template["Id"]?.GetValue<string>(),
                template["Name"]?.GetValue<string>(),
                template["Version"]?.GetValue<string>(),
                template);
        }

        public void RegisterTemplateFromControlNode(JsonObject controlNode)
        {
            var template = controlNode["Template"] as JsonObject;
            if (template is null)
            {
                return;
            }

            var templateId = template["Id"]?.GetValue<string>() ?? string.Empty;
            var templateName = template["Name"]?.GetValue<string>() ?? string.Empty;
            var version = template["Version"]?.GetValue<string>() ?? string.Empty;

            RegisterTemplate(templateName, templateId, version, template);
            RegisterAlias(NormalizeAliasFromId(templateId), templateId, templateName, version, template);
        }

        public void RegisterTemplate(string name, string id, string version, JsonObject? templatePrototype = null)
        {
            RegisterAlias(NormalizeAlias(name), id, name, version, templatePrototype);
            RegisterAlias(NormalizeAliasFromId(id), id, name, version, templatePrototype);
        }

        public bool TryResolve(string controlType, out ResolvedTemplate template)
        {
            template = default;
            var normalizedType = NormalizeControlType(controlType);
            if (string.IsNullOrWhiteSpace(normalizedType))
            {
                return false;
            }

            if (!_byAlias.TryGetValue(normalizedType, out var found))
            {
                return false;
            }

            var requestedVersion = ParseRequestedVersion(controlType);
            template = string.IsNullOrWhiteSpace(requestedVersion)
                ? found
                : found with { Version = requestedVersion };

            return true;
        }

        private void RegisterAlias(string alias, string? id, string? name, string? version, JsonObject? templatePrototype = null)
        {
            if (string.IsNullOrWhiteSpace(alias) || string.IsNullOrWhiteSpace(id) || string.IsNullOrWhiteSpace(name))
            {
                return;
            }

            if (!_byAlias.ContainsKey(alias))
            {
                _byAlias[alias] = new ResolvedTemplate(
                    id,
                    name,
                    string.IsNullOrWhiteSpace(version) ? "1.0.0" : version,
                    templatePrototype is null ? null : (JsonObject)templatePrototype.DeepClone());
            }
            else if (templatePrototype is not null && _byAlias[alias].TemplatePrototype is null)
            {
                var existing = _byAlias[alias];
                _byAlias[alias] = existing with { TemplatePrototype = (JsonObject)templatePrototype.DeepClone() };
            }
        }

        private static string NormalizeControlType(string controlType)
        {
            var raw = controlType?.Trim() ?? string.Empty;
            if (string.IsNullOrWhiteSpace(raw))
            {
                return string.Empty;
            }

            var at = raw.Split(new[] { '@' }, 2).Select(p => p.Trim()).ToArray();
            var typePart = at[0];
            var slash = typePart.Split(new[] { '/' }, 2).Select(p => p.Trim()).ToArray();
            var canonical = slash.Length == 2 ? slash[1] : slash[0];
            if (string.Equals(canonical, "TextInput", StringComparison.OrdinalIgnoreCase))
            {
                canonical = "PowerApps_CoreControls_TextInputCanvas";
            }

            return NormalizeAlias(canonical);
        }

        private static string ParseRequestedVersion(string controlType)
        {
            var raw = controlType?.Trim() ?? string.Empty;
            if (string.IsNullOrWhiteSpace(raw))
            {
                return string.Empty;
            }

            var at = raw.Split(new[] { '@' }, 2).Select(p => p.Trim()).ToArray();
            return at.Length > 1 ? at[1] : string.Empty;
        }

        private static string NormalizeAlias(string value)
        {
            return (value ?? string.Empty).Trim().ToLowerInvariant();
        }

        private static string NormalizeAliasFromId(string id)
        {
            if (string.IsNullOrWhiteSpace(id))
            {
                return string.Empty;
            }

            var trimmed = id.TrimEnd('/');
            var idx = trimmed.LastIndexOf('/');
            var tail = idx >= 0 ? trimmed.Substring(idx + 1) : trimmed;
            return NormalizeAlias(tail);
        }
    }

    private sealed class TemplatePropertyResolver
    {
        private readonly Dictionary<string, HashSet<string>> _allowedByTemplateKey = new(StringComparer.OrdinalIgnoreCase);

        public void RegisterTemplateProperties(string templateName, string templateId, IEnumerable<string> properties)
        {
            var names = properties.Where(p => !string.IsNullOrWhiteSpace(p)).ToList();
            if (names.Count == 0)
            {
                return;
            }

            foreach (var key in GetTemplateKeys(templateName, templateId))
            {
                if (!_allowedByTemplateKey.TryGetValue(key, out var set))
                {
                    set = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
                    _allowedByTemplateKey[key] = set;
                }

                foreach (var p in names)
                {
                    set.Add(p);
                }
            }
        }

        public void RegisterFromControlNode(JsonObject node)
        {
            var templateName = node["Template"]?["Name"]?.GetValue<string>() ?? string.Empty;
            var templateId = node["Template"]?["Id"]?.GetValue<string>() ?? string.Empty;
            var ruleProps = ((node["Rules"] as JsonArray) ?? new JsonArray())
                .OfType<JsonObject>()
                .Select(r => r["Property"]?.GetValue<string>() ?? string.Empty)
                .Where(p => !string.IsNullOrWhiteSpace(p));

            RegisterTemplateProperties(templateName, templateId, ruleProps);

            // DynamicProperties are parent-context properties injected by container templates
            // (e.g. FillPortions, AlignInContainer from auto-layout GroupContainers).
            // Register them so validation accepts them for this template type.
            var dynProps = ((node["DynamicProperties"] as JsonArray) ?? new JsonArray())
                .OfType<JsonObject>()
                .Select(d => d["PropertyName"]?.GetValue<string>() ?? string.Empty)
                .Where(p => !string.IsNullOrWhiteSpace(p));

            RegisterTemplateProperties(templateName, templateId, dynProps);
        }

        public bool TryGetAllowedProperties(JsonObject node, out HashSet<string> properties)
        {
            properties = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            var templateName = node["Template"]?["Name"]?.GetValue<string>() ?? string.Empty;
            var templateId = node["Template"]?["Id"]?.GetValue<string>() ?? string.Empty;

            var found = false;
            foreach (var key in GetTemplateKeys(templateName, templateId))
            {
                if (_allowedByTemplateKey.TryGetValue(key, out var set))
                {
                    properties.UnionWith(set);
                    found = true;
                }
            }

            return found;
        }

        private static IEnumerable<string> GetTemplateKeys(string templateName, string templateId)
        {
            if (!string.IsNullOrWhiteSpace(templateName))
            {
                yield return templateName.Trim().ToLowerInvariant();
            }

            if (string.IsNullOrWhiteSpace(templateId))
            {
                yield break;
            }

            var normalizedId = templateId.Trim().ToLowerInvariant();
            yield return normalizedId;

            var trimmed = normalizedId.TrimEnd('/');
            var idx = trimmed.LastIndexOf('/');
            if (idx >= 0 && idx < trimmed.Length - 1)
            {
                yield return trimmed.Substring(idx + 1);
            }
        }
    }

    private sealed class TemplateKeywordResolver
    {
        private readonly Dictionary<string, HashSet<string>> _requiredByTemplateKey = new(StringComparer.OrdinalIgnoreCase);

        public void RegisterTemplateKeywords(string templateName, string templateId, IEnumerable<string> requiredKeywords)
        {
            var keywords = requiredKeywords.Where(k => !string.IsNullOrWhiteSpace(k)).ToList();
            if (keywords.Count == 0)
            {
                return;
            }

            foreach (var key in GetTemplateKeys(templateName, templateId))
            {
                if (!_requiredByTemplateKey.TryGetValue(key, out var set))
                {
                    set = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
                    _requiredByTemplateKey[key] = set;
                }

                foreach (var keyword in keywords)
                {
                    set.Add(keyword);
                }
            }
        }

        public void RegisterFromControlNode(JsonObject node)
        {
            var templateName = node["Template"]?["Name"]?.GetValue<string>() ?? string.Empty;
            var templateId = node["Template"]?["Id"]?.GetValue<string>() ?? string.Empty;
            var variantName = node["VariantName"]?.GetValue<string>() ?? string.Empty;

            if (!string.IsNullOrWhiteSpace(variantName))
            {
                RegisterTemplateKeywords(templateName, templateId, ["Variant"]);
            }
        }

        public bool TryGetRequiredKeywords(JsonObject node, out HashSet<string> requiredKeywords)
        {
            requiredKeywords = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            var templateName = node["Template"]?["Name"]?.GetValue<string>() ?? string.Empty;
            var templateId = node["Template"]?["Id"]?.GetValue<string>() ?? string.Empty;

            var found = false;
            foreach (var key in GetTemplateKeys(templateName, templateId))
            {
                if (_requiredByTemplateKey.TryGetValue(key, out var set))
                {
                    requiredKeywords.UnionWith(set);
                    found = true;
                }
            }

            return found;
        }

        private static IEnumerable<string> GetTemplateKeys(string templateName, string templateId)
        {
            if (!string.IsNullOrWhiteSpace(templateName))
            {
                yield return templateName.Trim().ToLowerInvariant();
            }

            if (string.IsNullOrWhiteSpace(templateId))
            {
                yield break;
            }

            var normalizedId = templateId.Trim().ToLowerInvariant();
            yield return normalizedId;

            var trimmed = normalizedId.TrimEnd('/');
            var idx = trimmed.LastIndexOf('/');
            if (idx >= 0 && idx < trimmed.Length - 1)
            {
                yield return trimmed.Substring(idx + 1);
            }
        }
    }

    private sealed class ControlMetadataResolver
    {
        private readonly Dictionary<string, string> _categoryByTemplateAndProperty = new(StringComparer.OrdinalIgnoreCase);
        private readonly Dictionary<string, string> _datatypeByTemplateAndProperty = new(StringComparer.OrdinalIgnoreCase);
        private readonly Dictionary<string, Dictionary<string, int>> _styleHistogram = new(StringComparer.OrdinalIgnoreCase);
        private readonly Dictionary<string, string> _styleTemplateByName = new(StringComparer.OrdinalIgnoreCase);
        private readonly Dictionary<string, Dictionary<string, string>> _styleDefaultRulesByName = new(StringComparer.OrdinalIgnoreCase);
        private readonly Dictionary<string, JsonArray> _rulesPrototypeByTemplate = new(StringComparer.OrdinalIgnoreCase);
        private readonly Dictionary<string, JsonArray> _statePrototypeByTemplate = new(StringComparer.OrdinalIgnoreCase);
        private readonly Dictionary<string, bool> _hasDynamicPropertiesByTemplate = new(StringComparer.OrdinalIgnoreCase);
        private readonly HashSet<string> _rulesPrototypeSeededFromTemplate = new(StringComparer.OrdinalIgnoreCase);
        private readonly HashSet<string> _statePrototypeSeededFromTemplate = new(StringComparer.OrdinalIgnoreCase);

        public void RegisterThemeStyleDefaultsFromThemesJson(JsonObject themesRoot)
        {
            var current = themesRoot["CurrentTheme"]?.GetValue<string>() ?? string.Empty;
            var customThemes = themesRoot["CustomThemes"] as JsonArray;
            if (customThemes is null || customThemes.Count == 0)
            {
                return;
            }

            var selectedTheme = customThemes
                .OfType<JsonObject>()
                .FirstOrDefault(t => string.Equals(t["name"]?.GetValue<string>(), current, StringComparison.OrdinalIgnoreCase))
                ?? customThemes.OfType<JsonObject>().FirstOrDefault();

            if (selectedTheme is null)
            {
                return;
            }

            var palette = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
            if (selectedTheme["palette"] is JsonArray paletteEntries)
            {
                foreach (var item in paletteEntries.OfType<JsonObject>())
                {
                    var name = item["name"]?.GetValue<string>() ?? string.Empty;
                    var value = item["value"]?.GetValue<string>() ?? string.Empty;
                    if (!string.IsNullOrWhiteSpace(name))
                    {
                        palette[name] = value;
                    }
                }
            }

            if (selectedTheme["styles"] is not JsonArray styleEntries)
            {
                return;
            }

            foreach (var style in styleEntries.OfType<JsonObject>())
            {
                var styleName = style["name"]?.GetValue<string>() ?? string.Empty;
                if (string.IsNullOrWhiteSpace(styleName))
                {
                    continue;
                }

                var templateName = style["controlTemplateName"]?.GetValue<string>() ?? string.Empty;
                if (!string.IsNullOrWhiteSpace(templateName))
                {
                    _styleTemplateByName[styleName] = templateName;
                }

                if (style["propertyValuesMap"] is not JsonArray propertyValues)
                {
                    continue;
                }

                var mapped = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
                foreach (var row in propertyValues.OfType<JsonObject>())
                {
                    var property = row["property"]?.GetValue<string>() ?? string.Empty;
                    var value = row["value"]?.GetValue<string>() ?? string.Empty;
                    if (string.IsNullOrWhiteSpace(property) || string.IsNullOrWhiteSpace(value))
                    {
                        continue;
                    }

                    mapped[property] = ResolvePaletteTokens(value, palette);
                }

                if (mapped.Count > 0)
                {
                    _styleDefaultRulesByName[styleName] = mapped;
                }
            }
        }

        public void RegisterTemplatePropertyDatatypesFromXml(string templateName, string templateId, string templateXml)
        {
            if (string.IsNullOrWhiteSpace(templateXml))
            {
                return;
            }

            try
            {
                var root = XDocument.Parse(templateXml).Root;
                if (root is null)
                {
                    return;
                }

                foreach (var prop in root.Descendants().Where(e => e.Name.LocalName == "property"))
                {
                    var name = prop.Attribute("name")?.Value ?? string.Empty;
                    var datatype = prop.Attribute("datatype")?.Value ?? string.Empty;
                    if (string.IsNullOrWhiteSpace(name) || string.IsNullOrWhiteSpace(datatype))
                    {
                        continue;
                    }

                    foreach (var key in GetTemplateKeys(templateName, templateId))
                    {
                        var composite = $"{key}|{name}";
                        if (!_datatypeByTemplateAndProperty.ContainsKey(composite))
                        {
                            _datatypeByTemplateAndProperty[composite] = datatype;
                        }
                    }
                }
            }
            catch
            {
                // Ignore malformed template xml.
            }
        }

        public void RegisterTemplateDefaultsFromXml(string templateName, string templateId, string templateXml)
        {
            if (string.IsNullOrWhiteSpace(templateXml))
            {
                return;
            }

            try
            {
                var root = XDocument.Parse(templateXml).Root;
                if (root is null)
                {
                    return;
                }

                var rules = new JsonArray();
                var states = new JsonArray();
                var seen = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

                foreach (var includeProperty in root.Descendants().Where(e => e.Name.LocalName == "includeProperty"))
                {
                    var propertyName = includeProperty.Attribute("name")?.Value ?? string.Empty;
                    if (string.IsNullOrWhiteSpace(propertyName) || !seen.Add(propertyName))
                    {
                        continue;
                    }

                    var defaultValue = includeProperty.Attribute("defaultValue")?.Value ?? string.Empty;
                    if (!string.IsNullOrWhiteSpace(defaultValue))
                    {
                        rules.Add(new JsonObject
                        {
                            ["Property"] = propertyName,
                            ["Category"] = InferCategory(propertyName),
                            ["InvariantScript"] = NormalizeReservedEnumToken(defaultValue),
                            ["RuleProviderType"] = "Unknown",
                        });
                    }

                    states.Add(propertyName);
                }

                if (rules.Count == 0 && states.Count == 0)
                {
                    return;
                }

                foreach (var key in GetTemplateKeys(templateName, templateId))
                {
                    if (rules.Count > 0)
                    {
                        _rulesPrototypeByTemplate[key] = (JsonArray)rules.DeepClone();
                        _rulesPrototypeSeededFromTemplate.Add(key);
                    }

                    if (states.Count > 0)
                    {
                        _statePrototypeByTemplate[key] = (JsonArray)states.DeepClone();
                        _statePrototypeSeededFromTemplate.Add(key);
                    }
                }
            }
            catch
            {
                // Ignore malformed template xml.
            }
        }

        public void NormalizeReservedEnumTokensInTopFiles(IReadOnlyDictionary<string, JsonObject> topFiles)
        {
            foreach (var topFile in topFiles.Values)
            {
                if (topFile["TopParent"] is not JsonObject topParent)
                {
                    continue;
                }

                foreach (var node in Flatten(topParent))
                {
                    NormalizeReservedEnumTokensInRules(node);
                }
            }
        }

        public void NormalizeReservedEnumTokensInRules(JsonObject node)
        {
            var rules = node["Rules"] as JsonArray;
            if (rules is null || rules.Count == 0)
            {
                return;
            }

            var templateName = node["Template"]?["Name"]?.GetValue<string>() ?? string.Empty;
            var templateId = node["Template"]?["Id"]?.GetValue<string>() ?? string.Empty;

            foreach (var rule in rules.OfType<JsonObject>())
            {
                var property = rule["Property"]?.GetValue<string>() ?? string.Empty;
                if (string.IsNullOrWhiteSpace(property))
                {
                    continue;
                }

                var datatype = ResolveDatatype(templateName, templateId, property);
                var expectedEnumType = string.IsNullOrWhiteSpace(datatype) ? property : datatype;

                var invariantScript = rule["InvariantScript"]?.GetValue<string>() ?? string.Empty;
                if (string.IsNullOrWhiteSpace(invariantScript))
                {
                    continue;
                }

                var normalized = NormalizeReservedEnumTokenForDatatype(invariantScript, expectedEnumType);
                if (string.Equals(normalized, invariantScript, StringComparison.Ordinal))
                {
                    normalized = NormalizeReservedEnumToken(invariantScript);
                }

                if (!string.Equals(invariantScript, normalized, StringComparison.Ordinal))
                {
                    rule["InvariantScript"] = normalized;
                }
            }
        }

        private string? ResolveDatatype(string templateName, string templateId, string property)
        {
            foreach (var key in GetTemplateKeys(templateName, templateId))
            {
                var composite = $"{key}|{property}";
                if (_datatypeByTemplateAndProperty.TryGetValue(composite, out var datatype)
                    && !string.IsNullOrWhiteSpace(datatype))
                {
                    return datatype;
                }
            }

            return null;
        }

        public void RegisterFromControlNode(JsonObject node)
        {
            var templateName = node["Template"]?["Name"]?.GetValue<string>() ?? string.Empty;
            var templateId = node["Template"]?["Id"]?.GetValue<string>() ?? string.Empty;
            var keys = GetTemplateKeys(templateName, templateId);
            if (keys.Count == 0)
            {
                return;
            }

            var styleName = node["StyleName"]?.GetValue<string>() ?? string.Empty;
            if (!string.IsNullOrWhiteSpace(styleName))
            {
                foreach (var key in keys)
                {
                    if (!_styleHistogram.TryGetValue(key, out var map))
                    {
                        map = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);
                        _styleHistogram[key] = map;
                    }

                    map[styleName] = map.TryGetValue(styleName, out var count) ? count + 1 : 1;
                }
            }

            var rules = (node["Rules"] as JsonArray) ?? new JsonArray();
            var states = (node["ControlPropertyState"] as JsonArray) ?? new JsonArray();

            var nonContextualRules = rules
                .OfType<JsonObject>()
                .Where(r => !IsContextualInvariantScript(r["InvariantScript"]?.GetValue<string>() ?? string.Empty))
                .Select(r => (JsonObject)r.DeepClone())
                .ToList();

            var candidateRules = new JsonArray(nonContextualRules.Select(r => (JsonNode)r).ToArray());
            var candidateRuleProperties = nonContextualRules
                .Select(r => r["Property"]?.GetValue<string>() ?? string.Empty)
                .Where(p => !string.IsNullOrWhiteSpace(p))
                .ToHashSet(StringComparer.OrdinalIgnoreCase);

            var candidateStates = new JsonArray();
            foreach (var stateEntry in states)
            {
                var stateProperty = GetStatePropertyName(stateEntry);
                if (string.IsNullOrWhiteSpace(stateProperty) || !candidateRuleProperties.Contains(stateProperty))
                {
                    continue;
                }

                candidateStates.Add(stateEntry?.DeepClone());
            }

            foreach (var key in keys)
            {
                if (candidateRules.Count > 0)
                {
                    if (_rulesPrototypeSeededFromTemplate.Contains(key)
                        && _rulesPrototypeByTemplate.TryGetValue(key, out var seededRules)
                        && seededRules.Count > 0)
                    {
                        MergeRulesWithSourcePriority(seededRules, candidateRules);
                    }
                    else if (!_rulesPrototypeByTemplate.TryGetValue(key, out var existingRules)
                             || existingRules.Count < candidateRules.Count)
                    {
                        _rulesPrototypeByTemplate[key] = (JsonArray)candidateRules.DeepClone();
                    }
                }

                if (candidateStates.Count > 0)
                {
                    if (_statePrototypeSeededFromTemplate.Contains(key)
                        && _statePrototypeByTemplate.TryGetValue(key, out var seededStates)
                        && seededStates.Count > 0)
                    {
                        MergeStatesWithSourcePriority(seededStates, candidateStates);
                    }
                    else if (!_statePrototypeByTemplate.TryGetValue(key, out var existingStates)
                             || existingStates.Count < candidateStates.Count)
                    {
                        _statePrototypeByTemplate[key] = (JsonArray)candidateStates.DeepClone();
                    }
                }

                if (!_hasDynamicPropertiesByTemplate.ContainsKey(key) && node["HasDynamicProperties"] is JsonValue hv && hv.TryGetValue<bool>(out var hasDynamic))
                {
                    _hasDynamicPropertiesByTemplate[key] = hasDynamic;
                }
            }

            foreach (var rule in rules.OfType<JsonObject>())
            {
                var property = rule["Property"]?.GetValue<string>() ?? string.Empty;
                var category = rule["Category"]?.GetValue<string>() ?? string.Empty;
                if (string.IsNullOrWhiteSpace(property) || string.IsNullOrWhiteSpace(category))
                {
                    continue;
                }

                foreach (var key in keys)
                {
                    var composite = $"{key}|{property}";
                    if (!_categoryByTemplateAndProperty.ContainsKey(composite))
                    {
                        _categoryByTemplateAndProperty[composite] = category;
                    }
                }
            }
        }

        public string? ResolveCategory(JsonObject node, string property)
        {
            var templateName = node["Template"]?["Name"]?.GetValue<string>() ?? string.Empty;
            var templateId = node["Template"]?["Id"]?.GetValue<string>() ?? string.Empty;
            foreach (var key in GetTemplateKeys(templateName, templateId))
            {
                var composite = $"{key}|{property}";
                if (_categoryByTemplateAndProperty.TryGetValue(composite, out var category)
                    && !string.IsNullOrWhiteSpace(category))
                {
                    return category;
                }
            }

            return null;
        }

        public string? ResolveStyle(JsonObject node)
        {
            var templateName = node["Template"]?["Name"]?.GetValue<string>() ?? string.Empty;
            var templateId = node["Template"]?["Id"]?.GetValue<string>() ?? string.Empty;

            foreach (var key in GetTemplateKeys(templateName, templateId))
            {
                if (!_styleHistogram.TryGetValue(key, out var map) || map.Count == 0)
                {
                    continue;
                }

                var defaultStyle = map
                    .Where(kv => kv.Key.StartsWith("default", StringComparison.OrdinalIgnoreCase))
                    .OrderByDescending(kv => kv.Value)
                    .ThenBy(kv => kv.Key, StringComparer.OrdinalIgnoreCase)
                    .FirstOrDefault();

                if (!string.IsNullOrWhiteSpace(defaultStyle.Key))
                {
                    return defaultStyle.Key;
                }

                var best = map.OrderByDescending(kv => kv.Value).ThenBy(kv => kv.Key, StringComparer.OrdinalIgnoreCase).First();
                if (!string.IsNullOrWhiteSpace(best.Key))
                {
                    return best.Key;
                }
            }

            return null;
        }

        public void ApplyTemplateDefaults(JsonObject node, string templateName, string templateId)
        {
            var keys = GetTemplateKeys(templateName, templateId);
            if (keys.Count == 0)
            {
                return;
            }

            var rules = node["Rules"] as JsonArray;
            if (rules is null || rules.Count == 0)
            {
                foreach (var key in keys)
                {
                    if (_rulesPrototypeByTemplate.TryGetValue(key, out var prototypeRules) && prototypeRules.Count > 0)
                    {
                        node["Rules"] = prototypeRules.DeepClone();
                        break;
                    }
                }
            }

            var state = node["ControlPropertyState"] as JsonArray;
            if (state is null || state.Count == 0)
            {
                foreach (var key in keys)
                {
                    if (_statePrototypeByTemplate.TryGetValue(key, out var prototypeState) && prototypeState.Count > 0)
                    {
                        node["ControlPropertyState"] = prototypeState.DeepClone();
                        break;
                    }
                }
            }

            if (node["HasDynamicProperties"] is null)
            {
                foreach (var key in keys)
                {
                    if (_hasDynamicPropertiesByTemplate.TryGetValue(key, out var hasDynamic))
                    {
                        node["HasDynamicProperties"] = hasDynamic;
                        break;
                    }
                }
            }

            // Apply the default style name derived from other same-template controls in the source
            // (e.g. "defaultGroupContainerStyle" for groupContainer). Only fill in if currently empty,
            // and never override a style that has already been explicitly set.
            // Screens always have an empty StyleName.
            var currentStyleName = node["StyleName"]?.GetValue<string>() ?? string.Empty;
            if (string.IsNullOrWhiteSpace(currentStyleName)
                && !string.Equals(templateName, "screen", StringComparison.OrdinalIgnoreCase))
            {
                var resolvedStyle = ResolveStyle(node);
                if (!string.IsNullOrWhiteSpace(resolvedStyle))
                {
                    node["StyleName"] = resolvedStyle;
                }
            }

            ApplyStyleDefaultRules(node, templateName);

            NormalizeReservedEnumTokensInRules(node);
            EnsureGalleryLayoutRuleFromVariant(node);
            UpdateControlPropertyStateFromRules(node);

            EnsureAutoLayoutChildBaseline(node, templateName);
        }

        private static void EnsureGalleryLayoutRuleFromVariant(JsonObject node)
        {
            var templateName = node["Template"]?["Name"]?.GetValue<string>() ?? string.Empty;
            if (!string.Equals(templateName, "gallery", StringComparison.OrdinalIgnoreCase))
            {
                return;
            }

            var variantName = node["VariantName"]?.GetValue<string>() ?? string.Empty;
            if (string.IsNullOrWhiteSpace(variantName))
            {
                return;
            }

            string? desiredLayout = null;
            if (variantName.IndexOf("vertical", StringComparison.OrdinalIgnoreCase) >= 0)
            {
                desiredLayout = "Layout.Vertical";
            }
            else if (variantName.IndexOf("horizontal", StringComparison.OrdinalIgnoreCase) >= 0)
            {
                desiredLayout = "Layout.Horizontal";
            }

            if (string.IsNullOrWhiteSpace(desiredLayout))
            {
                return;
            }

            var rules = (node["Rules"] as JsonArray) ?? new JsonArray();
            var layoutRule = rules
                .OfType<JsonObject>()
                .FirstOrDefault(r => string.Equals(r["Property"]?.GetValue<string>() ?? string.Empty, "Layout", StringComparison.OrdinalIgnoreCase));

            if (layoutRule is null)
            {
                rules.Add(new JsonObject
                {
                    ["Property"] = "Layout",
                    ["Category"] = "Design",
                    ["InvariantScript"] = desiredLayout,
                    ["RuleProviderType"] = "Unknown",
                });
            }
            else if (string.IsNullOrWhiteSpace(layoutRule["InvariantScript"]?.GetValue<string>() ?? string.Empty))
            {
                layoutRule["InvariantScript"] = desiredLayout;
                layoutRule["Category"] = layoutRule["Category"]?.GetValue<string>() ?? "Design";
                layoutRule["RuleProviderType"] = layoutRule["RuleProviderType"]?.GetValue<string>() ?? "Unknown";
            }

            node["Rules"] = rules;
        }

        // Every control can be placed inside an auto-layout GroupContainer, which injects these
        // parent-context dynamic properties into its children at runtime. Pre-populating them here
        // means the property resolver (and pack-time validation) can accept them for any template
        // without any per-template hardcoding in the validation layer.
        private static void EnsureAutoLayoutChildBaseline(JsonObject node, string templateName)
        {
            // Screens are never children of containers.
            if (string.Equals(templateName, "screen", StringComparison.OrdinalIgnoreCase))
            {
                return;
            }

            if (node["DynamicProperties"] is not JsonArray dynamicProperties)
            {
                dynamicProperties = new JsonArray();
                node["DynamicProperties"] = dynamicProperties;
            }

            void EnsureDynamicProperty(string propertyName, string category, string invariantScript)
            {
                var existing = dynamicProperties
                    .OfType<JsonObject>()
                    .FirstOrDefault(d => string.Equals(
                        d["PropertyName"]?.GetValue<string>() ?? string.Empty,
                        propertyName,
                        StringComparison.OrdinalIgnoreCase));

                if (existing is not null)
                {
                    return;
                }

                dynamicProperties.Add(new JsonObject
                {
                    ["PropertyName"] = propertyName,
                    ["Rule"] = new JsonObject
                    {
                        ["Property"] = propertyName,
                        ["Category"] = category,
                        ["InvariantScript"] = invariantScript,
                        ["RuleProviderType"] = "Unknown",
                    },
                    ["ControlPropertyState"] = propertyName,
                });
            }

            EnsureDynamicProperty("FillPortions", "Design", "0");
            EnsureDynamicProperty("AlignInContainer", "Design", "AlignInContainer.SetByContainer");
            EnsureDynamicProperty("LayoutMinWidth", "Design", "150");
            EnsureDynamicProperty("LayoutMinHeight", "Design", "100");
            EnsureDynamicProperty("LayoutMaxWidth", "Design", "0");
            EnsureDynamicProperty("LayoutMaxHeight", "Design", "0");
        }

        private static List<string> GetTemplateKeys(string templateName, string templateId)
        {
            var keys = new List<string>();
            if (!string.IsNullOrWhiteSpace(templateName))
            {
                keys.Add(templateName.Trim().ToLowerInvariant());
            }

            if (!string.IsNullOrWhiteSpace(templateId))
            {
                keys.Add(templateId.Trim().ToLowerInvariant());
                var trimmed = templateId.Trim().TrimEnd('/');
                var idx = trimmed.LastIndexOf('/');
                if (idx >= 0 && idx < trimmed.Length - 1)
                {
                    keys.Add(trimmed.Substring(idx + 1).ToLowerInvariant());
                }
            }

            return keys.Distinct(StringComparer.OrdinalIgnoreCase).ToList();
        }

        private static void MergeRulesWithSourcePriority(JsonArray targetRules, JsonArray sourceRules)
        {
            var indexByProperty = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);
            var i = 0;
            foreach (var existing in targetRules.OfType<JsonObject>())
            {
                var property = existing["Property"]?.GetValue<string>() ?? string.Empty;
                if (!string.IsNullOrWhiteSpace(property))
                {
                    indexByProperty[property] = i;
                }

                i++;
            }

            foreach (var source in sourceRules.OfType<JsonObject>())
            {
                var property = source["Property"]?.GetValue<string>() ?? string.Empty;
                if (string.IsNullOrWhiteSpace(property))
                {
                    continue;
                }

                var clone = source.DeepClone();
                if (indexByProperty.TryGetValue(property, out var index))
                {
                    targetRules[index] = clone;
                }
                else
                {
                    targetRules.Add(clone);
                    indexByProperty[property] = targetRules.Count - 1;
                }
            }
        }

        private static void MergeStatesWithSourcePriority(JsonArray targetStates, JsonArray sourceStates)
        {
            var merged = new List<string>();
            var seen = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

            foreach (var state in sourceStates)
            {
                var value = state?.GetValue<string>() ?? string.Empty;
                if (!string.IsNullOrWhiteSpace(value) && seen.Add(value))
                {
                    merged.Add(value);
                }
            }

            foreach (var state in targetStates)
            {
                var value = state?.GetValue<string>() ?? string.Empty;
                if (string.IsNullOrWhiteSpace(value) || !seen.Add(value))
                {
                    continue;
                }

                merged.Add(value);
            }

            targetStates.Clear();
            foreach (var value in merged)
            {
                targetStates.Add(value);
            }
        }

        private static bool IsContextualInvariantScript(string script)
        {
            var normalized = script?.Trim() ?? string.Empty;
            if (string.IsNullOrWhiteSpace(normalized))
            {
                return false;
            }

            if (normalized.IndexOf("ThisItem", StringComparison.OrdinalIgnoreCase) >= 0
                || normalized.IndexOf("ThisRecord", StringComparison.OrdinalIgnoreCase) >= 0
                || normalized.IndexOf("Gallery", StringComparison.OrdinalIgnoreCase) >= 0
                || normalized.IndexOf("Parent.Template", StringComparison.OrdinalIgnoreCase) >= 0)
            {
                return true;
            }

            return string.Equals(normalized, "Select(Parent)", StringComparison.OrdinalIgnoreCase)
                   || string.Equals(normalized, "Select( Parent )", StringComparison.OrdinalIgnoreCase);
        }

        private static string GetStatePropertyName(JsonNode? stateEntry)
        {
            if (stateEntry is null)
            {
                return string.Empty;
            }

            if (stateEntry is JsonValue value)
            {
                return value.GetValue<string>() ?? string.Empty;
            }

            if (stateEntry is JsonObject obj)
            {
                return obj["InvariantPropertyName"]?.GetValue<string>()
                       ?? obj["Property"]?.GetValue<string>()
                       ?? string.Empty;
            }

            return string.Empty;
        }

        private void ApplyStyleDefaultRules(JsonObject node, string templateName)
        {
            var styleName = node["StyleName"]?.GetValue<string>() ?? string.Empty;
            if (string.IsNullOrWhiteSpace(styleName)
                || !_styleDefaultRulesByName.TryGetValue(styleName, out var styleDefaults)
                || styleDefaults.Count == 0)
            {
                return;
            }

            if (_styleTemplateByName.TryGetValue(styleName, out var styleTemplate)
                && !string.IsNullOrWhiteSpace(styleTemplate)
                && !string.Equals(styleTemplate, templateName, StringComparison.OrdinalIgnoreCase))
            {
                return;
            }

            var rules = (node["Rules"] as JsonArray) ?? new JsonArray();
            var existingProperties = rules
                .OfType<JsonObject>()
                .Select(r => r["Property"]?.GetValue<string>() ?? string.Empty)
                .Where(p => !string.IsNullOrWhiteSpace(p))
                .ToHashSet(StringComparer.OrdinalIgnoreCase);

            foreach (var (property, value) in styleDefaults)
            {
                if (existingProperties.Contains(property))
                {
                    continue;
                }

                rules.Add(new JsonObject
                {
                    ["Property"] = property,
                    ["Category"] = InferCategory(property),
                    ["InvariantScript"] = NormalizeReservedEnumToken(value),
                    ["RuleProviderType"] = "Unknown",
                });
            }

            node["Rules"] = rules;
        }

        private static string ResolvePaletteTokens(string value, IReadOnlyDictionary<string, string> palette)
        {
            if (string.IsNullOrWhiteSpace(value))
            {
                return value;
            }

            const string prefix = "%Palette.";
            const string suffix = "%";

            if (value.StartsWith(prefix, StringComparison.OrdinalIgnoreCase)
                && value.EndsWith(suffix, StringComparison.OrdinalIgnoreCase))
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
}