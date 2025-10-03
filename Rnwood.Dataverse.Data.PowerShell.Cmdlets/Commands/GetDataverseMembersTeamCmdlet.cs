using System;
using System.Management.Automation;
using Microsoft.Crm.Sdk.Messages;
using Microsoft.PowerPlatform.Dataverse.Client;
using Microsoft.Xrm.Sdk;

namespace Rnwood.Dataverse.Data.PowerShell.Commands
{
    [Cmdlet(VerbsCommon.Get, "DataverseMembersTeam")]
    [OutputType(typeof(RetrieveMembersTeamResponse))]
    ///<summary>Executes RetrieveMembersTeamRequest SDK message.</summary>
    public class GetDataverseMembersTeamCmdlet : OrganizationServiceCmdlet
    {
        [Parameter(Mandatory = true, HelpMessage = "DataverseConnection instance obtained from Get-DataverseConnection cmdlet")]
        public override ServiceClient Connection { get; set; }
        [Parameter(Mandatory = false, HelpMessage = "EntityId parameter")]
        public Guid EntityId { get; set; }
        [Parameter(ParameterSetName = "ColumnSetObject", Mandatory = false, HelpMessage = "ColumnSet SDK object for specifying columns")]
        public Microsoft.Xrm.Sdk.Query.ColumnSet MemberColumnSet { get; set; }

        [Parameter(ParameterSetName = "Columns", Mandatory = true, HelpMessage = "Array of column logical names to retrieve")]
        public string[] Columns { get; set; }

        [Parameter(ParameterSetName = "AllColumns", Mandatory = true, HelpMessage = "Retrieve all columns")]
        public SwitchParameter AllColumns { get; set; }

        protected override void ProcessRecord()
        {
            base.ProcessRecord();

            var request = new RetrieveMembersTeamRequest();
            request.EntityId = EntityId;            
            
            // Handle PowerShell-friendly parameter sets
            if (ParameterSetName == "Columns" || ParameterSetName == "AllColumns")
            {
                request.MemberColumnSet = DataverseComplexTypeConverter.ToColumnSet(Columns, AllColumns.IsPresent);
            }
            else
            {
                request.MemberColumnSet = MemberColumnSet;
            }
            
            var response = (RetrieveMembersTeamResponse)Connection.Execute(request);
            WriteObject(response);
        }
    }
}
