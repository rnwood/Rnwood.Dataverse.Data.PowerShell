using System;
using System.Management.Automation;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Query;

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
        [Parameter(HelpMessage = "View type to filter by: 0=OtherView, 1=PublicView, 2=AdvancedFind, 4=SubGrid, 8=Dashboard, 16=MobileClientView, 64=LookupView, 128=MainApplicationView, 256=QuickFindSearch, 512=Associated, 1024=CalendarView, 2048=InteractiveExperience")]
        public int? QueryType { get; set; }

        /// <summary>
        /// Processes the cmdlet request.
        /// </summary>
        protected override void ProcessRecord()
        {
            base.ProcessRecord();

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
                    var pattern = new WildcardPattern(Name, WildcardOptions.IgnoreCase);
                    // We'll filter in memory since Dataverse doesn't support wildcard queries directly
                    WriteVerbose($"Will filter by name pattern: {Name}");
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
                query.Criteria.AddCondition("querytype", ConditionOperator.Equal, QueryType.Value);
                WriteVerbose($"Filtering by query type: {QueryType.Value}");
            }

            // Execute query
            var views = Connection.RetrieveMultiple(query);

            WriteVerbose($"Found {views.Entities.Count} view(s) in {entityName}");

            // Convert to PSObjects and output
            foreach (var view in views.Entities)
            {
                // If Name parameter contains wildcards, filter in memory
                if (!string.IsNullOrEmpty(Name) && WildcardPattern.ContainsWildcardCharacters(Name))
                {
                    var viewName = view.GetAttributeValue<string>("name");
                    var pattern = new WildcardPattern(Name, WildcardOptions.IgnoreCase);
                    if (!pattern.IsMatch(viewName ?? ""))
                    {
                        continue;
                    }
                }

                var psObject = new PSObject();
                
                // Add all attributes from the view entity
                foreach (var attribute in view.Attributes)
                {
                    if (attribute.Value is AliasedValue aliasedValue)
                    {
                        psObject.Properties.Add(new PSNoteProperty(attribute.Key, aliasedValue.Value));
                    }
                    else
                    {
                        psObject.Properties.Add(new PSNoteProperty(attribute.Key, attribute.Value));
                    }
                }

                // Add a friendly property to indicate view type
                psObject.Properties.Add(new PSNoteProperty("ViewType", entityName == "savedquery" ? "System" : "Personal"));
                
                // Add normalized Id property for easier pipeline usage
                string idAttributeName = entityName == "savedquery" ? "savedqueryid" : "userqueryid";
                if (view.Attributes.ContainsKey(idAttributeName))
                {
                    psObject.Properties.Add(new PSNoteProperty("Id", view.Attributes[idAttributeName]));
                }

                WriteObject(psObject);
            }
        }
    }
}
