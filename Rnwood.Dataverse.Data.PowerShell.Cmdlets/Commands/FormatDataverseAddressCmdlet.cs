using System;
using System.Management.Automation;
using Microsoft.Crm.Sdk.Messages;
using Microsoft.PowerPlatform.Dataverse.Client;
using Microsoft.Xrm.Sdk;

namespace Rnwood.Dataverse.Data.PowerShell.Commands
{
    [Cmdlet(VerbsCommon.Format, "DataverseAddress")]
    [OutputType(typeof(FormatAddressResponse))]
    ///<summary>Executes FormatAddressRequest SDK message.</summary>
    public class FormatDataverseAddressCmdlet : OrganizationServiceCmdlet
    {
        [Parameter(Mandatory = true, HelpMessage = "DataverseConnection instance obtained from Get-DataverseConnection cmdlet")]
        public override ServiceClient Connection { get; set; }
        [Parameter(Mandatory = false, HelpMessage = "Line1 parameter")]
        public String Line1 { get; set; }
        [Parameter(Mandatory = false, HelpMessage = "City parameter")]
        public String City { get; set; }
        [Parameter(Mandatory = false, HelpMessage = "StateOrProvince parameter")]
        public String StateOrProvince { get; set; }
        [Parameter(Mandatory = false, HelpMessage = "PostalCode parameter")]
        public String PostalCode { get; set; }
        [Parameter(Mandatory = false, HelpMessage = "Country parameter")]
        public String Country { get; set; }

        protected override void ProcessRecord()
        {
            base.ProcessRecord();

            var request = new FormatAddressRequest();
            request.Line1 = Line1;            request.City = City;            request.StateOrProvince = StateOrProvince;            request.PostalCode = PostalCode;            request.Country = Country;
            var response = (FormatAddressResponse)Connection.Execute(request);
            WriteObject(response);
        }
    }
}
