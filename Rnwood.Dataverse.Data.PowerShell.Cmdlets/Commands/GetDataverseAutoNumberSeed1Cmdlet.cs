using System;
using System.Management.Automation;
using Microsoft.Crm.Sdk.Messages;
using Microsoft.PowerPlatform.Dataverse.Client;
using Microsoft.Xrm.Sdk;

namespace Rnwood.Dataverse.Data.PowerShell.Commands
{
    [Cmdlet(VerbsCommon.Get, "DataverseAutoNumberSeed1")]
    [OutputType(typeof(GetAutoNumberSeed1Response))]
    ///<summary>Executes GetAutoNumberSeed1Request SDK message.</summary>
    public class GetDataverseAutoNumberSeed1Cmdlet : OrganizationServiceCmdlet
    {
        [Parameter(Mandatory = true, HelpMessage = "DataverseConnection instance obtained from Get-DataverseConnection cmdlet")]
        public override ServiceClient Connection { get; set; }
        [Parameter(Mandatory = false, HelpMessage = "EntityName parameter")]
        public String EntityName { get; set; }
        [Parameter(Mandatory = false, HelpMessage = "AttributeName parameter")]
        public String AttributeName { get; set; }

        protected override void ProcessRecord()
        {
            base.ProcessRecord();

            var request = new GetAutoNumberSeed1Request();
            request.EntityName = EntityName;            request.AttributeName = AttributeName;
            var response = (GetAutoNumberSeed1Response)Connection.Execute(request);
            WriteObject(response);
        }
    }
}
