using System;
using Microsoft.Xrm.Sdk;

namespace Fake4Dataverse.Handlers
{
    internal static class OrganizationRequestTypeAdapter
    {
        internal static T AsTyped<T>(OrganizationRequest request)
            where T : OrganizationRequest, new()
        {
            if (request is T typedRequest)
                return typedRequest;

            var adapted = new T();
            if (!string.Equals(request.RequestName, adapted.RequestName, StringComparison.OrdinalIgnoreCase))
                throw new InvalidOperationException(
                    $"Request '{request.RequestName}' cannot be adapted to typed request '{typeof(T).Name}' ({adapted.RequestName}).");

            foreach (var kvp in request.Parameters)
                adapted.Parameters[kvp.Key] = kvp.Value;

            if (request.RequestId.HasValue)
                adapted.RequestId = request.RequestId;

            return adapted;
        }
    }
}
