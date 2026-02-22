using System.Globalization;
using System.IO.Compression;
using System.Net.Http;
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
{    private static readonly HttpClient DataverseHttpClient = new();

    public static async Task UpsertDataverseTableDataSourceFromEnvironmentAsync(
        string unpackDirectory,
        string environmentUrl,
        string tableLogicalName,
        string? dataSourceName = null,
        string? datasetName = null,
        string? apiId = null)
    {
        if (string.IsNullOrWhiteSpace(unpackDirectory) || !Directory.Exists(unpackDirectory))
        {
            throw new DirectoryNotFoundException($"Unpack directory not found: '{unpackDirectory}'.");
        }

        if (string.IsNullOrWhiteSpace(environmentUrl))
        {
            throw new ArgumentException("Environment URL must be provided.", nameof(environmentUrl));
        }

        if (string.IsNullOrWhiteSpace(tableLogicalName))
        {
            throw new ArgumentException("Dataverse table logical name must be provided.", nameof(tableLogicalName));
        }

        var normalizedEnvironmentUrl = NormalizeEnvironmentUrl(environmentUrl);
        var normalizedTable = tableLogicalName.Trim();
        var escapedTable = EscapeODataLiteral(normalizedTable);
        var normalizedDatasetName = string.IsNullOrWhiteSpace(datasetName) ? "default.cds" : datasetName;

        if (TryRestoreRemovedDataSource(unpackDirectory, dataSourceName, normalizedDatasetName, normalizedTable, out var restoredEntry, out var originalIndex))
        {
            var (_, restoredRoot, restoredDataSources) = OpenDataSourcesDocument(unpackDirectory);
            UpsertDataSourceEntry(restoredDataSources, restoredEntry!, originalIndex);
            restoredRoot["DataSources"] = restoredDataSources;
            SaveDataSourcesDocument(unpackDirectory, restoredRoot);
            return;
        }

        var token = await AcquireDataverseAccessTokenAsync(normalizedEnvironmentUrl);

        var entityMetadata = await DataverseGetJsonAsync(
            normalizedEnvironmentUrl,
            token,
            $"EntityDefinitions(LogicalName='{escapedTable}')?$select=LogicalName,LogicalCollectionName,EntitySetName,PrimaryIdAttribute,PrimaryNameAttribute,ObjectTypeCode,DisplayName,DisplayCollectionName");

        var allAttributes = await DataverseGetJsonAsync(
            normalizedEnvironmentUrl,
            token,
            $"EntityDefinitions(LogicalName='{escapedTable}')/Attributes?$select=LogicalName,DisplayName");

        var booleanAttrPayload = await DataverseGetJsonAsync(normalizedEnvironmentUrl, token,
            $"EntityDefinitions(LogicalName='{escapedTable}')/Attributes/Microsoft.Dynamics.CRM.BooleanAttributeMetadata?$select=LogicalName,SchemaName,DisplayName&$expand=OptionSet");
        var picklistAttrPayload = await DataverseGetJsonAsync(normalizedEnvironmentUrl, token,
            $"EntityDefinitions(LogicalName='{escapedTable}')/Attributes/Microsoft.Dynamics.CRM.PicklistAttributeMetadata?$select=LogicalName,SchemaName,DisplayName&$expand=OptionSet");
        var multiPicklistAttrPayload = await DataverseGetJsonAsync(normalizedEnvironmentUrl, token,
            $"EntityDefinitions(LogicalName='{escapedTable}')/Attributes/Microsoft.Dynamics.CRM.MultiSelectPicklistAttributeMetadata?$select=LogicalName,SchemaName,DisplayName&$expand=OptionSet");
        var stateAttrPayload = await DataverseGetJsonAsync(normalizedEnvironmentUrl, token,
            $"EntityDefinitions(LogicalName='{escapedTable}')/Attributes/Microsoft.Dynamics.CRM.StateAttributeMetadata?$select=LogicalName,SchemaName,DisplayName&$expand=OptionSet");
        var statusAttrPayload = await DataverseGetJsonAsync(normalizedEnvironmentUrl, token,
            $"EntityDefinitions(LogicalName='{escapedTable}')/Attributes/Microsoft.Dynamics.CRM.StatusAttributeMetadata?$select=LogicalName,SchemaName,DisplayName&$expand=OptionSet");
        var entityNameAttrPayload = await DataverseGetJsonAsync(normalizedEnvironmentUrl, token,
            $"EntityDefinitions(LogicalName='{escapedTable}')/Attributes/Microsoft.Dynamics.CRM.EntityNameAttributeMetadata?$select=MetadataId,LogicalName,SchemaName,DisplayName&$expand=OptionSet");

        var optionAttributePayloads = new List<JsonObject>
        {
            booleanAttrPayload,
            picklistAttrPayload,
            multiPicklistAttrPayload,
            stateAttrPayload,
            statusAttrPayload,
        };

        var viewsPayload = await DataverseGetJsonAsync(
            normalizedEnvironmentUrl,
            token,
            $"savedqueries?$select=savedqueryid,name,returnedtypecode&$filter=returnedtypecode eq '{escapedTable}'");

        var defaultPublicViewPayload = await DataverseGetJsonAsync(
            normalizedEnvironmentUrl,
            token,
            $"savedqueries?$select=layoutxml&$filter=returnedtypecode eq '{escapedTable}' and isdefault eq true&$top=1");

        var formsPayload = await DataverseGetJsonAsync(
            normalizedEnvironmentUrl,
            token,
            $"systemforms?$select=formid,name,objecttypecode,type&$filter=objecttypecode eq '{escapedTable}'");

        var logicalName = entityMetadata["LogicalName"]?.GetValue<string>() ?? normalizedTable;
        var entitySetName = entityMetadata["EntitySetName"]?.GetValue<string>() ?? (logicalName + "s");
        var logicalCollectionName = entityMetadata["LogicalCollectionName"]?.GetValue<string>() ?? entitySetName;
        var displayCollectionName = GetLocalizedLabel(entityMetadata["DisplayCollectionName"], logicalCollectionName);

        var normalizedApiId = string.IsNullOrWhiteSpace(apiId)
            ? "/providers/microsoft.powerapps/apis/shared_commondataserviceforapps"
            : apiId;

        var preferredDataSourceName = ResolvePreferredDataSourceName(
            dataSourceName,
            displayCollectionName,
            logicalName,
            logicalCollectionName,
            entitySetName);

        var displayNameMap = BuildAttributeDisplayNameMap(allAttributes);
        var optionSetEntries = BuildOptionSetEntries(
            optionAttributePayloads,
            normalizedDatasetName,
            displayCollectionName,
            logicalName);

        var viewInfoEntry = BuildViewInfoEntry(
            viewsPayload,
            normalizedDatasetName,
            logicalName,
            displayCollectionName);

        var tableDefinitionObj = new JsonObject
        {
            ["TableName"] = logicalName,
            ["EntityMetadata"] = entityMetadata.ToJsonString(),
            ["PicklistOptionSetAttribute"] = picklistAttrPayload.ToJsonString(),
            ["MultiSelectPicklistOptionSetAttribute"] = multiPicklistAttrPayload.ToJsonString(),
            ["StateOptionSetAttribute"] = stateAttrPayload.ToJsonString(),
            ["StatusOptionSetAttribute"] = statusAttrPayload.ToJsonString(),
            ["BooleanOptionSetAttribute"] = booleanAttrPayload.ToJsonString(),
            ["EntityNameOptionSetAttribute"] = entityNameAttrPayload.ToJsonString(),
            ["Views"] = viewsPayload.ToJsonString(),
            ["DefaultPublicView"] = defaultPublicViewPayload.ToJsonString(),
            ["IconUrl"] = string.Empty,
            ["Forms"] = formsPayload.ToJsonString(),
        };

        var nativeEntry = new JsonObject
        {
            ["Name"] = preferredDataSourceName,
            ["IsSampleData"] = false,
            ["IsWritable"] = true,
            ["Type"] = "NativeCDSDataSourceInfo",
            ["DatasetName"] = normalizedDatasetName,
            ["EntitySetName"] = entitySetName,
            ["LogicalName"] = logicalName,
            ["PreferredName"] = displayCollectionName,
            ["IsHidden"] = false,
            ["WadlMetadata"] = new JsonObject { ["WadlXml"] = string.Empty },
            ["ApiId"] = normalizedApiId,
            ["CdsActionInfo"] = new JsonObject(),
            ["CdsFCBContext"] = JsonValue.Create("{\"FileType\":true,\"ImageExternalStorage\":true}"),
            ["TableDefinition"] = JsonValue.Create(tableDefinitionObj.ToJsonString()),
            ["NativeCDSDataSourceInfoNameMapping"] = displayNameMap,
        };

        var (_, root, dataSources) = OpenDataSourcesDocument(unpackDirectory);
        UpsertDataSourceEntry(dataSources, nativeEntry);
        foreach (var optionSetEntry in optionSetEntries)
        {
            UpsertDataSourceEntry(dataSources, optionSetEntry);
        }

        if (viewInfoEntry is not null)
        {
            UpsertDataSourceEntry(dataSources, viewInfoEntry);
        }

        root["DataSources"] = dataSources;
        SaveDataSourcesDocument(unpackDirectory, root);

        UpsertQualifiedFormsValues(unpackDirectory, normalizedDatasetName, logicalName, displayCollectionName, formsPayload);
    }

    public static async Task UpsertDataverseTableDataSourceAsync(
        string unpackDirectory,
        string environmentUrl,
        string tableLogicalName,
        string accessToken,
        string? dataSourceName = null,
        string? datasetName = null,
        string? apiId = null)
    {
        if (string.IsNullOrWhiteSpace(unpackDirectory) || !Directory.Exists(unpackDirectory))
        {
            throw new DirectoryNotFoundException($"Unpack directory not found: '{unpackDirectory}'.");
        }

        if (string.IsNullOrWhiteSpace(environmentUrl))
        {
            throw new ArgumentException("Environment URL must be provided.", nameof(environmentUrl));
        }

        if (string.IsNullOrWhiteSpace(tableLogicalName))
        {
            throw new ArgumentException("Dataverse table logical name must be provided.", nameof(tableLogicalName));
        }

        if (string.IsNullOrWhiteSpace(accessToken))
        {
            throw new ArgumentException("Access token must be provided.", nameof(accessToken));
        }

        var normalizedEnvironmentUrl = NormalizeEnvironmentUrl(environmentUrl);
        var normalizedTable = tableLogicalName.Trim();
        var escapedTable = EscapeODataLiteral(normalizedTable);
        var normalizedDatasetName = string.IsNullOrWhiteSpace(datasetName) ? "default.cds" : datasetName;

        if (TryRestoreRemovedDataSource(unpackDirectory, dataSourceName, normalizedDatasetName, normalizedTable, out var restoredEntry, out var originalIndex))
        {
            var (_, restoredRoot, restoredDataSources) = OpenDataSourcesDocument(unpackDirectory);
            UpsertDataSourceEntry(restoredDataSources, restoredEntry!, originalIndex);
            restoredRoot["DataSources"] = restoredDataSources;
            SaveDataSourcesDocument(unpackDirectory, restoredRoot);
            return;
        }

        var entityMetadata = await DataverseGetJsonAsync(normalizedEnvironmentUrl, accessToken, $"EntityDefinitions(LogicalName='{escapedTable}')?$select=LogicalName,LogicalCollectionName,EntitySetName,PrimaryIdAttribute,PrimaryNameAttribute,ObjectTypeCode,DisplayName,DisplayCollectionName");
        var allAttributes = await DataverseGetJsonAsync(normalizedEnvironmentUrl, accessToken, $"EntityDefinitions(LogicalName='{escapedTable}')/Attributes?$select=LogicalName,DisplayName");
        var booleanAttrPayload = await DataverseGetJsonAsync(normalizedEnvironmentUrl, accessToken, $"EntityDefinitions(LogicalName='{escapedTable}')/Attributes/Microsoft.Dynamics.CRM.BooleanAttributeMetadata?$select=LogicalName,SchemaName,DisplayName&$expand=OptionSet");
        var picklistAttrPayload = await DataverseGetJsonAsync(normalizedEnvironmentUrl, accessToken, $"EntityDefinitions(LogicalName='{escapedTable}')/Attributes/Microsoft.Dynamics.CRM.PicklistAttributeMetadata?$select=LogicalName,SchemaName,DisplayName&$expand=OptionSet");
        var multiPicklistAttrPayload = await DataverseGetJsonAsync(normalizedEnvironmentUrl, accessToken, $"EntityDefinitions(LogicalName='{escapedTable}')/Attributes/Microsoft.Dynamics.CRM.MultiSelectPicklistAttributeMetadata?$select=LogicalName,SchemaName,DisplayName&$expand=OptionSet");
        var stateAttrPayload = await DataverseGetJsonAsync(normalizedEnvironmentUrl, accessToken, $"EntityDefinitions(LogicalName='{escapedTable}')/Attributes/Microsoft.Dynamics.CRM.StateAttributeMetadata?$select=LogicalName,SchemaName,DisplayName&$expand=OptionSet");
        var statusAttrPayload = await DataverseGetJsonAsync(normalizedEnvironmentUrl, accessToken, $"EntityDefinitions(LogicalName='{escapedTable}')/Attributes/Microsoft.Dynamics.CRM.StatusAttributeMetadata?$select=LogicalName,SchemaName,DisplayName&$expand=OptionSet");
        var entityNameAttrPayload = await DataverseGetJsonAsync(normalizedEnvironmentUrl, accessToken, $"EntityDefinitions(LogicalName='{escapedTable}')/Attributes/Microsoft.Dynamics.CRM.EntityNameAttributeMetadata?$select=MetadataId,LogicalName,SchemaName,DisplayName&$expand=OptionSet");

        var optionAttributePayloads = new List<JsonObject> { booleanAttrPayload, picklistAttrPayload, multiPicklistAttrPayload, stateAttrPayload, statusAttrPayload };

        var viewsPayload = await DataverseGetJsonAsync(normalizedEnvironmentUrl, accessToken, $"savedqueries?$select=savedqueryid,name,returnedtypecode&$filter=returnedtypecode eq '{escapedTable}'");
        var defaultPublicViewPayload = await DataverseGetJsonAsync(normalizedEnvironmentUrl, accessToken, $"savedqueries?$select=layoutxml&$filter=returnedtypecode eq '{escapedTable}' and isdefault eq true&$top=1");
        var formsPayload = await DataverseGetJsonAsync(normalizedEnvironmentUrl, accessToken, $"systemforms?$select=formid,name,objecttypecode,type&$filter=objecttypecode eq '{escapedTable}'");

        var logicalName = entityMetadata["LogicalName"]?.GetValue<string>() ?? normalizedTable;
        var entitySetName = entityMetadata["EntitySetName"]?.GetValue<string>() ?? (logicalName + "s");
        var logicalCollectionName = entityMetadata["LogicalCollectionName"]?.GetValue<string>() ?? entitySetName;
        var displayCollectionName = GetLocalizedLabel(entityMetadata["DisplayCollectionName"], logicalCollectionName);

        var normalizedApiId = string.IsNullOrWhiteSpace(apiId)
            ? "/providers/microsoft.powerapps/apis/shared_commondataserviceforapps"
            : apiId;

        var preferredDataSourceName = ResolvePreferredDataSourceName(dataSourceName, displayCollectionName, logicalName, logicalCollectionName, entitySetName);
        var displayNameMap = BuildAttributeDisplayNameMap(allAttributes);
        var optionSetEntries = BuildOptionSetEntries(optionAttributePayloads, normalizedDatasetName, displayCollectionName, logicalName);
        var viewInfoEntry = BuildViewInfoEntry(viewsPayload, normalizedDatasetName, logicalName, displayCollectionName);

        var tableDefinitionObj = new JsonObject
        {
            ["TableName"] = logicalName,
            ["EntityMetadata"] = entityMetadata.ToJsonString(),
            ["PicklistOptionSetAttribute"] = picklistAttrPayload.ToJsonString(),
            ["MultiSelectPicklistOptionSetAttribute"] = multiPicklistAttrPayload.ToJsonString(),
            ["StateOptionSetAttribute"] = stateAttrPayload.ToJsonString(),
            ["StatusOptionSetAttribute"] = statusAttrPayload.ToJsonString(),
            ["BooleanOptionSetAttribute"] = booleanAttrPayload.ToJsonString(),
            ["EntityNameOptionSetAttribute"] = entityNameAttrPayload.ToJsonString(),
            ["Views"] = viewsPayload.ToJsonString(),
            ["DefaultPublicView"] = defaultPublicViewPayload.ToJsonString(),
            ["IconUrl"] = string.Empty,
            ["Forms"] = formsPayload.ToJsonString(),
        };

        var nativeEntry = new JsonObject
        {
            ["Name"] = preferredDataSourceName,
            ["IsSampleData"] = false,
            ["IsWritable"] = true,
            ["Type"] = "NativeCDSDataSourceInfo",
            ["DatasetName"] = normalizedDatasetName,
            ["EntitySetName"] = entitySetName,
            ["LogicalName"] = logicalName,
            ["PreferredName"] = displayCollectionName,
            ["IsHidden"] = false,
            ["WadlMetadata"] = new JsonObject { ["WadlXml"] = string.Empty },
            ["ApiId"] = normalizedApiId,
            ["CdsActionInfo"] = new JsonObject(),
            ["CdsFCBContext"] = JsonValue.Create("{\"FileType\":true,\"ImageExternalStorage\":true}"),
            ["TableDefinition"] = JsonValue.Create(tableDefinitionObj.ToJsonString()),
            ["NativeCDSDataSourceInfoNameMapping"] = displayNameMap,
        };

        var (_, root, dataSources) = OpenDataSourcesDocument(unpackDirectory);
        UpsertDataSourceEntry(dataSources, nativeEntry);
        foreach (var optionSetEntry in optionSetEntries)
        {
            UpsertDataSourceEntry(dataSources, optionSetEntry);
        }

        if (viewInfoEntry is not null)
        {
            UpsertDataSourceEntry(dataSources, viewInfoEntry);
        }

        root["DataSources"] = dataSources;
        SaveDataSourcesDocument(unpackDirectory, root);

        UpsertQualifiedFormsValues(unpackDirectory, normalizedDatasetName, logicalName, displayCollectionName, formsPayload);
    }

    private static async Task<string> AcquireDataverseAccessTokenAsync(string environmentUrl)
    {
        var uri = new Uri(environmentUrl);
        var scope = $"{uri.Scheme}://{uri.Host}/.default";
        var credential = new DeviceCodeCredential(new DeviceCodeCredentialOptions
        {
            TenantId = "organizations",
            ClientId = "04b07795-8ddb-461a-bbee-02f9e1bf7b46",
            DeviceCodeCallback = (code, _) =>
            {
                Console.WriteLine(code.Message);
                return Task.CompletedTask;
            },
        });

        var token = await credential.GetTokenAsync(new TokenRequestContext([scope]));
        return token.Token;
    }

    private static async Task<JsonObject> DataverseGetJsonAsync(string environmentUrl, string accessToken, string relativePath)
    {
        var baseUrl = environmentUrl.TrimEnd('/');
        var url = relativePath.StartsWith("http://", StringComparison.OrdinalIgnoreCase)
                  || relativePath.StartsWith("https://", StringComparison.OrdinalIgnoreCase)
            ? relativePath
            : $"{baseUrl}/api/data/v9.2/{relativePath}";

        using var req = new HttpRequestMessage(HttpMethod.Get, url);
        req.Headers.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);
        req.Headers.Accept.Clear();
        req.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
        req.Headers.Add("OData-MaxVersion", "4.0");
        req.Headers.Add("OData-Version", "4.0");

        using var res = await DataverseHttpClient.SendAsync(req);
        var body = await res.Content.ReadAsStringAsync();
        if (!res.IsSuccessStatusCode)
        {
            throw new InvalidDataException($"Dataverse request failed ({(int)res.StatusCode} {res.ReasonPhrase}) for '{relativePath}'. Response: {body}");
        }

        return JsonNode.Parse(body) as JsonObject
               ?? throw new InvalidDataException($"Dataverse response was not a JSON object for '{relativePath}'.");
    }

    private static List<JsonObject> BuildOptionSetEntries(
        IEnumerable<JsonObject> optionAttributePayloads,
        string datasetName,
        string relatedEntityDisplayName,
        string relatedEntityLogicalName)
    {
        var results = new List<JsonObject>();
        foreach (var payload in optionAttributePayloads)
        {
            var values = payload["value"] as JsonArray;
            if (values is null)
            {
                continue;
            }

            foreach (var attr in values.OfType<JsonObject>())
            {
                var logicalName = attr["LogicalName"]?.GetValue<string>() ?? string.Empty;
                if (string.IsNullOrWhiteSpace(logicalName))
                {
                    continue;
                }

                var optionSet = attr["OptionSet"] as JsonObject;
                if (optionSet is null)
                {
                    continue;
                }

                var optionSetType = optionSet["OptionSetType"]?.GetValue<string>() ?? string.Empty;
                var optionSetName = optionSet["Name"]?.GetValue<string>() ?? $"{relatedEntityLogicalName}_{logicalName}";
                var displayName = GetLocalizedLabel(attr["DisplayName"], logicalName);

                var mapping = new JsonObject();
                var isBoolean = string.Equals(optionSetType, "Boolean", StringComparison.OrdinalIgnoreCase);
                if (isBoolean)
                {
                    AddBooleanOption(optionSet, "TrueOption", mapping);
                    AddBooleanOption(optionSet, "FalseOption", mapping);
                }
                else
                {
                    var options = optionSet["Options"] as JsonArray;
                    if (options is not null)
                    {
                        foreach (var option in options.OfType<JsonObject>())
                        {
                            var value = option["Value"]?.GetValue<int?>();
                            if (value is null)
                            {
                                continue;
                            }

                            var label = GetLocalizedLabel(option["Label"], value.Value.ToString(CultureInfo.InvariantCulture));
                            mapping[value.Value.ToString(CultureInfo.InvariantCulture)] = label;
                        }
                    }
                }

                if (mapping.Count == 0)
                {
                    continue;
                }

                results.Add(new JsonObject
                {
                    ["Type"] = "OptionSetInfo",
                    ["Name"] = optionSetName,
                    ["DatasetName"] = datasetName,
                    ["DisplayName"] = $"{displayName} ({relatedEntityDisplayName})",
                    ["RelatedEntityName"] = relatedEntityDisplayName,
                    ["RelatedColumnInvariantName"] = logicalName,
                    ["OptionSetInfoNameMapping"] = mapping,
                    ["OptionSetIsGlobal"] = optionSet["IsGlobal"]?.GetValue<bool?>() ?? false,
                    ["OptionSetIsBooleanValued"] = isBoolean,
                    ["OptionSetTypeKey"] = ToOptionSetTypeKey(optionSetType),
                    ["OptionSetReference"] = new JsonObject
                    {
                        ["OptionSetReferenceItem0"] = new JsonObject
                        {
                            ["OptionSetReferenceEntityName"] = relatedEntityDisplayName,
                            ["OptionSetReferenceColumnName"] = logicalName,
                        },
                    },
                });
            }
        }

        return results;
    }

    private static void AddBooleanOption(JsonObject optionSet, string optionPropertyName, JsonObject output)
    {
        var option = optionSet[optionPropertyName] as JsonObject;
        if (option is null)
        {
            return;
        }

        var value = option["Value"]?.GetValue<int?>();
        if (value is null)
        {
            return;
        }

        var label = GetLocalizedLabel(option["Label"], value.Value.ToString(CultureInfo.InvariantCulture));
        output[value.Value.ToString(CultureInfo.InvariantCulture)] = label;
    }

    private static JsonObject? BuildViewInfoEntry(JsonObject viewsPayload, string datasetName, string logicalName, string displayCollectionName)
    {
        var values = viewsPayload["value"] as JsonArray;
        if (values is null || values.Count == 0)
        {
            return null;
        }

        var map = new JsonObject();
        foreach (var view in values.OfType<JsonObject>())
        {
            var id = view["savedqueryid"]?.GetValue<string>() ?? string.Empty;
            var name = view["name"]?.GetValue<string>() ?? string.Empty;
            if (string.IsNullOrWhiteSpace(id) || string.IsNullOrWhiteSpace(name))
            {
                continue;
            }

            map[id] = name;
        }

        if (map.Count == 0)
        {
            return null;
        }

        return new JsonObject
        {
            ["Type"] = "ViewInfo",
            ["Name"] = $"{datasetName}_{logicalName}_views",
            ["DisplayName"] = $"{displayCollectionName} (Views)",
            ["RelatedEntityName"] = displayCollectionName,
            ["ViewInfoNameMapping"] = map,
        };
    }

    private static JsonObject BuildAttributeDisplayNameMap(JsonObject allAttributesPayload)
    {
        var map = new JsonObject();
        var values = allAttributesPayload["value"] as JsonArray;
        if (values is null)
        {
            return map;
        }

        foreach (var attr in values.OfType<JsonObject>())
        {
            var logical = attr["LogicalName"]?.GetValue<string>() ?? string.Empty;
            if (string.IsNullOrWhiteSpace(logical))
            {
                continue;
            }

            var display = GetLocalizedLabel(attr["DisplayName"], logical);
            map[logical] = display;
        }

        return map;
    }

    private static void UpsertQualifiedFormsValues(
        string unpackDirectory,
        string datasetName,
        string logicalName,
        string displayCollectionName,
        JsonObject formsPayload)
    {
        var forms = formsPayload["value"] as JsonArray;
        if (forms is null || forms.Count == 0)
        {
            return;
        }

        var values = new JsonObject();
        foreach (var form in forms.OfType<JsonObject>())
        {
            var formName = form["name"]?.GetValue<string>() ?? string.Empty;
            var formId = form["formid"]?.GetValue<string>() ?? string.Empty;
            if (string.IsNullOrWhiteSpace(formName) || string.IsNullOrWhiteSpace(formId))
            {
                continue;
            }

            values[formName] = formId;
        }

        if (values.Count == 0)
        {
            return;
        }

        var path = Path.Combine(unpackDirectory, "References", "QualifiedValues.json");
        Directory.CreateDirectory(Path.GetDirectoryName(path)!);

        JsonObject root;
        JsonArray arr;
        if (File.Exists(path)
            && JsonNode.Parse(File.ReadAllText(path)) is JsonObject existingRoot
            && existingRoot["QualifiedValues"] is JsonArray existingArray)
        {
            root = existingRoot;
            arr = existingArray;
        }
        else
        {
            root = new JsonObject();
            arr = new JsonArray();
            root["QualifiedValues"] = arr;
        }

        var ns = $"{displayCollectionName} (Forms)";
        var kind = $"{datasetName}_{logicalName}_forms";
        var newEntry = new JsonObject
        {
            ["Namespace"] = ns,
            ["Kind"] = kind,
            ["Values"] = values,
        };

        var replaced = false;
        for (var i = 0; i < arr.Count; i++)
        {
            if (arr[i] is not JsonObject existing)
            {
                continue;
            }

            var existingNs = existing["Namespace"]?.GetValue<string>() ?? string.Empty;
            var existingKind = existing["Kind"]?.GetValue<string>() ?? string.Empty;
            if (!string.Equals(existingNs, ns, StringComparison.OrdinalIgnoreCase)
                || !string.Equals(existingKind, kind, StringComparison.OrdinalIgnoreCase))
            {
                continue;
            }

            arr[i] = newEntry;
            replaced = true;
            break;
        }

        if (!replaced)
        {
            arr.Add(newEntry);
        }

        root["QualifiedValues"] = arr;
        File.WriteAllText(path, root.ToJsonString(JsonOptions));
    }

    private static string NormalizeEnvironmentUrl(string environmentUrl)
    {
        var uri = new Uri(environmentUrl, UriKind.Absolute);
        return $"{uri.Scheme}://{uri.Host}";
    }

    private static string EscapeODataLiteral(string value)
    {
        return value.Replace("'", "''");
    }

    private static string ToOptionSetTypeKey(string optionSetType)
    {
        if (string.Equals(optionSetType, "Boolean", StringComparison.OrdinalIgnoreCase))
        {
            return "BooleanType";
        }

        if (string.Equals(optionSetType, "State", StringComparison.OrdinalIgnoreCase))
        {
            return "StateType";
        }

        if (string.Equals(optionSetType, "Status", StringComparison.OrdinalIgnoreCase))
        {
            return "StatusType";
        }

        if (string.Equals(optionSetType, "MultiSelectPicklist", StringComparison.OrdinalIgnoreCase))
        {
            return "MultiSelectPicklistType";
        }

        return "PicklistType";
    }

    private static string ResolvePreferredDataSourceName(
        string? requestedDataSourceName,
        string displayCollectionName,
        string logicalName,
        string logicalCollectionName,
        string entitySetName)
    {
        if (string.IsNullOrWhiteSpace(requestedDataSourceName))
        {
            return displayCollectionName;
        }

        var requested = requestedDataSourceName.Trim();
        var canonicalRequested = NormalizeSymbolName(requested);

        var knownGeneratedNames = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
        {
            NormalizeSymbolName(logicalName),
            NormalizeSymbolName(logicalCollectionName),
            NormalizeSymbolName(entitySetName),
            NormalizeSymbolName(ToPascalCase(logicalName)),
            NormalizeSymbolName(ToPascalCase(logicalCollectionName)),
            NormalizeSymbolName(ToPascalCase(entitySetName))
        };

        if (knownGeneratedNames.Contains(canonicalRequested))
        {
            return displayCollectionName;
        }

        return requested;
    }

    private static string NormalizeSymbolName(string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return string.Empty;
        }

        var cleaned = Regex.Replace(value, "[^A-Za-z0-9]", string.Empty);
        return cleaned;
    }

    private static string ToPascalCase(string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return string.Empty;
        }

        var parts = Regex.Split(value.Trim(), "[^A-Za-z0-9]+")
            .Where(p => !string.IsNullOrWhiteSpace(p))
            .ToArray();

        if (parts.Length == 0)
        {
            return string.Empty;
        }

        var sb = new StringBuilder();
        foreach (var part in parts)
        {
            var lower = part.ToLowerInvariant();
            sb.Append(char.ToUpperInvariant(lower[0]));
            if (lower.Length > 1)
            {
                sb.Append(lower, 1, lower.Length - 1);
            }
        }

        return sb.ToString();
    }

    private static string GetLocalizedLabel(JsonNode? labelNode, string fallback)
    {
        if (labelNode is JsonObject labelObject)
        {
            var userLabel = labelObject["UserLocalizedLabel"] as JsonObject;
            var userText = userLabel?["Label"]?.GetValue<string>();
            if (!string.IsNullOrWhiteSpace(userText))
            {
                return userText;
            }

            var localized = labelObject["LocalizedLabels"] as JsonArray;
            var firstText = localized?
                .OfType<JsonObject>()
                .Select(o => o["Label"]?.GetValue<string>() ?? string.Empty)
                .FirstOrDefault(s => !string.IsNullOrWhiteSpace(s));
            if (!string.IsNullOrWhiteSpace(firstText))
            {
                return firstText;
            }
        }

        return fallback;
    }
}