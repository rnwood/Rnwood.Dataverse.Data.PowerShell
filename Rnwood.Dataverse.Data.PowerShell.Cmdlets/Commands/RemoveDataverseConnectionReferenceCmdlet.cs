using System;
using System.Management.Automation;
using Microsoft.PowerPlatform.Dataverse.Client;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Query;

namespace Rnwood.Dataverse.Data.PowerShell.Commands
{
    /// <summary>
    /// Removes connection references from Dataverse.
    /// </summary>
    [Cmdlet(VerbsCommon.Remove, "DataverseConnectionReference", SupportsShouldProcess = true, ConfirmImpact = ConfirmImpact.High)]
    public class RemoveDataverseConnectionReferenceCmdlet : OrganizationServiceCmdlet
    {
        /// <summary>
        /// Gets or sets the logical name of the connection reference to remove.
        /// </summary>
        [Parameter(Mandatory = true, Position = 0, ValueFromPipeline = true, ValueFromPipelineByPropertyName = true, HelpMessage = "Logical name of the connection reference to remove.")]
        [ValidateNotNullOrEmpty]
        public string ConnectionReferenceLogicalName { get; set; }

        /// <summary>
        /// Processes the cmdlet request.
        /// </summary>
        protected override void ProcessRecord()
        {
            base.ProcessRecord();

            WriteVerbose($"Looking for connection reference '{ConnectionReferenceLogicalName}'");

            // Query for the connection reference
            var query = new QueryExpression("connectionreference")
            {
                ColumnSet = new ColumnSet("connectionreferenceid", "connectionreferencelogicalname", "connectionreferencedisplayname"),
                Criteria = new FilterExpression
                {
                    Conditions =
                    {
                        new ConditionExpression("connectionreferencelogicalname", ConditionOperator.Equal, ConnectionReferenceLogicalName)
                    }
                },
                TopCount = 1
            };

            var results = Connection.RetrieveMultiple(query);

            if (results.Entities.Count == 0)
            {
                WriteError(new ErrorRecord(
                    new InvalidOperationException($"Connection reference with logical name '{ConnectionReferenceLogicalName}' not found."),
                    "ConnectionReferenceNotFound",
                    ErrorCategory.ObjectNotFound,
                    ConnectionReferenceLogicalName));
                return;
            }

            var connRef = results.Entities[0];
            var connRefId = connRef.Id;
            var displayName = connRef.GetAttributeValue<string>("connectionreferencedisplayname");

            if (!ShouldProcess($"Connection reference '{ConnectionReferenceLogicalName}' ('{displayName}')", "Remove"))
            {
                return;
            }

            WriteVerbose($"Removing connection reference: '{displayName}' (ID: {connRefId})");
            Connection.Delete("connectionreference", connRefId);
            WriteVerbose($"Successfully removed connection reference '{ConnectionReferenceLogicalName}'");
        }
    }
}
