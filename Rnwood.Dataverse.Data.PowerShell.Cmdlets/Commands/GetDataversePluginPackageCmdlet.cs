using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Query;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Management.Automation;

namespace Rnwood.Dataverse.Data.PowerShell.Commands
{
    /// <summary>
    /// Retrieves plugin package records from a Dataverse environment.
    /// </summary>
    [Cmdlet(VerbsCommon.Get, "DataversePluginPackage")]
    [OutputType(typeof(PSObject))]
    public class GetDataversePluginPackageCmdlet : OrganizationServiceCmdlet
    {
        /// <summary>
        /// Gets or sets the ID of the plugin package to retrieve.
        /// </summary>
        [Parameter(ParameterSetName = "ById", Mandatory = true, ValueFromPipelineByPropertyName = true, HelpMessage = "ID of the plugin package to retrieve")]
        public Guid Id { get; set; }

        /// <summary>
        /// Gets or sets the unique name of the plugin package to retrieve.
        /// </summary>
        [Parameter(ParameterSetName = "ByName", Mandatory = true, HelpMessage = "Unique name of the plugin package to retrieve")]
        public string UniqueName { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether to return all plugin packages.
        /// </summary>
        [Parameter(ParameterSetName = "All", HelpMessage = "Return all plugin packages")]
        public SwitchParameter All { get; set; }

        /// <summary>
        /// Process the cmdlet.
        /// </summary>
        protected override void ProcessRecord()
        {
            base.ProcessRecord();

            QueryExpression query = new QueryExpression("pluginpackage")
            {
                ColumnSet = new ColumnSet(true)
            };

            if (ParameterSetName == "ById")
            {
                query.Criteria.AddCondition("pluginpackageid", ConditionOperator.Equal, Id);
            }
            else if (ParameterSetName == "ByName")
            {
                query.Criteria.AddCondition("uniquename", ConditionOperator.Equal, UniqueName);
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
