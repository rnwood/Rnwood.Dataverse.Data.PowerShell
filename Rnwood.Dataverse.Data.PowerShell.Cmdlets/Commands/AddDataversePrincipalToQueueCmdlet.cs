using System;
using System.Management.Automation;
using Microsoft.Crm.Sdk.Messages;
using Microsoft.PowerPlatform.Dataverse.Client;
using Microsoft.Xrm.Sdk;

namespace Rnwood.Dataverse.Data.PowerShell.Commands
{
    [Cmdlet(VerbsCommon.Add, "DataversePrincipalToQueue", SupportsShouldProcess = true, ConfirmImpact = ConfirmImpact.Medium)]
    [OutputType(typeof(AddPrincipalToQueueResponse))]
    ///<summary>Executes AddPrincipalToQueueRequest SDK message.</summary>
    public class AddDataversePrincipalToQueueCmdlet : OrganizationServiceCmdlet
    {
        [Parameter(Mandatory = true, HelpMessage = "DataverseConnection instance obtained from Get-DataverseConnection cmdlet")]
        public override ServiceClient Connection { get; set; }
        [Parameter(Mandatory = false, HelpMessage = "QueueId parameter")]
        public Guid QueueId { get; set; }
        [Parameter(Mandatory = false, HelpMessage = "Principal parameter")]
        public PSObject Principal { get; set; }

        protected override void ProcessRecord()
        {
            base.ProcessRecord();

            var request = new AddPrincipalToQueueRequest();
            request.QueueId = QueueId;            if (Principal != null)
            {
                var entity = new Entity();
                foreach (PSPropertyInfo prop in Principal.Properties)
                {
                    if (prop.Name != "Id" && prop.Name != "TableName" && prop.Name != "LogicalName")
                    {
                        entity[prop.Name] = prop.Value;
                    }
                }
                request.Principal = entity;
            }

            if (ShouldProcess("Executing AddPrincipalToQueueRequest", "AddPrincipalToQueueRequest"))
            {
                var response = (AddPrincipalToQueueResponse)Connection.Execute(request);
                WriteObject(response);
            }
        }
    }
}
