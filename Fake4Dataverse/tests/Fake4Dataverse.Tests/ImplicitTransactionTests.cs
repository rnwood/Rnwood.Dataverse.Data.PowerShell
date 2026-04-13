using System;
using Fake4Dataverse.Metadata;
using Fake4Dataverse.Pipeline;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Query;
using Xunit;
using InvalidPluginExecutionException = Microsoft.Xrm.Sdk.InvalidPluginExecutionException;

namespace Fake4Dataverse.Tests
{
    public class ImplicitTransactionTests
    {
        [Fact]
        public void Create_PostOperationThrows_RollsBackEntity()
        {
            var env = new FakeDataverseEnvironment();
            var service = env.CreateOrganizationService();

            env.Pipeline.RegisterPostOperation("Create", "account", _ =>
            {
                throw new InvalidPluginExecutionException("PostOp failure");
            });

            Assert.Throws<InvalidPluginExecutionException>(() =>
                service.Create(new Entity("account") { ["name"] = "Contoso" }));

            // The entity must not exist because the implicit transaction rolled back
            var all = service.RetrieveMultiple(new QueryExpression("account") { ColumnSet = new ColumnSet(true) });
            Assert.Empty(all.Entities);
        }

        [Fact]
        public void Update_PostOperationThrows_RollsBackChanges()
        {
            var env = new FakeDataverseEnvironment();
            var service = env.CreateOrganizationService();

            var id = service.Create(new Entity("account") { ["name"] = "Original" });

            env.Pipeline.RegisterPostOperation("Update", "account", _ =>
            {
                throw new InvalidPluginExecutionException("PostOp failure");
            });

            var update = new Entity("account", id) { ["name"] = "Modified" };
            Assert.Throws<InvalidPluginExecutionException>(() => service.Update(update));

            // Value must be reverted to original
            var retrieved = service.Retrieve("account", id, new ColumnSet(true));
            Assert.Equal("Original", retrieved.GetAttributeValue<string>("name"));
        }

        [Fact]
        public void Delete_PostOperationThrows_RollsBackDeletion()
        {
            var env = new FakeDataverseEnvironment();
            var service = env.CreateOrganizationService();

            var id = service.Create(new Entity("account") { ["name"] = "Contoso" });

            env.Pipeline.RegisterPostOperation("Delete", "account", _ =>
            {
                throw new InvalidPluginExecutionException("PostOp failure");
            });

            Assert.Throws<InvalidPluginExecutionException>(() => service.Delete("account", id));

            // Entity must still exist because the implicit transaction rolled back
            var retrieved = service.Retrieve("account", id, new ColumnSet(true));
            Assert.Equal("Contoso", retrieved.GetAttributeValue<string>("name"));
        }

        [Fact]
        public void Create_PreValidationThrows_NoEntityCreated()
        {
            var env = new FakeDataverseEnvironment();
            var service = env.CreateOrganizationService();

            env.Pipeline.RegisterPreValidation("Create", "account", _ =>
            {
                throw new InvalidPluginExecutionException("Validation failure");
            });

            Assert.Throws<InvalidPluginExecutionException>(() =>
                service.Create(new Entity("account") { ["name"] = "Contoso" }));

            var all1 = service.RetrieveMultiple(new QueryExpression("account") { ColumnSet = new ColumnSet(true) });
            Assert.Empty(all1.Entities);
        }

        [Fact]
        public void Create_PreOperationThrows_NoEntityCreated()
        {
            var env = new FakeDataverseEnvironment();
            var service = env.CreateOrganizationService();

            env.Pipeline.RegisterPreOperation("Create", "account", _ =>
            {
                throw new InvalidPluginExecutionException("PreOp failure");
            });

            Assert.Throws<InvalidPluginExecutionException>(() =>
                service.Create(new Entity("account") { ["name"] = "Contoso" }));

            var all2 = service.RetrieveMultiple(new QueryExpression("account") { ColumnSet = new ColumnSet(true) });
            Assert.Empty(all2.Entities);
        }

        [Fact]
        public void Update_PostOperationThrows_VersionNumberReverted()
        {
            var env = new FakeDataverseEnvironment();
            var service = env.CreateOrganizationService();

            var id = service.Create(new Entity("account") { ["name"] = "Original" });
            var afterCreate = service.Retrieve("account", id, new ColumnSet(true));
            var versionAfterCreate = afterCreate.GetAttributeValue<long>("versionnumber");

            env.Pipeline.RegisterPostOperation("Update", "account", _ =>
            {
                throw new InvalidPluginExecutionException("PostOp failure");
            });

            var update = new Entity("account", id) { ["name"] = "Modified" };
            Assert.Throws<InvalidPluginExecutionException>(() => service.Update(update));

            // Version number must be reverted to pre-update value
            var retrieved = service.Retrieve("account", id, new ColumnSet(true));
            Assert.Equal(versionAfterCreate, retrieved.GetAttributeValue<long>("versionnumber"));
        }

        [Fact]
        public void Create_SuccessWithPostOperation_EntityPersists()
        {
            var env = new FakeDataverseEnvironment();
            var service = env.CreateOrganizationService();
            bool postFired = false;

            env.Pipeline.RegisterPostOperation("Create", "account", _ =>
            {
                postFired = true;
            });

            var id = service.Create(new Entity("account") { ["name"] = "Contoso" });

            Assert.True(postFired);
            Assert.NotEqual(Guid.Empty, id);
            var retrieved = service.Retrieve("account", id, new ColumnSet(true));
            Assert.Equal("Contoso", retrieved.GetAttributeValue<string>("name"));
        }

        [Fact]
        public void Delete_PostOperationThrows_RelatedRecordNotOrphaned()
        {
            var env = new FakeDataverseEnvironment();
            var service = env.CreateOrganizationService();

            var accountId = service.Create(new Entity("account") { ["name"] = "Contoso" });
            var contactId = service.Create(new Entity("contact")
            {
                ["fullname"] = "John Doe",
                ["parentcustomerid"] = new EntityReference("account", accountId)
            });

            env.Pipeline.RegisterPostOperation("Delete", "account", _ =>
            {
                throw new InvalidPluginExecutionException("PostOp failure");
            });

            Assert.Throws<InvalidPluginExecutionException>(() =>
                service.Delete("account", accountId));

            // Parent account must still exist
            var parent = service.Retrieve("account", accountId, new ColumnSet(true));
            Assert.NotNull(parent);
        }

        [Fact]
        public void Delete_WithCascadeDelete_PostOperationThrows_RollsBackParentAndChildren()
        {
            var env = new FakeDataverseEnvironment();
            env.MetadataStore.AddOneToManyRelationship(
                "account_contacts",
                "account", "accountid",
                "contact", "parentcustomerid",
                new CascadeConfiguration { Delete = CascadeType.Cascade });

            var service = env.CreateOrganizationService();
            var accountId = service.Create(new Entity("account") { ["name"] = "Contoso" });
            var contactId = service.Create(new Entity("contact")
            {
                ["fullname"] = "John Doe",
                ["parentcustomerid"] = new EntityReference("account", accountId)
            });

            env.Pipeline.RegisterPostOperation("Delete", "account", _ =>
            {
                throw new InvalidPluginExecutionException("PostOp failure");
            });

            Assert.Throws<InvalidPluginExecutionException>(() => service.Delete("account", accountId));

            var parent = service.Retrieve("account", accountId, new ColumnSet(true));
            Assert.Equal("Contoso", parent.GetAttributeValue<string>("name"));

            var child = service.Retrieve("contact", contactId, new ColumnSet(true));
            Assert.Equal("John Doe", child.GetAttributeValue<string>("fullname"));
            Assert.Equal(accountId, child.GetAttributeValue<EntityReference>("parentcustomerid")?.Id);
        }

        [Fact]
        public void Create_WithoutPipeline_NoTransactionOverhead()
        {
            // When pipeline is disabled, operations should still work normally
            var options = FakeOrganizationServiceOptions.Lenient;
            var env = new FakeDataverseEnvironment(options);
            var service = env.CreateOrganizationService();

            var id = service.Create(new Entity("account") { ["name"] = "Contoso" });

            Assert.NotEqual(Guid.Empty, id);
            var retrieved = service.Retrieve("account", id, new ColumnSet(true));
            Assert.Equal("Contoso", retrieved.GetAttributeValue<string>("name"));
        }

        [Fact]
        public void Update_PostOperationThrows_MultipleFieldsReverted()
        {
            var env = new FakeDataverseEnvironment();
            var service = env.CreateOrganizationService();

            var id = service.Create(new Entity("account")
            {
                ["name"] = "Original Name",
                ["revenue"] = new Money(1000m),
                ["description"] = "Original Description"
            });

            env.Pipeline.RegisterPostOperation("Update", "account", _ =>
            {
                throw new InvalidPluginExecutionException("PostOp failure");
            });

            var update = new Entity("account", id)
            {
                ["name"] = "Modified Name",
                ["revenue"] = new Money(9999m),
                ["description"] = "Modified Description"
            };
            Assert.Throws<InvalidPluginExecutionException>(() => service.Update(update));

            var retrieved = service.Retrieve("account", id, new ColumnSet(true));
            Assert.Equal("Original Name", retrieved.GetAttributeValue<string>("name"));
            Assert.Equal(1000m, retrieved.GetAttributeValue<Money>("revenue").Value);
            Assert.Equal("Original Description", retrieved.GetAttributeValue<string>("description"));
        }

        [Fact]
        public void Create_PostOperationThrows_OtherEntitiesUnaffected()
        {
            var env = new FakeDataverseEnvironment();
            var service = env.CreateOrganizationService();

            // Create an existing entity first
            var existingId = service.Create(new Entity("contact") { ["fullname"] = "Jane" });

            env.Pipeline.RegisterPostOperation("Create", "account", _ =>
            {
                throw new InvalidPluginExecutionException("PostOp failure");
            });

            Assert.Throws<InvalidPluginExecutionException>(() =>
                service.Create(new Entity("account") { ["name"] = "Contoso" }));

            // The contact must still exist, unaffected by the failed account creation
            var contact = service.Retrieve("contact", existingId, new ColumnSet(true));
            Assert.NotNull(contact);
            var accounts = service.RetrieveMultiple(new QueryExpression("account") { ColumnSet = new ColumnSet(true) });
            Assert.Empty(accounts.Entities);
        }
    }
}
