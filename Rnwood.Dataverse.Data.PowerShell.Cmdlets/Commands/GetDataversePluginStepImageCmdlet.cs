using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Query;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Management.Automation;

namespace Rnwood.Dataverse.Data.PowerShell.Commands
{
    /// <summary>
    /// Retrieves SDK message processing step image records (plugin step images) from a Dataverse environment.
    /// </summary>
    [Cmdlet(VerbsCommon.Get, "DataversePluginStepImage", DefaultParameterSetName = "All")]
    [OutputType(typeof(PSObject))]
    public class GetDataversePluginStepImageCmdlet : OrganizationServiceCmdlet
    {
        /// <summary>
        /// Gets or sets the ID of the plugin step image to retrieve.
        /// </summary>
        [Parameter(ParameterSetName = "ById", Mandatory = true, ValueFromPipelineByPropertyName = true, HelpMessage = "ID of the plugin step image to retrieve")]
        public Guid Id { get; set; }

        /// <summary>
        /// Gets or sets the entity alias of the plugin step image to retrieve.
        /// </summary>
        [Parameter(ParameterSetName = "ByAlias", Mandatory = true, HelpMessage = "Entity alias of the plugin step image to retrieve")]
        public string EntityAlias { get; set; }

        /// <summary>
        /// Gets or sets the plugin step ID to filter images by.
        /// </summary>
        [Parameter(ParameterSetName = "ByStep", Mandatory = true, HelpMessage = "Plugin step ID to filter images by")]
        [Parameter(ParameterSetName = "All", HelpMessage = "Plugin step ID to filter images by")]
        public Guid? SdkMessageProcessingStepId { get; set; }

        /// <summary>
        /// Process the cmdlet.
        /// </summary>
        protected override void ProcessRecord()
        {
            base.ProcessRecord();

            QueryExpression query = new QueryExpression("sdkmessageprocessingstepimage")
            {
                ColumnSet = new ColumnSet(true)
            };

            if (ParameterSetName == "ById")
            {
                query.Criteria.AddCondition("sdkmessageprocessingstepimageid", ConditionOperator.Equal, Id);
            }
            else if (ParameterSetName == "ByAlias")
            {
                query.Criteria.AddCondition("entityalias", ConditionOperator.Equal, EntityAlias);
            }

            if (SdkMessageProcessingStepId.HasValue)
            {
                query.Criteria.AddCondition("sdkmessageprocessingstepid", ConditionOperator.Equal, SdkMessageProcessingStepId.Value);
            }

            EntityMetadataFactory entityMetadataFactory = new EntityMetadataFactory(Connection);
            DataverseEntityConverter converter = new DataverseEntityConverter(Connection, entityMetadataFactory);

            foreach (Entity entity in QueryHelpers.ExecuteQueryWithPaging(query, Connection, WriteVerbose))
            {
                PSObject psObject = converter.ConvertToPSObject(entity, new ColumnSet(true), _ => ValueType.Display);
                WriteObject(psObject);
            }
        }
    }
}
