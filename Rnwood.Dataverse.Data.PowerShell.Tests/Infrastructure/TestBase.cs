using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Reflection;
using System.Runtime.Serialization;
using FakeItEasy;
using FakeXrmEasy.Abstractions;
using FakeXrmEasy.Abstractions.Enums;
using FakeXrmEasy.FakeMessageExecutors;
using FakeXrmEasy.Middleware;
using FakeXrmEasy.Middleware.Crud;
using FakeXrmEasy.Middleware.Messages;
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
    /// Base class for all tests providing FakeXrmEasy mock connection infrastructure.
    /// Mirrors the functionality of Common.ps1's getMockConnection() function.
    /// </summary>
    /// <remarks>
    /// Note: PowerShell cmdlet invocation is NOT available in xUnit tests because
    /// Microsoft.PowerShell.SDK only provides reference assemblies, not runtime assemblies.
    /// Use this base class for testing internal cmdlet logic via FakeXrmEasy.
    /// For full cmdlet integration testing, use E2E tests in Rnwood.Dataverse.Data.PowerShell.E2ETests/
    /// which use PowerShellProcessRunner to execute cmdlets in child PowerShell processes.
    /// </remarks>
    public abstract class TestBase : IDisposable
    {
        private static readonly Dictionary<string, EntityMetadata> MetadataCache = new();
        private static readonly object MetadataCacheLock = new();
        private List<EntityMetadata>? _loadedMetadata;
        
        // Fixed identity values for WhoAmIRequest (consistent per test)
        private readonly Guid _mockUserId = Guid.NewGuid();
        private readonly Guid _mockBusinessUnitId = Guid.NewGuid();
        private readonly Guid _mockOrganizationId = Guid.NewGuid();
        
        /// <summary>
        /// Gets the current test's organization service (wrapped with interceptor).
        /// </summary>
        protected IOrganizationService? Service { get; private set; }

        /// <summary>
        /// Gets the current test's XrmFakedContext.
        /// </summary>
        protected IXrmFakedContext? Context { get; private set; }

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
        /// Optional delegate to intercept requests before FakeXrmEasy handles them.
        /// Return an OrganizationResponse to short-circuit, or null to let FakeXrmEasy handle it.
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
        /// Use this when you need to modify entity metadata (e.g., add alternate keys) without affecting other tests.
        /// </summary>
        /// <param name="requestInterceptor">
        /// Optional delegate to intercept requests before FakeXrmEasy handles them.
        /// Return an OrganizationResponse to short-circuit, or null to let FakeXrmEasy handle it.
        /// </param>
        /// <param name="customMetadata">Custom entity metadata collection for this connection</param>
        /// <returns>A mock ServiceClient</returns>
        protected ServiceClient CreateMockConnectionWithCustomMetadata(
            Func<OrganizationRequest, OrganizationResponse?>? requestInterceptor,
            List<EntityMetadata> customMetadata)
        {
            _loadedMetadata = customMetadata;

            // Create XrmFakedContext with middleware - same approach as Get-DataverseConnection -Mock
            var context = MiddlewareBuilder
                .New()
                .AddCrud()
                .AddFakeMessageExecutors(Assembly.GetAssembly(typeof(RetrieveEntityRequestExecutor)))
                .UseMessages()
                .UseCrud()
                .SetLicense(FakeXrmEasyLicense.NonCommercial)
                .Build();

            // Initialize metadata
            context.InitializeMetadata(customMetadata);

            Context = context;
            var baseService = context.GetOrganizationService();

            // Combine user interceptor with default interceptors for unsupported FakeXrmEasy requests
            // This mirrors Common.ps1's getMockConnection behavior
            OrganizationResponse? CombinedInterceptor(OrganizationRequest request)
            {
                // First try the user's custom interceptor - it takes priority
                if (requestInterceptor != null)
                {
                    var customResult = requestInterceptor(request);
                    if (customResult != null)
                    {
                        return customResult;
                    }
                }

                if (request is RetrieveRequest retrieveRequest && retrieveRequest.Target != null)
                {
                    var target = retrieveRequest.Target;
                    if (target.LogicalName is "systemuser" or "team")
                    {
                        var entity = GetOrCreateEntity(context, target.LogicalName, target.Id);
                        var response = new RetrieveResponse();
                        response.Results["Entity"] = entity;
                        return response;
                    }

                    if (target.LogicalName == "contact" && target.Id == Guid.Empty)
                    {
                        var entity = GetOrCreateEntity(context, target.LogicalName, target.Id);
                        var response = new RetrieveResponse();
                        response.Results["Entity"] = entity;
                        return response;
                    }
                }

                if (request is RetrieveMultipleRequest retrieveMultipleRequest)
                {
                    // Handle RetrieveMultiple for organization entity (used by SQL4Cds DataSource constructor)
                    if (retrieveMultipleRequest.Query is QueryExpression qe && qe.EntityName == "organization")
                    {
                        var orgEntity = new Entity("organization", _mockOrganizationId)
                        {
                            ["name"] = "MockOrganization",
                            ["localeid"] = 1033 // English
                        };
                        var response = new RetrieveMultipleResponse();
                        response.Results["EntityCollection"] = new EntityCollection(new List<Entity> { orgEntity });
                        return response;
                    }

                    if (retrieveMultipleRequest.Query is QueryExpression qe2 && qe2.EntityName is "systemuser" or "team")
                    {
                        var targetId = ExtractIdFromFilters(qe2.Criteria);
                        var entity = GetOrCreateEntity(context, qe2.EntityName, targetId);
                        var response = new RetrieveMultipleResponse();
                        response.Results["EntityCollection"] = new EntityCollection(new List<Entity> { entity });
                        return response;
                    }

                    if (retrieveMultipleRequest.Query is QueryByAttribute qba && qba.EntityName is "systemuser" or "team")
                    {
                        var targetId = ExtractIdFromQueryByAttribute(qba);
                        var entity = GetOrCreateEntity(context, qba.EntityName, targetId);
                        var response = new RetrieveMultipleResponse();
                        response.Results["EntityCollection"] = new EntityCollection(new List<Entity> { entity });
                        return response;
                    }
                }

                // Handle AssignRequest by updating ownerid on the target entity
                if (request is AssignRequest assignRequest && assignRequest.Target != null && assignRequest.Assignee != null)
                {
                    var target = assignRequest.Target;
                    var assignee = assignRequest.Assignee;

                    var targetId = ResolveTargetId(target, context);
                    var entity = baseService.Retrieve(target.LogicalName, targetId, new ColumnSet(true));
                    entity["ownerid"] = new EntityReference(assignee.LogicalName, assignee.Id);
                    baseService.Update(entity);

                    return new AssignResponse { ResponseName = "Assign" };
                }

                // Handle SetStateRequest by setting statecode/statuscode on the entity
                if (request is SetStateRequest setStateRequest && setStateRequest.EntityMoniker != null)
                {
                    var target = setStateRequest.EntityMoniker;
                    var targetId = ResolveTargetId(target, context);
                    var entity = baseService.Retrieve(target.LogicalName, targetId, new ColumnSet(true));

                    if (setStateRequest.State != null)
                    {
                        entity["statecode"] = setStateRequest.State;
                    }
                    if (setStateRequest.Status != null)
                    {
                        entity["statuscode"] = setStateRequest.Status;
                    }

                    baseService.Update(entity);
                    return new SetStateResponse { ResponseName = "SetState" };
                }

                // Handle RetrieveUnpublishedMultipleRequest by delegating to regular RetrieveMultiple
                // This allows tests to work with forms and other components that query unpublished data first
                var requestTypeName = request.GetType().Name;
                if (requestTypeName == "RetrieveUnpublishedMultipleRequest")
                {
                    // Extract the Query property and execute as regular RetrieveMultiple
                    var queryProperty = request.GetType().GetProperty("Query");
                    if (queryProperty != null)
                    {
                        var query = queryProperty.GetValue(request) as QueryBase;
                        if (query != null)
                        {
                            var rmRequest = new RetrieveMultipleRequest { Query = query };
                            var rmResponse = baseService.Execute(rmRequest) as RetrieveMultipleResponse;
                            var response = new RetrieveUnpublishedMultipleResponse();
                            response.Results["EntityCollection"] = rmResponse?.EntityCollection ?? new EntityCollection();
                            return response;
                        }
                    }
                    // Fallback to empty collection if we can't extract query
                    var emptyResponse = new RetrieveUnpublishedMultipleResponse();
                    emptyResponse.Results["EntityCollection"] = new EntityCollection();
                    return emptyResponse;
                }

                // Handle RetrieveUnpublishedRequest similarly
                if (requestTypeName == "RetrieveUnpublishedRequest")
                {
                    var targetProperty = request.GetType().GetProperty("Target");
                    var columnSetProperty = request.GetType().GetProperty("ColumnSet");
                    if (targetProperty != null && columnSetProperty != null)
                    {
                        var target = targetProperty.GetValue(request) as EntityReference;
                        var columnSet = columnSetProperty.GetValue(request) as ColumnSet;
                        if (target != null && columnSet != null)
                        {
                            try
                            {
                                var entity = baseService.Retrieve(target.LogicalName, target.Id, columnSet);
                                var response = new RetrieveUnpublishedResponse();
                                response.Results["Entity"] = entity;
                                return response;
                            }
                            catch (System.ServiceModel.FaultException)
                            {
                                // Entity not found - re-throw the fault so cmdlet can handle it properly
                                throw;
                            }
                        }
                    }
                }

                // Handle unsupported requests that FakeXrmEasy doesn't support
                return DefaultInterceptor(request, customMetadata, _mockUserId, _mockBusinessUnitId, _mockOrganizationId);
            }

            // Wrap service layers:
            // 1. ThreadSafeOrganizationService - allows parallel operations without Clone()
            // 2. MockOrganizationServiceWithInterceptor - handles unsupported FakeXrmEasy requests
            var threadSafeService = new ThreadSafeOrganizationService(baseService);
            Service = new MockOrganizationServiceWithInterceptor(threadSafeService, CombinedInterceptor);

            // Create ServiceClient wrapper using factory
            // This ServiceClient does not support Clone(), so Invoke-DataverseParallel
            // will fall back to using the original connection (which is now thread-safe)
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
        /// Default request interceptor that handles unsupported FakeXrmEasy requests.
        /// Mirrors the behavior from Common.ps1's getMockConnection.
        /// </summary>
        protected static OrganizationResponse? DefaultInterceptor(
            OrganizationRequest request, 
            IEnumerable<EntityMetadata> loadedMetadata,
            Guid mockUserId,
            Guid mockBusinessUnitId,
            Guid mockOrganizationId)
        {
            var requestTypeName = request.GetType().Name;

            // Note: RetrieveUnpublishedMultipleRequest and RetrieveUnpublishedRequest are handled 
            // in CombinedInterceptor where we have access to the base service

            // Handle RetrieveAllEntitiesRequest - return loaded metadata
            if (requestTypeName == "RetrieveAllEntitiesRequest")
            {
                var response = new RetrieveAllEntitiesResponse();
                response.Results["EntityMetadata"] = loadedMetadata.ToArray();
                return response;
            }

            // Handle ValidateAppRequest - return success response
            if (requestTypeName == "ValidateAppRequest")
            {
                var response = new ValidateAppResponse();
                // Create minimal validation response indicating success
                var validationResponseType = typeof(ValidateAppResponse).Assembly
                    .GetType("Microsoft.Crm.Sdk.Messages.AppValidationResponse");
                if (validationResponseType != null)
                {
                    var validationResponse = Activator.CreateInstance(validationResponseType);
                    var listProp = validationResponseType.GetProperty("ValidationIssueList");
                    listProp?.SetValue(validationResponse, Array.Empty<object>());
                    response.Results["AppValidationResponse"] = validationResponse;
                }
                return response;
            }

            // Handle PublishXmlRequest - return empty response
            if (requestTypeName == "PublishXmlRequest")
            {
                return new PublishXmlResponse();
            }

            // Handle UpdateEntityRequest - return empty response
            if (requestTypeName == "UpdateEntityRequest")
            {
                return new UpdateEntityResponse();
            }

            // Handle RetrieveRequest for organization entity (used by GetBaseLanguageCode)
            if (request is RetrieveRequest retrieveRequest && 
                retrieveRequest.Target?.LogicalName == "organization")
            {
                var orgEntity = new Entity("organization", retrieveRequest.Target.Id)
                {
                    ["languagecode"] = 1033 // English
                };
                var response = new RetrieveResponse();
                response.Results["Entity"] = orgEntity;
                return response;
            }

            // Handle RetrieveEntityRequest - return entity metadata from cache
            if (request is RetrieveEntityRequest retrieveEntityRequest)
            {
                foreach (var meta in loadedMetadata)
                {
                    if (meta.LogicalName == retrieveEntityRequest.LogicalName)
                    {
                        var response = new RetrieveEntityResponse();
                        response.Results["EntityMetadata"] = meta;
                        return response;
                    }
                }
                // If entity not found, throw not found exception so cmdlet knows to create it
                throw new System.ServiceModel.FaultException<OrganizationServiceFault>(
                    new OrganizationServiceFault 
                    { 
                        ErrorCode = -2147220969, // Entity not found error code
                        Message = $"Entity '{retrieveEntityRequest.LogicalName}' does not exist"
                    },
                    new System.ServiceModel.FaultReason($"Entity '{retrieveEntityRequest.LogicalName}' does not exist"));
            }

            // Handle RetrieveAttributeRequest - return attribute metadata from entity metadata
            if (request is RetrieveAttributeRequest retrieveAttributeRequest)
            {
                foreach (var meta in loadedMetadata)
                {
                    if (meta.LogicalName == retrieveAttributeRequest.EntityLogicalName)
                    {
                        var attribute = meta.Attributes?.FirstOrDefault(
                            a => a.LogicalName == retrieveAttributeRequest.LogicalName);
                        if (attribute != null)
                        {
                            var response = new RetrieveAttributeResponse();
                            response.Results["AttributeMetadata"] = attribute;
                            return response;
                        }
                        // Attribute not found - throw not found exception
                        throw new System.ServiceModel.FaultException<OrganizationServiceFault>(
                            new OrganizationServiceFault 
                            { 
                                ErrorCode = -2147220969,
                                Message = $"Attribute '{retrieveAttributeRequest.LogicalName}' does not exist on entity '{retrieveAttributeRequest.EntityLogicalName}'"
                            },
                            new System.ServiceModel.FaultReason($"Attribute '{retrieveAttributeRequest.LogicalName}' does not exist"));
                    }
                }
                // Entity not found - throw not found exception
                throw new System.ServiceModel.FaultException<OrganizationServiceFault>(
                    new OrganizationServiceFault 
                    { 
                        ErrorCode = -2147220969,
                        Message = $"Entity '{retrieveAttributeRequest.EntityLogicalName}' does not exist"
                    },
                    new System.ServiceModel.FaultReason($"Entity '{retrieveAttributeRequest.EntityLogicalName}' does not exist"));
            }

            // Handle AddAppComponentsRequest and RemoveAppComponentsRequest
            if (requestTypeName == "AddAppComponentsRequest" || requestTypeName == "RemoveAppComponentsRequest")
            {
                return new OrganizationResponse();
            }

            // Handle WhoAmIRequest - return mock identity (by type or RequestName)
            if (request is WhoAmIRequest || request.RequestName == "WhoAmI")
            {
                var response = new WhoAmIResponse
                {
                    Results =
                    {
                        ["UserId"] = mockUserId,
                        ["BusinessUnitId"] = mockBusinessUnitId,
                        ["OrganizationId"] = mockOrganizationId
                    }
                };
                return response;
            }

            // Handle CreateAttributeRequest - return mock UUID
            if (requestTypeName == "CreateAttributeRequest" || request.RequestName == "CreateAttribute")
            {
                var response = new OrganizationResponse { ResponseName = "CreateAttribute" };
                response.Results["AttributeId"] = Guid.NewGuid();
                return response;
            }

            // Handle UpdateAttributeRequest - return empty response
            if (requestTypeName == "UpdateAttributeRequest" || request.RequestName == "UpdateAttribute")
            {
                return new OrganizationResponse { ResponseName = "UpdateAttribute" };
            }

            // Handle DeleteAttributeRequest - return empty response
            if (requestTypeName == "DeleteAttributeRequest" || request.RequestName == "DeleteAttribute")
            {
                return new OrganizationResponse { ResponseName = "DeleteAttribute" };
            }

            // Handle CreateEntityKeyRequest (by type name or RequestName property)
            if (requestTypeName == "CreateEntityKeyRequest" || request.RequestName == "CreateEntityKey")
            {
                // Return a CreateEntityKeyResponse with proper type
                var response = new CreateEntityKeyResponse();
                response.Results["EntityKeyId"] = Guid.NewGuid();
                return response;
            }

            // Handle DeleteEntityKeyRequest (by type name or RequestName property)
            if (requestTypeName == "DeleteEntityKeyRequest" || request.RequestName == "DeleteEntityKey")
            {
                return new OrganizationResponse { ResponseName = "DeleteEntityKey" };
            }

            // Handle RetrieveEntityKeyRequest - return not found by default
            // Tests that need specific keys should provide their own interceptor

            // Handle RetrieveMissingDependenciesRequest - return empty collection by default
            if (request is RetrieveMissingDependenciesRequest || requestTypeName == "RetrieveMissingDependenciesRequest")
            {
                var response = new RetrieveMissingDependenciesResponse();
                response.Results["EntityCollection"] = new EntityCollection();
                return response;
            }

            // Handle RetrieveDependenciesForUninstallRequest - return empty collection by default
            if (request is RetrieveDependenciesForUninstallRequest || requestTypeName == "RetrieveDependenciesForUninstallRequest")
            {
                var response = new RetrieveDependenciesForUninstallResponse();
                response.Results["EntityCollection"] = new EntityCollection();
                return response;
            }

            // Handle CreateEntityRequest - return mock entity metadata ID
            if (requestTypeName == "CreateEntityRequest" || request.RequestName == "CreateEntity")
            {
                var response = new CreateEntityResponse();
                response.Results["EntityId"] = Guid.NewGuid();
                response.Results["AttributeId"] = Guid.NewGuid();
                return response;
            }

            // Handle CreateManyToManyRequest - return mock relationship ID
            if (requestTypeName == "CreateManyToManyRequest" || request.RequestName == "CreateManyToMany")
            {
                var response = new CreateManyToManyResponse();
                response.Results["ManyToManyRelationshipId"] = Guid.NewGuid();
                return response;
            }

            // Handle CreateOneToManyRequest - return mock relationship ID
            if (requestTypeName == "CreateOneToManyRequest" || request.RequestName == "CreateOneToMany")
            {
                var response = new CreateOneToManyResponse();
                response.Results["RelationshipId"] = Guid.NewGuid();
                response.Results["AttributeId"] = Guid.NewGuid();
                return response;
            }

            // Handle QueryExpressionToFetchXmlRequest - convert QueryExpression to FetchXML
            if (request is QueryExpressionToFetchXmlRequest qeToFetchXmlRequest)
            {
                var queryExpression = qeToFetchXmlRequest.Query as QueryExpression;
                if (queryExpression == null)
                {
                    throw new ArgumentException("Query must be a QueryExpression");
                }
                var fetchXml = ConvertQueryExpressionToFetchXml(queryExpression);
                var response = new QueryExpressionToFetchXmlResponse();
                response.Results["FetchXml"] = fetchXml;
                return response;
            }

            // Handle RetrieveRelationshipRequest - return not found by default
            // Tests that need existing relationships should provide their own test data
            if (request is RetrieveRelationshipRequest || requestTypeName == "RetrieveRelationshipRequest" || request.RequestName == "RetrieveRelationship")
            {
                // Throw not found exception so cmdlet knows to create rather than update
                throw new System.ServiceModel.FaultException<OrganizationServiceFault>(
                    new OrganizationServiceFault 
                    { 
                        ErrorCode = -2147220969, // Not found error code
                        Message = "Relationship not found"
                    },
                    new System.ServiceModel.FaultReason("Relationship not found"));
            }

            // Don't return anything - let FakeXrmEasy handle the request
            return null;
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
        /// This uses serialization/deserialization to ensure a complete deep copy.
        /// </summary>
        protected static EntityMetadata CloneEntityMetadata(EntityMetadata source)
        {
            // Use DataContractSerializer for deep cloning
            var serializer = new DataContractSerializer(typeof(EntityMetadata));
            using var ms = new MemoryStream();
            serializer.WriteObject(ms, source);
            ms.Position = 0;
            return (EntityMetadata)serializer.ReadObject(ms)!;
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

        private static Guid ResolveTargetId(EntityReference target, IXrmFakedContext context)
        {
            if (target.Id != Guid.Empty)
            {
                return target.Id;
            }

            var existing = context.CreateQuery(target.LogicalName).FirstOrDefault();
            if (existing != null)
            {
                return existing.Id;
            }

            // If nothing exists yet, create a placeholder so follow-up requests succeed
            var placeholder = new Entity(target.LogicalName) { Id = Guid.NewGuid() };
            context.Initialize(new[] { placeholder });
            return placeholder.Id;
        }

        private static Entity GetOrCreateEntity(IXrmFakedContext context, string logicalName, Guid id)
        {
            var entity = id != Guid.Empty
                ? context.CreateQuery(logicalName).FirstOrDefault(e => e.Id == id)
                : context.CreateQuery(logicalName).FirstOrDefault();

            if (entity != null)
            {
                return entity;
            }

            var placeholder = new Entity(logicalName)
            {
                Id = id == Guid.Empty ? Guid.NewGuid() : id
            };
            context.Initialize(new[] { placeholder });
            return placeholder;
        }

        private static Guid ExtractIdFromFilters(FilterExpression criteria)
        {
            foreach (var condition in criteria.Conditions)
            {
                if (condition.AttributeName.Equals("systemuserid", StringComparison.OrdinalIgnoreCase) ||
                    condition.AttributeName.Equals("teamid", StringComparison.OrdinalIgnoreCase) ||
                    condition.AttributeName.Equals("id", StringComparison.OrdinalIgnoreCase))
                {
                    if (condition.Values.Count > 0 && condition.Values[0] is Guid guid)
                    {
                        return guid;
                    }
                }
            }

            foreach (var filter in criteria.Filters)
            {
                var nested = ExtractIdFromFilters(filter);
                if (nested != Guid.Empty)
                {
                    return nested;
                }
            }

            return Guid.Empty;
        }

        private static Guid ExtractIdFromQueryByAttribute(QueryByAttribute query)
        {
            for (int i = 0; i < query.Attributes.Count; i++)
            {
                var attr = query.Attributes[i] as string;
                if (attr != null && (attr.Equals("systemuserid", StringComparison.OrdinalIgnoreCase)
                    || attr.Equals("teamid", StringComparison.OrdinalIgnoreCase)
                    || attr.Equals("id", StringComparison.OrdinalIgnoreCase)))
                {
                    if (i < query.Values.Count && query.Values[i] is Guid guid)
                    {
                        return guid;
                    }
                }
            }

            return Guid.Empty;
        }

        /// <summary>
        /// Converts a QueryExpression to FetchXML. This is a simplified conversion
        /// for testing purposes that handles common scenarios.
        /// </summary>
        private static string ConvertQueryExpressionToFetchXml(QueryExpression query)
        {
            var fetchXml = new System.Xml.Linq.XElement("fetch",
                new System.Xml.Linq.XAttribute("mapping", "logical"));

            var entity = new System.Xml.Linq.XElement("entity",
                new System.Xml.Linq.XAttribute("name", query.EntityName));

            // Add columns
            if (query.ColumnSet.AllColumns)
            {
                entity.Add(new System.Xml.Linq.XElement("all-attributes"));
            }
            else
            {
                foreach (var column in query.ColumnSet.Columns)
                {
                    entity.Add(new System.Xml.Linq.XElement("attribute",
                        new System.Xml.Linq.XAttribute("name", column)));
                }
            }

            // Add orders
            foreach (var order in query.Orders)
            {
                entity.Add(new System.Xml.Linq.XElement("order",
                    new System.Xml.Linq.XAttribute("attribute", order.AttributeName),
                    new System.Xml.Linq.XAttribute("descending", order.OrderType == OrderType.Descending ? "true" : "false")));
            }

            // Add filter conditions (simplified)
            if (query.Criteria != null && (query.Criteria.Conditions.Count > 0 || query.Criteria.Filters.Count > 0))
            {
                var filter = ConvertFilterExpressionToFetchXml(query.Criteria);
                entity.Add(filter);
            }

            fetchXml.Add(entity);
            return fetchXml.ToString();
        }

        /// <summary>
        /// Converts a FilterExpression to a FetchXML filter element.
        /// </summary>
        private static System.Xml.Linq.XElement ConvertFilterExpressionToFetchXml(FilterExpression filterExpression)
        {
            var filter = new System.Xml.Linq.XElement("filter",
                new System.Xml.Linq.XAttribute("type", filterExpression.FilterOperator == LogicalOperator.And ? "and" : "or"));

            foreach (var condition in filterExpression.Conditions)
            {
                var conditionElement = new System.Xml.Linq.XElement("condition",
                    new System.Xml.Linq.XAttribute("attribute", condition.AttributeName),
                    new System.Xml.Linq.XAttribute("operator", GetFetchXmlOperator(condition.Operator)));

                if (condition.Values != null && condition.Values.Count > 0)
                {
                    if (condition.Values.Count == 1)
                    {
                        conditionElement.Add(new System.Xml.Linq.XAttribute("value", condition.Values[0]?.ToString() ?? ""));
                    }
                    else
                    {
                        foreach (var value in condition.Values)
                        {
                            conditionElement.Add(new System.Xml.Linq.XElement("value", value?.ToString() ?? ""));
                        }
                    }
                }

                filter.Add(conditionElement);
            }

            foreach (var nestedFilter in filterExpression.Filters)
            {
                filter.Add(ConvertFilterExpressionToFetchXml(nestedFilter));
            }

            return filter;
        }

        /// <summary>
        /// Maps ConditionOperator to FetchXML operator string.
        /// </summary>
        private static string GetFetchXmlOperator(ConditionOperator op)
        {
            return op switch
            {
                ConditionOperator.Equal => "eq",
                ConditionOperator.NotEqual => "ne",
                ConditionOperator.GreaterThan => "gt",
                ConditionOperator.GreaterEqual => "ge",
                ConditionOperator.LessThan => "lt",
                ConditionOperator.LessEqual => "le",
                ConditionOperator.Like => "like",
                ConditionOperator.NotLike => "not-like",
                ConditionOperator.In => "in",
                ConditionOperator.NotIn => "not-in",
                ConditionOperator.Null => "null",
                ConditionOperator.NotNull => "not-null",
                ConditionOperator.Between => "between",
                ConditionOperator.NotBetween => "not-between",
                ConditionOperator.Contains => "like",
                ConditionOperator.DoesNotContain => "not-like",
                ConditionOperator.BeginsWith => "begins-with",
                ConditionOperator.DoesNotBeginWith => "not-begin-with",
                ConditionOperator.EndsWith => "ends-with",
                ConditionOperator.DoesNotEndWith => "not-end-with",
                _ => "eq"
            };
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
            Service = null;
            Context = null;
            Connection = null;
            GC.SuppressFinalize(this);
        }
    }
}
