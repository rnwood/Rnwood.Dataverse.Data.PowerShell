using System;
using System.Management.Automation;
using Microsoft.Crm.Sdk.Messages;
using Microsoft.PowerPlatform.Dataverse.Client;
using Microsoft.Xrm.Sdk;

namespace Rnwood.Dataverse.Data.PowerShell.Commands
{
    [Cmdlet(VerbsCommon.Set, "DataverseParentBusinessUnit", SupportsShouldProcess = true, ConfirmImpact = ConfirmImpact.Medium)]
    [OutputType(typeof(SetParentBusinessUnitResponse))]
    ///<summary>Executes SetParentBusinessUnitRequest SDK message.</summary>
    public class SetDataverseParentBusinessUnitCmdlet : OrganizationServiceCmdlet
    {
        [Parameter(Mandatory = true, HelpMessage = "DataverseConnection instance obtained from Get-DataverseConnection cmdlet")]
        public override ServiceClient Connection { get; set; }
        [Parameter(Mandatory = false, HelpMessage = "BusinessUnitId parameter")]
        public Guid BusinessUnitId { get; set; }
        [Parameter(Mandatory = false, HelpMessage = "ParentId parameter")]
        public Guid ParentId { get; set; }

        protected override void ProcessRecord()
        {
            base.ProcessRecord();

            var request = new SetParentBusinessUnitRequest();
            request.BusinessUnitId = BusinessUnitId;            request.ParentId = ParentId;
            if (ShouldProcess("Executing SetParentBusinessUnitRequest", "SetParentBusinessUnitRequest"))
            {
                var response = (SetParentBusinessUnitResponse)Connection.Execute(request);
                WriteObject(response);
            }
        }
    }
}
