using System;
using System.Management.Automation;
using Microsoft.Crm.Sdk.Messages;
using Microsoft.PowerPlatform.Dataverse.Client;
using Microsoft.Xrm.Sdk;

namespace Rnwood.Dataverse.Data.PowerShell.Commands
{
    [Cmdlet(VerbsCommon.New, "DataverseInstance", SupportsShouldProcess = true, ConfirmImpact = ConfirmImpact.Medium)]
    [OutputType(typeof(CreateInstanceResponse))]
    ///<summary>Executes CreateInstanceRequest SDK message.</summary>
    public class NewDataverseInstanceCmdlet : OrganizationServiceCmdlet
    {
        [Parameter(Mandatory = true, HelpMessage = "DataverseConnection instance obtained from Get-DataverseConnection cmdlet")]
        public override ServiceClient Connection { get; set; }
        [Parameter(Mandatory = false, HelpMessage = "Target parameter")]
        public PSObject Target { get; set; }
        [Parameter(Mandatory = false, HelpMessage = "Count parameter")]
        public Int32 Count { get; set; }

        protected override void ProcessRecord()
        {
            base.ProcessRecord();

            var request = new CreateInstanceRequest();
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
            request.Count = Count;
            if (ShouldProcess("Executing CreateInstanceRequest", "CreateInstanceRequest"))
            {
                var response = (CreateInstanceResponse)Connection.Execute(request);
                WriteObject(response);
            }
        }
    }
}
