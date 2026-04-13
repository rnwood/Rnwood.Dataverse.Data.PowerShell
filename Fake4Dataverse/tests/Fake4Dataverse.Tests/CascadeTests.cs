using System;
using System.ServiceModel;
using Fake4Dataverse.Metadata;
using Microsoft.Crm.Sdk.Messages;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Query;
using Xunit;

namespace Fake4Dataverse.Tests
{
    public class CascadeTests
    {
        private FakeOrganizationService CreateServiceWithRelationship(CascadeConfiguration cascade)
        {
            var env = new FakeDataverseEnvironment();
            env.MetadataStore.AddOneToManyRelationship(
                "account_contacts",
                "account", "accountid",
                "contact", "parentcustomerid",
                cascade);
            return env.CreateOrganizationService();
        }

        [Fact]
        public void CascadeDelete_Cascade_DeletesChildren()
        {
            var service = CreateServiceWithRelationship(new CascadeConfiguration { Delete = CascadeType.Cascade });

            var accountId = service.Create(new Entity("account") { ["name"] = "Parent" });
            var contactId = service.Create(new Entity("contact")
            {
                ["fullname"] = "Child",
                ["parentcustomerid"] = new EntityReference("account", accountId)
            });

            service.Delete("account", accountId);

            var ex = Assert.Throws<FaultException<OrganizationServiceFault>>(
                () => service.Retrieve("contact", contactId, new ColumnSet(true)));
            Assert.Equal(DataverseFault.ObjectDoesNotExist, ex.Detail.ErrorCode);
        }

        [Fact]
        public void CascadeDelete_RemoveLink_ClearsForeignKey()
        {
            var service = CreateServiceWithRelationship(new CascadeConfiguration { Delete = CascadeType.RemoveLink });

            var accountId = service.Create(new Entity("account") { ["name"] = "Parent" });
            var contactId = service.Create(new Entity("contact")
            {
                ["fullname"] = "Child",
                ["parentcustomerid"] = new EntityReference("account", accountId)
            });

            service.Delete("account", accountId);

            var contact = service.Retrieve("contact", contactId, new ColumnSet("parentcustomerid"));
            Assert.Null(contact.GetAttributeValue<EntityReference>("parentcustomerid"));
        }

        [Fact]
        public void CascadeDelete_Restrict_ThrowsWhenChildrenExist()
        {
            var service = CreateServiceWithRelationship(new CascadeConfiguration { Delete = CascadeType.Restrict });

            var accountId = service.Create(new Entity("account") { ["name"] = "Parent" });
            service.Create(new Entity("contact")
            {
                ["fullname"] = "Child",
                ["parentcustomerid"] = new EntityReference("account", accountId)
            });

            Assert.Throws<FaultException<OrganizationServiceFault>>(
                () => service.Delete("account", accountId));
        }

        [Fact]
        public void CascadeDelete_Restrict_AllowsDeleteWhenNoChildren()
        {
            var service = CreateServiceWithRelationship(new CascadeConfiguration { Delete = CascadeType.Restrict });

            var accountId = service.Create(new Entity("account") { ["name"] = "Parent" });

            // Should not throw — no children
            service.Delete("account", accountId);
        }

        [Fact]
        public void CascadeDelete_NoCascade_LeavesChildrenIntact()
        {
            var service = CreateServiceWithRelationship(new CascadeConfiguration { Delete = CascadeType.NoCascade });

            var accountId = service.Create(new Entity("account") { ["name"] = "Parent" });
            var contactId = service.Create(new Entity("contact")
            {
                ["fullname"] = "Child",
                ["parentcustomerid"] = new EntityReference("account", accountId)
            });

            service.Delete("account", accountId);

            // Child should still exist with the (now dangling) FK
            var contact = service.Retrieve("contact", contactId, new ColumnSet(true));
            Assert.Equal("Child", contact.GetAttributeValue<string>("fullname"));
        }

        [Fact]
        public void CascadeAssign_Cascade_UpdatesChildOwner()
        {
            var service = CreateServiceWithRelationship(new CascadeConfiguration { Assign = CascadeType.Cascade });

            var accountId = service.Create(new Entity("account") { ["name"] = "Parent" });
            var contactId = service.Create(new Entity("contact")
            {
                ["fullname"] = "Child",
                ["parentcustomerid"] = new EntityReference("account", accountId)
            });

            var newOwner = new EntityReference("systemuser", Guid.NewGuid());
            service.Execute(new AssignRequest
            {
                Target = new EntityReference("account", accountId),
                Assignee = newOwner
            });

            var contact = service.Retrieve("contact", contactId, new ColumnSet("ownerid"));
            Assert.Equal(newOwner.Id, contact.GetAttributeValue<EntityReference>("ownerid").Id);
        }

        [Fact]
        public void CascadeAssign_NoCascade_DoesNotUpdateChildOwner()
        {
            var service = CreateServiceWithRelationship(new CascadeConfiguration { Assign = CascadeType.NoCascade });

            var accountId = service.Create(new Entity("account") { ["name"] = "Parent" });
            var contactId = service.Create(new Entity("contact")
            {
                ["fullname"] = "Child",
                ["parentcustomerid"] = new EntityReference("account", accountId)
            });

            var originalOwner = service.Retrieve("contact", contactId, new ColumnSet("ownerid"))
                .GetAttributeValue<EntityReference>("ownerid");

            var newOwner = new EntityReference("systemuser", Guid.NewGuid());
            service.Execute(new AssignRequest
            {
                Target = new EntityReference("account", accountId),
                Assignee = newOwner
            });

            var contact = service.Retrieve("contact", contactId, new ColumnSet("ownerid"));
            Assert.Equal(originalOwner.Id, contact.GetAttributeValue<EntityReference>("ownerid").Id);
        }

        [Fact]
        public void CascadeDelete_Cascade_MultipleChildren_AllDeleted()
        {
            var service = CreateServiceWithRelationship(new CascadeConfiguration { Delete = CascadeType.Cascade });

            var accountId = service.Create(new Entity("account") { ["name"] = "Parent" });
            var contact1 = service.Create(new Entity("contact")
            {
                ["fullname"] = "Child1",
                ["parentcustomerid"] = new EntityReference("account", accountId)
            });
            var contact2 = service.Create(new Entity("contact")
            {
                ["fullname"] = "Child2",
                ["parentcustomerid"] = new EntityReference("account", accountId)
            });

            service.Delete("account", accountId);

            Assert.Throws<FaultException<OrganizationServiceFault>>(
                () => service.Retrieve("contact", contact1, new ColumnSet(true)));
            Assert.Throws<FaultException<OrganizationServiceFault>>(
                () => service.Retrieve("contact", contact2, new ColumnSet(true)));
        }

        [Fact]
        public void CascadeDelete_Cascade_MultiLevel_DeletesGrandchildren()
        {
            var env = new FakeDataverseEnvironment();
            var service = env.CreateOrganizationService();
            // Set up two cascade relationships: account → contact → task
            env.MetadataStore.AddOneToManyRelationship(
                "account_contacts", "account", "accountid",
                "contact", "parentcustomerid",
                new CascadeConfiguration { Delete = CascadeType.Cascade });
            env.MetadataStore.AddOneToManyRelationship(
                "contact_tasks", "contact", "contactid",
                "task", "regardingobjectid",
                new CascadeConfiguration { Delete = CascadeType.Cascade });

            var accountId = service.Create(new Entity("account") { ["name"] = "Grandparent" });
            var contactId = service.Create(new Entity("contact")
            {
                ["fullname"] = "Parent",
                ["parentcustomerid"] = new EntityReference("account", accountId)
            });
            var taskId = service.Create(new Entity("task")
            {
                ["subject"] = "Grandchild",
                ["regardingobjectid"] = new EntityReference("contact", contactId)
            });

            // Deleting grandparent should cascade through to grandchild
            service.Delete("account", accountId);

            Assert.Throws<FaultException<OrganizationServiceFault>>(
                () => service.Retrieve("contact", contactId, new ColumnSet(true)));
            Assert.Throws<FaultException<OrganizationServiceFault>>(
                () => service.Retrieve("task", taskId, new ColumnSet(true)));
        }
    }
}
