using System;
using System.Management.Automation;
using Microsoft.Crm.Sdk.Messages;
using Microsoft.PowerPlatform.Dataverse.Client;
using Microsoft.Xrm.Sdk;

namespace Rnwood.Dataverse.Data.PowerShell.Commands
{
    [Cmdlet(VerbsCommon.Get, "DataversePrincipalSyncAttributeMappings")]
    [OutputType(typeof(RetrievePrincipalSyncAttributeMappingsResponse))]
    ///<summary>Executes RetrievePrincipalSyncAttributeMappingsRequest SDK message.</summary>
    public class GetDataversePrincipalSyncAttributeMappingsCmdlet : OrganizationServiceCmdlet
    {
        [Parameter(Mandatory = true, HelpMessage = "DataverseConnection instance obtained from Get-DataverseConnection cmdlet")]
        public override ServiceClient Connection { get; set; }
        [Parameter(Mandatory = false, ValueFromPipelineByPropertyName = true, HelpMessage = "Principal parameter")]
        public object Principal { get; set; }

        protected override void ProcessRecord()
        {
            base.ProcessRecord();

            var request = new RetrievePrincipalSyncAttributeMappingsRequest();
            if (Principal != null)
            {
                request.Principal = DataverseTypeConverter.ToEntityReference(Principal, null, "Principal");
            }

            var response = (RetrievePrincipalSyncAttributeMappingsResponse)Connection.Execute(request);
            WriteObject(response);
        }
    }
}
