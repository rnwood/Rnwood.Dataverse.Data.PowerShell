using System;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Messages;

namespace Fake4Dataverse.Handlers
{
    /// <summary>
    /// Handles <see cref="ExecuteTransactionRequest"/> by executing all requests atomically
    /// within a single logical transaction. If any request fails, all changes made by preceding
    /// requests in the batch are discarded. Changes are staged in a transaction-local
    /// copy-on-write buffer and only committed to the shared store when all requests succeed,
    /// matching real Dataverse all-or-nothing semantics.
    /// </summary>
    /// <remarks>
    /// <para><strong>Fidelity:</strong> Full</para>
    /// <para>All requests in the batch share a single <c>TransactionCopyOnWriteState</c>; failure of any request causes all staged mutations to be discarded. Nesting <see cref="ExecuteTransactionRequest"/> inside another <see cref="ExecuteTransactionRequest"/> throws <c>FaultException</c> matching real Dataverse behavior.</para>
    /// <para><strong>Configuration:</strong> None — all per-request options apply to each inner request as configured on the <see cref="FakeDataverseEnvironment"/>.</para>
    /// </remarks>
    internal sealed class ExecuteTransactionRequestHandler : IOrganizationRequestHandler
    {
        public bool CanHandle(OrganizationRequest request) =>
            string.Equals(request.RequestName, "ExecuteTransaction", System.StringComparison.OrdinalIgnoreCase);

        public OrganizationResponse Handle(OrganizationRequest request, IOrganizationService service)
        {
            var txRequest = OrganizationRequestTypeAdapter.AsTyped<ExecuteTransactionRequest>(request);
            var requests = txRequest.Requests;
            if (requests == null)
                throw DataverseFault.InvalidArgumentFault("ExecuteTransactionRequest.Requests must not be null.");

            // Validate nesting: ExecuteTransactionRequest cannot contain ExecuteMultiple or ExecuteTransaction
            for (int i = 0; i < requests.Count; i++)
            {
                var name = requests[i].RequestName;
                if (string.Equals(name, "ExecuteMultiple", StringComparison.OrdinalIgnoreCase)
                    || string.Equals(name, "ExecuteTransaction", StringComparison.OrdinalIgnoreCase))
                {
                    throw DataverseFault.Create(DataverseFault.Unspecified,
                        $"ExecuteTransactionRequest cannot contain nested {name} requests.");
                }
            }

            // Stage all writes in a dedicated copy-on-write transaction and commit only on success.
            InMemoryEntityStore? store = null;
            if (service is FakeOrganizationService fakeService)
                store = fakeService.Environment.Store;

            TransactionCopyOnWriteState? previousTransaction = null;
            var transaction = new TransactionCopyOnWriteState();
            if (store != null)
            {
                previousTransaction = store.ActiveTransaction;
                store.ActiveTransaction = transaction;
            }

            bool commit = false;

            try
            {
                var response = new ExecuteTransactionResponse();
                var responses = new OrganizationResponseCollection();

                for (int i = 0; i < requests.Count; i++)
                {
                    try
                    {
                        var subResponse = service.Execute(requests[i]);
                        responses.Add(subResponse);
                    }
                    catch (Exception ex)
                    {
                        response.Results["FaultedRequestIndex"] = i;
                        throw DataverseFault.Create(DataverseFault.Unspecified,
                            $"ExecuteTransaction failed at request index {i}: {ex.Message}");
                    }
                }

                response.Results["Responses"] = responses;
                commit = true;
                return response;
            }
            finally
            {
                if (store != null)
                {
                    store.ActiveTransaction = previousTransaction;
                    if (commit)
                        store.CommitTransaction(transaction);
                }
            }
        }
    }
}
