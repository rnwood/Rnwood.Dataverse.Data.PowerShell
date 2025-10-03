using System;
using System.Management.Automation;
using Microsoft.Crm.Sdk.Messages;
using Microsoft.PowerPlatform.Dataverse.Client;
using Microsoft.Xrm.Sdk;

namespace Rnwood.Dataverse.Data.PowerShell.Commands
{
    [Cmdlet(VerbsCommon.Set, "DataverseAutoNumberSeed1", SupportsShouldProcess = true, ConfirmImpact = ConfirmImpact.Medium)]
    [OutputType(typeof(SetAutoNumberSeed1Response))]
    ///<summary>Executes SetAutoNumberSeed1Request SDK message.</summary>
    public class SetDataverseAutoNumberSeed1Cmdlet : OrganizationServiceCmdlet
    {
        [Parameter(Mandatory = true, HelpMessage = "DataverseConnection instance obtained from Get-DataverseConnection cmdlet")]
        public override ServiceClient Connection { get; set; }
        [Parameter(Mandatory = false, HelpMessage = "EntityName parameter")]
        public String EntityName { get; set; }
        [Parameter(Mandatory = false, HelpMessage = "AttributeName parameter")]
        public String AttributeName { get; set; }
        [Parameter(Mandatory = false, HelpMessage = "Value parameter")]
        public Int64 Value { get; set; }

        protected override void ProcessRecord()
        {
            base.ProcessRecord();

            var request = new SetAutoNumberSeed1Request();
            request.EntityName = EntityName;            request.AttributeName = AttributeName;            request.Value = Value;
            if (ShouldProcess("Executing SetAutoNumberSeed1Request", "SetAutoNumberSeed1Request"))
            {
                var response = (SetAutoNumberSeed1Response)Connection.Execute(request);
                WriteObject(response);
            }
        }
    }
}
