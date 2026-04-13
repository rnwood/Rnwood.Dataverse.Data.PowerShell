using System;
using System.Collections.Generic;

namespace Fake4Dataverse.Metadata
{
    internal sealed class EntityMetadataInfo
    {
        public string LogicalName { get; }
        public string? SchemaName { get; set; }
        public string? PrimaryIdAttribute { get; set; }
        public string? PrimaryNameAttribute { get; set; }
        public int? ObjectTypeCode { get; set; }

        public Dictionary<string, AttributeMetadataInfo> Attributes { get; } =
            new Dictionary<string, AttributeMetadataInfo>(StringComparer.OrdinalIgnoreCase);

        public List<AlternateKeyInfo> AlternateKeys { get; } = new List<AlternateKeyInfo>();

        public EntityMetadataInfo(string logicalName)
        {
            LogicalName = logicalName ?? throw new ArgumentNullException(nameof(logicalName));
        }
    }

    internal sealed class AlternateKeyInfo
    {
        public string Name { get; }
        public string[] AttributeNames { get; }

        public AlternateKeyInfo(string name, string[] attributeNames)
        {
            Name = name ?? throw new ArgumentNullException(nameof(name));
            AttributeNames = attributeNames ?? throw new ArgumentNullException(nameof(attributeNames));
        }
    }
}
