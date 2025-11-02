using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Query;
using System;
using System.Collections;
using System.Linq;
using System.Management.Automation;
using System.Xml.Linq;

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
        [Parameter(HelpMessage = "Retrieve system views (savedquery) instead of personal views (userquery)")]
        public SwitchParameter SystemView { get; set; }

        /// <summary>
        /// Gets or sets whether to retrieve personal views (userquery). Default is both system and personal.
        /// </summary>
        [Parameter(HelpMessage = "Retrieve personal views (userquery) instead of system views (savedquery)")]
        public SwitchParameter PersonalView { get; set; }

        /// <summary>
        /// Gets or sets the view type to filter by.
        /// </summary>
        [Parameter(HelpMessage = "View type to filter by")]
        public QueryType? QueryType { get; set; }

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
            bool querySystemViews = !PersonalView.IsPresent || SystemView.IsPresent;
            bool queryPersonalViews = !SystemView.IsPresent || PersonalView.IsPresent;

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
                // Use DataverseEntityConverter to properly convert values (e.g., OptionSetValue to display labels)
                var psObject = entityConverter.ConvertToPSObject(view, new ColumnSet(true), _ => ValueType.Display);

                // Add a friendly property to indicate view type
                psObject.Properties.Add(new PSNoteProperty("ViewType", entityName == "savedquery" ? "System" : "Personal"));
                
                // Add normalized Id property for easier pipeline usage
                string idAttributeName = entityName == "savedquery" ? "savedqueryid" : "userqueryid";
                if (view.Attributes.ContainsKey(idAttributeName))
                {
                    psObject.Properties.Add(new PSNoteProperty("Id", view.Attributes[idAttributeName]));
                }

                // Parse layoutxml to create Columns property in the format expected by Set-DataverseView
                var columnsProperty = ParseLayoutXmlForColumns(view.GetAttributeValue<string>("layoutxml"));
                if (columnsProperty != null)
                {
                    psObject.Properties.Add(new PSNoteProperty("Columns", columnsProperty));
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
                        if (!string.IsNullOrEmpty(widthStr) && int.TryParse(widthStr, out int width) && width != 100)
                        {
                            // Only include width if it's not the default 100
                            var columnConfig = new Hashtable();
                            columnConfig["name"] = columnName;
                            columnConfig["width"] = width;
                            columns.Add(columnConfig);
                        }
                        else
                        {
                            // Just the column name as a string
                            columns.Add(columnName);
                        }
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
    }
}
