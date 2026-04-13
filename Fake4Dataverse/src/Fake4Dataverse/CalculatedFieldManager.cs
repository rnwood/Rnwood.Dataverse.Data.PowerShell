using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Query;

namespace Fake4Dataverse
{
    /// <summary>
    /// Manages calculated and rollup field definitions for the fake service.
    /// Calculated fields are evaluated on Retrieve; rollup fields aggregate related entities.
    /// </summary>
    public sealed class CalculatedFieldManager
    {
        private readonly Dictionary<(string EntityName, string AttributeName), Func<Entity, object?>> _calculatedFields =
            new Dictionary<(string, string), Func<Entity, object?>>(new EntityAttributeComparer());

        private readonly List<RollupFieldDefinition> _rollupFields = new List<RollupFieldDefinition>();

        /// <summary>
        /// Registers a calculated field that is auto-computed when entities are retrieved.
        /// </summary>
        /// <param name="entityName">The logical name of the entity.</param>
        /// <param name="attributeName">The logical name of the calculated attribute.</param>
        /// <param name="formula">A function that computes the field value from the entity.</param>
        public void RegisterCalculatedField(string entityName, string attributeName, Func<Entity, object?> formula)
        {
            if (string.IsNullOrEmpty(entityName)) throw new ArgumentException("Entity name is required.", nameof(entityName));
            if (string.IsNullOrEmpty(attributeName)) throw new ArgumentException("Attribute name is required.", nameof(attributeName));
            if (formula == null) throw new ArgumentNullException(nameof(formula));

            _calculatedFields[(entityName, attributeName)] = formula;
        }

        /// <summary>
        /// Registers a rollup field that aggregates values from related entities on retrieve.
        /// </summary>
        /// <param name="entityName">The entity that owns the rollup field.</param>
        /// <param name="attributeName">The rollup attribute name.</param>
        /// <param name="relatedEntity">The related entity to aggregate from.</param>
        /// <param name="relatedAttribute">The attribute on the related entity to aggregate.</param>
        /// <param name="lookupAttribute">The lookup attribute on the related entity that references the parent.</param>
        /// <param name="aggregateType">The aggregate function: Sum, Count, Avg, Min, or Max.</param>
        /// <param name="filter">Optional filter to restrict which related records are included.</param>
        public void RegisterRollupField(
            string entityName,
            string attributeName,
            string relatedEntity,
            string relatedAttribute,
            string lookupAttribute,
            RollupType aggregateType,
            FilterExpression? filter = null)
        {
            if (string.IsNullOrEmpty(entityName)) throw new ArgumentException("Entity name is required.", nameof(entityName));
            if (string.IsNullOrEmpty(attributeName)) throw new ArgumentException("Attribute name is required.", nameof(attributeName));
            if (string.IsNullOrEmpty(relatedEntity)) throw new ArgumentException("Related entity is required.", nameof(relatedEntity));
            if (string.IsNullOrEmpty(lookupAttribute)) throw new ArgumentException("Lookup attribute is required.", nameof(lookupAttribute));

            _rollupFields.Add(new RollupFieldDefinition(
                entityName, attributeName, relatedEntity, relatedAttribute, lookupAttribute, aggregateType, filter));
        }

        /// <summary>
        /// Applies calculated and rollup field values to an entity after retrieval.
        /// </summary>
        internal void ApplyCalculatedFields(Entity entity, InMemoryEntityStore store)
        {
            foreach (var kvp in _calculatedFields)
            {
                if (!string.Equals(kvp.Key.EntityName, entity.LogicalName, StringComparison.OrdinalIgnoreCase))
                    continue;

                entity[kvp.Key.AttributeName] = kvp.Value(entity);
            }

            foreach (var rollup in _rollupFields)
            {
                if (!string.Equals(rollup.EntityName, entity.LogicalName, StringComparison.OrdinalIgnoreCase))
                    continue;

                entity[rollup.AttributeName] = ComputeRollup(rollup, entity, store);
            }
        }

        /// <summary>
        /// Returns true if any calculated or rollup fields are registered.
        /// </summary>
        internal bool HasFields => _calculatedFields.Count > 0 || _rollupFields.Count > 0;

        private static object? ComputeRollup(RollupFieldDefinition rollup, Entity parent, InMemoryEntityStore store)
        {
            var relatedEntities = store.GetAll(rollup.RelatedEntity);
            var matching = new List<Entity>();

            foreach (var related in relatedEntities)
            {
                var lookupRef = related.GetAttributeValue<EntityReference>(rollup.LookupAttribute);
                if (lookupRef == null || lookupRef.Id != parent.Id)
                    continue;

                if (rollup.Filter != null && !EvaluateSimpleFilter(related, rollup.Filter))
                    continue;

                matching.Add(related);
            }

            if (rollup.AggregateType == RollupType.Count)
                return matching.Count;

            var values = new List<decimal>();
            foreach (var m in matching)
            {
                var val = m.Contains(rollup.RelatedAttribute) ? m[rollup.RelatedAttribute] : null;
                if (val is Money money)
                    values.Add(money.Value);
                else if (val is int intVal)
                    values.Add(intVal);
                else if (val is decimal decVal)
                    values.Add(decVal);
                else if (val is double dblVal)
                    values.Add((decimal)dblVal);
                else if (val is float fltVal)
                    values.Add((decimal)fltVal);
            }

            if (values.Count == 0)
                return null;

            switch (rollup.AggregateType)
            {
                case RollupType.Sum: return values.Sum();
                case RollupType.Avg: return values.Average();
                case RollupType.Min: return values.Min();
                case RollupType.Max: return values.Max();
                default: return null;
            }
        }

        private static bool EvaluateSimpleFilter(Entity entity, FilterExpression filter)
        {
            bool result = filter.FilterOperator == LogicalOperator.And;

            foreach (var condition in filter.Conditions)
            {
                bool match = false;
                var val = entity.Contains(condition.AttributeName) ? entity[condition.AttributeName] : null;

                switch (condition.Operator)
                {
                    case ConditionOperator.Equal:
                        match = ValuesEqual(val, condition.Values.Count > 0 ? condition.Values[0] : null);
                        break;
                    case ConditionOperator.NotEqual:
                        match = !ValuesEqual(val, condition.Values.Count > 0 ? condition.Values[0] : null);
                        break;
                    case ConditionOperator.GreaterThan:
                        match = CompareValues(val, condition.Values.Count > 0 ? condition.Values[0] : null) > 0;
                        break;
                    case ConditionOperator.LessThan:
                        match = CompareValues(val, condition.Values.Count > 0 ? condition.Values[0] : null) < 0;
                        break;
                    case ConditionOperator.Null:
                        match = val == null;
                        break;
                    case ConditionOperator.NotNull:
                        match = val != null;
                        break;
                    default:
                        match = true;
                        break;
                }

                if (filter.FilterOperator == LogicalOperator.And)
                    result = result && match;
                else
                    result = result || match;
            }

            return result;
        }

        private static int CompareValues(object? a, object? b)
        {
            if (a == null && b == null) return 0;
            if (a == null) return -1;
            if (b == null) return 1;

            var da = ToDecimal(a);
            var db = ToDecimal(b);
            if (da.HasValue && db.HasValue)
                return da.Value.CompareTo(db.Value);

            return 0;
        }

        private static bool ValuesEqual(object? a, object? b)
        {
            if (Equals(a, b)) return true;

            // Handle OptionSetValue vs int comparison
            var da = ToDecimal(a);
            var db = ToDecimal(b);
            if (da.HasValue && db.HasValue)
                return da.Value == db.Value;

            return false;
        }

        private static decimal? ToDecimal(object? val)
        {
            if (val is Money m) return m.Value;
            if (val is int i) return i;
            if (val is decimal d) return d;
            if (val is double dbl) return (decimal)dbl;
            if (val is float f) return (decimal)f;
            if (val is OptionSetValue osv) return osv.Value;
            return null;
        }

        private sealed class EntityAttributeComparer : IEqualityComparer<(string EntityName, string AttributeName)>
        {
            public bool Equals((string EntityName, string AttributeName) x, (string EntityName, string AttributeName) y) =>
                string.Equals(x.EntityName, y.EntityName, StringComparison.OrdinalIgnoreCase) &&
                string.Equals(x.AttributeName, y.AttributeName, StringComparison.OrdinalIgnoreCase);

            public int GetHashCode((string EntityName, string AttributeName) obj) =>
                StringComparer.OrdinalIgnoreCase.GetHashCode(obj.EntityName) ^
                StringComparer.OrdinalIgnoreCase.GetHashCode(obj.AttributeName);
        }

        private sealed class RollupFieldDefinition
        {
            public string EntityName { get; }
            public string AttributeName { get; }
            public string RelatedEntity { get; }
            public string RelatedAttribute { get; }
            public string LookupAttribute { get; }
            public RollupType AggregateType { get; }
            public FilterExpression? Filter { get; }

            public RollupFieldDefinition(
                string entityName, string attributeName,
                string relatedEntity, string relatedAttribute, string lookupAttribute,
                RollupType aggregateType, FilterExpression? filter)
            {
                EntityName = entityName;
                AttributeName = attributeName;
                RelatedEntity = relatedEntity;
                RelatedAttribute = relatedAttribute;
                LookupAttribute = lookupAttribute;
                AggregateType = aggregateType;
                Filter = filter;
            }
        }
    }

    /// <summary>
    /// Specifies the aggregate function for a rollup field.
    /// </summary>
    public enum RollupType
    {
        /// <summary>Sum of related values.</summary>
        Sum,
        /// <summary>Count of related records.</summary>
        Count,
        /// <summary>Average of related values.</summary>
        Avg,
        /// <summary>Minimum of related values.</summary>
        Min,
        /// <summary>Maximum of related values.</summary>
        Max
    }
}
