using System;
using System.Management.Automation;
using Microsoft.Crm.Sdk.Messages;
using Microsoft.PowerPlatform.Dataverse.Client;
using Microsoft.Xrm.Sdk;

namespace Rnwood.Dataverse.Data.PowerShell.Commands
{
    [Cmdlet(VerbsCommon.Get, "DataverseAppComponents")]
    [OutputType(typeof(RetrieveAppComponentsResponse))]
    ///<summary>Executes RetrieveAppComponentsRequest SDK message.</summary>
    public class GetDataverseAppComponentsCmdlet : OrganizationServiceCmdlet
    {
        [Parameter(Mandatory = true, HelpMessage = "DataverseConnection instance obtained from Get-DataverseConnection cmdlet")]
        public override ServiceClient Connection { get; set; }
        [Parameter(Mandatory = false, HelpMessage = "AppModuleId parameter")]
        public Guid AppModuleId { get; set; }

        protected override void ProcessRecord()
        {
            base.ProcessRecord();

            var request = new RetrieveAppComponentsRequest();
            request.AppModuleId = AppModuleId;
            var response = (RetrieveAppComponentsResponse)Connection.Execute(request);
            WriteObject(response);
        }
    }
}
