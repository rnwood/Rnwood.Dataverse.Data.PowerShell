using Microsoft.Crm.Sdk;
using Microsoft.Crm.Sdk.Messages;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Messages;
using Microsoft.Xrm.Sdk.Metadata;
using Microsoft.Xrm.Sdk.Query;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Xml.Linq;

namespace Rnwood.Dataverse.Data.PowerShell.Commands
{
    /// <summary>
    /// Helper class for handling QuickFind view FetchXml preprocessing and postprocessing.
    /// </summary>
    internal static class QuickFindHelper
    {
        /// <summary>
        /// Preprocesses FetchXml for QuickFind views by replacing placeholders with type-appropriate dummy values.
        /// </summary>
        /// <param name="fetchXml">The original FetchXml.</param>
        /// <param name="tableName">The logical name of the table.</param>
        /// <param name="entityMetadataFactory">The entity metadata factory for cached metadata access.</param>
        /// <param name="placeholderMap">Output dictionary mapping attribute names to placeholders.</param>
        /// <returns>The preprocessed FetchXml with placeholders replaced by type-appropriate dummy values.</returns>
        public static string PreprocessFetchXmlForQuickFind(string fetchXml, string tableName, EntityMetadataFactory entityMetadataFactory, out Dictionary<string, string> placeholderMap)
        {
            placeholderMap = new Dictionary<string, string>();
            XDocument doc = XDocument.Parse(fetchXml);

            // Get cached metadata
            var metadata = entityMetadataFactory.GetLimitedMetadata(tableName);
            var attributes = metadata.Attributes.ToDictionary(a => a.LogicalName);

            // Find conditions with placeholders
            var conditions = doc.Descendants("condition");
            foreach (var cond in conditions)
            {
                var valueAttr = cond.Attribute("value");
                var attributeName = cond.Attribute("attribute")?.Value;
                if (valueAttr != null && !string.IsNullOrEmpty(attributeName) && attributes.TryGetValue(attributeName, out var attr))
                {
                    string value = valueAttr.Value;
                    // Find placeholders like {0}, {1}, etc.
                    var placeholders = Regex.Matches(value, @"\{(\d+)\}");
                    foreach (Match match in placeholders)
                    {
                        string placeholder = match.Value; // e.g. {0}
                        string dummyValue = GetDummyValueForType(attr.AttributeType.Value);
                        placeholderMap[attributeName] = placeholder;
                        value = value.Replace(placeholder, dummyValue);
                    }
                    valueAttr.Value = value;
                }
            }

            return doc.ToString();
        }

        /// <summary>
        /// Postprocesses FetchXml for QuickFind views by replacing type-appropriate dummy values back to placeholders.
        /// </summary>
        /// <param name="fetchXml">The FetchXml with type-appropriate dummy values.</param>
        /// <param name="placeholderMap">Dictionary mapping attribute names to placeholders.</param>
        /// <param name="entityMetadataFactory">The entity metadata factory for cached metadata access.</param>
        /// <param name="tableName">The logical name of the table.</param>
        /// <returns>The postprocessed FetchXml with placeholders restored.</returns>
        public static string PostprocessFetchXmlForQuickFind(string fetchXml, Dictionary<string, string> placeholderMap)
        {
            XDocument doc = XDocument.Parse(fetchXml);
            XNamespace ns = "http://schemas.microsoft.com/crm/2006/query";

            // Get cached metadata


            // Find conditions with dummy values
            var conditions = doc.Descendants("condition");
            foreach (var cond in conditions)
            {
                var valueAttr = cond.Attribute("value");
                var attributeName = cond.Attribute("attribute")?.Value;
                if (valueAttr != null && !string.IsNullOrEmpty(attributeName) && placeholderMap.TryGetValue(attributeName, out var placeholder))
                {
                    valueAttr.Value = placeholder;
                }
            }

            return doc.ToString();
        }

        /// <summary>
        /// Gets a dummy value string for a given attribute type.
        /// </summary>
        /// <param name="type">The attribute type code.</param>
        /// <returns>A string representation of a dummy value for the type.</returns>
        public static string GetDummyValueForType(AttributeTypeCode type)
        {
            switch (type)
            {
                case AttributeTypeCode.Uniqueidentifier:
                    return Guid.NewGuid().ToString();
                case AttributeTypeCode.Boolean:
                    return "true";
                case AttributeTypeCode.Integer:
                case AttributeTypeCode.BigInt:
                    return "123";
                case AttributeTypeCode.Decimal:
                case AttributeTypeCode.Double:
                case AttributeTypeCode.Money:
                    return "123.45";
                case AttributeTypeCode.DateTime:
                    return DateTime.Now.ToString("yyyy-MM-ddTHH:mm:ss");
                case AttributeTypeCode.String:
                case AttributeTypeCode.Memo:
                    return "dummy";
                case AttributeTypeCode.Picklist:
                case AttributeTypeCode.Status:
                    return "1";
                case AttributeTypeCode.Lookup:
                case AttributeTypeCode.Customer:
                case AttributeTypeCode.Owner:
                    return Guid.NewGuid().ToString();
                default:
                    return "dummy";
            }
        }

        /// <summary>
        /// Replaces placeholders in filter conditions with their original values.
        /// </summary>
        /// <param name="filter">The filter expression to process.</param>
        /// <param name="placeholderMap">Dictionary mapping attribute names to placeholder values.</param>
        public static void ReplacePlaceholdersInFilter(FilterExpression filter, Dictionary<string, string> placeholderMap)
        {
            if (filter == null) return;

            foreach (var condition in filter.Conditions)
            {
                if (placeholderMap.TryGetValue(condition.AttributeName, out var originalValue))
                {
                    for (int i = 0; i < condition.Values.Count; i++)
                    {
                        condition.Values[i] = originalValue;
                    }
                }
            }

            foreach (var subFilter in filter.Filters)
            {
                ReplacePlaceholdersInFilter(subFilter, placeholderMap);
            }
        }

        /// <summary>
        /// Preprocesses FilterExpression for QuickFind views by replacing placeholders with type-appropriate dummy values.
        /// </summary>
        /// <param name="filter">The filter expression to process.</param>
        /// <param name="tableName">The logical name of the table.</param>
        /// <param name="entityMetadataFactory">The entity metadata factory for cached metadata access.</param>
        /// <param name="placeholderMap">Output dictionary mapping attribute names to placeholders.</param>
        public static void PreprocessFilterForQuickFind(FilterExpression filter, string tableName, EntityMetadataFactory entityMetadataFactory, out Dictionary<string, string> placeholderMap)
        {
            placeholderMap = new Dictionary<string, string>();
            var metadata = entityMetadataFactory.GetLimitedMetadata(tableName);
            var attributes = metadata.Attributes.ToDictionary(a => a.LogicalName);
            PreprocessFilterRecursive(filter, placeholderMap, attributes);
        }

        private static void PreprocessFilterRecursive(FilterExpression filter, Dictionary<string, string> placeholderMap, Dictionary<string, AttributeMetadata> attributes)
        {
            foreach (var condition in filter.Conditions)
            {
                var attributeName = condition.AttributeName;
                if (attributes.TryGetValue(attributeName, out var attr))
                {
                    for (int i = 0; i < condition.Values.Count; i++)
                    {
                        if (condition.Values[i] is string strValue)
                        {
                            var placeholders = Regex.Matches(strValue, @"\{(\d+)\}");
                            foreach (Match match in placeholders)
                            {
                                string placeholder = match.Value;
                                string dummyValue = GetDummyValueForType(attr.AttributeType.Value);
                                placeholderMap[attributeName] = placeholder;
                                strValue = strValue.Replace(placeholder, dummyValue);
                            }
                            condition.Values[i] = strValue;
                            filter.IsQuickFindFilter = true;
                        }
                    }
                }
            }

            foreach (var subFilter in filter.Filters)
            {
                PreprocessFilterRecursive(subFilter, placeholderMap, attributes);
            }
        }
    }
}