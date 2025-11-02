using Microsoft.Crm.Sdk;
using Microsoft.Crm.Sdk.Messages;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Query;
using System;
using System.Collections;
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
                    if (view.Attributes.ContainsKey(idAttributeName))
                    {
                        psObject.Properties.Add(new PSNoteProperty("Id", view.Attributes[idAttributeName]));
                    }

                    psObject.Properties.Add(new PSNoteProperty("ViewType", entityName == "savedquery" ? "System" : "Personal"));

                    // Add properties that correspond to Set-DataverseView parameters
                    if (view.Attributes.ContainsKey("name"))
                    {
                        psObject.Properties.Add(new PSNoteProperty("Name", view.Attributes["name"]));
                    }
                    if (view.Attributes.ContainsKey("returnedtypecode"))
                    {
                        psObject.Properties.Add(new PSNoteProperty("TableName", view.Attributes["returnedtypecode"]));
                    }
                    if (view.Attributes.ContainsKey("description"))
                    {
                        psObject.Properties.Add(new PSNoteProperty("Description", view.Attributes["description"]));
                    }
                    if (view.Attributes.ContainsKey("querytype"))
                    {
                        psObject.Properties.Add(new PSNoteProperty("QueryType", (QueryType)view.Attributes["querytype"]));
                    }
                    if (view.Attributes.ContainsKey("isdefault"))
                    {
                        psObject.Properties.Add(new PSNoteProperty("IsDefault", view.Attributes["isdefault"]));
                    }

                    // Parse FetchXML to extract Columns, Filters, and Links
                    var parsedProperties = ParseFetchXmlForQueryComponents(view.GetAttributeValue<string>("fetchxml"));
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
                }

                WriteObject(psObject);
            }
        }

        private object[] ParseLayoutXmlForColumns(string layoutXml)
        {
            if (string.IsNullOrEmpty(layoutXml))
            {
                return null;
            }

            try
            {
                XNamespace ns = "http://schemas.microsoft.com/crm/2006/query";
                XDocument doc = XDocument.Parse(layoutXml);
                var cells = doc.Descendants(ns + "cell");

                var columns = new System.Collections.Generic.List<object>();

                foreach (var cell in cells)
                {
                    string columnName = cell.Attribute("name")?.Value;
                    string widthStr = cell.Attribute("width")?.Value;

                    if (!string.IsNullOrEmpty(columnName))
                    {
                        var columnConfig = new PSSerializableHashtable();
                        columnConfig["name"] = columnName;

                        if (!string.IsNullOrEmpty(widthStr) && int.TryParse(widthStr, out int width))
                        {
                            columnConfig["width"] = width;

                        }
                        columns.Add(columnConfig);
                    }
                }

                return columns.ToArray();
            }
            catch (Exception ex)
            {
                WriteVerbose($"Failed to parse layout XML for columns: {ex.Message}");
                return null;
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
                XNamespace ns = "http://schemas.microsoft.com/crm/2006/query";
                XDocument doc = XDocument.Parse(layoutXml);
                var cells = doc.Descendants(ns + "cell");

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

        private System.Collections.Generic.Dictionary<string, object> ParseFetchXmlForQueryComponents(string fetchXml)
        {
            var result = new System.Collections.Generic.Dictionary<string, object>();

            if (string.IsNullOrEmpty(fetchXml))
            {
                return result;
            }

            try
            {
                // Use SDK to convert FetchXML to QueryExpression
                var translateRequest = new FetchXmlToQueryExpressionRequest
                {
                    FetchXml = fetchXml
                };
                var translateResponse = (FetchXmlToQueryExpressionResponse)Connection.Execute(translateRequest);
                var query = translateResponse.Query;

                // Extract columns
                if (query.ColumnSet != null && query.ColumnSet.Columns != null && query.ColumnSet.Columns.Count > 0)
                {
                    result["Columns"] = query.ColumnSet.Columns.ToArray();
                }

                // Extract filters
                if (query.Criteria != null && (query.Criteria.Conditions.Count > 0 || query.Criteria.Filters.Count > 0))
                {
                    var filterHashtables = ConvertFilterExpressionToHashtables(query.Criteria);
                    if (filterHashtables.Length > 0)
                    {
                        result["Filters"] = filterHashtables;
                    }
                }

                // Extract links
                if (query.LinkEntities != null && query.LinkEntities.Count > 0)
                {
                    var dataverseLinks = query.LinkEntities.Select(le => new DataverseLinkEntity(le)).ToArray();
                    result["Links"] = dataverseLinks;
                }
                else
                {
                    result["Links"] = new DataverseLinkEntity[0];
                }
            }
            catch (Exception ex)
            {
                WriteVerbose($"Failed to parse FetchXML for query components: {ex.Message}");
            }

            return result;
        }

        private Hashtable[] ConvertFilterExpressionToHashtables(FilterExpression filter)
        {
            var result = new System.Collections.Generic.List<Hashtable>();

            if (filter == null || (filter.Conditions.Count == 0 && filter.Filters.Count == 0))
            {
                return result.ToArray();
            }

            // Convert conditions
            foreach (var condition in filter.Conditions)
            {
                var hashtable = new PSSerializableHashtable();
                string key = condition.AttributeName;

                // Add operator if not Equal
                if (condition.Operator != ConditionOperator.Equal)
                {
                    key += ":" + condition.Operator.ToString();
                }

                if (condition.Values.Count == 1)
                {
                    hashtable[key] = condition.Values[0];
                }
                else if (condition.Values.Count > 1)
                {
                    hashtable[key] = condition.Values.ToArray();
                }
                else
                {
                    hashtable[key] = null;
                }

                result.Add(hashtable);
            }

            // Convert nested filters - for simplicity, we'll flatten them into separate hashtables
            foreach (var nestedFilter in filter.Filters)
            {
                var nestedHashtables = ConvertFilterExpressionToHashtables(nestedFilter);
                result.AddRange(nestedHashtables);
            }

            return result.ToArray();
        }
    }
}
