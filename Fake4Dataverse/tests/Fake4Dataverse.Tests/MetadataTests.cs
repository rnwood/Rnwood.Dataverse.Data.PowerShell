using System;
using System.Linq;
using System.ServiceModel;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Messages;
using Microsoft.Xrm.Sdk.Metadata;
using Xunit;

namespace Fake4Dataverse.Tests
{
    public class MetadataTests
    {
        #region Metadata Store Configuration

        [Fact]
        public void AddEntity_StoresEntityMetadata()
        {
            var env = new FakeDataverseEnvironment();
            var service = env.CreateOrganizationService();
            env.MetadataStore.AddEntity("account")
                .WithSchemaName("Account")
                .WithPrimaryIdAttribute("accountid")
                .WithPrimaryNameAttribute("name")
                .WithObjectTypeCode(1);

            var response = (RetrieveEntityResponse)service.Execute(
                new RetrieveEntityRequest { LogicalName = "account" });

            Assert.Equal("account", response.EntityMetadata.LogicalName);
            Assert.Equal("Account", response.EntityMetadata.SchemaName);
        }

        [Fact]
        public void AddEntity_WithAttributes_StoresAttributes()
        {
            var env = new FakeDataverseEnvironment();
            var service = env.CreateOrganizationService();
            env.MetadataStore.AddEntity("account")
                .WithStringAttribute("name", maxLength: 200)
                .WithIntegerAttribute("numberofemployees", minValue: 0, maxValue: 1000000);

            var response = (RetrieveEntityResponse)service.Execute(
                new RetrieveEntityRequest { LogicalName = "account" });

            Assert.Equal(2, response.EntityMetadata.Attributes.Length);
        }

        #endregion

        #region Required Field Validation

        [Fact]
        public void Create_MissingRequiredField_ThrowsFault()
        {
            var env = new FakeDataverseEnvironment(new FakeOrganizationServiceOptions { ValidateWithMetadata = true });
            var service = env.CreateOrganizationService();
            env.MetadataStore.AddEntity("account")
                .WithStringAttribute("name", requiredLevel: AttributeRequiredLevel.ApplicationRequired);

            var entity = new Entity("account");

            var ex = Assert.Throws<FaultException<OrganizationServiceFault>>(
                () => service.Create(entity));
            Assert.Contains("name", ex.Detail.Message);
        }

        [Fact]
        public void Create_RequiredFieldPresent_Succeeds()
        {
            var env = new FakeDataverseEnvironment(new FakeOrganizationServiceOptions { ValidateWithMetadata = true });
            var service = env.CreateOrganizationService();
            env.MetadataStore.AddEntity("account")
                .WithStringAttribute("name", requiredLevel: AttributeRequiredLevel.ApplicationRequired);

            var entity = new Entity("account") { ["name"] = "Contoso" };

            var id = service.Create(entity);
            Assert.NotEqual(Guid.Empty, id);
        }

        [Fact]
        public void Create_SystemRequiredFieldMissing_ThrowsFault()
        {
            var env = new FakeDataverseEnvironment(new FakeOrganizationServiceOptions { ValidateWithMetadata = true });
            var service = env.CreateOrganizationService();
            env.MetadataStore.AddEntity("contact")
                .WithStringAttribute("lastname", requiredLevel: AttributeRequiredLevel.SystemRequired);

            var entity = new Entity("contact") { ["firstname"] = "John" };

            var ex = Assert.Throws<FaultException<OrganizationServiceFault>>(
                () => service.Create(entity));
            Assert.Contains("lastname", ex.Detail.Message);
        }

        [Fact]
        public void Update_SetRequiredFieldToNull_ThrowsFault()
        {
            var env = new FakeDataverseEnvironment(new FakeOrganizationServiceOptions { ValidateWithMetadata = true });
            var service = env.CreateOrganizationService();
            env.MetadataStore.AddEntity("account")
                .WithStringAttribute("name", requiredLevel: AttributeRequiredLevel.ApplicationRequired);

            var id = service.Create(new Entity("account") { ["name"] = "Contoso" });

            var update = new Entity("account", id) { ["name"] = null };

            Assert.Throws<FaultException<OrganizationServiceFault>>(
                () => service.Update(update));
        }

        #endregion

        #region String Max Length Validation

        [Fact]
        public void Create_StringExceedsMaxLength_ThrowsFault()
        {
            var env = new FakeDataverseEnvironment(new FakeOrganizationServiceOptions { ValidateWithMetadata = true });
            var service = env.CreateOrganizationService();
            env.MetadataStore.AddEntity("account")
                .WithStringAttribute("name", maxLength: 5);

            var entity = new Entity("account") { ["name"] = "TooLongName" };

            var ex = Assert.Throws<FaultException<OrganizationServiceFault>>(
                () => service.Create(entity));
            Assert.Contains("maximum length", ex.Detail.Message);
        }

        [Fact]
        public void Create_StringWithinMaxLength_Succeeds()
        {
            var env = new FakeDataverseEnvironment(new FakeOrganizationServiceOptions { ValidateWithMetadata = true });
            var service = env.CreateOrganizationService();
            env.MetadataStore.AddEntity("account")
                .WithStringAttribute("name", maxLength: 100);

            var entity = new Entity("account") { ["name"] = "Contoso" };

            var id = service.Create(entity);
            Assert.NotEqual(Guid.Empty, id);
        }

        [Fact]
        public void Update_StringExceedsMaxLength_ThrowsFault()
        {
            var env = new FakeDataverseEnvironment(new FakeOrganizationServiceOptions { ValidateWithMetadata = true });
            var service = env.CreateOrganizationService();
            env.MetadataStore.AddEntity("account")
                .WithStringAttribute("name", maxLength: 5);

            var id = service.Create(new Entity("account") { ["name"] = "OK" });

            var update = new Entity("account", id) { ["name"] = "TooLongName" };

            Assert.Throws<FaultException<OrganizationServiceFault>>(
                () => service.Update(update));
        }

        #endregion

        #region Numeric Min/Max Validation

        [Fact]
        public void Create_IntegerBelowMin_ThrowsFault()
        {
            var env = new FakeDataverseEnvironment(new FakeOrganizationServiceOptions { ValidateWithMetadata = true });
            var service = env.CreateOrganizationService();
            env.MetadataStore.AddEntity("account")
                .WithIntegerAttribute("numberofemployees", minValue: 0, maxValue: 1000);

            var entity = new Entity("account") { ["numberofemployees"] = -1 };

            var ex = Assert.Throws<FaultException<OrganizationServiceFault>>(
                () => service.Create(entity));
            Assert.Contains("below minimum", ex.Detail.Message);
        }

        [Fact]
        public void Create_IntegerAboveMax_ThrowsFault()
        {
            var env = new FakeDataverseEnvironment(new FakeOrganizationServiceOptions { ValidateWithMetadata = true });
            var service = env.CreateOrganizationService();
            env.MetadataStore.AddEntity("account")
                .WithIntegerAttribute("numberofemployees", minValue: 0, maxValue: 1000);

            var entity = new Entity("account") { ["numberofemployees"] = 2000 };

            var ex = Assert.Throws<FaultException<OrganizationServiceFault>>(
                () => service.Create(entity));
            Assert.Contains("above maximum", ex.Detail.Message);
        }

        [Fact]
        public void Create_IntegerInRange_Succeeds()
        {
            var env = new FakeDataverseEnvironment(new FakeOrganizationServiceOptions { ValidateWithMetadata = true });
            var service = env.CreateOrganizationService();
            env.MetadataStore.AddEntity("account")
                .WithIntegerAttribute("numberofemployees", minValue: 0, maxValue: 1000);

            var entity = new Entity("account") { ["numberofemployees"] = 500 };

            var id = service.Create(entity);
            Assert.NotEqual(Guid.Empty, id);
        }

        [Fact]
        public void Create_MoneyBelowMin_ThrowsFault()
        {
            var env = new FakeDataverseEnvironment(new FakeOrganizationServiceOptions { ValidateWithMetadata = true });
            var service = env.CreateOrganizationService();
            env.MetadataStore.AddEntity("account")
                .WithMoneyAttribute("revenue", minValue: 0, maxValue: 1000000);

            var entity = new Entity("account") { ["revenue"] = new Money(-100m) };

            Assert.Throws<FaultException<OrganizationServiceFault>>(
                () => service.Create(entity));
        }

        #endregion

        #region OptionSet Validation

        [Fact]
        public void Create_InvalidOptionSetValue_ThrowsFault()
        {
            var env = new FakeDataverseEnvironment(new FakeOrganizationServiceOptions { ValidateWithMetadata = true });
            var service = env.CreateOrganizationService();
            env.MetadataStore.AddEntity("account")
                .WithOptionSetAttribute("industrycode", validValues: new[] { 1, 2, 3 });

            var entity = new Entity("account") { ["industrycode"] = new OptionSetValue(99) };

            var ex = Assert.Throws<FaultException<OrganizationServiceFault>>(
                () => service.Create(entity));
            Assert.Contains("invalid option set value", ex.Detail.Message);
        }

        [Fact]
        public void Create_ValidOptionSetValue_Succeeds()
        {
            var env = new FakeDataverseEnvironment(new FakeOrganizationServiceOptions { ValidateWithMetadata = true });
            var service = env.CreateOrganizationService();
            env.MetadataStore.AddEntity("account")
                .WithOptionSetAttribute("industrycode", validValues: new[] { 1, 2, 3 });

            var entity = new Entity("account") { ["industrycode"] = new OptionSetValue(2) };

            var id = service.Create(entity);
            Assert.NotEqual(Guid.Empty, id);
        }

        #endregion

        #region EntityReference Target Validation

        [Fact]
        public void Create_InvalidEntityReferenceTarget_ThrowsFault()
        {
            var env = new FakeDataverseEnvironment(new FakeOrganizationServiceOptions { ValidateWithMetadata = true });
            var service = env.CreateOrganizationService();
            env.MetadataStore.AddEntity("contact")
                .WithLookupAttribute("parentcustomerid", targetEntityTypes: new[] { "account" });

            var entity = new Entity("contact")
            {
                ["parentcustomerid"] = new EntityReference("lead", Guid.NewGuid())
            };

            var ex = Assert.Throws<FaultException<OrganizationServiceFault>>(
                () => service.Create(entity));
            Assert.Contains("does not accept entity type", ex.Detail.Message);
        }

        [Fact]
        public void Create_ValidEntityReferenceTarget_Succeeds()
        {
            var env = new FakeDataverseEnvironment(new FakeOrganizationServiceOptions { ValidateWithMetadata = true });
            var service = env.CreateOrganizationService();
            env.MetadataStore.AddEntity("contact")
                .WithLookupAttribute("parentcustomerid", targetEntityTypes: new[] { "account" });

            var entity = new Entity("contact")
            {
                ["parentcustomerid"] = new EntityReference("account", Guid.NewGuid())
            };

            var id = service.Create(entity);
            Assert.NotEqual(Guid.Empty, id);
        }

        #endregion

        #region Metadata Request Handlers

        [Fact]
        public void RetrieveEntityRequest_ReturnsEntityMetadata()
        {
            var env = new FakeDataverseEnvironment();
            var service = env.CreateOrganizationService();
            env.MetadataStore.AddEntity("account")
                .WithSchemaName("Account")
                .WithPrimaryIdAttribute("accountid")
                .WithPrimaryNameAttribute("name")
                .WithObjectTypeCode(1)
                .WithStringAttribute("name", maxLength: 200);

            var response = (RetrieveEntityResponse)service.Execute(
                new RetrieveEntityRequest { LogicalName = "account" });

            Assert.NotNull(response.EntityMetadata);
            Assert.Equal("account", response.EntityMetadata.LogicalName);
            Assert.Equal("Account", response.EntityMetadata.SchemaName);
            Assert.Single(response.EntityMetadata.Attributes);
        }

        [Fact]
        public void RetrieveEntityRequest_EntityNotFound_ThrowsFault()
        {
            var env = new FakeDataverseEnvironment();
            var service = env.CreateOrganizationService();

            Assert.Throws<FaultException<OrganizationServiceFault>>(
                () => service.Execute(new RetrieveEntityRequest { LogicalName = "nonexistent" }));
        }

        [Fact]
        public void RetrieveAllEntitiesRequest_ReturnsAllEntities()
        {
            var env = new FakeDataverseEnvironment();
            var service = env.CreateOrganizationService();
            env.MetadataStore.AddEntity("account").WithSchemaName("Account");
            env.MetadataStore.AddEntity("contact").WithSchemaName("Contact");

            var response = (RetrieveAllEntitiesResponse)service.Execute(
                new RetrieveAllEntitiesRequest());

            Assert.Equal(2, response.EntityMetadata.Length);
            Assert.Contains(response.EntityMetadata, e => e.LogicalName == "account");
            Assert.Contains(response.EntityMetadata, e => e.LogicalName == "contact");
        }

        [Fact]
        public void RetrieveAllEntitiesRequest_Empty_ReturnsEmptyArray()
        {
            var env = new FakeDataverseEnvironment();
            var service = env.CreateOrganizationService();

            var response = (RetrieveAllEntitiesResponse)service.Execute(
                new RetrieveAllEntitiesRequest());

            Assert.Empty(response.EntityMetadata);
        }

        [Fact]
        public void RetrieveAttributeRequest_ReturnsAttributeMetadata()
        {
            var env = new FakeDataverseEnvironment();
            var service = env.CreateOrganizationService();
            env.MetadataStore.AddEntity("account")
                .WithStringAttribute("name", maxLength: 200, requiredLevel: AttributeRequiredLevel.ApplicationRequired);

            var response = (RetrieveAttributeResponse)service.Execute(
                new RetrieveAttributeRequest
                {
                    EntityLogicalName = "account",
                    LogicalName = "name"
                });

            Assert.NotNull(response.AttributeMetadata);
            Assert.Equal("name", response.AttributeMetadata.LogicalName);
            var strAttr = Assert.IsType<StringAttributeMetadata>(response.AttributeMetadata);
            Assert.Equal(200, strAttr.MaxLength);
        }

        [Fact]
        public void RetrieveAttributeRequest_AttributeNotFound_ThrowsFault()
        {
            var env = new FakeDataverseEnvironment();
            var service = env.CreateOrganizationService();
            env.MetadataStore.AddEntity("account");

            Assert.Throws<FaultException<OrganizationServiceFault>>(
                () => service.Execute(new RetrieveAttributeRequest
                {
                    EntityLogicalName = "account",
                    LogicalName = "nonexistent"
                }));
        }

        [Fact]
        public void RetrieveAttributeRequest_EntityNotFound_ThrowsFault()
        {
            var env = new FakeDataverseEnvironment();
            var service = env.CreateOrganizationService();

            Assert.Throws<FaultException<OrganizationServiceFault>>(
                () => service.Execute(new RetrieveAttributeRequest
                {
                    EntityLogicalName = "nonexistent",
                    LogicalName = "name"
                }));
        }

        #endregion

        #region Validation Off By Default

        [Fact]
        public void ValidationOffByDefault_InvalidCreate_Succeeds()
        {
            var env = new FakeDataverseEnvironment();
            var service = env.CreateOrganizationService();
            env.MetadataStore.AddEntity("account")
                .WithStringAttribute("name", maxLength: 5, requiredLevel: AttributeRequiredLevel.ApplicationRequired);

            // No required field, string too long — should succeed because validation is OFF.
            var entity = new Entity("account") { ["name"] = "VeryLongName" };

            var id = service.Create(entity);
            Assert.NotEqual(Guid.Empty, id);
        }

        [Fact]
        public void ValidateWithMetadata_DefaultIsFalse()
        {
            var env = new FakeDataverseEnvironment();
            var service = env.CreateOrganizationService();
            Assert.False(env.Options.ValidateWithMetadata);
        }

        #endregion

        #region Auto-Discovery

        [Fact]
        public void AutoDiscover_InfersStringAttributeType()
        {
            var env = new FakeDataverseEnvironment();
            var service = env.CreateOrganizationService();
            env.MetadataStore.AutoDiscoverMetadata = true;

            service.Create(new Entity("account") { ["name"] = "Contoso" });

            var response = (RetrieveEntityResponse)service.Execute(
                new RetrieveEntityRequest { LogicalName = "account" });

            var nameAttr = response.EntityMetadata.Attributes
                .FirstOrDefault(a => a.LogicalName == "name");
            Assert.NotNull(nameAttr);
            Assert.IsType<StringAttributeMetadata>(nameAttr);
        }

        [Fact]
        public void AutoDiscover_InfersMultipleAttributeTypes()
        {
            var env = new FakeDataverseEnvironment();
            var service = env.CreateOrganizationService();
            env.MetadataStore.AutoDiscoverMetadata = true;

            service.Create(new Entity("account")
            {
                ["name"] = "Contoso",
                ["numberofemployees"] = 500,
                ["revenue"] = new Money(1000m),
                ["primarycontactid"] = new EntityReference("contact", Guid.NewGuid())
            });

            var response = (RetrieveEntityResponse)service.Execute(
                new RetrieveEntityRequest { LogicalName = "account" });

            // Auto-discovered attributes + system attributes (createdon, modifiedon, createdby, modifiedby, statecode, statuscode, versionnumber)
            Assert.Contains(response.EntityMetadata.Attributes, a => a.LogicalName == "name");
            Assert.Contains(response.EntityMetadata.Attributes, a => a.LogicalName == "numberofemployees");
            Assert.Contains(response.EntityMetadata.Attributes, a => a.LogicalName == "revenue");
            Assert.Contains(response.EntityMetadata.Attributes, a => a.LogicalName == "primarycontactid");
        }

        [Fact]
        public void AutoDiscover_DoesNotOverwriteExistingMetadata()
        {
            var env = new FakeDataverseEnvironment();
            var service = env.CreateOrganizationService();
            env.MetadataStore.AutoDiscoverMetadata = true;
            env.MetadataStore.AddEntity("account")
                .WithStringAttribute("name", maxLength: 100);

            service.Create(new Entity("account") { ["name"] = "Contoso", ["phone"] = "555-1234" });

            var response = (RetrieveEntityResponse)service.Execute(
                new RetrieveEntityRequest { LogicalName = "account" });

            // "name" should retain its explicit maxLength metadata.
            var nameAttr = response.EntityMetadata.Attributes.First(a => a.LogicalName == "name");
            var strAttr = Assert.IsType<StringAttributeMetadata>(nameAttr);
            Assert.Equal(100, strAttr.MaxLength);
        }

        [Fact]
        public void AutoDiscover_DisabledByDefault_NoMetadataCreated()
        {
            var env = new FakeDataverseEnvironment();
            var service = env.CreateOrganizationService();

            service.Create(new Entity("account") { ["name"] = "Contoso" });

            // No metadata defined, auto-discover is off, so RetrieveEntityRequest should throw.
            Assert.Throws<FaultException<OrganizationServiceFault>>(
                () => service.Execute(new RetrieveEntityRequest { LogicalName = "account" }));
        }

        #endregion

        #region Relationship Metadata

        [Fact]
        public void RetrieveEntityRequest_IncludesRelationships()
        {
            var env = new FakeDataverseEnvironment();
            var service = env.CreateOrganizationService();
            env.MetadataStore.AddEntity("account")
                .WithManyToManyRelationship("account_contact", "account", "contact");
            env.MetadataStore.AddEntity("contact");
            env.MetadataStore.AddOneToManyRelationship(
                "account_contacts_1n", "account", "accountid", "contact", "parentcustomerid");

            var response = (RetrieveEntityResponse)service.Execute(
                new RetrieveEntityRequest { LogicalName = "account" });

            Assert.NotNull(response.EntityMetadata.ManyToManyRelationships);
            Assert.Single(response.EntityMetadata.ManyToManyRelationships);
            Assert.Equal("account_contact", response.EntityMetadata.ManyToManyRelationships[0].SchemaName);

            Assert.NotNull(response.EntityMetadata.OneToManyRelationships);
            Assert.Single(response.EntityMetadata.OneToManyRelationships);
        }

        [Fact]
        public void Associate_ValidRelationship_Succeeds()
        {
            var env = new FakeDataverseEnvironment(new FakeOrganizationServiceOptions { ValidateWithMetadata = true });
            var service = env.CreateOrganizationService();
            env.MetadataStore.AddManyToManyRelationship("account_contact", "account", "contact");

            var accountId = service.Create(new Entity("account") { ["name"] = "Contoso" });
            var contactId = service.Create(new Entity("contact") { ["lastname"] = "Doe" });

            service.Associate(
                "account",
                accountId,
                new Relationship("account_contact"),
                new EntityReferenceCollection { new EntityReference("contact", contactId) });
        }

        [Fact]
        public void Associate_InvalidEntityType_ThrowsFault()
        {
            var env = new FakeDataverseEnvironment(new FakeOrganizationServiceOptions { ValidateWithMetadata = true });
            var service = env.CreateOrganizationService();
            env.MetadataStore.AddManyToManyRelationship("account_contact", "account", "contact");

            var accountId = service.Create(new Entity("account") { ["name"] = "Contoso" });
            var leadId = service.Create(new Entity("lead") { ["subject"] = "Test" });

            Assert.Throws<FaultException<OrganizationServiceFault>>(
                () => service.Associate(
                    "account",
                    accountId,
                    new Relationship("account_contact"),
                    new EntityReferenceCollection { new EntityReference("lead", leadId) }));
        }

        [Fact]
        public void Disassociate_InvalidEntityType_ThrowsFault()
        {
            var env = new FakeDataverseEnvironment(new FakeOrganizationServiceOptions { ValidateWithMetadata = true });
            var service = env.CreateOrganizationService();
            env.MetadataStore.AddManyToManyRelationship("account_contact", "account", "contact");

            var accountId = service.Create(new Entity("account") { ["name"] = "Contoso" });

            Assert.Throws<FaultException<OrganizationServiceFault>>(
                () => service.Disassociate(
                    "account",
                    accountId,
                    new Relationship("account_contact"),
                    new EntityReferenceCollection { new EntityReference("lead", Guid.NewGuid()) }));
        }

        [Fact]
        public void Associate_UndefinedRelationship_SkipsValidation()
        {
            var env = new FakeDataverseEnvironment(new FakeOrganizationServiceOptions { ValidateWithMetadata = true });
            var service = env.CreateOrganizationService();

            var accountId = service.Create(new Entity("account") { ["name"] = "Contoso" });
            var contactId = service.Create(new Entity("contact") { ["lastname"] = "Doe" });

            // No relationship metadata defined — should not throw.
            service.Associate(
                "account",
                accountId,
                new Relationship("some_undefined_relationship"),
                new EntityReferenceCollection { new EntityReference("contact", contactId) });
        }

        #endregion

        #region Attribute Type Specific Metadata Responses

        [Fact]
        public void RetrieveAttribute_IntegerAttribute_HasMinMaxValues()
        {
            var env = new FakeDataverseEnvironment();
            var service = env.CreateOrganizationService();
            env.MetadataStore.AddEntity("account")
                .WithIntegerAttribute("numberofemployees", minValue: 0, maxValue: 100000);

            var response = (RetrieveAttributeResponse)service.Execute(
                new RetrieveAttributeRequest
                {
                    EntityLogicalName = "account",
                    LogicalName = "numberofemployees"
                });

            var intAttr = Assert.IsType<IntegerAttributeMetadata>(response.AttributeMetadata);
            Assert.Equal(0, intAttr.MinValue);
            Assert.Equal(100000, intAttr.MaxValue);
        }

        [Fact]
        public void RetrieveAttribute_PicklistAttribute_HasOptions()
        {
            var env = new FakeDataverseEnvironment();
            var service = env.CreateOrganizationService();
            env.MetadataStore.AddEntity("account")
                .WithOptionSetAttribute("industrycode", validValues: new[] { 1, 2, 3 });

            var response = (RetrieveAttributeResponse)service.Execute(
                new RetrieveAttributeRequest
                {
                    EntityLogicalName = "account",
                    LogicalName = "industrycode"
                });

            var plAttr = Assert.IsType<PicklistAttributeMetadata>(response.AttributeMetadata);
            Assert.NotNull(plAttr.OptionSet);
            Assert.Equal(3, plAttr.OptionSet.Options.Count);
        }

        [Fact]
        public void RetrieveAttribute_LookupAttribute_HasTargets()
        {
            var env = new FakeDataverseEnvironment();
            var service = env.CreateOrganizationService();
            env.MetadataStore.AddEntity("contact")
                .WithLookupAttribute("parentcustomerid", targetEntityTypes: new[] { "account", "contact" });

            var response = (RetrieveAttributeResponse)service.Execute(
                new RetrieveAttributeRequest
                {
                    EntityLogicalName = "contact",
                    LogicalName = "parentcustomerid"
                });

            var lookupAttr = Assert.IsType<LookupAttributeMetadata>(response.AttributeMetadata);
            Assert.Equal(2, lookupAttr.Targets.Length);
            Assert.Contains("account", lookupAttr.Targets);
            Assert.Contains("contact", lookupAttr.Targets);
        }

        #endregion

        #region Decimal / Double / Money Validation

        [Fact]
        public void Create_DecimalBelowMin_ThrowsFault()
        {
            var env = new FakeDataverseEnvironment(new FakeOrganizationServiceOptions { ValidateWithMetadata = true });
            var service = env.CreateOrganizationService();
            env.MetadataStore.AddEntity("product")
                .WithDecimalAttribute("price", minValue: 0m, maxValue: 10000m);

            var entity = new Entity("product") { ["price"] = -5m };

            Assert.Throws<FaultException<OrganizationServiceFault>>(
                () => service.Create(entity));
        }

        [Fact]
        public void Create_DoubleAboveMax_ThrowsFault()
        {
            var env = new FakeDataverseEnvironment(new FakeOrganizationServiceOptions { ValidateWithMetadata = true });
            var service = env.CreateOrganizationService();
            env.MetadataStore.AddEntity("sensor")
                .WithDoubleAttribute("temperature", minValue: -50.0, maxValue: 100.0);

            var entity = new Entity("sensor") { ["temperature"] = 200.0 };

            Assert.Throws<FaultException<OrganizationServiceFault>>(
                () => service.Create(entity));
        }

        #endregion
    }
}
