using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Messages;
using Microsoft.Xrm.Sdk.Metadata;
using System;
using System.Linq;
using System.Management.Automation;

namespace Rnwood.Dataverse.Data.PowerShell.Commands
{
    /// <summary>
    /// Retrieves option set values for a choice field in Dataverse.
    /// </summary>
    [Cmdlet(VerbsCommon.Get, "DataverseOptionSetMetadata")]
    [OutputType(typeof(OptionSetMetadataBase))]
    public class GetDataverseOptionSetMetadataCmdlet : OrganizationServiceCmdlet
    {
        private const string PARAMSET_ENTITY_ATTRIBUTE = "EntityAttribute";
        private const string PARAMSET_GLOBAL = "Global";

        /// <summary>
        /// Gets or sets the logical name of the entity.
        /// </summary>
        [Parameter(ParameterSetName = PARAMSET_ENTITY_ATTRIBUTE, Mandatory = true, Position = 0, HelpMessage = "Logical name of the entity (table)")]
        [ArgumentCompleter(typeof(TableNameArgumentCompleter))]
        [Alias("TableName")]
        public string EntityName { get; set; }

        /// <summary>
        /// Gets or sets the logical name of the attribute.
        /// </summary>
        [Parameter(ParameterSetName = PARAMSET_ENTITY_ATTRIBUTE, Mandatory = true, Position = 1, HelpMessage = "Logical name of the choice attribute (column)")]
        [ArgumentCompleter(typeof(ColumnNameArgumentCompleter))]
        [Alias("ColumnName")]
        public string AttributeName { get; set; }

        /// <summary>
        /// Gets or sets the name of a global option set.
        /// If not specified, returns all global option sets.
        /// </summary>
        [Parameter(ParameterSetName = PARAMSET_GLOBAL, Mandatory = false, HelpMessage = "Name of the global option set. If not specified, returns all global option sets.")]
        public string Name { get; set; }

        /// <summary>
        /// Gets or sets whether to retrieve only published metadata.
        /// When not specified (default), retrieves unpublished (draft) metadata which includes all changes.
        /// </summary>
        [Parameter(HelpMessage = "Retrieve only published metadata. By default, unpublished (draft) metadata is retrieved which includes all changes.")]
        public SwitchParameter Published { get; set; }

        /// <summary>
        /// Processes the cmdlet.
        /// </summary>
        protected override void ProcessRecord()
        {
            base.ProcessRecord();

            OptionSetMetadata optionSetMetadata = null;

            if (ParameterSetName == PARAMSET_ENTITY_ATTRIBUTE)
            {
                // Retrieve attribute metadata
                var attributeRequest = new RetrieveAttributeRequest
                {
                    EntityLogicalName = EntityName,
                    LogicalName = AttributeName,
                    RetrieveAsIfPublished = !Published.IsPresent
                };

                WriteVerbose($"Retrieving attribute metadata for '{EntityName}.{AttributeName}'");

                var attributeResponse = (RetrieveAttributeResponse)QueryHelpers.ExecuteWithThrottlingRetry(Connection, attributeRequest);
                var attributeMetadata = attributeResponse.AttributeMetadata;

                // Extract option set based on attribute type
                if (attributeMetadata is PicklistAttributeMetadata picklistAttr)
                {
                    optionSetMetadata = picklistAttr.OptionSet;
                }
                else if (attributeMetadata is MultiSelectPicklistAttributeMetadata multiPicklistAttr)
                {
                    optionSetMetadata = multiPicklistAttr.OptionSet;
                }
                else if (attributeMetadata is StateAttributeMetadata stateAttr)
                {
                    optionSetMetadata = stateAttr.OptionSet;
                }
                else if (attributeMetadata is StatusAttributeMetadata statusAttr)
                {
                    optionSetMetadata = statusAttr.OptionSet;
                }
                else if (attributeMetadata is BooleanAttributeMetadata booleanAttr)
                {
                    // Handle Boolean as a special case - return BooleanOptionSetMetadata
                    WriteObject(booleanAttr.OptionSet);
                    return;
                }
                else
                {
                    ThrowTerminatingError(new ErrorRecord(
                        new InvalidOperationException($"Attribute '{AttributeName}' on entity '{EntityName}' is not a choice field. It is of type '{attributeMetadata.AttributeType}'."),
                        "NotAChoiceField",
                        ErrorCategory.InvalidArgument,
                        attributeMetadata));
                    return;
                }
            }
            else // PARAMSET_GLOBAL
            {
                if (string.IsNullOrWhiteSpace(Name))
                {
                    // List all global option sets
                    RetrieveAllGlobalOptionSets();
                    return;
                }
                else
                {
                    // Retrieve specific global option set
                    var optionSetRequest = new RetrieveOptionSetRequest
                    {
                        Name = Name,
                        RetrieveAsIfPublished = !Published.IsPresent
                    };

                    WriteVerbose($"Retrieving global option set '{Name}'");

                    var optionSetResponse = (RetrieveOptionSetResponse)QueryHelpers.ExecuteWithThrottlingRetry(Connection, optionSetRequest);
                    optionSetMetadata = optionSetResponse.OptionSetMetadata as OptionSetMetadata;

                    if (optionSetMetadata == null)
                    {
                        ThrowTerminatingError(new ErrorRecord(
                            new InvalidOperationException($"Option set '{Name}' was not found or is not a standard option set."),
                            "OptionSetNotFound",
                            ErrorCategory.ObjectNotFound,
                            Name));
                        return;
                    }
                }
            }

            if (optionSetMetadata == null)
            {
                ThrowTerminatingError(new ErrorRecord(
                    new InvalidOperationException($"No option set metadata found."),
                    "NoOptionSetMetadata",
                    ErrorCategory.InvalidResult,
                    null));
                return;
            }

            WriteObject(optionSetMetadata);
        }

        private void RetrieveAllGlobalOptionSets()
        {
            var request = new RetrieveAllOptionSetsRequest
            {
                RetrieveAsIfPublished = !Published.IsPresent
            };

            WriteVerbose("Retrieving all global option sets");

            var response = (RetrieveAllOptionSetsResponse)QueryHelpers.ExecuteWithThrottlingRetry(Connection, request);
            var optionSets = response.OptionSetMetadata;

            WriteVerbose($"Retrieved {optionSets.Length} option sets");

            // Filter to only standard option sets (not boolean)
            var results = optionSets
                .OfType<OptionSetMetadata>()
                .Where(os => os.IsGlobal == true)
                .OrderBy(os => os.Name, StringComparer.OrdinalIgnoreCase)
                .ToArray();

            WriteObject(results, true);
        }
    }
}
