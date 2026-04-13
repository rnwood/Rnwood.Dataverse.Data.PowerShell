using Fake4Dataverse.Shouldly;
using Microsoft.Crm.Sdk.Messages;
using Microsoft.Xrm.Sdk;
using Shouldly;
using Xunit;

namespace Fake4Dataverse.Tests
{
    public class ShouldlyAdapterTests
    {
        [Fact]
        public void ShouldHaveCreatedAndExecuted_Passes_WhenOperationsWereRecorded()
        {
            var env = new FakeDataverseEnvironment();
            var service = env.CreateOrganizationService();
            var id = service.Create(new Entity("account") { ["name"] = "Contoso" });
            service.Execute(new WhoAmIRequest());

            Should.NotThrow(() =>
                service.ShouldHaveCreated("account", id)
                    .ShouldHaveExecuted<WhoAmIRequest>()
                    .ShouldNotHaveDeleted("account", id));
        }

        [Fact]
        public void ShouldHaveUpdated_Throws_WhenUpdateWasNotRecorded()
        {
            var env = new FakeDataverseEnvironment();
            var service = env.CreateOrganizationService();
            var id = service.Create(new Entity("account") { ["name"] = "Contoso" });

            Should.Throw<ShouldAssertException>(() => service.ShouldHaveUpdated("account", id));
        }
    }
}
