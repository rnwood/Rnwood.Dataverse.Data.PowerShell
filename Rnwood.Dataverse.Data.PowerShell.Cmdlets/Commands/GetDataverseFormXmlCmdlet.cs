using System;
using System.Management.Automation;
using Microsoft.Crm.Sdk.Messages;
using Microsoft.PowerPlatform.Dataverse.Client;
using Microsoft.Xrm.Sdk;

namespace Rnwood.Dataverse.Data.PowerShell.Commands
{
    [Cmdlet(VerbsCommon.Get, "DataverseFormXml")]
    [OutputType(typeof(RetrieveFormXmlResponse))]
    ///<summary>Executes RetrieveFormXmlRequest SDK message.</summary>
    public class GetDataverseFormXmlCmdlet : OrganizationServiceCmdlet
    {
        [Parameter(Mandatory = true, HelpMessage = "DataverseConnection instance obtained from Get-DataverseConnection cmdlet")]
        public override ServiceClient Connection { get; set; }
        [Parameter(Mandatory = false, HelpMessage = "EntityName parameter")]
        public String EntityName { get; set; }

        protected override void ProcessRecord()
        {
            base.ProcessRecord();

            var request = new RetrieveFormXmlRequest();
            request.EntityName = EntityName;
            var response = (RetrieveFormXmlResponse)Connection.Execute(request);
            WriteObject(response);
        }
    }
}
