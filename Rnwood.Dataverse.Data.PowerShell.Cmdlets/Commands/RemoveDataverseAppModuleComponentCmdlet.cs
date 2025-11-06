using Microsoft.Crm.Sdk.Messages;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Query;
using System;
using System.Management.Automation;
using System.ServiceModel;

namespace Rnwood.Dataverse.Data.PowerShell.Commands
{
    /// <summary>
    /// Removes an app module component from Dataverse.
    /// </summary>
    [Cmdlet(VerbsCommon.Remove, "DataverseAppModuleComponent", SupportsShouldProcess = true, ConfirmImpact = ConfirmImpact.Medium)]
    public class RemoveDataverseAppModuleComponentCmdlet : OrganizationServiceCmdlet
    {
        /// <summary>
        /// Gets or sets the ID of the app module component to remove.
        /// </summary>
        [Parameter(Mandatory = true, ValueFromPipelineByPropertyName = true, HelpMessage = "ID of the app module component to remove")]
        public Guid Id { get; set; }

        /// <summary>
        /// If specified, the cmdlet will not raise an error if the component does not exist.
        /// </summary>
        [Parameter(HelpMessage = "If specified, the cmdlet will not raise an error if the component does not exist")]
        public SwitchParameter IfExists { get; set; }

        /// <summary>
        /// Processes each record in the pipeline.
        /// </summary>
        protected override void ProcessRecord()
        {
            try
            {
                base.ProcessRecord();

                // Retrieve the component to get the AppModuleId and ComponentType
                Entity componentEntity = null;
                try
                {
                    componentEntity = Connection.Retrieve("appmodulecomponent", Id, new ColumnSet("appmoduleidunique", "componenttype"));
                }
                catch (FaultException<OrganizationServiceFault> ex)
                {
                    if (IfExists && (ex.Detail.ErrorCode == -2147220969 || ex.Message.Contains("Does Not Exist")))
                    {
                        WriteVerbose($"App module component with ID {Id} does not exist: {ex.Message}");
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
                        WriteVerbose($"App module component with ID {Id} does not exist: {ex.Message}");
                        return;
                    }
                    else
                    {
                        throw;
                    }
                }

                if (componentEntity != null)
                {
                    Guid appModuleId = componentEntity.GetAttributeValue<Guid>("appmoduleidunique");
                    int componentType = componentEntity.GetAttributeValue<int>("componenttype");

                    if (ShouldProcess($"App module component with ID '{Id}'", "Remove"))
                    {
                        var request = new RemoveAppComponentsRequest();
                        request.Parameters["AppModuleId"] = appModuleId;
                        request.Parameters["ComponentIds"] = new[] { Id };
                        request.Parameters["ComponentType"] = componentType;
                        
                        Connection.Execute(request);
                        WriteVerbose($"Removed app module component with ID: {Id}");
                    }
                }
            }
            catch (FaultException<OrganizationServiceFault> ex)
            {
                if (IfExists && (ex.Detail.ErrorCode == -2147220969 || ex.Message.Contains("Does Not Exist")))
                {
                    WriteVerbose($"App module component with ID {Id} does not exist: {ex.Message}");
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
                    WriteVerbose($"App module component with ID {Id} does not exist: {ex.Message}");
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
