using System;
using System.Management.Automation;
using Microsoft.Crm.Sdk.Messages;
using Microsoft.PowerPlatform.Dataverse.Client;
using Microsoft.Xrm.Sdk;

namespace Rnwood.Dataverse.Data.PowerShell.Commands
{
    [Cmdlet(VerbsCommon.Set, "DataverseBusinessEquipment", SupportsShouldProcess = true, ConfirmImpact = ConfirmImpact.Medium)]
    [OutputType(typeof(SetBusinessEquipmentResponse))]
    ///<summary>Executes SetBusinessEquipmentRequest SDK message.</summary>
    public class SetDataverseBusinessEquipmentCmdlet : OrganizationServiceCmdlet
    {
        [Parameter(Mandatory = true, HelpMessage = "DataverseConnection instance obtained from Get-DataverseConnection cmdlet")]
        public override ServiceClient Connection { get; set; }
        [Parameter(Mandatory = false, HelpMessage = "EquipmentId parameter")]
        public Guid EquipmentId { get; set; }
        [Parameter(Mandatory = false, HelpMessage = "BusinessUnitId parameter")]
        public Guid BusinessUnitId { get; set; }

        protected override void ProcessRecord()
        {
            base.ProcessRecord();

            var request = new SetBusinessEquipmentRequest();
            request.EquipmentId = EquipmentId;            request.BusinessUnitId = BusinessUnitId;
            if (ShouldProcess("Executing SetBusinessEquipmentRequest", "SetBusinessEquipmentRequest"))
            {
                var response = (SetBusinessEquipmentResponse)Connection.Execute(request);
                WriteObject(response);
            }
        }
    }
}
