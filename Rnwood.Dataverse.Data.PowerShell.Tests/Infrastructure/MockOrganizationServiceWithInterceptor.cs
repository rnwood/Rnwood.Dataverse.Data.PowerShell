using System;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Query;

namespace Rnwood.Dataverse.Data.PowerShell.Tests.Infrastructure
{
    /// <summary>
    /// A wrapper around IOrganizationService that allows intercepting requests with a C# delegate.
    /// This is the xUnit equivalent of MockOrganizationServiceWithScriptBlock used in Pester tests.
    /// </summary>
    public class MockOrganizationServiceWithInterceptor : IOrganizationService
    {
        private readonly IOrganizationService _innerService;
        private readonly Func<OrganizationRequest, OrganizationResponse?>? _requestInterceptor;

        /// <summary>
        /// Initializes a new instance of the MockOrganizationServiceWithInterceptor class.
        /// </summary>
        /// <param name="innerService">The inner service to delegate to (typically FakeXrmEasy).</param>
        /// <param name="requestInterceptor">
        /// Optional delegate to intercept requests. Return an OrganizationResponse to short-circuit,
        /// or null to let the inner service handle the request.
        /// </param>
        public MockOrganizationServiceWithInterceptor(
            IOrganizationService innerService, 
            Func<OrganizationRequest, OrganizationResponse?>? requestInterceptor = null)
        {
            _innerService = innerService ?? throw new ArgumentNullException(nameof(innerService));
            _requestInterceptor = requestInterceptor;
        }

        /// <inheritdoc />
        public OrganizationResponse Execute(OrganizationRequest request)
        {
            // Try the interceptor first
            if (_requestInterceptor != null)
            {
                var interceptedResponse = _requestInterceptor(request);
                if (interceptedResponse != null)
                {
                    return interceptedResponse;
                }
            }

            // Delegate to inner service, but catch unsupported request exceptions
            try
            {
                return _innerService.Execute(request);
            }
            catch (Exception ex) when (IsUnsupportedRequestException(ex))
            {
                var requestTypeName = request.GetType().Name;
                throw new NotSupportedException(
                    $"FakeXrmEasy does not support '{requestTypeName}'. " +
                    $"Add a request interceptor in your test to handle this request type. " +
                    $"Example: CreateMockConnection(request => {{ if (request is {requestTypeName}) return new {requestTypeName.Replace("Request", "Response")}(); return null; }}, \"contact\")",
                    ex);
            }
        }

        private static bool IsUnsupportedRequestException(Exception ex)
        {
            // Check for common FakeXrmEasy "not supported" patterns
            if (ex is NotImplementedException)
                return true;
            
            var message = ex.Message?.ToLowerInvariant() ?? "";
            return message.Contains("not supported") ||
                   message.Contains("not implemented") ||
                   message.Contains("pull request") ||
                   message.Contains("virtual") ||
                   message.Contains("no fake message executor");
        }

        /// <inheritdoc />
        public void Associate(string entityName, Guid entityId, Relationship relationship, EntityReferenceCollection relatedEntities) =>
            _innerService.Associate(entityName, entityId, relationship, relatedEntities);

        /// <inheritdoc />
        public Guid Create(Entity entity) => _innerService.Create(entity);

        /// <inheritdoc />
        public void Delete(string entityName, Guid id) => _innerService.Delete(entityName, id);

        /// <inheritdoc />
        public void Disassociate(string entityName, Guid entityId, Relationship relationship, EntityReferenceCollection relatedEntities) =>
            _innerService.Disassociate(entityName, entityId, relationship, relatedEntities);

        /// <inheritdoc />
        public Entity Retrieve(string entityName, Guid id, ColumnSet columnSet) => _innerService.Retrieve(entityName, id, columnSet);

        /// <inheritdoc />
        public EntityCollection RetrieveMultiple(QueryBase query) => _innerService.RetrieveMultiple(query);

        /// <inheritdoc />
        public void Update(Entity entity) => _innerService.Update(entity);
    }
}
