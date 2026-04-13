using System;
using AwesomeAssertions;
using Fake4Dataverse.AwesomeAssertions;
using Microsoft.Crm.Sdk.Messages;
using Microsoft.Xrm.Sdk;
using Xunit;

namespace Fake4Dataverse.Tests
{
    public class AwesomeAssertionsAdapterTests
    {
        [Fact]
        public void Should_HaveCreatedUpdatedAndExecuted_Passes_WhenOperationsWereRecorded()
        {
            var env = new FakeDataverseEnvironment();
            var service = env.CreateOrganizationService();
            var id = service.Create(new Entity("account") { ["name"] = "Contoso" });
            service.Update(new Entity("account", id) { ["name"] = "Fabrikam" });
            service.Execute(new WhoAmIRequest());

            service.Should()
                .HaveCreated("account", id)
                .HaveUpdated("account", id)
                .HaveExecuted<WhoAmIRequest>();
        }

        [Fact]
        public void Should_HaveDeleted_Throws_WhenDeleteWasNotRecorded()
        {
            var env = new FakeDataverseEnvironment();
            var service = env.CreateOrganizationService();
            var id = service.Create(new Entity("account") { ["name"] = "Contoso" });

            Action act = () => service.Should().HaveDeleted("account", id);
            act.Should().Throw<Exception>();
        }
    }
}
