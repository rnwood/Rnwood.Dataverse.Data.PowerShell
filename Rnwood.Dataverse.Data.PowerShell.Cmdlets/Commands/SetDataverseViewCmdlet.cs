using MarkMpn.Sql4Cds.Engine.FetchXml;
using Microsoft.Crm.Sdk.Messages;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Messages;
using Microsoft.Xrm.Sdk.Metadata;
using Microsoft.Xrm.Sdk.Query;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Management.Automation;
using System.ServiceModel;
using System.Text.RegularExpressions;
using System.Xml.Linq;

namespace Rnwood.Dataverse.Data.PowerShell.Commands
{
    /// <summary>
    /// Creates or updates a view (savedquery or userquery) in Dataverse. If a view with the specified ID exists, it will be updated, otherwise a new view is created.
    /// </summary>
    [Cmdlet(VerbsCommon.Set, "DataverseView", SupportsShouldProcess = true, ConfirmImpact = ConfirmImpact.Medium)]
    [OutputType(typeof(Guid))]
    public class SetDataverseViewCmdlet : OrganizationServiceCmdlet
    {
        private EntityMetadataFactory entityMetadataFactory;

        /// <summary>
        /// Gets or sets the ID of the view to update. If not specified or if the view doesn't exist, a new view is created.
        /// </summary>
        [Parameter(ValueFromPipelineByPropertyName = true, HelpMessage = "ID of the view to update. If not specified or if the view doesn't exist, a new view is created.")]
        public Guid Id { get; set; }

        /// <summary>
        /// Gets or sets the name of the view. Required when creating a new view.
        /// </summary>
        [Parameter(ValueFromPipelineByPropertyName = true, HelpMessage = "Name of the view. Required when creating a new view.")]
        public string Name { get; set; }

        /// <summary>
        /// Gets or sets the logical name of the table (entity) this view is for. Required when creating a new view.
        /// </summary>
        [Parameter(ValueFromPipelineByPropertyName = true, HelpMessage = "Logical name of the table this view is for. Required when creating a new view.")]
        [ArgumentCompleter(typeof(TableNameArgumentCompleter))]
        [Alias("EntityName")]
        public string TableName { get; set; }

        /// <summary>
        /// Gets or sets whether this is a system view (savedquery) or personal view (userquery). Default is system view.
        /// </summary>
        [Parameter(ValueFromPipelineByPropertyName = true, HelpMessage = "Work with a system view (savedquery) instead of a personal view (userquery)")]
        [ValidateSet("System", "Personal")]
        public string ViewType { get; set; } = "System";

        /// <summary>
        /// Gets or sets the description of the view.
        /// </summary>
        [Parameter(ValueFromPipelineByPropertyName = true, HelpMessage = "Description of the view")]
        public string Description { get; set; }

        /// <summary>
        /// Gets or sets the columns to include in the view. Used for creating new views or replacing columns in existing views.
        /// </summary>
        [Parameter(ValueFromPipelineByPropertyName = true, HelpMessage = "Columns to include in the view. Can be an array of column names or hashtables with column configuration (name, width, etc.)")]
        [ArgumentCompleter(typeof(ColumnNameArgumentCompleter))]
        public object[] Columns { get; set; }

        /// <summary>
        /// Gets or sets columns to add to an existing view.
        /// </summary>
        [Parameter(HelpMessage = "Columns to add to the view. Can be an array of column names or hashtables with column configuration (name, width, etc.)")]
        [ArgumentCompleter(typeof(ColumnNameArgumentCompleter))]
        public object[] AddColumns { get; set; }

        /// <summary>
        /// Gets or sets the column name to insert new columns before. Used with AddColumns.
        /// </summary>
        [Parameter(HelpMessage = "Column name to insert new columns before in the layout. Used with AddColumns parameter.")]
        [ArgumentCompleter(typeof(ColumnNameArgumentCompleter))]
        public string InsertColumnsBefore { get; set; }

        /// <summary>
        /// Gets or sets the column name to insert new columns after. Used with AddColumns.
        /// </summary>
        [Parameter(HelpMessage = "Column name to insert new columns after in the layout. Used with AddColumns parameter.")]
        [ArgumentCompleter(typeof(ColumnNameArgumentCompleter))]
        public string InsertColumnsAfter { get; set; }

        /// <summary>
        /// Gets or sets columns to remove from an existing view.
        /// </summary>
        [Parameter(HelpMessage = "Columns to remove from the view")]
        [ArgumentCompleter(typeof(ColumnNameArgumentCompleter))]
        public string[] RemoveColumns { get; set; }

        /// <summary>
        /// Gets or sets columns to update in an existing view.
        /// </summary>
        [Parameter(HelpMessage = "Columns to update in the view. Hashtables with column configuration (name, width, etc.)")]
        public Hashtable[] UpdateColumns { get; set; }

        /// <summary>
        /// Gets or sets filter values for the view query.
        /// </summary>
        [Parameter(ValueFromPipelineByPropertyName = true, HelpMessage = "One or more hashtables to filter records. Each hashtable's entries are combined with AND; multiple hashtables are combined with OR. Keys may be 'column' or 'column:Operator' (Operator is a ConditionOperator name). Values may be a literal, an array (treated as IN), $null (treated as ISNULL), or a nested hashtable with keys 'value' and 'operator'. Supports grouped filters using 'and', 'or', 'not', or 'xor' keys with nested hashtables for complex logical expressions.")]
        [ArgumentCompleter(typeof(FilterValuesArgumentCompleter))]
        [Alias("Filters")]
        public Hashtable[] FilterValues { get; set; }

        /// <summary>
        /// Gets or sets the FetchXml query for the view.
        /// </summary>
        [Parameter(HelpMessage = "FetchXml query to use for the view")]
        public string FetchXml { get; set; }

        /// <summary>
        /// Gets or sets link entities for the view query.
        /// </summary>
        [Parameter(ValueFromPipelineByPropertyName = true, HelpMessage = "Link entities to apply to the view query. Accepts DataverseLinkEntity objects or simplified hashtable syntax")]
        [ArgumentCompleter(typeof(LinksArgumentCompleter))]
        public DataverseLinkEntity[] Links { get; set; }

        /// <summary>
        /// Gets or sets the order by columns for the view query.
        /// </summary>
        [Parameter(ValueFromPipelineByPropertyName = true, HelpMessage = "List of columns to order records by. Suffix column name with - to sort descending.")]
        public string[] OrderBy { get; set; }

        /// <summary>
        /// Gets or sets the layout XML for the view. If not specified, a default layout will be generated.
        /// </summary>
        [Parameter(HelpMessage = "Layout XML for the view. If not specified when creating, a default layout will be generated from Columns")]
        public string LayoutXml { get; set; }

        /// <summary>
        /// Gets or sets whether this is the default view for the table.
        /// </summary>
        [Parameter(ValueFromPipelineByPropertyName = true, HelpMessage = "Set this view as the default view for the table")]
        public SwitchParameter IsDefault { get; set; }

        /// <summary>
        /// Gets or sets the view type. Default is PublicView.
        /// </summary>
        [Parameter(ValueFromPipelineByPropertyName = true, HelpMessage = "View type. Default is MainApplicationView")]
        public QueryType? QueryType { get; set; }

        /// <summary>
        /// If specified, existing views matching the ID will not be updated.
        /// </summary>
        [Parameter(HelpMessage = "If specified, existing views matching the ID will not be updated")]
        public SwitchParameter NoUpdate { get; set; }

        /// <summary>
        /// If specified, then no view will be created even if no existing view matching the ID is found.
        /// </summary>
        [Parameter(HelpMessage = "If specified, then no view will be created even if no existing view matching the ID is found")]
        public SwitchParameter NoCreate { get; set; }

        /// <summary>
        /// If specified, returns the ID of the created or updated view.
        /// </summary>
        [Parameter(HelpMessage = "If specified, returns the ID of the created or updated view")]
        public SwitchParameter PassThru { get; set; }

        /// <summary>
        /// Processes each record in the pipeline.
        /// </summary>
        protected override void ProcessRecord()
        {
            base.ProcessRecord();

            entityMetadataFactory = new EntityMetadataFactory(Connection);

            // Validate parameter combinations
            if (!string.IsNullOrEmpty(InsertColumnsBefore) && !string.IsNullOrEmpty(InsertColumnsAfter))
            {
                throw new ArgumentException("Cannot specify both InsertColumnsBefore and InsertColumnsAfter parameters. Choose one insertion position.");
            }

            if ((!string.IsNullOrEmpty(InsertColumnsBefore) || !string.IsNullOrEmpty(InsertColumnsAfter)) && (AddColumns == null || AddColumns.Length == 0))
            {
                throw new ArgumentException("InsertColumnsBefore and InsertColumnsAfter parameters can only be used with the AddColumns parameter.");
            }

            try
            {
                string entityName = ViewType == "System" ? "savedquery" : "userquery";
                Entity viewEntity = null;
                bool isUpdate = false;
                Guid viewId = Id;

                // Try to retrieve existing view if ID is provided
                if (Id != Guid.Empty)
                {
                    try
                    {
                        viewEntity = Connection.Retrieve(entityName, Id, new ColumnSet(true));
                        isUpdate = true;
                        WriteVerbose($"Found existing {(ViewType == "System" ? "system" : "personal")} view with ID: {Id}");
                    }
                    catch (FaultException<OrganizationServiceFault> ex)
                    {
                        if (ex.HResult == -2146233088) // Object does not exist
                        {
                            WriteVerbose($"View with ID {Id} not found");
                        }
                        else
                        {
                            throw;
                        }
                    }
                }

                if (isUpdate)
                {
                    // Update existing view
                    if (NoUpdate)
                    {
                        WriteVerbose("NoUpdate flag specified, skipping update");
                        if (PassThru)
                        {
                            WriteObject(viewId);
                        }
                        return;
                    }

                    if (ShouldProcess($"{ViewType} view with ID '{Id}'", "Update"))
                    {
                        Entity updateEntity = new Entity(entityName) { Id = viewId };
                        bool updated = false;

                        // Update name if provided and different
                        if (!string.IsNullOrEmpty(Name))
                        {
                            string currentName = viewEntity.GetAttributeValue<string>("name");
                            if (currentName != Name)
                            {
                                updateEntity["name"] = Name;
                                updated = true;
                            }
                        }

                        // Update description if provided and different
                        if (!string.IsNullOrEmpty(Description))
                        {
                            string currentDescription = viewEntity.GetAttributeValue<string>("description");
                            if (currentDescription != Description)
                            {
                                updateEntity["description"] = Description;
                                updated = true;
                            }
                        }

                        // Update FetchXml if provided directly and different
                        if (!string.IsNullOrEmpty(FetchXml))
                        {
                            string currentFetchXml = viewEntity.GetAttributeValue<string>("fetchxml");
                            if (currentFetchXml != FetchXml)
                            {
                                updateEntity["fetchxml"] = FetchXml;
                                updated = true;
                            }
                        }
                        // Or build/modify FetchXml based on simple parameters
                        else if (Columns != null || AddColumns != null || RemoveColumns != null || FilterValues != null || Links != null || OrderBy != null)
                        {
                            string currentFetchXml = viewEntity.GetAttributeValue<string>("fetchxml");
                            bool isQuickFind = viewEntity.GetAttributeValue<int>("querytype") == 4;
                            string tableName = viewEntity.GetAttributeValue<string>("returnedtypecode");
                            string modifiedFetchXml = ModifyFetchXml(currentFetchXml, isQuickFind, tableName, viewId);
                            if (currentFetchXml != modifiedFetchXml)
                            {
                                updateEntity["fetchxml"] = modifiedFetchXml;
                                updated = true;
                            }
                        }

                        // Update LayoutXml if provided directly and different
                        if (!string.IsNullOrEmpty(LayoutXml))
                        {
                            string currentLayoutXml = viewEntity.GetAttributeValue<string>("layoutxml");
                            if (currentLayoutXml != LayoutXml)
                            {
                                updateEntity["layoutxml"] = LayoutXml;
                                updated = true;
                            }
                        }
                        // Or modify LayoutXml based on column changes
                        else if (Columns != null || AddColumns != null || RemoveColumns != null || UpdateColumns != null)
                        {
                            string currentLayoutXml = viewEntity.GetAttributeValue<string>("layoutxml");
                            string modifiedLayoutXml = ModifyLayoutXml(currentLayoutXml);
                            if (currentLayoutXml != modifiedLayoutXml)
                            {
                                updateEntity["layoutxml"] = modifiedLayoutXml;
                                updated = true;
                            }
                        }

                        // Update IsDefault if provided and different (only for system views)
                        if (ViewType == "System" && IsDefault.IsPresent)
                        {
                            bool currentIsDefault = viewEntity.GetAttributeValue<bool>("isdefault");
                            if (currentIsDefault != IsDefault.ToBool())
                            {
                                updateEntity["isdefault"] = IsDefault.ToBool();
                                updated = true;
                            }
                        }

                        // Update QueryType if provided and different
                        if (QueryType.HasValue)
                        {
                            int currentQueryType = viewEntity.GetAttributeValue<int>("querytype");
                            if (currentQueryType != (int)QueryType.Value)
                            {
                                updateEntity["querytype"] = (int)QueryType.Value;
                                updated = true;
                            }
                        }

                        if (updated)
                        {
                            var converter = new DataverseEntityConverter(Connection, entityMetadataFactory);
                            string columnSummary = QueryHelpers.GetColumnSummary(updateEntity, converter, false);
                            Connection.Update(updateEntity);
                            WriteVerbose($"Updated {(ViewType == "System" ? "system" : "personal")} view with ID: {Id} columns:\n{columnSummary}");
                        }
                        else
                        {
                            WriteWarning("No modifications specified. View was not updated.");
                        }

                        if (PassThru)
                        {
                            WriteObject(viewId);
                        }
                    }
                }
                else
                {
                    // Create new view
                    if (NoCreate)
                    {
                        WriteVerbose("NoCreate flag specified and view not found, skipping creation");
                        return;
                    }

                    // Validate required parameters for creation
                    if (string.IsNullOrEmpty(Name))
                    {
                        throw new ArgumentException("Name is required when creating a new view");
                    }
                    if (string.IsNullOrEmpty(TableName))
                    {
                        throw new ArgumentException("TableName is required when creating a new view");
                    }

                    string fetchXml = FetchXml;
                    string layoutXml = LayoutXml;

                    // Build FetchXml from simple parameters if not provided directly
                    if (string.IsNullOrEmpty(fetchXml) && (Columns != null || FilterValues != null || Links != null || OrderBy != null))
                    {
                        fetchXml = BuildFetchXmlFromSimpleFilter(QueryType.HasValue && QueryType.Value == Commands.QueryType.QuickFindSearch);
                    }

                    // Build layout XML if not provided
                    if (string.IsNullOrEmpty(layoutXml))
                    {
                        layoutXml = BuildDefaultLayoutXml();
                    }

                    if (ShouldProcess($"{ViewType} view '{Name}' for table '{TableName}'", "Create"))
                    {
                        Entity newEntity = new Entity(entityName);

                        if (Id != Guid.Empty)
                        {
                            newEntity.Id = Id;
                        }

                        newEntity["name"] = Name;
                        newEntity["returnedtypecode"] = TableName;
                        newEntity["fetchxml"] = fetchXml;
                        newEntity["layoutxml"] = layoutXml;
                        newEntity["querytype"] = (int)(QueryType ?? Commands.QueryType.MainApplicationView);

                        if (!string.IsNullOrEmpty(Description))
                        {
                            newEntity["description"] = Description;
                        }

                        if (ViewType == "System" && IsDefault)
                        {
                            newEntity["isdefault"] = true;
                        }

                        var converter = new DataverseEntityConverter(Connection, entityMetadataFactory);
                        string columnSummary = QueryHelpers.GetColumnSummary(newEntity, converter, false);
                        viewId = Connection.Create(newEntity);
                        WriteVerbose($"Created new {(ViewType == "System" ? "system" : "personal")} view with ID: {viewId} columns:\n{columnSummary}");

                        if (PassThru)
                        {
                            WriteObject(viewId);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                WriteError(new ErrorRecord(ex, "SetDataverseViewError", ErrorCategory.InvalidOperation, null));
            }
        }

        private string BuildFetchXmlFromSimpleFilter(bool isQuickfind)
        {
            // Build using QueryExpression, then convert to FetchXml
            QueryExpression query = new QueryExpression(TableName);


            // Add columns
            if (Columns != null && Columns.Length > 0)
            {
                query.ColumnSet = new ColumnSet(Columns.Select(c => ExtractColumnName(c)).ToArray());
            }
            else
            {
                query.ColumnSet = new ColumnSet();
            }

            // Add filters if provided
            if (FilterValues != null && FilterValues.Length > 0)
            {
                FilterHelpers.ProcessHashFilterValues(query.Criteria, FilterValues, false);
            }

            // Add links if provided
            if (Links != null && Links.Length > 0)
            {
                foreach (var link in Links)
                {
                    query.LinkEntities.Add(link.LinkEntity);
                }
            }

            // Add order by
            if (OrderBy != null)
            {
                foreach (string orderByColumn in OrderBy)
                {
                    string columnName = orderByColumn.TrimEnd('+', '-');
                    bool descending = orderByColumn.EndsWith("-");
                    query.AddOrder(columnName, descending ? OrderType.Descending : OrderType.Ascending);
                }
            }

            // Convert to FetchXml
            return ConvertQueryExpressionToFetchXml(query);
        }

        private string ModifyFetchXml(string currentFetchXml, bool isQuickFind, string tableName, Guid viewId)
        {
            Dictionary<string, string> fetchToQueryPlaceholderMap = null;
            if (isQuickFind)
            {
                currentFetchXml = QuickFindHelper.PreprocessFetchXmlForQuickFind(currentFetchXml, tableName, entityMetadataFactory, out fetchToQueryPlaceholderMap);
            }

            QueryExpression query = ConvertFetchXmlToQueryExpression(currentFetchXml);
            if (isQuickFind)
            {
                QuickFindHelper.ReplacePlaceholdersInFilter(query.Criteria, fetchToQueryPlaceholderMap);
            }

            // Replace columns if Columns parameter is provided
            if (Columns != null && Columns.Length > 0)
            {
                query.ColumnSet = new ColumnSet(Columns.Select(c => ExtractColumnName(c)).ToArray());
            }

            // Add columns
            if (AddColumns != null && AddColumns.Length > 0)
            {
                var currentColumns = query.ColumnSet.AllColumns ? new System.Collections.Generic.List<string>() : query.ColumnSet.Columns.ToList();

                foreach (object col in AddColumns)
                {
                    string columnName = ExtractColumnName(col);
                    if (!string.IsNullOrEmpty(columnName) && !currentColumns.Contains(columnName))
                    {
                        // Default behavior: append to end
                        currentColumns.Add(columnName);
                    }
                }

                query.ColumnSet = new ColumnSet(currentColumns.ToArray());
            }

            // Remove columns
            if (RemoveColumns != null && RemoveColumns.Length > 0)
            {
                if (!query.ColumnSet.AllColumns)
                {
                    var currentColumns = query.ColumnSet.Columns.ToList();
                    foreach (string columnName in RemoveColumns)
                    {
                        currentColumns.Remove(columnName);
                    }
                    query.ColumnSet = new ColumnSet(currentColumns.ToArray());
                }
            }

            // Add/replace filters
            if (FilterValues != null && FilterValues.Length > 0)
            {
                query.Criteria = new FilterExpression(LogicalOperator.And);

                FilterHelpers.ProcessHashFilterValues(query.Criteria, FilterValues, false);
            }

            // Add/replace links
            if (Links != null && Links.Length > 0)
            {
                query.LinkEntities.Clear();
                foreach (var link in Links)
                {
                    query.LinkEntities.Add(link.LinkEntity);
                }
            }

            // Add/replace order by
            if (OrderBy != null && OrderBy.Length > 0)
            {
                query.Orders.Clear();
                foreach (string orderByColumn in OrderBy)
                {
                    string columnName = orderByColumn.TrimEnd('+', '-');
                    bool descending = orderByColumn.EndsWith("-");
                    query.AddOrder(columnName, descending ? OrderType.Descending : OrderType.Ascending);
                }
            }
            else
            {
                // If OrderBy is null or empty, clear existing orders
                query.Orders.Clear();
            }

            Dictionary<string, string> queryToFetchPlaceholderMap = null;
            if (isQuickFind)
            {
                QuickFindHelper.PreprocessFilterForQuickFind(query.Criteria, tableName, entityMetadataFactory, out queryToFetchPlaceholderMap);
            }

            // Convert back to FetchXml
            string modifiedFetchXml = ConvertQueryExpressionToFetchXml(query);

            if (isQuickFind)
            {
                modifiedFetchXml = QuickFindHelper.PostprocessFetchXmlForQuickFind(modifiedFetchXml, queryToFetchPlaceholderMap);

                // Fix the fetch tag by adding savedqueryid
                XDocument doc = XDocument.Parse(modifiedFetchXml);
                XElement fetch = doc.Root;
                fetch.SetAttributeValue("savedqueryid", viewId.ToString());
                fetch.SetAttributeValue("output-format", "xml-platform");
                fetch.SetAttributeValue("useraworderby", null);
                fetch.SetAttributeValue("no-lock", null);
                fetch.SetAttributeValue("distinct", null);
                fetch.SetAttributeValue("version", "1.0");
                modifiedFetchXml = doc.ToString();
            }

            return modifiedFetchXml;
        }

        private string BuildDefaultLayoutXml()
        {
            XNamespace ns = "http://schemas.microsoft.com/crm/2006/query";
            XElement grid = new XElement(ns + "grid",
                new XAttribute("name", "resultset"),
                new XAttribute("object", TableName),
                new XAttribute("jump", TableName + "id"),
                new XAttribute("select", "1"),
                new XAttribute("icon", "1"),
                new XAttribute("preview", "1")
            );

            XElement row = new XElement(ns + "row",
                new XAttribute("name", "result"),
                new XAttribute("id", TableName + "id")
            );

            if (Columns != null && Columns.Length > 0)
            {
                int width = 100; // Default width
                foreach (object col in Columns)
                {
                    string columnName = ExtractColumnName(col);
                    int columnWidth = ExtractColumnWidth(col, width);

                    if (!string.IsNullOrEmpty(columnName))
                    {
                        row.Add(new XElement(ns + "cell",
                            new XAttribute("name", columnName),
                            new XAttribute("width", columnWidth)
                        ));
                    }
                }
            }
            else
            {
                // If no columns specified, add a default column for the primary key
                row.Add(new XElement(ns + "cell",
                    new XAttribute("name", TableName + "id"),
                    new XAttribute("width", 100)
                ));
            }

            grid.Add(row);
            return grid.ToString();
        }

        private string ModifyLayoutXml(string currentLayoutXml)
        {
            XNamespace ns = "http://schemas.microsoft.com/crm/2006/query";
            XDocument doc;
            XElement row;

            try
            {
                doc = XDocument.Parse(currentLayoutXml);
                row = doc.Descendants(ns + "row").FirstOrDefault();
            }
            catch (Exception)
            {
                // If parsing fails or no row element found, rebuild from scratch
                return BuildDefaultLayoutXml();
            }

            if (row == null)
            {
                // If no row element found, rebuild from scratch
                return BuildDefaultLayoutXml();
            }

            // Replace columns if Columns parameter is provided
            if (Columns != null && Columns.Length > 0)
            {
                // Remove all existing cells
                row.Elements(ns + "cell").Remove();

                // Add new columns
                foreach (object col in Columns)
                {
                    string columnName = ExtractColumnName(col);
                    int columnWidth = ExtractColumnWidth(col, 100);

                    if (!string.IsNullOrEmpty(columnName))
                    {
                        row.Add(new XElement(ns + "cell",
                            new XAttribute("name", columnName),
                            new XAttribute("width", columnWidth)
                        ));
                    }
                }
            }

            // Add columns to LayoutXml
            if (AddColumns != null && AddColumns.Length > 0)
            {
                foreach (object col in AddColumns)
                {
                    string columnName = ExtractColumnName(col);
                    int columnWidth = ExtractColumnWidth(col, 100);

                    if (!string.IsNullOrEmpty(columnName))
                    {
                        // Check if cell already exists
                        var existingCell = row.Elements(ns + "cell")
                            .FirstOrDefault(c => c.Attribute("name")?.Value == columnName);

                        if (existingCell == null)
                        {
                            var newCell = new XElement(ns + "cell",
                                new XAttribute("name", columnName),
                                new XAttribute("width", columnWidth));

                            // Insert at specific position if InsertBefore or InsertAfter is specified
                            if (!string.IsNullOrEmpty(InsertColumnsBefore))
                            {
                                var insertBeforeCell = row.Elements(ns + "cell")
                                    .FirstOrDefault(c => c.Attribute("name")?.Value == InsertColumnsBefore);
                                if (insertBeforeCell != null)
                                {
                                    insertBeforeCell.AddBeforeSelf(newCell);
                                }
                                else
                                {
                                    row.Add(newCell);
                                }
                            }
                            else if (!string.IsNullOrEmpty(InsertColumnsAfter))
                            {
                                var insertAfterCell = row.Elements(ns + "cell")
                                    .FirstOrDefault(c => c.Attribute("name")?.Value == InsertColumnsAfter);
                                if (insertAfterCell != null)
                                {
                                    insertAfterCell.AddAfterSelf(newCell);
                                }
                                else
                                {
                                    row.Add(newCell);
                                }
                            }
                            else
                            {
                                // Default behavior: append to end
                                row.Add(newCell);
                            }
                        }
                    }
                }
            }

            // Remove columns from LayoutXml
            if (RemoveColumns != null && RemoveColumns.Length > 0)
            {
                foreach (string columnName in RemoveColumns)
                {
                    var cellToRemove = row.Elements(ns + "cell")
                        .FirstOrDefault(c => c.Attribute("name")?.Value == columnName);

                    cellToRemove?.Remove();
                }
            }

            // Update columns in LayoutXml
            if (UpdateColumns != null && UpdateColumns.Length > 0)
            {
                foreach (Hashtable colConfig in UpdateColumns)
                {
                    string columnName = (colConfig["name"] ?? colConfig["Name"])?.ToString();
                    if (string.IsNullOrEmpty(columnName))
                    {
                        continue;
                    }

                    var existingCell = row.Elements(ns + "cell")
                        .FirstOrDefault(c => c.Attribute("name")?.Value == columnName);

                    if (existingCell != null)
                    {
                        int columnWidth = ExtractColumnWidth(colConfig, 100);
                        existingCell.SetAttributeValue("width", columnWidth);
                    }
                }
            }

            return doc.ToString();
        }

        /// <summary>
        /// Converts a QueryExpression to FetchXml.
        /// </summary>
        private string ConvertQueryExpressionToFetchXml(QueryExpression query)
        {
            var request = new QueryExpressionToFetchXmlRequest { Query = query };
            var response = (QueryExpressionToFetchXmlResponse)Connection.Execute(request);
            string fetchXml = response.FetchXml;

            // Post-process to remove single top-level AND filter if it contains only nested filters
            XDocument doc = XDocument.Parse(fetchXml);
            XElement entity = doc.Descendants("entity").FirstOrDefault();
            if (entity != null)
            {
                var filters = entity.Elements("filter").ToList();
                if (filters.Count == 1 && filters[0].Attribute("type")?.Value == "and")
                {
                    XElement filter = filters[0];
                    var children = filter.Elements().ToList();
                    if (children.All(c => c.Name.LocalName == "filter"))
                    {
                        // Move nested filters to entity level
                        foreach (var child in children)
                        {
                            entity.Add(child);
                        }
                        // Remove the redundant AND filter
                        filter.Remove();
                    }
                }
            }

            return doc.ToString();
        }

        /// <summary>
        /// Converts FetchXml to a QueryExpression.
        /// </summary>
        private QueryExpression ConvertFetchXmlToQueryExpression(string fetchXml)
        {
            var request = new FetchXmlToQueryExpressionRequest { FetchXml = fetchXml };
            var response = (FetchXmlToQueryExpressionResponse)Connection.Execute(request);
            return response.Query;
        }

        /// <summary>
        /// Extracts the column name from a column specification (string or hashtable).
        /// </summary>
        private string ExtractColumnName(object col)
        {
            if (col is string columnName)
            {
                return columnName;
            }
            else if (col is Hashtable colConfig)
            {
                return (colConfig["name"] ?? colConfig["Name"])?.ToString();
            }
            return null;
        }

        /// <summary>
        /// Extracts the column width from a column specification (string or hashtable).
        /// </summary>
        private int ExtractColumnWidth(object col, int defaultWidth)
        {
            if (col is Hashtable colConfig)
            {
                if (colConfig.ContainsKey("width") || colConfig.ContainsKey("Width"))
                {
                    object widthObj = colConfig["width"] ?? colConfig["Width"];
                    if (widthObj != null && int.TryParse(widthObj.ToString(), out int parsedWidth))
                    {
                        return parsedWidth;
                    }
                }
            }
            return defaultWidth;
        }
    }
}
