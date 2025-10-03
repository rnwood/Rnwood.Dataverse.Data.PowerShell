using System;
using System.Management.Automation;
using Microsoft.Crm.Sdk.Messages;
using Microsoft.PowerPlatform.Dataverse.Client;
using Microsoft.Xrm.Sdk;

namespace Rnwood.Dataverse.Data.PowerShell.Commands
{
    [Cmdlet(VerbsCommon.Optimize, "DataverseMapEntity")]
    [OutputType(typeof(AutoMapEntityResponse))]
    ///<summary>Executes AutoMapEntityRequest SDK message.</summary>
    public class AutoDataverseMapEntityCmdlet : OrganizationServiceCmdlet
    {
        [Parameter(Mandatory = true, HelpMessage = "DataverseConnection instance obtained from Get-DataverseConnection cmdlet")]
        public override ServiceClient Connection { get; set; }
        [Parameter(Mandatory = false, HelpMessage = "EntityMapId parameter")]
        public Guid EntityMapId { get; set; }

        protected override void ProcessRecord()
        {
            base.ProcessRecord();

            var request = new AutoMapEntityRequest();
            request.EntityMapId = EntityMapId;
            var response = (AutoMapEntityResponse)Connection.Execute(request);
            WriteObject(response);
        }
    }
}
