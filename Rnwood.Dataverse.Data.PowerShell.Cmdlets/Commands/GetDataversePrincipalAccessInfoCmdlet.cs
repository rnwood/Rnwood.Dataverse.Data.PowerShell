using System;
using System.Management.Automation;
using Microsoft.Crm.Sdk.Messages;
using Microsoft.PowerPlatform.Dataverse.Client;
using Microsoft.Xrm.Sdk;

namespace Rnwood.Dataverse.Data.PowerShell.Commands
{
    [Cmdlet(VerbsCommon.Get, "DataversePrincipalAccessInfo")]
    [OutputType(typeof(RetrievePrincipalAccessInfoResponse))]
    ///<summary>Executes RetrievePrincipalAccessInfoRequest SDK message.</summary>
    public class GetDataversePrincipalAccessInfoCmdlet : OrganizationServiceCmdlet
    {
        [Parameter(Mandatory = true, HelpMessage = "DataverseConnection instance obtained from Get-DataverseConnection cmdlet")]
        public override ServiceClient Connection { get; set; }
        [Parameter(Mandatory = false, ValueFromPipelineByPropertyName = true, HelpMessage = "Principal parameter")]
        public object Principal { get; set; }
        [Parameter(Mandatory = false, HelpMessage = "ObjectId parameter")]
        public Guid ObjectId { get; set; }
        [Parameter(Mandatory = false, HelpMessage = "EntityName parameter")]
        public String EntityName { get; set; }

        protected override void ProcessRecord()
        {
            base.ProcessRecord();

            var request = new RetrievePrincipalAccessInfoRequest();
            if (Principal != null)
            {
                request.Principal = DataverseTypeConverter.ToEntityReference(Principal, null, "Principal");
            }
            request.ObjectId = ObjectId;            request.EntityName = EntityName;
            var response = (RetrievePrincipalAccessInfoResponse)Connection.Execute(request);
            WriteObject(response);
        }
    }
}
