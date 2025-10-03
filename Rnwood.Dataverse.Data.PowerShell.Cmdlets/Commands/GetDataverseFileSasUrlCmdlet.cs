using System;
using System.Management.Automation;
using Microsoft.Crm.Sdk.Messages;
using Microsoft.PowerPlatform.Dataverse.Client;
using Microsoft.Xrm.Sdk;

namespace Rnwood.Dataverse.Data.PowerShell.Commands
{
    [Cmdlet(VerbsCommon.Get, "DataverseFileSasUrl")]
    [OutputType(typeof(GetFileSasUrlResponse))]
    ///<summary>Executes GetFileSasUrlRequest SDK message.</summary>
    public class GetDataverseFileSasUrlCmdlet : OrganizationServiceCmdlet
    {
        [Parameter(Mandatory = true, HelpMessage = "DataverseConnection instance obtained from Get-DataverseConnection cmdlet")]
        public override ServiceClient Connection { get; set; }
        [Parameter(Mandatory = false, ValueFromPipelineByPropertyName = true, HelpMessage = "Target parameter")]
        public object Target { get; set; }
        [Parameter(Mandatory = false, HelpMessage = "FileAttributeName parameter")]
        public String FileAttributeName { get; set; }
        [Parameter(Mandatory = false, HelpMessage = "DataSource parameter")]
        public String DataSource { get; set; }

        protected override void ProcessRecord()
        {
            base.ProcessRecord();

            var request = new GetFileSasUrlRequest();
            if (Target != null)
            {
                request.Target = DataverseTypeConverter.ToEntityReference(Target, null, "Target");
            }
            request.FileAttributeName = FileAttributeName;            request.DataSource = DataSource;
            var response = (GetFileSasUrlResponse)Connection.Execute(request);
            WriteObject(response);
        }
    }
}
