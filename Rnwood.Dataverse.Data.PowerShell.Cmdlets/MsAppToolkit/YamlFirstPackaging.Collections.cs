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
{    private const string RemovedDataSourcesCacheFileName = ".removed-datasources.cache.json";

    public static void GenerateCollectionDataSourceFromJson(string unpackDirectory, string collectionName, string jsonExamplePath)
    {
        if (string.IsNullOrWhiteSpace(unpackDirectory) || !Directory.Exists(unpackDirectory))
        {
            throw new DirectoryNotFoundException($"Unpack directory not found: '{unpackDirectory}'.");
        }

        if (string.IsNullOrWhiteSpace(collectionName))
        {
            throw new ArgumentException("Collection name must be provided.", nameof(collectionName));
        }

        if (string.IsNullOrWhiteSpace(jsonExamplePath) || !File.Exists(jsonExamplePath))
        {
            throw new FileNotFoundException($"JSON example file was not found: '{jsonExamplePath}'.", jsonExamplePath);
        }

        var jsonText = File.ReadAllText(jsonExamplePath);
        JsonNode? parsed;
        try
        {
            parsed = JsonNode.Parse(jsonText);
        }
        catch (Exception ex)
        {
            throw new InvalidDataException($"JSON example file is invalid: {jsonExamplePath}", ex);
        }

        var sampleArray = parsed switch
        {
            JsonArray arr => arr,
            JsonObject obj => new JsonArray(obj.DeepClone()),
            _ => throw new InvalidDataException("JSON example must be a JSON object or array of objects."),
        };

        var columns = InferColumns(sampleArray);
        var schema = BuildSchema(columns);

        var dataSourcesPath = Path.Combine(unpackDirectory, "References", "DataSources.json");
        Directory.CreateDirectory(Path.GetDirectoryName(dataSourcesPath)!);

        JsonObject root;
        JsonArray dataSources;
        if (File.Exists(dataSourcesPath)
            && JsonNode.Parse(File.ReadAllText(dataSourcesPath)) is JsonObject existingRoot
            && existingRoot["DataSources"] is JsonArray existingArray)
        {
            root = existingRoot;
            dataSources = existingArray;
        }
        else
        {
            root = new JsonObject();
            dataSources = new JsonArray();
            root["DataSources"] = dataSources;
        }

        var newEntry = new JsonObject
        {
            ["Name"] = collectionName,
            ["Schema"] = schema,
            ["IsSampleData"] = false,
            ["IsWritable"] = true,
            ["Type"] = "StaticDataSourceInfo",
            ["OriginalSchema"] = schema,
            ["Data"] = sampleArray.ToJsonString(),
            ["OriginalName"] = collectionName,
            ["OrderedColumnNames"] = new JsonArray(columns.Select(c => JsonValue.Create(c.Name)).ToArray()),
        };

        var replaced = false;
        for (var i = 0; i < dataSources.Count; i++)
        {
            if (dataSources[i] is not JsonObject ds)
            {
                continue;
            }

            var name = ds["Name"]?.GetValue<string>() ?? string.Empty;
            if (!string.Equals(name, collectionName, StringComparison.OrdinalIgnoreCase))
            {
                continue;
            }

            dataSources[i] = newEntry;
            replaced = true;
            break;
        }

        if (!replaced)
        {
            dataSources.Add(newEntry);
        }

        root["DataSources"] = dataSources;
        File.WriteAllText(dataSourcesPath, root.ToJsonString(JsonOptions));
    }

    public static void UpsertDataSourceFromJson(string unpackDirectory, string dataSourceJsonPath)
    {
        if (string.IsNullOrWhiteSpace(unpackDirectory) || !Directory.Exists(unpackDirectory))
        {
            throw new DirectoryNotFoundException($"Unpack directory not found: '{unpackDirectory}'.");
        }

        if (string.IsNullOrWhiteSpace(dataSourceJsonPath) || !File.Exists(dataSourceJsonPath))
        {
            throw new FileNotFoundException($"Data source JSON file was not found: '{dataSourceJsonPath}'.", dataSourceJsonPath);
        }

        JsonObject newEntry;
        try
        {
            newEntry = JsonNode.Parse(File.ReadAllText(dataSourceJsonPath)) as JsonObject
                ?? throw new InvalidDataException("Data source JSON must be a JSON object.");
        }
        catch (Exception ex) when (ex is JsonException or InvalidDataException)
        {
            throw new InvalidDataException($"Data source JSON is invalid: '{dataSourceJsonPath}'.", ex);
        }

        var name = newEntry["Name"]?.GetValue<string>() ?? string.Empty;
        if (string.IsNullOrWhiteSpace(name))
        {
            throw new InvalidDataException("Data source JSON must include a non-empty 'Name' property.");
        }

        var (_, root, dataSources) = OpenDataSourcesDocument(unpackDirectory);
        UpsertDataSourceEntry(dataSources, newEntry);
        root["DataSources"] = dataSources;
        SaveDataSourcesDocument(unpackDirectory, root);
    }

    public static bool RemoveDataSource(string unpackDirectory, string dataSourceName)
    {
        if (string.IsNullOrWhiteSpace(unpackDirectory) || !Directory.Exists(unpackDirectory))
        {
            throw new DirectoryNotFoundException($"Unpack directory not found: '{unpackDirectory}'.");
        }

        if (string.IsNullOrWhiteSpace(dataSourceName))
        {
            throw new ArgumentException("Data source name must be provided.", nameof(dataSourceName));
        }

        var (_, root, dataSources) = OpenDataSourcesDocument(unpackDirectory);
        var removed = false;
        for (var i = dataSources.Count - 1; i >= 0; i--)
        {
            if (dataSources[i] is not JsonObject ds)
            {
                continue;
            }

            var name = ds["Name"]?.GetValue<string>() ?? string.Empty;
            if (!string.Equals(name, dataSourceName, StringComparison.OrdinalIgnoreCase))
            {
                continue;
            }

            CacheRemovedDataSource(unpackDirectory, ds, i);
            dataSources.RemoveAt(i);
            removed = true;
        }

        if (removed)
        {
            root["DataSources"] = dataSources;
            SaveDataSourcesDocument(unpackDirectory, root);
        }

        return removed;
    }

    public static IReadOnlyList<JsonObject> GetDataSources(string unpackDirectory)
    {
        if (string.IsNullOrWhiteSpace(unpackDirectory) || !Directory.Exists(unpackDirectory))
        {
            throw new DirectoryNotFoundException($"Unpack directory not found: '{unpackDirectory}'.");
        }

        var (_, _, dataSources) = OpenDataSourcesDocument(unpackDirectory);
        var result = new List<JsonObject>();
        for (var i = 0; i < dataSources.Count; i++)
        {
            if (dataSources[i] is JsonObject ds)
            {
                result.Add((JsonObject)ds.DeepClone());
            }
        }
        return result;
    }

    private static void ValidateDataSourcesContainCollections(
        Dictionary<string, byte[]> entries,
        IReadOnlyDictionary<string, JsonObject> topControlFiles,
        IReadOnlyDictionary<string, JsonObject> topComponentFiles)
    {
        var requiredDataSourceNames = ExtractRequiredDataSourceNamesFromRules(topControlFiles, topComponentFiles);

        var dataSourceNames = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        if (entries.TryGetValue("References/DataSources.json", out var bytes)
            && JsonNode.Parse(bytes) is JsonObject parsed
            && parsed["DataSources"] is JsonArray parsedArray)
        {
            foreach (var ds in parsedArray.OfType<JsonObject>())
            {
                var name = ds["Name"]?.GetValue<string>() ?? string.Empty;
                if (!string.IsNullOrWhiteSpace(name))
                {
                    dataSourceNames.Add(name);
                }
            }
        }

        var missing = requiredDataSourceNames
            .Where(n => !dataSourceNames.Contains(n))
            .OrderBy(n => n, StringComparer.OrdinalIgnoreCase)
            .ToList();

        if (missing.Count > 0)
        {
            throw new InvalidDataException(
                "Missing data sources in References/DataSources.json for: "
                + string.Join(", ", missing)
                + ". Add these data sources explicitly before packing."
                + " Checked formula calls include Collect, ClearCollect, Patch, Remove, RemoveIf, Update, UpdateIf, Defaults, Refresh.");
        }
    }

    private static void RemoveStaticCollectionDataSources(
        Dictionary<string, byte[]> entries,
        IReadOnlyDictionary<string, JsonObject> topControlFiles,
        IReadOnlyDictionary<string, JsonObject> topComponentFiles)
    {
        if (!entries.TryGetValue("References/DataSources.json", out var bytes)
            || JsonNode.Parse(bytes) is not JsonObject root
            || root["DataSources"] is not JsonArray dataSources)
        {
            return;
        }

        var collectionTargets = ExtractCollectionTargetsFromCollectCalls(topControlFiles, topComponentFiles);
        if (collectionTargets.Count == 0)
        {
            return;
        }

        var changed = false;
        for (var i = dataSources.Count - 1; i >= 0; i--)
        {
            if (dataSources[i] is not JsonObject ds)
            {
                continue;
            }

            var name = ds["Name"]?.GetValue<string>() ?? string.Empty;
            var type = ds["Type"]?.GetValue<string>() ?? string.Empty;
            if (!collectionTargets.Contains(name)
                || !string.Equals(type, "StaticDataSourceInfo", StringComparison.OrdinalIgnoreCase))
            {
                continue;
            }

            dataSources.RemoveAt(i);
            changed = true;
        }

        if (!changed)
        {
            return;
        }

        root["DataSources"] = dataSources;
        entries["References/DataSources.json"] = Encoding.UTF8.GetBytes(root.ToJsonString(JsonOptions));
    }

    private static HashSet<string> ExtractCollectionTargetsFromCollectCalls(
        IReadOnlyDictionary<string, JsonObject> topControlFiles,
        IReadOnlyDictionary<string, JsonObject> topComponentFiles)
    {
        var names = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

        IEnumerable<JsonObject> EnumerateTopParents(IReadOnlyDictionary<string, JsonObject> files)
        {
            foreach (var v in files.Values)
            {
                if (v["TopParent"] is JsonObject top)
                {
                    yield return top;
                }
            }
        }

        foreach (var top in EnumerateTopParents(topControlFiles).Concat(EnumerateTopParents(topComponentFiles)))
        {
            foreach (var node in Flatten(top))
            {
                var rules = (node["Rules"] as JsonArray) ?? new JsonArray();
                foreach (var rule in rules.OfType<JsonObject>())
                {
                    var script = rule["InvariantScript"]?.GetValue<string>() ?? string.Empty;
                    if (string.IsNullOrWhiteSpace(script))
                    {
                        continue;
                    }

                    foreach (var call in EnumerateFunctionCalls(script, CollectionSeedFunctions))
                    {
                        if (call.Arguments.Count == 0)
                        {
                            continue;
                        }

                        var name = ParseCollectionName(call.Arguments[0]);
                        if (!string.IsNullOrWhiteSpace(name))
                        {
                            names.Add(name);
                        }
                    }
                }
            }
        }

        return names;
    }

    private static void EnsureCollectionDataSourcesFromRules(
        Dictionary<string, byte[]> entries,
        IReadOnlyDictionary<string, JsonObject> topControlFiles,
        IReadOnlyDictionary<string, JsonObject> topComponentFiles)
    {
        var seedRowsByCollection = ExtractStaticCollectionSeedRows(topControlFiles, topComponentFiles);
        if (seedRowsByCollection.Count == 0)
        {
            return;
        }

        JsonObject root;
        JsonArray dataSources;
        if (entries.TryGetValue("References/DataSources.json", out var existingBytes)
            && JsonNode.Parse(existingBytes) is JsonObject existingRoot
            && existingRoot["DataSources"] is JsonArray existingArray)
        {
            root = existingRoot;
            dataSources = existingArray;
        }
        else
        {
            root = new JsonObject();
            dataSources = new JsonArray();
            root["DataSources"] = dataSources;
        }

        var changed = false;
        foreach (var (collectionName, rows) in seedRowsByCollection)
        {
            if (rows.Count == 0)
            {
                continue;
            }

            var existing = dataSources
                .OfType<JsonObject>()
                .FirstOrDefault(ds => string.Equals(ds["Name"]?.GetValue<string>() ?? string.Empty, collectionName, StringComparison.OrdinalIgnoreCase));

            var newEntry = BuildStaticCollectionDataSourceEntry(collectionName, rows);
            if (existing is null)
            {
                dataSources.Add(newEntry);
                changed = true;
                continue;
            }

            var type = existing["Type"]?.GetValue<string>() ?? string.Empty;
            if (!string.Equals(type, "StaticDataSourceInfo", StringComparison.OrdinalIgnoreCase)
                && !string.IsNullOrWhiteSpace(type))
            {
                continue;
            }

            var existingCount = GetDataSourceRowCount(existing);
            if (existingCount >= rows.Count)
            {
                continue;
            }

            if (existing["IsSampleData"] is not null)
            {
                newEntry["IsSampleData"] = existing["IsSampleData"]!.DeepClone();
            }

            if (existing["IsWritable"] is not null)
            {
                newEntry["IsWritable"] = existing["IsWritable"]!.DeepClone();
            }

            if (existing["OriginalName"] is not null)
            {
                newEntry["OriginalName"] = existing["OriginalName"]!.DeepClone();
            }

            for (var i = 0; i < dataSources.Count; i++)
            {
                if (ReferenceEquals(dataSources[i], existing))
                {
                    dataSources[i] = newEntry;
                    changed = true;
                    break;
                }
            }
        }

        if (!changed)
        {
            return;
        }

        root["DataSources"] = dataSources;
        entries["References/DataSources.json"] = Encoding.UTF8.GetBytes(root.ToJsonString(JsonOptions));
    }

    private static Dictionary<string, List<JsonObject>> ExtractStaticCollectionSeedRows(
        IReadOnlyDictionary<string, JsonObject> topControlFiles,
        IReadOnlyDictionary<string, JsonObject> topComponentFiles)
    {
        var rowsByCollection = new Dictionary<string, List<JsonObject>>(StringComparer.OrdinalIgnoreCase);

        IEnumerable<JsonObject> EnumerateTopParents(IReadOnlyDictionary<string, JsonObject> files)
        {
            foreach (var v in files.Values)
            {
                if (v["TopParent"] is JsonObject top)
                {
                    yield return top;
                }
            }
        }

        foreach (var top in EnumerateTopParents(topControlFiles).Concat(EnumerateTopParents(topComponentFiles)))
        {
            foreach (var node in Flatten(top))
            {
                var rules = (node["Rules"] as JsonArray) ?? new JsonArray();
                foreach (var rule in rules.OfType<JsonObject>())
                {
                    var script = rule["InvariantScript"]?.GetValue<string>() ?? string.Empty;
                    if (string.IsNullOrWhiteSpace(script))
                    {
                        continue;
                    }

                    foreach (var call in EnumerateFunctionCalls(script, CollectionSeedFunctions))
                    {
                        if (call.Arguments.Count <= 1)
                        {
                            continue;
                        }

                        var collectionName = ParseCollectionName(call.Arguments[0]);
                        if (string.IsNullOrWhiteSpace(collectionName))
                        {
                            continue;
                        }

                        var parsedRows = new List<JsonObject>();
                        foreach (var arg in call.Arguments.Skip(1))
                        {
                            if (TryParseStaticRecordLiteral(arg, out var row))
                            {
                                parsedRows.Add(row);
                            }
                        }

                        if (parsedRows.Count == 0)
                        {
                            continue;
                        }

                        if (!rowsByCollection.TryGetValue(collectionName, out var existingRows)
                            || parsedRows.Count > existingRows.Count)
                        {
                            rowsByCollection[collectionName] = parsedRows;
                        }
                    }
                }
            }
        }

        return rowsByCollection;
    }

    private static bool TryParseStaticRecordLiteral(string expression, out JsonObject record)
    {
        record = new JsonObject();
        var trimmed = expression?.Trim() ?? string.Empty;
        if (!(trimmed.Length > 0 && trimmed[0] == '{') || !(trimmed.Length > 0 && trimmed[trimmed.Length - 1] == '}'))
        {
            return false;
        }

        var jsonish = RecordKeyRegex.Replace(trimmed, "$1\"$2\":");
        jsonish = TrueFalseRegex.Replace(jsonish, m =>
            string.Equals(m.Value, "true", StringComparison.OrdinalIgnoreCase) ? "true" : "false");

        try
        {
            if (JsonNode.Parse(jsonish) is not JsonObject parsed)
            {
                return false;
            }

            record = parsed;
            return true;
        }
        catch
        {
            return false;
        }
    }

    private static JsonObject BuildStaticCollectionDataSourceEntry(string collectionName, IReadOnlyList<JsonObject> rows)
    {
        var sampleArray = new JsonArray(rows.Select(r => (JsonNode)r.DeepClone()).ToArray());
        var columns = InferColumns(sampleArray);
        var schema = BuildSchema(columns);
        var isSample = collectionName.EndsWith("Sample", StringComparison.OrdinalIgnoreCase);

        return new JsonObject
        {
            ["Name"] = collectionName,
            ["Schema"] = schema,
            ["IsSampleData"] = isSample,
            ["IsWritable"] = true,
            ["Type"] = "StaticDataSourceInfo",
            ["OriginalSchema"] = schema,
            ["Data"] = sampleArray.ToJsonString(),
            ["OriginalName"] = collectionName,
            ["OrderedColumnNames"] = new JsonArray(columns.Select(c => JsonValue.Create(c.Name)).ToArray()),
        };
    }

    private static int GetDataSourceRowCount(JsonObject dataSource)
    {
        var dataText = dataSource["Data"]?.GetValue<string>() ?? string.Empty;
        if (string.IsNullOrWhiteSpace(dataText))
        {
            return 0;
        }

        try
        {
            var parsed = JsonNode.Parse(dataText);
            return parsed switch
            {
                JsonArray arr => arr.Count,
                JsonObject => 1,
                _ => 0,
            };
        }
        catch
        {
            return 0;
        }
    }

    private static Dictionary<string, int> ExtractStaticCollectionSeedMinimums(
        IReadOnlyDictionary<string, JsonObject> topControlFiles,
        IReadOnlyDictionary<string, JsonObject> topComponentFiles)
    {
        var required = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);

        IEnumerable<JsonObject> EnumerateTopParents(IReadOnlyDictionary<string, JsonObject> files)
        {
            foreach (var v in files.Values)
            {
                if (v["TopParent"] is JsonObject top)
                {
                    yield return top;
                }
            }
        }

        foreach (var top in EnumerateTopParents(topControlFiles).Concat(EnumerateTopParents(topComponentFiles)))
        {
            foreach (var node in Flatten(top))
            {
                var rules = (node["Rules"] as JsonArray) ?? new JsonArray();
                foreach (var rule in rules.OfType<JsonObject>())
                {
                    var script = rule["InvariantScript"]?.GetValue<string>() ?? string.Empty;
                    if (string.IsNullOrWhiteSpace(script))
                    {
                        continue;
                    }

                    foreach (var call in EnumerateFunctionCalls(script, CollectionSeedFunctions))
                    {
                        var collectionName = call.Arguments.Count > 0
                            ? ParseCollectionName(call.Arguments[0])
                            : string.Empty;
                        if (string.IsNullOrWhiteSpace(collectionName))
                        {
                            continue;
                        }

                        var staticSeedCount = call.Arguments
                            .Skip(1)
                            .Count(IsStaticRecordLiteral);

                        if (staticSeedCount <= 0)
                        {
                            continue;
                        }

                        if (!required.TryGetValue(collectionName, out var current) || staticSeedCount > current)
                        {
                            required[collectionName] = staticSeedCount;
                        }
                    }
                }
            }
        }

        return required;
    }

    private static bool IsStaticRecordLiteral(string expression)
    {
        var trimmed = expression?.Trim() ?? string.Empty;
        if (!(trimmed.Length > 0 && trimmed[0] == '{') || !(trimmed.Length > 0 && trimmed[trimmed.Length - 1] == '}'))
        {
            return false;
        }

        var jsonish = RecordKeyRegex.Replace(trimmed, "$1\"$2\":");
        jsonish = TrueFalseRegex.Replace(jsonish, m =>
            string.Equals(m.Value, "true", StringComparison.OrdinalIgnoreCase) ? "true" : "false");

        try
        {
            return JsonNode.Parse(jsonish) is JsonObject;
        }
        catch
        {
            return false;
        }
    }

    private static IEnumerable<FunctionCall> EnumerateFunctionCalls(string script, IReadOnlyCollection<string> functionNames)
    {
        if (string.IsNullOrWhiteSpace(script))
        {
            yield break;
        }

        var index = 0;
        while (index < script.Length)
        {
            var nextIndex = -1;
            var functionName = string.Empty;
            foreach (var candidate in functionNames)
            {
                var candidateIndex = IndexOfFunction(script, candidate, index);
                if (candidateIndex < 0)
                {
                    continue;
                }

                if (nextIndex < 0 || candidateIndex < nextIndex)
                {
                    nextIndex = candidateIndex;
                    functionName = candidate;
                }
            }

            if (nextIndex < 0)
            {
                yield break;
            }

            var openParen = nextIndex + functionName.Length;
            while (openParen < script.Length && char.IsWhiteSpace(script[openParen]))
            {
                openParen++;
            }

            if (openParen >= script.Length || script[openParen] != '(')
            {
                index = nextIndex + 1;
                continue;
            }

            var closeParen = FindMatchingParen(script, openParen);
            if (closeParen < 0)
            {
                yield break;
            }

            var args = SplitTopLevelArguments(script.Substring(openParen + 1, closeParen - openParen - 1));
            yield return new FunctionCall(functionName, args);

            index = closeParen + 1;
        }
    }

    private static int IndexOfFunction(string text, string functionName, int startIndex)
    {
        var i = startIndex;
        while (i < text.Length)
        {
            var idx = text.IndexOf(functionName, i, StringComparison.OrdinalIgnoreCase);
            if (idx < 0)
            {
                return -1;
            }

            var beforeOk = idx == 0 || !char.IsLetterOrDigit(text[idx - 1]);
            var afterPos = idx + functionName.Length;
            var afterOk = afterPos >= text.Length || !char.IsLetterOrDigit(text[afterPos]);
            if (beforeOk && afterOk)
            {
                return idx;
            }

            i = idx + 1;
        }

        return -1;
    }

    private static int FindMatchingParen(string text, int openParenIndex)
    {
        var depth = 0;
        var inString = false;
        for (var i = openParenIndex; i < text.Length; i++)
        {
            var c = text[i];

            if (c == '"')
            {
                var escaped = i > 0 && text[i - 1] == '\\';
                if (!escaped)
                {
                    inString = !inString;
                }
            }

            if (inString)
            {
                continue;
            }

            if (c == '(')
            {
                depth++;
                continue;
            }

            if (c == ')')
            {
                depth--;
                if (depth == 0)
                {
                    return i;
                }
            }
        }

        return -1;
    }

    private static List<string> SplitTopLevelArguments(string argsText)
    {
        var args = new List<string>();
        var current = new StringBuilder();
        var parenDepth = 0;
        var braceDepth = 0;
        var bracketDepth = 0;
        var inString = false;

        for (var i = 0; i < argsText.Length; i++)
        {
            var c = argsText[i];
            var escaped = i > 0 && argsText[i - 1] == '\\';

            if (c == '"' && !escaped)
            {
                inString = !inString;
                current.Append(c);
                continue;
            }

            if (!inString)
            {
                switch (c)
                {
                    case '(':
                        parenDepth++;
                        break;
                    case ')':
                        parenDepth--;
                        break;
                    case '{':
                        braceDepth++;
                        break;
                    case '}':
                        braceDepth--;
                        break;
                    case '[':
                        bracketDepth++;
                        break;
                    case ']':
                        bracketDepth--;
                        break;
                    case ',':
                        if (parenDepth == 0 && braceDepth == 0 && bracketDepth == 0)
                        {
                            var token = current.ToString().Trim();
                            if (token.Length > 0)
                            {
                                args.Add(token);
                            }

                            current.Clear();
                            continue;
                        }

                        break;
                }
            }

            current.Append(c);
        }

        var last = current.ToString().Trim();
        if (last.Length > 0)
        {
            args.Add(last);
        }

        return args;
    }

    private static string ParseCollectionName(string firstArgument)
    {
        var arg = firstArgument?.Trim() ?? string.Empty;
        if (arg.Length == 0)
        {
            return string.Empty;
        }

        if (arg.Length >= 2 && arg[0] == '\'' && arg[arg.Length - 1] == '\'')
        {
            return arg.Substring(1, arg.Length - 2);
        }

        return IdentifierRegex.IsMatch(arg) ? arg : string.Empty;
    }

    private static HashSet<string> ExtractRequiredDataSourceNamesFromRules(
        IReadOnlyDictionary<string, JsonObject> topControlFiles,
        IReadOnlyDictionary<string, JsonObject> topComponentFiles)
    {
        var names = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

        IEnumerable<JsonObject> EnumerateTopParents(IReadOnlyDictionary<string, JsonObject> files)
        {
            foreach (var v in files.Values)
            {
                if (v["TopParent"] is JsonObject top)
                {
                    yield return top;
                }
            }
        }

        foreach (var top in EnumerateTopParents(topControlFiles).Concat(EnumerateTopParents(topComponentFiles)))
        {
            foreach (var node in Flatten(top))
            {
                var rules = (node["Rules"] as JsonArray) ?? new JsonArray();
                foreach (var rule in rules.OfType<JsonObject>())
                {
                    var script = rule["InvariantScript"]?.GetValue<string>() ?? string.Empty;
                    if (string.IsNullOrWhiteSpace(script))
                    {
                        continue;
                    }

                    foreach (var call in EnumerateFunctionCalls(script, DataSourceRequiredFunctions))
                    {
                        if (call.Arguments.Count == 0)
                        {
                            continue;
                        }

                        var name = ParseCollectionName(call.Arguments[0]);
                        if (!string.IsNullOrWhiteSpace(name))
                        {
                            names.Add(name);
                        }
                    }
                }
            }
        }

        return names;
    }


    private static readonly Regex CollectionFunctionRegex = new(
        @"\b(?:ClearCollect|Collect)\s*\(\s*(?:'([^']+)'|([A-Za-z_][A-Za-z0-9_]*))",
        RegexOptions.Compiled | RegexOptions.CultureInvariant | RegexOptions.IgnoreCase);

    private static readonly string[] DataSourceRequiredFunctions =
    [
        "Patch",
        "Remove",
        "RemoveIf",
        "Update",
        "UpdateIf",
        "Defaults",
        "Refresh",
    ];

    private static readonly string[] CollectionSeedFunctions =
    [
        "Collect",
        "ClearCollect",
    ];

    private static readonly Regex IdentifierRegex = new(
        @"^[A-Za-z_][A-Za-z0-9_]*$",
        RegexOptions.Compiled | RegexOptions.CultureInvariant);

    private static readonly Regex RecordKeyRegex = new(
        @"(^|[,{]\s*)([A-Za-z_][A-Za-z0-9_]*)\s*:",
        RegexOptions.Compiled | RegexOptions.CultureInvariant);

    private static readonly Regex TrueFalseRegex = new(
        @"\b(true|false)\b",
        RegexOptions.Compiled | RegexOptions.CultureInvariant | RegexOptions.IgnoreCase);


    private static string BuildSchema(IReadOnlyList<(string Name, string TypeCode)> columns)
    {
        if (columns.Count == 0)
        {
            return "*[]";
        }

        return "*[" + string.Join(", ", columns.Select(c => $"{c.Name}:{c.TypeCode}")) + "]";
    }

    private static List<(string Name, string TypeCode)> InferColumns(JsonArray sampleArray)
    {
        var order = new List<string>();
        var seen = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        var typeByName = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);

        foreach (var node in sampleArray)
        {
            if (node is not JsonObject row)
            {
                continue;
            }

            foreach (var kv in row)
            {
                var name = kv.Key;
                if (!seen.Contains(name))
                {
                    order.Add(name);
                    seen.Add(name);
                }

                var inferred = InferTypeCode(name, kv.Value);
                if (!typeByName.TryGetValue(name, out var current))
                {
                    typeByName[name] = inferred;
                    continue;
                }

                if (!string.Equals(current, inferred, StringComparison.OrdinalIgnoreCase))
                {
                    typeByName[name] = ResolveTypeConflict(current, inferred);
                }
            }
        }

        return order.Select(name => (name, typeByName.TryGetValue(name, out var t) ? t : "s")).ToList();
    }

    private static string ResolveTypeConflict(string left, string right)
    {
        if (string.Equals(left, right, StringComparison.OrdinalIgnoreCase))
        {
            return left;
        }

        if ((left == "n" && right == "b") || (left == "b" && right == "n"))
        {
            return "n";
        }

        return "s";
    }

    private static string InferTypeCode(string columnName, JsonNode? value)
    {
        if (value is null)
        {
            return "s";
        }

        if (value is JsonArray || value is JsonObject)
        {
            return "s";
        }

        if (value is not JsonValue scalar)
        {
            return "s";
        }

        if (scalar.TryGetValue<bool>(out _))
        {
            return "b";
        }

        if (scalar.TryGetValue<decimal>(out _))
        {
            return "n";
        }

        if (scalar.TryGetValue<string>(out var s))
        {
            if (LooksLikeImageField(columnName, s))
            {
                return "i";
            }

            return "s";
        }

        return "s";
    }

    private static bool LooksLikeImageField(string columnName, string value)
    {
        if (columnName.IndexOf("image", StringComparison.OrdinalIgnoreCase) >= 0
            || columnName.IndexOf("photo", StringComparison.OrdinalIgnoreCase) >= 0
            || columnName.IndexOf("icon", StringComparison.OrdinalIgnoreCase) >= 0
            || columnName.IndexOf("logo", StringComparison.OrdinalIgnoreCase) >= 0)
        {
            return true;
        }

        var trimmed = value?.Trim() ?? string.Empty;
        if (trimmed.Length == 0)
        {
            return false;
        }

        return trimmed.EndsWith(".png", StringComparison.OrdinalIgnoreCase)
               || trimmed.EndsWith(".jpg", StringComparison.OrdinalIgnoreCase)
               || trimmed.EndsWith(".jpeg", StringComparison.OrdinalIgnoreCase)
               || trimmed.EndsWith(".svg", StringComparison.OrdinalIgnoreCase)
               || trimmed.StartsWith("data:image/", StringComparison.OrdinalIgnoreCase);
    }

    private static string GetRemovedDataSourcesCachePath(string unpackDirectory)
    {
        return Path.Combine(unpackDirectory, "References", RemovedDataSourcesCacheFileName);
    }

    private static void CacheRemovedDataSource(string unpackDirectory, JsonObject dataSource, int originalIndex)
    {
        var cachePath = GetRemovedDataSourcesCachePath(unpackDirectory);
        Directory.CreateDirectory(Path.GetDirectoryName(cachePath)!);

        JsonObject root;
        JsonArray removed;
        if (File.Exists(cachePath)
            && JsonNode.Parse(File.ReadAllText(cachePath)) is JsonObject existingRoot
            && existingRoot["RemovedDataSources"] is JsonArray existingRemoved)
        {
            root = existingRoot;
            removed = existingRemoved;
        }
        else
        {
            root = new JsonObject();
            removed = new JsonArray();
            root["RemovedDataSources"] = removed;
        }

        var name = dataSource["Name"]?.GetValue<string>() ?? string.Empty;
        var datasetName = dataSource["DatasetName"]?.GetValue<string>() ?? string.Empty;
        var logicalName = dataSource["LogicalName"]?.GetValue<string>() ?? string.Empty;

        for (var i = removed.Count - 1; i >= 0; i--)
        {
            if (removed[i] is not JsonObject existing)
            {
                continue;
            }

            var existingName = existing["Name"]?.GetValue<string>() ?? string.Empty;
            var existingDataset = existing["DatasetName"]?.GetValue<string>() ?? string.Empty;
            var existingLogical = existing["LogicalName"]?.GetValue<string>() ?? string.Empty;
            if (!string.Equals(existingName, name, StringComparison.OrdinalIgnoreCase)
                || !string.Equals(existingDataset, datasetName, StringComparison.OrdinalIgnoreCase)
                || !string.Equals(existingLogical, logicalName, StringComparison.OrdinalIgnoreCase))
            {
                continue;
            }

            removed.RemoveAt(i);
        }

        removed.Add(new JsonObject
        {
            ["Name"] = name,
            ["DatasetName"] = datasetName,
            ["LogicalName"] = logicalName,
            ["OriginalIndex"] = originalIndex,
            ["Entry"] = dataSource.DeepClone(),
        });

        root["RemovedDataSources"] = removed;
        File.WriteAllText(cachePath, root.ToJsonString(JsonOptions));
    }

    private static bool TryRestoreRemovedDataSource(
        string unpackDirectory,
        string? requestedDataSourceName,
        string datasetName,
        string logicalName,
        out JsonObject? restoredEntry,
        out int originalIndex)
    {
        restoredEntry = null;
        originalIndex = -1;

        var cachePath = GetRemovedDataSourcesCachePath(unpackDirectory);
        if (!File.Exists(cachePath)
            || JsonNode.Parse(File.ReadAllText(cachePath)) is not JsonObject root
            || root["RemovedDataSources"] is not JsonArray removed)
        {
            return false;
        }

        JsonObject? candidate = null;
        var candidatePos = -1;
        for (var i = removed.Count - 1; i >= 0; i--)
        {
            if (removed[i] is not JsonObject item)
            {
                continue;
            }

            var itemDataset = item["DatasetName"]?.GetValue<string>() ?? string.Empty;
            var itemLogical = item["LogicalName"]?.GetValue<string>() ?? string.Empty;
            if (!string.Equals(itemDataset, datasetName, StringComparison.OrdinalIgnoreCase)
                || !string.Equals(itemLogical, logicalName, StringComparison.OrdinalIgnoreCase))
            {
                continue;
            }

            var itemName = item["Name"]?.GetValue<string>() ?? string.Empty;
            if (!string.IsNullOrWhiteSpace(requestedDataSourceName)
                && !string.Equals(itemName, requestedDataSourceName, StringComparison.OrdinalIgnoreCase))
            {
                continue;
            }

            candidate = item;
            candidatePos = i;
            break;
        }

        if (candidate is null || candidate["Entry"] is not JsonObject entry)
        {
            return false;
        }

        restoredEntry = (JsonObject)entry.DeepClone();
        originalIndex = candidate["OriginalIndex"]?.GetValue<int?>() ?? -1;
        removed.RemoveAt(candidatePos);
        root["RemovedDataSources"] = removed;
        File.WriteAllText(cachePath, root.ToJsonString(JsonOptions));
        return true;
    }

    private static (string DataSourcesPath, JsonObject Root, JsonArray DataSources) OpenDataSourcesDocument(string unpackDirectory)
    {
        var dataSourcesPath = Path.Combine(unpackDirectory, "References", "DataSources.json");
        Directory.CreateDirectory(Path.GetDirectoryName(dataSourcesPath)!);

        JsonObject root;
        JsonArray dataSources;
        if (File.Exists(dataSourcesPath)
            && JsonNode.Parse(File.ReadAllText(dataSourcesPath)) is JsonObject existingRoot
            && existingRoot["DataSources"] is JsonArray existingArray)
        {
            root = existingRoot;
            dataSources = existingArray;
        }
        else
        {
            root = new JsonObject();
            dataSources = new JsonArray();
            root["DataSources"] = dataSources;
        }

        return (dataSourcesPath, root, dataSources);
    }

    private static void SaveDataSourcesDocument(string unpackDirectory, JsonObject root)
    {
        var path = Path.Combine(unpackDirectory, "References", "DataSources.json");
        Directory.CreateDirectory(Path.GetDirectoryName(path)!);
        File.WriteAllText(path, root.ToJsonString(JsonOptions));
    }

    private static void UpsertDataSourceEntry(JsonArray dataSources, JsonObject newEntry, int preferredInsertIndex = -1)
    {
        var name = newEntry["Name"]?.GetValue<string>() ?? string.Empty;
        var replaced = false;
        for (var i = 0; i < dataSources.Count; i++)
        {
            if (dataSources[i] is not JsonObject ds)
            {
                continue;
            }

            var existingName = ds["Name"]?.GetValue<string>() ?? string.Empty;
            if (!string.Equals(existingName, name, StringComparison.OrdinalIgnoreCase))
            {
                continue;
            }

            dataSources[i] = newEntry;
            replaced = true;
            break;
        }

        if (!replaced)
        {
            if (preferredInsertIndex >= 0 && preferredInsertIndex <= dataSources.Count)
            {
                dataSources.Insert(preferredInsertIndex, newEntry);
            }
            else
            {
                dataSources.Add(newEntry);
            }
        }
    }
}
