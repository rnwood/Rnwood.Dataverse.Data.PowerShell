using System;
using System.Management.Automation;
using Microsoft.Crm.Sdk.Messages;
using Microsoft.PowerPlatform.Dataverse.Client;
using Microsoft.Xrm.Sdk;

namespace Rnwood.Dataverse.Data.PowerShell.Commands
{
    [Cmdlet(VerbsCommon.Get, "DataverseAbsoluteAndSiteCollectionUrl")]
    [OutputType(typeof(RetrieveAbsoluteAndSiteCollectionUrlResponse))]
    ///<summary>Executes RetrieveAbsoluteAndSiteCollectionUrlRequest SDK message.</summary>
    public class GetDataverseAbsoluteAndSiteCollectionUrlCmdlet : OrganizationServiceCmdlet
    {
        [Parameter(Mandatory = true, HelpMessage = "DataverseConnection instance obtained from Get-DataverseConnection cmdlet")]
        public override ServiceClient Connection { get; set; }
        [Parameter(Mandatory = false, ValueFromPipelineByPropertyName = true, HelpMessage = "Target parameter")]
        public object Target { get; set; }

        protected override void ProcessRecord()
        {
            base.ProcessRecord();

            var request = new RetrieveAbsoluteAndSiteCollectionUrlRequest();
            if (Target != null)
            {
                request.Target = DataverseTypeConverter.ToEntityReference(Target, null, "Target");
            }

            var response = (RetrieveAbsoluteAndSiteCollectionUrlResponse)Connection.Execute(request);
            WriteObject(response);
        }
    }
}
