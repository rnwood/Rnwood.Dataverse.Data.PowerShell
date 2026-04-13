using System;
using Microsoft.Xrm.Sdk;

namespace Fake4Dataverse.Handlers
{
    internal static class AsyncOperationHelper
    {
        internal static Guid CreateCompletedAsyncOperation(IOrganizationService service, string name, string message)
        {
            var asyncOperation = new Entity("asyncoperation")
            {
                ["name"] = name,
                ["message"] = message,
                ["statecode"] = new OptionSetValue(3),
                ["statuscode"] = new OptionSetValue(30)
            };

            return service.Create(asyncOperation);
        }
    }
}