using System;
using System.Management.Automation;
using Microsoft.Crm.Sdk.Messages;
using Microsoft.PowerPlatform.Dataverse.Client;
using Microsoft.Xrm.Sdk;

namespace Rnwood.Dataverse.Data.PowerShell.Commands
{
    [Cmdlet(VerbsCommon.Move, "DataverseObjectsOwner")]
    [OutputType(typeof(ReassignObjectsOwnerResponse))]
    ///<summary>Executes ReassignObjectsOwnerRequest SDK message.</summary>
    public class MoveDataverseObjectsOwnerCmdlet : OrganizationServiceCmdlet
    {
        [Parameter(Mandatory = true, HelpMessage = "DataverseConnection instance obtained from Get-DataverseConnection cmdlet")]
        public override ServiceClient Connection { get; set; }
        [Parameter(Mandatory = false, ValueFromPipelineByPropertyName = true, HelpMessage = "FromPrincipal parameter")]
        public object FromPrincipal { get; set; }
        [Parameter(Mandatory = false, ValueFromPipelineByPropertyName = true, HelpMessage = "ToPrincipal parameter")]
        public object ToPrincipal { get; set; }

        protected override void ProcessRecord()
        {
            base.ProcessRecord();

            var request = new ReassignObjectsOwnerRequest();
            if (FromPrincipal != null)
            {
                request.FromPrincipal = DataverseTypeConverter.ToEntityReference(FromPrincipal, null, "FromPrincipal");
            }
            if (ToPrincipal != null)
            {
                request.ToPrincipal = DataverseTypeConverter.ToEntityReference(ToPrincipal, null, "ToPrincipal");
            }

            var response = (ReassignObjectsOwnerResponse)Connection.Execute(request);
            WriteObject(response);
        }
    }
}
