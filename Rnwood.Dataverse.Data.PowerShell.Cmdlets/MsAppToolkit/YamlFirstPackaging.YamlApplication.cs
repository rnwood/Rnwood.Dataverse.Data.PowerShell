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
{    private static void UpsertProperties(JsonObject control, Dictionary<object, object?> yamlProps)
    {
        var rules = (control["Rules"] as JsonArray) ?? new JsonArray();
        var byProperty = rules.OfType<JsonObject>()
            .Where(r => !string.IsNullOrWhiteSpace(r["Property"]?.GetValue<string>()))
            .ToDictionary(r => r["Property"]!.GetValue<string>(), r => r, StringComparer.OrdinalIgnoreCase);

        foreach (var (k, v) in yamlProps)
        {
            var property = k?.ToString() ?? string.Empty;
            if (string.IsNullOrWhiteSpace(property))
            {
                continue;
            }

            if (!byProperty.TryGetValue(property, out var rule))
            {
                rule = new JsonObject
                {
                    ["Property"] = property,
                    ["Category"] = InferCategory(property),
                    ["RuleProviderType"] = "Unknown",
                };
                rules.Add(rule);
            }

            rule["InvariantScript"] = StripFormula(v?.ToString() ?? string.Empty);
        }

        control["Rules"] = rules;
        UpdateControlPropertyStateFromRules(control);
    }

    private static void ApplyFromYaml(
        JsonObject node,
        Dictionary<object, object?> payload,
        string parentName,
        Dictionary<string, JsonObject> allExistingNodes,
        ControlIdAllocator idAllocator,
        TemplateResolver templateResolver,
        ControlMetadataResolver metadataResolver,
        TemplatePropertyResolver propertyResolver,
        TemplateKeywordResolver keywordResolver,
        IReadOnlyDictionary<string, bool>? appFlags = null,
        IReadOnlyDictionary<string, IReadOnlyList<TemplateAppFlagRequirement>>? flagRequirementsMap = null)
    {
        if (payload.TryGetValue("Control", out var ct))
        {
            ApplyTemplateFromControlType(node, ct?.ToString() ?? string.Empty, templateResolver, metadataResolver);
        }

        // Ensure template defaults are applied even when a YAML node omits `Control`
        // (for example top-level screen nodes). This materializes expected baseline
        // properties like Width/Height/LoadingSpinner for screens.
        var currentTemplateName = node["Template"]?["Name"]?.GetValue<string>() ?? string.Empty;
        var currentTemplateId = node["Template"]?["Id"]?.GetValue<string>() ?? string.Empty;
        if (!string.IsNullOrWhiteSpace(currentTemplateName) || !string.IsNullOrWhiteSpace(currentTemplateId))
        {
            metadataResolver.ApplyTemplateDefaults(node, currentTemplateName, currentTemplateId);
        }

        ValidateRequiredKeywordsFromTemplate(node, payload, keywordResolver);

        if (payload.TryGetValue("Variant", out var variant))
        {
            var requestedVariant = variant?.ToString() ?? string.Empty;
            var existingVariant = node["VariantName"]?.GetValue<string>() ?? string.Empty;
            var templateName = node["Template"]?["Name"]?.GetValue<string>() ?? string.Empty;
            var keepExistingAliasMapping = !string.IsNullOrWhiteSpace(existingVariant)
                                           && (string.Equals(requestedVariant, "AutoLayout", StringComparison.OrdinalIgnoreCase)
                                               || string.Equals(requestedVariant, "ManualLayout", StringComparison.OrdinalIgnoreCase));

            if (!keepExistingAliasMapping)
            {
                // Resolve user-friendly alias names to the canonical variant names that the runtime expects.
                var resolvedVariant = ResolveCanonicalVariantName(templateName, requestedVariant);

                node["VariantName"] = resolvedVariant;
                EnsureGalleryLayoutRuleFromVariant(node);
            }
        }

        if (payload.TryGetValue("IsLocked", out var isLocked) && bool.TryParse(isLocked?.ToString(), out var l))
        {
            node["IsLocked"] = l;
        }

        var controlNameForValidation = node["Name"]?.GetValue<string>() ?? parentName;
        ValidateNoUnrecognizedControlKeys(payload, controlNameForValidation, parentName);

        if (payload.TryGetValue("Properties", out var propsObj) && propsObj is Dictionary<object, object?> props)
        {
            ValidatePropertiesAgainstTemplate(node, props, propertyResolver);
            if (appFlags is not null && flagRequirementsMap is not null)
            {
                EnforceAppFlagRequirements(node, appFlags, flagRequirementsMap);
            }
            ReplaceProperties(node, props, metadataResolver);
        }

        // Preserve existing style metadata for parity; avoid forcing default styles.

        if (payload.TryGetValue("Children", out var childrenObj) && childrenObj is List<object?> children)
        {
            var nodeTemplateName = node["Template"]?["Name"]?.GetValue<string>() ?? string.Empty;

            var existingChildren = ((node["Children"] as JsonArray) ?? new JsonArray())
                .OfType<JsonObject>()
                .ToList();

            var yamlChildNames = children
                .OfType<Dictionary<object, object?>>()
                .Where(singleton => singleton.Count == 1)
                .Select(singleton => singleton.First().Key?.ToString() ?? string.Empty)
                .Where(name => !string.IsNullOrWhiteSpace(name))
                .ToList();

            if (string.Equals(nodeTemplateName, "screen", StringComparison.OrdinalIgnoreCase)
                && existingChildren.Count > 0
                && yamlChildNames.Count > 0)
            {
                var existingChildNames = existingChildren
                    .Select(c => c["Name"]?.GetValue<string>() ?? string.Empty)
                    .Where(name => !string.IsNullOrWhiteSpace(name))
                    .ToHashSet(StringComparer.OrdinalIgnoreCase);

                var hasAnyOverlap = yamlChildNames.Any(existingChildNames.Contains);
                if (!hasAnyOverlap)
                {
                    // If YAML child names do not overlap at all with existing screen children,
                    // preserve the original tree to avoid destructive topology conversion.
                    node["Children"] = new JsonArray(existingChildren.Select(c => (JsonNode)c.DeepClone()).ToArray());
                    return;
                }
            }

            if (string.Equals(nodeTemplateName, "gallery", StringComparison.OrdinalIgnoreCase))
            {
                var existingGalleryTemplate = ((node["Children"] as JsonArray) ?? new JsonArray())
                    .OfType<JsonObject>()
                    .FirstOrDefault(c => string.Equals(c["Template"]?["Name"]?.GetValue<string>() ?? string.Empty, "galleryTemplate", StringComparison.OrdinalIgnoreCase)
                                         || string.Equals(c["Template"]?["Id"]?.GetValue<string>() ?? string.Empty, "http://microsoft.com/appmagic/galleryTemplate", StringComparison.OrdinalIgnoreCase));

                if (existingGalleryTemplate is not null)
                {
                    var existingGalleryTemplateName = existingGalleryTemplate["Name"]?.GetValue<string>() ?? string.Empty;
                    if (!string.IsNullOrWhiteSpace(existingGalleryTemplateName))
                    {
                        node["__PreservedGalleryTemplateName"] = existingGalleryTemplateName;
                    }

                    var existingGalleryTemplateControlUniqueId = existingGalleryTemplate["ControlUniqueId"]?.GetValue<string>() ?? string.Empty;
                    if (!string.IsNullOrWhiteSpace(existingGalleryTemplateControlUniqueId))
                    {
                        node["__PreservedGalleryTemplateControlUniqueId"] = existingGalleryTemplateControlUniqueId;
                    }

                    if (existingGalleryTemplate["PublishOrderIndex"] is not null)
                    {
                        node["__PreservedGalleryTemplatePublishOrderIndex"] = existingGalleryTemplate["PublishOrderIndex"]!.DeepClone();
                    }
                }
            }

            var childrenArray = new JsonArray();
            var index = 0;
            foreach (var item in children)
            {
                if (item is not Dictionary<object, object?> singleton || singleton.Count != 1)
                {
                    continue;
                }

                var kv = singleton.First();
                var childName = kv.Key?.ToString() ?? string.Empty;
                if (string.IsNullOrWhiteSpace(childName) || kv.Value is not Dictionary<object, object?> childPayload)
                {
                    continue;
                }

                if (!childPayload.ContainsKey("Control"))
                {
                    throw new InvalidDataException($"Control '{childName}' is missing required 'Control' type in YAML.");
                }

                JsonObject child;
                JsonObject? existingNode = null;
                if (allExistingNodes.TryGetValue(childName, out var existing))
                {
                    child = (JsonObject)existing.DeepClone();
                    existingNode = existing;
                }
                else
                {
                    child = CreateDefaultControl(childName);
                }

                child["Name"] = childName;
                child["Parent"] = parentName;
                if (existingNode is null || child["Index"] is null)
                {
                    child["Index"] = index;
                }

                if (existingNode is null || child["PublishOrderIndex"] is null)
                {
                    child["PublishOrderIndex"] = child["Index"]?.GetValue<int>() ?? index;
                }
                child["ControlUniqueId"] = idAllocator.GetOrCreate(childName, existing: allExistingNodes.TryGetValue(childName, out var old) ? old : null);

                ApplyFromYaml(child, childPayload, childName, allExistingNodes, idAllocator, templateResolver, metadataResolver, propertyResolver, keywordResolver, appFlags, flagRequirementsMap);
                childrenArray.Add(child);
                index++;
            }

            node["Children"] = childrenArray;
        }
        else
        {
            node["Children"] ??= new JsonArray();
        }
    }

    private static void ValidateNoUnrecognizedControlKeys(
        Dictionary<object, object?> payload,
        string controlName,
        string parentName)
    {
        var invalidKeys = payload
            .Select(kv => kv.Key?.ToString() ?? string.Empty)
            .Where(k => !string.IsNullOrWhiteSpace(k) && !StructuralYamlControlKeys.Contains(k))
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .OrderBy(k => k, StringComparer.OrdinalIgnoreCase)
            .ToList();

        if (invalidKeys.Count == 0)
        {
            return;
        }

        var controlPath = string.IsNullOrWhiteSpace(parentName) ? controlName : $"{parentName}/{controlName}";
        throw new InvalidDataException(
            $"Unrecognized control-level keys for '{controlPath}': {string.Join(", ", invalidKeys)}. " +
            "Control members must use only: Control, Variant, IsLocked, Properties, Children. " +
            "If these are control properties, place them under 'Properties:' with the correct indentation.");
    }

    private static void ApplyTemplateFromControlType(JsonObject node, string controlType, TemplateResolver templateResolver, ControlMetadataResolver metadataResolver)
    {
        if (!templateResolver.TryResolve(controlType, out var resolved))
        {
            throw new InvalidDataException($"Could not resolve a template for YAML control type '{controlType}'. Add a matching control template in 'All controls.msapp' (or ensure this app already references it).");
        }

        var oldTemplateId = node["Template"]?["Id"]?.GetValue<string>() ?? string.Empty;
        var oldTemplateName = node["Template"]?["Name"]?.GetValue<string>() ?? string.Empty;
        var templateChanged = !string.Equals(oldTemplateId, resolved.TemplateId, StringComparison.OrdinalIgnoreCase)
                              || !string.Equals(oldTemplateName, resolved.TemplateName, StringComparison.OrdinalIgnoreCase);

        JsonObject template;
        if (resolved.TemplatePrototype is not null)
        {
            template = (JsonObject)resolved.TemplatePrototype.DeepClone();
        }
        else
        {
            template = (node["Template"] as JsonObject) ?? new JsonObject();
        }

        template["Id"] = resolved.TemplateId;
        template["Name"] = resolved.TemplateName;
        template["Version"] = resolved.Version;
        node["Template"] = template;

        if (templateChanged)
        {
            // Switching template families (for example legacy text -> modern PCF text input)
            // should discard stale rule/state payloads so the target template defaults materialize.
            node["Rules"] = new JsonArray();
            node["ControlPropertyState"] = new JsonArray();
            node.Remove("DynamicProperties");
            node.Remove("HasDynamicProperties");
        }

        metadataResolver.ApplyTemplateDefaults(node, resolved.TemplateName, resolved.TemplateId);
    }

    private static readonly Lazy<IReadOnlyDictionary<string, IReadOnlyList<string>>> EmbeddedVariantsByTemplate
        = new(() =>
        {
            ReadEmbeddedAllControlsEntries(out var embeddedEntries);
            return CollectVariantsByTemplateName(embeddedEntries);
        });

    private static string NormalizeVariantToken(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return string.Empty;
        }

        var sb = new StringBuilder(value.Length);
        foreach (var ch in value)
        {
            if (char.IsLetterOrDigit(ch))
            {
                sb.Append(char.ToLowerInvariant(ch));
            }
        }

        return sb.ToString();
    }

    private static string ResolveCanonicalVariantName(string templateName, string requestedVariant)
    {
        if (string.IsNullOrWhiteSpace(requestedVariant))
        {
            return string.Empty;
        }

        var variantsByTemplate = EmbeddedVariantsByTemplate.Value;
        if (!string.IsNullOrWhiteSpace(templateName)
            && variantsByTemplate.TryGetValue(templateName, out var canonicalVariants)
            && canonicalVariants.Count > 0)
        {
            var exact = canonicalVariants.FirstOrDefault(v => string.Equals(v, requestedVariant, StringComparison.OrdinalIgnoreCase));
            if (!string.IsNullOrWhiteSpace(exact))
            {
                return exact;
            }

            var requestedToken = NormalizeVariantToken(requestedVariant);
            if (!string.IsNullOrWhiteSpace(requestedToken))
            {
                var normalized = canonicalVariants
                    .Select(v => new { Value = v, Token = NormalizeVariantToken(v) })
                    .ToList();

                // Token-containment matching keeps this template-driven while allowing friendly aliases
                // like Vertical/Horizontal/FlexibleHeight/AutoLayout/ManualLayout.
                var contains = normalized
                    .Where(v => v.Token.IndexOf(requestedToken, StringComparison.OrdinalIgnoreCase) >= 0)
                    .OrderBy(v => v.Token.Length)
                    .ThenBy(v => v.Value, StringComparer.OrdinalIgnoreCase)
                    .FirstOrDefault();
                if (contains is not null)
                {
                    return contains.Value;
                }

                var reverseContains = normalized
                    .Where(v => requestedToken.IndexOf(v.Token, StringComparison.OrdinalIgnoreCase) >= 0)
                    .OrderByDescending(v => v.Token.Length)
                    .ThenBy(v => v.Value, StringComparer.OrdinalIgnoreCase)
                    .FirstOrDefault();
                if (reverseContains is not null)
                {
                    return reverseContains.Value;
                }

                var synonyms = new Dictionary<string, string[]>(StringComparer.OrdinalIgnoreCase)
                {
                    { "vertical", new string[] { "vertical" } },
                    { "horizontal", new string[] { "horizontal" } },
                    { "autolayout", new string[] { "autolayout" } },
                    { "manuallayout", new string[] { "manuallayout" } },
                    { "flexibleheight", new string[] { "variabletemplateheight", "flexibleheight" } },
                    { "variabletemplateheight", new string[] { "variabletemplateheight", "flexibleheight" } },
                };

                if (synonyms.TryGetValue(requestedToken, out var tokens))
                {
                    foreach (var t in tokens)
                    {
                        var match = normalized
                            .Where(v => v.Token.IndexOf(t, StringComparison.OrdinalIgnoreCase) >= 0)
                            .OrderBy(v => v.Token.Length)
                            .ThenBy(v => v.Value, StringComparer.OrdinalIgnoreCase)
                            .FirstOrDefault();
                        if (match is not null)
                        {
                            return match.Value;
                        }
                    }
                }
            }
        }

        return requestedVariant;
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
        UpdateControlPropertyStateFromRules(node);
    }

    private static void ReplaceProperties(JsonObject node, Dictionary<object, object?> props, ControlMetadataResolver metadataResolver)
    {
        var existingRulesArray = ((node["Rules"] as JsonArray) ?? new JsonArray())
            .OfType<JsonObject>()
            .Where(r => !string.IsNullOrWhiteSpace(r["Property"]?.GetValue<string>()))
            .ToList();

        var orderedRules = new List<JsonObject>();
        var byProperty = new Dictionary<string, JsonObject>(StringComparer.OrdinalIgnoreCase);
        foreach (var existing in existingRulesArray)
        {
            var clone = (JsonObject)existing.DeepClone();
            var property = clone["Property"]?.GetValue<string>() ?? string.Empty;
            if (string.IsNullOrWhiteSpace(property) || byProperty.ContainsKey(property))
            {
                continue;
            }

            orderedRules.Add(clone);
            byProperty[property] = clone;
        }

        foreach (var (k, v) in props)
        {
            var property = NormalizeSourcePropertyName(node, k?.ToString() ?? string.Empty);
            if (string.IsNullOrWhiteSpace(property))
            {
                continue;
            }

            var inferredCategory = metadataResolver.ResolveCategory(node, property) ?? InferCategory(property);
            var category = inferredCategory;
            JsonObject? oldRule = null;
            if (byProperty.TryGetValue(property, out oldRule))
            {
                var oldCategory = oldRule["Category"]?.GetValue<string>() ?? string.Empty;
                category = string.Equals(oldCategory, "Data", StringComparison.OrdinalIgnoreCase)
                           && !string.Equals(inferredCategory, "Data", StringComparison.OrdinalIgnoreCase)
                    ? inferredCategory
                    : (string.IsNullOrWhiteSpace(oldCategory) ? inferredCategory : oldCategory);

                oldRule["Category"] = category;
                oldRule["InvariantScript"] = StripFormula(v?.ToString() ?? string.Empty);
                oldRule["RuleProviderType"] = oldRule["RuleProviderType"]?.GetValue<string>() ?? "Unknown";
                continue;
            }

            var created = new JsonObject
            {
                ["Property"] = property,
                ["Category"] = category,
                ["InvariantScript"] = StripFormula(v?.ToString() ?? string.Empty),
                ["RuleProviderType"] = oldRule?["RuleProviderType"]?.GetValue<string>()
                    ?? (string.Equals(category, "Behavior", StringComparison.OrdinalIgnoreCase)
                        || string.Equals(category, "OnDemandData", StringComparison.OrdinalIgnoreCase)
                        ? "User"
                        : "Unknown"),
            };

            orderedRules.Add(created);
            byProperty[property] = created;
        }

        var rules = new JsonArray(orderedRules.Select(r => (JsonNode)r).ToArray());
        node["Rules"] = rules;
        UpdateControlPropertyStateFromRules(node);
    }

    private static string InferCategory(string property)
    {
        if (property.StartsWith("On", StringComparison.OrdinalIgnoreCase))
        {
            return "Behavior";
        }

        if (DesignProperties.Contains(property))
        {
            return "Design";
        }

        return "Data";
    }


    private static string StripFormula(string value)
    {
        return (value.Length > 0 && value[0] == '=') ? value.Substring(1) : value;
    }


    private static void ValidateRequiredKeywordsFromTemplate(
        JsonObject node,
        Dictionary<object, object?> payload,
        TemplateKeywordResolver keywordResolver)
    {
        if (!keywordResolver.TryGetRequiredKeywords(node, out var required) || required.Count == 0)
        {
            return;
        }

        var missing = new List<string>();
        foreach (var keyword in required)
        {
            var has = payload.TryGetValue(keyword, out var raw)
                      && !string.IsNullOrWhiteSpace(raw?.ToString());
            if (!has)
            {
                missing.Add(keyword);
            }
        }

        if (missing.Count == 0)
        {
            return;
        }

        var controlName = node["Name"]?.GetValue<string>()
                          ?? "<unnamed>";
        var templateName = node["Template"]?["Name"]?.GetValue<string>() ?? "<unknown-template>";
        throw new InvalidDataException(
            $"Control '{controlName}' (template '{templateName}') is missing required YAML keyword(s): {string.Join(", ", missing.OrderBy(x => x, StringComparer.OrdinalIgnoreCase))}.");
    }


    private static void ValidatePropertiesAgainstTemplate(JsonObject node, Dictionary<object, object?> props, TemplatePropertyResolver resolver)
    {
        var controlName = node["Name"]?.GetValue<string>() ?? "<unnamed>";
        var parentName = node["Parent"]?.GetValue<string>() ?? string.Empty;
        var controlPath = string.IsNullOrWhiteSpace(parentName) ? controlName : $"{parentName}/{controlName}";
        var templateNameForNode = node["Template"]?["Name"]?.GetValue<string>() ?? string.Empty;

        if (string.Equals(templateNameForNode, "screen", StringComparison.OrdinalIgnoreCase))
        {
            // Screen properties in source schema can include behavior members (for example OnVisible)
            // that are not always present in extracted template metadata.
            return;
        }

        if (!resolver.TryGetAllowedProperties(node, out var allowedProperties) || allowedProperties.Count == 0)
        {
            var templateNameMissing = node["Template"]?["Name"]?.GetValue<string>() ?? "<unknown-template>";
            var templateVersionMissing = node["Template"]?["Version"]?.GetValue<string>() ?? "<unknown-version>";

            throw new InvalidDataException(
                $"Cannot validate properties for control '{controlPath}' because template metadata is unavailable for '{templateNameMissing}@{templateVersionMissing}'. "
                + "Property validation is strict; ensure References/Templates.json contains this template definition.");
        }

        foreach (var ruleProp in ((node["Rules"] as JsonArray) ?? new JsonArray())
                     .OfType<JsonObject>()
                     .Select(r => r["Property"]?.GetValue<string>() ?? string.Empty)
                     .Where(p => !string.IsNullOrWhiteSpace(p)))
        {
            allowedProperties.Add(ruleProp);
        }

        foreach (var stateProp in ((node["ControlPropertyState"] as JsonArray) ?? new JsonArray())
                     .OfType<JsonObject>()
                     .Select(s => s["Property"]?.GetValue<string>() ?? string.Empty)
                     .Where(p => !string.IsNullOrWhiteSpace(p)))
        {
            allowedProperties.Add(stateProp);
        }

        // DynamicProperties are injected by ApplyTemplateDefaults (e.g. FillPortions, AlignInContainer
        // for auto-layout children). Reading them here keeps validation data-driven with no hardcoded names.
        foreach (var dynProp in ((node["DynamicProperties"] as JsonArray) ?? new JsonArray())
                     .OfType<JsonObject>()
                     .Select(d => d["PropertyName"]?.GetValue<string>() ?? string.Empty)
                     .Where(p => !string.IsNullOrWhiteSpace(p)))
        {
            allowedProperties.Add(dynProp);
        }

        // Modern TextInput (PCF) exposes Value/Placeholder in YAML sources.
        // Keep this compatibility shim in validation (instead of materializing hardcoded
        // baseline Rules/DynamicProperties on the control).
        if (IsModernTextInputTemplate(node))
        {
            allowedProperties.Add("Value");
            allowedProperties.Add("Placeholder");
            allowedProperties.Add("Default");
            allowedProperties.Add("HintText");
        }

        var unknown = new List<string>();
        foreach (var (k, _) in props)
        {
            var rawPropertyName = k?.ToString() ?? string.Empty;
            if (string.IsNullOrWhiteSpace(rawPropertyName))
            {
                continue;
            }

            var propertyName = NormalizeSourcePropertyName(node, rawPropertyName);
            if (!allowedProperties.Contains(propertyName))
            {
                unknown.Add(rawPropertyName);
            }
        }

        if (unknown.Count > 0)
        {
            var templateName = node["Template"]?["Name"]?.GetValue<string>() ?? "<unknown-template>";
            var templateVersion = node["Template"]?["Version"]?.GetValue<string>() ?? "<unknown-version>";

            var orderedAllowed = allowedProperties.OrderBy(x => x, StringComparer.OrdinalIgnoreCase).ToList();
            var suggestions = unknown
                .OrderBy(x => x, StringComparer.OrdinalIgnoreCase)
                .Select(u =>
                {
                    var nearest = GetNearestPropertyNames(u, orderedAllowed, maxSuggestions: 3);
                    return nearest.Count == 0
                        ? $"- {u}"
                        : $"- {u} (did you mean: {string.Join(", ", nearest)})";
                })
                .ToList();

            throw new InvalidDataException(
                $"Unknown properties for control '{controlPath}' (template '{templateName}@{templateVersion}')."
                + Environment.NewLine
                + string.Join(Environment.NewLine, suggestions)
                + Environment.NewLine
                + $"Allowed properties ({orderedAllowed.Count}): {string.Join(", ", orderedAllowed)}"
                + Environment.NewLine
                + "Tip: verify with 'template-properties <template-name> [template-version]'.");
        }
    }


    private static string NormalizeSourcePropertyName(JsonObject node, string propertyName)
    {
        var normalized = propertyName?.Trim() ?? string.Empty;
        if (string.IsNullOrWhiteSpace(normalized))
        {
            return string.Empty;
        }

        if (IsModernTextInputTemplate(node))
        {
            if (string.Equals(normalized, "Default", StringComparison.OrdinalIgnoreCase))
            {
                return "Value";
            }

            if (string.Equals(normalized, "HintText", StringComparison.OrdinalIgnoreCase))
            {
                return "Placeholder";
            }
        }

        return normalized;
    }

    private static bool IsModernTextInputTemplate(JsonObject node)
    {
        var templateName = node["Template"]?["Name"]?.GetValue<string>() ?? string.Empty;
        var templateId = node["Template"]?["Id"]?.GetValue<string>() ?? string.Empty;
        return string.Equals(templateName, "PowerApps_CoreControls_TextInputCanvas", StringComparison.OrdinalIgnoreCase)
               || templateId.IndexOf("PowerApps_CoreControls_TextInputCanvas", StringComparison.OrdinalIgnoreCase) >= 0;
    }

    private static List<string> GetNearestPropertyNames(string input, IReadOnlyList<string> candidates, int maxSuggestions)
    {
        if (string.IsNullOrWhiteSpace(input) || candidates.Count == 0 || maxSuggestions <= 0)
        {
            return new List<string>();
        }

        var normalizedInput = input.Trim();
        var scored = new List<(string Name, int Distance)>();
        foreach (var candidate in candidates)
        {
            if (string.IsNullOrWhiteSpace(candidate))
            {
                continue;
            }

            var distance = LevenshteinDistance(normalizedInput, candidate);
            scored.Add((candidate, distance));
        }

        var threshold = Math.Max(2, normalizedInput.Length / 3);
        return scored
            .OrderBy(s => s.Distance)
            .ThenBy(s => s.Name, StringComparer.OrdinalIgnoreCase)
            .Where(s => s.Distance <= threshold)
            .Take(maxSuggestions)
            .Select(s => s.Name)
            .ToList();
    }

    private static int LevenshteinDistance(string a, string b)
    {
        if (string.Equals(a, b, StringComparison.OrdinalIgnoreCase))
        {
            return 0;
        }

        if (a.Length == 0)
        {
            return b.Length;
        }

        if (b.Length == 0)
        {
            return a.Length;
        }

        var aa = a.ToLowerInvariant();
        var bb = b.ToLowerInvariant();
        var prev = new int[bb.Length + 1];
        var curr = new int[bb.Length + 1];

        for (var j = 0; j <= bb.Length; j++)
        {
            prev[j] = j;
        }

        for (var i = 1; i <= aa.Length; i++)
        {
            curr[0] = i;
            for (var j = 1; j <= bb.Length; j++)
            {
                var cost = aa[i - 1] == bb[j - 1] ? 0 : 1;
                curr[j] = Math.Min(
                    Math.Min(curr[j - 1] + 1, prev[j] + 1),
                    prev[j - 1] + cost);
            }

            (prev, curr) = (curr, prev);
        }

        return prev[bb.Length];
    }


    private static readonly HashSet<string> DesignProperties = new(StringComparer.OrdinalIgnoreCase)
    {
        "Align", "Appearance", "AutoHeight", "BorderColor", "BorderStyle", "BorderThickness", "Color",
        "DisabledBorderColor", "DisabledColor", "DisabledFill", "DisplayMode", "Fill", "FocusedBorderColor",
        "FocusedBorderThickness", "Font", "FontWeight", "Height", "HoverBorderColor", "HoverColor", "HoverFill",
        "IconBackground", "ImagePosition", "Italic", "LineHeight", "LoadingSpinner", "LoadingSpinnerColor",
        "Orientation", "PaddingBottom", "PaddingLeft", "PaddingRight", "PaddingTop", "PressedBorderColor",
        "PressedColor", "PressedFill", "Size", "Strikethrough", "TemplateSize", "Underline", "VerticalAlign",
        "Visible", "Width", "X", "Y", "ZIndex",
    };

    private static readonly HashSet<string> StructuralYamlControlKeys = new(StringComparer.OrdinalIgnoreCase)
    {
        "Control",
        "Variant",
        "IsLocked",
        "Properties",
        "Children",
    };
}