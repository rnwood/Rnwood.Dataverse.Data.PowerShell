using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Query;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Management.Automation;

namespace Rnwood.Dataverse.Data.PowerShell.Commands
{
    /// <summary>
    /// Retrieves SDK message processing step records (plugin steps) from a Dataverse environment.
    /// </summary>
    [Cmdlet(VerbsCommon.Get, "DataversePluginStep", DefaultParameterSetName = "All")]
    [OutputType(typeof(PSObject))]
    public class GetDataversePluginStepCmdlet : OrganizationServiceCmdlet
    {
        /// <summary>
        /// Gets or sets the ID of the plugin step to retrieve.
        /// </summary>
        [Parameter(ParameterSetName = "ById", Mandatory = true, ValueFromPipelineByPropertyName = true, HelpMessage = "ID of the plugin step to retrieve")]
        public Guid Id { get; set; }

        /// <summary>
        /// Gets or sets the name of the plugin step to retrieve.
        /// </summary>
        [Parameter(ParameterSetName = "ByName", Mandatory = true, HelpMessage = "Name of the plugin step to retrieve")]
        public string Name { get; set; }

        /// <summary>
        /// Gets or sets the plugin type ID to filter steps by.
        /// </summary>
        [Parameter(ParameterSetName = "ByPluginType", Mandatory = true, HelpMessage = "Plugin type ID to filter steps by")]
        [Parameter(ParameterSetName = "All", HelpMessage = "Plugin type ID to filter steps by")]
        public Guid? PluginTypeId { get; set; }

        /// <summary>
        /// Process the cmdlet.
        /// </summary>
        protected override void ProcessRecord()
        {
            base.ProcessRecord();

            QueryExpression query = new QueryExpression("sdkmessageprocessingstep")
            {
                ColumnSet = new ColumnSet(true)
            };

            if (ParameterSetName == "ById")
            {
                query.Criteria.AddCondition("sdkmessageprocessingstepid", ConditionOperator.Equal, Id);
            }
            else if (ParameterSetName == "ByName")
            {
                query.Criteria.AddCondition("name", ConditionOperator.Equal, Name);
            }

            if (PluginTypeId.HasValue)
            {
                query.Criteria.AddCondition("plugintypeid", ConditionOperator.Equal, PluginTypeId.Value);
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
