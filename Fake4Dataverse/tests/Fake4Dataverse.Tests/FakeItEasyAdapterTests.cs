using System;
using System.Threading;
using System.Threading.Tasks;
using System.ServiceModel;
using Fake4Dataverse.FakeItEasy;
using FakeItEasy;
using Microsoft.Crm.Sdk.Messages;
using Microsoft.PowerPlatform.Dataverse.Client;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Query;
using Xunit;

namespace Fake4Dataverse.Tests
{
    public class FakeItEasyAdapterTests
    {
        [Fact]
        public void AsFake_DelegatesOrganizationServiceOperations()
        {
            var env = new FakeDataverseEnvironment();
            var service = env.CreateOrganizationService();
            var fake = service.AsFake();

            var id = fake.Create(new Entity("account") { ["name"] = "Contoso" });
            fake.Update(new Entity("account", id) { ["name"] = "Fabrikam" });

            var retrieved = fake.Retrieve("account", id, new ColumnSet("name"));
            Assert.Equal("Fabrikam", retrieved.GetAttributeValue<string>("name"));

            fake.Delete("account", id);

            Assert.Throws<FaultException<OrganizationServiceFault>>(() =>
                service.Retrieve("account", id, new ColumnSet(true)));

            A.CallTo(() => fake.Create(A<Entity>.Ignored)).MustHaveHappenedOnceExactly();
            A.CallTo(() => fake.Update(A<Entity>.Ignored)).MustHaveHappenedOnceExactly();
            A.CallTo(() => fake.Delete("account", id)).MustHaveHappenedOnceExactly();
        }

        [Fact]
        public void AsFakeFactory_ReturnsServiceBackedOrganizationService()
        {
            var env = new FakeDataverseEnvironment();
            var service = env.CreateOrganizationService();
            var fakeFactory = service.AsFakeFactory();

            var organizationService = fakeFactory.CreateOrganizationService(Guid.NewGuid());
            var id = organizationService.Create(new Entity("account") { ["name"] = "Contoso" });

            var retrieved = service.Retrieve("account", id, new ColumnSet("name"));
            Assert.Equal("Contoso", retrieved.GetAttributeValue<string>("name"));

            A.CallTo(() => fakeFactory.CreateOrganizationService(A<Guid?>.Ignored)).MustHaveHappenedOnceExactly();
        }

        [Fact]
        public async Task AsFakeAsync_DelegatesAsyncOperations_ForAsyncAndAsync2Overloads()
        {
            var env = new FakeDataverseEnvironment();
            var service = env.CreateOrganizationService();
            var fakeAsync = service.AsFakeAsync();
            IOrganizationServiceAsync2 asyncService = fakeAsync;

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

            A.CallTo(() => fakeAsync.CreateAsync(A<Entity>.Ignored, A<CancellationToken>.Ignored)).MustHaveHappenedOnceExactly();
            A.CallTo(() => fakeAsync.RetrieveAsync(A<string>.Ignored, A<Guid>.Ignored, A<ColumnSet>.Ignored)).MustHaveHappenedOnceExactly();
            A.CallTo(() => fakeAsync.UpdateAsync(A<Entity>.Ignored, A<CancellationToken>.Ignored)).MustHaveHappenedOnceExactly();
            A.CallTo(() => fakeAsync.CreateAndReturnAsync(A<Entity>.Ignored, A<CancellationToken>.Ignored)).MustHaveHappenedOnceExactly();
            A.CallTo(() => fakeAsync.ExecuteAsync(A<OrganizationRequest>.Ignored, A<CancellationToken>.Ignored)).MustHaveHappenedOnceExactly();
        }
    }
}
