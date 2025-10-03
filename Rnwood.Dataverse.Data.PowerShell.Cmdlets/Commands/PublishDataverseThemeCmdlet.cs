using System;
using System.Management.Automation;
using Microsoft.Crm.Sdk.Messages;
using Microsoft.PowerPlatform.Dataverse.Client;
using Microsoft.Xrm.Sdk;

namespace Rnwood.Dataverse.Data.PowerShell.Commands
{
    [Cmdlet(VerbsData.Publish, "DataverseTheme", SupportsShouldProcess = true, ConfirmImpact = ConfirmImpact.Medium)]
    [OutputType(typeof(PublishThemeResponse))]
    ///<summary>Executes PublishThemeRequest SDK message.</summary>
    public class PublishDataverseThemeCmdlet : OrganizationServiceCmdlet
    {
        [Parameter(Mandatory = true, HelpMessage = "DataverseConnection instance obtained from Get-DataverseConnection cmdlet")]
        public override ServiceClient Connection { get; set; }
        [Parameter(Mandatory = false, ValueFromPipelineByPropertyName = true, HelpMessage = "Target parameter")]
        public object Target { get; set; }

        protected override void ProcessRecord()
        {
            base.ProcessRecord();

            var request = new PublishThemeRequest();
            if (Target != null)
            {
                request.Target = DataverseTypeConverter.ToEntityReference(Target, null, "Target");
            }

            if (ShouldProcess("Executing PublishThemeRequest", "PublishThemeRequest"))
            {
                var response = (PublishThemeResponse)Connection.Execute(request);
                WriteObject(response);
            }
        }
    }
}
