using System;
using System.Management.Automation;
using Microsoft.Crm.Sdk.Messages;

namespace Rnwood.Dataverse.Data.PowerShell.Commands
{
    /// <summary>
    /// Publishes customizations in Dataverse.
    /// </summary>
    [Cmdlet(VerbsData.Publish, "DataverseCustomizations", SupportsShouldProcess = true, ConfirmImpact = ConfirmImpact.Medium)]
    public class PublishDataverseCustomizationsCmdlet : OrganizationServiceCmdlet
    {
        /// <summary>
        /// Gets or sets the specific entity logical name to publish. If not specified, all customizations are published.
        /// </summary>
        [Parameter(Position = 0, HelpMessage = "The logical name of the entity to publish. If not specified, all customizations are published.")]
        public string EntityName { get; set; }

        /// <summary>
        /// Processes the cmdlet request.
        /// </summary>
        protected override void ProcessRecord()
        {
            base.ProcessRecord();

            if (string.IsNullOrEmpty(EntityName))
            {
                if (!ShouldProcess("All customizations", "Publish"))
                {
                    return;
                }

                WriteVerbose("Publishing all customizations...");

                var request = new PublishAllXmlRequest();
                Connection.Execute(request);

                WriteVerbose("All customizations published successfully.");
            }
            else
            {
                if (!ShouldProcess($"Entity '{EntityName}'", "Publish"))
                {
                    return;
                }

                WriteVerbose($"Publishing customizations for entity '{EntityName}'...");

                var request = new PublishXmlRequest
                {
                    ParameterXml = $"<importexportxml><entities><entity>{EntityName}</entity></entities></importexportxml>"
                };

                Connection.Execute(request);

                WriteVerbose($"Customizations for entity '{EntityName}' published successfully.");
            }

            WriteObject("Customizations published successfully.");
        }
    }
}
