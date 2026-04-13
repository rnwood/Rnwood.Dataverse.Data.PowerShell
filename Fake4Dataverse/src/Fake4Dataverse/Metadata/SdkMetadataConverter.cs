using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Metadata;

namespace Fake4Dataverse.Metadata
{
    /// <summary>
    /// Converts internal metadata DTOs to SDK metadata objects for use in responses.
    /// </summary>
    internal static class SdkMetadataConverter
    {
        internal static EntityMetadata ToSdkEntityMetadata(
            EntityMetadataInfo entity,
            IReadOnlyList<OneToManyRelationshipInfo>? oneToMany = null,
            IReadOnlyList<ManyToManyRelationshipInfo>? manyToMany = null)
        {
            var em = new EntityMetadata();
            em.LogicalName = entity.LogicalName;
            em.SchemaName = entity.SchemaName;

            if (entity.PrimaryIdAttribute != null)
                SetProperty(em, nameof(EntityMetadata.PrimaryIdAttribute), entity.PrimaryIdAttribute);

            if (entity.PrimaryNameAttribute != null)
                SetProperty(em, nameof(EntityMetadata.PrimaryNameAttribute), entity.PrimaryNameAttribute);

            if (entity.ObjectTypeCode.HasValue)
                SetProperty(em, nameof(EntityMetadata.ObjectTypeCode), entity.ObjectTypeCode);

            // Build attribute metadata array
            var attributes = entity.Attributes.Values
                .Select(ToSdkAttributeMetadata)
                .ToArray();
            SetProperty(em, nameof(EntityMetadata.Attributes), attributes);

            // Build relationship metadata arrays
            if (oneToMany != null && oneToMany.Count > 0)
            {
                var otm = oneToMany.Select(r =>
                {
                    var rel = new OneToManyRelationshipMetadata();
                    rel.SchemaName = r.SchemaName;
                    rel.ReferencedEntity = r.ReferencedEntity;
                    rel.ReferencedAttribute = r.ReferencedAttribute;
                    rel.ReferencingEntity = r.ReferencingEntity;
                    rel.ReferencingAttribute = r.ReferencingAttribute;
                    return rel;
                }).ToArray();
                SetProperty(em, nameof(EntityMetadata.OneToManyRelationships), otm);
            }

            if (manyToMany != null && manyToMany.Count > 0)
            {
                var mtm = manyToMany.Select(r =>
                {
                    var rel = new ManyToManyRelationshipMetadata();
                    rel.SchemaName = r.SchemaName;
                    rel.Entity1LogicalName = r.Entity1LogicalName;
                    rel.Entity2LogicalName = r.Entity2LogicalName;
                    if (r.IntersectEntityName != null)
                        rel.IntersectEntityName = r.IntersectEntityName;
                    return rel;
                }).ToArray();
                SetProperty(em, nameof(EntityMetadata.ManyToManyRelationships), mtm);
            }

            return em;
        }

        internal static AttributeMetadata ToSdkAttributeMetadata(AttributeMetadataInfo info)
        {
            AttributeMetadata attr;
            switch (info.AttributeType)
            {
                case AttributeTypeCode.String:
                case AttributeTypeCode.Memo:
                    var strAttr = new StringAttributeMetadata();
                    if (info.MaxLength.HasValue)
                        strAttr.MaxLength = info.MaxLength.Value;
                    attr = strAttr;
                    break;

                case AttributeTypeCode.Integer:
                    var intAttr = new IntegerAttributeMetadata();
                    if (info.MinValue.HasValue) intAttr.MinValue = (int)info.MinValue.Value;
                    if (info.MaxValue.HasValue) intAttr.MaxValue = (int)info.MaxValue.Value;
                    attr = intAttr;
                    break;

                case AttributeTypeCode.Decimal:
                    var decAttr = new DecimalAttributeMetadata();
                    if (info.MinValue.HasValue) decAttr.MinValue = (decimal)info.MinValue.Value;
                    if (info.MaxValue.HasValue) decAttr.MaxValue = (decimal)info.MaxValue.Value;
                    attr = decAttr;
                    break;

                case AttributeTypeCode.Double:
                    var dblAttr = new DoubleAttributeMetadata();
                    if (info.MinValue.HasValue) dblAttr.MinValue = info.MinValue.Value;
                    if (info.MaxValue.HasValue) dblAttr.MaxValue = info.MaxValue.Value;
                    attr = dblAttr;
                    break;

                case AttributeTypeCode.Money:
                    var moneyAttr = new MoneyAttributeMetadata();
                    if (info.MinValue.HasValue) moneyAttr.MinValue = info.MinValue.Value;
                    if (info.MaxValue.HasValue) moneyAttr.MaxValue = info.MaxValue.Value;
                    attr = moneyAttr;
                    break;

                case AttributeTypeCode.Picklist:
                    var plAttr = new PicklistAttributeMetadata();
                    if (info.ValidOptionSetValues != null)
                    {
                        var options = new OptionSetMetadata();
                        foreach (var val in info.ValidOptionSetValues)
                            options.Options.Add(new OptionMetadata(new Label(val.ToString(), 1033), val));
                        plAttr.OptionSet = options;
                    }
                    attr = plAttr;
                    break;

                case AttributeTypeCode.Lookup:
                case AttributeTypeCode.Customer:
                case AttributeTypeCode.Owner:
                    var lookupAttr = new LookupAttributeMetadata();
                    if (info.ValidTargetEntityTypes != null)
                        lookupAttr.Targets = info.ValidTargetEntityTypes.ToArray();
                    attr = lookupAttr;
                    break;

                case AttributeTypeCode.Boolean:
                    attr = new BooleanAttributeMetadata();
                    break;

                case AttributeTypeCode.DateTime:
                    attr = new DateTimeAttributeMetadata();
                    break;

                case AttributeTypeCode.BigInt:
                    attr = new BigIntAttributeMetadata();
                    break;

                default:
                    attr = new AttributeMetadata();
                    SetProperty(attr, nameof(AttributeMetadata.AttributeType), info.AttributeType);
                    break;
            }

            attr.LogicalName = info.LogicalName;
            attr.RequiredLevel = new AttributeRequiredLevelManagedProperty(info.RequiredLevel);
            return attr;
        }

        private static void SetProperty(object target, string propertyName, object? value)
        {
            var prop = target.GetType().GetProperty(
                propertyName,
                BindingFlags.Public | BindingFlags.Instance);
            if (prop != null && prop.CanWrite)
            {
                prop.SetValue(target, value);
                return;
            }

            // Fallback: try with non-public setter (some SDK properties have internal setters).
            prop = target.GetType().GetProperty(
                propertyName,
                BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
            prop?.SetValue(target, value);
        }
    }
}
