using System;
using System.Management.Automation;
using Microsoft.Crm.Sdk.Messages;
using Microsoft.PowerPlatform.Dataverse.Client;
using Microsoft.Xrm.Sdk;

namespace Rnwood.Dataverse.Data.PowerShell.Commands
{
    [Cmdlet(VerbsCommon.Add, "DataverseChannelAccessProfilePrivileges", SupportsShouldProcess = true, ConfirmImpact = ConfirmImpact.Medium)]
    [OutputType(typeof(AddChannelAccessProfilePrivilegesResponse))]
    ///<summary>Executes AddChannelAccessProfilePrivilegesRequest SDK message.</summary>
    public class AddDataverseChannelAccessProfilePrivilegesCmdlet : OrganizationServiceCmdlet
    {
        [Parameter(Mandatory = true, HelpMessage = "DataverseConnection instance obtained from Get-DataverseConnection cmdlet")]
        public override ServiceClient Connection { get; set; }
        [Parameter(Mandatory = false, HelpMessage = "ChannelAccessProfileId parameter")]
        public Guid ChannelAccessProfileId { get; set; }
        [Parameter(Mandatory = false, HelpMessage = "Privileges parameter")]
        public Microsoft.Crm.Sdk.Messages.ChannelAccessProfilePrivilege[] Privileges { get; set; }

        protected override void ProcessRecord()
        {
            base.ProcessRecord();

            var request = new AddChannelAccessProfilePrivilegesRequest();
            request.ChannelAccessProfileId = ChannelAccessProfileId;            request.Privileges = Privileges;
            if (ShouldProcess("Executing AddChannelAccessProfilePrivilegesRequest", "AddChannelAccessProfilePrivilegesRequest"))
            {
                var response = (AddChannelAccessProfilePrivilegesResponse)Connection.Execute(request);
                WriteObject(response);
            }
        }
    }
}
