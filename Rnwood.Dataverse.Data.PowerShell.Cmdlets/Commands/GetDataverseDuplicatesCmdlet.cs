using System;
using System.Management.Automation;
using Microsoft.Crm.Sdk.Messages;
using Microsoft.PowerPlatform.Dataverse.Client;
using Microsoft.Xrm.Sdk;

namespace Rnwood.Dataverse.Data.PowerShell.Commands
{
    [Cmdlet(VerbsCommon.Get, "DataverseDuplicates")]
    [OutputType(typeof(RetrieveDuplicatesResponse))]
    ///<summary>Executes RetrieveDuplicatesRequest SDK message.</summary>
    public class GetDataverseDuplicatesCmdlet : OrganizationServiceCmdlet
    {
        [Parameter(Mandatory = true, HelpMessage = "DataverseConnection instance obtained from Get-DataverseConnection cmdlet")]
        public override ServiceClient Connection { get; set; }
        [Parameter(Mandatory = false, HelpMessage = "BusinessEntity parameter")]
        public PSObject BusinessEntity { get; set; }
        [Parameter(Mandatory = false, HelpMessage = "MatchingEntityName parameter")]
        public String MatchingEntityName { get; set; }
        [Parameter(ParameterSetName = "PagingInfoObject", Mandatory = false, HelpMessage = "PagingInfo SDK object for pagination")]
        public Microsoft.Xrm.Sdk.Query.PagingInfo PagingInfo { get; set; }

        [Parameter(ParameterSetName = "SimplePaging", Mandatory = false, HelpMessage = "Page number to retrieve (1-based)")]
        public int PageNumber { get; set; } = 1;

        [Parameter(ParameterSetName = "SimplePaging", Mandatory = false, HelpMessage = "Number of records per page")]
        public int PageSize { get; set; } = 5000;

        protected override void ProcessRecord()
        {
            base.ProcessRecord();

            var request = new RetrieveDuplicatesRequest();
            if (BusinessEntity != null)
            {
                var entity = new Entity();
                foreach (PSPropertyInfo prop in BusinessEntity.Properties)
                {
                    if (prop.Name != "Id" && prop.Name != "TableName" && prop.Name != "LogicalName")
                    {
                        entity[prop.Name] = prop.Value;
                    }
                }
                request.BusinessEntity = entity;
            }
            request.MatchingEntityName = MatchingEntityName;            
            
            // Handle PowerShell-friendly parameter sets
            if (ParameterSetName == "SimplePaging")
            {
                request.PagingInfo = DataverseComplexTypeConverter.ToPagingInfo(PageNumber, PageSize);
            }
            else
            {
                request.PagingInfo = PagingInfo;
            }
            
            var response = (RetrieveDuplicatesResponse)Connection.Execute(request);
            WriteObject(response);
        }
    }
}
