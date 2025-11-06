---
external help file: Rnwood.Dataverse.Data.PowerShell.Cmdlets.dll-Help.xml
Module Name: Rnwood.Dataverse.Data.PowerShell
online version:
schema: 2.0.0
---

# Set-DataverseEntityMetadata

## SYNOPSIS
Creates or updates an entity (table) in Dataverse.

## SYNTAX

```
Set-DataverseEntityMetadata [-EntityName] <String> [-SchemaName <String>] [-DisplayName <String>]
 [-DisplayCollectionName <String>] [-Description <String>] [-OwnershipType <String>] [-HasActivities]
 [-HasNotes] [-IsAuditEnabled] [-ChangeTrackingEnabled] [-PrimaryAttributeSchemaName <String>]
 [-PrimaryAttributeDisplayName <String>] [-PrimaryAttributeMaxLength <Int32>] [-PassThru]
 [-Connection <ServiceClient>] [-WhatIf] [-Confirm] [<CommonParameters>]
```

## DESCRIPTION
The `Set-DataverseEntityMetadata` cmdlet creates new entities (tables) or updates existing ones in Dataverse. It allows you to configure entity properties such as display names, ownership type, and features like activities and notes support.

When creating a new entity, the `-SchemaName` and `-PrimaryAttributeSchemaName` parameters are required. The cmdlet automatically creates a primary name attribute for the entity.

When updating an existing entity, only the properties you specify will be changed. Some entity properties are immutable after creation (e.g., OwnershipType, SchemaName).

**Important Notes:**
- After creating or updating entities, you must publish customizations for changes to be visible
- Creating entities requires System Administrator or System Customizer security role
- The metadata cache is automatically invalidated after changes
- Some properties cannot be changed after entity creation

## EXAMPLES

### Example 1: Create a simple custom entity
```powershell
PS C:\> Set-DataverseEntityMetadata -EntityName new_product `
    -SchemaName new_Product `
    -DisplayName "Product" `
    -DisplayCollectionName "Products" `
    -Description "Custom product catalog" `
    -OwnershipType UserOwned `
    -PrimaryAttributeSchemaName new_name `
    -PrimaryAttributeDisplayName "Product Name"
```

Creates a new user-owned entity named `new_product` with a primary name attribute.

### Example 2: Create entity with activities and notes support
```powershell
PS C:\> Set-DataverseEntityMetadata -EntityName new_project `
    -SchemaName new_Project `
    -DisplayName "Project" `
    -DisplayCollectionName "Projects" `
    -OwnershipType UserOwned `
    -HasActivities `
    -HasNotes `
    -PrimaryAttributeSchemaName new_name `
    -PrimaryAttributeDisplayName "Project Name"
```

Creates an entity that supports activities (tasks, appointments) and notes (annotations).

### Example 3: Create organization-owned entity
```powershell
PS C:\> Set-DataverseEntityMetadata -EntityName new_setting `
    -SchemaName new_Setting `
    -DisplayName "Application Setting" `
    -DisplayCollectionName "Application Settings" `
    -OwnershipType OrganizationOwned `
    -PrimaryAttributeSchemaName new_name `
    -PrimaryAttributeDisplayName "Setting Name"
```

Creates an organization-owned entity (not owned by specific users or teams).

### Example 4: Create entity with audit and change tracking enabled
```powershell
PS C:\> Set-DataverseEntityMetadata -EntityName new_transaction `
    -SchemaName new_Transaction `
    -DisplayName "Transaction" `
    -DisplayCollectionName "Transactions" `
    -OwnershipType UserOwned `
    -IsAuditEnabled `
    -ChangeTrackingEnabled `
    -PrimaryAttributeSchemaName new_name `
    -PrimaryAttributeDisplayName "Transaction ID"
```

Creates an entity with auditing and change tracking enabled for compliance requirements.

### Example 5: Create entity with custom primary name length
```powershell
PS C:\> Set-DataverseEntityMetadata -EntityName new_code `
    -SchemaName new_Code `
    -DisplayName "Code" `
    -DisplayCollectionName "Codes" `
    -OwnershipType UserOwned `
    -PrimaryAttributeSchemaName new_code `
    -PrimaryAttributeDisplayName "Code" `
    -PrimaryAttributeMaxLength 50
```

Creates an entity with a primary name attribute limited to 50 characters.

### Example 6: Update entity display names
```powershell
PS C:\> Set-DataverseEntityMetadata -EntityName new_product `
    -DisplayName "Product Catalog" `
    -DisplayCollectionName "Product Catalogs" `
    -Description "Updated product catalog description"
```

Updates the display names and description of an existing entity.

### Example 7: Enable audit on existing entity
```powershell
PS C:\> Set-DataverseEntityMetadata -EntityName account `
    -IsAuditEnabled
```

Enables auditing for the standard `account` entity.

### Example 8: Enable change tracking on existing entity
```powershell
PS C:\> Set-DataverseEntityMetadata -EntityName contact `
    -ChangeTrackingEnabled
```

Enables change tracking for the `contact` entity to support data synchronization scenarios.

### Example 9: Create entity and return metadata
```powershell
PS C:\> $result = Set-DataverseEntityMetadata -EntityName new_item `
    -SchemaName new_Item `
    -DisplayName "Item" `
    -DisplayCollectionName "Items" `
    -OwnershipType UserOwned `
    -PrimaryAttributeSchemaName new_name `
    -PrimaryAttributeDisplayName "Item Name" `
    -PassThru

PS C:\> $result

LogicalName      : new_item
SchemaName       : new_Item
DisplayName      : Item
EntitySetName    : new_items
MetadataId       : a1234567-89ab-cdef-0123-456789abcdef
OwnershipType    : UserOwned
IsCustomEntity   : True
IsAuditEnabled   : False
```

Creates an entity and returns its metadata using `-PassThru`.

### Example 10: Use -WhatIf to preview changes
```powershell
PS C:\> Set-DataverseEntityMetadata -EntityName new_test `
    -SchemaName new_Test `
    -DisplayName "Test Entity" `
    -DisplayCollectionName "Test Entities" `
    -OwnershipType UserOwned `
    -PrimaryAttributeSchemaName new_name `
    -PrimaryAttributeDisplayName "Name" `
    -WhatIf

What if: Performing the operation "Create entity 'new_Test'" on target "Dataverse organization".
```

Uses `-WhatIf` to see what would happen without actually creating the entity.

### Example 11: Create entity with activities, notes, and audit
```powershell
PS C:\> Set-DataverseEntityMetadata -EntityName new_case `
    -SchemaName new_Case `
    -DisplayName "Support Case" `
    -DisplayCollectionName "Support Cases" `
    -OwnershipType UserOwned `
    -HasActivities `
    -HasNotes `
    -IsAuditEnabled `
    -ChangeTrackingEnabled `
    -PrimaryAttributeSchemaName new_casenumber `
    -PrimaryAttributeDisplayName "Case Number" `
    -PrimaryAttributeMaxLength 20
```

Creates a full-featured entity with all available options enabled.

### Example 12: Update with Force parameter
```powershell
PS C:\> Set-DataverseEntityMetadata -EntityName new_product `
    -DisplayName "Updated Product" `
    -Force
```

Updates an entity without confirmation prompt (Force parameter bypasses confirmation).

### Example 13: Create team-owned entity
```powershell
PS C:\> Set-DataverseEntityMetadata -EntityName new_resource `
    -SchemaName new_Resource `
    -DisplayName "Shared Resource" `
    -DisplayCollectionName "Shared Resources" `
    -OwnershipType TeamOwned `
    -PrimaryAttributeSchemaName new_name `
    -PrimaryAttributeDisplayName "Resource Name"
```

Creates a team-owned entity where records are owned by teams rather than individual users.

### Example 14: Batch create multiple entities
```powershell
PS C:\> $entities = @(
    @{ SchemaName = "new_Category"; DisplayName = "Category"; PrimaryAttribute = "new_name" }
    @{ SchemaName = "new_Tag"; DisplayName = "Tag"; PrimaryAttribute = "new_name" }
    @{ SchemaName = "new_Comment"; DisplayName = "Comment"; PrimaryAttribute = "new_text" }
)

PS C:\> foreach ($entity in $entities) {
    $name = $entity.SchemaName.ToLower()
    Set-DataverseEntityMetadata -EntityName $name `
        -SchemaName $entity.SchemaName `
        -DisplayName $entity.DisplayName `
        -DisplayCollectionName "$($entity.DisplayName)s" `
        -OwnershipType UserOwned `
        -PrimaryAttributeSchemaName $entity.PrimaryAttribute `
        -PrimaryAttributeDisplayName "Name"
}
```

Creates multiple entities by iterating through a collection.

### Example 15: Create with specific connection
```powershell
PS C:\> $conn = Get-DataverseConnection -Url "https://myorg.crm.dynamics.com" -Interactive

PS C:\> Set-DataverseEntityMetadata -Connection $conn `
    -EntityName new_inventory `
    -SchemaName new_Inventory `
    -DisplayName "Inventory Item" `
    -DisplayCollectionName "Inventory Items" `
    -OwnershipType UserOwned `
    -PrimaryAttributeSchemaName new_name `
    -PrimaryAttributeDisplayName "Item Name"
```

Creates an entity using a specific connection instead of the default connection.

## PARAMETERS

### -ChangeTrackingEnabled
Whether change tracking is enabled for the entity. Change tracking enables efficient data synchronization by tracking changes at the table level.

When enabled, Dataverse maintains a log of changes (creates, updates, deletes) that external systems can query to stay synchronized.

```yaml
Type: SwitchParameter
Parameter Sets: (All)
Aliases:

Required: False
Position: Named
Default value: None
Accept pipeline input: False
Accept wildcard characters: False
```

### -Confirm
Prompts you for confirmation before running the cmdlet.

```yaml
Type: SwitchParameter
Parameter Sets: (All)
Aliases: cf

Required: False
Position: Named
Default value: None
Accept pipeline input: False
Accept wildcard characters: False
```

### -Connection
DataverseConnection instance obtained from Get-DataverseConnection cmdlet, or string specifying Dataverse organization URL (e.g. http://server.com/MyOrg/).

If not provided, uses the default connection set via `Get-DataverseConnection -SetAsDefault`.

```yaml
Type: ServiceClient
Parameter Sets: (All)
Aliases:

Required: False
Position: Named
Default value: None
Accept pipeline input: False
Accept wildcard characters: False
```

### -Description
Description of the entity that appears as tooltip text in the UI. This helps users understand the purpose of the entity.

```yaml
Type: String
Parameter Sets: (All)
Aliases:

Required: False
Position: Named
Default value: None
Accept pipeline input: False
Accept wildcard characters: False
```

### -DisplayCollectionName
Display collection name (plural) of the entity used in the UI when showing multiple records.

For example, if DisplayName is "Product", DisplayCollectionName should be "Products".

```yaml
Type: String
Parameter Sets: (All)
Aliases:

Required: False
Position: Named
Default value: None
Accept pipeline input: False
Accept wildcard characters: False
```

### -DisplayName
Display name of the entity shown in the UI. This is the user-friendly name that appears in forms, views, and navigation.

```yaml
Type: String
Parameter Sets: (All)
Aliases:

Required: False
Position: Named
Default value: None
Accept pipeline input: False
Accept wildcard characters: False
```

### -EntityName
Logical name of the entity (table). This is the internal name used to reference the entity programmatically.

For new entities combined with `-SchemaName`, this becomes the logical name after the publisher prefix is added (e.g., `new_product`).

For existing entities, this identifies which entity to update.

```yaml
Type: String
Parameter Sets: (All)
Aliases: TableName

Required: True
Position: 0
Default value: None
Accept pipeline input: False
Accept wildcard characters: False
```

### -HasActivities
Whether the entity supports activities (tasks, phone calls, appointments, emails, etc.).

When enabled, users can:
- Associate activities with records from this entity
- See activity timelines on forms
- Create activities related to these records

**Note**: This property cannot be changed after entity creation.

```yaml
Type: SwitchParameter
Parameter Sets: (All)
Aliases:

Required: False
Position: Named
Default value: None
Accept pipeline input: False
Accept wildcard characters: False
```

### -HasNotes
Whether the entity supports notes (annotations). Notes allow users to add text notes and file attachments to records.

When enabled, users can:
- Add notes with rich text
- Attach files to records
- View notes history

**Note**: This property cannot be changed after entity creation.

```yaml
Type: SwitchParameter
Parameter Sets: (All)
Aliases:

Required: False
Position: Named
Default value: None
Accept pipeline input: False
Accept wildcard characters: False
```

### -IsAuditEnabled
Whether audit is enabled for this entity. When enabled, changes to records are tracked in the audit history.

**Note**: Auditing must also be enabled at the organization level and for specific fields.

```yaml
Type: SwitchParameter
Parameter Sets: (All)
Aliases:

Required: False
Position: Named
Default value: None
Accept pipeline input: False
Accept wildcard characters: False
```

### -OwnershipType
Ownership type of the entity that determines who can own records.

Valid values:
- **UserOwned**: Records are owned by individual users (default for most custom entities)
- **TeamOwned**: Records are owned by teams
- **OrganizationOwned**: Records are owned by the organization (no specific owner)

**Note**: This property is immutable after entity creation.

```yaml
Type: String
Parameter Sets: (All)
Aliases:
Accepted values: UserOwned, TeamOwned, OrganizationOwned

Required: False
Position: Named
Default value: None
Accept pipeline input: False
Accept wildcard characters: False
```

### -PassThru
Return the created or updated entity metadata as a PSObject.

The returned object includes properties like LogicalName, SchemaName, DisplayName, EntitySetName, MetadataId, and OwnershipType.

```yaml
Type: SwitchParameter
Parameter Sets: (All)
Aliases:

Required: False
Position: Named
Default value: None
Accept pipeline input: False
Accept wildcard characters: False
```

### -PrimaryAttributeDisplayName
Display name for the primary name attribute. This is the user-friendly label shown in the UI for the name field.

Required when creating a new entity. For example: "Product Name", "Customer Name", "Case Number".

```yaml
Type: String
Parameter Sets: (All)
Aliases:

Required: False
Position: Named
Default value: None
Accept pipeline input: False
Accept wildcard characters: False
```

### -PrimaryAttributeMaxLength
Maximum length for the primary name attribute in characters. Default is 100 if not specified.

Valid range is 1-4000 characters. Consider your naming conventions when setting this value.

```yaml
Type: Int32
Parameter Sets: (All)
Aliases:

Required: False
Position: Named
Default value: None
Accept pipeline input: False
Accept wildcard characters: False
```

### -PrimaryAttributeSchemaName
Schema name for the primary name attribute. Required when creating a new entity.

Must follow naming conventions with publisher prefix (e.g., `new_name`, `new_productname`).

**Note**: Cannot be changed after entity creation.

```yaml
Type: String
Parameter Sets: (All)
Aliases:

Required: False
Position: Named
Default value: None
Accept pipeline input: False
Accept wildcard characters: False
```

### -SchemaName
Schema name of the entity with publisher prefix (e.g., `new_CustomEntity`). Required when creating a new entity.

This is the unique name that includes your solution publisher's prefix. It becomes the basis for the logical name.

**Note**: This property is immutable after creation.

```yaml
Type: String
Parameter Sets: (All)
Aliases:

Required: False
Position: Named
Default value: None
Accept pipeline input: False
Accept wildcard characters: False
```

### -WhatIf
Shows what would happen if the cmdlet runs. The cmdlet is not run.

```yaml
Type: SwitchParameter
Parameter Sets: (All)
Aliases: wi

Required: False
Position: Named
Default value: None
Accept pipeline input: False
Accept wildcard characters: False
```

### CommonParameters
This cmdlet supports the common parameters: -Debug, -ErrorAction, -ErrorVariable, -InformationAction, -InformationVariable, -OutVariable, -OutBuffer, -PipelineVariable, -Verbose, -WarningAction, and -WarningVariable. For more information, see [about_CommonParameters](http://go.microsoft.com/fwlink/?LinkID=113216).

## INPUTS

### None

## OUTPUTS

### System.Management.Automation.PSObject

## NOTES

This cmdlet provides programmatic access to Dataverse metadata. For comprehensive documentation and examples, see the metadata concept guide at docs/core-concepts/metadata.md

## RELATED LINKS

[Get-DataverseEntityMetadata](Get-DataverseEntityMetadata.md)

[Set-DataverseAttributeMetadata](Set-DataverseAttributeMetadata.md)

[Remove-DataverseEntityMetadata](Remove-DataverseEntityMetadata.md)

[Microsoft Learn: Create custom tables](https://learn.microsoft.com/power-apps/maker/data-platform/data-platform-create-entity)
