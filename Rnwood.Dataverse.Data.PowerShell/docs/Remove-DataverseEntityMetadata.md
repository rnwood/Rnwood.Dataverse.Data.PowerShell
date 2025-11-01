---
external help file: Rnwood.Dataverse.Data.PowerShell.Cmdlets.dll-Help.xml
Module Name: Rnwood.Dataverse.Data.PowerShell
online version:
schema: 2.0.0
---

# Remove-DataverseEntityMetadata

## SYNOPSIS
Deletes an entity (table) from Dataverse.

## SYNTAX

```
Remove-DataverseEntityMetadata [-EntityName] <String> [-Force] [-Connection <ServiceClient>]
 [-ProgressAction <ActionPreference>] [-WhatIf] [-Confirm] [<CommonParameters>]
```

## DESCRIPTION
The `Remove-DataverseEntityMetadata` cmdlet permanently deletes an entity (table) and all its data from Dataverse. This operation cannot be undone and will delete:
- The entity definition and all its attributes
- All records in the entity
- All relationships to the entity
- All views, forms, and charts associated with the entity
- All security roles and permissions for the entity

**?? WARNING: This is a destructive operation that permanently deletes data!**

By default, this cmdlet prompts for confirmation (ConfirmImpact = High). Use `-Force` to suppress the confirmation prompt.

## EXAMPLES

### Example 1: Delete an entity with confirmation
```powershell
PS C:\> Remove-DataverseEntityMetadata -EntityName new_customentity

Confirm
Are you sure you want to perform this action?
Performing the operation "Delete entity" on target "new_customentity".
[Y] Yes  [A] Yes to All  [N] No  [L] No to All  [S] Suspend  [?] Help (default is "Y"): Y
```

Deletes the entity `new_customentity` after prompting for confirmation.

### Example 2: Delete an entity without confirmation
```powershell
PS C:\> Remove-DataverseEntityMetadata -EntityName new_customentity -Force
```

Deletes the entity `new_customentity` without prompting for confirmation.

### Example 3: Delete multiple entities
```powershell
PS C:\> @("new_entity1", "new_entity2", "new_entity3") | Remove-DataverseEntityMetadata -Force
```

Deletes multiple entities by passing entity names through the pipeline.

### Example 4: Use WhatIf to preview deletion
```powershell
PS C:\> Remove-DataverseEntityMetadata -EntityName new_testentity -WhatIf

What if: Performing the operation "Delete entity" on target "new_testentity".
```

Shows what would happen without actually deleting the entity.

### Example 5: Delete entity with specific connection
```powershell
PS C:\> $conn = Get-DataverseConnection -Url "https://myorg.crm.dynamics.com" -Interactive
PS C:\> Remove-DataverseEntityMetadata -Connection $conn -EntityName new_oldentity -Force
```

Deletes an entity using a specific connection.

### Example 6: Delete test entities matching a pattern
```powershell
PS C:\> Get-DataverseEntityMetadata -Connection $conn | 
    Where-Object { $_.LogicalName -like "new_test*" } | 
    ForEach-Object { 
        Write-Host "Deleting $($_.LogicalName)..."
        Remove-DataverseEntityMetadata -EntityName $_.LogicalName -Force 
    }
```

Finds and deletes all entities whose names start with "new_test".

### Example 7: Conditional deletion based on records
```powershell
PS C:\> $recordCount = (Get-DataverseRecord -TableName new_customentity).Count
PS C:\> if ($recordCount -eq 0) {
    Write-Host "Entity is empty, safe to delete"
    Remove-DataverseEntityMetadata -EntityName new_customentity -Force
} else {
    Write-Warning "Entity contains $recordCount records. Deletion cancelled."
}
```

Checks if an entity is empty before deleting it.

### Example 8: Handle deletion errors
```powershell
PS C:\> try {
    Remove-DataverseEntityMetadata -EntityName new_customentity -Force -ErrorAction Stop
    Write-Host "Entity deleted successfully"
} catch {
    Write-Error "Failed to delete entity: $($_.Exception.Message)"
}
```

Handles potential errors during entity deletion with proper error handling.

## PARAMETERS

### -Confirm
Prompts you for confirmation before running the cmdlet.

**Note:** This cmdlet has ConfirmImpact = High, so confirmation is required by default unless `-Force` or `-Confirm:$false` is specified.

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

### -EntityName
Logical name of the entity (table) to delete. This is the internal name of the table (e.g., `new_customentity`, `account`, `contact`).

**?? WARNING:** Deleting an entity is permanent and cannot be undone. All data, relationships, and customizations will be lost.

```yaml
Type: String
Parameter Sets: (All)
Aliases: TableName

Required: True
Position: 0
Default value: None
Accept pipeline input: True (ByPropertyName, ByValue)
Accept wildcard characters: False
```

### -Force
Force deletion without confirmation prompt. Use with caution as this bypasses the safety confirmation.

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

### -ProgressAction
Controls how PowerShell handles progress messages. This is a common parameter added automatically by PowerShell.

```yaml
Type: ActionPreference
Parameter Sets: (All)
Aliases: proga

Required: False
Position: Named
Default value: None
Accept pipeline input: False
Accept wildcard characters: False
```

### CommonParameters
This cmdlet supports the common parameters: -Debug, -ErrorAction, -ErrorVariable, -InformationAction, -InformationVariable, -OutVariable, -OutBuffer, -PipelineVariable, -Verbose, -WarningAction, and -WarningVariable. For more information, see [about_CommonParameters](http://go.microsoft.com/fwlink/?LinkID=113216).

## INPUTS

### System.String
You can pipe entity names to this cmdlet.

## OUTPUTS

### None
This cmdlet does not produce any output.

## NOTES

### Destructive Operation
This cmdlet permanently deletes entities and all associated data. This operation:
- **Cannot be undone** - There is no way to recover a deleted entity or its data
- **Deletes all records** - All data in the entity is permanently removed
- **Removes relationships** - All relationships to/from the entity are deleted
- **Deletes customizations** - All forms, views, charts, business rules are removed
- **Requires high privileges** - System Administrator or System Customizer role required

### What Gets Deleted
When you delete an entity, the following are permanently removed:
- Entity definition and all attributes (columns)
- All records and their data
- All One-to-Many and Many-to-Many relationships
- All forms, views, and charts
- All business rules and workflows
- All security roles and permissions specific to the entity
- All audit history for the entity

### Managed Entities
You cannot delete entities that are:
- Part of managed solutions (must delete the solution instead)
- System entities (e.g., account, contact, systemuser)
- In use by other solutions or dependencies

### Publishing
After deleting an entity, customizations are automatically published. No additional publish step is required.

### Metadata Cache
The metadata cache is automatically invalidated after deletion to ensure subsequent operations don't try to access the deleted entity.

### Best Practices
- **Always use WhatIf first** to verify you're deleting the correct entity
- **Export data before deletion** if you might need it later
- **Check dependencies** using solution explorer before deletion
- **Use confirmation prompts** (don't use -Force) unless in automated scripts
- **Backup your environment** before bulk deletions
- **Test in dev environment** before deleting in production

### Alternatives to Deletion
Consider these alternatives before deleting an entity:
- **Deactivate records** instead of deleting the entity
- **Hide from navigation** if you want to keep data but remove from UI
- **Export to another environment** if you need to preserve the data
- **Use managed solutions** for cleaner deployment lifecycle

### Error Scenarios
The cmdlet will fail if:
- Entity doesn't exist (throws error)
- Entity is part of a managed solution
- Entity has dependencies that prevent deletion
- User lacks sufficient privileges
- Entity is a system entity

### Related Cmdlets
- `Get-DataverseEntityMetadata` - Retrieve entity information before deletion
- `Set-DataverseEntityMetadata` - Create or update entities
- `Remove-DataverseAttributeMetadata` - Delete individual attributes instead
- `Remove-DataverseRelationshipMetadata` - Delete relationships

## RELATED LINKS

[Get-DataverseEntityMetadata](Get-DataverseEntityMetadata.md)

[Set-DataverseEntityMetadata](Set-DataverseEntityMetadata.md)

[Remove-DataverseAttributeMetadata](Remove-DataverseAttributeMetadata.md)

[Microsoft Learn: Delete or remove a table](https://learn.microsoft.com/power-apps/maker/data-platform/data-platform-delete-entity)
