using System;
using System.IO;
using System.Linq;
using Fake4Dataverse.Metadata;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Metadata;
using Xunit;

namespace Fake4Dataverse.Tests
{
    public class MetadataXmlTests
    {
        // ── XML Round-Trip Helpers ───────────────────────────────────────────

        private static string SerializeSingle(EntityMetadata em) =>
            EntityMetadataXmlLoader.SerializeToXml(em);

        private static string SerializeArray(EntityMetadata[] ems) =>
            EntityMetadataXmlLoader.SerializeToXml(ems);

        // ── LoadFromXml – single entity ──────────────────────────────────────

        [Fact]
        public void LoadFromXml_SingleEntity_RegistersLogicalName()
        {
            var env = new FakeDataverseEnvironment();
            var em = new EntityMetadata { LogicalName = "account" };
            em.SchemaName = "Account";

            env.LoadMetadataFromXml(SerializeSingle(em));

            var info = env.MetadataStore.GetEntityMetadataInfo("account");
            Assert.NotNull(info);
            Assert.Equal("account", info.LogicalName);
        }

        [Fact]
        public void LoadFromXml_SingleEntity_SchemaAndPrimaryFields()
        {
            var env = new FakeDataverseEnvironment();
            var em = BuildEntityMetadata("account", "Account", "accountid", "name", objectTypeCode: 1);

            env.LoadMetadataFromXml(SerializeSingle(em));

            var info = env.MetadataStore.GetEntityMetadataInfo("account");
            Assert.NotNull(info);
            Assert.Equal("Account", info.SchemaName);
            Assert.Equal("accountid", info.PrimaryIdAttribute);
            Assert.Equal("name", info.PrimaryNameAttribute);
            Assert.Equal(1, info.ObjectTypeCode);
        }

        [Fact]
        public void LoadFromXml_SingleEntity_AttributesImported()
        {
            var env = new FakeDataverseEnvironment();
            var em = BuildEntityMetadata("account", "Account", "accountid", "name");
            em.SetAttributes(new AttributeMetadata[]
            {
                new StringAttributeMetadata { LogicalName = "name", MaxLength = 160,
                    RequiredLevel = new AttributeRequiredLevelManagedProperty(AttributeRequiredLevel.None) },
                new IntegerAttributeMetadata { LogicalName = "numberofemployees", MinValue = 0, MaxValue = 1000000,
                    RequiredLevel = new AttributeRequiredLevelManagedProperty(AttributeRequiredLevel.None) },
            });

            env.LoadMetadataFromXml(SerializeSingle(em));

            var info = env.MetadataStore.GetEntityMetadataInfo("account");
            Assert.NotNull(info);
            Assert.True(info.Attributes.ContainsKey("name"));
            Assert.True(info.Attributes.ContainsKey("numberofemployees"));
            Assert.Equal(AttributeTypeCode.String, info.Attributes["name"].AttributeType);
            Assert.Equal(160, info.Attributes["name"].MaxLength);
            Assert.Equal(AttributeTypeCode.Integer, info.Attributes["numberofemployees"].AttributeType);
            Assert.Equal(0, info.Attributes["numberofemployees"].MinValue);
            Assert.Equal(1000000, info.Attributes["numberofemployees"].MaxValue);
        }

        // ── LoadFromXml – array of entities ─────────────────────────────────

        [Fact]
        public void LoadFromXml_ArrayOfEntities_AllEntitiesRegistered()
        {
            var env = new FakeDataverseEnvironment();
            var entities = new[]
            {
                BuildEntityMetadata("account", "Account", "accountid", "name"),
                BuildEntityMetadata("contact", "Contact", "contactid", "fullname"),
            };

            env.LoadMetadataFromXml(SerializeArray(entities));

            Assert.NotNull(env.MetadataStore.GetEntityMetadataInfo("account"));
            Assert.NotNull(env.MetadataStore.GetEntityMetadataInfo("contact"));
        }

        [Fact]
        public void LoadFromXml_ArrayOfEntities_MergesWithExisting()
        {
            var env = new FakeDataverseEnvironment();
            env.MetadataStore.AddEntity("account").WithStringAttribute("name");

            var em = BuildEntityMetadata("account", "Account", "accountid", "name");
            em.SetAttributes(new AttributeMetadata[]
            {
                new StringAttributeMetadata { LogicalName = "telephone1",
                    RequiredLevel = new AttributeRequiredLevelManagedProperty(AttributeRequiredLevel.None) },
            });

            env.LoadMetadataFromXml(SerializeArray(new[] { em }));

            var info = env.MetadataStore.GetEntityMetadataInfo("account");
            Assert.NotNull(info);
            // Original attribute from builder is still present.
            Assert.True(info.Attributes.ContainsKey("name"));
            // New attribute from XML is also present.
            Assert.True(info.Attributes.ContainsKey("telephone1"));
        }

        // ── LoadFromXmlFile ──────────────────────────────────────────────────

        [Fact]
        public void LoadFromXmlFile_SingleEntity_RegistersMetadata()
        {
            var env = new FakeDataverseEnvironment();
            var em = BuildEntityMetadata("webresource", "WebResource", "webresourceid", "name");
            var xml = SerializeSingle(em);

            var path = Path.GetTempFileName();
            try
            {
                File.WriteAllText(path, xml);
                env.LoadMetadataFromXmlFile(path);

                Assert.NotNull(env.MetadataStore.GetEntityMetadataInfo("webresource"));
            }
            finally
            {
                File.Delete(path);
            }
        }

        // ── Invalid XML ──────────────────────────────────────────────────────

        [Fact]
        public void LoadFromXml_NullXml_Throws()
        {
            var env = new FakeDataverseEnvironment();
            Assert.Throws<ArgumentNullException>(() => env.LoadMetadataFromXml(null!));
        }

        [Fact]
        public void LoadFromXml_UnknownRootElement_Throws()
        {
            var env = new FakeDataverseEnvironment();
            var xml = "<UnknownElement><Something/></UnknownElement>";
            Assert.Throws<ArgumentException>(() => env.LoadMetadataFromXml(xml));
        }

        // ── Auto Solution-Awareness via componentstate attribute ─────────────

        [Fact]
        public void IsSolutionAwareEntity_EntityWithComponentstateInMetadata_ReturnsTrue()
        {
            var env = new FakeDataverseEnvironment();
            env.MetadataStore.AddEntity("webresource")
                .WithAttribute("componentstate", AttributeTypeCode.Picklist);

            Assert.True(env.IsSolutionAwareEntity("webresource"));
        }

        [Fact]
        public void IsSolutionAwareEntity_EntityWithoutComponentstateInMetadata_ReturnsFalse()
        {
            var env = new FakeDataverseEnvironment();
            env.MetadataStore.AddEntity("account").WithStringAttribute("name");

            Assert.False(env.IsSolutionAwareEntity("account"));
        }

        [Fact]
        public void IsSolutionAwareEntity_ExplicitRegistration_StillReturnsTrue()
        {
            var env = new FakeDataverseEnvironment();
            // No metadata at all, but explicitly registered.
            env.RegisterSolutionAwareEntity("webresource");

            Assert.True(env.IsSolutionAwareEntity("webresource"));
        }

        [Fact]
        public void Create_EntityWithComponentstateMetadata_GoesToUnpublishedStore()
        {
            var env = new FakeDataverseEnvironment();
            var service = env.CreateOrganizationService();

            // Register metadata with componentstate — no explicit RegisterSolutionAwareEntity call.
            env.MetadataStore.AddEntity("webresource")
                .WithAttribute("componentstate", AttributeTypeCode.Picklist)
                .WithStringAttribute("name");

            var id = service.Create(new Entity("webresource") { ["name"] = "test.js" });

            // Should NOT appear in regular Retrieve (it's unpublished).
            Assert.Throws<System.ServiceModel.FaultException<Microsoft.Xrm.Sdk.OrganizationServiceFault>>(
                () => service.Retrieve("webresource", id, new Microsoft.Xrm.Sdk.Query.ColumnSet(true)));

            // Should appear in RetrieveUnpublished.
            var retrieveRequest = new OrganizationRequest("RetrieveUnpublished")
            {
                ["Target"] = new EntityReference("webresource", id),
                ["ColumnSet"] = new Microsoft.Xrm.Sdk.Query.ColumnSet(true),
            };
            var response = service.Execute(retrieveRequest);
            var retrieved = response.Results["Entity"] as Entity;

            Assert.NotNull(retrieved);
            Assert.Equal("test.js", retrieved.GetAttributeValue<string>("name"));
        }

        [Fact]
        public void LoadFromXml_EntityWithComponentstateAttribute_AutoSolutionAware()
        {
            var env = new FakeDataverseEnvironment();
            var service = env.CreateOrganizationService();

            var em = BuildEntityMetadata("webresource", "WebResource", "webresourceid", "name");
            em.SetAttributes(new AttributeMetadata[]
            {
                new StringAttributeMetadata { LogicalName = "name",
                    RequiredLevel = new AttributeRequiredLevelManagedProperty(AttributeRequiredLevel.None) },
                new PicklistAttributeMetadata { LogicalName = "componentstate",
                    RequiredLevel = new AttributeRequiredLevelManagedProperty(AttributeRequiredLevel.None) },
            });

            env.LoadMetadataFromXml(SerializeSingle(em));

            Assert.True(env.IsSolutionAwareEntity("webresource"));

            var id = service.Create(new Entity("webresource") { ["name"] = "app.js" });

            // Unpublished; not visible via normal Retrieve.
            Assert.Throws<System.ServiceModel.FaultException<Microsoft.Xrm.Sdk.OrganizationServiceFault>>(
                () => service.Retrieve("webresource", id, new Microsoft.Xrm.Sdk.Query.ColumnSet(true)));
        }

        // ── Attribute type coverage ──────────────────────────────────────────

        [Fact]
        public void LoadFromXml_AllCommonAttributeTypes_ImportedCorrectly()
        {
            var env = new FakeDataverseEnvironment();
            var em = BuildEntityMetadata("custom_entity", "CustomEntity", "custom_entityid", "name");
            em.SetAttributes(new AttributeMetadata[]
            {
                new StringAttributeMetadata { LogicalName = "string_field", MaxLength = 100,
                    RequiredLevel = new AttributeRequiredLevelManagedProperty(AttributeRequiredLevel.None) },
                new MemoAttributeMetadata { LogicalName = "memo_field", MaxLength = 2000,
                    RequiredLevel = new AttributeRequiredLevelManagedProperty(AttributeRequiredLevel.None) },
                new IntegerAttributeMetadata { LogicalName = "int_field", MinValue = -100, MaxValue = 100,
                    RequiredLevel = new AttributeRequiredLevelManagedProperty(AttributeRequiredLevel.None) },
                new DecimalAttributeMetadata { LogicalName = "decimal_field", MinValue = 0m, MaxValue = 999m,
                    RequiredLevel = new AttributeRequiredLevelManagedProperty(AttributeRequiredLevel.None) },
                new DoubleAttributeMetadata { LogicalName = "double_field", MinValue = 0.0, MaxValue = 1.0,
                    RequiredLevel = new AttributeRequiredLevelManagedProperty(AttributeRequiredLevel.None) },
                new MoneyAttributeMetadata { LogicalName = "money_field", MinValue = 0.0, MaxValue = 9999.99,
                    RequiredLevel = new AttributeRequiredLevelManagedProperty(AttributeRequiredLevel.None) },
                new BooleanAttributeMetadata { LogicalName = "bool_field",
                    RequiredLevel = new AttributeRequiredLevelManagedProperty(AttributeRequiredLevel.None) },
                new DateTimeAttributeMetadata { LogicalName = "date_field",
                    RequiredLevel = new AttributeRequiredLevelManagedProperty(AttributeRequiredLevel.None) },
                new LookupAttributeMetadata { LogicalName = "lookup_field", Targets = new[] { "account" },
                    RequiredLevel = new AttributeRequiredLevelManagedProperty(AttributeRequiredLevel.None) },
            });

            env.LoadMetadataFromXml(SerializeSingle(em));

            var info = env.MetadataStore.GetEntityMetadataInfo("custom_entity");
            Assert.NotNull(info);

            Assert.Equal(AttributeTypeCode.String, info.Attributes["string_field"].AttributeType);
            Assert.Equal(100, info.Attributes["string_field"].MaxLength);

            Assert.Equal(AttributeTypeCode.Memo, info.Attributes["memo_field"].AttributeType);
            Assert.Equal(2000, info.Attributes["memo_field"].MaxLength);

            Assert.Equal(AttributeTypeCode.Integer, info.Attributes["int_field"].AttributeType);
            Assert.Equal(-100, info.Attributes["int_field"].MinValue);

            Assert.Equal(AttributeTypeCode.Decimal, info.Attributes["decimal_field"].AttributeType);

            Assert.Equal(AttributeTypeCode.Double, info.Attributes["double_field"].AttributeType);

            Assert.Equal(AttributeTypeCode.Money, info.Attributes["money_field"].AttributeType);

            Assert.Equal(AttributeTypeCode.Boolean, info.Attributes["bool_field"].AttributeType);

            Assert.Equal(AttributeTypeCode.DateTime, info.Attributes["date_field"].AttributeType);

            Assert.Equal(AttributeTypeCode.Lookup, info.Attributes["lookup_field"].AttributeType);
            Assert.NotNull(info.Attributes["lookup_field"].ValidTargetEntityTypes);
            Assert.Contains("account", info.Attributes["lookup_field"].ValidTargetEntityTypes!);
        }

        // ── Helpers ──────────────────────────────────────────────────────────

        private static EntityMetadata BuildEntityMetadata(
            string logicalName,
            string schemaName,
            string primaryIdAttribute,
            string primaryNameAttribute,
            int? objectTypeCode = null)
        {
            var em = new EntityMetadata();
            em.LogicalName = logicalName;
            em.SchemaName = schemaName;
            SetReadOnlyProperty(em, "PrimaryIdAttribute", primaryIdAttribute);
            SetReadOnlyProperty(em, "PrimaryNameAttribute", primaryNameAttribute);
            if (objectTypeCode.HasValue)
                SetReadOnlyProperty(em, "ObjectTypeCode", objectTypeCode.Value);
            return em;
        }

        private static void SetReadOnlyProperty(object target, string propertyName, object value)
        {
            var prop = target.GetType().GetProperty(
                propertyName,
                System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance);
            if (prop != null && prop.CanWrite)
            {
                prop.SetValue(target, value);
                return;
            }
            // Try non-public setter.
            prop = target.GetType().GetProperty(
                propertyName,
                System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.NonPublic
                | System.Reflection.BindingFlags.Instance);
            prop?.SetValue(target, value);
        }
    }

    // Extension method to make test setup more convenient.
    internal static class EntityMetadataTestExtensions
    {
        internal static void SetAttributes(this EntityMetadata em, AttributeMetadata[] attributes)
        {
            var prop = typeof(EntityMetadata).GetProperty(
                "Attributes",
                System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance);
            if (prop != null && prop.CanWrite)
            {
                prop.SetValue(em, attributes);
                return;
            }
            prop = typeof(EntityMetadata).GetProperty(
                "Attributes",
                System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.NonPublic
                | System.Reflection.BindingFlags.Instance);
            prop?.SetValue(em, attributes);
        }
    }
}
