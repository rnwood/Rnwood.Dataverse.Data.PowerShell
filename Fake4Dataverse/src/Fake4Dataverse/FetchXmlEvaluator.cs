using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Xml.Linq;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Query;

namespace Fake4Dataverse
{
    /// <summary>
    /// Parses FetchXml strings and evaluates them against the in-memory store.
    /// </summary>
    internal sealed class FetchXmlEvaluator
    {
        private readonly QueryExpressionEvaluator _queryEvaluator;

        internal FetchXmlEvaluator(QueryExpressionEvaluator queryEvaluator)
        {
            _queryEvaluator = queryEvaluator;
        }

        public EntityCollection Evaluate(string fetchXml, InMemoryEntityStore store)
        {
            if (string.IsNullOrEmpty(fetchXml))
                throw new ArgumentException("FetchXml cannot be null or empty.", nameof(fetchXml));

            var doc = XDocument.Parse(fetchXml);
            var fetchEl = doc.Root;
            if (fetchEl == null || fetchEl.Name.LocalName != "fetch")
                throw new ArgumentException("FetchXml must have a <fetch> root element.", nameof(fetchXml));

            var entityEl = fetchEl.Element("entity");
            if (entityEl == null)
                throw new ArgumentException("FetchXml must contain an <entity> element.", nameof(fetchXml));

            bool isAggregate = string.Equals(Attr(fetchEl, "aggregate"), "true", StringComparison.OrdinalIgnoreCase);
            int? top = ParseOptionalInt(Attr(fetchEl, "top"));
            int? count = ParseOptionalInt(Attr(fetchEl, "count"));
            int? page = ParseOptionalInt(Attr(fetchEl, "page"));
            bool distinct = string.Equals(Attr(fetchEl, "distinct"), "true", StringComparison.OrdinalIgnoreCase);
            string? noLockRaw = Attr(fetchEl, "no-lock");
            bool noLock = noLockRaw == null || string.Equals(noLockRaw, "true", StringComparison.OrdinalIgnoreCase);

            string entityName = Attr(entityEl, "name")
                ?? throw new ArgumentException("Entity element must have a 'name' attribute.");

            var columns = ParseAttributeElements(entityEl);
            var filter = ParseFilter(entityEl.Element("filter"));
            var orders = ParseOrders(entityEl);
            var linkEntities = ParseLinkEntities(entityEl);

            if (isAggregate)
            {
                return EvaluateAggregate(entityName, columns, filter, linkEntities, store);
            }

            // Convert to QueryExpression and evaluate
            var query = new QueryExpression(entityName);

            if (columns.Count == 0 || columns.Any(c => c.IsAllAttributes))
            {
                query.ColumnSet = new ColumnSet(true);
            }
            else
            {
                query.ColumnSet = new ColumnSet(columns.Where(c => !c.IsAllAttributes).Select(c => c.Name).ToArray());
            }

            if (filter != null)
                query.Criteria = filter;

            foreach (var order in orders)
                query.Orders.Add(order);

            foreach (var link in linkEntities)
                query.LinkEntities.Add(link);

            if (top.HasValue)
                query.TopCount = top.Value;

            if (count.HasValue && count.Value > 0)
            {
                query.PageInfo = new PagingInfo
                {
                    Count = count.Value,
                    PageNumber = page ?? 1
                };
            }

            if (distinct)
                query.Distinct = true;

            query.NoLock = noLock;

            var result = _queryEvaluator.Evaluate(query, store);

            // Apply column aliases: wrap values in AliasedValue keyed by alias
            var aliasedColumns = columns.Where(c => !string.IsNullOrEmpty(c.Alias) && !c.IsAllAttributes).ToList();
            if (aliasedColumns.Count > 0)
            {
                foreach (var entity in result.Entities)
                {
                    foreach (var col in aliasedColumns)
                    {
                        object? value = entity.Contains(col.Name) ? entity[col.Name] : null;
                        entity[col.Alias!] = new AliasedValue(entityName, col.Name, InMemoryEntityStore.CloneAttributeValue(value));
                    }
                }
            }

            return result;
        }

        private EntityCollection EvaluateAggregate(
            string entityName,
            List<FetchAttribute> columns,
            FilterExpression? filter,
            List<LinkEntity> linkEntities,
            InMemoryEntityStore store)
        {
            // Get all rows (filtered)
            var query = new QueryExpression(entityName) { ColumnSet = new ColumnSet(true) };
            if (filter != null)
                query.Criteria = filter;
            foreach (var link in linkEntities)
                query.LinkEntities.Add(link);

            var rawResult = _queryEvaluator.Evaluate(query, store);
            var rows = rawResult.Entities.ToList();

            var groupByColumns = columns.Where(c => c.GroupBy).ToList();
            var aggregateColumns = columns.Where(c => !string.IsNullOrEmpty(c.Aggregate)).ToList();

            if (groupByColumns.Count == 0)
            {
                // Single group — all rows
                var result = new Entity(entityName);
                foreach (var agg in aggregateColumns)
                {
                    result[agg.Alias ?? agg.Name] = new AliasedValue(entityName, agg.Name,
                        ComputeAggregate(rows, agg.Name, agg.Aggregate!));
                }

                var collection = new EntityCollection(new List<Entity> { result });
                collection.EntityName = entityName;
                return collection;
            }

            // Group rows
            var groups = rows.GroupBy(e =>
            {
                var key = new List<object?>();
                foreach (var g in groupByColumns)
                {
                    key.Add(e.Contains(g.Name) ? e[g.Name] : null);
                }
                return new GroupKey(key);
            });

            var resultEntities = new List<Entity>();
            foreach (var group in groups)
            {
                var entity = new Entity(entityName);
                for (int i = 0; i < groupByColumns.Count; i++)
                {
                    var col = groupByColumns[i];
                    entity[col.Alias ?? col.Name] = new AliasedValue(entityName, col.Name, InMemoryEntityStore.CloneAttributeValue(group.Key.Values[i]));
                }

                foreach (var agg in aggregateColumns)
                {
                    entity[agg.Alias ?? agg.Name] = new AliasedValue(entityName, agg.Name,
                        InMemoryEntityStore.CloneAttributeValue(ComputeAggregate(group.ToList(), agg.Name, agg.Aggregate!)));
                }

                resultEntities.Add(entity);
            }

            var coll = new EntityCollection(resultEntities);
            coll.EntityName = entityName;
            return coll;
        }

        private static object? ComputeAggregate(List<Entity> rows, string attributeName, string function)
        {
            switch (function.ToLowerInvariant())
            {
                case "count":
                    return rows.Count;
                case "countcolumn":
                    return rows.Count(r => r.Contains(attributeName) && r[attributeName] != null);
                case "sum":
                    return rows.Where(r => r.Contains(attributeName))
                        .Sum(r => ToDecimal(r[attributeName]));
                case "avg":
                    var avgValues = rows.Where(r => r.Contains(attributeName) && r[attributeName] != null).ToList();
                    if (avgValues.Count == 0) return null;
                    return avgValues.Sum(r => ToDecimal(r[attributeName])) / avgValues.Count;
                case "min":
                    var minValues = rows.Where(r => r.Contains(attributeName) && r[attributeName] != null).ToList();
                    if (minValues.Count == 0) return null;
                    return minValues.Min(r => ToDecimal(r[attributeName]));
                case "max":
                    var maxValues = rows.Where(r => r.Contains(attributeName) && r[attributeName] != null).ToList();
                    if (maxValues.Count == 0) return null;
                    return maxValues.Max(r => ToDecimal(r[attributeName]));
                default:
                    throw new NotSupportedException($"Aggregate function '{function}' is not supported.");
            }
        }

        private static decimal ToDecimal(object? value)
        {
            if (value is Money m) return m.Value;
            if (value is decimal d) return d;
            if (value is int i) return i;
            if (value is long l) return l;
            if (value is double dbl) return (decimal)dbl;
            if (value is float f) return (decimal)f;
            if (value != null) return Convert.ToDecimal(value, CultureInfo.InvariantCulture);
            return 0;
        }

        #region FetchXml Parsing

        private static List<FetchAttribute> ParseAttributeElements(XElement entityEl)
        {
            var attributes = new List<FetchAttribute>();
            foreach (var attrEl in entityEl.Elements("attribute"))
            {
                attributes.Add(new FetchAttribute
                {
                    Name = Attr(attrEl, "name") ?? string.Empty,
                    Alias = Attr(attrEl, "alias"),
                    Aggregate = Attr(attrEl, "aggregate"),
                    GroupBy = string.Equals(Attr(attrEl, "groupby"), "true", StringComparison.OrdinalIgnoreCase)
                });
            }

            if (entityEl.Elements("all-attributes").Any())
            {
                attributes.Add(new FetchAttribute { IsAllAttributes = true });
            }

            return attributes;
        }

        private static FilterExpression? ParseFilter(XElement? filterEl)
        {
            if (filterEl == null) return null;

            var filter = new FilterExpression();
            var type = Attr(filterEl, "type");
            filter.FilterOperator = string.Equals(type, "or", StringComparison.OrdinalIgnoreCase)
                ? LogicalOperator.Or
                : LogicalOperator.And;

            foreach (var condEl in filterEl.Elements("condition"))
            {
                var attrName = Attr(condEl, "attribute") ?? string.Empty;
                var opStr = Attr(condEl, "operator") ?? string.Empty;
                var valueStr = Attr(condEl, "value");
                var op = ParseConditionOperator(opStr);

                if (valueStr != null)
                {
                    filter.AddCondition(attrName, op, ParseTypedValue(valueStr));
                }
                else
                {
                    // Check for child <value> elements (for In, Between, etc.)
                    var childValues = condEl.Elements("value").Select(v => ParseTypedValue(v.Value)).ToArray();
                    if (childValues.Length > 0)
                        filter.AddCondition(attrName, op, childValues);
                    else
                        filter.AddCondition(attrName, op);
                }
            }

            foreach (var subFilterEl in filterEl.Elements("filter"))
            {
                var subFilter = ParseFilter(subFilterEl);
                if (subFilter != null)
                    filter.Filters.Add(subFilter);
            }

            return filter;
        }

        private static List<OrderExpression> ParseOrders(XElement entityEl)
        {
            var orders = new List<OrderExpression>();
            foreach (var orderEl in entityEl.Elements("order"))
            {
                var attr = Attr(orderEl, "attribute") ?? string.Empty;
                var desc = string.Equals(Attr(orderEl, "descending"), "true", StringComparison.OrdinalIgnoreCase);
                orders.Add(new OrderExpression(attr, desc ? OrderType.Descending : OrderType.Ascending));
            }
            return orders;
        }

        private static List<LinkEntity> ParseLinkEntities(XElement parentEl)
        {
            var links = new List<LinkEntity>();
            foreach (var linkEl in parentEl.Elements("link-entity"))
            {
                var linkToEntity = Attr(linkEl, "name") ?? string.Empty;
                var fromAttr = Attr(linkEl, "from") ?? string.Empty;
                var toAttr = Attr(linkEl, "to") ?? string.Empty;
                var linkTypeStr = Attr(linkEl, "link-type");
                var alias = Attr(linkEl, "alias");

                var joinOp = ParseJoinOperator(linkTypeStr);

                var link = new LinkEntity
                {
                    LinkFromEntityName = Attr(parentEl, "name") ?? string.Empty,
                    LinkFromAttributeName = toAttr,   // "to" in FetchXml = from attribute on parent
                    LinkToEntityName = linkToEntity,
                    LinkToAttributeName = fromAttr,    // "from" in FetchXml = attribute on linked entity
                    JoinOperator = joinOp
                };

                if (!string.IsNullOrEmpty(alias))
                    link.EntityAlias = alias;

                var linkAttrs = ParseAttributeElements(linkEl);
                if (linkAttrs.Count > 0 && linkAttrs.Any(a => a.IsAllAttributes))
                {
                    link.Columns = new ColumnSet(true);
                }
                else if (linkAttrs.Count > 0)
                {
                    link.Columns = new ColumnSet(linkAttrs.Select(a => a.Name).ToArray());
                }

                var linkFilter = ParseFilter(linkEl.Element("filter"));
                if (linkFilter != null)
                    link.LinkCriteria = linkFilter;

                // Nested link entities
                foreach (var nestedLink in ParseLinkEntities(linkEl))
                    link.LinkEntities.Add(nestedLink);

                links.Add(link);
            }
            return links;
        }

        private static ConditionOperator ParseConditionOperator(string op)
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
                case "in": return ConditionOperator.In;
                case "not-in": return ConditionOperator.NotIn;
                case "null": return ConditionOperator.Null;
                case "not-null": return ConditionOperator.NotNull;
                case "between": return ConditionOperator.Between;
                case "not-between": return ConditionOperator.NotBetween;
                case "begins-with": return ConditionOperator.BeginsWith;
                case "not-begin-with": return ConditionOperator.DoesNotBeginWith;
                case "ends-with": return ConditionOperator.EndsWith;
                case "not-end-with": return ConditionOperator.DoesNotEndWith;
                case "contain": case "like-with-wildcards": return ConditionOperator.Contains;
                case "not-contain": return ConditionOperator.DoesNotContain;
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

        private static string? Attr(XElement el, string name)
        {
            return el.Attribute(name)?.Value;
        }

        private static int? ParseOptionalInt(string? value)
        {
            if (value != null && int.TryParse(value, NumberStyles.Integer, CultureInfo.InvariantCulture, out var result))
                return result;
            return null;
        }

        #endregion

        private sealed class FetchAttribute
        {
            public string Name { get; set; } = string.Empty;
            public string? Alias { get; set; }
            public string? Aggregate { get; set; }
            public bool GroupBy { get; set; }
            public bool IsAllAttributes { get; set; }
        }

        private sealed class GroupKey : IEquatable<GroupKey>
        {
            public readonly List<object?> Values;

            public GroupKey(List<object?> values)
            {
                Values = values;
            }

            public bool Equals(GroupKey? other)
            {
                if (other == null || Values.Count != other.Values.Count) return false;
                for (int i = 0; i < Values.Count; i++)
                {
                    if (!GroupValueEquals(Values[i], other.Values[i])) return false;
                }
                return true;
            }

            public override bool Equals(object? obj) => Equals(obj as GroupKey);

            public override int GetHashCode()
            {
                unchecked
                {
                    int hash = 17;
                    foreach (var v in Values)
                        hash = hash * 31 + GroupValueHashCode(v);
                    return hash;
                }
            }

            private static bool GroupValueEquals(object? a, object? b)
            {
                if (a is string sa && b is string sb)
                    return string.Equals(sa, sb, StringComparison.OrdinalIgnoreCase);
                return Equals(a, b);
            }

            private static int GroupValueHashCode(object? value)
            {
                if (value is string s)
                    return StringComparer.OrdinalIgnoreCase.GetHashCode(s);
                return value?.GetHashCode() ?? 0;
            }
        }
    }
}
