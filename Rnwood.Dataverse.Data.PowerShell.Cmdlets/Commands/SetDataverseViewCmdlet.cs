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
    /// Modifies an existing view (savedquery or userquery) in Dataverse.
    /// </summary>
    [Cmdlet(VerbsCommon.Set, "DataverseView", SupportsShouldProcess = true, ConfirmImpact = ConfirmImpact.Medium)]
    public class SetDataverseViewCmdlet : OrganizationServiceCmdlet
    {
        private const string PARAMSET_SIMPLE = "Simple";
        private const string PARAMSET_FETCHXML = "FetchXml";

        /// <summary>
        /// Gets or sets the ID of the view to modify.
        /// </summary>
        [Parameter(Mandatory = true, ValueFromPipelineByPropertyName = true, HelpMessage = "ID of the view to modify")]
        public Guid Id { get; set; }

        /// <summary>
        /// Gets or sets whether this is a system view (savedquery) or personal view (userquery). Default is personal view.
        /// </summary>
        [Parameter(HelpMessage = "Modify a system view (savedquery) instead of a personal view (userquery)")]
        public SwitchParameter SystemView { get; set; }

        /// <summary>
        /// Gets or sets the new name of the view.
        /// </summary>
        [Parameter(HelpMessage = "New name for the view")]
        public string Name { get; set; }

        /// <summary>
        /// Gets or sets the new description of the view.
        /// </summary>
        [Parameter(HelpMessage = "New description for the view")]
        public string Description { get; set; }

        /// <summary>
        /// Gets or sets columns to add to the view.
        /// </summary>
        [Parameter(ParameterSetName = PARAMSET_SIMPLE, HelpMessage = "Columns to add to the view. Can be an array of column names or hashtables with column configuration (name, width, etc.)")]
        [ArgumentCompleter(typeof(ColumnNameArgumentCompleter))]
        public object[] AddColumns { get; set; }

        /// <summary>
        /// Gets or sets columns to remove from the view.
        /// </summary>
        [Parameter(ParameterSetName = PARAMSET_SIMPLE, HelpMessage = "Columns to remove from the view")]
        [ArgumentCompleter(typeof(ColumnNameArgumentCompleter))]
        public string[] RemoveColumns { get; set; }

        /// <summary>
        /// Gets or sets columns to update in the view.
        /// </summary>
        [Parameter(ParameterSetName = PARAMSET_SIMPLE, HelpMessage = "Columns to update in the view. Hashtables with column configuration (name, width, etc.)")]
        public Hashtable[] UpdateColumns { get; set; }

        /// <summary>
        /// Gets or sets filter values to add/replace in the view query.
        /// </summary>
        [Parameter(ParameterSetName = PARAMSET_SIMPLE, HelpMessage = "Filter values to add or replace in the view. One or more hashtables to filter records.")]
        [ArgumentCompleter(typeof(FilterValuesArgumentCompleter))]
        public Hashtable[] FilterValues { get; set; }

        /// <summary>
        /// Gets or sets the FetchXml query to replace the view's query.
        /// </summary>
        [Parameter(ParameterSetName = PARAMSET_FETCHXML, HelpMessage = "FetchXml query to replace the view's query")]
        public string FetchXml { get; set; }

        /// <summary>
        /// Gets or sets the layout XML to replace the view's layout.
        /// </summary>
        [Parameter(HelpMessage = "Layout XML to replace the view's layout")]
        public string LayoutXml { get; set; }

        /// <summary>
        /// Gets or sets whether this is the default view for the table.
        /// </summary>
        [Parameter(HelpMessage = "Set this view as the default view for the table")]
        public SwitchParameter IsDefault { get; set; }

        /// <summary>
        /// Processes each record in the pipeline.
        /// </summary>
        protected override void ProcessRecord()
        {
            base.ProcessRecord();

            try
            {
                string entityName = SystemView ? "savedquery" : "userquery";

                if (ShouldProcess($"{(SystemView ? "System" : "Personal")} view with ID '{Id}'", "Modify"))
                {
                    // Retrieve existing view
                    Entity viewEntity = Connection.Retrieve(entityName, Id, new ColumnSet(true));

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
                    // Or modify FetchXml based on simple parameters
                    else if (ParameterSetName == PARAMSET_SIMPLE && (AddColumns != null || RemoveColumns != null || FilterValues != null))
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
                    else if (ParameterSetName == PARAMSET_SIMPLE && (AddColumns != null || RemoveColumns != null || UpdateColumns != null))
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
                }
            }
            catch (Exception ex)
            {
                WriteError(new ErrorRecord(ex, "SetDataverseViewError", ErrorCategory.InvalidOperation, null));
            }
        }

        private string ModifyFetchXml(string currentFetchXml)
        {
            XDocument doc = XDocument.Parse(currentFetchXml);
            XElement entityElement = doc.Descendants("entity").FirstOrDefault();

            if (entityElement == null)
            {
                throw new InvalidOperationException("Invalid FetchXml: No entity element found");
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

        private string ModifyLayoutXml(string currentLayoutXml)
        {
            XNamespace ns = "http://schemas.microsoft.com/crm/2006/query";
            XDocument doc = XDocument.Parse(currentLayoutXml);
            XElement row = doc.Descendants(ns + "row").FirstOrDefault();

            if (row == null)
            {
                throw new InvalidOperationException("Invalid LayoutXml: No row element found");
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
