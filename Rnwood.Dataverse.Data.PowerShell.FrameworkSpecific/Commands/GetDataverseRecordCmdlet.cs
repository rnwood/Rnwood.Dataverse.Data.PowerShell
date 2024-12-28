

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
	[Cmdlet(VerbsCommon.Get, "DataverseRecord")]
	[OutputType(typeof(IEnumerable<PSObject>))]
	public class GetDataverseRecordCmdlet : OrganizationServiceCmdlet
	{
		[Parameter(Mandatory = true, HelpMessage = "DataverseConnection instance obtained from Get-DataverseConnnection cmdlet, or string specifying Dataverse organization URL (e.g. http://server.com/MyOrg/)")]
		public override ServiceClient Connection { get; set; }

		private const string PARAMSET_FETCHXML = "FetchXml";

		private const string PARAMSET_SIMPLE = "Simple";

		[Parameter(ParameterSetName = PARAMSET_SIMPLE, Mandatory = true, Position = 0, HelpMessage = "Logical name of table for which to retrieve records")]
		[Alias("EntityName")]
		public string TableName { get; set; }

		[Parameter(HelpMessage = "If set writes total record count matching query to verbose output")]
		public SwitchParameter VerboseRecordCount { get; set; }

		[Parameter(HelpMessage = "If set, writes total record count matching query to output instead of results")]
		public SwitchParameter RecordCount { get; set; }

		[Parameter(ParameterSetName = PARAMSET_FETCHXML, HelpMessage = "FetchXml to use")]
		public string FetchXml
		{
			get;
			set;
		}

		[Parameter(ParameterSetName = PARAMSET_SIMPLE, Mandatory = false, HelpMessage = "List of hashsets of @{\"columnnames(:operator)\"=\"value\"} to filter records by. If operator is not specified, uses an EQUALS condition (or ISNULL if null value). If more than one hashset is provided then they are logically combined using an OR condition. e.g. @{firstname=\"bob\", age=25}, @{firstname=\"sue\"} will find records where (firstname=bob AND age=25) OR (firstname=sue)")]
		public Hashtable[] FilterValues
		{
			get;
			set;
		}

		[Parameter(ParameterSetName = PARAMSET_SIMPLE, HelpMessage = "Extra criteria to apply to query")]
		public FilterExpression Criteria
		{
			get;
			set;
		}

		[Parameter(ParameterSetName = PARAMSET_SIMPLE, HelpMessage = "Link entities to apply to query")]
		public DataverseLinkEntity[] Links
		{
			get;
			set;
		}

		[Parameter(ParameterSetName = PARAMSET_SIMPLE, Mandatory = false, HelpMessage = "List of hashsets of column names,values to filter records by using an NOTEQUALS condition (or ISNOTNULL if null value). If more than one hashset is provided then they are logically combined using an AND condition by default. e.g. @{firstname=\"bob\", age=25}, @{firstname=\"sue\"} will find records where (firstname<>bob AND age<>25) OR (firstname<>sue)")]
		public Hashtable[] ExcludeFilterValues
		{
			get;
			set;
		}

		[Parameter(ParameterSetName = PARAMSET_SIMPLE, Mandatory = false, HelpMessage = "If specified the exclude filters will be logically combined using OR instead of the default of AND")]
		public SwitchParameter ExcludeFilterOr
		{
			get;
			set;
		}

		[Parameter(ParameterSetName = PARAMSET_SIMPLE, Mandatory = false, HelpMessage = "If specified only active records (statecode=0 or isactive=true) will be output")]
		public SwitchParameter ActiveOnly
		{
			get;
			set;
		}

		[Parameter(ParameterSetName = PARAMSET_SIMPLE, HelpMessage = "List of primary keys (IDs) of records to retrieve.")]
		public Guid[] Id
		{
			get;
			set;
		}

		[Parameter(ParameterSetName = PARAMSET_SIMPLE, HelpMessage = "List of names (primary attribute value) of records to retrieve.")]
		public string[] Name
		{
			get;
			set;
		}

		[Parameter(ParameterSetName = PARAMSET_SIMPLE, HelpMessage = "List of record ids to exclude")]
		public Guid[] ExcludeId
		{
			get;
			set;
		}

		[Parameter(ParameterSetName = PARAMSET_SIMPLE, Mandatory = false, HelpMessage = "List of columns to return in records (default is all). Each column name may be suffixed with :Raw or :Display to override the value type which will be output from the default")]
		public string[] Columns
		{
			get;
			set;
		}

		[Parameter(ParameterSetName = PARAMSET_SIMPLE, Mandatory = false, HelpMessage = "List of columns to exclude from records (default is none). Ignored if Columns parameter is used.s")]
		public string[] ExcludeColumns
		{
			get;
			set;
		}

		[Parameter(ParameterSetName = PARAMSET_SIMPLE, Mandatory = false, HelpMessage = "List of columns to order records by. Suffix column name with - to sort descending. e.g \"age-\", \"lastname\" will sort by age descending then lastname ascending")]
		public string[] OrderBy
		{
			get;
			set;
		}

		[Parameter(Mandatory = false, HelpMessage = "Number of records to limit result to. Default is all results.")]
		public int? Top
		{
			get;
			set;
		}

		[Parameter(Mandatory = false, HelpMessage = "Number of records to request per page. Default is 1000.")]
		public int? PageSize
		{
			get;
			set;
		}

		[Parameter(HelpMessage = "Outputs Names for lookup values. The default behaviour is to output the ID.")]
		public SwitchParameter LookupValuesReturnName { get; set; }

		[Parameter(ParameterSetName = PARAMSET_SIMPLE, HelpMessage = "Excludes system columns from output. Default is all columns except system columns. Ignored if Columns parameter is used.")]
		public SwitchParameter IncludeSystemColumns { get; set; }

		private EntityMetadataFactory entiyMetadataFactory;
		private DataverseEntityConverter entityConverter;
		private EntityMetadata entityMetadata;

		protected override void BeginProcessing()
		{
			base.BeginProcessing();

			entiyMetadataFactory = new EntityMetadataFactory(Connection);
			entityConverter = new DataverseEntityConverter(Connection, entiyMetadataFactory);
			entityMetadata = entiyMetadataFactory.GetMetadata(TableName);
		}

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
			QueryExpression query;

			switch (ParameterSetName)
			{
				case PARAMSET_FETCHXML:
					query = GetFetchXmlQuery();
					break;
				case PARAMSET_SIMPLE:
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

				foreach (Hashtable filterValues in FilterValues)
				{
					FilterExpression includeFilterExpression = includesFilterExpression.AddFilter(LogicalOperator.And);
					foreach (DictionaryEntry filterValue in filterValues)
					{
						ConditionOperator op = filterValue.Value == null ? ConditionOperator.Null : ConditionOperator.Equal;

						string[] keyBits = ((string)filterValue.Key).Split(':');
						string fieldName = keyBits[0];
						if (keyBits.Length == 2)
						{
							try
							{
								op = (ConditionOperator)Enum.Parse(typeof(ConditionOperator), keyBits[1]);
							} catch (ArgumentException e)
							{
								throw new InvalidDataException($"The key '{filterValue.Key}' is invalid. {e.Message}. Valid operators are {string.Join(", ", Enum.GetNames(typeof(ConditionOperator)))}");

							}
						} else if (keyBits.Length > 2)
						{
							throw new InvalidDataException($"The key '{filterValue.Key}' is invalid. Valid formats are 'fieldname' or 'fieldname:operator'");
						}

						includeFilterExpression.AddCondition(fieldName, op,
															 filterValue.Value);
					}
				}
			}

			if (ExcludeFilterValues != null)
			{
				FilterExpression excludesFilterExpression = query.Criteria.AddFilter(ExcludeFilterOr.IsPresent ? LogicalOperator.Or : LogicalOperator.And);

				foreach (Hashtable excludeFilterValues in ExcludeFilterValues)
				{
					FilterExpression excludeFilterExpression = excludesFilterExpression.AddFilter(LogicalOperator.And);
					foreach (DictionaryEntry filterValue in excludeFilterValues)
					{
						if (filterValue.Value == null)
						{
							excludeFilterExpression.AddCondition((string)filterValue.Key, ConditionOperator.NotNull);
						}
						else
						{
							excludeFilterExpression.AddCondition((string)filterValue.Key, ConditionOperator.NotEqual,
																 filterValue.Value);
						}
					}
				}
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

		private QueryExpression GetFetchXmlQuery()
		{
			QueryExpression query;
			FetchXmlToQueryExpressionRequest translateQueryRequest = new FetchXmlToQueryExpressionRequest { FetchXml = FetchXml };
			var translateQueryResponse = (FetchXmlToQueryExpressionResponse)Connection.Execute(translateQueryRequest);
			query = translateQueryResponse.Query;
			return query;
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