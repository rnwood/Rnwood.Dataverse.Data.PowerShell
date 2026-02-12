using Microsoft.Crm.Sdk.Messages;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Messages;
using Microsoft.Xrm.Sdk.Metadata;
using System;
using System.Collections;
using System.Linq;
using System.Management.Automation;
using System.ServiceModel;

namespace Rnwood.Dataverse.Data.PowerShell.Commands
{
    /// <summary>
    /// Creates or updates an attribute (column) in Dataverse.
    /// </summary>
    [Cmdlet(VerbsCommon.Set, "DataverseAttributeMetadata", SupportsShouldProcess = true, ConfirmImpact = ConfirmImpact.Medium)]
    [OutputType(typeof(PSObject))]
    public class SetDataverseAttributeMetadataCmdlet : OrganizationServiceCmdlet
    {
        private int _baseLanguageCode;

        /// <summary>
        /// Gets or sets the logical name of the entity.
        /// </summary>
        [Parameter(Mandatory = true, Position = 0, HelpMessage = "Logical name of the entity (table)")]
        [ArgumentCompleter(typeof(TableNameArgumentCompleter))]
        [Alias("TableName")]
        public string EntityName { get; set; }

        /// <summary>
        /// Gets or sets the logical name of the attribute.
        /// </summary>
        [Parameter(Mandatory = true, Position = 1, HelpMessage = "Logical name of the attribute (column)")]
        [Alias("ColumnName")]
        public string AttributeName { get; set; }

        /// <summary>
        /// Gets or sets the schema name of the attribute (used for create).
        /// </summary>
        [Parameter(HelpMessage = "Schema name of the attribute (required for create, e.g., 'new_CustomField')")]
        public string SchemaName { get; set; }

        /// <summary>
        /// Gets or sets the display name of the attribute.
        /// </summary>
        [Parameter(HelpMessage = "Display name of the attribute")]
        public string DisplayName { get; set; }

        /// <summary>
        /// Gets or sets the description of the attribute.
        /// </summary>
        [Parameter(HelpMessage = "Description of the attribute")]
        public string Description { get; set; }

        /// <summary>
        /// Gets or sets the type of attribute to create.
        /// </summary>
        [Parameter(HelpMessage = "Type of attribute (String, Memo, Integer, Decimal, Double, Money, DateTime, Boolean, Picklist, MultiSelectPicklist, Lookup, etc.)")]
        [ValidateSet("String", "Memo", "Integer", "BigInt", "Decimal", "Double", "Money", "DateTime", "Boolean", 
                     "Picklist", "MultiSelectPicklist", "Lookup", "File", "Image", "UniqueIdentifier")]
        public string AttributeType { get; set; }

        /// <summary>
        /// Gets or sets whether the attribute is required.
        /// </summary>
        [Parameter(HelpMessage = "Required level: None, SystemRequired, ApplicationRequired, Recommended")]
        [ValidateSet("None", "SystemRequired", "ApplicationRequired", "Recommended")]
        public string RequiredLevel { get; set; }

        /// <summary>
        /// Gets or sets whether the attribute is searchable.
        /// </summary>
        [Parameter(HelpMessage = "Whether the attribute is searchable")]
        public SwitchParameter IsSearchable { get; set; }

        /// <summary>
        /// Gets or sets whether the attribute is secured.
        /// </summary>
        [Parameter(HelpMessage = "Whether the attribute is secured (requires field-level security)")]
        public SwitchParameter IsSecured { get; set; }

        /// <summary>
        /// Gets or sets whether audit is enabled.
        /// </summary>
        [Parameter(HelpMessage = "Whether audit is enabled for this attribute")]
        public SwitchParameter IsAuditEnabled { get; set; }

        // String-specific properties
        /// <summary>
        /// Gets or sets the maximum length for string/memo attributes.
        /// </summary>
        [Parameter(HelpMessage = "Maximum length for String or Memo attributes")]
        public int? MaxLength { get; set; }

        /// <summary>
        /// Gets or sets the format for string attributes.
        /// </summary>
        [Parameter(HelpMessage = "Format for String attributes (Text, TextArea, Email, Url, TickerSymbol, Phone, PhoneticGuide, VersionNumber, Json)")]
        [ValidateSet("Text", "TextArea", "Email", "Url", "TickerSymbol", "Phone", "PhoneticGuide", "VersionNumber", "Json")]
        public string StringFormat { get; set; }

        // Numeric-specific properties
        /// <summary>
        /// Gets or sets the minimum value for numeric attributes.
        /// </summary>
        [Parameter(HelpMessage = "Minimum value for Integer, Decimal, Double, or Money attributes")]
        public object MinValue { get; set; }

        /// <summary>
        /// Gets or sets the maximum value for numeric attributes.
        /// </summary>
        [Parameter(HelpMessage = "Maximum value for Integer, Decimal, Double, or Money attributes")]
        public object MaxValue { get; set; }

        /// <summary>
        /// Gets or sets the precision for decimal/double/money attributes.
        /// </summary>
        [Parameter(HelpMessage = "Precision for Decimal, Double, or Money attributes")]
        public int? Precision { get; set; }

        // DateTime-specific properties
        /// <summary>
        /// Gets or sets the format for datetime attributes.
        /// </summary>
        [Parameter(HelpMessage = "Format for DateTime attributes (DateOnly, DateAndTime)")]
        [ValidateSet("DateOnly", "DateAndTime")]
        public string DateTimeFormat { get; set; }

        /// <summary>
        /// Gets or sets the behavior for datetime attributes.
        /// </summary>
        [Parameter(HelpMessage = "Behavior for DateTime attributes (UserLocal, DateOnly, TimeZoneIndependent)")]
        [ValidateSet("UserLocal", "DateOnly", "TimeZoneIndependent")]
        public string DateTimeBehavior { get; set; }

        // Boolean-specific properties
        /// <summary>
        /// Gets or sets the true option label for boolean attributes.
        /// </summary>
        [Parameter(HelpMessage = "Label for the true option in Boolean attributes")]
        public string TrueLabel { get; set; }

        /// <summary>
        /// Gets or sets the false option label for boolean attributes.
        /// </summary>
        [Parameter(HelpMessage = "Label for the false option in Boolean attributes")]
        public string FalseLabel { get; set; }

        /// <summary>
        /// Gets or sets the default value for boolean attributes.
        /// </summary>
        [Parameter(HelpMessage = "Default value for Boolean attributes")]
        public bool? DefaultValue { get; set; }

        // Picklist-specific properties
        /// <summary>
        /// Gets or sets the option set name for picklist attributes.
        /// </summary>
        [Parameter(HelpMessage = "Name of an existing global option set to use, or name for a new local option set")]
        public string OptionSetName { get; set; }

        /// <summary>
        /// Gets or sets the options for picklist attributes.
        /// </summary>
        [Parameter(HelpMessage = "Array of hashtables defining options: @(@{Value=1; Label='Option 1'}, @{Value=2; Label='Option 2'}). For statuscode attributes, include State: @{Value=1; Label='Draft'; State=0}")]
        public Hashtable[] Options { get; set; }

        // Lookup-specific properties
        /// <summary>
        /// Gets or sets the target entities for lookup attributes.
        /// </summary>
        [Parameter(HelpMessage = "Array of target entity logical names for Lookup attributes")]
        public string[] Targets { get; set; }

        /// <summary>
        /// Gets or sets the relationship schema name for lookup attributes.
        /// If not specified, a default name will be generated.
        /// </summary>
        [Parameter(HelpMessage = "Schema name for the relationship created with the Lookup attribute (e.g., 'new_project_contact'). If not specified, generates from entity names.")]
        public string RelationshipSchemaName { get; set; }

        /// <summary>
        /// Gets or sets the cascading behavior for assign operations (Lookup only).
        /// </summary>
        [Parameter(HelpMessage = "Cascade behavior for Assign: NoCascade, Cascade, Active, UserOwned, RemoveLink (Lookup only)")]
        [ValidateSet("NoCascade", "Cascade", "Active", "UserOwned", "RemoveLink")]
        public string CascadeAssign { get; set; }

        /// <summary>
        /// Gets or sets the cascading behavior for share operations (Lookup only).
        /// </summary>
        [Parameter(HelpMessage = "Cascade behavior for Share: NoCascade, Cascade, Active, UserOwned (Lookup only)")]
        [ValidateSet("NoCascade", "Cascade", "Active", "UserOwned")]
        public string CascadeShare { get; set; }

        /// <summary>
        /// Gets or sets the cascading behavior for unshare operations (Lookup only).
        /// </summary>
        [Parameter(HelpMessage = "Cascade behavior for Unshare: NoCascade, Cascade, Active, UserOwned (Lookup only)")]
        [ValidateSet("NoCascade", "Cascade", "Active", "UserOwned")]
        public string CascadeUnshare { get; set; }

        /// <summary>
        /// Gets or sets the cascading behavior for reparent operations (Lookup only).
        /// </summary>
        [Parameter(HelpMessage = "Cascade behavior for Reparent: NoCascade, Cascade, Active, UserOwned, RemoveLink (Lookup only)")]
        [ValidateSet("NoCascade", "Cascade", "Active", "UserOwned", "RemoveLink")]
        public string CascadeReparent { get; set; }

        /// <summary>
        /// Gets or sets the cascading behavior for delete operations (Lookup only).
        /// </summary>
        [Parameter(HelpMessage = "Cascade behavior for Delete: NoCascade, RemoveLink, Restrict, Cascade (Lookup only)")]
        [ValidateSet("NoCascade", "RemoveLink", "Restrict", "Cascade")]
        public string CascadeDelete { get; set; }

        /// <summary>
        /// Gets or sets the cascading behavior for merge operations (Lookup only).
        /// </summary>
        [Parameter(HelpMessage = "Cascade behavior for Merge: NoCascade, Cascade (Lookup only)")]
        [ValidateSet("NoCascade", "Cascade")]
        public string CascadeMerge { get; set; }

        // File/Image-specific properties
        /// <summary>
        /// Gets or sets the maximum size in KB for file/image attributes.
        /// </summary>
        [Parameter(HelpMessage = "Maximum size in KB for File or Image attributes")]
        public int? MaxSizeInKB { get; set; }

        /// <summary>
        /// Gets or sets whether to return the created/updated attribute metadata.
        /// </summary>
        [Parameter(HelpMessage = "Return the created or updated attribute metadata")]
        public SwitchParameter PassThru { get; set; }

        /// <summary>
        /// Gets or sets whether to publish the attribute after creating or updating.
        /// </summary>
        [Parameter(HelpMessage = "If specified, publishes the attribute after creating or updating")]
        public SwitchParameter Publish { get; set; }

        /// <summary>
        /// Processes the cmdlet.
        /// </summary>
        protected override void ProcessRecord()
        {
            base.ProcessRecord();

            _baseLanguageCode = GetBaseLanguageCode();

            // Check if attribute exists
            AttributeMetadata existingAttribute = null;
            bool attributeExists = false;

            try
            {
                var retrieveRequest = new RetrieveAttributeRequest
                {
                    EntityLogicalName = EntityName,
                    LogicalName = AttributeName,
                    RetrieveAsIfPublished = true
                };

                var retrieveResponse = (RetrieveAttributeResponse)Connection.Execute(retrieveRequest);
                existingAttribute = retrieveResponse.AttributeMetadata;
                attributeExists = true;
                WriteVerbose($"Attribute '{AttributeName}' already exists on entity '{EntityName}'");
            }
            catch (FaultException<OrganizationServiceFault> ex)
            {
                if (QueryHelpers.IsNotFoundException(ex))
                {
                    WriteVerbose($"Attribute '{AttributeName}' does not exist - will create");
                }
                else
                {
                    throw;
                }
            }

            if (attributeExists)
            {
                // Update existing attribute
                UpdateAttribute(existingAttribute);
            }
            else
            {
                // Create new attribute
                CreateAttribute();
            }
        }

        private void CreateAttribute()
        {
            if (string.IsNullOrWhiteSpace(AttributeType))
            {
                ThrowTerminatingError(new ErrorRecord(
                    new ArgumentException("AttributeType is required when creating a new attribute"),
                    "AttributeTypeRequired",
                    ErrorCategory.InvalidArgument,
                    null));
                return;
            }

            if (string.IsNullOrWhiteSpace(SchemaName))
            {
                ThrowTerminatingError(new ErrorRecord(
                    new ArgumentException("SchemaName is required when creating a new attribute"),
                    "SchemaNameRequired",
                    ErrorCategory.InvalidArgument,
                    null));
                return;
            }

            // Special handling for lookup attributes - they use a different creation method
            if (AttributeType?.ToLowerInvariant() == "lookup")
            {
                CreateLookupAttributeWithRelationship();
                return;
            }

            AttributeMetadata attributeMetadata = CreateAttributeMetadata();

            var request = new CreateAttributeRequest
            {
                EntityName = EntityName,
                Attribute = attributeMetadata
            };

            if (!ShouldProcess($"Entity '{EntityName}'", $"Create attribute '{SchemaName}' of type '{AttributeType}'"))
            {
                return;
            }

            WriteVerbose($"Creating attribute '{SchemaName}' on entity '{EntityName}'");

            var response = (CreateAttributeResponse)Connection.Execute(request);

            WriteVerbose($"Attribute created successfully with MetadataId: {response.AttributeId}");

            // Invalidate cache for this entity
            InvalidateEntityCache();

            // Publish the attribute if specified
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
                // Retrieve and return the created attribute
                var retrieveRequest = new RetrieveAttributeRequest
                {
                    EntityLogicalName = EntityName,
                    LogicalName = AttributeName,
                    RetrieveAsIfPublished = true
                };

                var retrieveResponse = (RetrieveAttributeResponse)Connection.Execute(retrieveRequest);
                var result = ConvertAttributeMetadataToPSObject(retrieveResponse.AttributeMetadata);
                WriteObject(result);
            }
        }

        private void CreateLookupAttributeWithRelationship()
        {
            if (Targets == null || Targets.Length == 0)
            {
                ThrowTerminatingError(new ErrorRecord(
                    new ArgumentException("Targets must be specified for Lookup attributes"),
                    "TargetsRequired",
                    ErrorCategory.InvalidArgument,
                    null));
                return;
            }

            // For single-target lookups, we can create using CreateOneToManyRequest
            // For multi-target (polymorphic) lookups, we need more complex logic
            if (Targets.Length > 1)
            {
                // Multi-target lookups require special handling
                ThrowTerminatingError(new ErrorRecord(
                    new NotImplementedException("Multi-target (polymorphic) lookup creation is not yet supported. Please use Set-DataverseRelationshipMetadata to create the relationship and lookup together, or create each single-target relationship separately."),
                    "PolymorphicLookupNotSupported",
                    ErrorCategory.NotImplemented,
                    null));
                return;
            }

            string targetEntity = Targets[0];
            string referencingEntity = EntityName;
            
            // Generate relationship schema name if not provided
            string relationshipSchemaName = RelationshipSchemaName;
            if (string.IsNullOrWhiteSpace(relationshipSchemaName))
            {
                // Generate default: referenced_referencing_attributeschemaname
                relationshipSchemaName = $"{targetEntity}_{referencingEntity}_{SchemaName}";
            }

            if (!ShouldProcess($"Entity '{EntityName}'", $"Create lookup attribute '{SchemaName}' with relationship '{relationshipSchemaName}' to '{targetEntity}'"))
            {
                return;
            }

            var lookup = new LookupAttributeMetadata
            {
                SchemaName = SchemaName,
                LogicalName = AttributeName,
                DisplayName = new Label(DisplayName ?? SchemaName, _baseLanguageCode),
                Description = string.IsNullOrWhiteSpace(Description) 
                    ? new Label(string.Empty, _baseLanguageCode) 
                    : new Label(Description, _baseLanguageCode)
            };

            // Set required level
            if (!string.IsNullOrWhiteSpace(RequiredLevel))
            {
                lookup.RequiredLevel = new AttributeRequiredLevelManagedProperty(
                    (AttributeRequiredLevel)Enum.Parse(typeof(AttributeRequiredLevel), RequiredLevel));
            }
            else
            {
                lookup.RequiredLevel = new AttributeRequiredLevelManagedProperty(AttributeRequiredLevel.None);
            }

            // Set searchable
            if (IsSearchable.IsPresent)
            {
                lookup.IsValidForAdvancedFind = new BooleanManagedProperty(IsSearchable.ToBool());
            }

            var relationship = new OneToManyRelationshipMetadata
            {
                SchemaName = relationshipSchemaName,
                ReferencedEntity = targetEntity,
                ReferencingEntity = referencingEntity,
                CascadeConfiguration = new CascadeConfiguration
                {
                    Assign = ParseCascadeType(CascadeAssign ?? "NoCascade"),
                    Share = ParseCascadeType(CascadeShare ?? "NoCascade"),
                    Unshare = ParseCascadeType(CascadeUnshare ?? "NoCascade"),
                    Reparent = ParseCascadeType(CascadeReparent ?? "NoCascade"),
                    Delete = ParseCascadeType(CascadeDelete ?? "RemoveLink"),
                    Merge = ParseCascadeType(CascadeMerge ?? "NoCascade")
                }
            };

            var request = new CreateOneToManyRequest
            {
                Lookup = lookup,
                OneToManyRelationship = relationship
            };

            WriteVerbose($"Creating lookup attribute '{SchemaName}' with relationship '{relationshipSchemaName}' from {targetEntity} to {referencingEntity}");

            var response = (CreateOneToManyResponse)Connection.Execute(request);

            WriteVerbose($"Lookup attribute and relationship created successfully with AttributeId: {response.AttributeId}, RelationshipId: {response.RelationshipId}");

            // Invalidate cache for this entity
            InvalidateEntityCache();

            // Publish the attribute if specified
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
                // Retrieve and return the created attribute
                var retrieveRequest = new RetrieveAttributeRequest
                {
                    EntityLogicalName = EntityName,
                    LogicalName = AttributeName,
                    RetrieveAsIfPublished = true
                };

                var retrieveResponse = (RetrieveAttributeResponse)Connection.Execute(retrieveRequest);
                var result = ConvertAttributeMetadataToPSObject(retrieveResponse.AttributeMetadata);
                WriteObject(result);
            }
        }

        private AttributeMetadata CreateAttributeMetadata()
        {
            AttributeMetadata attribute = null;

            switch (AttributeType?.ToLowerInvariant())
            {
                case "string":
                    attribute = CreateStringAttribute();
                    break;
                case "memo":
                    attribute = CreateMemoAttribute();
                    break;
                case "integer":
                    attribute = CreateIntegerAttribute();
                    break;
                case "bigint":
                    attribute = CreateBigIntAttribute();
                    break;
                case "decimal":
                    attribute = CreateDecimalAttribute();
                    break;
                case "double":
                    attribute = CreateDoubleAttribute();
                    break;
                case "money":
                    attribute = CreateMoneyAttribute();
                    break;
                case "datetime":
                    attribute = CreateDateTimeAttribute();
                    break;
                case "boolean":
                    attribute = CreateBooleanAttribute();
                    break;
                case "picklist":
                    attribute = CreatePicklistAttribute();
                    break;
                case "multiselectpicklist":
                    attribute = CreateMultiSelectPicklistAttribute();
                    break;
                case "file":
                    attribute = CreateFileAttribute();
                    break;
                case "image":
                    attribute = CreateImageAttribute();
                    break;
                case "uniqueidentifier":
                    attribute = CreateUniqueIdentifierAttribute();
                    break;
                case "lookup":
                    // Lookup attributes are handled separately in CreateLookupAttributeWithRelationship()
                    // This case should not be reached because CreateAttribute() redirects lookup creation
                    ThrowTerminatingError(new ErrorRecord(
                        new InvalidOperationException("Lookup attributes should be created via CreateLookupAttributeWithRelationship()"),
                        "UnexpectedLookupFlow",
                        ErrorCategory.InvalidOperation,
                        AttributeType));
                    return null;
                default:
                    ThrowTerminatingError(new ErrorRecord(
                        new ArgumentException($"Unsupported attribute type: {AttributeType}"),
                        "UnsupportedAttributeType",
                        ErrorCategory.InvalidArgument,
                        AttributeType));
                    return null;
            }

            // Set common properties
            attribute.LogicalName = AttributeName;
            attribute.SchemaName = SchemaName;

            if (!string.IsNullOrWhiteSpace(DisplayName))
            {
                attribute.DisplayName = new Label(DisplayName, _baseLanguageCode);
            }

            if (!string.IsNullOrWhiteSpace(Description))
            {
                attribute.Description = new Label(Description, _baseLanguageCode);
            }

            if (!string.IsNullOrWhiteSpace(RequiredLevel))
            {
                attribute.RequiredLevel = new AttributeRequiredLevelManagedProperty(
                    (AttributeRequiredLevel)Enum.Parse(typeof(AttributeRequiredLevel), RequiredLevel));
            }

            if (IsSearchable.IsPresent)
            {
                attribute.IsValidForAdvancedFind = new BooleanManagedProperty(IsSearchable.ToBool());
            }

            if (IsSecured.IsPresent)
            {
                attribute.IsSecured = IsSecured.ToBool();
            }

            if (IsAuditEnabled.IsPresent)
            {
                attribute.IsAuditEnabled = new BooleanManagedProperty(IsAuditEnabled.ToBool());
            }

            return attribute;
        }

        private StringAttributeMetadata CreateStringAttribute()
        {
            var attr = new StringAttributeMetadata
            {
                MaxLength = MaxLength ?? 100
            };

            if (!string.IsNullOrWhiteSpace(StringFormat))
            {
                attr.Format = (Microsoft.Xrm.Sdk.Metadata.StringFormat)Enum.Parse(
                    typeof(Microsoft.Xrm.Sdk.Metadata.StringFormat), StringFormat);
            }

            return attr;
        }

        private MemoAttributeMetadata CreateMemoAttribute()
        {
            return new MemoAttributeMetadata
            {
                MaxLength = MaxLength ?? 2000
            };
        }

        private IntegerAttributeMetadata CreateIntegerAttribute()
        {
            var attr = new IntegerAttributeMetadata();

            if (MinValue != null)
            {
                attr.MinValue = Convert.ToInt32(MinValue);
            }

            if (MaxValue != null)
            {
                attr.MaxValue = Convert.ToInt32(MaxValue);
            }

            return attr;
        }

        private BigIntAttributeMetadata CreateBigIntAttribute()
        {
            return new BigIntAttributeMetadata();
        }

        private DecimalAttributeMetadata CreateDecimalAttribute()
        {
            var attr = new DecimalAttributeMetadata
            {
                Precision = Precision ?? 2
            };

            if (MinValue != null)
            {
                attr.MinValue = Convert.ToDecimal(MinValue);
            }

            if (MaxValue != null)
            {
                attr.MaxValue = Convert.ToDecimal(MaxValue);
            }

            return attr;
        }

        private DoubleAttributeMetadata CreateDoubleAttribute()
        {
            var attr = new DoubleAttributeMetadata
            {
                Precision = Precision ?? 2
            };

            if (MinValue != null)
            {
                attr.MinValue = Convert.ToDouble(MinValue);
            }

            if (MaxValue != null)
            {
                attr.MaxValue = Convert.ToDouble(MaxValue);
            }

            return attr;
        }

        private MoneyAttributeMetadata CreateMoneyAttribute()
        {
            var attr = new MoneyAttributeMetadata
            {
                Precision = Precision ?? 2
            };

            if (MinValue != null)
            {
                attr.MinValue = Convert.ToDouble(MinValue);
            }

            if (MaxValue != null)
            {
                attr.MaxValue = Convert.ToDouble(MaxValue);
            }

            return attr;
        }

        private DateTimeAttributeMetadata CreateDateTimeAttribute()
        {
            var attr = new DateTimeAttributeMetadata();

            if (!string.IsNullOrWhiteSpace(DateTimeFormat))
            {
                attr.Format = DateTimeFormat.ToLowerInvariant() == "dateonly" 
                    ? Microsoft.Xrm.Sdk.Metadata.DateTimeFormat.DateOnly 
                    : Microsoft.Xrm.Sdk.Metadata.DateTimeFormat.DateAndTime;
            }

            if (!string.IsNullOrWhiteSpace(DateTimeBehavior))
            {
                // DateTimeBehavior is a property, not a constructor parameter
                // Valid values: UserLocal, DateOnly, TimeZoneIndependent
                switch (DateTimeBehavior.ToLowerInvariant())
                {
                    case "userlocal":
                        attr.DateTimeBehavior = new DateTimeBehavior { Value = "UserLocal" };
                        break;
                    case "dateonly":
                        attr.DateTimeBehavior = new DateTimeBehavior { Value = "DateOnly" };
                        break;
                    case "timezoneindependent":
                        attr.DateTimeBehavior = new DateTimeBehavior { Value = "TimeZoneIndependent" };
                        break;
                }
            }

            return attr;
        }

        private BooleanAttributeMetadata CreateBooleanAttribute()
        {
            var attr = new BooleanAttributeMetadata();

            var trueOption = new OptionMetadata(new Label(TrueLabel ?? "Yes", _baseLanguageCode), 1);
            var falseOption = new OptionMetadata(new Label(FalseLabel ?? "No", _baseLanguageCode), 0);

            attr.OptionSet = new BooleanOptionSetMetadata(trueOption, falseOption);

            if (DefaultValue.HasValue)
            {
                attr.DefaultValue = DefaultValue.Value;
            }

            return attr;
        }

        private PicklistAttributeMetadata CreatePicklistAttribute()
        {
            var attr = new PicklistAttributeMetadata();

            if (!string.IsNullOrWhiteSpace(OptionSetName))
            {
                // Use existing global option set
                attr.OptionSet = new OptionSetMetadata
                {
                    Name = OptionSetName,
                    IsGlobal = true
                };
            }
            else if (Options != null && Options.Length > 0)
            {
                // Create new local option set
                var optionSet = new OptionSetMetadata
                {
                    IsGlobal = false,
                    OptionSetType = OptionSetType.Picklist
                };

                foreach (var option in Options)
                {
                    var value = option["Value"] != null ? Convert.ToInt32(option["Value"]) : (int?)null;
                    var label = option["Label"] as string;

                    if (string.IsNullOrWhiteSpace(label))
                    {
                        continue;
                    }

                    OptionMetadata optionMetadata;
                    
                    // Check if this is a status option with State property
                    if (option.ContainsKey("State") && option["State"] != null)
                    {
                        var state = Convert.ToInt32(option["State"]);
                        var statusOption = new StatusOptionMetadata
                        {
                            Value = value,
                            Label = new Label(label, _baseLanguageCode),
                            State = state
                        };
                        optionMetadata = statusOption;
                    }
                    else
                    {
                        optionMetadata = value.HasValue 
                            ? new OptionMetadata(new Label(label, _baseLanguageCode), value.Value)
                            : new OptionMetadata(new Label(label, _baseLanguageCode), null);
                    }

                    optionSet.Options.Add(optionMetadata);
                }

                attr.OptionSet = optionSet;
            }
            else
            {
                ThrowTerminatingError(new ErrorRecord(
                    new ArgumentException("Either OptionSetName or Options must be specified for Picklist attributes"),
                    "OptionSetRequired",
                    ErrorCategory.InvalidArgument,
                    null));
            }

            return attr;
        }

        private MultiSelectPicklistAttributeMetadata CreateMultiSelectPicklistAttribute()
        {
            var attr = new MultiSelectPicklistAttributeMetadata();

            if (!string.IsNullOrWhiteSpace(OptionSetName))
            {
                // Use existing global option set
                attr.OptionSet = new OptionSetMetadata
                {
                    Name = OptionSetName,
                    IsGlobal = true
                };
            }
            else if (Options != null && Options.Length > 0)
            {
                // Create new local option set
                var optionSet = new OptionSetMetadata
                {
                    IsGlobal = false,
                    OptionSetType = OptionSetType.Picklist
                };

                foreach (var option in Options)
                {
                    var value = option["Value"] != null ? Convert.ToInt32(option["Value"]) : (int?)null;
                    var label = option["Label"] as string;

                    if (string.IsNullOrWhiteSpace(label))
                    {
                        continue;
                    }

                    var optionMetadata = value.HasValue 
                        ? new OptionMetadata(new Label(label, _baseLanguageCode), value.Value)
                        : new OptionMetadata(new Label(label, _baseLanguageCode), null);

                    optionSet.Options.Add(optionMetadata);
                }

                attr.OptionSet = optionSet;
            }
            else
            {
                ThrowTerminatingError(new ErrorRecord(
                    new ArgumentException("Either OptionSetName or Options must be specified for MultiSelectPicklist attributes"),
                    "OptionSetRequired",
                    ErrorCategory.InvalidArgument,
                    null));
            }

            return attr;
        }

        private CascadeType ParseCascadeType(string cascadeType)
        {
            if (string.IsNullOrWhiteSpace(cascadeType))
            {
                return CascadeType.NoCascade;
            }

            switch (cascadeType)
            {
                case "NoCascade":
                    return CascadeType.NoCascade;
                case "Cascade":
                    return CascadeType.Cascade;
                case "Active":
                    return CascadeType.Active;
                case "UserOwned":
                    return CascadeType.UserOwned;
                case "RemoveLink":
                    return CascadeType.RemoveLink;
                case "Restrict":
                    return CascadeType.Restrict;
                default:
                    return CascadeType.NoCascade;
            }
        }

        private FileAttributeMetadata CreateFileAttribute()
        {
            return new FileAttributeMetadata
            {
                MaxSizeInKB = MaxSizeInKB ?? 32768
            };
        }

        private ImageAttributeMetadata CreateImageAttribute()
        {
            return new ImageAttributeMetadata
            {
                MaxSizeInKB = MaxSizeInKB ?? 10240
            };
        }

        private UniqueIdentifierAttributeMetadata CreateUniqueIdentifierAttribute()
        {
            return new UniqueIdentifierAttributeMetadata();
        }

        private void UpdateAttribute(AttributeMetadata existingAttribute)
        {
            // Validate immutable properties first
            ValidateImmutableAttributeProperties(existingAttribute);

            // Clone the existing attribute
            var attributeToUpdate = CloneAttributeForUpdate(existingAttribute);

            bool hasChanges = false;

            // Update display name
            if (!string.IsNullOrWhiteSpace(DisplayName))
            {
                attributeToUpdate.DisplayName = new Label(DisplayName, _baseLanguageCode);
                hasChanges = true;
            }

            // Update description
            if (!string.IsNullOrWhiteSpace(Description))
            {
                attributeToUpdate.Description = new Label(Description, _baseLanguageCode);
                hasChanges = true;
            }

            // Update required level
            if (!string.IsNullOrWhiteSpace(RequiredLevel))
            {
                attributeToUpdate.RequiredLevel = new AttributeRequiredLevelManagedProperty(
                    (AttributeRequiredLevel)Enum.Parse(typeof(AttributeRequiredLevel), RequiredLevel));
                hasChanges = true;
            }

            // Update searchable
            if (IsSearchable.IsPresent)
            {
                attributeToUpdate.IsValidForAdvancedFind = new BooleanManagedProperty(IsSearchable.ToBool());
                hasChanges = true;
            }

            // Update audit enabled
            if (IsAuditEnabled.IsPresent)
            {
                attributeToUpdate.IsAuditEnabled = new BooleanManagedProperty(IsAuditEnabled.ToBool());
                hasChanges = true;
            }

            // Update type-specific properties
            hasChanges |= UpdateTypeSpecificProperties(attributeToUpdate);

            if (!hasChanges)
            {
                WriteWarning("No changes specified for update");
                
                if (PassThru)
                {
                    var result = ConvertAttributeMetadataToPSObject(existingAttribute);
                    WriteObject(result);
                }
                return;
            }

            var request = new UpdateAttributeRequest
            {
                EntityName = EntityName,
                Attribute = attributeToUpdate
            };

            if (!ShouldProcess($"Entity '{EntityName}'", $"Update attribute '{AttributeName}'"))
            {
                return;
            }

            WriteVerbose($"Updating attribute '{AttributeName}' on entity '{EntityName}'");

            Connection.Execute(request);

            WriteVerbose($"Attribute updated successfully");

            // Invalidate cache for this entity
            InvalidateEntityCache();

            // Publish the attribute if specified
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
                // Retrieve and return the updated attribute
                var retrieveRequest = new RetrieveAttributeRequest
                {
                    EntityLogicalName = EntityName,
                    LogicalName = AttributeName,
                    RetrieveAsIfPublished = true
                };

                var retrieveResponse = (RetrieveAttributeResponse)Connection.Execute(retrieveRequest);
                var result = ConvertAttributeMetadataToPSObject(retrieveResponse.AttributeMetadata);
                WriteObject(result);
            }
        }

        private void ValidateImmutableAttributeProperties(AttributeMetadata existingAttribute)
        {
            // Check if SchemaName was provided and is different (immutable)
            if (MyInvocation.BoundParameters.ContainsKey(nameof(SchemaName)) &&
                !string.Equals(SchemaName, existingAttribute.SchemaName, StringComparison.OrdinalIgnoreCase))
            {
                ThrowTerminatingError(new ErrorRecord(
                    new InvalidOperationException($"Cannot change SchemaName from '{existingAttribute.SchemaName}' to '{SchemaName}'. This property is immutable after creation."),
                    "ImmutableSchemaName",
                    ErrorCategory.InvalidOperation,
                    SchemaName));
            }

            // Check if AttributeType was provided and is different (immutable)
            if (MyInvocation.BoundParameters.ContainsKey(nameof(AttributeType)))
            {
                var existingTypeName = existingAttribute.AttributeType?.ToString() ?? existingAttribute.GetType().Name.Replace("AttributeMetadata", "");
                if (!string.Equals(AttributeType, existingTypeName, StringComparison.OrdinalIgnoreCase))
                {
                    ThrowTerminatingError(new ErrorRecord(
                        new InvalidOperationException($"Cannot change AttributeType from '{existingTypeName}' to '{AttributeType}'. This property is immutable after creation."),
                        "ImmutableAttributeType",
                        ErrorCategory.InvalidOperation,
                        AttributeType));
                }
            }

            // Check if IsSecured was provided and is different (cannot be changed via UpdateAttribute)
            if (MyInvocation.BoundParameters.ContainsKey(nameof(IsSecured)) &&
                IsSecured.ToBool() != (existingAttribute.IsSecured ?? false))
            {
                ThrowTerminatingError(new ErrorRecord(
                    new InvalidOperationException($"Cannot change IsSecured from '{existingAttribute.IsSecured ?? false}' to '{IsSecured.ToBool()}'. Field-level security settings must be configured separately through security profiles."),
                    "ImmutableIsSecured",
                    ErrorCategory.InvalidOperation,
                    null));
            }

            // Validate type-specific immutable properties
            if (existingAttribute is StringAttributeMetadata stringAttr)
            {
                if (MyInvocation.BoundParameters.ContainsKey(nameof(StringFormat)))
                {
                    var currentFormat = stringAttr.Format?.ToString() ?? "Text";
                    if (!string.Equals(StringFormat, currentFormat, StringComparison.OrdinalIgnoreCase))
                    {
                        ThrowTerminatingError(new ErrorRecord(
                            new InvalidOperationException($"Cannot change StringFormat from '{currentFormat}' to '{StringFormat}'. This property is immutable after creation."),
                            "ImmutableStringFormat",
                            ErrorCategory.InvalidOperation,
                            StringFormat));
                    }
                }
            }
            else if (existingAttribute is DateTimeAttributeMetadata dateTimeAttr)
            {
                if (MyInvocation.BoundParameters.ContainsKey(nameof(DateTimeFormat)))
                {
                    var currentFormat = dateTimeAttr.Format?.ToString() ?? "DateAndTime";
                    if (!string.Equals(DateTimeFormat, currentFormat, StringComparison.OrdinalIgnoreCase))
                    {
                        ThrowTerminatingError(new ErrorRecord(
                            new InvalidOperationException($"Cannot change DateTimeFormat from '{currentFormat}' to '{DateTimeFormat}'. This property is immutable after creation."),
                            "ImmutableDateTimeFormat",
                            ErrorCategory.InvalidOperation,
                            DateTimeFormat));
                    }
                }

                if (MyInvocation.BoundParameters.ContainsKey(nameof(DateTimeBehavior)))
                {
                    var currentBehavior = dateTimeAttr.DateTimeBehavior?.Value ?? "UserLocal";
                    if (!string.Equals(DateTimeBehavior, currentBehavior, StringComparison.OrdinalIgnoreCase))
                    {
                        ThrowTerminatingError(new ErrorRecord(
                            new InvalidOperationException($"Cannot change DateTimeBehavior from '{currentBehavior}' to '{DateTimeBehavior}'. This property is immutable after creation."),
                            "ImmutableDateTimeBehavior",
                            ErrorCategory.InvalidOperation,
                            DateTimeBehavior));
                    }
                }
            }
            else if (existingAttribute is BooleanAttributeMetadata booleanAttr)
            {
                if (MyInvocation.BoundParameters.ContainsKey(nameof(TrueLabel)))
                {
                    var existingTrueLabel = booleanAttr.OptionSet?.TrueOption?.Label?.UserLocalizedLabel?.Label ?? "Yes";
                    if (!string.Equals(TrueLabel, existingTrueLabel, StringComparison.OrdinalIgnoreCase))
                    {
                        ThrowTerminatingError(new ErrorRecord(
                            new InvalidOperationException($"Cannot change TrueLabel from '{existingTrueLabel}' to '{TrueLabel}'. To update option set labels, use Set-DataverseOptionSetMetadata."),
                            "ImmutableBooleanLabels",
                            ErrorCategory.InvalidOperation,
                            null));
                    }
                }

                if (MyInvocation.BoundParameters.ContainsKey(nameof(FalseLabel)))
                {
                    var existingFalseLabel = booleanAttr.OptionSet?.FalseOption?.Label?.UserLocalizedLabel?.Label ?? "No";
                    if (!string.Equals(FalseLabel, existingFalseLabel, StringComparison.OrdinalIgnoreCase))
                    {
                        ThrowTerminatingError(new ErrorRecord(
                            new InvalidOperationException($"Cannot change FalseLabel from '{existingFalseLabel}' to '{FalseLabel}'. To update option set labels, use Set-DataverseOptionSetMetadata."),
                            "ImmutableBooleanLabels",
                            ErrorCategory.InvalidOperation,
                            null));
                    }
                }

                if (MyInvocation.BoundParameters.ContainsKey(nameof(DefaultValue)) &&
                    DefaultValue != booleanAttr.DefaultValue)
                {
                    ThrowTerminatingError(new ErrorRecord(
                        new InvalidOperationException($"Cannot change DefaultValue from '{booleanAttr.DefaultValue}' to '{DefaultValue}'. This property is immutable for boolean attributes."),
                        "ImmutableDefaultValue",
                        ErrorCategory.InvalidOperation,
                        null));
                }
            }
            else
            {
                PicklistAttributeMetadata picklistAttr = existingAttribute as PicklistAttributeMetadata;
                MultiSelectPicklistAttributeMetadata multiPicklistAttr = existingAttribute as MultiSelectPicklistAttributeMetadata;
                StatusAttributeMetadata statusAttr = existingAttribute as StatusAttributeMetadata;
                LookupAttributeMetadata lookupAttr = existingAttribute as LookupAttributeMetadata;
                
                if (picklistAttr != null || multiPicklistAttr != null)
                {
                    var optionSet = picklistAttr?.OptionSet ?? multiPicklistAttr?.OptionSet;
                    if (MyInvocation.BoundParameters.ContainsKey(nameof(OptionSetName)) &&
                        !string.Equals(OptionSetName, optionSet?.Name, StringComparison.OrdinalIgnoreCase))
                    {
                        ThrowTerminatingError(new ErrorRecord(
                            new InvalidOperationException($"Cannot change OptionSetName from '{optionSet?.Name}' to '{OptionSetName}'. To update option set values, use Set-DataverseOptionSetMetadata."),
                            "ImmutableOptionSetName",
                            ErrorCategory.InvalidOperation,
                            OptionSetName));
                    }

                    if (MyInvocation.BoundParameters.ContainsKey(nameof(Options)))
                    {
                        ThrowTerminatingError(new ErrorRecord(
                            new InvalidOperationException($"Cannot change Options after creation. To update option set values, use Set-DataverseOptionSetMetadata."),
                            "ImmutableOptions",
                            ErrorCategory.InvalidOperation,
                            null));
                    }
                }
                else if (statusAttr != null)
                {
                    // StatusAttributeMetadata allows updating options with State property
                    if (MyInvocation.BoundParameters.ContainsKey(nameof(OptionSetName)))
                    {
                        ThrowTerminatingError(new ErrorRecord(
                            new InvalidOperationException($"Cannot change OptionSetName for statuscode attributes."),
                            "ImmutableOptionSetName",
                            ErrorCategory.InvalidOperation,
                            OptionSetName));
                    }
                    
                    // Options are allowed for statuscode - will be handled in UpdateTypeSpecificProperties
                }
                else if (lookupAttr != null)
                {
                    // Validate immutable lookup properties
                    if (MyInvocation.BoundParameters.ContainsKey(nameof(Targets)))
                    {
                        ThrowTerminatingError(new ErrorRecord(
                            new InvalidOperationException($"Cannot change Targets after creation. The target entities of a lookup are immutable after creation."),
                            "ImmutableTargets",
                            ErrorCategory.InvalidOperation,
                            null));
                    }

                    if (MyInvocation.BoundParameters.ContainsKey(nameof(RelationshipSchemaName)))
                    {
                        ThrowTerminatingError(new ErrorRecord(
                            new InvalidOperationException($"Cannot change RelationshipSchemaName after creation. The relationship is immutable after creation."),
                            "ImmutableRelationship",
                            ErrorCategory.InvalidOperation,
                            null));
                    }

                    // Cascade behaviors cannot be changed via UpdateAttribute - use Set-DataverseRelationshipMetadata
                    var cascadeParams = new[] { nameof(CascadeAssign), nameof(CascadeShare), nameof(CascadeUnshare), 
                                                nameof(CascadeReparent), nameof(CascadeDelete), nameof(CascadeMerge) };
                    foreach (var param in cascadeParams)
                    {
                        if (MyInvocation.BoundParameters.ContainsKey(param))
                        {
                            ThrowTerminatingError(new ErrorRecord(
                                new InvalidOperationException($"Cannot change cascade behaviors via Set-DataverseAttributeMetadata. Use Set-DataverseRelationshipMetadata to update the relationship's cascade configuration."),
                                "CascadeBehaviorUpdateNotSupported",
                                ErrorCategory.InvalidOperation,
                                null));
                        }
                    }
                }
            }
        }

        private void InvalidateEntityCache()
        {
            var connectionKey = MetadataCache.GetConnectionKey(Connection as Microsoft.PowerPlatform.Dataverse.Client.ServiceClient);
            if (connectionKey != null)
            {
                MetadataCache.InvalidateEntity(connectionKey, EntityName);
                WriteVerbose($"Invalidated metadata cache for entity '{EntityName}'");
            }
        }

        private AttributeMetadata CloneAttributeForUpdate(AttributeMetadata existing)
        {
            // For updates, we need to preserve the MetadataId
            // We can't clone all properties, so we'll create a new instance of the same type
            // and copy only the essential properties

            AttributeMetadata cloned = null;

            if (existing is StringAttributeMetadata stringAttr)
            {
                cloned = new StringAttributeMetadata
                {
                    MaxLength = stringAttr.MaxLength,
                    Format = stringAttr.Format
                };
            }
            else if (existing is MemoAttributeMetadata memoAttr)
            {
                cloned = new MemoAttributeMetadata
                {
                    MaxLength = memoAttr.MaxLength
                };
            }
            else if (existing is IntegerAttributeMetadata intAttr)
            {
                cloned = new IntegerAttributeMetadata
                {
                    MinValue = intAttr.MinValue,
                    MaxValue = intAttr.MaxValue,
                    Format = intAttr.Format
                };
            }
            else if (existing is DecimalAttributeMetadata decimalAttr)
            {
                cloned = new DecimalAttributeMetadata
                {
                    MinValue = decimalAttr.MinValue,
                    MaxValue = decimalAttr.MaxValue,
                    Precision = decimalAttr.Precision
                };
            }
            else if (existing is DoubleAttributeMetadata doubleAttr)
            {
                cloned = new DoubleAttributeMetadata
                {
                    MinValue = doubleAttr.MinValue,
                    MaxValue = doubleAttr.MaxValue,
                    Precision = doubleAttr.Precision
                };
            }
            else if (existing is MoneyAttributeMetadata moneyAttr)
            {
                cloned = new MoneyAttributeMetadata
                {
                    MinValue = moneyAttr.MinValue,
                    MaxValue = moneyAttr.MaxValue,
                    Precision = moneyAttr.Precision
                };
            }
            else if (existing is DateTimeAttributeMetadata dateTimeAttr)
            {
                cloned = new DateTimeAttributeMetadata
                {
                    Format = dateTimeAttr.Format
                };
            }
            else if (existing is BooleanAttributeMetadata booleanAttr)
            {
                cloned = new BooleanAttributeMetadata
                {
                    DefaultValue = booleanAttr.DefaultValue,
                    OptionSet = booleanAttr.OptionSet
                };
            }
            else if (existing is PicklistAttributeMetadata picklistAttr)
            {
                cloned = new PicklistAttributeMetadata
                {
                    OptionSet = picklistAttr.OptionSet
                };
            }
            else if (existing is StatusAttributeMetadata statusAttr)
            {
                cloned = new StatusAttributeMetadata
                {
                    OptionSet = statusAttr.OptionSet
                };
            }
            else if (existing is LookupAttributeMetadata lookupAttr)
            {
                // Lookup attributes can have their display name, description, and required level updated
                cloned = new LookupAttributeMetadata
                {
                    // Targets are immutable after creation
                };
            }
            else
            {
                // For other types, we'll create a basic instance
                cloned = (AttributeMetadata)Activator.CreateInstance(existing.GetType());
            }

            // Copy common properties
            cloned.MetadataId = existing.MetadataId;
            cloned.LogicalName = existing.LogicalName;
            cloned.SchemaName = existing.SchemaName;
            cloned.DisplayName = existing.DisplayName;
            cloned.Description = existing.Description;
            cloned.RequiredLevel = existing.RequiredLevel;
            cloned.IsValidForAdvancedFind = existing.IsValidForAdvancedFind;
            cloned.IsAuditEnabled = existing.IsAuditEnabled;

            return cloned;
        }

        private bool UpdateTypeSpecificProperties(AttributeMetadata attribute)
        {
            bool hasChanges = false;

            if (attribute is StringAttributeMetadata stringAttr && MaxLength.HasValue)
            {
                stringAttr.MaxLength = MaxLength.Value;
                hasChanges = true;
            }
            else if (attribute is MemoAttributeMetadata memoAttr && MaxLength.HasValue)
            {
                memoAttr.MaxLength = MaxLength.Value;
                hasChanges = true;
            }
            else if (attribute is IntegerAttributeMetadata intAttr)
            {
                if (MinValue != null)
                {
                    intAttr.MinValue = Convert.ToInt32(MinValue);
                    hasChanges = true;
                }
                if (MaxValue != null)
                {
                    intAttr.MaxValue = Convert.ToInt32(MaxValue);
                    hasChanges = true;
                }
            }
            else if (attribute is DecimalAttributeMetadata decimalAttr)
            {
                if (MinValue != null)
                {
                    decimalAttr.MinValue = Convert.ToDecimal(MinValue);
                    hasChanges = true;
                }
                if (MaxValue != null)
                {
                    decimalAttr.MaxValue = Convert.ToDecimal(MaxValue);
                    hasChanges = true;
                }
                if (Precision.HasValue)
                {
                    decimalAttr.Precision = Precision.Value;
                    hasChanges = true;
                }
            }
            else if (attribute is DoubleAttributeMetadata doubleAttr)
            {
                if (MinValue != null)
                {
                    doubleAttr.MinValue = Convert.ToDouble(MinValue);
                    hasChanges = true;
                }
                if (MaxValue != null)
                {
                    doubleAttr.MaxValue = Convert.ToDouble(MaxValue);
                    hasChanges = true;
                }
                if (Precision.HasValue)
                {
                    doubleAttr.Precision = Precision.Value;
                    hasChanges = true;
                }
            }
            else if (attribute is MoneyAttributeMetadata moneyAttr)
            {
                if (MinValue != null)
                {
                    moneyAttr.MinValue = Convert.ToDouble(MinValue);
                    hasChanges = true;
                }
                if (MaxValue != null)
                {
                    moneyAttr.MaxValue = Convert.ToDouble(MaxValue);
                    hasChanges = true;
                }
                if (Precision.HasValue)
                {
                    moneyAttr.Precision = Precision.Value;
                    hasChanges = true;
                }
            }
            else if (attribute is StatusAttributeMetadata statusAttr && Options != null && Options.Length > 0)
            {
                // Update statuscode options with State property
                // We need to work with the existing OptionSet to preserve its properties
                if (statusAttr.OptionSet == null)
                {
                    statusAttr.OptionSet = new OptionSetMetadata
                    {
                        IsGlobal = false,
                        OptionSetType = OptionSetType.Picklist
                    };
                }
                
                // Clear existing options and add new ones
                statusAttr.OptionSet.Options.Clear();

                foreach (var option in Options)
                {
                    var value = option["Value"] != null ? Convert.ToInt32(option["Value"]) : (int?)null;
                    var label = option["Label"] as string;

                    if (string.IsNullOrWhiteSpace(label))
                    {
                        continue;
                    }

                    if (!value.HasValue)
                    {
                        WriteWarning($"Skipping option '{label}' - Value is required for statuscode options");
                        continue;
                    }

                    // State is required for statuscode options
                    if (!option.ContainsKey("State") || option["State"] == null)
                    {
                        ThrowTerminatingError(new ErrorRecord(
                            new ArgumentException($"State property is required for statuscode option '{label}' (Value={value.Value}). Example: @{{Value=1; Label='Draft'; State=0}}"),
                            "StateRequired",
                            ErrorCategory.InvalidArgument,
                            option));
                        return hasChanges;
                    }

                    var state = Convert.ToInt32(option["State"]);
                    var statusOption = new StatusOptionMetadata
                    {
                        Value = value,
                        Label = new Label(label, _baseLanguageCode),
                        State = state
                    };

                    statusAttr.OptionSet.Options.Add(statusOption);
                }

                hasChanges = true;
            }

            return hasChanges;
        }

        private PSObject ConvertAttributeMetadataToPSObject(AttributeMetadata attr)
        {
            var result = new PSObject();
            result.Properties.Add(new PSNoteProperty("LogicalName", attr.LogicalName));
            result.Properties.Add(new PSNoteProperty("SchemaName", attr.SchemaName));
            result.Properties.Add(new PSNoteProperty("DisplayName", attr.DisplayName?.UserLocalizedLabel?.Label));
            result.Properties.Add(new PSNoteProperty("AttributeType", attr.AttributeType?.ToString()));
            result.Properties.Add(new PSNoteProperty("MetadataId", attr.MetadataId));
            return result;
        }
    }
}
