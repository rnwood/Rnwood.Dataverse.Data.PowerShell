using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Query;
using Xunit;

namespace Fake4Dataverse.Tests
{
    public class ActivityPartyTests
    {
        [Fact]
        public void Create_EmailWithActivityParties_StoresParties()
        {
            var env = new FakeDataverseEnvironment();
            var service = env.CreateOrganizationService();

            var from = new Entity("activityparty")
            {
                ["partyid"] = new EntityReference("systemuser", Guid.NewGuid()),
                ["participationtypemask"] = new OptionSetValue(1) // From
            };
            var to = new Entity("activityparty")
            {
                ["partyid"] = new EntityReference("contact", Guid.NewGuid()),
                ["participationtypemask"] = new OptionSetValue(2) // To
            };

            var email = new Entity("email")
            {
                ["subject"] = "Test Email",
                ["from"] = new EntityCollection(new List<Entity> { from }),
                ["to"] = new EntityCollection(new List<Entity> { to })
            };

            var id = service.Create(email);
            var result = service.Retrieve("email", id, new ColumnSet(true));

            Assert.Equal("Test Email", result.GetAttributeValue<string>("subject"));

            var fromParties = result.GetAttributeValue<EntityCollection>("from");
            Assert.NotNull(fromParties);
            Assert.Single(fromParties.Entities);

            var toParties = result.GetAttributeValue<EntityCollection>("to");
            Assert.NotNull(toParties);
            Assert.Single(toParties.Entities);
        }

        [Fact]
        public void Create_EmailWithMultipleToParties()
        {
            var env = new FakeDataverseEnvironment();
            var service = env.CreateOrganizationService();

            var to1 = new Entity("activityparty")
            {
                ["partyid"] = new EntityReference("contact", Guid.NewGuid()),
                ["participationtypemask"] = new OptionSetValue(2)
            };
            var to2 = new Entity("activityparty")
            {
                ["partyid"] = new EntityReference("contact", Guid.NewGuid()),
                ["participationtypemask"] = new OptionSetValue(2)
            };

            var email = new Entity("email")
            {
                ["subject"] = "Multi-recipient",
                ["to"] = new EntityCollection(new List<Entity> { to1, to2 })
            };

            var id = service.Create(email);
            var result = service.Retrieve("email", id, new ColumnSet(true));

            var toParties = result.GetAttributeValue<EntityCollection>("to");
            Assert.NotNull(toParties);
            Assert.Equal(2, toParties.Entities.Count);
        }

        [Fact]
        public void Create_EmailWithCcBcc()
        {
            var env = new FakeDataverseEnvironment();
            var service = env.CreateOrganizationService();

            var cc = new Entity("activityparty")
            {
                ["partyid"] = new EntityReference("contact", Guid.NewGuid()),
                ["participationtypemask"] = new OptionSetValue(3) // CC
            };
            var bcc = new Entity("activityparty")
            {
                ["partyid"] = new EntityReference("contact", Guid.NewGuid()),
                ["participationtypemask"] = new OptionSetValue(4) // BCC
            };

            var email = new Entity("email")
            {
                ["subject"] = "CC/BCC Test",
                ["cc"] = new EntityCollection(new List<Entity> { cc }),
                ["bcc"] = new EntityCollection(new List<Entity> { bcc })
            };

            var id = service.Create(email);
            var result = service.Retrieve("email", id, new ColumnSet(true));

            var ccParties = result.GetAttributeValue<EntityCollection>("cc");
            Assert.NotNull(ccParties);
            Assert.Single(ccParties.Entities);

            var bccParties = result.GetAttributeValue<EntityCollection>("bcc");
            Assert.NotNull(bccParties);
            Assert.Single(bccParties.Entities);
        }

        [Fact]
        public void Create_PhoneCallWithParties()
        {
            var env = new FakeDataverseEnvironment();
            var service = env.CreateOrganizationService();

            var from = new Entity("activityparty")
            {
                ["partyid"] = new EntityReference("systemuser", Guid.NewGuid()),
                ["participationtypemask"] = new OptionSetValue(1)
            };
            var to = new Entity("activityparty")
            {
                ["partyid"] = new EntityReference("contact", Guid.NewGuid()),
                ["participationtypemask"] = new OptionSetValue(2)
            };

            var phonecall = new Entity("phonecall")
            {
                ["subject"] = "Follow-up call",
                ["from"] = new EntityCollection(new List<Entity> { from }),
                ["to"] = new EntityCollection(new List<Entity> { to })
            };

            var id = service.Create(phonecall);
            var result = service.Retrieve("phonecall", id, new ColumnSet(true));

            Assert.Equal("Follow-up call", result.GetAttributeValue<string>("subject"));
            Assert.NotNull(result.GetAttributeValue<EntityCollection>("from"));
            Assert.NotNull(result.GetAttributeValue<EntityCollection>("to"));
        }

        [Fact]
        public void ActivityPartyHasPartyId()
        {
            var env = new FakeDataverseEnvironment();
            var service = env.CreateOrganizationService();

            var contactId = Guid.NewGuid();
            var to = new Entity("activityparty")
            {
                ["partyid"] = new EntityReference("contact", contactId),
                ["participationtypemask"] = new OptionSetValue(2)
            };

            var email = new Entity("email")
            {
                ["subject"] = "Party ID test",
                ["to"] = new EntityCollection(new List<Entity> { to })
            };

            var id = service.Create(email);
            var result = service.Retrieve("email", id, new ColumnSet(true));

            var toParties = result.GetAttributeValue<EntityCollection>("to");
            Assert.NotNull(toParties);
            var party = toParties.Entities.First();
            var partyRef = party.GetAttributeValue<EntityReference>("partyid");
            Assert.NotNull(partyRef);
            Assert.Equal(contactId, partyRef.Id);
            Assert.Equal("contact", partyRef.LogicalName);
        }

        [Fact]
        public void RetrieveMultiple_WithActivityParties()
        {
            var env = new FakeDataverseEnvironment();
            var service = env.CreateOrganizationService();

            var to = new Entity("activityparty")
            {
                ["partyid"] = new EntityReference("contact", Guid.NewGuid()),
                ["participationtypemask"] = new OptionSetValue(2)
            };

            service.Create(new Entity("email")
            {
                ["subject"] = "Email 1",
                ["to"] = new EntityCollection(new List<Entity> { to })
            });

            var query = new QueryExpression("email") { ColumnSet = new ColumnSet(true) };
            var results = service.RetrieveMultiple(query);

            Assert.Single(results.Entities);
            var email = results.Entities[0];
            Assert.NotNull(email.GetAttributeValue<EntityCollection>("to"));
        }
    }
}
