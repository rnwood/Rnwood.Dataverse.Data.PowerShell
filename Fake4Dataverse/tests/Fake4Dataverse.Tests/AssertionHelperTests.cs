using System;
using Microsoft.Crm.Sdk.Messages;
using Microsoft.Xrm.Sdk;
using Xunit;

namespace Fake4Dataverse.Tests
{
    public class AssertionHelperTests
    {
        [Fact]
        public void Should_HaveCreated_Passes_WhenEntityWasCreated()
        {
            var env = new FakeDataverseEnvironment();
            var service = env.CreateOrganizationService();
            var id = service.Create(new Entity("account") { ["name"] = "Contoso" });

            service.Should().HaveCreated("account");
            service.Should().HaveCreated("account", id);
        }

        [Fact]
        public void Should_HaveCreated_Throws_WhenEntityWasNotCreated()
        {
            var env = new FakeDataverseEnvironment();
            var service = env.CreateOrganizationService();

            Assert.Throws<FakeServiceAssertionException>(() =>
                service.Should().HaveCreated("account"));
        }

        [Fact]
        public void Should_HaveCreated_WithId_Throws_WhenDifferentIdCreated()
        {
            var env = new FakeDataverseEnvironment();
            var service = env.CreateOrganizationService();
            service.Create(new Entity("account") { ["name"] = "Contoso" });

            Assert.Throws<FakeServiceAssertionException>(() =>
                service.Should().HaveCreated("account", Guid.NewGuid()));
        }

        [Fact]
        public void Should_HaveUpdated_Passes_WhenEntityWasUpdated()
        {
            var env = new FakeDataverseEnvironment();
            var service = env.CreateOrganizationService();
            var id = service.Create(new Entity("account") { ["name"] = "Contoso" });
            service.Update(new Entity("account", id) { ["name"] = "Fabrikam" });

            service.Should().HaveUpdated("account", id);
        }

        [Fact]
        public void Should_HaveUpdated_Throws_WhenEntityWasNotUpdated()
        {
            var env = new FakeDataverseEnvironment();
            var service = env.CreateOrganizationService();
            var id = service.Create(new Entity("account") { ["name"] = "Contoso" });

            Assert.Throws<FakeServiceAssertionException>(() =>
                service.Should().HaveUpdated("account", id));
        }

        [Fact]
        public void Should_HaveUpdated_WithAttributes_Passes_WhenMatchingAttributesUpdated()
        {
            var env = new FakeDataverseEnvironment();
            var service = env.CreateOrganizationService();
            var id = service.Create(new Entity("account") { ["name"] = "Contoso" });
            service.Update(new Entity("account", id) { ["name"] = "Fabrikam" });

            service.Should().HaveUpdated("account", id, ("name", "Fabrikam"));
        }

        [Fact]
        public void Should_HaveUpdated_WithAttributes_Throws_WhenAttributesMismatch()
        {
            var env = new FakeDataverseEnvironment();
            var service = env.CreateOrganizationService();
            var id = service.Create(new Entity("account") { ["name"] = "Contoso" });
            service.Update(new Entity("account", id) { ["name"] = "Fabrikam" });

            Assert.Throws<FakeServiceAssertionException>(() =>
                service.Should().HaveUpdated("account", id, ("name", "WrongName")));
        }

        [Fact]
        public void Should_HaveDeleted_Passes_WhenEntityWasDeleted()
        {
            var env = new FakeDataverseEnvironment();
            var service = env.CreateOrganizationService();
            var id = service.Create(new Entity("account") { ["name"] = "Contoso" });
            service.Delete("account", id);

            service.Should().HaveDeleted("account", id);
        }

        [Fact]
        public void Should_HaveDeleted_Throws_WhenEntityWasNotDeleted()
        {
            var env = new FakeDataverseEnvironment();
            var service = env.CreateOrganizationService();
            var id = service.Create(new Entity("account") { ["name"] = "Contoso" });

            Assert.Throws<FakeServiceAssertionException>(() =>
                service.Should().HaveDeleted("account", id));
        }

        [Fact]
        public void Should_HaveExecuted_Passes_WhenRequestWasExecuted()
        {
            var env = new FakeDataverseEnvironment();
            var service = env.CreateOrganizationService();
            service.Execute(new WhoAmIRequest());

            service.Should().HaveExecuted<WhoAmIRequest>();
        }

        [Fact]
        public void Should_HaveExecuted_Throws_WhenRequestWasNotExecuted()
        {
            var env = new FakeDataverseEnvironment();
            var service = env.CreateOrganizationService();

            Assert.Throws<FakeServiceAssertionException>(() =>
                service.Should().HaveExecuted<WhoAmIRequest>());
        }

        [Fact]
        public void Should_HaveExecuted_ByName_Passes_WhenRequestWasExecuted()
        {
            var env = new FakeDataverseEnvironment();
            var service = env.CreateOrganizationService();
            service.Execute(new WhoAmIRequest());

            service.Should().HaveExecuted("WhoAmI");
        }

        [Fact]
        public void Should_NotHaveCreated_Passes_WhenNoCreate()
        {
            var env = new FakeDataverseEnvironment();
            var service = env.CreateOrganizationService();

            service.Should().NotHaveCreated("account");
        }

        [Fact]
        public void Should_NotHaveCreated_Throws_WhenCreateOccurred()
        {
            var env = new FakeDataverseEnvironment();
            var service = env.CreateOrganizationService();
            service.Create(new Entity("account") { ["name"] = "Contoso" });

            Assert.Throws<FakeServiceAssertionException>(() =>
                service.Should().NotHaveCreated("account"));
        }

        [Fact]
        public void Should_NotHaveDeleted_Passes_WhenNoDelete()
        {
            var env = new FakeDataverseEnvironment();
            var service = env.CreateOrganizationService();

            service.Should().NotHaveDeleted("account", Guid.NewGuid());
        }

        [Fact]
        public void Should_NotHaveExecuted_Passes_WhenNoExecute()
        {
            var env = new FakeDataverseEnvironment();
            var service = env.CreateOrganizationService();

            service.Should().NotHaveExecuted<WhoAmIRequest>();
        }

        [Fact]
        public void Should_ChainedAssertions_AllPass()
        {
            var env = new FakeDataverseEnvironment();
            var service = env.CreateOrganizationService();
            var id = service.Create(new Entity("account") { ["name"] = "Contoso" });
            service.Update(new Entity("account", id) { ["name"] = "Fabrikam" });
            service.Execute(new WhoAmIRequest());

            service.Should()
                .HaveCreated("account", id)
                .HaveUpdated("account", id)
                .HaveExecuted<WhoAmIRequest>()
                .NotHaveDeleted("account", id);
        }
    }
}
