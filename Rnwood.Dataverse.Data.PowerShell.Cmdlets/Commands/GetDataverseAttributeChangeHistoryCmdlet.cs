using System;
using System.Management.Automation;
using Microsoft.Crm.Sdk.Messages;
using Microsoft.PowerPlatform.Dataverse.Client;
using Microsoft.Xrm.Sdk;

namespace Rnwood.Dataverse.Data.PowerShell.Commands
{
    [Cmdlet(VerbsCommon.Get, "DataverseAttributeChangeHistory")]
    [OutputType(typeof(RetrieveAttributeChangeHistoryResponse))]
    ///<summary>Executes RetrieveAttributeChangeHistoryRequest SDK message.</summary>
    public class GetDataverseAttributeChangeHistoryCmdlet : OrganizationServiceCmdlet
    {
        [Parameter(Mandatory = true, HelpMessage = "DataverseConnection instance obtained from Get-DataverseConnection cmdlet")]
        public override ServiceClient Connection { get; set; }
        [Parameter(Mandatory = false, ValueFromPipelineByPropertyName = true, HelpMessage = "Target parameter")]
        public object Target { get; set; }
        [Parameter(Mandatory = false, HelpMessage = "AttributeLogicalName parameter")]
        public String AttributeLogicalName { get; set; }
        [Parameter(Mandatory = false, HelpMessage = "PagingInfo parameter")]
        public Microsoft.Xrm.Sdk.Query.PagingInfo PagingInfo { get; set; }

        protected override void ProcessRecord()
        {
            base.ProcessRecord();

            var request = new RetrieveAttributeChangeHistoryRequest();
            if (Target != null)
            {
                request.Target = DataverseTypeConverter.ToEntityReference(Target, null, "Target");
            }
            request.AttributeLogicalName = AttributeLogicalName;            request.PagingInfo = PagingInfo;
            var response = (RetrieveAttributeChangeHistoryResponse)Connection.Execute(request);
            WriteObject(response);
        }
    }
}
