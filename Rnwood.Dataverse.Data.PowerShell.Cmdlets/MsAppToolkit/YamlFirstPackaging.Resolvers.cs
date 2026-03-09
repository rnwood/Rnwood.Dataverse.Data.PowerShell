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
{    private static TemplateResolver BuildTemplateResolver(Dictionary<string, byte[]> entries, string unpackDirectory, IReadOnlyList<Dictionary<string, byte[]>> referenceEntriesList)
    {
        var resolver = new TemplateResolver();
        PopulateTemplateResolverFromEntries(entries, resolver);

        foreach (var refEntries in referenceEntriesList)
        {
            PopulateTemplateResolverFromEntries(refEntries, resolver);
        }

        return resolver;
    }

    private static ControlMetadataResolver BuildControlMetadataResolver(Dictionary<string, byte[]> entries, IReadOnlyList<Dictionary<string, byte[]>> referenceEntriesList, int userProvidedReferenceStartIndex = 0)
    {
        var resolver = new ControlMetadataResolver();
        PopulateMetadataResolverFromEntries(entries, resolver);

        // Mark all prototypes registered so far as "seeded" so that controls from
        // reference sources (higher priority) always replace them via condition 1
        // (unconditional replace) instead of condition 2 (bigger-count-wins).
        resolver.MarkAllPrototypesAsSeeded();

        for (int i = 0; i < referenceEntriesList.Count; i++)
        {
            // Enable per-control-name registration only for user-provided reference msapps
            // (not for the embedded AllControls.msapp, whose control names are arbitrary).
            resolver.RegisterPerControlNames = (i >= userProvidedReferenceStartIndex);
            PopulateMetadataResolverFromEntries(referenceEntriesList[i], resolver);
            // Each subsequent source should override the previous one, so re-mark
            // as seeded after every source to ensure later sources always win.
            resolver.MarkAllPrototypesAsSeeded();
        }

        // The embedded "All controls.msapp" enriches the template prototype rules and other
        // defaults, but its StyleName values belong to that generic app and are not representative
        // of the app being packed.  Clear the histogram so that style resolution is driven solely
        // by the app's own existing controls (registered in the first pass above).
        resolver.ClearStyleHistogram();

        return resolver;
    }

    private static TemplatePropertyResolver BuildTemplatePropertyResolver(Dictionary<string, byte[]> entries, IReadOnlyList<Dictionary<string, byte[]>> referenceEntriesList)
    {
        var resolver = new TemplatePropertyResolver();
        PopulateTemplatePropertyResolverFromEntries(entries, resolver);

        foreach (var refEntries in referenceEntriesList)
        {
            PopulateTemplatePropertyResolverFromEntries(refEntries, resolver);
        }

        return resolver;
    }

    private static TemplateKeywordResolver BuildTemplateKeywordResolver(Dictionary<string, byte[]> entries, IReadOnlyList<Dictionary<string, byte[]>> referenceEntriesList)
    {
        var resolver = new TemplateKeywordResolver();
        PopulateTemplateKeywordResolverFromEntries(entries, resolver);

        foreach (var refEntries in referenceEntriesList)
        {
            PopulateTemplateKeywordResolverFromEntries(refEntries, resolver);
        }

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

        /// <summary>
        /// Registers a synthetic placeholder template for an unknown control type so that it can be
        /// processed without throwing. The resulting template ID is prefixed with
        /// <c>http://microsoft.com/appmagic/synthetic/</c> so callers can identify synthetic entries
        /// and skip strict property validation when no metadata is available.
        /// </summary>
        public void RegisterSyntheticTemplate(string controlType)
        {
            var normalizedName = NormalizeControlType(controlType);
            if (string.IsNullOrWhiteSpace(normalizedName))
            {
                return;
            }

            var version = ParseRequestedVersion(controlType);
            var syntheticId = $"http://microsoft.com/appmagic/synthetic/{normalizedName}";
            RegisterTemplate(normalizedName, syntheticId, string.IsNullOrWhiteSpace(version) ? "1.0.0" : version);
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
            var isClassic = slash.Length == 2 && string.Equals(slash[0], "Classic", StringComparison.OrdinalIgnoreCase);
            var canonical = slash.Length == 2 ? slash[1] : slash[0];
            if (string.Equals(canonical, "TextInput", StringComparison.OrdinalIgnoreCase))
            {
                // Classic/TextInput maps to the "text" template (http://microsoft.com/appmagic/text),
                // which has the classic property set (Color, Size, HoverColor, HoverFill, etc.).
                // Bare TextInput (no Classic/ prefix) maps to the modern PCF canvas control.
                canonical = isClassic ? "text" : "PowerApps_CoreControls_TextInputCanvas";
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

        /// <summary>
        /// Clears the style histogram so that style resolution relies only on values seeded
        /// from controls registered before this call.  Call this after processing the embedded
        /// fallback app to ensure generic embedded styles do not override the app's own styles.
        /// </summary>
        public void ClearStyleHistogram() => _styleHistogram.Clear();

        /// <summary>
        /// Marks all currently registered rule and state prototypes as "seeded from template".
        /// This ensures that <see cref="RegisterFromControlNode"/> from subsequent (higher-priority)
        /// sources unconditionally replaces existing prototypes instead of using the
        /// "bigger count wins" fallback logic.
        /// </summary>
        public void MarkAllPrototypesAsSeeded()
        {
            foreach (var key in _rulesPrototypeByTemplate.Keys)
            {
                _rulesPrototypeSeededFromTemplate.Add(key);
            }

            foreach (var key in _statePrototypeByTemplate.Keys)
            {
                _statePrototypeSeededFromTemplate.Add(key);
            }
        }

        /// <summary>
        /// Removes rules whose property name is not in the per-control-name prototype
        /// for this control.  Studio's Src/ YAML often includes properties that are
        /// omitted from Controls JSON because they are at default values.  This method
        /// prunes those excess rules so the Controls JSON matches the reference source.
        /// Call this after ReplaceProperties has applied YAML overrides.
        /// </summary>
        public void PruneExcessRulesToPerControlPrototype(JsonObject node, HashSet<string>? yamlPropertyNames = null)
        {
            var controlName = node["Name"]?.GetValue<string>() ?? string.Empty;
            HashSet<string>? allowedProperties = null;

            if (!string.IsNullOrWhiteSpace(controlName)
                && _allRulePropertyNamesByControlName.TryGetValue(controlName, out allowedProperties))
            {
                // Per-control prototype found — prune to its rule set.
            }
            else if (yamlPropertyNames is not null && yamlPropertyNames.Count > 0)
            {
                // No per-control prototype (no reference msapp).  Fall back to
                // YAML-only properties so that template/style defaults are not
                // materialised for fresh-from-YAML controls.
                allowedProperties = yamlPropertyNames;
            }
            else
            {
                return;
            }

            // Gallery controls have rules (TemplateFill, OnSelect, etc.) that get
            // moved to the galleryTemplate child by a later transform pass.  Those
            // rules won't be in this control's prototype yet, so skip pruning for
            // galleries to avoid removing rules before the transform can move them.
            var templateName = node["Template"]?["Name"]?.GetValue<string>() ?? string.Empty;
            if (string.Equals(templateName, "gallery", StringComparison.OrdinalIgnoreCase))
            {
                return;
            }

            var rules = node["Rules"] as JsonArray;
            if (rules is null)
            {
                return;
            }

            for (int i = rules.Count - 1; i >= 0; i--)
            {
                var property = (rules[i] as JsonObject)?["Property"]?.GetValue<string>() ?? string.Empty;
                if (!string.IsNullOrWhiteSpace(property)
                    && !allowedProperties.Contains(property))
                {
                    rules.RemoveAt(i);
                }
            }
        }

        /// <summary>
        /// Applies per-control metadata defaults from the reference source to a control node.
        /// Fills in metadata fields that the Src/ YAML doesn't carry (MetaDataIDKey,
        /// VariantName, StyleName, LayoutName, IsFromScreenLayout, IsAutoGenerated) when
        /// the current values are empty/unset and a reference prototype exists.
        /// </summary>
        public void ApplyPerControlMetadataDefaults(JsonObject node)
        {
            var controlName = node["Name"]?.GetValue<string>() ?? string.Empty;
            if (string.IsNullOrWhiteSpace(controlName)
                || !_metadataByControlName.TryGetValue(controlName, out var meta))
            {
                return;
            }

            if (!string.IsNullOrWhiteSpace(meta.MetaDataIDKey)
                && string.IsNullOrWhiteSpace(node["MetaDataIDKey"]?.GetValue<string>() ?? string.Empty))
            {
                node["MetaDataIDKey"] = meta.MetaDataIDKey;
            }

            if (!string.IsNullOrWhiteSpace(meta.VariantName)
                && string.IsNullOrWhiteSpace(node["VariantName"]?.GetValue<string>() ?? string.Empty))
            {
                node["VariantName"] = meta.VariantName;
            }

            if (!string.IsNullOrWhiteSpace(meta.StyleName)
                && string.IsNullOrWhiteSpace(node["StyleName"]?.GetValue<string>() ?? string.Empty))
            {
                node["StyleName"] = meta.StyleName;
            }

            if (!string.IsNullOrWhiteSpace(meta.LayoutName)
                && string.IsNullOrWhiteSpace(node["LayoutName"]?.GetValue<string>() ?? string.Empty))
            {
                node["LayoutName"] = meta.LayoutName;
            }

            if (meta.IsFromScreenLayout.HasValue && meta.IsFromScreenLayout.Value)
            {
                // Override the default false from CreateDefaultControl.  YAML never sets
                // this field explicitly — it's inferred from MetaDataIDKey or the reference.
                node["IsFromScreenLayout"] = true;
            }

            if (meta.IsAutoGenerated.HasValue && meta.IsAutoGenerated.Value)
            {
                node["IsAutoGenerated"] = true;
            }

            // IsLocked is on the DataCard in YAML but children also need it in Controls JSON.
            // The reference carries it per-control-name, filling in where YAML doesn't specify.
            if (meta.IsLocked.HasValue && meta.IsLocked.Value)
            {
                var currentLocked = (node["IsLocked"] as JsonValue)?.TryGetValue<bool>(out var cl) == true && cl;
                if (!currentLocked)
                {
                    node["IsLocked"] = true;
                }
            }

            // DynamicProperties: carry reference values for AutoLayout properties
            // (FillPortions, LayoutMinHeight, AlignInContainer, etc.) that the Src/ YAML
            // often omits.  Replace baseline template defaults entirely with the reference
            // snapshot.  The later FixAutoLayoutChildDynamicProperties pass will sync any
            // YAML-specified values into DynamicProperties, so YAML always wins.
            if (meta.DynamicProperties is not null && meta.DynamicProperties.Count > 0)
            {
                node["DynamicProperties"] = (JsonArray)meta.DynamicProperties.DeepClone();
            }
        }

        private readonly Dictionary<string, string> _styleTemplateByName = new(StringComparer.OrdinalIgnoreCase);
        private readonly Dictionary<string, Dictionary<string, string>> _styleDefaultRulesByName = new(StringComparer.OrdinalIgnoreCase);
        private readonly Dictionary<string, JsonArray> _rulesPrototypeByTemplate = new(StringComparer.OrdinalIgnoreCase);
        private readonly Dictionary<string, JsonArray> _statePrototypeByTemplate = new(StringComparer.OrdinalIgnoreCase);
        private readonly Dictionary<string, bool> _hasDynamicPropertiesByTemplate = new(StringComparer.OrdinalIgnoreCase);
        private readonly HashSet<string> _rulesPrototypeSeededFromTemplate = new(StringComparer.OrdinalIgnoreCase);
        private readonly HashSet<string> _statePrototypeSeededFromTemplate = new(StringComparer.OrdinalIgnoreCase);
        /// <summary>
        /// Tracks template keys whose rules prototype was set from real control observations
        /// (via <see cref="RegisterFromControlNode"/>). These prototypes already include
        /// style-derived properties, so <see cref="ApplyStyleDefaultRules"/> should be skipped
        /// to avoid adding extra properties that the source controls don't have.
        /// </summary>
        private readonly HashSet<string> _rulesPrototypeFromControlObservation = new(StringComparer.OrdinalIgnoreCase);

        /// <summary>
        /// Controls whether <see cref="RegisterFromControlNode"/> populates per-control-name
        /// prototypes (<see cref="_rulesPrototypeByControlName"/> etc.). These should only be
        /// populated from explicitly-provided reference msapps, not from the embedded
        /// AllControls.msapp (whose control names are arbitrary and may coincidentally collide
        /// with the target app's control names).
        /// </summary>
        public bool RegisterPerControlNames { get; set; }

        /// <summary>
        /// Per-control-name rules prototype. Different controls of the same template type
        /// may have different rule sets (e.g. gallery-child labels have TabIndex while form
        /// labels have AutoHeight). This dictionary stores the exact rules observed for each
        /// control name in the reference sources, allowing precise reconstruction.
        /// </summary>
        private readonly Dictionary<string, JsonArray> _rulesPrototypeByControlName = new(StringComparer.OrdinalIgnoreCase);
        private readonly Dictionary<string, JsonArray> _statePrototypeByControlName = new(StringComparer.OrdinalIgnoreCase);

        /// <summary>
        /// Tracks ALL rule property names (including contextual Self./Parent. references)
        /// observed for each control name in reference sources. Used by
        /// <see cref="PruneExcessRulesToPerControlPrototype"/> to remove YAML-added rules
        /// that Studio includes in Src/ YAML but omits from Controls JSON.
        /// </summary>
        private readonly Dictionary<string, HashSet<string>> _allRulePropertyNamesByControlName = new(StringComparer.OrdinalIgnoreCase);

        /// <summary>
        /// Per-control-name metadata observed from reference sources (when
        /// <see cref="RegisterPerControlNames"/> is enabled). Includes fields that the
        /// Src/ YAML doesn't carry (MetaDataIDKey, VariantName, StyleName,
        /// IsFromScreenLayout, IsAutoGenerated, LayoutName). Applied by
        /// <see cref="ApplyPerControlMetadataDefaults"/> to fill in metadata not set by YAML.
        /// </summary>
        private readonly Dictionary<string, PerControlMetadata> _metadataByControlName = new(StringComparer.OrdinalIgnoreCase);

        private sealed class PerControlMetadata
        {
            public string MetaDataIDKey { get; set; } = string.Empty;
            public string VariantName { get; set; } = string.Empty;
            public string StyleName { get; set; } = string.Empty;
            public string LayoutName { get; set; } = string.Empty;
            public bool? IsFromScreenLayout { get; set; }
            public bool? IsAutoGenerated { get; set; }
            public bool? IsLocked { get; set; }
            /// <summary>Deep clone of the DynamicProperties array from the reference control.</summary>
            public JsonArray? DynamicProperties { get; set; }
        }

        private static readonly HashSet<string> _supportsNestedControls = new(StringComparer.OrdinalIgnoreCase)
        {
            "dataCard", "dataGrid", "dataTable", "dataTableColumn",
            "datatableHeaderCellCard", "datatableRowCellCard",
            "entityForm", "fluidGrid", "form", "formViewer",
            "gallery", "groupContainer", "layoutContainer",
            "pcfDataField", "typedDataCard",
        };

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

                // Process template-owned <property> elements first. Per the authoritative
                // PowerApps-Tooling, all properties with default values are added as rules
                // Studio's canonical ControlPropertyState ordering places
                // <property>-only entries (those NOT duplicated in <includeProperty>)
                // first, followed by <includeProperty> entries in their declared order.
                // Properties that appear in both follow the <includeProperty> position.

                var includePropertyNames = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
                foreach (var ip in root.Descendants().Where(e => e.Name.LocalName == "includeProperty"))
                {
                    var name = ip.Attribute("name")?.Value ?? string.Empty;
                    if (!string.IsNullOrWhiteSpace(name))
                    {
                        includePropertyNames.Add(name);
                    }
                }

                // Pass 1: <property> elements that are NOT also <includeProperty>.
                // These come first in the canonical CPS ordering.
                foreach (var property in root.Descendants().Where(e => e.Name.LocalName == "property"))
                {
                    var propertyName = property.Attribute("name")?.Value ?? string.Empty;
                    if (string.IsNullOrWhiteSpace(propertyName) || !seen.Add(propertyName))
                    {
                        continue;
                    }

                    var defaultValue = property.Attribute("defaultValue")?.Value ?? string.Empty;
                    if (!string.IsNullOrWhiteSpace(defaultValue)
                        && !IsTrivialIncludePropertyDefault(propertyName, defaultValue))
                    {
                        rules.Add(new JsonObject
                        {
                            ["Property"] = propertyName,
                            ["Category"] = InferCategory(propertyName),
                            ["InvariantScript"] = NormalizeReservedEnumToken(defaultValue),
                            ["RuleProviderType"] = "Unknown",
                        });
                    }

                    // Only add to states if NOT an includeProperty (those go in pass 2).
                    if (!includePropertyNames.Contains(propertyName))
                    {
                        states.Add(propertyName);
                    }
                }

                // Pass 2: <includeProperty> elements in declared order.
                // Also create rules for <property> elements that were deferred in pass 1
                // because they are also includeProperty (their state position comes from here).
                foreach (var includeProperty in root.Descendants().Where(e => e.Name.LocalName == "includeProperty"))
                {
                    var propertyName = includeProperty.Attribute("name")?.Value ?? string.Empty;
                    if (string.IsNullOrWhiteSpace(propertyName))
                    {
                        continue;
                    }

                    // If this was already added by pass 1 as property-only, skip.
                    if (!seen.Add(propertyName))
                    {
                        continue;
                    }

                    states.Add(propertyName);
                }

                // Pass 3: add deferred <property> names (those also in includeProperty)
                // to states in their includeProperty position (already done above since
                // seen.Add succeeds only once, and pass 1 skipped their state entry).
                // But we still need rules for properties that were in both sets and had
                // default values. Re-scan property elements for those not yet in rules.
                foreach (var property in root.Descendants().Where(e => e.Name.LocalName == "property"))
                {
                    var propertyName = property.Attribute("name")?.Value ?? string.Empty;
                    if (string.IsNullOrWhiteSpace(propertyName))
                    {
                        continue;
                    }

                    // Skip if rule already created in pass 1 (property-only entries).
                    if (!includePropertyNames.Contains(propertyName))
                    {
                        continue;
                    }

                    // Check if rule already exists.
                    var alreadyHasRule = rules.OfType<JsonObject>()
                        .Any(r => string.Equals(r["Property"]?.GetValue<string>() ?? string.Empty,
                            propertyName, StringComparison.OrdinalIgnoreCase));
                    if (alreadyHasRule)
                    {
                        continue;
                    }

                    var defaultValue = property.Attribute("defaultValue")?.Value ?? string.Empty;
                    if (!string.IsNullOrWhiteSpace(defaultValue)
                        && !IsTrivialIncludePropertyDefault(propertyName, defaultValue))
                    {
                        rules.Add(new JsonObject
                        {
                            ["Property"] = propertyName,
                            ["Category"] = InferCategory(propertyName),
                            ["InvariantScript"] = NormalizeReservedEnumToken(defaultValue),
                            ["RuleProviderType"] = "Unknown",
                        });
                    }
                }

                if (rules.Count == 0 && states.Count == 0)
                {
                    return;
                }

                foreach (var key in GetTemplateKeys(templateName, templateId))
                {
                    if (rules.Count > 0)
                    {
                        // Only seed rules from template XML when no rules prototype has been
                        // set yet.  Control-observed rules (from RegisterFromControlNode) are
                        // more accurate because they include style-derived and theme-derived
                        // properties that Studio adds but template XML omits.  Unconditionally
                        // overwriting here would destroy those observations.
                        if (!_rulesPrototypeByTemplate.TryGetValue(key, out var existingRules)
                            || existingRules.Count == 0)
                        {
                            _rulesPrototypeByTemplate[key] = (JsonArray)rules.DeepClone();
                        }

                        // Always mark as seeded so that RegisterFromControlNode can fully
                        // replace template-XML defaults with control-observed rules.
                        _rulesPrototypeSeededFromTemplate.Add(key);
                    }

                    if (states.Count > 0)
                    {
                        if (!_statePrototypeByTemplate.TryGetValue(key, out var existingStates)
                            || existingStates.Count == 0)
                        {
                            _statePrototypeByTemplate[key] = (JsonArray)states.DeepClone();
                        }

                        _statePrototypeSeededFromTemplate.Add(key);
                    }
                }
            }
            catch
            {
                // Ignore malformed template xml.
            }
        }

        /// <summary>
        /// Returns <see langword="true" /> when an <c>includeProperty</c> default value should NOT be
        /// emitted as an explicit Rule in the rules prototype.  Per the PowerApps-Tooling reference,
        /// only localization-key placeholders (e.g. <c>##key##</c>) are suppressed.
        /// </summary>
        private static bool IsTrivialIncludePropertyDefault(string propertyName, string defaultValue)
        {
            // Localization-key placeholders are never meaningful default values.
            var trimmed = defaultValue.Trim();
            return trimmed.StartsWith("##", StringComparison.Ordinal) && trimmed.EndsWith("##", StringComparison.Ordinal);
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
                        // When existing controls provide rule observations, use ONLY the
                        // control-observed rules. Template XML defaults are intentionally discarded
                        // to avoid generating extra ControlPropertyState entries that Power Apps
                        // Studio omits when properties are at their defaults.
                        // Preserve the template XML's authoritative categories for matching properties.
                        foreach (var rule in candidateRules.OfType<JsonObject>())
                        {
                            var prop = rule["Property"]?.GetValue<string>() ?? string.Empty;
                            if (string.IsNullOrWhiteSpace(prop))
                            {
                                continue;
                            }

                            var seededRule = seededRules.OfType<JsonObject>()
                                .FirstOrDefault(r => string.Equals(
                                    r["Property"]?.GetValue<string>() ?? string.Empty,
                                    prop, StringComparison.OrdinalIgnoreCase));
                            if (seededRule is not null)
                            {
                                rule["Category"] = seededRule["Category"]?.GetValue<string>()
                                    ?? rule["Category"]?.GetValue<string>() ?? string.Empty;
                            }
                        }

                        _rulesPrototypeByTemplate[key] = (JsonArray)candidateRules.DeepClone();
                        _rulesPrototypeFromControlObservation.Add(key);
                    }
                    else if (!_rulesPrototypeByTemplate.TryGetValue(key, out var existingRules)
                             || existingRules.Count < candidateRules.Count)
                    {
                        _rulesPrototypeByTemplate[key] = (JsonArray)candidateRules.DeepClone();
                        _rulesPrototypeFromControlObservation.Add(key);
                    }
                }

                if (candidateStates.Count > 0)
                {
                    if (_statePrototypeSeededFromTemplate.Contains(key)
                        && _statePrototypeByTemplate.TryGetValue(key, out var seededStates)
                        && seededStates.Count > 0)
                    {
                        // Preserve the template XML canonical ordering.  Only append
                        // properties observed in control CPS that the template XML
                        // does not already define.  MergeStatesWithSourcePriority would
                        // put control entries first, corrupting the canonical order when
                        // different controls have different property subsets.
                        var existing = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
                        foreach (var entry in seededStates)
                        {
                            var p = GetStatePropertyName(entry);
                            if (!string.IsNullOrWhiteSpace(p))
                            {
                                existing.Add(p);
                            }
                        }

                        foreach (var entry in candidateStates)
                        {
                            var p = GetStatePropertyName(entry);
                            if (!string.IsNullOrWhiteSpace(p) && !existing.Contains(p))
                            {
                                seededStates.Add(entry.DeepClone());
                                existing.Add(p);
                            }
                        }
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

            // Per-control-name prototypes: record the exact rules and CPS observed for
            // each named control.  This allows ApplyTemplateDefaults to use the precise
            // baseline for a specific control (e.g. a gallery-child label gets different
            // defaults than a form-child label of the same template type).
            // Only register when RegisterPerControlNames is enabled (i.e. processing an
            // explicitly-provided reference msapp, not the generic AllControls.msapp).
            var controlName = node["Name"]?.GetValue<string>() ?? string.Empty;
            if (RegisterPerControlNames && !string.IsNullOrWhiteSpace(controlName) && candidateRules.Count > 0)
            {
                _rulesPrototypeByControlName[controlName] = (JsonArray)candidateRules.DeepClone();
                if (candidateStates.Count > 0)
                {
                    _statePrototypeByControlName[controlName] = (JsonArray)candidateStates.DeepClone();
                }

                // Store ALL rule property names (including contextual) for precise pruning.
                // The non-contextual prototype is used as the baseline, and contextual rules
                // come from YAML.  But YAML may also carry defaults that Studio omits from
                // Controls JSON — _allRulePropertyNamesByControlName lets us prune those.
                var allPropNames = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
                foreach (var rule in rules.OfType<JsonObject>())
                {
                    var prop = rule["Property"]?.GetValue<string>() ?? string.Empty;
                    if (!string.IsNullOrWhiteSpace(prop))
                    {
                        allPropNames.Add(prop);
                    }
                }

                _allRulePropertyNamesByControlName[controlName] = allPropNames;

                // Also store per-control metadata fields that Studio includes in Controls
                // JSON but the Src/ YAML omits (MetaDataIDKey, VariantName, StyleName, etc.).
                _metadataByControlName[controlName] = new PerControlMetadata
                {
                    MetaDataIDKey = node["MetaDataIDKey"]?.GetValue<string>() ?? string.Empty,
                    VariantName = node["VariantName"]?.GetValue<string>() ?? string.Empty,
                    StyleName = node["StyleName"]?.GetValue<string>() ?? string.Empty,
                    LayoutName = node["LayoutName"]?.GetValue<string>() ?? string.Empty,
                    IsFromScreenLayout = (node["IsFromScreenLayout"] as JsonValue)?.TryGetValue<bool>(out var fsl) == true ? fsl : null,
                    IsAutoGenerated = (node["IsAutoGenerated"] as JsonValue)?.TryGetValue<bool>(out var iag) == true ? iag : null,
                    IsLocked = (node["IsLocked"] as JsonValue)?.TryGetValue<bool>(out var il) == true ? il : null,
                    DynamicProperties = node["DynamicProperties"] is JsonArray dp && dp.Count > 0
                        ? (JsonArray)dp.DeepClone() : null,
                };
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

            // Deterministic fallback per PowerApps-Tooling: default{TemplateName}Style
            if (!string.IsNullOrWhiteSpace(templateName))
            {
                return $"default{char.ToUpperInvariant(templateName[0])}{templateName.Substring(1)}Style";
            }

            return null;
        }

        public void ApplyTemplateDefaults(JsonObject node, string templateName, string templateId, JsonObject? parentNode = null)
        {
            var keys = GetTemplateKeys(templateName, templateId);
            if (keys.Count == 0)
            {
                return;
            }

            // Prefer per-control-name prototype (exact match from reference source)
            // over per-template prototype (shared across all controls of same type).
            var controlName = node["Name"]?.GetValue<string>() ?? string.Empty;
            bool hasPerControlPrototype = !string.IsNullOrWhiteSpace(controlName)
                && _rulesPrototypeByControlName.ContainsKey(controlName);

            var rules = node["Rules"] as JsonArray;
            bool prototypeFromControlObservation = hasPerControlPrototype
                || keys.Any(k => _rulesPrototypeFromControlObservation.Contains(k));
            if (rules is null || rules.Count == 0)
            {
                if (hasPerControlPrototype)
                {
                    node["Rules"] = _rulesPrototypeByControlName[controlName].DeepClone();
                }
                else
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
            }

            var state = node["ControlPropertyState"] as JsonArray;
            if (state is null || state.Count == 0)
            {
                if (hasPerControlPrototype && _statePrototypeByControlName.TryGetValue(controlName, out var perControlState) && perControlState.Count > 0)
                {
                    node["ControlPropertyState"] = perControlState.DeepClone();
                }
                else
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

            var hasDynamicProperties = (node["HasDynamicProperties"] as JsonValue)?.TryGetValue<bool>(out var hasDyn) == true && hasDyn;
            // Skip style defaults when the prototype came from control observations in a
            // reference source — those prototypes already include the style-derived properties
            // that Studio materializes.  Applying style defaults on top would add extra
            // properties that the reference controls don't have.
            if (!hasDynamicProperties && !prototypeFromControlObservation)
            {
                ApplyStyleDefaultRules(node, templateName);
            }

            NormalizeReservedEnumTokensInRules(node);
            EnsureGalleryLayoutRuleFromVariant(node);
            EnsureGroupContainerVariantDefaults(node);
            UpdateControlPropertyStateFromRules(node);

            // Per PowerApps-Tooling: dynamic properties are only injected when the parent
            // is a non-manual groupContainer (AddsChildDynamicProperties).
            if (IsInResponsiveLayout(parentNode))
            {
                EnsureAutoLayoutChildBaseline(node, templateName);
            }
        }

        /// <summary>
        /// Returns true when the parent control adds child dynamic-properties, matching
        /// PowerApps-Tooling DynamicProperties.AddsChildDynamicProperties.
        /// </summary>
        private static bool IsInResponsiveLayout(JsonObject? parentNode)
        {
            if (parentNode is null)
            {
                return false;
            }

            var parentTemplate = parentNode["Template"]?["Name"]?.GetValue<string>() ?? string.Empty;
            if (!string.Equals(parentTemplate, "groupContainer", StringComparison.OrdinalIgnoreCase))
            {
                return false;
            }

            var parentVariant = parentNode["VariantName"]?.GetValue<string>() ?? string.Empty;
            // manualLayoutContainer does NOT add child dynamic props
            return !string.Equals(parentVariant, "manualLayoutContainer", StringComparison.OrdinalIgnoreCase);
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

        /// <summary>
        /// Applies variant-specific defaults for groupContainer variants
        /// (horizontalAutoLayoutContainer and verticalAutoLayoutContainer)
        /// matching PowerApps-Tooling GlobalTemplates.cs.
        /// </summary>
        private static void EnsureGroupContainerVariantDefaults(JsonObject node)
        {
            var templateName = node["Template"]?["Name"]?.GetValue<string>() ?? string.Empty;
            if (!string.Equals(templateName, "groupContainer", StringComparison.OrdinalIgnoreCase))
            {
                return;
            }

            var variantName = node["VariantName"]?.GetValue<string>() ?? string.Empty;
            string? layoutDirection = null;

            if (variantName.IndexOf("horizontal", StringComparison.OrdinalIgnoreCase) >= 0)
            {
                layoutDirection = "LayoutDirection.Horizontal";
            }
            else if (variantName.IndexOf("vertical", StringComparison.OrdinalIgnoreCase) >= 0)
            {
                layoutDirection = "LayoutDirection.Vertical";
            }

            // manualLayoutContainer and unknown variants don't get auto-layout defaults
            if (layoutDirection is null)
            {
                return;
            }

            var rules = (node["Rules"] as JsonArray) ?? new JsonArray();
            node["Rules"] = rules;

            void EnsureRule(string property, string category, string invariantScript)
            {
                var existing = rules.OfType<JsonObject>()
                    .FirstOrDefault(r => string.Equals(
                        r["Property"]?.GetValue<string>() ?? string.Empty,
                        property, StringComparison.OrdinalIgnoreCase));
                if (existing is null)
                {
                    rules.Add(new JsonObject
                    {
                        ["Property"] = property,
                        ["Category"] = category,
                        ["InvariantScript"] = invariantScript,
                        ["RuleProviderType"] = "Unknown",
                    });
                }
            }

            EnsureRule("LayoutMode", "Design", "LayoutMode.Auto");
            EnsureRule("LayoutDirection", "Design", layoutDirection);
            EnsureRule("LayoutWrap", "Design", "false");
            EnsureRule("LayoutAlignItems", "Design", "LayoutAlignItems.Start");
            EnsureRule("LayoutJustifyContent", "Design", "LayoutJustifyContent.Start");
            EnsureRule("LayoutGap", "Design", "0");
            EnsureRule("LayoutOverflowX", "Design", "LayoutOverflow.Hide");
            EnsureRule("LayoutOverflowY", "Design", "LayoutOverflow.Hide");
        }

        private string? TryGetPrototypeRuleDefault(string templateName, string templateId, string propertyName)
        {
            foreach (var key in GetTemplateKeys(templateName, templateId))
            {
                if (_rulesPrototypeByTemplate.TryGetValue(key, out var rules))
                {
                    foreach (var rule in rules.OfType<JsonObject>())
                    {
                        if (string.Equals(rule["Property"]?.GetValue<string>() ?? string.Empty, propertyName, StringComparison.OrdinalIgnoreCase))
                        {
                            return rule["InvariantScript"]?.GetValue<string>();
                        }
                    }
                }
            }
            return null;
        }

        private void EnsureAutoLayoutChildBaseline(JsonObject node, string templateName)
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

            var isContainer = _supportsNestedControls.Contains(templateName);
            var templateId = node["Template"]?["Id"]?.GetValue<string>() ?? string.Empty;

            EnsureDynamicProperty("FillPortions", "Design", isContainer ? "1" : "0");
            EnsureDynamicProperty("AlignInContainer", "Design",
                isContainer ? "AlignInContainer.Stretch" : "AlignInContainer.SetByContainer");

            string layoutMinHeight;
            string layoutMinWidth;
            if (isContainer)
            {
                layoutMinHeight = "32";
                layoutMinWidth = "32";
            }
            else
            {
                layoutMinHeight = TryGetPrototypeRuleDefault(templateName, templateId, "Height") ?? "0";
                layoutMinWidth = TryGetPrototypeRuleDefault(templateName, templateId, "Width") ?? "0";
            }

            EnsureDynamicProperty("LayoutMinHeight", "Design", layoutMinHeight);
            EnsureDynamicProperty("LayoutMinWidth", "Design", layoutMinWidth);
            EnsureDynamicProperty("LayoutMaxHeight", "Design", "0");
            EnsureDynamicProperty("LayoutMaxWidth", "Design", "0");
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
            var merged = new List<JsonNode>();
            var indexByProperty = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);

            static JsonNode CloneOrCreate(JsonNode? stateEntry, string propertyName)
            {
                if (stateEntry is null)
                {
                    return JsonValue.Create(propertyName)!;
                }

                return stateEntry.DeepClone();
            }

            void mergeFrom(JsonArray source)
            {
                foreach (var stateEntry in source)
                {
                    var propertyName = GetStatePropertyName(stateEntry);
                    if (string.IsNullOrWhiteSpace(propertyName))
                    {
                        continue;
                    }

                    var clone = CloneOrCreate(stateEntry, propertyName);
                    if (indexByProperty.TryGetValue(propertyName, out var existingIndex))
                    {
                        merged[existingIndex] = clone;
                    }
                    else
                    {
                        indexByProperty[propertyName] = merged.Count;
                        merged.Add(clone);
                    }
                }
            }

            // Source entries first so they win on conflicts.
            mergeFrom(sourceStates);
            mergeFrom(targetStates);

            targetStates.Clear();
            foreach (var entry in merged)
            {
                targetStates.Add(entry);
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
                if (value.TryGetValue<string>(out var stringValue) && !string.IsNullOrWhiteSpace(stringValue))
                {
                    return stringValue;
                }

                var raw = value.ToJsonString().Trim();
                if (string.IsNullOrWhiteSpace(raw) || string.Equals(raw, "null", StringComparison.OrdinalIgnoreCase))
                {
                    return string.Empty;
                }

                if (raw.Length >= 2 && raw[0] == '"' && raw[raw.Length - 1] == '"')
                {
                    return raw.Substring(1, raw.Length - 2);
                }

                return raw;
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