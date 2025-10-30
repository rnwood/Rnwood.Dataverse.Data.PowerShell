# Metadata CRUD Cmdlets - Usage Examples

This document provides examples of using the new comprehensive metadata CRUD cmdlets.

## Get Cmdlets (Read Operations)

### Get-DataverseEntityMetadata
Retrieves entity metadata with detailed information.

```powershell
# Get metadata for a specific entity
Get-DataverseEntityMetadata -EntityName contact

# Include attribute metadata
Get-DataverseEntityMetadata -EntityName contact -IncludeAttributes

# Include relationships
Get-DataverseEntityMetadata -EntityName contact -IncludeRelationships

# Include all metadata (attributes, relationships, privileges)
Get-DataverseEntityMetadata -EntityName contact -IncludeAttributes -IncludeRelationships -IncludePrivileges

# List all entities in the environment
Get-DataverseEntityMetadata

# List all entities with attributes included
Get-DataverseEntityMetadata -IncludeAttributes
```

### Get-DataverseEntities
Lists all entities with filtering options.

```powershell
# List all entities (basic info)
Get-DataverseEntities

# List with detailed information
Get-DataverseEntities -IncludeDetails

# Filter to only custom entities
Get-DataverseEntities -OnlyCustom -IncludeDetails

# Filter to only managed entities
Get-DataverseEntities -OnlyManaged

# Filter to only customizable entities
Get-DataverseEntities -OnlyCustomizable
```

### Get-DataverseAttribute
Retrieves attribute/column metadata.

```powershell
# Get metadata for a specific attribute
Get-DataverseAttribute -EntityName contact -AttributeName firstname

# Get all attributes for an entity
Get-DataverseAttribute -EntityName contact

# Pipeline example - get all string attributes
Get-DataverseAttribute -EntityName contact | Where-Object { $_.AttributeType -eq 'String' }

# Get attributes with max length info
Get-DataverseAttribute -EntityName contact | Where-Object { $_.MaxLength } | Select-Object LogicalName, MaxLength
```

### Get-DataverseOptionSet
Retrieves option set values for choice fields.

```powershell
# Get option set for an entity attribute
Get-DataverseOptionSet -EntityName contact -AttributeName gendercode

# Get a global option set by name
Get-DataverseOptionSet -Name my_globaloptions

# List all global option sets
Get-DataverseOptionSet

# Get options and display them
$optionSet = Get-DataverseOptionSet -EntityName contact -AttributeName preferredcontactmethodcode
$optionSet.Options | Format-Table Value, Label
```

## Set Cmdlets (Create/Update Operations)

### Set-DataverseAttribute
Creates or updates attributes with comprehensive type support.

```powershell
# Create a new string attribute
Set-DataverseAttribute -EntityName new_customentity `
    -AttributeName new_description `
    -SchemaName new_Description `
    -AttributeType String `
    -DisplayName "Description" `
    -Description "Custom description field" `
    -MaxLength 500 `
    -RequiredLevel None `
    -IsSearchable

# Create an integer attribute with range
Set-DataverseAttribute -EntityName new_customentity `
    -AttributeName new_quantity `
    -SchemaName new_Quantity `
    -AttributeType Integer `
    -DisplayName "Quantity" `
    -MinValue 0 `
    -MaxValue 1000 `
    -RequiredLevel ApplicationRequired

# Create a decimal attribute with precision
Set-DataverseAttribute -EntityName new_customentity `
    -AttributeName new_amount `
    -SchemaName new_Amount `
    -AttributeType Decimal `
    -DisplayName "Amount" `
    -Precision 2 `
    -MinValue 0.00 `
    -MaxValue 999999.99

# Create a datetime attribute
Set-DataverseAttribute -EntityName new_customentity `
    -AttributeName new_duedate `
    -SchemaName new_DueDate `
    -AttributeType DateTime `
    -DisplayName "Due Date" `
    -DateTimeFormat DateOnly `
    -DateTimeBehavior UserLocal

# Create a boolean attribute
Set-DataverseAttribute -EntityName new_customentity `
    -AttributeName new_isactive `
    -SchemaName new_IsActive `
    -AttributeType Boolean `
    -DisplayName "Is Active" `
    -TrueLabel "Active" `
    -FalseLabel "Inactive" `
    -DefaultValue $true

# Create a picklist with options
$options = @(
    @{Value=1; Label='Low'}
    @{Value=2; Label='Medium'}
    @{Value=3; Label='High'}
)

Set-DataverseAttribute -EntityName new_customentity `
    -AttributeName new_priority `
    -SchemaName new_Priority `
    -AttributeType Picklist `
    -DisplayName "Priority" `
    -Options $options

# Create a multi-select picklist
$options = @(
    @{Value=1; Label='Email'}
    @{Value=2; Label='Phone'}
    @{Value=3; Label='In Person'}
)

Set-DataverseAttribute -EntityName new_customentity `
    -AttributeName new_contactmethods `
    -SchemaName new_ContactMethods `
    -AttributeType MultiSelectPicklist `
    -DisplayName "Contact Methods" `
    -Options $options

# Use an existing global option set
Set-DataverseAttribute -EntityName new_customentity `
    -AttributeName new_category `
    -SchemaName new_Category `
    -AttributeType Picklist `
    -DisplayName "Category" `
    -OptionSetName my_globalcategories

# Create a file attribute
Set-DataverseAttribute -EntityName new_customentity `
    -AttributeName new_attachment `
    -SchemaName new_Attachment `
    -AttributeType File `
    -DisplayName "Attachment" `
    -MaxSizeInKB 10240

# Update an existing attribute (display name and description)
Set-DataverseAttribute -EntityName contact `
    -AttributeName firstname `
    -DisplayName "First Name (Updated)" `
    -Description "Updated description" `
    -Force

# Update with -PassThru to get the result
$updatedAttr = Set-DataverseAttribute -EntityName contact `
    -AttributeName firstname `
    -DisplayName "First Name" `
    -Force `
    -PassThru

$updatedAttr
```

### Set-DataverseEntityMetadata
Creates or updates entity metadata.

```powershell
# Create a new custom entity
Set-DataverseEntityMetadata -EntityName new_project `
    -SchemaName new_Project `
    -DisplayName "Project" `
    -DisplayCollectionName "Projects" `
    -Description "Manages projects" `
    -OwnershipType UserOwned `
    -PrimaryAttributeSchemaName new_name `
    -PrimaryAttributeDisplayName "Project Name" `
    -PrimaryAttributeMaxLength 200 `
    -HasActivities `
    -HasNotes `
    -IsAuditEnabled `
    -PassThru

# Update an existing entity
Set-DataverseEntityMetadata -EntityName new_project `
    -DisplayName "Projects (Updated)" `
    -Description "Updated description for projects" `
    -IsAuditEnabled `
    -ChangeTrackingEnabled `
    -Force `
    -PassThru
```

### Set-DataverseOptionSet
Creates or updates global option sets.

```powershell
# Create a new global option set
$options = @(
    @{Value=100; Label='Bronze'; Description='Bronze tier'}
    @{Value=200; Label='Silver'; Description='Silver tier'}
    @{Value=300; Label='Gold'; Description='Gold tier'}
    @{Value=400; Label='Platinum'; Description='Platinum tier'}
)

Set-DataverseOptionSet -Name new_customertier `
    -DisplayName "Customer Tier" `
    -Description "Customer membership tiers" `
    -Options $options `
    -PassThru

# Update an existing global option set
$updatedOptions = @(
    @{Value=100; Label='Bronze Level'}
    @{Value=200; Label='Silver Level'}
    @{Value=300; Label='Gold Level'}
    @{Value=400; Label='Platinum Level'}
    @{Value=500; Label='Diamond Level'} # New option
)

Set-DataverseOptionSet -Name new_customertier `
    -DisplayName "Customer Tier (Updated)" `
    -Options $updatedOptions `
    -Force `
    -PassThru
```

## Remove Cmdlets (Delete Operations)

### Remove-DataverseAttribute
Deletes attributes from entities.

```powershell
# Remove an attribute (will prompt for confirmation)
Remove-DataverseAttribute -EntityName new_customentity -AttributeName new_oldfield

# Remove with force (no confirmation)
Remove-DataverseAttribute -EntityName new_customentity -AttributeName new_oldfield -Force

# Use WhatIf to see what would happen
Remove-DataverseAttribute -EntityName new_customentity -AttributeName new_oldfield -WhatIf
```

### Remove-DataverseEntityMetadata
Deletes entities.

```powershell
# Remove an entity (will prompt for confirmation)
Remove-DataverseEntityMetadata -EntityName new_oldentity

# Remove with force (no confirmation)
Remove-DataverseEntityMetadata -EntityName new_oldentity -Force

# Use WhatIf to see what would happen
Remove-DataverseEntityMetadata -EntityName new_oldentity -WhatIf
```

## Advanced Scenarios

### Bulk Attribute Creation
```powershell
# Create multiple attributes from a CSV or array
$attributes = @(
    @{Name='new_field1'; Type='String'; DisplayName='Field 1'; MaxLength=100}
    @{Name='new_field2'; Type='Integer'; DisplayName='Field 2'; MinValue=0; MaxValue=100}
    @{Name='new_field3'; Type='DateTime'; DisplayName='Field 3'; DateTimeFormat='DateOnly'}
)

foreach ($attr in $attributes) {
    $params = @{
        EntityName = 'new_customentity'
        AttributeName = $attr.Name
        SchemaName = $attr.Name -replace '^new_', 'new_' # Ensure proper case
        AttributeType = $attr.Type
        DisplayName = $attr.DisplayName
    }
    
    # Add type-specific parameters
    if ($attr.MaxLength) { $params.MaxLength = $attr.MaxLength }
    if ($attr.MinValue) { $params.MinValue = $attr.MinValue }
    if ($attr.MaxValue) { $params.MaxValue = $attr.MaxValue }
    if ($attr.DateTimeFormat) { $params.DateTimeFormat = $attr.DateTimeFormat }
    
    Set-DataverseAttribute @params -Confirm:$false
}
```

### Clone Entity Structure
```powershell
# Get all attributes from one entity
$sourceAttributes = Get-DataverseAttribute -EntityName source_entity

# Create similar attributes in target entity
foreach ($attr in $sourceAttributes | Where-Object { $_.IsCustomAttribute -eq $true }) {
    $newName = $attr.LogicalName -replace '^source_', 'target_'
    
    Set-DataverseAttribute -EntityName target_entity `
        -AttributeName $newName `
        -SchemaName $attr.SchemaName -replace '^source_', 'target_' `
        -AttributeType $attr.AttributeTypeName `
        -DisplayName $attr.DisplayName `
        -MaxLength $attr.MaxLength `
        -Confirm:$false
}
```

### Audit Metadata Changes
```powershell
# Get all entities and check audit status
$entities = Get-DataverseEntities -IncludeDetails

$auditStatus = $entities | Select-Object LogicalName, DisplayName, IsAuditEnabled

# Enable audit for all custom entities
$customEntities = Get-DataverseEntities -OnlyCustom

foreach ($entity in $customEntities) {
    Set-DataverseEntityMetadata -EntityName $entity.LogicalName `
        -IsAuditEnabled `
        -Force `
        -Confirm:$false
}
```

## Notes

- All cmdlets support `-Connection` parameter to specify which Dataverse environment to use
- All Set/Remove cmdlets support `-WhatIf` and `-Confirm` for testing
- Use `-Verbose` to see detailed operation progress
- The cmdlets handle all Dataverse attribute types comprehensively
- Type-specific properties are validated based on the attribute type
- Global option sets can be reused across multiple entities
