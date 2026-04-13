using System;
using System.Threading;
using System.Threading.Tasks;
using Fake4Dataverse.Moq;
using Microsoft.Crm.Sdk.Messages;
using Microsoft.PowerPlatform.Dataverse.Client;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Query;
using Moq;
using Xunit;

namespace Fake4Dataverse.Tests
{
    public class MoqAdapterTests
    {
        [Fact]
        public void AsMock_DelegatesOrganizationServiceOperations()
        {
            var env = new FakeDataverseEnvironment();
            var service = env.CreateOrganizationService();
            var mock = service.AsMock();

            var id = mock.Object.Create(new Entity("account") { ["name"] = "Contoso" });
            mock.Object.Update(new Entity("account", id) { ["name"] = "Fabrikam" });

            var retrieved = mock.Object.Retrieve("account", id, new ColumnSet("name"));
            Assert.Equal("Fabrikam", retrieved.GetAttributeValue<string>("name"));

            mock.Verify(m => m.Create(It.IsAny<Entity>()), Times.Once);
            mock.Verify(m => m.Update(It.IsAny<Entity>()), Times.Once);
        }

        [Fact]
        public async Task AsMockAsync_DelegatesAsyncOperations_ForAsyncAndAsync2Overloads()
        {
            var env = new FakeDataverseEnvironment();
            var service = env.CreateOrganizationService();
            var mockAsync = service.AsMockAsync();
            IOrganizationServiceAsync2 asyncService = mockAsync.Object;

            var id = await asyncService.CreateAsync(
                new Entity("account") { ["name"] = "Contoso" },
                CancellationToken.None);

            // inherited IOrganizationServiceAsync overload (no token)
            var retrieved = await asyncService.RetrieveAsync("account", id, new ColumnSet("name"));
            Assert.Equal("Contoso", retrieved.GetAttributeValue<string>("name"));

            await asyncService.UpdateAsync(
                new Entity("account", id) { ["name"] = "Fabrikam" },
                CancellationToken.None);

            var created = await asyncService.CreateAndReturnAsync(
                new Entity("account") { ["name"] = "Tailspin" },
                CancellationToken.None);

            var whoAmI = await asyncService.ExecuteAsync(new WhoAmIRequest(), CancellationToken.None);
            Assert.IsType<WhoAmIResponse>(whoAmI);

            Assert.Equal("Tailspin", created.GetAttributeValue<string>("name"));

            mockAsync.Verify(m => m.CreateAsync(It.IsAny<Entity>(), It.IsAny<CancellationToken>()), Times.Once);
            mockAsync.Verify(m => m.RetrieveAsync(It.IsAny<string>(), It.IsAny<Guid>(), It.IsAny<ColumnSet>()), Times.Once);
            mockAsync.Verify(m => m.UpdateAsync(It.IsAny<Entity>(), It.IsAny<CancellationToken>()), Times.Once);
            mockAsync.Verify(m => m.CreateAndReturnAsync(It.IsAny<Entity>(), It.IsAny<CancellationToken>()), Times.Once);
            mockAsync.Verify(m => m.ExecuteAsync(It.IsAny<OrganizationRequest>(), It.IsAny<CancellationToken>()), Times.Once);
        }
    }
}