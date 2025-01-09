

using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Metadata;
using Microsoft.Xrm.Sdk.Query;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Management.Automation;
using System.Text;

namespace Rnwood.Dataverse.Data.PowerShell.Commands
{
    internal class DataverseEntityConverter
    {
        public DataverseEntityConverter(IOrganizationService service, EntityMetadataFactory entityMetadataFactory)
        {
            this.service = service;
            this.entityMetadataFactory = entityMetadataFactory;
        }

        private IOrganizationService service;
        private EntityMetadataFactory entityMetadataFactory;

        public PSObject ConvertToPSObject(Entity entity, ColumnSet includedColumns, Func<AttributeMetadata, ValueType> getValueType)
        {
            PSObject result = new PSObject();

            result.Properties.Add(new PSNoteProperty("Id", entity.Id));
            result.Properties.Add(new PSNoteProperty("TableName", entity.LogicalName));

            EntityMetadata entityMetadata = entityMetadataFactory.GetMetadata(entity.LogicalName);

            foreach (AttributeMetadata attributeMetadata in entityMetadata.Attributes.OrderBy(a => a.LogicalName, StringComparer.OrdinalIgnoreCase))
            {
                if (includedColumns.AllColumns || includedColumns.Columns.Contains(attributeMetadata.LogicalName, StringComparer.OrdinalIgnoreCase))
                {
                    result.Properties.Add(new PSNoteProperty(attributeMetadata.LogicalName, GetPSValue(entity, entityMetadata, attributeMetadata, getValueType)));
                }
            }

            foreach (KeyValuePair<string, object> attr in entity.Attributes.Where(aa => aa.Value is AliasedValue && !entityMetadata.Attributes.Any(a => a.LogicalName.Equals(aa.Key, StringComparison.OrdinalIgnoreCase))))
            {
                AliasedValue aliasedValue = (AliasedValue)attr.Value;

                result.Properties.Add(new PSNoteProperty(attr.Key, aliasedValue.Value));
            }

            foreach (KeyValuePair<string, object> attr in entity.Attributes.Where(aa => aa.Value is EntityCollection && !entityMetadata.Attributes.Any(a => a.LogicalName.Equals(aa.Key, StringComparison.OrdinalIgnoreCase))))
            {
                if (includedColumns.AllColumns || includedColumns.Columns.Contains(attr.Key, StringComparer.OrdinalIgnoreCase))
                {
                    EntityCollection entities = (EntityCollection)attr.Value;
                    List<PSObject> psObjects = new List<PSObject>(entities.Entities.Count);

                    if (entities.Entities.Any())
                    {
                        EntityMetadata referencedEntityMetadata = entityMetadataFactory.GetMetadata(entities.Entities.First().LogicalName);

                        foreach (Entity referenceEntity in entities.Entities)
                        {
                            psObjects.Add(ConvertToPSObject(referenceEntity, new ColumnSet(GetAllColumnNames(referencedEntityMetadata, false, null)), getValueType));
                        }
                    }

                    result.Properties.Add(new PSNoteProperty(attr.Key, psObjects.ToArray()));
                }
            }

            return result;
        }

        public Entity ConvertToDataverseEntity(PSObject psObject, string entityName, ConvertToDataverseEntityOptions options)
        {
            if (psObject.ImmediateBaseObject is Entity e)
            {
                return e;
            }

            if (psObject.ImmediateBaseObject is Hashtable ht)
            {
                psObject = new PSObject();
                foreach (var kvp in ht.Cast<DictionaryEntry>())
                {
                    psObject.Properties.Add(new PSNoteProperty((string)kvp.Key, kvp.Value));
                }
            }

            EntityMetadata entityMetadata = entityMetadataFactory.GetMetadata(entityName);

            Entity result = new Entity(entityName);

            foreach (PSPropertyInfo property in psObject.Properties)
            {
                if (property.Name.Equals("Id", StringComparison.OrdinalIgnoreCase))
                {
                    Guid id;
                    if (Guid.TryParse(Convert.ToString(property.Value), out id))
                    {
                        result.Id = id;
                    }

                    continue;
                }

                if (options.IgnoredPropertyName.Contains(property.Name) || property.Name.Equals(
                 "EntityName", StringComparison.OrdinalIgnoreCase) || property.Name.Equals(
                 "TableName", StringComparison.OrdinalIgnoreCase))
                {
                    continue;
                }

                ConvertToDataverseEntityColumnOptions columnOptions;
                if (!options.ColumnOptions.TryGetValue(property.Name, out columnOptions))
                {
                    columnOptions = new ConvertToDataverseEntityColumnOptions();
                }

                AttributeMetadata attributeMetadata = entityMetadataFactory.GetAttribute(entityName, property.Name);

                if (attributeMetadata == null)
                {
                    if (property.Name == "calendarrules") //Magic attribute not in metadata
                    {
                    }
                    else
                    {
                        throw new ArgumentException(string.Format("Table {0} does not contain a column with the name {1}", entityName, property.Name));
                    }
                }

                object convertedValue = ConvertToDataverseValue(entityMetadata, property.Name, attributeMetadata, property.Value, columnOptions);

                if (attributeMetadata != null)
                {
                    result[attributeMetadata.LogicalName] = convertedValue;

                    if (attributeMetadata.LogicalName == entityMetadata.PrimaryIdAttribute && convertedValue != null)
                    {
                        result.Id = (Guid)convertedValue;
                    }
                }
                else
                {
                    result[property.Name.ToLower()] = convertedValue;
                }
            }

            return result;
        }

        public static string[] GetAllColumnNames(EntityMetadata entityMetadata, bool includeSystemColumns, string[] excludeColumns)
        {
            string[] magicColumns = new string[0];

            if (entityMetadata.LogicalName.Equals("calendar", StringComparison.OrdinalIgnoreCase))
            {
                magicColumns = new string[] { "calendarrules" };
            }

            return entityMetadata.Attributes
                                 .Where(a => a.IsValidForRead.GetValueOrDefault() && a.AttributeOf == null)
                                 .Select(a => a.LogicalName)
                                 .Concat(magicColumns)
                                 .Except(!includeSystemColumns
                                             ? new[]
                                                 { "organizationid",
                                                     "createdby", "createdon", "createdonbehalfby", "modifiedby",
                                                     "modifiedon", "modifiedonbehalfby", "ownerid",
                                                     "transactioncurrencyid",
                                                     "importsequencenumber", "overriddencreatedon",
                                                     "owningbusinessunit", "owningteam", "owninguser", "timezoneruleversionnumber",
                                                     "utcconversiontimezonecode", "versionnumber"
                                                 }
                                             : new string[0], StringComparer.OrdinalIgnoreCase)
                                  .Except(excludeColumns ?? new string[0], StringComparer.OrdinalIgnoreCase)
                                 .ToArray();
        }

        public object GetPSValue(Entity entity, EntityMetadata entityMetadata, AttributeMetadata attributeMetadata, Func<AttributeMetadata, ValueType> getValueType)
        {
            ValueType valueType = getValueType(attributeMetadata);
            bool useRawValues = valueType == ValueType.Raw;

            switch (attributeMetadata.AttributeType.Value)
            {
                case AttributeTypeCode.Owner:
                case AttributeTypeCode.Lookup:
                case AttributeTypeCode.Customer:
                    EntityReference entityReferenceValue =
                        entity.GetAttributeValue<EntityReference>(attributeMetadata.LogicalName);

                    if (entityReferenceValue != null)
                    {
                        //The LogicalName returned in entityreferences on activitymimeattachment entity are invalid for some reason, so don't try to resolve.

                        DataverseEntityReference nameAndId = new DataverseEntityReference(entityReferenceValue);

                        if (useRawValues || attributeMetadata.LogicalName == "objectid" || attributeMetadata.LogicalName == "attachmentid")
                        {
                            return nameAndId;
                        }

                        if (entityReferenceValue.Name != null)
                        {
                            return entityReferenceValue.Name;
                        }

                        //For some odd reason the EntityReference Name that is returned is sometimes null even though the record it's pointing at has a name
                        string nameAttribute = entityMetadataFactory.GetMetadata(entityReferenceValue.LogicalName).PrimaryNameAttribute;

                        //Some internal entities do not have a primary name attribute
                        if (nameAttribute == null)
                        {
                            return nameAndId;
                        }

                        if (entityReferenceValue.Id == Guid.Empty)
                        {
                            return null;
                        }

                        string name = service.Retrieve(entityReferenceValue.LogicalName, entityReferenceValue.Id,
                                         new ColumnSet(nameAttribute)).GetAttributeValue<string>(nameAttribute);

                        if (string.IsNullOrEmpty(name))
                        {
                            return nameAndId;
                        }

                        return name;
                    }

                    return null;

                case AttributeTypeCode.Picklist:
                case AttributeTypeCode.Status:
                case AttributeTypeCode.State:

                    OptionSetValue optionSetValue =
                        entity.GetAttributeValue<OptionSetValue>(attributeMetadata.LogicalName);

                    if (optionSetValue != null)
                    {
                        if (useRawValues)
                        {
                            return optionSetValue.Value;
                        }

                        EnumAttributeMetadata pickListMetadata = (EnumAttributeMetadata)attributeMetadata;

                        OptionMetadata option = pickListMetadata.OptionSet.Options.FirstOrDefault(o => o.Value == optionSetValue.Value);
                        return option != null ? option.Label.UserLocalizedLabel.Label : (object)optionSetValue.Value;
                    }

                    return null;

                case AttributeTypeCode.DateTime:
                    DateTime? dateValue = entity.GetAttributeValue<DateTime?>(attributeMetadata.LogicalName);

                    //Deal with an issue in Dataverse web service or sdk (not sure which)
                    //Some scheduling entities store their date/times in the DB as local times (as opposed to UTC which is used in all other cases)
                    //These dates are incorrectly being returned with a DateTimeKind of UTC when they are actually local times rel
                    if (entityMetadata.LogicalName != "usersettings" && entityMetadata.Attributes.Any(a => a.LogicalName.Equals("timezonecode", StringComparison.OrdinalIgnoreCase)))
                    {
                        if (!entity.Contains("timezonecode"))
                        {
                            throw new InvalidOperationException("This is an table with special non-UTC datetimes. Must include timezonecode in entity to allow conversion");
                        }

                        int? tzCode = entity.GetAttributeValue<int?>("timezonecode");
                        if (tzCode.HasValue && dateValue.HasValue)
                        {
                            if (tzCode.Value != 85 && tzCode.Value != -1)
                            {
                                throw new NotImplementedException("Unhandled timezonecode " + tzCode);
                            }

                            return DateTime.SpecifyKind(dateValue.Value, DateTimeKind.Local);
                        }
                    }

                    return dateValue.HasValue ? dateValue.Value.ToLocalTime() : (DateTime?)null;

                case AttributeTypeCode.Money:
                    Money moneyValue = entity.GetAttributeValue<Money>(attributeMetadata.LogicalName);
                    return moneyValue != null ? moneyValue.Value : (decimal?)null;

                case AttributeTypeCode.Uniqueidentifier:
                    Guid? guidValue = entity.GetAttributeValue<Guid?>(attributeMetadata.LogicalName);

                    if (guidValue.HasValue && entityMetadata.IsIntersect.GetValueOrDefault() && !useRawValues)
                    {
                        ManyToManyRelationshipMetadata manyToManyRelationshipMetadata = entityMetadata.ManyToManyRelationships[0];

                        if (manyToManyRelationshipMetadata.Entity1IntersectAttribute.Equals(attributeMetadata.LogicalName, StringComparison.OrdinalIgnoreCase))
                        {
                            string nameAttribute = entityMetadataFactory.GetMetadata(manyToManyRelationshipMetadata.Entity1LogicalName).PrimaryNameAttribute;
                            return service.Retrieve(manyToManyRelationshipMetadata.Entity1LogicalName, guidValue.Value, new ColumnSet(nameAttribute)).GetAttributeValue<string>(nameAttribute);
                        }
                        else if (manyToManyRelationshipMetadata.Entity2IntersectAttribute.Equals(attributeMetadata.LogicalName, StringComparison.OrdinalIgnoreCase))
                        {
                            string nameAttribute = entityMetadataFactory.GetMetadata(manyToManyRelationshipMetadata.Entity2LogicalName).PrimaryNameAttribute;
                            return service.Retrieve(manyToManyRelationshipMetadata.Entity2LogicalName, guidValue.Value, new ColumnSet(nameAttribute)).GetAttributeValue<string>(nameAttribute);
                        }
                    }
                    return guidValue;

                case AttributeTypeCode.PartyList:

                    EntityCollection entities = entity.GetAttributeValue<EntityCollection>(attributeMetadata.LogicalName);
                    List<PSObject> psObjects = new List<PSObject>(entities.Entities.Count);

                    if (entities.Entities.Any())
                    {
                        EntityMetadata referencedEntityMetadata = entityMetadataFactory.GetMetadata(entities.Entities.First().LogicalName);

                        foreach (Entity referenceEntity in entities.Entities)
                        {
                            psObjects.Add(ConvertToPSObject(referenceEntity, new ColumnSet(GetAllColumnNames(referencedEntityMetadata, false, new[] { "exchangeentryid" })), getValueType));
                        }
                    }

                    return psObjects.ToArray();

                default:
                    if (attributeMetadata is MultiSelectPicklistAttributeMetadata
                        multiPicklistAttributeMetadata)
                    {
                        OptionSetValueCollection optionSetValues = entity.GetAttributeValue<OptionSetValueCollection>(attributeMetadata.LogicalName);

                        if (optionSetValues != null)
                        {
                            if (useRawValues)
                            {
                                return optionSetValues.Select(v => v.Value).ToArray();
                            }

                            return optionSetValues.Select(v =>
                            {
                                OptionMetadata option = multiPicklistAttributeMetadata.OptionSet.Options.FirstOrDefault(o => o.Value == v.Value);
                                return option != null ? option.Label.UserLocalizedLabel.Label : (object)v.Value;
                            }).ToArray();
                        }
                    }

                    return entity.GetAttributeValue<object>(attributeMetadata.LogicalName);
            }
        }

        public object ConvertToDataverseValue(EntityMetadata entityMetadata, string propertyName, AttributeMetadata attributeMetadata, object psValue, ConvertToDataverseEntityColumnOptions columnOptions)
        {
            if (propertyName.Equals("calendarrules", StringComparison.OrdinalIgnoreCase)) //Magic attribute no metadata
            {
                if (psValue is IEnumerable)
                {
                    ConvertToDataverseEntityOptions options = new ConvertToDataverseEntityOptions();

                    Entity[] entities = ((IEnumerable)psValue).Cast<object>().Select(PSObject.AsPSObject)
                        .Select(o => ConvertToDataverseEntity(o, "calendarrule", options)).ToArray();

                    return new EntityCollection(entities);
                }
                else
                {
                    throw new Exception("calendarrule property must contain collection of objects");
                }
            }

            if (psValue is PSObject && !(((PSObject)psValue).BaseObject is PSCustomObject))
            {
                psValue = ((PSObject)psValue).BaseObject;
            }

            object convertedValue;
            string stringValue = Convert.ToString(psValue);

            switch (attributeMetadata.AttributeType.Value)
            {
                case AttributeTypeCode.PartyList:
                    if (psValue is IEnumerable)
                    {
                        ConvertToDataverseEntityOptions options = new ConvertToDataverseEntityOptions();

                        Entity[] entities = ((IEnumerable)psValue).Cast<object>().Select(PSObject.AsPSObject)
                            .Select(o => ConvertToDataverseEntity(o, "activityparty", options)).ToArray();

                        return new EntityCollection(entities);
                    }
                    else
                    {
                        throw new Exception("partylist property must contain collection of objects");
                    }

                case AttributeTypeCode.Uniqueidentifier:
                    {
                        if (!string.IsNullOrWhiteSpace(stringValue))
                        {
                            if (entityMetadata.IsIntersect.GetValueOrDefault())
                            {
                                Guid guidValue;
                                if (Guid.TryParse(stringValue, out guidValue))
                                {
                                    return guidValue;
                                }

                                ManyToManyRelationshipMetadata manyToManyRelationshipMetadata = entityMetadata.ManyToManyRelationships[0];

                                string lookupEntity = null;
                                if (manyToManyRelationshipMetadata.Entity1IntersectAttribute.Equals(attributeMetadata.LogicalName, StringComparison.OrdinalIgnoreCase))
                                {
                                    lookupEntity = manyToManyRelationshipMetadata.Entity1LogicalName;
                                }
                                else if (manyToManyRelationshipMetadata.Entity2IntersectAttribute.Equals(attributeMetadata.LogicalName, StringComparison.OrdinalIgnoreCase))
                                {
                                    lookupEntity = manyToManyRelationshipMetadata.Entity2LogicalName;
                                }

                                if (lookupEntity != null)
                                {
                                    var targetEntityMetadata = entityMetadataFactory.GetMetadata(lookupEntity);
                                    string lookupNameAttribute = targetEntityMetadata.PrimaryNameAttribute;


                                    QueryByAttribute lookupQuery = new QueryByAttribute(lookupEntity);
                                    lookupQuery.TopCount = 2;
                                    lookupQuery.ColumnSet = new ColumnSet();

                                    if (columnOptions.LookupColumn != null)
                                    {
                                        lookupQuery.AddAttributeValue(columnOptions.LookupColumn, stringValue);
                                    }
                                    else
                                    {
                                        lookupQuery.AddAttributeValue(lookupNameAttribute, stringValue);
                                    }

                                    var lookupRecords = service.RetrieveMultiple(lookupQuery).Entities;

                                    if (lookupRecords.Count == 1)
                                    {
                                        return lookupRecords[0].Id;
                                    }
                                    else if (lookupRecords.Count > 1)
                                    {
                                        throw new Exception(string.Format("Could not find a single {0} record matching {2} '{1}'", lookupEntity, stringValue, columnOptions.LookupColumn ?? "name"));
                                    }
                                }

                                throw new Exception(string.Format("Could not find a single {0} record uniquely matching {2} '{1}'", lookupEntity, stringValue, columnOptions.LookupColumn ?? "name"));

                            }

                            convertedValue = Guid.Parse(stringValue);
                        }

                        else
                        {
                            convertedValue = null;
                        }
                        break;
                    }


                case AttributeTypeCode.BigInt:
                    {
                        if (!string.IsNullOrWhiteSpace(stringValue))
                        {
                            convertedValue = Convert.ToInt64(psValue);
                        }
                        else
                        {
                            convertedValue = null;
                        }
                        break;
                    }

                case AttributeTypeCode.Boolean:
                    {
                        if (!string.IsNullOrWhiteSpace(stringValue))
                        {
                            convertedValue = Convert.ToBoolean(psValue);
                        }
                        else
                        {
                            convertedValue = null;
                        }
                        break;
                    }

                case AttributeTypeCode.DateTime:
                    {
                        if (psValue is DateTime)
                        {
                            DateTime dateValue = (DateTime)psValue;
                            if (dateValue.Kind == DateTimeKind.Unspecified)
                            {
                                dateValue = DateTime.SpecifyKind(dateValue, DateTimeKind.Local);
                            }

                            convertedValue = dateValue.ToUniversalTime();
                        }
                        else if (!string.IsNullOrWhiteSpace(stringValue))
                        {
                            convertedValue = DateTime.Parse(stringValue, null, System.Globalization.DateTimeStyles.AssumeLocal).ToUniversalTime();
                        }
                        else
                        {
                            convertedValue = null;
                        }

                        //Deal with an issue in Dataverse web service or sdk (not sure which)
                        //Some scheduling entities store their date/times in the DB as local times (as opposed to UTC which is used in all other cases)
                        //These dates are incorrectly being returned with a DateTimeKind of UTC when they are actually local times
                        if (convertedValue is DateTime && entityMetadata.Attributes.Any(a => a.LogicalName.Equals("timezonecode", StringComparison.OrdinalIgnoreCase)))
                        {
                            convertedValue = DateTime.SpecifyKind(((DateTime)convertedValue).ToLocalTime(), DateTimeKind.Utc);
                        }

                        break;
                    }

                case AttributeTypeCode.Money:
                    {
                        if (!string.IsNullOrWhiteSpace(stringValue))
                        {
                            convertedValue = new Money(Convert.ToDecimal(psValue));
                        }
                        else
                        {
                            convertedValue = null;
                        }
                        break;
                    }

                case AttributeTypeCode.Decimal:
                    {
                        if (!string.IsNullOrWhiteSpace(stringValue))
                        {
                            convertedValue = Convert.ToDecimal(psValue);
                        }
                        else
                        {
                            convertedValue = null;
                        }
                        break;
                    }

                case AttributeTypeCode.Double:
                    {
                        if (!string.IsNullOrWhiteSpace(stringValue))
                        {
                            convertedValue = Convert.ToDouble(psValue);
                        }
                        else
                        {
                            convertedValue = null;
                        }
                        break;
                    }

                case AttributeTypeCode.Integer:
                    {
                        if (!string.IsNullOrWhiteSpace(stringValue))
                        {
                            convertedValue = Convert.ToInt32(psValue);
                        }
                        else
                        {
                            convertedValue = null;
                        }
                        break;
                    }

                case AttributeTypeCode.Memo:
                    {
                        convertedValue = stringValue;
                        break;
                    }

                case AttributeTypeCode.EntityName:
                case AttributeTypeCode.String:
                    {
                        if (psValue != null)
                        {
                            convertedValue = Convert.ToString(psValue);
                        }
                        else
                        {
                            convertedValue = null;
                        }
                        break;
                    }

                case AttributeTypeCode.Picklist:
                case AttributeTypeCode.Status:
                case AttributeTypeCode.State:
                    {
                        if (!string.IsNullOrWhiteSpace(stringValue))
                        {
                            EnumAttributeMetadata pickListAttributeMetadata = (EnumAttributeMetadata)attributeMetadata;
                            convertedValue = pickListAttributeMetadata.OptionSet.Options
                                .Where(o => string.Equals(o.Label.UserLocalizedLabel.Label, stringValue, StringComparison.OrdinalIgnoreCase))
                                .Select(o => new OptionSetValue(o.Value.Value))
                                .FirstOrDefault();

                            if (convertedValue == null)
                            {
                                int intValue;
                                if (int.TryParse(stringValue, out intValue))
                                {
                                    convertedValue = new OptionSetValue(intValue);
                                }
                            }

                            if (convertedValue == null)
                            {
                                throw new FormatException(string.Format("Could not find options set value for matching label or value for string '{0}' for attribute {1}", stringValue, propertyName));
                            }
                        }
                        else
                        {
                            convertedValue = null;
                        }
                        break;
                    }
                case AttributeTypeCode.Owner:
                case AttributeTypeCode.Lookup:
                case AttributeTypeCode.Customer:
                    {
                        if (psValue != null && psValue is DataverseEntityReference)
                        {
                            convertedValue = ((DataverseEntityReference)psValue).ToEntityReference();
                        }
                        if (psValue != null && psValue is PSObject)
                        {
                            PSObject customObj = (PSObject)psValue;

                            var nameProp = customObj.Properties.FirstOrDefault(p => p.Name.Equals("TableName", StringComparison.OrdinalIgnoreCase));
                            if (nameProp == null)
                            {
                                nameProp = customObj.Properties.FirstOrDefault(p => p.Name.Equals("EntityName", StringComparison.OrdinalIgnoreCase));
                            }

                            if (nameProp == null)
                            {
                                throw new FormatException("Could not convert value to entity reference. TableName(/EntityName) property is missing");
                            }

                            convertedValue = new EntityReference((string)nameProp.Value, Guid.Parse(customObj.Properties["Id"].Value.ToString()));
                        }
                        else if (!string.IsNullOrWhiteSpace(stringValue) &&
                            (
                                stringValue.Contains("TableName=")
                                || stringValue.Contains("EntityName=")
                            )
                            && stringValue.Contains("Id="))
                        {
                            string[] keyValuePairs = stringValue.Split(new[] { ';' }, StringSplitOptions.RemoveEmptyEntries);
                            string entityName = null;
                            Guid? id = null;

                            foreach (string keyValuePair in keyValuePairs)
                            {
                                string[] keyValuePairBits = keyValuePair.Split(new[] { '=' }, 2).Select(v => v.Trim()).ToArray();
                                if (keyValuePairBits.Length == 2)
                                {
                                    string key = keyValuePairBits[0];
                                    string value = keyValuePairBits[1];

                                    if (key.Equals("Id", StringComparison.OrdinalIgnoreCase))
                                    {
                                        Guid idValue;
                                        if (Guid.TryParse(value, out idValue))
                                        {
                                            id = idValue;
                                        }
                                    }
                                    else if (key.Equals("TableName", StringComparison.OrdinalIgnoreCase)
                                        || key.Equals("EntityName", StringComparison.OrdinalIgnoreCase))
                                    {
                                        entityName = value;
                                    }
                                }
                            }

                            if (string.IsNullOrEmpty(entityName) || !id.HasValue)
                            {
                                throw new FormatException(string.Format("Could not convert value '{0}' to entity reference. Either TableName(/EntityName) or Id value is missing or invalid", stringValue));
                            }

                            convertedValue = new EntityReference(entityName, id.Value);
                        }
                        else if (!string.IsNullOrWhiteSpace(stringValue))
                        {
                            LookupAttributeMetadata lookupAttributeMetadata = (LookupAttributeMetadata)attributeMetadata;
                            convertedValue = null;

                            if (lookupAttributeMetadata.Targets == null || lookupAttributeMetadata.Targets.Length == 0)
                            {
                                //Dataverse bug!
                                //Targets missing so go find them from relationship data, which is not affected by this issue.
                                lookupAttributeMetadata.Targets = entityMetadata.ManyToOneRelationships.Where(r => r.ReferencingAttribute.Equals(attributeMetadata.LogicalName, StringComparison.OrdinalIgnoreCase)).Select(r => r.ReferencedEntity).ToArray();
                            }

                            if (lookupAttributeMetadata.Targets.Length == 0)
                            {
                                throw new FormatException("Could not determine the target tables for lookup column " + attributeMetadata.LogicalName + " - Dataverse metadata returned 0 targets!");
                            }

                            //Optimise the most common case. Single target lookup column and incoming guid as the value to convert
                            //Don't query to check record exists - Dataverse will do that.
                            if (lookupAttributeMetadata.Targets.Length == 1)
                            {
                                Guid guidValue;
                                if (Guid.TryParse(stringValue, out guidValue))
                                {
                                    convertedValue = new EntityReference(lookupAttributeMetadata.Targets[0], guidValue);
                                    break;
                                }
                            }

                            bool applyStateFilter = true;

                            for (int i = 1; i <= 2; i++)
                            {
                                foreach (string targetEntityName in lookupAttributeMetadata.Targets)
                                {
                                    EntityMetadata targetEntityMetadata;

                                    try
                                    {
                                        targetEntityMetadata = entityMetadataFactory.GetMetadata(targetEntityName);
                                    }
                                    catch (Exception e)
                                    {
                                        //Dataverse returns entities which don't exists in Targets!
                                        if (e.Message == "Could not find table")
                                        {
                                            continue;
                                        }

                                        throw;
                                    }

                                    QueryByAttribute targetEntityQuery = new QueryByAttribute(targetEntityName);
                                    targetEntityQuery.ColumnSet = new ColumnSet();
                                    targetEntityQuery.TopCount = 2;

                                    if (targetEntityMetadata.Attributes.Any(a => string.Equals(a.LogicalName, "createdon", StringComparison.OrdinalIgnoreCase)))
                                    {
                                        targetEntityQuery.AddOrder("createdon", OrderType.Descending);
                                    }

                                    Guid guidValue;
                                    if (Guid.TryParse(stringValue, out guidValue))
                                    {
                                        targetEntityQuery.AddAttributeValue(targetEntityMetadata.PrimaryIdAttribute, guidValue);
                                    }
                                    else
                                    {
                                        if (columnOptions.LookupColumn != null)
                                        {
                                            targetEntityQuery.AddAttributeValue(columnOptions.LookupColumn, stringValue);
                                        }
                                        else
                                        {
                                            targetEntityQuery.AddAttributeValue(targetEntityMetadata.PrimaryNameAttribute, stringValue);
                                        }

                                        if (applyStateFilter)
                                        {
                                            if (targetEntityMetadata.Attributes.Any(a => string.Equals(a.LogicalName, "statecode", StringComparison.OrdinalIgnoreCase)))
                                            {
                                                targetEntityQuery.AddAttributeValue("statecode", 0);
                                            }

                                            if (targetEntityMetadata.Attributes.Any(a => string.Equals(a.LogicalName, "isdisabled", StringComparison.OrdinalIgnoreCase)))
                                            {
                                                targetEntityQuery.AddAttributeValue("isdisabled", false);
                                            }
                                        }
                                    }

                                    DataCollection<Entity> targetRecords = service.RetrieveMultiple(targetEntityQuery).Entities;
                                    if (targetRecords.Count > 0)
                                    {
                                        convertedValue = targetRecords[0].ToEntityReference();
                                        break;
                                    }
                                }

                                if (convertedValue != null)
                                {
                                    break;
                                }

                                applyStateFilter = false;
                            }

                            if (convertedValue == null)
                            {
                                throw new FormatException(string.Format("Could not find any {0} record with ID matching or any single active record with {2} matching '{1}'", string.Join("/", lookupAttributeMetadata.Targets), stringValue, columnOptions.LookupColumn ?? "name"));
                            }
                        }
                        else
                        {
                            convertedValue = null;
                        }
                        break;
                    }

                default:
                    if (attributeMetadata is MultiSelectPicklistAttributeMetadata
         multiPicklistAttributeMetadata)
                    {
                        if (psValue == null)
                        {
                            convertedValue = null;
                            break;
                        }

                        if (!(psValue is IEnumerable enumerable))
                        {
                            enumerable = new[] { psValue };
                        }

                        OptionSetValueCollection optionSetValues = new OptionSetValueCollection();
                        foreach (var item in enumerable)
                        {

                        }

                        convertedValue = optionSetValues;

                    }

                    throw new NotImplementedException("Conversion to column type " + attributeMetadata.AttributeType.Value + " not implemented");
            }

            return convertedValue;
        }
    }
}