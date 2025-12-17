using Microsoft.Crm.Sdk.Messages;
using Microsoft.PowerPlatform.Dataverse.Client;
using Microsoft.Xrm.Sdk;
using System;
using System.Management.Automation;
using System.ServiceModel;

namespace Rnwood.Dataverse.Data.PowerShell.Commands
{
    /// <summary>
    /// Deletes file data from a Dataverse file column.
    /// </summary>
    [Cmdlet(VerbsCommon.Remove, "DataverseFileData", SupportsShouldProcess = true, ConfirmImpact = ConfirmImpact.High)]
    [OutputType(typeof(void))]
    public class RemoveDataverseFileDataCmdlet : OrganizationServiceCmdlet
    {
        /// <summary>
        /// Gets or sets the logical name of the table containing the file column.
        /// </summary>
        [Parameter(Mandatory = true, ValueFromPipelineByPropertyName = true, HelpMessage = "Logical name of table containing the file column")]
        [ArgumentCompleter(typeof(TableNameArgumentCompleter))]
        [Alias("EntityName", "LogicalName")]
        public string TableName { get; set; }

        /// <summary>
        /// Gets or sets the ID of the record containing the file to delete.
        /// </summary>
        [Parameter(Mandatory = true, ValueFromPipelineByPropertyName = true, HelpMessage = "ID of the record containing the file to delete")]
        public Guid Id { get; set; }

        /// <summary>
        /// Gets or sets the logical name of the file column.
        /// </summary>
        [Parameter(Mandatory = true, ValueFromPipelineByPropertyName = true, HelpMessage = "Logical name of the file column")]
        [ArgumentCompleter(typeof(ColumnNameArgumentCompleter))]
        public string ColumnName { get; set; }

        /// <summary>
        /// Gets or sets whether to suppress errors if the file does not exist.
        /// </summary>
        [Parameter(HelpMessage = "If specified, the cmdlet will not raise an error if the file does not exist")]
        public SwitchParameter IfExists { get; set; }

        /// <summary>
        /// Processes each record in the pipeline to delete file data.
        /// </summary>
        protected override void ProcessRecord()
        {
            base.ProcessRecord();

            if (!ShouldProcess($"{TableName} record {Id}, column {ColumnName}", "Delete file"))
            {
                return;
            }

            try
            {
                // Retrieve the file ID from the file column
                var entity = Connection.Retrieve(TableName, Id, new Microsoft.Xrm.Sdk.Query.ColumnSet(ColumnName));
                
                if (!entity.Contains(ColumnName) || entity[ColumnName] == null)
                {
                    if (IfExists)
                    {
                        WriteVerbose($"File column '{ColumnName}' is empty or does not exist on record {Id} in table '{TableName}'");
                        return;
                    }
                    else
                    {
                        throw new InvalidOperationException($"File column '{ColumnName}' is empty or does not exist on record {Id} in table '{TableName}'");
                    }
                }

                // Get the file ID from the column value
                Guid fileId;
                var columnValue = entity[ColumnName];
                
                if (columnValue is Guid guid)
                {
                    fileId = guid;
                }
                else if (columnValue is EntityReference entityRef)
                {
                    fileId = entityRef.Id;
                }
                else
                {
                    throw new InvalidOperationException($"Unexpected value type in file column '{ColumnName}': {columnValue.GetType().Name}");
                }

                var deleteRequest = new DeleteFileRequest
                {
                    FileId = fileId
                };

                Connection.Execute(deleteRequest);
                WriteVerbose($"Successfully deleted file from {TableName} record {Id}, column {ColumnName}");
            }
            catch (FaultException<OrganizationServiceFault> ex)
            {
                // Check if the error is because the file doesn't exist
                if (IfExists && (ex.Message.Contains("does not exist") || ex.Message.Contains("not found")))
                {
                    WriteVerbose($"File does not exist in {TableName} record {Id}, column {ColumnName}");
                    return;
                }

                WriteError(new ErrorRecord(ex, "FileDeleteError", ErrorCategory.WriteError, Id));
            }
            catch (Exception ex)
            {
                WriteError(new ErrorRecord(ex, "FileDeleteError", ErrorCategory.WriteError, Id));
            }
        }
    }
}
