using System;
using System.Management.Automation;
using Microsoft.Crm.Sdk.Messages;
using Microsoft.PowerPlatform.Dataverse.Client;
using Microsoft.Xrm.Sdk;

namespace Rnwood.Dataverse.Data.PowerShell.Commands
{
    [Cmdlet(VerbsCommon.Get, "DataverseProductProperties")]
    [OutputType(typeof(RetrieveProductPropertiesResponse))]
    ///<summary>Executes RetrieveProductPropertiesRequest SDK message.</summary>
    public class GetDataverseProductPropertiesCmdlet : OrganizationServiceCmdlet
    {
        [Parameter(Mandatory = true, HelpMessage = "DataverseConnection instance obtained from Get-DataverseConnection cmdlet")]
        public override ServiceClient Connection { get; set; }
        [Parameter(Mandatory = false, ValueFromPipelineByPropertyName = true, HelpMessage = "ParentObject parameter")]
        public object ParentObject { get; set; }

        protected override void ProcessRecord()
        {
            base.ProcessRecord();

            var request = new RetrieveProductPropertiesRequest();
            if (ParentObject != null)
            {
                request.ParentObject = DataverseTypeConverter.ToEntityReference(ParentObject, null, "ParentObject");
            }

            var response = (RetrieveProductPropertiesResponse)Connection.Execute(request);
            WriteObject(response);
        }
    }
}
