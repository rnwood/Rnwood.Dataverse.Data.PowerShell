using System;
using System.Management.Automation;
using Microsoft.Crm.Sdk.Messages;
using Microsoft.PowerPlatform.Dataverse.Client;
using Microsoft.Xrm.Sdk;

namespace Rnwood.Dataverse.Data.PowerShell.Commands
{
    [Cmdlet(VerbsCommon.Get, "DataverseActivePath")]
    [OutputType(typeof(RetrieveActivePathResponse))]
    ///<summary>Executes RetrieveActivePathRequest SDK message.</summary>
    public class GetDataverseActivePathCmdlet : OrganizationServiceCmdlet
    {
        [Parameter(Mandatory = true, HelpMessage = "DataverseConnection instance obtained from Get-DataverseConnection cmdlet")]
        public override ServiceClient Connection { get; set; }
        [Parameter(Mandatory = false, HelpMessage = "ProcessInstanceId parameter")]
        public Guid ProcessInstanceId { get; set; }

        protected override void ProcessRecord()
        {
            base.ProcessRecord();

            var request = new RetrieveActivePathRequest();
            request.ProcessInstanceId = ProcessInstanceId;
            var response = (RetrieveActivePathResponse)Connection.Execute(request);
            WriteObject(response);
        }
    }
}
