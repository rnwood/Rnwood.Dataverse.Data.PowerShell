using System;
using System.Management.Automation;
using Microsoft.Crm.Sdk.Messages;
using Microsoft.PowerPlatform.Dataverse.Client;
using Microsoft.Xrm.Sdk;

namespace Rnwood.Dataverse.Data.PowerShell.Commands
{
    [Cmdlet(VerbsCommon.Set, "DataverseProcess", SupportsShouldProcess = true, ConfirmImpact = ConfirmImpact.Medium)]
    [OutputType(typeof(SetProcessResponse))]
    ///<summary>Executes SetProcessRequest SDK message.</summary>
    public class SetDataverseProcessCmdlet : OrganizationServiceCmdlet
    {
        [Parameter(Mandatory = true, HelpMessage = "DataverseConnection instance obtained from Get-DataverseConnection cmdlet")]
        public override ServiceClient Connection { get; set; }
        [Parameter(Mandatory = false, ValueFromPipelineByPropertyName = true, HelpMessage = "Target parameter")]
        public object Target { get; set; }
        [Parameter(Mandatory = false, ValueFromPipelineByPropertyName = true, HelpMessage = "NewProcess parameter")]
        public object NewProcess { get; set; }
        [Parameter(Mandatory = false, ValueFromPipelineByPropertyName = true, HelpMessage = "NewProcessInstance parameter")]
        public object NewProcessInstance { get; set; }

        protected override void ProcessRecord()
        {
            base.ProcessRecord();

            var request = new SetProcessRequest();
            if (Target != null)
            {
                request.Target = DataverseTypeConverter.ToEntityReference(Target, null, "Target");
            }
            if (NewProcess != null)
            {
                request.NewProcess = DataverseTypeConverter.ToEntityReference(NewProcess, null, "NewProcess");
            }
            if (NewProcessInstance != null)
            {
                request.NewProcessInstance = DataverseTypeConverter.ToEntityReference(NewProcessInstance, null, "NewProcessInstance");
            }

            if (ShouldProcess("Executing SetProcessRequest", "SetProcessRequest"))
            {
                var response = (SetProcessResponse)Connection.Execute(request);
                WriteObject(response);
            }
        }
    }
}
