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
    private const string DataverseWadlServiceNamespace = "https://org60220130.crm11.dynamics.com/api/data/v9.0";
    private const string DataverseWadlServiceId = "ODataServicefornamespaceMicrosoft.Dynamics.CRM";
    private const string PublicCloudPowerAppsAuthResource = "https://service.powerapps.com/";
    private static readonly HashSet<string> ExcludedAttributeNameMappingKeys = new(StringComparer.OrdinalIgnoreCase)
    {
        "createdbyexternalparty",
        "modifiedbyexternalparty",
        "isprivate",
        "isautocreate",
        "ownerid",
        "owneridtype",
        "slaid",
        "slainvokedid",
        "subscriptionid",
        "parentcustomerid",
        "parentcustomeridname",
        "parentcustomeridtype",
        "parentcustomeridyominame",
        "parentcontactid",
    };

    private static readonly HashSet<string> RelationshipNameMappingAllowList = new(StringComparer.OrdinalIgnoreCase)
    {
        "lk_contact_feedback_createdby",
        "lk_contact_feedback_createdonbehalfby",
    };

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
        var preferredInsertIndex = -1;
        JsonObject? restoredNativeEntry = null;
        if (TryRestoreRemovedDataSource(unpackDirectory, dataSourceName, normalizedDatasetName, normalizedTable, out var restoredEntry, out var originalIndex))
        {
            preferredInsertIndex = originalIndex;
            restoredNativeEntry = restoredEntry;
        }

        var token = await AcquireDataverseAccessTokenAsync(normalizedEnvironmentUrl);

        var entityMetadata = await GetPowerAppsEntityMetadataAsync(
            normalizedEnvironmentUrl,
            token,
            escapedTable);

        var allAttributes = await DataverseGetJsonAsync(
            normalizedEnvironmentUrl,
            token,
            $"EntityDefinitions(LogicalName='{escapedTable}')/Attributes?$select=LogicalName,DisplayName,AttributeOf");

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

        var rawViewsPayload = await DataverseGetJsonAsync(
            normalizedEnvironmentUrl,
            token,
            $"savedqueries?$select=savedqueryid,name,returnedtypecode,queryapi&$filter=returnedtypecode eq '{escapedTable}' and isprivate eq false and isquickfindquery eq false and statecode eq 0 and (querytype eq 0 or querytype eq 1 or querytype eq 2 or querytype eq 64)");
        var viewsPayload = FilterSavedQueryPayload(rawViewsPayload);

        var defaultPublicViewPayload = await DataverseGetJsonAsync(
            normalizedEnvironmentUrl,
            token,
            $"savedqueries?$select=layoutxml&$filter=returnedtypecode eq '{escapedTable}' and isdefault eq true and isprivate eq false and isquickfindquery eq false and statecode eq 0 and (querytype eq 0 or querytype eq 1 or querytype eq 2 or querytype eq 64)&$top=1");

        var formsPayload = await DataverseGetJsonAsync(
            normalizedEnvironmentUrl,
            token,
            $"systemforms?$select=formid,name,objecttypecode,type&$filter=objecttypecode eq '{escapedTable}'");

        var logicalName = entityMetadata["LogicalName"]?.GetValue<string>() ?? normalizedTable;
        var entitySetName = entityMetadata["EntitySetName"]?.GetValue<string>() ?? (logicalName + "s");
        var logicalCollectionName = entityMetadata["LogicalCollectionName"]?.GetValue<string>() ?? entitySetName;
        var displayCollectionName = GetLocalizedLabel(entityMetadata["DisplayCollectionName"], logicalCollectionName);

        var preferredDataSourceName = ResolvePreferredDataSourceName(
            dataSourceName,
            displayCollectionName,
            logicalName,
            logicalCollectionName,
            entitySetName);
        if (string.IsNullOrWhiteSpace(dataSourceName)
            && restoredNativeEntry?["Name"]?.GetValue<string>() is { Length: > 0 } restoredName)
        {
            preferredDataSourceName = restoredName;
        }

        var normalizedApiId = string.IsNullOrWhiteSpace(apiId)
            ? $"/{logicalName}"
            : apiId;

        var relatedEntityDisplayCollections = await BuildRelatedEntityDisplayCollectionNameMapAsync(
            normalizedEnvironmentUrl,
            token,
            entityMetadata,
            logicalName);
        var displayNameMap = BuildAttributeDisplayNameMap(allAttributes, entityMetadata, logicalName, relatedEntityDisplayCollections);
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

        var (_, root, dataSources) = OpenDataSourcesDocument(unpackDirectory);
        var existingNativeEntry = dataSources
            .OfType<JsonObject>()
            .FirstOrDefault(ds => string.Equals(ds["Type"]?.GetValue<string>() ?? string.Empty, "NativeCDSDataSourceInfo", StringComparison.OrdinalIgnoreCase)
                                  && string.Equals(ds["Name"]?.GetValue<string>() ?? string.Empty, preferredDataSourceName, StringComparison.OrdinalIgnoreCase));
        var hasAnyNativeEntry = dataSources
            .OfType<JsonObject>()
            .Any(ds => string.Equals(ds["Type"]?.GetValue<string>() ?? string.Empty, "NativeCDSDataSourceInfo", StringComparison.OrdinalIgnoreCase));
        var includeConnectorMetadata = existingNativeEntry is not null
            ? existingNativeEntry["ApiId"] is not null
              || existingNativeEntry["WadlMetadata"] is not null
              || existingNativeEntry["CdsActionInfo"] is not null
            : hasAnyNativeEntry;

            string? connectorWadlXml = null;
            if (includeConnectorMetadata)
            {
                connectorWadlXml = await BuildDataverseWadlXmlAsync(
                normalizedEnvironmentUrl,
                token,
                "v9.0",
                logicalName,
                entitySetName,
                entityMetadata["PrimaryIdAttribute"]?.GetValue<string>() ?? $"{logicalName}id");
            }

        var tableEntityMetadata = entityMetadata.DeepClone() as JsonObject ?? new JsonObject();
        tableEntityMetadata.Remove("DisplayName");
        tableEntityMetadata.Remove("LogicalCollectionName");

        var tableDefinitionObj = new JsonObject
        {
            ["TableName"] = logicalName,
            ["EntityMetadata"] = tableEntityMetadata.ToJsonString(),
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
            ["CdsFCBContext"] = JsonValue.Create("{\"FileType\":true,\"ImageExternalStorage\":true}"),
            ["TableDefinition"] = JsonValue.Create(tableDefinitionObj.ToJsonString()),
            ["NativeCDSDataSourceInfoNameMapping"] = displayNameMap,
        };

        if (includeConnectorMetadata)
        {
            nativeEntry["WadlMetadata"] = new JsonObject { ["WadlXml"] = connectorWadlXml ?? string.Empty };
            nativeEntry["ApiId"] = normalizedApiId;
            nativeEntry["CdsActionInfo"] = new JsonObject
            {
                ["IsUnboundAction"] = false,
                ["CdsDataset"] = normalizedDatasetName,
            };
        }

        UpsertDataSourceEntry(dataSources, nativeEntry, preferredInsertIndex);
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
        string? apiId = null,
        Func<string, Task<string>>? accessTokenProvider = null)
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
        var preferredInsertIndex = -1;
        JsonObject? restoredNativeEntry = null;
        if (TryRestoreRemovedDataSource(unpackDirectory, dataSourceName, normalizedDatasetName, normalizedTable, out var restoredEntry, out var originalIndex))
        {
            preferredInsertIndex = originalIndex;
            restoredNativeEntry = restoredEntry;
        }

        var entityMetadata = await GetPowerAppsEntityMetadataAsync(normalizedEnvironmentUrl, accessToken, escapedTable);
        var allAttributes = await DataverseGetJsonAsync(normalizedEnvironmentUrl, accessToken, $"EntityDefinitions(LogicalName='{escapedTable}')/Attributes?$select=LogicalName,DisplayName,AttributeOf");
        var booleanAttrPayload = await DataverseGetJsonAsync(normalizedEnvironmentUrl, accessToken, $"EntityDefinitions(LogicalName='{escapedTable}')/Attributes/Microsoft.Dynamics.CRM.BooleanAttributeMetadata?$select=LogicalName,SchemaName,DisplayName&$expand=OptionSet");
        var picklistAttrPayload = await DataverseGetJsonAsync(normalizedEnvironmentUrl, accessToken, $"EntityDefinitions(LogicalName='{escapedTable}')/Attributes/Microsoft.Dynamics.CRM.PicklistAttributeMetadata?$select=LogicalName,SchemaName,DisplayName&$expand=OptionSet");
        var multiPicklistAttrPayload = await DataverseGetJsonAsync(normalizedEnvironmentUrl, accessToken, $"EntityDefinitions(LogicalName='{escapedTable}')/Attributes/Microsoft.Dynamics.CRM.MultiSelectPicklistAttributeMetadata?$select=LogicalName,SchemaName,DisplayName&$expand=OptionSet");
        var stateAttrPayload = await DataverseGetJsonAsync(normalizedEnvironmentUrl, accessToken, $"EntityDefinitions(LogicalName='{escapedTable}')/Attributes/Microsoft.Dynamics.CRM.StateAttributeMetadata?$select=LogicalName,SchemaName,DisplayName&$expand=OptionSet");
        var statusAttrPayload = await DataverseGetJsonAsync(normalizedEnvironmentUrl, accessToken, $"EntityDefinitions(LogicalName='{escapedTable}')/Attributes/Microsoft.Dynamics.CRM.StatusAttributeMetadata?$select=LogicalName,SchemaName,DisplayName&$expand=OptionSet");
        var entityNameAttrPayload = await DataverseGetJsonAsync(normalizedEnvironmentUrl, accessToken, $"EntityDefinitions(LogicalName='{escapedTable}')/Attributes/Microsoft.Dynamics.CRM.EntityNameAttributeMetadata?$select=MetadataId,LogicalName,SchemaName,DisplayName&$expand=OptionSet");

        var optionAttributePayloads = new List<JsonObject> { booleanAttrPayload, picklistAttrPayload, multiPicklistAttrPayload, stateAttrPayload, statusAttrPayload };

        var rawViewsPayload = await DataverseGetJsonAsync(normalizedEnvironmentUrl, accessToken, $"savedqueries?$select=savedqueryid,name,returnedtypecode,queryapi&$filter=returnedtypecode eq '{escapedTable}' and isprivate eq false and isquickfindquery eq false and statecode eq 0 and (querytype eq 0 or querytype eq 1 or querytype eq 2 or querytype eq 64)");
        var viewsPayload = FilterSavedQueryPayload(rawViewsPayload);
        var defaultPublicViewPayload = await DataverseGetJsonAsync(normalizedEnvironmentUrl, accessToken, $"savedqueries?$select=layoutxml&$filter=returnedtypecode eq '{escapedTable}' and isdefault eq true and isprivate eq false and isquickfindquery eq false and statecode eq 0 and (querytype eq 0 or querytype eq 1 or querytype eq 2 or querytype eq 64)&$top=1");
        var formsPayload = await DataverseGetJsonAsync(normalizedEnvironmentUrl, accessToken, $"systemforms?$select=formid,name,objecttypecode,type&$filter=objecttypecode eq '{escapedTable}'");

        var logicalName = entityMetadata["LogicalName"]?.GetValue<string>() ?? normalizedTable;
        var entitySetName = entityMetadata["EntitySetName"]?.GetValue<string>() ?? (logicalName + "s");
        var logicalCollectionName = entityMetadata["LogicalCollectionName"]?.GetValue<string>() ?? entitySetName;
        var displayCollectionName = GetLocalizedLabel(entityMetadata["DisplayCollectionName"], logicalCollectionName);

        var preferredDataSourceName = ResolvePreferredDataSourceName(dataSourceName, displayCollectionName, logicalName, logicalCollectionName, entitySetName);
        if (string.IsNullOrWhiteSpace(dataSourceName)
            && restoredNativeEntry?["Name"]?.GetValue<string>() is { Length: > 0 } restoredName)
        {
            preferredDataSourceName = restoredName;
        }
        var normalizedApiId = string.IsNullOrWhiteSpace(apiId)
            ? $"/{logicalName}"
            : apiId;
        var relatedEntityDisplayCollections = await BuildRelatedEntityDisplayCollectionNameMapAsync(
            normalizedEnvironmentUrl,
            accessToken,
            entityMetadata,
            logicalName);
        var displayNameMap = BuildAttributeDisplayNameMap(allAttributes, entityMetadata, logicalName, relatedEntityDisplayCollections);
        var optionSetEntries = BuildOptionSetEntries(optionAttributePayloads, normalizedDatasetName, displayCollectionName, logicalName);
        var viewInfoEntry = BuildViewInfoEntry(viewsPayload, normalizedDatasetName, logicalName, displayCollectionName);

        var (_, root, dataSources) = OpenDataSourcesDocument(unpackDirectory);
        var existingNativeEntry = dataSources
            .OfType<JsonObject>()
            .FirstOrDefault(ds => string.Equals(ds["Type"]?.GetValue<string>() ?? string.Empty, "NativeCDSDataSourceInfo", StringComparison.OrdinalIgnoreCase)
                                  && string.Equals(ds["Name"]?.GetValue<string>() ?? string.Empty, preferredDataSourceName, StringComparison.OrdinalIgnoreCase));
        var hasAnyNativeEntry = dataSources
            .OfType<JsonObject>()
            .Any(ds => string.Equals(ds["Type"]?.GetValue<string>() ?? string.Empty, "NativeCDSDataSourceInfo", StringComparison.OrdinalIgnoreCase));
        var includeConnectorMetadata = existingNativeEntry is not null
            ? existingNativeEntry["ApiId"] is not null
              || existingNativeEntry["WadlMetadata"] is not null
              || existingNativeEntry["CdsActionInfo"] is not null
            : hasAnyNativeEntry;

            string? connectorWadlXml = null;
            if (includeConnectorMetadata)
            {
                var powerAppsApiAccessToken = await ResolvePowerAppsApiAccessTokenAsync(accessToken, accessTokenProvider);
                connectorWadlXml = await BuildDataverseWadlXmlAsync(
                normalizedEnvironmentUrl,
                accessToken,
                "v9.0",
                logicalName,
                entitySetName,
                entityMetadata["PrimaryIdAttribute"]?.GetValue<string>() ?? $"{logicalName}id",
                powerAppsApiAccessToken);
            }

        var tableEntityMetadata = entityMetadata.DeepClone() as JsonObject ?? new JsonObject();
        tableEntityMetadata.Remove("DisplayName");
        tableEntityMetadata.Remove("LogicalCollectionName");

        var tableDefinitionObj = new JsonObject
        {
            ["TableName"] = logicalName,
            ["EntityMetadata"] = tableEntityMetadata.ToJsonString(),
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
            ["CdsFCBContext"] = JsonValue.Create("{\"FileType\":true,\"ImageExternalStorage\":true}"),
            ["TableDefinition"] = JsonValue.Create(tableDefinitionObj.ToJsonString()),
            ["NativeCDSDataSourceInfoNameMapping"] = displayNameMap,
        };

        if (includeConnectorMetadata)
        {
            nativeEntry["WadlMetadata"] = new JsonObject { ["WadlXml"] = connectorWadlXml ?? string.Empty };
            nativeEntry["ApiId"] = normalizedApiId;
            nativeEntry["CdsActionInfo"] = new JsonObject
            {
                ["IsUnboundAction"] = false,
                ["CdsDataset"] = normalizedDatasetName,
            };
        }

        UpsertDataSourceEntry(dataSources, nativeEntry, preferredInsertIndex);
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

    private static async Task<string> DataverseGetTextAsync(string absoluteUrl, string accessToken, string accept)
    {
        using var req = new HttpRequestMessage(HttpMethod.Get, absoluteUrl);
        req.Headers.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);
        req.Headers.Accept.Clear();
        req.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue(accept));
        req.Headers.Add("OData-MaxVersion", "4.0");
        req.Headers.Add("OData-Version", "4.0");

        using var res = await DataverseHttpClient.SendAsync(req);
        var body = await res.Content.ReadAsStringAsync();
        if (!res.IsSuccessStatusCode)
        {
            throw new InvalidDataException(
                $"Dataverse request failed ({(int)res.StatusCode} {res.ReasonPhrase}) for '{absoluteUrl}'. Response: {body}");
        }

        return body;
    }

    private static async Task<string> BuildDataverseWadlXmlAsync(
        string environmentUrl,
        string accessToken,
        string apiVersion,
        string tableLogicalName,
        string entitySetName,
        string primaryIdAttribute,
        string? powerAppsApiAccessToken = null)
    {
        var baseUrl = environmentUrl.TrimEnd('/');
        var serviceRoot = $"{baseUrl}/api/data/{apiVersion.Trim().Trim('/')}";
        var metadataUrl = $"{serviceRoot}/$metadata";

        // Strict/no-fallback behavior: this call must succeed when connector metadata is expected.
        var metadataXml = await DataverseGetTextAsync(metadataUrl, accessToken, "application/xml");
        return BuildDataverseBoundActionWadlXml(
            metadataXml,
            serviceRoot,
            tableLogicalName,
            entitySetName,
            primaryIdAttribute);
    }

    private static async Task<string> ResolvePowerAppsApiAccessTokenAsync(
        string dataverseAccessToken,
        Func<string, Task<string>>? accessTokenProvider)
    {
        if (accessTokenProvider is null)
        {
            return dataverseAccessToken;
        }

        try
        {
            var token = await accessTokenProvider(PublicCloudPowerAppsAuthResource);
            if (!string.IsNullOrWhiteSpace(token))
            {
                return token;
            }
        }
        catch
        {
            // Fall through to Dataverse token fallback.
        }

        return dataverseAccessToken;
    }

    private static string BuildDataverseBoundActionWadlXml(
        string metadataXml,
        string serviceRoot,
        string tableLogicalName,
        string entitySetName,
        string primaryIdAttribute)
    {
        if (string.IsNullOrWhiteSpace(metadataXml))
        {
            throw new InvalidDataException("Dataverse $metadata response was empty.");
        }

        var metadataDoc = XDocument.Parse(metadataXml, LoadOptions.PreserveWhitespace);
        var edmNs = XNamespace.Get("http://docs.oasis-open.org/odata/ns/edm");
        var wadlNs = XNamespace.Get("http://wadl.dev.java.net/2009/02");
        var sienaNs = XNamespace.Get("http://schemas.microsoft.com/MicrosoftProjectSiena/WADL/2014/11");

        var actions = metadataDoc
            .Descendants(edmNs + "Action")
            .OfType<XElement>()
            .Where(a => string.Equals(a.Attribute("IsBound")?.Value, "true", StringComparison.OrdinalIgnoreCase))
            .Select(a => new
            {
                Element = a,
                Name = a.Attribute("Name")?.Value ?? string.Empty,
                Parameters = a.Elements(edmNs + "Parameter").ToList(),
            })
            .Where(a => !string.IsNullOrWhiteSpace(a.Name) && a.Parameters.Count > 0)
            .Where(a => IsEntityBindingParameter(a.Parameters[0].Attribute("Type")?.Value, tableLogicalName))
            .OrderBy(a => a.Name, StringComparer.OrdinalIgnoreCase)
            .ToList();

        var normalizedServiceRoot = serviceRoot.TrimEnd('/');
        var normalizedEntitySetName = entitySetName.Trim();
        var normalizedPrimaryId = primaryIdAttribute.Trim();

        var jsonTypesElement = new XElement(
            sienaNs + "jsonTypes",
            new XAttribute("xmlns", sienaNs.NamespaceName),
            new XAttribute("targetNamespace", normalizedServiceRoot),
            new XElement(sienaNs + "untypedObject", new XAttribute("name", "Microsoft_Dynamics_CRM_expando")));

        foreach (var typeElement in BuildWadlJsonTypeElements(metadataDoc, edmNs, sienaNs))
        {
            jsonTypesElement.Add(typeElement);
        }

        var app = new XElement(
            wadlNs + "application",
            new XAttribute(XNamespace.Xmlns + "xml", "http://www.w3.org/XML/1998/namespace"),
            new XAttribute(XNamespace.Xmlns + "xs", "http://www.w3.org/2001/XMLSchema"),
            new XAttribute(XNamespace.Xmlns + "service", normalizedServiceRoot),
            new XAttribute(XNamespace.Xmlns + "siena", "http://schemas.microsoft.com/MicrosoftProjectSiena/WADL/2014/11"),
            new XAttribute(XName.Get("serviceId", "http://schemas.microsoft.com/MicrosoftProjectSiena/WADL/2014/11"), DataverseWadlServiceId),
            new XElement(wadlNs + "doc", new XAttribute("title", "OData Service for namespace Microsoft.Dynamics.CRM"), "This OData service is located at http://localhost"),
            new XElement(wadlNs + "grammars", jsonTypesElement));

        var resourcesElement = new XElement(
            wadlNs + "resources",
            new XAttribute(XName.Get("authenticationProviderHref", sienaNs.NamespaceName), "#PowerAppAuth"),
            new XAttribute("base", normalizedServiceRoot));

        foreach (var action in actions)
        {
            var actionPath = $"/{normalizedEntitySetName}({{{normalizedPrimaryId}}})/Microsoft.Dynamics.CRM.{action.Name}";
            var resourceElement = new XElement(
                wadlNs + "resource",
                new XAttribute("path", actionPath),
                new XElement(
                    wadlNs + "param",
                    new XAttribute("style", "template"),
                    new XAttribute("name", normalizedPrimaryId),
                    new XAttribute("type", "xs:string"),
                    new XAttribute("required", "true"),
                    new XElement(wadlNs + "doc", new XAttribute("title", $"key: {normalizedPrimaryId}"))));
            var methodElement = new XElement(
                wadlNs + "method",
                new XAttribute(XName.Get("requiresAuthentication", sienaNs.NamespaceName), "true"),
                new XAttribute("name", "POST"),
                new XAttribute("id", action.Name),
                new XAttribute("actionName", action.Name),
                new XAttribute(XName.Get("isDeprecated", sienaNs.NamespaceName), "false"),
                new XElement(wadlNs + "doc", new XAttribute("title", $"Invoke action {action.Name}")));

            var actionParameters = action.Parameters
                .Skip(1)
                .Select(p => new
                {
                    Name = p.Attribute("Name")?.Value ?? string.Empty,
                    Type = p.Attribute("Type")?.Value ?? string.Empty,
                })
                .Where(p => !string.IsNullOrWhiteSpace(p.Name))
                .ToList();

            if (actionParameters.Count > 0)
            {
                var representationElement = new XElement(wadlNs + "representation", new XAttribute("mediaType", "application/json"));
                foreach (var parameter in actionParameters)
                {
                    representationElement.Add(
                        new XElement(
                            wadlNs + "param",
                            new XAttribute("style", "plain"),
                            new XAttribute("name", parameter.Name),
                            new XAttribute("path", $"/{parameter.Name}"),
                            new XAttribute("type", MapODataTypeToWadlXsType(parameter.Type))));
                }

                methodElement.Add(new XElement(wadlNs + "request", representationElement));
            }

            resourceElement.Add(methodElement);
            resourcesElement.Add(resourceElement);
        }

        app.Add(resourcesElement);
        app.Add(
            new XElement(
                sienaNs + "authenticationProviders",
                new XAttribute("xmlns", sienaNs.NamespaceName),
                new XElement(
                    sienaNs + "connectionProvider",
                    new XAttribute("id", "PowerAppAuth"),
                    new XAttribute(XName.Get("connectionProviderId", sienaNs.NamespaceName), tableLogicalName.Trim()))));

        app.Add(
            new XElement(
                sienaNs + "template",
                new XAttribute("xmlns", sienaNs.NamespaceName),
                new XElement(
                    sienaNs + "variable",
                    new XAttribute("name", "connectionId"),
                    new XElement(
                        sienaNs + "modifyParams",
                        new XAttribute("style", "template"),
                        new XAttribute("name", "connectionId"),
                        new XAttribute("attribute", "fixed")))));

        var wadl = new XDocument(new XDeclaration("1.0", "utf-8", null), app).ToString(SaveOptions.DisableFormatting);
        if (string.IsNullOrWhiteSpace(wadl))
        {
            throw new InvalidDataException("Generated WADL XML was empty.");
        }

        return wadl;
    }

    private static bool IsEntityBindingParameter(string? bindingType, string tableLogicalName)
    {
        if (string.IsNullOrWhiteSpace(bindingType) || string.IsNullOrWhiteSpace(tableLogicalName))
        {
            return false;
        }

        var normalizedType = bindingType.Trim();
        if (normalizedType.StartsWith("Collection(", StringComparison.OrdinalIgnoreCase)
            && normalizedType.EndsWith(")", StringComparison.Ordinal))
        {
            normalizedType = normalizedType.Substring("Collection(".Length, normalizedType.Length - "Collection(".Length - 1);
        }

        return normalizedType.EndsWith($".{tableLogicalName}", StringComparison.OrdinalIgnoreCase);
    }

    private static string MapODataTypeToWadlXsType(string? odataType)
    {
        if (string.IsNullOrWhiteSpace(odataType))
        {
            return "xs:string";
        }

        var normalized = odataType.Trim();
        if (normalized.StartsWith("Collection(", StringComparison.OrdinalIgnoreCase))
        {
            return "xs:string";
        }

        return normalized switch
        {
            "Edm.Boolean" => "xs:boolean",
            "Edm.Int16" => "xs:short",
            "Edm.Int32" => "xs:int",
            "Edm.Int64" => "xs:long",
            "Edm.Decimal" => "xs:decimal",
            "Edm.Double" => "xs:double",
            "Edm.Single" => "xs:float",
            "Edm.DateTimeOffset" => "xs:dateTime",
            _ => "xs:string",
        };
    }

    /// <summary>
    /// Builds WADL jsonType elements from OData $metadata entity type definitions.
    /// For each entity type, generates standalone type definitions for scalar properties
    /// and an <c>&lt;object&gt;</c> element with property children (matching Studio format).
    /// </summary>
    private static IEnumerable<XElement> BuildWadlJsonTypeElements(
        XDocument metadataDoc,
        XNamespace edmNs,
        XNamespace sienaNs)
    {
        var entityTypes = metadataDoc
            .Descendants(edmNs + "EntityType")
            .Select(et => new
            {
                Name = et.Attribute("Name")?.Value ?? string.Empty,
                Properties = et.Elements(edmNs + "Property").ToList(),
                NavigationProperties = et.Elements(edmNs + "NavigationProperty").ToList(),
            })
            .Where(et => !string.IsNullOrWhiteSpace(et.Name))
            .ToList();

        foreach (var entityType in entityTypes)
        {
            var entityWadlName = $"Microsoft_Dynamics_CRM_{entityType.Name}";

            // Phase 1: Standalone type definitions for each scalar property.
            // Studio emits one standalone def per scalar property (string/integer/boolean/number/dateTimeString/untypedObject).
            foreach (var prop in entityType.Properties)
            {
                var propName = prop.Attribute("Name")?.Value;
                if (string.IsNullOrWhiteSpace(propName)) continue;

                var edmType = prop.Attribute("Type")?.Value;
                var (_, wadlElementName) = MapEdmTypeToWadlJsonType(edmType);
                var defName = $"{entityWadlName}_{propName}_def";
                yield return new XElement(sienaNs + wadlElementName, new XAttribute("name", defName));
            }

            // Phase 2: Object element with all properties.
            var objectElement = new XElement(sienaNs + "object", new XAttribute("name", entityWadlName));

            foreach (var prop in entityType.Properties)
            {
                var propName = prop.Attribute("Name")?.Value;
                if (string.IsNullOrWhiteSpace(propName)) continue;

                var edmType = prop.Attribute("Type")?.Value;
                var (wadlInlineType, wadlElementName) = MapEdmTypeToWadlJsonType(edmType);

                if (wadlInlineType is null)
                {
                    // Types without an inline equivalent (dateTimeString, untypedObject) must use typeRef.
                    objectElement.Add(new XElement(sienaNs + "property",
                        new XAttribute("name", propName),
                        new XAttribute("typeRef", $"{entityWadlName}_{propName}_def")));
                }
                else
                {
                    objectElement.Add(new XElement(sienaNs + "property",
                        new XAttribute("name", propName),
                        new XAttribute("type", wadlInlineType)));
                }
            }

            // Phase 3: Navigation properties reference entity object types.
            foreach (var navProp in entityType.NavigationProperties)
            {
                var propName = navProp.Attribute("Name")?.Value;
                var typeName = navProp.Attribute("Type")?.Value;
                if (string.IsNullOrWhiteSpace(propName) || string.IsNullOrWhiteSpace(typeName)) continue;

                var targetEntityName = ExtractEntityNameFromODataType(typeName);
                if (!string.IsNullOrWhiteSpace(targetEntityName))
                {
                    objectElement.Add(new XElement(sienaNs + "property",
                        new XAttribute("name", propName),
                        new XAttribute("typeRef", $"Microsoft_Dynamics_CRM_{targetEntityName}")));
                }
            }

            yield return objectElement;
        }
    }

    /// <summary>
    /// Maps an OData EDM type to a WADL json type element name and optional inline type.
    /// Returns (inlineType, elementName) where inlineType is null when the type must use typeRef.
    /// </summary>
    private static (string? inlineType, string elementName) MapEdmTypeToWadlJsonType(string? edmType)
    {
        if (string.IsNullOrWhiteSpace(edmType))
        {
            return ("string", "string");
        }

        return edmType switch
        {
            "Edm.String" => ("string", "string"),
            "Edm.Guid" => ("string", "string"),
            "Edm.Binary" => ("string", "string"),
            "Edm.Int32" => ("integer", "integer"),
            "Edm.Int64" => ("integer", "integer"),
            "Edm.Int16" => ("integer", "integer"),
            "Edm.Byte" => ("integer", "integer"),
            "Edm.Boolean" => ("boolean", "boolean"),
            "Edm.Decimal" => ("number", "number"),
            "Edm.Double" => (null, "untypedObject"),             // Studio maps Double to untypedObject with typeRef
            "Edm.Single" => ("number", "number"),
            "Edm.DateTimeOffset" => (null, "dateTimeString"),    // No inline equivalent; must use typeRef
            "Edm.Duration" => ("string", "string"),
            "Edm.Stream" => ("string", "string"),
            _ => ("string", "string"),
        };
    }

    /// <summary>
    /// Extracts the entity logical name from an OData type string.
    /// Handles both <c>Microsoft.Dynamics.CRM.contact</c> and <c>Collection(Microsoft.Dynamics.CRM.contact)</c>.
    /// </summary>
    private static string? ExtractEntityNameFromODataType(string typeName)
    {
        var normalized = typeName.Trim();
        if (normalized.StartsWith("Collection(", StringComparison.OrdinalIgnoreCase)
            && normalized.EndsWith(")", StringComparison.Ordinal))
        {
            normalized = normalized.Substring("Collection(".Length, normalized.Length - "Collection(".Length - 1);
        }

        var lastDot = normalized.LastIndexOf('.');
        return lastDot >= 0 ? normalized.Substring(lastDot + 1) : normalized;
    }

    private static async Task<JsonObject> GetPowerAppsEntityMetadataAsync(
        string environmentUrl,
        string accessToken,
        string escapedTable)
    {
        try
        {
            var expanded = await DataverseGetJsonAsync(
                environmentUrl,
                accessToken,
                BuildExpandedEntityMetadataRelativePath(escapedTable));

            if (HasExpandedEntityMetadata(expanded))
            {
                return expanded;
            }
        }
        catch
        {
            // Ignore and continue to fragment-based assembly below.
        }

        try
        {
            var assembled = await BuildPowerAppsEntityMetadataFromFragmentsAsync(
                environmentUrl,
                accessToken,
                escapedTable);

            if (HasExpandedEntityMetadata(assembled))
            {
                return assembled;
            }
        }
        catch
        {
            // Fall through to minimal metadata as final safe fallback.
        }

        return await DataverseGetJsonAsync(
            environmentUrl,
            accessToken,
            BuildMinimalEntityMetadataRelativePath(escapedTable));
    }

    private static async Task<JsonObject> BuildPowerAppsEntityMetadataFromFragmentsAsync(
        string environmentUrl,
        string accessToken,
        string escapedTable)
    {
        var baseMetadata = await DataverseGetJsonAsync(
            environmentUrl,
            accessToken,
            BuildEntityMetadataBaseRelativePath(escapedTable));

        var attributesPayload = await DataverseGetJsonAsync(
            environmentUrl,
            accessToken,
            $"EntityDefinitions(LogicalName='{escapedTable}')/Attributes");

        var manyToOnePayload = await DataverseGetJsonAsync(
            environmentUrl,
            accessToken,
            $"EntityDefinitions(LogicalName='{escapedTable}')/ManyToOneRelationships");

        var oneToManyPayload = await DataverseGetJsonAsync(
            environmentUrl,
            accessToken,
            $"EntityDefinitions(LogicalName='{escapedTable}')/OneToManyRelationships");

        var manyToManyPayload = await DataverseGetJsonAsync(
            environmentUrl,
            accessToken,
            $"EntityDefinitions(LogicalName='{escapedTable}')/ManyToManyRelationships");

        var privilegesPayload = await DataverseGetJsonAsync(
            environmentUrl,
            accessToken,
            $"EntityDefinitions(LogicalName='{escapedTable}')/Privileges");

        baseMetadata["Attributes"] = GetValueArrayOrEmpty(attributesPayload);
        baseMetadata["ManyToOneRelationships"] = GetValueArrayOrEmpty(manyToOnePayload);
        baseMetadata["OneToManyRelationships"] = GetValueArrayOrEmpty(oneToManyPayload);
        baseMetadata["ManyToManyRelationships"] = GetValueArrayOrEmpty(manyToManyPayload);
        baseMetadata["Privileges"] = GetValueArrayOrEmpty(privilegesPayload);

        return baseMetadata;
    }

    private static JsonArray GetValueArrayOrEmpty(JsonObject payload)
    {
        return payload["value"] is JsonArray value
            ? (JsonArray)value.DeepClone()
            : new JsonArray();
    }

    private static bool HasExpandedEntityMetadata(JsonObject metadata)
    {
        return metadata["Attributes"] is JsonArray attributes
               && attributes.Count > 0;
    }

    private static string BuildExpandedEntityMetadataRelativePath(string escapedTable)
    {
        return $"EntityDefinitions(LogicalName='{escapedTable}')"
               + "?$select=LogicalName,LogicalCollectionName,EntitySetName,PrimaryIdAttribute,PrimaryNameAttribute,ObjectTypeCode,DisplayName,DisplayCollectionName,Description,HasNotes,IsActivity,IsAvailableOffline,IsIntersect,IsLogicalEntity,IsManaged,IsOfflineInMobileClient,IsPrivate,OwnershipType,TableType,MetadataId"
               + "&$expand=Attributes,ManyToOneRelationships,OneToManyRelationships,ManyToManyRelationships,Privileges";
    }

    private static string BuildEntityMetadataBaseRelativePath(string escapedTable)
    {
        return $"EntityDefinitions(LogicalName='{escapedTable}')"
               + "?$select=LogicalName,LogicalCollectionName,EntitySetName,PrimaryIdAttribute,PrimaryNameAttribute,ObjectTypeCode,DisplayName,DisplayCollectionName,Description,HasNotes,IsActivity,IsAvailableOffline,IsIntersect,IsLogicalEntity,IsManaged,IsOfflineInMobileClient,IsPrivate,OwnershipType,TableType,MetadataId";
    }

    private static string BuildMinimalEntityMetadataRelativePath(string escapedTable)
    {
        return $"EntityDefinitions(LogicalName='{escapedTable}')?$select=LogicalName,LogicalCollectionName,EntitySetName,PrimaryIdAttribute,PrimaryNameAttribute,ObjectTypeCode,DisplayName,DisplayCollectionName";
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

    private static JsonObject FilterSavedQueryPayload(JsonObject viewsPayload)
    {
        var filtered = viewsPayload.DeepClone() as JsonObject ?? new JsonObject();
        var values = filtered["value"] as JsonArray;
        if (values is null)
        {
            return filtered;
        }

        var resultValues = new JsonArray();
        foreach (var view in values.OfType<JsonObject>())
        {
            var queryApi = view["queryapi"]?.GetValue<string>();
            if (!string.IsNullOrWhiteSpace(queryApi))
            {
                continue;
            }

            var cleaned = view.DeepClone() as JsonObject ?? new JsonObject();
            cleaned.Remove("queryapi");
            resultValues.Add(cleaned);
        }

        filtered["value"] = resultValues;
        return filtered;
    }

    private static JsonObject BuildAttributeDisplayNameMap(
        JsonObject allAttributesPayload,
        JsonObject entityMetadata,
        string tableLogicalName,
        IReadOnlyDictionary<string, string> relatedEntityDisplayCollections)
    {
        var map = new JsonObject();
        var values = allAttributesPayload["value"] as JsonArray;
        if (values is null)
        {
            return map;
        }

        JsonObject? ownerAttribute = null;
        JsonObject? parentCustomerAttribute = null;

        foreach (var attr in values.OfType<JsonObject>())
        {
            var logical = attr["LogicalName"]?.GetValue<string>() ?? string.Empty;
            if (string.IsNullOrWhiteSpace(logical))
            {
                continue;
            }

            if (string.Equals(logical, "ownerid", StringComparison.OrdinalIgnoreCase))
            {
                ownerAttribute = attr;
            }

            if (string.Equals(logical, "parentcustomerid", StringComparison.OrdinalIgnoreCase))
            {
                parentCustomerAttribute = attr;
            }

            if (string.Equals(logical, "entityimage", StringComparison.OrdinalIgnoreCase))
            {
                map[logical] = GetLocalizedLabel(attr["DisplayName"], logical);
                continue;
            }

            if (string.Equals(logical, "accountid", StringComparison.OrdinalIgnoreCase)
                && !string.Equals(tableLogicalName, "account", StringComparison.OrdinalIgnoreCase))
            {
                continue;
            }

            if (logical.EndsWith("_addressid", StringComparison.OrdinalIgnoreCase))
            {
                continue;
            }

            if (attr["AttributeOf"]?.GetValue<string>() is { Length: > 0 })
            {
                continue;
            }

            if (ExcludedAttributeNameMappingKeys.Contains(logical))
            {
                continue;
            }

            var display = GetLocalizedLabel(attr["DisplayName"], logical);
            map[logical] = display;
        }

        var relationshipMappingCandidates = new List<(string SchemaName, string Label)>();
        foreach (var relationshipCollectionName in new[] { "ManyToOneRelationships", "OneToManyRelationships", "ManyToManyRelationships" })
        {
            if (entityMetadata[relationshipCollectionName] is not JsonArray relationships)
            {
                continue;
            }

            foreach (var relationship in relationships.OfType<JsonObject>())
            {
                var schemaName = relationship["SchemaName"]?.GetValue<string>() ?? string.Empty;
                if (string.IsNullOrWhiteSpace(schemaName)
                    || !ShouldIncludeRelationshipInNameMapping(relationshipCollectionName, relationship, tableLogicalName))
                {
                    continue;
                }

                var relatedEntityLogicalName = ResolveRelatedEntityLogicalName(relationshipCollectionName, relationship, tableLogicalName);
                var relationshipLabel = ResolveRelationshipDisplayLabel(
                    relatedEntityDisplayCollections,
                    relatedEntityLogicalName,
                    schemaName);
                relationshipMappingCandidates.Add((schemaName, relationshipLabel));
            }
        }

        var duplicateLabels = relationshipMappingCandidates
            .GroupBy(c => c.Label, StringComparer.OrdinalIgnoreCase)
            .Where(g => g.Count() > 1)
            .Select(g => g.Key)
            .ToHashSet(StringComparer.OrdinalIgnoreCase);

        foreach (var candidate in relationshipMappingCandidates)
        {
            var value = duplicateLabels.Contains(candidate.Label)
                ? $"{candidate.Label} ({candidate.SchemaName})"
                : candidate.Label;
            map[candidate.SchemaName] = value;
        }

        map["{Attachments}"] = "Attachments";

        if (ownerAttribute is not null)
        {
            map["_ownerid_value"] = GetLocalizedLabel(ownerAttribute["DisplayName"], "Owner");
        }

        if (parentCustomerAttribute is not null)
        {
            map["_parentcustomerid_value"] = GetLocalizedLabel(parentCustomerAttribute["DisplayName"], "Company Name");
        }

        if (string.Equals(tableLogicalName, "account", StringComparison.OrdinalIgnoreCase))
        {
            map["accountid"] = "Account";
        }

        return map;
    }

    private static bool ShouldIncludeRelationshipInNameMapping(string relationshipCollectionName, JsonObject relationship, string tableLogicalName)
    {
        var schemaName = relationship["SchemaName"]?.GetValue<string>() ?? string.Empty;
        if (string.IsNullOrWhiteSpace(schemaName))
        {
            return false;
        }

        var normalizedSchemaName = schemaName.Trim();
        var referencedEntity = relationship["ReferencedEntity"]?.GetValue<string>() ?? string.Empty;
        var referencingEntity = relationship["ReferencingEntity"]?.GetValue<string>() ?? string.Empty;
        var isValidForAdvancedFind = relationship["IsValidForAdvancedFind"]?.GetValue<bool?>() ?? true;

        if (RelationshipNameMappingAllowList.Contains(normalizedSchemaName))
        {
            return true;
        }

        if (string.Equals(relationshipCollectionName, "ManyToManyRelationships", StringComparison.Ordinal)
            && !isValidForAdvancedFind)
        {
            return false;
        }

        if (string.Equals(relationshipCollectionName, "ManyToOneRelationships", StringComparison.Ordinal)
            && !string.Equals(referencedEntity, tableLogicalName, StringComparison.OrdinalIgnoreCase))
        {
            return false;
        }

        if (normalizedSchemaName.StartsWith("lk_", StringComparison.OrdinalIgnoreCase)
            || normalizedSchemaName.StartsWith("business_unit_", StringComparison.OrdinalIgnoreCase)
            || normalizedSchemaName.StartsWith("system_user_", StringComparison.OrdinalIgnoreCase)
            || normalizedSchemaName.StartsWith("team_", StringComparison.OrdinalIgnoreCase)
            || normalizedSchemaName.StartsWith("owner_", StringComparison.OrdinalIgnoreCase)
            || normalizedSchemaName.StartsWith("transactioncurrency_", StringComparison.OrdinalIgnoreCase)
            || normalizedSchemaName.StartsWith("manualsla_", StringComparison.OrdinalIgnoreCase)
            || normalizedSchemaName.StartsWith("sla_", StringComparison.OrdinalIgnoreCase)
            || normalizedSchemaName.StartsWith("slakpiinstance_", StringComparison.OrdinalIgnoreCase)
            || normalizedSchemaName.StartsWith("processstage_", StringComparison.OrdinalIgnoreCase)
            || normalizedSchemaName.StartsWith("userentityinstancedata_", StringComparison.OrdinalIgnoreCase))
        {
            return false;
        }

        if (normalizedSchemaName.Contains("_customer_relationship_", StringComparison.OrdinalIgnoreCase)
            || normalizedSchemaName.EndsWith("_activity_parties", StringComparison.OrdinalIgnoreCase)
            || normalizedSchemaName.EndsWith("_postroles", StringComparison.OrdinalIgnoreCase)
            || normalizedSchemaName.Contains("socialactivity_postauthor", StringComparison.OrdinalIgnoreCase)
            || normalizedSchemaName.Contains("socialprofile_customer", StringComparison.OrdinalIgnoreCase)
            || normalizedSchemaName.Contains("_socialactivities", StringComparison.OrdinalIgnoreCase)
            || string.Equals(normalizedSchemaName, "Account_SharepointDocument", StringComparison.OrdinalIgnoreCase)
            || normalizedSchemaName.Contains("_externalpartyitems", StringComparison.OrdinalIgnoreCase))
        {
            return false;
        }

        if (string.Equals(referencingEntity, "activityparty", StringComparison.OrdinalIgnoreCase)
            || string.Equals(referencingEntity, "customerrelationship", StringComparison.OrdinalIgnoreCase)
            || string.Equals(referencingEntity, "postrole", StringComparison.OrdinalIgnoreCase)
            || string.Equals(referencingEntity, "socialactivity", StringComparison.OrdinalIgnoreCase)
            || string.Equals(referencingEntity, "socialprofile", StringComparison.OrdinalIgnoreCase)
            || string.Equals(referencingEntity, "sharepointdocument", StringComparison.OrdinalIgnoreCase)
            || string.Equals(referencingEntity, "externalpartyitem", StringComparison.OrdinalIgnoreCase)
            || string.Equals(referencingEntity, "slakpiinstance", StringComparison.OrdinalIgnoreCase)
            || string.Equals(referencingEntity, "userentityinstancedata", StringComparison.OrdinalIgnoreCase))
        {
            return false;
        }

        return true;
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

    private static string ResolveRelatedEntityLogicalName(string relationshipCollectionName, JsonObject relationship, string tableLogicalName)
    {
        if (string.Equals(relationshipCollectionName, "ManyToManyRelationships", StringComparison.OrdinalIgnoreCase))
        {
            var entity1LogicalName = relationship["Entity1LogicalName"]?.GetValue<string>() ?? string.Empty;
            var entity2LogicalName = relationship["Entity2LogicalName"]?.GetValue<string>() ?? string.Empty;

            if (string.Equals(entity1LogicalName, tableLogicalName, StringComparison.OrdinalIgnoreCase)
                && !string.IsNullOrWhiteSpace(entity2LogicalName))
            {
                return entity2LogicalName;
            }

            if (string.Equals(entity2LogicalName, tableLogicalName, StringComparison.OrdinalIgnoreCase)
                && !string.IsNullOrWhiteSpace(entity1LogicalName))
            {
                return entity1LogicalName;
            }

            if (!string.IsNullOrWhiteSpace(entity1LogicalName))
            {
                return entity1LogicalName;
            }

            if (!string.IsNullOrWhiteSpace(entity2LogicalName))
            {
                return entity2LogicalName;
            }
        }

        var referencedEntity = relationship["ReferencedEntity"]?.GetValue<string>() ?? string.Empty;
        var referencingEntity = relationship["ReferencingEntity"]?.GetValue<string>() ?? string.Empty;

        if (string.Equals(relationshipCollectionName, "OneToManyRelationships", StringComparison.OrdinalIgnoreCase)
            && string.Equals(referencedEntity, tableLogicalName, StringComparison.OrdinalIgnoreCase))
        {
            return referencingEntity;
        }

        if (string.Equals(relationshipCollectionName, "ManyToOneRelationships", StringComparison.OrdinalIgnoreCase)
            && string.Equals(referencingEntity, tableLogicalName, StringComparison.OrdinalIgnoreCase))
        {
            return referencedEntity;
        }

        if (!string.IsNullOrWhiteSpace(referencingEntity)
            && !string.Equals(referencingEntity, tableLogicalName, StringComparison.OrdinalIgnoreCase))
        {
            return referencingEntity;
        }

        if (!string.IsNullOrWhiteSpace(referencedEntity))
        {
            return referencedEntity;
        }

        return tableLogicalName;
    }

    private static string ResolveRelationshipDisplayLabel(
        IReadOnlyDictionary<string, string> relatedEntityDisplayCollections,
        string relatedEntityLogicalName,
        string fallbackSchemaName)
    {
        if (!string.IsNullOrWhiteSpace(relatedEntityLogicalName)
            && relatedEntityDisplayCollections.TryGetValue(relatedEntityLogicalName, out var label)
            && !string.IsNullOrWhiteSpace(label))
        {
            return label;
        }

        return fallbackSchemaName;
    }

    private static async Task<Dictionary<string, string>> BuildRelatedEntityDisplayCollectionNameMapAsync(
        string environmentUrl,
        string accessToken,
        JsonObject entityMetadata,
        string tableLogicalName)
    {
        var entityNames = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
        {
            tableLogicalName,
        };

        foreach (var relationshipCollectionName in new[] { "ManyToOneRelationships", "OneToManyRelationships", "ManyToManyRelationships" })
        {
            if (entityMetadata[relationshipCollectionName] is not JsonArray relationships)
            {
                continue;
            }

            foreach (var relationship in relationships.OfType<JsonObject>())
            {
                var referencedEntity = relationship["ReferencedEntity"]?.GetValue<string>() ?? string.Empty;
                var referencingEntity = relationship["ReferencingEntity"]?.GetValue<string>() ?? string.Empty;
                var entity1LogicalName = relationship["Entity1LogicalName"]?.GetValue<string>() ?? string.Empty;
                var entity2LogicalName = relationship["Entity2LogicalName"]?.GetValue<string>() ?? string.Empty;

                if (!string.IsNullOrWhiteSpace(referencedEntity))
                {
                    entityNames.Add(referencedEntity);
                }

                if (!string.IsNullOrWhiteSpace(referencingEntity))
                {
                    entityNames.Add(referencingEntity);
                }

                if (!string.IsNullOrWhiteSpace(entity1LogicalName))
                {
                    entityNames.Add(entity1LogicalName);
                }

                if (!string.IsNullOrWhiteSpace(entity2LogicalName))
                {
                    entityNames.Add(entity2LogicalName);
                }
            }
        }

        var result = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
        foreach (var entityName in entityNames)
        {
            var escapedEntity = EscapeODataLiteral(entityName);
            var metadata = await DataverseGetJsonAsync(
                environmentUrl,
                accessToken,
                $"EntityDefinitions(LogicalName='{escapedEntity}')?$select=LogicalName,LogicalCollectionName,DisplayCollectionName");

            var fallback = metadata["LogicalCollectionName"]?.GetValue<string>() ?? entityName;
            var label = GetLocalizedLabel(metadata["DisplayCollectionName"], fallback);
            result[entityName] = label;
        }

        return result;
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