using System;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Messages;
using Microsoft.Xrm.Sdk.Query;

namespace Rnwood.Dataverse.Data.PowerShell.Tests.Infrastructure
{
    /// <summary>
    /// A wrapper around IOrganizationService that allows intercepting requests with a C# delegate.
    /// This enables testing cmdlets that make SDK calls with dynamic mock responses.
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

            if (request is ExecuteMultipleRequest executeMultipleRequest)
            {
                return HandleExecuteMultiple(executeMultipleRequest);
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

        private ExecuteMultipleResponse HandleExecuteMultiple(ExecuteMultipleRequest executeMultipleRequest)
        {
            var response = new ExecuteMultipleResponse();
            response.Results["Responses"] = new ExecuteMultipleResponseItemCollection();

            for (int i = 0; i < executeMultipleRequest.Requests.Count; i++)
            {
                var innerRequest = executeMultipleRequest.Requests[i];
                var item = new ExecuteMultipleResponseItem { RequestIndex = i };

                try
                {
                    OrganizationResponse innerResponse = null;
                    if (_requestInterceptor != null)
                    {
                        innerResponse = _requestInterceptor(innerRequest);
                    }

                    innerResponse ??= _innerService.Execute(innerRequest);
                    item.Response = innerResponse;
                }
                catch (Exception ex)
                {
                    item.Fault = new OrganizationServiceFault
                    {
                        Message = ex.Message,
                        ErrorCode = -1,
                        TraceText = ex.ToString()
                    };

                    if (executeMultipleRequest.Settings?.ContinueOnError == false)
                    {
                        response.Responses.Add(item);
                        break;
                    }
                }

                response.Responses.Add(item);
            }

            return response;
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
        public Entity Retrieve(string entityName, Guid id, ColumnSet columnSet)
        {
            if (_requestInterceptor != null)
            {
                var request = new RetrieveRequest
                {
                    Target = new EntityReference(entityName, id),
                    ColumnSet = columnSet
                };

                var response = _requestInterceptor(request) as RetrieveResponse;
                if (response?.Results.Contains("Entity") == true)
                {
                    return (Entity)response.Results["Entity"];
                }
            }

            return _innerService.Retrieve(entityName, id, columnSet);
        }

        /// <inheritdoc />
        public EntityCollection RetrieveMultiple(QueryBase query)
        {
            if (_requestInterceptor != null)
            {
                var request = new RetrieveMultipleRequest { Query = query };
                var response = _requestInterceptor(request) as RetrieveMultipleResponse;
                if (response?.Results.Contains("EntityCollection") == true)
                {
                    return (EntityCollection)response.Results["EntityCollection"];
                }
            }

            return _innerService.RetrieveMultiple(query);
        }

        /// <inheritdoc />
        public void Update(Entity entity) => _innerService.Update(entity);
    }
}
