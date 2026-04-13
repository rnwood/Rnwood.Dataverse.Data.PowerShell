using System;
using System.ServiceModel;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Messages;
using Microsoft.Xrm.Sdk.Query;
using Xunit;

namespace Fake4Dataverse.Tests
{
    public class AssociateDisassociateTests
    {
        [Fact]
        public void Associate_CreatesRelationshipRecord()
        {
            var env = new FakeDataverseEnvironment();
            var service = env.CreateOrganizationService();
            var accountId = service.Create(new Entity("account") { ["name"] = "Contoso" });
            var contactId = service.Create(new Entity("contact") { ["lastname"] = "Doe" });

            service.Associate("account", accountId,
                new Relationship("account_contacts"),
                new EntityReferenceCollection { new EntityReference("contact", contactId) });

            var associations = service.RetrieveMultiple(
                new QueryExpression("association_account_contacts") { ColumnSet = new ColumnSet(true) });
            Assert.Single(associations.Entities);
        }

        [Fact]
        public void Disassociate_RemovesRelationshipRecord()
        {
            var env = new FakeDataverseEnvironment();
            var service = env.CreateOrganizationService();
            var accountId = service.Create(new Entity("account") { ["name"] = "Contoso" });
            var contactId = service.Create(new Entity("contact") { ["lastname"] = "Doe" });

            service.Associate("account", accountId,
                new Relationship("account_contacts"),
                new EntityReferenceCollection { new EntityReference("contact", contactId) });

            service.Disassociate("account", accountId,
                new Relationship("account_contacts"),
                new EntityReferenceCollection { new EntityReference("contact", contactId) });

            var associations = service.RetrieveMultiple(
                new QueryExpression("association_account_contacts") { ColumnSet = new ColumnSet(true) });
            Assert.Empty(associations.Entities);
        }

        [Fact]
        public void Associate_MultipleRelatedEntities_CreatesAll()
        {
            var env = new FakeDataverseEnvironment();
            var service = env.CreateOrganizationService();
            var accountId = service.Create(new Entity("account") { ["name"] = "Contoso" });
            var contact1 = service.Create(new Entity("contact") { ["lastname"] = "Doe" });
            var contact2 = service.Create(new Entity("contact") { ["lastname"] = "Smith" });

            service.Associate("account", accountId,
                new Relationship("account_contacts"),
                new EntityReferenceCollection
                {
                    new EntityReference("contact", contact1),
                    new EntityReference("contact", contact2)
                });

            var associations = service.RetrieveMultiple(
                new QueryExpression("association_account_contacts") { ColumnSet = new ColumnSet(true) });
            Assert.Equal(2, associations.Entities.Count);
        }

        [Fact]
        public void Associate_NullRelationship_ThrowsArgumentNullException()
        {
            var env = new FakeDataverseEnvironment();
            var service = env.CreateOrganizationService();
            var accountId = service.Create(new Entity("account") { ["name"] = "Contoso" });
            var contactId = service.Create(new Entity("contact") { ["lastname"] = "Doe" });

            Assert.Throws<ArgumentNullException>(() =>
                service.Associate("account", accountId,
                    null!,
                    new EntityReferenceCollection { new EntityReference("contact", contactId) }));
        }

        [Fact]
        public void Associate_NullRelatedEntities_ThrowsArgumentNullException()
        {
            var env = new FakeDataverseEnvironment();
            var service = env.CreateOrganizationService();
            var accountId = service.Create(new Entity("account") { ["name"] = "Contoso" });

            Assert.Throws<ArgumentNullException>(() =>
                service.Associate("account", accountId,
                    new Relationship("account_contacts"),
                    null!));
        }

        [Fact]
        public void Associate_SourceAndTargetPreservedCorrectly()
        {
            var env = new FakeDataverseEnvironment();
            var service = env.CreateOrganizationService();
            var accountId = service.Create(new Entity("account") { ["name"] = "Contoso" });
            var contactId = service.Create(new Entity("contact") { ["lastname"] = "Doe" });

            service.Associate("account", accountId,
                new Relationship("account_contacts"),
                new EntityReferenceCollection { new EntityReference("contact", contactId) });

            var associations = service.RetrieveMultiple(
                new QueryExpression("association_account_contacts") { ColumnSet = new ColumnSet(true) });
            var association = associations.Entities[0];

            var source = association.GetAttributeValue<EntityReference>("sourceid");
            var target = association.GetAttributeValue<EntityReference>("targetid");

            Assert.Equal(accountId, source.Id);
            Assert.Equal("account", source.LogicalName);
            Assert.Equal(contactId, target.Id);
            Assert.Equal("contact", target.LogicalName);
        }

        [Fact]
        public void Disassociate_NonExistentAssociation_DoesNotThrow()
        {
            var env = new FakeDataverseEnvironment();
            var service = env.CreateOrganizationService();
            var accountId = service.Create(new Entity("account") { ["name"] = "Contoso" });
            var contactId = service.Create(new Entity("contact") { ["lastname"] = "Doe" });

            // Disassociate without prior associate should not throw
            service.Disassociate("account", accountId,
                new Relationship("account_contacts"),
                new EntityReferenceCollection { new EntityReference("contact", contactId) });
        }

        [Fact]
        public void Disassociate_OnlyRemovesSpecifiedTarget()
        {
            var env = new FakeDataverseEnvironment();
            var service = env.CreateOrganizationService();
            var accountId = service.Create(new Entity("account") { ["name"] = "Contoso" });
            var contact1 = service.Create(new Entity("contact") { ["lastname"] = "Doe" });
            var contact2 = service.Create(new Entity("contact") { ["lastname"] = "Smith" });

            service.Associate("account", accountId,
                new Relationship("account_contacts"),
                new EntityReferenceCollection
                {
                    new EntityReference("contact", contact1),
                    new EntityReference("contact", contact2)
                });

            service.Disassociate("account", accountId,
                new Relationship("account_contacts"),
                new EntityReferenceCollection { new EntityReference("contact", contact1) });

            var associations = service.RetrieveMultiple(
                new QueryExpression("association_account_contacts") { ColumnSet = new ColumnSet(true) });
            Assert.Single(associations.Entities);
            Assert.Equal(contact2, associations.Entities[0].GetAttributeValue<EntityReference>("targetid").Id);
        }

        [Fact]
        public void Associate_NonExistingSource_ThrowsFaultException()
        {
            var env = new FakeDataverseEnvironment();
            var service = env.CreateOrganizationService();
            var contactId = service.Create(new Entity("contact") { ["lastname"] = "Smith" });

            var ex = Assert.Throws<FaultException<OrganizationServiceFault>>(() =>
                service.Associate("account", Guid.NewGuid(),
                    new Relationship("account_contacts"),
                    new EntityReferenceCollection { new EntityReference("contact", contactId) }));
            Assert.Equal(DataverseFault.ObjectDoesNotExist, ex.Detail.ErrorCode);
        }

        [Fact]
        public void Associate_NonExistingTarget_ThrowsFaultException()
        {
            var env = new FakeDataverseEnvironment();
            var service = env.CreateOrganizationService();
            var accountId = service.Create(new Entity("account") { ["name"] = "Contoso" });

            var ex = Assert.Throws<FaultException<OrganizationServiceFault>>(() =>
                service.Associate("account", accountId,
                    new Relationship("account_contacts"),
                    new EntityReferenceCollection { new EntityReference("contact", Guid.NewGuid()) }));
            Assert.Equal(DataverseFault.ObjectDoesNotExist, ex.Detail.ErrorCode);
        }

        [Fact]
        public void Associate_DuplicateAssociation_ThrowsFaultException()
        {
            var env = new FakeDataverseEnvironment();
            var service = env.CreateOrganizationService();
            var accountId = service.Create(new Entity("account") { ["name"] = "Contoso" });
            var contactId = service.Create(new Entity("contact") { ["lastname"] = "Smith" });

            service.Associate("account", accountId,
                new Relationship("account_contacts"),
                new EntityReferenceCollection { new EntityReference("contact", contactId) });

            var ex = Assert.Throws<FaultException<OrganizationServiceFault>>(() =>
                service.Associate("account", accountId,
                    new Relationship("account_contacts"),
                    new EntityReferenceCollection { new EntityReference("contact", contactId) }));
            Assert.Equal(DataverseFault.DuplicateRecord, ex.Detail.ErrorCode);
        }

        [Fact]
        public void Associate_NToN_TwoWayRetrieval()
        {
            var env = new FakeDataverseEnvironment();
            var service = env.CreateOrganizationService();
            var accountId = service.Create(new Entity("account") { ["name"] = "Contoso" });
            var contactId = service.Create(new Entity("contact") { ["lastname"] = "Smith" });

            service.Associate("account", accountId,
                new Relationship("account_contacts"),
                new EntityReferenceCollection { new EntityReference("contact", contactId) });

            // Query from account side
            var assocQuery = new QueryExpression("association_account_contacts") { ColumnSet = new ColumnSet(true) };
            assocQuery.Criteria.AddCondition("sourceid", ConditionOperator.Equal, accountId);
            var fromAccount = service.RetrieveMultiple(assocQuery);
            Assert.Single(fromAccount.Entities);

            // Query from contact side
            var reverseQuery = new QueryExpression("association_account_contacts") { ColumnSet = new ColumnSet(true) };
            reverseQuery.Criteria.AddCondition("targetid", ConditionOperator.Equal, contactId);
            var fromContact = service.RetrieveMultiple(reverseQuery);
            Assert.Single(fromContact.Entities);
        }

        [Fact]
        public void Disassociate_NToN_RemovesAssociationRecord()
        {
            var env = new FakeDataverseEnvironment();
            var service = env.CreateOrganizationService();
            var accountId = service.Create(new Entity("account") { ["name"] = "Contoso" });
            var contactId = service.Create(new Entity("contact") { ["lastname"] = "Smith" });

            service.Associate("account", accountId,
                new Relationship("account_contacts"),
                new EntityReferenceCollection { new EntityReference("contact", contactId) });

            service.Disassociate("account", accountId,
                new Relationship("account_contacts"),
                new EntityReferenceCollection { new EntityReference("contact", contactId) });

            var assocQuery = new QueryExpression("association_account_contacts") { ColumnSet = new ColumnSet(true) };
            var result = service.RetrieveMultiple(assocQuery);
            Assert.Empty(result.Entities);
        }

        [Fact]
        public void Associate_N1_SetsLookupField()
        {
            var env = new FakeDataverseEnvironment();
            var service = env.CreateOrganizationService();
            var accountId = service.Create(new Entity("account") { ["name"] = "Contoso" });
            var contactId = service.Create(new Entity("contact") { ["lastname"] = "Smith" });

            service.Associate("contact", contactId,
                new Relationship("contact_account") { PrimaryEntityRole = EntityRole.Referencing },
                new EntityReferenceCollection { new EntityReference("account", accountId) });

            var assocQuery = new QueryExpression("association_contact_account") { ColumnSet = new ColumnSet(true) };
            var result = service.RetrieveMultiple(assocQuery);
            Assert.Single(result.Entities);
        }

        [Fact]
        public void AssociateRequest_CreatesRelationshipRecord()
        {
            var env = new FakeDataverseEnvironment();
            var service = env.CreateOrganizationService();
            var accountId = service.Create(new Entity("account") { ["name"] = "Contoso" });
            var contactId = service.Create(new Entity("contact") { ["lastname"] = "Doe" });

            service.Execute(new AssociateRequest
            {
                Target = new EntityReference("account", accountId),
                Relationship = new Relationship("account_contacts"),
                RelatedEntities = new EntityReferenceCollection { new EntityReference("contact", contactId) }
            });

            var associations = service.RetrieveMultiple(
                new QueryExpression("association_account_contacts") { ColumnSet = new ColumnSet(true) });
            Assert.Single(associations.Entities);
        }

        [Fact]
        public void DisassociateRequest_RemovesRelationshipRecord()
        {
            var env = new FakeDataverseEnvironment();
            var service = env.CreateOrganizationService();
            var accountId = service.Create(new Entity("account") { ["name"] = "Contoso" });
            var contactId = service.Create(new Entity("contact") { ["lastname"] = "Doe" });

            service.Execute(new AssociateRequest
            {
                Target = new EntityReference("account", accountId),
                Relationship = new Relationship("account_contacts"),
                RelatedEntities = new EntityReferenceCollection { new EntityReference("contact", contactId) }
            });

            service.Execute(new DisassociateRequest
            {
                Target = new EntityReference("account", accountId),
                Relationship = new Relationship("account_contacts"),
                RelatedEntities = new EntityReferenceCollection { new EntityReference("contact", contactId) }
            });

            var associations = service.RetrieveMultiple(
                new QueryExpression("association_account_contacts") { ColumnSet = new ColumnSet(true) });
            Assert.Empty(associations.Entities);
        }
    }
}
