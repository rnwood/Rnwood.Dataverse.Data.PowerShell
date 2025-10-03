using System;
using System.Management.Automation;
using Microsoft.Crm.Sdk.Messages;
using Microsoft.PowerPlatform.Dataverse.Client;
using Microsoft.Xrm.Sdk;

namespace Rnwood.Dataverse.Data.PowerShell.Commands
{
    [Cmdlet(VerbsCommon.Get, "DataverseByResourcesService")]
    [OutputType(typeof(RetrieveByResourcesServiceResponse))]
    ///<summary>Executes RetrieveByResourcesServiceRequest SDK message.</summary>
    public class GetDataverseByResourcesServiceCmdlet : OrganizationServiceCmdlet
    {
        [Parameter(Mandatory = true, HelpMessage = "DataverseConnection instance obtained from Get-DataverseConnection cmdlet")]
        public override ServiceClient Connection { get; set; }
        [Parameter(Mandatory = false, HelpMessage = "ResourceIds parameter")]
        public Guid[] ResourceIds { get; set; }
        [Parameter(Mandatory = false, HelpMessage = "Query parameter")]
        public Microsoft.Xrm.Sdk.Query.QueryBase Query { get; set; }

        protected override void ProcessRecord()
        {
            base.ProcessRecord();

            var request = new RetrieveByResourcesServiceRequest();
            request.ResourceIds = ResourceIds;            request.Query = Query;
            var response = (RetrieveByResourcesServiceResponse)Connection.Execute(request);
            WriteObject(response);
        }
    }
}
