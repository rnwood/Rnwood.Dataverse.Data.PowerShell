using System;
using System.Management.Automation;
using Microsoft.Crm.Sdk.Messages;
using Microsoft.PowerPlatform.Dataverse.Client;
using Microsoft.Xrm.Sdk;

namespace Rnwood.Dataverse.Data.PowerShell.Commands
{
    [Cmdlet(VerbsData.Update, "DataverseEntitlement", SupportsShouldProcess = true, ConfirmImpact = ConfirmImpact.Medium)]
    [OutputType(typeof(RenewEntitlementResponse))]
    ///<summary>Executes RenewEntitlementRequest SDK message.</summary>
    public class UpdateDataverseEntitlementCmdlet : OrganizationServiceCmdlet
    {
        [Parameter(Mandatory = true, HelpMessage = "DataverseConnection instance obtained from Get-DataverseConnection cmdlet")]
        public override ServiceClient Connection { get; set; }
        [Parameter(Mandatory = false, HelpMessage = "EntitlementId parameter")]
        public Guid EntitlementId { get; set; }
        [Parameter(Mandatory = false, HelpMessage = "Status parameter")]
        public Int32 Status { get; set; }

        protected override void ProcessRecord()
        {
            base.ProcessRecord();

            var request = new RenewEntitlementRequest();
            request.EntitlementId = EntitlementId;            request.Status = Status;
            if (ShouldProcess("Executing RenewEntitlementRequest", "RenewEntitlementRequest"))
            {
                var response = (RenewEntitlementResponse)Connection.Execute(request);
                WriteObject(response);
            }
        }
    }
}
