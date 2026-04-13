using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Query;

namespace Fake4Dataverse
{
    /// <summary>
    /// Evaluates <see cref="QueryExpression"/> queries against the in-memory store.
    /// </summary>
    internal sealed class QueryExpressionEvaluator
    {
        internal IClock Clock { get; set; } = SystemClock.Instance;
        internal Guid CallerId { get; set; } = new Guid("00000000-0000-0000-0000-000000000001");
        internal Guid BusinessUnitId { get; set; } = new Guid("00000000-0000-0000-0000-000000000003");

        public EntityCollection Evaluate(QueryExpression query, InMemoryEntityStore store)
        {
            if (query == null) throw new ArgumentNullException(nameof(query));

            var entities = TryIndexedLookup(query, store) ?? store.GetAll(query.EntityName);
            IEnumerable<Entity> results = entities;

            if (query.Criteria != null)
                results = results.Where(e => EvaluateFilter(e, query.Criteria));

            // Apply LinkEntity joins
            if (query.LinkEntities.Count > 0)
                results = ApplyLinkEntities(results, query.LinkEntities, store);

            results = ApplyOrdering(results, query.Orders);

            var list = results.ToList();

            // Apply Distinct
            if (query.Distinct)
                list = DeduplicateEntities(list);

            if (query.TopCount.HasValue && query.TopCount.Value > 0)
                list = list.Take(query.TopCount.Value).ToList();

            // Apply paging
            bool moreRecords = false;
            string? pagingCookie = null;
            if (query.PageInfo != null && query.PageInfo.Count > 0)
            {
                int pageSize = query.PageInfo.Count;
                int pageNumber = query.PageInfo.PageNumber > 0 ? query.PageInfo.PageNumber : 1;
                int skip = (pageNumber - 1) * pageSize;

                moreRecords = list.Count > skip + pageSize;
                list = list.Skip(skip).Take(pageSize).ToList();

                if (moreRecords)
                {
                    pagingCookie = $"<cookie page=\"{pageNumber}\"><accountid last=\"{(list.Count > 0 ? list[list.Count - 1].Id.ToString() : Guid.Empty.ToString())}\" first=\"{(list.Count > 0 ? list[0].Id.ToString() : Guid.Empty.ToString())}\" /></cookie>";
                }
            }

            var projected = list.Select(e => ProjectEntity(e, query.ColumnSet)).ToList();

            var collection = new EntityCollection(projected);
            collection.EntityName = query.EntityName;
            collection.MoreRecords = moreRecords;
            collection.PagingCookie = pagingCookie;
            collection.TotalRecordCount = moreRecords ? -1 : projected.Count;
            return collection;
        }

        private IReadOnlyList<Entity>? TryIndexedLookup(QueryExpression query, InMemoryEntityStore store)
        {
            var index = store.Index;
            if (index == null) return null;
            if (query.Criteria == null || query.Criteria.FilterOperator != LogicalOperator.And) return null;
            if (query.Criteria.Conditions.Count == 0) return null;

            HashSet<Guid>? candidateIds = null;
            foreach (var condition in query.Criteria.Conditions)
            {
                if (condition.Operator != ConditionOperator.Equal || condition.Values.Count == 0)
                    continue;

                var ids = index.Lookup(query.EntityName, condition.AttributeName, condition.Values[0]);
                if (ids == null) continue;

                if (candidateIds == null)
                    candidateIds = ids;
                else
                    candidateIds.IntersectWith(ids);
            }

            if (candidateIds == null) return null;

            var activeTransaction = store.ActiveTransaction;
            if (activeTransaction != null)
            {
                foreach (var staged in activeTransaction.GetEntityChanges(query.EntityName))
                {
                    if (staged.Value.Entity != null)
                        candidateIds.Add(staged.Key);
                }
            }

            return store.GetByIds(query.EntityName, candidateIds);
        }

        private bool EvaluateFilter(Entity entity, FilterExpression filter)
        {
            if (filter == null) return true;

            var conditionResults = filter.Conditions.Select(c => EvaluateCondition(entity, c));
            var subFilterResults = filter.Filters.Select(f => EvaluateFilter(entity, f));
            var allResults = conditionResults.Concat(subFilterResults).ToList();

            if (allResults.Count == 0) return true;

            return filter.FilterOperator == LogicalOperator.And
                ? allResults.All(r => r)
                : allResults.Any(r => r);
        }

        private bool EvaluateCondition(Entity entity, ConditionExpression condition)
        {
            var value = entity.Contains(condition.AttributeName)
                ? entity[condition.AttributeName]
                : null;

            var now = Clock.UtcNow;

            switch (condition.Operator)
            {
                case ConditionOperator.Equal:
                {
                    var condVal = condition.Values.FirstOrDefault();
                    // Empty-string equality also matches null values because
                    // Dataverse normalises empty strings to null on storage.
                    if (condVal is string s && s.Length == 0 && value == null)
                        return true;
                    return NormalizedEquals(value, condVal);
                }
                case ConditionOperator.NotEqual:
                    return !NormalizedEquals(value, condition.Values.FirstOrDefault());
                case ConditionOperator.Null:
                    return value == null;
                case ConditionOperator.NotNull:
                    return value != null;
                case ConditionOperator.GreaterThan:
                    return Compare(value, condition.Values.FirstOrDefault()) > 0;
                case ConditionOperator.GreaterEqual:
                    return Compare(value, condition.Values.FirstOrDefault()) >= 0;
                case ConditionOperator.LessThan:
                    return Compare(value, condition.Values.FirstOrDefault()) < 0;
                case ConditionOperator.LessEqual:
                    return Compare(value, condition.Values.FirstOrDefault()) <= 0;
                case ConditionOperator.Like:
                    return EvaluateLike(value as string, condition.Values.FirstOrDefault() as string);
                case ConditionOperator.NotLike:
                    return !EvaluateLike(value as string, condition.Values.FirstOrDefault() as string);
                case ConditionOperator.In:
                    return condition.Values.Any(v => NormalizedEquals(value, v));
                case ConditionOperator.NotIn:
                    return !condition.Values.Any(v => NormalizedEquals(value, v));
                case ConditionOperator.BeginsWith:
                    return value is string s1 && condition.Values.FirstOrDefault() is string prefix
                        && s1.StartsWith(prefix, StringComparison.OrdinalIgnoreCase);
                case ConditionOperator.EndsWith:
                    return value is string s2 && condition.Values.FirstOrDefault() is string suffix
                        && s2.EndsWith(suffix, StringComparison.OrdinalIgnoreCase);
                case ConditionOperator.DoesNotBeginWith:
                    return !(value is string s5 && condition.Values.FirstOrDefault() is string prefix2
                        && s5.StartsWith(prefix2, StringComparison.OrdinalIgnoreCase));
                case ConditionOperator.DoesNotEndWith:
                    return !(value is string s6 && condition.Values.FirstOrDefault() is string suffix2
                        && s6.EndsWith(suffix2, StringComparison.OrdinalIgnoreCase));
                case ConditionOperator.Contains:
                    return value is string s3 && condition.Values.FirstOrDefault() is string sub
                        && s3.IndexOf(sub, StringComparison.OrdinalIgnoreCase) >= 0;
                case ConditionOperator.DoesNotContain:
                    return !(value is string s4 && condition.Values.FirstOrDefault() is string sub2
                        && s4.IndexOf(sub2, StringComparison.OrdinalIgnoreCase) >= 0);

                // Range operators
                case ConditionOperator.Between:
                    return EvaluateBetween(value, condition.Values, inclusive: true);
                case ConditionOperator.NotBetween:
                    return !EvaluateBetween(value, condition.Values, inclusive: true);

                // Date: single-day operators
                case ConditionOperator.Yesterday:
                    return IsDateInRange(value, now.Date.AddDays(-1), now.Date);
                case ConditionOperator.Today:
                    return IsDateInRange(value, now.Date, now.Date.AddDays(1));
                case ConditionOperator.Tomorrow:
                    return IsDateInRange(value, now.Date.AddDays(1), now.Date.AddDays(2));

                // Date: relative range operators
                case ConditionOperator.Last7Days:
                    return IsDateInRange(value, now.Date.AddDays(-7), now);
                case ConditionOperator.Next7Days:
                    return IsDateInRange(value, now, now.Date.AddDays(8));

                case ConditionOperator.LastWeek:
                    return IsDateInWeekOffset(value, now, -1);
                case ConditionOperator.ThisWeek:
                    return IsDateInWeekOffset(value, now, 0);
                case ConditionOperator.NextWeek:
                    return IsDateInWeekOffset(value, now, 1);

                case ConditionOperator.LastMonth:
                    return IsDateInMonthOffset(value, now, -1);
                case ConditionOperator.ThisMonth:
                    return IsDateInMonthOffset(value, now, 0);
                case ConditionOperator.NextMonth:
                    return IsDateInMonthOffset(value, now, 1);

                case ConditionOperator.LastYear:
                    return IsDateInYearOffset(value, now, -1);
                case ConditionOperator.ThisYear:
                    return IsDateInYearOffset(value, now, 0);
                case ConditionOperator.NextYear:
                    return IsDateInYearOffset(value, now, 1);

                // LastX / NextX
                case ConditionOperator.LastXHours:
                    return IsDateInRange(value, now.AddHours(-ToInt(condition.Values.FirstOrDefault())), now);
                case ConditionOperator.NextXHours:
                    return IsDateInRange(value, now, now.AddHours(ToInt(condition.Values.FirstOrDefault())));
                case ConditionOperator.LastXDays:
                    return IsDateInRange(value, now.AddDays(-ToInt(condition.Values.FirstOrDefault())), now);
                case ConditionOperator.NextXDays:
                    return IsDateInRange(value, now, now.AddDays(ToInt(condition.Values.FirstOrDefault())));
                case ConditionOperator.LastXWeeks:
                    return IsDateInRange(value, now.AddDays(-7 * ToInt(condition.Values.FirstOrDefault())), now);
                case ConditionOperator.NextXWeeks:
                    return IsDateInRange(value, now, now.AddDays(7 * ToInt(condition.Values.FirstOrDefault())));
                case ConditionOperator.LastXMonths:
                    return IsDateInRange(value, now.AddMonths(-ToInt(condition.Values.FirstOrDefault())), now);
                case ConditionOperator.NextXMonths:
                    return IsDateInRange(value, now, now.AddMonths(ToInt(condition.Values.FirstOrDefault())));
                case ConditionOperator.LastXYears:
                    return IsDateInRange(value, now.AddYears(-ToInt(condition.Values.FirstOrDefault())), now);
                case ConditionOperator.NextXYears:
                    return IsDateInRange(value, now, now.AddYears(ToInt(condition.Values.FirstOrDefault())));

                // On / OnOrBefore / OnOrAfter
                case ConditionOperator.On:
                    return IsDateInRange(value, ToDate(condition.Values.FirstOrDefault()), ToDate(condition.Values.FirstOrDefault()).AddDays(1));
                case ConditionOperator.OnOrBefore:
                    return ToDateTime(value) < ToDate(condition.Values.FirstOrDefault()).AddDays(1);
                case ConditionOperator.OnOrAfter:
                    return ToDateTime(value) >= ToDate(condition.Values.FirstOrDefault());

                // OlderThanX
                case ConditionOperator.OlderThanXMinutes:
                    return ToDateTime(value) < now.AddMinutes(-ToInt(condition.Values.FirstOrDefault()));
                case ConditionOperator.OlderThanXHours:
                    return ToDateTime(value) < now.AddHours(-ToInt(condition.Values.FirstOrDefault()));
                case ConditionOperator.OlderThanXDays:
                    return ToDateTime(value) < now.AddDays(-ToInt(condition.Values.FirstOrDefault()));
                case ConditionOperator.OlderThanXWeeks:
                    return ToDateTime(value) < now.AddDays(-7 * ToInt(condition.Values.FirstOrDefault()));
                case ConditionOperator.OlderThanXMonths:
                    return ToDateTime(value) < now.AddMonths(-ToInt(condition.Values.FirstOrDefault()));
                case ConditionOperator.OlderThanXYears:
                    return ToDateTime(value) < now.AddYears(-ToInt(condition.Values.FirstOrDefault()));

                // User context operators
                case ConditionOperator.EqualUserId:
                    return Equals(Normalize(value), CallerId);
                case ConditionOperator.NotEqualUserId:
                    return !Equals(Normalize(value), CallerId);
                case ConditionOperator.EqualBusinessId:
                    return Equals(Normalize(value), BusinessUnitId);
                case ConditionOperator.NotEqualBusinessId:
                    return !Equals(Normalize(value), BusinessUnitId);

                // Multi-select optionset
                case ConditionOperator.ContainValues:
                    return EvaluateContainValues(value, condition.Values);
                case ConditionOperator.DoesNotContainValues:
                    return !EvaluateContainValues(value, condition.Values);

                default:
                    throw new NotSupportedException($"ConditionOperator '{condition.Operator}' is not yet supported by Fake4Dataverse.");
            }
        }

        #region Helpers

        private static List<Entity> DeduplicateEntities(List<Entity> entities)
        {
            var seen = new HashSet<string>(StringComparer.Ordinal);
            var result = new List<Entity>();
            foreach (var e in entities)
            {
                var key = BuildDistinctKey(e);
                if (seen.Add(key))
                    result.Add(e);
            }
            return result;
        }

        private static string BuildDistinctKey(Entity entity)
        {
            // Build a composite key from all attribute values (including aliased)
            // to support DISTINCT with linked entity results
            var parts = new List<string>();
            foreach (var attr in entity.Attributes.OrderBy(a => a.Key, StringComparer.OrdinalIgnoreCase))
            {
                var val = attr.Value;
                if (val is AliasedValue av)
                    val = av.Value;
                val = Normalize(val);
                parts.Add($"{attr.Key}={val}");
            }
            return string.Join("|", parts);
        }

        private static object? Normalize(object? value)
        {
            if (value is EntityReference er) return er.Id;
            if (value is OptionSetValue osv) return osv.Value;
            if (value is Money m) return m.Value;
            return value;
        }

        private static bool NormalizedEquals(object? left, object? right)
        {
            var l = Normalize(left);
            var r = Normalize(right);
            if (l is string sl && r is string sr)
                return string.Equals(sl, sr, StringComparison.OrdinalIgnoreCase);
            return Equals(l, r);
        }

        private static int Compare(object? left, object? right)
        {
            left = Normalize(left);
            right = Normalize(right);
            if (left == null && right == null) return 0;
            if (left == null) return -1;
            if (right == null) return 1;

            if (left is IComparable comparable)
            {
                if (left.GetType() != right.GetType())
                    right = Convert.ChangeType(right, left.GetType());
                return comparable.CompareTo(right);
            }

            return 0;
        }

        private static bool EvaluateLike(string? value, string? pattern)
        {
            if (value == null || pattern == null) return false;

            // Convert LIKE pattern (with % and _ wildcards) to a simple regex-like match.
            // '%' matches zero or more characters; '_' matches exactly one character.
            return MatchLikePattern(value, 0, pattern, 0);
        }

        /// <summary>
        /// Recursive matching of a LIKE pattern with '%' (zero or more chars)
        /// and '_' (exactly one char) wildcards against a value string.
        /// </summary>
        private static bool MatchLikePattern(string value, int vi, string pattern, int pi)
        {
            while (pi < pattern.Length)
            {
                char pc = pattern[pi];

                if (pc == '%')
                {
                    // Skip consecutive '%' characters
                    while (pi < pattern.Length && pattern[pi] == '%')
                        pi++;

                    if (pi == pattern.Length)
                        return true; // trailing % matches everything

                    // Try matching the rest of the pattern from every position in value
                    for (int i = vi; i <= value.Length; i++)
                    {
                        if (MatchLikePattern(value, i, pattern, pi))
                            return true;
                    }
                    return false;
                }

                if (vi >= value.Length)
                    return false;

                if (pc == '_')
                {
                    // '_' matches exactly one character
                    vi++;
                    pi++;
                    continue;
                }

                // Literal character comparison (case-insensitive)
                if (char.ToUpperInvariant(pc) != char.ToUpperInvariant(value[vi]))
                    return false;

                vi++;
                pi++;
            }

            return vi == value.Length;
        }

        private static bool EvaluateBetween(object? value, DataCollection<object> condValues, bool inclusive)
        {
            if (condValues.Count < 2) return false;
            var lo = Compare(value, condValues[0]);
            var hi = Compare(value, condValues[1]);
            return lo >= 0 && hi <= 0;
        }

        private static DateTime ToDateTime(object? value)
        {
            if (value is DateTime dt) return dt;
            if (value is string s && DateTime.TryParse(s, out var parsed)) return parsed;
            return DateTime.MinValue;
        }

        private static DateTime ToDate(object? value)
        {
            return ToDateTime(value).Date;
        }

        private static int ToInt(object? value)
        {
            if (value is int i) return i;
            if (value is long l) return (int)l;
            if (value != null) return Convert.ToInt32(value);
            return 0;
        }

        private static bool IsDateInRange(object? value, DateTime rangeStart, DateTime rangeEnd)
        {
            var dt = ToDateTime(value);
            if (dt == DateTime.MinValue) return false;
            return dt >= rangeStart && dt < rangeEnd;
        }

        private static bool IsDateInWeekOffset(object? value, DateTime now, int weekOffset)
        {
            // Week starts on Monday (ISO).
            int daysSinceMonday = ((int)now.DayOfWeek + 6) % 7;
            var weekStart = now.Date.AddDays(-daysSinceMonday).AddDays(weekOffset * 7);
            var weekEnd = weekStart.AddDays(7);
            return IsDateInRange(value, weekStart, weekEnd);
        }

        private static bool IsDateInMonthOffset(object? value, DateTime now, int monthOffset)
        {
            var target = now.AddMonths(monthOffset);
            var monthStart = new DateTime(target.Year, target.Month, 1);
            var monthEnd = monthStart.AddMonths(1);
            return IsDateInRange(value, monthStart, monthEnd);
        }

        private static bool IsDateInYearOffset(object? value, DateTime now, int yearOffset)
        {
            var targetYear = now.Year + yearOffset;
            var yearStart = new DateTime(targetYear, 1, 1);
            var yearEnd = new DateTime(targetYear + 1, 1, 1);
            return IsDateInRange(value, yearStart, yearEnd);
        }

        private static bool EvaluateContainValues(object? value, DataCollection<object> expected)
        {
            // Multi-select optionset stored as comma-separated string or OptionSetValueCollection
            if (value == null) return false;

            var actualValues = new HashSet<int>();
            if (value is OptionSetValueCollection osvc)
            {
                foreach (var osv in osvc)
                    actualValues.Add(osv.Value);
            }
            else if (value is string csvString)
            {
                foreach (var part in csvString.Split(','))
                {
                    if (int.TryParse(part.Trim(), out var parsed))
                        actualValues.Add(parsed);
                }
            }

            foreach (var exp in expected)
            {
                if (!actualValues.Contains(ToInt(exp)))
                    return false;
            }
            return true;
        }

        #endregion

        private IEnumerable<Entity> ApplyLinkEntities(IEnumerable<Entity> parentRows, DataCollection<LinkEntity> linkEntities, InMemoryEntityStore store)
        {
            IEnumerable<Entity> results = parentRows;
            foreach (var link in linkEntities)
            {
                results = ApplyLinkEntity(results, link, store);
            }
            return results;
        }

        private static bool IsSemiJoin(JoinOperator joinOperator)
        {
            return joinOperator == JoinOperator.Exists
                || joinOperator == JoinOperator.In
                || joinOperator == JoinOperator.Any;
        }

        private static bool IsAntiJoin(JoinOperator joinOperator)
        {
            return joinOperator == JoinOperator.NotAny;
        }

        private static bool IsNotAllJoin(JoinOperator joinOperator)
        {
            return joinOperator == JoinOperator.NotAll;
        }

        private IEnumerable<Entity> ApplyLinkEntity(IEnumerable<Entity> parentRows, LinkEntity link, InMemoryEntityStore store)
        {
            var linkedRows = store.GetAll(link.LinkToEntityName);
            var isOuterJoin = link.JoinOperator == JoinOperator.LeftOuter;
            var isSemiJoin = IsSemiJoin(link.JoinOperator);
            var isAntiJoin = IsAntiJoin(link.JoinOperator);
            var isNotAll = IsNotAllJoin(link.JoinOperator);
            var alias = !string.IsNullOrEmpty(link.EntityAlias) ? link.EntityAlias : link.LinkToEntityName;

            var output = new List<Entity>();

            foreach (var parent in parentRows)
            {
                var parentKey = parent.Contains(link.LinkFromAttributeName)
                    ? Normalize(parent[link.LinkFromAttributeName])
                    : null;

                if (isNotAll)
                {
                    // NotAll: include parent if at least one key-matched child does NOT satisfy LinkCriteria.
                    bool hasNonMatching = false;
                    foreach (var linked in linkedRows)
                    {
                        var linkedKey = linked.Contains(link.LinkToAttributeName)
                            ? Normalize(linked[link.LinkToAttributeName])
                            : null;

                        if (!Equals(parentKey, linkedKey))
                            continue;

                        // Child matches the join key; check if it fails LinkCriteria
                        if (link.LinkCriteria != null && link.LinkCriteria.Conditions.Count + link.LinkCriteria.Filters.Count > 0)
                        {
                            if (!EvaluateFilter(linked, link.LinkCriteria))
                            {
                                hasNonMatching = true;
                                break;
                            }
                        }
                    }

                    if (hasNonMatching)
                    {
                        output.Add(CloneEntityWithAliased(parent));
                    }

                    continue;
                }

                if (isSemiJoin || isAntiJoin)
                {
                    // Semi-join (Exists/In/Any): include parent once if any child matches.
                    // Anti-join (NotAny): include parent only if no child matches.
                    bool anyMatch = false;
                    foreach (var linked in linkedRows)
                    {
                        var linkedKey = linked.Contains(link.LinkToAttributeName)
                            ? Normalize(linked[link.LinkToAttributeName])
                            : null;

                        if (!Equals(parentKey, linkedKey))
                            continue;

                        if (link.LinkCriteria != null && link.LinkCriteria.Conditions.Count + link.LinkCriteria.Filters.Count > 0)
                        {
                            if (!EvaluateFilter(linked, link.LinkCriteria))
                                continue;
                        }

                        anyMatch = true;
                        break;
                    }

                    if (isSemiJoin && anyMatch)
                    {
                        output.Add(CloneEntityWithAliased(parent));
                    }
                    else if (isAntiJoin && !anyMatch)
                    {
                        output.Add(CloneEntityWithAliased(parent));
                    }

                    continue;
                }

                // Inner / LeftOuter join
                bool matched = false;
                foreach (var linked in linkedRows)
                {
                    var linkedKey = linked.Contains(link.LinkToAttributeName)
                        ? Normalize(linked[link.LinkToAttributeName])
                        : null;

                    if (!Equals(parentKey, linkedKey))
                        continue;

                    // Apply LinkCriteria filter
                    if (link.LinkCriteria != null && link.LinkCriteria.Conditions.Count + link.LinkCriteria.Filters.Count > 0)
                    {
                        if (!EvaluateFilter(linked, link.LinkCriteria))
                            continue;
                    }

                    matched = true;
                    var merged = CloneEntityWithAliased(parent);
                    AddAliasedAttributes(merged, linked, alias, link.Columns);

                    // Process nested link entities
                    if (link.LinkEntities.Count > 0)
                    {
                        // For nested links, the linked entity's attributes are needed
                        // as the "from" side for the next join level.
                        var tempKeys = new HashSet<string>();
                        foreach (var attr in linked.Attributes)
                        {
                            if (!merged.Contains(attr.Key))
                            {
                                merged[attr.Key] = InMemoryEntityStore.CloneAttributeValue(attr.Value);
                                tempKeys.Add(attr.Key);
                            }
                        }

                        var nestedResults = ApplyLinkEntities(new[] { merged }, link.LinkEntities, store).ToList();

                        // Clean up temporary attributes
                        foreach (var nr in nestedResults)
                            foreach (var key in tempKeys)
                                nr.Attributes.Remove(key);

                        output.AddRange(nestedResults);
                    }
                    else
                    {
                        output.Add(merged);
                    }
                }

                if (!matched && isOuterJoin)
                {
                    output.Add(CloneEntityWithAliased(parent));
                }
            }

            return output;
        }

        private static Entity CloneEntityWithAliased(Entity source)
        {
            var clone = new Entity(source.LogicalName, source.Id);
            foreach (var attr in source.Attributes)
                clone[attr.Key] = InMemoryEntityStore.CloneAttributeValue(attr.Value);
            return clone;
        }

        private static void AddAliasedAttributes(Entity target, Entity linked, string alias, ColumnSet? columns)
        {
            bool allColumns = columns == null || columns.AllColumns;
            foreach (var attr in linked.Attributes)
            {
                if (!allColumns && !columns!.Columns.Contains(attr.Key))
                    continue;

                var aliasedKey = $"{alias}.{attr.Key}";
                target[aliasedKey] = new AliasedValue(linked.LogicalName, attr.Key, InMemoryEntityStore.CloneAttributeValue(attr.Value));
            }
        }

        private static IEnumerable<Entity> ApplyOrdering(IEnumerable<Entity> entities, DataCollection<OrderExpression> orders)
        {
            if (orders == null || orders.Count == 0)
                return entities;

            IOrderedEnumerable<Entity>? ordered = null;
            foreach (var order in orders)
            {
                if (ordered == null)
                {
                    ordered = order.OrderType == OrderType.Ascending
                        ? entities.OrderBy(e => e.Contains(order.AttributeName) ? Normalize(e[order.AttributeName]) as IComparable : null)
                        : entities.OrderByDescending(e => e.Contains(order.AttributeName) ? Normalize(e[order.AttributeName]) as IComparable : null);
                }
                else
                {
                    ordered = order.OrderType == OrderType.Ascending
                        ? ordered.ThenBy(e => e.Contains(order.AttributeName) ? Normalize(e[order.AttributeName]) as IComparable : null)
                        : ordered.ThenByDescending(e => e.Contains(order.AttributeName) ? Normalize(e[order.AttributeName]) as IComparable : null);
                }
            }

            return ordered ?? entities;
        }

        private static Entity ProjectEntity(Entity source, ColumnSet? columnSet)
        {
            if (columnSet == null || columnSet.AllColumns)
            {
                var clone = new Entity(source.LogicalName, source.Id);
                foreach (var attr in source.Attributes)
                    clone[attr.Key] = InMemoryEntityStore.CloneAttributeValue(attr.Value);
                return clone;
            }

            var projected = new Entity(source.LogicalName, source.Id);
            foreach (var col in columnSet.Columns)
            {
                if (source.Contains(col))
                    projected[col] = InMemoryEntityStore.CloneAttributeValue(source[col]);
            }

            // Always preserve aliased attributes from linked entities
            foreach (var attr in source.Attributes)
            {
                if (attr.Value is AliasedValue)
                    projected[attr.Key] = InMemoryEntityStore.CloneAttributeValue(attr.Value);
            }

            return projected;
        }
    }
}
