using System;
using System.ServiceModel;
using Microsoft.Crm.Sdk.Messages;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Messages;
using Microsoft.Xrm.Sdk.Query;
using Xunit;

namespace Fake4Dataverse.Tests
{
    public class StateAndOwnershipRequestTests
    {
        [Fact]
        public void SetStateRequest_UpdatesStatecodeAndStatuscode()
        {
            var env = new FakeDataverseEnvironment();
            var service = env.CreateOrganizationService();
            var id = service.Create(new Entity("account") { ["name"] = "Contoso" });

            service.Execute(new SetStateRequest
            {
                EntityMoniker = new EntityReference("account", id),
                State = new OptionSetValue(1),
                Status = new OptionSetValue(2)
            });

            var retrieved = service.Retrieve("account", id, new ColumnSet("statecode", "statuscode"));
            Assert.Equal(1, retrieved.GetAttributeValue<OptionSetValue>("statecode").Value);
            Assert.Equal(2, retrieved.GetAttributeValue<OptionSetValue>("statuscode").Value);
        }

        [Fact]
        public void AssignRequest_UpdatesOwnerid()
        {
            var env = new FakeDataverseEnvironment();
            var service = env.CreateOrganizationService();
            var id = service.Create(new Entity("account") { ["name"] = "Contoso" });
            var newOwner = new EntityReference("systemuser", Guid.NewGuid());

            service.Execute(new AssignRequest
            {
                Target = new EntityReference("account", id),
                Assignee = newOwner
            });

            var retrieved = service.Retrieve("account", id, new ColumnSet("ownerid"));
            var ownerid = retrieved.GetAttributeValue<EntityReference>("ownerid");
            Assert.Equal(newOwner.Id, ownerid.Id);
        }

        [Fact]
        public void Create_AutoSetsDefaultStateAndStatus()
        {
            var env = new FakeDataverseEnvironment();
            var service = env.CreateOrganizationService();
            var id = service.Create(new Entity("account") { ["name"] = "Test" });

            var retrieved = service.Retrieve("account", id, new ColumnSet("statecode", "statuscode"));
            Assert.Equal(0, retrieved.GetAttributeValue<OptionSetValue>("statecode").Value);
            Assert.Equal(1, retrieved.GetAttributeValue<OptionSetValue>("statuscode").Value);
        }

        [Fact]
        public void Update_CanSetStatecodeDirectly()
        {
            var env = new FakeDataverseEnvironment();
            var service = env.CreateOrganizationService();
            var id = service.Create(new Entity("account") { ["name"] = "Test" });

            service.Update(new Entity("account", id)
            {
                ["statecode"] = new OptionSetValue(1),
                ["statuscode"] = new OptionSetValue(2)
            });

            var retrieved = service.Retrieve("account", id, new ColumnSet("statecode", "statuscode"));
            Assert.Equal(1, retrieved.GetAttributeValue<OptionSetValue>("statecode").Value);
            Assert.Equal(2, retrieved.GetAttributeValue<OptionSetValue>("statuscode").Value);
        }

        [Fact]
        public void Create_WithExplicitState_RespectsProvidedValues()
        {
            var env = new FakeDataverseEnvironment();
            var service = env.CreateOrganizationService();
            var id = service.Create(new Entity("account")
            {
                ["name"] = "Inactive",
                ["statecode"] = new OptionSetValue(1),
                ["statuscode"] = new OptionSetValue(2)
            });

            var retrieved = service.Retrieve("account", id, new ColumnSet("statecode", "statuscode"));
            Assert.Equal(1, retrieved.GetAttributeValue<OptionSetValue>("statecode").Value);
            Assert.Equal(2, retrieved.GetAttributeValue<OptionSetValue>("statuscode").Value);
        }

        [Fact]
        public void SetState_WithRegisteredTransition_AllowsValidTransition()
        {
            var env = new FakeDataverseEnvironment();
            var service = env.CreateOrganizationService();
            env.RegisterStatusTransition("incident", 0, 1, 1, 5);
            var id = service.Create(new Entity("incident") { ["title"] = "Test Case" });

            service.Execute(new SetStateRequest
            {
                EntityMoniker = new EntityReference("incident", id),
                State = new OptionSetValue(1),
                Status = new OptionSetValue(5)
            });

            var retrieved = service.Retrieve("incident", id, new ColumnSet("statecode", "statuscode"));
            Assert.Equal(1, retrieved.GetAttributeValue<OptionSetValue>("statecode").Value);
            Assert.Equal(5, retrieved.GetAttributeValue<OptionSetValue>("statuscode").Value);
        }

        [Fact]
        public void SetState_WithRegisteredTransition_RejectsInvalidTransition()
        {
            var env = new FakeDataverseEnvironment();
            var service = env.CreateOrganizationService();
            env.RegisterStatusTransition("incident", 0, 1, 1, 5);
            var id = service.Create(new Entity("incident") { ["title"] = "Test Case" });

            Assert.Throws<FaultException<OrganizationServiceFault>>(() =>
                service.Execute(new SetStateRequest
                {
                    EntityMoniker = new EntityReference("incident", id),
                    State = new OptionSetValue(2),
                    Status = new OptionSetValue(6)
                }));
        }

        [Fact]
        public void IsValidStateTransition_ReturnsTrue_ForValidTransition()
        {
            var env = new FakeDataverseEnvironment();
            var service = env.CreateOrganizationService();
            env.RegisterStatusTransition("incident", 0, 1, 1, 5);
            var id = service.Create(new Entity("incident") { ["title"] = "Test" });

            var request = new OrganizationRequest("IsValidStateTransition");
            request["Entity"] = new EntityReference("incident", id);
            request["NewState"] = "1";
            request["NewStatus"] = 5;

            var response = service.Execute(request);
            Assert.True((bool)response["IsValid"]);
        }

        [Fact]
        public void IsValidStateTransition_ReturnsFalse_ForInvalidTransition()
        {
            var env = new FakeDataverseEnvironment();
            var service = env.CreateOrganizationService();
            env.RegisterStatusTransition("incident", 0, 1, 1, 5);
            var id = service.Create(new Entity("incident") { ["title"] = "Test" });

            var request = new OrganizationRequest("IsValidStateTransition");
            request["Entity"] = new EntityReference("incident", id);
            request["NewState"] = "2";
            request["NewStatus"] = 6;

            var response = service.Execute(request);
            Assert.False((bool)response["IsValid"]);
        }

        [Fact]
        public void Create_WithCustomDefaultStatusCode_UsesRegisteredDefaults()
        {
            var env = new FakeDataverseEnvironment();
            var service = env.CreateOrganizationService();
            env.RegisterDefaultStatusCode("opportunity", 0, 2);

            var id = service.Create(new Entity("opportunity") { ["name"] = "Big Deal" });

            var retrieved = service.Retrieve("opportunity", id, new ColumnSet("statecode", "statuscode"));
            Assert.Equal(0, retrieved.GetAttributeValue<OptionSetValue>("statecode").Value);
            Assert.Equal(2, retrieved.GetAttributeValue<OptionSetValue>("statuscode").Value);
        }
    }
}
