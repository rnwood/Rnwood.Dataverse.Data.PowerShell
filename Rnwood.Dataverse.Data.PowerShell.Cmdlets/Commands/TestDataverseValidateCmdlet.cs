using System;
using System.Management.Automation;
using Microsoft.Crm.Sdk.Messages;
using Microsoft.PowerPlatform.Dataverse.Client;
using Microsoft.Xrm.Sdk;

namespace Rnwood.Dataverse.Data.PowerShell.Commands
{
    [Cmdlet(VerbsDiagnostic.Test, "DataverseValidate")]
    [OutputType(typeof(ValidateResponse))]
    ///<summary>Executes ValidateRequest SDK message.</summary>
    public class TestDataverseValidateCmdlet : OrganizationServiceCmdlet
    {
        [Parameter(Mandatory = true, HelpMessage = "DataverseConnection instance obtained from Get-DataverseConnection cmdlet")]
        public override ServiceClient Connection { get; set; }
        [Parameter(Mandatory = false, HelpMessage = "Activities parameter")]
        public Microsoft.Xrm.Sdk.EntityCollection Activities { get; set; }

        protected override void ProcessRecord()
        {
            base.ProcessRecord();

            var request = new ValidateRequest();
            request.Activities = Activities;
            var response = (ValidateResponse)Connection.Execute(request);
            WriteObject(response);
        }
    }
}
