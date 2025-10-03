using System;
using System.Management.Automation;
using Microsoft.Crm.Sdk.Messages;
using Microsoft.PowerPlatform.Dataverse.Client;
using Microsoft.Xrm.Sdk;

namespace Rnwood.Dataverse.Data.PowerShell.Commands
{
    [Cmdlet(VerbsCommon.Set, "DataverseRecordState", SupportsShouldProcess = true, ConfirmImpact = ConfirmImpact.Medium)]
    [OutputType(typeof(SetStateResponse))]
    ///<summary>Changes the state and status of a record.</summary>
    public class SetDataverseRecordStateCmdlet : OrganizationServiceCmdlet
    {
        [Parameter(Mandatory = true, HelpMessage = "DataverseConnection instance obtained from Get-DataverseConnection cmdlet")]
        public override ServiceClient Connection { get; set; }

        [Parameter(Mandatory = true, ValueFromPipelineByPropertyName = true, HelpMessage = "Reference to the record to update. Can be an EntityReference, a PSObject with Id and TableName properties, or a Guid (requires TableName parameter).")]
        public object Target { get; set; }

        [Parameter(Mandatory = false, ValueFromPipelineByPropertyName = true, HelpMessage = "Logical name of the table when Target is specified as a Guid")]
        [Alias("EntityName", "LogicalName")]
        public string TableName { get; set; }

        [Parameter(Mandatory = true, HelpMessage = "State code value to set. Can be an integer or an OptionSetValue.")]
        public object State { get; set; }

        [Parameter(Mandatory = true, HelpMessage = "Status code value to set. Can be an integer or an OptionSetValue.")]
        public object Status { get; set; }

        protected override void ProcessRecord()
        {
            base.ProcessRecord();

            SetStateRequest request = new SetStateRequest();

            // Convert Target to EntityReference
            request.EntityMoniker = ConvertToEntityReference(Target, TableName);

            // Convert State to OptionSetValue
            request.State = ConvertToOptionSetValue(State, "State");

            // Convert Status to OptionSetValue
            request.Status = ConvertToOptionSetValue(Status, "Status");

            if (ShouldProcess($"Record {request.EntityMoniker.LogicalName} {request.EntityMoniker.Id}", $"Set state to {request.State.Value} and status to {request.Status.Value}"))
            {
                SetStateResponse response = (SetStateResponse)Connection.Execute(request);
                WriteObject(response);
            }
        }

        private EntityReference ConvertToEntityReference(object value, string tableName)
        {
            if (value is EntityReference entityRef)
            {
                return entityRef;
            }
            else if (value is PSObject psObj)
            {
                var idProp = psObj.Properties["Id"];
                var tableNameProp = psObj.Properties["TableName"] ?? psObj.Properties["LogicalName"];

                if (idProp != null && tableNameProp != null)
                {
                    return new EntityReference((string)tableNameProp.Value, (Guid)idProp.Value);
                }
            }
            else if (value is Guid guid)
            {
                if (string.IsNullOrEmpty(tableName))
                {
                    throw new ArgumentException("TableName parameter is required when Target is specified as a Guid");
                }
                return new EntityReference(tableName, guid);
            }
            else if (value is string strValue && Guid.TryParse(strValue, out Guid parsedGuid))
            {
                if (string.IsNullOrEmpty(tableName))
                {
                    throw new ArgumentException("TableName parameter is required when Target is specified as a string Guid");
                }
                return new EntityReference(tableName, parsedGuid);
            }

            throw new ArgumentException("Unable to convert Target to EntityReference. Expected EntityReference, PSObject with Id and TableName properties, or Guid with TableName parameter.");
        }

        private OptionSetValue ConvertToOptionSetValue(object value, string parameterName)
        {
            if (value is OptionSetValue osv)
            {
                return osv;
            }
            else if (value is int intValue)
            {
                return new OptionSetValue(intValue);
            }
            else if (value is string strValue && int.TryParse(strValue, out int parsedInt))
            {
                return new OptionSetValue(parsedInt);
            }

            throw new ArgumentException($"Unable to convert {parameterName} to OptionSetValue. Expected OptionSetValue or integer value.");
        }
    }
}
