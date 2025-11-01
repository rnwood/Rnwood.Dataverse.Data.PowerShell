using System;
using System.Collections.Generic;
using System.Linq;
using System.Management.Automation;
using Microsoft.PowerPlatform.Dataverse.Client;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Query;

namespace Rnwood.Dataverse.Data.PowerShell.Commands
{
    /// <summary>
    /// Gets connection references from Dataverse.
    /// </summary>
    [Cmdlet(VerbsCommon.Get, "DataverseConnectionReference")]
    [OutputType(typeof(PSObject))]
    public class GetDataverseConnectionReferenceCmdlet : OrganizationServiceCmdlet
    {
        /// <summary>
        /// Gets or sets the logical name of the connection reference to retrieve.
        /// </summary>
        [Parameter(Position = 0, ValueFromPipeline = true, ValueFromPipelineByPropertyName = true, HelpMessage = "Logical name of the connection reference to retrieve.")]
        public string ConnectionReferenceLogicalName { get; set; }

        /// <summary>
        /// Gets or sets the display name filter for connection references.
        /// </summary>
        [Parameter(HelpMessage = "Display name filter for connection references (supports wildcards).")]
        public string DisplayName { get; set; }

        /// <summary>
        /// Gets or sets the connector ID filter for connection references.
        /// </summary>
        [Parameter(HelpMessage = "Connector ID filter for connection references (supports wildcards).")]
        public string ConnectorId { get; set; }

        /// <summary>
        /// Processes the cmdlet request.
        /// </summary>
        protected override void ProcessRecord()
        {
            base.ProcessRecord();

            // Build query for connection references
            var query = new QueryExpression("connectionreference")
            {
                ColumnSet = new ColumnSet("connectionreferenceid", "connectionreferencelogicalname", "connectionreferencedisplayname", "connectionid", "connectorid", "description"),
                PageInfo = new PagingInfo { PageNumber = 1, Count = 5000 }
            };

            var filter = new FilterExpression();

            if (!string.IsNullOrEmpty(ConnectionReferenceLogicalName))
            {
                if (ConnectionReferenceLogicalName.Contains("*") || ConnectionReferenceLogicalName.Contains("?"))
                {
                    filter.AddCondition("connectionreferencelogicalname", ConditionOperator.Like, ConnectionReferenceLogicalName.Replace("*", "%").Replace("?", "_"));
                }
                else
                {
                    filter.AddCondition("connectionreferencelogicalname", ConditionOperator.Equal, ConnectionReferenceLogicalName);
                }
            }

            if (!string.IsNullOrEmpty(DisplayName))
            {
                if (DisplayName.Contains("*") || DisplayName.Contains("?"))
                {
                    filter.AddCondition("connectionreferencedisplayname", ConditionOperator.Like, DisplayName.Replace("*", "%").Replace("?", "_"));
                }
                else
                {
                    filter.AddCondition("connectionreferencedisplayname", ConditionOperator.Equal, DisplayName);
                }
            }

            if (!string.IsNullOrEmpty(ConnectorId))
            {
                if (ConnectorId.Contains("*") || ConnectorId.Contains("?"))
                {
                    filter.AddCondition("connectorid", ConditionOperator.Like, ConnectorId.Replace("*", "%").Replace("?", "_"));
                }
                else
                {
                    filter.AddCondition("connectorid", ConditionOperator.Equal, ConnectorId);
                }
            }

            if (filter.Conditions.Count > 0)
            {
                query.Criteria = filter;
            }

            WriteVerbose("Querying for connection references...");

            var allResults = new List<Entity>();
            EntityCollection ec;
            do
            {
                ec = Connection.RetrieveMultiple(query);
                allResults.AddRange(ec.Entities);
                if (ec.MoreRecords)
                {
                    query.PageInfo.PageNumber++;
                    query.PageInfo.PagingCookie = ec.PagingCookie;
                }
            } while (ec.MoreRecords);

            WriteVerbose($"Found {allResults.Count} connection reference(s)");

            // Output results
            foreach (var connRef in allResults)
            {
                var result = new PSObject();
                result.Properties.Add(new PSNoteProperty("ConnectionReferenceId", connRef.Id));
                result.Properties.Add(new PSNoteProperty("ConnectionReferenceLogicalName", connRef.GetAttributeValue<string>("connectionreferencelogicalname")));
                result.Properties.Add(new PSNoteProperty("ConnectionReferenceDisplayName", connRef.GetAttributeValue<string>("connectionreferencedisplayname")));
                result.Properties.Add(new PSNoteProperty("ConnectionId", connRef.GetAttributeValue<string>("connectionid")));
                result.Properties.Add(new PSNoteProperty("ConnectorId", connRef.GetAttributeValue<string>("connectorid")));
                result.Properties.Add(new PSNoteProperty("Description", connRef.GetAttributeValue<string>("description")));

                WriteObject(result);
            }
        }
    }
}
