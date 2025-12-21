using Microsoft.PowerPlatform.Dataverse.Client;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Query;
using System;
using System.Management.Automation;

namespace Rnwood.Dataverse.Data.PowerShell.Commands
{
    /// <summary>
    /// Retrieves Canvas apps from a Dataverse environment.
    /// </summary>
    [Cmdlet(VerbsCommon.Get, "DataverseCanvasApp")]
    [OutputType(typeof(PSObject))]
    public class GetDataverseCanvasAppCmdlet : OrganizationServiceCmdlet
    {
        private const string PARAMSET_QUERY = "Query";
        private const string PARAMSET_ID = "Id";

        /// <summary>
        /// Gets or sets the ID of the Canvas app to retrieve.
        /// </summary>
        [Parameter(ParameterSetName = PARAMSET_ID, Mandatory = true, HelpMessage = "ID of the Canvas app to retrieve")]
        public Guid Id { get; set; }

        /// <summary>
        /// Gets or sets the name or name pattern of the Canvas app to retrieve. Supports wildcards (* and ?).
        /// </summary>
        [Parameter(ParameterSetName = PARAMSET_QUERY, Position = 0, HelpMessage = "Name or name pattern of the Canvas app. Supports wildcards (* and ?)")]
        public string Name { get; set; }

        /// <summary>
        /// Gets or sets the display name or pattern to filter by. Supports wildcards (* and ?).
        /// </summary>
        [Parameter(ParameterSetName = PARAMSET_QUERY, HelpMessage = "Display name or pattern to filter by. Supports wildcards (* and ?)")]
        public string DisplayName { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether to filter to only unmanaged Canvas apps.
        /// </summary>
        [Parameter(ParameterSetName = PARAMSET_QUERY, HelpMessage = "If set, filters to only unmanaged Canvas apps")]
        public SwitchParameter Unmanaged { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether to include the document content (.msapp file) in the results.
        /// </summary>
        [Parameter(HelpMessage = "If set, includes the document content (.msapp file bytes) in the results")]
        public SwitchParameter IncludeDocument { get; set; }

        /// <inheritdoc />
        protected override void ProcessRecord()
        {
            base.ProcessRecord();

            QueryExpression query;

            if (ParameterSetName == PARAMSET_ID)
            {
                query = new QueryExpression("canvasapp")
                {
                    ColumnSet = new ColumnSet(true),
                    Criteria = new FilterExpression()
                };
                query.Criteria.AddCondition("canvasappid", ConditionOperator.Equal, Id);
            }
            else
            {
                query = new QueryExpression("canvasapp");

                // Include all columns or exclude document by default
                if (IncludeDocument)
                {
                    query.ColumnSet = new ColumnSet(true);
                }
                else
                {
                    // Get all columns except document-related ones
                    query.ColumnSet = new ColumnSet(
                        "canvasappid",
                        "name",
                        "displayname",
                        "description",
                        "commitMessage",
                        "publisher",
                        "authorizationreferences",
                        "connectionreferences",
                        "databasereferences",
                        "appcomponents",
                        "appcomponentdependencies",
                        "status",
                        "tags",
                        "createdon",
                        "modifiedon",
                        "iscustomizable",
                        "canvasapptype",
                        "bypassconsent",
                        "admincontrolbypassconsent",
                        "canconsumeapppass",
                        "iscdsupdated",
                        "isfeaturedapp",
                        "isheroapp",
                        "isteamsfirstparty",
                        "cdsdependencies",
                        "minclientversion",
                        "createdbyClientversion",
                        "appversion"
                    );
                }

                query.Criteria = new FilterExpression();

                // Add filters based on parameters
                if (!string.IsNullOrEmpty(Name))
                {
                    if (Name.Contains("*") || Name.Contains("?"))
                    {
                        string likePattern = Name.Replace("*", "%").Replace("?", "_");
                        query.Criteria.AddCondition("name", ConditionOperator.Like, likePattern);
                        WriteVerbose($"Filtering by name pattern: {Name}");
                    }
                    else
                    {
                        query.Criteria.AddCondition("name", ConditionOperator.Equal, Name);
                        WriteVerbose($"Filtering by name: {Name}");
                    }
                }

                if (!string.IsNullOrEmpty(DisplayName))
                {
                    if (DisplayName.Contains("*") || DisplayName.Contains("?"))
                    {
                        string likePattern = DisplayName.Replace("*", "%").Replace("?", "_");
                        query.Criteria.AddCondition("displayname", ConditionOperator.Like, likePattern);
                        WriteVerbose($"Filtering by display name pattern: {DisplayName}");
                    }
                    else
                    {
                        query.Criteria.AddCondition("displayname", ConditionOperator.Equal, DisplayName);
                        WriteVerbose($"Filtering by display name: {DisplayName}");
                    }
                }

                if (Unmanaged.IsPresent)
                {
                    query.Criteria.AddCondition("ismanaged", ConditionOperator.Equal, false);
                    WriteVerbose("Filtering for unmanaged Canvas apps only");
                }
            }

            WriteVerbose("Retrieving Canvas apps...");
            var results = Connection.RetrieveMultiple(query);
            WriteVerbose($"Found {results.Entities.Count} Canvas app(s)");

            // Convert to PSObjects
            var metadataFactory = new EntityMetadataFactory(Connection);
            var converter = new DataverseEntityConverter(Connection, metadataFactory);

            foreach (var entity in results.Entities)
            {
                var psObject = converter.ConvertToPSObject(entity, query.ColumnSet, _ => ValueType.Display);
                WriteObject(psObject);
            }
        }
    }
}
