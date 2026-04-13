using System;
using System.Globalization;
using System.Linq;
using System.Xml.Linq;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Query;

namespace Fake4Dataverse.Handlers
{
    internal static class FetchExecutionHelper
    {
        internal static string ExecuteAndSerialize(IOrganizationService service, string fetchXml)
        {
            if (string.IsNullOrWhiteSpace(fetchXml))
                throw new ArgumentException("FetchXml is required.", nameof(fetchXml));

            var result = service.RetrieveMultiple(new FetchExpression(fetchXml));
            return Serialize(result);
        }

        private static string Serialize(EntityCollection result)
        {
            var root = new XElement("resultset",
                new XAttribute("morerecords", result.MoreRecords ? "1" : "0"),
                new XAttribute("count", result.Entities.Count),
                new XAttribute("entityname", result.EntityName ?? string.Empty));

            foreach (var entity in result.Entities)
            {
                var resultElement = new XElement("result",
                    new XAttribute("logicalname", entity.LogicalName));

                if (entity.Id != Guid.Empty)
                {
                    resultElement.Add(new XAttribute("id", entity.Id.ToString("D")));

                    var primaryIdAttribute = entity.LogicalName + "id";
                    if (!entity.Attributes.ContainsKey(primaryIdAttribute))
                    {
                        resultElement.Add(new XElement(primaryIdAttribute, entity.Id.ToString("D")));
                    }
                }

                foreach (var attribute in entity.Attributes.OrderBy(kvp => kvp.Key, StringComparer.OrdinalIgnoreCase))
                {
                    resultElement.Add(CreateAttributeElement(attribute.Key, attribute.Value));
                }

                root.Add(resultElement);
            }

            return new XDocument(root).ToString(SaveOptions.DisableFormatting);
        }

        private static XElement CreateAttributeElement(string attributeName, object? value)
        {
            if (value is AliasedValue aliasedValue)
                value = aliasedValue.Value;

            if (value is EntityReference entityReference)
            {
                var element = new XElement(attributeName, entityReference.Id.ToString("D"));
                element.SetAttributeValue("entityname", entityReference.LogicalName);
                if (!string.IsNullOrWhiteSpace(entityReference.Name))
                    element.SetAttributeValue("name", entityReference.Name);

                return element;
            }

            return new XElement(attributeName, ConvertValueToString(value));
        }

        private static string ConvertValueToString(object? value)
        {
            if (value == null)
                return string.Empty;

            if (value is Money money)
                return money.Value.ToString(CultureInfo.InvariantCulture);

            if (value is OptionSetValue optionSetValue)
                return optionSetValue.Value.ToString(CultureInfo.InvariantCulture);

            if (value is OptionSetValueCollection optionSetValues)
                return string.Join(",", optionSetValues.Select(v => v.Value.ToString(CultureInfo.InvariantCulture)));

            if (value is DateTime dateTime)
                return dateTime.ToString("o", CultureInfo.InvariantCulture);

            if (value is bool boolean)
                return boolean ? "true" : "false";

            if (value is byte[] bytes)
                return Convert.ToBase64String(bytes);

            return Convert.ToString(value, CultureInfo.InvariantCulture) ?? string.Empty;
        }
    }
}