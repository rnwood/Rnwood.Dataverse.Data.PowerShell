---
external help file: Rnwood.Dataverse.Data.PowerShell.Cmdlets.dll-Help.xml
Module Name: Rnwood.Dataverse.Data.PowerShell
online version:
schema: 2.0.0
---

# Set-DataverseMsAppComponent

## SYNOPSIS
Adds or updates a component in a .msapp file.

## SYNTAX

### YamlContent
```
Set-DataverseMsAppComponent [-MsAppPath] <String> [-ComponentName] <String> [-YamlContent] <String>
 [-ProgressAction <ActionPreference>] [-WhatIf] [-Confirm] [<CommonParameters>]
```

### YamlFile
```
Set-DataverseMsAppComponent [-MsAppPath] <String> [-ComponentName] <String> -YamlFilePath <String>
 [-ProgressAction <ActionPreference>] [-WhatIf] [-Confirm] [<CommonParameters>]
```

## DESCRIPTION
Adds or updates a component in a Canvas app .msapp file. Components are defined using Power Apps YAML format. The .msapp file is modified in place.

## EXAMPLES

### Example 1: Add a new component
```powershell
PS C:\> $yaml = @"
Component:
  MyButton:
    Properties:
      Width: 100
      Height: 50
"@
PS C:\> Set-DataverseMsAppComponent -MsAppPath "myapp.msapp" -ComponentName "MyButton" -YamlContent $yaml
```

Adds a new component to the .msapp file.

## PARAMETERS

### -ComponentName
Name of the component

```yaml
Type: String
Parameter Sets: (All)
Aliases:

Required: True
Position: 1
Default value: None
Accept pipeline input: False
Accept wildcard characters: False
```

### -MsAppPath
Path to the .msapp file

```yaml
Type: String
Parameter Sets: (All)
Aliases:

Required: True
Position: 0
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

### -YamlContent
YAML content for the component

```yaml
Type: String
Parameter Sets: YamlContent
Aliases:

Required: True
Position: 2
Default value: None
Accept pipeline input: False
Accept wildcard characters: False
```

### -YamlFilePath
Path to a YAML file containing the component definition

```yaml
Type: String
Parameter Sets: YamlFile
Aliases:

Required: True
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

### CommonParameters
This cmdlet supports the common parameters: -Debug, -ErrorAction, -ErrorVariable, -InformationAction, -InformationVariable, -OutVariable, -OutBuffer, -PipelineVariable, -Verbose, -WarningAction, and -WarningVariable. For more information, see [about_CommonParameters](http://go.microsoft.com/fwlink/?LinkID=113216).

## INPUTS

### None
## OUTPUTS

### System.Object
## NOTES

## RELATED LINKS
