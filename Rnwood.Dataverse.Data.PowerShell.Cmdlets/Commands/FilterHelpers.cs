using Microsoft.Xrm.Sdk.Query;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Management.Automation;
using System.Xml.Linq;
using Rnwood.Dataverse.Data.PowerShell.Model;

namespace Rnwood.Dataverse.Data.PowerShell.Commands
{
    internal static class FilterHelpers
    {
        private const int MaxXorItems = 8;

        public static void ProcessHashFilterValues(FilterExpression parentFilterExpression, Hashtable[] filterValuesArray, bool isExcludeFilter)
        {
            foreach (Hashtable filterValues in filterValuesArray)
            {
                // Each hashtable represents a single sub-filter; by default its
                // entries are combined with AND. We create a container filter for
                // the hashtable so it integrates correctly with the parent
                // expression which may combine multiple hashtables with OR/AND.

                foreach (DictionaryEntry filterValue in filterValues)
                {
                    string key = filterValue.Key.ToString();

                    // Support grouped subfilters with keys 'and' or 'or' whose
                    // value is an array (or single) of hashtables. This allows
                    // infinite depth recursion of groups such as:
                    // @{ 'and' = @(@{a=1}, @{ 'or' = @(@{b=2}, @{c=3}) }) }
                    if (string.Equals(key, "and", StringComparison.OrdinalIgnoreCase)
                        || string.Equals(key, "or", StringComparison.OrdinalIgnoreCase))
                    {
                        LogicalOperator groupOperator = string.Equals(key, "and", StringComparison.OrdinalIgnoreCase) ? LogicalOperator.And : LogicalOperator.Or;

                        // Normalize the value into a Hashtable[] so we can recurse
                        // regardless of whether the caller supplied a single
                        // hashtable or an array/list of them.
                        List<Hashtable> nested = new List<Hashtable>();
                        if (filterValue.Value is Hashtable singleHt)
                        {
                            nested.Add(singleHt);
                        }
                        else if (filterValue.Value is IEnumerable enumerable)
                        {
                            foreach (object o in enumerable)
                            {
                                try
                                {
                                    Hashtable nh = ToHashtable(o);
                                    nested.Add(nh);
                                }
                                catch (InvalidDataException e)
                                {
                                    throw new InvalidDataException($"Grouped filter operator '{key}' must contain hashtables. {e.Message}");
                                }
                            }
                        }
                        else
                        {
                            throw new InvalidDataException($"Grouped filter operator '{key}' must contain a hashtable or an array of hashtables.");
                        }

                        // For exclude filters we must invert the group logical
                        // operator to implement a NOT over the whole group using
                        // De Morgan's laws (e.g. NOT(A OR B) == (NOT A) AND
                        // (NOT B)). The leaf operators are still inverted when
                        // isExcludeFilter is true so flipping the group operator
                        // produces the correct semantics.
                        LogicalOperator effectiveGroupOperator = groupOperator;
                        if (isExcludeFilter)
                        {
                            effectiveGroupOperator = groupOperator == LogicalOperator.And ? LogicalOperator.Or : LogicalOperator.And;
                        }

                        // Create a grouping filter under the container with the
                        // effective logical operator, then recurse to process the
                        // nested hashtables into that group.
                        FilterExpression groupFilter = parentFilterExpression.AddFilter(effectiveGroupOperator);
                        ProcessHashFilterValues(groupFilter, nested.ToArray(), isExcludeFilter);

                        continue;
                    }

                    // Support a 'not' operator to negate a nested expression.
                    // The semantics are: NOT(innerExpression). For a simple
                    // hashtable inner expression (multiple fields combined by
                    // AND) this is converted to an OR of the negated leaf
                    // conditions per De Morgan's laws. If the inner expression
                    // is itself a grouped expression (e.g. @{ 'or' = @(...)}),
                    // the inversion of the group operator is applied and the
                    // leaf operators are toggled by flipping isExcludeFilter.
                    if (string.Equals(key, "not", StringComparison.OrdinalIgnoreCase))
                    {
                        // Normalize nested elements
                        List<Hashtable> nested = new List<Hashtable>();

                        if (filterValue.Value is Hashtable singleHt)
                        {
                            // If the single hashtable is itself a wrapper for
                            // an explicit group (single key 'and'/'or'), treat
                            // that specially so we invert the group operator.
                            if (singleHt.Count == 1 && singleHt.Keys.Cast<object>().First() is string k &&
                                (string.Equals(k, "and", StringComparison.OrdinalIgnoreCase) || string.Equals(k, "or", StringComparison.OrdinalIgnoreCase)))
                            {
                                // Extract inner list and operator
                                var innerObj = singleHt[k];
                                if (innerObj is IEnumerable ie)
                                {
                                    foreach (object o in ie)
                                    {
                                            try
                                            {
                                                Hashtable nh = ToHashtable(o);
                                                nested.Add(nh);
                                            }
                                            catch (InvalidDataException e)
                                            {
                                                throw new InvalidDataException($"Grouped filter operator '{k}' must contain hashtables. {e.Message}");
                                            }
                                    }
                                }
                                else if (innerObj is Hashtable nh)
                                {
                                    nested.Add(nh);
                                }
                                else
                                {
                                    throw new InvalidDataException($"Grouped filter operator '{k}' must contain a hashtable or an array of hashtables.");
                                }

                                // The inner group operator is k. NOT(innerGroup)
                                // requires the effective operator to be the
                                // inverse of k (De Morgan). Compute that and
                                // process nested with inverted leaf semantics.
                                LogicalOperator innerOp = string.Equals(k, "and", StringComparison.OrdinalIgnoreCase) ? LogicalOperator.And : LogicalOperator.Or;
                                LogicalOperator effective = innerOp == LogicalOperator.And ? LogicalOperator.Or : LogicalOperator.And;
                                // Create grouping filter with effective operator
                                FilterExpression groupFilter = parentFilterExpression.AddFilter(effective);
                                ProcessHashFilterValues(groupFilter, nested.ToArray(), !isExcludeFilter);
                                continue;
                            }

                            // Otherwise break the hashtable into single-entry
                            // hashtables so NOT(a=1 AND b=2) -> NOT(a=1) OR NOT(b=2)
                            foreach (DictionaryEntry innerEntry in singleHt)
                            {
                                Hashtable single = new Hashtable();
                                single.Add(innerEntry.Key, innerEntry.Value);
                                nested.Add(single);
                            }
                        }
                        else if (filterValue.Value is IEnumerable enumerable)
                        {
                            foreach (object o in enumerable)
                            {
                                try
                                {
                                    Hashtable nh = ToHashtable(o);
                                    nested.Add(nh);
                                }
                                catch (InvalidDataException e)
                                {
                                    throw new InvalidDataException($"'not' operator must contain hashtables. {e.Message}");
                                }
                            }
                        }
                        else
                        {
                            throw new InvalidDataException($"'not' operator must contain a hashtable or an array of hashtables.");
                        }

                        // Default behaviour: array of hashtables is treated as
                        // an AND group that we are negating, so its negation
                        // should be combined with OR (De Morgan). Therefore we
                        // create an OR group and recurse with inverted leaf
                        // semantics.
                        FilterExpression notGroup = parentFilterExpression.AddFilter(LogicalOperator.Or);
                        ProcessHashFilterValues(notGroup, nested.ToArray(), !isExcludeFilter);
                        continue;
                    }

                    // Support exclusive-or grouping: 'xor'. Semantics: exactly
                    // one of the nested hashtables is true. This is expanded to
                    // an OR of terms where each term is (Ai AND NOT all others).
                    if (string.Equals(key, "xor", StringComparison.OrdinalIgnoreCase))
                    {
                        List<Hashtable> nested = new List<Hashtable>();
                        if (filterValue.Value is Hashtable singleHt)
                        {
                            nested.Add(singleHt);
                        }
                        else if (filterValue.Value is IEnumerable ie)
                        {
                            foreach (object o in ie)
                            {
                                try
                                {
                                    Hashtable nh = ToHashtable(o);
                                    nested.Add(nh);
                                }
                                catch (InvalidDataException e)
                                {
                                    throw new InvalidDataException($"Grouped XOR operator must contain hashtables. {e.Message}");
                                }
                            }
                        }
                        else
                        {
                            throw new InvalidDataException($"Grouped XOR operator must contain a hashtable or an array of hashtables.");
                        }

                        int n = nested.Count;
                        if (n > MaxXorItems)
                        {
                            throw new InvalidDataException($"The 'xor' group contains {n} items which would trigger exponential expansion ({Math.Pow(2, n):N0} combinations) for exclusion semantics. To avoid excessive computation, the maximum allowed items in an 'xor' group is {MaxXorItems}. Consider using a smaller group, FetchXML, SQL, or other logic.");
                        }
                        if (n == 0)
                        {
                            continue;
                        }

                        // If this is an include filter, expand XOR to OR of (Ai AND NOT others)
                        if (!isExcludeFilter)
                        {
                            FilterExpression xorOuter = parentFilterExpression.AddFilter(LogicalOperator.Or);
                            for (int i = 0; i < n; i++)
                            {
                                // Term: Ai AND NOT(Aj for j != i)
                                FilterExpression term = xorOuter.AddFilter(LogicalOperator.And);
                                ProcessHashFilterValues(term, new[] { nested[i] }, isExcludeFilter);
                                for (int j = 0; j < n; j++)
                                {
                                    if (j == i) continue;
                                    Hashtable notHt = new Hashtable();
                                    notHt.Add("not", nested[j]);
                                    ProcessHashFilterValues(term, new[] { notHt }, isExcludeFilter);
                                }
                            }
                        }
                        else
                        {
                            // For exclude filters the semantics are: exclude rows
                            // matching XOR(nested). To include the complement we
                            // must add NOT XOR(nested) to the query which is the
                            // union of two cases: zero true OR two-or-more true.
                            // We generate combinations for the two-or-more case.
                            List<Hashtable> complements = new List<Hashtable>();

                            // Zero true: all NOT Aj
                            Hashtable zeroHt = new Hashtable();
                            List<Hashtable> zeroList = new List<Hashtable>();
                            for (int j = 0; j < n; j++)
                            {
                                zeroList.Add(new Hashtable() { { "not", nested[j] } });
                            }
                            zeroHt.Add("and", zeroList.ToArray());
                            complements.Add(zeroHt);

                            // Two-or-more true: all combinations of size >= 2
                            // This is exponential in n, but n is expected to be small
                            // for typical usage.
                            for (int k = 2; k <= n; k++)
                            {
                                foreach (var combo in GetCombinations(Enumerable.Range(0, n).ToArray(), k))
                                {
                                    Hashtable comboHt = new Hashtable();
                                    List<Hashtable> comboList = new List<Hashtable>();
                                    for (int idx = 0; idx < n; idx++)
                                    {
                                        if (combo.Contains(idx))
                                        {
                                            comboList.Add(nested[idx]);
                                        }
                                        else
                                        {
                                            comboList.Add(new Hashtable() { { "not", nested[idx] } });
                                        }
                                    }
                                    comboHt.Add("and", comboList.ToArray());
                                    complements.Add(comboHt);
                                }
                            }

                            // Add complements as ORed terms; process them as normal
                            // hashtables (we already constructed the required
                            // NOTs explicitly) so pass isExcludeFilter=false when
                            // recursing.
                            FilterExpression xorCompOuter = parentFilterExpression.AddFilter(LogicalOperator.Or);
                            ProcessHashFilterValues(xorCompOuter, complements.ToArray(), false);
                        }

                        continue;
                    }

                    // If we get here the key should represent a field (optionally
                    // with ':Operator' suffix) whose value is a literal/array/$null
                    // or a nested hashtable in the { value=...; operator='...' }
                    // form. This mirrors the previous behaviour but now supports
                    // the presence of grouped subfilters alongside field
                    // conditions.
                    ConditionOperator op = filterValue.Value == null ? ConditionOperator.Null : ConditionOperator.Equal;
                    object value = filterValue.Value;

                    string[] keyBits = ((string)filterValue.Key).Split(':');
                    string fieldName = keyBits[0];
                    if (keyBits.Length == 2)
                    {
                        try
                        {
                            op = (ConditionOperator)Enum.Parse(typeof(ConditionOperator), keyBits[1]);
                        }
                        catch (ArgumentException e)
                        {
                            throw new InvalidDataException($"The key '{filterValue.Key}' is invalid. {e.Message}. Valid operators are {string.Join(", ", Enum.GetNames(typeof(ConditionOperator)))}");

                        }
                    }
                    else if (keyBits.Length > 2)
                    {
                        throw new InvalidDataException($"The key '{filterValue.Key}' is invalid. Valid formats are 'fieldname' or 'fieldname:operator'");
                    }
                    else if (filterValue.Value is Hashtable ht)
                    {
                        if (!ht.ContainsKey("operator"))
                        {
                            throw new InvalidDataException($"The operator for key '{filterValue.Key}' is missing. When using a hashtable value the key 'operator' must be specified. e.g. @{filterValue.Key}=@{{value=25; operator='GreaterThan'}}");
                        }

                        var opObj = ht["operator"];
                        try
                        {
                            op = (ConditionOperator)Enum.Parse(typeof(ConditionOperator), opObj?.ToString() ?? "");
                        }
                        catch (ArgumentException e)
                        {
                            throw new InvalidDataException($"The operator for key '{filterValue.Key}' is invalid. {e.Message}. Valid operators are {string.Join(", ", Enum.GetNames(typeof(ConditionOperator)))}");
                        }

                        if (!OperatorIsValueLess(op))
                        {
                            if (!ht.ContainsKey("value"))
                            {
                                throw new InvalidDataException($"The value for key '{filterValue.Key}' is invalid. When using a hashtable value the key 'value' must be specified. e.g. @{filterValue.Key}=@{{value=25; operator='GreaterThan'}}");
                            }

                            value = ht["value"];
                        }

                    }

                    if (isExcludeFilter)
                    {
                        op = InvertOperator(op);
                    }

                    // Support qualified field names in the form 'entity.attribute'.
                    // When present pass the entity name into AddCondition so the
                    // underlying SDK can resolve the attribute appropriately
                    // for linked/aliased entities.
                    string entityPrefix = null;
                    string attributeName = fieldName;
                    if (fieldName.Contains('.'))
                    {
                        string[] parts = fieldName.Split('.');
                        if (parts.Length == 2)
                        {
                            entityPrefix = parts[0];
                            attributeName = parts[1];
                        }
                    }

                    if (OperatorIsValueLess(op))
                    {
                        if (!string.IsNullOrEmpty(entityPrefix))
                        {
                            parentFilterExpression.AddCondition(entityPrefix, attributeName, op);
                        }
                        else
                        {
                            parentFilterExpression.AddCondition(attributeName, op);
                        }
                    }
                    else if (value is Array array)
                    {
                        if (!string.IsNullOrEmpty(entityPrefix))
                        {
                            parentFilterExpression.AddCondition(entityPrefix, attributeName, op, (object[])array);
                        }
                        else
                        {
                            parentFilterExpression.AddCondition(attributeName, op, (object[])array);
                        }
                    }
                    else
                    {
                        if (!string.IsNullOrEmpty(entityPrefix))
                        {
                            parentFilterExpression.AddCondition(entityPrefix, attributeName, op, value);
                        }
                        else
                        {
                            parentFilterExpression.AddCondition(attributeName, op, value);
                        }
                    }
                }
            }
        }

        private static ConditionOperator InvertOperator(ConditionOperator op)
        {
            switch (op)
            {
                case ConditionOperator.Equal:
                    return ConditionOperator.NotEqual;
                case ConditionOperator.NotEqual:
                    return ConditionOperator.Equal;
                case ConditionOperator.GreaterThan:
                    return ConditionOperator.LessEqual;
                case ConditionOperator.GreaterEqual:
                    return ConditionOperator.LessThan;
                case ConditionOperator.LessThan:
                    return ConditionOperator.GreaterEqual;
                case ConditionOperator.LessEqual:
                    return ConditionOperator.GreaterThan;
                case ConditionOperator.In:
                    return ConditionOperator.NotIn;
                case ConditionOperator.NotIn:
                    return ConditionOperator.In;
                case ConditionOperator.Like:
                    return ConditionOperator.NotLike;
                case ConditionOperator.NotLike:
                    return ConditionOperator.Like;
                case ConditionOperator.BeginsWith:
                    return ConditionOperator.DoesNotBeginWith;
                case ConditionOperator.DoesNotBeginWith:
                    return ConditionOperator.BeginsWith;
                case ConditionOperator.EndsWith:
                    return ConditionOperator.DoesNotEndWith;
                case ConditionOperator.DoesNotEndWith:
                    return ConditionOperator.EndsWith;
                case ConditionOperator.Null:
                    return ConditionOperator.NotNull;
                case ConditionOperator.NotNull:
                    return ConditionOperator.Null;
                default:
                    throw new InvalidDataException($"The operator '{op}' cannot be inverted. Only Equal, NotEqual, GreaterThan, GreaterEqual, LessThan, LessEqual, In, NotIn, Like, NotLike, BeginsWith, DoesNotBeginWith, EndsWith, DoesNotEndWith, Null and NotNull can be inverted.");
            }
        }

        private static bool OperatorIsValueLess(ConditionOperator op)
        {
            return new[] { ConditionOperator.Null, ConditionOperator.NotNull, ConditionOperator.EqualUserLanguage, ConditionOperator.EqualUserOrUserHierarchy, ConditionOperator.EqualUserOrUserHierarchyAndTeams, ConditionOperator.EqualUserOrUserTeams, ConditionOperator.EqualUserTeams, ConditionOperator.EqualRoleBusinessId }.Contains(op) ||
                        (op.ToString().StartsWith("Next") && !op.ToString().StartsWith("NextX")) ||
                        (op.ToString().StartsWith("Last") && !op.ToString().StartsWith("LastX")) ||
                        op.ToString().StartsWith("This");
        }

        private static IEnumerable<int[]> GetCombinations(int[] items, int k)
        {
            // Simple recursive combinations generator
            if (k == 0)
            {
                yield return new int[0];
                yield break;
            }

            if (items.Length == k)
            {
                yield return items;
                yield break;
            }

            for (int i = 0; i <= items.Length - k; i++)
            {
                int head = items[i];
                int[] tail = items.Skip(i + 1).ToArray();
                foreach (var comb in GetCombinations(tail, k - 1))
                {
                    int[] result = new int[comb.Length + 1];
                    result[0] = head;
                    Array.Copy(comb, 0, result, 1, comb.Length);
                    yield return result;
                }
            }
        }

        public static Hashtable ToHashtable(object o)
        {
            if (o == null)
            {
                throw new InvalidDataException("Item is null and cannot be treated as a hashtable.");
            }

            if (o is Hashtable ht)
            {
                return ht;
            }

            if (o is PSObject pso)
            {
                return ToHashtable(pso.BaseObject);
            }

            if (o is IDictionary dict)
            {
                Hashtable h = new Hashtable();
                foreach (DictionaryEntry de in dict)
                {
                    h.Add(de.Key, de.Value);
                }
                return h;
            }

            throw new InvalidDataException($"Item of type {o.GetType().FullName} cannot be converted to a hashtable.");
        }

        public static PSSerializableHashtable ConvertFilterExpressionToHashtables(FilterExpression filter, int? queryType)
        {
            if (filter == null || (filter.Conditions.Count == 0 && filter.Filters.Count == 0))
            {
                return null;
            }

            var ht = new PSSerializableHashtable();
            string op = filter.FilterOperator == LogicalOperator.Or ? "or" : "and";


            List<Hashtable> list = new List<Hashtable>();

            if (filter.Conditions.Any())
            {
                // Add conditions
                var columnsHt = new PSSerializableHashtable();
                foreach (var condition in filter.Conditions)
                {
                    var condHt = new PSSerializableHashtable();
                    condHt["operator"] = condition.Operator.ToString();
                    if (condition.Values.Count == 1)
                    {
                        condHt["value"] = condition.Values[0];
                    }
                    else if (condition.Values.Count > 1)
                    {
                        condHt["value"] = condition.Values.ToArray();
                    }
                    else
                    {
                        condHt["value"] = null;
                    }

                    columnsHt[condition.AttributeName] = condHt;
                }
                list.Add(columnsHt);
            }

            // Add nested filters
            foreach (var nestedFilter in filter.Filters)
            {
                list.Add(ConvertFilterExpressionToHashtables(nestedFilter, queryType));
            }

            ht[op] = list.ToArray();            

            return ht;
        }
    }
}
