using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Xml.Linq;
using Microsoft.Xrm.Sdk.Metadata;

namespace Fake4Dataverse.Metadata
{
    /// <summary>
    /// Parses DataContract-serialized <see cref="EntityMetadata"/> XML produced by the
    /// Dataverse SDK (e.g. from <c>RetrieveEntityRequest</c> / <c>RetrieveAllEntitiesRequest</c>
    /// responses serialized with <c>DataContractSerializer</c>).
    /// </summary>
    internal static class EntityMetadataXmlLoader
    {
        // Discover all concrete AttributeMetadata subtypes from the SDK assembly once.
        // Uses a try/catch on ReflectionTypeLoadException because some types in the CRM SDK
        // assembly depend on System.IdentityModel / System.ServiceModel which are not available
        // in modern .NET runtimes (net8+). Failed types are simply excluded.
        private static readonly Type[] KnownAttributeTypes = DiscoverAttributeSubtypes();

        private static Type[] DiscoverAttributeSubtypes()
        {
            Type[] allTypes;
            try
            {
                allTypes = typeof(AttributeMetadata).Assembly.GetTypes();
            }
            catch (System.Reflection.ReflectionTypeLoadException ex)
            {
                allTypes = (ex.Types ?? Array.Empty<Type?>())
                    .OfType<Type>()
                    .ToArray();
            }

            return allTypes
                .Where(t => !t.IsAbstract && !t.IsInterface && typeof(AttributeMetadata).IsAssignableFrom(t))
                .ToArray();
        }

        /// <summary>
        /// Parses XML that contains either a single <c>&lt;EntityMetadata&gt;</c> element or an
        /// <c>&lt;ArrayOfEntityMetadata&gt;</c> root element and returns the contained metadata.
        /// </summary>
        internal static EntityMetadata[] ParseXml(string xml)
        {
            if (string.IsNullOrWhiteSpace(xml))
                throw new ArgumentException("XML must not be null or empty.", nameof(xml));

            var doc = XDocument.Parse(xml);
            var rootName = doc.Root?.Name.LocalName
                ?? throw new ArgumentException("Empty XML document.", nameof(xml));

            if (string.Equals(rootName, "ArrayOfEntityMetadata", StringComparison.Ordinal))
            {
                var serializer = new DataContractSerializer(typeof(EntityMetadata[]), KnownAttributeTypes);
                using var reader = doc.CreateReader();
                var result = serializer.ReadObject(reader) as EntityMetadata[];
                return result ?? Array.Empty<EntityMetadata>();
            }

            if (string.Equals(rootName, "EntityMetadata", StringComparison.Ordinal))
            {
                var serializer = new DataContractSerializer(typeof(EntityMetadata), KnownAttributeTypes);
                using var reader = doc.CreateReader();
                var single = serializer.ReadObject(reader) as EntityMetadata;
                return single != null ? new[] { single } : Array.Empty<EntityMetadata>();
            }

            throw new ArgumentException(
                $"Unexpected XML root element '{rootName}'. Expected 'EntityMetadata' or 'ArrayOfEntityMetadata'.",
                nameof(xml));
        }

        /// <summary>
        /// Serializes an <see cref="EntityMetadata"/> object to a DataContract XML string.
        /// Useful for round-trip testing.
        /// </summary>
        internal static string SerializeToXml(EntityMetadata entity)
        {
            if (entity == null) throw new ArgumentNullException(nameof(entity));
            var serializer = new DataContractSerializer(typeof(EntityMetadata), KnownAttributeTypes);
            var sb = new System.Text.StringBuilder();
            using (var xmlWriter = System.Xml.XmlWriter.Create(sb))
            {
                serializer.WriteObject(xmlWriter, entity);
            }
            return sb.ToString();
        }

        /// <summary>
        /// Serializes an array of <see cref="EntityMetadata"/> objects to a DataContract XML string.
        /// Useful for round-trip testing.
        /// </summary>
        internal static string SerializeToXml(EntityMetadata[] entities)
        {
            if (entities == null) throw new ArgumentNullException(nameof(entities));
            var serializer = new DataContractSerializer(typeof(EntityMetadata[]), KnownAttributeTypes);
            var sb = new System.Text.StringBuilder();
            using (var xmlWriter = System.Xml.XmlWriter.Create(sb))
            {
                serializer.WriteObject(xmlWriter, entities);
            }
            return sb.ToString();
        }
    }
}
