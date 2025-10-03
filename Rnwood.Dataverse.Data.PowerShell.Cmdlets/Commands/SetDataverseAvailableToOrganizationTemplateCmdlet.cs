using System;
using System.Management.Automation;
using Microsoft.Crm.Sdk.Messages;
using Microsoft.PowerPlatform.Dataverse.Client;
using Microsoft.Xrm.Sdk;

namespace Rnwood.Dataverse.Data.PowerShell.Commands
{
    [Cmdlet(VerbsCommon.Set, "DataverseAvailableToOrganizationTemplate", SupportsShouldProcess = true, ConfirmImpact = ConfirmImpact.Medium)]
    [OutputType(typeof(MakeAvailableToOrganizationTemplateResponse))]
    ///<summary>Executes MakeAvailableToOrganizationTemplateRequest SDK message.</summary>
    public class SetDataverseAvailableToOrganizationTemplateCmdlet : OrganizationServiceCmdlet
    {
        [Parameter(Mandatory = true, HelpMessage = "DataverseConnection instance obtained from Get-DataverseConnection cmdlet")]
        public override ServiceClient Connection { get; set; }
        [Parameter(Mandatory = false, HelpMessage = "TemplateId parameter")]
        public Guid TemplateId { get; set; }

        protected override void ProcessRecord()
        {
            base.ProcessRecord();

            var request = new MakeAvailableToOrganizationTemplateRequest();
            request.TemplateId = TemplateId;
            if (ShouldProcess("Executing MakeAvailableToOrganizationTemplateRequest", "MakeAvailableToOrganizationTemplateRequest"))
            {
                var response = (MakeAvailableToOrganizationTemplateResponse)Connection.Execute(request);
                WriteObject(response);
            }
        }
    }
}
