using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Management.Automation;
using Microsoft.PowerPlatform.Dataverse.Client;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Query;

namespace Rnwood.Dataverse.Data.PowerShell.Commands
{
    /// <summary>
    /// Sets connection reference values in Dataverse. Can set a single connection reference or multiple connection references at once.
    /// </summary>
    [Cmdlet(VerbsCommon.Set, "DataverseConnectionReference", SupportsShouldProcess = true, ConfirmImpact = ConfirmImpact.Medium, DefaultParameterSetName = "Single")]
    public class SetDataverseConnectionReferenceCmdlet : OrganizationServiceCmdlet
    {
        /// <summary>
        /// Gets or sets the logical name of the connection reference to set (for single parameter set).
        /// </summary>
        [Parameter(Mandatory = true, Position = 0, ParameterSetName = "Single", HelpMessage = "Logical name of the connection reference to set.")]
        [ValidateNotNullOrEmpty]
        public string ConnectionReferenceLogicalName { get; set; }

        /// <summary>
        /// Gets or sets the connection ID to set for the connection reference (for single parameter set).
        /// </summary>
        [Parameter(Mandatory = true, Position = 1, ParameterSetName = "Single", HelpMessage = "Connection ID (GUID) to set for the connection reference.")]
        [ValidateNotNullOrEmpty]
        public string ConnectionId { get; set; }

        /// <summary>
        /// Gets or sets connection references as a hashtable (for multiple parameter set).
        /// </summary>
        [Parameter(Mandatory = true, ParameterSetName = "Multiple", HelpMessage = "Hashtable of connection reference logical names to connection IDs (e.g., @{'new_sharedconnectionref' = '00000000-0000-0000-0000-000000000000'}).")]
        [ValidateNotNullOrEmpty]
        public Hashtable ConnectionReferences { get; set; }

        /// <summary>
        /// Processes the cmdlet request.
        /// </summary>
        protected override void ProcessRecord()
        {
            base.ProcessRecord();

            Dictionary<string, string> referencesToSet = new Dictionary<string, string>();

            // Build the list of references to set based on parameter set
            if (ParameterSetName == "Single")
            {
                if (!ShouldProcess($"Connection reference '{ConnectionReferenceLogicalName}'", "Set"))
                {
                    return;
                }

                referencesToSet[ConnectionReferenceLogicalName] = ConnectionId;
            }
            else // Multiple
            {
                if (!ShouldProcess($"{ConnectionReferences.Count} connection reference(s)", "Set"))
                {
                    return;
                }

                foreach (DictionaryEntry entry in ConnectionReferences)
                {
                    var logicalName = entry.Key.ToString();
                    var connectionId = entry.Value.ToString();
                    referencesToSet[logicalName] = connectionId;
                }
            }

            WriteVerbose($"Setting {referencesToSet.Count} connection reference(s)...");

            // Process each connection reference
            foreach (var kvp in referencesToSet)
            {
                var logicalName = kvp.Key;
                var connectionId = kvp.Value;

                WriteVerbose($"Setting connection reference '{logicalName}' to connection '{connectionId}'");

                // Query for the connection reference by logical name
                var query = new QueryExpression("connectionreference")
                {
                    ColumnSet = new ColumnSet("connectionreferenceid", "connectionreferencelogicalname", "connectionreferencedisplayname", "connectionid"),
                    Criteria = new FilterExpression
                    {
                        Conditions =
                        {
                            new ConditionExpression("connectionreferencelogicalname", ConditionOperator.Equal, logicalName)
                        }
                    },
                    TopCount = 1
                };

                var results = Connection.RetrieveMultiple(query);

                if (results.Entities.Count == 0)
                {
                    WriteError(new ErrorRecord(
                        new InvalidOperationException($"Connection reference with logical name '{logicalName}' not found."),
                        "ConnectionReferenceNotFound",
                        ErrorCategory.ObjectNotFound,
                        logicalName));
                    continue;
                }

                var connRef = results.Entities[0];
                var connRefId = connRef.Id;
                var displayName = connRef.GetAttributeValue<string>("connectionreferencedisplayname");
                var currentConnectionId = connRef.GetAttributeValue<string>("connectionid");

                WriteVerbose($"  Found connection reference: '{displayName}' (ID: {connRefId})");
                WriteVerbose($"  Current connection ID: {currentConnectionId ?? "(none)"}");

                // Update the connection reference
                var updateEntity = new Entity("connectionreference", connRefId);
                updateEntity["connectionid"] = connectionId;

                Connection.Update(updateEntity);
                WriteVerbose($"  Successfully updated connection reference '{logicalName}'");

                // Output the result
                var result = new PSObject();
                result.Properties.Add(new PSNoteProperty("ConnectionReferenceLogicalName", logicalName));
                result.Properties.Add(new PSNoteProperty("ConnectionId", connectionId));
                result.Properties.Add(new PSNoteProperty("ConnectionReferenceId", connRefId));
                result.Properties.Add(new PSNoteProperty("PreviousConnectionId", currentConnectionId));
                WriteObject(result);
            }
        }
    }
}
