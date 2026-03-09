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
        IReadOnlyDictionary<string, IReadOnlyList<TemplateAppFlagRequirement>>? flagRequirementsMap = null,
        JsonObject? parentNode = null)
    {
        var nodeName = node["Name"]?.GetValue<string>() ?? string.Empty;
        var isExistingNode = !string.IsNullOrWhiteSpace(nodeName)
                             && allExistingNodes.ContainsKey(nodeName);

        if (payload.TryGetValue("Control", out var ct))
        {
            ApplyTemplateFromControlType(node, ct?.ToString() ?? string.Empty, templateResolver, metadataResolver, parentNode);
        }

        // Ensure template defaults are applied even when a YAML node omits `Control`
        // (for example top-level screen nodes). This materializes expected baseline
        // properties like Width/Height/LoadingSpinner for screens.
        var currentTemplateName = node["Template"]?["Name"]?.GetValue<string>() ?? string.Empty;
        var currentTemplateId = node["Template"]?["Id"]?.GetValue<string>() ?? string.Empty;
        if (!isExistingNode
            && (!string.IsNullOrWhiteSpace(currentTemplateName) || !string.IsNullOrWhiteSpace(currentTemplateId)))
        {
            metadataResolver.ApplyTemplateDefaults(node, currentTemplateName, currentTemplateId, parentNode: parentNode);
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
                // Special case: for groupcontainer AutoLayout, both "verticalAutoLayoutContainer" and
                // "horizontalAutoLayoutContainer" contain the "autolayout" token, so the generic token-
                // containment logic is ambiguous.  Disambiguate by inspecting the LayoutDirection property
                // that is present in the YAML payload before it has been applied to the node.
                string resolvedVariant;
                if (string.Equals(templateName, "groupcontainer", StringComparison.OrdinalIgnoreCase)
                    && string.Equals(requestedVariant, "autolayout", StringComparison.OrdinalIgnoreCase))
                {
                    var isVertical = payload.TryGetValue("Properties", out var propsForLayout)
                        && propsForLayout is Dictionary<object, object?> layoutProps
                        && layoutProps.TryGetValue("LayoutDirection", out var layoutDir)
                        && layoutDir?.ToString()?.IndexOf("Vertical", StringComparison.OrdinalIgnoreCase) >= 0;
                    resolvedVariant = isVertical ? "verticalAutoLayoutContainer" : "horizontalAutoLayoutContainer";
                }
                // Studio-generated Src/ YAML uses Classic-prefixed variant names for data cards
                // and forms. Map to the canonical JSON variant names used by the runtime.
                else if (string.Equals(templateName, "typedDataCard", StringComparison.OrdinalIgnoreCase))
                {
                    resolvedVariant = ResolveDataCardVariant(requestedVariant);
                }
                else if (string.Equals(templateName, "form", StringComparison.OrdinalIgnoreCase)
                         && string.Equals(requestedVariant, "Classic", StringComparison.OrdinalIgnoreCase))
                {
                    // Form "Classic" variant maps to an empty VariantName in Controls JSON.
                    resolvedVariant = string.Empty;
                }
                else
                {
                    if (isExistingNode)
                    {
                        // Preserve source-app variant verbatim for existing controls.
                        // This avoids alias churn during roundtrip (e.g. textualEditCard vs ClassicTextualEdit).
                        resolvedVariant = existingVariant;
                    }
                    else
                    {
                        resolvedVariant = ResolveCanonicalVariantName(templateName, requestedVariant);
                    }
                }

                node["VariantName"] = resolvedVariant;
                EnsureGalleryLayoutRuleFromVariant(node);
            }
        }

        if (payload.TryGetValue("IsLocked", out var isLocked) && bool.TryParse(isLocked?.ToString(), out var l))
        {
            node["IsLocked"] = l;
        }

        // Layout: stored as LayoutName in the JSON (e.g. "vertical" for Form controls).
        // Normalize to lowercase to match the canonical JSON representation used by Studio.
        if (payload.TryGetValue("Layout", out var layoutVal) && layoutVal is not null)
        {
            // Preserve source fidelity for existing controls: do not inject LayoutName where one did not exist.
            if (!isExistingNode || node["LayoutName"] is not null)
            {
                node["LayoutName"] = layoutVal.ToString().ToLowerInvariant();
            }
        }

        // MetadataKey: stored as MetaDataIDKey in JSON (e.g. "%name.ID%" for data cards, "%Form.ID%" for forms).
        // Studio-generated YAML strips the %...ID% wrapper (e.g. writes "FieldName" instead of
        // "%FieldName.ID%").  Re-wrap bare names to restore the JSON format Power Apps expects.
        if (payload.TryGetValue("MetadataKey", out var metadataKeyVal) && metadataKeyVal is not null)
        {
            // Preserve source fidelity for existing controls: do not inject MetaDataIDKey where one did not exist.
            if (!isExistingNode || node["MetaDataIDKey"] is not null)
            {
                var rawKey = metadataKeyVal.ToString();
                // If the value doesn't already have the %...ID% wrapper, add it.
                if (!string.IsNullOrWhiteSpace(rawKey) && !rawKey.StartsWith("%"))
                {
                    rawKey = $"%{rawKey}.ID%";
                }
                node["MetaDataIDKey"] = rawKey;
            }
        }

        // Infer MetaDataIDKey for form/data-card templates when not explicitly provided in YAML.
        // Studio-generated Src/ YAML omits MetadataKey on form and data card parent controls;
        // the pack process infers it from the template type and the DataField property.
        if (!isExistingNode && string.IsNullOrWhiteSpace(node["MetaDataIDKey"]?.GetValue<string>() ?? string.Empty))
        {
            var templateNameForMetadata = (node["Template"] as JsonObject)?["Name"]?.GetValue<string>() ?? string.Empty;

            if (string.Equals(templateNameForMetadata, "form", StringComparison.OrdinalIgnoreCase))
            {
                node["MetaDataIDKey"] = "%Form.ID%";
            }
            else if (string.Equals(templateNameForMetadata, "typedDataCard", StringComparison.OrdinalIgnoreCase))
            {
                // Extract field name from DataField property: ="name" → %name.ID%
                if (payload.TryGetValue("Properties", out var propsForCard)
                    && propsForCard is Dictionary<object, object?> cardProps
                    && cardProps.TryGetValue("DataField", out var dataFieldVal))
                {
                    var dataField = StripFormula(dataFieldVal?.ToString() ?? string.Empty).Trim('"');
                    if (!string.IsNullOrWhiteSpace(dataField))
                    {
                        node["MetaDataIDKey"] = $"%{dataField}.ID%";
                    }
                }
            }
        }

        // IsFromScreenLayout and IsAutoGenerated: read from YAML if authored (unlikely, as
        // pa.schema.yaml does not list these for Control-instance), then fall back to
        // inferring from MetaDataIDKey or well-known auto-generated template names.
        // Any control with a non-empty MetaDataIDKey is an auto-generated layout/form control
        // (e.g. "%Form.ID%", "%name.ID%", "ErrorMessage", "FieldName", "FieldValue") and should
        // have IsFromScreenLayout=true, IsAutoGenerated=true.
        // Additionally, the "galleryTemplate" template is always auto-generated as the row
        // template container for Gallery controls and has an empty MetaDataIDKey by design.
        if (payload.TryGetValue("IsFromScreenLayout", out var isFromScreenLayoutVal) && bool.TryParse(isFromScreenLayoutVal?.ToString(), out var fsl))
        {
            node["IsFromScreenLayout"] = fsl;
        }
        else if (!isExistingNode)
        {
            var metadataKeyForInference = node["MetaDataIDKey"]?.GetValue<string>() ?? string.Empty;
            var templateNameForInference = (node["Template"] as JsonObject)?["Name"]?.GetValue<string>() ?? string.Empty;
            if (!string.IsNullOrEmpty(metadataKeyForInference)
                || string.Equals(templateNameForInference, "galleryTemplate", StringComparison.OrdinalIgnoreCase))
            {
                node["IsFromScreenLayout"] = true;
            }
        }

        if (payload.TryGetValue("IsAutoGenerated", out var isAutoGeneratedVal) && bool.TryParse(isAutoGeneratedVal?.ToString(), out var iag))
        {
            node["IsAutoGenerated"] = iag;
        }
        else if (!isExistingNode)
        {
            var metadataKeyForAGInference = node["MetaDataIDKey"]?.GetValue<string>() ?? string.Empty;
            var templateNameForAGInference = (node["Template"] as JsonObject)?["Name"]?.GetValue<string>() ?? string.Empty;
            if (!string.IsNullOrEmpty(metadataKeyForAGInference)
                || string.Equals(templateNameForAGInference, "galleryTemplate", StringComparison.OrdinalIgnoreCase))
            {
                node["IsAutoGenerated"] = true;
            }
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
            ReplaceProperties(node, props, metadataResolver, preserveExistingRulesOnly: isExistingNode);

            // Prune rules added from YAML that Studio includes in Src/ YAML but
            // omits from Controls JSON (properties at default values).  When a
            // per-control-name prototype exists (reference msapp), prune to that
            // set; otherwise fall back to YAML-only properties so that template/
            // style defaults are not materialised for fresh-from-YAML controls.
            if (!isExistingNode)
            {
                var yamlPropertyNames = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
                foreach (var (k, _) in props)
                {
                    var p = NormalizeSourcePropertyName(node, k?.ToString() ?? string.Empty);
                    if (!string.IsNullOrWhiteSpace(p))
                    {
                        yamlPropertyNames.Add(p);
                    }
                }

                metadataResolver.PruneExcessRulesToPerControlPrototype(node, yamlPropertyNames);
                UpdateControlPropertyStateFromRules(node);
            }
        }

        // Apply per-control metadata defaults from the reference msapp.  These fill in
        // fields that the Src/ YAML doesn't carry (MetaDataIDKey, VariantName, StyleName,
        // IsFromScreenLayout, IsAutoGenerated, LayoutName).  YAML-specified values
        // (applied above) take precedence because ApplyPerControlMetadataDefaults only
        // fills empty/unset fields.
        if (!isExistingNode)
        {
            metadataResolver.ApplyPerControlMetadataDefaults(node);
        }

        // Preserve existing style metadata for parity; avoid forcing default styles.

        if (payload.TryGetValue("Children", out var childrenObj) && childrenObj is List<object?> children)
        {
            var nodeTemplateName = node["Template"]?["Name"]?.GetValue<string>() ?? string.Empty;

            var existingChildren = ((node["Children"] as JsonArray) ?? new JsonArray())
                .OfType<JsonObject>()
                .ToList();
            var existingChildrenByName = existingChildren
                .Where(c => !string.IsNullOrWhiteSpace(c["Name"]?.GetValue<string>() ?? string.Empty))
                .ToDictionary(
                    c => c["Name"]!.GetValue<string>(),
                    c => c,
                    StringComparer.OrdinalIgnoreCase);

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
                    node["__PreservedGalleryTemplateNode"] = existingGalleryTemplate.DeepClone();

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

                // IMPORTANT: resolve by current parent first to avoid cross-parent name collisions
                // (for example many controls named Title1/Subtitle1 across the app).
                if (existingChildrenByName.TryGetValue(childName, out var existingSibling))
                {
                    child = (JsonObject)existingSibling.DeepClone();
                    existingNode = existingSibling;
                }
                else if (allExistingNodes.TryGetValue(childName, out var existing))
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

                ApplyFromYaml(child, childPayload, childName, allExistingNodes, idAllocator, templateResolver, metadataResolver, propertyResolver, keywordResolver, appFlags, flagRequirementsMap, parentNode: node);
                childrenArray.Add(child);
                index++;
            }

            if (isExistingNode && existingChildren.Count > 0)
            {
                // Preserve existing sibling order to avoid visual layout drift when YAML order differs
                // from the control JSON order in source apps.
                var builtByName = childrenArray
                    .OfType<JsonObject>()
                    .Where(c => !string.IsNullOrWhiteSpace(c["Name"]?.GetValue<string>() ?? string.Empty))
                    .ToDictionary(c => c["Name"]!.GetValue<string>(), c => c, StringComparer.OrdinalIgnoreCase);

                var reordered = new JsonArray();
                var added = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

                foreach (var existingChild in existingChildren)
                {
                    var existingName = existingChild["Name"]?.GetValue<string>() ?? string.Empty;
                    if (string.IsNullOrWhiteSpace(existingName))
                    {
                        continue;
                    }

                    if (builtByName.TryGetValue(existingName, out var built) && added.Add(existingName))
                    {
                        reordered.Add((JsonNode)built.DeepClone());
                    }
                }

                foreach (var built in childrenArray.OfType<JsonObject>())
                {
                    var name = built["Name"]?.GetValue<string>() ?? string.Empty;
                    if (string.IsNullOrWhiteSpace(name) || added.Add(name))
                    {
                        reordered.Add((JsonNode)built.DeepClone());
                    }
                }

                childrenArray = reordered;
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
            "Control members must use only: Control, Variant, IsLocked, Layout, MetadataKey, IsFromScreenLayout, IsAutoGenerated, Properties, Children. " +
            "If these are control properties, place them under 'Properties:' with the correct indentation.");
    }

    private static void ApplyTemplateFromControlType(JsonObject node, string controlType, TemplateResolver templateResolver, ControlMetadataResolver metadataResolver, JsonObject? parentNode = null)
    {
        if (!templateResolver.TryResolve(controlType, out var resolved))
        {
            // Auto-register a synthetic placeholder so this control can be packed without a template
            // catalog entry. Property validation will be skipped for synthetic-ID templates.
            templateResolver.RegisterSyntheticTemplate(controlType);
            if (!templateResolver.TryResolve(controlType, out resolved))
            {
                throw new InvalidDataException($"Could not resolve a template for YAML control type '{controlType}'. Add a matching control template in 'All controls.msapp' (or ensure this app already references it).");
            }
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

        if (templateChanged)
        {
            metadataResolver.ApplyTemplateDefaults(node, resolved.TemplateName, resolved.TemplateId, parentNode: parentNode);
        }
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

    /// <summary>
    /// Maps Studio-friendly YAML variant names for typedDataCard to the canonical
    /// JSON VariantName values used by the runtime.
    /// </summary>
    private static string ResolveDataCardVariant(string requestedVariant)
    {
        // Studio-generated Src/ YAML uses "Classic" prefix:
        //   ClassicTextualEdit  → textualEditCard
        //   ClassicComboBoxEdit → comboBoxEditCard
        //   ClassicToggleEdit   → toggleEditCard
        //   ClassicDateEdit     → dateEditCard
        // When the canonical name is already used, pass it through.
        var mapping = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
        {
            { "ClassicTextualEdit", "textualEditCard" },
            { "ClassicComboBoxEdit", "comboBoxEditCard" },
            { "ClassicToggleEdit", "toggleEditCard" },
            { "ClassicDateEdit", "dateEditCard" },
            { "ClassicLookupEdit", "lookUpEditCard" },
            { "ClassicMultilineEdit", "multiLineEditCard" },
        };

        return mapping.TryGetValue(requestedVariant, out var canonical) ? canonical : requestedVariant;
    }

    private static bool AreSemanticallyEquivalentVariants(string left, string right)
    {
        var a = NormalizeVariantToken(left);
        var b = NormalizeVariantToken(right);
        if (string.IsNullOrWhiteSpace(a) || string.IsNullOrWhiteSpace(b))
        {
            return string.Equals(a, b, StringComparison.OrdinalIgnoreCase);
        }

        if (string.Equals(a, b, StringComparison.OrdinalIgnoreCase))
        {
            return true;
        }

        var aNoClassic = Regex.Replace(a, "classic", string.Empty, RegexOptions.IgnoreCase | RegexOptions.CultureInvariant);
        var bNoClassic = Regex.Replace(b, "classic", string.Empty, RegexOptions.IgnoreCase | RegexOptions.CultureInvariant);
        return string.Equals(aNoClassic, bNoClassic, StringComparison.OrdinalIgnoreCase);
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

    private static void ReplaceProperties(JsonObject node, Dictionary<object, object?> props, ControlMetadataResolver metadataResolver, bool preserveExistingRulesOnly)
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

            if (preserveExistingRulesOnly)
            {
                // For existing controls, keep original rule surface to preserve source fidelity.
                continue;
            }

            var created = new JsonObject
            {
                ["Property"] = property,
                ["Category"] = category,
                ["InvariantScript"] = StripFormula(v?.ToString() ?? string.Empty),
                ["RuleProviderType"] = "Unknown",
            };

            orderedRules.Add(created);
            byProperty[property] = created;
        }

        // Template default rules (applied by ApplyTemplateDefaults before this method)
        // are preserved for new controls.  Studio materialises all template defaults as
        // explicit rules in Controls/*.json; only the Src/ YAML omits them.

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
        var templateNameForNode = node["Template"]?["Name"]?.GetValue<string>() ?? string.Empty;
        if (string.Equals(templateNameForNode, "screen", StringComparison.OrdinalIgnoreCase))
        {
            // Screen YAML commonly omits Variant while still round-tripping from exported apps.
            // Keep required-keyword validation strict for other controls.
            return;
        }

        if (!keywordResolver.TryGetRequiredKeywords(node, out var required) || required.Count == 0)
        {
            return;
        }

        var missing = new List<string>();
        foreach (var keyword in required)
        {
            var has = payload.TryGetValue(keyword, out var raw)
                      && !string.IsNullOrWhiteSpace(raw?.ToString());

            if (!has && string.Equals(keyword, "Variant", StringComparison.OrdinalIgnoreCase))
            {
                var existingVariant = node["VariantName"]?.GetValue<string>() ?? string.Empty;
                has = !string.IsNullOrWhiteSpace(existingVariant);

                // Many real exported apps omit Variant for controls where the runtime applies
                // an implicit/default variant. Keep validation strict for non-Variant keywords.
                if (!has)
                {
                    continue;
                }
            }

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

        var templateIdForNode = node["Template"]?["Id"]?.GetValue<string>() ?? string.Empty;
        if (templateIdForNode.StartsWith("http://microsoft.com/appmagic/synthetic/", StringComparison.OrdinalIgnoreCase))
        {
            // Synthetically-registered template (control type not in embedded catalog).
            // No property metadata is available, so property validation is skipped.
            return;
        }

        if (string.Equals(templateNameForNode, "galleryTemplate", StringComparison.OrdinalIgnoreCase))
        {
            // galleryTemplate is an auto-generated row-template container for Gallery controls.
            // It is not registered in References/Templates.json and has no standalone template
            // metadata. Its properties come from the gallery's DynamicControlTemplate at runtime.
            // Skip strict property validation for this template.
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
        // Position, size, visibility
        "Align", "AutoHeight", "Height", "Visible", "Width", "WidthFit", "X", "Y", "ZIndex",
        // Colors and fills
        "BorderColor", "BorderStyle", "BorderThickness", "ChevronBackground", "ChevronDisabledBackground",
        "ChevronDisabledFill", "ChevronFill", "ChevronHoverBackground", "ChevronHoverFill", "Color",
        "DisabledBorderColor", "DisabledColor", "DisabledFill", "Fill", "FocusedBorderColor",
        "FocusedBorderThickness", "HoverBorderColor", "HoverColor", "HoverFill",
        "PressedBorderColor", "PressedColor", "PressedFill", "SelectionColor", "SelectionFill",
        // Appearance (typography, shape, images)
        "Appearance", "DropShadow", "Font", "FontWeight", "Format", "Icon", "ImagePosition",
        "ImageRotation", "Italic", "LineHeight", "Mode", "Overflow", "PreserveAspectRatio",
        "RadiusBottomLeft", "RadiusBottomRight", "RadiusTopLeft", "RadiusTopRight",
        "Size", "Strikethrough", "Underline", "VerticalAlign", "VirtualKeyboardMode", "Wrap",
        // Padding
        "PaddingBottom", "PaddingLeft", "PaddingRight", "PaddingTop",
        // Display mode
        "DisplayMode",
        // Gallery / list / combo-box appearance
        "DelayItemLoading", "Layout", "LoadingSpinner", "LoadingSpinnerColor",
        "MoreItemsButtonColor", "NumberOfColumns", "Orientation",
        "SelectionTagColor", "SelectionTagFill", "Template", "TemplateFill",
        "TemplatePadding", "TemplateSize", "Transition",
        // Auto-layout / responsive
        "LayoutAlignItems", "LayoutDirection", "LayoutGap", "LayoutGridColumns", "LayoutGridRows",
        "LayoutJustifyContent", "LayoutMode", "LayoutOverflowX", "LayoutOverflowY", "LayoutWrap",
        // Theme, screen, tabs
        "ChildTabPriority", "EnableChildFocus", "IconBackground", "MinScreenHeight", "MinScreenWidth",
        "ResizerGrip", "ShowControls", "ShowValue", "SnapToColumns", "TabIndex", "Theme",
    };

    private static readonly HashSet<string> StructuralYamlControlKeys = new(StringComparer.OrdinalIgnoreCase)
    {
        "Control",
        "Variant",
        "IsLocked",
        "Layout",
        "MetadataKey",
        // IsFromScreenLayout and IsAutoGenerated are not in pa.schema.yaml and are no longer
        // emitted by our YAML export, but are still accepted for backward compatibility with
        // YAML files generated by older versions of this tool. The values are read if present;
        // otherwise they are inferred from the MetaDataIDKey pattern during pack.
        "IsFromScreenLayout",
        "IsAutoGenerated",
        "Properties",
        "Children",
    };
}