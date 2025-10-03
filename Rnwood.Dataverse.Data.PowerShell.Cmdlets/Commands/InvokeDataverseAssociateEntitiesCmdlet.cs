using System;
using System.Management.Automation;
using Microsoft.Crm.Sdk.Messages;
using Microsoft.PowerPlatform.Dataverse.Client;
using Microsoft.Xrm.Sdk;

namespace Rnwood.Dataverse.Data.PowerShell.Commands
{
    [Cmdlet(VerbsLifecycle.Invoke, "DataverseAssociateEntities")]
    [OutputType(typeof(AssociateEntitiesResponse))]
    ///<summary>Executes AssociateEntitiesRequest SDK message.</summary>
    public class InvokeDataverseAssociateEntitiesCmdlet : OrganizationServiceCmdlet
    {
        [Parameter(Mandatory = true, HelpMessage = "DataverseConnection instance obtained from Get-DataverseConnection cmdlet")]
        public override ServiceClient Connection { get; set; }
        [Parameter(Mandatory = false, ValueFromPipelineByPropertyName = true, HelpMessage = "Moniker1 parameter")]
        public object Moniker1 { get; set; }
        [Parameter(Mandatory = false, ValueFromPipelineByPropertyName = true, HelpMessage = "Moniker2 parameter")]
        public object Moniker2 { get; set; }
        [Parameter(Mandatory = false, HelpMessage = "RelationshipName parameter")]
        public String RelationshipName { get; set; }

        protected override void ProcessRecord()
        {
            base.ProcessRecord();

            var request = new AssociateEntitiesRequest();
            if (Moniker1 != null)
            {
                request.Moniker1 = DataverseTypeConverter.ToEntityReference(Moniker1, null, "Moniker1");
            }
            if (Moniker2 != null)
            {
                request.Moniker2 = DataverseTypeConverter.ToEntityReference(Moniker2, null, "Moniker2");
            }
            request.RelationshipName = RelationshipName;
            var response = (AssociateEntitiesResponse)Connection.Execute(request);
            WriteObject(response);
        }
    }
}
