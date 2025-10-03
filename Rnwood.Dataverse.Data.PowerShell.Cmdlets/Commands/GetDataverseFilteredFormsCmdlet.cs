using System;
using System.Management.Automation;
using Microsoft.Crm.Sdk.Messages;
using Microsoft.PowerPlatform.Dataverse.Client;
using Microsoft.Xrm.Sdk;

namespace Rnwood.Dataverse.Data.PowerShell.Commands
{
    [Cmdlet(VerbsCommon.Get, "DataverseFilteredForms")]
    [OutputType(typeof(RetrieveFilteredFormsResponse))]
    ///<summary>Executes RetrieveFilteredFormsRequest SDK message.</summary>
    public class GetDataverseFilteredFormsCmdlet : OrganizationServiceCmdlet
    {
        [Parameter(Mandatory = true, HelpMessage = "DataverseConnection instance obtained from Get-DataverseConnection cmdlet")]
        public override ServiceClient Connection { get; set; }
        [Parameter(Mandatory = false, HelpMessage = "EntityLogicalName parameter")]
        public String EntityLogicalName { get; set; }
        [Parameter(Mandatory = false, HelpMessage = "FormType parameter")]
        public object FormType { get; set; }
        [Parameter(Mandatory = false, HelpMessage = "SystemUserId parameter")]
        public Guid SystemUserId { get; set; }

        protected override void ProcessRecord()
        {
            base.ProcessRecord();

            var request = new RetrieveFilteredFormsRequest();
            request.EntityLogicalName = EntityLogicalName;            if (FormType != null)
            {
                request.FormType = DataverseTypeConverter.ToOptionSetValue(FormType, "FormType");
            }
            request.SystemUserId = SystemUserId;
            var response = (RetrieveFilteredFormsResponse)Connection.Execute(request);
            WriteObject(response);
        }
    }
}
