using System;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Query;
using Xunit;

namespace Fake4Dataverse.Tests
{
    public class SnapshotTests
    {
        [Fact]
        public void TakeSnapshot_RestoreSnapshot_RestoresEntities()
        {
            var env = new FakeDataverseEnvironment();
            var service = env.CreateOrganizationService();
            var id = service.Create(new Entity("account") { ["name"] = "Contoso" });

            var snapshot = env.TakeSnapshot();
            service.Update(new Entity("account", id) { ["name"] = "Modified" });

            env.RestoreSnapshot(snapshot);

            var retrieved = service.Retrieve("account", id, new ColumnSet("name"));
            Assert.Equal("Contoso", retrieved.GetAttributeValue<string>("name"));
        }

        [Fact]
        public void RestoreSnapshot_RemovesEntitiesAddedAfterSnapshot()
        {
            var env = new FakeDataverseEnvironment();
            var service = env.CreateOrganizationService();
            var snapshot = env.TakeSnapshot();

            service.Create(new Entity("account") { ["name"] = "Contoso" });

            env.RestoreSnapshot(snapshot);

            var result = service.RetrieveMultiple(new QueryExpression("account") { ColumnSet = new ColumnSet(true) });
            Assert.Empty(result.Entities);
        }

        [Fact]
        public void RestoreSnapshot_RestoresDeletedEntities()
        {
            var env = new FakeDataverseEnvironment();
            var service = env.CreateOrganizationService();
            var id = service.Create(new Entity("account") { ["name"] = "Contoso" });
            var snapshot = env.TakeSnapshot();

            service.Delete("account", id);

            env.RestoreSnapshot(snapshot);

            var retrieved = service.Retrieve("account", id, new ColumnSet(true));
            Assert.Equal("Contoso", retrieved.GetAttributeValue<string>("name"));
        }

        [Fact]
        public void Scope_AutoRestoresOnDispose()
        {
            var env = new FakeDataverseEnvironment();
            var service = env.CreateOrganizationService();
            var id = service.Create(new Entity("account") { ["name"] = "Original" });

            using (env.Scope())
            {
                service.Update(new Entity("account", id) { ["name"] = "Modified" });
                var modified = service.Retrieve("account", id, new ColumnSet("name"));
                Assert.Equal("Modified", modified.GetAttributeValue<string>("name"));
            }

            var restored = service.Retrieve("account", id, new ColumnSet("name"));
            Assert.Equal("Original", restored.GetAttributeValue<string>("name"));
        }

        [Fact]
        public void Scope_RemovesCreatedEntitiesOnDispose()
        {
            var env = new FakeDataverseEnvironment();
            var service = env.CreateOrganizationService();

            using (env.Scope())
            {
                service.Create(new Entity("account") { ["name"] = "Temporary" });
                var result = service.RetrieveMultiple(new QueryExpression("account") { ColumnSet = new ColumnSet(true) });
                Assert.Single(result.Entities);
            }

            var afterScope = service.RetrieveMultiple(new QueryExpression("account") { ColumnSet = new ColumnSet(true) });
            Assert.Empty(afterScope.Entities);
        }

        [Fact]
        public void MultipleSnapshots_IndependentRestore()
        {
            var env = new FakeDataverseEnvironment();
            var service = env.CreateOrganizationService();
            var id = service.Create(new Entity("account") { ["name"] = "V1" });

            var snapshot1 = env.TakeSnapshot();
            service.Update(new Entity("account", id) { ["name"] = "V2" });

            var snapshot2 = env.TakeSnapshot();
            service.Update(new Entity("account", id) { ["name"] = "V3" });

            env.RestoreSnapshot(snapshot2);
            Assert.Equal("V2", service.Retrieve("account", id, new ColumnSet("name")).GetAttributeValue<string>("name"));

            env.RestoreSnapshot(snapshot1);
            Assert.Equal("V1", service.Retrieve("account", id, new ColumnSet("name")).GetAttributeValue<string>("name"));
        }

        [Fact]
        public void Snapshot_RestoresBinaryData()
        {
            var env = new FakeDataverseEnvironment();
            var service = env.CreateOrganizationService();
            var id = service.Create(new Entity("account") { ["name"] = "Contoso" });
            env.SetBinaryAttribute("account", id, "logo", new byte[] { 1, 2, 3 });

            var snapshot = env.TakeSnapshot();
            env.SetBinaryAttribute("account", id, "logo", new byte[] { 4, 5, 6 });

            env.RestoreSnapshot(snapshot);

            var data = env.GetBinaryAttribute("account", id, "logo");
            Assert.NotNull(data);
            Assert.Equal(new byte[] { 1, 2, 3 }, data);
        }

        [Fact]
        public void RestoreSnapshot_ThrowsForInvalidSnapshot()
        {
            var env = new FakeDataverseEnvironment();
            Assert.Throws<ArgumentException>(() => env.RestoreSnapshot("not a snapshot"));
        }

        [Fact]
        public void RestoreSnapshot_ThrowsForNull()
        {
            var env = new FakeDataverseEnvironment();
            Assert.Throws<ArgumentNullException>(() => env.RestoreSnapshot(null!));
        }

        [Fact]
        public void NestedScopes_RestoreCorrectly()
        {
            var env = new FakeDataverseEnvironment();
            var service = env.CreateOrganizationService();
            var id = service.Create(new Entity("account") { ["name"] = "Original" });

            using (env.Scope())
            {
                service.Update(new Entity("account", id) { ["name"] = "Level1" });

                using (env.Scope())
                {
                    service.Update(new Entity("account", id) { ["name"] = "Level2" });
                    Assert.Equal("Level2", service.Retrieve("account", id, new ColumnSet("name")).GetAttributeValue<string>("name"));
                }

                Assert.Equal("Level1", service.Retrieve("account", id, new ColumnSet("name")).GetAttributeValue<string>("name"));
            }

            Assert.Equal("Original", service.Retrieve("account", id, new ColumnSet("name")).GetAttributeValue<string>("name"));
        }
    }
}
