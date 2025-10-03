using System;
using System.Management.Automation;
using Microsoft.Crm.Sdk.Messages;
using Microsoft.PowerPlatform.Dataverse.Client;
using Microsoft.Xrm.Sdk;

namespace Rnwood.Dataverse.Data.PowerShell.Commands
{
    [Cmdlet(VerbsCommon.Get, "DataverseTimeZoneCodeByLocalizedName")]
    [OutputType(typeof(GetTimeZoneCodeByLocalizedNameResponse))]
    ///<summary>Executes GetTimeZoneCodeByLocalizedNameRequest SDK message.</summary>
    public class GetDataverseTimeZoneCodeByLocalizedNameCmdlet : OrganizationServiceCmdlet
    {
        [Parameter(Mandatory = true, HelpMessage = "DataverseConnection instance obtained from Get-DataverseConnection cmdlet")]
        public override ServiceClient Connection { get; set; }
        [Parameter(Mandatory = false, HelpMessage = "LocalizedStandardName parameter")]
        public String LocalizedStandardName { get; set; }
        [Parameter(Mandatory = false, HelpMessage = "LocaleId parameter")]
        public Int32 LocaleId { get; set; }

        protected override void ProcessRecord()
        {
            base.ProcessRecord();

            var request = new GetTimeZoneCodeByLocalizedNameRequest();
            request.LocalizedStandardName = LocalizedStandardName;            request.LocaleId = LocaleId;
            var response = (GetTimeZoneCodeByLocalizedNameResponse)Connection.Execute(request);
            WriteObject(response);
        }
    }
}
