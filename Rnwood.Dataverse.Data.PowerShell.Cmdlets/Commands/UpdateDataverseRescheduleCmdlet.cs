using System;
using System.Management.Automation;
using Microsoft.Crm.Sdk.Messages;
using Microsoft.PowerPlatform.Dataverse.Client;
using Microsoft.Xrm.Sdk;

namespace Rnwood.Dataverse.Data.PowerShell.Commands
{
    [Cmdlet(VerbsData.Update, "DataverseReschedule", SupportsShouldProcess = true, ConfirmImpact = ConfirmImpact.Medium)]
    [OutputType(typeof(RescheduleResponse))]
    ///<summary>Executes RescheduleRequest SDK message.</summary>
    public class UpdateDataverseRescheduleCmdlet : OrganizationServiceCmdlet
    {
        [Parameter(Mandatory = true, HelpMessage = "DataverseConnection instance obtained from Get-DataverseConnection cmdlet")]
        public override ServiceClient Connection { get; set; }
        [Parameter(Mandatory = false, HelpMessage = "Target parameter")]
        public PSObject Target { get; set; }
        [Parameter(Mandatory = false, HelpMessage = "ReturnNotifications parameter")]
        public Boolean ReturnNotifications { get; set; }

        protected override void ProcessRecord()
        {
            base.ProcessRecord();

            var request = new RescheduleRequest();
            if (Target != null)
            {
                var entity = new Entity();
                foreach (PSPropertyInfo prop in Target.Properties)
                {
                    if (prop.Name != "Id" && prop.Name != "TableName" && prop.Name != "LogicalName")
                    {
                        entity[prop.Name] = prop.Value;
                    }
                }
                request.Target = entity;
            }
            request.ReturnNotifications = ReturnNotifications;
            if (ShouldProcess("Executing RescheduleRequest", "RescheduleRequest"))
            {
                var response = (RescheduleResponse)Connection.Execute(request);
                WriteObject(response);
            }
        }
    }
}
