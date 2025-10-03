using System;
using System.Management.Automation;
using Microsoft.Crm.Sdk.Messages;
using Microsoft.PowerPlatform.Dataverse.Client;
using Microsoft.Xrm.Sdk;

namespace Rnwood.Dataverse.Data.PowerShell.Commands
{
    [Cmdlet(VerbsCommon.Get, "DataverseChannelAccessProfilePrivileges")]
    [OutputType(typeof(RetrieveChannelAccessProfilePrivilegesResponse))]
    ///<summary>Executes RetrieveChannelAccessProfilePrivilegesRequest SDK message.</summary>
    public class GetDataverseChannelAccessProfilePrivilegesCmdlet : OrganizationServiceCmdlet
    {
        [Parameter(Mandatory = true, HelpMessage = "DataverseConnection instance obtained from Get-DataverseConnection cmdlet")]
        public override ServiceClient Connection { get; set; }
        [Parameter(Mandatory = false, HelpMessage = "ChannelAccessProfileId parameter")]
        public Guid ChannelAccessProfileId { get; set; }

        protected override void ProcessRecord()
        {
            base.ProcessRecord();

            var request = new RetrieveChannelAccessProfilePrivilegesRequest();
            request.ChannelAccessProfileId = ChannelAccessProfileId;
            var response = (RetrieveChannelAccessProfilePrivilegesResponse)Connection.Execute(request);
            WriteObject(response);
        }
    }
}
