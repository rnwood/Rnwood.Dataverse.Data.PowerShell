using System;
using Microsoft.Crm.Sdk.Messages;
using Microsoft.Xrm.Sdk;

namespace Fake4Dataverse.Handlers
{
    /// <summary>
    /// Handles <see cref="RetrieveExchangeRateRequest"/> by returning the configured exchange rate for a transaction currency.
    /// </summary>
    /// <remarks>
    /// <para><strong>Fidelity:</strong> Full</para>
    /// <para>Returns the exact rate configured in <see cref="CurrencyManager"/> for the requested currency ID, or <c>1.0</c> when no explicit rate has been configured.</para>
    /// <para><strong>Configuration:</strong> Reads directly from <see cref="FakeDataverseEnvironment.Currency"/>.</para>
    /// </remarks>
    internal sealed class RetrieveExchangeRateRequestHandler : IOrganizationRequestHandler
    {
        public bool CanHandle(OrganizationRequest request) =>
            string.Equals(request.RequestName, "RetrieveExchangeRate", StringComparison.OrdinalIgnoreCase);

        public OrganizationResponse Handle(OrganizationRequest request, IOrganizationService service)
        {
            var retrieveRequest = OrganizationRequestTypeAdapter.AsTyped<RetrieveExchangeRateRequest>(request);
            var fakeService = service as FakeOrganizationService
                ?? throw new InvalidOperationException("RetrieveExchangeRateRequestHandler requires FakeOrganizationService.");

            var response = new RetrieveExchangeRateResponse();
            response.Results["ExchangeRate"] = fakeService.Environment.Currency.GetExchangeRate(retrieveRequest.TransactionCurrencyId);
            return response;
        }
    }
}