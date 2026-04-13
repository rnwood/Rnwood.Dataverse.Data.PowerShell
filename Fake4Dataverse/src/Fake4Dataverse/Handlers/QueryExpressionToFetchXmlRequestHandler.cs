using System;
using System.Xml.Linq;
using Microsoft.Crm.Sdk.Messages;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Query;

namespace Fake4Dataverse.Handlers
{
    /// <summary>
    /// Handles <see cref="QueryExpressionToFetchXmlRequest"/> by converting the
    /// supported <see cref="QueryExpression"/> surface to a FetchXML string.
    /// Unsupported operators and join types throw <see cref="NotSupportedException"/>.
    /// </summary>
    /// <remarks>
    /// <para><strong>Fidelity:</strong> Partial</para>
    /// <para>Produces a FetchXml string representation of the <see cref="QueryExpression"/>. Complex criteria (multi-level nested <c>FilterExpression</c>, link-entity filters) may not serialize to perfectly equivalent FetchXml in all cases.</para>
    /// <para><strong>Configuration:</strong> None — conversion is unconditional.</para>
    /// </remarks>
    internal sealed class QueryExpressionToFetchXmlRequestHandler : IOrganizationRequestHandler
    {
        public bool CanHandle(OrganizationRequest request) =>
            string.Equals(request.RequestName, "QueryExpressionToFetchXml", StringComparison.OrdinalIgnoreCase);

        public OrganizationResponse Handle(OrganizationRequest request, IOrganizationService service)
        {
            var query = (QueryExpression)request["Query"];
            if (query == null)
                throw new ArgumentException("Query parameter is required.");

            var fetchXml = ConvertToFetchXml(query);

            var response = new QueryExpressionToFetchXmlResponse();
            response.Results["FetchXml"] = fetchXml;
            return response;
        }

        private static string ConvertToFetchXml(QueryExpression query)
        {
            var fetchEl = new XElement("fetch",
                new XAttribute("mapping", "logical"));

            if (query.TopCount.HasValue)
                fetchEl.Add(new XAttribute("top", query.TopCount.Value));

            if (query.Distinct)
                fetchEl.Add(new XAttribute("distinct", "true"));

            var entityEl = new XElement("entity",
                new XAttribute("name", query.EntityName));

            // Columns
            if (query.ColumnSet.AllColumns)
            {
                entityEl.Add(new XElement("all-attributes"));
            }
            else
            {
                foreach (var col in query.ColumnSet.Columns)
                    entityEl.Add(new XElement("attribute", new XAttribute("name", col)));
            }

            // Orders
            foreach (var order in query.Orders)
            {
                entityEl.Add(new XElement("order",
                    new XAttribute("attribute", order.AttributeName),
                    new XAttribute("descending", order.OrderType == OrderType.Descending ? "true" : "false")));
            }

            // Filter
            if (query.Criteria != null &&
                (query.Criteria.Conditions.Count > 0 || query.Criteria.Filters.Count > 0))
            {
                entityEl.Add(ConvertFilter(query.Criteria));
            }

            // Link entities
            foreach (var link in query.LinkEntities)
                entityEl.Add(ConvertLinkEntity(link));

            fetchEl.Add(entityEl);
            return fetchEl.ToString();
        }

        private static XElement ConvertFilter(FilterExpression filter)
        {
            var el = new XElement("filter",
                new XAttribute("type", filter.FilterOperator == LogicalOperator.And ? "and" : "or"));

            foreach (var cond in filter.Conditions)
            {
                var condEl = new XElement("condition",
                    new XAttribute("attribute", cond.AttributeName),
                    new XAttribute("operator", GetOperatorString(cond.Operator)));

                if (cond.Values != null && cond.Values.Count > 0)
                {
                    if (cond.Values.Count == 1)
                    {
                        condEl.Add(new XAttribute("value", cond.Values[0]?.ToString() ?? ""));
                    }
                    else
                    {
                        foreach (var val in cond.Values)
                            condEl.Add(new XElement("value", val?.ToString() ?? ""));
                    }
                }

                el.Add(condEl);
            }

            foreach (var child in filter.Filters)
                el.Add(ConvertFilter(child));

            return el;
        }

        private static XElement ConvertLinkEntity(LinkEntity link)
        {
            var el = new XElement("link-entity",
                new XAttribute("name", link.LinkToEntityName),
                new XAttribute("from", link.LinkToAttributeName),
                new XAttribute("to", link.LinkFromAttributeName));

            el.Add(new XAttribute("link-type", GetJoinOperatorString(link.JoinOperator)));

            if (!string.IsNullOrEmpty(link.EntityAlias))
                el.Add(new XAttribute("alias", link.EntityAlias));

            if (link.Columns.AllColumns)
            {
                el.Add(new XElement("all-attributes"));
            }
            else
            {
                foreach (var col in link.Columns.Columns)
                    el.Add(new XElement("attribute", new XAttribute("name", col)));
            }

            if (link.LinkCriteria != null &&
                (link.LinkCriteria.Conditions.Count > 0 || link.LinkCriteria.Filters.Count > 0))
            {
                el.Add(ConvertFilter(link.LinkCriteria));
            }

            foreach (var nested in link.LinkEntities)
                el.Add(ConvertLinkEntity(nested));

            return el;
        }

        private static string GetOperatorString(ConditionOperator op)
        {
            switch (op)
            {
                case ConditionOperator.Equal: return "eq";
                case ConditionOperator.NotEqual: return "ne";
                case ConditionOperator.GreaterThan: return "gt";
                case ConditionOperator.GreaterEqual: return "ge";
                case ConditionOperator.LessThan: return "lt";
                case ConditionOperator.LessEqual: return "le";
                case ConditionOperator.Like: return "like";
                case ConditionOperator.NotLike: return "not-like";
                case ConditionOperator.In: return "in";
                case ConditionOperator.NotIn: return "not-in";
                case ConditionOperator.Null: return "null";
                case ConditionOperator.NotNull: return "not-null";
                case ConditionOperator.Between: return "between";
                case ConditionOperator.NotBetween: return "not-between";
                case ConditionOperator.Contains: return "contain";
                case ConditionOperator.DoesNotContain: return "not-contain";
                case ConditionOperator.BeginsWith: return "begins-with";
                case ConditionOperator.DoesNotBeginWith: return "not-begin-with";
                case ConditionOperator.EndsWith: return "ends-with";
                case ConditionOperator.DoesNotEndWith: return "not-end-with";
                case ConditionOperator.On: return "on";
                case ConditionOperator.OnOrBefore: return "on-or-before";
                case ConditionOperator.OnOrAfter: return "on-or-after";
                case ConditionOperator.Yesterday: return "yesterday";
                case ConditionOperator.Today: return "today";
                case ConditionOperator.Tomorrow: return "tomorrow";
                case ConditionOperator.Last7Days: return "last-seven-days";
                case ConditionOperator.Next7Days: return "next-seven-days";
                case ConditionOperator.LastXDays: return "last-x-days";
                case ConditionOperator.NextXDays: return "next-x-days";
                case ConditionOperator.LastXHours: return "last-x-hours";
                case ConditionOperator.NextXHours: return "next-x-hours";
                case ConditionOperator.LastXWeeks: return "last-x-weeks";
                case ConditionOperator.NextXWeeks: return "next-x-weeks";
                case ConditionOperator.LastXMonths: return "last-x-months";
                case ConditionOperator.NextXMonths: return "next-x-months";
                case ConditionOperator.LastXYears: return "last-x-years";
                case ConditionOperator.NextXYears: return "next-x-years";
                case ConditionOperator.ThisWeek: return "this-week";
                case ConditionOperator.LastWeek: return "last-week";
                case ConditionOperator.NextWeek: return "next-week";
                case ConditionOperator.ThisMonth: return "this-month";
                case ConditionOperator.LastMonth: return "last-month";
                case ConditionOperator.NextMonth: return "next-month";
                case ConditionOperator.ThisYear: return "this-year";
                case ConditionOperator.LastYear: return "last-year";
                case ConditionOperator.NextYear: return "next-year";
                case ConditionOperator.OlderThanXMinutes: return "older-than-x-minutes";
                case ConditionOperator.OlderThanXHours: return "older-than-x-hours";
                case ConditionOperator.OlderThanXDays: return "older-than-x-days";
                case ConditionOperator.OlderThanXWeeks: return "older-than-x-weeks";
                case ConditionOperator.OlderThanXMonths: return "older-than-x-months";
                case ConditionOperator.OlderThanXYears: return "older-than-x-years";
                case ConditionOperator.EqualUserId: return "eq-userid";
                case ConditionOperator.NotEqualUserId: return "ne-userid";
                case ConditionOperator.EqualBusinessId: return "eq-businessid";
                case ConditionOperator.NotEqualBusinessId: return "ne-businessid";
                case ConditionOperator.ContainValues: return "contain-values";
                case ConditionOperator.DoesNotContainValues: return "not-contain-values";
                default:
                    throw new NotSupportedException($"ConditionOperator '{op}' cannot be converted to FetchXml.");
            }
        }

        private static string GetJoinOperatorString(JoinOperator joinOperator)
        {
            switch (joinOperator)
            {
                case JoinOperator.Inner: return "inner";
                case JoinOperator.LeftOuter: return "outer";
                case JoinOperator.Exists: return "exists";
                case JoinOperator.In: return "in";
                case JoinOperator.Any: return "any";
                case JoinOperator.NotAny: return "not-any";
                case JoinOperator.NotAll: return "not-all";
                case JoinOperator.Natural: return "natural";
                default:
                    throw new NotSupportedException($"JoinOperator '{joinOperator}' cannot be converted to FetchXml.");
            }
        }
    }
}
