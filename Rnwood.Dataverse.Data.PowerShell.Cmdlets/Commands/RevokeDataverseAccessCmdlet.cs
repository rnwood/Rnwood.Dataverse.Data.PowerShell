using System;
using System.Management.Automation;
using Microsoft.Crm.Sdk.Messages;
using Microsoft.PowerPlatform.Dataverse.Client;
using Microsoft.Xrm.Sdk;

namespace Rnwood.Dataverse.Data.PowerShell.Commands
{
    [Cmdlet(VerbsSecurity.Revoke, "DataverseAccess", SupportsShouldProcess = true, ConfirmImpact = ConfirmImpact.High)]
    [OutputType(typeof(RevokeAccessResponse))]
    ///<summary>Revokes access to a record for a user or team.</summary>
    public class RevokeDataverseAccessCmdlet : OrganizationServiceCmdlet
    {
        [Parameter(Mandatory = true, HelpMessage = "DataverseConnection instance obtained from Get-DataverseConnection cmdlet")]
        public override ServiceClient Connection { get; set; }

        [Parameter(Mandatory = true, ValueFromPipelineByPropertyName = true, HelpMessage = "Reference to the record to revoke access from. Can be an EntityReference, a PSObject with Id and TableName properties, or a Guid (requires TableName parameter).")]
        public object Target { get; set; }

        [Parameter(Mandatory = false, ValueFromPipelineByPropertyName = true, HelpMessage = "Logical name of the table when Target is specified as a Guid")]
        [Alias("EntityName", "LogicalName")]
        public string TableName { get; set; }

        [Parameter(Mandatory = true, HelpMessage = "Reference to the user or team to revoke access from. Can be an EntityReference, a PSObject with Id and TableName properties, or a Guid (requires RevokeeTableName parameter).")]
        public object Revokee { get; set; }

        [Parameter(Mandatory = false, HelpMessage = "Logical name of the revokee table (systemuser or team) when Revokee is specified as a Guid")]
        public string RevokeeTableName { get; set; }

        protected override void ProcessRecord()
        {
            base.ProcessRecord();

            RevokeAccessRequest request = new RevokeAccessRequest();

            // Convert Target to EntityReference
            request.Target = DataverseTypeConverter.ToEntityReference(Target, TableName, "Target");

            // Convert Revokee to EntityReference
            request.Revokee = DataverseTypeConverter.ToEntityReference(Revokee, RevokeeTableName, "Revokee");

            if (ShouldProcess($"Record {request.Target.LogicalName} {request.Target.Id}", $"Revoke access from {request.Revokee.LogicalName} {request.Revokee.Id}"))
            {
                RevokeAccessResponse response = (RevokeAccessResponse)Connection.Execute(request);
                WriteObject(response);
            }
        }
    }
}
