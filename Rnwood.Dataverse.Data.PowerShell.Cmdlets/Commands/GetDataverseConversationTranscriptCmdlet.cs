using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Query;
using System;
using System.Management.Automation;

namespace Rnwood.Dataverse.Data.PowerShell.Commands
{
    /// <summary>
    /// Retrieves conversation transcripts from Dataverse.
    /// </summary>
    [Cmdlet(VerbsCommon.Get, "DataverseConversationTranscript", DefaultParameterSetName = "All")]
    [OutputType(typeof(PSObject))]
    public class GetDataverseConversationTranscriptCmdlet : OrganizationServiceCmdlet
    {
        /// <summary>
        /// Gets or sets the conversation transcript ID to retrieve.
        /// </summary>
        [Parameter(ParameterSetName = "ById", Mandatory = true, ValueFromPipelineByPropertyName = true, HelpMessage = "Conversation transcript ID (GUID) to retrieve.")]
        public Guid? ConversationTranscriptId { get; set; }

        /// <summary>
        /// Gets or sets the bot ID to filter by.
        /// </summary>
        [Parameter(HelpMessage = "Bot ID to filter transcripts by.")]
        public Guid? BotId { get; set; }

        /// <summary>
        /// Gets or sets the conversation ID to filter by.
        /// </summary>
        [Parameter(HelpMessage = "Conversation ID to filter by.")]
        public string ConversationId { get; set; }

        /// <summary>
        /// Gets or sets the start date to filter by.
        /// </summary>
        [Parameter(HelpMessage = "Filter conversations starting from this date.")]
        public DateTime? StartDate { get; set; }

        /// <summary>
        /// Gets or sets the end date to filter by.
        /// </summary>
        [Parameter(HelpMessage = "Filter conversations up to this date.")]
        public DateTime? EndDate { get; set; }

        /// <summary>
        /// Gets or sets the maximum number of records to return.
        /// </summary>
        [Parameter(HelpMessage = "Maximum number of transcripts to return. Default is all.")]
        public int? Top { get; set; }

        /// <summary>
        /// Processes the cmdlet.
        /// </summary>
        protected override void ProcessRecord()
        {
            base.ProcessRecord();

            QueryExpression query = new QueryExpression("conversationtranscript")
            {
                ColumnSet = new ColumnSet(true)
            };

            if (ConversationTranscriptId.HasValue)
            {
                query.Criteria.AddCondition("conversationtranscriptid", ConditionOperator.Equal, ConversationTranscriptId.Value);
            }

            if (BotId.HasValue)
            {
                query.Criteria.AddCondition("bot", ConditionOperator.Equal, BotId.Value);
            }

            if (!string.IsNullOrEmpty(ConversationId))
            {
                query.Criteria.AddCondition("conversationid", ConditionOperator.Equal, ConversationId);
            }

            if (StartDate.HasValue)
            {
                query.Criteria.AddCondition("createdon", ConditionOperator.GreaterEqual, StartDate.Value);
            }

            if (EndDate.HasValue)
            {
                query.Criteria.AddCondition("createdon", ConditionOperator.LessEqual, EndDate.Value);
            }

            if (Top.HasValue)
            {
                query.TopCount = Top.Value;
            }

            // Order by created date descending (most recent first)
            query.AddOrder("createdon", OrderType.Descending);

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
