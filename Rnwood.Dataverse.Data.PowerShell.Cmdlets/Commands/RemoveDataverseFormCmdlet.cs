using Microsoft.Crm.Sdk.Messages;
using Microsoft.PowerPlatform.Dataverse.Client;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Query;
using System;
using System.Management.Automation;

namespace Rnwood.Dataverse.Data.PowerShell.Commands
{
    /// <summary>
    /// Removes/deletes a form from a Dataverse environment.
    /// </summary>
    [Cmdlet(VerbsCommon.Remove, "DataverseForm", SupportsShouldProcess = true, ConfirmImpact = ConfirmImpact.High)]
    public class RemoveDataverseFormCmdlet : OrganizationServiceCmdlet
    {
        /// <summary>
        /// Gets or sets the form ID to delete.
        /// </summary>
        [Parameter(ParameterSetName = "ById", Mandatory = true, ValueFromPipeline = true, ValueFromPipelineByPropertyName = true, HelpMessage = "ID of the form to delete")]
        [Alias("formid")]
        public Guid Id { get; set; }

        /// <summary>
        /// Gets or sets the logical name of the entity/table.
        /// </summary>
        [Parameter(ParameterSetName = "ByName", Mandatory = true, HelpMessage = "Logical name of the entity/table")]
        [ArgumentCompleter(typeof(TableNameArgumentCompleter))]
        [Alias("EntityName", "TableName")]
        public string Entity { get; set; }

        /// <summary>
        /// Gets or sets the name of the form to delete.
        /// </summary>
        [Parameter(ParameterSetName = "ByName", Mandatory = true, HelpMessage = "Name of the form to delete")]
        public string Name { get; set; }

        /// <summary>
        /// Gets or sets whether to publish after deletion.
        /// </summary>
        [Parameter(HelpMessage = "Publish the entity after deleting the form")]
        public SwitchParameter Publish { get; set; }

        /// <summary>
        /// Gets or sets whether to suppress errors if the form doesn't exist.
        /// </summary>
        [Parameter(HelpMessage = "Don't raise an error if the form doesn't exist")]
        public SwitchParameter IfExists { get; set; }

        /// <summary>
        /// Processes the cmdlet request.
        /// </summary>
        protected override void ProcessRecord()
        {
            base.ProcessRecord();

            Guid formId;
            string entityName = null;

            if (ParameterSetName == "ByName")
            {
                // Look up the form by name
                QueryExpression query = new QueryExpression("systemform")
                {
                    ColumnSet = new ColumnSet("formid", "objecttypecode"),
                    Criteria = new FilterExpression
                    {
                        Conditions =
                        {
                            new ConditionExpression("objecttypecode", ConditionOperator.Equal, Entity),
                            new ConditionExpression("name", ConditionOperator.Equal, Name)
                        }
                    }
                };

                EntityCollection results = Connection.RetrieveMultiple(query);

                if (results.Entities.Count == 0)
                {
                    if (IfExists.IsPresent)
                    {
                        WriteVerbose($"Form '{Name}' for entity '{Entity}' does not exist");
                        return;
                    }
                    throw new InvalidOperationException($"Form '{Name}' not found for entity '{Entity}'");
                }

                if (results.Entities.Count > 1)
                {
                    throw new InvalidOperationException($"Multiple forms found with name '{Name}' for entity '{Entity}'");
                }

                formId = results.Entities[0].GetAttributeValue<Guid>("formid");
                entityName = results.Entities[0].GetAttributeValue<string>("objecttypecode");
            }
            else
            {
                formId = Id;
                
                // Get entity name for publishing if needed
                if (Publish.IsPresent)
                {
                    try
                    {
                        Entity form = Connection.Retrieve("systemform", formId, new ColumnSet("objecttypecode"));
                        entityName = form.GetAttributeValue<string>("objecttypecode");
                    }
                    catch (Exception)
                    {
                        if (IfExists.IsPresent)
                        {
                            WriteVerbose($"Form '{formId}' does not exist");
                            return;
                        }
                        throw;
                    }
                }
            }

            // Confirm deletion
            string target = ParameterSetName == "ByName" ? $"form '{Name}' for entity '{Entity}'" : $"form '{formId}'";
            
            if (!ShouldProcess(target, "Delete"))
            {
                return;
            }

            // Delete the form
            try
            {
                Connection.Delete("systemform", formId);
                WriteVerbose($"Deleted form '{formId}'");

                // Publish if requested
                if (Publish.IsPresent && !string.IsNullOrEmpty(entityName))
                {
                    WriteVerbose($"Publishing entity '{entityName}'...");
                    var publishRequest = new PublishXmlRequest
                    {
                        ParameterXml = $"<importexportxml><entities><entity>{entityName}</entity></entities></importexportxml>"
                    };
                    Connection.Execute(publishRequest);
                    WriteVerbose("Entity published successfully");
                }
            }
            catch (Exception ex)
            {
                if (IfExists.IsPresent && ex.Message.Contains("does not exist"))
                {
                    WriteVerbose($"Form '{formId}' does not exist");
                    return;
                }
                throw new InvalidOperationException($"Failed to delete form: {ex.Message}", ex);
            }
        }
    }
}
