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
            request.EntityMoniker = DataverseTypeConverter.ToEntityReference(Target, TableName, "Target");

            // Convert State to OptionSetValue
            request.State = DataverseTypeConverter.ToOptionSetValue(State, "State");

            // Convert Status to OptionSetValue
            request.Status = DataverseTypeConverter.ToOptionSetValue(Status, "Status");

            if (ShouldProcess($"Record {request.EntityMoniker.LogicalName} {request.EntityMoniker.Id}", $"Set state to {request.State.Value} and status to {request.Status.Value}"))
            {
                SetStateResponse response = (SetStateResponse)Connection.Execute(request);
                WriteObject(response);
            }
        }
    }
}
