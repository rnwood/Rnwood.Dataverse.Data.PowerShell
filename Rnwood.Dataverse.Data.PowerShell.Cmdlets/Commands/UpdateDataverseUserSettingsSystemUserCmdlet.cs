using System;
using System.Management.Automation;
using Microsoft.Crm.Sdk.Messages;
using Microsoft.PowerPlatform.Dataverse.Client;
using Microsoft.Xrm.Sdk;

namespace Rnwood.Dataverse.Data.PowerShell.Commands
{
    [Cmdlet(VerbsData.Update, "DataverseUserSettingsSystemUser", SupportsShouldProcess = true, ConfirmImpact = ConfirmImpact.Medium)]
    [OutputType(typeof(UpdateUserSettingsSystemUserResponse))]
    ///<summary>Executes UpdateUserSettingsSystemUserRequest SDK message.</summary>
    public class UpdateDataverseUserSettingsSystemUserCmdlet : OrganizationServiceCmdlet
    {
        [Parameter(Mandatory = true, HelpMessage = "DataverseConnection instance obtained from Get-DataverseConnection cmdlet")]
        public override ServiceClient Connection { get; set; }
        [Parameter(Mandatory = false, HelpMessage = "UserId parameter")]
        public Guid UserId { get; set; }
        [Parameter(Mandatory = false, HelpMessage = "Settings parameter")]
        public PSObject Settings { get; set; }

        protected override void ProcessRecord()
        {
            base.ProcessRecord();

            var request = new UpdateUserSettingsSystemUserRequest();
            request.UserId = UserId;            if (Settings != null)
            {
                var entity = new Entity();
                foreach (PSPropertyInfo prop in Settings.Properties)
                {
                    if (prop.Name != "Id" && prop.Name != "TableName" && prop.Name != "LogicalName")
                    {
                        entity[prop.Name] = prop.Value;
                    }
                }
                request.Settings = entity;
            }

            if (ShouldProcess("Executing UpdateUserSettingsSystemUserRequest", "UpdateUserSettingsSystemUserRequest"))
            {
                var response = (UpdateUserSettingsSystemUserResponse)Connection.Execute(request);
                WriteObject(response);
            }
        }
    }
}
