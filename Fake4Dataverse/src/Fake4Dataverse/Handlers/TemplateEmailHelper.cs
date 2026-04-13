using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Query;

namespace Fake4Dataverse.Handlers
{
    internal static class TemplateEmailHelper
    {
        private static readonly Regex TokenRegex = new Regex(@"\{!\s*(?<entity>[A-Za-z0-9_]+)\s*:\s*(?<attribute>[A-Za-z0-9_]+)\s*;\}", RegexOptions.Compiled | RegexOptions.IgnoreCase);

        internal static Entity BuildEmailFromTemplate(
            IOrganizationService service,
            Guid templateId,
            string? regardingType,
            Guid regardingId,
            Entity? targetEmail,
            Guid[]? recipientIds,
            string? recipientType,
            EntityReference? sender)
        {
            var email = new Entity("email");

            if (targetEmail != null)
            {
                foreach (var attribute in targetEmail.Attributes)
                    email[attribute.Key] = InMemoryEntityStore.CloneAttributeValue(attribute.Value);
            }

            var template = TryRetrieve(service, "template", templateId);
            var regarding = !string.IsNullOrWhiteSpace(regardingType) && regardingId != Guid.Empty
                ? TryRetrieve(service, regardingType!, regardingId)
                : null;

            if (templateId != Guid.Empty)
                email["templateid"] = new EntityReference("template", templateId);

            if (regarding != null)
            {
                email["regardingobjectid"] = new EntityReference(regarding.LogicalName, regarding.Id);
            }
            else if (!string.IsNullOrWhiteSpace(regardingType) && regardingId != Guid.Empty)
            {
                email["regardingobjectid"] = new EntityReference(regardingType!, regardingId);
            }

            var subjectTemplate = template?.GetAttributeValue<string>("subject");
            var bodyTemplate = template?.GetAttributeValue<string>("body");

            if (!string.IsNullOrEmpty(subjectTemplate))
                email["subject"] = RenderTemplate(subjectTemplate!, regardingType, regarding);

            if (bodyTemplate != null)
                email["description"] = RenderTemplate(bodyTemplate, regardingType, regarding);

            if (!email.Contains("subject"))
                email["subject"] = "Template Email";

            if (!email.Contains("description"))
                email["description"] = string.Empty;

            if (sender != null)
                email["from"] = CreateActivityParties(new[] { sender }, 1);

            if (recipientIds != null && recipientIds.Length > 0 && !string.IsNullOrWhiteSpace(recipientType))
            {
                var recipients = new List<EntityReference>(recipientIds.Length);
                foreach (var recipientId in recipientIds)
                    recipients.Add(new EntityReference(recipientType!, recipientId));

                email["to"] = CreateActivityParties(recipients, 2);
            }

            return email;
        }

        internal static void MarkAsSent(Entity email)
        {
            email["statecode"] = new OptionSetValue(1);
            email["statuscode"] = new OptionSetValue(3);
        }

        private static EntityCollection CreateActivityParties(IEnumerable<EntityReference> parties, int participationTypeMask)
        {
            var entities = new List<Entity>();
            foreach (var party in parties)
            {
                entities.Add(new Entity("activityparty")
                {
                    ["partyid"] = party,
                    ["participationtypemask"] = new OptionSetValue(participationTypeMask)
                });
            }

            return new EntityCollection(entities);
        }

        private static string RenderTemplate(string templateText, string? regardingType, Entity? regarding)
        {
            return TokenRegex.Replace(templateText, match =>
            {
                if (regarding == null || string.IsNullOrWhiteSpace(regardingType))
                    return match.Value;

                var entityName = match.Groups["entity"].Value;
                if (!string.Equals(entityName, regardingType, StringComparison.OrdinalIgnoreCase))
                    return match.Value;

                var attributeName = match.Groups["attribute"].Value;
                return FormatAttributeValue(regarding.Contains(attributeName) ? regarding[attributeName] : null);
            });
        }

        private static string FormatAttributeValue(object? value)
        {
            if (value == null)
                return string.Empty;

            if (value is AliasedValue aliasedValue)
                return FormatAttributeValue(aliasedValue.Value);

            if (value is string text)
                return text;

            if (value is Money money)
                return money.Value.ToString(System.Globalization.CultureInfo.InvariantCulture);

            if (value is OptionSetValue optionSetValue)
                return optionSetValue.Value.ToString(System.Globalization.CultureInfo.InvariantCulture);

            if (value is EntityReference entityReference)
                return !string.IsNullOrWhiteSpace(entityReference.Name)
                    ? entityReference.Name!
                    : entityReference.Id.ToString();

            if (value is DateTime dateTime)
                return dateTime.ToString("o", System.Globalization.CultureInfo.InvariantCulture);

            if (value is bool boolean)
                return boolean ? bool.TrueString : bool.FalseString;

            return Convert.ToString(value, System.Globalization.CultureInfo.InvariantCulture) ?? string.Empty;
        }

        private static Entity? TryRetrieve(IOrganizationService service, string entityName, Guid id)
        {
            if (id == Guid.Empty)
                return null;

            try
            {
                return service.Retrieve(entityName, id, new ColumnSet(true));
            }
            catch
            {
                return null;
            }
        }
    }
}