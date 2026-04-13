using System;
using Fake4Dataverse.Samples.AccountService;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Query;
using Xunit;

namespace Fake4Dataverse.Samples.AccountService.Tests
{
    /// <summary>
    /// Sample tests demonstrating how to use Fake4Dataverse with a service layer class.
    /// </summary>
    public class AccountServiceTests
    {
        [Fact]
        public void CreateAccount_ReturnsNewId_AndSetsAttributes()
        {
            // Arrange
            var env = new FakeDataverseEnvironment();
            var service = env.CreateOrganizationService();
            var accountService = new AccountService(service);

            // Act
            var id = accountService.CreateAccount("Contoso Ltd", 5000000m);

            // Assert
            Assert.NotEqual(Guid.Empty, id);
            var account = service.Retrieve("account", id, new ColumnSet(true));
            Assert.Equal("Contoso Ltd", account["name"]);
            Assert.Equal(5000000m, ((Money)account["revenue"]).Value);

            service.Should().HaveCreated("account", id);
        }

        [Fact]
        public void DeactivateAccount_SetsStateCodes()
        {
            // Arrange
            var env = new FakeDataverseEnvironment();
            var service = env.CreateOrganizationService();
            var id = service.Create(new Entity("account") { ["name"] = "Contoso" });
            var accountService = new AccountService(service);

            // Act
            accountService.DeactivateAccount(id);

            // Assert
            var account = service.Retrieve("account", id, new ColumnSet("statecode", "statuscode"));
            Assert.Equal(1, ((OptionSetValue)account["statecode"]).Value);
            Assert.Equal(2, ((OptionSetValue)account["statuscode"]).Value);
        }

        [Fact]
        public void TransferAccount_UpdatesOwner()
        {
            // Arrange
            var env = new FakeDataverseEnvironment();
            var service = env.CreateOrganizationService();
            var id = service.Create(new Entity("account") { ["name"] = "Contoso" });
            var newOwner = Guid.NewGuid();
            var accountService = new AccountService(service);

            // Act
            accountService.TransferAccount(id, newOwner);

            // Assert
            var account = service.Retrieve("account", id, new ColumnSet("ownerid"));
            var ownerRef = (EntityReference)account["ownerid"];
            Assert.Equal(newOwner, ownerRef.Id);
            Assert.Equal("systemuser", ownerRef.LogicalName);

            service.Should().HaveUpdated("account", id);
        }

        [Fact]
        public void ScopedTest_ChangesAreRolledBack()
        {
            var env = new FakeDataverseEnvironment();
            var service = env.CreateOrganizationService();
            service.Create(new Entity("account") { ["name"] = "Permanent" });

            using (env.Scope())
            {
                service.Create(new Entity("account") { ["name"] = "Temporary" });
                var all = service.RetrieveMultiple(new QueryExpression("account") { ColumnSet = new ColumnSet(true) });
                Assert.Equal(2, all.Entities.Count);
            }

            var remaining = service.RetrieveMultiple(new QueryExpression("account") { ColumnSet = new ColumnSet(true) });
            Assert.Single(remaining.Entities);
            Assert.Equal("Permanent", remaining.Entities[0]["name"]);
        }

        [Fact]
        public void QueryWithLinkEntity_JoinsRelatedData()
        {
            var env = new FakeDataverseEnvironment();
            var service = env.CreateOrganizationService();
            var accountId = service.Create(new Entity("account") { ["name"] = "Contoso" });
            service.Create(new Entity("contact")
            {
                ["parentcustomerid"] = new EntityReference("account", accountId),
                ["lastname"] = "Smith"
            });
            service.Create(new Entity("contact")
            {
                ["parentcustomerid"] = new EntityReference("account", accountId),
                ["lastname"] = "Jones"
            });

            var query = new QueryExpression("contact")
            {
                ColumnSet = new ColumnSet("lastname")
            };
            var link = query.AddLink("account", "parentcustomerid", "accountid");
            link.Columns = new ColumnSet("name");
            link.EntityAlias = "acct";

            var results = service.RetrieveMultiple(query);
            Assert.Equal(2, results.Entities.Count);
        }
    }
}
