using System;
using System.ServiceModel;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Messages;
using Microsoft.Xrm.Sdk.Query;
using Xunit;

namespace Fake4Dataverse.Tests
{
    public class AlternateKeyTests
    {
        private FakeOrganizationService CreateServiceWithAlternateKey()
        {
            var env = new FakeDataverseEnvironment();
            env.MetadataStore.AddEntity("account")
                .WithAlternateKey("ak_accountnumber", "accountnumber");
            return env.CreateOrganizationService();
        }

        [Fact]
        public void Retrieve_ByAlternateKey_ReturnsEntity()
        {
            var service = CreateServiceWithAlternateKey();
            var id = service.Create(new Entity("account") { ["name"] = "Contoso", ["accountnumber"] = "ACC-001" });

            var target = new EntityReference("account");
            target.KeyAttributes.Add("accountnumber", "ACC-001");

            var response = (RetrieveResponse)service.Execute(new RetrieveRequest
            {
                Target = target,
                ColumnSet = new ColumnSet("name", "accountnumber")
            });

            Assert.Equal(id, response.Entity.Id);
            Assert.Equal("Contoso", response.Entity.GetAttributeValue<string>("name"));
        }

        [Fact]
        public void Retrieve_ByAlternateKey_NotFound_Throws()
        {
            var service = CreateServiceWithAlternateKey();

            var target = new EntityReference("account");
            target.KeyAttributes.Add("accountnumber", "NOPE");

            Assert.Throws<FaultException<OrganizationServiceFault>>(() =>
                service.Execute(new RetrieveRequest
                {
                    Target = target,
                    ColumnSet = new ColumnSet(true)
                }));
        }

        [Fact]
        public void Update_ByAlternateKey_UpdatesEntity()
        {
            var service = CreateServiceWithAlternateKey();
            var id = service.Create(new Entity("account") { ["name"] = "Contoso", ["accountnumber"] = "ACC-001" });

            var updateEntity = new Entity("account");
            updateEntity.KeyAttributes.Add("accountnumber", "ACC-001");
            updateEntity["name"] = "Fabrikam";
            service.Update(updateEntity);

            var retrieved = service.Retrieve("account", id, new ColumnSet("name"));
            Assert.Equal("Fabrikam", retrieved.GetAttributeValue<string>("name"));
        }

        [Fact]
        public void Delete_ByAlternateKey_DeletesEntity()
        {
            var service = CreateServiceWithAlternateKey();
            var id = service.Create(new Entity("account") { ["name"] = "Contoso", ["accountnumber"] = "ACC-001" });

            var target = new EntityReference("account");
            target.KeyAttributes.Add("accountnumber", "ACC-001");

            service.Execute(new DeleteRequest { Target = target });

            Assert.Throws<FaultException<OrganizationServiceFault>>(() =>
                service.Retrieve("account", id, new ColumnSet(true)));
        }

        [Fact]
        public void Upsert_ByAlternateKey_Insert_CreatesNewEntity()
        {
            var service = CreateServiceWithAlternateKey();

            var target = new Entity("account") { ["name"] = "NewCo", ["accountnumber"] = "ACC-NEW" };
            target.KeyAttributes.Add("accountnumber", "ACC-NEW");

            var response = (UpsertResponse)service.Execute(new UpsertRequest { Target = target });

            Assert.True((bool)response.Results["RecordCreated"]);
            var targetRef = (EntityReference)response.Results["Target"];
            var retrieved = service.Retrieve("account", targetRef.Id, new ColumnSet("name", "accountnumber"));
            Assert.Equal("NewCo", retrieved.GetAttributeValue<string>("name"));
        }

        [Fact]
        public void Upsert_ByAlternateKey_Update_UpdatesExistingEntity()
        {
            var service = CreateServiceWithAlternateKey();
            var id = service.Create(new Entity("account") { ["name"] = "Original", ["accountnumber"] = "ACC-001" });

            var target = new Entity("account") { ["name"] = "Updated", ["accountnumber"] = "ACC-001" };
            target.KeyAttributes.Add("accountnumber", "ACC-001");

            var response = (UpsertResponse)service.Execute(new UpsertRequest { Target = target });

            Assert.False((bool)response.Results["RecordCreated"]);
            var retrieved = service.Retrieve("account", id, new ColumnSet("name"));
            Assert.Equal("Updated", retrieved.GetAttributeValue<string>("name"));
        }

        [Fact]
        public void Retrieve_ByCompositeAlternateKey_ReturnsEntity()
        {
            var env = new FakeDataverseEnvironment();
            var service = env.CreateOrganizationService();
            env.MetadataStore.AddEntity("contact")
                .WithAlternateKey("ak_name", "firstname", "lastname");

            var id = service.Create(new Entity("contact")
            {
                ["firstname"] = "John",
                ["lastname"] = "Doe",
                ["emailaddress1"] = "john@example.com"
            });

            var target = new EntityReference("contact");
            target.KeyAttributes.Add("firstname", "John");
            target.KeyAttributes.Add("lastname", "Doe");

            var response = (RetrieveResponse)service.Execute(new RetrieveRequest
            {
                Target = target,
                ColumnSet = new ColumnSet(true)
            });

            Assert.Equal(id, response.Entity.Id);
        }
    }
}
