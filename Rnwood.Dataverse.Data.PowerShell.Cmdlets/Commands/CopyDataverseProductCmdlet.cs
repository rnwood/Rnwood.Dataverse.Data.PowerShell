using System;
using System.Management.Automation;
using Microsoft.Crm.Sdk.Messages;
using Microsoft.PowerPlatform.Dataverse.Client;
using Microsoft.Xrm.Sdk;

namespace Rnwood.Dataverse.Data.PowerShell.Commands
{
    [Cmdlet(VerbsCommon.Copy, "DataverseProduct", SupportsShouldProcess = true, ConfirmImpact = ConfirmImpact.Medium)]
    [OutputType(typeof(CloneProductResponse))]
    ///<summary>Executes CloneProductRequest SDK message.</summary>
    public class CopyDataverseProductCmdlet : OrganizationServiceCmdlet
    {
        [Parameter(Mandatory = true, HelpMessage = "DataverseConnection instance obtained from Get-DataverseConnection cmdlet")]
        public override ServiceClient Connection { get; set; }
        [Parameter(Mandatory = false, ValueFromPipelineByPropertyName = true, HelpMessage = "Source parameter")]
        public object Source { get; set; }

        protected override void ProcessRecord()
        {
            base.ProcessRecord();

            var request = new CloneProductRequest();
            if (Source != null)
            {
                request.Source = DataverseTypeConverter.ToEntityReference(Source, null, "Source");
            }

            if (ShouldProcess("Executing CloneProductRequest", "CloneProductRequest"))
            {
                var response = (CloneProductResponse)Connection.Execute(request);
                WriteObject(response);
            }
        }
    }
}
