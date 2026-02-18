using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Query;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Management.Automation;

namespace Rnwood.Dataverse.Data.PowerShell.Commands
{
    /// <summary>
    /// Retrieves Copilot Studio bots from Dataverse.
    /// </summary>
    [Cmdlet(VerbsCommon.Get, "DataverseBot", DefaultParameterSetName = "All")]
    [OutputType(typeof(PSObject))]
    public class GetDataverseBotCmdlet : OrganizationServiceCmdlet
    {
        /// <summary>
        /// Gets or sets the bot ID to retrieve.
        /// </summary>
        [Parameter(ParameterSetName = "ById", Mandatory = true, ValueFromPipelineByPropertyName = true, HelpMessage = "Bot ID (GUID) to retrieve.")]
        public Guid? BotId { get; set; }

        /// <summary>
        /// Gets or sets the bot name to filter by.
        /// </summary>
        [Parameter(ParameterSetName = "ByName", Mandatory = true, HelpMessage = "Bot name to filter by (exact match).")]
        public string Name { get; set; }

        /// <summary>
        /// Gets or sets the bot schema name to filter by.
        /// </summary>
        [Parameter(ParameterSetName = "BySchemaName", Mandatory = true, HelpMessage = "Bot schema name to filter by (exact match).")]
        public string SchemaName { get; set; }

        /// <summary>
        /// Gets or sets the maximum number of records to return.
        /// </summary>
        [Parameter(HelpMessage = "Maximum number of bots to return. Default is all.")]
        public int? Top { get; set; }

        /// <summary>
        /// Processes the cmdlet.
        /// </summary>
        protected override void ProcessRecord()
        {
            base.ProcessRecord();

            QueryExpression query = new QueryExpression("bot")
            {
                ColumnSet = new ColumnSet(true)
            };

            if (BotId.HasValue)
            {
                query.Criteria.AddCondition("botid", ConditionOperator.Equal, BotId.Value);
            }
            else if (!string.IsNullOrEmpty(Name))
            {
                query.Criteria.AddCondition("name", ConditionOperator.Equal, Name);
            }
            else if (!string.IsNullOrEmpty(SchemaName))
            {
                query.Criteria.AddCondition("schemaname", ConditionOperator.Equal, SchemaName);
            }

            if (Top.HasValue)
            {
                query.TopCount = Top.Value;
            }

            EntityCollection results = Connection.RetrieveMultiple(query);

            var entityMetadataFactory = new EntityMetadataFactory(Connection);
            var converter = new DataverseEntityConverter(Connection, entityMetadataFactory);

            foreach (var entity in results.Entities)
            {
                var psObject = converter.ConvertToPSObject(entity, new ColumnSet(true), _ => ValueType.Raw);
                WriteObject(psObject);
            }
        }
    }
}
