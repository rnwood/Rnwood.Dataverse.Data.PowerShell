using System;
using System.Management.Automation;
using Microsoft.Crm.Sdk.Messages;
using Microsoft.PowerPlatform.Dataverse.Client;
using Microsoft.Xrm.Sdk;

namespace Rnwood.Dataverse.Data.PowerShell.Commands
{
    [Cmdlet(VerbsCommon.Get, "DataversePersonalWall")]
    [OutputType(typeof(RetrievePersonalWallResponse))]
    ///<summary>Executes RetrievePersonalWallRequest SDK message.</summary>
    public class GetDataversePersonalWallCmdlet : OrganizationServiceCmdlet
    {
        [Parameter(Mandatory = true, HelpMessage = "DataverseConnection instance obtained from Get-DataverseConnection cmdlet")]
        public override ServiceClient Connection { get; set; }
        [Parameter(Mandatory = false, HelpMessage = "PageNumber parameter")]
        public Int32 PageNumber { get; set; }
        [Parameter(Mandatory = false, HelpMessage = "PageSize parameter")]
        public Int32 PageSize { get; set; }
        [Parameter(Mandatory = false, HelpMessage = "CommentsPerPost parameter")]
        public Int32 CommentsPerPost { get; set; }
        [Parameter(Mandatory = false, HelpMessage = "StartDate parameter")]
        public DateTime StartDate { get; set; }
        [Parameter(Mandatory = false, HelpMessage = "EndDate parameter")]
        public DateTime EndDate { get; set; }
        [Parameter(Mandatory = false, HelpMessage = "Type parameter")]
        public object Type { get; set; }
        [Parameter(Mandatory = false, HelpMessage = "Source parameter")]
        public object Source { get; set; }
        [Parameter(Mandatory = false, HelpMessage = "SortDirection parameter")]
        public Boolean SortDirection { get; set; }
        [Parameter(Mandatory = false, HelpMessage = "Keyword parameter")]
        public String Keyword { get; set; }

        protected override void ProcessRecord()
        {
            base.ProcessRecord();

            var request = new RetrievePersonalWallRequest();
            request.PageNumber = PageNumber;            request.PageSize = PageSize;            request.CommentsPerPost = CommentsPerPost;            request.StartDate = StartDate;            request.EndDate = EndDate;            if (Type != null)
            {
                request.Type = DataverseTypeConverter.ToOptionSetValue(Type, "Type");
            }
            if (Source != null)
            {
                request.Source = DataverseTypeConverter.ToOptionSetValue(Source, "Source");
            }
            request.SortDirection = SortDirection;            request.Keyword = Keyword;
            var response = (RetrievePersonalWallResponse)Connection.Execute(request);
            WriteObject(response);
        }
    }
}
