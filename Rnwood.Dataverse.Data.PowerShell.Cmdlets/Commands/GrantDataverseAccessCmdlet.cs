using System;
using System.Management.Automation;
using Microsoft.Crm.Sdk.Messages;
using Microsoft.PowerPlatform.Dataverse.Client;
using Microsoft.Xrm.Sdk;

namespace Rnwood.Dataverse.Data.PowerShell.Commands
{
    [Cmdlet(VerbsSecurity.Grant, "DataverseAccess", SupportsShouldProcess = true, ConfirmImpact = ConfirmImpact.Medium)]
    [OutputType(typeof(GrantAccessResponse))]
    ///<summary>Grants access to a record for a user or team.</summary>
    public class GrantDataverseAccessCmdlet : OrganizationServiceCmdlet
    {
        [Parameter(Mandatory = true, HelpMessage = "DataverseConnection instance obtained from Get-DataverseConnection cmdlet")]
        public override ServiceClient Connection { get; set; }

        [Parameter(Mandatory = true, ValueFromPipelineByPropertyName = true, HelpMessage = "Reference to the record to grant access to. Can be an EntityReference, a PSObject with Id and TableName properties, or a Guid (requires TableName parameter).")]
        public object Target { get; set; }

        [Parameter(Mandatory = false, ValueFromPipelineByPropertyName = true, HelpMessage = "Logical name of the table when Target is specified as a Guid")]
        [Alias("EntityName", "LogicalName")]
        public string TableName { get; set; }

        [Parameter(Mandatory = true, HelpMessage = "Reference to the user or team to grant access to. Can be an EntityReference, a PSObject with Id and TableName properties, or a Guid (requires PrincipalTableName parameter).")]
        public object Principal { get; set; }

        [Parameter(Mandatory = false, HelpMessage = "Logical name of the principal table (systemuser or team) when Principal is specified as a Guid")]
        public string PrincipalTableName { get; set; }

        [Parameter(Mandatory = true, HelpMessage = "Access rights to grant. Can be a combination of: None, ReadAccess, WriteAccess, AppendAccess, AppendToAccess, CreateAccess, DeleteAccess, ShareAccess, AssignAccess")]
        public AccessRights AccessRights { get; set; }

        protected override void ProcessRecord()
        {
            base.ProcessRecord();

            GrantAccessRequest request = new GrantAccessRequest();

            // Convert Target to EntityReference
            request.Target = ConvertToEntityReference(Target, TableName, "Target");

            // Create PrincipalAccess
            request.PrincipalAccess = new PrincipalAccess
            {
                Principal = ConvertToEntityReference(Principal, PrincipalTableName, "Principal"),
                AccessMask = AccessRights
            };

            if (ShouldProcess($"Record {request.Target.LogicalName} {request.Target.Id}", $"Grant {AccessRights} to {request.PrincipalAccess.Principal.LogicalName} {request.PrincipalAccess.Principal.Id}"))
            {
                GrantAccessResponse response = (GrantAccessResponse)Connection.Execute(request);
                WriteObject(response);
            }
        }

        private EntityReference ConvertToEntityReference(object value, string tableName, string parameterName)
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
                    throw new ArgumentException($"TableName parameter is required when {parameterName} is specified as a Guid");
                }
                return new EntityReference(tableName, guid);
            }
            else if (value is string strValue && Guid.TryParse(strValue, out Guid parsedGuid))
            {
                if (string.IsNullOrEmpty(tableName))
                {
                    throw new ArgumentException($"TableName parameter is required when {parameterName} is specified as a string Guid");
                }
                return new EntityReference(tableName, parsedGuid);
            }

            throw new ArgumentException($"Unable to convert {parameterName} to EntityReference. Expected EntityReference, PSObject with Id and TableName properties, or Guid with TableName parameter.");
        }
    }
}
