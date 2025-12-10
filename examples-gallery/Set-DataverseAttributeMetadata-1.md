---
title: "Set-DataverseAttributeMetadata"
tags: ['CRUD', 'Metadata']
source: "Metadata-CRUD-Examples.md"
---
Creates or updates attributes with comprehensive type support.

```powershell
# Create a new string attribute
Set-DataverseAttributeMetadata -EntityName new_customentity `
   -AttributeName new_description `
   -SchemaName new_Description `
   -AttributeType String `
   -DisplayName "Description" `
   -Description "Custom description field" `
   -MaxLength 500 `
   -RequiredLevel None `
   -IsSearchable

# Create an integer attribute with range
Set-DataverseAttributeMetadata -EntityName new_customentity `
   -AttributeName new_quantity `
   -SchemaName new_Quantity `
   -AttributeType Integer `
   -DisplayName "Quantity" `
   -MinValue 0 `
   -MaxValue 1000 `
   -RequiredLevel ApplicationRequired

# Create a decimal attribute with precision
Set-DataverseAttributeMetadata -EntityName new_customentity `
   -AttributeName new_amount `
   -SchemaName new_Amount `
   -AttributeType Decimal `
   -DisplayName "Amount" `
   -Precision 2 `
   -MinValue 0.00 `
   -MaxValue 999999.99

# Create a datetime attribute
Set-DataverseAttributeMetadata -EntityName new_customentity `
   -AttributeName new_duedate `
   -SchemaName new_DueDate `
   -AttributeType DateTime `
   -DisplayName "Due Date" `
   -DateTimeFormat DateOnly `
   -DateTimeBehavior UserLocal

# Create a boolean attribute
Set-DataverseAttributeMetadata -EntityName new_customentity `
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

Set-DataverseAttributeMetadata -EntityName new_customentity `
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

Set-DataverseAttributeMetadata -EntityName new_customentity `
   -AttributeName new_contactmethods `
   -SchemaName new_ContactMethods `
   -AttributeType MultiSelectPicklist `
   -DisplayName "Contact Methods" `
   -Options $options

# Use an existing global option set
Set-DataverseAttributeMetadata -EntityName new_customentity `
   -AttributeName new_category `
   -SchemaName new_Category `
   -AttributeType Picklist `
   -DisplayName "Category" `
   -OptionSetName my_globalcategories

# Create a lookup attribute to a single target entity
Set-DataverseAttributeMetadata -EntityName new_customentity `
   -AttributeName new_accountid `
   -SchemaName new_AccountId `
   -AttributeType Lookup `
   -DisplayName "Account" `
   -Description "Related account" `
   -Targets @('account') `
   -RequiredLevel None

# Create a lookup with custom relationship name
Set-DataverseAttributeMetadata -EntityName new_project `
   -AttributeName new_parentprojectid `
   -SchemaName new_ParentProjectId `
   -AttributeType Lookup `
   -DisplayName "Parent Project" `
   -Targets @('new_project') `
   -RelationshipSchemaName new_project_parentproject

# Create a lookup with cascade delete behavior
Set-DataverseAttributeMetadata -EntityName new_task `
   -AttributeName new_projectid `
   -SchemaName new_ProjectId `
   -AttributeType Lookup `
   -DisplayName "Project" `
   -Targets @('new_project') `
   -CascadeDelete Cascade `
   -CascadeAssign Cascade `
   -RequiredLevel ApplicationRequired

# Create a lookup with all cascade behaviors specified
Set-DataverseAttributeMetadata -EntityName new_lineitem `
   -AttributeName new_orderid `
   -SchemaName new_OrderId `
   -AttributeType Lookup `
   -DisplayName "Order" `
   -Targets @('new_order') `
   -CascadeDelete Cascade `
   -CascadeAssign Cascade `
   -CascadeShare Cascade `
   -CascadeUnshare Cascade `
   -CascadeReparent Cascade `
   -CascadeMerge Cascade `
   -IsSearchable

# Create a file attribute
Set-DataverseAttributeMetadata -EntityName new_customentity `
   -AttributeName new_attachment `
   -SchemaName new_Attachment `
   -AttributeType File `
   -DisplayName "Attachment" `
   -MaxSizeInKB 10240

# Update an existing attribute (display name and description)
Set-DataverseAttributeMetadata -EntityName contact `
   -AttributeName firstname `
   -DisplayName "First Name (Updated)" `
   -Description "Updated description" `
   -Force

# Update a lookup attribute's display name
Set-DataverseAttributeMetadata -EntityName contact `
   -AttributeName parentcustomerid `
   -DisplayName "Company Name" `
   -Description "The parent company for this contact" `
   -Force

# Update with -PassThru to get the result
$updatedAttr = Set-DataverseAttributeMetadata -EntityName contact `
   -AttributeName firstname `
   -DisplayName "First Name" `
   -Force `
   -PassThru

$updatedAttr

```
