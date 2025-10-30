using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Query;
using System;
using System.Collections;
using System.Linq;
using System.Management.Automation;
using System.Text;
using System.Xml.Linq;

namespace Rnwood.Dataverse.Data.PowerShell.Commands
{
    /// <summary>
    /// Creates a new view (savedquery or userquery) in Dataverse.
    /// </summary>
    [Cmdlet(VerbsCommon.New, "DataverseView", SupportsShouldProcess = true, ConfirmImpact = ConfirmImpact.Low)]
    [OutputType(typeof(Guid))]
    public class NewDataverseViewCmdlet : OrganizationServiceCmdlet
    {
        private const string PARAMSET_SIMPLE = "Simple";
        private const string PARAMSET_FETCHXML = "FetchXml";

        /// <summary>
        /// Gets or sets the name of the view.
        /// </summary>
        [Parameter(Mandatory = true, HelpMessage = "Name of the view")]
        public string Name { get; set; }

        /// <summary>
        /// Gets or sets the logical name of the table (entity) this view is for.
        /// </summary>
        [Parameter(Mandatory = true, HelpMessage = "Logical name of the table this view is for")]
        [ArgumentCompleter(typeof(TableNameArgumentCompleter))]
        [Alias("EntityName")]
        public string TableName { get; set; }

        /// <summary>
        /// Gets or sets whether to create a system view (savedquery) or personal view (userquery). Default is personal view.
        /// </summary>
        [Parameter(HelpMessage = "Create a system view (savedquery) instead of a personal view (userquery)")]
        public SwitchParameter SystemView { get; set; }

        /// <summary>
        /// Gets or sets the description of the view.
        /// </summary>
        [Parameter(HelpMessage = "Description of the view")]
        public string Description { get; set; }

        /// <summary>
        /// Gets or sets the columns to include in the view. Array of column names or hashtables with column configuration.
        /// </summary>
        [Parameter(ParameterSetName = PARAMSET_SIMPLE, Mandatory = true, HelpMessage = "Columns to include in the view. Can be an array of column names or hashtables with column configuration (name, width, etc.)")]
        [ArgumentCompleter(typeof(ColumnNameArgumentCompleter))]
        public object[] Columns { get; set; }

        /// <summary>
        /// Gets or sets filter values for the view query.
        /// </summary>
        [Parameter(ParameterSetName = PARAMSET_SIMPLE, HelpMessage = "One or more hashtables to filter records. Each hashtable's entries are combined with AND; multiple hashtables are combined with OR. Keys may be 'column' or 'column:Operator' (Operator is a ConditionOperator name). Values may be a literal, an array (treated as IN), $null (treated as ISNULL), or a nested hashtable with keys 'value' and 'operator'. Supports grouped filters using 'and', 'or', 'not', or 'xor' keys with nested hashtables for complex logical expressions.")]
        [ArgumentCompleter(typeof(FilterValuesArgumentCompleter))]
        public Hashtable[] FilterValues { get; set; }

        /// <summary>
        /// Gets or sets the FetchXml query for the view.
        /// </summary>
        [Parameter(ParameterSetName = PARAMSET_FETCHXML, Mandatory = true, HelpMessage = "FetchXml query to use for the view")]
        public string FetchXml { get; set; }

        /// <summary>
        /// Gets or sets the layout XML for the view. If not specified, a default layout will be generated.
        /// </summary>
        [Parameter(HelpMessage = "Layout XML for the view. If not specified, a default layout will be generated from Columns")]
        public string LayoutXml { get; set; }

        /// <summary>
        /// Gets or sets whether this is the default view for the table.
        /// </summary>
        [Parameter(HelpMessage = "Set this view as the default view for the table")]
        public SwitchParameter IsDefault { get; set; }

        /// <summary>
        /// Gets or sets the view type. Default is 1 (public view).
        /// </summary>
        [Parameter(HelpMessage = "View type: 0=OtherView, 1=PublicView, 2=AdvancedFind, 4=SubGrid, 8=Dashboard, 16=MobileClientView, 64=LookupView, 128=MainApplicationView, 256=QuickFindSearch, 512=Associated, 1024=CalendarView, 2048=InteractiveExperience. Default is 1 (PublicView)")]
        public int QueryType { get; set; } = 1; // Default to PublicView

        /// <summary>
        /// Processes each record in the pipeline.
        /// </summary>
        protected override void ProcessRecord()
        {
            base.ProcessRecord();

            try
            {
                string entityName = SystemView ? "savedquery" : "userquery";
                string fetchXml = FetchXml;
                string layoutXml = LayoutXml;

                // Build FetchXml from simple filter if needed
                if (ParameterSetName == PARAMSET_SIMPLE)
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
                    Entity viewEntity = new Entity(entityName);
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

                    Guid viewId = Connection.Create(viewEntity);
                    WriteVerbose($"Created {(SystemView ? "system" : "personal")} view with ID: {viewId}");
                    WriteObject(viewId);
                }
            }
            catch (Exception ex)
            {
                WriteError(new ErrorRecord(ex, "NewDataverseViewError", ErrorCategory.InvalidOperation, null));
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
    }
}
