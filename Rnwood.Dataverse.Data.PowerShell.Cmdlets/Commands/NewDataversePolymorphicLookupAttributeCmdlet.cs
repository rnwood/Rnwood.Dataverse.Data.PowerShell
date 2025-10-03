using System;
using System.Management.Automation;
using Microsoft.Crm.Sdk.Messages;
using Microsoft.PowerPlatform.Dataverse.Client;
using Microsoft.Xrm.Sdk;

namespace Rnwood.Dataverse.Data.PowerShell.Commands
{
    [Cmdlet(VerbsCommon.New, "DataversePolymorphicLookupAttribute", SupportsShouldProcess = true, ConfirmImpact = ConfirmImpact.Medium)]
    [OutputType(typeof(CreatePolymorphicLookupAttributeResponse))]
    ///<summary>Executes CreatePolymorphicLookupAttributeRequest SDK message.</summary>
    public class NewDataversePolymorphicLookupAttributeCmdlet : OrganizationServiceCmdlet
    {
        [Parameter(Mandatory = true, HelpMessage = "DataverseConnection instance obtained from Get-DataverseConnection cmdlet")]
        public override ServiceClient Connection { get; set; }
        [Parameter(Mandatory = false, HelpMessage = "Lookup parameter")]
        public Microsoft.Xrm.Sdk.Metadata.LookupAttributeMetadata Lookup { get; set; }
        [Parameter(Mandatory = false, HelpMessage = "OneToManyRelationships parameter")]
        public Microsoft.Xrm.Sdk.Metadata.OneToManyRelationshipMetadata[] OneToManyRelationships { get; set; }
        [Parameter(Mandatory = false, HelpMessage = "SolutionUniqueName parameter")]
        public String SolutionUniqueName { get; set; }

        protected override void ProcessRecord()
        {
            base.ProcessRecord();

            var request = new CreatePolymorphicLookupAttributeRequest();
            request.Lookup = Lookup;            request.OneToManyRelationships = OneToManyRelationships;            request.SolutionUniqueName = SolutionUniqueName;
            if (ShouldProcess("Executing CreatePolymorphicLookupAttributeRequest", "CreatePolymorphicLookupAttributeRequest"))
            {
                var response = (CreatePolymorphicLookupAttributeResponse)Connection.Execute(request);
                WriteObject(response);
            }
        }
    }
}
