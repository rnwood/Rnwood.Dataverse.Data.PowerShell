using System;
using System.Management.Automation;
using Microsoft.Crm.Sdk.Messages;
using Microsoft.PowerPlatform.Dataverse.Client;
using Microsoft.Xrm.Sdk;

namespace Rnwood.Dataverse.Data.PowerShell.Commands
{
    [Cmdlet(VerbsCommon.Get, "DataverseDefaultPriceLevel")]
    [OutputType(typeof(GetDefaultPriceLevelResponse))]
    ///<summary>Executes GetDefaultPriceLevelRequest SDK message.</summary>
    public class GetDataverseDefaultPriceLevelCmdlet : OrganizationServiceCmdlet
    {
        [Parameter(Mandatory = true, HelpMessage = "DataverseConnection instance obtained from Get-DataverseConnection cmdlet")]
        public override ServiceClient Connection { get; set; }
        [Parameter(Mandatory = false, HelpMessage = "EntityName parameter")]
        public String EntityName { get; set; }
        [Parameter(Mandatory = false, HelpMessage = "ColumnSet parameter")]
        public Microsoft.Xrm.Sdk.Query.ColumnSet ColumnSet { get; set; }

        protected override void ProcessRecord()
        {
            base.ProcessRecord();

            var request = new GetDefaultPriceLevelRequest();
            request.EntityName = EntityName;            request.ColumnSet = ColumnSet;
            var response = (GetDefaultPriceLevelResponse)Connection.Execute(request);
            WriteObject(response);
        }
    }
}
