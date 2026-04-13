using System;
using System.Reflection;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Client;
using Microsoft.Xrm.Sdk.Metadata;

namespace Fake4Dataverse.EarlyBound
{
    /// <summary>
    /// Extension methods for registering early-bound entity metadata with <see cref="FakeDataverseEnvironment"/>.
    /// </summary>
    public static class EarlyBoundMetadataExtensions
    {
        /// <summary>
        /// Scans the specified assembly for classes decorated with <see cref="EntityLogicalNameAttribute"/>
        /// and registers entity metadata for each one, including attribute logical names from
        /// <see cref="AttributeLogicalNameAttribute"/> properties.
        /// </summary>
        public static void RegisterEarlyBoundEntities(this FakeDataverseEnvironment environment, Assembly assembly)
        {
            if (environment == null) throw new ArgumentNullException(nameof(environment));
            if (assembly == null) throw new ArgumentNullException(nameof(assembly));

            foreach (var type in assembly.GetTypes())
            {
                RegisterEarlyBoundType(environment, type, throwOnMissingEntityAttribute: false);
            }
        }

        /// <summary>
        /// Registers metadata for a single early-bound entity type.
        /// </summary>
        public static void RegisterEarlyBoundEntity<TEntity>(this FakeDataverseEnvironment environment) where TEntity : Entity
        {
            if (environment == null) throw new ArgumentNullException(nameof(environment));

            RegisterEarlyBoundType(environment, typeof(TEntity), throwOnMissingEntityAttribute: true);
        }

        private static void RegisterEarlyBoundType(FakeDataverseEnvironment environment, Type type, bool throwOnMissingEntityAttribute)
        {
            if (!typeof(Entity).IsAssignableFrom(type) || type.IsAbstract)
                return;

            var entityAttr = type.GetCustomAttribute<EntityLogicalNameAttribute>();
            if (entityAttr == null)
            {
                if (throwOnMissingEntityAttribute)
                    throw new ArgumentException($"Type {type.Name} is not decorated with [EntityLogicalName].");
                return;
            }

            var entityName = entityAttr.LogicalName;
            var properties = type.GetProperties(BindingFlags.Public | BindingFlags.Instance);
            string? primaryIdAttribute = null;
            string? primaryNameAttribute = null;

            var builder = environment.MetadataStore.AddEntity(entityName);

            foreach (var prop in properties)
            {
                var logicalNameAttribute = prop.GetCustomAttribute<AttributeLogicalNameAttribute>();
                if (logicalNameAttribute == null)
                    continue;

                var logicalName = logicalNameAttribute.LogicalName;
                if (string.IsNullOrWhiteSpace(logicalName))
                    continue;

                if (string.Equals(logicalName, entityName + "id", StringComparison.OrdinalIgnoreCase))
                    primaryIdAttribute = logicalName;

                if (primaryNameAttribute == null && IsPrimaryNameCandidate(prop, logicalName, entityName))
                    primaryNameAttribute = logicalName;

                var attributeType = TryMapPropertyTypeToAttributeType(prop.PropertyType, logicalName);
                if (attributeType.HasValue)
                    builder.WithAttribute(logicalName, attributeType.Value);
            }

            builder.WithPrimaryIdAttribute(primaryIdAttribute ?? entityName + "id");
            if (primaryNameAttribute != null)
                builder.WithPrimaryNameAttribute(primaryNameAttribute);
        }

        private static bool IsPrimaryNameCandidate(PropertyInfo property, string logicalName, string entityName)
        {
            return string.Equals(property.Name, "Name", StringComparison.Ordinal)
                || string.Equals(logicalName, "name", StringComparison.OrdinalIgnoreCase)
                || string.Equals(logicalName, entityName + "name", StringComparison.OrdinalIgnoreCase)
                || string.Equals(logicalName, "fullname", StringComparison.OrdinalIgnoreCase);
        }

        private static AttributeTypeCode? TryMapPropertyTypeToAttributeType(Type propertyType, string logicalName)
        {
            var type = Nullable.GetUnderlyingType(propertyType) ?? propertyType;

            if (type.IsEnum)
                return AttributeTypeCode.Picklist;

            if (type == typeof(string)) return AttributeTypeCode.String;
            if (type == typeof(int) || type == typeof(short)) return AttributeTypeCode.Integer;
            if (type == typeof(long)) return AttributeTypeCode.BigInt;
            if (type == typeof(decimal)) return AttributeTypeCode.Decimal;
            if (type == typeof(double) || type == typeof(float)) return AttributeTypeCode.Double;
            if (type == typeof(bool)) return AttributeTypeCode.Boolean;
            if (type == typeof(DateTime)) return AttributeTypeCode.DateTime;
            if (type == typeof(Guid)) return AttributeTypeCode.Uniqueidentifier;
            if (type == typeof(Money)) return AttributeTypeCode.Money;
            if (type == typeof(OptionSetValue)) return AttributeTypeCode.Picklist;
            if (type == typeof(EntityReference)) return InferEntityReferenceType(logicalName);
            if (type == typeof(EntityCollection) || type == typeof(EntityReferenceCollection)) return AttributeTypeCode.PartyList;
            if (type == typeof(byte[])) return AttributeTypeCode.Virtual;

            return null;
        }

        private static AttributeTypeCode InferEntityReferenceType(string logicalName)
        {
            if (logicalName.EndsWith("ownerid", StringComparison.OrdinalIgnoreCase))
                return AttributeTypeCode.Owner;

            if (logicalName.IndexOf("customer", StringComparison.OrdinalIgnoreCase) >= 0)
                return AttributeTypeCode.Customer;

            return AttributeTypeCode.Lookup;
        }
    }
}
