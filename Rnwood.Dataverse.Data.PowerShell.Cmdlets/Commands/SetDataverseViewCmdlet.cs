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
    /// Creates or updates a view (savedquery or userquery) in Dataverse. If a view with the specified ID exists, it will be updated, otherwise a new view is created.
    /// </summary>
    [Cmdlet(VerbsCommon.Set, "DataverseView", SupportsShouldProcess = true, ConfirmImpact = ConfirmImpact.Medium)]
    [OutputType(typeof(Guid))]
    public class SetDataverseViewCmdlet : OrganizationServiceCmdlet
    {
        /// <summary>
        /// Gets or sets the ID of the view to update. If not specified or if the view doesn't exist, a new view is created.
        /// </summary>
        [Parameter(ValueFromPipelineByPropertyName = true, HelpMessage = "ID of the view to update. If not specified or if the view doesn't exist, a new view is created.")]
        public Guid Id { get; set; }

        /// <summary>
        /// Gets or sets the name of the view. Required when creating a new view.
        /// </summary>
        [Parameter(HelpMessage = "Name of the view. Required when creating a new view.")]
        public string Name { get; set; }

        /// <summary>
        /// Gets or sets the logical name of the table (entity) this view is for. Required when creating a new view.
        /// </summary>
        [Parameter(HelpMessage = "Logical name of the table this view is for. Required when creating a new view.")]
        [ArgumentCompleter(typeof(TableNameArgumentCompleter))]
        [Alias("EntityName")]
        public string TableName { get; set; }

        /// <summary>
        /// Gets or sets whether this is a system view (savedquery) or personal view (userquery). Default is personal view.
        /// </summary>
        [Parameter(HelpMessage = "Work with a system view (savedquery) instead of a personal view (userquery)")]
        public SwitchParameter SystemView { get; set; }

        /// <summary>
        /// Gets or sets the description of the view.
        /// </summary>
        [Parameter(HelpMessage = "Description of the view")]
        public string Description { get; set; }

        /// <summary>
        /// Gets or sets the columns to include in the view. Used for creating new views or replacing columns in existing views.
        /// </summary>
        [Parameter(HelpMessage = "Columns to include in the view. Can be an array of column names or hashtables with column configuration (name, width, etc.)")]
        [ArgumentCompleter(typeof(ColumnNameArgumentCompleter))]
        public object[] Columns { get; set; }

        /// <summary>
        /// Gets or sets columns to add to an existing view.
        /// </summary>
        [Parameter(HelpMessage = "Columns to add to the view. Can be an array of column names or hashtables with column configuration (name, width, etc.)")]
        [ArgumentCompleter(typeof(ColumnNameArgumentCompleter))]
        public object[] AddColumns { get; set; }

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
        [Parameter(HelpMessage = "One or more hashtables to filter records. Each hashtable's entries are combined with AND; multiple hashtables are combined with OR. Keys may be 'column' or 'column:Operator' (Operator is a ConditionOperator name). Values may be a literal, an array (treated as IN), $null (treated as ISNULL), or a nested hashtable with keys 'value' and 'operator'. Supports grouped filters using 'and', 'or', 'not', or 'xor' keys with nested hashtables for complex logical expressions.")]
        [ArgumentCompleter(typeof(FilterValuesArgumentCompleter))]
        public Hashtable[] FilterValues { get; set; }

        /// <summary>
        /// Gets or sets the FetchXml query for the view.
        /// </summary>
        [Parameter(HelpMessage = "FetchXml query to use for the view")]
        public string FetchXml { get; set; }

        /// <summary>
        /// Gets or sets the layout XML for the view. If not specified, a default layout will be generated.
        /// </summary>
        [Parameter(HelpMessage = "Layout XML for the view. If not specified when creating, a default layout will be generated from Columns")]
        public string LayoutXml { get; set; }

        /// <summary>
        /// Gets or sets whether this is the default view for the table.
        /// </summary>
        [Parameter(HelpMessage = "Set this view as the default view for the table")]
        public SwitchParameter IsDefault { get; set; }

        /// <summary>
        /// Gets or sets the view type. Default is 1 (public view). Only used when creating a new view.
        /// </summary>
        [Parameter(HelpMessage = "View type: 0=OtherView, 1=PublicView, 2=AdvancedFind, 4=SubGrid, 8=Dashboard, 16=MobileClientView, 64=LookupView, 128=MainApplicationView, 256=QuickFindSearch, 512=Associated, 1024=CalendarView, 2048=InteractiveExperience. Default is 1 (PublicView)")]
        public int QueryType { get; set; } = 1; // Default to PublicView

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

            try
            {
                string entityName = SystemView ? "savedquery" : "userquery";
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
                        WriteVerbose($"Found existing {(SystemView ? "system" : "personal")} view with ID: {Id}");
                    }
                    catch
                    {
                        WriteVerbose($"View with ID {Id} not found");
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

                    if (ShouldProcess($"{(SystemView ? "System" : "Personal")} view with ID '{Id}'", "Update"))
                    {
                        bool updated = false;

                        // Update name if provided
                        if (!string.IsNullOrEmpty(Name))
                        {
                            viewEntity["name"] = Name;
                            updated = true;
                        }

                        // Update description if provided
                        if (!string.IsNullOrEmpty(Description))
                        {
                            viewEntity["description"] = Description;
                            updated = true;
                        }

                        // Update FetchXml if provided directly
                        if (!string.IsNullOrEmpty(FetchXml))
                        {
                            viewEntity["fetchxml"] = FetchXml;
                            updated = true;
                        }
                        // Or build/modify FetchXml based on simple parameters
                        else if (Columns != null || AddColumns != null || RemoveColumns != null || FilterValues != null)
                        {
                            string currentFetchXml = viewEntity.GetAttributeValue<string>("fetchxml");
                            string modifiedFetchXml = ModifyFetchXml(currentFetchXml);
                            viewEntity["fetchxml"] = modifiedFetchXml;
                            updated = true;
                        }

                        // Update LayoutXml if provided directly
                        if (!string.IsNullOrEmpty(LayoutXml))
                        {
                            viewEntity["layoutxml"] = LayoutXml;
                            updated = true;
                        }
                        // Or modify LayoutXml based on column changes
                        else if (Columns != null || AddColumns != null || RemoveColumns != null || UpdateColumns != null)
                        {
                            string currentLayoutXml = viewEntity.GetAttributeValue<string>("layoutxml");
                            string modifiedLayoutXml = ModifyLayoutXml(currentLayoutXml);
                            viewEntity["layoutxml"] = modifiedLayoutXml;
                            updated = true;
                        }

                        // Update IsDefault if provided
                        if (SystemView && IsDefault.IsPresent)
                        {
                            viewEntity["isdefault"] = IsDefault.ToBool();
                            updated = true;
                        }

                        if (updated)
                        {
                            Connection.Update(viewEntity);
                            WriteVerbose($"Updated {(SystemView ? "system" : "personal")} view with ID: {Id}");
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

                    // Build FetchXml from simple filter if needed (when FetchXml not provided but Columns are)
                    if (string.IsNullOrEmpty(fetchXml) && Columns != null)
                    {
                        fetchXml = BuildFetchXmlFromSimpleFilter();
                    }

                    // Build layout XML if not provided
                    if (string.IsNullOrEmpty(layoutXml))
                    {
                        layoutXml = BuildDefaultLayoutXml();
                    }

                    if (ShouldProcess($"{(SystemView ? "System" : "Personal")} view '{Name}' for table '{TableName}'", "Create"))
                    {
                        viewEntity = new Entity(entityName);
                        
                        if (Id != Guid.Empty)
                        {
                            viewEntity.Id = Id;
                        }
                        
                        viewEntity["name"] = Name;
                        viewEntity["returnedtypecode"] = TableName;
                        viewEntity["fetchxml"] = fetchXml;
                        viewEntity["layoutxml"] = layoutXml;
                        viewEntity["querytype"] = QueryType;

                        if (!string.IsNullOrEmpty(Description))
                        {
                            viewEntity["description"] = Description;
                        }

                        if (SystemView && IsDefault)
                        {
                            viewEntity["isdefault"] = true;
                        }

                        viewId = Connection.Create(viewEntity);
                        WriteVerbose($"Created {(SystemView ? "system" : "personal")} view with ID: {viewId}");
                        
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

        private string BuildFetchXmlFromSimpleFilter()
        {
            XElement fetchElement = new XElement("fetch");
            XElement entityElement = new XElement("entity", new XAttribute("name", TableName));

            // Add columns
            if (Columns != null && Columns.Length > 0)
            {
                foreach (object col in Columns)
                {
                    if (col is string columnName)
                    {
                        entityElement.Add(new XElement("attribute", new XAttribute("name", columnName)));
                    }
                    else if (col is Hashtable colConfig)
                    {
                        if (colConfig.ContainsKey("name") || colConfig.ContainsKey("Name"))
                        {
                            string colName = (colConfig["name"] ?? colConfig["Name"])?.ToString();
                            if (!string.IsNullOrEmpty(colName))
                            {
                                entityElement.Add(new XElement("attribute", new XAttribute("name", colName)));
                            }
                        }
                    }
                }
            }
            else
            {
                entityElement.Add(new XElement("all-attributes"));
            }

            // Add filters if provided
            if (FilterValues != null && FilterValues.Length > 0)
            {
                QueryExpression query = new QueryExpression(TableName);
                FilterHelpers.ProcessHashFilterValues(query.Criteria, FilterValues, false);
                
                // Convert filter expression to FetchXml filter element
                XElement filterElement = ConvertFilterExpressionToFetchXml(query.Criteria);
                if (filterElement != null && filterElement.HasElements)
                {
                    entityElement.Add(filterElement);
                }
            }

            fetchElement.Add(entityElement);
            return fetchElement.ToString();
        }

        private string ModifyFetchXml(string currentFetchXml)
        {
            XDocument doc = XDocument.Parse(currentFetchXml);
            XElement entityElement = doc.Descendants("entity").FirstOrDefault();

            if (entityElement == null)
            {
                throw new InvalidOperationException("Invalid FetchXml: No entity element found");
            }

            // Replace columns if Columns parameter is provided
            if (Columns != null && Columns.Length > 0)
            {
                // Remove all existing attributes
                entityElement.Elements("attribute").Remove();
                entityElement.Elements("all-attributes").Remove();

                // Add new columns
                foreach (object col in Columns)
                {
                    string columnName = null;

                    if (col is string colStr)
                    {
                        columnName = colStr;
                    }
                    else if (col is Hashtable colConfig)
                    {
                        columnName = (colConfig["name"] ?? colConfig["Name"])?.ToString();
                    }

                    if (!string.IsNullOrEmpty(columnName))
                    {
                        entityElement.Add(new XElement("attribute", new XAttribute("name", columnName)));
                    }
                }
            }

            // Add columns to FetchXml
            if (AddColumns != null && AddColumns.Length > 0)
            {
                foreach (object col in AddColumns)
                {
                    string columnName = null;

                    if (col is string colStr)
                    {
                        columnName = colStr;
                    }
                    else if (col is Hashtable colConfig)
                    {
                        columnName = (colConfig["name"] ?? colConfig["Name"])?.ToString();
                    }

                    if (!string.IsNullOrEmpty(columnName))
                    {
                        // Check if column already exists
                        var existingAttribute = entityElement.Elements("attribute")
                            .FirstOrDefault(a => a.Attribute("name")?.Value == columnName);

                        if (existingAttribute == null)
                        {
                            entityElement.Add(new XElement("attribute", new XAttribute("name", columnName)));
                        }
                    }
                }
            }

            // Remove columns from FetchXml
            if (RemoveColumns != null && RemoveColumns.Length > 0)
            {
                foreach (string columnName in RemoveColumns)
                {
                    var attributeToRemove = entityElement.Elements("attribute")
                        .FirstOrDefault(a => a.Attribute("name")?.Value == columnName);

                    attributeToRemove?.Remove();
                }
            }

            // Add/replace filters in FetchXml
            if (FilterValues != null && FilterValues.Length > 0)
            {
                // Remove existing filter
                entityElement.Elements("filter").Remove();

                // Convert filter hashtables to FetchXml filter conditions
                string tableName = entityElement.Attribute("name")?.Value;
                QueryExpression query = new QueryExpression(tableName);
                FilterHelpers.ProcessHashFilterValues(query.Criteria, FilterValues, false);

                // Convert filter expression to FetchXml
                XElement filterElement = ConvertFilterExpressionToFetchXml(query.Criteria);
                if (filterElement != null && filterElement.HasElements)
                {
                    entityElement.Add(filterElement);
                }
            }

            return doc.ToString();
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
                    string columnName = null;
                    int columnWidth = width;

                    if (col is string colStr)
                    {
                        columnName = colStr;
                    }
                    else if (col is Hashtable colConfig)
                    {
                        columnName = (colConfig["name"] ?? colConfig["Name"])?.ToString();
                        if (colConfig.ContainsKey("width") || colConfig.ContainsKey("Width"))
                        {
                            object widthObj = colConfig["width"] ?? colConfig["Width"];
                            if (widthObj != null && int.TryParse(widthObj.ToString(), out int parsedWidth))
                            {
                                columnWidth = parsedWidth;
                            }
                        }
                    }

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
            XDocument doc = XDocument.Parse(currentLayoutXml);
            XElement row = doc.Descendants(ns + "row").FirstOrDefault();

            if (row == null)
            {
                throw new InvalidOperationException("Invalid LayoutXml: No row element found");
            }

            // Replace columns if Columns parameter is provided
            if (Columns != null && Columns.Length > 0)
            {
                // Remove all existing cells
                row.Elements(ns + "cell").Remove();

                // Add new columns
                foreach (object col in Columns)
                {
                    string columnName = null;
                    int columnWidth = 100;

                    if (col is string colStr)
                    {
                        columnName = colStr;
                    }
                    else if (col is Hashtable colConfig)
                    {
                        columnName = (colConfig["name"] ?? colConfig["Name"])?.ToString();
                        if (colConfig.ContainsKey("width") || colConfig.ContainsKey("Width"))
                        {
                            object widthObj = colConfig["width"] ?? colConfig["Width"];
                            if (widthObj != null && int.TryParse(widthObj.ToString(), out int parsedWidth))
                            {
                                columnWidth = parsedWidth;
                            }
                        }
                    }

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
                    string columnName = null;
                    int columnWidth = 100;

                    if (col is string colStr)
                    {
                        columnName = colStr;
                    }
                    else if (col is Hashtable colConfig)
                    {
                        columnName = (colConfig["name"] ?? colConfig["Name"])?.ToString();
                        if (colConfig.ContainsKey("width") || colConfig.ContainsKey("Width"))
                        {
                            object widthObj = colConfig["width"] ?? colConfig["Width"];
                            if (widthObj != null && int.TryParse(widthObj.ToString(), out int parsedWidth))
                            {
                                columnWidth = parsedWidth;
                            }
                        }
                    }

                    if (!string.IsNullOrEmpty(columnName))
                    {
                        // Check if cell already exists
                        var existingCell = row.Elements(ns + "cell")
                            .FirstOrDefault(c => c.Attribute("name")?.Value == columnName);

                        if (existingCell == null)
                        {
                            row.Add(new XElement(ns + "cell",
                                new XAttribute("name", columnName),
                                new XAttribute("width", columnWidth)
                            ));
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
                        if (colConfig.ContainsKey("width") || colConfig.ContainsKey("Width"))
                        {
                            object widthObj = colConfig["width"] ?? colConfig["Width"];
                            if (widthObj != null && int.TryParse(widthObj.ToString(), out int parsedWidth))
                            {
                                existingCell.SetAttributeValue("width", parsedWidth);
                            }
                        }
                    }
                }
            }

            return doc.ToString();
        }

        private XElement ConvertFilterExpressionToFetchXml(FilterExpression filter)
        {
            if (filter.Conditions.Count == 0 && filter.Filters.Count == 0)
            {
                return null;
            }

            XElement filterElement = new XElement("filter");
            filterElement.Add(new XAttribute("type", filter.FilterOperator == LogicalOperator.Or ? "or" : "and"));

            // Add conditions
            foreach (ConditionExpression condition in filter.Conditions)
            {
                XElement conditionElement = new XElement("condition",
                    new XAttribute("attribute", condition.AttributeName),
                    new XAttribute("operator", GetFetchXmlOperator(condition.Operator))
                );

                // Add value(s)
                if (condition.Values != null && condition.Values.Count > 0)
                {
                    if (condition.Values.Count == 1)
                    {
                        conditionElement.Add(new XAttribute("value", condition.Values[0]?.ToString() ?? ""));
                    }
                    else
                    {
                        foreach (object value in condition.Values)
                        {
                            conditionElement.Add(new XElement("value", value?.ToString() ?? ""));
                        }
                    }
                }

                filterElement.Add(conditionElement);
            }

            // Add nested filters
            foreach (FilterExpression nestedFilter in filter.Filters)
            {
                XElement nestedFilterElement = ConvertFilterExpressionToFetchXml(nestedFilter);
                if (nestedFilterElement != null)
                {
                    filterElement.Add(nestedFilterElement);
                }
            }

            return filterElement;
        }

        private string GetFetchXmlOperator(ConditionOperator op)
        {
            switch (op)
            {
                case ConditionOperator.Equal: return "eq";
                case ConditionOperator.NotEqual: return "ne";
                case ConditionOperator.GreaterThan: return "gt";
                case ConditionOperator.LessThan: return "lt";
                case ConditionOperator.GreaterEqual: return "ge";
                case ConditionOperator.LessEqual: return "le";
                case ConditionOperator.Like: return "like";
                case ConditionOperator.NotLike: return "not-like";
                case ConditionOperator.In: return "in";
                case ConditionOperator.NotIn: return "not-in";
                case ConditionOperator.Between: return "between";
                case ConditionOperator.NotBetween: return "not-between";
                case ConditionOperator.Null: return "null";
                case ConditionOperator.NotNull: return "not-null";
                case ConditionOperator.Yesterday: return "yesterday";
                case ConditionOperator.Today: return "today";
                case ConditionOperator.Tomorrow: return "tomorrow";
                case ConditionOperator.Last7Days: return "last-seven-days";
                case ConditionOperator.Next7Days: return "next-seven-days";
                case ConditionOperator.LastWeek: return "last-week";
                case ConditionOperator.ThisWeek: return "this-week";
                case ConditionOperator.NextWeek: return "next-week";
                case ConditionOperator.LastMonth: return "last-month";
                case ConditionOperator.ThisMonth: return "this-month";
                case ConditionOperator.NextMonth: return "next-month";
                case ConditionOperator.On: return "on";
                case ConditionOperator.OnOrBefore: return "on-or-before";
                case ConditionOperator.OnOrAfter: return "on-or-after";
                case ConditionOperator.LastYear: return "last-year";
                case ConditionOperator.ThisYear: return "this-year";
                case ConditionOperator.NextYear: return "next-year";
                case ConditionOperator.LastXHours: return "last-x-hours";
                case ConditionOperator.NextXHours: return "next-x-hours";
                case ConditionOperator.LastXDays: return "last-x-days";
                case ConditionOperator.NextXDays: return "next-x-days";
                case ConditionOperator.LastXWeeks: return "last-x-weeks";
                case ConditionOperator.NextXWeeks: return "next-x-weeks";
                case ConditionOperator.LastXMonths: return "last-x-months";
                case ConditionOperator.NextXMonths: return "next-x-months";
                case ConditionOperator.LastXYears: return "last-x-years";
                case ConditionOperator.NextXYears: return "next-x-years";
                case ConditionOperator.EqualUserId: return "eq-userid";
                case ConditionOperator.NotEqualUserId: return "ne-userid";
                case ConditionOperator.EqualBusinessId: return "eq-businessid";
                case ConditionOperator.NotEqualBusinessId: return "ne-businessid";
                case ConditionOperator.Contains: return "contains";
                case ConditionOperator.DoesNotContain: return "not-contain";
                case ConditionOperator.BeginsWith: return "begins-with";
                case ConditionOperator.DoesNotBeginWith: return "not-begin-with";
                case ConditionOperator.EndsWith: return "ends-with";
                case ConditionOperator.DoesNotEndWith: return "not-end-with";
                default: return op.ToString().ToLower();
            }
        }
    }
}
