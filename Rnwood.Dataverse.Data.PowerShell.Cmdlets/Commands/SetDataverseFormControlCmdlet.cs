using Microsoft.Crm.Sdk.Messages;
using Microsoft.PowerPlatform.Dataverse.Client;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Query;
using Microsoft.Xrm.Sdk.Messages;
using Microsoft.Xrm.Sdk.Metadata;
using Rnwood.Dataverse.Data.PowerShell.Model;
using System;
using System.Linq;
using System.Management.Automation;
using System.ServiceModel;
using System.Xml.Linq;

namespace Rnwood.Dataverse.Data.PowerShell.Commands
{
    /// <summary>
    /// Creates or updates a control in a Dataverse form section.
    /// </summary>
    [Cmdlet(VerbsCommon.Set, "DataverseFormControl", DefaultParameterSetName = "Standard", SupportsShouldProcess = true, ConfirmImpact = ConfirmImpact.Medium)]
    [OutputType(typeof(string))]
    public class SetDataverseFormControlCmdlet : OrganizationServiceCmdlet
    {
        /// <summary>
        /// Gets or sets the form ID.
        /// </summary>
        [Parameter(Mandatory = true, ValueFromPipelineByPropertyName = true, HelpMessage = "ID of the form")]
        public Guid FormId { get; set; }

        /// <summary>
        /// Gets or sets the section name where the control is located.
        /// Not required for header controls (when TabName is '[Header]').
        /// </summary>
        [Parameter(Mandatory = false, HelpMessage = "Name of the section containing the control. Not required for header controls.")]
        public string SectionName { get; set; }

        /// <summary>
        /// Gets or sets the tab name where the section is located.
        /// </summary>
        [Parameter(Mandatory = true, HelpMessage = "Name of the tab containing the section")]
        public string TabName { get; set; }

        /// <summary>
        /// Gets or sets the control ID for updating an existing control.
        /// </summary>
        [Parameter(Mandatory = false, HelpMessage = "ID of the control to create or update")]
        public string ControlId { get; set; }

        /// <summary>
        /// Gets or sets the data field name (attribute) for the control.
        /// Not required for special controls like Subgrid, WebResource, QuickForm, Spacer, IFrame, Timer, KBSearch.
        /// </summary>
        [Parameter(Mandatory = false, ParameterSetName = "Standard", HelpMessage = "Data field name (attribute) for the control. Not required for special controls like Subgrid, WebResource, QuickForm, Spacer, IFrame, Timer, KBSearch.")]
        public string DataField { get; set; }

        /// <summary>
        /// Gets or sets the raw XML for the control element.
        /// </summary>
        [Parameter(ParameterSetName = "RawXml", Mandatory = true, HelpMessage = "Raw XML for the control element")]
        public string ControlXml { get; set; }

        /// <summary>
        /// Gets or sets the control type.
        /// If not specified, will be automatically determined based on the attribute metadata.
        /// </summary>
        [Parameter(HelpMessage = "Control type (Standard, Lookup, OptionSet, DateTime, Boolean, Subgrid, WebResource, QuickForm, Spacer, IFrame, Timer, KBSearch, Notes, Email, Memo, Money, Data). If not specified, will be automatically determined based on attribute metadata.")]
        [ValidateSet("Standard", "Lookup", "OptionSet", "DateTime", "Boolean", "Subgrid", "WebResource", "QuickForm", "Spacer", "IFrame", "Timer", "KBSearch", "Notes", "Email", "Memo", "Money", "Data")]
        public string ControlType { get; set; }

        /// <summary>
        /// Gets or sets the label for the control.
        /// </summary>
        [Parameter(HelpMessage = "Label text for the control")]
        public string Label { get; set; }

        /// <summary>
        /// Gets or sets the language code for the label (default: 1033 for English).
        /// </summary>
        [Parameter(HelpMessage = "Language code for the label (default: 1033)")]
        public int LanguageCode { get; set; } = 1033;

        /// <summary>
        /// Gets or sets whether the control is disabled.
        /// </summary>
        [Parameter(HelpMessage = "Whether the control is disabled")]
        public SwitchParameter Disabled { get; set; }

        /// <summary>
        /// Gets or sets whether the control is visible.
        /// </summary>
        [Parameter(HelpMessage = "Whether the control is visible")]
        public SwitchParameter Visible { get; set; } = true;

        /// <summary>
        /// Gets or sets the number of rows (for multiline text).
        /// </summary>
        [Parameter(HelpMessage = "Number of rows for multiline text controls")]
        public int? Rows { get; set; }

        /// <summary>
        /// Gets or sets the colspan for the control.
        /// </summary>
        [Parameter(HelpMessage = "Number of columns the control spans")]
        public int? ColSpan { get; set; }

        /// <summary>
        /// Gets or sets the rowspan for the control.
        /// </summary>
        [Parameter(HelpMessage = "Number of rows the control spans")]
        public int? RowSpan { get; set; }

        /// <summary>
        /// Gets or sets whether to show the label.
        /// </summary>
        [Parameter(HelpMessage = "Whether to show the control label")]
        public SwitchParameter ShowLabel { get; set; } = true;

        /// <summary>
        /// Gets or sets whether the control is required.
        /// </summary>
        [Parameter(HelpMessage = "Whether the control is required")]
        public SwitchParameter IsRequired { get; set; }

        /// <summary>
        /// Gets or sets custom parameters for special controls (WebResource URL, Subgrid settings, etc.).
        /// </summary>
        [Parameter(HelpMessage = "Custom parameters as a hashtable for special controls")]
        public System.Collections.Hashtable Parameters { get; set; }

        /// <summary>
        /// Gets or sets whether to return the control ID.
        /// </summary>
        [Parameter(HelpMessage = "Return the control ID")]
        public SwitchParameter PassThru { get; set; }

        /// <summary>
        /// Gets or sets the zero-based row index where the control should be placed.
        /// </summary>
        [Parameter(HelpMessage = "Zero-based row index where the control should be placed")]
        public int? Row { get; set; }

        /// <summary>
        /// Gets or sets the zero-based column index where the control should be placed.
        /// </summary>
        [Parameter(HelpMessage = "Zero-based column index where the control should be placed")]
        public int? Column { get; set; }

        /// <summary>
        /// Gets or sets the ID of the cell that will contain the control.
        /// </summary>
        [Parameter(HelpMessage = "ID of the cell that will contain the control")]
        public string CellId { get; set; }

        /// <summary>
        /// Gets or sets whether the cell should be auto-sized.
        /// </summary>
        [Parameter(HelpMessage = "Whether the cell should be auto-sized")]
        public SwitchParameter Auto { get; set; }

        /// <summary>
        /// Gets or sets the lock level for the cell.
        /// </summary>
        [Parameter(HelpMessage = "Lock level for the cell")]
        public int? LockLevel { get; set; }

        /// <summary>
        /// Gets or sets whether the control is hidden.
        /// </summary>
        [Parameter(HelpMessage = "Whether the control is hidden")]
        public SwitchParameter Hidden { get; set; }

        private string _entityName;

        /// <summary>
        /// Processes the cmdlet request.
        /// </summary>
        protected override void ProcessRecord()
        {
            base.ProcessRecord();

            // Check if this is a header control operation
            bool isHeaderControl = TabName != null && TabName.Equals("[Header]", StringComparison.OrdinalIgnoreCase);
            
            // For header controls, SectionName is not needed (header acts as both tab and section)
            if (isHeaderControl && string.IsNullOrEmpty(SectionName))
            {
                SectionName = "[Header]";
            }
            
            // Validate that SectionName is provided for non-header controls
            if (!isHeaderControl && string.IsNullOrEmpty(SectionName))
            {
                throw new ArgumentException("SectionName is required for non-header controls.");
            }

            // Retrieve the form and determine entity name
            Entity form = FormXmlHelper.RetrieveForm(Connection, FormId, new ColumnSet("formxml", "objecttypecode"));
            var (doc, systemForm) = FormXmlHelper.ParseFormXml(form);

            // Get the entity name for metadata lookups
            _entityName = (string) form["objecttypecode"];

            XElement targetSection;
            XElement targetTab;
            XElement rowsElement;
            int columnCount;

            if (isHeaderControl)
            {
                // Handle header controls
                var header = systemForm.Element("header");
                if (header == null)
                {
                    // Create header element if it doesn't exist
                    header = new XElement("header",
                        new XAttribute("id", Guid.NewGuid().ToString("B")),
                        new XAttribute("celllabelposition", "Top"));
                    systemForm.AddFirst(header);
                }

                rowsElement = header.Element("rows");
                if (rowsElement == null)
                {
                    rowsElement = new XElement("rows");
                    header.Add(rowsElement);
                }

                // For header, use the header element as both section and tab
                targetSection = header;
                targetTab = header;
                columnCount = 1; // Header typically has one column
            }
            else
            {
                // Find the section
                var sectionResult = FormXmlHelper.FindTargetSection(systemForm, TabName, SectionName);
                if (sectionResult.Section == null)
                {
                    throw new InvalidOperationException($"Section '{SectionName}' not found in form");
                }

                targetSection = sectionResult.Section;
                targetTab = sectionResult.Tab;
                rowsElement = FormXmlHelper.GetOrCreateRowsElement(targetSection);

                // Get column count
                var columnsElement = targetTab.Element("columns");
                columnCount = columnsElement?.Elements("column").Count() ?? 1;
            }

            // Determine target position
            int targetRow = Row ?? -1;
            int targetColumn = Column ?? 0;

            if (targetColumn >= columnCount)
            {
                throw new InvalidOperationException($"Column {targetColumn} is out of range. Valid range is 0-{columnCount - 1}");
            }

            var rows = rowsElement.Elements("row").ToList();

            XElement control = null;
            XElement cell = null;
            XElement existingCell = null;
            string controlId = ControlId;
            bool isUpdate = false;
            string finalControlType = null; // Will be set based on parameter set

            if (ParameterSetName == "RawXml")
            {
                WriteVerbose("DEBUG: Using RawXml parameter set");
                // Parse the raw XML
                try
                {
                    control = XElement.Parse(ControlXml);
                }
                catch (Exception ex)
                {
                    throw new InvalidOperationException($"Failed to parse ControlXml: {ex.Message}", ex);
                }

                // Determine control ID
                if (!string.IsNullOrEmpty(ControlId))
                {
                    controlId = ControlId;
                    control.SetAttributeValue("id", controlId);
                    
                    // Check if control with same ID already exists
                    XElement existingControl;
                    XElement tempExistingCell;
                    
                    if (isHeaderControl)
                    {
                        (existingControl, tempExistingCell) = FormXmlHelper.FindControlInHeader(systemForm, controlId: controlId);
                    }
                    else
                    {
                        (existingControl, tempExistingCell) = FormXmlHelper.FindControlById(systemForm, TabName, SectionName, controlId);
                    }
                    
                    if (existingControl != null)
                    {
                        // Remove from existing position
                        existingControl.Remove();
                        existingCell = tempExistingCell;
                        isUpdate = true;
                    }
                }
                else
                {
                    // ControlId not provided, use DataField + SectionName as key
                    string dataFieldName = control.Attribute("datafieldname")?.Value;
                    if (!string.IsNullOrEmpty(dataFieldName))
                    {
                        // Check if control with same DataField already exists in section
                        XElement existingControl;
                        XElement tempExistingCell;
                        
                        if (isHeaderControl)
                        {
                            (existingControl, tempExistingCell) = FormXmlHelper.FindControlInHeader(systemForm, dataField: dataFieldName);
                        }
                        else
                        {
                            (existingControl, tempExistingCell) = FormXmlHelper.FindControlByDataField(systemForm, TabName, SectionName, dataFieldName);
                        }
                        
                        if (existingControl != null)
                        {
                            // Remove from existing position
                            existingControl.Remove();
                            controlId = existingControl.Attribute("id")?.Value;
                            control.SetAttributeValue("id", controlId);
                            existingCell = tempExistingCell;
                            isUpdate = true;
                        }
                        else
                        {
                            // Generate new ID
                            controlId = Guid.NewGuid().ToString();
                            control.SetAttributeValue("id", controlId);
                        }
                    }
                    else
                    {
                        // No datafieldname, generate new ID
                        controlId = Guid.NewGuid().ToString();
                        control.SetAttributeValue("id", controlId);
                    }
                }

                // Position the control
                PositionControl(rows, rowsElement, columnCount, ref targetRow, ref targetColumn, isUpdate, existingCell, control, out cell);
            }
            else
            {
                // Standard parameter set - determine control type if not provided and validate metadata
                finalControlType = DetermineAndValidateControlType();

                // Determine control ID and check if exists
                if (!string.IsNullOrEmpty(ControlId))
                {
                    // Find by ID
                    XElement existingControl;
                    XElement tempExistingCell;
                    
                    if (isHeaderControl)
                    {
                        (existingControl, tempExistingCell) = FormXmlHelper.FindControlInHeader(systemForm, controlId: ControlId);
                    }
                    else
                    {
                        (existingControl, tempExistingCell) = FormXmlHelper.FindControlById(systemForm, TabName, SectionName, ControlId);
                    }
                    
                    if (existingControl != null)
                    {
                        control = existingControl;
                        existingCell = tempExistingCell;
                        isUpdate = true;
                        controlId = ControlId;
                    }
                }
                else if (!string.IsNullOrEmpty(DataField))
                {
                    // Find by DataField (only if DataField is provided)
                    XElement existingControl;
                    XElement tempExistingCell;
                    
                    if (isHeaderControl)
                    {
                        (existingControl, tempExistingCell) = FormXmlHelper.FindControlInHeader(systemForm, dataField: DataField);
                    }
                    else
                    {
                        (existingControl, tempExistingCell) = FormXmlHelper.FindControlByDataField(systemForm, TabName, SectionName, DataField);
                    }
                    
                    if (existingControl != null)
                    {
                        control = existingControl;
                        existingCell = tempExistingCell;
                        isUpdate = true;
                        controlId = existingControl.Attribute("id")?.Value;
                    }
                }
                // else: No ControlId and no DataField (special controls) - will create new

                if (!isUpdate)
                {
                    WriteVerbose("DEBUG: Creating new control");
                    // Create new control
                    controlId = !string.IsNullOrEmpty(ControlId) ? ControlId : Guid.NewGuid().ToString();
                    control = new XElement("control");
                    // Position the control
                    PositionControl(rows, rowsElement, columnCount, ref targetRow, ref targetColumn, isUpdate, null, control, out cell);
                }
                else
                {
                    WriteVerbose("DEBUG: Updating existing control");
                    // For update, if position specified, move it
                    if (Row.HasValue && Column.HasValue)
                    {
                        // Remove from current position
                        control.Remove();
                        // Position at new location
                        PositionControl(rows, rowsElement, columnCount, ref targetRow, ref targetColumn, isUpdate, null, control, out cell);
                    }
                    else
                    {
                        // Keep in current position
                        cell = existingCell;
                    }
                }

                WriteVerbose("DEBUG: About to call UpdateControlAttributes");
                // Update control attributes with determined control type
                UpdateControlAttributes(control, finalControlType, controlId, cell);
                WriteVerbose("DEBUG: UpdateControlAttributes completed");
            }

            // Set cell ID
            if (!string.IsNullOrEmpty(CellId))
            {
                cell.SetAttributeValue("id", CellId);
            }
            else if (cell.Attribute("id") == null)
            {
                cell.SetAttributeValue("id", Guid.NewGuid().ToString());
            }

            // Set cell Auto attribute
            if (MyInvocation.BoundParameters.ContainsKey(nameof(Auto)))
            {
                if (Auto.IsPresent)
                {
                    cell.SetAttributeValue("auto", "true");
                }
                else
                {
                    cell.SetAttributeValue("auto", null);
                }
            }

            // Set cell LockLevel attribute
            if (LockLevel.HasValue)
            {
                cell.SetAttributeValue("locklevel", LockLevel.Value.ToString());
            }

            // Confirm action
            string action = isUpdate ? "Update" : "Create";
            string controlDescriptor = isUpdate 
                ? $"control '{ControlId}'" 
                : (!string.IsNullOrEmpty(DataField) ? $"control '{DataField}'" : $"control of type '{finalControlType}'");
            if (!ShouldProcess($"form '{FormId}', section '{SectionName}'", $"{action} {controlDescriptor}"))
            {
                WriteVerbose("DEBUG: ShouldProcess returned false, returning early");
                return;
            }

            WriteVerbose("DEBUG: About to update form");
            // Update the form
            FormXmlHelper.UpdateFormXml(Connection, FormId, doc);

            // Return the control ID if PassThru is specified
            if (PassThru.IsPresent)
            {
                WriteObject(controlId);
            }

            WriteVerbose($"TEMP DEBUG: {action}d {controlDescriptor} (ID: {controlId}) in form '{FormId}' - UpdateControlAttributes was called: {(string.IsNullOrEmpty(control?.Attribute("classid")?.Value) ? "NO" : "YES")}");
            WriteVerbose("DEBUG: ProcessRecord completed");
        }

        /// <summary>
        /// Determines the appropriate control type based on attribute metadata if not specified, and validates that the metadata exists.
        /// </summary>
        private string DetermineAndValidateControlType()
        {
            // Check if this is a special control type that doesn't require a DataField
            if (!string.IsNullOrEmpty(ControlType) && IsSpecialControlType(ControlType))
            {
                // Special controls (Subgrid, WebResource, etc.) don't need DataField or metadata validation
                WriteVerbose($"Special control type '{ControlType}' specified - skipping DataField validation");
                return ControlType;
            }

            if (string.IsNullOrEmpty(DataField))
            {
                throw new ArgumentException("DataField is required for attribute-bound controls (Standard, Lookup, OptionSet, DateTime, Boolean, Email, Memo, Money, Notes, Data). For special controls (Subgrid, WebResource, QuickForm, Spacer, IFrame, Timer, KBSearch), specify ControlType parameter.");
            }

            // First check if this is a virtual/computed field or relationship
            if (DataField.Contains('.'))
            {
                // This might be a relationship navigation (e.g., "primarycontactid.fullname")
                return ValidateAndDetermineRelationshipControlType();
            }

            // Retrieve attribute metadata using unpublished metadata
            AttributeMetadata attributeMetadata = GetAttributeMetadata(DataField);

            if (attributeMetadata == null)
            {
                // Maybe it's a relationship field, try to find it
                var relationshipControlType = TryDetermineRelationshipControlType(DataField);
                if (relationshipControlType != null)
                {
                    return relationshipControlType;
                }

                throw new InvalidOperationException($"Attribute or relationship '{DataField}' not found in entity '{_entityName}'. Please verify the field exists and try again.");
            }

            // If ControlType is specified, validate it's appropriate for the attribute type
            if (!string.IsNullOrEmpty(ControlType))
            {
                ValidateControlTypeForAttribute(ControlType, attributeMetadata);
                return ControlType;
            }

            // Auto-determine control type based on attribute metadata
            return DetermineControlTypeFromAttribute(attributeMetadata);
        }

        /// <summary>
        /// Determines if the control type is a special control that doesn't require a DataField.
        /// </summary>
        private bool IsSpecialControlType(string controlType)
        {
            // Special controls that are not bound to a specific attribute
            var specialControlTypes = new[] { "Subgrid", "WebResource", "QuickForm", "Spacer", "IFrame", "Timer", "KBSearch" };
            return specialControlTypes.Contains(controlType, StringComparer.OrdinalIgnoreCase);
        }

        /// <summary>
        /// Gets attribute metadata using unpublished metadata.
        /// </summary>
        private AttributeMetadata GetAttributeMetadata(string attributeName)
        {
            try
            {
                var request = new RetrieveAttributeRequest
                {
                    EntityLogicalName = _entityName,
                    LogicalName = attributeName,
                    RetrieveAsIfPublished = false  // Use unpublished metadata
                };

                var response = (RetrieveAttributeResponse)Connection.Execute(request);
                return response.AttributeMetadata;
            }
            catch (FaultException<OrganizationServiceFault> ex)
            {
                if (QueryHelpers.IsNotFoundException(ex))
                {
                    return null;
                }
                throw;
            }
        }

        /// <summary>
        /// Validates and determines control type for relationship navigation fields.
        /// </summary>
        private string ValidateAndDetermineRelationshipControlType()
        {
            // For relationship navigation fields (e.g., "primarycontactid.fullname"),
            // we'll default to "Standard" as they're typically display-only
            WriteVerbose($"DataField '{DataField}' appears to be a relationship navigation field. Using Standard control type.");
            return ControlType ?? "Standard";
        }

        /// <summary>
        /// Tries to determine if DataField is a relationship field and return appropriate control type.
        /// Supports lookup attributes (many-to-one), one-to-many, and many-to-many relationships.
        /// </summary>
        private string TryDetermineRelationshipControlType(string fieldName)
        {
            try
            {
                // Get entity metadata with relationships
                var request = new RetrieveEntityRequest
                {
                    LogicalName = _entityName,
                    EntityFilters = EntityFilters.Relationships,
                    RetrieveAsIfPublished = false
                };

                var response = (RetrieveEntityResponse)Connection.Execute(request);
                var entityMetadata = response.EntityMetadata;

                // Check if this field name matches a lookup attribute from many-to-one relationships
                var manyToOneRelationship = entityMetadata.ManyToOneRelationships
                    .FirstOrDefault(r => r.ReferencingAttribute == fieldName);

                if (manyToOneRelationship != null)
                {
                    WriteVerbose($"Field '{fieldName}' identified as many-to-one lookup relationship to '{manyToOneRelationship.ReferencedEntity}'");
                    return ControlType ?? "Lookup";
                }

                // Check if this is a one-to-many relationship name (should create a subgrid)
                var oneToManyRelationship = entityMetadata.OneToManyRelationships
                    .FirstOrDefault(r => r.SchemaName.Equals(fieldName, StringComparison.OrdinalIgnoreCase));

                if (oneToManyRelationship != null)
                {
                    WriteVerbose($"Field '{fieldName}' identified as one-to-many relationship to '{oneToManyRelationship.ReferencedEntity}' - using Subgrid control type");
                    return ControlType ?? "Subgrid";
                }

                // Check if this is a many-to-many relationship name (should create a subgrid)
                var manyToManyRelationship = entityMetadata.ManyToManyRelationships
                    .FirstOrDefault(r => r.SchemaName.Equals(fieldName, StringComparison.OrdinalIgnoreCase));

                if (manyToManyRelationship != null)
                {
                    WriteVerbose($"Field '{fieldName}' identified as many-to-many relationship '{manyToManyRelationship.SchemaName}' - using Subgrid control type");
                    return ControlType ?? "Subgrid";
                }

                return null;
            }
            catch (Exception ex)
            {
                WriteVerbose($"Failed to check relationship metadata: {ex.Message}");
                return null;
            }
        }

        /// <summary>
        /// Validates that the specified control type is appropriate for the attribute type.
        /// </summary>
        private void ValidateControlTypeForAttribute(string controlType, AttributeMetadata attributeMetadata)
        {
            string attributeTypeName = attributeMetadata.AttributeType?.ToString() ?? "Unknown";
            
            // Define valid control types for each attribute type
            var validControlTypes = GetValidControlTypesForAttribute(attributeMetadata);
            
            if (!validControlTypes.Contains(controlType, StringComparer.OrdinalIgnoreCase))
            {
                var validTypesString = string.Join(", ", validControlTypes);
                WriteWarning($"Control type '{controlType}' may not be appropriate for attribute type '{attributeTypeName}'. " +
                           $"Recommended control types: {validTypesString}");
            }
        }

        /// <summary>
        /// Determines the appropriate control type based on attribute metadata.
        /// </summary>
        private string DetermineControlTypeFromAttribute(AttributeMetadata attributeMetadata)
        {
            WriteVerbose($"Auto-determining control type for attribute '{attributeMetadata.LogicalName}' of type '{attributeMetadata.AttributeType}'");

            switch (attributeMetadata.AttributeType)
            {
                case AttributeTypeCode.Boolean:
                    return "Boolean";

                case AttributeTypeCode.DateTime:
                    return "DateTime";

                case AttributeTypeCode.Decimal:
                case AttributeTypeCode.Double:
                case AttributeTypeCode.Integer:
                case AttributeTypeCode.BigInt:
                    return "Standard";

                case AttributeTypeCode.Money:
                    return "Money";

                case AttributeTypeCode.Memo:
                    return "Memo";

                case AttributeTypeCode.String:
                    if (attributeMetadata is StringAttributeMetadata stringAttr)
                    {
                        switch (stringAttr.Format)
                        {
                            case Microsoft.Xrm.Sdk.Metadata.StringFormat.Email:
                                return "Email";
                            case Microsoft.Xrm.Sdk.Metadata.StringFormat.Url:
                            case Microsoft.Xrm.Sdk.Metadata.StringFormat.TextArea:
                                return "Standard";
                            default:
                                return "Standard";
                        }
                    }
                    return "Standard";

                case AttributeTypeCode.Picklist:
                    return "OptionSet";

                case AttributeTypeCode.Lookup:
                case AttributeTypeCode.Customer:
                case AttributeTypeCode.Owner:
                    return "Lookup";

                case AttributeTypeCode.Uniqueidentifier:
                    return "Standard";

                case AttributeTypeCode.State:
                case AttributeTypeCode.Status:
                    return "OptionSet";

                default:
                    // Check for MultiSelectPicklist by type name since it doesn't have a specific AttributeTypeCode
                    if (attributeMetadata is MultiSelectPicklistAttributeMetadata)
                    {
                        return "OptionSet";
                    }
                    
                    WriteVerbose($"Unknown attribute type '{attributeMetadata.AttributeType}', defaulting to Standard control");
                    return "Standard";
            }
        }

        /// <summary>
        /// Gets the list of valid control types for the given attribute.
        /// </summary>
        private string[] GetValidControlTypesForAttribute(AttributeMetadata attributeMetadata)
        {
            switch (attributeMetadata.AttributeType)
            {
                case AttributeTypeCode.Boolean:
                    return new[] { "Boolean", "Standard" };

                case AttributeTypeCode.DateTime:
                    return new[] { "DateTime", "Standard" };

                case AttributeTypeCode.Decimal:
                case AttributeTypeCode.Double:
                case AttributeTypeCode.Integer:
                case AttributeTypeCode.BigInt:
                    return new[] { "Standard" };

                case AttributeTypeCode.Money:
                    return new[] { "Money", "Standard" };

                case AttributeTypeCode.Memo:
                    return new[] { "Memo", "Standard", "Notes" };

                case AttributeTypeCode.String:
                    if (attributeMetadata is StringAttributeMetadata stringAttr && 
                        stringAttr.Format == Microsoft.Xrm.Sdk.Metadata.StringFormat.Email)
                    {
                        return new[] { "Email", "Standard" };
                    }
                    return new[] { "Standard", "Email" };

                case AttributeTypeCode.Picklist:
                case AttributeTypeCode.State:
                case AttributeTypeCode.Status:
                    return new[] { "OptionSet", "Standard" };

                case AttributeTypeCode.Lookup:
                case AttributeTypeCode.Customer:
                case AttributeTypeCode.Owner:
                    return new[] { "Lookup", "Standard" };

                case AttributeTypeCode.Uniqueidentifier:
                    return new[] { "Standard" };

                default:
                    // Check for MultiSelectPicklist by type name
                    if (attributeMetadata is MultiSelectPicklistAttributeMetadata)
                    {
                        return new[] { "OptionSet", "Standard" };
                    }
                    
                    return new[] { "Standard" };
            }
        }

        /// <summary>
        /// Updates control attributes with the determined control type.
        /// </summary>
        private void UpdateControlAttributes(XElement control, string finalControlType, string controlId, XElement cell)
        {
            WriteVerbose($"DEBUG: UpdateControlAttributes called - finalControlType='{finalControlType}', controlId='{controlId}'");
            
            control.SetAttributeValue("id", controlId);
            
            // Only set datafieldname if DataField is provided (special controls may not have a DataField)
            if (!string.IsNullOrEmpty(DataField))
            {
                control.SetAttributeValue("datafieldname", DataField);
            }

            // Set the class ID based on the determined control type
            var classId = GetClassId(finalControlType);
            WriteVerbose($"DEBUG: GetClassId returned '{classId}' for finalControlType '{finalControlType}'");
            
            control.SetAttributeValue("classid", classId);
            WriteVerbose($"DEBUG: Set classid attribute, control now has classid='{control.Attribute("classid")?.Value}'");

            // TEMPORARY DEBUG: Verify the classid was set
            var actualClassId = control.Attribute("classid")?.Value;
            if (string.IsNullOrEmpty(actualClassId))
            {
                throw new InvalidOperationException($"TEMP DEBUG: Failed to set classid. finalControlType='{finalControlType}', expectedClassId='{classId}'");
            }
            if (actualClassId != classId)
            {
                throw new InvalidOperationException($"TEMP DEBUG: classid mismatch. Expected='{classId}', Actual='{actualClassId}'");
            }
            WriteVerbose($"DEBUG: classid validation passed");

            // Set other control attributes as before...
            if (MyInvocation.BoundParameters.ContainsKey(nameof(Disabled)))
            {
                if (Disabled.IsPresent)
                {
                    control.SetAttributeValue("disabled", "true");
                }
                else
                {
                    control.SetAttributeValue("disabled", null);
                }
            }

            if (MyInvocation.BoundParameters.ContainsKey(nameof(Visible)))
            {
                if (!Visible.IsPresent)
                {
                    control.SetAttributeValue("visible", "false");
                }
                else
                {
                    control.SetAttributeValue("visible", null);
                }
            }

            if (MyInvocation.BoundParameters.ContainsKey(nameof(Hidden)))
            {
                if (Hidden.IsPresent)
                {
                    control.SetAttributeValue("visible", "false");
                }
                else
                {
                    control.SetAttributeValue("visible", null);
                }
            }

            if (MyInvocation.BoundParameters.ContainsKey(nameof(ShowLabel)))
            {
                if (!ShowLabel.IsPresent)
                {
                    control.SetAttributeValue("showlabel", "false");
                }
                else
                {
                    control.SetAttributeValue("showlabel", null);
                }
            }

            if (MyInvocation.BoundParameters.ContainsKey(nameof(IsRequired)))
            {
                if (IsRequired.IsPresent)
                {
                    control.SetAttributeValue("isrequired", "true");
                }
                else
                {
                    control.SetAttributeValue("isrequired", null);
                }
            }

            // Update cell attributes
            if (ColSpan.HasValue)
            {
                cell.SetAttributeValue("colspan", ColSpan.Value);
            }

            if (RowSpan.HasValue)
            {
                cell.SetAttributeValue("rowspan", RowSpan.Value);
            }

            // Update or add labels
            if (!string.IsNullOrEmpty(Label))
            {
                XElement labelsElement = control.Element("labels");
                if (labelsElement == null)
                {
                    labelsElement = new XElement("labels");
                    control.Add(labelsElement);
                }
                else
                {
                    labelsElement.RemoveAll();
                }
                
                labelsElement.Add(new XElement("label",
                    new XAttribute("description", Label),
                    new XAttribute("languagecode", LanguageCode)
                ));
            }

            // Add parameters for special controls
            if (Parameters != null && Parameters.Count > 0)
            {
                XElement paramsElement = control.Element("parameters");
                if (paramsElement == null)
                {
                    paramsElement = new XElement("parameters");
                    control.Add(paramsElement);
                }
                else
                {
                    paramsElement.RemoveAll();
                }

                foreach (System.Collections.DictionaryEntry param in Parameters)
                {
                    paramsElement.Add(new XElement(param.Key.ToString(), param.Value?.ToString()));
                }
            }
            
            WriteVerbose($"DEBUG: UpdateControlAttributes completed, final control XML: {control}");
        }

        private void PositionControl(System.Collections.Generic.List<XElement> rows, XElement rowsElement, int columnCount, ref int targetRow, ref int targetColumn, bool isUpdate, XElement existingCell, XElement control, out XElement cell)
        {
            // If targetRow is -1, determine based on update status
            if (targetRow == -1)
            {
                if (isUpdate && existingCell != null)
                {
                    // Find current position
                    XElement currentRow = existingCell.Parent;
                    var currentRowList = rows;
                    targetRow = currentRowList.IndexOf(currentRow);
                    var currentCellList = currentRow.Elements("cell").ToList();
                    targetColumn = currentCellList.IndexOf(existingCell);
                }
                else
                {
                    targetRow = rows.Count;
                }
            }

            // Add rows if necessary
            while (rows.Count <= targetRow)
            {
                var newRow = new XElement("row");
                for (int i = 0; i < columnCount; i++)
                {
                    newRow.Add(new XElement("cell"));
                }
                rowsElement.Add(newRow);
                rows.Add(newRow);
            }

            var targetRowElement = rows[targetRow];
            var cells = targetRowElement.Elements("cell").ToList();

            // Add cells if necessary
            while (cells.Count <= targetColumn)
            {
                var newCell = new XElement("cell");
                cells.Add(newCell);
                targetRowElement.Add(newCell);
            }

            cell = cells[targetColumn];

            // Remove any existing control
            cell.Elements("control").Remove();

            // Add the new control
            cell.Add(control);
        }

        private string GetClassId(string controlType)
        {
            // Class IDs for different control types
            switch (controlType)
            {
                case "Standard":
                    return "{4273EDBD-AC1D-40D3-9FB2-095C621B552D}"; // Standard text field
                case "Lookup":
                    return "{270BD3DB-D9AF-4782-9025-509E298DEC0A}"; // Lookup control
                case "OptionSet":
                    return "{3EF39988-22BB-4F0B-BBBE-64B5A3748AEE}"; // Option set (dropdown)
                case "DateTime":
                    return "{5B773807-9FB2-42DB-97C3-7A91EFF8ADFF}"; // Date and time picker
                case "Boolean":
                    return "{67FAC785-CD58-4F9F-ABB3-4B7DDC6ED5ED}"; // Two option (boolean)
                case "Subgrid":
                    return "{E7A81278-8635-4d9e-8D4D-59480B391C5B}"; // Subgrid
                case "WebResource":
                    return "{9FDF5F91-88B1-47f4-AD53-C11EFC01A01D}"; // Web resource
                case "QuickForm":
                    return "{5C5600E0-1D6E-4205-A272-BE80DA87FD42}"; // Quick view form
                case "Spacer":
                    return "{5546E6CD-394C-4BEE-94A8-4B09EB3A5C4A}"; // Spacer
                case "IFrame":
                    return "{FD2A7985-3187-444e-908D-6624B21F69C0}"; // IFrame
                case "Timer":
                    return "{B0C6723A-8503-4FD7-BB28-C8A06AC933C2}"; // Timer control
                case "KBSearch":
                    return "{5C5600E0-1D6E-4205-A272-BE80DA87FD42}"; // Knowledge base search
                case "Notes":
                    return "{06375649-c143-495e-a496-c962e5b4488e}"; // Notes control
                case "Email":
                    return "{ADA2203E-B4CD-49BE-9DDF-234642B43B52}"; // Email control
                case "Memo":
                    return "{E0DECE4B-6FC8-4A8F-A065-082708572369}"; // Memo control
                case "Money":
                    return "{533B9E00-756B-4312-95A0-DC888637AC78}"; // Money control
                case "Data":
                    return "{5546E6CD-394C-4BEE-94A8-4425E17EF6C6}"; // Data control
                default:
                    return "{4273EDBD-AC1D-40D3-9FB2-095C621B552D}"; // Default to standard
            }
        }
    }
}
