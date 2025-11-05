using Microsoft.Crm.Sdk;
using Microsoft.Crm.Sdk.Messages;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Metadata;
using Microsoft.Xrm.Sdk.Query;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Management.Automation;
using System.Xml.Linq;
using Rnwood.Dataverse.Data.PowerShell.Model;

namespace Rnwood.Dataverse.Data.PowerShell.Commands
{
    /// <summary>
    /// Retrieves view information (savedquery or userquery) from a Dataverse environment.
    /// </summary>
    [Cmdlet(VerbsCommon.Get, "DataverseView")]
    [OutputType(typeof(PSObject))]
    public class GetDataverseViewCmdlet : OrganizationServiceCmdlet
    {
        /// <summary>
        /// Gets or sets the ID of the view to retrieve.
        /// </summary>
        [Parameter(Position = 0, ValueFromPipelineByPropertyName = true, HelpMessage = "The ID of the view to retrieve.")]
        public Guid Id { get; set; }

        /// <summary>
        /// Gets or sets the name of the view to retrieve.
        /// </summary>
        [Parameter(HelpMessage = "The name of the view to retrieve. Supports wildcards.")]
        [SupportsWildcards]
        public string Name { get; set; }

        /// <summary>
        /// Gets or sets the logical name of the table (entity) to retrieve views for.
        /// </summary>
        [Parameter(HelpMessage = "Logical name of the table to retrieve views for.")]
        [ArgumentCompleter(typeof(TableNameArgumentCompleter))]
        [Alias("EntityName")]
        public string TableName { get; set; }

        /// <summary>
        /// Gets or sets whether to retrieve system views (savedquery) or personal views (userquery). Default is both.
        /// </summary>
        [Parameter(ValueFromPipelineByPropertyName = true, HelpMessage = "Retrieve views of the specified type")]
        [ValidateSet("System", "Personal")]
        public string ViewType { get; set; }

        /// <summary>
        /// Gets or sets the view type to filter by.
        /// </summary>
        [Parameter(HelpMessage = "View type to filter by")]
        public QueryType? QueryType { get; set; }

        /// <summary>
        /// Gets or sets whether to return raw values instead of display values.
        /// </summary>
        [Parameter(HelpMessage = "Return raw values instead of display values")]
        public SwitchParameter Raw { get; set; }

        private DataverseEntityConverter entityConverter;
        private EntityMetadataFactory entityMetadataFactory;

        /// <summary>
        /// Processes the cmdlet request.
        /// </summary>
        protected override void ProcessRecord()
        {
            base.ProcessRecord();

            entityMetadataFactory = new EntityMetadataFactory(Connection);
            entityConverter = new DataverseEntityConverter(Connection, entityMetadataFactory);

            // Determine which view types to query
            bool querySystemViews = string.IsNullOrEmpty(ViewType) || ViewType == "System";
            bool queryPersonalViews = string.IsNullOrEmpty(ViewType) || ViewType == "Personal";

            if (querySystemViews)
            {
                WriteVerbose("Querying system views (savedquery)...");
                QueryViews("savedquery");
            }

            if (queryPersonalViews)
            {
                WriteVerbose("Querying personal views (userquery)...");
                QueryViews("userquery");
            }
        }

        private void QueryViews(string entityName)
        {
            // Build query
            var query = new QueryExpression(entityName)
            {
                ColumnSet = new ColumnSet(true) // Get all columns
            };

            // Add filters
            if (Id != Guid.Empty)
            {
                query.Criteria.AddCondition(entityName + "id", ConditionOperator.Equal, Id);
                WriteVerbose($"Filtering by ID: {Id}");
            }

            if (!string.IsNullOrEmpty(Name))
            {
                // Check if wildcards are present
                if (WildcardPattern.ContainsWildcardCharacters(Name))
                {
                    // Convert PowerShell wildcards to SQL LIKE pattern
                    string likePattern = Name.Replace("*", "%").Replace("?", "_");
                    query.Criteria.AddCondition("name", ConditionOperator.Like, likePattern);
                    WriteVerbose($"Filtering by name pattern: {Name} (LIKE: {likePattern})");
                }
                else
                {
                    query.Criteria.AddCondition("name", ConditionOperator.Equal, Name);
                    WriteVerbose($"Filtering by name: {Name}");
                }
            }

            if (!string.IsNullOrEmpty(TableName))
            {
                query.Criteria.AddCondition("returnedtypecode", ConditionOperator.Equal, TableName);
                WriteVerbose($"Filtering by table name: {TableName}");
            }

            if (QueryType.HasValue)
            {
                query.Criteria.AddCondition("querytype", ConditionOperator.Equal, (int)QueryType.Value);
                WriteVerbose($"Filtering by query type: {QueryType.Value}");
            }

            // Execute query with paging
            WriteVerbose($"Executing query for {entityName} views");
            var views = QueryHelpers.ExecuteQueryWithPaging(query, Connection, WriteVerbose);

            WriteVerbose($"Found views in {entityName}");

            // Convert to PSObjects and output with streaming
            foreach (var view in views)
            {
                PSObject psObject;

                if (Raw.IsPresent)
                {
                    // Return raw values
                    psObject = entityConverter.ConvertToPSObject(view, new ColumnSet(true), _ => ValueType.Raw);
                }
                else
                {
                    // Create PSObject with only properties that match Set-DataverseView parameters plus basic info
                    psObject = new PSObject();

                    // Add normalized Id property for easier pipeline usage
                    string idAttributeName = entityName == "savedquery" ? "savedqueryid" : "userqueryid";
                    if (view.Attributes.TryGetValue(idAttributeName, out var idValue))
                    {
                        psObject.Properties.Add(new PSNoteProperty("Id", idValue));
                    }

                    psObject.Properties.Add(new PSNoteProperty("ViewType", entityName == "savedquery" ? "System" : "Personal"));

                    // Add properties that correspond to Set-DataverseView parameters
                    if (view.Attributes.TryGetValue("name", out var nameValue))
                    {
                        psObject.Properties.Add(new PSNoteProperty("Name", nameValue));
                    }
                    if (view.Attributes.TryGetValue("returnedtypecode", out var returnedTypeCodeValue))
                    {
                        psObject.Properties.Add(new PSNoteProperty("TableName", returnedTypeCodeValue));
                    }
                    if (view.Attributes.TryGetValue("description", out var descriptionValue))
                    {
                        psObject.Properties.Add(new PSNoteProperty("Description", descriptionValue));
                    }
                    if (view.Attributes.TryGetValue("querytype", out var queryTypeValue))
                    {
                        psObject.Properties.Add(new PSNoteProperty("QueryType", (QueryType)queryTypeValue));
                    }
                    if (view.Attributes.TryGetValue("isdefault", out var isDefaultValue))
                    {
                        psObject.Properties.Add(new PSNoteProperty("IsDefault", isDefaultValue));
                    }

                    // Parse FetchXML to extract Columns, Filters, and Links
                    var tableName = view.GetAttributeValue<string>("returnedtypecode");
                    var parsedProperties = ParseFetchXmlForQueryComponents(view.GetAttributeValue<string>("fetchxml"), view.GetAttributeValue<int?>("querytype"), tableName);
                    if (parsedProperties.ContainsKey("Columns"))
                    {
                        // Combine column names from FetchXML with width info from layoutxml
                        var columnsWithWidths = CombineColumnsWithLayoutInfo(
                            (string[])parsedProperties["Columns"],
                            view.GetAttributeValue<string>("layoutxml"));
                        psObject.Properties.Add(new PSNoteProperty("Columns", columnsWithWidths));
                    }
                    if (parsedProperties.ContainsKey("Filters"))
                    {
                        psObject.Properties.Add(new PSNoteProperty("Filters", parsedProperties["Filters"]));
                    }
                    if (parsedProperties.ContainsKey("Links"))
                    {
                        psObject.Properties.Add(new PSNoteProperty("Links", parsedProperties["Links"]));
                    }
                    if (parsedProperties.ContainsKey("OrderBy"))
                    {
                        psObject.Properties.Add(new PSNoteProperty("OrderBy", parsedProperties["OrderBy"]));
                    }
                }

                WriteObject(psObject);
            }
        }

        private object[] CombineColumnsWithLayoutInfo(string[] columnNames, string layoutXml)
        {
            if (columnNames == null || columnNames.Length == 0)
            {
                return new object[0];
            }

            // Parse layoutxml to get width information
            var layoutWidths = new System.Collections.Generic.Dictionary<string, int>();
            if (!string.IsNullOrEmpty(layoutXml))
            {
                XDocument doc = XDocument.Parse(layoutXml);
                var cells = doc.Descendants("cell");

                foreach (var cell in cells)
                {
                    string columnName = cell.Attribute("name")?.Value;
                    string widthStr = cell.Attribute("width")?.Value;

                    if (!string.IsNullOrEmpty(columnName) && !string.IsNullOrEmpty(widthStr) && int.TryParse(widthStr, out int width))
                    {
                        layoutWidths[columnName] = width;
                    }
                }

            }

            // Combine column names with width info - always return hashtables
            var columns = new System.Collections.Generic.List<object>();
            foreach (string columnName in columnNames)
            {
                var columnConfig = new PSSerializableHashtable();
                columnConfig["name"] = columnName;

                // Use width from layoutxml if available, otherwise default to 100
                if (layoutWidths.TryGetValue(columnName, out int width))
                {
                    columnConfig["width"] = width;
                }
                else
                {
                    columnConfig["width"] = 100; // Default width
                }

                columns.Add(columnConfig);
            }

            return columns.ToArray();
        }

        private System.Collections.Generic.Dictionary<string, object> ParseFetchXmlForQueryComponents(string fetchXml, int? queryType, string tableName)
        {
            var result = new System.Collections.Generic.Dictionary<string, object>();

            if (string.IsNullOrEmpty(fetchXml))
            {
                return result;
            }

            string modifiedFetchXml = fetchXml;
            Dictionary<string, string> placeholderMap = null;

            if (queryType == 4) // QuickFindSearch
            {
                modifiedFetchXml = QuickFindHelper.PreprocessFetchXmlForQuickFind(fetchXml, tableName, entityMetadataFactory, out placeholderMap);
            }

            // Use SDK to convert FetchXML to QueryExpression
            var translateRequest = new FetchXmlToQueryExpressionRequest
            {
                FetchXml = modifiedFetchXml
            };
            var translateResponse = (FetchXmlToQueryExpressionResponse)Connection.Execute(translateRequest);
            var query = translateResponse.Query;

            if (queryType == 4 && placeholderMap != null)
            {
                // Replace back the unique keys with placeholders in the filter conditions
                QuickFindHelper.ReplacePlaceholdersInFilter(query.Criteria, placeholderMap);
            }

            result["Columns"] = query.ColumnSet?.Columns.ToArray() ?? new string[0];


            // Extract filters
            if (query.Criteria != null && (query.Criteria.Conditions.Count > 0 || query.Criteria.Filters.Count > 0))
            {
                var filterHashtables = FilterHelpers.ConvertFilterExpressionToHashtables(query.Criteria, queryType);
                if (filterHashtables != null)
                {
                    result["Filters"] = new Hashtable[] { filterHashtables };
                }
                else
                {
                    result["Filters"] = new Hashtable[0];
                }
            }
            else
            {
                result["Filters"] = new Hashtable[0];
            }

            // Extract links
            if (query.LinkEntities != null && query.LinkEntities.Count > 0)
            {
                var hashtableLinks = query.LinkEntities.Select(le => new DataverseLinkEntity(le).ToHashtable()).ToArray();
                result["Links"] = hashtableLinks;
            }
            else
            {
                result["Links"] = new PSSerializableHashtable[0];
            }

            // Extract order by
            if (query.Orders != null && query.Orders.Count > 0)
            {
                var orderByStrings = query.Orders.Select(o => o.AttributeName + (o.OrderType == OrderType.Descending ? "-" : "")).ToArray();
                result["OrderBy"] = orderByStrings;
            }
            else
            {
                result["OrderBy"] = new string[0];
            }

            return result;
        }
    }
}
