using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Xml.Linq;
using Microsoft.Crm.Sdk.Messages;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Query;

namespace Fake4Dataverse.Handlers
{
    /// <summary>
    /// Handles the <c>FetchXmlToQueryExpression</c> request by converting a
    /// supported non-aggregate FetchXml string to a <see cref="QueryExpression"/>.
    /// </summary>
    /// <remarks>
    /// <para><strong>Fidelity:</strong> Partial</para>
    /// <para>Converts non-aggregate FetchXml to a <see cref="QueryExpression"/> using the built-in <see cref="FetchXmlEvaluator"/> parser. Aggregate FetchXml (with <c>aggregate="true"</c>) is not convertible and throws <see cref="NotSupportedException"/>.</para>
    /// <para><strong>Configuration:</strong> None — conversion is unconditional.</para>
    /// </remarks>
    internal sealed class FetchXmlToQueryExpressionRequestHandler : IOrganizationRequestHandler
    {
        /// <inheritdoc />
        public bool CanHandle(OrganizationRequest request) =>
            string.Equals(request.RequestName, "FetchXmlToQueryExpression", StringComparison.OrdinalIgnoreCase);

        /// <inheritdoc />
        public OrganizationResponse Handle(OrganizationRequest request, IOrganizationService service)
        {
            var fetchXml = (string)request["FetchXml"];
            if (string.IsNullOrEmpty(fetchXml))
                throw new ArgumentException("FetchXml is required.");

            var doc = XDocument.Parse(fetchXml);
            var fetchEl = doc.Root;
            if (fetchEl == null || fetchEl.Name.LocalName != "fetch")
                throw new InvalidOperationException("FetchXml must have a <fetch> root element.");

            var entityElement = fetchEl.Element("entity");
            if (entityElement == null)
                throw new InvalidOperationException("FetchXml must contain an entity element.");

            // Aggregates cannot be represented in QueryExpression
            if (string.Equals(Attr(fetchEl, "aggregate"), "true", StringComparison.OrdinalIgnoreCase))
                throw new NotSupportedException("Aggregate FetchXml cannot be converted to QueryExpression.");

            var entityName = entityElement.Attribute("name")?.Value
                ?? throw new InvalidOperationException("Entity name is required.");

            var query = new QueryExpression(entityName);

            // Parse columns
            foreach (var attr in entityElement.Elements("attribute"))
            {
                var name = attr.Attribute("name")?.Value;
                if (!string.IsNullOrEmpty(name))
                    query.ColumnSet.AddColumn(name);
            }

            if (entityElement.Element("all-attributes") != null)
                query.ColumnSet.AllColumns = true;
            else if (query.ColumnSet.Columns.Count == 0)
                query.ColumnSet.AllColumns = true;

            // Parse filter
            var filterElement = entityElement.Element("filter");
            if (filterElement != null)
                ParseFilter(filterElement, query.Criteria);

            // Parse order
            foreach (var order in entityElement.Elements("order"))
            {
                var attrName = order.Attribute("attribute")?.Value;
                var descending = string.Equals(order.Attribute("descending")?.Value, "true", StringComparison.OrdinalIgnoreCase);
                if (!string.IsNullOrEmpty(attrName))
                    query.AddOrder(attrName, descending ? OrderType.Descending : OrderType.Ascending);
            }

            // Parse top
            var topAttr = fetchEl.Attribute("top");
            if (topAttr != null && int.TryParse(topAttr.Value, out var topVal))
                query.TopCount = topVal;

            // Parse paging (count + page)
            var countAttr = fetchEl.Attribute("count");
            if (countAttr != null && int.TryParse(countAttr.Value, out var count) && count > 0)
            {
                var pageAttr = fetchEl.Attribute("page");
                int page = 1;
                if (pageAttr != null && int.TryParse(pageAttr.Value, out var parsedPage) && parsedPage > 0)
                    page = parsedPage;

                query.PageInfo = new PagingInfo
                {
                    Count = count,
                    PageNumber = page
                };
            }

            // Parse distinct
            var distinctAttr = fetchEl.Attribute("distinct");
            if (distinctAttr != null && string.Equals(distinctAttr.Value, "true", StringComparison.OrdinalIgnoreCase))
                query.Distinct = true;

            // Parse no-lock
            var noLockAttr = fetchEl.Attribute("no-lock");
            if (noLockAttr != null && string.Equals(noLockAttr.Value, "true", StringComparison.OrdinalIgnoreCase))
                query.NoLock = true;

            // Parse link-entities
            foreach (var linkEl in entityElement.Elements("link-entity"))
                ParseLinkEntity(linkEl, query.LinkEntities, entityName);

            var response = new FetchXmlToQueryExpressionResponse();
            response["Query"] = query;
            return response;
        }

        private static void ParseFilter(XElement filterElement, FilterExpression filter)
        {
            var type = filterElement.Attribute("type")?.Value;
            filter.FilterOperator = string.Equals(type, "or", StringComparison.OrdinalIgnoreCase)
                ? LogicalOperator.Or
                : LogicalOperator.And;

            foreach (var condition in filterElement.Elements("condition"))
            {
                var attribute = condition.Attribute("attribute")?.Value;
                var operatorStr = condition.Attribute("operator")?.Value;
                var valueStr = condition.Attribute("value")?.Value;
                var entityname = condition.Attribute("entityname")?.Value;

                if (string.IsNullOrEmpty(attribute) || string.IsNullOrEmpty(operatorStr))
                    continue;

                var op = ParseOperator(operatorStr!);

                var cond = new ConditionExpression();
                cond.AttributeName = attribute!;
                cond.Operator = op;

                if (!string.IsNullOrEmpty(entityname))
                    cond.EntityName = entityname;

                if (valueStr != null)
                {
                    cond.Values.Add(ParseTypedValue(valueStr));
                }
                else
                {
                    // Check for child <value> elements (for In, Between, etc.)
                    var childValues = condition.Elements("value").Select(v => ParseTypedValue(v.Value)).ToArray();
                    if (childValues.Length > 0)
                    {
                        foreach (var cv in childValues)
                            cond.Values.Add(cv);
                    }
                }

                filter.AddCondition(cond);
            }

            foreach (var subFilter in filterElement.Elements("filter"))
            {
                var child = new FilterExpression();
                ParseFilter(subFilter, child);
                filter.AddFilter(child);
            }
        }

        private static void ParseLinkEntity(XElement linkEl, DataCollection<LinkEntity> linkEntities, string parentEntityName)
        {
            var linkToEntity = linkEl.Attribute("name")?.Value ?? string.Empty;
            var fromAttr = linkEl.Attribute("from")?.Value ?? string.Empty;
            var toAttr = linkEl.Attribute("to")?.Value ?? string.Empty;
            var linkTypeStr = linkEl.Attribute("link-type")?.Value;
            var alias = linkEl.Attribute("alias")?.Value;

            var joinOp = ParseJoinOperator(linkTypeStr);

            var link = new LinkEntity
            {
                LinkFromEntityName = parentEntityName,
                LinkFromAttributeName = toAttr,
                LinkToEntityName = linkToEntity,
                LinkToAttributeName = fromAttr,
                JoinOperator = joinOp
            };

            if (!string.IsNullOrEmpty(alias))
                link.EntityAlias = alias;

            // Parse columns
            foreach (var attr in linkEl.Elements("attribute"))
            {
                var name = attr.Attribute("name")?.Value;
                if (!string.IsNullOrEmpty(name))
                    link.Columns.AddColumn(name);
            }

            if (linkEl.Element("all-attributes") != null)
                link.Columns.AllColumns = true;

            // Parse filter
            var filterElement = linkEl.Element("filter");
            if (filterElement != null)
                ParseFilter(filterElement, link.LinkCriteria);

            // Parse order
            foreach (var order in linkEl.Elements("order"))
            {
                var attrName = order.Attribute("attribute")?.Value;
                var descending = string.Equals(order.Attribute("descending")?.Value, "true", StringComparison.OrdinalIgnoreCase);
                if (!string.IsNullOrEmpty(attrName))
                    link.Orders.Add(new OrderExpression(attrName, descending ? OrderType.Descending : OrderType.Ascending));
            }

            // Nested link entities
            foreach (var nestedLink in linkEl.Elements("link-entity"))
                ParseLinkEntity(nestedLink, link.LinkEntities, linkToEntity);

            linkEntities.Add(link);
        }

        private static JoinOperator ParseJoinOperator(string? linkType)
        {
            if (string.IsNullOrEmpty(linkType))
                return JoinOperator.Inner;

            switch (linkType!.ToLowerInvariant())
            {
                case "inner": return JoinOperator.Inner;
                case "outer": return JoinOperator.LeftOuter;
                case "exists": return JoinOperator.Exists;
                case "in": return JoinOperator.In;
                case "any": return JoinOperator.Any;
                case "not-any": return JoinOperator.NotAny;
                case "not-all": return JoinOperator.NotAll;
                case "natural": return JoinOperator.Natural;
                default:
                    throw new NotSupportedException($"FetchXml link-type '{linkType}' is not supported.");
            }
        }

        private static object ParseTypedValue(string valueStr)
        {
            if (int.TryParse(valueStr, NumberStyles.Integer, CultureInfo.InvariantCulture, out var intVal))
                return intVal;
            if (decimal.TryParse(valueStr, NumberStyles.Number, CultureInfo.InvariantCulture, out var decVal))
                return decVal;
            if (DateTime.TryParse(valueStr, CultureInfo.InvariantCulture, DateTimeStyles.None, out var dtVal))
                return dtVal;
            if (Guid.TryParse(valueStr, out var guidVal))
                return guidVal;
            return valueStr;
        }

        private static ConditionOperator ParseOperator(string op)
        {
            switch (op.ToLowerInvariant())
            {
                case "eq": return ConditionOperator.Equal;
                case "ne": case "neq": return ConditionOperator.NotEqual;
                case "gt": return ConditionOperator.GreaterThan;
                case "ge": case "gte": return ConditionOperator.GreaterEqual;
                case "lt": return ConditionOperator.LessThan;
                case "le": case "lte": return ConditionOperator.LessEqual;
                case "like": return ConditionOperator.Like;
                case "not-like": return ConditionOperator.NotLike;
                case "null": return ConditionOperator.Null;
                case "not-null": return ConditionOperator.NotNull;
                case "in": return ConditionOperator.In;
                case "not-in": return ConditionOperator.NotIn;
                case "begins-with": return ConditionOperator.BeginsWith;
                case "not-begin-with": return ConditionOperator.DoesNotBeginWith;
                case "ends-with": return ConditionOperator.EndsWith;
                case "not-end-with": return ConditionOperator.DoesNotEndWith;
                case "contain": case "contains": return ConditionOperator.Contains;
                case "not-contain": return ConditionOperator.DoesNotContain;
                case "between": return ConditionOperator.Between;
                case "not-between": return ConditionOperator.NotBetween;
                case "on": return ConditionOperator.On;
                case "on-or-before": return ConditionOperator.OnOrBefore;
                case "on-or-after": return ConditionOperator.OnOrAfter;
                case "yesterday": return ConditionOperator.Yesterday;
                case "today": return ConditionOperator.Today;
                case "tomorrow": return ConditionOperator.Tomorrow;
                case "last-seven-days": return ConditionOperator.Last7Days;
                case "next-seven-days": return ConditionOperator.Next7Days;
                case "last-x-days": return ConditionOperator.LastXDays;
                case "next-x-days": return ConditionOperator.NextXDays;
                case "last-x-hours": return ConditionOperator.LastXHours;
                case "next-x-hours": return ConditionOperator.NextXHours;
                case "last-x-weeks": return ConditionOperator.LastXWeeks;
                case "next-x-weeks": return ConditionOperator.NextXWeeks;
                case "last-x-months": return ConditionOperator.LastXMonths;
                case "next-x-months": return ConditionOperator.NextXMonths;
                case "last-x-years": return ConditionOperator.LastXYears;
                case "next-x-years": return ConditionOperator.NextXYears;
                case "this-week": return ConditionOperator.ThisWeek;
                case "last-week": return ConditionOperator.LastWeek;
                case "next-week": return ConditionOperator.NextWeek;
                case "this-month": return ConditionOperator.ThisMonth;
                case "last-month": return ConditionOperator.LastMonth;
                case "next-month": return ConditionOperator.NextMonth;
                case "this-year": return ConditionOperator.ThisYear;
                case "last-year": return ConditionOperator.LastYear;
                case "next-year": return ConditionOperator.NextYear;
                case "older-than-x-minutes": return ConditionOperator.OlderThanXMinutes;
                case "older-than-x-hours": return ConditionOperator.OlderThanXHours;
                case "older-than-x-days": return ConditionOperator.OlderThanXDays;
                case "older-than-x-weeks": return ConditionOperator.OlderThanXWeeks;
                case "older-than-x-months": return ConditionOperator.OlderThanXMonths;
                case "older-than-x-years": return ConditionOperator.OlderThanXYears;
                case "eq-userid": return ConditionOperator.EqualUserId;
                case "ne-userid": return ConditionOperator.NotEqualUserId;
                case "eq-businessid": return ConditionOperator.EqualBusinessId;
                case "ne-businessid": return ConditionOperator.NotEqualBusinessId;
                case "contain-values": return ConditionOperator.ContainValues;
                case "not-contain-values": return ConditionOperator.DoesNotContainValues;
                default:
                    throw new NotSupportedException($"FetchXml condition operator '{op}' is not supported.");
            }
        }

        private static string? Attr(XElement el, string name)
        {
            return el.Attribute(name)?.Value;
        }
    }
}
