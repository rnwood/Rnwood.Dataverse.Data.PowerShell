using System;
using System.Management.Automation;
using Microsoft.Crm.Sdk.Messages;
using Microsoft.PowerPlatform.Dataverse.Client;
using Microsoft.Xrm.Sdk;

namespace Rnwood.Dataverse.Data.PowerShell.Commands
{
    [Cmdlet(VerbsData.Initialize, "DataverseFrom")]
    [OutputType(typeof(InitializeFromResponse))]
    ///<summary>Executes InitializeFromRequest SDK message.</summary>
    public class InitializeDataverseFromCmdlet : OrganizationServiceCmdlet
    {
        [Parameter(Mandatory = true, HelpMessage = "DataverseConnection instance obtained from Get-DataverseConnection cmdlet")]
        public override ServiceClient Connection { get; set; }
        [Parameter(Mandatory = false, ValueFromPipelineByPropertyName = true, HelpMessage = "EntityMoniker parameter")]
        public object EntityMoniker { get; set; }
        [Parameter(Mandatory = false, HelpMessage = "TargetEntityName parameter")]
        public String TargetEntityName { get; set; }
        [Parameter(Mandatory = false, HelpMessage = "TargetFieldType parameter")]
        public Microsoft.Crm.Sdk.Messages.TargetFieldType TargetFieldType { get; set; }
        [Parameter(Mandatory = false, HelpMessage = "SkipParentalRelationshipMapping parameter")]
        public Boolean SkipParentalRelationshipMapping { get; set; }

        protected override void ProcessRecord()
        {
            base.ProcessRecord();

            var request = new InitializeFromRequest();
            if (EntityMoniker != null)
            {
                request.EntityMoniker = DataverseTypeConverter.ToEntityReference(EntityMoniker, null, "EntityMoniker");
            }
            request.TargetEntityName = TargetEntityName;            request.TargetFieldType = TargetFieldType;            request.SkipParentalRelationshipMapping = SkipParentalRelationshipMapping;
            var response = (InitializeFromResponse)Connection.Execute(request);
            WriteObject(response);
        }
    }
}
