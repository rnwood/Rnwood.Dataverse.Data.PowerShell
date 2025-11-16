using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Query;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Management.Automation;

namespace Rnwood.Dataverse.Data.PowerShell.Commands
{
    /// <summary>
    /// Retrieves plugin type records from a Dataverse environment.
    /// </summary>
    [Cmdlet(VerbsCommon.Get, "DataversePluginType", DefaultParameterSetName = "All")]
    [OutputType(typeof(PSObject))]
    public class GetDataversePluginTypeCmdlet : OrganizationServiceCmdlet
    {
        /// <summary>
        /// Gets or sets the ID of the plugin type to retrieve.
        /// </summary>
        [Parameter(ParameterSetName = "ById", Mandatory = true, ValueFromPipelineByPropertyName = true, HelpMessage = "ID of the plugin type to retrieve")]
        public Guid Id { get; set; }

        /// <summary>
        /// Gets or sets the type name of the plugin type to retrieve.
        /// </summary>
        [Parameter(ParameterSetName = "ByName", Mandatory = true, HelpMessage = "Type name of the plugin type to retrieve")]
        public string TypeName { get; set; }

        /// <summary>
        /// Gets or sets the plugin assembly ID to filter types by.
        /// </summary>
        [Parameter(ParameterSetName = "ByAssembly", Mandatory = true, HelpMessage = "Plugin assembly ID to filter types by")]
        [Parameter(ParameterSetName = "All", HelpMessage = "Plugin assembly ID to filter types by")]
        public Guid? PluginAssemblyId { get; set; }

        /// <summary>
        /// Process the cmdlet.
        /// </summary>
        protected override void ProcessRecord()
        {
            base.ProcessRecord();

            QueryExpression query = new QueryExpression("plugintype")
            {
                ColumnSet = new ColumnSet(true)
            };

            if (ParameterSetName == "ById")
            {
                query.Criteria.AddCondition("plugintypeid", ConditionOperator.Equal, Id);
            }
            else if (ParameterSetName == "ByName")
            {
                query.Criteria.AddCondition("typename", ConditionOperator.Equal, TypeName);
            }

            if (PluginAssemblyId.HasValue)
            {
                query.Criteria.AddCondition("pluginassemblyid", ConditionOperator.Equal, PluginAssemblyId.Value);
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
