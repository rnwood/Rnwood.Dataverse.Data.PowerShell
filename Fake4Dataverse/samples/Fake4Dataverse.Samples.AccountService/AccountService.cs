using System;
using Microsoft.Xrm.Sdk;

namespace Fake4Dataverse.Samples.AccountService
{
    /// <summary>
    /// A realistic service class that uses IOrganizationService to manage accounts.
    /// This represents production code that you would test with Fake4Dataverse.
    /// </summary>
    public class AccountService
    {
        private readonly IOrganizationService _service;

        public AccountService(IOrganizationService service)
        {
            _service = service ?? throw new ArgumentNullException(nameof(service));
        }

        public Guid CreateAccount(string name, decimal revenue)
        {
            var account = new Entity("account")
            {
                ["name"] = name,
                ["revenue"] = new Money(revenue)
            };
            return _service.Create(account);
        }

        public void DeactivateAccount(Guid accountId)
        {
            var update = new Entity("account", accountId)
            {
                ["statecode"] = new OptionSetValue(1),
                ["statuscode"] = new OptionSetValue(2)
            };
            _service.Update(update);
        }

        public void TransferAccount(Guid accountId, Guid newOwnerId)
        {
            var update = new Entity("account", accountId)
            {
                ["ownerid"] = new EntityReference("systemuser", newOwnerId)
            };
            _service.Update(update);
        }
    }
}
