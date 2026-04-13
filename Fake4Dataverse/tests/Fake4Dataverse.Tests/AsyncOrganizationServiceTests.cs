using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Crm.Sdk.Messages;
using Microsoft.PowerPlatform.Dataverse.Client;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Query;
using Xunit;

namespace Fake4Dataverse.Tests
{
    public class AsyncOrganizationServiceTests
    {
        [Fact]
        public void FakeOrganizationService_Implements_IOrganizationServiceAsync2()
        {
            var env = new FakeDataverseEnvironment();
            var service = env.CreateOrganizationService();

            Assert.IsAssignableFrom<IOrganizationServiceAsync2>(service);
            Assert.IsAssignableFrom<IOrganizationServiceAsync>(service);
        }

        [Fact]
        public async Task IOrganizationServiceAsync_CreateAsync_WithoutCancellationToken_Works()
        {
            var env = new FakeDataverseEnvironment();
            var service = env.CreateOrganizationService();
            var asyncService = (IOrganizationServiceAsync)service;

            var id = await asyncService.CreateAsync(new Entity("account") { ["name"] = "Contoso" });

            Assert.NotEqual(Guid.Empty, id);
        }

        [Fact]
        public async Task CreateAsync_WithValidEntity_ReturnsIdAndStoresEntity()
        {
            var env = new FakeDataverseEnvironment();
            var service = env.CreateOrganizationService();
            var asyncService = (IOrganizationServiceAsync2)service;

            var id = await asyncService.CreateAsync(new Entity("account") { ["name"] = "Contoso" }, CancellationToken.None);

            Assert.NotEqual(Guid.Empty, id);
            var created = service.Retrieve("account", id, new ColumnSet("name"));
            Assert.Equal("Contoso", created.GetAttributeValue<string>("name"));
        }

        [Fact]
        public async Task CreateAndReturnAsync_WithValidEntity_ReturnsPersistedEntity()
        {
            var env = new FakeDataverseEnvironment();
            var service = env.CreateOrganizationService();
            var asyncService = (IOrganizationServiceAsync2)service;

            var created = await asyncService.CreateAndReturnAsync(new Entity("account") { ["name"] = "Contoso" }, CancellationToken.None);

            Assert.Equal("account", created.LogicalName);
            Assert.NotEqual(Guid.Empty, created.Id);
            Assert.Equal("Contoso", created.GetAttributeValue<string>("name"));
            Assert.Equal(created.Id, created.GetAttributeValue<Guid>("accountid"));
        }

        [Fact]
        public async Task RetrieveAsync_WithExistingRecord_ReturnsEntity()
        {
            var env = new FakeDataverseEnvironment();
            var service = env.CreateOrganizationService();
            var asyncService = (IOrganizationServiceAsync2)service;
            var id = service.Create(new Entity("account") { ["name"] = "Contoso" });

            var retrieved = await asyncService.RetrieveAsync("account", id, new ColumnSet("name"), CancellationToken.None);

            Assert.Equal(id, retrieved.Id);
            Assert.Equal("Contoso", retrieved.GetAttributeValue<string>("name"));
        }

        [Fact]
        public async Task RetrieveMultipleAsync_WithQueryExpression_ReturnsMatchingEntities()
        {
            var env = new FakeDataverseEnvironment();
            var service = env.CreateOrganizationService();
            var asyncService = (IOrganizationServiceAsync2)service;
            service.Create(new Entity("account") { ["name"] = "Contoso" });

            var results = await asyncService.RetrieveMultipleAsync(new QueryExpression("account") { ColumnSet = new ColumnSet("name") }, CancellationToken.None);

            Assert.Single(results.Entities);
            Assert.Equal("Contoso", results.Entities[0].GetAttributeValue<string>("name"));
        }

        [Fact]
        public async Task UpdateAsync_WithExistingRecord_UpdatesEntity()
        {
            var env = new FakeDataverseEnvironment();
            var service = env.CreateOrganizationService();
            var asyncService = (IOrganizationServiceAsync2)service;
            var id = service.Create(new Entity("account") { ["name"] = "Contoso" });

            await asyncService.UpdateAsync(new Entity("account", id) { ["name"] = "Fabrikam" }, CancellationToken.None);

            var updated = service.Retrieve("account", id, new ColumnSet("name"));
            Assert.Equal("Fabrikam", updated.GetAttributeValue<string>("name"));
        }

        [Fact]
        public async Task DeleteAsync_WithExistingRecord_RemovesEntity()
        {
            var env = new FakeDataverseEnvironment();
            var service = env.CreateOrganizationService();
            var asyncService = (IOrganizationServiceAsync2)service;
            var id = service.Create(new Entity("account") { ["name"] = "Contoso" });

            await asyncService.DeleteAsync("account", id, CancellationToken.None);

            Assert.Throws<System.ServiceModel.FaultException<OrganizationServiceFault>>(() =>
                service.Retrieve("account", id, new ColumnSet(true)));
        }

        [Fact]
        public async Task AssociateAsync_And_DisassociateAsync_MaintainRelationshipRecords()
        {
            var env = new FakeDataverseEnvironment();
            var service = env.CreateOrganizationService();
            var asyncService = (IOrganizationServiceAsync2)service;
            var accountId = service.Create(new Entity("account") { ["name"] = "Contoso" });
            var contactId = service.Create(new Entity("contact") { ["lastname"] = "Doe" });

            await asyncService.AssociateAsync(
                "account",
                accountId,
                new Relationship("account_contacts"),
                new EntityReferenceCollection { new EntityReference("contact", contactId) },
                CancellationToken.None);

            var associated = service.RetrieveMultiple(new QueryExpression("association_account_contacts") { ColumnSet = new ColumnSet(true) });
            Assert.Single(associated.Entities);

            await asyncService.DisassociateAsync(
                "account",
                accountId,
                new Relationship("account_contacts"),
                new EntityReferenceCollection { new EntityReference("contact", contactId) },
                CancellationToken.None);

            var disassociated = service.RetrieveMultiple(new QueryExpression("association_account_contacts") { ColumnSet = new ColumnSet(true) });
            Assert.Empty(disassociated.Entities);
        }

        [Fact]
        public async Task ExecuteAsync_WithWhoAmIRequest_ReturnsResponse()
        {
            var env = new FakeDataverseEnvironment();
            var service = env.CreateOrganizationService();
            var asyncService = (IOrganizationServiceAsync2)service;

            var response = await asyncService.ExecuteAsync(new WhoAmIRequest(), CancellationToken.None);

            var whoAmI = Assert.IsType<WhoAmIResponse>(response);
            Assert.NotEqual(Guid.Empty, whoAmI.UserId);
        }

        [Fact]
        public async Task AsyncMethods_WithCanceledToken_ThrowOperationCanceledException()
        {
            var env = new FakeDataverseEnvironment();
            var service = env.CreateOrganizationService();
            var asyncService = (IOrganizationServiceAsync2)service;
            var canceled = new CancellationToken(canceled: true);

            await Assert.ThrowsAsync<OperationCanceledException>(() =>
                asyncService.CreateAsync(new Entity("account") { ["name"] = "Contoso" }, canceled));
        }
    }
}