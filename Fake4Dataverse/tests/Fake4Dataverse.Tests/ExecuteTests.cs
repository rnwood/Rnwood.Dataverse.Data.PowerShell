using System;
using Microsoft.Crm.Sdk.Messages;
using Microsoft.Xrm.Sdk;
using Xunit;

namespace Fake4Dataverse.Tests
{
    public class ExecuteTests
    {
        [Fact]
        public void Execute_WhoAmI_ReturnsDefaultIds()
        {
            var env = new FakeDataverseEnvironment();
            var service = env.CreateOrganizationService();

            var response = (WhoAmIResponse)service.Execute(new WhoAmIRequest());

            Assert.NotEqual(Guid.Empty, response.UserId);
            Assert.NotEqual(Guid.Empty, response.OrganizationId);
            Assert.NotEqual(Guid.Empty, response.BusinessUnitId);
        }

        [Fact]
        public void Execute_CustomHandler_OverridesBuiltIn()
        {
            var env = new FakeDataverseEnvironment();
            var service = env.CreateOrganizationService();
            var customUserId = Guid.NewGuid();
            env.HandlerRegistry.Register(new Handlers.WhoAmIRequestHandler
            {
                UserId = customUserId
            });

            var response = (WhoAmIResponse)service.Execute(new WhoAmIRequest());

            Assert.Equal(customUserId, response.UserId);
        }

        [Fact]
        public void Execute_UnregisteredRequest_Throws()
        {
            var env = new FakeDataverseEnvironment();
            var service = env.CreateOrganizationService();
            var unknownRequest = new OrganizationRequest("SomeCustomAction");

            Assert.Throws<NotSupportedException>(() => service.Execute(unknownRequest));
        }
    }
}
