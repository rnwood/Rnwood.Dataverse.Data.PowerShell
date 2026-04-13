using System;
using System.ServiceModel;
using Fake4Dataverse.EarlyBound;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Client;
using Microsoft.Xrm.Sdk.Messages;
using Microsoft.Xrm.Sdk.Metadata;
using Xunit;

namespace Fake4Dataverse.Tests
{
    [EntityLogicalName("eb_account")]
    public sealed class EarlyBoundMetadataAccount : Entity
    {
        public const string EntityLogicalName = "eb_account";

        public EarlyBoundMetadataAccount() : base(EntityLogicalName)
        {
        }

        [AttributeLogicalName("eb_accountid")]
        public Guid? EarlyBoundMetadataAccountId { get; set; }

        [AttributeLogicalName("name")]
        public string? Name { get; set; }

        [AttributeLogicalName("revenue")]
        public Money? Revenue { get; set; }

        [AttributeLogicalName("numberofemployees")]
        public int? NumberOfEmployees { get; set; }

        [AttributeLogicalName("creditlimit")]
        public decimal? CreditLimit { get; set; }

        [AttributeLogicalName("customscore")]
        public double? CustomScore { get; set; }

        [AttributeLogicalName("isprivate")]
        public bool? IsPrivate { get; set; }

        [AttributeLogicalName("lastusedon")]
        public DateTime? LastUsedOn { get; set; }

        [AttributeLogicalName("externalid")]
        public Guid? ExternalId { get; set; }

        [AttributeLogicalName("ownerid")]
        public EntityReference? OwnerId { get; set; }

        [AttributeLogicalName("parentcustomerid")]
        public EntityReference? ParentCustomerId { get; set; }

        [AttributeLogicalName("primarycontactid")]
        public EntityReference? PrimaryContactId { get; set; }

        [AttributeLogicalName("statuscode")]
        public EarlyBoundStatusCode? StatusCode { get; set; }

        [AttributeLogicalName("eb_bigcounter")]
        public long? BigCounter { get; set; }

        [AttributeLogicalName("to")]
        public EntityCollection? To { get; set; }

        [AttributeLogicalName("entityimage")]
        public byte[]? EntityImage { get; set; }
    }

    [EntityLogicalName("eb_contact")]
    public sealed class EarlyBoundMetadataContact : Entity
    {
        public const string EntityLogicalName = "eb_contact";

        public EarlyBoundMetadataContact() : base(EntityLogicalName)
        {
        }

        [AttributeLogicalName("eb_contactid")]
        public Guid? EarlyBoundMetadataContactId { get; set; }

        [AttributeLogicalName("fullname")]
        public string? FullName { get; set; }
    }

    [EntityLogicalName("eb_note")]
    public sealed class EarlyBoundWithoutPrimaryIdProperty : Entity
    {
        public const string EntityLogicalName = "eb_note";

        public EarlyBoundWithoutPrimaryIdProperty() : base(EntityLogicalName)
        {
        }

        [AttributeLogicalName("subject")]
        public string? Subject { get; set; }
    }

    [EntityLogicalName("eb_ignored")]
    public sealed class DecoratedNonEntityType
    {
        [AttributeLogicalName("name")]
        public string? Name { get; set; }
    }

    public sealed class MissingEntityLogicalNameTestEntity : Entity
    {
        public MissingEntityLogicalNameTestEntity() : base("missingentitylogicalname")
        {
        }

        [AttributeLogicalName("name")]
        public string? Name { get; set; }
    }

    public enum EarlyBoundStatusCode
    {
        Active = 1,
        Inactive = 2
    }

    public class EarlyBoundMetadataExtensionsTests
    {
        [Fact]
        public void RegisterEarlyBoundEntity_RegistersPrimaryIdNameAndAttributes()
        {
            var env = new FakeDataverseEnvironment();
            var service = env.CreateOrganizationService();

            env.RegisterEarlyBoundEntity<EarlyBoundMetadataAccount>();

            var metadata = RetrieveEntityMetadata(service, EarlyBoundMetadataAccount.EntityLogicalName);

            Assert.Equal("eb_accountid", metadata.PrimaryIdAttribute);
            Assert.Equal("name", metadata.PrimaryNameAttribute);
            Assert.Contains(metadata.Attributes, a => a.LogicalName == "name");
            Assert.Contains(metadata.Attributes, a => a.LogicalName == "revenue");
            Assert.Contains(metadata.Attributes, a => a.LogicalName == "primarycontactid");
            Assert.Contains(metadata.Attributes, a => a.LogicalName == "statuscode");
        }

        [Fact]
        public void RegisterEarlyBoundEntity_MapsCommonClrTypesToMetadataTypes()
        {
            var env = new FakeDataverseEnvironment();
            var service = env.CreateOrganizationService();

            env.RegisterEarlyBoundEntity<EarlyBoundMetadataAccount>();

            Assert.IsType<StringAttributeMetadata>(RetrieveAttributeMetadata(service, "eb_account", "name"));
            Assert.IsType<MoneyAttributeMetadata>(RetrieveAttributeMetadata(service, "eb_account", "revenue"));
            Assert.IsType<IntegerAttributeMetadata>(RetrieveAttributeMetadata(service, "eb_account", "numberofemployees"));
            Assert.IsType<DecimalAttributeMetadata>(RetrieveAttributeMetadata(service, "eb_account", "creditlimit"));
            Assert.IsType<DoubleAttributeMetadata>(RetrieveAttributeMetadata(service, "eb_account", "customscore"));
            Assert.IsType<BooleanAttributeMetadata>(RetrieveAttributeMetadata(service, "eb_account", "isprivate"));
            Assert.IsType<DateTimeAttributeMetadata>(RetrieveAttributeMetadata(service, "eb_account", "lastusedon"));

            var uniqueIdentifier = RetrieveAttributeMetadata(service, "eb_account", "externalid");
            Assert.Equal(AttributeTypeCode.Uniqueidentifier, uniqueIdentifier.AttributeType);

            Assert.IsType<LookupAttributeMetadata>(RetrieveAttributeMetadata(service, "eb_account", "ownerid"));
            Assert.IsType<LookupAttributeMetadata>(RetrieveAttributeMetadata(service, "eb_account", "parentcustomerid"));
            Assert.IsType<LookupAttributeMetadata>(RetrieveAttributeMetadata(service, "eb_account", "primarycontactid"));
            Assert.IsType<PicklistAttributeMetadata>(RetrieveAttributeMetadata(service, "eb_account", "statuscode"));

            var bigInt = RetrieveAttributeMetadata(service, "eb_account", "eb_bigcounter");
            Assert.Equal(AttributeTypeCode.BigInt, bigInt.AttributeType);

            var partyList = RetrieveAttributeMetadata(service, "eb_account", "to");
            Assert.Equal(AttributeTypeCode.PartyList, partyList.AttributeType);

            var virtualType = RetrieveAttributeMetadata(service, "eb_account", "entityimage");
            Assert.Equal(AttributeTypeCode.Virtual, virtualType.AttributeType);
        }

        [Fact]
        public void RegisterEarlyBoundEntities_RegistersAllEntityTypesInAssembly()
        {
            var env = new FakeDataverseEnvironment();
            var service = env.CreateOrganizationService();

            env.RegisterEarlyBoundEntities(typeof(EarlyBoundMetadataExtensionsTests).Assembly);

            var accountMetadata = RetrieveEntityMetadata(service, EarlyBoundMetadataAccount.EntityLogicalName);
            var contactMetadata = RetrieveEntityMetadata(service, EarlyBoundMetadataContact.EntityLogicalName);

            Assert.Equal("eb_accountid", accountMetadata.PrimaryIdAttribute);
            Assert.Equal("eb_contactid", contactMetadata.PrimaryIdAttribute);
            Assert.Equal("name", accountMetadata.PrimaryNameAttribute);
            Assert.Equal("fullname", contactMetadata.PrimaryNameAttribute);
        }

        [Fact]
        public void RegisterEarlyBoundEntities_IgnoresDecoratedTypesThatDoNotInheritEntity()
        {
            var env = new FakeDataverseEnvironment();
            var service = env.CreateOrganizationService();

            env.RegisterEarlyBoundEntities(typeof(EarlyBoundMetadataExtensionsTests).Assembly);

            Assert.Throws<FaultException<OrganizationServiceFault>>(() =>
                service.Execute(new RetrieveEntityRequest { LogicalName = "eb_ignored" }));
        }

        [Fact]
        public void RegisterEarlyBoundEntity_MissingEntityLogicalName_ThrowsArgumentException()
        {
            var env = new FakeDataverseEnvironment();
            var service = env.CreateOrganizationService();

            Assert.Throws<ArgumentException>(() => env.RegisterEarlyBoundEntity<MissingEntityLogicalNameTestEntity>());
        }

        [Fact]
        public void RegisterEarlyBoundEntity_UsesFallbackPrimaryIdWhenMissingSpecificIdProperty()
        {
            var env = new FakeDataverseEnvironment();
            var service = env.CreateOrganizationService();

            env.RegisterEarlyBoundEntity<EarlyBoundWithoutPrimaryIdProperty>();

            var metadata = RetrieveEntityMetadata(service, EarlyBoundWithoutPrimaryIdProperty.EntityLogicalName);
            Assert.Equal("eb_noteid", metadata.PrimaryIdAttribute);
        }

        private static EntityMetadata RetrieveEntityMetadata(FakeOrganizationService service, string entityLogicalName)
        {
            var response = (RetrieveEntityResponse)service.Execute(new RetrieveEntityRequest
            {
                LogicalName = entityLogicalName
            });

            return response.EntityMetadata;
        }

        private static AttributeMetadata RetrieveAttributeMetadata(FakeOrganizationService service, string entityLogicalName, string attributeLogicalName)
        {
            var response = (RetrieveAttributeResponse)service.Execute(new RetrieveAttributeRequest
            {
                EntityLogicalName = entityLogicalName,
                LogicalName = attributeLogicalName
            });

            return response.AttributeMetadata;
        }
    }
}
