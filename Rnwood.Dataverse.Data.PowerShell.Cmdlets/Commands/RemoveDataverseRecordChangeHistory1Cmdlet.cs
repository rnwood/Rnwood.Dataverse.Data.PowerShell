using System;
using System.Management.Automation;
using Microsoft.Crm.Sdk.Messages;
using Microsoft.PowerPlatform.Dataverse.Client;
using Microsoft.Xrm.Sdk;

namespace Rnwood.Dataverse.Data.PowerShell.Commands
{
    [Cmdlet(VerbsCommon.Remove, "DataverseRecordChangeHistory1", SupportsShouldProcess = true, ConfirmImpact = ConfirmImpact.Medium)]
    [OutputType(typeof(DeleteRecordChangeHistory1Response))]
    ///<summary>Executes DeleteRecordChangeHistory1Request SDK message.</summary>
    public class RemoveDataverseRecordChangeHistory1Cmdlet : OrganizationServiceCmdlet
    {
        [Parameter(Mandatory = true, HelpMessage = "DataverseConnection instance obtained from Get-DataverseConnection cmdlet")]
        public override ServiceClient Connection { get; set; }
        [Parameter(Mandatory = false, ValueFromPipelineByPropertyName = true, HelpMessage = "Target parameter")]
        public object Target { get; set; }

        protected override void ProcessRecord()
        {
            base.ProcessRecord();

            var request = new DeleteRecordChangeHistory1Request();
            if (Target != null)
            {
                request.Target = DataverseTypeConverter.ToEntityReference(Target, null, "Target");
            }

            if (ShouldProcess("Executing DeleteRecordChangeHistory1Request", "DeleteRecordChangeHistory1Request"))
            {
                var response = (DeleteRecordChangeHistory1Response)Connection.Execute(request);
                WriteObject(response);
            }
        }
    }
}
