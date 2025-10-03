using System;
using System.Management.Automation;
using Microsoft.Crm.Sdk.Messages;
using Microsoft.PowerPlatform.Dataverse.Client;
using Microsoft.Xrm.Sdk;

namespace Rnwood.Dataverse.Data.PowerShell.Commands
{
    [Cmdlet(VerbsCommon.Reset, "DataverseUserFilters")]
    [OutputType(typeof(ResetUserFiltersResponse))]
    ///<summary>Executes ResetUserFiltersRequest SDK message.</summary>
    public class ResetDataverseUserFiltersCmdlet : OrganizationServiceCmdlet
    {
        [Parameter(Mandatory = true, HelpMessage = "DataverseConnection instance obtained from Get-DataverseConnection cmdlet")]
        public override ServiceClient Connection { get; set; }
        [Parameter(Mandatory = false, HelpMessage = "QueryType parameter")]
        public Int32 QueryType { get; set; }

        protected override void ProcessRecord()
        {
            base.ProcessRecord();

            var request = new ResetUserFiltersRequest();
            request.QueryType = QueryType;
            var response = (ResetUserFiltersResponse)Connection.Execute(request);
            WriteObject(response);
        }
    }
}
