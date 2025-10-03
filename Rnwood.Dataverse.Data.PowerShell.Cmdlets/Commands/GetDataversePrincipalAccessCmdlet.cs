using System;
using System.Management.Automation;
using Microsoft.Crm.Sdk.Messages;
using Microsoft.PowerPlatform.Dataverse.Client;
using Microsoft.Xrm.Sdk;

namespace Rnwood.Dataverse.Data.PowerShell.Commands
{
    [Cmdlet(VerbsCommon.Get, "DataversePrincipalAccess")]
    [OutputType(typeof(RetrievePrincipalAccessResponse))]
    ///<summary>Executes RetrievePrincipalAccessRequest SDK message.</summary>
    public class GetDataversePrincipalAccessCmdlet : OrganizationServiceCmdlet
    {
        [Parameter(Mandatory = true, HelpMessage = "DataverseConnection instance obtained from Get-DataverseConnection cmdlet")]
        public override ServiceClient Connection { get; set; }
        [Parameter(Mandatory = false, ValueFromPipelineByPropertyName = true, HelpMessage = "Target parameter")]
        public object Target { get; set; }
        [Parameter(Mandatory = false, ValueFromPipelineByPropertyName = true, HelpMessage = "Principal parameter")]
        public object Principal { get; set; }

        protected override void ProcessRecord()
        {
            base.ProcessRecord();

            var request = new RetrievePrincipalAccessRequest();
            if (Target != null)
            {
                request.Target = DataverseTypeConverter.ToEntityReference(Target, null, "Target");
            }
            if (Principal != null)
            {
                request.Principal = DataverseTypeConverter.ToEntityReference(Principal, null, "Principal");
            }

            var response = (RetrievePrincipalAccessResponse)Connection.Execute(request);
            WriteObject(response);
        }
    }
}
