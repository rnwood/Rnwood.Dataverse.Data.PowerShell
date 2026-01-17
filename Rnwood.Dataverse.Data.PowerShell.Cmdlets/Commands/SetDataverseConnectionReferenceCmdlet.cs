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
        /// Gets or sets the connector ID for the connection reference (for single parameter set).
        /// </summary>
        [Parameter(Position = 2, ParameterSetName = "Single", HelpMessage = "Connector ID (GUID) that defines the type of connection. Required when creating new connection references.")]
        public string ConnectorId { get; set; }

        /// <summary>
        /// Gets or sets the display name for the connection reference (for single parameter set).
        /// </summary>
        [Parameter(ParameterSetName = "Single", HelpMessage = "Display name for the connection reference.")]
        public string DisplayName { get; set; }

        /// <summary>
        /// Gets or sets the description for the connection reference (for single parameter set).
        /// </summary>
        [Parameter(ParameterSetName = "Single", HelpMessage = "Description for the connection reference.")]
        public string Description { get; set; }

        /// <summary>
        /// Gets or sets connection references as a hashtable (for multiple parameter set).
        /// </summary>
        [Parameter(Mandatory = true, ParameterSetName = "Multiple", HelpMessage = "Hashtable of connection reference logical names or connector names to connection IDs (e.g., @{'new_sharedconnectionref' = '00000000-0000-0000-0000-000000000000'} or @{'shared_sharepointonline' = '00000000-0000-0000-0000-000000000000'}). Keys can be either specific connection reference logical names or connector names (value after last '/' in connector ID). Connection references without a specific logical name mapping will fall back to checking by connector name.")]
        [ValidateNotNullOrEmpty]
        public Hashtable ConnectionReferences { get; set; }

        /// <summary>
        /// Processes the cmdlet request.
        /// </summary>
        protected override void ProcessRecord()
        {
            base.ProcessRecord();

            Dictionary<string, ConnectionReferenceInfo> referencesToSet = new Dictionary<string, ConnectionReferenceInfo>();

            // Build the list of references to set based on parameter set
            if (ParameterSetName == "Single")
            {
                if (!ShouldProcess($"Connection reference '{ConnectionReferenceLogicalName}'", "Set"))
                {
                    return;
                }

                referencesToSet[ConnectionReferenceLogicalName] = new ConnectionReferenceInfo
                {
                    ConnectionId = ConnectionId,
                    ConnectorId = ConnectorId,
                    DisplayName = DisplayName,
                    Description = Description
                };
            }
            else // Multiple
            {
                if (!ShouldProcess($"{ConnectionReferences.Count} connection reference(s)", "Set"))
                {
                    return;
                }

                // Query all connection references to get their connector IDs for connector name fallback
                WriteVerbose("Querying all connection references to support connector name fallback...");
                var allConnRefsQuery = new QueryExpression("connectionreference")
                {
                    ColumnSet = new ColumnSet("connectionreferenceid", "connectionreferencelogicalname", "connectorid")
                };
                
                var allConnRefs = Connection.RetrieveMultiple(allConnRefsQuery);
                var connectorIdsByLogicalName = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
                var logicalNamesByConnRefId = new Dictionary<Guid, string>();
                
                foreach (var connRef in allConnRefs.Entities)
                {
                    var logicalName = connRef.GetAttributeValue<string>("connectionreferencelogicalname");
                    var connectorId = connRef.GetAttributeValue<string>("connectorid");
                    var connRefId = connRef.Id;
                    
                    if (!string.IsNullOrEmpty(logicalName))
                    {
                        logicalNamesByConnRefId[connRefId] = logicalName;
                        
                        if (!string.IsNullOrEmpty(connectorId))
                        {
                            connectorIdsByLogicalName[logicalName] = connectorId;
                        }
                    }
                }
                
                WriteVerbose($"Found {allConnRefs.Entities.Count} connection reference(s) in the environment");
                
                // Track which connection references have been mapped to avoid duplicates
                var mappedConnectionReferences = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
                
                foreach (DictionaryEntry entry in ConnectionReferences)
                {
                    var providedKey = entry.Key.ToString();
                    var connectionId = entry.Value.ToString();
                    
                    // Check if the provided key matches a connection reference logical name directly
                    var matchingLogicalName = logicalNamesByConnRefId.Values.FirstOrDefault(ln =>
                        string.Equals(ln, providedKey, StringComparison.OrdinalIgnoreCase));
                    
                    if (matchingLogicalName != null)
                    {
                        // Direct match by logical name
                        if (!mappedConnectionReferences.Contains(matchingLogicalName))
                        {
                            referencesToSet[matchingLogicalName] = new ConnectionReferenceInfo
                            {
                                ConnectionId = connectionId
                            };
                            mappedConnectionReferences.Add(matchingLogicalName);
                            WriteVerbose($"Mapping connection reference '{matchingLogicalName}' by logical name");
                        }
                    }
                    else
                    {
                        // Check if the provided key matches a connector name (fallback)
                        var matchingConnRefs = connectorIdsByLogicalName
                            .Where(kvp => DoesKeyMatchConnectorName(providedKey, kvp.Value))
                            .Select(kvp => kvp.Key)
                            .ToList();
                        
                        if (matchingConnRefs.Count > 0)
                        {
                            // Found connection reference(s) with this connector name
                            foreach (var connRefLogicalName in matchingConnRefs)
                            {
                                // Only map if not already mapped by logical name
                                if (!mappedConnectionReferences.Contains(connRefLogicalName))
                                {
                                    var connectorName = GetConnectorNameFromId(connectorIdsByLogicalName[connRefLogicalName]);
                                    WriteVerbose($"Mapping connection reference '{connRefLogicalName}' to connection ID via connector name '{connectorName}'");
                                    
                                    referencesToSet[connRefLogicalName] = new ConnectionReferenceInfo
                                    {
                                        ConnectionId = connectionId
                                    };
                                    mappedConnectionReferences.Add(connRefLogicalName);
                                }
                            }
                        }
                        else
                        {
                            WriteVerbose($"Key '{providedKey}' does not match any connection reference logical name or connector name, skipping.");
                        }
                    }
                }
            }

            WriteVerbose($"Setting {referencesToSet.Count} connection reference(s)...");

            // Process each connection reference
            foreach (var kvp in referencesToSet)
            {
                var logicalName = kvp.Key;
                var info = kvp.Value;

                WriteVerbose($"Setting connection reference '{logicalName}' to connection '{info.ConnectionId}'");

                // Query for the connection reference by logical name
                var query = new QueryExpression("connectionreference")
                {
                    ColumnSet = new ColumnSet("connectionreferenceid", "connectionreferencelogicalname", "connectionreferencedisplayname", "connectionid", "connectorid", "description"),
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
                    if (ParameterSetName == "Multiple")
                    {
                        WriteError(new ErrorRecord(
                            new InvalidOperationException($"Connection reference with logical name '{logicalName}' not found."),
                            "ConnectionReferenceNotFound",
                            ErrorCategory.ObjectNotFound,
                            logicalName));
                        continue;
                    }

                    // Single parameter set: create new connection reference
                    WriteVerbose($"  Connection reference not found. Creating new connection reference for '{logicalName}'");

                    // Validate that ConnectorId is provided for creation
                    if (string.IsNullOrEmpty(info.ConnectorId))
                    {
                        WriteError(new ErrorRecord(
                            new InvalidOperationException($"ConnectorId is required when creating a new connection reference '{logicalName}'."),
                            "ConnectorIdRequiredForCreation",
                            ErrorCategory.InvalidArgument,
                            logicalName));
                        continue;
                    }

                    // Create the connection reference
                    var connRefEntity = new Entity("connectionreference");
                    connRefEntity["connectionreferencelogicalname"] = logicalName;
                    connRefEntity["connectionid"] = info.ConnectionId;
                    connRefEntity["connectorid"] = info.ConnectorId;
                    
                    if (!string.IsNullOrEmpty(info.DisplayName))
                    {
                        connRefEntity["connectionreferencedisplayname"] = info.DisplayName;
                    }
                    else
                    {
                        // Default display name to logical name if not provided
                        connRefEntity["connectionreferencedisplayname"] = logicalName;
                    }

                    if (!string.IsNullOrEmpty(info.Description))
                    {
                        connRefEntity["description"] = info.Description;
                    }

                    var newConnRefId = Connection.Create(connRefEntity);
                    WriteVerbose($"  Created connection reference with ID: {newConnRefId}");

                }
                else
                {
                    var connRef = results.Entities[0];
                    var connRefId = connRef.Id;
                    var currentDisplayName = connRef.GetAttributeValue<string>("connectionreferencedisplayname");
                    var currentConnectionId = connRef.GetAttributeValue<string>("connectionid");
                    var currentConnectorId = connRef.GetAttributeValue<string>("connectorid");
                    var currentDescription = connRef.GetAttributeValue<string>("description");

                    WriteVerbose($"  Found connection reference: '{currentDisplayName}' (ID: {connRefId})");
                    WriteVerbose($"  Current connection ID: {currentConnectionId ?? "(none)"}");

                    // Update the connection reference
                    var updateEntity = new Entity("connectionreference", connRefId);
                    bool hasChanges = false;

                    // Update connection ID
                    if (info.ConnectionId != currentConnectionId)
                    {
                        updateEntity["connectionid"] = info.ConnectionId;
                        hasChanges = true;
                    }

                    // Update connector ID if provided and different (only for single parameter set)
                    if (ParameterSetName == "Single" && info.ConnectorId != null && info.ConnectorId != currentConnectorId)
                    {
                        WriteError(new ErrorRecord(
                            new InvalidOperationException($"Cannot change ConnectorId of existing connection reference '{logicalName}'. Current: '{currentConnectorId}', Requested: '{info.ConnectorId}'."),
                            "ConnectorIdCannotBeChanged",
                            ErrorCategory.InvalidArgument,
                            logicalName));
                        continue;
                    }

                    // Update display name if provided and different (only for single parameter set)
                    if (ParameterSetName == "Single" && !string.IsNullOrEmpty(info.DisplayName) && info.DisplayName != currentDisplayName)
                    {
                        updateEntity["connectionreferencedisplayname"] = info.DisplayName;
                        hasChanges = true;
                    }

                    // Update description if provided and different (only for single parameter set)
                    if (ParameterSetName == "Single" && info.Description != null && info.Description != currentDescription)
                    {
                        updateEntity["description"] = info.Description;
                        hasChanges = true;
                    }

                    if (hasChanges)
                    {
                        Connection.Update(updateEntity);
                        WriteVerbose($"  Successfully updated connection reference '{logicalName}'");
                    }
                    else
                    {
                        WriteVerbose($"  No changes needed for connection reference '{logicalName}'");
                    }


                }
            }
        }

        /// <summary>
        /// Extracts the connector name from a full connector ID path.
        /// For example, "/providers/Microsoft.PowerApps/apis/shared_sharepointonline" becomes "shared_sharepointonline".
        /// </summary>
        private string GetConnectorNameFromId(string connectorId)
        {
            if (string.IsNullOrEmpty(connectorId))
            {
                return connectorId;
            }

            var lastSlashIndex = connectorId.LastIndexOf('/');
            if (lastSlashIndex >= 0 && lastSlashIndex < connectorId.Length - 1)
            {
                return connectorId.Substring(lastSlashIndex + 1);
            }

            return connectorId;
        }

        /// <summary>
        /// Checks if a provided key matches a connector ID, comparing just the connector name (after last /).
        /// </summary>
        private bool DoesKeyMatchConnectorName(string providedKey, string connectorId)
        {
            if (string.IsNullOrEmpty(providedKey) || string.IsNullOrEmpty(connectorId))
            {
                return false;
            }

            // Extract connector names from both
            var providedConnectorName = GetConnectorNameFromId(providedKey);
            var actualConnectorName = GetConnectorNameFromId(connectorId);

            return string.Equals(providedConnectorName, actualConnectorName, StringComparison.OrdinalIgnoreCase);
        }

        private class ConnectionReferenceInfo
        {
            public string ConnectionId { get; set; }
            public string ConnectorId { get; set; }
            public string DisplayName { get; set; }
            public string Description { get; set; }
        }
    }
}
