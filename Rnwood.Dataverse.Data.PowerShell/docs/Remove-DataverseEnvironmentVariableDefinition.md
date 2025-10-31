---
external help file: Rnwood.Dataverse.Data.PowerShell.Cmdlets.dll-Help.xml
Module Name: Rnwood.Dataverse.Data.PowerShell
online version:
schema: 2.0.0
---

# Remove-DataverseEnvironmentVariableDefinition

## SYNOPSIS
Removes environment variable definitions and their values from Dataverse.

## SYNTAX

```
Remove-DataverseEnvironmentVariableDefinition [-SchemaName] <String> [-ValueOnly] [-Connection <ServiceClient>]
 [-ProgressAction <ActionPreference>] [-WhatIf] [-Confirm] [<CommonParameters>]
```

## DESCRIPTION
Removes environment variable definitions from Dataverse. By default, removes both the definition and any associated value.
Use the -ValueOnly switch to remove only the value while preserving the definition.

## EXAMPLES

### Example 1: Remove an environment variable definition and its value
```powershell
PS C:\> Remove-DataverseEnvironmentVariableDefinition -SchemaName "new_apiurl"
```

Removes the environment variable definition "new_apiurl" and its associated value.

### Example 2: Remove only the value
```powershell
PS C:\> Remove-DataverseEnvironmentVariableDefinition -SchemaName "new_apiurl" -ValueOnly
```

Removes only the value for "new_apiurl", preserving the definition.

### Example 3: Remove with confirmation
```powershell
PS C:\> Remove-DataverseEnvironmentVariableDefinition -SchemaName "new_apiurl" -Confirm
```

Prompts for confirmation before removing the definition and value.

### Example 4: Remove with WhatIf
```powershell
PS C:\> Remove-DataverseEnvironmentVariableDefinition -SchemaName "new_apiurl" -WhatIf
```

Shows what would happen if the cmdlet runs without actually removing anything.

### Example 5: Remove from pipeline
```powershell
PS C:\> Get-DataverseEnvironmentVariableDefinition -SchemaName "new_test*" | 
    Remove-DataverseEnvironmentVariableDefinition
```

Removes all environment variable definitions with schema names starting with "new_test".

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

### -SchemaName
Schema name of the environment variable to remove.

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

### -ValueOnly
If specified, removes only the environment variable value, not the definition.

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
## OUTPUTS

### System.Object
## NOTES
- By default, removes both the definition and value
- Use -ValueOnly to remove only the value
- If the definition doesn't exist, an error will be returned
- Consider the impact on solutions that reference this environment variable before removing

## RELATED LINKS

[Get-DataverseEnvironmentVariableDefinition](Get-DataverseEnvironmentVariableDefinition.md)
[Set-DataverseEnvironmentVariableDefinition](Set-DataverseEnvironmentVariableDefinition.md)
[Remove-DataverseEnvironmentVariableValue](Remove-DataverseEnvironmentVariableValue.md)
