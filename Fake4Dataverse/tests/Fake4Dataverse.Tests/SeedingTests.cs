using System;
using System.Collections.Generic;
using Fake4Dataverse.Pipeline;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Query;
using Xunit;

namespace Fake4Dataverse.Tests
{
    public class SeedingTests
    {
        [Fact]
        public void Seed_SingleEntity_CanBeRetrieved()
        {
            var env = new FakeDataverseEnvironment();
            var service = env.CreateOrganizationService();
            var id = Guid.NewGuid();
            var entity = new Entity("account", id) { ["name"] = "Contoso" };

            env.Seed(entity);

            var retrieved = service.Retrieve("account", id, new ColumnSet(true));
            Assert.Equal("Contoso", retrieved.GetAttributeValue<string>("name"));
        }

        [Fact]
        public void Seed_MultipleEntities()
        {
            var env = new FakeDataverseEnvironment();
            var service = env.CreateOrganizationService();
            var id1 = Guid.NewGuid();
            var id2 = Guid.NewGuid();

            env.Seed(
                new Entity("account", id1) { ["name"] = "Contoso" },
                new Entity("account", id2) { ["name"] = "Fabrikam" }
            );

            var result = service.RetrieveMultiple(new QueryExpression("account") { ColumnSet = new ColumnSet(true) });
            Assert.Equal(2, result.Entities.Count);
        }

        [Fact]
        public void Seed_DoesNotTriggerPipeline()
        {
            var env = new FakeDataverseEnvironment();
            var service = env.CreateOrganizationService();
            bool pipelineCalled = false;
            env.Pipeline.RegisterStep("Create", PipelineStage.PreValidation, ctx => pipelineCalled = true);

            var id = Guid.NewGuid();
            env.Seed(new Entity("account", id) { ["name"] = "Contoso" });

            Assert.False(pipelineCalled);
        }

        [Fact]
        public void Seed_DoesNotSetAutoFields()
        {
            var env = new FakeDataverseEnvironment();
            var service = env.CreateOrganizationService();
            var id = Guid.NewGuid();
            env.Seed(new Entity("account", id) { ["name"] = "Contoso" });

            var retrieved = service.Retrieve("account", id, new ColumnSet(true));
            Assert.False(retrieved.Contains("createdon"));
            Assert.False(retrieved.Contains("modifiedon"));
            Assert.False(retrieved.Contains("createdby"));
            Assert.False(retrieved.Contains("modifiedby"));
        }

        [Fact]
        public void Seed_DoesNotLogToOperationLog()
        {
            var env = new FakeDataverseEnvironment();
            var service = env.CreateOrganizationService();
            env.Seed(new Entity("account", Guid.NewGuid()) { ["name"] = "Contoso" });

            Assert.Empty(service.OperationLog.Records);
        }

        [Fact]
        public void Seed_WithGeneratedId()
        {
            var env = new FakeDataverseEnvironment();
            var service = env.CreateOrganizationService();
            var entity = new Entity("account") { ["name"] = "Contoso" };

            env.Seed(entity);

            var result = service.RetrieveMultiple(new QueryExpression("account") { ColumnSet = new ColumnSet(true) });
            Assert.Single(result.Entities);
            Assert.NotEqual(Guid.Empty, result.Entities[0].Id);
        }

        [Fact]
        public void Seed_IEnumerable_Works()
        {
            var env = new FakeDataverseEnvironment();
            var service = env.CreateOrganizationService();
            var entities = new List<Entity>
            {
                new Entity("account", Guid.NewGuid()) { ["name"] = "A" },
                new Entity("account", Guid.NewGuid()) { ["name"] = "B" }
            };

            env.Seed(entities);

            var result = service.RetrieveMultiple(new QueryExpression("account") { ColumnSet = new ColumnSet(true) });
            Assert.Equal(2, result.Entities.Count);
        }

        [Fact]
        public void SeedFromJson_BasicFormat()
        {
            var env = new FakeDataverseEnvironment();
            var service = env.CreateOrganizationService();
            var id = Guid.NewGuid();
            var json = $@"[{{""logicalName"": ""account"", ""id"": ""{id}"", ""attributes"": {{""name"": ""Contoso""}}}}]";

            env.SeedFromJson(json);

            var retrieved = service.Retrieve("account", id, new ColumnSet(true));
            Assert.Equal("Contoso", retrieved.GetAttributeValue<string>("name"));
        }

        [Fact]
        public void SeedFromJson_MultipleEntities()
        {
            var env = new FakeDataverseEnvironment();
            var service = env.CreateOrganizationService();
            var id1 = Guid.NewGuid();
            var id2 = Guid.NewGuid();
            var json = $@"[
                {{""logicalName"": ""account"", ""id"": ""{id1}"", ""attributes"": {{""name"": ""Contoso""}}}},
                {{""logicalName"": ""contact"", ""id"": ""{id2}"", ""attributes"": {{""lastname"": ""Doe""}}}}
            ]";

            env.SeedFromJson(json);

            Assert.Equal("Contoso", service.Retrieve("account", id1, new ColumnSet(true)).GetAttributeValue<string>("name"));
            Assert.Equal("Doe", service.Retrieve("contact", id2, new ColumnSet(true)).GetAttributeValue<string>("lastname"));
        }

        [Fact]
        public void SeedFromJson_NumericAttributes()
        {
            var env = new FakeDataverseEnvironment();
            var service = env.CreateOrganizationService();
            var id = Guid.NewGuid();
            var json = $@"[{{""logicalName"": ""account"", ""id"": ""{id}"", ""attributes"": {{""employeecount"": 42, ""revenue"": 1000.50}}}}]";

            env.SeedFromJson(json);

            var retrieved = service.Retrieve("account", id, new ColumnSet(true));
            Assert.Equal(42, retrieved.GetAttributeValue<int>("employeecount"));
            Assert.Equal(1000.50m, retrieved.GetAttributeValue<decimal>("revenue"));
        }

        [Fact]
        public void SeedFromJson_BooleanAttribute()
        {
            var env = new FakeDataverseEnvironment();
            var service = env.CreateOrganizationService();
            var id = Guid.NewGuid();
            var json = $@"[{{""logicalName"": ""account"", ""id"": ""{id}"", ""attributes"": {{""active"": true}}}}]";

            env.SeedFromJson(json);

            var retrieved = service.Retrieve("account", id, new ColumnSet(true));
            Assert.True(retrieved.GetAttributeValue<bool>("active"));
        }

        [Fact]
        public void SeedFromJson_WithoutId_GeneratesId()
        {
            var env = new FakeDataverseEnvironment();
            var service = env.CreateOrganizationService();
            var json = @"[{""logicalName"": ""account"", ""attributes"": {""name"": ""Contoso""}}]";

            env.SeedFromJson(json);

            var result = service.RetrieveMultiple(new QueryExpression("account") { ColumnSet = new ColumnSet(true) });
            Assert.Single(result.Entities);
            Assert.NotEqual(Guid.Empty, result.Entities[0].Id);
        }

        [Fact]
        public void EntityBuilder_BasicUsage()
        {
            var entity = new EntityBuilder("account")
                .WithAttribute("name", "Contoso")
                .Build();

            Assert.Equal("account", entity.LogicalName);
            Assert.Equal("Contoso", entity.GetAttributeValue<string>("name"));
        }

        [Fact]
        public void EntityBuilder_WithId()
        {
            var id = Guid.NewGuid();
            var entity = new EntityBuilder("account")
                .WithId(id)
                .WithAttribute("name", "Contoso")
                .Build();

            Assert.Equal(id, entity.Id);
        }

        [Fact]
        public void EntityBuilder_WithName()
        {
            var entity = new EntityBuilder("account")
                .WithName("Contoso")
                .Build();

            Assert.Equal("Contoso", entity.GetAttributeValue<string>("name"));
        }

        [Fact]
        public void EntityBuilder_WithState()
        {
            var entity = new EntityBuilder("account")
                .WithState(1, 2)
                .Build();

            Assert.Equal(1, entity.GetAttributeValue<OptionSetValue>("statecode").Value);
            Assert.Equal(2, entity.GetAttributeValue<OptionSetValue>("statuscode").Value);
        }

        [Fact]
        public void EntityBuilder_WithOwner()
        {
            var ownerId = Guid.NewGuid();
            var entity = new EntityBuilder("account")
                .WithOwner(ownerId)
                .Build();

            var owner = entity.GetAttributeValue<EntityReference>("ownerid");
            Assert.Equal(ownerId, owner.Id);
            Assert.Equal("systemuser", owner.LogicalName);
        }

        [Fact]
        public void EntityBuilder_IntegratesWithSeed()
        {
            var env = new FakeDataverseEnvironment();
            var service = env.CreateOrganizationService();
            var id = Guid.NewGuid();

            env.Seed(
                new EntityBuilder("account")
                    .WithId(id)
                    .WithName("Contoso")
                    .WithState(0)
                    .Build()
            );

            var retrieved = service.Retrieve("account", id, new ColumnSet(true));
            Assert.Equal("Contoso", retrieved.GetAttributeValue<string>("name"));
            Assert.Equal(0, retrieved.GetAttributeValue<OptionSetValue>("statecode").Value);
        }
    }
}
