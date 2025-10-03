using System;
using System.Management.Automation;
using Microsoft.Crm.Sdk.Messages;
using Microsoft.PowerPlatform.Dataverse.Client;
using Microsoft.Xrm.Sdk;

namespace Rnwood.Dataverse.Data.PowerShell.Commands
{
    [Cmdlet(VerbsData.Publish, "DataverseProductHierarchy", SupportsShouldProcess = true, ConfirmImpact = ConfirmImpact.Medium)]
    [OutputType(typeof(PublishProductHierarchyResponse))]
    ///<summary>Executes PublishProductHierarchyRequest SDK message.</summary>
    public class PublishDataverseProductHierarchyCmdlet : OrganizationServiceCmdlet
    {
        [Parameter(Mandatory = true, HelpMessage = "DataverseConnection instance obtained from Get-DataverseConnection cmdlet")]
        public override ServiceClient Connection { get; set; }
        [Parameter(Mandatory = false, ValueFromPipelineByPropertyName = true, HelpMessage = "Target parameter")]
        public object Target { get; set; }

        protected override void ProcessRecord()
        {
            base.ProcessRecord();

            var request = new PublishProductHierarchyRequest();
            if (Target != null)
            {
                request.Target = DataverseTypeConverter.ToEntityReference(Target, null, "Target");
            }

            if (ShouldProcess("Executing PublishProductHierarchyRequest", "PublishProductHierarchyRequest"))
            {
                var response = (PublishProductHierarchyResponse)Connection.Execute(request);
                WriteObject(response);
            }
        }
    }
}
