

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
        /// Supports grouped filters using 'and', 'or', 'not', or 'xor' keys with nested hashtables for complex logical expressions.
        /// </summary>
        [ArgumentCompleter(typeof(FilterValuesArgumentCompleter))]
        [Parameter(ParameterSetName = PARAMSET_SIMPLE, Mandatory = false, HelpMessage = "One or more hashtables to filter records. Each hashtable's entries are combined with AND; multiple hashtables are combined with OR. Keys may be 'column' or 'column:Operator' (Operator is a ConditionOperator name). Values may be a literal, an array (treated as IN), $null (treated as ISNULL), or a nested hashtable with keys 'value' and 'operator' (e.g. @{age=@{value=25; operator='GreaterThan'}}). Supports grouped filters using 'and', 'or', 'not', or 'xor' keys with nested hashtables for complex logical expressions. Examples: @{firstname='bob'; age=25}, @{firstname='sue'} => (firstname=bob AND age=25) OR (firstname=sue).")]
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
        /// Link entities to apply to query. Accepts DataverseLinkEntity objects or simplified hashtable syntax.
        /// Hashtable format: @{ 'fromEntity.fromAttribute' = 'toEntity.toAttribute'; type = 'Inner'; alias = 'aliasName'; filter = @{...}; links = @(...) }.
        /// The 'filter' key uses the same format as FilterValues and supports 'and', 'or', 'not', 'xor' operators.
        /// The 'links' key allows nested child joins.
        /// </summary>
        [ArgumentCompleter(typeof(LinksArgumentCompleter))]
        [Parameter(ParameterSetName = PARAMSET_SIMPLE, HelpMessage = "Link entities to apply to query. Accepts DataverseLinkEntity objects or simplified hashtable syntax: @{ 'fromEntity.fromAttribute' = 'toEntity.toAttribute'; type = 'Inner'; alias = 'aliasName'; filter = @{...}; links = @(...) }. The 'filter' key uses the same format as FilterValues and supports 'and', 'or', 'not', 'xor' operators. The 'links' key allows nested child joins.")]
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
        /// Supports grouped filters using 'and', 'or', 'not', or 'xor' keys with nested hashtables for complex logical expressions.
        /// </summary>
        [Parameter(ParameterSetName = PARAMSET_SIMPLE, Mandatory = false, HelpMessage = "List of hashtables of field names/values to exclude. Defaults to a NOT EQUAL condition for values (or IS NOT NULL when $null is supplied). Multiple hashtables are combined using AND by default; use -ExcludeFilterOr to combine them using OR instead. Supports grouped filters using 'and', 'or', 'not', or 'xor' keys with nested hashtables for complex logical expressions. e.g. @{firstname=\"bob\", age=25}, @{lastname=\"smith\"}")]
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
                FilterHelpers.ProcessHashFilterValues(includesFilterExpression, FilterValues, false);
            }

            if (ExcludeFilterValues != null)
            {
                    // Build the inverted exclude filters. Top-level combination across the exclude
                    // hashtables is AND by default (a record is excluded only if it matches all
                    // provided hashtables). When -ExcludeFilterOr is specified the top-level
                    // combination becomes OR (a record is excluded if it matches any provided hashtable).
                    FilterExpression excludesFilterExpression = query.Criteria.AddFilter(ExcludeFilterOr.IsPresent ? LogicalOperator.Or : LogicalOperator.And);
                    FilterHelpers.ProcessHashFilterValues(excludesFilterExpression, ExcludeFilterValues, true);
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
        // Filter parsing and helper methods were moved to FilterHelpers to
        // avoid duplication between Get-DataverseRecord and DataverseLinkEntity.
        private QueryExpression GetFetchXmlQuery()
        {
            QueryExpression query;
            FetchXmlToQueryExpressionRequest translateQueryRequest = new FetchXmlToQueryExpressionRequest { FetchXml = FetchXml };
            var translateQueryResponse = (FetchXmlToQueryExpressionResponse)Connection.Execute(translateQueryRequest);
            query = translateQueryResponse.Query;
            return query;
        }

        // Combination and hashtable conversion helpers moved to FilterHelpers.

        // ToHashtable moved to FilterHelpers.

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