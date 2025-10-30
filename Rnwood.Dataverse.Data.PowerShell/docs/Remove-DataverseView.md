---
external help file: Rnwood.Dataverse.Data.PowerShell.Cmdlets.dll-Help.xml
Module Name: Rnwood.Dataverse.Data.PowerShell
online version:
schema: 2.0.0
---

# Remove-DataverseView

## SYNOPSIS
Removes a view (savedquery or userquery) from Dataverse.

## SYNTAX

```
Remove-DataverseView -Id <Guid> [-SystemView] [-IfExists] [-Connection <ServiceClient>]
 [-ProgressAction <ActionPreference>] [-WhatIf] [-Confirm] [<CommonParameters>]
```

## DESCRIPTION
Removes a view (savedquery for system views or userquery for personal views) from Dataverse. The view is permanently deleted.

Use the `-SystemView` switch when removing system views. Personal views are removed by default.

The `-IfExists` parameter can be used to suppress errors when attempting to remove a view that doesn't exist, which is useful in cleanup scripts.

**Important:** Removing a view is a destructive operation and cannot be undone. Use the `-WhatIf` parameter to preview what would be deleted before confirming the operation.

## EXAMPLES

### Example 1: Remove a personal view
```powershell
PS C:\> Remove-DataverseView -Connection $c -Id $viewId
```

Removes a personal view. Will prompt for confirmation.

### Example 2: Remove a system view
```powershell
PS C:\> Remove-DataverseView -Connection $c -Id $systemViewId -SystemView
```

Removes a system view. Will prompt for confirmation.

### Example 3: Remove without confirmation
```powershell
PS C:\> Remove-DataverseView -Connection $c -Id $viewId -Confirm:$false
```

Removes a view without prompting for confirmation.

### Example 4: Remove with IfExists flag
```powershell
PS C:\> Remove-DataverseView -Connection $c -Id $viewId -IfExists
```

Removes a view but doesn't raise an error if the view doesn't exist. Useful in cleanup scripts.

### Example 5: Remove multiple views via pipeline
```powershell
PS C:\> $viewIds = @(
    [Guid]"11111111-1111-1111-1111-111111111111",
    [Guid]"22222222-2222-2222-2222-222222222222"
)

PS C:\> $viewIds | ForEach-Object { 
    [PSCustomObject]@{ Id = $_ } 
} | Remove-DataverseView -Connection $c -Confirm:$false
```

Removes multiple views by piping view IDs.

### Example 6: Remove with WhatIf
```powershell
PS C:\> Remove-DataverseView -Connection $c -Id $viewId -WhatIf
```

Shows what would happen without actually removing the view.

### Example 7: Find and remove custom views
```powershell
PS C:\> # Get all custom views for contact table
PS C:\> $customViews = Get-DataverseRecord -Connection $c -TableName userquery `
    -FilterValues @{returnedtypecode = "contact"}

PS C:\> # Remove views matching a pattern
PS C:\> $customViews | Where-Object { $_.name -like "Test*" } | 
    Remove-DataverseView -Connection $c -Confirm:$false
```

Finds and removes all personal views for the contact table that start with "Test".

### Example 8: Safe removal with error handling
```powershell
PS C:\> try {
    Remove-DataverseView -Connection $c -Id $viewId -SystemView -ErrorAction Stop
    Write-Host "View removed successfully"
} catch {
    Write-Warning "Failed to remove view: $($_.Exception.Message)"
}
```

Removes a view with proper error handling.

## PARAMETERS

### -Id
ID of the view to remove.

```yaml
Type: Guid
Parameter Sets: (All)
Aliases:

Required: True
Position: Named
Default value: None
Accept pipeline input: True (ByPropertyName)
Accept wildcard characters: False
```

### -SystemView
Remove a system view (savedquery) instead of a personal view (userquery).

```yaml
Type: SwitchParameter
Parameter Sets: (All)
Aliases:

Required: False
Position: Named
Default value: False
Accept pipeline input: False
Accept wildcard characters: False
```

### -IfExists
If specified, the cmdlet will not raise an error if the view does not exist.

```yaml
Type: SwitchParameter
Parameter Sets: (All)
Aliases:

Required: False
Position: Named
Default value: False
Accept pipeline input: False
Accept wildcard characters: False
```

### -Connection
Dataverse connection to use for the operation.

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

### CommonParameters
This cmdlet supports the common parameters: -Debug, -ErrorAction, -ErrorVariable, -InformationAction, -InformationVariable, -OutVariable, -OutBuffer, -PipelineVariable, -Verbose, -WarningAction, and -WarningVariable. For more information, see [about_CommonParameters](http://go.microsoft.com/fwlink/?LinkID=113216).

## INPUTS

### System.Guid
The ID of the view can be provided via pipeline.

## OUTPUTS

### None

## NOTES
This operation is destructive and cannot be undone. Always use `-WhatIf` first to preview the operation.

## RELATED LINKS
- [New-DataverseView](New-DataverseView.md)
- [Set-DataverseView](Set-DataverseView.md)
- [Get-DataverseRecord](Get-DataverseRecord.md)
