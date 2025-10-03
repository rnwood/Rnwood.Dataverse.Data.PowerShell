using System;
using System.Management.Automation;
using Microsoft.Crm.Sdk.Messages;
using Microsoft.PowerPlatform.Dataverse.Client;
using Microsoft.Xrm.Sdk;

namespace Rnwood.Dataverse.Data.PowerShell.Commands
{
    [Cmdlet(VerbsCommon.Get, "DataverseMissingComponents")]
    [OutputType(typeof(RetrieveMissingComponentsResponse))]
    ///<summary>Executes RetrieveMissingComponentsRequest SDK message.</summary>
    public class GetDataverseMissingComponentsCmdlet : OrganizationServiceCmdlet
    {
        [Parameter(Mandatory = true, HelpMessage = "DataverseConnection instance obtained from Get-DataverseConnection cmdlet")]
        public override ServiceClient Connection { get; set; }
        [Parameter(Mandatory = false, HelpMessage = "CustomizationFile parameter")]
        public Byte[] CustomizationFile { get; set; }

        protected override void ProcessRecord()
        {
            base.ProcessRecord();

            var request = new RetrieveMissingComponentsRequest();
            request.CustomizationFile = CustomizationFile;
            var response = (RetrieveMissingComponentsResponse)Connection.Execute(request);
            WriteObject(response);
        }
    }
}
