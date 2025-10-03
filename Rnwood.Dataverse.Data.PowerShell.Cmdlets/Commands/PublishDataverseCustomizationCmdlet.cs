using System;
using System.Management.Automation;
using Microsoft.Crm.Sdk.Messages;
using Microsoft.PowerPlatform.Dataverse.Client;

namespace Rnwood.Dataverse.Data.PowerShell.Commands
{
    [Cmdlet(VerbsData.Publish, "DataverseCustomization")]
    [OutputType(typeof(PublishXmlResponse))]
    ///<summary>Publishes customizations in Dataverse.</summary>
    public class PublishDataverseCustomizationCmdlet : OrganizationServiceCmdlet
    {
        [Parameter(Mandatory = true, HelpMessage = "DataverseConnection instance obtained from Get-DataverseConnection cmdlet")]
        public override ServiceClient Connection { get; set; }

        [Parameter(Mandatory = false, Position = 0, HelpMessage = "XML string specifying which customizations to publish. If not specified, publishes all customizations.")]
        public string ParameterXml { get; set; } = "<importexportxml><entities/><optionsets/><webresources/><customcontrols/><entitymaps/><entityrelationships/><ribbons/><sitemap/></importexportxml>";

        protected override void ProcessRecord()
        {
            base.ProcessRecord();

            PublishXmlRequest request = new PublishXmlRequest
            {
                ParameterXml = ParameterXml
            };

            WriteVerbose($"Publishing customizations with XML: {ParameterXml}");

            PublishXmlResponse response = (PublishXmlResponse)Connection.Execute(request);
            WriteObject(response);
        }
    }
}
