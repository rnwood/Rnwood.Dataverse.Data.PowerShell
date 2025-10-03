using System;
using System.Management.Automation;
using Microsoft.Crm.Sdk.Messages;
using Microsoft.PowerPlatform.Dataverse.Client;
using Microsoft.Xrm.Sdk;

namespace Rnwood.Dataverse.Data.PowerShell.Commands
{
    [Cmdlet(VerbsCommon.Copy, "DataverseCampaign", SupportsShouldProcess = true, ConfirmImpact = ConfirmImpact.Medium)]
    [OutputType(typeof(CopyCampaignResponse))]
    ///<summary>Executes CopyCampaignRequest SDK message.</summary>
    public class CopyDataverseCampaignCmdlet : OrganizationServiceCmdlet
    {
        [Parameter(Mandatory = true, HelpMessage = "DataverseConnection instance obtained from Get-DataverseConnection cmdlet")]
        public override ServiceClient Connection { get; set; }
        [Parameter(Mandatory = false, HelpMessage = "BaseCampaign parameter")]
        public Guid BaseCampaign { get; set; }
        [Parameter(Mandatory = false, HelpMessage = "SaveAsTemplate parameter")]
        public Boolean SaveAsTemplate { get; set; }

        protected override void ProcessRecord()
        {
            base.ProcessRecord();

            var request = new CopyCampaignRequest();
            request.BaseCampaign = BaseCampaign;            request.SaveAsTemplate = SaveAsTemplate;
            if (ShouldProcess("Executing CopyCampaignRequest", "CopyCampaignRequest"))
            {
                var response = (CopyCampaignResponse)Connection.Execute(request);
                WriteObject(response);
            }
        }
    }
}
