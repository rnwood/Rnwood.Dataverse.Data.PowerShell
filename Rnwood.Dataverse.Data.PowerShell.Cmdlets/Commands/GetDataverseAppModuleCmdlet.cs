using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Query;
using System;
using System.Linq;
using System.Management.Automation;

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
            var appModules = QueryHelpers.ExecuteQueryWithPaging(query, Connection, WriteVerbose);

            WriteVerbose($"Found {appModules.Count()} app module(s)");

            // Convert to PSObjects and output with streaming
            foreach (var appModule in appModules)
            {
                PSObject psObject;

                if (Raw.IsPresent)
                {
                    // Return raw values
                    psObject = entityConverter.ConvertToPSObject(appModule, new ColumnSet(true), _ => ValueType.Raw);
                }
                else
                {
                    // Create PSObject with commonly used properties
                    psObject = new PSObject();

                    // Add normalized Id property for easier pipeline usage
                    if (appModule.Attributes.TryGetValue("appmoduleid", out var idValue))
                    {
                        psObject.Properties.Add(new PSNoteProperty("Id", idValue));
                    }

                    // Add key properties
                    if (appModule.Attributes.TryGetValue("uniquename", out var uniqueNameValue))
                    {
                        psObject.Properties.Add(new PSNoteProperty("UniqueName", uniqueNameValue));
                    }
                    if (appModule.Attributes.TryGetValue("name", out var nameValue))
                    {
                        psObject.Properties.Add(new PSNoteProperty("Name", nameValue));
                    }
                    if (appModule.Attributes.TryGetValue("description", out var descriptionValue))
                    {
                        psObject.Properties.Add(new PSNoteProperty("Description", descriptionValue));
                    }
                    if (appModule.Attributes.TryGetValue("publishedon", out var publishedOnValue))
                    {
                        psObject.Properties.Add(new PSNoteProperty("PublishedOn", publishedOnValue));
                    }
                    if (appModule.Attributes.TryGetValue("url", out var urlValue))
                    {
                        psObject.Properties.Add(new PSNoteProperty("Url", urlValue));
                    }
                    if (appModule.Attributes.TryGetValue("webresourceid", out var webResourceIdValue))
                    {
                        psObject.Properties.Add(new PSNoteProperty("WebResourceId", webResourceIdValue));
                    }
                    if (appModule.Attributes.TryGetValue("formfactor", out var formFactorValue))
                    {
                        psObject.Properties.Add(new PSNoteProperty("FormFactor", formFactorValue));
                    }
                    if (appModule.Attributes.TryGetValue("clienttype", out var clientTypeValue))
                    {
                        psObject.Properties.Add(new PSNoteProperty("ClientType", clientTypeValue));
                    }
                }

                WriteObject(psObject);
            }
        }
    }
}
