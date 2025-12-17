using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Query;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Management.Automation;
using Microsoft.Xrm.Sdk.Metadata;

namespace Rnwood.Dataverse.Data.PowerShell.Commands
{
    /// <summary>
    /// Retrieves app module (model-driven app) information from a Dataverse environment.
    /// </summary>
    [Cmdlet(VerbsCommon.Get, "DataverseAppModule")]
    [OutputType(typeof(PSObject))]
    public class GetDataverseAppModuleCmdlet : OrganizationServiceCmdlet
    {
        /// <summary>
        /// Gets or sets the ID of the app module to retrieve.
        /// </summary>
        [Parameter(Position = 0, ValueFromPipelineByPropertyName = true, HelpMessage = "The ID of the app module to retrieve.")]
        public Guid Id { get; set; }

        /// <summary>
        /// Gets or sets the unique name of the app module to retrieve.
        /// </summary>
        [Parameter(ValueFromPipelineByPropertyName = true, HelpMessage = "The unique name of the app module to retrieve.")]
        public string UniqueName { get; set; }

        /// <summary>
        /// Gets or sets the name of the app module to retrieve.
        /// </summary>
        [Parameter(HelpMessage = "The name of the app module to retrieve. Supports wildcards.")]
        [SupportsWildcards]
        public string Name { get; set; }

        /// <summary>
        /// Gets or sets whether to return raw values instead of display values.
        /// </summary>
        [Parameter(HelpMessage = "Return raw values instead of display values")]
        public SwitchParameter Raw { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether to retrieve only published records.
        /// </summary>
        [Parameter(HelpMessage = "Allows published records to be retrieved instead of the default behavior that includes both published and unpublished records")]
        public SwitchParameter Published { get; set; }

        /// <summary>
        /// Processes the cmdlet request.
        /// </summary>
        protected override void ProcessRecord()
        {
            base.ProcessRecord();

            WriteVerbose("Querying app modules (appmodule)...");
            QueryAppModules();
        }

        private void QueryAppModules()
        {
            // Build query
            var query = new QueryExpression("appmodule")
            {
                ColumnSet = new ColumnSet(true) // Get all columns
            };

            // Add filters
            if (Id != Guid.Empty)
            {
                query.Criteria.AddCondition("appmoduleid", ConditionOperator.Equal, Id);
                WriteVerbose($"Filtering by ID: {Id}");
            }

            if (!string.IsNullOrEmpty(UniqueName))
            {
                query.Criteria.AddCondition("uniquename", ConditionOperator.Equal, UniqueName);
                WriteVerbose($"Filtering by unique name: {UniqueName}");
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

            // Execute query with paging
            WriteVerbose("Executing query for appmodule");
            IEnumerable<Entity> appModules;
            if (!Published.IsPresent)
            {
                // Get both unpublished and published, with deduplication (unpublished preferred)
                appModules = QueryHelpers.ExecuteQueryWithPublishedAndUnpublished(query, Connection, WriteVerbose);
            }
            else
            {
                // Get only published records
                appModules = QueryHelpers.ExecuteQueryWithPaging(query, Connection, WriteVerbose);
            }

            WriteVerbose($"Found {appModules.Count()} app module(s)");

            // Convert to PSObjects and output with streaming
            foreach (var appModule in appModules)
            {

                if (Raw.IsPresent)
                {
                    // Return raw values
                    WriteObject(appModule);
                }
                else
                {
                    // Create PSObject with commonly used properties
                    PSObject psObject = new PSObject();

                    // Add normalized Id property for easier pipeline usage
                    psObject.Properties.Add(new PSNoteProperty("Id", appModule.GetAttributeValue<Guid>("appmoduleid")));

                    // Add key properties
                    psObject.Properties.Add(new PSNoteProperty("UniqueName", appModule.GetAttributeValue<string>("uniquename")));
                    psObject.Properties.Add(new PSNoteProperty("Name", appModule.GetAttributeValue<string>("name")));
                    psObject.Properties.Add(new PSNoteProperty("Description", appModule.GetAttributeValue<string>("description")));
                    psObject.Properties.Add(new PSNoteProperty("PublishedOn", appModule.GetAttributeValue<DateTime?>("publishedon")));
                    psObject.Properties.Add(new PSNoteProperty("Url", appModule.GetAttributeValue<string>("url")));
                    psObject.Properties.Add(new PSNoteProperty("WebResourceId", appModule.GetAttributeValue<Guid?>("webresourceid")));
                    psObject.Properties.Add(new PSNoteProperty("FormFactor", appModule.GetAttributeValue<int?>("formfactor")));
                    psObject.Properties.Add(new PSNoteProperty("ClientType", appModule.GetAttributeValue<int?>("clienttype")));
                    var navTypeValue = appModule.GetAttributeValue<OptionSetValue>("navigationtype")?.Value;
                    NavigationType? navigationType = null;
                    if (navTypeValue.HasValue)
                    {
                        navigationType = (NavigationType)navTypeValue.Value;
                    }
                    psObject.Properties.Add(new PSNoteProperty("NavigationType", navigationType));
                    psObject.Properties.Add(new PSNoteProperty("IsFeatured", appModule.GetAttributeValue<bool?>("isfeatured")));

                    WriteObject(psObject);
                }
            }
        }
    }
}
