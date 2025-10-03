using System;
using System.Management.Automation;
using Microsoft.Crm.Sdk.Messages;
using Microsoft.PowerPlatform.Dataverse.Client;
using Microsoft.Xrm.Sdk;

namespace Rnwood.Dataverse.Data.PowerShell.Commands
{
    [Cmdlet(VerbsCommon.Remove, "DataverseOpenInstances", SupportsShouldProcess = true, ConfirmImpact = ConfirmImpact.Medium)]
    [OutputType(typeof(DeleteOpenInstancesResponse))]
    ///<summary>Executes DeleteOpenInstancesRequest SDK message.</summary>
    public class RemoveDataverseOpenInstancesCmdlet : OrganizationServiceCmdlet
    {
        [Parameter(Mandatory = true, HelpMessage = "DataverseConnection instance obtained from Get-DataverseConnection cmdlet")]
        public override ServiceClient Connection { get; set; }
        [Parameter(Mandatory = false, HelpMessage = "Target parameter")]
        public PSObject Target { get; set; }
        [Parameter(Mandatory = false, HelpMessage = "SeriesEndDate parameter")]
        public DateTime SeriesEndDate { get; set; }
        [Parameter(Mandatory = false, HelpMessage = "StateOfPastInstances parameter")]
        public Int32 StateOfPastInstances { get; set; }

        protected override void ProcessRecord()
        {
            base.ProcessRecord();

            var request = new DeleteOpenInstancesRequest();
            if (Target != null)
            {
                var entity = new Entity();
                foreach (PSPropertyInfo prop in Target.Properties)
                {
                    if (prop.Name != "Id" && prop.Name != "TableName" && prop.Name != "LogicalName")
                    {
                        entity[prop.Name] = prop.Value;
                    }
                }
                request.Target = entity;
            }
            request.SeriesEndDate = SeriesEndDate;            request.StateOfPastInstances = StateOfPastInstances;
            if (ShouldProcess("Executing DeleteOpenInstancesRequest", "DeleteOpenInstancesRequest"))
            {
                var response = (DeleteOpenInstancesResponse)Connection.Execute(request);
                WriteObject(response);
            }
        }
    }
}
