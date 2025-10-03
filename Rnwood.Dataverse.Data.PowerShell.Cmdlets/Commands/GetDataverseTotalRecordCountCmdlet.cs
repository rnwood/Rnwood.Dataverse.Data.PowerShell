using System;
using System.Management.Automation;
using Microsoft.Crm.Sdk.Messages;
using Microsoft.PowerPlatform.Dataverse.Client;
using Microsoft.Xrm.Sdk;

namespace Rnwood.Dataverse.Data.PowerShell.Commands
{
    [Cmdlet(VerbsCommon.Get, "DataverseTotalRecordCount")]
    [OutputType(typeof(RetrieveTotalRecordCountResponse))]
    ///<summary>Executes RetrieveTotalRecordCountRequest SDK message.</summary>
    public class GetDataverseTotalRecordCountCmdlet : OrganizationServiceCmdlet
    {
        [Parameter(Mandatory = true, HelpMessage = "DataverseConnection instance obtained from Get-DataverseConnection cmdlet")]
        public override ServiceClient Connection { get; set; }
        [Parameter(Mandatory = false, HelpMessage = "EntityNames parameter")]
        public String[] EntityNames { get; set; }

        protected override void ProcessRecord()
        {
            base.ProcessRecord();

            var request = new RetrieveTotalRecordCountRequest();
            request.EntityNames = EntityNames;
            var response = (RetrieveTotalRecordCountResponse)Connection.Execute(request);
            WriteObject(response);
        }
    }
}
