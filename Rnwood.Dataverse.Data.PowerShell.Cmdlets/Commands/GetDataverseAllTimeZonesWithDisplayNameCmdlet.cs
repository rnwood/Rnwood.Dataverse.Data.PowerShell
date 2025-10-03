using System;
using System.Management.Automation;
using Microsoft.Crm.Sdk.Messages;
using Microsoft.PowerPlatform.Dataverse.Client;
using Microsoft.Xrm.Sdk;

namespace Rnwood.Dataverse.Data.PowerShell.Commands
{
    [Cmdlet(VerbsCommon.Get, "DataverseAllTimeZonesWithDisplayName")]
    [OutputType(typeof(GetAllTimeZonesWithDisplayNameResponse))]
    ///<summary>Executes GetAllTimeZonesWithDisplayNameRequest SDK message.</summary>
    public class GetDataverseAllTimeZonesWithDisplayNameCmdlet : OrganizationServiceCmdlet
    {
        [Parameter(Mandatory = true, HelpMessage = "DataverseConnection instance obtained from Get-DataverseConnection cmdlet")]
        public override ServiceClient Connection { get; set; }
        [Parameter(Mandatory = false, HelpMessage = "LocaleId parameter")]
        public Int32 LocaleId { get; set; }

        protected override void ProcessRecord()
        {
            base.ProcessRecord();

            var request = new GetAllTimeZonesWithDisplayNameRequest();
            request.LocaleId = LocaleId;
            var response = (GetAllTimeZonesWithDisplayNameResponse)Connection.Execute(request);
            WriteObject(response);
        }
    }
}
