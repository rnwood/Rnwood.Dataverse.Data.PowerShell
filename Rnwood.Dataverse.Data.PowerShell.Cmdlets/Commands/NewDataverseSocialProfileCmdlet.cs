using System;
using System.Management.Automation;
using Microsoft.Crm.Sdk.Messages;
using Microsoft.PowerPlatform.Dataverse.Client;
using Microsoft.Xrm.Sdk;

namespace Rnwood.Dataverse.Data.PowerShell.Commands
{
    [Cmdlet(VerbsCommon.New, "DataverseSocialProfile", SupportsShouldProcess = true, ConfirmImpact = ConfirmImpact.Medium)]
    [OutputType(typeof(GenerateSocialProfileResponse))]
    ///<summary>Executes GenerateSocialProfileRequest SDK message.</summary>
    public class NewDataverseSocialProfileCmdlet : OrganizationServiceCmdlet
    {
        [Parameter(Mandatory = true, HelpMessage = "DataverseConnection instance obtained from Get-DataverseConnection cmdlet")]
        public override ServiceClient Connection { get; set; }
        [Parameter(Mandatory = false, HelpMessage = "Entity parameter")]
        public PSObject Entity { get; set; }

        protected override void ProcessRecord()
        {
            base.ProcessRecord();

            var request = new GenerateSocialProfileRequest();
            if (Entity != null)
            {
                var entity = new Entity();
                foreach (PSPropertyInfo prop in Entity.Properties)
                {
                    if (prop.Name != "Id" && prop.Name != "TableName" && prop.Name != "LogicalName")
                    {
                        entity[prop.Name] = prop.Value;
                    }
                }
                request.Entity = entity;
            }

            if (ShouldProcess("Executing GenerateSocialProfileRequest", "GenerateSocialProfileRequest"))
            {
                var response = (GenerateSocialProfileResponse)Connection.Execute(request);
                WriteObject(response);
            }
        }
    }
}
