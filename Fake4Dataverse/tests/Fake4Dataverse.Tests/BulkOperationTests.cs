using System;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Query;
using Xunit;

namespace Fake4Dataverse.Tests
{
    public class BulkOperationTests
    {
        [Fact]
        public void CreateMultiple_CreatesAllEntities()
        {
            var env = new FakeDataverseEnvironment();
            var service = env.CreateOrganizationService();
            var targets = new EntityCollection();
            targets.Entities.Add(new Entity("account") { ["name"] = "A" });
            targets.Entities.Add(new Entity("account") { ["name"] = "B" });
            targets.Entities.Add(new Entity("account") { ["name"] = "C" });

            var request = new OrganizationRequest("CreateMultiple");
            request.Parameters["Targets"] = targets;

            var response = service.Execute(request);
            var ids = (Guid[])response.Results["Ids"];

            Assert.Equal(3, ids.Length);
            foreach (var id in ids)
            {
                Assert.NotEqual(Guid.Empty, id);
            }

            var all = service.RetrieveMultiple(new QueryExpression("account") { ColumnSet = new ColumnSet(true) });
            Assert.Equal(3, all.Entities.Count);
        }

        [Fact]
        public void CreateMultiple_ReturnsCorrectIds()
        {
            var env = new FakeDataverseEnvironment();
            var service = env.CreateOrganizationService();
            var targets = new EntityCollection();
            targets.Entities.Add(new Entity("account") { ["name"] = "First" });
            targets.Entities.Add(new Entity("account") { ["name"] = "Second" });

            var request = new OrganizationRequest("CreateMultiple");
            request.Parameters["Targets"] = targets;

            var response = service.Execute(request);
            var ids = (Guid[])response.Results["Ids"];

            var first = service.Retrieve("account", ids[0], new ColumnSet("name"));
            Assert.Equal("First", first.GetAttributeValue<string>("name"));

            var second = service.Retrieve("account", ids[1], new ColumnSet("name"));
            Assert.Equal("Second", second.GetAttributeValue<string>("name"));
        }

        [Fact]
        public void UpdateMultiple_UpdatesAllEntities()
        {
            var env = new FakeDataverseEnvironment();
            var service = env.CreateOrganizationService();
            var id1 = service.Create(new Entity("account") { ["name"] = "Original1" });
            var id2 = service.Create(new Entity("account") { ["name"] = "Original2" });

            var targets = new EntityCollection();
            targets.Entities.Add(new Entity("account", id1) { ["name"] = "Updated1" });
            targets.Entities.Add(new Entity("account", id2) { ["name"] = "Updated2" });

            var request = new OrganizationRequest("UpdateMultiple");
            request.Parameters["Targets"] = targets;

            service.Execute(request);

            Assert.Equal("Updated1", service.Retrieve("account", id1, new ColumnSet("name")).GetAttributeValue<string>("name"));
            Assert.Equal("Updated2", service.Retrieve("account", id2, new ColumnSet("name")).GetAttributeValue<string>("name"));
        }

        [Fact]
        public void RetrieveVersion_ReturnsVersionString()
        {
            var env = new FakeDataverseEnvironment();
            var service = env.CreateOrganizationService();

            var response = service.Execute(new OrganizationRequest("RetrieveVersion"));

            var version = (string)response.Results["Version"];
            Assert.False(string.IsNullOrEmpty(version));
        }

        [Fact]
        public void CreateMultiple_EmptyTargets_ReturnsEmptyIds()
        {
            var env = new FakeDataverseEnvironment();
            var service = env.CreateOrganizationService();
            var targets = new EntityCollection();

            var request = new OrganizationRequest("CreateMultiple");
            request.Parameters["Targets"] = targets;

            var response = service.Execute(request);
            var ids = (Guid[])response.Results["Ids"];

            Assert.Empty(ids);
        }

        [Fact]
        public void UpdateMultiple_EmptyTargets_Succeeds()
        {
            var env = new FakeDataverseEnvironment();
            var service = env.CreateOrganizationService();
            var targets = new EntityCollection();

            var request = new OrganizationRequest("UpdateMultiple");
            request.Parameters["Targets"] = targets;

            // Should not throw
            service.Execute(request);
        }
    }
}
