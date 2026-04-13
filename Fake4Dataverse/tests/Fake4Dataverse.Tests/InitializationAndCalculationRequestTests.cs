using Microsoft.Crm.Sdk.Messages;
using Microsoft.Xrm.Sdk;
using Xunit;

namespace Fake4Dataverse.Tests
{
    public class InitializationAndCalculationRequestTests
    {
        [Fact]
        public void InitializeFromRequest_CopiesSourceAttributes()
        {
            var env = new FakeDataverseEnvironment();
            var service = env.CreateOrganizationService();
            var accountId = service.Create(new Entity("account")
            {
                ["name"] = "Contoso",
                ["telephone1"] = "555-1234",
                ["revenue"] = new Money(1000000m)
            });

            var response = (InitializeFromResponse)service.Execute(new InitializeFromRequest
            {
                EntityMoniker = new EntityReference("account", accountId),
                TargetEntityName = "account"
            });

            var newEntity = (Entity)response.Results["Entity"];
            Assert.Equal("account", newEntity.LogicalName);
            Assert.Equal("Contoso", newEntity.GetAttributeValue<string>("name"));
            Assert.Equal("555-1234", newEntity.GetAttributeValue<string>("telephone1"));
            Assert.False(newEntity.Contains("createdon"));
            Assert.False(newEntity.Contains("modifiedon"));
        }

        [Fact]
        public void CalculateRollupFieldRequest_TriggersRollupCalculation()
        {
            var env = new FakeDataverseEnvironment();
            var service = env.CreateOrganizationService();

            env.CalculatedFields.RegisterRollupField(
                "account", "totalrevenue",
                "opportunity", "estimatedvalue", "parentaccountid",
                RollupType.Sum);

            var accountId = service.Create(new Entity("account") { ["name"] = "Contoso" });
            service.Create(new Entity("opportunity")
            {
                ["name"] = "Deal 1",
                ["estimatedvalue"] = 100m,
                ["parentaccountid"] = new EntityReference("account", accountId)
            });
            service.Create(new Entity("opportunity")
            {
                ["name"] = "Deal 2",
                ["estimatedvalue"] = 200m,
                ["parentaccountid"] = new EntityReference("account", accountId)
            });

            var response = (CalculateRollupFieldResponse)service.Execute(new CalculateRollupFieldRequest
            {
                Target = new EntityReference("account", accountId),
                FieldName = "totalrevenue"
            });

            var entity = (Entity)response.Results["Entity"];
            Assert.Equal(300m, entity.GetAttributeValue<decimal>("totalrevenue"));
        }
    }
}
