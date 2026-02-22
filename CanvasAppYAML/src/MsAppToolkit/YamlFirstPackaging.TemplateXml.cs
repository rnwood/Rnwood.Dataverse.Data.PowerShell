using System.Globalization;
using System.IO.Compression;
using System.Linq;
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
{    private static IReadOnlyList<UsedTemplate> ReadUsedTemplates(IReadOnlyDictionary<string, byte[]> entries)
    {
        if (!entries.TryGetValue("References/Templates.json", out var templatesBytes))
        {
            return Array.Empty<UsedTemplate>();
        }

        var templatesRoot = JsonNode.Parse(templatesBytes)?.AsObject();
        var usedTemplates = templatesRoot?["UsedTemplates"] as JsonArray;
        if (usedTemplates is null)
        {
            return Array.Empty<UsedTemplate>();
        }

        var list = new List<UsedTemplate>();
        foreach (var item in usedTemplates.OfType<JsonObject>())
        {
            var name = item["Name"]?.GetValue<string>() ?? string.Empty;
            var id = item["Id"]?.GetValue<string>() ?? string.Empty;
            var version = item["Version"]?.GetValue<string>() ?? string.Empty;
            var templateXml = item["Template"]?.GetValue<string>() ?? string.Empty;
            if (string.IsNullOrWhiteSpace(name) || string.IsNullOrWhiteSpace(version))
            {
                continue;
            }

            if (string.IsNullOrWhiteSpace(id))
            {
                id = ParseTemplateId(templateXml);
            }

            if (string.IsNullOrWhiteSpace(id))
            {
                id = $"http://microsoft.com/appmagic/{name}";
            }

            list.Add(new UsedTemplate(name, version, id, templateXml));
        }

        return list;
    }

    private static IReadOnlyList<JsonObject> ReadPcfTemplates(IReadOnlyDictionary<string, byte[]> entries)
    {
        if (!entries.TryGetValue("References/Templates.json", out var templatesBytes))
        {
            return Array.Empty<JsonObject>();
        }

        var templatesRoot = JsonNode.Parse(templatesBytes)?.AsObject();
        var pcfTemplates = templatesRoot?["PcfTemplates"] as JsonArray;
        if (pcfTemplates is null)
        {
            return Array.Empty<JsonObject>();
        }

        return pcfTemplates.OfType<JsonObject>().Select(t => (JsonObject)t.DeepClone()).ToList();
    }

    /// <summary>
    /// Reads all control nodes in the given entries (Controls/*.json + Components/*.json)
    /// and returns a mapping of template-name (lowercased) → distinct VariantName values.
    /// This is the authoritative source for which templates require a Variant keyword and
    /// what the valid variant values are.
    /// </summary>
    private static IReadOnlyDictionary<string, IReadOnlyList<string>> CollectVariantsByTemplateName(Dictionary<string, byte[]> entries)
    {
        var result = new Dictionary<string, HashSet<string>>(StringComparer.OrdinalIgnoreCase);

        var allTopParents = ParseTopParentFiles(entries, "Controls/")
            .Concat(ParseTopParentFiles(entries, "Components/"))
            .Select(kv => kv.Value["TopParent"] as JsonObject)
            .Where(v => v is not null)
            .Cast<JsonObject>();

        foreach (var top in allTopParents)
        {
            foreach (var node in Flatten(top))
            {
                var tplName = node["Template"]?["Name"]?.GetValue<string>() ?? string.Empty;
                var variantName = node["VariantName"]?.GetValue<string>() ?? string.Empty;
                if (string.IsNullOrWhiteSpace(tplName) || string.IsNullOrWhiteSpace(variantName))
                {
                    continue;
                }

                if (!result.TryGetValue(tplName, out var set))
                {
                    set = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
                    result[tplName] = set;
                }

                set.Add(variantName);
            }
        }

        return result.ToDictionary(
            kv => kv.Key,
            kv => (IReadOnlyList<string>)kv.Value.OrderBy(v => v, StringComparer.OrdinalIgnoreCase).ToList(),
            StringComparer.OrdinalIgnoreCase);
    }

    /// <summary>
    /// Returns true when packing will require a non-empty Variant keyword for the given template.
    /// This is the case when any existing control node uses this template with a non-empty VariantName.
    /// </summary>
    private static bool RequiresVariantKeyword(
        string templateName,
        string templateId,
        string templateXml,
        IReadOnlyDictionary<string, IReadOnlyList<string>> variantMap)
    {
        // Explicit variant nodes inside the template XML also trigger the requirement.
        if (ParseVariantsFromTemplateXml(templateXml).Count > 0)
        {
            return true;
        }

        if (!string.IsNullOrWhiteSpace(templateName) && variantMap.ContainsKey(templateName))
        {
            return true;
        }

        return false;
    }

    /// <summary>
    /// Returns the union of variants from the template XML and from existing control nodes.
    /// </summary>
    private static IReadOnlyList<string> MergeVariants(
        string templateName,
        string templateId,
        string templateXml,
        IReadOnlyDictionary<string, IReadOnlyList<string>> variantMap)
    {
        var merged = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

        foreach (var v in ParseVariantsFromTemplateXml(templateXml))
        {
            merged.Add(v);
        }

        if (!string.IsNullOrWhiteSpace(templateName) && variantMap.TryGetValue(templateName, out var fromNodes))
        {
            foreach (var v in fromNodes)
            {
                merged.Add(v);
            }
        }

        return merged.OrderBy(v => v, StringComparer.OrdinalIgnoreCase).ToList();
    }

    /// <summary>
    /// Parses variant names declared directly in a template's XML via &lt;variant name="…"/&gt; elements.
    /// </summary>
    private static IReadOnlyList<string> ParseVariantsFromTemplateXml(string templateXml)
    {
        if (string.IsNullOrWhiteSpace(templateXml))
        {
            return Array.Empty<string>();
        }

        try
        {
            var root = XDocument.Parse(templateXml).Root;
            if (root is null)
            {
                return Array.Empty<string>();
            }

            return root
                .Descendants()
                .Where(e => string.Equals(e.Name.LocalName, "variant", StringComparison.OrdinalIgnoreCase))
                .Select(e => e.Attribute("name")?.Value ?? string.Empty)
                .Where(n => !string.IsNullOrWhiteSpace(n))
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .OrderBy(n => n, StringComparer.OrdinalIgnoreCase)
                .ToList();
        }
        catch
        {
            return Array.Empty<string>();
        }
    }

    /// <summary>Parses top-level insertMetadata category names from template XML.</summary>
    private static IReadOnlyList<string> ParseInsertCategoriesFromTemplateXml(string templateXml)
    {
        if (string.IsNullOrWhiteSpace(templateXml)) return Array.Empty<string>();
        try
        {
            var root = XDocument.Parse(templateXml).Root;
            if (root is null) return Array.Empty<string>();
            return root
                .Descendants()
                .Where(e => string.Equals(e.Name.LocalName, "insertMetadata", StringComparison.OrdinalIgnoreCase))
                .SelectMany(im => im.Elements()
                    .Where(c => string.Equals(c.Name.LocalName, "category", StringComparison.OrdinalIgnoreCase))
                    .Select(c => c.Attribute("name")?.Value ?? string.Empty))
                .Where(n => !string.IsNullOrWhiteSpace(n))
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .OrderBy(n => n, StringComparer.OrdinalIgnoreCase)
                .ToList();
        }
        catch { return Array.Empty<string>(); }
    }

    /// <summary>
    /// Returns preview flag names declared in the template root-level requiredConditions block.
    /// These apply when the template itself is used, regardless of variant.
    /// </summary>
    private static IReadOnlyList<string> ParseRootPreviewFlagsFromTemplateXml(string templateXml)
    {
        if (string.IsNullOrWhiteSpace(templateXml)) return Array.Empty<string>();
        try
        {
            var root = XDocument.Parse(templateXml).Root;
            if (root is null) return Array.Empty<string>();
            // Find requiredConditions that are direct children of the root (or the first non-variant child),
            // i.e. NOT nested inside a controlVariant element.
            var variantNames = root.Descendants()
                .Where(e => string.Equals(e.Name.LocalName, "controlVariant", StringComparison.OrdinalIgnoreCase))
                .Select(e => (XElement?)e)
                .ToHashSet();

            return root.Descendants()
                .Where(e => string.Equals(e.Name.LocalName, "previewFlag", StringComparison.OrdinalIgnoreCase))
                .Where(e => !e.Ancestors().Any(a => variantNames.Contains(a)))
                .Select(e => e.Attribute("name")?.Value ?? string.Empty)
                .Where(f => !string.IsNullOrWhiteSpace(f))
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .ToList();
        }
        catch { return Array.Empty<string>(); }
    }

    /// <summary>
    /// Returns preview flag names required by each controlVariant in the template XML.
    /// Key = variant name, Value = list of required preview flag names.
    /// </summary>
    private static IReadOnlyDictionary<string, IReadOnlyList<string>> ParseVariantPreviewFlagsFromTemplateXml(string templateXml)
    {
        if (string.IsNullOrWhiteSpace(templateXml))
            return new Dictionary<string, IReadOnlyList<string>>(StringComparer.OrdinalIgnoreCase);
        try
        {
            var root = XDocument.Parse(templateXml).Root;
            if (root is null)
                return new Dictionary<string, IReadOnlyList<string>>(StringComparer.OrdinalIgnoreCase);

            var result = new Dictionary<string, IReadOnlyList<string>>(StringComparer.OrdinalIgnoreCase);
            foreach (var variant in root.Descendants()
                         .Where(e => string.Equals(e.Name.LocalName, "controlVariant", StringComparison.OrdinalIgnoreCase)))
            {
                var vName = variant.Attribute("name")?.Value ?? string.Empty;
                if (string.IsNullOrWhiteSpace(vName)) continue;
                var flags = variant.Descendants()
                    .Where(e => string.Equals(e.Name.LocalName, "previewFlag", StringComparison.OrdinalIgnoreCase))
                    .Select(e => e.Attribute("name")?.Value ?? string.Empty)
                    .Where(f => !string.IsNullOrWhiteSpace(f))
                    .Distinct(StringComparer.OrdinalIgnoreCase)
                    .ToList();
                if (flags.Count > 0)
                    result[vName] = flags;
            }
            return result;
        }
        catch { return new Dictionary<string, IReadOnlyList<string>>(StringComparer.OrdinalIgnoreCase); }
    }

    /// <summary>
    /// Builds the list of AppPreviewFlagsMap requirements derived from a template's
    /// insertMetadata categories and controlVariant previewFlag declarations.
    /// </summary>
    private static IReadOnlyList<TemplateAppFlagRequirement> BuildAppFlagRequirements(string templateXml)
    {
        var reqs = new List<TemplateAppFlagRequirement>();
        if (string.IsNullOrWhiteSpace(templateXml)) return reqs;

        var seen = new HashSet<(string FlagName, bool RequiredValue)>();
        void AddUnique(TemplateAppFlagRequirement req)
        {
            if (seen.Add((req.FlagName, req.RequiredValue)))
                reqs.Add(req);
        }

        var categories = ParseInsertCategoriesFromTemplateXml(templateXml);

        // ClassicControls controls that also appear in the "Display" insert category (e.g. Label)
        // do NOT have a Fluent V9 replacement and work regardless of the classiccontrols flag.
        // Only controls that are ClassicControls but NOT in Display are replaced by Fluent V9 PCF
        // equivalents when classiccontrols=false (e.g. Button, Checkbox, Dropdown, Radio, ...).
        if (categories.Contains("ClassicControls", StringComparer.OrdinalIgnoreCase)
            && !categories.Contains("Display", StringComparer.OrdinalIgnoreCase))
        {
            AddUnique(new TemplateAppFlagRequirement(
                "classiccontrols", true,
                "Classic Control (Input) -- when classiccontrols=false (the default), Studio may replace this control with a " +
                "Fluent V9 PCF equivalent that has different/fewer properties (e.g. button loses Fill, Color, Size, RadiusXxx). " +
                "Set AppPreviewFlagsMap.classiccontrols=true to use the classic property set, or switch to a control that " +
                "has no Fluent V9 replacement (e.g. use Label instead of Button for fully-styled interactive elements)."));
        }

        if (categories.Contains("FluentV9", StringComparer.OrdinalIgnoreCase))
        {
            AddUnique(new TemplateAppFlagRequirement(
                "fluentv9controls", true,
                "Fluent V9 Control -- only available when fluentv9controls=true (default true in modern apps)."));
        }

        // Template-level (root) requiredConditions -- apply regardless of variant.
        var rootFlags = ParseRootPreviewFlagsFromTemplateXml(templateXml);
        foreach (var flagName in rootFlags)
        {
            var appFlagKey = MapPreviewFlagNameToAppFlag(flagName);
            if (appFlagKey is not null)
            {
                AddUnique(new TemplateAppFlagRequirement(
                    appFlagKey, true,
                    $"Template-level required flag '{flagName}' " +
                    $"(AppPreviewFlagsMap.{appFlagKey}=true)."));
            }
        }

        // Variant-level requiredConditions.
        var variantFlags = ParseVariantPreviewFlagsFromTemplateXml(templateXml);
        foreach (var (variantName, flags) in variantFlags.OrderBy(kv => kv.Key, StringComparer.OrdinalIgnoreCase))
        {
            foreach (var flagName in flags)
            {
                var appFlagKey = MapPreviewFlagNameToAppFlag(flagName);
                if (appFlagKey is not null)
                {
                    AddUnique(new TemplateAppFlagRequirement(
                        appFlagKey, true,
                        $"Variant '{variantName}' requires preview flag '{flagName}' " +
                        $"(AppPreviewFlagsMap.{appFlagKey}=true)."));
                }
            }
        }

        return reqs;
    }

    /// <summary>Maps a previewFlag/@name value to the corresponding AppPreviewFlagsMap key.</summary>
    private static string? MapPreviewFlagNameToAppFlag(string previewFlagName) =>
        previewFlagName switch
        {
            "FluentV9ControlsPreview" => "fluentv9controls",
            _ => null
        };

    /// <summary>Reads AppPreviewFlagsMap from the app's Properties.json entries, falling back to defaults when missing.</summary>
    private static IReadOnlyDictionary<string, bool> ReadAppPreviewFlagsMap(Dictionary<string, byte[]> entries)
    {
        // Build defaults first (matches what NormalizePropertiesMetadata applies to new apps).
        var defaults = new Dictionary<string, bool>(StringComparer.OrdinalIgnoreCase);
        foreach (var (k, v) in BuildDefaultAppPreviewFlagsMap())
        {
            if (v is JsonValue jv && (jv.GetValueKind() == JsonValueKind.True || jv.GetValueKind() == JsonValueKind.False))
                defaults[k] = jv.GetValue<bool>();
        }

        if (!entries.TryGetValue("Properties.json", out var bytes))
            return defaults;
        try
        {
            var root = JsonNode.Parse(Encoding.UTF8.GetString(bytes))?.AsObject();
            var flagsNode = root?["AppPreviewFlagsMap"]?.AsObject();
            if (flagsNode is null)
                return defaults;

            // Overlay explicit values from Properties.json on top of the defaults.
            var result = new Dictionary<string, bool>(defaults, StringComparer.OrdinalIgnoreCase);
            foreach (var (key, value) in flagsNode)
            {
                if (value is not null && (value.GetValueKind() == JsonValueKind.True || value.GetValueKind() == JsonValueKind.False))
                    result[key] = value.GetValue<bool>();
            }
            return result;
        }
        catch { return defaults; }
    }

    /// <summary>
    /// Builds a map of embedded template name to AppPreviewFlagsMap requirements
    /// derived from the embedded All controls.msapp template XMLs.
    /// </summary>
    private static IReadOnlyDictionary<string, IReadOnlyList<TemplateAppFlagRequirement>> BuildTemplateFlagRequirementsMap()
    {
        ReadEmbeddedAllControlsEntries(out var embeddedEntries);
        var templates = ReadUsedTemplates(embeddedEntries);
        return templates.ToDictionary(
            t => t.Name,
            t => BuildAppFlagRequirements(t.TemplateXml),
            StringComparer.OrdinalIgnoreCase);
    }

    /// <summary>
    /// Throws <see cref="InvalidDataException"/> when a control's template has AppPreviewFlagsMap
    /// requirements that conflict with the current app's flag settings.
    /// </summary>
    private static void EnforceAppFlagRequirements(
        JsonObject node,
        IReadOnlyDictionary<string, bool> appFlags,
        IReadOnlyDictionary<string, IReadOnlyList<TemplateAppFlagRequirement>> flagRequirementsMap)
    {
        var templateName = node["Template"]?["Name"]?.GetValue<string>() ?? string.Empty;
        var templateVersion = node["Template"]?["Version"]?.GetValue<string>() ?? string.Empty;
        if (string.IsNullOrWhiteSpace(templateName)) return;
        if (string.Equals(templateName, "screen", StringComparison.OrdinalIgnoreCase)) return;

        if (!flagRequirementsMap.TryGetValue(templateName, out var requirements) || requirements.Count == 0)
            return;

        var controlName = node["Name"]?.GetValue<string>() ?? "<unnamed>";
        foreach (var req in requirements)
        {
            if (appFlags.TryGetValue(req.FlagName, out var actualValue) && actualValue != req.RequiredValue)
            {
                throw new InvalidDataException(
                    $"Control '{controlName}' uses '{templateName}@{templateVersion}': " +
                    $"AppPreviewFlagsMap.{req.FlagName}={req.RequiredValue} required but is {actualValue}. " +
                    req.Reason);
            }
        }
    }

    private static IReadOnlyList<EmbeddedTemplateProperty> ParseTemplateProperties(string templateXml)
    {
        if (string.IsNullOrWhiteSpace(templateXml))
        {
            return Array.Empty<EmbeddedTemplateProperty>();
        }

        try
        {
            var root = XDocument.Parse(templateXml).Root;
            if (root is null)
            {
                return Array.Empty<EmbeddedTemplateProperty>();
            }

            var seen = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            var props = new List<EmbeddedTemplateProperty>();
            foreach (var prop in root.Descendants().Where(e => e.Name.LocalName is "property" or "includeProperty"))
            {
                var name = prop.Attribute("name")?.Value;
                if (string.IsNullOrWhiteSpace(name) || !seen.Add(name))
                {
                    continue;
                }

                var defaultValue = prop.Attribute("defaultValue")?.Value ?? string.Empty;
                var datatype = prop.Attribute("datatype")?.Value ?? string.Empty;
                defaultValue = NormalizeReservedEnumTokenForDatatype(defaultValue, datatype);
                props.Add(new EmbeddedTemplateProperty(name, defaultValue));
            }

            return props;
        }
        catch
        {
            return Array.Empty<EmbeddedTemplateProperty>();
        }
    }

    private static HashSet<string> ParseTemplatePropertyNames(string templateXml)
    {
        var names = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        if (string.IsNullOrWhiteSpace(templateXml))
        {
            return names;
        }

        try
        {
            var root = XDocument.Parse(templateXml).Root;
            if (root is null)
            {
                return names;
            }

            foreach (var prop in root.Descendants().Where(e => e.Name.LocalName is "property" or "includeProperty" or "overrideProperty"))
            {
                var name = prop.Attribute("name")?.Value;
                if (!string.IsNullOrWhiteSpace(name))
                {
                    names.Add(name);
                }
            }
        }
        catch
        {
            // Ignore malformed template xml.
        }

        return names;
    }

    private static HashSet<string> ParseRequiredYamlKeywords(string templateXml)
    {
        var keywords = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        if (string.IsNullOrWhiteSpace(templateXml))
        {
            return keywords;
        }

        try
        {
            var root = XDocument.Parse(templateXml).Root;
            if (root is null)
            {
                return keywords;
            }

            // Many templates expose variant definitions via <variant .../> entries.
            // When present, YAML source schema requires a non-empty 'Variant' keyword.
            var hasVariantNodes = root
                .Descendants()
                .Any(e => string.Equals(e.Name.LocalName, "variant", StringComparison.OrdinalIgnoreCase));
            if (hasVariantNodes)
            {
                keywords.Add("Variant");
            }
        }
        catch
        {
            // Ignore malformed template xml.
        }

        return keywords;
    }

    private static string ParseTemplateId(string templateXml)
    {
        if (string.IsNullOrWhiteSpace(templateXml))
        {
            return string.Empty;
        }

        try
        {
            var root = XDocument.Parse(templateXml).Root;
            if (root is null)
            {
                return string.Empty;
            }

            return root.Attribute("id")?.Value
                   ?? root.Attribute(XName.Get("id", "http://openajax.org/metadata"))?.Value
                   ?? string.Empty;
        }
        catch
        {
            return string.Empty;
        }
    }

    private static string NormalizeReservedEnumTokenForDatatype(string value, string datatype)
    {
        if (string.IsNullOrWhiteSpace(value) || string.IsNullOrWhiteSpace(datatype))
        {
            return value;
        }

        var trimmed = value.Trim();
        var m = Regex.Match(trimmed, "^%([A-Za-z_][A-Za-z0-9_]*)\\.RESERVED%\\.(.+)$", RegexOptions.CultureInvariant);
        if (!m.Success)
        {
            return value;
        }

        var enumType = m.Groups[1].Value;
        var enumMember = m.Groups[2].Value.Trim();
        if (string.IsNullOrWhiteSpace(enumMember))
        {
            return value;
        }

        if (!string.Equals(enumType, datatype, StringComparison.OrdinalIgnoreCase))
        {
            return value;
        }

        return $"{enumType}.{enumMember}";
    }

    private static string NormalizeReservedEnumToken(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return value;
        }

        var trimmed = value.Trim();
        var m = Regex.Match(trimmed, "^%([A-Za-z_][A-Za-z0-9_]*)\\.RESERVED%\\.(.+)$", RegexOptions.CultureInvariant);
        if (!m.Success)
        {
            return value;
        }

        var enumType = m.Groups[1].Value;
        var enumMember = m.Groups[2].Value.Trim();
        if (string.IsNullOrWhiteSpace(enumMember))
        {
            return value;
        }

        return $"{enumType}.{enumMember}";
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

    private static string ToYamlControlName(string templateName)
    {
        if (string.IsNullOrWhiteSpace(templateName))
        {
            return templateName;
        }

        return char.ToUpperInvariant(templateName[0]) + templateName.Substring(1);
    }
}