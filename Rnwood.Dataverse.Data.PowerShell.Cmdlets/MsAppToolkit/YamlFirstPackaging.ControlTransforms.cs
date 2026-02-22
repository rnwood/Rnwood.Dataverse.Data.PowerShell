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
{    private static string ResolveComponentTemplateName(string componentName, JsonObject template, Dictionary<string, JsonObject> metadataByName)
    {
        if (metadataByName.TryGetValue(componentName, out var meta))
        {
            return meta["TemplateName"]?.GetValue<string>() ?? template["Name"]?.GetValue<string>() ?? Guid.NewGuid().ToString("N");
        }

        return template["Name"]?.GetValue<string>() ?? Guid.NewGuid().ToString("N");
    }

    private static void ReplaceEntries(Dictionary<string, byte[]> entries, string folderPrefix, Dictionary<string, JsonObject> replacement)
    {
        var keys = entries.Keys.Where(k => k.StartsWith(folderPrefix, StringComparison.OrdinalIgnoreCase) && k.EndsWith(".json", StringComparison.OrdinalIgnoreCase)).ToList();
        foreach (var k in keys)
        {
            entries.Remove(k);
        }

        foreach (var (k, v) in replacement)
        {
            entries[k] = Encoding.UTF8.GetBytes(v.ToJsonString(JsonOptions));
        }
    }

    private static void UpdateUsedTemplates(
        Dictionary<string, byte[]> entries,
        IReadOnlyDictionary<string, JsonObject> topControlFiles,
        IReadOnlyDictionary<string, JsonObject> topComponentFiles)
    {
        var excluded = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
        {
            "appinfo",
            "hostControl",
            "screen",
            "galleryTemplate",
        };

        var catalog = new Dictionary<string, UsedTemplate>(StringComparer.OrdinalIgnoreCase);
        var pcfCatalog = new Dictionary<string, JsonObject>(StringComparer.OrdinalIgnoreCase);
        void RegisterCatalogEntry(UsedTemplate t)
        {
            if (string.IsNullOrWhiteSpace(t.Name) || string.IsNullOrWhiteSpace(t.TemplateXml))
            {
                return;
            }

            var exactKey = $"{t.Name}|{t.Version}";
            if (!catalog.ContainsKey(exactKey))
            {
                catalog[exactKey] = t;
            }

            if (!catalog.TryGetValue(t.Name, out var existing)
                || SemverTuple(t.Version).CompareTo(SemverTuple(existing.Version)) > 0)
            {
                catalog[t.Name] = t;
            }
        }

        void RegisterPcfCatalogEntries(IReadOnlyList<JsonObject> templates)
        {
            foreach (var t in templates)
            {
                var name = t["Name"]?.GetValue<string>() ?? string.Empty;
                if (string.IsNullOrWhiteSpace(name))
                {
                    continue;
                }

                var version = t["Version"]?.GetValue<string>() ?? string.Empty;
                if (!pcfCatalog.TryGetValue(name, out var existing))
                {
                    pcfCatalog[name] = (JsonObject)t.DeepClone();
                    continue;
                }

                var existingVersion = existing["Version"]?.GetValue<string>() ?? string.Empty;
                if (SemverTuple(version).CompareTo(SemverTuple(existingVersion)) > 0)
                {
                    pcfCatalog[name] = (JsonObject)t.DeepClone();
                }
            }
        }

        foreach (var t in ReadUsedTemplates(entries))
        {
            RegisterCatalogEntry(t);
        }

        RegisterPcfCatalogEntries(ReadPcfTemplates(entries));

        ReadEmbeddedAllControlsEntries(out var embeddedEntries);
        foreach (var t in ReadUsedTemplates(embeddedEntries))
        {
            RegisterCatalogEntry(t);
        }

        RegisterPcfCatalogEntries(ReadPcfTemplates(embeddedEntries));

        var seen = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        var seenPcf = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        var used = new List<UsedTemplate>();
        var usedPcf = new List<JsonObject>();

        void CollectFromTop(JsonObject top)
        {
            foreach (var node in Flatten(top))
            {
                var template = node["Template"] as JsonObject;
                var name = template?["Name"]?.GetValue<string>() ?? string.Empty;
                var version = template?["Version"]?.GetValue<string>() ?? string.Empty;
                if (string.IsNullOrWhiteSpace(name) || excluded.Contains(name))
                {
                    continue;
                }

                var seenKey = $"{name}|{version}";
                if (!seen.Add(seenKey))
                {
                    continue;
                }

                if (catalog.TryGetValue(seenKey, out var exact))
                {
                    used.Add(exact);
                    continue;
                }

                if (pcfCatalog.TryGetValue(name, out var pcfByName))
                {
                    if (seenPcf.Add(name))
                    {
                        var pcf = (JsonObject)pcfByName.DeepClone();
                        if (!string.IsNullOrWhiteSpace(version))
                        {
                            pcf["Version"] = version;
                        }

                        usedPcf.Add(pcf);
                    }

                    continue;
                }

                if (catalog.TryGetValue(name, out var byName))
                {
                    used.Add(string.IsNullOrWhiteSpace(version)
                        ? byName
                        : byName with { Version = version });
                }
            }
        }

        foreach (var top in topControlFiles.Values.Select(v => v["TopParent"] as JsonObject).Where(v => v is not null).Cast<JsonObject>())
        {
            CollectFromTop(top);
        }

        foreach (var top in topComponentFiles.Values.Select(v => v["TopParent"] as JsonObject).Where(v => v is not null).Cast<JsonObject>())
        {
            CollectFromTop(top);
        }

        if (!entries.TryGetValue("References/Templates.json", out var templatesBytes)
            || JsonNode.Parse(templatesBytes) is not JsonObject templatesRoot)
        {
            templatesRoot = new JsonObject();
        }

        var usedTemplatesNode = new JsonArray();
        foreach (var t in used)
        {
            if (string.IsNullOrWhiteSpace(t.TemplateXml))
            {
                continue;
            }

            usedTemplatesNode.Add(new JsonObject
            {
                ["Name"] = t.Name,
                ["Version"] = t.Version,
                ["Template"] = t.TemplateXml,
            });
        }

        templatesRoot["UsedTemplates"] = usedTemplatesNode;
        if (usedPcf.Count > 0)
        {
            var pcfTemplatesNode = new JsonArray();
            foreach (var pcf in usedPcf.OrderBy(p => p["Name"]?.GetValue<string>() ?? string.Empty, StringComparer.OrdinalIgnoreCase))
            {
                pcfTemplatesNode.Add(pcf);
            }

            templatesRoot["PcfTemplates"] = pcfTemplatesNode;
        }
        else
        {
            templatesRoot.Remove("PcfTemplates");
        }

        entries["References/Templates.json"] = Encoding.UTF8.GetBytes(templatesRoot.ToJsonString(JsonOptions));
    }

    private sealed record GalleryTemplateDescriptor(string Id, string Name, string Version, HashSet<string> Properties);

    private static void ApplyLegacyControlParityTransforms(
        Dictionary<string, JsonObject> topControlFiles,
        Dictionary<string, JsonObject> topComponentFiles,
        GalleryTemplateDescriptor? galleryTemplate)
    {
        foreach (var top in topControlFiles.Values
                     .Select(v => v["TopParent"] as JsonObject)
                     .Where(v => v is not null)
                     .Cast<JsonObject>())
        {
            ApplyLegacyControlParityTransformsToNode(top, galleryTemplate);
        }

        foreach (var top in topComponentFiles.Values
                     .Select(v => v["TopParent"] as JsonObject)
                     .Where(v => v is not null)
                     .Cast<JsonObject>())
        {
            ApplyLegacyControlParityTransformsToNode(top, galleryTemplate);
        }
    }

    private static void ApplyLegacyControlParityTransformsToNode(JsonObject node, GalleryTemplateDescriptor? galleryTemplate)
    {
        var children = (node["Children"] as JsonArray) ?? new JsonArray();
        foreach (var child in children.OfType<JsonObject>())
        {
            ApplyLegacyControlParityTransformsToNode(child, galleryTemplate);
        }

        RemoveUnresolvedTemplateTokenRules(node);
        ApplyGalleryTemplateTransform(node, galleryTemplate);
        FlattenGroupControls(node);
        ReindexChildren(node);
    }

    private static void RemoveUnresolvedTemplateTokenRules(JsonObject node)
    {
        var rules = node["Rules"] as JsonArray;
        if (rules is null || rules.Count == 0)
        {
            return;
        }

        var removedProperties = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        var rewrittenRules = new JsonArray();

        foreach (var rule in rules.OfType<JsonObject>())
        {
            var property = rule["Property"]?.GetValue<string>() ?? string.Empty;
            var script = rule["InvariantScript"]?.GetValue<string>() ?? string.Empty;

            // Some templates emit unresolved placeholder tokens (for example:
            // ##gallery_CopilotOverlayLabel##). These evaluate as Error values in Studio.
            // Drop those rules and corresponding state entries to keep packed apps clean.
            if (Regex.IsMatch(script, "^##[^#]+##$", RegexOptions.CultureInvariant))
            {
                if (!string.IsNullOrWhiteSpace(property))
                {
                    removedProperties.Add(property);
                }

                continue;
            }

            rewrittenRules.Add((JsonNode)rule.DeepClone());
        }

        if (removedProperties.Count == 0)
        {
            return;
        }

        node["Rules"] = rewrittenRules;

        var state = node["ControlPropertyState"] as JsonArray;
        if (state is not null && state.Count > 0)
        {
            var rewrittenState = new JsonArray();
            foreach (var entry in state)
            {
                var property = GetStatePropertyName(entry);
                if (!string.IsNullOrWhiteSpace(property) && removedProperties.Contains(property))
                {
                    continue;
                }

                rewrittenState.Add(entry?.DeepClone());
            }

            node["ControlPropertyState"] = rewrittenState;
        }
    }

    private static void ApplyGalleryTemplateTransform(JsonObject galleryNode, GalleryTemplateDescriptor? descriptor)
    {
        if (descriptor is null)
        {
            return;
        }

        var templateName = galleryNode["Template"]?["Name"]?.GetValue<string>() ?? string.Empty;
        if (!string.Equals(templateName, "gallery", StringComparison.OrdinalIgnoreCase))
        {
            return;
        }

        var children = (galleryNode["Children"] as JsonArray) ?? new JsonArray();
        galleryNode["Children"] = children;

        JsonObject? galleryTemplateChild = null;
        var existingTemplateChildren = new List<JsonObject>();
        foreach (var child in children.OfType<JsonObject>())
        {
            var childTemplate = child["Template"] as JsonObject;
            var childTemplateName = childTemplate?["Name"]?.GetValue<string>() ?? string.Empty;
            var childTemplateId = childTemplate?["Id"]?.GetValue<string>() ?? string.Empty;
            var isGalleryTemplate = string.Equals(childTemplateName, descriptor.Name, StringComparison.OrdinalIgnoreCase)
                                    || string.Equals(childTemplateId, descriptor.Id, StringComparison.OrdinalIgnoreCase);
            if (!isGalleryTemplate)
            {
                continue;
            }

            existingTemplateChildren.Add(child);
            galleryTemplateChild ??= child;
        }

        if (galleryTemplateChild is null)
        {
            var galleryName = galleryNode["Name"]?.GetValue<string>() ?? "Gallery";
            var preservedTemplateName = galleryNode["__PreservedGalleryTemplateName"]?.GetValue<string>() ?? string.Empty;
            var preservedTemplateId = galleryNode["__PreservedGalleryTemplateControlUniqueId"]?.GetValue<string>() ?? string.Empty;
            var preservedTemplatePublishOrder = galleryNode["__PreservedGalleryTemplatePublishOrderIndex"]?.GetValue<int?>();
            galleryTemplateChild = CreateDefaultControl(
                string.IsNullOrWhiteSpace(preservedTemplateName)
                    ? NextGalleryTemplateName(children)
                    : preservedTemplateName);
            galleryTemplateChild["Parent"] = galleryName;
            galleryTemplateChild["IsDataControl"] = true;
            galleryTemplateChild["StyleName"] = string.Empty;
            galleryTemplateChild["HasDynamicProperties"] = false;
            if (!string.IsNullOrWhiteSpace(preservedTemplateId))
            {
                galleryTemplateChild["ControlUniqueId"] = preservedTemplateId;
            }
            else
            {
                galleryTemplateChild["ControlUniqueId"] = Guid.NewGuid().ToString("N");
            }

            if (preservedTemplatePublishOrder is not null)
            {
                galleryTemplateChild["PublishOrderIndex"] = preservedTemplatePublishOrder;
            }
            else
            {
                // Keep parity with Studio-generated galleries: template publishes first.
                galleryTemplateChild["PublishOrderIndex"] = 1;
            }

            galleryTemplateChild["Template"] = new JsonObject
            {
                ["Id"] = descriptor.Id,
                ["Version"] = descriptor.Version,
                ["LastModifiedTimestamp"] = "0",
                ["Name"] = descriptor.Name,
                ["FirstParty"] = true,
                ["IsPremiumPcfControl"] = false,
                ["IsCustomGroupControlTemplate"] = false,
                ["CustomGroupControlTemplateName"] = string.Empty,
                ["IsComponentDefinition"] = false,
                ["OverridableProperties"] = new JsonObject(),
            };

            children.Insert(0, galleryTemplateChild);
        }

        for (var i = existingTemplateChildren.Count - 1; i >= 0; i--)
        {
            var extra = existingTemplateChildren[i];
            if (ReferenceEquals(extra, galleryTemplateChild))
            {
                continue;
            }

            children.Remove(extra);
        }

        var parentRules = (galleryNode["Rules"] as JsonArray) ?? new JsonArray();
        var childRules = (galleryTemplateChild["Rules"] as JsonArray) ?? new JsonArray();

        var moved = new List<JsonObject>();
        foreach (var rule in parentRules.OfType<JsonObject>())
        {
            var property = rule["Property"]?.GetValue<string>() ?? string.Empty;
            if (string.IsNullOrWhiteSpace(property) || !descriptor.Properties.Contains(property))
            {
                continue;
            }

            moved.Add(rule);
        }

        if (moved.Count > 0)
        {
            foreach (var rule in moved)
            {
                parentRules.Remove(rule);

                var property = rule["Property"]?.GetValue<string>() ?? string.Empty;
                var existing = childRules
                    .OfType<JsonObject>()
                    .FirstOrDefault(r => string.Equals(r["Property"]?.GetValue<string>() ?? string.Empty, property, StringComparison.OrdinalIgnoreCase));

                if (existing is not null)
                {
                    childRules.Remove(existing);
                }

                childRules.Add(rule);
            }
        }

        galleryNode["Rules"] = parentRules;
        galleryTemplateChild["Rules"] = childRules;

        UpdateControlPropertyStateFromRules(galleryNode);
        UpdateControlPropertyStateFromRules(galleryTemplateChild);

        children.Remove(galleryTemplateChild);
        children.Insert(0, galleryTemplateChild);

        // Normalize publish order for predictable Studio behavior:
        // galleryTemplate first, then each visual child in sibling order.
        var publishOrder = 1;
        galleryTemplateChild["PublishOrderIndex"] = publishOrder;
        foreach (var child in children.OfType<JsonObject>())
        {
            if (ReferenceEquals(child, galleryTemplateChild))
            {
                continue;
            }

            publishOrder++;
            child["PublishOrderIndex"] = publishOrder;
        }

        galleryNode.Remove("__PreservedGalleryTemplateName");
        galleryNode.Remove("__PreservedGalleryTemplateControlUniqueId");
        galleryNode.Remove("__PreservedGalleryTemplatePublishOrderIndex");
    }

    private static string NextGalleryTemplateName(JsonArray siblings)
    {
        var used = new HashSet<string>(siblings
            .OfType<JsonObject>()
            .Select(s => s["Name"]?.GetValue<string>() ?? string.Empty)
            .Where(n => !string.IsNullOrWhiteSpace(n)), StringComparer.OrdinalIgnoreCase);

        var i = 1;
        while (used.Contains($"galleryTemplate{i}"))
        {
            i++;
        }

        return $"galleryTemplate{i}";
    }

    private static void FlattenGroupControls(JsonObject parentNode)
    {
        var parentName = parentNode["Name"]?.GetValue<string>() ?? string.Empty;
        var children = (parentNode["Children"] as JsonArray) ?? new JsonArray();
        if (children.Count == 0)
        {
            parentNode["Children"] = children;
            return;
        }

        var rewrittenChildren = new JsonArray();
        var liftedChildren = new List<JsonObject>();

        foreach (var child in children.OfType<JsonObject>())
        {
            var childClone = (JsonObject)child.DeepClone();
            var templateName = child["Template"]?["Name"]?.GetValue<string>() ?? string.Empty;
            var isGroup = string.Equals(templateName, "group", StringComparison.OrdinalIgnoreCase)
                          || (child["IsGroupControl"]?.GetValue<bool>() ?? false);

            if (!isGroup)
            {
                rewrittenChildren.Add(childClone);
                continue;
            }

            var grouped = (child["Children"] as JsonArray) ?? new JsonArray();
            if (grouped.Count == 0)
            {
                rewrittenChildren.Add(childClone);
                continue;
            }

            var groupedNames = new JsonArray();
            foreach (var groupedChild in grouped.OfType<JsonObject>())
            {
                var groupedClone = (JsonObject)groupedChild.DeepClone();
                var groupedName = groupedChild["Name"]?.GetValue<string>() ?? string.Empty;
                if (!string.IsNullOrWhiteSpace(groupedName))
                {
                    groupedNames.Add(groupedName);
                }

                groupedClone["Parent"] = parentName;
                liftedChildren.Add(groupedClone);
            }

            childClone["IsGroupControl"] = true;
            childClone["GroupedControlsKey"] = groupedNames;
            childClone["Children"] = new JsonArray();
            rewrittenChildren.Add(childClone);
        }

        foreach (var lifted in liftedChildren)
        {
            rewrittenChildren.Add(lifted);
        }

        parentNode["Children"] = rewrittenChildren;
    }

    private static void ReindexChildren(JsonObject parentNode)
    {
        var children = (parentNode["Children"] as JsonArray) ?? new JsonArray();
        for (var i = 0; i < children.Count; i++)
        {
            if (children[i] is not JsonObject child)
            {
                continue;
            }

            if (child["Index"] is null)
            {
                child["Index"] = i;
            }

            if (child["PublishOrderIndex"] is null)
            {
                child["PublishOrderIndex"] = child["Index"]?.GetValue<int>() ?? i;
            }
        }

        parentNode["Children"] = children;
    }

    private static void UpdateControlPropertyStateFromRules(JsonObject controlNode)
    {
        var rules = (controlNode["Rules"] as JsonArray) ?? new JsonArray();
        var desired = new List<string>();
        foreach (var rule in rules.OfType<JsonObject>())
        {
            var property = rule["Property"]?.GetValue<string>() ?? string.Empty;
            if (string.IsNullOrWhiteSpace(property))
            {
                continue;
            }

            if (!desired.Contains(property, StringComparer.OrdinalIgnoreCase))
            {
                desired.Add(property);
            }
        }

        var existingByProperty = new Dictionary<string, JsonNode>(StringComparer.OrdinalIgnoreCase);
        var existingOrder = new List<string>();
        foreach (var stateEntry in ((controlNode["ControlPropertyState"] as JsonArray) ?? new JsonArray()))
        {
            var property = GetStatePropertyName(stateEntry);
            if (string.IsNullOrWhiteSpace(property))
            {
                continue;
            }

            if (!existingByProperty.ContainsKey(property))
            {
                existingByProperty[property] = stateEntry?.DeepClone() ?? JsonValue.Create(property)!;
            }

            if (!existingOrder.Contains(property, StringComparer.OrdinalIgnoreCase))
            {
                existingOrder.Add(property);
            }
        }

        var ordered = new List<string>();
        foreach (var property in existingOrder)
        {
            if (desired.Contains(property, StringComparer.OrdinalIgnoreCase)
                && !ordered.Contains(property, StringComparer.OrdinalIgnoreCase))
            {
                ordered.Add(property);
            }
        }

        foreach (var property in desired)
        {
            if (!ordered.Contains(property, StringComparer.OrdinalIgnoreCase))
            {
                ordered.Add(property);
            }
        }

        var state = new JsonArray();
        foreach (var property in ordered)
        {
            if (existingByProperty.TryGetValue(property, out var existingNode)
                && existingNode is JsonObject)
            {
                state.Add(existingNode.DeepClone());
                continue;
            }

            if (IsAppInfoControl(controlNode)
                && (string.Equals(property, "OnStart", StringComparison.OrdinalIgnoreCase)
                    || string.Equals(property, "StartScreen", StringComparison.OrdinalIgnoreCase)))
            {
                state.Add(CreateInvariantPropertyStateEntry(property));
                continue;
            }

            state.Add(property);
        }

        controlNode["ControlPropertyState"] = state;
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

    private static bool IsAppInfoControl(JsonObject controlNode)
    {
        var templateName = controlNode["Template"]?["Name"]?.GetValue<string>() ?? string.Empty;
        var templateId = controlNode["Template"]?["Id"]?.GetValue<string>() ?? string.Empty;
        return string.Equals(templateName, "appinfo", StringComparison.OrdinalIgnoreCase)
               || templateId.EndsWith("/appinfo", StringComparison.OrdinalIgnoreCase);
    }

    private static JsonObject CreateInvariantPropertyStateEntry(string property)
    {
        return new JsonObject
        {
            ["InvariantPropertyName"] = property,
            ["AutoRuleBindingEnabled"] = false,
            ["AutoRuleBindingString"] = "",
            ["NameMapSourceSchema"] = "?",
            ["IsLockable"] = false,
            ["AFDDataSourceName"] = "",
        };
    }

    private static GalleryTemplateDescriptor? BuildGalleryTemplateDescriptor(Dictionary<string, byte[]> entries)
    {
        static GalleryTemplateDescriptor Fallback()
            => new("http://microsoft.com/appmagic/galleryTemplate", "galleryTemplate", "1.0", new HashSet<string>(StringComparer.OrdinalIgnoreCase));

        if (!entries.TryGetValue("References/Templates.json", out var templatesBytes))
        {
            return Fallback();
        }

        var templatesRoot = JsonNode.Parse(templatesBytes)?.AsObject();
        var usedTemplates = templatesRoot?["UsedTemplates"] as JsonArray;
        if (usedTemplates is null)
        {
            return Fallback();
        }

        var gallery = usedTemplates
            .OfType<JsonObject>()
            .FirstOrDefault(t => string.Equals(t["Name"]?.GetValue<string>() ?? string.Empty, "gallery", StringComparison.OrdinalIgnoreCase));
        if (gallery is null)
        {
            return Fallback();
        }

        var templateXml = gallery["Template"]?.GetValue<string>() ?? string.Empty;
        if (string.IsNullOrWhiteSpace(templateXml))
        {
            return Fallback();
        }

        try
        {
            var doc = XDocument.Parse(templateXml);
            var nested = doc.Descendants().FirstOrDefault(e => e.Name.LocalName == "dataControlWidget"
                                                               && string.Equals(e.Attribute("name")?.Value, "galleryTemplate", StringComparison.OrdinalIgnoreCase));
            if (nested is null)
            {
                return null;
            }

            var id = nested.Attribute("id")?.Value ?? "http://microsoft.com/appmagic/galleryTemplate";
            var name = nested.Attribute("name")?.Value ?? "galleryTemplate";
            var version = nested.Attribute("version")?.Value ?? "1.0";

            var properties = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            foreach (var prop in nested.Descendants().Where(e => e.Name.LocalName is "property" or "includeProperty" or "overrideProperty"))
            {
                var propName = prop.Attribute("name")?.Value;
                if (!string.IsNullOrWhiteSpace(propName))
                {
                    properties.Add(propName);
                }
            }

            return new GalleryTemplateDescriptor(id, name, version, properties);
        }
        catch
        {
            return Fallback();
        }
    }

    private static void UpdateEditorState(
        Dictionary<string, byte[]> entries,
        ISerializer serializer,
        IDeserializer deserializer,
        List<string> screenOrderFromFiles,
        List<string> componentOrderFromFiles,
        List<string> discoveredScreens,
        List<string> discoveredComponents)
    {
        var hadExistingEditorState = entries.TryGetValue("Src/_EditorState.pa.yaml", out var existingBytes);
        var existingScreensOrder = new List<string>();
        var existingComponentsOrder = new List<string>();
        if (hadExistingEditorState)
        {
            var text = Encoding.UTF8.GetString(existingBytes!);
            if (TryParseYamlMap(deserializer, text, out var root)
                && TryGetMap(root, "EditorState", out var editor))
            {
                if (editor.TryGetValue("ScreensOrder", out var so) && so is List<object?> sl)
                {
                    existingScreensOrder.AddRange(sl.Select(v => v?.ToString() ?? string.Empty).Where(v => !string.IsNullOrWhiteSpace(v)));
                }

                if (editor.TryGetValue("ComponentDefinitionsOrder", out var co) && co is List<object?> cl)
                {
                    existingComponentsOrder.AddRange(cl.Select(v => v?.ToString() ?? string.Empty).Where(v => !string.IsNullOrWhiteSpace(v)));
                }
            }
        }

        var mergedScreens = MergeOrder(existingScreensOrder, screenOrderFromFiles, discoveredScreens);
        var mergedComponents = MergeOrder(existingComponentsOrder, componentOrderFromFiles, discoveredComponents);

        if (hadExistingEditorState
            && mergedScreens.SequenceEqual(existingScreensOrder, StringComparer.OrdinalIgnoreCase)
            && mergedComponents.SequenceEqual(existingComponentsOrder, StringComparer.OrdinalIgnoreCase))
        {
            entries["Src/_EditorState.pa.yaml"] = existingBytes!;
            return;
        }

        var payload = new Dictionary<string, object?>
        {
            ["EditorState"] = new Dictionary<string, object?>
            {
                ["ScreensOrder"] = mergedScreens,
            },
        };

        if (mergedComponents.Count > 0
            && payload["EditorState"] is Dictionary<string, object?> editorState)
        {
            editorState["ComponentDefinitionsOrder"] = mergedComponents;
        }

        var yaml = string.Join(Environment.NewLine, HeaderLines) + Environment.NewLine + serializer.Serialize(payload);
        entries["Src/_EditorState.pa.yaml"] = Encoding.UTF8.GetBytes(yaml);
    }

    private static List<string> MergeOrder(List<string> existing, List<string> fromFiles, List<string> discovered)
    {
        var result = new List<string>();
        void AddIfValid(string value)
        {
            if (string.IsNullOrWhiteSpace(value))
            {
                return;
            }

            if (!discovered.Contains(value, StringComparer.OrdinalIgnoreCase))
            {
                return;
            }

            if (!result.Contains(value, StringComparer.OrdinalIgnoreCase))
            {
                result.Add(value);
            }
        }

        foreach (var e in existing) AddIfValid(e);
        foreach (var f in fromFiles) AddIfValid(f);
        foreach (var d in discovered) AddIfValid(d);
        return result;
    }

    private static Dictionary<string, JsonObject> ReadComponentMetadata(Dictionary<string, byte[]> entries)
    {
        var map = new Dictionary<string, JsonObject>(StringComparer.OrdinalIgnoreCase);
        if (!entries.TryGetValue("ComponentsMetadata.json", out var bytes))
        {
            return map;
        }

        var root = JsonNode.Parse(bytes)?.AsObject();
        var arr = root?["Components"] as JsonArray;
        if (arr is null)
        {
            return map;
        }

        foreach (var c in arr.OfType<JsonObject>())
        {
            var name = c["Name"]?.GetValue<string>() ?? string.Empty;
            if (!string.IsNullOrWhiteSpace(name))
            {
                map[name] = c;
            }
        }

        return map;
    }

    private static void UpdateComponentMetadata(Dictionary<string, byte[]> entries, Dictionary<string, Dictionary<object, object?>> componentPayloads, Dictionary<string, JsonObject> topComponentFiles)
    {
        var hadComponentsMetadata = entries.ContainsKey("ComponentsMetadata.json");
        var existing = ReadComponentMetadata(entries);
        var components = new JsonArray();

        foreach (var name in componentPayloads.Keys.OrderBy(x => x, StringComparer.OrdinalIgnoreCase))
        {
            var payload = componentPayloads[name];
            var top = topComponentFiles.Values.FirstOrDefault(v => string.Equals(v["TopParent"]?["Name"]?.GetValue<string>(), name, StringComparison.OrdinalIgnoreCase));
            var topParent = top?["TopParent"] as JsonObject;
            var templateName = topParent?["Template"]?["Name"]?.GetValue<string>()
                ?? (existing.TryGetValue(name, out var old) ? old["TemplateName"]?.GetValue<string>() : null)
                ?? Guid.NewGuid().ToString("N");

            var allowCustomization = payload.TryGetValue("AllowCustomization", out var ac) && bool.TryParse(ac?.ToString(), out var acBool) ? acBool : true;
            var allowAccessToGlobals = payload.TryGetValue("AccessAppScope", out var aa) && bool.TryParse(aa?.ToString(), out var aaBool) && aaBool;
            var description = payload.TryGetValue("Description", out var d) ? d?.ToString() ?? string.Empty : string.Empty;

            components.Add(new JsonObject
            {
                ["Name"] = name,
                ["TemplateName"] = templateName,
                ["Description"] = description,
                ["AllowCustomization"] = allowCustomization,
                ["AllowAccessToGlobals"] = allowAccessToGlobals,
            });
        }

        if (components.Count == 0 && !hadComponentsMetadata)
        {
            // Preserve Studio behavior: do not introduce ComponentsMetadata.json for apps without components.
            return;
        }

        entries["ComponentsMetadata.json"] = Encoding.UTF8.GetBytes(new JsonObject
        {
            ["Components"] = components,
        }.ToJsonString(JsonOptions));
    }

    private static void UpdatePropertiesControlCount(
        Dictionary<string, byte[]> entries,
        IReadOnlyDictionary<string, JsonObject> topControlFiles,
        IReadOnlyDictionary<string, JsonObject> topComponentFiles)
    {
        if (!entries.TryGetValue("Properties.json", out var bytes))
        {
            return;
        }

        var root = JsonNode.Parse(bytes)?.AsObject();
        if (root is null)
        {
            return;
        }

        var computedCounts = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);

        void CountNode(JsonObject node)
        {
            var templateName = node["Template"]?["Name"]?.GetValue<string>() ?? string.Empty;
            if (!string.IsNullOrWhiteSpace(templateName)
                && !string.Equals(templateName, "appinfo", StringComparison.OrdinalIgnoreCase)
                && !string.Equals(templateName, "hostControl", StringComparison.OrdinalIgnoreCase))
            {
                computedCounts[templateName] = computedCounts.TryGetValue(templateName, out var c) ? c + 1 : 1;
            }

            foreach (var child in (node["Children"] as JsonArray ?? new JsonArray()).OfType<JsonObject>())
            {
                CountNode(child);
            }
        }

        foreach (var top in topControlFiles.Values.Select(v => v["TopParent"] as JsonObject).Where(v => v is not null).Cast<JsonObject>())
        {
            CountNode(top);
        }

        foreach (var top in topComponentFiles.Values.Select(v => v["TopParent"] as JsonObject).Where(v => v is not null).Cast<JsonObject>())
        {
            CountNode(top);
        }

        var controlCountNode = new JsonObject();
        var existingOrder = (root["ControlCount"] as JsonObject)
            ?.Select(kv => kv.Key)
            .ToList()
            ?? [];

        foreach (var key in existingOrder)
        {
            if (!computedCounts.TryGetValue(key, out var value))
            {
                continue;
            }

            controlCountNode[key] = value;
        }

        foreach (var kv in computedCounts)
        {
            if (controlCountNode.ContainsKey(kv.Key))
            {
                continue;
            }

            controlCountNode[kv.Key] = kv.Value;
        }

        root["ControlCount"] = controlCountNode;
        entries["Properties.json"] = Encoding.UTF8.GetBytes(root.ToJsonString(JsonOptions));
    }

    /// <summary>
    /// Properties that are owned by the AutoLayout container system.
    /// These must live in DynamicProperties (not Rules) when a control is a direct
    /// child of an AutoLayout GroupContainer (HasDynamicProperties = true).
    /// </summary>
    private static readonly HashSet<string> AutoLayoutLayoutPropertyNames = new(StringComparer.OrdinalIgnoreCase)
    {
        "FillPortions", "AlignInContainer",
        "LayoutMinWidth", "LayoutMinHeight", "LayoutMaxWidth", "LayoutMaxHeight",
    };

    /// <summary>
    /// For every direct child of an AutoLayout GroupContainer:
    /// 1. Sets HasDynamicProperties = true.
    /// 2. Moves any AutoLayout-owned property values from Rules into the matching
    ///    DynamicProperties entry so that the runtime reads the correct value.
    /// 3. Removes those properties from Rules and refreshes ControlPropertyState.
    /// </summary>
    internal static void FixAutoLayoutChildDynamicProperties(
        Dictionary<string, JsonObject> topControlFiles,
        Dictionary<string, JsonObject> topComponentFiles)
    {
        static void ProcessNode(JsonObject node)
        {
            var variantName = node["VariantName"]?.GetValue<string>() ?? string.Empty;
            var isAutoLayout = string.Equals(variantName, "AutoLayout", StringComparison.OrdinalIgnoreCase)
                               || string.Equals(variantName, "horizontalAutoLayoutContainer", StringComparison.OrdinalIgnoreCase)
                               || string.Equals(variantName, "verticalAutoLayoutContainer", StringComparison.OrdinalIgnoreCase);

            var children = (node["Children"] as JsonArray) ?? new JsonArray();
            foreach (var childNode in children.OfType<JsonObject>())
            {
                if (isAutoLayout)
                {
                    PromoteToAutoLayoutChild(childNode);
                }
                else
                {
                    DemoteFromNonAutoLayoutChild(childNode);
                }

                ProcessNode(childNode);
            }
        }

        static void PromoteToAutoLayoutChild(JsonObject node)
        {
            node["HasDynamicProperties"] = true;

            var dynamicPropsArray = node["DynamicProperties"] as JsonArray;
            if (dynamicPropsArray is null)
            {
                return;
            }

            // Build lookup: propertyName -> DynamicProperties entry (by clone so we can modify cleanly)
            var dynByName = new Dictionary<string, JsonObject>(StringComparer.OrdinalIgnoreCase);
            foreach (var dynEntry in dynamicPropsArray.OfType<JsonObject>())
            {
                var propName = dynEntry["PropertyName"]?.GetValue<string>() ?? string.Empty;
                if (!string.IsNullOrWhiteSpace(propName))
                {
                    dynByName[propName] = dynEntry;
                }
            }

            var existingRules = (node["Rules"] as JsonArray) ?? new JsonArray();

            // Collect rules to keep and rules to move into DynamicProperties.
            var rulesToKeep = new List<JsonObject>();
            foreach (var rule in existingRules.OfType<JsonObject>())
            {
                var property = rule["Property"]?.GetValue<string>() ?? string.Empty;
                if (!AutoLayoutLayoutPropertyNames.Contains(property))
                {
                    rulesToKeep.Add(rule);
                    continue;
                }

                // Sync the YAML-specified value into the matching DynamicProperties entry.
                if (dynByName.TryGetValue(property, out var dynEntry)
                    && dynEntry["Rule"] is JsonObject dynRule)
                {
                    var script = rule["InvariantScript"]?.GetValue<string>() ?? string.Empty;
                    dynRule["InvariantScript"] = JsonValue.Create(script);
                }
                // This rule is now owned by DynamicProperties — do not keep in Rules.
            }

            // Rebuild the Rules array from scratch using deep-cloned nodes.
            var newRules = new JsonArray();
            foreach (var r in rulesToKeep)
            {
                newRules.Add((JsonNode)r.DeepClone());
            }

            node["Rules"] = newRules;
            // NOTE: do not call UpdateControlPropertyStateFromRules here to avoid JsonValueCustomized issues.
        }

        static void DemoteFromNonAutoLayoutChild(JsonObject node)
        {
            if (node["DynamicProperties"] is not JsonArray dynamicPropsArray)
            {
                if (node["HasDynamicProperties"] is JsonValue hv && hv.TryGetValue<bool>(out var hasDyn) && hasDyn)
                {
                    node["HasDynamicProperties"] = false;
                }

                return;
            }

            var filtered = new JsonArray();
            foreach (var entry in dynamicPropsArray.OfType<JsonObject>())
            {
                var propertyName = entry["PropertyName"]?.GetValue<string>() ?? string.Empty;
                if (AutoLayoutLayoutPropertyNames.Contains(propertyName))
                {
                    continue;
                }

                filtered.Add((JsonNode)entry.DeepClone());
            }

            if (filtered.Count == 0)
            {
                node.Remove("DynamicProperties");
                node["HasDynamicProperties"] = false;
                return;
            }

            node["DynamicProperties"] = filtered;
            node["HasDynamicProperties"] = true;
        }

        foreach (var wrapper in topControlFiles.Values.Concat(topComponentFiles.Values))
        {
            if (wrapper["TopParent"] is JsonObject topParent)
            {
                ProcessNode(topParent);
            }
        }
    }

}