using System;
using System.Linq;
using System.ServiceModel;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Messages;
using Microsoft.Xrm.Sdk.Metadata;
using Xunit;

namespace Fake4Dataverse.Tests
{
    public class MetadataMessageTests
    {
        #region CreateEntityRequest

        [Fact]
        public void CreateEntityRequest_WithValidEntity_Succeeds()
        {
            var env = new FakeDataverseEnvironment();
            var service = env.CreateOrganizationService();
            var entityMetadata = new EntityMetadata();
            entityMetadata.LogicalName = "new_custom";
            entityMetadata.SchemaName = "new_Custom";

            var response = (CreateEntityResponse)service.Execute(
                new CreateEntityRequest { Entity = entityMetadata });

            Assert.NotEqual(Guid.Empty, response.EntityId);
        }

        [Fact]
        public void CreateEntityRequest_ThenRetrieve_ReturnsEntity()
        {
            var env = new FakeDataverseEnvironment();
            var service = env.CreateOrganizationService();
            var entityMetadata = new EntityMetadata();
            entityMetadata.LogicalName = "new_custom";
            entityMetadata.SchemaName = "new_Custom";

            service.Execute(new CreateEntityRequest { Entity = entityMetadata });

            var response = (RetrieveEntityResponse)service.Execute(
                new RetrieveEntityRequest { LogicalName = "new_custom" });

            Assert.Equal("new_custom", response.EntityMetadata.LogicalName);
            Assert.Equal("new_Custom", response.EntityMetadata.SchemaName);
        }

        [Fact]
        public void CreateEntityRequest_DuplicateEntity_ThrowsFault()
        {
            var env = new FakeDataverseEnvironment();
            var service = env.CreateOrganizationService();
            var entityMetadata = new EntityMetadata();
            entityMetadata.LogicalName = "new_custom";

            service.Execute(new CreateEntityRequest { Entity = entityMetadata });

            Assert.Throws<FaultException<OrganizationServiceFault>>(
                () => service.Execute(new CreateEntityRequest { Entity = entityMetadata }));
        }

        #endregion

        #region UpdateEntityRequest

        [Fact]
        public void UpdateEntityRequest_UpdatesSchemaName()
        {
            var env = new FakeDataverseEnvironment();
            var service = env.CreateOrganizationService();
            var entityMetadata = new EntityMetadata();
            entityMetadata.LogicalName = "new_custom";
            entityMetadata.SchemaName = "OldName";
            service.Execute(new CreateEntityRequest { Entity = entityMetadata });

            var updated = new EntityMetadata();
            updated.LogicalName = "new_custom";
            updated.SchemaName = "NewName";
            service.Execute(new UpdateEntityRequest { Entity = updated });

            var response = (RetrieveEntityResponse)service.Execute(
                new RetrieveEntityRequest { LogicalName = "new_custom" });
            Assert.Equal("NewName", response.EntityMetadata.SchemaName);
        }

        [Fact]
        public void UpdateEntityRequest_EntityNotFound_ThrowsFault()
        {
            var env = new FakeDataverseEnvironment();
            var service = env.CreateOrganizationService();
            var entityMetadata = new EntityMetadata();
            entityMetadata.LogicalName = "nonexistent";

            Assert.Throws<FaultException<OrganizationServiceFault>>(
                () => service.Execute(new UpdateEntityRequest { Entity = entityMetadata }));
        }

        #endregion

        #region DeleteEntityRequest

        [Fact]
        public void DeleteEntityRequest_RemovesEntity()
        {
            var env = new FakeDataverseEnvironment();
            var service = env.CreateOrganizationService();
            var entityMetadata = new EntityMetadata();
            entityMetadata.LogicalName = "new_custom";
            service.Execute(new CreateEntityRequest { Entity = entityMetadata });

            service.Execute(new DeleteEntityRequest { LogicalName = "new_custom" });

            Assert.Throws<FaultException<OrganizationServiceFault>>(
                () => service.Execute(new RetrieveEntityRequest { LogicalName = "new_custom" }));
        }

        [Fact]
        public void DeleteEntityRequest_EntityNotFound_ThrowsFault()
        {
            var env = new FakeDataverseEnvironment();
            var service = env.CreateOrganizationService();

            Assert.Throws<FaultException<OrganizationServiceFault>>(
                () => service.Execute(new DeleteEntityRequest { LogicalName = "nonexistent" }));
        }

        #endregion

        #region CreateAttributeRequest

        [Fact]
        public void CreateAttributeRequest_AddsAttribute()
        {
            var env = new FakeDataverseEnvironment();
            var service = env.CreateOrganizationService();
            env.MetadataStore.AddEntity("account");

            var attr = new StringAttributeMetadata { LogicalName = "new_field" };
            var request = new CreateAttributeRequest();
            request.Parameters["EntityName"] = "account";
            request.Parameters["Attribute"] = attr;

            var response = (CreateAttributeResponse)service.Execute(request);

            Assert.NotEqual(Guid.Empty, response.AttributeId);

            var retrieveResponse = (RetrieveAttributeResponse)service.Execute(
                new RetrieveAttributeRequest
                {
                    EntityLogicalName = "account",
                    LogicalName = "new_field"
                });
            Assert.Equal("new_field", retrieveResponse.AttributeMetadata.LogicalName);
        }

        [Fact]
        public void CreateAttributeRequest_EntityNotFound_ThrowsFault()
        {
            var env = new FakeDataverseEnvironment();
            var service = env.CreateOrganizationService();
            var attr = new StringAttributeMetadata { LogicalName = "new_field" };
            var request = new CreateAttributeRequest();
            request.Parameters["EntityName"] = "nonexistent";
            request.Parameters["Attribute"] = attr;

            Assert.Throws<FaultException<OrganizationServiceFault>>(
                () => service.Execute(request));
        }

        [Fact]
        public void CreateAttributeRequest_DuplicateAttribute_ThrowsFault()
        {
            var env = new FakeDataverseEnvironment();
            var service = env.CreateOrganizationService();
            env.MetadataStore.AddEntity("account").WithStringAttribute("name");

            var attr = new StringAttributeMetadata { LogicalName = "name" };
            var request = new CreateAttributeRequest();
            request.Parameters["EntityName"] = "account";
            request.Parameters["Attribute"] = attr;

            Assert.Throws<FaultException<OrganizationServiceFault>>(
                () => service.Execute(request));
        }

        #endregion

        #region UpdateAttributeRequest

        [Fact]
        public void UpdateAttributeRequest_UpdatesAttribute()
        {
            var env = new FakeDataverseEnvironment();
            var service = env.CreateOrganizationService();
            env.MetadataStore.AddEntity("account").WithStringAttribute("name");

            var attr = new IntegerAttributeMetadata { LogicalName = "name" };
            var request = new UpdateAttributeRequest();
            request.Parameters["EntityName"] = "account";
            request.Parameters["Attribute"] = attr;

            service.Execute(request);

            var response = (RetrieveAttributeResponse)service.Execute(
                new RetrieveAttributeRequest
                {
                    EntityLogicalName = "account",
                    LogicalName = "name"
                });
            Assert.IsType<IntegerAttributeMetadata>(response.AttributeMetadata);
        }

        [Fact]
        public void UpdateAttributeRequest_AttributeNotFound_ThrowsFault()
        {
            var env = new FakeDataverseEnvironment();
            var service = env.CreateOrganizationService();
            env.MetadataStore.AddEntity("account");

            var attr = new StringAttributeMetadata { LogicalName = "nonexistent" };
            var request = new UpdateAttributeRequest();
            request.Parameters["EntityName"] = "account";
            request.Parameters["Attribute"] = attr;

            Assert.Throws<FaultException<OrganizationServiceFault>>(
                () => service.Execute(request));
        }

        #endregion

        #region DeleteAttributeRequest

        [Fact]
        public void DeleteAttributeRequest_RemovesAttribute()
        {
            var env = new FakeDataverseEnvironment();
            var service = env.CreateOrganizationService();
            env.MetadataStore.AddEntity("account").WithStringAttribute("name");

            service.Execute(new DeleteAttributeRequest
            {
                EntityLogicalName = "account",
                LogicalName = "name"
            });

            Assert.Throws<FaultException<OrganizationServiceFault>>(
                () => service.Execute(new RetrieveAttributeRequest
                {
                    EntityLogicalName = "account",
                    LogicalName = "name"
                }));
        }

        [Fact]
        public void DeleteAttributeRequest_AttributeNotFound_ThrowsFault()
        {
            var env = new FakeDataverseEnvironment();
            var service = env.CreateOrganizationService();
            env.MetadataStore.AddEntity("account");

            Assert.Throws<FaultException<OrganizationServiceFault>>(
                () => service.Execute(new DeleteAttributeRequest
                {
                    EntityLogicalName = "account",
                    LogicalName = "nonexistent"
                }));
        }

        #endregion

        #region CreateOneToManyRequest

        [Fact]
        public void CreateOneToManyRequest_CreatesRelationship()
        {
            var env = new FakeDataverseEnvironment();
            var service = env.CreateOrganizationService();
            env.MetadataStore.AddEntity("account");
            env.MetadataStore.AddEntity("contact");

            var rel = new OneToManyRelationshipMetadata
            {
                SchemaName = "account_contacts",
                ReferencedEntity = "account",
                ReferencedAttribute = "accountid",
                ReferencingEntity = "contact",
                ReferencingAttribute = "parentcustomerid"
            };

            var response = (CreateOneToManyResponse)service.Execute(
                new CreateOneToManyRequest { OneToManyRelationship = rel });

            Assert.NotEqual(Guid.Empty, response.RelationshipId);
        }

        [Fact]
        public void CreateOneToManyRequest_ThenRetrieve_ReturnsRelationship()
        {
            var env = new FakeDataverseEnvironment();
            var service = env.CreateOrganizationService();

            var rel = new OneToManyRelationshipMetadata
            {
                SchemaName = "account_contacts",
                ReferencedEntity = "account",
                ReferencedAttribute = "accountid",
                ReferencingEntity = "contact",
                ReferencingAttribute = "parentcustomerid"
            };

            service.Execute(new CreateOneToManyRequest { OneToManyRelationship = rel });

            var response = (RetrieveRelationshipResponse)service.Execute(
                new RetrieveRelationshipRequest { Name = "account_contacts" });

            var otm = Assert.IsType<OneToManyRelationshipMetadata>(response.RelationshipMetadata);
            Assert.Equal("account", otm.ReferencedEntity);
            Assert.Equal("contact", otm.ReferencingEntity);
        }

        [Fact]
        public void CreateOneToManyRequest_Duplicate_ThrowsFault()
        {
            var env = new FakeDataverseEnvironment();
            var service = env.CreateOrganizationService();
            var rel = new OneToManyRelationshipMetadata
            {
                SchemaName = "account_contacts",
                ReferencedEntity = "account",
                ReferencedAttribute = "accountid",
                ReferencingEntity = "contact",
                ReferencingAttribute = "parentcustomerid"
            };

            service.Execute(new CreateOneToManyRequest { OneToManyRelationship = rel });

            Assert.Throws<FaultException<OrganizationServiceFault>>(
                () => service.Execute(new CreateOneToManyRequest { OneToManyRelationship = rel }));
        }

        #endregion

        #region CreateManyToManyRequest

        [Fact]
        public void CreateManyToManyRequest_CreatesRelationship()
        {
            var env = new FakeDataverseEnvironment();
            var service = env.CreateOrganizationService();

            var rel = new ManyToManyRelationshipMetadata
            {
                SchemaName = "account_contact_nn",
                Entity1LogicalName = "account",
                Entity2LogicalName = "contact",
                IntersectEntityName = "account_contact_intersect"
            };

            var response = (CreateManyToManyResponse)service.Execute(
                new CreateManyToManyRequest { ManyToManyRelationship = rel });

            Assert.NotEqual(Guid.Empty, response.ManyToManyRelationshipId);
        }

        [Fact]
        public void CreateManyToManyRequest_ThenRetrieve_ReturnsManyToMany()
        {
            var env = new FakeDataverseEnvironment();
            var service = env.CreateOrganizationService();
            var rel = new ManyToManyRelationshipMetadata
            {
                SchemaName = "account_contact_nn",
                Entity1LogicalName = "account",
                Entity2LogicalName = "contact",
                IntersectEntityName = "account_contact_intersect"
            };

            service.Execute(new CreateManyToManyRequest { ManyToManyRelationship = rel });

            var response = (RetrieveRelationshipResponse)service.Execute(
                new RetrieveRelationshipRequest { Name = "account_contact_nn" });

            var mtm = Assert.IsType<ManyToManyRelationshipMetadata>(response.RelationshipMetadata);
            Assert.Equal("account", mtm.Entity1LogicalName);
            Assert.Equal("contact", mtm.Entity2LogicalName);
        }

        #endregion

        #region UpdateRelationshipRequest

        [Fact]
        public void UpdateRelationshipRequest_UpdatesOneToMany()
        {
            var env = new FakeDataverseEnvironment();
            var service = env.CreateOrganizationService();
            env.MetadataStore.AddOneToManyRelationship(
                "account_contacts", "account", "accountid", "contact", "parentcustomerid");

            var updated = new OneToManyRelationshipMetadata
            {
                SchemaName = "account_contacts",
                ReferencedEntity = "account",
                ReferencedAttribute = "accountid",
                ReferencingEntity = "contact",
                ReferencingAttribute = "new_accountid"
            };

            service.Execute(new UpdateRelationshipRequest { Relationship = updated });

            var response = (RetrieveRelationshipResponse)service.Execute(
                new RetrieveRelationshipRequest { Name = "account_contacts" });

            var otm = Assert.IsType<OneToManyRelationshipMetadata>(response.RelationshipMetadata);
            Assert.Equal("new_accountid", otm.ReferencingAttribute);
        }

        [Fact]
        public void UpdateRelationshipRequest_NotFound_ThrowsFault()
        {
            var env = new FakeDataverseEnvironment();
            var service = env.CreateOrganizationService();
            var rel = new OneToManyRelationshipMetadata
            {
                SchemaName = "nonexistent",
                ReferencedEntity = "a",
                ReferencedAttribute = "b",
                ReferencingEntity = "c",
                ReferencingAttribute = "d"
            };

            Assert.Throws<FaultException<OrganizationServiceFault>>(
                () => service.Execute(new UpdateRelationshipRequest { Relationship = rel }));
        }

        #endregion

        #region DeleteRelationshipRequest

        [Fact]
        public void DeleteRelationshipRequest_RemovesRelationship()
        {
            var env = new FakeDataverseEnvironment();
            var service = env.CreateOrganizationService();
            env.MetadataStore.AddOneToManyRelationship(
                "account_contacts", "account", "accountid", "contact", "parentcustomerid");

            service.Execute(new DeleteRelationshipRequest { Name = "account_contacts" });

            Assert.Throws<FaultException<OrganizationServiceFault>>(
                () => service.Execute(new RetrieveRelationshipRequest { Name = "account_contacts" }));
        }

        [Fact]
        public void DeleteRelationshipRequest_NotFound_ThrowsFault()
        {
            var env = new FakeDataverseEnvironment();
            var service = env.CreateOrganizationService();

            Assert.Throws<FaultException<OrganizationServiceFault>>(
                () => service.Execute(new DeleteRelationshipRequest { Name = "nonexistent" }));
        }

        #endregion

        #region RetrieveRelationshipRequest

        [Fact]
        public void RetrieveRelationshipRequest_OneToMany_ReturnsCorrectType()
        {
            var env = new FakeDataverseEnvironment();
            var service = env.CreateOrganizationService();
            env.MetadataStore.AddOneToManyRelationship(
                "account_contacts", "account", "accountid", "contact", "parentcustomerid");

            var response = (RetrieveRelationshipResponse)service.Execute(
                new RetrieveRelationshipRequest { Name = "account_contacts" });

            Assert.IsType<OneToManyRelationshipMetadata>(response.RelationshipMetadata);
        }

        [Fact]
        public void RetrieveRelationshipRequest_ManyToMany_ReturnsCorrectType()
        {
            var env = new FakeDataverseEnvironment();
            var service = env.CreateOrganizationService();
            env.MetadataStore.AddManyToManyRelationship("account_contact_nn", "account", "contact");

            var response = (RetrieveRelationshipResponse)service.Execute(
                new RetrieveRelationshipRequest { Name = "account_contact_nn" });

            Assert.IsType<ManyToManyRelationshipMetadata>(response.RelationshipMetadata);
        }

        [Fact]
        public void RetrieveRelationshipRequest_NotFound_ThrowsFault()
        {
            var env = new FakeDataverseEnvironment();
            var service = env.CreateOrganizationService();

            Assert.Throws<FaultException<OrganizationServiceFault>>(
                () => service.Execute(new RetrieveRelationshipRequest { Name = "nonexistent" }));
        }

        #endregion

        #region CreateEntityKeyRequest

        [Fact]
        public void CreateEntityKeyRequest_CreatesKey()
        {
            var env = new FakeDataverseEnvironment();
            var service = env.CreateOrganizationService();
            env.MetadataStore.AddEntity("account");

            var key = new EntityKeyMetadata();
            key.LogicalName = "account_number_key";
            key.KeyAttributes = new[] { "accountnumber" };

            var request = new CreateEntityKeyRequest();
            request.Parameters["EntityName"] = "account";
            request.Parameters["EntityKey"] = key;

            var response = (CreateEntityKeyResponse)service.Execute(request);

            Assert.NotEqual(Guid.Empty, response.EntityKeyId);
        }

        [Fact]
        public void CreateEntityKeyRequest_ThenRetrieve_ReturnsKey()
        {
            var env = new FakeDataverseEnvironment();
            var service = env.CreateOrganizationService();
            env.MetadataStore.AddEntity("account");

            var key = new EntityKeyMetadata();
            key.LogicalName = "account_number_key";
            key.KeyAttributes = new[] { "accountnumber" };

            var createReq = new CreateEntityKeyRequest();
            createReq.Parameters["EntityName"] = "account";
            createReq.Parameters["EntityKey"] = key;
            service.Execute(createReq);

            var retrieveReq = new RetrieveEntityKeyRequest();
            retrieveReq.Parameters["EntityLogicalName"] = "account";
            retrieveReq.Parameters["LogicalName"] = "account_number_key";

            var response = (RetrieveEntityKeyResponse)service.Execute(retrieveReq);
            Assert.Equal("account_number_key", response.EntityKeyMetadata.LogicalName);
            Assert.Contains("accountnumber", response.EntityKeyMetadata.KeyAttributes);
        }

        [Fact]
        public void CreateEntityKeyRequest_DuplicateKey_ThrowsFault()
        {
            var env = new FakeDataverseEnvironment();
            var service = env.CreateOrganizationService();
            env.MetadataStore.AddEntity("account");

            var key = new EntityKeyMetadata();
            key.LogicalName = "account_number_key";
            key.KeyAttributes = new[] { "accountnumber" };

            var request = new CreateEntityKeyRequest();
            request.Parameters["EntityName"] = "account";
            request.Parameters["EntityKey"] = key;

            service.Execute(request);

            Assert.Throws<FaultException<OrganizationServiceFault>>(
                () => service.Execute(request));
        }

        #endregion

        #region DeleteEntityKeyRequest

        [Fact]
        public void DeleteEntityKeyRequest_RemovesKey()
        {
            var env = new FakeDataverseEnvironment();
            var service = env.CreateOrganizationService();
            env.MetadataStore.AddEntity("account")
                .WithAlternateKey("account_number_key", "accountnumber");

            service.Execute(new DeleteEntityKeyRequest
            {
                EntityLogicalName = "account",
                Name = "account_number_key"
            });

            var retrieveReq = new RetrieveEntityKeyRequest();
            retrieveReq.Parameters["EntityLogicalName"] = "account";
            retrieveReq.Parameters["LogicalName"] = "account_number_key";

            Assert.Throws<FaultException<OrganizationServiceFault>>(
                () => service.Execute(retrieveReq));
        }

        [Fact]
        public void DeleteEntityKeyRequest_NotFound_ThrowsFault()
        {
            var env = new FakeDataverseEnvironment();
            var service = env.CreateOrganizationService();
            env.MetadataStore.AddEntity("account");

            Assert.Throws<FaultException<OrganizationServiceFault>>(
                () => service.Execute(new DeleteEntityKeyRequest
                {
                    EntityLogicalName = "account",
                    Name = "nonexistent"
                }));
        }

        #endregion

        #region ReactivateEntityKeyRequest

        [Fact]
        public void ReactivateEntityKeyRequest_ExistingKey_Succeeds()
        {
            var env = new FakeDataverseEnvironment();
            var service = env.CreateOrganizationService();
            env.MetadataStore.AddEntity("account")
                .WithAlternateKey("account_number_key", "accountnumber");

            var request = new ReactivateEntityKeyRequest();
            request.Parameters["EntityLogicalName"] = "account";
            request.Parameters["EntityKeyLogicalName"] = "account_number_key";

            service.Execute(request); // Should not throw
        }

        [Fact]
        public void ReactivateEntityKeyRequest_KeyNotFound_ThrowsFault()
        {
            var env = new FakeDataverseEnvironment();
            var service = env.CreateOrganizationService();
            env.MetadataStore.AddEntity("account");

            var request = new ReactivateEntityKeyRequest();
            request.Parameters["EntityLogicalName"] = "account";
            request.Parameters["EntityKeyLogicalName"] = "nonexistent";

            Assert.Throws<FaultException<OrganizationServiceFault>>(
                () => service.Execute(request));
        }

        #endregion

        #region CreateOptionSetRequest

        [Fact]
        public void CreateOptionSetRequest_CreatesGlobalOptionSet()
        {
            var env = new FakeDataverseEnvironment();
            var service = env.CreateOrganizationService();
            var optionSet = new OptionSetMetadata();
            optionSet.Name = "new_colors";
            optionSet.Options.Add(new OptionMetadata(new Label("Red", 1033), 1));
            optionSet.Options.Add(new OptionMetadata(new Label("Blue", 1033), 2));

            var response = (CreateOptionSetResponse)service.Execute(
                new CreateOptionSetRequest { OptionSet = optionSet });

            Assert.NotEqual(Guid.Empty, response.OptionSetId);
        }

        [Fact]
        public void CreateOptionSetRequest_ThenRetrieve_ReturnsOptionSet()
        {
            var env = new FakeDataverseEnvironment();
            var service = env.CreateOrganizationService();
            var optionSet = new OptionSetMetadata();
            optionSet.Name = "new_colors";
            optionSet.Options.Add(new OptionMetadata(new Label("Red", 1033), 1));

            service.Execute(new CreateOptionSetRequest { OptionSet = optionSet });

            var response = (RetrieveOptionSetResponse)service.Execute(
                new RetrieveOptionSetRequest { Name = "new_colors" });

            var retrieved = Assert.IsType<OptionSetMetadata>(response.OptionSetMetadata);
            Assert.Equal("new_colors", retrieved.Name);
            Assert.Single(retrieved.Options);
        }

        [Fact]
        public void CreateOptionSetRequest_Duplicate_ThrowsFault()
        {
            var env = new FakeDataverseEnvironment();
            var service = env.CreateOrganizationService();
            var optionSet = new OptionSetMetadata { Name = "new_colors" };

            service.Execute(new CreateOptionSetRequest { OptionSet = optionSet });

            Assert.Throws<FaultException<OrganizationServiceFault>>(
                () => service.Execute(new CreateOptionSetRequest { OptionSet = optionSet }));
        }

        #endregion

        #region UpdateOptionSetRequest

        [Fact]
        public void UpdateOptionSetRequest_UpdatesGlobalOptionSet()
        {
            var env = new FakeDataverseEnvironment();
            var service = env.CreateOrganizationService();
            var optionSet = new OptionSetMetadata { Name = "new_colors" };
            optionSet.Options.Add(new OptionMetadata(new Label("Red", 1033), 1));
            service.Execute(new CreateOptionSetRequest { OptionSet = optionSet });

            var updated = new OptionSetMetadata { Name = "new_colors" };
            updated.Options.Add(new OptionMetadata(new Label("Red", 1033), 1));
            updated.Options.Add(new OptionMetadata(new Label("Blue", 1033), 2));
            service.Execute(new UpdateOptionSetRequest { OptionSet = updated });

            var response = (RetrieveOptionSetResponse)service.Execute(
                new RetrieveOptionSetRequest { Name = "new_colors" });
            var retrieved = Assert.IsType<OptionSetMetadata>(response.OptionSetMetadata);
            Assert.Equal(2, retrieved.Options.Count);
        }

        [Fact]
        public void UpdateOptionSetRequest_NotFound_ThrowsFault()
        {
            var env = new FakeDataverseEnvironment();
            var service = env.CreateOrganizationService();
            var optionSet = new OptionSetMetadata { Name = "nonexistent" };

            Assert.Throws<FaultException<OrganizationServiceFault>>(
                () => service.Execute(new UpdateOptionSetRequest { OptionSet = optionSet }));
        }

        #endregion

        #region DeleteOptionSetRequest

        [Fact]
        public void DeleteOptionSetRequest_RemovesGlobalOptionSet()
        {
            var env = new FakeDataverseEnvironment();
            var service = env.CreateOrganizationService();
            var optionSet = new OptionSetMetadata { Name = "new_colors" };
            service.Execute(new CreateOptionSetRequest { OptionSet = optionSet });

            service.Execute(new DeleteOptionSetRequest { Name = "new_colors" });

            var response = (RetrieveOptionSetResponse)service.Execute(
                new RetrieveOptionSetRequest { Name = "new_colors" });
            Assert.Null(response.OptionSetMetadata);
        }

        [Fact]
        public void DeleteOptionSetRequest_NotFound_ThrowsFault()
        {
            var env = new FakeDataverseEnvironment();
            var service = env.CreateOrganizationService();

            Assert.Throws<FaultException<OrganizationServiceFault>>(
                () => service.Execute(new DeleteOptionSetRequest { Name = "nonexistent" }));
        }

        #endregion

        #region RetrieveAllOptionSetsRequest

        [Fact]
        public void RetrieveAllOptionSetsRequest_ReturnsAllGlobalOptionSets()
        {
            var env = new FakeDataverseEnvironment();
            var service = env.CreateOrganizationService();
            service.Execute(new CreateOptionSetRequest
            {
                OptionSet = new OptionSetMetadata { Name = "new_colors" }
            });
            service.Execute(new CreateOptionSetRequest
            {
                OptionSet = new OptionSetMetadata { Name = "new_sizes" }
            });

            var response = (RetrieveAllOptionSetsResponse)service.Execute(
                new RetrieveAllOptionSetsRequest());

            Assert.Equal(2, response.OptionSetMetadata.Length);
        }

        [Fact]
        public void RetrieveAllOptionSetsRequest_Empty_ReturnsEmpty()
        {
            var env = new FakeDataverseEnvironment();
            var service = env.CreateOrganizationService();

            var response = (RetrieveAllOptionSetsResponse)service.Execute(
                new RetrieveAllOptionSetsRequest());

            Assert.Empty(response.OptionSetMetadata);
        }

        #endregion

        #region InsertOptionValueRequest

        [Fact]
        public void InsertOptionValueRequest_AddsToGlobalOptionSet()
        {
            var env = new FakeDataverseEnvironment();
            var service = env.CreateOrganizationService();
            var optionSet = new OptionSetMetadata { Name = "new_colors" };
            optionSet.Options.Add(new OptionMetadata(new Label("Red", 1033), 1));
            service.Execute(new CreateOptionSetRequest { OptionSet = optionSet });

            var response = (InsertOptionValueResponse)service.Execute(
                new InsertOptionValueRequest
                {
                    OptionSetName = "new_colors",
                    Value = 2,
                    Label = new Label("Blue", 1033)
                });

            Assert.Equal(2, response.NewOptionValue);
        }

        [Fact]
        public void InsertOptionValueRequest_WithoutValue_GeneratesValue()
        {
            var env = new FakeDataverseEnvironment();
            var service = env.CreateOrganizationService();

            var response = (InsertOptionValueResponse)service.Execute(
                new InsertOptionValueRequest { OptionSetName = "any" });

            Assert.NotEqual(0, response.NewOptionValue);
        }

        #endregion

        #region InsertStatusValueRequest

        [Fact]
        public void InsertStatusValueRequest_ReturnsNewValue()
        {
            var env = new FakeDataverseEnvironment();
            var service = env.CreateOrganizationService();

            var response = (InsertStatusValueResponse)service.Execute(
                new InsertStatusValueRequest
                {
                    EntityLogicalName = "account",
                    AttributeLogicalName = "statuscode",
                    Value = 100
                });

            Assert.Equal(100, response.NewOptionValue);
        }

        [Fact]
        public void InsertStatusValueRequest_WithoutValue_GeneratesValue()
        {
            var env = new FakeDataverseEnvironment();
            var service = env.CreateOrganizationService();

            var response = (InsertStatusValueResponse)service.Execute(
                new InsertStatusValueRequest
                {
                    EntityLogicalName = "account",
                    AttributeLogicalName = "statuscode"
                });

            Assert.NotEqual(0, response.NewOptionValue);
        }

        #endregion

        #region DeleteOptionValueRequest

        [Fact]
        public void DeleteOptionValueRequest_RemovesFromGlobalOptionSet()
        {
            var env = new FakeDataverseEnvironment();
            var service = env.CreateOrganizationService();
            var optionSet = new OptionSetMetadata { Name = "new_colors" };
            optionSet.Options.Add(new OptionMetadata(new Label("Red", 1033), 1));
            optionSet.Options.Add(new OptionMetadata(new Label("Blue", 1033), 2));
            service.Execute(new CreateOptionSetRequest { OptionSet = optionSet });

            service.Execute(new DeleteOptionValueRequest
            {
                OptionSetName = "new_colors",
                Value = 1
            });

            var response = (RetrieveOptionSetResponse)service.Execute(
                new RetrieveOptionSetRequest { Name = "new_colors" });
            var retrieved = Assert.IsType<OptionSetMetadata>(response.OptionSetMetadata);
            Assert.Single(retrieved.Options);
            Assert.Equal(2, retrieved.Options[0].Value);
        }

        #endregion

        #region UpdateOptionValueRequest

        [Fact]
        public void UpdateOptionValueRequest_UpdatesLabel()
        {
            var env = new FakeDataverseEnvironment();
            var service = env.CreateOrganizationService();
            var optionSet = new OptionSetMetadata { Name = "new_colors" };
            optionSet.Options.Add(new OptionMetadata(new Label("Red", 1033), 1));
            service.Execute(new CreateOptionSetRequest { OptionSet = optionSet });

            service.Execute(new UpdateOptionValueRequest
            {
                OptionSetName = "new_colors",
                Value = 1,
                Label = new Label("Crimson", 1033)
            });

            var response = (RetrieveOptionSetResponse)service.Execute(
                new RetrieveOptionSetRequest { Name = "new_colors" });
            var retrieved = Assert.IsType<OptionSetMetadata>(response.OptionSetMetadata);
            Assert.Single(retrieved.Options);
            Assert.Equal("Crimson", retrieved.Options[0].Label.LocalizedLabels[0].Label);
        }

        #endregion

        #region OrderOptionRequest

        [Fact]
        public void OrderOptionRequest_ReordersOptions()
        {
            var env = new FakeDataverseEnvironment();
            var service = env.CreateOrganizationService();
            var optionSet = new OptionSetMetadata { Name = "new_colors" };
            optionSet.Options.Add(new OptionMetadata(new Label("Red", 1033), 1));
            optionSet.Options.Add(new OptionMetadata(new Label("Blue", 1033), 2));
            optionSet.Options.Add(new OptionMetadata(new Label("Green", 1033), 3));
            service.Execute(new CreateOptionSetRequest { OptionSet = optionSet });

            service.Execute(new OrderOptionRequest
            {
                OptionSetName = "new_colors",
                Values = new[] { 3, 1, 2 }
            });

            var response = (RetrieveOptionSetResponse)service.Execute(
                new RetrieveOptionSetRequest { Name = "new_colors" });
            var retrieved = Assert.IsType<OptionSetMetadata>(response.OptionSetMetadata);
            Assert.Equal(3, retrieved.Options[0].Value);
            Assert.Equal(1, retrieved.Options[1].Value);
            Assert.Equal(2, retrieved.Options[2].Value);
        }

        #endregion

        #region UpdateStateValueRequest

        [Fact]
        public void UpdateStateValueRequest_Succeeds()
        {
            var env = new FakeDataverseEnvironment();
            var service = env.CreateOrganizationService();

            // Should not throw — it's a no-op
            service.Execute(new UpdateStateValueRequest
            {
                EntityLogicalName = "account",
                AttributeLogicalName = "statecode",
                Value = 1
            });
        }

        #endregion

        #region CanBeReferencedRequest

        [Fact]
        public void CanBeReferencedRequest_RegisteredEntity_ReturnsTrue()
        {
            var env = new FakeDataverseEnvironment();
            var service = env.CreateOrganizationService();
            env.MetadataStore.AddEntity("account");

            var response = (CanBeReferencedResponse)service.Execute(
                new CanBeReferencedRequest { EntityName = "account" });

            Assert.True(response.CanBeReferenced);
        }

        [Fact]
        public void CanBeReferencedRequest_UnregisteredEntity_ReturnsFalse()
        {
            var env = new FakeDataverseEnvironment();
            var service = env.CreateOrganizationService();

            var response = (CanBeReferencedResponse)service.Execute(
                new CanBeReferencedRequest { EntityName = "nonexistent" });

            Assert.False(response.CanBeReferenced);
        }

        #endregion

        #region CanBeReferencingRequest

        [Fact]
        public void CanBeReferencingRequest_RegisteredEntity_ReturnsTrue()
        {
            var env = new FakeDataverseEnvironment();
            var service = env.CreateOrganizationService();
            env.MetadataStore.AddEntity("contact");

            var response = (CanBeReferencingResponse)service.Execute(
                new CanBeReferencingRequest { EntityName = "contact" });

            Assert.True(response.CanBeReferencing);
        }

        [Fact]
        public void CanBeReferencingRequest_UnregisteredEntity_ReturnsFalse()
        {
            var env = new FakeDataverseEnvironment();
            var service = env.CreateOrganizationService();

            var response = (CanBeReferencingResponse)service.Execute(
                new CanBeReferencingRequest { EntityName = "nonexistent" });

            Assert.False(response.CanBeReferencing);
        }

        #endregion

        #region CanManyToManyRequest

        [Fact]
        public void CanManyToManyRequest_RegisteredEntity_ReturnsTrue()
        {
            var env = new FakeDataverseEnvironment();
            var service = env.CreateOrganizationService();
            env.MetadataStore.AddEntity("account");

            var response = (CanManyToManyResponse)service.Execute(
                new CanManyToManyRequest { EntityName = "account" });

            Assert.True(response.CanManyToMany);
        }

        [Fact]
        public void CanManyToManyRequest_UnregisteredEntity_ReturnsFalse()
        {
            var env = new FakeDataverseEnvironment();
            var service = env.CreateOrganizationService();

            var response = (CanManyToManyResponse)service.Execute(
                new CanManyToManyRequest { EntityName = "nonexistent" });

            Assert.False(response.CanManyToMany);
        }

        #endregion

        #region GetValidManyToManyRequest

        [Fact]
        public void GetValidManyToManyRequest_ReturnsAllRegisteredEntities()
        {
            var env = new FakeDataverseEnvironment();
            var service = env.CreateOrganizationService();
            env.MetadataStore.AddEntity("account");
            env.MetadataStore.AddEntity("contact");

            var response = (GetValidManyToManyResponse)service.Execute(
                new GetValidManyToManyRequest());

            var entities = (EntityMetadata[])response.Results["EntityMetadata"];
            Assert.Equal(2, entities.Length);
        }

        #endregion

        #region GetValidReferencedEntitiesRequest

        [Fact]
        public void GetValidReferencedEntitiesRequest_ReturnsEntityNames()
        {
            var env = new FakeDataverseEnvironment();
            var service = env.CreateOrganizationService();
            env.MetadataStore.AddEntity("account");
            env.MetadataStore.AddEntity("contact");

            var response = (GetValidReferencedEntitiesResponse)service.Execute(
                new GetValidReferencedEntitiesRequest { ReferencingEntityName = "contact" });

            Assert.Equal(2, response.EntityNames.Length);
            Assert.Contains("account", response.EntityNames);
        }

        #endregion

        #region GetValidReferencingEntitiesRequest

        [Fact]
        public void GetValidReferencingEntitiesRequest_ReturnsEntityNames()
        {
            var env = new FakeDataverseEnvironment();
            var service = env.CreateOrganizationService();
            env.MetadataStore.AddEntity("account");
            env.MetadataStore.AddEntity("contact");

            var response = (GetValidReferencingEntitiesResponse)service.Execute(
                new GetValidReferencingEntitiesRequest { ReferencedEntityName = "account" });

            Assert.Equal(2, response.EntityNames.Length);
        }

        #endregion

        #region CreateCustomerRelationshipsRequest

        [Fact]
        public void CreateCustomerRelationshipsRequest_CreatesRelationships()
        {
            var env = new FakeDataverseEnvironment();
            var service = env.CreateOrganizationService();

            var lookup = new LookupAttributeMetadata { LogicalName = "customerid" };
            var rels = new[]
            {
                new OneToManyRelationshipMetadata
                {
                    SchemaName = "contact_customer_accounts",
                    ReferencedEntity = "account",
                    ReferencedAttribute = "accountid",
                    ReferencingEntity = "contact"
                },
                new OneToManyRelationshipMetadata
                {
                    SchemaName = "contact_customer_contacts",
                    ReferencedEntity = "contact",
                    ReferencedAttribute = "contactid",
                    ReferencingEntity = "contact"
                }
            };

            var response = (CreateCustomerRelationshipsResponse)service.Execute(
                new CreateCustomerRelationshipsRequest
                {
                    Lookup = lookup,
                    OneToManyRelationships = rels
                });

            Assert.NotEqual(Guid.Empty, response.AttributeId);
            Assert.Equal(2, response.RelationshipIds.Length);

            // Verify first relationship was created
            var rel1 = (RetrieveRelationshipResponse)service.Execute(
                new RetrieveRelationshipRequest { Name = "contact_customer_accounts" });
            Assert.IsType<OneToManyRelationshipMetadata>(rel1.RelationshipMetadata);
        }

        #endregion

        #region RetrieveMetadataChangesRequest

        [Fact]
        public void RetrieveMetadataChangesRequest_ReturnsEntitiesAndTimestamp()
        {
            var env = new FakeDataverseEnvironment();
            var service = env.CreateOrganizationService();
            env.MetadataStore.AddEntity("account");
            env.MetadataStore.AddEntity("contact");

            var response = (RetrieveMetadataChangesResponse)service.Execute(
                new RetrieveMetadataChangesRequest());

            Assert.Equal(2, response.EntityMetadata.Count);
            Assert.NotNull(response.ServerVersionStamp);
        }

        #endregion

        #region RetrieveTimestampRequest

        [Fact]
        public void RetrieveTimestampRequest_ReturnsTimestamp()
        {
            var env = new FakeDataverseEnvironment();
            var service = env.CreateOrganizationService();

            var response = (RetrieveTimestampResponse)service.Execute(
                new RetrieveTimestampRequest());

            Assert.NotNull(response.Timestamp);
        }

        [Fact]
        public void RetrieveTimestampRequest_IncreasesAfterMetadataChange()
        {
            var env = new FakeDataverseEnvironment();
            var service = env.CreateOrganizationService();

            var before = (RetrieveTimestampResponse)service.Execute(
                new RetrieveTimestampRequest());

            var entityMetadata = new EntityMetadata();
            entityMetadata.LogicalName = "new_custom";
            service.Execute(new CreateEntityRequest { Entity = entityMetadata });

            var after = (RetrieveTimestampResponse)service.Execute(
                new RetrieveTimestampRequest());

            Assert.NotEqual(before.Timestamp, after.Timestamp);
        }

        #endregion

        #region RetrieveAllManagedPropertiesRequest

        [Fact]
        public void RetrieveAllManagedPropertiesRequest_ReturnsResponse()
        {
            var env = new FakeDataverseEnvironment();
            var service = env.CreateOrganizationService();

            var response = (RetrieveAllManagedPropertiesResponse)service.Execute(
                new RetrieveAllManagedPropertiesRequest());

            Assert.NotNull(response);
        }

        #endregion

        #region RetrieveManagedPropertyRequest

        [Fact]
        public void RetrieveManagedPropertyRequest_ReturnsResponse()
        {
            var env = new FakeDataverseEnvironment();
            var service = env.CreateOrganizationService();

            var response = (RetrieveManagedPropertyResponse)service.Execute(
                new RetrieveManagedPropertyRequest { MetadataId = Guid.NewGuid() });

            Assert.NotNull(response);
        }

        #endregion

        #region IsDataEncryptionActiveRequest

        [Fact]
        public void IsDataEncryptionActiveRequest_ReturnsFalse()
        {
            var env = new FakeDataverseEnvironment();
            var service = env.CreateOrganizationService();

            var response = (IsDataEncryptionActiveResponse)service.Execute(
                new IsDataEncryptionActiveRequest());

            Assert.False(response.IsActive);
        }

        #endregion

        #region RetrieveDataEncryptionKeyRequest

        [Fact]
        public void RetrieveDataEncryptionKeyRequest_ReturnsResponse()
        {
            var env = new FakeDataverseEnvironment();
            var service = env.CreateOrganizationService();

            var response = (RetrieveDataEncryptionKeyResponse)service.Execute(
                new RetrieveDataEncryptionKeyRequest());

            Assert.NotNull(response);
        }

        #endregion

        #region SetDataEncryptionKeyRequest

        [Fact]
        public void SetDataEncryptionKeyRequest_Succeeds()
        {
            var env = new FakeDataverseEnvironment();
            var service = env.CreateOrganizationService();

            service.Execute(new SetDataEncryptionKeyRequest()); // Should not throw
        }

        #endregion

        #region ConvertDateAndTimeBehaviorRequest

        [Fact]
        public void ConvertDateAndTimeBehaviorRequest_ReturnsJobId()
        {
            var env = new FakeDataverseEnvironment();
            var service = env.CreateOrganizationService();

            var response = (ConvertDateAndTimeBehaviorResponse)service.Execute(
                new ConvertDateAndTimeBehaviorRequest());

            Assert.NotEqual(Guid.Empty, response.JobId);
        }

        #endregion

        #region ExecuteAsyncRequest

        [Fact]
        public void ExecuteAsyncRequest_ExecutesInnerRequest()
        {
            var env = new FakeDataverseEnvironment();
            var service = env.CreateOrganizationService();

            var innerRequest = new CreateRequest
            {
                Target = new Entity("account") { ["name"] = "Contoso" }
            };

            var response = (ExecuteAsyncResponse)service.Execute(
                new ExecuteAsyncRequest { Request = innerRequest });

            Assert.NotEqual(Guid.Empty, response.AsyncJobId);
            var asyncOperation = service.Retrieve("asyncoperation", response.AsyncJobId, new Microsoft.Xrm.Sdk.Query.ColumnSet("name", "message"));
            Assert.Equal("Execute Async", asyncOperation.GetAttributeValue<string>("name"));
            Assert.Equal("Create", asyncOperation.GetAttributeValue<string>("message"));

            // Verify the inner request was actually executed
            var accounts = service.RetrieveMultiple(
                new Microsoft.Xrm.Sdk.Query.QueryExpression("account"));
            Assert.Single(accounts.Entities);
        }

        #endregion

        #region RetrieveEntityChangesRequest

        [Fact]
        public void RetrieveEntityChangesRequest_ReturnsResponse()
        {
            var env = new FakeDataverseEnvironment();
            var service = env.CreateOrganizationService();

            var response = (RetrieveEntityChangesResponse)service.Execute(
                new RetrieveEntityChangesRequest { EntityName = "account" });

            Assert.NotNull(response);
        }

        #endregion

        #region CreateAsyncJobToRevokeInheritedAccessRequest

        [Fact]
        public void CreateAsyncJobToRevokeInheritedAccessRequest_ReturnsJobId()
        {
            var env = new FakeDataverseEnvironment();
            var service = env.CreateOrganizationService();

            var response = (CreateAsyncJobToRevokeInheritedAccessResponse)service.Execute(
                new CreateAsyncJobToRevokeInheritedAccessRequest
                {
                    RelationshipSchema = "account_contacts"
                });

            var asyncJobId = (Guid)response.Results["AsyncJobId"];
            Assert.NotEqual(Guid.Empty, asyncJobId);
        }

        #endregion
    }
}
