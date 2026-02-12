using Microsoft.Crm.Sdk.Messages;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Messages;
using Microsoft.Xrm.Sdk.Metadata;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Management.Automation;
using System.ServiceModel;

namespace Rnwood.Dataverse.Data.PowerShell.Commands
{
    /// <summary>
    /// Creates or updates a global or local option set in Dataverse.
    /// </summary>
    [Cmdlet(VerbsCommon.Set, "DataverseOptionSetMetadata", SupportsShouldProcess = true, ConfirmImpact = ConfirmImpact.Medium, DefaultParameterSetName = PARAMSET_GLOBAL)]
    [OutputType(typeof(PSObject))]
    public class SetDataverseOptionSetMetadataCmdlet : OrganizationServiceCmdlet
    {
        private const string PARAMSET_ENTITY_ATTRIBUTE = "EntityAttribute";
        private const string PARAMSET_GLOBAL = "Global";

        private int _baseLanguageCode;

        /// <summary>
        /// Gets or sets the logical name of the entity (for local option sets).
        /// </summary>
        [Parameter(ParameterSetName = PARAMSET_ENTITY_ATTRIBUTE, Mandatory = true, Position = 0, HelpMessage = "Logical name of the entity (table)")]
        [ArgumentCompleter(typeof(TableNameArgumentCompleter))]
        [Alias("TableName")]
        public string EntityName { get; set; }

        /// <summary>
        /// Gets or sets the logical name of the attribute (for local option sets).
        /// </summary>
        [Parameter(ParameterSetName = PARAMSET_ENTITY_ATTRIBUTE, Mandatory = true, Position = 1, HelpMessage = "Logical name of the choice attribute (column)")]
        [ArgumentCompleter(typeof(ColumnNameArgumentCompleter))]
        [Alias("ColumnName")]
        public string AttributeName { get; set; }

        /// <summary>
        /// Gets or sets the name of the global option set.
        /// </summary>
        [Parameter(ParameterSetName = PARAMSET_GLOBAL, Mandatory = true, Position = 0, HelpMessage = "Name of the global option set")]
        public string Name { get; set; }

        /// <summary>
        /// Gets or sets the display name of the option set.
        /// </summary>
        [Parameter(HelpMessage = "Display name of the option set")]
        public string DisplayName { get; set; }

        /// <summary>
        /// Gets or sets the description of the option set.
        /// </summary>
        [Parameter(HelpMessage = "Description of the option set")]
        public string Description { get; set; }

        /// <summary>
        /// Gets or sets the options for the option set.
        /// </summary>
        [Parameter(Mandatory = true, HelpMessage = "Array of hashtables defining options: @(@{Value=1; Label='Option 1'}, @{Value=2; Label='Option 2'})")]
        public Hashtable[] Options { get; set; }

        /// <summary>
        /// Gets or sets whether to not remove existing options that are not provided.
        /// </summary>
        [Parameter(HelpMessage = "Do not remove existing options that are not provided")]
        public SwitchParameter NoRemoveMissingOptions { get; set; }

        /// <summary>
        /// Gets or sets whether to return the created/updated option set metadata.
        /// </summary>
        [Parameter(HelpMessage = "Return the created or updated option set metadata")]
        public SwitchParameter PassThru { get; set; }

        /// <summary>
        /// Gets or sets whether to publish the option set after creation or update.
        /// </summary>
        [Parameter(HelpMessage = "Publish the option set after creation or update")]
        public SwitchParameter Publish { get; set; }

        /// <summary>
        /// Processes the cmdlet.
        /// </summary>
        protected override void ProcessRecord()
        {
            base.ProcessRecord();

            _baseLanguageCode = GetBaseLanguageCode();

            if (Options == null || Options.Length == 0)
            {
                ThrowTerminatingError(new ErrorRecord(
                    new ArgumentException("Options must be specified"),
                    "OptionsRequired",
                    ErrorCategory.InvalidArgument,
                    null));
                return;
            }

            if (ParameterSetName == PARAMSET_ENTITY_ATTRIBUTE)
            {
                // Update local option set (for an attribute)
                UpdateLocalOptionSet();
            }
            else // PARAMSET_GLOBAL
            {
                // Check if global option set exists
                OptionSetMetadata existingOptionSet = null;
                bool optionSetExists = false;

                try
                {
                    var retrieveRequest = new RetrieveOptionSetRequest
                    {
                        Name = Name,
                        RetrieveAsIfPublished = false
                    };

                    var retrieveResponse = (RetrieveOptionSetResponse)Connection.Execute(retrieveRequest);
                    existingOptionSet = retrieveResponse.OptionSetMetadata as OptionSetMetadata;
                    optionSetExists = existingOptionSet != null;

                    if (optionSetExists)
                    {
                        WriteVerbose($"Global option set '{Name}' already exists");
                    }
                }
                catch (FaultException<OrganizationServiceFault> ex)
                {
                    if (QueryHelpers.IsNotFoundException(ex))
                    {
                        WriteVerbose($"Global option set '{Name}' does not exist - will create");
                    }
                    else
                    {
                        throw;
                    }
                }
       
                if (optionSetExists)
                {
                    // Update existing global option set
                    UpdateGlobalOptionSet(existingOptionSet);
                }
                else
                {
                    // Create new global option set
                    CreateGlobalOptionSet();
                }
            }
        }

        private void UpdateLocalOptionSet()
        {
            // Retrieve the attribute metadata first
            var retrieveAttributeRequest = new RetrieveAttributeRequest
            {
                EntityLogicalName = EntityName,
                LogicalName = AttributeName,
                RetrieveAsIfPublished = false
            };

            WriteVerbose($"Retrieving attribute metadata for '{EntityName}.{AttributeName}'");

            var retrieveAttributeResponse = (RetrieveAttributeResponse)Connection.Execute(retrieveAttributeRequest);
            var attributeMetadata = retrieveAttributeResponse.AttributeMetadata;

            // Extract option set based on attribute type
            OptionSetMetadata existingOptionSet = null;
            if (attributeMetadata is PicklistAttributeMetadata picklistAttr)
            {
                existingOptionSet = picklistAttr.OptionSet;
            }
            else if (attributeMetadata is MultiSelectPicklistAttributeMetadata multiPicklistAttr)
            {
                existingOptionSet = multiPicklistAttr.OptionSet;
            }
            else if (attributeMetadata is StateAttributeMetadata stateAttr)
            {
                existingOptionSet = stateAttr.OptionSet;
            }
            else if (attributeMetadata is StatusAttributeMetadata statusAttr)
            {
                existingOptionSet = statusAttr.OptionSet;
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

            if (existingOptionSet == null)
            {
                ThrowTerminatingError(new ErrorRecord(
                    new InvalidOperationException($"No option set found for attribute '{AttributeName}' on entity '{EntityName}'."),
                    "NoOptionSetFound",
                    ErrorCategory.InvalidResult,
                    null));
                return;
            }

            WriteVerbose($"Found local option set for '{EntityName}.{AttributeName}' (IsGlobal={existingOptionSet.IsGlobal})");

            // Update the local option set
            bool hasChanges = false;

            // Update options - add new ones or update existing ones
            foreach (var option in Options)
            {
                var value = option["Value"] != null ? Convert.ToInt32(option["Value"]) : (int?)null;
                var label = option["Label"] as string;
                var description = option["Description"] as string;

                if (string.IsNullOrWhiteSpace(label) || !value.HasValue)
                {
                    continue;
                }

                // Check if option exists
                var existingOption = existingOptionSet.Options?.FirstOrDefault(o => o.Value == value.Value);

                if (existingOption != null)
                {
                    // Update existing option
                    var updateOptionRequest = new UpdateOptionValueRequest
                    {
                        EntityLogicalName = EntityName,
                        AttributeLogicalName = AttributeName,
                        Value = value.Value,
                        Label = new Label(label, _baseLanguageCode),
                        MergeLabels = true
                    };

                    if (!string.IsNullOrWhiteSpace(description))
                    {
                        updateOptionRequest.Description = new Label(description, _baseLanguageCode);
                    }

                    if (!ShouldProcess($"Option value '{value.Value}' in local option set '{EntityName}.{AttributeName}'", "Update option"))
                    {
                        return;
                    }

                    Connection.Execute(updateOptionRequest);
                    hasChanges = true;
                    WriteVerbose($"Updated option value {value.Value} with label '{label}'");
                }
                else
                {
                    // Insert new option
                    var insertOptionRequest = new InsertOptionValueRequest
                    {
                        EntityLogicalName = EntityName,
                        AttributeLogicalName = AttributeName,
                        Value = value.Value,
                        Label = new Label(label, _baseLanguageCode)
                    };

                    if (!string.IsNullOrWhiteSpace(description))
                    {
                        insertOptionRequest.Description = new Label(description, _baseLanguageCode);
                    }

                    if (!ShouldProcess($"Local option set '{EntityName}.{AttributeName}'", $"Insert new option with value {value.Value}"))
                    {
                        return;
                    }

                    Connection.Execute(insertOptionRequest);
                    hasChanges = true;
                    WriteVerbose($"Inserted new option value {value.Value} with label '{label}'");
                }
            }

            // Remove options that are not provided
            if (!NoRemoveMissingOptions.IsPresent)
            {
                var providedValues = new HashSet<int>();
                foreach (var option in Options)
                {
                    var value = option["Value"] != null ? Convert.ToInt32(option["Value"]) : (int?)null;
                    if (value.HasValue)
                    {
                        providedValues.Add(value.Value);
                    }
                }

                foreach (var existingOption in existingOptionSet.Options.Where(o => o.Value.HasValue && !providedValues.Contains(o.Value.Value)))
                {
                    var deleteRequest = new DeleteOptionValueRequest
                    {
                        EntityLogicalName = EntityName,
                        AttributeLogicalName = AttributeName,
                        Value = existingOption.Value.Value
                    };

                    if (!ShouldProcess($"Option value '{existingOption.Value.Value}' in local option set '{EntityName}.{AttributeName}'", "Delete option"))
                    {
                        return;
                    }

                    Connection.Execute(deleteRequest);
                    hasChanges = true;
                    WriteVerbose($"Deleted option value {existingOption.Value.Value}");
                }
            }

            if (!hasChanges)
            {
                WriteWarning("No changes specified for update");
                
                if (PassThru)
                {
                    var result = ConvertOptionSetToPSObject(existingOptionSet);
                    WriteObject(result);
                }
                return;
            }

            WriteVerbose($"Updated local option set for '{EntityName}.{AttributeName}'");

            // Publish the entity if specified
            if (Publish && ShouldProcess($"Entity '{EntityName}'", "Publish"))
            {
                var publishRequest = new PublishXmlRequest
                {
                    ParameterXml = $"<importexportxml><entities><entity>{EntityName}</entity></entities></importexportxml>"
                };
                Connection.Execute(publishRequest);
                WriteVerbose($"Published entity '{EntityName}'");
                
                // Wait for publish to complete
                PublishHelpers.WaitForPublishComplete(Connection, WriteVerbose);
            }

            if (PassThru)
            {
                // Retrieve and return the updated option set
                var retrieveRequest = new RetrieveAttributeRequest
                {
                    EntityLogicalName = EntityName,
                    LogicalName = AttributeName,
                    RetrieveAsIfPublished = true
                };

                var retrieveResponse = (RetrieveAttributeResponse)Connection.Execute(retrieveRequest);
                var updatedAttributeMetadata = retrieveResponse.AttributeMetadata;

                OptionSetMetadata updatedOptionSet = null;
                if (updatedAttributeMetadata is PicklistAttributeMetadata updatedPicklistAttr)
                {
                    updatedOptionSet = updatedPicklistAttr.OptionSet;
                }
                else if (updatedAttributeMetadata is MultiSelectPicklistAttributeMetadata updatedMultiPicklistAttr)
                {
                    updatedOptionSet = updatedMultiPicklistAttr.OptionSet;
                }
                else if (updatedAttributeMetadata is StateAttributeMetadata updatedStateAttr)
                {
                    updatedOptionSet = updatedStateAttr.OptionSet;
                }
                else if (updatedAttributeMetadata is StatusAttributeMetadata updatedStatusAttr)
                {
                    updatedOptionSet = updatedStatusAttr.OptionSet;
                }

                if (updatedOptionSet != null)
                {
                    var result = ConvertOptionSetToPSObject(updatedOptionSet);
                    WriteObject(result);
                }
            }
        }

        private void CreateGlobalOptionSet()
        {
            var optionSet = new OptionSetMetadata
            {
                Name = Name,
                IsGlobal = true,
                OptionSetType = OptionSetType.Picklist
            };

            if (!string.IsNullOrWhiteSpace(DisplayName))
            {
                optionSet.DisplayName = new Label(DisplayName, _baseLanguageCode);
            }

            if (!string.IsNullOrWhiteSpace(Description))
            {
                optionSet.Description = new Label(Description, _baseLanguageCode);
            }

            foreach (var option in Options)
            {
                var value = option["Value"] != null ? Convert.ToInt32(option["Value"]) : (int?)null;
                var label = option["Label"] as string;
                var color = option["Color"] as string;
                var description = option["Description"] as string;

                if (string.IsNullOrWhiteSpace(label))
                {
                    continue;
                }

                var optionMetadata = value.HasValue
                    ? new OptionMetadata(new Label(label, _baseLanguageCode), value.Value)
                    : new OptionMetadata(new Label(label, _baseLanguageCode), null);

                if (!string.IsNullOrWhiteSpace(color))
                {
                    optionMetadata.Color = color;
                }

                if (!string.IsNullOrWhiteSpace(description))
                {
                    optionMetadata.Description = new Label(description, _baseLanguageCode);
                }

                optionSet.Options.Add(optionMetadata);
            }

            var request = new CreateOptionSetRequest
            {
                OptionSet = optionSet
            };

            if (!ShouldProcess($"Global option set '{Name}'", $"Create option set with {optionSet.Options.Count} options"))
            {
                return;
            }

            WriteVerbose($"Creating global option set '{Name}' with {optionSet.Options.Count} options");

            var response = (CreateOptionSetResponse)Connection.Execute(request);

            WriteVerbose($"Option set created successfully with MetadataId: {response.OptionSetId}");

            // Publish the option set if specified
            if (Publish && ShouldProcess($"Global option set '{Name}'", "Publish"))
            {
                var publishRequest = new PublishXmlRequest
                {
                    ParameterXml = $"<importexportxml><optionsets><optionset>{Name}</optionset></optionsets></importexportxml>"
                };
                Connection.Execute(publishRequest);
                WriteVerbose($"Published global option set '{Name}'");
                
                // Wait for publish to complete
                PublishHelpers.WaitForPublishComplete(Connection, WriteVerbose);
            }

            if (PassThru)
            {
                // Retrieve and return the created option set
                var retrieveRequest = new RetrieveOptionSetRequest
                {
                    Name = Name,
                    RetrieveAsIfPublished = true
                };

                var retrieveResponse = (RetrieveOptionSetResponse)Connection.Execute(retrieveRequest);
                var result = ConvertOptionSetToPSObject((OptionSetMetadata)retrieveResponse.OptionSetMetadata);
                WriteObject(result);
            }
        }

        private void UpdateGlobalOptionSet(OptionSetMetadata existingOptionSet)
        {
            // Validate immutable properties first
            ValidateImmutableProperties(existingOptionSet);

            bool hasChanges = false;

            // Update display name
            if (!string.IsNullOrWhiteSpace(DisplayName))
            {
                var updateDisplayNameRequest = new UpdateOptionSetRequest
                {
                    OptionSet = new OptionSetMetadata
                    {
                        Name = Name,
                        DisplayName = new Label(DisplayName, _baseLanguageCode)
                    },
                    MergeLabels = true
                };

                if (!ShouldProcess($"Global option set '{Name}'", "Update display name"))
                {
                    return;
                }

                Connection.Execute(updateDisplayNameRequest);
                hasChanges = true;
                WriteVerbose($"Updated display name to '{DisplayName}'");
            }

            // Update description
            if (!string.IsNullOrWhiteSpace(Description))
            {
                var updateDescriptionRequest = new UpdateOptionSetRequest
                {
                    OptionSet = new OptionSetMetadata
                    {
                        Name = Name,
                        Description = new Label(Description, _baseLanguageCode)
                    },
                    MergeLabels = true
                };

                if (!ShouldProcess($"Global option set '{Name}'", "Update description"))
                {
                    return;
                }

                Connection.Execute(updateDescriptionRequest);
                hasChanges = true;
                WriteVerbose($"Updated description");
            }

            // Update options - add new ones or update existing ones
            foreach (var option in Options)
            {
                var value = option["Value"] != null ? Convert.ToInt32(option["Value"]) : (int?)null;
                var label = option["Label"] as string;
                var description = option["Description"] as string;

                if (string.IsNullOrWhiteSpace(label) || !value.HasValue)
                {
                    continue;
                }

                // Check if option exists
                var existingOption = existingOptionSet.Options?.FirstOrDefault(o => o.Value == value.Value);

                if (existingOption != null)
                {
                    // Update existing option
                    var updateOptionRequest = new UpdateOptionValueRequest
                    {
                        OptionSetName = Name,
                        Value = value.Value,
                        Label = new Label(label, _baseLanguageCode),
                        MergeLabels = true
                    };

                    if (!string.IsNullOrWhiteSpace(description))
                    {
                        updateOptionRequest.Description = new Label(description, _baseLanguageCode);
                    }

                    if (!ShouldProcess($"Option value '{value.Value}' in global option set '{Name}'", "Update option"))
                    {
                        return;
                    }

                    Connection.Execute(updateOptionRequest);
                    hasChanges = true;
                    WriteVerbose($"Updated option value {value.Value} with label '{label}'");
                }
                else
                {
                    // Insert new option
                    var insertOptionRequest = new InsertOptionValueRequest
                    {
                        OptionSetName = Name,
                        Value = value.Value,
                        Label = new Label(label, _baseLanguageCode)
                    };

                    if (!string.IsNullOrWhiteSpace(description))
                    {
                        insertOptionRequest.Description = new Label(description, _baseLanguageCode);
                    }

                    if (!ShouldProcess($"Global option set '{Name}'", $"Insert new option with value {value.Value}"))
                    {
                        return;
                    }

                    Connection.Execute(insertOptionRequest);
                    hasChanges = true;
                    WriteVerbose($"Inserted new option value {value.Value} with label '{label}'");
                }
            }

            // Remove options that are not provided
            if (!NoRemoveMissingOptions.IsPresent)
            {
                var providedValues = new HashSet<int>();
                foreach (var option in Options)
                {
                    var value = option["Value"] != null ? Convert.ToInt32(option["Value"]) : (int?)null;
                    if (value.HasValue)
                    {
                        providedValues.Add(value.Value);
                    }
                }

                foreach (var existingOption in existingOptionSet.Options.Where(o => o.Value.HasValue && !providedValues.Contains(o.Value.Value)))
                {
                    var deleteRequest = new DeleteOptionValueRequest
                    {
                        OptionSetName = Name,
                        Value = existingOption.Value.Value
                    };

                    if (!ShouldProcess($"Option value '{existingOption.Value.Value}' in global option set '{Name}'", "Delete option"))
                    {
                        return;
                    }

                    Connection.Execute(deleteRequest);
                    hasChanges = true;
                    WriteVerbose($"Deleted option value {existingOption.Value.Value}");
                }
            }

            if (!hasChanges)
            {
                WriteWarning("No changes specified for update");
                
                if (PassThru)
                {
                    var result = ConvertOptionSetToPSObject(existingOptionSet);
                    WriteObject(result);
                }
                return;
            }

            WriteVerbose($"Updated global option set '{Name}'");

            // Publish the option set if specified
            if (Publish && ShouldProcess($"Global option set '{Name}'", "Publish"))
            {
                var publishRequest = new PublishXmlRequest
                {
                    ParameterXml = $"<importexportxml><optionsets><optionset>{Name}</optionset></optionsets></importexportxml>"
                };
                Connection.Execute(publishRequest);
                WriteVerbose($"Published global option set '{Name}'");
                
                // Wait for publish to complete
                PublishHelpers.WaitForPublishComplete(Connection, WriteVerbose);
            }

            if (PassThru)
            {
                // Retrieve and return the updated option set
                var retrieveRequest = new RetrieveOptionSetRequest
                {
                    Name = Name,
                    RetrieveAsIfPublished = true
                };

                var retrieveResponse = (RetrieveOptionSetResponse)Connection.Execute(retrieveRequest);
                var result = ConvertOptionSetToPSObject((OptionSetMetadata)retrieveResponse.OptionSetMetadata);
                WriteObject(result);
            }
        }

        private void ValidateImmutableProperties(OptionSetMetadata existingOptionSet)
        {
            // Check if Name was provided and is different (immutable after creation)
            // Note: Name is the key for retrieval, so it should always match, but check for consistency
            if (!string.Equals(Name, existingOptionSet.Name, StringComparison.OrdinalIgnoreCase))
            {
                ThrowTerminatingError(new ErrorRecord(
                    new InvalidOperationException($"Option set name mismatch. Retrieved '{existingOptionSet.Name}' but specified '{Name}'. Option set names are immutable."),
                    "ImmutableOptionSetName",
                    ErrorCategory.InvalidOperation,
                    Name));
            }

        }
        
        private PSObject ConvertOptionSetToPSObject(OptionSetMetadata optionSet)
        {
            var result = new PSObject();
            result.Properties.Add(new PSNoteProperty("Name", optionSet.Name));
            result.Properties.Add(new PSNoteProperty("DisplayName", optionSet.DisplayName?.UserLocalizedLabel?.Label));
            result.Properties.Add(new PSNoteProperty("Description", optionSet.Description?.UserLocalizedLabel?.Label));
            result.Properties.Add(new PSNoteProperty("IsGlobal", optionSet.IsGlobal));
            result.Properties.Add(new PSNoteProperty("MetadataId", optionSet.MetadataId));

            if (optionSet.Options != null)
            {
                var options = optionSet.Options
                    .OrderBy(o => o.Value)
                    .Select(o => new PSObject(new
                    {
                        Value = o.Value,
                        Label = o.Label?.UserLocalizedLabel?.Label,
                        Color = o.Color
                    }))
                    .ToArray();
                result.Properties.Add(new PSNoteProperty("Options", options));
            }

            return result;
        }
    }
}
