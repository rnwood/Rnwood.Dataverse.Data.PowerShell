using System;
using System.Management.Automation;
using Microsoft.Crm.Sdk.Messages;
using Microsoft.PowerPlatform.Dataverse.Client;
using Microsoft.Xrm.Sdk;

namespace Rnwood.Dataverse.Data.PowerShell.Commands
{
    [Cmdlet(VerbsCommon.Get, "DataverseDecryptionKey")]
    [OutputType(typeof(GetDecryptionKeyResponse))]
    ///<summary>Executes GetDecryptionKeyRequest SDK message.</summary>
    public class GetDataverseDecryptionKeyCmdlet : OrganizationServiceCmdlet
    {
        [Parameter(Mandatory = true, HelpMessage = "DataverseConnection instance obtained from Get-DataverseConnection cmdlet")]
        public override ServiceClient Connection { get; set; }

        protected override void ProcessRecord()
        {
            base.ProcessRecord();

            var request = new GetDecryptionKeyRequest();

            var response = (GetDecryptionKeyResponse)Connection.Execute(request);
            WriteObject(response);
        }
    }
}
