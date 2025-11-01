---
external help file: Rnwood.Dataverse.Data.PowerShell.Cmdlets.dll-Help.xml
Module Name: Rnwood.Dataverse.Data.PowerShell
online version:
schema: 2.0.0
---

# Remove-DataverseRelationshipMetadata

## SYNOPSIS
Deletes a relationship from Dataverse.

## SYNTAX

```
Remove-DataverseRelationshipMetadata [-SchemaName] <String> [-EntityName <String>] [-Force]
 [-Connection <ServiceClient>] [-ProgressAction <ActionPreference>] [-WhatIf] [-Confirm] [<CommonParameters>]
```

## DESCRIPTION
The `Remove-DataverseRelationshipMetadata` cmdlet permanently deletes a relationship from Dataverse. This affects both OneToMany (1:N) and ManyToMany (N:N) relationships.

**For OneToMany relationships:**
- Deletes the relationship definition
- Deletes the lookup attribute on the referencing entity
- Removes relationship navigation from both entities
- May delete related records depending on cascade delete settings

**For ManyToMany relationships:**
- Deletes the relationship definition
- Deletes the intersect entity and all association records
- Removes relationship navigation from both entities

**?? WARNING: This is a destructive operation!**
- For 1:N relationships with cascade delete, related records may be deleted
- For N:N relationships, all association records in the intersect table are deleted
- The operation cannot be undone

By default, this cmdlet prompts for confirmation (ConfirmImpact = High). Use `-Force` to suppress the confirmation prompt.

## EXAMPLES

### Example 1: Delete a OneToMany relationship
```powershell
PS C:\> Remove-DataverseRelationshipMetadata -SchemaName new_project_contact

Confirm
Are you sure you want to perform this action?
Performing the operation "Delete relationship 'new_project_contact'" on target "Dataverse organization".
[Y] Yes  [A] Yes to All  [N] No  [L] No to All  [S] Suspend  [?] Help (default is "Y"): Y
```

Deletes the OneToMany relationship `new_project_contact` after prompting for confirmation.

### Example 2: Delete a ManyToMany relationship without confirmation
```powershell
PS C:\> Remove-DataverseRelationshipMetadata -SchemaName new_project_contact_association -Force
```

Deletes a ManyToMany relationship without prompting. All association records are deleted.

### Example 3: Delete relationship with entity name for cache invalidation
```powershell
PS C:\> Remove-DataverseRelationshipMetadata -SchemaName new_project_task `
    -EntityName new_project -Force
```

Deletes a relationship and invalidates the metadata cache for the `new_project` entity.

### Example 4: Use WhatIf to preview deletion
```powershell
PS C:\> Remove-DataverseRelationshipMetadata -SchemaName new_test_relationship -WhatIf

What if: Performing the operation "Delete relationship 'new_test_relationship'" on target "Dataverse organization".
```

Shows what would happen without actually deleting the relationship.

### Example 5: Delete multiple test relationships
```powershell
PS C:\> @("new_test_rel1", "new_test_rel2", "new_test_rel3") | 
    Remove-DataverseRelationshipMetadata -Force
```

Deletes multiple relationships by passing relationship names through the pipeline.

### Example 6: Find and delete relationships for an entity
```powershell
PS C:\> $relationships = Get-DataverseRelationshipMetadata -EntityName new_customentity

PS C:\> $relationships | Where-Object { $_.SchemaName -like "new_test*" } | ForEach-Object {
    Write-Host "Deleting relationship: $($_.SchemaName)"
    Remove-DataverseRelationshipMetadata -SchemaName $_.SchemaName -Force
}
```

Finds and deletes all test relationships (starting with "new_test") for an entity.

### Example 7: Conditional deletion based on usage
```powershell
PS C:\> $rel = Get-DataverseRelationshipMetadata -SchemaName new_project_contact

PS C:\> if ($rel.RelationshipType -eq "ManyToMany") {
    # Check if there are any associations
    $query = "SELECT COUNT(*) as Count FROM $($rel.IntersectEntityName)"
    $count = (Invoke-DataverseSql -Sql $query).Count
    
    if ($count -eq 0) {
        Write-Host "No associations found, safe to delete"
        Remove-DataverseRelationshipMetadata -SchemaName new_project_contact -Force
    } else {
        Write-Warning "Relationship has $count associations. Review before deletion."
    }
}
```

Checks if a ManyToMany relationship has any associations before deleting.

### Example 8: Delete with error handling
```powershell
PS C:\> try {
    Remove-DataverseRelationshipMetadata -SchemaName new_obsolete_rel `
        -Force -ErrorAction Stop
    Write-Host "Relationship deleted successfully"
    
    # Publish customizations after deletion
    Invoke-DataversePublishAllXml
    Write-Host "Customizations published"
} catch {
    Write-Error "Failed to delete relationship: $($_.Exception.Message)"
}
```

Deletes a relationship with proper error handling and publishes customizations.

### Example 9: Delete all custom relationships for cleanup
```powershell
PS C:\> Get-DataverseRelationshipMetadata | 
    Where-Object { $_.IsCustomRelationship -and $_.SchemaName -like "new_*" } |
    ForEach-Object {
        Write-Host "Deleting $($_.SchemaName) ($($_.RelationshipType))..."
        try {
            Remove-DataverseRelationshipMetadata -SchemaName $_.SchemaName `
                -Force -ErrorAction Stop
            Write-Host "  Deleted successfully" -ForegroundColor Green
        } catch {
            Write-Warning "  Could not delete: $($_.Exception.Message)"
        }
    }
```

Finds and attempts to delete all custom relationships, handling errors for dependencies.

### Example 10: Use specific connection
```powershell
PS C:\> $conn = Get-DataverseConnection -Url "https://myorg.crm.dynamics.com" -Interactive
PS C:\> Remove-DataverseRelationshipMetadata -Connection $conn `
    -SchemaName new_deprecated_rel -Force
```

Deletes a relationship using a specific connection.

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
Entity name involved in the relationship (for cache invalidation). This is optional but recommended to ensure the metadata cache is properly invalidated for the affected entity.

If not specified, the cmdlet will still delete the relationship but may not invalidate the cache optimally.

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

### -Force
Bypass confirmation prompt. Use with caution as this bypasses the safety confirmation for a destructive operation.

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

### -SchemaName
Schema name of the relationship to delete (e.g., `new_project_contact`, `account_contact`).

You can find relationship schema names using `Get-DataverseRelationshipMetadata`.

```yaml
Type: String
Parameter Sets: (All)
Aliases:

Required: True
Position: 0
Default value: None
Accept pipeline input: True (ByPropertyName, ByValue)
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
You can pipe relationship schema names to this cmdlet.

## OUTPUTS

### None
This cmdlet does not produce any output.

## NOTES

### Destructive Operation
This cmdlet permanently deletes relationships and may delete data:
- **Cannot be undone** - There is no way to recover a deleted relationship
- **OneToMany**: May delete related records if cascade delete is configured
- **ManyToMany**: Always deletes all association records in the intersect table
- **Requires high privileges** - System Administrator or System Customizer role required

### OneToMany (1:N) Relationships
When you delete a 1:N relationship:
- The relationship definition is removed
- The lookup attribute on the referencing (child) entity is deleted
- All data in the lookup field is lost
- If cascade delete is set to "Cascade", all child records are also deleted
- If cascade delete is set to "Restrict", deletion fails if child records exist
- Navigation properties are removed from both entities

### ManyToMany (N:N) Relationships
When you delete an N:N relationship:
- The relationship definition is removed
- The intersect entity is deleted
- **ALL association records are permanently deleted**
- No records in the primary entities are deleted
- Navigation properties are removed from both entities
- Connection data is lost (cannot be recovered)

### Cascade Delete Behavior
For OneToMany relationships, check the cascade delete setting before deletion:
- **Cascade**: Child records will be deleted automatically
- **RemoveLink**: Only the lookup value is cleared (safe)
- **Restrict**: Deletion fails if child records exist
- **NoCascade**: Lookup value remains, creating orphaned references

Use `Get-DataverseRelationshipMetadata` to check cascade settings before deletion.

### Protected Relationships
You cannot delete relationships that are:
- Part of managed solutions (must delete the solution instead)
- System relationships (e.g., account to contact)
- Required by other components or solutions
- Used by business processes

### Publishing
After deleting a relationship, you should publish customizations:
```powershell
Invoke-DataversePublishAllXml -Connection $connection
```

### Metadata Cache
The metadata cache for involved entities is automatically invalidated after deletion when `-EntityName` is provided. For best results, specify the entity name parameter.

### Best Practices
- **Always use WhatIf first** to verify you're deleting the correct relationship
- **Check cascade delete settings** for 1:N relationships
- **Export association data** for N:N relationships before deletion if you might need it
- **Check dependencies** in solution explorer before deletion
- **Test in dev environment** before deleting in production
- **Use confirmation prompts** (don't use -Force) unless in automated scripts
- **Document why the relationship is being deleted** for future reference

### Dependencies to Check Before Deletion
Check if the relationship is used in:
- Forms (related entity grids, subgrids)
- Views (columns from related entities)
- Business rules
- Workflows and Power Automate flows
- Plugin code
- Reports using JOINs
- Charts using related entity data
- Security role permissions

### Alternatives to Deletion
Consider these alternatives before deleting a relationship:
- **Hide the lookup field** from forms if you want to keep data but remove from UI
- **Make the lookup read-only** to prevent new associations
- **Clear association data first** then delete if you want time to backup
- **Document the relationship** if it might be needed in the future

### Error Scenarios
The cmdlet will fail if:
- Relationship doesn't exist (throws error)
- Relationship is part of a managed solution
- Relationship is system-required
- User lacks sufficient privileges
- Relationship has dependencies that prevent deletion
- Cascade delete is "Restrict" and child records exist

### Related Cmdlets
- `Get-DataverseRelationshipMetadata` - Retrieve relationship information before deletion
- `Set-DataverseRelationshipMetadata` - Create or update relationships
- `Remove-DataverseAttributeMetadata` - Delete lookup attributes (which also deletes the relationship)
- `Remove-DataverseEntityMetadata` - Delete entire entities

## RELATED LINKS

[Get-DataverseRelationshipMetadata](Get-DataverseRelationshipMetadata.md)

[Set-DataverseRelationshipMetadata](Set-DataverseRelationshipMetadata.md)

[Microsoft Learn: Delete a relationship](https://learn.microsoft.com/power-apps/maker/data-platform/data-platform-delete-relationship)
