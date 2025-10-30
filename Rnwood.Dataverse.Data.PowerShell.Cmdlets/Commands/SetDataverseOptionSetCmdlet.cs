using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Messages;
using Microsoft.Xrm.Sdk.Metadata;
using System;
using System.Collections;
using System.Linq;
using System.Management.Automation;

namespace Rnwood.Dataverse.Data.PowerShell.Commands
{
    /// <summary>
    /// Creates or updates a global option set in Dataverse.
    /// </summary>
    [Cmdlet(VerbsCommon.Set, "DataverseOptionSet", SupportsShouldProcess = true, ConfirmImpact = ConfirmImpact.High)]
    [OutputType(typeof(PSObject))]
    public class SetDataverseOptionSetCmdlet : OrganizationServiceCmdlet
    {
        /// <summary>
        /// Gets or sets the name of the option set.
        /// </summary>
        [Parameter(Mandatory = true, Position = 0, HelpMessage = "Name of the global option set")]
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
        /// Gets or sets whether to force update even if the option set exists.
        /// </summary>
        [Parameter(HelpMessage = "Force update if the option set already exists")]
        public SwitchParameter Force { get; set; }

        /// <summary>
        /// Gets or sets whether to return the created/updated option set metadata.
        /// </summary>
        [Parameter(HelpMessage = "Return the created or updated option set metadata")]
        public SwitchParameter PassThru { get; set; }

        /// <summary>
        /// Processes the cmdlet.
        /// </summary>
        protected override void ProcessRecord()
        {
            base.ProcessRecord();

            if (Options == null || Options.Length == 0)
            {
                ThrowTerminatingError(new ErrorRecord(
                    new ArgumentException("Options must be specified"),
                    "OptionsRequired",
                    ErrorCategory.InvalidArgument,
                    null));
                return;
            }

            // Check if option set exists
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
            catch (Exception)
            {
                WriteVerbose($"Global option set '{Name}' does not exist - will create");
            }

            if (optionSetExists && !Force)
            {
                if (!ShouldContinue($"Global option set '{Name}' already exists. Update it?", "Confirm Update"))
                {
                    return;
                }
            }

            if (optionSetExists)
            {
                // Update existing option set
                UpdateOptionSet(existingOptionSet);
            }
            else
            {
                // Create new option set
                CreateOptionSet();
            }
        }

        private void CreateOptionSet()
        {
            var optionSet = new OptionSetMetadata
            {
                Name = Name,
                IsGlobal = true,
                OptionSetType = OptionSetType.Picklist
            };

            if (!string.IsNullOrWhiteSpace(DisplayName))
            {
                optionSet.DisplayName = new Label(new LocalizedLabel(DisplayName, 1033), new LocalizedLabel[0]);
            }

            if (!string.IsNullOrWhiteSpace(Description))
            {
                optionSet.Description = new Label(new LocalizedLabel(Description, 1033), new LocalizedLabel[0]);
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
                    ? new OptionMetadata(new Label(new LocalizedLabel(label, 1033), new LocalizedLabel[0]), value.Value)
                    : new OptionMetadata(new Label(new LocalizedLabel(label, 1033), new LocalizedLabel[0]), null);

                if (!string.IsNullOrWhiteSpace(color))
                {
                    optionMetadata.Color = color;
                }

                if (!string.IsNullOrWhiteSpace(description))
                {
                    optionMetadata.Description = new Label(new LocalizedLabel(description, 1033), new LocalizedLabel[0]);
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

            if (PassThru)
            {
                // Retrieve and return the created option set
                var retrieveRequest = new RetrieveOptionSetRequest
                {
                    Name = Name,
                    RetrieveAsIfPublished = false
                };

                var retrieveResponse = (RetrieveOptionSetResponse)Connection.Execute(retrieveRequest);
                var result = ConvertOptionSetToPSObject((OptionSetMetadata)retrieveResponse.OptionSetMetadata);
                WriteObject(result);
            }
        }

        private void UpdateOptionSet(OptionSetMetadata existingOptionSet)
        {
            bool hasChanges = false;

            // Update display name
            if (!string.IsNullOrWhiteSpace(DisplayName))
            {
                var updateDisplayNameRequest = new UpdateOptionSetRequest
                {
                    OptionSet = new OptionSetMetadata
                    {
                        Name = Name,
                        DisplayName = new Label(new LocalizedLabel(DisplayName, 1033), new LocalizedLabel[0])
                    }
                };

                Connection.Execute(updateDisplayNameRequest);
                hasChanges = true;
            }

            // Update description
            if (!string.IsNullOrWhiteSpace(Description))
            {
                var updateDescriptionRequest = new UpdateOptionSetRequest
                {
                    OptionSet = new OptionSetMetadata
                    {
                        Name = Name,
                        Description = new Label(new LocalizedLabel(Description, 1033), new LocalizedLabel[0])
                    }
                };

                Connection.Execute(updateDescriptionRequest);
                hasChanges = true;
            }

            // Update options - add new ones or update existing ones
            foreach (var option in Options)
            {
                var value = option["Value"] != null ? Convert.ToInt32(option["Value"]) : (int?)null;
                var label = option["Label"] as string;
                var color = option["Color"] as string;
                var description = option["Description"] as string;

                if (string.IsNullOrWhiteSpace(label) || !value.HasValue)
                {
                    continue;
                }

                // Check if option exists
                var existingOption = existingOptionSet.Options.FirstOrDefault(o => o.Value == value.Value);

                if (existingOption != null)
                {
                    // Update existing option
                    var updateOptionRequest = new UpdateOptionValueRequest
                    {
                        OptionSetName = Name,
                        Value = value.Value,
                        Label = new Label(new LocalizedLabel(label, 1033), new LocalizedLabel[0])
                    };

                    if (!string.IsNullOrWhiteSpace(color))
                    {
                        updateOptionRequest.MergeLabels = true; // Preserve other labels
                        // Note: Color update requires additional steps
                    }

                    if (!string.IsNullOrWhiteSpace(description))
                    {
                        updateOptionRequest.Description = new Label(new LocalizedLabel(description, 1033), new LocalizedLabel[0]);
                    }

                    Connection.Execute(updateOptionRequest);
                    hasChanges = true;
                }
                else
                {
                    // Insert new option
                    var insertOptionRequest = new InsertOptionValueRequest
                    {
                        OptionSetName = Name,
                        Value = value.Value,
                        Label = new Label(new LocalizedLabel(label, 1033), new LocalizedLabel[0])
                    };

                    if (!string.IsNullOrWhiteSpace(description))
                    {
                        insertOptionRequest.Description = new Label(new LocalizedLabel(description, 1033), new LocalizedLabel[0]);
                    }

                    Connection.Execute(insertOptionRequest);
                    hasChanges = true;
                }
            }

            if (!hasChanges)
            {
                WriteWarning("No changes specified for update");
                return;
            }

            if (!ShouldProcess($"Global option set '{Name}'", "Update option set"))
            {
                return;
            }

            WriteVerbose($"Updated global option set '{Name}'");

            if (PassThru)
            {
                // Retrieve and return the updated option set
                var retrieveRequest = new RetrieveOptionSetRequest
                {
                    Name = Name,
                    RetrieveAsIfPublished = false
                };

                var retrieveResponse = (RetrieveOptionSetResponse)Connection.Execute(retrieveRequest);
                var result = ConvertOptionSetToPSObject((OptionSetMetadata)retrieveResponse.OptionSetMetadata);
                WriteObject(result);
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
