using Microsoft.Xrm.Sdk;
using Xunit;

namespace Fake4Dataverse.Tests
{
    public class CustomApiRequestTests
    {
        [Fact]
        public void RegisterCustomApi_MatchesByRequestName()
        {
            var env = new FakeDataverseEnvironment();
            var service = env.CreateOrganizationService();
            env.RegisterCustomApi("myorg_CustomAction", (req, svc) =>
            {
                var response = new OrganizationResponse();
                response.Results["Output"] = $"Hello, {req["Input"]}!";
                return response;
            });

            var request = new OrganizationRequest("myorg_CustomAction");
            request["Input"] = "World";

            var result = service.Execute(request);
            Assert.Equal("Hello, World!", result.Results["Output"]);
        }

        [Fact]
        public void RegisterCustomApi_CaseInsensitiveMatch()
        {
            var env = new FakeDataverseEnvironment();
            var service = env.CreateOrganizationService();
            env.RegisterCustomApi("myorg_Action", (req, svc) => new OrganizationResponse());

            var request = new OrganizationRequest("MYORG_ACTION");
            service.Execute(request);
        }
    }
}
