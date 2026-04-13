using System;
using System.Collections.Generic;
using System.ServiceModel;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Messages;
using Microsoft.Xrm.Sdk.Query;
using Xunit;

namespace Fake4Dataverse.Tests
{
    public class CrudTests
    {
        [Fact]
        public void Create_ReturnsNewGuid()
        {
            var env = new FakeDataverseEnvironment();
            var service = env.CreateOrganizationService();
            var entity = new Entity("account") { ["name"] = "Contoso" };

            var id = service.Create(entity);

            Assert.NotEqual(Guid.Empty, id);
        }

        [Fact]
        public void Create_WithExplicitId_UsesProvidedId()
        {
            var env = new FakeDataverseEnvironment();
            var service = env.CreateOrganizationService();
            var expectedId = Guid.NewGuid();
            var entity = new Entity("account", expectedId) { ["name"] = "Contoso" };

            var id = service.Create(entity);

            Assert.Equal(expectedId, id);
        }

        [Fact]
        public void Retrieve_ReturnsCreatedEntity()
        {
            var env = new FakeDataverseEnvironment();
            var service = env.CreateOrganizationService();
            var entity = new Entity("account") { ["name"] = "Contoso" };
            var id = service.Create(entity);

            var retrieved = service.Retrieve("account", id, new ColumnSet(true));

            Assert.Equal("Contoso", retrieved.GetAttributeValue<string>("name"));
            Assert.Equal(id, retrieved.Id);
        }

        [Fact]
        public void Retrieve_WithColumnSet_ReturnsOnlyRequestedColumns()
        {
            var env = new FakeDataverseEnvironment();
            var service = env.CreateOrganizationService();
            var entity = new Entity("account") { ["name"] = "Contoso", ["revenue"] = new Money(1000m) };
            var id = service.Create(entity);

            var retrieved = service.Retrieve("account", id, new ColumnSet("name"));

            Assert.Equal("Contoso", retrieved.GetAttributeValue<string>("name"));
            Assert.False(retrieved.Contains("revenue"));
        }

        [Fact]
        public void Retrieve_NonExistent_ThrowsFaultException()
        {
            var env = new FakeDataverseEnvironment();
            var service = env.CreateOrganizationService();

            var ex = Assert.Throws<FaultException<OrganizationServiceFault>>(() =>
                service.Retrieve("account", Guid.NewGuid(), new ColumnSet(true)));
            Assert.Equal(DataverseFault.ObjectDoesNotExist, ex.Detail.ErrorCode);
        }

        [Fact]
        public void Update_ModifiesExistingEntity()
        {
            var env = new FakeDataverseEnvironment();
            var service = env.CreateOrganizationService();
            var id = service.Create(new Entity("account") { ["name"] = "Contoso" });

            service.Update(new Entity("account", id) { ["name"] = "Fabrikam" });

            var retrieved = service.Retrieve("account", id, new ColumnSet("name"));
            Assert.Equal("Fabrikam", retrieved.GetAttributeValue<string>("name"));
        }

        [Fact]
        public void Delete_RemovesEntity()
        {
            var env = new FakeDataverseEnvironment();
            var service = env.CreateOrganizationService();
            var id = service.Create(new Entity("account") { ["name"] = "Contoso" });

            service.Delete("account", id);

            Assert.Throws<FaultException<OrganizationServiceFault>>(() =>
                service.Retrieve("account", id, new ColumnSet(true)));
        }

        [Fact]
        public void Delete_NonExistent_ThrowsFaultException()
        {
            var env = new FakeDataverseEnvironment();
            var service = env.CreateOrganizationService();

            var ex = Assert.Throws<FaultException<OrganizationServiceFault>>(() =>
                service.Delete("account", Guid.NewGuid()));
            Assert.Equal(DataverseFault.ObjectDoesNotExist, ex.Detail.ErrorCode);
        }

        [Fact]
        public void Reset_ClearsAllData()
        {
            var env = new FakeDataverseEnvironment();
            var service = env.CreateOrganizationService();
            service.Create(new Entity("account") { ["name"] = "Contoso" });
            service.Create(new Entity("contact") { ["lastname"] = "Doe" });

            env.Reset();

            var accounts = service.RetrieveMultiple(new QueryExpression("account") { ColumnSet = new ColumnSet(true) });
            Assert.Empty(accounts.Entities);
        }

        [Fact]
        public void Create_DuplicateId_ThrowsFaultException()
        {
            var env = new FakeDataverseEnvironment();
            var service = env.CreateOrganizationService();
            var id = service.Create(new Entity("account") { ["name"] = "Contoso" });

            var ex = Assert.Throws<FaultException<OrganizationServiceFault>>(() =>
                service.Create(new Entity("account", id) { ["name"] = "Duplicate" }));
            Assert.Equal(DataverseFault.DuplicateRecord, ex.Detail.ErrorCode);
        }

        [Fact]
        public void Create_EmptyString_StoredAsNull()
        {
            var env = new FakeDataverseEnvironment();
            var service = env.CreateOrganizationService();
            var id = service.Create(new Entity("lead") { ["subject"] = string.Empty });

            var retrieved = service.Retrieve("lead", id, new ColumnSet("subject"));
            Assert.Null(retrieved.GetAttributeValue<string>("subject"));
        }

        [Fact]
        public void Update_EmptyString_StoredAsNull()
        {
            var env = new FakeDataverseEnvironment();
            var service = env.CreateOrganizationService();
            var id = service.Create(new Entity("lead") { ["subject"] = "nonemptystring" });

            service.Update(new Entity("lead", id) { ["subject"] = string.Empty });

            var retrieved = service.Retrieve("lead", id, new ColumnSet("subject"));
            Assert.Null(retrieved.GetAttributeValue<string>("subject"));
        }

        [Fact]
        public void Create_WithCreateRequest_Works()
        {
            var env = new FakeDataverseEnvironment();
            var service = env.CreateOrganizationService();
            var entity = new Entity("contact") { ["firstname"] = "John" };
            var req = new CreateRequest { Target = entity };

            var resp = (CreateResponse)service.Execute(req);

            Assert.NotEqual(Guid.Empty, resp.id);
            var retrieved = service.Retrieve("contact", resp.id, new ColumnSet("firstname"));
            Assert.Equal("John", retrieved.GetAttributeValue<string>("firstname"));
        }

        [Fact]
        public void Update_WithUpdateRequest_Works()
        {
            var env = new FakeDataverseEnvironment();
            var service = env.CreateOrganizationService();
            var id = service.Create(new Entity("contact") { ["firstname"] = "John" });
            var update = new Entity("contact", id) { ["firstname"] = "Jane" };

            service.Execute(new UpdateRequest { Target = update });

            var retrieved = service.Retrieve("contact", id, new ColumnSet("firstname"));
            Assert.Equal("Jane", retrieved.GetAttributeValue<string>("firstname"));
        }

        [Fact]
        public void Delete_WithDeleteRequest_Works()
        {
            var env = new FakeDataverseEnvironment();
            var service = env.CreateOrganizationService();
            var id = service.Create(new Entity("account") { ["name"] = "ToDelete" });

            service.Execute(new DeleteRequest
            {
                Target = new EntityReference("account", id)
            });

            Assert.Throws<FaultException<OrganizationServiceFault>>(() =>
                service.Retrieve("account", id, new ColumnSet(true)));
        }

        [Fact]
        public void Retrieve_NullColumnSet_ReturnsAllAttributes()
        {
            var env = new FakeDataverseEnvironment();
            var service = env.CreateOrganizationService();
            var id = service.Create(new Entity("account") { ["name"] = "Contoso", ["revenue"] = new Money(1000m) });

            // null ColumnSet should behave like AllColumns=true
            var retrieved = service.Retrieve("account", id, null!);

            Assert.Equal("Contoso", retrieved.GetAttributeValue<string>("name"));
            Assert.NotNull(retrieved.GetAttributeValue<Money>("revenue"));
        }

        [Fact]
        public void Retrieve_AlwaysIncludesId()
        {
            var env = new FakeDataverseEnvironment();
            var service = env.CreateOrganizationService();
            var id = service.Create(new Entity("account") { ["name"] = "Contoso" });

            // Request only "name" column — Id should still be present
            var retrieved = service.Retrieve("account", id, new ColumnSet("name"));

            Assert.Equal(id, retrieved.Id);
        }

        [Fact]
        public void Retrieve_WithEmptyColumnSet_StillReturnsId()
        {
            var env = new FakeDataverseEnvironment();
            var service = env.CreateOrganizationService();
            var id = service.Create(new Entity("account") { ["name"] = "Contoso" });

            var retrieved = service.Retrieve("account", id, new ColumnSet(false));

            Assert.Equal(id, retrieved.Id);
            Assert.False(retrieved.Contains("name"));
        }

        [Fact]
        public void Retrieve_NonExistentAttribute_SilentlyOmitted()
        {
            var env = new FakeDataverseEnvironment();
            var service = env.CreateOrganizationService();
            var id = service.Create(new Entity("account") { ["name"] = "Contoso" });

            var retrieved = service.Retrieve("account", id, new ColumnSet("name", "nonexistent_field_xyz"));

            Assert.Equal("Contoso", retrieved.GetAttributeValue<string>("name"));
            Assert.False(retrieved.Contains("nonexistent_field_xyz"));
        }

        [Fact]
        public void Update_PartialAttributes_OnlyUpdatesProvided()
        {
            var env = new FakeDataverseEnvironment();
            var service = env.CreateOrganizationService();
            var id = service.Create(new Entity("account") { ["name"] = "Contoso", ["revenue"] = new Money(1000m) });

            // Update only name — revenue should remain unchanged
            service.Update(new Entity("account", id) { ["name"] = "Fabrikam" });

            var retrieved = service.Retrieve("account", id, new ColumnSet("name", "revenue"));
            Assert.Equal("Fabrikam", retrieved.GetAttributeValue<string>("name"));
            Assert.Equal(1000m, retrieved.GetAttributeValue<Money>("revenue").Value);
        }

        [Fact]
        public void Update_NonExistent_ThrowsFaultException()
        {
            var env = new FakeDataverseEnvironment();
            var service = env.CreateOrganizationService();

            var ex = Assert.Throws<FaultException<OrganizationServiceFault>>(() =>
                service.Update(new Entity("account", Guid.NewGuid()) { ["name"] = "Ghost" }));
            Assert.Equal(DataverseFault.ObjectDoesNotExist, ex.Detail.ErrorCode);
        }

        [Fact]
        public void Create_NullEntity_ThrowsArgumentException()
        {
            var env = new FakeDataverseEnvironment();
            var service = env.CreateOrganizationService();
            Assert.Throws<ArgumentNullException>(() => service.Create(null!));
        }

        [Fact]
        public void Update_NullEntity_ThrowsArgumentException()
        {
            var env = new FakeDataverseEnvironment();
            var service = env.CreateOrganizationService();
            Assert.Throws<ArgumentNullException>(() => service.Update(null!));
        }

        [Fact]
        public void Create_EmptyEntityName_ThrowsArgumentException()
        {
            var env = new FakeDataverseEnvironment();
            var service = env.CreateOrganizationService();
            Assert.Throws<ArgumentException>(() => service.Create(new Entity("")));
        }

        [Fact]
        public void Update_EmptyId_ThrowsArgumentException()
        {
            var env = new FakeDataverseEnvironment();
            var service = env.CreateOrganizationService();
            Assert.Throws<ArgumentException>(() => service.Update(new Entity("account") { ["name"] = "X" }));
        }

        [Fact]
        public void Create_SetsPrimaryIdAttribute()
        {
            var env = new FakeDataverseEnvironment();
            var service = env.CreateOrganizationService();
            var id = service.Create(new Entity("account") { ["name"] = "Contoso" });

            var retrieved = service.Retrieve("account", id, new ColumnSet(true));

            Assert.Equal(id, retrieved.Id);
            Assert.Equal(id, retrieved.GetAttributeValue<Guid>("accountid"));
        }

        [Fact]
        public void Retrieve_WithRetrieveRequest_Works()
        {
            var env = new FakeDataverseEnvironment();
            var service = env.CreateOrganizationService();
            var id = service.Create(new Entity("account") { ["name"] = "TestReq" });

            var resp = (RetrieveResponse)service.Execute(new RetrieveRequest
            {
                Target = new EntityReference("account", id),
                ColumnSet = new ColumnSet("name")
            });

            Assert.Equal("TestReq", resp.Entity.GetAttributeValue<string>("name"));
        }

        [Fact]
        public void Create_MultipleEntities_EachGetsUniqueId()
        {
            var env = new FakeDataverseEnvironment();
            var service = env.CreateOrganizationService();
            var ids = new List<Guid>();
            for (int i = 0; i < 100; i++)
            {
                ids.Add(service.Create(new Entity("account") { ["name"] = $"Acc {i}" }));
            }

            Assert.Equal(100, new System.Collections.Generic.HashSet<Guid>(ids).Count);
        }

        [Fact]
        public void Create_OverriddenCreatedOn_UsesBackdatedTimestamp()
        {
            var env = new FakeDataverseEnvironment();
            var service = env.CreateOrganizationService();
            var backdated = new DateTime(2020, 1, 1, 0, 0, 0, DateTimeKind.Utc);
            var entity = new Entity("account")
            {
                ["name"] = "Backdated",
                ["overriddencreatedon"] = backdated
            };

            var id = service.Create(entity);

            var retrieved = service.Retrieve("account", id, new ColumnSet("createdon", "overriddencreatedon"));
            Assert.Equal(backdated, retrieved.GetAttributeValue<DateTime>("createdon"));
        }

        [Fact]
        public void Create_WithGenericOrganizationRequest_Works()
        {
            var env = new FakeDataverseEnvironment();
            var service = env.CreateOrganizationService();
            var entity = new Entity("contact") { ["firstname"] = "Generic" };
            var request = new OrganizationRequest("Create");
            request["Target"] = entity;

            var response = service.Execute(request);

            var id = (Guid)response["id"];
            Assert.NotEqual(Guid.Empty, id);
            var retrieved = service.Retrieve("contact", id, new ColumnSet("firstname"));
            Assert.Equal("Generic", retrieved.GetAttributeValue<string>("firstname"));
        }

        [Fact]
        public void Create_FormattedValuesPreserved_OnRetrieve()
        {
            var env = new FakeDataverseEnvironment();
            var service = env.CreateOrganizationService();
            var entity = new Entity("account") { ["name"] = "Test" };
            entity.FormattedValues["statecode"] = "Active";
            var id = service.Create(entity);

            var retrieved = service.Retrieve("account", id, new ColumnSet(true));

            Assert.Equal("Active", retrieved.FormattedValues["statecode"]);
        }

        [Fact]
        public void Create_MutatingOriginalMoneyAfterCreate_DoesNotAffectStoredEntity()
        {
            var env = new FakeDataverseEnvironment();
            var service = env.CreateOrganizationService();
            var revenue = new Money(1000m);

            var id = service.Create(new Entity("account")
            {
                ["name"] = "Contoso",
                ["revenue"] = revenue
            });

            revenue.Value = 9999m;

            var retrieved = service.Retrieve("account", id, new ColumnSet("revenue"));
            Assert.Equal(1000m, retrieved.GetAttributeValue<Money>("revenue").Value);
        }

        [Fact]
        public void Update_MutatingOriginalMoneyAfterUpdate_DoesNotAffectStoredEntity()
        {
            var env = new FakeDataverseEnvironment();
            var service = env.CreateOrganizationService();
            var id = service.Create(new Entity("account")
            {
                ["name"] = "Contoso",
                ["revenue"] = new Money(1000m)
            });

            var revenue = new Money(2500m);
            service.Update(new Entity("account", id)
            {
                ["revenue"] = revenue
            });

            revenue.Value = 7777m;

            var retrieved = service.Retrieve("account", id, new ColumnSet("revenue"));
            Assert.Equal(2500m, retrieved.GetAttributeValue<Money>("revenue").Value);
        }
    }
}
