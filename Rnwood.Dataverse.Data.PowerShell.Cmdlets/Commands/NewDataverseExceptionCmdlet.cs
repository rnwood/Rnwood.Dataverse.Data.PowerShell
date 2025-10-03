using System;
using System.Management.Automation;
using Microsoft.Crm.Sdk.Messages;
using Microsoft.PowerPlatform.Dataverse.Client;
using Microsoft.Xrm.Sdk;

namespace Rnwood.Dataverse.Data.PowerShell.Commands
{
    [Cmdlet(VerbsCommon.New, "DataverseException", SupportsShouldProcess = true, ConfirmImpact = ConfirmImpact.Medium)]
    [OutputType(typeof(CreateExceptionResponse))]
    ///<summary>Executes CreateExceptionRequest SDK message.</summary>
    public class NewDataverseExceptionCmdlet : OrganizationServiceCmdlet
    {
        [Parameter(Mandatory = true, HelpMessage = "DataverseConnection instance obtained from Get-DataverseConnection cmdlet")]
        public override ServiceClient Connection { get; set; }
        [Parameter(Mandatory = false, HelpMessage = "Target parameter")]
        public PSObject Target { get; set; }
        [Parameter(Mandatory = false, HelpMessage = "OriginalStartDate parameter")]
        public DateTime OriginalStartDate { get; set; }
        [Parameter(Mandatory = false, HelpMessage = "IsDeleted parameter")]
        public Boolean IsDeleted { get; set; }

        protected override void ProcessRecord()
        {
            base.ProcessRecord();

            var request = new CreateExceptionRequest();
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
            request.OriginalStartDate = OriginalStartDate;            request.IsDeleted = IsDeleted;
            if (ShouldProcess("Executing CreateExceptionRequest", "CreateExceptionRequest"))
            {
                var response = (CreateExceptionResponse)Connection.Execute(request);
                WriteObject(response);
            }
        }
    }
}
