using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Query;
using System;
using System.Linq;
using System.Management.Automation;

namespace Rnwood.Dataverse.Data.PowerShell.Commands
{
    /// <summary>
    /// Retrieves Copilot Studio bot components (topics, skills, actions) from Dataverse.
    /// </summary>
    [Cmdlet(VerbsCommon.Get, "DataverseBotComponent", DefaultParameterSetName = "All")]
    [OutputType(typeof(PSObject))]
    public class GetDataverseBotComponentCmdlet : OrganizationServiceCmdlet
    {
        /// <summary>
        /// Gets or sets the bot component ID to retrieve.
        /// </summary>
        [Parameter(ParameterSetName = "ById", Mandatory = true, ValueFromPipelineByPropertyName = true, HelpMessage = "Bot component ID (GUID) to retrieve.")]
        public Guid? BotComponentId { get; set; }

        /// <summary>
        /// Gets or sets the bot component name to filter by.
        /// </summary>
        [Parameter(ParameterSetName = "ByName", HelpMessage = "Bot component name to filter by (exact match).")]
        public string Name { get; set; }

        /// <summary>
        /// Gets or sets the bot component schema name to filter by.
        /// </summary>
        [Parameter(ParameterSetName = "BySchemaName", HelpMessage = "Bot component schema name to filter by (exact match).")]
        public string SchemaName { get; set; }

        /// <summary>
        /// Gets or sets the parent bot ID to filter by.
        /// </summary>
        [Parameter(HelpMessage = "Parent bot ID to filter components by.")]
        public Guid? ParentBotId { get; set; }

        /// <summary>
        /// Gets or sets the component type to filter by.
        /// </summary>
        [Parameter(HelpMessage = "Component type to filter by (10=Topic, 11=Skill, etc.).")]
        public int? ComponentType { get; set; }

        /// <summary>
        /// Gets or sets the category to filter by.
        /// </summary>
        [Parameter(HelpMessage = "Category to filter by.")]
        public string Category { get; set; }

        /// <summary>
        /// Gets or sets the maximum number of records to return.
        /// </summary>
        [Parameter(HelpMessage = "Maximum number of bot components to return. Default is all.")]
        public int? Top { get; set; }

        /// <summary>
        /// Processes the cmdlet.
        /// </summary>
        protected override void ProcessRecord()
        {
            base.ProcessRecord();

            QueryExpression query = new QueryExpression("botcomponent")
            {
                ColumnSet = new ColumnSet(true)
            };

            if (BotComponentId.HasValue)
            {
                query.Criteria.AddCondition("botcomponentid", ConditionOperator.Equal, BotComponentId.Value);
            }
            
            if (!string.IsNullOrEmpty(Name))
            {
                query.Criteria.AddCondition("name", ConditionOperator.Equal, Name);
            }
            
            if (!string.IsNullOrEmpty(SchemaName))
            {
                query.Criteria.AddCondition("schemaname", ConditionOperator.Equal, SchemaName);
            }
            
            if (ParentBotId.HasValue)
            {
                query.Criteria.AddCondition("parentbotid", ConditionOperator.Equal, ParentBotId.Value);
            }
            
            if (ComponentType.HasValue)
            {
                query.Criteria.AddCondition("componenttype", ConditionOperator.Equal, ComponentType.Value);
            }
            
            if (!string.IsNullOrEmpty(Category))
            {
                query.Criteria.AddCondition("category", ConditionOperator.Equal, Category);
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
