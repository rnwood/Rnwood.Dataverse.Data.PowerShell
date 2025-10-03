using System;
using System.Management.Automation;
using Microsoft.Crm.Sdk.Messages;
using Microsoft.PowerPlatform.Dataverse.Client;
using Microsoft.Xrm.Sdk;

namespace Rnwood.Dataverse.Data.PowerShell.Commands
{
    [Cmdlet(VerbsCommon.Add, "DataverseRecurrence", SupportsShouldProcess = true, ConfirmImpact = ConfirmImpact.Medium)]
    [OutputType(typeof(AddRecurrenceResponse))]
    ///<summary>Executes AddRecurrenceRequest SDK message.</summary>
    public class AddDataverseRecurrenceCmdlet : OrganizationServiceCmdlet
    {
        [Parameter(Mandatory = true, HelpMessage = "DataverseConnection instance obtained from Get-DataverseConnection cmdlet")]
        public override ServiceClient Connection { get; set; }
        [Parameter(Mandatory = false, HelpMessage = "Target parameter")]
        public PSObject Target { get; set; }
        [Parameter(Mandatory = false, HelpMessage = "AppointmentId parameter")]
        public Guid AppointmentId { get; set; }

        protected override void ProcessRecord()
        {
            base.ProcessRecord();

            var request = new AddRecurrenceRequest();
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
            request.AppointmentId = AppointmentId;
            if (ShouldProcess("Executing AddRecurrenceRequest", "AddRecurrenceRequest"))
            {
                var response = (AddRecurrenceResponse)Connection.Execute(request);
                WriteObject(response);
            }
        }
    }
}
