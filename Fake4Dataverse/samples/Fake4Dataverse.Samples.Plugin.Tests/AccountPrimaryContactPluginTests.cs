using System;
using Fake4Dataverse.Pipeline;
using Fake4Dataverse.Samples.Plugin;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Query;
using Xunit;

namespace Fake4Dataverse.Samples.Plugin.Tests
{
    /// <summary>
    /// Sample tests for registering and executing a real Dataverse plugin with Fake4Dataverse.
    /// </summary>
    public class AccountPrimaryContactPluginTests
    {
        [Fact]
        public void CreateAccount_ExecutesPlugin_AndCreatesRelatedContact()
        {
            // Arrange
            var env = new FakeDataverseEnvironment();
            var service = env.CreateOrganizationService();
            env.Pipeline.RegisterStep("Create", PipelineStage.PostOperation, "account", new AccountPrimaryContactPlugin());

            // Act
            var accountId = service.Create(new Entity("account") { ["name"] = "Contoso" });

            // Assert
            var contacts = service.RetrieveMultiple(new QueryExpression("contact")
            {
                ColumnSet = new ColumnSet("lastname", "parentcustomerid")
            });

            Assert.Single(contacts.Entities);
            Assert.Equal("Contoso Primary Contact", contacts.Entities[0].GetAttributeValue<string>("lastname"));

            var parent = contacts.Entities[0].GetAttributeValue<EntityReference>("parentcustomerid");
            Assert.NotNull(parent);
            Assert.Equal("account", parent.LogicalName);
            Assert.Equal(accountId, parent.Id);
        }

        [Fact]
        public void CreateAccount_ExecutesPlugin_AndCapturesTrace()
        {
            // Arrange
            var env = new FakeDataverseEnvironment();
            var service = env.CreateOrganizationService();
            env.Pipeline.RegisterStep("Create", PipelineStage.PostOperation, "account", new AccountPrimaryContactPlugin());

            // Act
            service.Create(new Entity("account") { ["name"] = "Fabrikam" });

            // Assert
            Assert.Contains(env.Pipeline.Traces, trace => trace.IndexOf("Created primary contact for account", StringComparison.Ordinal) >= 0);
        }

        [Fact]
        public void CreateAccount_WithoutName_DoesNotCreateContact()
        {
            // Arrange
            var env = new FakeDataverseEnvironment();
            var service = env.CreateOrganizationService();
            env.Pipeline.RegisterStep("Create", PipelineStage.PostOperation, "account", new AccountPrimaryContactPlugin());

            // Act
            service.Create(new Entity("account"));

            // Assert
            var contacts = service.RetrieveMultiple(new QueryExpression("contact")
            {
                ColumnSet = new ColumnSet(true)
            });

            Assert.Empty(contacts.Entities);
        }
    }
}
