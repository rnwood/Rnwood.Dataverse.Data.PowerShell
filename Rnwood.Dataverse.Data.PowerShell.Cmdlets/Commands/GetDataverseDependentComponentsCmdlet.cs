using System;
using System.Management.Automation;
using Microsoft.Crm.Sdk.Messages;
using Microsoft.PowerPlatform.Dataverse.Client;
using Microsoft.Xrm.Sdk;

namespace Rnwood.Dataverse.Data.PowerShell.Commands
{
    [Cmdlet(VerbsCommon.Get, "DataverseDependentComponents")]
    [OutputType(typeof(RetrieveDependentComponentsResponse))]
    ///<summary>Executes RetrieveDependentComponentsRequest SDK message.</summary>
    public class GetDataverseDependentComponentsCmdlet : OrganizationServiceCmdlet
    {
        [Parameter(Mandatory = true, HelpMessage = "DataverseConnection instance obtained from Get-DataverseConnection cmdlet")]
        public override ServiceClient Connection { get; set; }
        [Parameter(Mandatory = false, HelpMessage = "ObjectId parameter")]
        public Guid ObjectId { get; set; }
        [Parameter(Mandatory = false, HelpMessage = "ComponentType parameter")]
        public Int32 ComponentType { get; set; }

        protected override void ProcessRecord()
        {
            base.ProcessRecord();

            var request = new RetrieveDependentComponentsRequest();
            request.ObjectId = ObjectId;            request.ComponentType = ComponentType;
            var response = (RetrieveDependentComponentsResponse)Connection.Execute(request);
            WriteObject(response);
        }
    }
}
