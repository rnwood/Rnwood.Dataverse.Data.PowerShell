using System;
using System.Management.Automation;
using Microsoft.Crm.Sdk.Messages;
using Microsoft.PowerPlatform.Dataverse.Client;
using Microsoft.Xrm.Sdk;

namespace Rnwood.Dataverse.Data.PowerShell.Commands
{
    [Cmdlet(VerbsCommon.Close, "DataverseIncident", SupportsShouldProcess = true, ConfirmImpact = ConfirmImpact.Medium)]
    [OutputType(typeof(CloseIncidentResponse))]
    ///<summary>Executes CloseIncidentRequest SDK message.</summary>
    public class CloseDataverseIncidentCmdlet : OrganizationServiceCmdlet
    {
        [Parameter(Mandatory = true, HelpMessage = "DataverseConnection instance obtained from Get-DataverseConnection cmdlet")]
        public override ServiceClient Connection { get; set; }
        [Parameter(Mandatory = false, HelpMessage = "IncidentResolution parameter")]
        public PSObject IncidentResolution { get; set; }
        [Parameter(Mandatory = false, HelpMessage = "Status parameter")]
        public object Status { get; set; }

        protected override void ProcessRecord()
        {
            base.ProcessRecord();

            var request = new CloseIncidentRequest();
            if (IncidentResolution != null)
            {
                var entity = new Entity();
                foreach (PSPropertyInfo prop in IncidentResolution.Properties)
                {
                    if (prop.Name != "Id" && prop.Name != "TableName" && prop.Name != "LogicalName")
                    {
                        entity[prop.Name] = prop.Value;
                    }
                }
                request.IncidentResolution = entity;
            }
            if (Status != null)
            {
                request.Status = DataverseTypeConverter.ToOptionSetValue(Status, "Status");
            }

            if (ShouldProcess("Executing CloseIncidentRequest", "CloseIncidentRequest"))
            {
                var response = (CloseIncidentResponse)Connection.Execute(request);
                WriteObject(response);
            }
        }
    }
}
