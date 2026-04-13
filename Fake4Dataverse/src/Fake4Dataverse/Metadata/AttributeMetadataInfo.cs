using System;
using System.Collections.Generic;
using Microsoft.Xrm.Sdk.Metadata;

namespace Fake4Dataverse.Metadata
{
    internal sealed class AttributeMetadataInfo
    {
        public string LogicalName { get; }
        public AttributeTypeCode AttributeType { get; set; }
        public AttributeRequiredLevel RequiredLevel { get; set; }
        public int? MaxLength { get; set; }
        public double? MinValue { get; set; }
        public double? MaxValue { get; set; }
        public HashSet<int>? ValidOptionSetValues { get; set; }
        public HashSet<string>? ValidTargetEntityTypes { get; set; }

        public AttributeMetadataInfo(string logicalName, AttributeTypeCode attributeType)
        {
            LogicalName = logicalName ?? throw new ArgumentNullException(nameof(logicalName));
            AttributeType = attributeType;
            RequiredLevel = AttributeRequiredLevel.None;
        }
    }
}
