---
external help file: Rnwood.Dataverse.Data.PowerShell.Cmdlets.dll-Help.xml
Module Name: Rnwood.Dataverse.Data.PowerShell
online version:
schema: 2.0.0
---

# Remove-DataverseAppModuleComponent

## SYNOPSIS
Removes an app module component from Dataverse.

## SYNTAX

```
Remove-DataverseAppModuleComponent -Id <Guid> [-IfExists] [-Connection <ServiceClient>]
 [-ProgressAction <ActionPreference>] [-WhatIf] [-Confirm] [<CommonParameters>]
```

## DESCRIPTION
The Remove-DataverseAppModuleComponent cmdlet removes a component from a model-driven app. This removes the relationship between the app and the component (entity, dashboard, etc.) but does not delete the component itself.

Use the IfExists parameter to suppress errors when attempting to remove components that don't exist.

## EXAMPLES

### Example 1: Remove a component from an app
```powershell
PS C:\> $component = Get-DataverseAppModuleComponent -Connection $c -AppModuleIdValue $appId | Where-Object { $_.ObjectId -eq $entityId }
PS C:\> Remove-DataverseAppModuleComponent -Connection $c -Id $component.Id -Confirm:$false
```

Finds and removes a specific entity component from an app module.

### Example 2: Remove with IfExists flag
```powershell
PS C:\> Remove-DataverseAppModuleComponent -Connection $c -Id $componentId -IfExists -Confirm:$false
```

Attempts to remove a component but doesn't error if it doesn't exist.

### Example 3: Remove all components of a type
```powershell
PS C:\> Get-DataverseAppModuleComponent -Connection $c -AppModuleIdValue $appId -ComponentType 60 |
    Remove-DataverseAppModuleComponent -Connection $c -Confirm:$false
```

Removes all dashboard components (type 60) from an app module.

### Example 4: Use WhatIf to preview
```powershell
PS C:\> Remove-DataverseAppModuleComponent -Connection $c -Id $componentId -WhatIf
```

Shows what would happen without actually removing the component.

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
DataverseConnection instance obtained from Get-DataverseConnection cmdlet, or string specifying Dataverse organization URL (e.g.
http://server.com/MyOrg/).
If not provided, uses the default connection set via Get-DataverseConnection -SetAsDefault.

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
ID of the app module component to remove

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

### -IfExists
If specified, the cmdlet will not raise an error if the component does not exist

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
Shows what would happen if the cmdlet runs.
The cmdlet is not run.

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

### System.Guid
## OUTPUTS

### System.Object
## NOTES

**Behavior:**
- Removes the component relationship from the app
- Does NOT delete the underlying component (entity, dashboard, etc.)
- Component can be added back to the app later

**IfExists Flag:**
- Suppresses errors when component doesn't exist
- Useful for cleanup scripts and idempotent operations

**Confirmation:**
- Prompts for confirmation by default (ConfirmImpact = Medium)
- Use -Confirm:$false to skip confirmation
- Use -WhatIf to preview without executing

**Use Cases:**
- Removing entities from an app
- Cleaning up unused components
- Reorganizing app structure
- Automated app configuration management


## RELATED LINKS

[Get-DataverseAppModuleComponent](Get-DataverseAppModuleComponent.md)

[Set-DataverseAppModuleComponent](Set-DataverseAppModuleComponent.md)

[Remove-DataverseAppModule](Remove-DataverseAppModule.md)