using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Query;
using System;
using System.Linq;
using System.Management.Automation;
using System.ServiceModel;

namespace Rnwood.Dataverse.Data.PowerShell.Commands
{
    /// <summary>
    /// Removes an app module (model-driven app) from Dataverse.
    /// </summary>
    [Cmdlet(VerbsCommon.Remove, "DataverseAppModule", SupportsShouldProcess = true, ConfirmImpact = ConfirmImpact.Medium)]
    public class RemoveDataverseAppModuleCmdlet : OrganizationServiceCmdlet
    {
        /// <summary>
        /// Gets or sets the ID of the app module to remove.
        /// </summary>
        [Parameter(Mandatory = true, ParameterSetName = "ById", ValueFromPipelineByPropertyName = true, HelpMessage = "ID of the app module to remove")]
        public Guid Id { get; set; }

        /// <summary>
        /// Gets or sets the unique name of the app module to remove.
        /// </summary>
        [Parameter(Mandatory = true, ParameterSetName = "ByUniqueName", ValueFromPipelineByPropertyName = true, HelpMessage = "Unique name of the app module to remove")]
        public string UniqueName { get; set; }

        /// <summary>
        /// If specified, the cmdlet will not raise an error if the app module does not exist.
        /// </summary>
        [Parameter(HelpMessage = "If specified, the cmdlet will not raise an error if the app module does not exist")]
        public SwitchParameter IfExists { get; set; }

        /// <summary>
        /// Processes each record in the pipeline.
        /// </summary>
        protected override void ProcessRecord()
        {
            try
            {
                base.ProcessRecord();

                Guid appModuleId = Id;

                // If UniqueName is provided, resolve it to an ID
                if (ParameterSetName == "ByUniqueName")
                {
                    var query = new QueryExpression("appmodule")
                    {
                        ColumnSet = new ColumnSet("appmoduleid"),
                        Criteria = new FilterExpression()
                    };
                    query.Criteria.AddCondition("uniquename", ConditionOperator.Equal, UniqueName);

                    var results = Connection.RetrieveMultiple(query);
                    if (results.Entities.Count == 0)
                    {
                        if (IfExists)
                        {
                            WriteVerbose($"App module with UniqueName {UniqueName} does not exist");
                            return;
                        }
                        else
                        {
                            throw new InvalidOperationException($"App module with UniqueName '{UniqueName}' not found");
                        }
                    }
                    appModuleId = results.Entities[0].Id;
                    WriteVerbose($"Resolved UniqueName '{UniqueName}' to ID: {appModuleId}");
                }

                if (ShouldProcess($"App module with ID '{appModuleId}'", "Remove"))
                {
                    Connection.Delete("appmodule", appModuleId);
                    WriteVerbose($"Removed app module with ID: {appModuleId}");
                }
            }
            catch (FaultException<OrganizationServiceFault> ex)
            {
                if (IfExists && (ex.Detail.ErrorCode == -2147220969 || ex.Message.Contains("Does Not Exist")))
                {
                    WriteVerbose($"App module does not exist: {ex.Message}");
                    return;
                }
                else
                {
                    throw;
                }
            }
            catch (FaultException ex)
            {
                if (IfExists && ex.Message.Contains("Does Not Exist"))
                {
                    WriteVerbose($"App module does not exist: {ex.Message}");
                    return;
                }
                else
                {
                    throw;
                }
            }
        }
    }
}
