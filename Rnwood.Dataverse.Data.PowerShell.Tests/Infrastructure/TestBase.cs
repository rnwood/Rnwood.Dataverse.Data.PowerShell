using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Reflection;
using System.Runtime.Serialization;
using System.ServiceModel;
using FakeItEasy;
using Fake4Dataverse;
using Microsoft.Crm.Sdk.Messages;
using Microsoft.Extensions.Logging;
using Microsoft.PowerPlatform.Dataverse.Client;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Messages;
using Microsoft.Xrm.Sdk.Metadata;
using Microsoft.Xrm.Sdk.Query;

namespace Rnwood.Dataverse.Data.PowerShell.Tests.Infrastructure
{
    /// <summary>
    /// Base class for all tests providing Fake4Dataverse mock connection infrastructure.
    /// </summary>
    public abstract class TestBase : IDisposable
    {
        private static readonly Dictionary<string, EntityMetadata> MetadataCache = new();
        private static readonly Dictionary<string, byte[]> MetadataSerializedCache = new();
        private static readonly object MetadataCacheLock = new();
        private List<EntityMetadata>? _loadedMetadata;
        
        static TestBase()
        {
            Rnwood.Dataverse.Data.PowerShell.Commands.DefaultConnectionManager.UseThreadLocalConnection = true;
            
            // Configure the clone extension to use our wrapper registry for mock connections
            Rnwood.Dataverse.Data.PowerShell.Commands.DataverseConnectionExtensions.GetCloneableWrapper = 
                (serviceClient) => {
                    if (WrapperRegistry.TryGetWrapper(serviceClient, out var wrapper))
                    {
                        return wrapper;
                    }
                    return null;
                };
        }


        // Fixed identity values for WhoAmIRequest (consistent per test)
        private readonly Guid _mockUserId = Guid.NewGuid();
        private readonly Guid _mockBusinessUnitId = Guid.NewGuid();
        private readonly Guid _mockOrganizationId = Guid.NewGuid();
        
        /// <summary>
        /// Gets the current test's organization service.
        /// </summary>
        protected IOrganizationService? Service { get; private set; }

        /// <summary>
        /// Gets the current test's FakeDataverseEnvironment.
        /// </summary>
        protected FakeDataverseEnvironment? Environment { get; private set; }

        /// <summary>
        /// Gets the current test's ServiceClient (wrapper around mock service).
        /// </summary>
        protected ServiceClient? Connection { get; private set; }
        
        /// <summary>
        /// Gets the mock user ID returned by WhoAmIRequest.
        /// </summary>
        protected Guid MockUserId => _mockUserId;

        /// <summary>
        /// Gets the loaded entity metadata for the current mock connection.
        /// Useful for request interceptors that need to return metadata.
        /// </summary>
        protected IReadOnlyList<EntityMetadata> LoadedMetadata => 
            _loadedMetadata ?? new List<EntityMetadata>();

        /// <summary>
        /// Creates a mock connection with the specified entities' metadata loaded.
        /// Returns a ServiceClient that wraps the mock service for use with cmdlets.
        /// </summary>
        /// <param name="entities">Entity names to load metadata for (default: "contact")</param>
        /// <returns>A mock ServiceClient</returns>
        protected ServiceClient CreateMockConnection(params string[] entities)
        {
            return CreateMockConnection(null, entities);
        }

        /// <summary>
        /// Creates a mock connection with a request interceptor and specified entities' metadata loaded.
        /// </summary>
        /// <param name="requestInterceptor">
        /// Optional delegate to intercept requests before Fake4Dataverse handles them.
        /// Return an OrganizationResponse to short-circuit, or null to let Fake4Dataverse handle it.
        /// </param>
        /// <param name="entities">Entity names to load metadata for (default: "contact")</param>
        /// <returns>A mock ServiceClient</returns>
        protected ServiceClient CreateMockConnection(
            Func<OrganizationRequest, OrganizationResponse?>? requestInterceptor,
            params string[] entities)
        {
            if (entities.Length == 0)
            {
                entities = new[] { "contact" };
            }

            // Load metadata for requested entities - CLONE to avoid modifying cached metadata
            var metadata = new List<EntityMetadata>();
            foreach (var entityName in entities)
            {
                var entityMetadata = LoadEntityMetadata(entityName);
                if (entityMetadata != null)
                {
                    // Clone the metadata to prevent tests from modifying the shared cache
                    metadata.Add(CloneEntityMetadata(entityMetadata));
                }
            }
            return CreateMockConnectionWithCustomMetadata(requestInterceptor, metadata);
        }

        /// <summary>
        /// Creates a mock connection with custom metadata.
        /// </summary>
        protected ServiceClient CreateMockConnectionWithCustomMetadata(
            Func<OrganizationRequest, OrganizationResponse?>? requestInterceptor,
            List<EntityMetadata> customMetadata)
        {
            _loadedMetadata = customMetadata;

            // Create Fake4Dataverse environment
            var env = new FakeDataverseEnvironment();
            env.OrganizationId = _mockOrganizationId;
            env.OrganizationName = "MockOrganization";
            Environment = env;

            // Initialize metadata into Fake4Dataverse's MetadataStore
            InitializeMetadataInStore(env, customMetadata);

            // Register custom handlers for requests not natively supported by Fake4Dataverse
            RegisterDefaultHandlers(env, customMetadata);

            // Seed organization record for SQL4Cds and other queries
            env.Seed(new Entity("organization", _mockOrganizationId)
            {
                ["name"] = "MockOrganization",
                ["localeid"] = 1033,
                ["languagecode"] = 1033
            });

            // Seed a systemuser for the mock user
            env.Seed(new Entity("systemuser", _mockUserId)
            {
                ["fullname"] = "Mock User",
                ["domainname"] = "mockuser@fakeorg.onmicrosoft.com"
            });

            // Create the organization service with our mock user ID
            var fakeService = env.CreateOrganizationService(_mockUserId);
            fakeService.BusinessUnitId = _mockBusinessUnitId;

            // If there's a custom interceptor, wrap the service
            if (requestInterceptor != null)
            {
                Service = new MockOrganizationServiceWithInterceptor(fakeService, requestInterceptor);
            }
            else
            {
                Service = fakeService;
            }

            // Create CloneableMockServiceClient that supports cloning for parallel operations
            var httpClient = new HttpClient(new FakeHttpMessageHandler());
            Connection = MockServiceClientFactory.Create(
                Service,
                httpClient,
                "https://fakeorg.crm.dynamics.com",
                new Version(9, 2),
                A.Fake<ILogger>());

            return Connection;
        }

        /// <summary>
        /// Initializes entity metadata from SDK EntityMetadata objects into the Fake4Dataverse MetadataStore.
        /// </summary>
        private static void InitializeMetadataInStore(FakeDataverseEnvironment env, List<EntityMetadata> metadataList)
        {
            foreach (var entityMeta in metadataList)
            {
                if (string.IsNullOrEmpty(entityMeta.LogicalName))
                    continue;

                var builder = env.MetadataStore.AddEntity(entityMeta.LogicalName);

                if (!string.IsNullOrEmpty(entityMeta.SchemaName))
                    builder.WithSchemaName(entityMeta.SchemaName);

                if (!string.IsNullOrEmpty(entityMeta.PrimaryIdAttribute))
                    builder.WithPrimaryIdAttribute(entityMeta.PrimaryIdAttribute);

                if (!string.IsNullOrEmpty(entityMeta.PrimaryNameAttribute))
                    builder.WithPrimaryNameAttribute(entityMeta.PrimaryNameAttribute);

                if (entityMeta.ObjectTypeCode.HasValue)
                    builder.WithObjectTypeCode(entityMeta.ObjectTypeCode.Value);

                // Add attributes - use typed builder methods for attributes that need more detail
                if (entityMeta.Attributes != null)
                {
                    foreach (var attr in entityMeta.Attributes)
                    {
                        if (string.IsNullOrEmpty(attr.LogicalName))
                            continue;

                        var attrType = attr.AttributeType ?? AttributeTypeCode.String;

                        if (attr is LookupAttributeMetadata lookupAttr && lookupAttr.Targets?.Length > 0)
                        {
                            builder.WithLookupAttribute(attr.LogicalName, lookupAttr.Targets);
                        }
                        else if (attr is PicklistAttributeMetadata picklistAttr && picklistAttr.OptionSet?.Options?.Count > 0)
                        {
                            var values = picklistAttr.OptionSet.Options
                                .Where(o => o.Value.HasValue)
                                .Select(o => o.Value!.Value)
                                .ToArray();
                            builder.WithOptionSetAttribute(attr.LogicalName, values);
                        }
                        else if (attrType == AttributeTypeCode.Boolean)
                        {
                            builder.WithBooleanAttribute(attr.LogicalName);
                        }
                        else
                        {
                            builder.WithAttribute(attr.LogicalName, attrType);
                        }
                    }
                }

                // Add alternate keys
                if (entityMeta.Keys != null)
                {
                    foreach (var key in entityMeta.Keys)
                    {
                        if (!string.IsNullOrEmpty(key.LogicalName) && key.KeyAttributes?.Length > 0)
                        {
                            builder.WithAlternateKey(key.LogicalName, key.KeyAttributes);
                        }
                    }
                }
            }

            // Add well-known system entities if not already present
            EnsureSystemEntity(env, "systemuser", "SystemUser", "systemuserid", "fullname", 8);
            EnsureSystemEntity(env, "team", "Team", "teamid", "name", 9);
            EnsureSystemEntity(env, "organization", "Organization", "organizationid", "name", 1);
        }

        private static void EnsureSystemEntity(FakeDataverseEnvironment env, string logicalName, string schemaName, string primaryIdAttr, string primaryNameAttr, int objectTypeCode)
        {
            // AddEntity returns existing builder if already added, so this is safe to call multiple times
            env.MetadataStore.AddEntity(logicalName)
                .WithSchemaName(schemaName)
                .WithPrimaryIdAttribute(primaryIdAttr)
                .WithPrimaryNameAttribute(primaryNameAttr)
                .WithObjectTypeCode(objectTypeCode);
        }

        /// <summary>
        /// Loads entity metadata from embedded XML resource or file.
        /// </summary>
        protected static EntityMetadata? LoadEntityMetadata(string entityName)
        {
            lock (MetadataCacheLock)
            {
                if (MetadataCache.TryGetValue(entityName, out var cachedMetadata))
                {
                    return cachedMetadata;
                }

                // Try to load from embedded resource
                var assembly = Assembly.GetExecutingAssembly();
                var resourceName = $"Rnwood.Dataverse.Data.PowerShell.Tests.Metadata.{entityName}.xml";

                using var stream = assembly.GetManifestResourceStream(resourceName);
                if (stream == null)
                {
                    // Try alternate path - files might be in output directory
                    var metadataPath = Path.Combine(
                        Path.GetDirectoryName(assembly.Location) ?? "",
                        "Metadata",
                        $"{entityName}.xml");

                    if (File.Exists(metadataPath))
                    {
                        using var fileStream = File.OpenRead(metadataPath);
                        var metadata = DeserializeMetadata(fileStream);
                        if (metadata != null)
                        {
                            MetadataCache[entityName] = metadata;
                        }
                        return metadata;
                    }

                    // Provide minimal built-in metadata for system entities when no XML is present
                    if (string.Equals(entityName, "systemuser", StringComparison.OrdinalIgnoreCase))
                    {
                        var metadata = BuildSystemUserMetadata();
                        MetadataCache[entityName] = metadata;
                        return metadata;
                    }

                    if (string.Equals(entityName, "team", StringComparison.OrdinalIgnoreCase))
                    {
                        var metadata = BuildTeamMetadata();
                        MetadataCache[entityName] = metadata;
                        return metadata;
                    }

                    throw new FileNotFoundException(
                        $"Metadata file not found for entity '{entityName}'. " +
                        $"Ensure {entityName}.xml exists in the tests folder.");
                }

                var deserializedMetadata = DeserializeMetadata(stream);
                if (deserializedMetadata != null)
                {
                    MetadataCache[entityName] = deserializedMetadata;
                }
                return deserializedMetadata;
            }
        }

        private static EntityMetadata? DeserializeMetadata(Stream stream)
        {
            var serializer = new DataContractSerializer(typeof(EntityMetadata));
            return serializer.ReadObject(stream) as EntityMetadata;
        }

        /// <summary>
        /// Creates a deep copy of entity metadata to prevent modifying the shared cache.
        /// This uses cached serialization/deserialization for better performance.
        /// </summary>
        protected static EntityMetadata CloneEntityMetadata(EntityMetadata source)
        {
            var logicalName = source.LogicalName;
            
            lock (MetadataCacheLock)
            {
                // Check if we have a cached serialized version
                if (!MetadataSerializedCache.TryGetValue(logicalName, out var serializedData))
                {
                    // Serialize once and cache the byte array
                    var serializer = new DataContractSerializer(typeof(EntityMetadata));
                    using var ms = new MemoryStream();
                    serializer.WriteObject(ms, source);
                    serializedData = ms.ToArray();
                    MetadataSerializedCache[logicalName] = serializedData;
                }
                
                // Deserialize from cached byte array
                var deserializer = new DataContractSerializer(typeof(EntityMetadata));
                using var readStream = new MemoryStream(serializedData);
                return (EntityMetadata)deserializer.ReadObject(readStream)!;
            }
        }

        private static EntityMetadata BuildSystemUserMetadata()
        {
            var metadata = new EntityMetadata();

            SetMetadataProperty(metadata, nameof(EntityMetadata.LogicalName), "systemuser");
            SetMetadataProperty(metadata, nameof(EntityMetadata.SchemaName), "SystemUser");
            SetMetadataProperty(metadata, nameof(EntityMetadata.PrimaryIdAttribute), "systemuserid");
            SetMetadataProperty(metadata, nameof(EntityMetadata.PrimaryNameAttribute), "fullname");
            SetMetadataProperty(metadata, nameof(EntityMetadata.ObjectTypeCode), 8);

            AttributeMetadata BuildAttribute(AttributeMetadata attr, string logicalName, string schemaName, AttributeTypeCode type)
            {
                SetMetadataProperty(attr, nameof(AttributeMetadata.LogicalName), logicalName);
                SetMetadataProperty(attr, nameof(AttributeMetadata.SchemaName), schemaName);
                SetMetadataProperty(attr, nameof(AttributeMetadata.AttributeType), type);
                return attr;
            }

            var fullname = BuildAttribute(new StringAttributeMetadata { MaxLength = 200 }, "fullname", "FullName", AttributeTypeCode.String);
            var domain = BuildAttribute(new StringAttributeMetadata { MaxLength = 200 }, "domainname", "DomainName", AttributeTypeCode.String);
            var id = BuildAttribute(new AttributeMetadata(), "systemuserid", "SystemUserId", AttributeTypeCode.Uniqueidentifier);

            SetMetadataProperty(metadata, nameof(EntityMetadata.Attributes), new AttributeMetadata[] { fullname, domain, id });

            return metadata;
        }

        private static EntityMetadata BuildTeamMetadata()
        {
            var metadata = new EntityMetadata();

            SetMetadataProperty(metadata, nameof(EntityMetadata.LogicalName), "team");
            SetMetadataProperty(metadata, nameof(EntityMetadata.SchemaName), "Team");
            SetMetadataProperty(metadata, nameof(EntityMetadata.PrimaryIdAttribute), "teamid");
            SetMetadataProperty(metadata, nameof(EntityMetadata.PrimaryNameAttribute), "name");
            SetMetadataProperty(metadata, nameof(EntityMetadata.ObjectTypeCode), 9);

            AttributeMetadata BuildAttribute(AttributeMetadata attr, string logicalName, string schemaName, AttributeTypeCode type)
            {
                SetMetadataProperty(attr, nameof(AttributeMetadata.LogicalName), logicalName);
                SetMetadataProperty(attr, nameof(AttributeMetadata.SchemaName), schemaName);
                SetMetadataProperty(attr, nameof(AttributeMetadata.AttributeType), type);
                return attr;
            }

            var name = BuildAttribute(new StringAttributeMetadata { MaxLength = 200 }, "name", "Name", AttributeTypeCode.String);
            var id = BuildAttribute(new AttributeMetadata(), "teamid", "TeamId", AttributeTypeCode.Uniqueidentifier);

            SetMetadataProperty(metadata, nameof(EntityMetadata.Attributes), new AttributeMetadata[] { name, id });

            return metadata;
        }

        private static void SetMetadataProperty(object target, string propertyName, object value)
        {
            var prop = target.GetType().GetProperty(propertyName, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
            prop?.SetValue(target, value);
        }

        /// <summary>
        /// Registers custom API handlers for request types not natively supported by Fake4Dataverse
        /// but used by the cmdlets under test.
        /// </summary>
        private static void RegisterDefaultHandlers(
            FakeDataverseEnvironment env,
            List<EntityMetadata> customMetadata)
        {
            // RetrieveUnpublishedMultipleRequest → delegate to RetrieveMultiple
            env.RegisterCustomApi("RetrieveUnpublishedMultiple", (request, service) =>
            {
                var query = (QueryBase)request["Query"];
                var result = service.RetrieveMultiple(query);
                var response = new RetrieveUnpublishedMultipleResponse();
                response.Results["EntityCollection"] = result;
                return response;
            });

            // RetrieveUnpublishedRequest → delegate to Retrieve
            env.RegisterCustomApi("RetrieveUnpublished", (request, service) =>
            {
                var target = (EntityReference)request["Target"];
                var columnSet = (ColumnSet)request["ColumnSet"];
                var entity = service.Retrieve(target.LogicalName, target.Id, columnSet);
                var response = new RetrieveUnpublishedResponse();
                response.Results["Entity"] = entity;
                return response;
            });

            // RetrieveMissingDependenciesRequest → empty collection
            env.RegisterCustomApi("RetrieveMissingDependencies", (request, service) =>
            {
                var response = new RetrieveMissingDependenciesResponse();
                response.Results["EntityCollection"] = new EntityCollection();
                return response;
            });

            // RetrieveDependenciesForUninstallRequest → empty collection
            env.RegisterCustomApi("RetrieveDependenciesForUninstall", (request, service) =>
            {
                var response = new RetrieveDependenciesForUninstallResponse();
                response.Results["EntityCollection"] = new EntityCollection();
                return response;
            });

            // ValidateAppRequest → return success with empty validation list
            env.RegisterCustomApi("ValidateApp", (request, service) =>
            {
                var response = new ValidateAppResponse();
                response.Results["AppValidationResponse"] = new AppValidationResponse
                {
                    ValidationSuccess = true,
                    ValidationIssueList = Array.Empty<ValidationIssue>()
                };
                return response;
            });

            // AddAppComponentsRequest → no-op
            env.RegisterCustomApi("AddAppComponents", (request, service) =>
            {
                return new AddAppComponentsResponse();
            });

            // RemoveAppComponentsRequest → no-op
            env.RegisterCustomApi("RemoveAppComponents", (request, service) =>
            {
                return new RemoveAppComponentsResponse();
            });

            // Override RetrieveEntity to return full SDK metadata from XML files.
            // The Fake4Dataverse MetadataStore uses a lossy internal format that strips
            // option set labels, boolean options, and typed attribute subclasses.
            // By returning the original SDK EntityMetadata, we preserve all details.
            // Falls back to the MetadataStore for entities not in XML files (e.g. newly created).
            env.RegisterCustomApi("RetrieveEntity", (request, service) =>
            {
                var logicalName = request.Parameters.ContainsKey("LogicalName")
                    ? (string?)request["LogicalName"] : null;

                EntityMetadata? match = null;
                if (!string.IsNullOrEmpty(logicalName))
                {
                    match = customMetadata.FirstOrDefault(m =>
                        string.Equals(m.LogicalName, logicalName, StringComparison.OrdinalIgnoreCase));
                }

                if (match != null)
                {
                    var response = new RetrieveEntityResponse();
                    response.Results["EntityMetadata"] = match;
                    return response;
                }

                // Fall back to MetadataStore for entities not in XML files
                // (e.g. newly created via CreateEntityRequest).
                // Bypass our custom API override and use the built-in typed handler.
                var typedRequest = new RetrieveEntityRequest
                {
                    LogicalName = logicalName,
                    EntityFilters = request.Parameters.ContainsKey("EntityFilters")
                        ? (Microsoft.Xrm.Sdk.Metadata.EntityFilters)request["EntityFilters"]
                        : Microsoft.Xrm.Sdk.Metadata.EntityFilters.All
                };
                return (RetrieveEntityResponse)env.HandlerRegistry.ExecuteSkippingCustomApis(typedRequest, service);
            });

            // Override RetrieveAllEntities to return full SDK metadata
            env.RegisterCustomApi("RetrieveAllEntities", (request, service) =>
            {
                var response = new RetrieveAllEntitiesResponse();
                response.Results["EntityMetadata"] = customMetadata.ToArray();
                return response;
            });

            // Override RetrieveAttribute to return full SDK attribute metadata from XML files.
            // This preserves SchemaName, MaxLength, option set labels, boolean options, etc.
            // Falls back to the MetadataStore for attributes not in XML files (e.g. newly created).
            env.RegisterCustomApi("RetrieveAttribute", (request, service) =>
            {
                var entityLogicalName = (string?)request["EntityLogicalName"];
                var logicalName = (string?)request["LogicalName"];

                var entityMeta = customMetadata.FirstOrDefault(m =>
                    string.Equals(m.LogicalName, entityLogicalName, StringComparison.OrdinalIgnoreCase));

                if (entityMeta?.Attributes != null && !string.IsNullOrEmpty(logicalName))
                {
                    var attr = entityMeta.Attributes.FirstOrDefault(a =>
                        string.Equals(a.LogicalName, logicalName, StringComparison.OrdinalIgnoreCase));

                    if (attr != null)
                    {
                        var response = new RetrieveAttributeResponse();
                        response.Results["AttributeMetadata"] = attr;
                        return response;
                    }
                }

                // Fall back to MetadataStore for attributes not in XML files
                // (e.g. newly created via CreateAttributeRequest).
                var typedRequest = new RetrieveAttributeRequest
                {
                    EntityLogicalName = entityLogicalName,
                    LogicalName = logicalName
                };
                return (RetrieveAttributeResponse)env.HandlerRegistry.ExecuteSkippingCustomApis(typedRequest, service);
            });
        }

        /// <summary>
        /// Creates a PowerShell instance with all test cmdlets registered.
        /// This is the standard way to test cmdlet behavior in xUnit tests.
        /// </summary>
        /// <returns>A PowerShell instance ready for cmdlet invocation. Caller must dispose.</returns>
        protected static System.Management.Automation.PowerShell CreatePowerShellWithCmdlets()
        {
            var initialSessionState = System.Management.Automation.Runspaces.InitialSessionState.CreateDefault();
            
            // Register all cmdlets from the Commands assembly
            var cmdletTypes = typeof(Commands.GetDataverseConnectionCmdlet).Assembly
                .GetTypes()
                .Where(t => t.IsSubclassOf(typeof(System.Management.Automation.PSCmdlet)) && !t.IsAbstract);
            
            foreach (var cmdletType in cmdletTypes)
            {
                var cmdletAttr = cmdletType.GetCustomAttributes(typeof(System.Management.Automation.CmdletAttribute), false)
                    .FirstOrDefault() as System.Management.Automation.CmdletAttribute;
                
                if (cmdletAttr != null)
                {
                    var cmdletName = $"{cmdletAttr.VerbName}-{cmdletAttr.NounName}";
                    initialSessionState.Commands.Add(
                        new System.Management.Automation.Runspaces.SessionStateCmdletEntry(cmdletName, cmdletType, null));
                }
            }
            
            var runspace = System.Management.Automation.Runspaces.RunspaceFactory.CreateRunspace(initialSessionState);
            runspace.Open();
            
            var ps = System.Management.Automation.PowerShell.Create();
            ps.Runspace = runspace;
            return ps;
        }

        public virtual void Dispose()
        {
            // Clear thread-local and process default connections to avoid leakage between tests
            Rnwood.Dataverse.Data.PowerShell.Commands.SetDataverseConnectionAsDefaultCmdlet.ClearDefault();

            Service = null;
            Environment = null;
            Connection = null;
            GC.SuppressFinalize(this);
        }
    }
}
