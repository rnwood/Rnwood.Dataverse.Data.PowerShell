using System;
using System.Management.Automation;
using Microsoft.Crm.Sdk.Messages;
using Microsoft.PowerPlatform.Dataverse.Client;
using Microsoft.Xrm.Sdk;

namespace Rnwood.Dataverse.Data.PowerShell.Commands
{
    [Cmdlet(VerbsCommon.Copy, "DataverseSystemForm", SupportsShouldProcess = true, ConfirmImpact = ConfirmImpact.Medium)]
    [OutputType(typeof(CopySystemFormResponse))]
    ///<summary>Executes CopySystemFormRequest SDK message.</summary>
    public class CopyDataverseSystemFormCmdlet : OrganizationServiceCmdlet
    {
        [Parameter(Mandatory = true, HelpMessage = "DataverseConnection instance obtained from Get-DataverseConnection cmdlet")]
        public override ServiceClient Connection { get; set; }
        [Parameter(Mandatory = false, HelpMessage = "Target parameter")]
        public PSObject Target { get; set; }
        [Parameter(Mandatory = false, HelpMessage = "SourceId parameter")]
        public Guid SourceId { get; set; }

        protected override void ProcessRecord()
        {
            base.ProcessRecord();

            var request = new CopySystemFormRequest();
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
            request.SourceId = SourceId;
            if (ShouldProcess("Executing CopySystemFormRequest", "CopySystemFormRequest"))
            {
                var response = (CopySystemFormResponse)Connection.Execute(request);
                WriteObject(response);
            }
        }
    }
}
