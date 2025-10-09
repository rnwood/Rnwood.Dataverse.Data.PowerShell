

using Microsoft.Crm.Sdk.Messages;
using Microsoft.PowerPlatform.Dataverse.Client;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Messages;
using Microsoft.Xrm.Sdk.Metadata;
using Microsoft.Xrm.Sdk.Query;

using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Management.Automation;
using System.Runtime.Serialization;
using System.Text;
using System.Xml.Linq;

namespace Rnwood.Dataverse.Data.PowerShell.Commands
{
    /// <summary>
    /// Retrieves records from a Dataverse table using various query methods.
    /// </summary>
    [Cmdlet(VerbsCommon.Get, "DataverseRecord")]
    [OutputType(typeof(IEnumerable<PSObject>))]
    public class GetDataverseRecordCmdlet : OrganizationServiceCmdlet
    {
        private const int MaxXorItems = 8;

        private const string PARAMSET_FETCHXML = "FetchXml";

        private const string PARAMSET_SIMPLE = "Simple";

        /// <summary>
        /// Gets or sets the logical name of the table to query.
        /// </summary>
        [ArgumentCompleter(typeof(TableNameArgumentCompleter))]
        [Parameter(ParameterSetName = PARAMSET_SIMPLE, Mandatory = true, Position = 0, HelpMessage = "Logical name of table for which to retrieve records")]
        [Alias("EntityName")]
        public string TableName { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether to write the total record count to verbose output.
        /// </summary>
        [Parameter(HelpMessage = "If set writes total record count matching query to verbose output")]
        public SwitchParameter VerboseRecordCount { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether to return only the count of records instead of the records themselves.
        /// </summary>
        [Parameter(HelpMessage = "If set, writes total record count matching query to output instead of results")]
        public SwitchParameter RecordCount { get; set; }
        /// <summary>
        /// FetchXml to use
        /// </summary>
        [Parameter(ParameterSetName = PARAMSET_FETCHXML, HelpMessage = "FetchXml to use")]
        public string FetchXml
        {
            get;
            set;
        }
        /// <summary>
        /// One or more hashtables that define filters to apply to the query.
        /// Each hashtable's entries are combined with AND; multiple hashtables are combined with OR.
        /// Filter keys may be "column" or "column:Operator" where Operator is a
        /// name from the ConditionOperator enum (e.g. GreaterThan, NotEqual).
        /// Values may be a literal, an array (treated as IN), $null (treated as ISNULL),
        /// or a nested hashtable with keys "value" and "operator" to specify operator and value
        /// (e.g. @{age=@{value=25; operator='GreaterThan'}}).
        /// </summary>
        [ArgumentCompleter(typeof(FilterValuesArgumentCompleter))]
        [Parameter(ParameterSetName = PARAMSET_SIMPLE, Mandatory = false, HelpMessage = "One or more hashtables to filter records. Each hashtable's entries are combined with AND; multiple hashtables are combined with OR. Keys may be 'column' or 'column:Operator' (Operator is a ConditionOperator name). Values may be a literal, an array (treated as IN), $null (treated as ISNULL), or a nested hashtable with keys 'value' and 'operator' (e.g. @{age=@{value=25; operator='GreaterThan'}}). Examples: @{firstname='bob'; age=25}, @{firstname='sue'} => (firstname=bob AND age=25) OR (firstname=sue).")]
        [Alias("IncludeFilter")]
        public Hashtable[] FilterValues
        {
            get;
            set;
        }
        /// <summary>
        /// Extra criteria to apply to query
        /// </summary>
        [Parameter(ParameterSetName = PARAMSET_SIMPLE, HelpMessage = "Extra criteria to apply to query")]
        public FilterExpression Criteria
        {
            get;
            set;
        }
        /// <summary>
        /// Link entities to apply to query
        /// </summary>
        [ArgumentCompleter(typeof(LinksArgumentCompleter))]
        [Parameter(ParameterSetName = PARAMSET_SIMPLE, HelpMessage = "Link entities to apply to query")]
        public DataverseLinkEntity[] Links
        {
            get;
            set;
        }
        /// <summary>
    /// List of hashtables of field names/values to exclude. Defaults to a NOT EQUAL condition for values (or IS NOT NULL when $null is supplied).
    /// Multiple hashtables are combined using AND by default; use -ExcludeFilterOr to combine them using OR instead.
    /// For example: @{firstname="bob", age=25}, @{lastname="smith"}:
    /// - Default (AND): excludes records matching both hashtables (firstname='bob' AND age=25) AND (lastname='smith').
    /// - With -ExcludeFilterOr (OR): excludes records matching either hashtable (firstname='bob' AND age=25) OR (lastname='smith').
        /// </summary>
    [Parameter(ParameterSetName = PARAMSET_SIMPLE, Mandatory = false, HelpMessage = "List of hashtables of field names/values to exclude. Defaults to a NOT EQUAL condition for values (or IS NOT NULL when $null is supplied). Multiple hashtables are combined using AND by default; use -ExcludeFilterOr to combine them using OR instead. e.g. @{firstname=\"bob\", age=25}, @{lastname=\"smith\"}")]
        [Alias("ExcludeFilter")]
        [ArgumentCompleter(typeof(FilterValuesArgumentCompleter))]
        public Hashtable[] ExcludeFilterValues
        {
            get;
            set;
        }

        /// <summary>
        /// When specified, multiple exclude hashtables are combined using OR instead of the default AND.
        /// </summary>
        [Parameter(ParameterSetName = PARAMSET_SIMPLE, Mandatory = false, HelpMessage = "If specified multiple hashtables exclude filters will be logically combined using OR instead of the default of AND")]
        public SwitchParameter ExcludeFilterOr
        {
            get;
            set;
        }
        /// <summary>
        /// If specified only active records (statecode=0 or isactive=true) will be output
        /// </summary>
        [Parameter(ParameterSetName = PARAMSET_SIMPLE, Mandatory = false, HelpMessage = "If specified only active records (statecode=0 or isactive=true) will be output")]
        public SwitchParameter ActiveOnly
        {
            get;
            set;
        }
        /// <summary>
        /// List of primary keys (IDs) of records to retrieve.
        /// </summary>
        [Parameter(ParameterSetName = PARAMSET_SIMPLE, HelpMessage = "List of primary keys (IDs) of records to retrieve.")]
        public Guid[] Id
        {
            get;
            set;
        }
        /// <summary>
        /// List of names (primary attribute value) of records to retrieve.
        /// </summary>
        [Parameter(ParameterSetName = PARAMSET_SIMPLE, HelpMessage = "List of names (primary attribute value) of records to retrieve.")]
        public string[] Name
        {
            get;
            set;
        }
        /// <summary>
        /// List of record ids to exclude
        /// </summary>
        [Parameter(ParameterSetName = PARAMSET_SIMPLE, HelpMessage = "List of record ids to exclude")]
        public Guid[] ExcludeId
        {
            get;
            set;
        }
        /// <summary>
        /// List of columns to return in records (default is all). Each column name may be suffixed with :Raw or :Display to override the value type which will be output from the default
        /// </summary>
        [ArgumentCompleter(typeof(ColumnNamesArgumentCompleter))]
        [Parameter(ParameterSetName = PARAMSET_SIMPLE, Mandatory = false, HelpMessage = "List of columns to return in records (default is all). Each column name may be suffixed with :Raw or :Display to override the value type which will be output from the default")]
        public string[] Columns
        {
            get;
            set;
        }
        /// <summary>
        /// List of columns to exclude from records (default is none). Ignored if Columns parameter is used.s
        /// </summary>
        [ArgumentCompleter(typeof(ColumnNamesArgumentCompleter))]
        [Parameter(ParameterSetName = PARAMSET_SIMPLE, Mandatory = false, HelpMessage = "List of columns to exclude from records (default is none). Ignored if Columns parameter is used.s")]
        public string[] ExcludeColumns
        {
            get;
            set;
        }
        /// <summary>
        /// List of columns to order records by. Suffix column name with - to sort descending. e.g \
        /// </summary>
        [Parameter(ParameterSetName = PARAMSET_SIMPLE, Mandatory = false, HelpMessage = "List of columns to order records by. Suffix column name with - to sort descending. e.g \"age-\", \"lastname\" will sort by age descending then lastname ascending")]
        public string[] OrderBy
        {
            get;
            set;
        }
        /// <summary>
        /// Number of records to limit result to. Default is all results.
        /// </summary>
        [Parameter(Mandatory = false, HelpMessage = "Number of records to limit result to. Default is all results.")]
        public int? Top
        {
            get;
            set;
        }
        /// <summary>
        /// Number of records to request per page. Default is 1000.
        /// </summary>
        [Parameter(Mandatory = false, HelpMessage = "Number of records to request per page. Default is 1000.")]
        public int? PageSize
        {
            get;
            set;
        }
        /// <summary>
        /// Outputs Names for lookup values. The default behaviour is to output the ID.
        /// </summary>
        [Parameter(HelpMessage = "Outputs Names for lookup values. The default behaviour is to output the ID.")]
        public SwitchParameter LookupValuesReturnName { get; set; }
        /// <summary>
        /// Excludes system columns from output. Default is all columns except system columns. Ignored if Columns parameter is used.
        /// </summary>
        [Parameter(ParameterSetName = PARAMSET_SIMPLE, HelpMessage = "Excludes system columns from output. Default is all columns except system columns. Ignored if Columns parameter is used.")]
        public SwitchParameter IncludeSystemColumns { get; set; }

        private EntityMetadataFactory entiyMetadataFactory;
        private DataverseEntityConverter entityConverter;
        private EntityMetadata entityMetadata;

        /// <summary>
        /// Initializes the cmdlet and sets up required helpers.
        /// </summary>
        protected override void BeginProcessing()
        {
            base.BeginProcessing();
        }

        /// <summary>
        /// Processes each record by executing the configured query and writing results to the pipeline.
        /// </summary>
        protected override void ProcessRecord()
        {
            base.ProcessRecord();

            ExecuteQuery();
        }

        private ValueType GetColumnValueType(AttributeMetadata attribute)
        {
            ValueType result;

            if (attribute.AttributeType.Value == AttributeTypeCode.Lookup || attribute.AttributeType.Value == AttributeTypeCode.Uniqueidentifier)
            {
                result = LookupValuesReturnName ? ValueType.Display : ValueType.Raw;
            }
            else
            {
                result = ValueType.Raw;
            }

            return result;
        }

        private void ExecuteQuery()
        {
            entiyMetadataFactory = new EntityMetadataFactory(Connection);
            entityConverter = new DataverseEntityConverter(Connection, entiyMetadataFactory);

            QueryExpression query;

            switch (ParameterSetName)
            {
                case PARAMSET_FETCHXML:
                    query = GetFetchXmlQuery();
                    Columns = query.ColumnSet.AllColumns ? null : query.ColumnSet.Columns.ToArray();   // Capture columns requested in FetchXML for use when converting to PSObject
                    entityMetadata = entiyMetadataFactory.GetMetadata(query.EntityName);
                    break;
                case PARAMSET_SIMPLE:
                    entityMetadata = entiyMetadataFactory.GetMetadata(TableName);
                    query = GetSimpleQuery();
                    break;
                default:
                    throw new NotImplementedException($"ParameterSetName not implemented: {ParameterSetName}");
            }

            if (RecordCount || VerboseRecordCount)
            {
                long recordCount = GetRecordCount(query);

                WriteObject(recordCount);
                WriteVerbose("Total records matching query: " + recordCount);
            }

            if (!RecordCount)
            {
                long recordCount = 0;
                foreach (Entity entity in GetRecords(query))
                {
                    PSObject output = entityConverter.ConvertToPSObject(entity, GetColumnSet(""), GetColumnValueType);

                    WriteObject(output);

                    recordCount++;

                    if (Top.HasValue && recordCount == Top.Value)
                    {
                        break;
                    }
                }
            }
        }

        private QueryExpression GetSimpleQuery()
        {
            QueryExpression query = new QueryExpression(TableName);
            query.ColumnSet = Columns != null && !Columns.Contains("calendarrules", StringComparer.OrdinalIgnoreCase) ? new ColumnSet(Columns) : new ColumnSet(!RecordCount);
            query.Criteria.FilterOperator = LogicalOperator.And;

            if (Id != null)
            {
                query.Criteria.AddCondition(entityMetadata.PrimaryIdAttribute, ConditionOperator.In, Id.Cast<object>().ToArray());
            }

            if (ActiveOnly.IsPresent)
            {
                AddActiveFilter(query);
            }

            if (ExcludeId != null)
            {
                query.Criteria.AddCondition(entityMetadata.PrimaryIdAttribute, ConditionOperator.NotIn, ExcludeId.Cast<object>().ToArray());
            }

            if (Name != null)
            {
                FilterExpression nameFilterExpression = query.Criteria.AddFilter(LogicalOperator.Or);

                foreach (string name in Name)
                {
                    nameFilterExpression.AddCondition(entityMetadata.PrimaryNameAttribute, ConditionOperator.Equal, name);
                }
            }

            if (FilterValues != null)
            {
                FilterExpression includesFilterExpression = query.Criteria.AddFilter(LogicalOperator.Or);
                ProcessHashFilterValues(includesFilterExpression, FilterValues, false);
            }

            if (ExcludeFilterValues != null)
            {
                    // Build the inverted exclude filters. Top-level combination across the exclude
                    // hashtables is AND by default (a record is excluded only if it matches all
                    // provided hashtables). When -ExcludeFilterOr is specified the top-level
                    // combination becomes OR (a record is excluded if it matches any provided hashtable).
                    FilterExpression excludesFilterExpression = query.Criteria.AddFilter(ExcludeFilterOr.IsPresent ? LogicalOperator.Or : LogicalOperator.And);
                    ProcessHashFilterValues(excludesFilterExpression, ExcludeFilterValues, true);
            }

            if (Criteria != null)
            {
                query.Criteria.AddFilter(Criteria);
            }

            if (Links != null)
            {
                query.LinkEntities.AddRange(Links.Select(l => l.LinkEntity));
            }

            if (OrderBy != null)
            {
                foreach (string orderByColumnName in OrderBy)
                {
                    query.AddOrder(orderByColumnName.TrimEnd('+', '-'), orderByColumnName.EndsWith("-") ? OrderType.Descending : OrderType.Ascending);
                }
            }

            return query;
        }
        private static void ProcessHashFilterValues(FilterExpression parentFilterExpression, Hashtable[] filterValuesArray, bool isExcludeFilter)
        {
            foreach (Hashtable filterValues in filterValuesArray)
            {
                // Each hashtable represents a single sub-filter; by default its
                // entries are combined with AND. We create a container filter for
                // the hashtable so it integrates correctly with the parent
                // expression which may combine multiple hashtables with OR/AND.
                FilterExpression containerFilter = parentFilterExpression.AddFilter(LogicalOperator.And);

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
                        FilterExpression groupFilter = containerFilter.AddFilter(effectiveGroupOperator);
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
                                FilterExpression groupFilter = containerFilter.AddFilter(effective);
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
                        FilterExpression notGroup = containerFilter.AddFilter(LogicalOperator.Or);
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
                            FilterExpression xorOuter = containerFilter.AddFilter(LogicalOperator.Or);
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
                            FilterExpression xorCompOuter = containerFilter.AddFilter(LogicalOperator.Or);
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

                    if (OperatorIsValueLess(op))
                    {
                        containerFilter.AddCondition(fieldName, op);
                    }
                    else if (value is Array array)
                    {
                        containerFilter.AddCondition(fieldName, op, (object[])array);
                    }
                    else
                    {
                        containerFilter.AddCondition(fieldName, op, value);
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

        private QueryExpression GetFetchXmlQuery()
        {
            QueryExpression query;
            FetchXmlToQueryExpressionRequest translateQueryRequest = new FetchXmlToQueryExpressionRequest { FetchXml = FetchXml };
            var translateQueryResponse = (FetchXmlToQueryExpressionResponse)Connection.Execute(translateQueryRequest);
            query = translateQueryResponse.Query;
            return query;
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

        private static Hashtable ToHashtable(object o)
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

        private long GetRecordCount(QueryExpression query)
        {
            ColumnSet oldColumnSet = query.ColumnSet;
            query.ColumnSet = new ColumnSet(false);

            long result = GetRecords(query).LongCount();

            query.ColumnSet = oldColumnSet;
            return result;
        }

        private IEnumerable<Entity> GetRecords(QueryExpression query)
        {

            PagingInfo pageInfo = new PagingInfo()
            {
                PageNumber = 1,
                Count = Top.GetValueOrDefault(PageSize.GetValueOrDefault(1000))
            };


            query.PageInfo = pageInfo;

            RetrieveMultipleRequest request = new RetrieveMultipleRequest()
            {
                Query = query
            };

            RetrieveMultipleResponse response;

            do
            {
                response = (RetrieveMultipleResponse)Connection.Execute(request);

                pageInfo.PageNumber++;
                pageInfo.PagingCookie = response.EntityCollection.PagingCookie;


                foreach (Entity entity in response.EntityCollection.Entities)
                {
                    yield return entity;
                }

            } while (response.EntityCollection.MoreRecords);
        }


        private void AddActiveFilter(QueryExpression query)
        {
            bool filterIncludesState = (FilterValues != null
                    && FilterValues.Any(fv =>
                        fv.Keys.Cast<string>().Any(k =>
                            k.Equals("statuscode", StringComparison.OrdinalIgnoreCase)
                            || k.Equals("statecode", StringComparison.OrdinalIgnoreCase)
                            || k.Equals("isdisabled", StringComparison.OrdinalIgnoreCase))
                        )
                    )
                    || (ExcludeFilterValues != null
                        && ExcludeFilterValues.Any(fv =>
                            fv.Keys.Cast<string>().Any(k =>
                                k.Equals("statuscode", StringComparison.OrdinalIgnoreCase)
                                || k.Equals("statecode", StringComparison.OrdinalIgnoreCase)
                                || k.Equals("isdisabled", StringComparison.OrdinalIgnoreCase))
                            )
                        );

            if (!filterIncludesState)
            {
                AddActiveFilter(query.Criteria, TableName);

                if (entityMetadata.IsIntersect.GetValueOrDefault())
                {
                    ManyToManyRelationshipMetadata manyToManyRelationshipMetadata = entityMetadata.ManyToManyRelationships[0];

                    LinkEntity link1 = query.AddLink(manyToManyRelationshipMetadata.Entity1LogicalName, manyToManyRelationshipMetadata.Entity1IntersectAttribute, manyToManyRelationshipMetadata.Entity1IntersectAttribute);
                    AddActiveFilter(link1.LinkCriteria, manyToManyRelationshipMetadata.Entity1LogicalName);

                    LinkEntity link2 = query.AddLink(manyToManyRelationshipMetadata.Entity2LogicalName, manyToManyRelationshipMetadata.Entity2IntersectAttribute, manyToManyRelationshipMetadata.Entity2IntersectAttribute);
                    AddActiveFilter(link2.LinkCriteria, manyToManyRelationshipMetadata.Entity2LogicalName);
                }
            }
        }

        private void AddActiveFilter(FilterExpression criteria, string entityName)
        {
            EntityMetadata entityMetadata = entiyMetadataFactory.GetMetadata(entityName);

            if (entityMetadata.Attributes.Any(
                    a => string.Equals(a.LogicalName, "statecode", StringComparison.OrdinalIgnoreCase)))
            {
                criteria.AddCondition("statecode", ConditionOperator.Equal, 0);
            }

            if (entityMetadata.Attributes.Any(
                    a => string.Equals(a.LogicalName, "isdisabled", StringComparison.OrdinalIgnoreCase)))
            {
                criteria.AddCondition("isdisabled", ConditionOperator.Equal, false);
            }
        }

        private ColumnSet GetColumnSet(string path)
        {
            Func<string, string> getPath = (c) =>
                {
                    string[] elements = c.Split(new[] { '.' });
                    return string.Join(".", elements.Take(elements.Count() - 1));
                };

            string[] columnNames = null;
            if (Columns != null)
            {
                columnNames = Columns.Select(f => f.Split(':')[0]).Except(ExcludeColumns ?? new string[0]).ToArray();
            }

            return new ColumnSet((columnNames ?? DataverseEntityConverter.GetAllColumnNames(entityMetadata, IncludeSystemColumns, ExcludeColumns)).Where(c => getPath(c) == path).ToArray());
        }
    }
}