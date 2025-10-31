using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Query;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Management.Automation;

namespace Rnwood.Dataverse.Data.PowerShell.Commands
{
    /// <summary>
    /// Retrieves plugin assembly records from a Dataverse environment.
    /// </summary>
    [Cmdlet(VerbsCommon.Get, "DataversePluginAssembly")]
    [OutputType(typeof(PSObject))]
    public class GetDataversePluginAssemblyCmdlet : OrganizationServiceCmdlet
    {
        /// <summary>
        /// Gets or sets the ID of the plugin assembly to retrieve.
        /// </summary>
        [Parameter(ParameterSetName = "ById", Mandatory = true, ValueFromPipelineByPropertyName = true, HelpMessage = "ID of the plugin assembly to retrieve")]
        public Guid Id { get; set; }

        /// <summary>
        /// Gets or sets the name of the plugin assembly to retrieve.
        /// </summary>
        [Parameter(ParameterSetName = "ByName", Mandatory = true, HelpMessage = "Name of the plugin assembly to retrieve")]
        public string Name { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether to return all plugin assemblies.
        /// </summary>
        [Parameter(ParameterSetName = "All", HelpMessage = "Return all plugin assemblies")]
        public SwitchParameter All { get; set; }

        /// <summary>
        /// Process the cmdlet.
        /// </summary>
        protected override void ProcessRecord()
        {
            base.ProcessRecord();

            QueryExpression query = new QueryExpression("pluginassembly")
            {
                ColumnSet = new ColumnSet(true)
            };

            if (ParameterSetName == "ById")
            {
                query.Criteria.AddCondition("pluginassemblyid", ConditionOperator.Equal, Id);
            }
            else if (ParameterSetName == "ByName")
            {
                query.Criteria.AddCondition("name", ConditionOperator.Equal, Name);
            }

            EntityCollection results = Connection.RetrieveMultiple(query);

            foreach (Entity entity in results.Entities)
            {
                EntityMetadataFactory entityMetadataFactory = new EntityMetadataFactory(Connection);
                DataverseEntityConverter converter = new DataverseEntityConverter(Connection, entityMetadataFactory);
                PSObject psObject = converter.ConvertToPSObject(entity, new ColumnSet(true), _ => ValueType.Display);
                WriteObject(psObject);
            }
        }
    }
}
