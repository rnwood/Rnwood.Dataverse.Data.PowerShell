---
external help file: Rnwood.Dataverse.Data.PowerShell.Cmdlets.dll-Help.xml
Module Name: Rnwood.Dataverse.Data.PowerShell
online version:
schema: 2.0.0
---

# Remove-DataverseSitemap

## SYNOPSIS
Removes (deletes) a sitemap from Dataverse.

## SYNTAX

### ByName
```
Remove-DataverseSitemap [-Name] <String> [-IfExists] [-Connection <ServiceClient>]
 [-ProgressAction <ActionPreference>] [-WhatIf] [-Confirm] [<CommonParameters>]
```

### ById
```
Remove-DataverseSitemap -Id <Guid> [-IfExists] [-Connection <ServiceClient>]
 [-ProgressAction <ActionPreference>] [-WhatIf] [-Confirm] [<CommonParameters>]
```

## DESCRIPTION

This cmdlet removes (deletes) a sitemap from a Dataverse environment. You can specify the sitemap to delete by name or by ID.

Only unmanaged sitemaps can be deleted. Managed sitemaps (deployed via managed solutions) cannot be deleted directly - they must be removed by uninstalling the solution that contains them.

Use the -WhatIf parameter to preview what would be deleted without actually performing the deletion.

## EXAMPLES

### Example 1: Remove a sitemap by name
```powershell
PS C:\> Remove-DataverseSitemap -Name "MySitemap"

Sitemap 'MySitemap' deleted successfully.
```

Deletes the sitemap with the specified name.

### Example 2: Remove a sitemap by ID
```powershell
PS C:\> Remove-DataverseSitemap -Id "a1b2c3d4-5678-90ab-cdef-1234567890ab"

Sitemap 'MySitemap' deleted successfully.
```

Deletes the sitemap with the specified ID.

### Example 3: Remove multiple sitemaps by piping
```powershell
PS C:\> Get-DataverseSitemap -Unmanaged | Where-Object { $_.Name -like "Test*" } | Remove-DataverseSitemap

Sitemap 'TestSitemap1' deleted successfully.
Sitemap 'TestSitemap2' deleted successfully.
Sitemap 'TestSitemap3' deleted successfully.
```

Retrieves all unmanaged sitemaps with names starting with "Test" and deletes them.

### Example 4: Remove sitemap only if it exists
```powershell
PS C:\> Remove-DataverseSitemap -Name "OptionalSitemap" -IfExists

Sitemap 'OptionalSitemap' not found. Skipping deletion.
```

Attempts to delete a sitemap but doesn't raise an error if it doesn't exist.

### Example 5: Preview deletion with WhatIf
```powershell
PS C:\> Remove-DataverseSitemap -Name "MySitemap" -WhatIf

What if: Performing the operation "Delete" on target "Sitemap 'MySitemap' (ID: a1b2c3d4-5678-90ab-cdef-1234567890ab)".
```

Shows what would be deleted without actually performing the deletion.

### Example 6: Conditional deletion with confirmation
```powershell
PS C:\> $sitemap = Get-DataverseSitemap -Name "MySitemap"
PS C:\> if ($sitemap.ModifiedOn -lt (Get-Date).AddMonths(-6)) {
>>     Remove-DataverseSitemap -Id $sitemap.Id -Confirm:$false
>> }
```

Deletes a sitemap if it hasn't been modified in the last 6 months, bypassing confirmation.

## PARAMETERS

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
DataverseConnection instance obtained from Get-DataverseConnection cmdlet, or string specifying Dataverse organization URL (e.g. http://server.com/MyOrg/). If not provided, uses the default connection set via Get-DataverseConnection -SetAsDefault.

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

### -Id
The unique identifier of the sitemap to remove.

```yaml
Type: Guid
Parameter Sets: ById
Aliases:

Required: True
Position: Named
Default value: None
Accept pipeline input: True (ByPropertyName)
Accept wildcard characters: False
```

### -IfExists
If specified, the cmdlet will not raise an error if the sitemap does not exist.

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

### -Name
The name of the sitemap to remove.

```yaml
Type: String
Parameter Sets: ByName
Aliases:

Required: True
Position: 0
Default value: None
Accept pipeline input: True (ByPropertyName)
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
{{ Fill ProgressAction Description }}

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
### System.Guid
## OUTPUTS

### System.Object
## NOTES

This cmdlet requires an active connection to a Dataverse environment.

Only unmanaged sitemaps can be deleted. Managed sitemaps are deployed via managed solutions and must be removed by uninstalling the solution.

This is a high-impact operation. Use the -WhatIf parameter to preview the operation, and -Confirm:$false to bypass confirmation prompts in automated scripts.

## RELATED LINKS

[Get-DataverseSitemap](Get-DataverseSitemap.md)

[Set-DataverseSitemap](Set-DataverseSitemap.md)

[Remove-DataverseRecord](Remove-DataverseRecord.md)
